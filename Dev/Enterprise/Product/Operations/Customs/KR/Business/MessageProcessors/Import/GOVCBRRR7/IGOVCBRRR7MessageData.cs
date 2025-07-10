using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRR7MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ImportDeclarationNumber { get; }
		ZString CustomsOfficeAndDepartmentID { get; }
	}

	class GOVCBRRR7MessageData : IGOVCBRRR7MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString CustomsOfficeAndDepartmentID { get; set; }
	}
}
