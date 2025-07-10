using System.Globalization;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Licensing.Registration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class CNSWClientSetting
	{
		CNSWClientSetting() { }

		public static CNSWClientSetting New(XElement element, DbConnection connection)
		{
			var result = new CNSWClientSetting();
			result.SetValues(element, connection);
			return result;
		}

		[ThreadSafe]
		public static readonly CNSWClientSetting Empty = new ();

		void SetValues(XElement element, DbConnection connection)
		{
			MachineName = element.Element("MachineName")?.Value ?? string.Empty;
			SendFolder = element.Element("SendFolder")?.Value ?? string.Empty;
			AcdaSendFolder = element.Element("AcdaSendFolder")?.Value ?? string.Empty;
			AcdaReceiveFolder = element.Element("AcdaReceiveFolder")?.Value ?? string.Empty;
			AcdaErrorResponseFolder = element.Element("AcdaErrorResponseFolder")?.Value ?? string.Empty	;
			AcdaArchiveFolder = element.Element("AcdaArchiveFolder")?.Value ?? string.Empty;
			ReceiveFolder = element.Element("ReceiveFolder")?.Value ?? string.Empty	;
			ErrorResponseFolder = element.Element("ErrorResponseFolder")?.Value ?? string.Empty;
			ArchiveFolder = element.Element("ArchiveFolder")?.Value ?? string.Empty;
			RunningIntervalInSeconds = int.Parse(element.Element("RunningIntervalInSeconds")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
			EHubClientID = element.Element("EHubClientID")?.Value ?? string.Empty;
			EHubClientStatus = element.Element("EHubClientStatus")?.Value ?? string.Empty;
			EHubClientPassword = CustomsServerEncryptor.Encrypt(Authentication.Instance, RegistrationKeyUtility.GetRegistrationKeyXmlPair(new FreightNotesRegistryItem().LoadValue(connection)).Key.Password);
			EHubGatewayServerAddress = DbRegistryHelper.GetEhubGatewayServerAddress(connection);
		}

		public string MachineName { get; set; }

		public string AcdaSendFolder { get; set; }

		public string AcdaReceiveFolder { get; set; }

		public string AcdaErrorResponseFolder { get; set; }

		public string AcdaArchiveFolder { get; set; }

		public string SendFolder { get; set; }

		public string ReceiveFolder { get; set; }

		public string ErrorResponseFolder { get; set; }

		public string ArchiveFolder { get; set; }

		public int RunningIntervalInSeconds { get; set; }

		public string EHubClientID { get; set; }

		public string EHubClientStatus { get; set; }

		public string EHubClientPassword { get; set; }

		public string EHubGatewayServerAddress { get; set; }
	}
}
