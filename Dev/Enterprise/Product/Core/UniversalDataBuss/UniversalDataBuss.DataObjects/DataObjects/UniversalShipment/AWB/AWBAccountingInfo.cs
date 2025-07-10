using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public class AWBAccountingInfo : IDataObject
	{
		public CodeDescriptionPair Type { get; set; }
		[MaxLength(34)]
		public ZString? Information { get; set; }
	}
}
