using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class Registration : IDataObject
	{
		[MaxLength(10)]
		public ZString? Number { get; set; }
	}
}
