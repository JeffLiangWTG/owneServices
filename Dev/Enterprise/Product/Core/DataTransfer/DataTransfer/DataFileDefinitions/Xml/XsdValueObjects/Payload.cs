using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.XmlSerializers")]
	[XmlRoot(ElementName = "Payload", Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclass("Enterprise.DataTransfer.Xml.XsdVersion1.AutoPayload")]
	public class Payload : AutoPayload, IXmlSerializable
	{
#if DEBUG
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
#endif
		[XmlIgnore] // For NoNewXmlSerializedFieldsOrProperties test only - anyway it will be processed manually in WriteXml
		public object Data
		{
			get { return data; }
			set
			{
				data = value;
				if (value != null)
				{
					IsSpecified = true;
				}
			}
		}
		object data;

		[XmlIgnore]
		public IValueObjectExportContext Context { get; set; }

		[XmlIgnore]
		public IValueObjectDataAdapter DataAdapter { get; set; }

		[XmlIgnore]
		public XmlValueObjectSerializer ValueObjectSerializer { get; set; }

		#region Xml

		public XmlElement ToXmlElement()
		{
			string xml = GetOuterXml();
			if (!string.IsNullOrEmpty(xml))
			{
				XmlDocument document = new XmlDocument();
				document.LoadXml(GetOuterXml());
				return document.DocumentElement;
			}
			return null;
		}

		public string GetOuterXml()
		{
			using (StringWriter sw = new StringWriter())
			using (XmlWriter writer = XmlWriter.Create(sw))
			{
				((IXmlSerializable)this).WriteXml(writer);
				writer.Flush();
				sw.Flush();

				String xml = sw.GetStringBuilder().Replace(" xmlns=\"" + XmlSchemaDefinitionsBase.EdiXmlNamespace + "\"", "").ToString().Trim();
				if (xml.StartsWith("<?"))
				{
					xml = xml.Substring(xml.IndexOf("?>") + 2);
				}
				return xml.Trim();
			}
		}

		#endregion

		#region IXmlSerializable Members

		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			throw new NotSupportedException("Payload data should not be deserialized directly.");
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			if (Data != null)
			{
				if (ValueObjectSerializer == null || Data is IEnumerable)
				{
					CargoWise.Common.Argument.NotNull(DataAdapter, "DataAdapter", "DataAdapter was not initialized");
				}

				XmlValueObjectSerializer serializer = ValueObjectSerializer ??
					new XmlValueObjectSerializer(DataAdapter.ValueObjectType);
				if (Data is IValueObject)
				{
					serializer.Serialize(writer, (IValueObject)Data);
				}
				else if (typeof(BusinessObject).IsAssignableFrom(Data.GetType()))
				{
					serializer.WriteToXml(writer, DataAdapter, (BusinessObject)Data, Context);
				}
				else if (Data is IEnumerable)
				{
					serializer.WriteCollectionToXml(writer, DataAdapter, (IEnumerable)Data, Context);
				}
				else
				{
					throw new NotSupportedException("Not supported element of type " + Data.GetType());
				}
			}
		}

		#endregion
	}
}
