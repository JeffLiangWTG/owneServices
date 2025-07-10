using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Value", true)]
	public class IsNeutralMaster : IDataObject
	{
		public ZBool? Value { get; set; }
		public ZBool? CreateAndAllocateNeutralStock { get; set; }
	}
}