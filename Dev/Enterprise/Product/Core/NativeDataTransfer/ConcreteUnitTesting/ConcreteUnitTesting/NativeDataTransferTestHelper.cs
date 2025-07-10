using System.IO;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	public static class NativeDataTransferTestHelper
	{
		public static Stream ExportToStream(BusinessObject bizo)
		{
			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer { Converter = converter };

			return xmlSerializer.SerializeToStream(bizo);
		}

		public static string ImportAndGetInsertLog(Stream exportStream)
		{
			var manager = new ImportServiceManagerForTesting();
			manager.ImportService.Import(exportStream);

			return manager.GetLogs();
		}
	}
}
