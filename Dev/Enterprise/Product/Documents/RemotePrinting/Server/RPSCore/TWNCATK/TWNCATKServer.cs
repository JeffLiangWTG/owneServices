using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class TWNCATKServer
	{
		protected virtual DbConnection NewConnection()
		{
			return DbHelper.NewConnection();
		}

		public TWNCATKClientSetting GetSettingByMachineName(string machineName)
		{
			TWNCATKClientSetting result;
			if (string.IsNullOrWhiteSpace(machineName))
			{
				result = TWNCATKClientSetting.Empty;
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
		TWNCATKClientSetting GetSettingByMachineName(string machineName, DbConnection connection)
		{
			TWNCATKClientSetting result = null;

			foreach (var bytes in DbRegistryHelper.GetStmDataSettings(connection, "TWNCATKClientSetting"))
			{
				if (bytes.Length > 0)
				{
					var document = XDocument.Parse(Encoding.Unicode.GetString(bytes));
					var element = document
						.Descendants("TWNCATKClientSetting")
						.FirstOrDefault(x => x.Element("MachineName")?.Value?.Equals(machineName) ?? false);

					if (element != null)
					{
						result = TWNCATKClientSetting.New(element, connection);
						break;
					}
				}
			}

			return result ?? TWNCATKClientSetting.Empty;
		}
	}
}
