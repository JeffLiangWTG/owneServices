using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocCDSEntryLineWrapper : DocBaseWrapper
	{
		public DocCDSEntryLineWrapper(CusEntryLine entryLine) : base(entryLine, entryLine.Factory)
		{
			this.entryLine = entryLine;
			SetFormattedBox47ItemCalculatedTaxes();
		}
		readonly CusEntryLine entryLine;

		public static DocCDSEntryLineWrapper New(CusEntryLine entryLine)
		{
			DocCDSEntryLineWrapper result = null;
			if (entryLine != null)
			{
				result = new DocCDSEntryLineWrapper(entryLine);
			}
			return result;
		}

		#region Properties

		GbCDSImportEntryLineWrapper ImportWrapper => importWrapper ?? (importWrapper = new GbCDSImportEntryLineWrapper(entryLine));
		GbCDSImportEntryLineWrapper importWrapper;

		public ZShort LineNumber => entryLine.CL_LineNumber;
		public ZString Box31Description => entryLine.CL_Description;
		public ZString Tariff => entryLine.FormattedTariff;
		public ZString TariffAdditional1 => entryLine.SupplementaryCode1;
		public ZString TariffAdditional2 => entryLine.SupplementaryCode2;
		public ZString DestinationCountry => entryLine.CountryOfDestination;
		public ZString CountryOfDispatch => entryLine.CountryOfExport;
		public ZString CountryOfPrefOrigin => entryLine.CountryOfOriginCode;
		public ZString CountryOfPrefOriginOverride => entryLine.CountryOfSupply;
		public ZString PreferenceCode => entryLine.PreferenceCode;
		public ZString ProcedureCode => entryLine.ProcedureCode;
		public ZString AdditionalProcedureCodes => GetAllAdditionalProcedureCodesAsString();

		ZString GetAllAdditionalProcedureCodesAsString()
		{
			var additionalProcedureCodesList = entryLine.RandomLine.AdditionalProcedureCodesAsString
				.Split(',')
				.Select(code => code.SubstringSafe(4, 8).Trim())
				.Where(code => !code.IsEmpty)
				.ToList();

			var additionalProcedureCode = ProcedureCode.SubstringSafe(4, 8).Trim();
			if (!additionalProcedureCode.IsEmpty)
			{
				additionalProcedureCodesList.Add(additionalProcedureCode);
			}
			additionalProcedureCodesList.Sort();
			return string.Join(",", additionalProcedureCodesList);
		}

		public ZDecimal SupplementaryQuantity => entryLine.SupplementaryQuantity;
		public ZString ValuationMethod => entryLine.ValuationMethod;
		public ZString ValueAdjCode => ((IGovernmentAgencyGoodsItem)ImportWrapper).ValuationAdjustmentAdditionCode;
		public ZDecimal StatisticalValue => entryLine.StatisticalValue;

		public ZString Box22ItemPackages
		{
			get
			{
				var packingDetails = new ZStringBuilder();
				var allPacksInAllLines = entryLine.PackagingDetails;
				var uniquePacks = new Dictionary<string, int>();
				foreach (var pivot in allPacksInAllLines)
				{   // Do a dictionary look up to merge packaging lines based on merge key of unit type and marks & numbers.  Then sum the quantities in the component lines to give a total.
					if (pivot.CHC_NumberOfPacks > 0)
					{
						var key = pivot.Package.CW_PackType + "~" + pivot.Package.CW_MarksAndNos;
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
				foreach (var key in uniquePacks.Keys)
				{
					var pc = key.Split('~')[0];
					var pm = key.Split('~')[1];
					var pn = uniquePacks[key].ToString(CultureInfo.InvariantCulture);
					packingDetails.Append(pn + fieldSeparator + pc + fieldSeparator + pm);
				}

				return packingDetails.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

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
		public ZString Box42ItemPriceAndCurrency
		{
			get
			{
				var currency = entryLine.Declaration.Country.RN_RX_NKLocalCurrency;
				return entryLine.TotalLinePrice.ToString();
			}
		}

		public ZString Box40ItemSummaryDeclarationPreviousDocs
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var prevDoc in entryLine.PreviousDocuments)
				{
					result.Append(prevDoc.CSI_SubType + fieldSeparator + prevDoc.CSI_Code + fieldSeparator + prevDoc.CSI_ReferenceNumber + (prevDoc.CSI_DateOfIssue.IsValid ? "-" + prevDoc.DateOfIssueInFormat : string.Empty));
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

		public ZString Box44ItemAdditionalDocs
		{
			get
			{
				var result = new ZStringBuilder();
				var documents = entryLine.Header.SupportingDocuments.Concat(entryLine.SupportingDocuments);
				foreach (var supDoc in documents)
				{
					result.Append(supDoc.CSI_Code + fieldSeparator +
						new ZStringBuilder().AppendIfNotEmpty(supDoc.CSI_ReferenceNumber).AppendIfNotEmpty(supDoc.CSI_SubType).ToStringWithDelimiterBetweenAppends("-") + fieldSeparator +
						supDoc.CSI_Status + fieldSeparator +
						supDoc.CSI_Description + fieldSeparator +
						supDoc.CSI_ReferenceNumber2 + fieldSeparator +
						(supDoc.CSI_DateOfIssue.IsValid ? supDoc.CSI_DateOfIssue.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture) : string.Empty) + fieldSeparator +
						supDoc.CSI_UnitOfQuantity + fieldSeparator +
						supDoc.CSI_Quantity);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

		public ZString Box44ItemAdditionalInformation
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var addInfo in entryLine.Header.AdditionalInfos)
				{
					result.Append(addInfo.CSI_Code + fieldSeparator + (addInfo.CSI_Description.IsEmpty ? ZString.Empty : addInfo.CSI_Description));
				}
				foreach (var addInfo in entryLine.AdditionalInfos)
				{
					result.Append(addInfo.CSI_Code + fieldSeparator + (addInfo.CSI_Description.IsEmpty ? ZString.Empty : addInfo.CSI_Description));
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

		public ZString Box44FiscalReferences
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var fiscalRef in entryLine.FiscalReferences)
				{
					result.Append(fiscalRef.CFR_Code + fieldSeparator + fiscalRef.CFR_Reference);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

		public ZString Box2Exporter
		{
			get
			{
				var exporter = Exporter;
				return exporter != null ? exporter.CompanyName + lineSeparator + exporter.Address1 : null;
			}
		}

		public ZString Box2ExporterNumber => Exporter?.GetEuIdentificationNumber() ?? ZString.Empty;

		OrgAddress Exporter => entryLine.Header.HasMultipleExportersViaLines() ? entryLine.Consignor : null;

		public ZString Box45AdditionsAndDeductions
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var chargeDeduction in entryLine.CDSChargeDeductions?.Where(x => (x.Value?.Amount ?? ZDecimal.Zero) != ZDecimal.Zero))
				{
					var dotPosition = chargeDeduction.Key.IndexOf(".");
					var chargeCode = dotPosition < 0 ? chargeDeduction.Key : chargeDeduction.Key.Left(dotPosition);
					result.Append(chargeCode + fieldSeparator + chargeDeduction.Value);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

		public ZString Box31ContainerNumbers
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var containerNumber in entryLine.InvoiceLines.SelectMany(invoiceLine => ((JobComInvoiceLine)invoiceLine).ContainersForInvoiceLinesForBindingOnly
					.Where(container => !container.ContainerNumber.IsEmpty && container.IsForInvoiceLine)
					.Select(container => container.ContainerNumber))
					.Distinct())
				{
					result.Append(containerNumber);
				}
				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			}
		}

		void SetFormattedBox47ItemCalculatedTaxes()
		{
			var chargeTypes = new ZStringBuilder();
			var taxBases = new ZStringBuilder();
			var taxRates = new ZStringBuilder();
			var taxAssessedAmounts = new ZStringBuilder();
			var moPs = new ZStringBuilder();
			var taxPayableAmounts = new ZStringBuilder();
			var taxMeasureUnits = new ZStringBuilder();

			var fees = entryLine.Header.HasAnyConfirmedFeesOnAnyMergedLine ? entryLine.ConfirmedFees.Cast<CusEntryLineFee>() : entryLine.Fees.Cast<CusEntryLineFee>();

			foreach (var cusEntryLineFee in fees)
			{
				chargeTypes.Append(cusEntryLineFee.CF_ChargeType.IsEmpty ? ZString.Empty : cusEntryLineFee.CF_ChargeType);
				taxBases.Append(cusEntryLineFee.CF_BaseValue.IsEmpty ? string.Empty : cusEntryLineFee.CF_BaseValue.ToString(2));
				taxRates.Append(cusEntryLineFee.CF_Rate.IsEmpty ? string.Empty : cusEntryLineFee.CF_Rate.ToString(2));
				var chargeAmountString = cusEntryLineFee.CF_ChargeAmount.IsEmpty ? string.Empty : cusEntryLineFee.CF_ChargeAmount.ToString(2);
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
