using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code")]
	public class UNDGMarinePollutant : ICodeDescriptionDataObject
	{
		[MaxLength(1)]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Description { get; set; }
	}
}
