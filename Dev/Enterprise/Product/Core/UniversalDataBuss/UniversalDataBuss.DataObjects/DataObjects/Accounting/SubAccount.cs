using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Outer)]
	public class SubAccount : IDataObject
	{
		[MaxLength(35)]
		public ZString? Code { get; set; }
		public CodeDescriptionPair Type { get; set; }
	}
}
