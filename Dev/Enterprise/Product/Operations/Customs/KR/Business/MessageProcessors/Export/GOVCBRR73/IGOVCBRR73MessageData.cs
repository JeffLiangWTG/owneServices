using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR73MessageData
	{
		ZString ResultType { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ExportDeclarationNumber { get; }
		ZString DocumentSubmitDescription { get; }
		ZString CustomsOfficerID { get; }
		ZString CustomsOfficerName { get; }
	}

	class GOVCBRR73MessageData : IGOVCBRR73MessageData
	{
		public ZString ResultType { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZString DocumentSubmitDescription { get; set; }
		public ZString CustomsOfficerID { get; set; }
		public ZString CustomsOfficerName { get; set; }
	}
}
