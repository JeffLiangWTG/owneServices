using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRA1MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ImportDeclarationNumber { get; }
		ZString CustomsOfficeAndDivision { get; }
	}

	class GOVCBRRA1MessageData : IGOVCBRRA1MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
	}
}
