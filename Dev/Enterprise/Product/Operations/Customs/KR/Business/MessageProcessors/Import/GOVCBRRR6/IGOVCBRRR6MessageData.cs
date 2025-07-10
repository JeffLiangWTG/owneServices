using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRR6MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString CarnetCertificateNumber { get; }
		ZString CustomsOfficeAndDivision { get; }
	}

	class GOVCBRRR6MessageData : IGOVCBRRR6MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString CarnetCertificateNumber { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
	}
}
