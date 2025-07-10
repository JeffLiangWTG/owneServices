using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.CountryCompliance.HungaryComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	/// <summary>
	/// Builds the invoiceData tag XML document for a Hungary GEN ManageInvoiceRequest.
	/// IMPORTANT: no CW1 database access is permitted; all data must be in ManageInvoiceRequestModel.
	/// </summary>
	public class InvoiceDataModel
	{
		public InvoiceDataModel(ManageInvoiceRequestModel model)
		{
			Model = Argument.NotNull(model, nameof(model));
		}

		public string InvoiceCompletenessIndicator => (NoResString)"false"; // Boolean value. Formatted string as required by XSD;

		public string MergeItemIndicator => (NoResString)"false"; // Boolean value.  Formatted string as required by XSD;

		ManageInvoiceRequestModel Model { get; }

		public string InvoiceNumber => Model.Transaction.Number;

		public string InvoiceIssueDateFormatted => FormatNullable(Model.Transaction.TransactionDate, "yyyy-MM-dd");    // date format string as required by XSD

		public PreviousInvoiceInfo InvoiceReference
		{
			get
			{
				PreviousInvoiceInfo previousInvoice = null;
				if (Model.PreviousInvoices?.Any() ?? false)
				{
					previousInvoice = new PreviousInvoiceInfo()
					{
						FirstOriginalInvoiceNumber = Model.PreviousInvoices.First().AH_TransactionNum,
						WasModifiedWithoutMaster = (!Model.PreviousInvoices.First().WasSubmittedSuccessfully).ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
						ModificationIndex = Model.PreviousInvoices.Count
					};
				}
				return previousInvoice;
			}
		}

		public PartyInfo SupplierInfo
		{
			get
			{
				var countryCode = Model.SupplierCountry;
				var gbr = Model.GetSupplierRegistrationNumber(OrgCusCode.CodeTypes.GovBusinessCode);
				var vat = Model.GetSupplierRegistrationNumber(OrgCusCode.CodeTypes.VATCode);
				var nonHUTaxID = Model.GetNonHungarySupplierTaxID(countryCode);
				var vatStatus = GetVatStatus(vat, gbr, false);
				var postCode = Model.Transaction?.BranchAddress?.Postcode;

				var supplierInfo = new PartyInfo()
				{
					AdditionalAddressDetail = FormattableString.Invariant($"{Model.Transaction?.BranchAddress?.Address1} {Model.Transaction?.BranchAddress?.Address2}").Trim(), // date format string as required by XSD;
					City = Model.Transaction?.BranchAddress?.City,
					CountryCode = Model.Transaction?.BranchAddress?.Country?.Code,
					Name = Model.Transaction?.BranchAddress?.CompanyName,
					PostCode = (postCode.HasValue && !postCode.Value.Trim().IsEmpty) ? postCode : EmptyPostCode,
					TaxNumber = gbr.HasValue ? gbr : (vat.HasValue ? vat : EmptyTaxNumber),
					GroupMemberTaxNumber = gbr.HasValue ? vat : null,
					CommunityMemberVatNumber = GetCommunityVatNumber(vat, nonHUTaxID, countryCode, vatStatus, Model.IsSupplierInEUCountry), // date format string as required by XSD;
				};

				return supplierInfo;
			}
		}

		public PartyInfo CustomerInfo
		{
			get
			{
				var countryCode = Model.CustomerCountry;
				var gbr = Model.GetDebtorRegistrationNumber(OrgCusCode.CodeTypes.GovBusinessCode);
				var vat = Model.GetDebtorRegistrationNumber(OrgCusCode.CodeTypes.VATCode);
				var nonHUTaxID = Model.GetNonHungaryCustomerTaxID(countryCode);
				var vatStatus = GetVatStatus(vat, gbr, true);
				var postCode = Model.Transaction?.OrganizationAddress?.Postcode;

				return new PartyInfo()
				{
					AdditionalAddressDetail = FormattableString.Invariant($"{Model.Transaction?.OrganizationAddress?.Address1} {Model.Transaction?.OrganizationAddress?.Address2}").Trim(), // date format string as required by XSD;
					City = Model.Transaction?.OrganizationAddress?.City,
					CountryCode = countryCode,
					Name = Model.Transaction?.OrganizationAddress?.CompanyName,
					PostCode = (postCode.HasValue && !postCode.Value.Trim().IsEmpty) ? postCode : EmptyPostCode,
					VATStatus = vatStatus,
					IsPrivatePerson = Model.IsCustomerPrivatePerson,
					InvoiceDeliveryMethod = Model.InvoiceDeliveryMethod,
					TaxNumber = gbr.HasValue ? gbr : (vat.HasValue ? vat : EmptyTaxNumber),
					GroupMemberTaxNumber = gbr.HasValue ? vat : null,
					CommunityMemberVatNumber = GetCommunityVatNumber(vat, nonHUTaxID, Model.CustomerCountry, vatStatus, Model.IsCustomerInEUCountry),
					ThirdStateTaxId = GetThirdStateTaxID(nonHUTaxID, countryCode, vatStatus)
				};
			}
		}

		public string InvoiceCategory => NormalInvoiceCategories.Contains(Model.Transaction.Category)
										? "NORMAL"        // hard coded string as required by XSD
										: AggregateInvoiceCategories.Contains(Model.Transaction.Category)
											? "AGGREGATE"     // hard coded string as required by XSD
											: string.Empty;

		public string InvoiceDeliveryDateFormatted => FormatNullable(Model.DeliveryDate, "yyyy-MM-dd"); // date format string as required by XSD;

		public string CurrencyCode => Model.Transaction.OSCurrency?.Code ?? ZString.Empty;

		public string ExchangeRateFormatted => FormatNullable(Model.Transaction.ExchangeRate, "F6"); // decimal format string as required by XSD;

		public string InvoiceAppearance
		{
			get
			{
				if (CustomerInfo.InvoiceDeliveryMethod.HasValue)
				{
					return CustomerInfo.InvoiceDeliveryMethod.ToString();
				}

				return CustomerInfo.IsPrivatePerson
					? nameof(InvoiceDeliveryMethod.UNKNOWN)
					: nameof(InvoiceDeliveryMethod.ELECTRONIC);
			}
		}

		public IEnumerable<InvoiceLineInfo> InvoiceLines => invoiceLines ?? (invoiceLines = GetLines());
		IEnumerable<InvoiceLineInfo> invoiceLines;

		public string InvoiceNetAmountFormatted => InvoiceSummary.NetAmountOS.ToString("F2", CultureInfo.InvariantCulture); // decimal format string as required by XSD;

		public string InvoiceNetAmountHUFFormatted => InvoiceSummary.NetAmountHUF.ToString("F2", CultureInfo.InvariantCulture); // decimal format string as required by XSD;

		public string InvoiceVatAmountFormatted => InvoiceSummary.VatAmountOS.ToString("F2", CultureInfo.InvariantCulture); // decimal format string as required by XSD;

		public string InvoiceVatAmountHUFFormatted => InvoiceSummary.VatAmountHUF.ToString("F2", CultureInfo.InvariantCulture); // decimal format string as required by XSD;

		public string PaymentDateFormatted => FormatNullable(Model.Transaction.DueDate, "yyyy-MM-dd");

		public string InvoiceGrossAmountFormatted => FormatNullable(Model.Transaction.OSTotal, "F2");

		public string InvoiceGrossAmountHUFFormatted => FormatNullable(Model.Transaction.LocalTotal, "F2");

		public IEnumerable<VatRateAggregateWithTypeInfo> InvoiceSummaryByVATRate => invoiceSummaryByVATRate ?? (invoiceSummaryByVATRate = GetAggregatedVATAmountsByVatRate());
		IEnumerable<VatRateAggregateWithTypeInfo> invoiceSummaryByVATRate;

		IEnumerable<InvoiceLineInfo> GetLines()
		{
			foreach (var indexedLine in IndexedLines)
			{
				var line = indexedLine.JournalLine;
				var vatRate = GetVatRateKey(line);
				var lineExchangeRate = CalculateLineExchangeRate(line, Model.CompanyIsReciprocal);
				yield return new InvoiceLineInfo()
				{
					LineDeliveryDateFormatted = FormatNullable(line.TaxDate, "yyyy-MM-dd"), // date format string as required by XSD
					LineDescription = FormatDescription(line.Description.GetValueOrDefault()), // decimal format string as required by XSD
					LineExchangeRateFormatted = FormatNullable(lineExchangeRate, "F6"),
					LineExpressionIndicator = (NoResString)"false", // hard coded string as required by XSD
					LineModificationReference = indexedLine.LineModificationReferenceIndex,
					LineModificationOperation = "CREATE",
					LineNatureIndicator = "SERVICE", // hard coded string as required by XSD
					LineNetAmountFormatted = FormatNullable(line.OSAmount, "F2"), // decimal format string as required by XSD
					lineNetAmountHUFFormatted = FormatNullable(line.LocalAmount, "F2"),
					LineNumber = indexedLine.Index,
					LineVatRate = vatRate.VATRateValue,
					VATType = vatRate.VATRateType,
					VATExemption = vatRate.VATExemption
				};
			}
		}

		string FormatDescription(ZString value)
		{
			var result = value.SubstringSafe(0, 512).Trim('\r', '\n').Replace("\n", " ").Replace("\r", " ");
			return new Regex("\\s+").Replace(result, " ");
		}

		ZDecimal? CalculateLineExchangeRate(PostingJournal line, bool isReciprocal)
		{
			// ChargeExchangeRate is not valid (= 1.0) when OSCurrency is different to ChargeCurrency.
			// So we calculate the exchange rate, as it isn't available on the PostingJournal
			var calculateExchangeRate = line.ChargeCurrency?.Code != line.OSCurrency?.Code
										&& line.OSAmount != 0m
										&& line.LocalAmount != 0m;
			if (calculateExchangeRate && isReciprocal)
			{
				return line.LocalAmount / line.OSAmount;
			}
			else if (calculateExchangeRate && !isReciprocal)
			{
				return line.OSAmount / line.LocalAmount;
			}
			else if (line.ChargeExchangeRate.HasValue)
			{
				return line.ChargeExchangeRate.Value;
			}
			else
			{
				return Model.Transaction.ExchangeRate;
			}
		}

		IEnumerable<PostingJournal> Lines => IndexedLines.Any() ? IndexedLines.Select(l => l.JournalLine) : Enumerable.Empty<PostingJournal>();

		IEnumerable<(PostingJournal JournalLine, int Index, int? LineModificationReferenceIndex)> IndexedLines => indexedLines ?? (indexedLines = GetIndexedTransactionLines());
		IEnumerable<(PostingJournal JournalLine, int Index, int? LineModificationReferenceIndex)> indexedLines;

		IEnumerable<(PostingJournal JournalLine, int Index, int? LineModificationReferenceIndex)> GetIndexedTransactionLines()
		{
			int? offset = null;
			if (Model.PreviousInvoices?.Any() ?? Model.Transaction?.TransactionType == TransactionType.CRD)
			{
				offset = Model.PreviousInvoices.Sum(x => x.MaximumLineSequence);
			}

			return (Model.Transaction?.PostingJournalCollection ?? Enumerable.Empty<PostingJournal>())
						.Where(l => l.LocalTotalAmount != 0) //exclude CMT lines
						.OrderBy(l => l.Sequence)
						.Select((l, i) => (l, i + 1, offset + i + 1));
		}

		VatRateAggregate InvoiceSummary => invoiceSummary ?? (invoiceSummary = GetVatRateAggregateInfo());
		VatRateAggregate invoiceSummary;

		VATRateKey GetVatRateKey(PostingJournal line)
		{
			var taxInfo = GetTaxInfo(line);

			(VATRateTagType tagType, string value, VATExemptionInfo vatExemptionInfo) result = default;
			if ((taxInfo.TaxTypeCode == AccTaxRate.Types.CapitalRated || taxInfo.TaxTypeCode == AccTaxRate.Types.Rated) && taxInfo.TaxRateAsRatio > 0m)
			{
				result = (VATRateTagType.VatPercentage, taxInfo.TaxRateAsRatio.ToString("F4", CultureInfo.InvariantCulture), null);  // decimal format string as required by XSD
			}
			else if (taxInfo.VATRateType == VATRateTagType.VatExemption)
			{
				result = (VATRateTagType.VatExemption, (line?.TaxMessageID?.Description).GetValueOrDefault().SubstringSafe(0, 50).TrimEnd(), GetVATExemptionInfoFromLine(taxInfo.GovtTaxGroupCode, taxInfo.TaxGroupDescription));
			}
			else if (taxInfo.VATRateType == VATRateTagType.VatOutOfScope)
			{
				result = (VATRateTagType.VatOutOfScope, (NoResString)"true", GetVATExemptionInfoFromLine(taxInfo.GovtTaxGroupCode, taxInfo.TaxGroupDescription));                  // hard coded string as required by XSD
			}
			else if (taxInfo.TaxTypeCode == AccTaxRate.Types.ReverseRated)
			{
				result = (VATRateTagType.VatDomesticReverseCharge, (NoResString)"true", null);       // hard coded string as required by XSD
			}
			else
			{
				result = (VATRateTagType.None, string.Empty, null);
			}

			return new VATRateKey()
			{
				VATExemption = result.vatExemptionInfo,
				VATRateType = result.tagType,
				VATRateValue = result.value
			};
		}

		InvoiceLineTaxInfo GetTaxInfo(PostingJournal line)
		{
			var taxTypeCode = (line?.VATTaxID?.TaxType?.Code).GetValueOrDefault();
			var taxRateAsRatio = (line?.VATTaxID?.TaxRate).GetValueOrDefault() / 100m;
			var (taxGroupCode, taxGroupDescription, govtTaxGroupCode) = GetTaxGroup(line?.TaxMessageID);

			var vatRateType = VATRateTagType.None;
			if (IsVATExempted(taxTypeCode, taxGroupCode, taxRateAsRatio))
			{
				vatRateType = VATRateTagType.VatExemption;
			}
			else if (IsVATOutOfScope(taxTypeCode, taxGroupCode))
			{
				vatRateType = VATRateTagType.VatOutOfScope;
			}

			return new InvoiceLineTaxInfo()
			{
				TaxTypeCode = taxTypeCode,
				TaxRateAsRatio = taxRateAsRatio,
				TaxGroupCode = taxGroupCode,
				TaxGroupDescription = taxGroupDescription,
				GovtTaxGroupCode = govtTaxGroupCode,
				VATRateType = vatRateType
			};
		}

		(ZString TaxGroupCode, ZString TaxGroupCodeDescription, ZString GovtTaxGroupCode) GetTaxGroup(TaxMessageID taxMessage)
		{
			var taxGroup = taxMessage?.TaxGroupCode;
			if (taxGroup != null)
			{
				var groupCode = taxGroup.Code ?? ZString.Empty;
				var groupDescription = taxGroup.Description ?? ZString.Empty;
				var huComplainceInfo = CountryComplianceFactory.GetITaxMessageGroupProvider(Core.Constants.CountryCodes.Hungary);
				var govtTaxCode = huComplainceInfo.GetTaxMessageGroup()
												.OfType<Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem>()
												.FirstOrDefault(c => c.Code == groupCode)?
												.RelatedItemCode ?? ZString.Empty;
				return (groupCode, groupDescription, govtTaxCode);
			}
			return (ZString.Empty, ZString.Empty, ZString.Empty);
		}

		bool IsVATExempted(ZString taxTypeCode, ZString taxGroupCode, decimal taxRatio) => IsVATExemptedTaxGroupCode(taxGroupCode);

		bool IsVATOutOfScope(ZString taxTypeCode, ZString taxGroupCode) => IsVATOutOfScopeTaxGroupCode(taxGroupCode);

		VATExemptionInfo GetVATExemptionInfoFromLine(ZString govtGroupCode, ZString description)
		{
			return !govtGroupCode.IsEmpty && !description.IsEmpty
			? new VATExemptionInfo()
			{
				Case = govtGroupCode,
				Reason = description
			}
			: null;
		}

		IEnumerable<VatRateAggregateWithTypeInfo> GetAggregatedVATAmountsByVatRate()
		{
			var amountsByVatRate = new List<VatRateAggregateWithTypeInfo>();
			foreach (var line in Lines)
			{
				var vatRateKey = GetVatRateKey(line);
				if (vatRateKey.VATRateType != VATRateTagType.None)
				{
					var vatRateInfo = amountsByVatRate.FirstOrDefault(vr => vr.Key.Match(vatRateKey));
					if (vatRateInfo == null)
					{
						vatRateInfo = new VatRateAggregateWithTypeInfo(vatRateKey);
						amountsByVatRate.Add(vatRateInfo);
					}
					vatRateInfo.Add(line);
				}
			}
			return amountsByVatRate;
		}

		VatRateAggregate GetVatRateAggregateInfo()
		{
			var aggregate = new VatRateAggregate();
			foreach (var line in Lines)
			{
				aggregate.Add(line);
			}
			return aggregate;
		}

		ZString? EmptyTaxNumber => null;

		ZString? EmptyPostCode => "0000";

		ZString? GetCommunityVatNumber(ZString? vatHU, ZString? nonHUTaxID, ZString? countryCode, ZString? vatStatus, bool isEUCountry)
		{
			if (nonHUTaxID.HasValue && IsCommunityVatNumberRequired(vatStatus, isEUCountry))
			{
				return !nonHUTaxID.Value.StartsWith(countryCode) ? (ZString?)FormattableString.Invariant($"{countryCode}{nonHUTaxID.Value}") : nonHUTaxID; // hard coded string as required by XSD;
			}
			return null;
		}

		bool IsCommunityVatNumberRequired(ZString? vatStatus, bool isEUCountry) =>
			vatStatus.HasValue && vatStatus.Value == Vat_Status_Other && isEUCountry;

		ZString? GetThirdStateTaxID(ZString? nonHUTaxID, ZString? countryCode, ZString? vatStatus) =>
			(vatStatus.HasValue && vatStatus.Value == Vat_Status_Other && !Model.IsCustomerInEUCountry && nonHUTaxID.HasValue) ? nonHUTaxID : null;

		ZString? GetVatStatus(ZString? vatHU, ZString? gbrHU, bool checkIsPrivatePerson)
		{
			if (checkIsPrivatePerson && Model.IsCustomerPrivatePerson)
			{
				return Vat_Status_Private;
			}
			else if (vatHU.HasValue || gbrHU.HasValue)
			{
				return Vat_Status_Domestic;
			}
			else
			{
				return Vat_Status_Other;
			}
		}
		string FormatNullable(ZDateTime? dt, string formatString) =>
			dt.HasValue ? dt.Value.ToString(formatString, CultureInfo.InvariantCulture) : string.Empty;

		string FormatNullable(ZDecimal? dt, string formatString) =>
			dt.HasValue ? dt.Value.ToString(formatString, CultureInfo.InvariantCulture) : string.Empty;

		#region Default Implementations

		bool IsVATExemptedTaxGroupCode(ZString taxGroupCode) =>
			new ZString[]
			{
				TaxMessageGroupCodes.H01,
				TaxMessageGroupCodes.H02,
				TaxMessageGroupCodes.H03,
				TaxMessageGroupCodes.H04,
				TaxMessageGroupCodes.H05,
				TaxMessageGroupCodes.H06
			}.Contains(taxGroupCode);

		bool IsVATOutOfScopeTaxGroupCode(ZString taxGroupCode) =>
			new ZString[]
			{
				TaxMessageGroupCodes.H21,
				TaxMessageGroupCodes.H22,
				TaxMessageGroupCodes.H23,
				TaxMessageGroupCodes.H24,
				TaxMessageGroupCodes.H25,
				TaxMessageGroupCodes.H26
			}.Contains(taxGroupCode);

		#endregion

		#region Static Data

		static readonly ImmutableHashSet<string> NormalInvoiceCategories = new[]
		{
			InvoiceTypesList.Codes.FinalInvoice,
			InvoiceTypesList.Codes.ForeignCurrencyInvoice,
			InvoiceTypesList.Codes.FreightInvoice,
			InvoiceTypesList.Codes.DestinationChargesInvoice,
			InvoiceTypesList.Codes.InvoicePerTaxCode,
			InvoiceTypesList.Codes.SelfBillingInvoice,
			InvoiceTypesList.Codes.DisbursementInForeignCurrency,
			InvoiceTypesList.Codes.DisbursementInvoice,
		}.ToImmutableHashSet();

		static readonly ImmutableHashSet<string> AggregateInvoiceCategories = new[]
		{
			InvoiceTypesList.Codes.FinalInvoice_Batching,
			InvoiceTypesList.Codes.DisbursementInvoice_Batching,
			InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
			InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
			InvoiceTypesList.Codes.FreightInvoice_Batching,
			InvoiceTypesList.Codes.InvoicePerTaxCode_Batching,
			InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
			InvoiceTypesList.Codes.SelfBillingInvoice_Batching,
		}.ToImmutableHashSet();

		#endregion

		const string Vat_Status_Private = "PRIVATE_PERSON";
		const string Vat_Status_Domestic = "DOMESTIC";
		const string Vat_Status_Other = "OTHER";
	}
}
