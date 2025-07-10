using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Licensing
{
	internal interface ILicenceUpdateNotification
	{
		void Send(BusinessObjectFactory factory, string updateDataXMLPacket, string message);
	}

	internal class LicenceUpdateNotification : ILicenceUpdateNotification
	{
		public void Send(BusinessObjectFactory factory, string updateDataXMLPacket, string message)
		{
			string xml = BuildResponseXml(updateDataXMLPacket, message);
			var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
			messageCreator.Create(factory, xml);
		}

		string BuildResponseXml(string updateDataXMLPacket, string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { OmitXmlDeclaration = true }))
			{
				writer.WriteStartElement("ReferenceDataUpdateResponse");
				writer.WriteElementString("Status", message);
				writer.WriteElementString("ReferenceDataUpdate", updateDataXMLPacket);
			}

			return stringBuilder.ToString();
		}
	}
}
