using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Xml;

namespace CargoWise.eHub.Gateway
{
	public class XHubMessageHeader : MessageHeader
	{
		readonly IList<Property> properties;
 		
		#region constructor

		public XHubMessageHeader(IList<Property> properties)
		{
			this.properties = properties;
		}

		#endregion

		#region Properties

		public override string Name => "XHubContext";

		public override string Namespace => string.Empty;

		#endregion

		#region WriteHeaderContents

		protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
		{
			foreach (Property property in this.properties)
			{
				writer.WriteStartElement("Property");
                writer.WriteElementString("Name", property.Name);
                writer.WriteElementString("Namespace", property.Namespace);
                writer.WriteElementString("Value", property.Value);
                writer.WriteEndElement();
			}
		}

		#endregion
	}
}