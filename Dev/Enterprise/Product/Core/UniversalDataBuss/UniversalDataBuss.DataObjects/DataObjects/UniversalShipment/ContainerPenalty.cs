using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class ContainerPenalty : IDataObject
	{
		public CodeDescriptionPair PenaltyType { get; set; }
		public CodeDescriptionPair CreditorType { get; set; }
		public OrganizationAddress Creditor { get; set; }
		public UNLOCO Location { get; set; }
		public ZDateTime? FreeTime { get; set; }
		public ZByte? FreeTimeAmount { get; set; }
		public ZDateTime? Duration { get; set; }
		public ZByte? DurationAmount { get; set; }
		public ZDateTime? FirstFreeDay { get; set; }
		public ZDateTime? LastFreeDay { get; set; }
		public TimeUnit? TimeUnit { get; set; }
		public ZDecimal? PerUnitCost { get; set; }
		public ZDecimal? TotalCost { get; set; }
		public ZDecimal? PerUnitSell { get; set; }
		public ZDecimal? TotalSell { get; set; }
		public Currency Currency { get; set; }
		public CodeDescriptionPair ProcessType { get; set; }
	}
}
