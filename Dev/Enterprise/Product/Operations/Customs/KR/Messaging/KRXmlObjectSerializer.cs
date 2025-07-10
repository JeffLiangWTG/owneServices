using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.Shared.MessageDefinitions;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.KR.Messaging
{
	public static class KRXmlObjectSerializer
	{
		public static T Deserialize<T>(TextReader textReader)
		{
			var type = typeof(T);
			var xsdSchemaEmbeddedResourceName = ((EmbeddedResourceAttribute)type.GetCustomAttributes(typeof(EmbeddedResourceAttribute), false).Single()).EmbeddedResourcePath;
			return XmlObjectSerializer.DeserializeWithXSDValidation<T>(XmlObjectSerializer.GetXmlSchemaSet(type, xsdSchemaEmbeddedResourceName, (key) => CreateXmlSchemaSet()), textReader);
		}

		public static T DeserializeWithoutSchemaValidation<T>(TextReader textReader)
		{
			var serializer = ZXmlSerializer.New(typeof(T));
			return (T)serializer.Deserialize(textReader);
		}

		public static Stream Serialize<T>(T xmlObject)
		{
			var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineHandling = NewLineHandling.Entitize };
			return XmlObjectSerializer.SerializeWithNamespacesAsStream(xmlObject, settings);
		}

		static XmlSchemaSet CreateXmlSchemaSet()
		{
			var newSchemaSet = new XmlSchemaSet();
			AdditonalSchemaSets.ForEach(x => newSchemaSet.Add(x));
			return newSchemaSet;
		}

		static List<XmlSchema> CreateAdditonalSchemaSets()
		{
			var schemaList = new List<XmlSchema>();
			var standardEmbeddedResourcesType = typeof(StandardEmbeddedResources);
			var messageDefinitionsKRAssembly = standardEmbeddedResourcesType.Assembly;
			var embeddedResourceAttributes = standardEmbeddedResourcesType.GetCustomAttributes(typeof(EmbeddedResourceAttribute), false);
			foreach (EmbeddedResourceAttribute embeddedResourceAttribute in embeddedResourceAttributes)
			{
				schemaList.Add(XmlObjectSerializer.CreateXmlSchema(messageDefinitionsKRAssembly, embeddedResourceAttribute.EmbeddedResourcePath));
			}
			return schemaList;
		}

		static List<XmlSchema> AdditonalSchemaSets => additonalSchemaSets ?? (additonalSchemaSets = CreateAdditonalSchemaSets());
		[ThreadStatic]
		static List<XmlSchema> additonalSchemaSets;
	}
}
