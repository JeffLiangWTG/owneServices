using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Location : IDataObject
	{
		public ZShort? Column { get; set; }
		public ZShort? Level { get; set; }
		public ZShort? Tray { get; set; }
		[MaxLength(25)]
		public ZString? Row { get; set; }
	}
}
