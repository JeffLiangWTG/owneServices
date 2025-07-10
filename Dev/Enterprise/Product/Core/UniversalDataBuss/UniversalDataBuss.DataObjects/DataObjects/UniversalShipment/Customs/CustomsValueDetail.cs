using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class CustomsValueDetail : IDataObject
	{
		[MaxLength(40)]
		public ZString? Type { get; set; }
		[MaxLength(1)]
		public ZString? Code { get; set; }
		[MaxLength(100)]
		public ZString? Details { get; set; }
	}
}
