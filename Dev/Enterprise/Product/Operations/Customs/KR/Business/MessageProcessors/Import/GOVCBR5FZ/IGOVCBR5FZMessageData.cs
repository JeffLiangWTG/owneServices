using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5FZMessageData
	{
		ZString TaxInvoiceCode { get; }
		ZString TaxInvoiceType { get; }
		ZString TaxInvoiceNumber { get; }
		ZString SumPaymentNumber { get; }
		ZString CustomsOfficeID { get; }
		ZString CustomsOfficeName { get; }
		ZString CustomsOfficeAddressLine1 { get; }
		ZString ImporterID { get; }
		ZString ImporterType { get; }
		ZString ImporterCompanyName { get; }
		ZString ImporterRepresentativeName { get; }
		ZString ImporterAddressLine { get; }
		ZDate PaymentDate { get; }
		ZString BlankCount { get; }
		ZDecimal TotalVATBaseAmount { get; }
		ZDecimal TotalVAT { get; }
		ZDate PeriodStartDate { get; }
		ZDate PeriodEndDate { get; }
		ZString Remark { get; }
		ZDecimal TotalNumber { get; }
		IEnumerable<IGOVCBR5FZDeclaration> Declarations { get; }
	}

	interface IGOVCBR5FZDeclaration
	{
		ZShort SequenceNumber { get; }
		ZString ImportDeclarationNumber { get; }
		ZString PaymentNumber { get; }
		ZDecimal VATBaseAmount { get; }
		ZDecimal Vat { get; }
		ZDate PaymentDate { get; }
	}

	class GOVCBR5FZMessageData : IGOVCBR5FZMessageData
	{
		public ZString TaxInvoiceCode { get; set; }
		public ZString TaxInvoiceType { get; set; }
		public ZString TaxInvoiceNumber { get; set; }
		public ZString SumPaymentNumber { get; set; }
		public ZString CustomsOfficeID { get; set; }
		public ZString CustomsOfficeName { get; set; }
		public ZString CustomsOfficeAddressLine1 { get; set; }
		public ZString ImporterID { get; set; }
		public ZString ImporterType { get; set; }
		public ZString ImporterCompanyName { get; set; }
		public ZString ImporterRepresentativeName { get; set; }
		public ZString ImporterAddressLine { get; set; }
		public ZDate PaymentDate { get; set; }
		public ZString BlankCount { get; set; }
		public ZDecimal TotalVATBaseAmount { get; set; }
		public ZDecimal TotalVAT { get; set; }
		public ZDate PeriodStartDate { get; set; }
		public ZDate PeriodEndDate { get; set; }
		public ZString Remark { get; set; }
		public ZDecimal TotalNumber { get; set; }
		public IEnumerable<IGOVCBR5FZDeclaration> Declarations { get; set; }
	}

	class GOVCBR5FZDeclaration : IGOVCBR5FZDeclaration
	{
		public ZShort SequenceNumber { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString PaymentNumber { get; set; }
		public ZDecimal VATBaseAmount { get; set; }
		public ZDecimal Vat { get; set; }
		public ZDate PaymentDate { get; set; }
	}
}
