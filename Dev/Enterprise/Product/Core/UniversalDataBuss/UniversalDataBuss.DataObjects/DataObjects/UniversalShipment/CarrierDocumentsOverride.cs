using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class CarrierDocumentsOverride : IDataObject
	{
		public AWBHeader AWBHeader { get; set; }
	}
}
