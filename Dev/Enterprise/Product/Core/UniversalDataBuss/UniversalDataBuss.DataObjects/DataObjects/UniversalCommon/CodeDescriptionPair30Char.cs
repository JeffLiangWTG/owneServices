using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code")]
	public class CodeDescriptionPair30Char : ICodeDescriptionDataObject
	{
		[MaxLength(30), Mandatory]
		public ZString? Code { get; set; }

		[MaxLength(80), Mandatory]
		public ZString? Description { get; set; }
	}
}
