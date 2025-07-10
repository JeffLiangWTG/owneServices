using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Inner), FlattenedIntoAttributes("Code")]
	public class RatingBehaviour : ICodeDescriptionDataObject
	{
		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}
