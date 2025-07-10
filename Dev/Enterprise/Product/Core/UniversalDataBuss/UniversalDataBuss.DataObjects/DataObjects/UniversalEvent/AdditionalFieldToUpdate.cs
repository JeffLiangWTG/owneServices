using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class AdditionalFieldToUpdate : IDataObject
	{
		[Mandatory, MaxLength(128)]
		public ZString? Type { get; set; }
		[Mandatory, MaxLength(1024)]
		public ZString? Value { get; set; }
	}
}
