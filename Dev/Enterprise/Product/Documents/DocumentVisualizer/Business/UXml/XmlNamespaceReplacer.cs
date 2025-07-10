using System;
using System.IO;
using System.Xml;
using CargoWise.IO;
using CargoWise.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	class XmlNamespaceReplacer
	{
		public string GetCustomNamespace(string universalNamespace, string userdefinedNamespace)
		{
			var escapedUri = StringUtilities.EscapeUriStringWithQuery(userdefinedNamespace);

			if (Uri.IsWellFormedUriString(escapedUri, UriKind.Relative))
			{
				if (escapedUri.StartsWith("/", StringComparison.Ordinal) && universalNamespace.EndsWith("/", StringComparison.Ordinal))
				{
					return universalNamespace.Substring(0, universalNamespace.Length - 1) + escapedUri;
				}

				if (!escapedUri.StartsWith("/", StringComparison.Ordinal) && !universalNamespace.EndsWith("/", StringComparison.Ordinal))
				{
					return universalNamespace + "/" + escapedUri;
				}

				return universalNamespace + escapedUri;
			}
			else if (Uri.IsWellFormedUriString(escapedUri, UriKind.Absolute))
			{
				return escapedUri;
			}
			else
			{
				throw new InvalidNamespaceException("XmlNamespace " + userdefinedNamespace + "is not valid.");
			}
		}

		public void ReplaceXmlWithNamespace(SubStreamableStream stream, Stream outputStream, string xmlNamespace)
		{
			using (XmlReader reader = XmlReader.Create(stream))
			{
				XmlWriterSettings ws = new XmlWriterSettings();
				ws.Indent = true;

				using (XmlWriter writer = XmlWriter.Create(outputStream, ws))
				{
					while (reader.Read())
					{
						switch (reader.NodeType)
						{
							case XmlNodeType.Element:
								ProcessElement(reader, writer, xmlNamespace);
								break;
							case XmlNodeType.Text:
								writer.WriteString(reader.Value);
								break;
							case XmlNodeType.Whitespace:
							case XmlNodeType.SignificantWhitespace:
								writer.WriteWhitespace(reader.Value);
								break;
							case XmlNodeType.CDATA:
								writer.WriteCData(reader.Value);
								break;
							case XmlNodeType.EntityReference:
								writer.WriteEntityRef(reader.Name);
								break;
							case XmlNodeType.XmlDeclaration:
							case XmlNodeType.ProcessingInstruction:
								writer.WriteProcessingInstruction(reader.Name, reader.Value);
								break;
							case XmlNodeType.DocumentType:
								writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
								break;
							case XmlNodeType.Comment:
								writer.WriteComment(reader.Value);
								break;
							case XmlNodeType.EndElement:
								writer.WriteFullEndElement();
								break;
						}
					}
				}
			}
		}

		void ProcessElement(XmlReader reader, XmlWriter writer, string ns)
		{
			writer.WriteStartElement(reader.Prefix, reader.LocalName, ns);

			if (reader.AttributeCount > 0 && reader.GetAttribute("xmlns") != null)
			{
				while (reader.MoveToNextAttribute())
				{
					if (reader.Name == (NoResString)"xmlns")
					{
						writer.WriteAttributeString("xmlns", ns);
					}
					else
					{
						writer.WriteAttributeString(reader.Name, reader.Value);
					}
				}
			}
			else
			{
				writer.WriteAttributes(reader, false);
			}

			if (reader.IsEmptyElement)
			{
				writer.WriteEndElement();
			}

			reader.MoveToElement();
		}

		[Serializable]
		public class InvalidNamespaceException : Exception
		{
			public InvalidNamespaceException(string message) : base(message) { }

#if NETFRAMEWORK
			protected InvalidNamespaceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
		}
	}
}
