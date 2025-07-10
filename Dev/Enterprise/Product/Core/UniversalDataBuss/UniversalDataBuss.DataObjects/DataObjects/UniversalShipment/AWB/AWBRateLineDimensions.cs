using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public class AWBRateLineDimensions : IDataObject
	{
		public ZInt? Length { get; set; }
		public ZInt? Width { get; set; }
		public ZInt? Height { get; set; }
		[MaxLength(2)]
		public CodeDescriptionPair Unit { get; set; }
		public ZInt? Count { get; set; }
	}
}
