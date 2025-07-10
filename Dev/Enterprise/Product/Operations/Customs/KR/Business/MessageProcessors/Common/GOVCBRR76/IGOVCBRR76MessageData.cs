using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR76MessageData
	{
		ZString DeclarationType { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ApplicationNumber { get; }
		ZDateTime AcceptDateTime { get; }
		ZString CustomsOfficeAndDivision { get; }
	}

	class GOVCBRR76MessageData : IGOVCBRR76MessageData
	{
		public ZString DeclarationType { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ApplicationNumber { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
	}
}
