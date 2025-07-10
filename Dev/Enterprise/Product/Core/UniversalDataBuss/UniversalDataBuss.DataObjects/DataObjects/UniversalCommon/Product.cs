using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code")]
	public class Product : ICodeDescriptionDataObject
	{
		[MaxLength(35), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(128)]
		public ZString? Description { get; set; }
	}
}
