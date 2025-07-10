using System.Collections.Generic;
using System.Xml;
#if NETFRAMEWORK
using Enterprise.RemotePrinting.Client.RemotePrintServer;
#endif

namespace Enterprise.RemotePrinting.Client.Setup.Testing
{
	public class DummyPrintServer : TinySoapServer
	{
		public DummyPrintServer(string url, string msiFileName, string version) : base(url)
		{
			this.msiFileName = msiFileName;
			this.version = version;
		}

		readonly string msiFileName;
		readonly string version;

		public string[] Requests
		{
			get
			{
				string[] result;
				lock (requests)
				{
					result = requests.ToArray();
				}
				return result;
			}
		}
		readonly List<string> requests = new List<string>();

		protected override bool HandleSoapMethod(XmlDocument inputDocument, XmlDocument outputDocument, XmlElement requestMethodElement, XmlElement responseMethodElement)
		{
			var ns = new XmlNamespaceManager(new NameTable());
			ns.AddNamespace("soap", SoapNsUri);
			ns.AddNamespace("x", ResultNamespace);

			var requestVersionInfoElement = inputDocument.SelectSingleNode("//soap:Header/x:RemotePrintingSoapHeader/x:VersionInfo", ns);
			var requestVersion = requestVersionInfoElement?.InnerText ?? "-";
			lock (requests)
			{
				requests.Add(requestMethodElement.LocalName + "|" + requestVersion);
			}

			switch (requestMethodElement.LocalName)
			{
#if NETFRAMEWORK
				case nameof(RemotePrintingService.CheckClientUpdate):
#else
				case "CheckClientUpdate":
#endif
					ProcessClientUpdate(outputDocument, requestMethodElement, responseMethodElement);
					return true;
#if NETFRAMEWORK
				case nameof(RemotePrintingService.GetWatermarkInfo):
#else
				case "GetWatermarkInfo":
#endif
					ProcessWatermarkInfo(outputDocument, requestMethodElement, responseMethodElement);
					return true;
#if NETFRAMEWORK
				case nameof(RemotePrintingService.Nudge):
#else
				case "Nudge":
#endif
					ProcessNudge(outputDocument, requestMethodElement, responseMethodElement);
					return true;
				default:
					return false;
			}
		}

		void ProcessNudge(XmlDocument outputDocument, XmlElement requestMethodElement, XmlElement responseMethodElement)
		{
			var reply = $"true";

			AddSoapResult(outputDocument, requestMethodElement, responseMethodElement, reply);
		}

		void ProcessClientUpdate(XmlDocument outputDocument, XmlElement requestMethodElement, XmlElement responseMethodElement)
		{
			var fileUrl = "file:///" + msiFileName.Replace('\\', '/').Replace(" ", "%20");
			var reply = $"<Version xmlns='{ResultNamespace}'>{version}</Version><Link xmlns='{ResultNamespace}'>{fileUrl}</Link>";

			AddSoapResult(outputDocument, requestMethodElement, responseMethodElement, reply);
		}

		void ProcessWatermarkInfo(XmlDocument outputDocument, XmlElement requestMethodElement, XmlElement responseMethodElement)
		{
			var reply =
$@"<UseTextWatermark xmlns='{ResultNamespace}'>true</UseTextWatermark>
<TextWatermark xmlns='{ResultNamespace}'>Test</TextWatermark>
<HorizontalAlignment xmlns='{ResultNamespace}'>Left</HorizontalAlignment>
<VerticalAlignment xmlns='{ResultNamespace}'>Top</VerticalAlignment>
<Rotation xmlns='{ResultNamespace}'>0</Rotation>
<FontSize xmlns='{ResultNamespace}'>10</FontSize>
<Opacity xmlns='{ResultNamespace}'>0</Opacity>
<HorizontalOffset xmlns='{ResultNamespace}'>0</HorizontalOffset>
<VerticalOffset xmlns='{ResultNamespace}'>0</VerticalOffset>";

			AddSoapResult(outputDocument, requestMethodElement, responseMethodElement, reply);
		}
	}
}
