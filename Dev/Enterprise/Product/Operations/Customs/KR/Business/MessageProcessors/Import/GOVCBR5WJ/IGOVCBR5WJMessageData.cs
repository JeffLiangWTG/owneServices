using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5WJMessageData
	{
		ZDate NoticeDate { get; }
		ZString HSDescription { get; }
		ZString TradeName { get; }
		ZString BrandName { get; }
		ZString ModelName { get; }
		ZString ClassificationReason { get; }
		ZString ContentDescription1 { get; }
		ZString ContentDescription2 { get; }
		ZString DeclarantCompanyName { get; }
		ZString ImporterCompanyName { get; }
		ZString AnalysisNumber { get; }
		ZString ImportDeclarationNumber { get; }
		ZInt EntryLineNo { get; }
		ZInt InvoiceLineNo { get; }
		ZDate DeclarationDate { get; }
		ZString DecisionHSCode { get; }
		ZString HSCode { get; }
	}

	class GOVCBR5WJMessageData : IGOVCBR5WJMessageData
	{
		public ZDate NoticeDate { get; set; }
		public ZString HSDescription { get; set; }
		public ZString TradeName { get; set; }
		public ZString BrandName { get; set; }
		public ZString ModelName { get; set; }
		public ZString ClassificationReason { get; set; }
		public ZString ContentDescription1 { get; set; }
		public ZString ContentDescription2 { get; set; }
		public ZString DeclarantCompanyName { get; set; }
		public ZString ImporterCompanyName { get; set; }
		public ZString AnalysisNumber { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZInt EntryLineNo { get; set; }
		public ZInt InvoiceLineNo { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZString DecisionHSCode { get; set; }
		public ZString HSCode { get; set; }
	}
}
