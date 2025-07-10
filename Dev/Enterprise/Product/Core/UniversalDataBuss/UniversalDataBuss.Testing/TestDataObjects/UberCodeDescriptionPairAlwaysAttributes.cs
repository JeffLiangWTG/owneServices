using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code", true)]
	public class UberCodeDescriptionPairAlwaysAttributes : ICodeDescriptionDataObject
	{
		[MaxLength(50), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Description { get; set; }
	}
}

