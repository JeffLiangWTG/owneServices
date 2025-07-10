using System;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class LogsRequestInfo
	{
		public LogsRequestInfo(string xml)
		{
			using (var stringReader = new StringReader(xml))
			{
				var reader = new XmlTextReader(stringReader);
				reader.Read();
				reader.ReadStartElement(SystemMessageList.Descriptions.LogsRequest);

				DateFromUtc = ParseDate(reader.ReadElementString("DateFromUtc"));
				DateToUtc = ParseDate(reader.ReadElementString("DateToUtc"));
				ServiceTaskCode = reader.ReadElementString("ServiceTaskCode");
				IncidentNumber = reader.ReadElementString("IncidentNumber");
				HostServerName = reader.ReadElementString("HostServerName");
				HostDBName = reader.ReadElementString("HostDBName");
				HostConnectionServerName = reader.ReadElementString("HostConnectionServerName");
				MaxZipSize = Convert.ToInt32(reader.ReadElementString("MaxZipSize"), CultureInfo.InvariantCulture);
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

		public DateTime DateFromUtc { get; private set; }
		public DateTime DateToUtc { get; private set; }
		public string ServiceTaskCode { get; private set; }
		public string IncidentNumber { get; private set; }
		public string HostServerName { get; private set; }
		public string HostDBName { get; private set; }
		public string HostConnectionServerName { get; private set; }
		public int MaxZipSize { get; private set; }

		public const string DateFormat = "yyyy/MM/dd";
	}
}
