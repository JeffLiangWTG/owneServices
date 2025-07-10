using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class SystemDefinedData : IDataObject
	{
		[MaxLength(35)] // StmSystemDefinedField.S1_Name
		public ZString? Name { get; set; }

		[MaxLength(64)] // StmSystemDefinedField.S1_Category
		public ZString? Category { get; set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? Value { get; set; }
	}
}