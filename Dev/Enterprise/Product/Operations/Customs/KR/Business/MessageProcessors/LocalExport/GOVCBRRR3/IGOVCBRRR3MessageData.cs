using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRR3MessageData
	{
		ZString DeclarationType { get; }
		ZString ResultType { get; }
		ZString ContentDescription { get; }
		ZString CustomsOfficeAndDivision { get; }
		ZString CustomsPersonName { get; }
		ZDateTime CustomsDateTime { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ConfirmNumber { get; }
		ZString NoticeNumber { get; }
		ZInt AmendSequence { get; }
	}

	class GOVCBRRR3MessageData : IGOVCBRRR3MessageData
	{
		public ZString DeclarationType { get; set; }
		public ZString ResultType { get; set; }
		public ZString ContentDescription { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZDateTime CustomsDateTime { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ConfirmNumber { get; set; }
		public ZString NoticeNumber { get; set; }
		public ZInt AmendSequence { get; set; }
	}
}
