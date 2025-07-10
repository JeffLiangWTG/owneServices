using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11)]
	public class DataSource : IDataSourceDataObject
	{
		public DataProvider DataProvider { get; set; }
		[MaxLength(35)]
		public ZString? Type { get; set; }
		[MaxLength(300)]
		public ZString? Key { get; set; }
	}
}

