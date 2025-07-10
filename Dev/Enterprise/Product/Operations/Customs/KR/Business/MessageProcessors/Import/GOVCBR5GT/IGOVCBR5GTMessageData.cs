using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5GTMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDate AmendDate { get; }
		ZString DeclarationOffice { get; }
		ZString CustomsPersonName { get; }
		ZString ComplementNumber { get; }
		ZString CustomsPersonPhoneNumber { get; }
		ZString AmendAcceptResult { get; }
	}

	class GOVCBR5GTMessageData : IGOVCBR5GTMessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate AmendDate { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString ComplementNumber { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
		public ZString AmendAcceptResult { get; set; }
	}
}
