using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5AAMessageData
	{
		ZString ExportDeclarationNumber { get; }
		ZDateTime EntryReleaseDateTime { get; }
		ZDate LoadingDate { get; }
		ZString AcceptanceDateTime { get; }
		ZString CustomsOfficeContent { get; }
		ZDecimal CustomsValueKRW { get; }
		ZDecimal CustomsValueUSD { get; }
		ZString ContentDescription { get; }
		ZString RoadNameRequest { get; }
	}

	class GOVCBR5AAMessageData : IGOVCBR5AAMessageData
	{
		public ZString ExportDeclarationNumber { get; set; }
		public ZDateTime EntryReleaseDateTime { get; set; }
		public ZDate LoadingDate { get; set; }
		public ZString AcceptanceDateTime { get; set; }
		public ZString CustomsOfficeContent { get; set; }
		public ZDecimal CustomsValueKRW { get; set; }
		public ZDecimal CustomsValueUSD { get; set; }
		public ZString ContentDescription { get; set; }
		public ZString RoadNameRequest { get; set; }
	}
}
