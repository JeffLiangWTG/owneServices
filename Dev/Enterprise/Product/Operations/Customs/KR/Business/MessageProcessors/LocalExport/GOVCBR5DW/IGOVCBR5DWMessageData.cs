using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5DWMessageData
	{
		ZString AcceptNumber { get; }
		ZDate AcceptDate { get; }
		ZString DeclarationNumber { get; }
		ZString DeclarantCompanyName { get; }
		ZString ManufacturerCompanyName { get; }
		ZString ImporterCompanyName { get; }
		ZDate ProductConfirmationDate { get; }
		ZString ProductConfirmationManagerID { get; }
		ZString ProductConfirmationManagerName { get; }
		ZInt TotalEntryLineCount { get; }
		ZInt TotalPackages { get; }
		ZDecimal TotalDeclarationAmount { get; }
		ZDecimal TotalGrossWeight { get; }
		ZString CustomsOfficeContent { get; }
	}

	class GOVCBR5DWMessageData : IGOVCBR5DWMessageData
	{
		public ZString AcceptNumber { get; set; }
		public ZDate AcceptDate { get; set; }
		public ZString DeclarationNumber { get; set; }
		public ZString DeclarantCompanyName { get; set; }
		public ZString ManufacturerCompanyName { get; set; }
		public ZString ImporterCompanyName { get; set; }
		public ZDate ProductConfirmationDate { get; set; }
		public ZString ProductConfirmationManagerID { get; set; }
		public ZString ProductConfirmationManagerName { get; set; }
		public ZInt TotalEntryLineCount { get; set; }
		public ZInt TotalPackages { get; set; }
		public ZDecimal TotalDeclarationAmount { get; set; }
		public ZDecimal TotalGrossWeight { get; set; }
		public ZString CustomsOfficeContent { get; set; }
	}
}
