using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class SealNumber : IDataObject
	{
		[MaxLength(20)]
		public ZString? Number { get; set; }
	}
}
