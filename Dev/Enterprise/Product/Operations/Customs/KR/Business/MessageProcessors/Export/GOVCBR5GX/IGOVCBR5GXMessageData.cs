using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5GXMessageData
	{
		ZDate NoticeDate { get; }
		ZString ExportDeclarationNumber { get; }
		ZDate ComplementDueDate { get; }
		ZString ComplementReasonCode { get; }
		ZString ComplementReasonName { get; }
		ZString ComplementDescription { get; }
		ZString CustomsOffice { get; }
		ZString CustomsOfficeName { get; }
		ZString DeclarantID { get; }
		ZString DeclarantName { get; }
		ZString ComplementNumber { get; }
	}

	class GOVCBR5GXMessageData : IGOVCBR5GXMessageData
	{
		public ZDate NoticeDate { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZDate ComplementDueDate { get; set; }
		public ZString ComplementReasonCode { get; set; }
		public ZString ComplementReasonName { get; set; }
		public ZString ComplementDescription { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZString CustomsOfficeName { get; set; }
		public ZString DeclarantID { get; set; }
		public ZString DeclarantName { get; set; }
		public ZString ComplementNumber { get; set; }
	}
}
