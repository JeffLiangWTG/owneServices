using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal;

[XsdSchema(Placement.Outer)]
public class Seal : IDataObject
{
	[MaxLength(20)]
	public ZString? SealNumber { get; set; }

	public ZInt? Sequence { get; set; }

	[MaxLength(3)]
	public ZString? StatusInformation { get; set; }
}
