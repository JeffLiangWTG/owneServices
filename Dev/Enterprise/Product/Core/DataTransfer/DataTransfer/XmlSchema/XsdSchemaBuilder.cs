using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Xml
{
	public class XsdSchemaBuilder : XmlErrorHandler
	{
		public ZXmlSchema Read(Stream xsdStream)
		{
			Errors.Clear();
			ZXmlSchema result = null;

			xsdStream.Position = 0;
			result = ZXmlSchema.Read(xsdStream, new ValidationEventHandler(ShowCompileErrors));

			return result;
		}

		public ZXmlSchema Read(string xml)
		{
			Stream xsdStream = StreamConverter.StringToStream(xml);
			return Read(xsdStream);
		}

		public void Include(ZXmlSchema parentSchema, params ZXmlSchema[] includedSchemas)
		{
			Errors.Clear();
			foreach (ZXmlSchema includedSchema in includedSchemas)
			{
				Include(parentSchema, includedSchema);
			}
		}

		public void Include(ZXmlSchema parentSchema, ZXmlSchema includedSchema)
		{
			Errors.Clear();
			XmlSchemaInclude include = new XmlSchemaInclude();
			include.Schema = includedSchema;
			parentSchema.Includes.Add(include);
		}

		public ZXmlSchema GetSchemaOfNestedElement(ZXmlSchema inputSchema, string outerElementName)
		{
			XmlSchemaElement resultantInnerElement = null;
			foreach (XmlSchemaElement element in inputSchema.Elements.Values)
			{
				if (element.Name == outerElementName)
				{
					object current = element;
					do
					{
						if (current is XmlSchemaElement)
						{
							current = ((XmlSchemaElement)current).ElementSchemaType;
						}
						else if (current is XmlSchemaComplexType)
						{
							current = ((XmlSchemaComplexType)current).ContentTypeParticle;
						}
						else if (current is XmlSchemaSequence)
						{
							foreach (object innerElement in ((XmlSchemaSequence)current).Items)
							{
								if (innerElement is XmlSchemaElement)
								{
									current = innerElement;
									break;
								}
							}
						}
						else
						{
							current = null;
						}
					}
					while (current != null && !(current is XmlSchemaElement));
					resultantInnerElement = (XmlSchemaElement)current;
				}
			}

			ZXmlSchema result = null;
			if (resultantInnerElement != null)
			{
				resultantInnerElement.MinOccursString = null;
				resultantInnerElement.MaxOccursString = null;
				result = CloneSchema(inputSchema);
				for (int i = result.Items.Count - 1; i >= 0; i--)
				{
					if (result.Items[i] is XmlSchemaElement)
					{
						result.Items.RemoveAt(i);
					}
				}
				result.Items.Add(resultantInnerElement);
			}

			return CloneSchemaToFixUnreproducibleBug(result);
		}

		public ZXmlSchema GetSchemaWithElementOfType(ZXmlSchema inputSchema, string schemaTypeName, string elementName)
		{
			ZXmlSchema result = CloneSchema(inputSchema);

			for (int i = result.Items.Count - 1; i >= 0; i--)
			{
				if (result.Items[i] is XmlSchemaElement)
				{
					result.Items.RemoveAt(i);
				}
			}

			XmlSchemaElement element = new XmlSchemaElement();
			element.Name = elementName;
			element.SchemaTypeName = new XmlQualifiedName(schemaTypeName);
			result.Items.Add(element);

			return CloneSchemaToFixUnreproducibleBug(result);
		}

#if DEBUG
		protected virtual
#endif
		ZXmlSchema CloneSchemaToFixUnreproducibleBug(ZXmlSchema xmlSchema)
		{
			// this fixes a difficult to reproduce problem that fails a unit test from time to time - see and vote clinty about this
			return CloneSchema(xmlSchema);
		}

		public ZXmlSchema CloneSchema(ZXmlSchema xmlSchema)
		{
			StringWriter writer = new StringWriter();
			xmlSchema.Write(writer);
			string xmlText = writer.GetStringBuilder().ToString();
			ZXmlSchema result = ZXmlSchema.Read(new StringReader(xmlText), null);

			result.Includes.Clear();
			foreach (XmlSchemaInclude include in xmlSchema.Includes)
			{
				XmlSchemaInclude clonedInclude = CloneInclude(include);
				result.Includes.Add(clonedInclude);
			}
			return result;
		}

		XmlSchemaInclude CloneInclude(XmlSchemaInclude include)
		{
			XmlSchemaInclude result = new XmlSchemaInclude();
			result.Id = include.Id;
			result.Namespaces = new XmlSerializerNamespaces(include.Namespaces.ToArray());
			if (include.Schema != null)
			{
				result.Schema = CloneSchema(include.Schema);
			}
			result.SchemaLocation = include.SchemaLocation;
			result.SourceUri = include.SourceUri;
			return result;
		}

		public void CompileSchema(ZXmlSchema schema)
		{
			try
			{
				XmlSchemas schemas = new XmlSchemas();
				schemas.Add(schema);
				Errors.Clear();
				schemas.Compile(new ValidationEventHandler(ShowCompileErrors), true);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Errors.Add(e.Message);
			}
		}
	}
}
