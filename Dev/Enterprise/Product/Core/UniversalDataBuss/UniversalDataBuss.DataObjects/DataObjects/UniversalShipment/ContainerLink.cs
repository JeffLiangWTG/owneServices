using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class ContainerLink : IDataObject
	{
		public ZInt? Link { get; set; }
		[MaxLength(20)]
		public ZString? ContainerNumber { get; set; }
	}
}