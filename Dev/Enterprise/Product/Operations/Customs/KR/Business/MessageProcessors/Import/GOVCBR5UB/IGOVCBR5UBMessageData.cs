using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5UBMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZString CustomsPersonName { get; }
		ZString DeclarationOffice { get; }
		ZDateTime ApprovalDate { get; }
		ZDateTime NoticeDateTime { get; }
		ZString PenaltyExemptionCode { get; }
		ZString ResultType { get; }
		ZString ResultReason { get; }
		ZString NoticeNumber { get; }
		ZInt PenaltyExemptionReqSequence { get; }
		ZDecimal PenaltyExemptionAmount { get; }
	}

	public class GOVCBR5UBMessageData : NonPersistentBusinessObject, IGOVCBR5UBMessageData
	{
		public GOVCBR5UBMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString ImportDeclarationNumber { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZDateTime ApprovalDate { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString PenaltyExemptionCode { get; set; }
		public ZString ResultType { get; set; }
		public ZString ResultReason { get; set; }
		public ZString NoticeNumber { get; set; }
		public ZInt PenaltyExemptionReqSequence { get; set; }
		public ZDecimal PenaltyExemptionAmount { get; set; }
		public ZString ResultTypeDescription { get; set; }
	}
}
