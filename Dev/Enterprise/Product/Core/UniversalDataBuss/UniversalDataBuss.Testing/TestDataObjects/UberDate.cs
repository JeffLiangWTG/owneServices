using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberDate : IDataObject
	{
		[CandidateKey, Mandatory]
		public UberDateType? Type { get; set; }
		[CandidateKey]
		public ZBool? IsEstimate { get; set; }
		[Mandatory]
		public ZDateTime? Value { get; set; }
	}

	public enum UberDateType
	{
		AvailableExFactory,
		Pickup,
		Pack,
		Departure,
	}
}
