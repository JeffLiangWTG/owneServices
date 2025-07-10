using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5BCMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDate ApprovalDate { get; }
		ZString AmendType { get; }
		ZString ResultType { get; }
		ZString CustomsOffice { get; }
		ZString ResultReason { get; }
		ZString CustomsManagerName { get; }
	}

	class GOVCBR5BCMessageData : IGOVCBR5BCMessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate ApprovalDate { get; set; }
		public ZString AmendType { get; set; }
		public ZString ResultType { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZString ResultReason { get; set; }
		public ZString CustomsManagerName { get; set; }
	}
}
