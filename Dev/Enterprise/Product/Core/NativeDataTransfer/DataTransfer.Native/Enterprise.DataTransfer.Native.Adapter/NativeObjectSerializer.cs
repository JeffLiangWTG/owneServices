using System;
using System.IO;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeObjectSerializer : INativeObjectSerializer
	{
		public Stream SerializeToScavengingOrganization(Guid pk, string tableName)
		{
			var xmlSerializer = GetNativeXmlSerializer();
			return xmlSerializer.SerializeToStream(null, new[] { new RowID(pk, tableName) }, null, pk);
		}

		static NativeXmlSerializer GetNativeXmlSerializer()
		{
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			return xmlSerializer;
		}
	}
}
