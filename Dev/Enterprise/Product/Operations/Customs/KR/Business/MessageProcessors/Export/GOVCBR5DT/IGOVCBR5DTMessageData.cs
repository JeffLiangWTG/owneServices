using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5DTMessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ExportDeclarationNumber { get; }
		ZInt AmendSequence { get; }
		ZDate DecisionDate { get; }
		ZString NoticeType { get; }
		ZString FaultParty { get; }
		ZString FaultPartyChangeReason { get; }
		ZString NoticeDescription { get; }
		ZString ApprovalNo { get; }
		ZString CustomsPersonID { get; }
		ZString CustomsPersonName { get; }
		ZString CustomsOfficeAndDivision { get; }
	}

	class GOVCBR5DTMessageData : IGOVCBR5DTMessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZInt AmendSequence { get; set; }
		public ZDate DecisionDate { get; set; }
		public ZString NoticeType { get; set; }
		public ZString FaultParty { get; set; }
		public ZString FaultPartyChangeReason { get; set; }
		public ZString NoticeDescription { get; set; }
		public ZString ApprovalNo { get; set; }
		public ZString CustomsPersonID { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
	}
}
