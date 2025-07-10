using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public class AWBRateLineVolume : IDataObject
	{
		public ZDecimal? Volume { get; set; }
		[MaxLength(2)]
		public CodeDescriptionPair Unit { get; set; }
	}
}
