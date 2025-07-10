using System;
using System.Globalization;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	public class XmlWriter : IXmlWriter, ISupportsRemovingEmptyElements
	{
		public bool RemoveEmptyElements { get; set; }

		public void WriteXML(IDataObject dataStructure, SubStreamableStream outputStream, string nameSpace = null, IDataOverrideProvider overrideProvider = null)
		{
			WriteXML(dataStructure, outputStream, true, nameSpace, overrideProvider);
		}

		public void WriteXML(IDataObject dataStructure, SubStreamableStream outputStream, bool writeXMLDeclaration, string nameSpace = null, IDataOverrideProvider overrideProvider = null)
		{
			var schema = nameSpace.GetUniversalXmlSchema() ?? SchemaVersionManager.Current;

			using (var result = new XmlBuilder(outputStream, schema.Namespace, schema.Version))
			{
				result.RemoveEmptyElements = RemoveEmptyElements;
				if (writeXMLDeclaration)
				{
					result.WriteXMLDeclaration();
				}
				Type type = dataStructure.GetType();
				var rootElementInfo = type.GetCustomAttribute<RootElementAttribute>(true);

				result.AddStartElementWithAttributes(rootElementInfo.RootElementName, string.Format(CultureInfo.InvariantCulture, @"xmlns=""{0}""", result.Namespace), string.Format(CultureInfo.InvariantCulture, @"version=""{0}""", result.Version));

				var overrideManager = new DataOverrideManager(overrideProvider, result);

				var writer = new ComplexTypeWriter(type.Name, type, result);
				writer.WriteXML(dataStructure, overrideManager);

				result.AddEndElement(rootElementInfo.RootElementName);
			}
		}
	}
}
