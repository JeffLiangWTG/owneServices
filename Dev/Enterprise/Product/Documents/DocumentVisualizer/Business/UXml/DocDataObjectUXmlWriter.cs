using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocDataObjectUXmlWriter : IXmlWriter
	{
		public DocDataObjectUXmlWriter(IDataObject dataObject, string userDefinedXmlNamespace)
		{
			Argument.NotNull(dataObject, nameof(dataObject));

			this.dataObject = dataObject;
			this.userDefinedXmlNamespace = userDefinedXmlNamespace;

			if (dataObject is ITopLevelDataObject topLevelDataObject
				&& topLevelDataObject.DataContext?.DataSourceCollection?.FirstOrDefault() is IDataSourceDataObject dataSource)
			{
				dataSourceKey = dataSource.Key;
				dataSourceType = dataSource.Type;
			}
		}

		readonly IDataObject dataObject;
		readonly ZString? dataSourceKey;
		readonly ZString? dataSourceType;

		readonly string userDefinedXmlNamespace;

		void IXmlWriter.WriteXML(IDataObject dataStructure, SubStreamableStream outputStream, string nameSpace, IDataOverrideProvider overrideProvider)
		{
			if (dataSourceKey.HasValue
				&& dataSourceType.HasValue
				&& dataStructure is ITopLevelDataObject topLevelDataObject
				&& topLevelDataObject.DataContext?.DataSourceCollection?.FirstOrDefault() is IDataSourceDataObject dataSource)
			{
				dataSource.Key = dataSourceKey;
				dataSource.Type = dataSourceType;
			}

			var ns = UniversalXmlInfo.Namespace_2012_11;
			var writer = new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter();

			if (!string.IsNullOrWhiteSpace(userDefinedXmlNamespace))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					// TODO: This could be implemented more efficiently by slicing the substream instead of the redundant copy for large data
					var xmlReplacer = new XmlNamespaceReplacer();
					writer.WriteXML(dataObject, stream, ns, overrideProvider);

					var newNamespace = xmlReplacer.GetCustomNamespace(ns, userDefinedXmlNamespace);
					xmlReplacer.ReplaceXmlWithNamespace(stream, outputStream, newNamespace);
				}
			}
			else
			{
				writer.WriteXML(dataObject, outputStream, ns);
			}

			outputStream.Seek(0, SeekOrigin.Begin);
		}
	}
}
