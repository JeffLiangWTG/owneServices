using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code")]
	public class CodeDescriptionPair8Char : ICodeDescriptionDataObject
	{
		[MaxLength(8), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(80)]
		public ZString? Description { get; set; }
	}
}
