using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5UNMessageData
	{
		ZString RefundDeclarationNumber { get; }
		ZString ImportCompanyName { get; }
		ZString ImportRepresentativeName { get; }
		ZString ImportAddressLine1 { get; }
		ZString ImportAddressLine2 { get; }
		ZString CustomsOfficeName { get; }
		ZString CustomsDivisionName { get; }
		ZDate SubmissionDate { get; }
		ZDate ApprovalDate { get; }
		ZDate PaymentDate { get; }
		ZString ApprovalNo { get; }
		ZString BankAccountNumber { get; }
		ZString NoticeNumber { get; }
		IReadOnlyDictionary<ZString, IDutyTaxFee> DutyTax { get; }
	}
	public interface IDutyTaxFee
	{
		ZString DutyTaxType { get; set; }
		ZDecimal OriginalAmount { get; set; }
		ZDecimal SupplementaryAmount { get; set; }
		ZDecimal DifferenceAmount { get; set; }
	}
	public class GOVCBR5UNMessageData : IGOVCBR5UNMessageData
	{
		public ZString RefundDeclarationNumber { get; set; }
		public ZString ImportCompanyName { get; set; }
		public ZString ImportRepresentativeName { get; set; }
		public ZString ImportAddressLine1 { get; set; }
		public ZString ImportAddressLine2 { get; set; }
		public ZString CustomsOfficeName { get; set; }
		public ZString CustomsDivisionName { get; set; }
		public ZDate SubmissionDate { get; set; }
		public ZDate ApprovalDate { get; set; }
		public ZDate PaymentDate { get; set; }
		public ZString ApprovalNo { get; set; }
		public ZString BankAccountNumber { get; set; }
		public ZString NoticeNumber { get; set; }
		public IReadOnlyDictionary<ZString, IDutyTaxFee> DutyTax { get; set; }
	}
	class DutyTaxFee : IDutyTaxFee
	{
		public ZString DutyTaxType { get; set; }
		public ZDecimal OriginalAmount { get; set; }
		public ZDecimal SupplementaryAmount { get; set; }
		public ZDecimal DifferenceAmount { get; set; }
	}
}
