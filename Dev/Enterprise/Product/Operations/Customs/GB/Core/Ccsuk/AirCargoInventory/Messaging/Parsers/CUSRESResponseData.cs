using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	public class CUSRESResponseData
	{
		public ZString CommonAccessReference { get; set; }
		public ZString DocumentNameCode { get; set; }
		public ZString CountryOfOrigin { get; set; }
		public ZString AirportOfOrigin { get; set; }
		public ZString AirportOfReceipt { get; set; }
		public ZString ShedId { get; set; }
		public ZString AirWaybillPrefixAndNumber { get; set; }
		public ZString HouseWaybillNumber { get; set; }
		public ZString SplitReference { get; set; }
		public ZString AgentName { get; set; }
		public ZString AgentCode { get; set; }
		public ZString AgentsReferenceNumber { get; set; }
		public ZString AgentsTelephoneNumber { get; set; }
		public ZInt NoOfPackagesExpected { get; set; }
		public ZString DescriptionOfGoods { get; set; }
		public ZString EntryNumber { get; set; }
		public ZDate EntryDate { get; set; }
		public ZString CustomsActionText { get; set; }
		public ZString CustomsActionCode_StatusOfRequest { get; set; }
	}
}
