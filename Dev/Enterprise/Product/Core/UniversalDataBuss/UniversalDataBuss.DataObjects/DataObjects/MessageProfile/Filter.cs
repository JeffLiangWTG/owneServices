using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Filter : IDataObject
	{
		[MaxLength(50)]
		public ZString? ElementName { get; set; }

		[MaxLength(50)]
		public ZString? DataContext { get; set; }
	}
}
