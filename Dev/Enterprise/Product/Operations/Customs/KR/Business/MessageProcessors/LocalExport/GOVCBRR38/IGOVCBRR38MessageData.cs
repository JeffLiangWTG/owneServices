using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public interface IGOVCBRR38MessageData
	{
		ZString DeclarationType { get; }
		ZDateTime NoticeDateTime { get; }
		ZDateTime AcceptDateTime { get; }
		ZString CustomsOfficeAndDivision { get; }
		ZString DeclarationNumber { get; }
		ZString ConfirmNumber { get; }
		ZString ContentDescription { get; }
	}

	class GOVCBRR38MessageData : IGOVCBRR38MessageData
	{
		public ZString DeclarationType { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
		public ZString DeclarationNumber { get; set; }
		public ZString ConfirmNumber { get; set; }
		public ZString ContentDescription { get; set; }
	}
}
