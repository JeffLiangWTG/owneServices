using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR97MessageData
	{
		ZString DeclarationType { get; }
		ZString TransactionNatureCode { get; }
		ZDate AmendDate { get; }
		ZString ExportDeclarationNumber { get; }
		ZString AmendType { get; }
		ZString FaultParty { get; }
		ZString CustomsOfficeAndDivision { get; }
		ZString CustomsPersonName { get; }
		ZString ReasonCode { get; }
		ZString AmendReasonDescription { get; }
		ZString SupplierCompanyName { get; }
	}

	class GOVCBRR97MessageData : IGOVCBRR97MessageData
	{
		public ZString DeclarationType { get; set; }
		public ZString TransactionNatureCode { get; set; }
		public ZDate AmendDate { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZString AmendType { get; set; }
		public ZString FaultParty { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString ReasonCode { get; set; }
		public ZString AmendReasonDescription { get; set; }
		public ZString SupplierCompanyName { get; set; }
	}
}
