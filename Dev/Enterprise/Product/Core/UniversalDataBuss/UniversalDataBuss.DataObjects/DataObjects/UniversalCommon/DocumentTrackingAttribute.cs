using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class DocumentTrackingAttribute : IDataObject
	{
		[MaxLength(35), Mandatory]
		public ZString? Type { get; set; }
		[MaxLength(80)]
		public ZString? Value { get; set; }
	}
}
