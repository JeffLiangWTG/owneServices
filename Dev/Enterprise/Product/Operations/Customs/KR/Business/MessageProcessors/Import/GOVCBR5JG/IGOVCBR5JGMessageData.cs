using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5JGMessageData
	{
		ZString PaymentNumber { get; }
		ZString ImportCompanyName { get; }
		ZString ImportRepresentativeName { get; }
		ZString ImportAddressLine1 { get; }
		ZString CustomsOffice { get; }
		ZDate PaymentDate { get; }
		ZString DutyTaxFeeType { get; }
		ZString BankAccountNumber { get; }
		ZString CustomsOfficeBankAccountNumber { get; }
		ZDecimal NoticeAmount { get; }
		ZDecimal TemporaryOpeningFee { get; }
		ZDecimal PermissionApplicationFee { get; }
		ZDecimal InspectionFee { get; }
		ZString ElectronNoticeNumber { get; }
		ZString CarrierID { get; }
		ZString AgentID { get; }
		ZString DeclarantID { get; }
	}

	class GOVCBR5JGMessageData : IGOVCBR5JGMessageData
	{
		public ZString PaymentNumber { get; set; }
		public ZString ImportCompanyName { get; set; }
		public ZString ImportRepresentativeName { get; set; }
		public ZString ImportAddressLine1 { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZDate PaymentDate { get; set; }
		public ZString DutyTaxFeeType { get; set; }
		public ZString BankAccountNumber { get; set; }
		public ZString CustomsOfficeBankAccountNumber { get; set; }
		public ZDecimal NoticeAmount { get; set; }
		public ZDecimal TemporaryOpeningFee { get; set; }
		public ZDecimal InspectionFee { get; set; }
		public ZDecimal PermissionApplicationFee { get; set; }
		public ZString ElectronNoticeNumber { get; set; }
		public ZString CarrierID { get; set; }
		public ZString AgentID { get; set; }
		public ZString DeclarantID { get; set; }
	}
}
