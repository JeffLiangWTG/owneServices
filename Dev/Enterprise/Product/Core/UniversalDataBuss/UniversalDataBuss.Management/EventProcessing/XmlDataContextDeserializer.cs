using System.IO;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing
{
	public class XmlDataContextDeserializer : IXmlDataContextDeserializer
	{
		IDataContextDataObject IXmlDataContextDeserializer.Parse(TextReader textReader, IXmlImportLogger logger)
		{
			var elementExtractor = new SingleElementExtractor(textReader);
			using (var stream = elementExtractor.GetNestedElementContentIncludingElementIdentifiers("DataContext"))
			{
				textReader.Dispose();

				if (stream == null)
				{
					return null;
				}

				var nameSpace = elementExtractor.RootElementInfo.Namespace;
				var version = elementExtractor.RootElementInfo.Version;
				if (string.IsNullOrEmpty(nameSpace) || nameSpace != UniversalXmlInfo.Namespace_2012_11)
				{
					nameSpace = UniversalXmlInfo.Namespace_2011_11;
					version = UniversalXmlInfo.Version_2011_11;
				}

				var dataContext = DataContextFactory.New(nameSpace);
				new XmlReader().ReadXML(dataContext, stream, logger, nameSpace, version);

				return dataContext;
			}
		}
	}
}
