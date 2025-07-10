using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5FKMessageData
	{
		ZDate SubmissionDate { get; }
		ZDateTime NoticeDate { get; }
		ZString ImportDeclarationNumber { get; }
		ZString NoticeCode { get; }
		ZDecimal InDepositAmount { get; }
		ZDecimal DelayPaymentAmount { get; }
		ZDecimal TotalInterestAndPenalty { get; }
		ZString NoticeNumber { get; }
		ZString CustomerOfficer { get; }
	}

	public class GOVCBR5FKMessageData : NonPersistentBusinessObject, IGOVCBR5FKMessageData
	{
		public GOVCBR5FKMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZDate SubmissionDate { get; set; }
		public ZDateTime NoticeDate { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString NoticeCode { get; set; }
		public ZDecimal InDepositAmount { get; set; }
		public ZDecimal DelayPaymentAmount { get; set; }
		public ZDecimal TotalInterestAndPenalty { get; set; }
		public ZString NoticeNumber { get; set; }
		public ZString CustomerOfficer { get; set; }

		public ZInt AmendSequence { get; set; }
		public ZDecimal DutyAmountPayable { get; set; }
		public ZDecimal DutyAmountDifference { get; set; }
		public ZDecimal LiquorTaxPayable { get; set; }
		public ZDecimal LiquorTaxDifference { get; set; }
		public ZDecimal AgricultureTaxPayable { get; set; }
		public ZDecimal AgricultureTaxDifference { get; set; }
		public ZDecimal TransportationTaxPayable { get; set; }
		public ZDecimal TransportationTaxDifference { get; set; }
		public ZDecimal EducationTaxPayable { get; set; }
		public ZDecimal EducationTaxDifference { get; set; }
		public ZDecimal SpecialConsumptionTaxPayable { get; set; }
		public ZDecimal SpecialConsumptionTaxDifference { get; set; }
		public ZDecimal VATPayable { get; set; }
		public ZDecimal VATDifference { get; set; }
		public ZDecimal DeclarationPenaltyPayable { get; set; }
		public ZDecimal DeclarationPenaltyDifference { get; set; }
		public ZDecimal TotalAmountPayableOneDayAfterDueDate { get; set; }
		public ZString TransactionNatureCode { get; set; }

		public IEnumerable<GOVCBR5FKChargeMessageData> Charges { get; set; }
  }

	public class GOVCBR5FKChargeMessageData
	{
		public ZString TypeCode { get; set; }
		public ZDecimal Amount { get; set; }
	}
}
