using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class MatchingCriteria : IDataObject
	{
		[Mandatory]
		[MaxLength(50)]
		public ZString? FieldName { get; set; }
		[Mandatory]
		[MaxLength(1024), AllowLineControlWhiteSpace]
		public ZString? Value { get; set; }
	}
}
