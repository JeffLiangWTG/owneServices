using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Xml;

namespace CargoWise.eHub.Gateway.XH
{
	public class XHMessageHeader : MessageHeader
	{
		readonly IList<XHMessageProperty> properties;
 		
		#region constructor

		public XHMessageHeader(IList<XHMessageProperty> properties)
		{
			this.properties = properties;
		}

		#endregion

		#region Properties

		public override string Name => "Context";

		public override string Namespace => string.Empty;

		#endregion

		#region WriteHeaderContents

		protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
		{
			foreach (XHMessageProperty property in this.properties)
			{
				writer.WriteStartElement("Property");
                writer.WriteElementString("Name", property.Name);
				writer.WriteElementString("Type", property.Type.ToString());
				writer.WriteElementString("Value", property.Value);
                writer.WriteEndElement();
			}
		}

		#endregion
	}
}