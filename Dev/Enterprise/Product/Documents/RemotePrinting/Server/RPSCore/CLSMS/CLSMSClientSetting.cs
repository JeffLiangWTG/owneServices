using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class CLSMSClientSetting
	{
		CLSMSClientSetting()
		{
		}

		public static CLSMSClientSetting New(XElement element, DbConnection connection)
		{
			var result = new CLSMSClientSetting();
			result.SetValues(element, connection);
			return result;
		}

		[ThreadSafe]
		public static readonly CLSMSClientSetting Empty = new ();

		void SetValues(XElement element, DbConnection connection)
		{
			var regKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			if (regKey != null)
			{
				RegistrationKey = regKey.EnterpriseCode + regKey.ServerCode;
			}
			else
			{
				RegistrationKey = ZString.Empty;
			}
			ServerAddress = DbRegistryHelper.GetxTServerAddress(connection);
			ServerCertificate = DbRegistryHelper.GetxTServerCertificate(connection);
			ApplicationNodeName = element.Element("ApplicationNodeName")?.Value ?? string.Empty;
			ApplicationNodePassword = CustomsServerEncryptor.Encrypt(Authentication.Instance, element.Element("ApplicationNodePassword")?.Value ?? string.Empty);
			RunningIntervalInSeconds = ZInt.ParseSafe(element.Element("RunningIntervalInSeconds")?.Value ?? string.Empty, 10);
			SendFolder = element.Element("SendFolder")?.Value ?? string.Empty;
			UnknownFolder = element.Element("UnknownFolder")?.Value ?? string.Empty;
			InvalidFolder = element.Element("InvalidFolder")?.Value ?? string.Empty;
			RejectedFolder = element.Element("RejectedFolder")?.Value ?? string.Empty;
			ReceiveFolder = element.Element("ReceiveFolder")?.Value ?? string.Empty;
			AcceptedFolder = element.Element("AcceptedFolder")?.Value ?? string.Empty;
			MachineName = element.Element("MachineName")?.Value ?? string.Empty;
		}

		#region Properties

		public string RegistrationKey { get; set; }

		public string ServerAddress { get; set; }

		public string ServerCertificate { get; set; }

		public string ApplicationNodeName { get; set; }

		public string ApplicationNodePassword { get; set; }

		public int RunningIntervalInSeconds { get; set; }

		public string SendFolder { get; set; }

		public string UnknownFolder { get; set; }

		public string InvalidFolder { get; set; }

		public string RejectedFolder { get; set; }

		public string ReceiveFolder { get; set; }

		public string AcceptedFolder { get; set; }

		public string MachineName { get; set; }

		#endregion
	}
}
