using System;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class LicenceUsageRequestInfo
	{
		public LicenceUsageRequestInfo(string xml)
		{
			using (var stringReader = new StringReader(xml))
			{
				var reader = new XmlTextReader(stringReader);
				reader.Read();
				reader.ReadStartElement(SystemMessageList.Descriptions.LicenceUsageRequest);

				DateFrom = ParseDate(reader.ReadElementString("DateFrom"));
				DateTo = ParseDate(reader.ReadElementString("DateTo"));
				RequestedBy = reader.ReadElementString("RequestedBy");
			}
		}

		DateTime ParseDate(string dateText)
		{
			ZDateTime d;
			if (!ZDateTime.TryParseExact(dateText, out d, DateFormat))
			{
				throw new XmlException("Date invalid format");
			}
			return d.ToDateTime();
		}

		public DateTime DateFrom { get; private set; }
		public DateTime DateTo { get; private set; }
		public string RequestedBy { get; private set; }

		public const string DateFormat = "yyyy/MM/dd";
	}
}
