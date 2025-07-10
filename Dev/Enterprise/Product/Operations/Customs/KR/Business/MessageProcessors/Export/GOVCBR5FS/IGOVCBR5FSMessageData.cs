using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5FSMessageData
	{
		ZString ExportDeclarationNumber { get; }
		ZDate EntryReleaseDate { get; }
		ZString SupplierCompanyName { get; }
		ZString DeclarantID { get; }
		ZString ManufacturerID { get; }
		ZString TradeName { get; }
		ZInt TotalPackQty { get; }
		ZString PackType { get; }
		ZDecimal TotalGrossWeightInKG { get; }
	}

	class GOVCBR5FSMessageData : IGOVCBR5FSMessageData
	{
		public ZString ExportDeclarationNumber { get; set; }
		public ZDate EntryReleaseDate { get; set; }
		public ZString SupplierCompanyName { get; set; }
		public ZString DeclarantID { get; set; }
		public ZString ManufacturerID { get; set; }
		public ZString TradeName { get; set; }
		public ZInt TotalPackQty { get; set; }
		public ZString PackType { get; set; }
		public ZDecimal TotalGrossWeightInKG { get; set; }
	}
}
