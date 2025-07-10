using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business.Licencing
{
	public static class ClientStaffReportRequest
	{
		public static void Send(LicenceDatabase database)
		{
			var licenceCode = database.LicenceCodeForSystemMessage;
			var xml = BuildXml();
			var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			messageCreator.Create(factory, xml, licenceCode);
			factory.Save();
		}

		static string BuildXml()
		{
			var stringBuilder = new StringBuilder(500);

			using (var writer = XmlWriter.Create(stringBuilder,
				new XmlWriterSettings
				{
					OmitXmlDeclaration = true,
					Indent = true
				}))
			{
				writer.WriteStartElement(SystemMessageList.Descriptions.StaffReportRequest);
				writer.WriteEndElement();
			}
			return stringBuilder.ToString();
		}
	}
}
