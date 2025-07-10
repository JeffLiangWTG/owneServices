using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	public class ImportAccompanyingDocumentEntryLineWrapper : DocBaseWrapper
	{
		public static ImportAccompanyingDocumentEntryLineWrapper New(CusEntryHeader entryHeader, CusEntryLine entryLine)
		{
			ImportAccompanyingDocumentEntryLineWrapper result = null;
			if (entryHeader != null && entryLine != null)
			{
				result = new ImportAccompanyingDocumentEntryLineWrapper(entryHeader, entryLine);
			}
			return result;
		}

		ImportAccompanyingDocumentEntryLineWrapper(CusEntryHeader entryHeader, CusEntryLine entryLine) : base(entryLine, entryLine.Factory)
		{
			this.entryHeader = entryHeader;
			this.entryLine = entryLine;
			SetFormattedBox47ItemCalculatedTaxes();
		}
		readonly CusEntryLine entryLine;
		readonly CusEntryHeader entryHeader;

		#region Properties

		public ZShort LineNumber => entryLine.CL_LineNumber;
		public ZString Box31Description => entryLine.CL_Description;
		public ZString Tariff => entryLine.FormattedTariff;
		public ZString TariffAdditional => entryLine.SupplementaryCode1 + " " + entryLine.SupplementaryCode2 + " " + entryLine.RandomLine.JI_AdditionalSupplements;
		public ZString NationalAdditionalCodes
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					if (!invoiceLine.JI_ZZF_NKTaxType.IsEmpty)
					{
						result.Append(invoiceLine.JI_ZZF_NKTaxType + fieldSeparator);
					}
					foreach (var tariff in invoiceLine.CusLineTariffDetails)
					{
						result.Append(tariff.BZ_Tariff + fieldSeparator);
					}
				}
				return result.ToString();
			}
		}

		public ZString DispatchCountry => entryLine.RandomLine.ZG_CountryOfDispatch;
		public ZString DestinationCountry => entryLine.CountryOfDestination;
		public ZString CountryOfPrefOrigin => entryLine.CountryOfOriginCode;
		public ZString CountryOfPrefOriginOverride => entryLine.CountryOfSupply;
		public ZString PreferenceCode => entryLine.PreferenceCode;
		public ZString ProcedureCode => entryLine.ProcedureCode.SubstringSafe(0, 4);
		public ZDecimal SupplementaryQuantity => entryLine.SupplementaryQuantity;
		public ZString ValuationMethod => entryLine.ValuationMethod;
		public ZString ValueAdjCode
		{
			get
			{
				var randomLine = entryLine.RandomLine;
				if (randomLine.JI_ValuationCode == Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1)
				{
					var invoiceHeader = entryHeader.RandomHeader;
					var flag1 = invoiceHeader.RelatedIndicator || randomLine.RelatedIndicator;
					var flag2 = invoiceHeader.RelatedIndicator2 || randomLine.RelatedIndicator2;
					var flag3 = invoiceHeader.RelatedIndicator3 || randomLine.RelatedIndicator3;
					var flag4 = invoiceHeader.RelatedIndicator4 || randomLine.RelatedIndicator4;
					return BoolToBit(flag1) + BoolToBit(flag2) + BoolToBit(flag3) + BoolToBit(flag4);
				}

				return ZString.Empty;
			}
		}

		static ZString BoolToBit(ZBool flag) => flag ? Enterprise.Customs.Business.Bool01List.Codes.Yes : Enterprise.Customs.Business.Bool01List.Codes.No;

		public ZDecimal StatisticalValue => entryLine.CL_StatisticalValue;

		public ZString Box22ItemPackages => Factory.GetValue(ref box22ItemPackagesCached, delegate
			{
				var packingDetails = new ZStringBuilder();
				var allPacksInAllLines = entryLine.PackagingDetails;
				var uniquePacks = new Dictionary<(ZString packType, ZString marksAndNos), int>();
				foreach (var pivot in allPacksInAllLines)
				{
					// Do a dictionary look up to merge packaging lines based on merge key of unit type and marks & numbers.  Then sum the quantities in the component lines to give a total.
					if (pivot.CHC_NumberOfPacks > 0)
					{
						var key = (pivot.Package.CW_PackType, pivot.Package.CW_MarksAndNos);
						if (uniquePacks.ContainsKey(key))
						{
							uniquePacks[key] += pivot.CHC_NumberOfPacks;
						}
						else
						{
							uniquePacks[key] = pivot.CHC_NumberOfPacks;
						}
					}
				}
				foreach (var key in uniquePacks.Keys.OrderBy(x => x))
				{
					var packType = key.packType;
					var marksAndNos = key.marksAndNos;
					var numberOfPacks = uniquePacks[key].ToString(CultureInfo.InvariantCulture);
					packingDetails.Append(numberOfPacks + fieldSeparator + packType + fieldSeparator + marksAndNos);
				}

				return packingDetails.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box22ItemPackagesCached;

		public ZString Box35ItemGrossMass
		{
			get
			{
				var effectiveGrossWeight = entryLine.EffectiveGrossWeight.InKilogramsSafe.Round(3);
				return effectiveGrossWeight == 0 ? ZString.Empty : (ZString)effectiveGrossWeight.ToString();
			}
		}

		public ZDecimal Box35ItemNetMass
		{
			get { return entryLine.CustomsQuantity.Round(3); }
		}

		public ZString Box39ItemQuota
		{
			get { return entryLine.QuotaOrderNumber; }
		}

		public ZString Box42ItemPriceAndCurrency => entryLine.TotalLinePriceInLocalCurrency.ToString() + " " + GlbCompany.CurrentCompany.LocalCurrency.Code;

		public ZString Box40ItemSummaryDeclarationPreviousDocs => Factory.GetValue(ref box40ItemSummaryDeclarationPreviousDocsCached, delegate
			{
				var result = new ZStringBuilder();

				foreach (var previousDocument in entryLine.PreviousDocuments
					.Select(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_AdditionalDescription, x.CSI_LineNo))
					.OrderBy(x => x.CSI_LineNo)
					.ThenBy(x => x.CSI_Code)
					.ThenBy(x => x.CSI_ReferenceNumber)
					.ThenBy(x => x.CSI_AdditionalDescription))
				{
					result.Append(previousDocument.CSI_Code + fieldSeparator + previousDocument.CSI_ReferenceNumber + fieldSeparator + previousDocument.CSI_LineNo);
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box40ItemSummaryDeclarationPreviousDocsCached;

		public ZString Box44ItemAdditionalDocs => Factory.GetValue(ref box44ItemAdditionalDocsCached, delegate
			{
				var result = new ZStringBuilder();
				var documents = entryHeader.SupportingDocuments.Concat(entryLine.SupportingDocuments);
				foreach (var supDoc in documents
				.OrderBy(x => x.CSI_LineNo)
				.ThenBy(x => x.CSI_Code)
				.ThenBy(x => x.CSI_ReferenceNumber)
				.ThenBy(x => x.CSI_AdditionalDescription))
				{
					result.Append(supDoc.CSI_Code + fieldSeparator +
						(supDoc.CSI_ReferenceNumber.IsEmpty ? ZString.Empty : supDoc.CSI_ReferenceNumber) +
						supDoc.CSI_AdditionalDescription + fieldSeparator +
						(supDoc.CSI_DateOfExpiry.IsValid ? supDoc.CSI_DateOfExpiry.ToShortDateString() : string.Empty) + fieldSeparator +
						supDoc.CSI_UnitOfQuantity + fieldSeparator +
						supDoc.CSI_Quantity + fieldSeparator +
						supDoc.CSI_RX_NKCurrency + fieldSeparator +
						supDoc.CSI_Value);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box44ItemAdditionalDocsCached;

		public ZString Box44ItemAdditionalInformation => Factory.GetValue(ref box44ItemAdditionalInformationCached, delegate
			{
				var result = new ZStringBuilder();
				foreach (var addInfo in entryHeader.AdditionalInfos
					.OrderBy(x => x.CSI_LineNo)
					.ThenBy(x => x.CSI_Code)
					.ThenBy(x => x.CSI_ReferenceNumber)
					.ThenBy(x => x.CSI_AdditionalDescription))
				{
					result.Append(addInfo.CSI_Code + fieldSeparator + (addInfo.CSI_Description.IsEmpty ? ZString.Empty : addInfo.CSI_Description));
				}
				foreach (var addInfo in entryLine.AdditionalInfos
					.OrderBy(x => x.CSI_LineNo)
					.ThenBy(x => x.CSI_Code)
					.ThenBy(x => x.CSI_ReferenceNumber)
					.ThenBy(x => x.CSI_AdditionalDescription))
				{
					result.Append(addInfo.CSI_Code + fieldSeparator + (addInfo.CSI_Description.IsEmpty ? ZString.Empty : addInfo.CSI_Description));
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});

		CachedProperty<ZString> box44ItemAdditionalInformationCached;

		public ZString Box44FiscalReferences => Factory.GetValue(ref box44FiscalReferencesCached, delegate
			{
				var result = new ZStringBuilder();
				foreach (var fiscalRef in entryLine.FiscalReferences
					.OrderBy(x => x.CFR_Code)
					.ThenBy(x => x.CFR_Reference))
				{
					result.Append(fiscalRef.CFR_Code + fieldSeparator + fiscalRef.CFR_Reference);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});

		CachedProperty<ZString> box44FiscalReferencesCached;

		public ZString Box2Exporter
		{
			get
			{
				var returnValue = new ZStringBuilder();
				var exporter = Exporter;

				if (exporter != null)
				{
					if (!exporter.CompanyName.IsEmpty)
					{
						returnValue.Append(exporter.CompanyName + lineSeparator);
					}
					if (!exporter.Address1.IsEmpty)
					{
						returnValue.Append(exporter.Address1 + lineSeparator);
					}
					if (!exporter.City.IsEmpty)
					{
						returnValue.Append(exporter.City + lineSeparator);
					}
					if (exporter.Country != null)
					{
						returnValue.Append(exporter.Country + lineSeparator);
					}
					if (!exporter.Postcode.IsEmpty)
					{
						returnValue.Append(exporter.Postcode + lineSeparator);
					}
				}

				return returnValue.ToString();
			}
		}

		public ZString Box2ExporterNumber => Exporter?.GetEuIdentificationNumber() ?? ZString.Empty;

		OrgAddress Exporter => CachedValueHelper.GetValue(ref exporterCached, () => entryHeader.HasMultipleExportersViaLines ? entryLine.Consignor : null);
		CachedValue<OrgAddress> exporterCached;

		public ZString Box45AdditionsAndDeductions => Factory.GetValue(ref box45AdditionsAndDeductionsCached, delegate
			{
				var result = new ZStringBuilder();
				foreach (var chargeDeduction in entryLine.AISChargeDeductions?.Where(x => (x.Value?.Amount ?? ZDecimal.Zero) != ZDecimal.Zero)
					.OrderBy(x => x.Key))
				{
					var dotPosition = chargeDeduction.Key.IndexOf(".");
					var chargeCode = dotPosition < 0 ? chargeDeduction.Key : chargeDeduction.Key.Left(dotPosition);
					result.Append(chargeCode + fieldSeparator + chargeDeduction.Value);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});

		CachedProperty<ZString> box45AdditionsAndDeductionsCached;

		public ZString Box31ContainerNumbers => Factory.GetValue(ref box31ContainerNumbersCached, delegate
			{
				var result = new ZStringBuilder();
				foreach (var containerNumber in entryLine.InvoiceLines.SelectMany(invoiceLine => ((JobComInvoiceLine)invoiceLine).ContainersForInvoiceLinesForBindingOnly
					.Where(container => !container.ContainerNumber.IsEmpty && container.IsForInvoiceLine)
					.Select(container => container.ContainerNumber)
					.OrderBy(container => container))
					.Distinct())
				{
					result.Append(containerNumber);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box31ContainerNumbersCached;

		void SetFormattedBox47ItemCalculatedTaxes()
		{
			var chargeTypes = new ZStringBuilder();
			var taxBases = new ZStringBuilder();
			var taxRates = new ZStringBuilder();
			var taxAssessedAmounts = new ZStringBuilder();
			var moPs = new ZStringBuilder();
			var taxPayableAmounts = new ZStringBuilder();
			var taxMeasureUnits = new ZStringBuilder();

			var usingConfirmedFees = entryHeader.HasAnyConfirmedFeesOnAnyMergedLine;
			var taxCalculations = usingConfirmedFees ? entryLine.ConfirmedFees.Cast<CusEntryLineFee>() : entryLine.Fees.Cast<CusEntryLineFee>();

			foreach (var cusEntryLineFee in taxCalculations
				.OrderBy(x => x.NationalFeeTypeCode)
				.ThenBy(x => x.CF_BaseValue)
				.ThenBy(x => x.CF_Rate)
				.ThenBy(x => x.CF_ChargeAmount)
				.ThenBy(x => x.CF_MethodOfPayment)
				.ThenBy(x => x.CF_IsLandedCostOnly)
				.ThenBy(x => x.CF_MethodOfCalculation))
			{
				chargeTypes.Append(cusEntryLineFee.NationalFeeTypeCode);
				taxBases.Append(cusEntryLineFee.CF_BaseValue.ToString(2));
				taxRates.Append(cusEntryLineFee.CF_Rate.ToString(2));
				var chargeAmountString = cusEntryLineFee.CF_ChargeAmount.ToString(2);
				taxAssessedAmounts.Append(chargeAmountString);
				moPs.Append(cusEntryLineFee.CF_MethodOfPayment.IsEmpty ? ZString.Empty : cusEntryLineFee.CF_MethodOfPayment);
				taxPayableAmounts.Append(cusEntryLineFee.CF_IsLandedCostOnly ? "0.00" : chargeAmountString);
				taxMeasureUnits.Append(cusEntryLineFee.CF_MethodOfCalculation == "%" ? ZString.Empty : cusEntryLineFee.CF_MethodOfCalculation);
			}
			Box47ChargeTypes = chargeTypes.ToStringWithDelimiterBetweenAppends(lineSeparator);
			Box47TaxBases = taxBases.ToStringWithDelimiterBetweenAppends(lineSeparator);
			Box47TaxRate = taxRates.ToStringWithDelimiterBetweenAppends(lineSeparator);
			Box47TaxAssessedAmounts = taxAssessedAmounts.ToStringWithDelimiterBetweenAppends(lineSeparator);
			Box47MoPs = moPs.ToStringWithDelimiterBetweenAppends(lineSeparator);
			Box47TaxPayableAmounts = taxPayableAmounts.ToStringWithDelimiterBetweenAppends(lineSeparator);
			Box47TaxMeasureUnits = taxMeasureUnits.ToStringWithDelimiterBetweenAppends(lineSeparator);
		}

		public ZString AdditionalFiscalReferences => Factory.GetValue(ref additionalFiscalReferencesCached, delegate
			{
				var result = new ZStringBuilder();

				foreach (JobComInvoiceLine invoiceLine in entryLine?.InvoiceLines?
					.OrderBy(x => ((JobComInvoiceLine)x).JI_LineNo))
				{
					foreach (var info in invoiceLine.AdditionalInfos?.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
						.OrderBy(x => x.CSI_Code)
						.ThenBy(x => x.CSI_Description))
					{
						result.AppendLine(info.CSI_Code + fieldSeparator + info.CSI_Description);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> additionalFiscalReferencesCached;

		#endregion

		public ZString Box47ChargeTypes { get; set; }
		public ZString Box47TaxBases { get; set; }
		public ZString Box47TaxRate { get; set; }
		public ZString Box47TaxAssessedAmounts { get; set; }
		public ZString Box47MoPs { get; set; }
		public ZString Box47TaxPayableAmounts { get; set; }
		public ZString Box47TaxMeasureUnits { get; set; }

		const string lineSeparator = "\r\n";
		const string fieldSeparator = " | ";
	}
}
