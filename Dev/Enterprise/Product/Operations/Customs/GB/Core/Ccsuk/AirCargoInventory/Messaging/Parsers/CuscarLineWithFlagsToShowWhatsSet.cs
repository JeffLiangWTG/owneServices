using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	public class CuscarLineWithFlagsToShowWhatsSet : ICuscarLine
	{
		public ZBool NumberOfPiecesSet { get; set; }
		public ZShort NumberOfPiecesExpected { get; set; }

		public ZBool WeightCodeSet { get; set; }
		public ZString WeightCode { get; set; }

		public ZBool WeightSet { get; set; }
		public ZDecimal Weight { get; set; }

		public ZBool DescriptionOfGoodsSet { get; set; }
		public ZString DescriptionOfGoods { get; set; }

		public ZBool NumberOfPiecesReceivedSet { get; set; }
		public ZShort NumberOfPiecesReceived { get; set; }

		public ZBool HarmonisedCommodityCodeSet { get; set; }
		public ZString HarmonisedCommodityCode { get; set; }

		public ZString LineOrSplitNumber { get; set; }
	}
}
