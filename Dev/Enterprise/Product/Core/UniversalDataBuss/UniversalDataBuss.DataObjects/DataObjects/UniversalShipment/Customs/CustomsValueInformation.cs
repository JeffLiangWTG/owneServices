using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class CustomsValueInformation : IDataObject
	{
		public CustomsValueInformation()
		{
		}

		public CustomsValueInformation(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZInt? Link { get; set; }
		public List<CustomsValueDetail> CustomsValueDetailCollection { get; private set; }
	}
}
