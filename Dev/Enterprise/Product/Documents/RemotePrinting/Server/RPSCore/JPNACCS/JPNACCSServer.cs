using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class JPNACCSServer
	{
		protected virtual DbConnection NewConnection() => DbHelper.NewConnection();

		public JPNACCSClientSetting GetSettingByMachineName(string machineName)
		{
			JPNACCSClientSetting result;
			if (string.IsNullOrWhiteSpace(machineName))
			{
				result = JPNACCSClientSetting.Empty;
			}
			else
			{
				using (var connection = NewConnection())
				{
					return GetSettingByMachineName(machineName, connection);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		JPNACCSClientSetting GetSettingByMachineName(string machineName, DbConnection connection)
		{
			JPNACCSClientSetting result = null;

			foreach (var bytes in DbRegistryHelper.GetStmDataSettings(connection, "JPMailboxAndRemoteWebPrintClientCredentials"))
			{
				if (bytes.Length > 0)
				{
					var document = XDocument.Parse(Encoding.Unicode.GetString(bytes));
					var element = document
						.Descendants("MailboxAndRemoteWebPrintClientCredentials")
						.FirstOrDefault(x => x.Element("LocalComputerAlias")?.Value?.Equals(machineName) ?? false);

					if (element != null)
					{
						result = JPNACCSClientSetting.New(element, connection);
						break;
					}
				}
			}

			return result ?? JPNACCSClientSetting.Empty;
		}
	}
}
