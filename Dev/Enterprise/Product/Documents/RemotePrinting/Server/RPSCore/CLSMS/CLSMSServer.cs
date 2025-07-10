using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class CLSMSServer
	{
		protected virtual DbConnection NewConnection() => DbHelper.NewConnection();

		public CLSMSClientSetting GetSettingByMachineName(string machineName)
		{
			CLSMSClientSetting result;
			if (string.IsNullOrWhiteSpace(machineName))
			{
				result = CLSMSClientSetting.Empty;
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
		CLSMSClientSetting GetSettingByMachineName(string machineName, DbConnection connection)
		{
			CLSMSClientSetting result = null;

			foreach (var bytes in DbRegistryHelper.GetStmDataSettings(connection, "CLSMSMessageSending"))
			{
				if (bytes.Length > 0)
				{
					var document = XDocument.Parse(Encoding.Unicode.GetString(bytes));
					var element = document
						.Descendants("CLSMSMessageSending")
						.FirstOrDefault(x => x.Element("MachineName")?.Value?.Equals(machineName) ?? false);

					if (element != null)
					{
						result = CLSMSClientSetting.New(element, connection);
						break;
					}
				}
			}

			return result ?? CLSMSClientSetting.Empty;
		}
	}
}
