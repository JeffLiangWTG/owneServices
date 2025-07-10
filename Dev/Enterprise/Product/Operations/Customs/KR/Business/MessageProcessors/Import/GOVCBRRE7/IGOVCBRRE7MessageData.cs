using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRE7MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ApplicationNumber { get; }
		ZDateTime AcceptDateTime { get; }
		ZString DeclarationOffice { get; }
		ZString ContentDescription { get; }
	}

	class GOVCBRRE7MessageData : IGOVCBRRE7MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ApplicationNumber { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString ContentDescription { get; set; }
	}
}
