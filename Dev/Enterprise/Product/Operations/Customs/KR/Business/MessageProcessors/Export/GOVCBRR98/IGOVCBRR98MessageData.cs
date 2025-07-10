using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR98MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString DeclarantName { get; }
		IEnumerable<IGoodsShipment> GoodsShipment { get; }
	}
	interface IGoodsShipment
	{
		ZString ExportDeclarationNumber { get; }
		ZDate DeclarationDate { get; }
		ZDate EntryReleaseDate { get; }
		ZDate LoadingDate { get; }
		ZString ExporterCompanyName { get; }
		ZString TradeName { get; }
		ZInt TotalPackQty { get; }
		ZDecimal TotalGrossWeightInKG { get; }
	}

	class GOVCBRR98MessageData : IGOVCBRR98MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString DeclarantName { get; set; }
		public IEnumerable<IGoodsShipment> GoodsShipment { get; set; }
	}
	class GoodsShipment : IGoodsShipment
	{
		public ZString ExportDeclarationNumber { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZDate EntryReleaseDate { get; set; }
		public ZDate LoadingDate { get; set; }
		public ZString ExporterCompanyName { get; set; }
		public ZString TradeName { get; set; }
		public ZInt TotalPackQty { get; set; }
		public ZDecimal TotalGrossWeightInKG { get; set; }
	}
}
