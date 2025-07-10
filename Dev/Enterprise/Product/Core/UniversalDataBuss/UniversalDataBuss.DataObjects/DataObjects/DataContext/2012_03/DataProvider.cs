using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11), FlattenedIntoAttributes("Code")]
	public class DataProvider : ICodeDataObject
	{
		[MaxLength(50)]
		public ZString? Code { get; set; }

		public DataProviderType? Type { get; set; }
	}

	public enum DataProviderType
	{
		EnterpriseID
	}

	public static class DataProviderExtensions
	{
		public static string GetCodeAndType(this DataProvider dataProvider)
		{
			if (dataProvider != null)
			{
				return dataProvider.Code.GetValueOrDefault() + (dataProvider.Type.HasValue ? " (" + dataProvider.Type.Value.ToString() + ")" : string.Empty);
			}

			return string.Empty;
		}
	}
}
