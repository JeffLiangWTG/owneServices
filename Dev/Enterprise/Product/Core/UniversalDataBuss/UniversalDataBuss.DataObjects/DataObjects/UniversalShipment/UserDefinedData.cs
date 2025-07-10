using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class UserDefinedData : IDataObject
	{
		[MaxLength(250)] // Non-persisten bizo is used here without SchemaColumn or ZPropertyInfo, and does not specify max length
		public ZString? Name { get; set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? Value { get; set; }
	}
}