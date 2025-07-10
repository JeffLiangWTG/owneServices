using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberXmlDate : IDataObject
	{
		[CandidateKey, Mandatory]
		public UberXmlDateType? Type { get; set; }
		[CandidateKey]
		public ZBool? IsEstimate { get; set; }
		[Mandatory]
		public UXmlDateTime? Value { get; set; }
	}

	public enum UberXmlDateType
	{
		Arrival,
		Departure,
		ShippedOnBoard
	}
}
