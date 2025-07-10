using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5FYMessageData
	{
		ZString SumPaymentNumber { get; }
		ZString BankAccountNumber { get; }
		ZString ImporterID { get; }
		ZString ImporterType { get; }
		ZString ImporterCompanyName { get; }
		ZString ImporterRepresentativeName { get; }
		ZString ImporterAddressLine1 { get; }
		ZString DeclarantID { get; }
		ZString CustomsOfficeName { get; }
		ZDate ExpirationDate { get; }
		ZDate NoticeDate { get; }
		ZDecimal TotalDutyAmount { get; }
		ZDecimal TotalVAT { get; }
		ZDecimal TotalLiquorTax { get; }
		ZDecimal TotalAgricultureTax { get; }
		ZDecimal TotalSpecialConsumptionTax { get; }
		ZDecimal TotalTransportationTax { get; }
		ZDecimal TotalEducationTax { get; }
		ZDecimal PenaltyForLateDeclaration { get; }
		ZDecimal PenaltyForMissedDeclarationb { get; }
		ZDecimal TotalPayableAmount { get; }
		IEnumerable<IGOVCBR5FYDeclaration> TotalDutyTax { get; }
	}

	interface IGOVCBR5FYDeclaration
	{
		ZShort SequenceNumber { get; }
		ZString ImportDeclarationNumber { get; }
		ZDecimal TotalDutyAndTax { get; }
		ZString PaymentReferenceNumber { get; }
		IEnumerable<IDutyTax> DutyTaxes { get; }
	}

	interface IDutyTax
	{
		ZString DutyTaxType { get; }
		ZDecimal DutyTaxFee { get; }
	}

	class GOVCBR5FYMessageData : IGOVCBR5FYMessageData
	{
		public ZString SumPaymentNumber { get; set; }
		public ZString BankAccountNumber { get; set; }
		public ZString ImporterID { get; set; }
		public ZString ImporterType { get; set; }
		public ZString ImporterCompanyName { get; set; }
		public ZString ImporterRepresentativeName { get; set; }
		public ZString ImporterAddressLine1 { get; set; }
		public ZString DeclarantID { get; set; }
		public ZString CustomsOfficeName { get; set; }
		public ZDate ExpirationDate { get; set; }
		public ZDate NoticeDate { get; set; }
		public ZDecimal TotalDutyAmount { get; set; }
		public ZDecimal TotalVAT { get; set; }
		public ZDecimal TotalLiquorTax { get; set; }
		public ZDecimal TotalAgricultureTax { get; set; }
		public ZDecimal TotalSpecialConsumptionTax { get; set; }
		public ZDecimal TotalTransportationTax { get; set; }
		public ZDecimal TotalEducationTax { get; set; }
		public ZDecimal PenaltyForLateDeclaration { get; set; }
		public ZDecimal PenaltyForMissedDeclarationb { get; set; }
		public ZDecimal TotalPayableAmount { get; set; }
		public IEnumerable<IGOVCBR5FYDeclaration> TotalDutyTax { get; set; }
	}
	class GOVCBR5FYDeclaration : IGOVCBR5FYDeclaration
	{
		public ZShort SequenceNumber { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZDecimal TotalDutyAndTax { get; set; }
		public ZString PaymentReferenceNumber { get; set; }
		public IEnumerable<IDutyTax> DutyTaxes { get; set; }
	}

	class DutyTax : IDutyTax
	{
		public ZString DutyTaxType { get; set; }
		public ZDecimal DutyTaxFee { get; set; }
	}
}
