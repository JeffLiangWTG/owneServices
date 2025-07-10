using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code"), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11)]
	public class UberDataProvider : ICodeDataObject
	{
		[MaxLength(50)]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Type { get; set; }
	}
}

