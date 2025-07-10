using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5UOMessageData
	{
		ZString RefundDeclarationNumber { get; }
		ZString ImportCompanyName { get; }
		ZString ImportRepresentativeName { get; }
		ZString FirstImportAddressLine { get; }
		ZString SecondImportAddressLine { get; }
		ZString CustomsOfficeName { get; }
		ZString CustomsDivisionName { get; }
		ZDate SubmissionDate { get; }
		ZDate ApprovalDate { get; }
		ZString ApprovalNo { get; }
		ZString BankCodeName { get; }
		ZString BankCodeName2 { get; }
		ZString BankAccountNumber { get; }
		ZDate NoticeDate { get; }
	}

	public class GOVCBR5UOMessageData : NonPersistentBusinessObject, IGOVCBR5UOMessageData
	{
		public GOVCBR5UOMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString RefundDeclarationNumber { get; set; }
		public ZString ImportCompanyName { get; set; }
		public ZString ImportRepresentativeName { get; set; }
		public ZString FirstImportAddressLine { get; set; }
		public ZString SecondImportAddressLine { get; set; }
		public ZString CustomsOfficeName { get; set; }
		public ZString CustomsDivisionName { get; set; }
		public ZDate SubmissionDate { get; set; }
		public ZDate ApprovalDate { get; set; }
		public ZString ApprovalNo { get; set; }
		public ZString BankCodeName { get; set; }
		public ZString BankCodeName2 { get; set; }
		public ZString BankAccountNumber { get; set; }
		public ZDate NoticeDate { get; set; }

		public ZDecimal DutyAmount { get; set; }
		public ZDecimal SpecialConsumptionTax { get; set; }
		public ZDecimal TransportationTax { get; set; }
		public ZDecimal LiquorTax { get; set; }
		public ZDecimal EducationTax { get; set; }
		public ZDecimal AgricultureTax { get; set; }
		public ZDecimal VAT { get; set; }
		public ZDecimal Penalty { get; set; }
		public ZDecimal NonDutyTaxRevenue { get; set; }
		public ZDecimal TotalAmount { get; set; }
		public ZDecimal RefundAmount { get; set; }
	}
}
