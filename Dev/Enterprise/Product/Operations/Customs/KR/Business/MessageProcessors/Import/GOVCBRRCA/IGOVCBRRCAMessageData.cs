using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRCAMessageData
	{
		ZString RefundDeclarationNumber { get; }
		ZDateTime NoticeDateTime { get; }
		ZString NoticeType { get; }
		ZString CustomsPersonName { get; }
		ZString DeclarationOffice { get; }
		ZString ResultReason { get; }
		ZString ContentDescription { get; }
		ZString ContentDescription2 { get; }
	}

	class GOVCBRRCAMessageData : IGOVCBRRCAMessageData
	{
		public ZString RefundDeclarationNumber { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString NoticeType { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString ResultReason { get; set; }
		public ZString ContentDescription { get; set; }
		public ZString ContentDescription2 { get; set; }
	}
}
