using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR67MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ApplicationNumber { get; }
		ZInt SequenceNo { get; }
		ZDateTime AcceptDateTime { get; }
		ZString NoticeDescription { get; }
	}

	class GOVCBRR67MessageData : IGOVCBRR67MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ApplicationNumber { get; set; }
		public ZInt SequenceNo { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZString NoticeDescription { get; set; }
	}
}
