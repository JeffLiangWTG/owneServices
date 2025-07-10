using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class NMFC : ICodeDescriptionDataObject
	{
		[MaxLength(15), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(240)]
		public ZString? Description { get; set; }
		[MaxLength(6), Mandatory]
		public ZString? ItemNo { get; set; }
		[MaxLength(8), Mandatory]
		public ZString? Class { get; set; }
	}
}
