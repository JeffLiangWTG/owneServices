using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Enterprise.Customs.CA.Business
{
	public class AVSQueryRetryInfoBO : AutoAVSQueryRetryInfoBO
	{
		protected override void SetDefaultValues()
		{
			RetryTimes = 0;
		}

		#region Serialize

		public string Serialize()
		{
			var strBuilder = new StringBuilder();
			StringWriter strWriter = null;
			try
			{
				strWriter = new StringWriter(strBuilder, CultureInfo.InvariantCulture);
				using (XmlTextWriter xmlWriter = new XmlTextWriter(strWriter))
				{
					strWriter = null;
					xmlWriter.WriteStartElement("AVSQueryRetryInfo");
					((IXmlSerializable)this).WriteXml(xmlWriter);
					xmlWriter.WriteEndElement();
					xmlWriter.Flush();
				}
				return strBuilder.ToString();
			}
			finally
			{
				if (strWriter != null)
				{
					strWriter.Dispose();
				}
			}
		}

		#endregion

		#region Deserialize

		public void Deserialize(string xml)
		{
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(xml);
				using (XmlReader xmlReader = XmlReader.Create(stringReader))
				{
					stringReader = null;
					((IXmlSerializable)this).ReadXml(xmlReader);
				}
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Dispose();
				}
			}
		}

		#endregion
	}
}
