using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs.ES;

namespace Enterprise.Customs.ES.Business
{
	public class ESDocC10Header : NonPersistentBusinessObject, IDocumentSupportable, IESDocC10Header
	{
		ESDocC10Header(CusEntryHeader entryHeader, BusinessObject supporterBO) : base(entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
			declaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
			this.supporterBO = supporterBO;
		}

		readonly BusinessObject supporterBO;

		public static ESDocC10Header New(CusEntryHeader entryHeader, BusinessObject supporterBO)
		{
			return entryHeader == null ? null : new ESDocC10Header(entryHeader, supporterBO);
		}

		public CusEntryHeader entryHeader { get; }
		readonly JobDeclaration declaration;

		ZZRefCusCodeListCombined CustomsOffice
		{
			get
			{
				if (customsOfficeCached == null)
				{
					customsOfficeCached = new CachedValue<ZZRefCusCodeListCombined>(() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, declaration.JE_CustomsOffice, Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
				}
				return customsOfficeCached.Value;
			}
		}
		CachedValue<ZZRefCusCodeListCombined> customsOfficeCached;

		ZString CustomsOfficeDescription => CustomsOffice?.ZZD_Description ?? ZString.Empty;
		ZString CustomsOfficeCity
		{
			get
			{
				if (!customsOfficeCity.HasValue)
				{
					customsOfficeCity = ZString.Empty;

					if (CustomsOffice is ZZRefCusCodeListCombined customsOffice)
					{
						foreach (ZZRefCusCodeListAttributeCombined item in customsOffice.Attributes)
						{
							if (item.ZZE_ZXE_NKName.EqualsIgnoringCase("CITY"))
							{
								customsOfficeCity = item.ZZE_Value;
								break;
							}
						}
					}
				}
				return customsOfficeCity.Value;
			}
		}
		ZString? customsOfficeCity;

		ZBool IsCanaryIsland => entryHeader.Declaration.DestinationStateIsCanaryIsland;

		public ZString SheetName => entryHeader.MovementReferenceNumber.IsEmpty ? entryHeader.CH_BGMReference : entryHeader.MovementReferenceNumber;

		public ZString Title => IsCanaryIsland ? CanaryIslandTitle : NormalTitle;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Text expected")]
		const string CanaryIslandTitle = "BASE DEL IGIC y AIEM";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Text expected")]
		const string NormalTitle = "BASE DEL IMPUESTO DEL VALOR AÑADIDO";

		public ZString CityDate => CustomsOfficeCity + " " + ZDateTime.Today.ToCustomsFormatDateStringddMMyyyyWithDash();
		public ZString DeclarantFullName => entryHeader.DeclarantOrganisation.OH_FullName;
		public ZString UserName => GlbStaff.CurrentUser.GS_FullName;

		#region HeaderGrid
		ZDecimal TotalPackages
		{
			get
			{
				if (!totalPackages.HasValue)
				{
					totalPackages = entryHeader.MergedLines.Sum(x => x.PackagingDetails.Sum(p => p.CHC_NumberOfPacks));
				}
				return totalPackages.Value;
			}
		}
		ZDecimal? totalPackages;

		ZDecimal TotalGrossWeight
		{
			get
			{
				if (!totalGrossWeight.HasValue)
				{
					totalGrossWeight = entryHeader.InvoiceLines.Sum(x => ((JobComInvoiceLine)x).EffectiveGrossWeight.InKilogramsSafe);
				}
				return totalGrossWeight.Value;
			}
		}
		ZDecimal? totalGrossWeight;

		ZDecimal CustomsValue
		{
			get
			{
				if (!customsValue.HasValue)
				{
					customsValue = entryHeader.InvoiceLines.Sum(x => ((JobComInvoiceLine)x).JI_Calc_ESCustomsValue);
				}
				return customsValue.Value;
			}
		}
		ZDecimal? customsValue;

		ZDecimal GetSupportingDocumentsReferenceNumberSum(IEnumerable<JobComInvoiceLine> invoiceLines, ZString docType)
		{
			return invoiceLines.Sum(l => l.SupportingDocuments.Cast<SupportingDocument>().Where(d => d.CSI_Code == docType)
							.Sum(d => DecimalHelper.DecimalParseToESFormat(d.CSI_ReferenceNumber)));
		}

		ZDecimal TransformedRPP
		{
			get
			{
				if (!transformedRPP.HasValue)
				{
					transformedRPP = GetSupportingDocumentsReferenceNumberSum(entryHeader.InvoiceLines.Cast<JobComInvoiceLine>(), SupportingDocumentType.TransformedRPP);
				}

				return transformedRPP.Value;
			}
		}
		ZDecimal? transformedRPP;

		RefCurrency LocalCurrency
		{
			get
			{
				if (localCurrencyCached == null)
				{
					localCurrencyCached = new CachedValue<RefCurrency>(() => declaration.LocalCurrency);
				}
				return localCurrencyCached.Value;
			}
		}
		CachedValue<RefCurrency> localCurrencyCached;

		ZDecimal DeductionsValue
		{
			get
			{
				if (!deductionsValue.HasValue)
				{
					var localCurrency = LocalCurrency;
					deductionsValue = entryHeader.InvoiceLines.Sum(l => new ESCustomsValuationCalculator((JobComInvoiceLine)l).GetVATDeductionValue(localCurrency));
				}
				return deductionsValue.Value;
			}
		}
		ZDecimal? deductionsValue;

		ZDecimal AditionDeducibleValue
		{
			get
			{
				if (!aditionDeducibleValue.HasValue)
				{
					var localCurrency = LocalCurrency;
					aditionDeducibleValue = entryHeader.InvoiceLines.Sum(l => new ESCustomsValuationCalculator((JobComInvoiceLine)l).GetVATAdditionValue(localCurrency));
				}
				return aditionDeducibleValue.Value;
			}
		}
		ZDecimal? aditionDeducibleValue;

		Dictionary<ZString, ZDecimal> REARebate
		{
			get
			{
				if (rEARebate == null)
				{
					rEARebate = new Dictionary<ZString, ZDecimal>();
					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						rEARebate.Add(GetREARebateText(entryLine), GetREARebateValue(entryLine));
					}
				}
				return rEARebate;
			}
		}
		Dictionary<ZString, ZDecimal> rEARebate;

		ZDecimal GetREARebateValue(CusEntryLine entryLine)
		{
			return GetSupportingDocumentsReferenceNumberSum(entryLine.InvoiceLines.Cast<JobComInvoiceLine>(), SupportingDocumentType.REARebate);
		}

		ZString GetREARebateText(CusEntryLine entryLine)
		{
			var line = entryLine.RandomLine;
			var reaProductCode = line.ZG_REAProductCode;

			var rateFormula = ZString.Empty;
			var rateFormulaQty = ZDecimal.Zero;
			var rateFormulaQtyUnit = ZString.Empty;

			if (!reaProductCode.IsEmpty && line.JI_PrimaryPreference == PrimaryPreferenceCode.REA && line.UniversalTariff is TariffView tariff)
			{
				var criteria = new SpecificRateSelectionCriteria(line.EffectiveCountryOfOrigin, Core.Constants.CountryCodes.Spain, ZString.Empty, ZString.Empty, new HashSet<ZString>() { reaProductCode }, line.EffectiveDateForDutyRate, UniversalReferenceConstants.RateTypeList.REA, line.REARateCode);
				var refCusRate = tariff.GetApplicableRate(criteria);
				if (refCusRate != null)
				{
					var rateCalc = new EU.Business.EUUniversalRateCalcData(entryLine, refCusRate);
					rateCalc.CustomsValueFormula = refCusRate.ZZ2_RateFormula;
					rateFormula = rateCalc.CustomsValueFormula;
					rateFormulaQty = line.JI_CustomsQuantity;
					var customsUnits = GetREARebateCustomsUnit(line.JI_CustomsUnitQty);

					rateFormulaQtyUnit = customsUnits;
				}
			}
			if (rateFormula.IsEmpty)
			{
				return string.Format((NoResString)"Ayuda P{0}", entryLine.CL_LineNumber);
			}
			else if (rateFormulaQty.IsEmpty || rateFormulaQtyUnit.IsEmpty)
			{
				return string.Format((NoResString)"Ayuda P{0}: {1}", entryLine.CL_LineNumber, rateFormula);
			}
			else
			{
				return string.Format((NoResString)"Ayuda P{0}: {1}, {2} {3}", entryLine.CL_LineNumber, rateFormula, rateFormulaQty.ToString(2, true), rateFormulaQtyUnit);
			}
		}

		ZString GetREARebateCustomsUnit(ZString lineCustomsUnitQty)
		{
			var query = new ZQuery(RefCusMapSchema.ZZM_CW1orCommercialValue, lineCustomsUnitQty);
			query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Spain);
			var cusMap = Factory.LoadTop1<RefCusMap>(query);
			return cusMap != null ? cusMap.ZZM_CustomsValue : lineCustomsUnitQty;
		}

		ZDecimal GetFeeAmoutByType(IEnumerable<ZString> feeTypes)
		{
			ZDecimal result = 0;
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				foreach (CusEntryLineFee fee in entryLine.Fees)
				{
					if (feeTypes.Contains(fee.G4_Type))
					{
						result += ZDecimal.Parse(fee.G4_Amount);
					}
				}
			}
			return result;
		}

		ZDecimal GetFeeAmoutByType(ZString feeType)
		{
			return GetFeeAmoutByType(new ZString[] { feeType });
		}

		ZDecimal SpecialTaxesValue
		{
			get
			{
				if (!specialTaxesValue.HasValue)
				{
					specialTaxesValue = entryHeader.MergedLines.Sum(e => e.Fees.Cast<CusEntryLineFee>().Where(f => f.G4_Type.StartsWith("0", StringComparison.CurrentCultureIgnoreCase)
								|| f.G4_Type.StartsWith("5", StringComparison.CurrentCultureIgnoreCase))
							.Sum(f => f.CF_ChargeAmount));
				}
				return specialTaxesValue.Value;
			}
		}
		ZDecimal? specialTaxesValue;

		Common.JobComInvCharge[] LineAccessoryCharges
		{
			get
			{
				if (lineAccessoryCharges == null)
				{
					static bool chargesCondition(Common.JobComInvCharge c)
						=> !c.J7_IsIncludedInITOT && !c.J7_IsDutiable && c.J7_IsGSTApplicable && c.J7_Amount > ZDecimal.Zero;

					var lineCharges = new List<Common.JobComInvCharge>();
					var invoiceLines = entryHeader.InvoiceLines;
					lineCharges.AddRange(invoiceLines.SelectMany(l => ((JobComInvoiceLine)l).Charges.Cast<InvoiceLineCharge>().Where(chargesCondition)));
					lineCharges.AddRange(invoiceLines.SelectMany(l => ((JobComInvoiceLine)l).ApportionedCharges.Cast<InvoiceLineApportionCharge>().Where(chargesCondition)));

					lineAccessoryCharges = [.. lineCharges];
				}
				return lineAccessoryCharges;
			}
		}
		Common.JobComInvCharge[] lineAccessoryCharges;

		ZDecimal TotalAmount
		{
			get
			{
				if (!totalAmount.HasValue)
				{
					totalAmount = entryHeader.MergedLines.Sum(e => e.Fees.Cast<CusEntryLineFee>()
																	.Where(f => f.G4_Type == RefCusRateCodes.Vat)
																	.Sum(f => f.G4_BaseAmount));
				}
				return totalAmount.Value;
			}
		}
		ZDecimal? totalAmount;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Text expected")]
		public ZString HeaderGrid
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(GetHeaderGridData(entryHeader.CH_BGMReference, (data) => "Expediente ....... : " + data));
				result.AppendIfNotEmpty(GetHeaderGridData(entryHeader.MovementReferenceNumber, (data) => "Num. Registro .... : " + data));
				result.AppendIfNotEmpty(GetHeaderGridData(declaration.JE_OwnerRef, (data) => "Referencia ....... : " + data));
				result.AppendIfNotEmpty(GetHeaderGridData(declaration.Importer?.OH_FullName ?? ZString.Empty, ((data) => "Importador ....... : " + data)));
				ZString yearString = ZDate.Today.Year.ToString(CultureInfo.InvariantCulture);
				result.AppendIfNotEmpty(GetHeaderGridData(declaration.JE_CustomsOffice, (data) => "Aduana de Despacho : " + yearString.Right(2) +
							"-" + data + "-" + CustomsOfficeDescription));

				result.AppendIfNotEmpty(GetHeaderGridData(TotalPackages, (data) => "Número Bultos .... : " + data.ToString()));

				result.AppendIfNotEmpty(GetHeaderGridData(TotalGrossWeight, (data) => "Peso Bruto ....... : " + data.ToString(2, true) +
							" Kgs"));

				result.AppendLine();
				ZString title = (IsCanaryIsland
					? "CONCEPTOS QUE INTEGRAN DICHA BASE"
					: "CONCEPTOS QUE INTEGRAN LA BASE DEL IVA");
				result.Append(title + "EUROS".PadLeft(57 - title.Length));
				result.Append(
					"----------------------------------------  ---------------");
				result.AppendIfNotEmpty(GetHeaderGridData(CustomsValue, (data) => "Valor en Aduana........................." +
							data.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(TransformedRPP, (data) => "Valor Prod. Transformados RPP (7009)...." +
							("-" + data.ToString(2, true)).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(DeductionsValue, (data) => "Deducciones (DV1)......................." +
							DeductionsValue.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(AditionDeducibleValue, (data) => "Adiciones (DV1) deducibles.............." +
							("-" + AditionDeducibleValue.ToString(2, true)).PadLeft(17)));

				foreach (var reaPair in REARebate)
				{
					result.AppendIfNotEmpty(GetHeaderGridData(reaPair.Value, (data) => reaPair.Key.Substring(0, 40).PadRight(40, '.') +
								("-" + data.ToString(2, true)).PadLeft(17)));
				}

				result.AppendIfNotEmpty(GetHeaderGridData(GetFeeAmoutByType(new ZString[] { "A00", "A10" }), (data) => "Derechos Arancelarios..................." + data.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(GetFeeAmoutByType(new List<ZString>(new ZString[] { "A30", "A35", "A40", "A45" })), (data) => "Derechos Antidumping...................." +
							data.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(GetFeeAmoutByType("A20"), (data) => "Derechos Adicionales...................." +
							GetFeeAmoutByType("A20").ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(SpecialTaxesValue, (data) => "Impuestos Especiales...................." +
							data.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(GetFeeAmoutByType(new List<ZString>(new ZString[] { "131", "132" })), (data) => "Mozos (Tarifa general).................." +
							data.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(GetFeeAmoutByType("E00"), (data) => "Derechos percibidos en nombre de otros p" +
								data.ToString(2, true).PadLeft(17)));

				result.AppendIfNotEmpty(GetHeaderGridData(GetFeeAmoutByType("117"), (data) => "Almacenaje.............................." + data.ToString(2, true).PadLeft(17)));

				if ((LineAccessoryCharges.Length) > 0)
				{
					result.Append("");
					result.Append("GASTOS ACCESORIOS");
					result.Append("");
					var currencyConverter = CurrencyConverter.New(Factory, ZDateTime.Now, ExchangeRateType.Customs, 0);
					var chargesToAdd = new Dictionary<string, ZDecimal>();
					var euCurrency = new ZArchitecture.Environment.Currency(CurrencyCodes.EuropeanUnion);
					foreach (var charge in LineAccessoryCharges)
					{
						var description = charge.ChargeCodeDescription.Substring(0, 40);
						var amountToAdd = currencyConverter.ConvertExact(new Money(charge.J7_Amount, charge.Currency), euCurrency).Amount.Round(2);
						if (chargesToAdd.TryGetValue(description, out var amount))
						{
							chargesToAdd[description] = amount + amountToAdd;
						}
						else
						{
							chargesToAdd.Add(description, amountToAdd);
						}
					}

					foreach (var pair in chargesToAdd)
					{
						_ = result.Append(pair.Key.PadRight(40, '.') + pair.Value.ToString(2, useCommas: true).PadLeft(17));
					}
				}

				result.Append(
					"----------------------------------------  ---------------");
				result.Append("TOTAL BASE".PadLeft(40) +
					TotalAmount.ToString(2, true).PadLeft(17));

				headerGrid = result.ToStringWithNewLineBetweenAppends();
				return headerGrid.Value;
			}
		}

		ZString? headerGrid;

		string GetHeaderGridData<T>(T value, Func<T, string> getData)
			where T : IZType
		{
			return value.IsEmpty ? string.Empty : getData(value);
		}
		#endregion

		public DocumentSupporter DocumentSupporter => ((IDocumentSupportable)supporterBO).DocumentSupporter;
	}
}
