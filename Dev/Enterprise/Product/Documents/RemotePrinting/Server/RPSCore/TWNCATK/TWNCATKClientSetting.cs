using System.Globalization;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Licensing.Registration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class TWNCATKClientSetting
	{
		TWNCATKClientSetting()
		{
		}

		public static TWNCATKClientSetting New(XElement element, DbConnection connection)
		{
			var result = new TWNCATKClientSetting();
			result.SetValues(element, connection);
			return result;
		}

		[ThreadSafe]
		public static readonly TWNCATKClientSetting Empty = new ();

		void SetValues(XElement element, DbConnection connection)
		{
			MachineName = element.Element("MachineName")?.Value ?? string.Empty;
			EHubClientID = element.Element("EHubClientID")?.Value ?? string.Empty;
			EHubClientStatus = element.Element("EHubClientStatus")?.Value ?? string.Empty;
			RunningIntervalInSeconds = int.Parse(element.Element("RunningIntervalInSeconds")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
			SendToFolder = element.Element("SendToFolder")?.Value ?? string.Empty;
			EHubGatewayServerAddress = DbRegistryHelper.GetEhubGatewayServerAddress(connection);
			EHubClientPassword = CustomsServerEncryptor.Encrypt(Authentication.Instance, RegistrationKeyUtility.GetRegistrationKeyXmlPair(new FreightNotesRegistryItem().LoadValue(connection)).Key.Password);
		}

		public string MachineName { get; set; }

		public string EHubClientID { get; set; }

		public string EHubClientStatus { get; set; }

		public int RunningIntervalInSeconds { get; set; }

		public string SendToFolder { get; set; }

		public string EHubGatewayServerAddress { get; set; }

		public string EHubClientPassword { get; set; }
	}
}
