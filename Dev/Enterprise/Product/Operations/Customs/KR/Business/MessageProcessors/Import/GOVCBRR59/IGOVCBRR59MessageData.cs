using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR59MessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDate DeclarationDate { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ResultType { get; }
		ZString CustomsManagerName { get; }
		ZString CustomsOffice { get; }
		ZInt EntryLineNo { get; }
		ZString HSCode { get; }
		ZDecimal Quantity { get; }
		ZString QuantityUnit { get; }
		ZDecimal NetWeightInKG { get; }
	}

	class GOVCBRR59MessageData : IGOVCBRR59MessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ResultType { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZInt EntryLineNo { get; set; }
		public ZString HSCode { get; set; }
		public ZDecimal Quantity { get; set; }
		public ZString QuantityUnit { get; set; }
		public ZDecimal NetWeightInKG { get; set; }
	}
}
