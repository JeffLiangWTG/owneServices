using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5AFMessageData
	{
		ZString DeclarationType { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ExportDeclarationNumber { get; }
		ZDateTime AcceptDateTime { get; }
		ZString CustomsPersonID { get; }
		ZString CustomsPersonName { get; }
		ZString CustomsManagerID { get; }
		ZString CustomsManagerName { get; }
		ZString AmendAcceptResult { get; }
	}

	class GOVCBR5AFMessageData : IGOVCBR5AFMessageData
	{
		public ZString DeclarationType { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZString CustomsPersonID { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString CustomsManagerID { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString AmendAcceptResult { get; set; }
	}
}
