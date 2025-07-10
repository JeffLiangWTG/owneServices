using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberContainer : IDataObject
	{
		[MaxLength(20)]
		public ZString? ContainerNumber { get; set; }
		public UberContainerType ContainerType { get; set; }
	}
}
