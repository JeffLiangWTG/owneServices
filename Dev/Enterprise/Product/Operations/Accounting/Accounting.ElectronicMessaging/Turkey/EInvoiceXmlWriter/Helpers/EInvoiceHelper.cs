using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.Shared.Universal;
using Constants = Enterprise.Core.Constants;
using OrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniveralRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class EInvoiceHelper
	{
		public EInvoiceHelper(TransactionInfo uInvoice, GlbCompany company)
		{
			UInvoice = uInvoice;
			Company = company;
			Factory = company.Factory;
			DecimalsOfInvoiceCurrency = GetRefCurrency().Decimals;
			StringFormatDecimalsOfInvoiceCurrency = "0." + "000000".Substring(0, DecimalsOfInvoiceCurrency);
			NonCommentInvoiceLines = FilterOutCommentInvoiceLines();
			CommentInvoiceLines = FilterCommentInvoiceLines();
			Debtor = GetOrgHeader(UInvoice.OrganizationAddress);
			var organizationCategory = (Debtor?.OH_Category ?? ZString.Empty);
			Category = organizationCategory == OrgConstants.Category.NaturalPersonIndividual
				? OrgCategory.Person
				: organizationCategory == OrgConstants.Category.Government
				? OrgCategory.Government
				: OrgCategory.Company;
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
			CustomerVATCode = GetOrgCusCodeValue(CountryCodes.Turkey, new ZString[] { OrgCusCode.CodeTypes.VATCode })?.Value.Value ?? ZString.Empty;
			CustomerTCKN = GetOrgCusCodeValue(CountryCodes.Turkey, new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.TCK })?.Value.Value ?? ZString.Empty;
			TaxMessages = Factory.Load<AccInvMsg>(new ZQuery(AccInvMsgSchema.A9_RN_NKCountryCode, CountryCodes.Turkey)).ToDictionary(x => x.A9_Code, x => x);
		}

		public BusinessObjectFactory Factory { get; }
		int DecimalsOfInvoiceCurrency { get; }
		public string StringFormatDecimalsOfInvoiceCurrency { get; }
		public GlbCompany Company { get; }
		public TransactionInfo UInvoice { get; }
		public List<PostingJournal> NonCommentInvoiceLines { get; set; }
		public List<PostingJournal> CommentInvoiceLines { get; set; }
		public string CustomerVATCode { get; }
		public string CustomerTCKN { get; }
		public OrgHeader Debtor { get; }
		Dictionary<ZString, AccInvMsg> TaxMessages { get; }

		enum OrgCategory { Company, Person, Government }
		readonly OrgCategory Category;
		public bool IsOrganizationCategoryNAT => Category == OrgCategory.Person;
		public bool IsOrganizationCategoryGOV => Category == OrgCategory.Government;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Turkish Language")]
		const string DefaultExemptionReasonMessage = "KDV den istisnadır";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Turkish Language")]
		public const string Sell = "Satış";

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif

		List<PostingJournal> FilterOutCommentInvoiceLines()
		{
			var sql = FormattableString.Invariant($@"SELECT {AccChargeCodeSchema.Constants.AC_Code} 
													FROM {AccChargeCodeSchema.Constants.SqlSchemaName}.{AccChargeCodeSchema.Constants.TableName}
													WHERE {AccChargeCodeSchema.Constants.AC_GC} = @companyPK
														AND {AccChargeCodeSchema.Constants.AC_ChargeType} = '{ChargeType.Comment}'"); // Part of SQL expression

			var @params = new ZSqlParameterCollection();
			@params.Add("@companyPK", Company.PK.ToGuid(), AccChargeCodeSchema.AC_GC);

			var cmtCodes = new DynamicBusinessObjectCollection(Factory);
			cmtCodes.Load(sql, @params);
			var cmtChargeCodeList = cmtCodes.Select(bizo => new ZString(bizo[AccChargeCodeSchema.Constants.AC_Code])).ToHashSet();

			return UInvoice.PostingJournalCollection
				.Where(line => !cmtChargeCodeList.Contains(line.ChargeCode?.Code?.SourceValue ?? ZString.Empty) && (line.ChargeCode?.ChargeType?.Code ?? string.Empty) != ChargeType.Comment)
				.ToList();
		}

		List<PostingJournal> FilterCommentInvoiceLines()
			=> UInvoice.PostingJournalCollection
			.Where(line => (line.ChargeCode?.ChargeType?.Code ?? string.Empty) == ChargeType.Comment)
			.ToList();

		internal TaxSubtotalType CreateTaxRateSubtotal(decimal taxableAmount, decimal taxAmount, decimal taxRate, string schemaName, string schemaTypeCode, string taxExemptionReasonCode = "", string taxExemptionReasonDescription = "")
		{
			var subTotal = new TaxSubtotalType();
			subTotal.TaxableAmount = new TaxableAmountType() { Value = FixDecimalPlacesAndSign(taxableAmount), currencyID = UInvoice.OSCurrency.Code.Value };
			subTotal.TaxAmount = new TaxAmountType() { Value = FixDecimalPlacesAndSign(taxAmount), currencyID = UInvoice.OSCurrency.Code.Value };
			subTotal.Percent = new PercentType1() { Value = taxRate };
			subTotal.TaxCategory = new TaxCategoryType();
			subTotal.TaxCategory.TaxScheme = new TaxSchemeType();
			subTotal.TaxCategory.TaxScheme.Name = new NameType1() { Value = schemaName };
			subTotal.TaxCategory.TaxScheme.TaxTypeCode = new TaxTypeCodeType() { Value = schemaTypeCode };
			if (!taxExemptionReasonCode.IsNullOrEmpty())
			{
				subTotal.TaxCategory.TaxExemptionReasonCode = new TaxExemptionReasonCodeType() { Value = taxExemptionReasonCode };
				subTotal.TaxCategory.TaxExemptionReason = new TaxExemptionReasonType() { Value = string.IsNullOrEmpty(taxExemptionReasonDescription) ? GetTaxMessage(taxExemptionReasonCode) : taxExemptionReasonDescription };
			}
			return subTotal;
		}

		internal AccBankAccount GetBankAccount() => Debtor != null ? AccBankAccount.GetDefaultReceiptBankAccountForDebtor(Debtor.PK, UInvoice.OSCurrency.Code.Value, GlbBranch.CurrentBranch, Factory) : null;

		internal string GetStateDescriptionByCountry(string countryCode, string stateCode) => TransactionInfoHelper.GetStateDescriptionByCountry(countryCode, stateCode, Factory) ?? ZString.Empty;

		internal UniveralRegistrationNumber GetOrgCusCodeValue(List<UniveralRegistrationNumber> registrationNumberCollection, string countryCode, params ZString[] cusCodeTypes) =>
			registrationNumberCollection?.FirstOrDefault(x => (cusCodeTypes?.Contains(x.Type.Code.Value) ?? false) && x.CountryOfIssue.Code.Value == countryCode);

		internal UniveralRegistrationNumber GetOrgCusCodeValue(string countryCode, params ZString[] cusCodeTypes) =>
			GetOrgCusCodeValue(UInvoice.OrganizationAddress.RegistrationNumberCollection, countryCode, cusCodeTypes);

		internal bool HasTurkeyCusCode(ZString cusCodeType) =>
			GetOrgCusCodeValue(CountryCodes.Turkey, cusCodeType) != null;

		internal OrgHeader GetOrgHeader(OrganizationAddress transactionAddress)
		{
			OrgHeader result = null;
			var orgCode = transactionAddress?.OrganizationCode?.SourceValue;
			if (orgCode.HasValue)
			{
				result = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode.Value);
			}
			return result;
		}

		internal bool CheckBatchAndPivotDiscarded(ZGuid batchID) =>
			Factory.Load<AccEInvoicingBatch>(batchID).AIB_Status == Constants.EInvoicingBatchState.Discarded &&
			Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, batchID)).AIP_Status == Constants.EInvoicingBatchState.Discarded;

		internal bool HasWithholdingTax(PostingJournal postingJournal) =>
			(postingJournal?.VATTaxID?.TaxRate ?? ZDecimal.Zero) != ZDecimal.Zero && (postingJournal?.VATTaxID?.ExtraTaxRate ?? ZDecimal.Zero) != ZDecimal.Zero;

		internal ZDecimal ConvertToPositiveValue(ZDecimal? value) => value.HasValue ? Math.Abs(value.Value) : (decimal)ZDecimal.Zero;

		internal string CurrencyToString()
		{
			var currency = GetRefCurrency();
			var numbertoStringTR = new NumberToString_TR_TR();
			var builder = new StringBuilder((NoResString)"Yalnız"); // Turkish Language
			var main = Math.Truncate(UInvoice.OSTotal.Value);
			if (main > 0)
			{
				builder.Append(" " + numbertoStringTR.GetNumberAsString((long)main) + " " + currency.Code);
			}

			var dec = UInvoice.OSTotal.Value - main;
			if (dec > 0)
			{
				builder.Append(" " + numbertoStringTR.GetNumberAsString((long)Math.Truncate(dec * 100)) + " " + currency.RX_SubUnitNameMultilingual.ToString("TR-TR"));
			}

			return builder.ToString();
		}

		internal string CheckAddressForNull(ZString? address1, ZString? address2) => ((address1 ?? ZString.Empty).TrimEnd() + " " + (address2 ?? ZString.Empty).TrimEnd()).TrimEnd();

		internal RefCurrency GetRefCurrency()
			=> Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, UInvoice.OSCurrency.Code.Value);

		internal decimal FixDecimalPlacesAndSign(decimal value)
		{
			var dec = Utilities.Round(value, DecimalsOfInvoiceCurrency);
			var str = dec.ToString(StringFormatDecimalsOfInvoiceCurrency);
			return ConvertToPositiveValue(decimal.Parse(str));
		}

		internal TransactionHeader TransactionHeader =>
				Factory.LoadTop1<TransactionHeader>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, UInvoice.Number ?? ZString.Empty),
													new ZQuery(AccTransactionHeaderSchema.AH_GC, Company.PK)));

		ZString GetTaxMessage(ZString taxMsgCode)
			=> taxMsgCode == TurkeyComplianceInfo.EInvoiceTaxCategoryConstants.DefaultExemptionReasonCode
			? DefaultExemptionReasonMessage
			: !TaxMessages.TryGetValue(taxMsgCode, out var taxMessage)
			? ZString.Empty
			: !taxMessage.A9_LocalMsg.IsEmpty
			? taxMessage.A9_LocalMsg
			: (ZString)taxMessage.A9_EnglishMsgMultilingual.ToString("TR-TR");

		public void CollectShipments(DataObjectList<Shipment> shipments, List<Shipment> collectedShipments, string mainShipmentType = "", string mainShipmentNumber = "")
		{
			shipments?.ForEach(shipment =>
			{
				if ((string.IsNullOrEmpty(mainShipmentType) && string.IsNullOrEmpty(mainShipmentNumber)) ||
					((DoesShipmentNumberExistInDataContext(shipment, mainShipmentNumber) || mainShipmentType == CFSLoadListConsol) && !collectedShipments.Contains(shipment)))
				{
					collectedShipments.Add(shipment);
				}
				CollectShipments(shipment.SubShipmentCollection, collectedShipments, mainShipmentType, mainShipmentNumber);
			});
		}

		bool DoesShipmentNumberExistInDataContext(Shipment shipment, ZString shipmentNumber)
		=> shipment.DataContext.DataSourceCollection.Any(x => x.Key.Value == shipmentNumber);

		public ZString? GetCustomsOfficeDescription(string code) =>
			ObjectFactory.Get<IRefCusCodeListTypesListProvider>()
			.GetList(Factory, CountryCodes.Turkey, "CUSOF", ZDateTime.Now.Date)
			.GetDescriptionFromCode(code);

		const string CFSLoadListConsol = nameof(CFSLoadListConsol);
	}

	#region Extension

	static class Extensions
	{
		public static ZStringBuilder AppendIfNotEmptyAndNotExists(this ZStringBuilder stringBuilder, string value)
		{
			if (!string.IsNullOrEmpty(value) && !stringBuilder.ToStringWithDelimiterBetweenAppends(Comma).Contains(value))
			{
				stringBuilder.Append(value);
			}
			return stringBuilder;
		}

		public static void Add(this Dictionary<ZString, ZStringBuilder> dictionary, string key, params string[] values)
		{
			var note = new ZStringBuilder();
			values.ForEach(x => note.AppendIfNotEmptyAndNotExists(x));
			if (!note.IsEmpty)
			{
				if (!dictionary.TryGetValue(key, out ZStringBuilder stringBuilder))
				{
					stringBuilder = new ZStringBuilder();
					dictionary[key] = stringBuilder;
				}
				stringBuilder.AppendIfNotEmptyAndNotExists(note.ToStringWithDelimiterBetweenAppends(" - "));
			}
		}

		public static void AddRangeIfNotNull<T>(this List<T> list, List<T> objectCollection)
		{
			if (objectCollection != null)
			{
				list.AddRange(objectCollection);
			}
		}

		public static void AddByType(this Dictionary<ZString, ZLong> dictionary, string key, ZLong? valueToAdd)
		{
			if (string.IsNullOrEmpty(key) || !valueToAdd.HasValue || valueToAdd.Value.IsEmpty)
			{
				return;
			}

			if (dictionary.TryGetValue(key, out ZLong value))
			{
				dictionary[key] = value + valueToAdd.Value;
			}
			else
			{
				dictionary[key] = valueToAdd.Value;
			}
		}

		public static void AddByType(this Dictionary<ZString, ZDecimal> dictionary, string key, ZDecimal? valueToAdd)
		{
			if (string.IsNullOrEmpty(key) || !valueToAdd.HasValue || valueToAdd.Value.IsEmpty)
			{
				return;
			}

			if (dictionary.TryGetValue(key, out ZDecimal value))
			{
				dictionary[key] = value + valueToAdd.Value;
			}
			else
			{
				dictionary[key] = valueToAdd.Value;
			}
		}

		public static string GetTaxMessageCodeWithFallback(this PostingJournal postingJournal, string fallbackValue = "")
			=> postingJournal.TaxMessageID?.TaxGroupCode?.GovernmentCode ?? postingJournal.TaxMessageID?.TaxMessageCode ?? fallbackValue;

		public static string GetTaxMessageDescriptionWithFallback(this PostingJournal postingJournal, string fallbackValue = "")
			=> postingJournal.TaxMessageID?.TaxGroupCode?.Description ?? fallbackValue;

		public static string FallbackToEmptyStringIfNull(this ZString? value)
			=> value.ToString() ?? string.Empty;

		public static string FallbackToEmptyStringIfNull(this ZDateTime? value)
			=> value != null ? value?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) : string.Empty;

		public static string FallbackToEmptyStringIfZeroOrNull(this int value)
			=> value > ZInt.Zero ? value.ToString() : string.Empty;

		public static string FallbackToEmptyStringIfZeroOrNull(this ZDecimal? value)
			=> value != null && value > ZDecimal.Zero ? value.ToString() : string.Empty;

		public const string Comma = ",";
	}

	#endregion
}
