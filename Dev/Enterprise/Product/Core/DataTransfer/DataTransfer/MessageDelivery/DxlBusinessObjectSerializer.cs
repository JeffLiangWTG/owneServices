using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.MessageDelivery
{
	public class DxlBusinessObjectSerializer : IBusinessObjectSerializer
	{
		public DxlBusinessObjectSerializer(IValueObjectDataAdapter dataAdapter, ZString messagePurpose)
		{
			serializer = new XmlBusinessObjectSerializer(dataAdapter, messagePurpose);
		}

		#region IBusinessObjectSerializer Members

		public SubStreamableStream SerializeToStream(BusinessObject businessObjectToSerialize)
		{
			serializer.Context = Context;
			serializer.Mode = Mode;
			using (var xmlStream = serializer.SerializeToStream(businessObjectToSerialize))
			{
				factory = businessObjectToSerialize.Factory;
				return ConvertXmlToDxlStream(xmlStream, Mode, GetNextDescartesMessageNumber());
			}
		}

		public SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObject)
		{
			serializer.Context = Context;
			serializer.Mode = Mode;
			using (var xmlStream = serializer.SerializeToStream(businessObject))
			{
				factory = businessObject.First().Factory;
				return ConvertXmlToDxlStream(xmlStream, Mode, GetNextDescartesMessageNumber());
			}
		}

		#endregion

		public IValueObjectExportContext Context { get; set; }
		public IEDICommunicationsMode Mode { get; set; }

		protected SubStreamableStream ConvertXmlToDxlStream(Stream xmlStream, IEDICommunicationsMode mode, string messageNumber)
		{
			var dxlStream = (SubStreamableStream)new MemoryStream();
			var xmlWriter = new XmlTextWriter(dxlStream, new UTF8Encoding(false));
			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.WriteStartDocument();
			{
				xmlWriter.WriteStartElement("S:Envelope");
				{
					xmlWriter.WriteAttributeString("xmlns", "S", null, "http://www.w3.org/2003/05/soap-envelope");
					xmlWriter.WriteAttributeString("xmlns", "wsa", null, "http://schemas.xmlsoap.org/ws/2004/03/addressing");
					xmlWriter.WriteAttributeString("xmlns", "ebi", null, "http://www.myvan.descartes.com/ebi/2004/r1");
					xmlWriter.WriteStartElement("S", "Header", null);
					{
						xmlWriter.WriteStartElement("wsa", "From", null);
						{
							xmlWriter.WriteElementString("wsa", "Address", null, string.Format(CultureInfo.InvariantCulture, "urn:zz:{0}", mode.EK_LocalPartyVanID));
						}
						xmlWriter.WriteEndElement();
						xmlWriter.WriteElementString("wsa", "To", null, string.Format(CultureInfo.InvariantCulture, "urn:zz:{0}", mode.EK_RelatedPartyVanID));
						xmlWriter.WriteElementString("wsa", "Action", null, string.Format(CultureInfo.InvariantCulture, "urn:myvan:{0}", mode.EK_MessagePurpose));
						xmlWriter.WriteStartElement("ebi", "Sequence", null);
						{
							xmlWriter.WriteElementString("ebi", "MessageNumber", null, messageNumber);
						}
						xmlWriter.WriteEndElement();
						xmlWriter.WriteStartElement("ebi", "Created", null);
						{
							xmlWriter.WriteValue(ZDateTime.Now.ToDateTime());
						}
						xmlWriter.WriteEndElement();
					}
					xmlWriter.WriteEndElement();
					xmlWriter.WriteStartElement("S", "Body", null);
					{
						var reader = new XmlTextReader(xmlStream);
						reader.MoveToContent();
						xmlWriter.WriteNode(reader, false);
						xmlStream.Position = 0;
					}
				}
				xmlWriter.WriteFullEndElement();
			}
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			dxlStream.Position = 0;
			return dxlStream;
		}

		string GetNextDescartesMessageNumber()
		{
			string messageNumber;

			if (factory.Connection.IsInTransaction)
			{
				messageNumber = GetNextDescartesMessageNumberCore(factory);
			}
			else
			{
				using (var manager = factory.Connection.BeginTransactionWithManager())
				{
					messageNumber = GetNextDescartesMessageNumberCore(factory);
					manager.CommitTransaction();
				}
			}

			return messageNumber;
		}

		string GetNextDescartesMessageNumberCore(IDbConnected connectionWrapper)
		{
			return Env.NumberFountains.DescartesMessageNumberFountain.GetNextFormatted(connectionWrapper);
		}

		IDbConnected factory;
		readonly XmlBusinessObjectSerializer serializer;
	}
}
