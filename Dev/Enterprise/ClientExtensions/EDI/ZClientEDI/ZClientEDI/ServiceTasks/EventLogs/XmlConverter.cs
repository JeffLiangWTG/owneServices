using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using CargoWise.Common;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	public static class XmlConverter
	{
		const string xsltPath = "Enterprise.Client.EDI.ServiceTasks.EventLogs.Resources.XSLT_EventLogs.xslt";

		public static string ConvertToIssueXmlFormat(XElement xmlFile)
		{
			return WriteXMLToXML(new XDocument(xmlFile));
		}

		static string WriteXMLToXML(XDocument xmlTree)
		{
			Argument.NotNull(xmlTree, "xmlTree");
			using (var sw = new StringWriter())
			using (StreamReader xsltReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(xsltPath)))
			{
				var xslt = new XslCompiledTransform();
				xslt.Load(XmlReader.Create(xsltReader));
				xslt.Transform(xmlTree.CreateReader(), null, sw);
				return sw.ToString();
			}
		}

		public static bool ExtractData(this XElement xmlFile, EventLogDataTransmissionHandler eventLogDataTransmissionHandler)
		{
			Argument.NotNull(xmlFile, "xmlFile");
			var eventData = xmlFile.Element(XmlPath + "EventData");
			if (eventData != null)
			{
				var eventDataElements = eventData.Elements();
				var eventLogsCallStackExtractor = new EventLogsCallStackExtractor(eventDataElements, XmlPath);
				if (eventLogDataTransmissionHandler.AbleToCollect(eventLogsCallStackExtractor.State))
				{
					var data = eventLogsCallStackExtractor.GetData();
					if (data != null)
					{
						xmlFile.Add(data);
					}
					xmlFile.Add(new XElement(XmlPath + "LoginName", eventLogDataTransmissionHandler.CurrentUser.GS_LoginName));
					xmlFile.Add(new XElement(XmlPath + "UsersEmailAddress", eventLogDataTransmissionHandler.CurrentUser.GS_EmailAddress));
					xmlFile.Add(new XElement(XmlPath + "Company", eventLogDataTransmissionHandler.CurrentCompany.GC_Name));
					return true;
				}
			}
			return false;
		}

		public static XNamespace XmlPath
		{
			get
			{
				return xmlPath ?? (xmlPath = "http://schemas.microsoft.com/win/2004/08/events/event");
			}
		}

		static XNamespace xmlPath;
	}
}
