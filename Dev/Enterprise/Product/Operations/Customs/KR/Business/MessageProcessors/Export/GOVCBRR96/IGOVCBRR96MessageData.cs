using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR96MessageData
	{
		ZString ExportDeclarationNumber { get; }
		ZDate NoticeDate { get; }
		ZString TradeName { get; }
		ZString ModelName { get; }
		ZString ClassificationReason { get; }
		ZString ContentDescription1 { get; }
		ZString ContentDescription2 { get; }
		ZString DeclarantCompanyName { get; }
		ZString SupplierCompanyName { get; }
		ZString AnalysisNumber { get; }
		ZInt EntryLineNo { get; }
		ZInt InvoiceLineNo { get; }
		ZString DecisionHSCode { get; }
		ZString HSCode { get; }
	}

	class GOVCBRR96MessageData : IGOVCBRR96MessageData
	{
		public ZString ExportDeclarationNumber { get; set; }
		public ZDate NoticeDate { get; set; }
		public ZString TradeName { get; set; }
		public ZString ModelName { get; set; }
		public ZString ClassificationReason { get; set; }
		public ZString ContentDescription1 { get; set; }
		public ZString ContentDescription2 { get; set; }
		public ZString DeclarantCompanyName { get; set; }
		public ZString SupplierCompanyName { get; set; }
		public ZString AnalysisNumber { get; set; }
		public ZInt EntryLineNo { get; set; }
		public ZInt InvoiceLineNo { get; set; }
		public ZString DecisionHSCode { get; set; }
		public ZString HSCode { get; set; }
	}
}
