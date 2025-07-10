using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class WeightData : IDataObject
	{
		[Mandatory]
		[MaxLength(20)]
		public ZString? ContainerJobID { get; set; }
		public ZDecimal? TotalWeight { get; set; }
		public UnitOfWeight TotalWeightUnit { get; set; }
		public TEU TEU { get; set; }
		public EmptyContainerAddress EmptyPickup { get; set; }
		public EmptyContainerAddress EmptyReturn { get; set; }
		public DirectionType? Direction { get; set; }
	}
}
