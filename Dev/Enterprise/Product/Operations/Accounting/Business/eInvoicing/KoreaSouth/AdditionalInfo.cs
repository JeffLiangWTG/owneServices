using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public partial class AdditionalInfo
	{
		public IEnumerable<ZString> DescriptionText { get; set; }
	}

	public partial class AdditionalInfo : ITaxInvoiceDocumentTypeAdditionalInfo
	{
		public ZString IssueID { get; set; }

		public ZString OriginalIssueID { get; set; }

		public ZString AmendStatusCode { get; set; }

		public ZString IssueDateTime { get; set; }

		public ZString SpecifiedPaymentMeansTypeCode { get; set; }

		public ZString SpecifiedPaymentMeansPaidAmount { get; set; }

		public ZString SpecifiedMonetarySummationChargeTotalAmount { get; set; }

		public ZString SpecifiedMonetarySummationTaxTotalAmount { get; set; }

		public ZString SpecifiedMonetarySummationGrandTotalAmount { get; set; }

		public ZString InvoiceeAlienRegistrationNo { get; set; }

		public ZString InvoiceePassportNo { get; set; }

		public List<TaxInvoiceTradeLineItem> Lines { get; set; }

		public ZString FullTypeCode { get; set; }

		public ZString PaymentStatus { get; set; }
	}

	public class TaxInvoiceTradeLineItem
	{
		public ZString DescriptionText { get; set; }

		public ZString InvoiceAmount { get; set; }

		public ZString CalculatedAmount { get; set; }

		public ZString JobNumber { get; set; }

		public ZInt Sequence { get; set; }

		public ZDateTime ReverseDate { get; set; }

		public ZString PurchaseExpiryDateTime { get; set; }

		public ZString NameText { get; set; }
	}

	partial class AdditionalInfo : IInvoiceePartyAdditionalInfo
	{
		public ZString InvoiceeID { get; set; }

		public ZString InvoiceeTypeCode { get; set; }

		public ZString InvoiceeNameText { get; set; }

		public ZString InvoiceeClassificationCode { get; set; }

		public ZString InvoiceeTaxRegistrationID { get; set; }

		public ZString InvoiceeSpecifiedPersonNameText { get; set; }

		public ZString InvoiceePrimaryDefinedContactPersonName { get; set; }

		public ZString InvoiceePrimaryDefinedContactTel { get; set; }

		public ZString InvoiceePrimaryDefinedContactURICommunication { get; set; }

		public ZString InvoiceeSecondaryDefinedContactPersonName { get; set; }

		public ZString InvoiceeSecondaryDefinedContactTel { get; set; }

		public ZString InvoiceeSecondaryDefinedContactURICommunication { get; set; }

		public ZString InvoiceeSpecifiedAddressLineOneText { get; set; }

		public ZString InvoiceeBusinessTypeCode { get; set; }

		ZString IInvoiceePartyAdditionalInfo.ID => InvoiceeID;

		ZString IInvoiceePartyAdditionalInfo.TypeCode => InvoiceeTypeCode;

		ZString IInvoiceePartyAdditionalInfo.NameText => InvoiceeNameText;

		ZString IInvoiceePartyAdditionalInfo.ClassificationCode => InvoiceeClassificationCode;

		ZString IInvoiceePartyAdditionalInfo.TaxRegistrationID => InvoiceeTaxRegistrationID;

		ZString IInvoiceePartyAdditionalInfo.SpecifiedPersonNameText => InvoiceeSpecifiedPersonNameText;

		ZString IInvoiceePartyAdditionalInfo.PrimaryDefinedContactPersonName => InvoiceePrimaryDefinedContactPersonName;

		ZString IInvoiceePartyAdditionalInfo.PrimaryDefinedContactTel => InvoiceePrimaryDefinedContactTel;

		ZString IInvoiceePartyAdditionalInfo.PrimaryDefinedContactURICommunication => InvoiceePrimaryDefinedContactURICommunication;

		ZString IInvoiceePartyAdditionalInfo.SecondaryDefinedContactPersonName => InvoiceeSecondaryDefinedContactPersonName;

		ZString IInvoiceePartyAdditionalInfo.SecondaryDefinedContactTel => InvoiceeSecondaryDefinedContactTel;

		ZString IInvoiceePartyAdditionalInfo.SecondaryDefinedContactURICommunication => InvoiceeSecondaryDefinedContactURICommunication;

		ZString IInvoiceePartyAdditionalInfo.SpecifiedAddressLineOneText => InvoiceeSpecifiedAddressLineOneText;

		ZString IInvoiceePartyAdditionalInfo.BusinessTypeCode => InvoiceeBusinessTypeCode;
	}

	partial class AdditionalInfo : IInvoicerPartyAdditionalInfo
	{
		public ZString InvoicerID { get; set; }

		public ZString InvoicerTypeCode { get; set; }

		public ZString InvoicerNameText { get; set; }

		public ZString InvoicerClassificationCode { get; set; }

		public ZString InvoicerTaxRegistrationID { get; set; }

		public ZString InvoicerSpecifiedPersonNameText { get; set; }

		public ZString InvoicerDefinedContactPersonName { get; set; }

		public ZString InvoicerDefinedContactTel { get; set; }

		public ZString InvoicerDefinedContactURICommunication { get; set; }

		public ZString InvoicerSpecifiedAddressLineOneText { get; set; }

		ZString IInvoicerPartyAdditionalInfo.ID => InvoicerID;

		ZString IInvoicerPartyAdditionalInfo.TypeCode => InvoicerTypeCode;

		ZString IInvoicerPartyAdditionalInfo.NameText => InvoicerNameText;

		ZString IInvoicerPartyAdditionalInfo.ClassificationCode => InvoicerClassificationCode;

		ZString IInvoicerPartyAdditionalInfo.TaxRegistrationID => InvoicerTaxRegistrationID;

		ZString IInvoicerPartyAdditionalInfo.SpecifiedPersonNameText => InvoicerSpecifiedPersonNameText;

		ZString IInvoicerPartyAdditionalInfo.DefinedContactPersonName => InvoicerDefinedContactPersonName;

		ZString IInvoicerPartyAdditionalInfo.DefinedContactTel => InvoicerDefinedContactTel;

		ZString IInvoicerPartyAdditionalInfo.DefinedContactURICommunication => InvoicerDefinedContactURICommunication;

		ZString IInvoicerPartyAdditionalInfo.SpecifiedAddressLineOneText => InvoicerSpecifiedAddressLineOneText;
	}
}
