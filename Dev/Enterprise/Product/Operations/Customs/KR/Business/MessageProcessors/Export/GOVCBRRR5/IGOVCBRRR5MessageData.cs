using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRR5MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ExportDeclarationNumber { get; }
		ZString ChangeReasonType { get; }
		ZString ChangeReasonDescription { get; }
		ZString ChangeReasonDescription2 { get; }
		ZString InspectionChangeType { get; }
		ZString CustomsManagerID { get; }
		ZString CustomsManagerName { get; }

		ZString SubCustomsManagerID { get; }
		ZString SubCustomsManagerName { get; }
	}

	class GOVCBRRR5MessageData : IGOVCBRRR5MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZString ChangeReasonType { get; set; }
		public ZString ChangeReasonDescription { get; set; }
		public ZString ChangeReasonDescription2 { get; set; }
		public ZString InspectionChangeType { get; set; }
		public ZString CustomsManagerID { get; set; }
		public ZString CustomsManagerName { get; set; }

		public ZString SubCustomsManagerID { get; set; }
		public ZString SubCustomsManagerName { get; set; }
	}
}
