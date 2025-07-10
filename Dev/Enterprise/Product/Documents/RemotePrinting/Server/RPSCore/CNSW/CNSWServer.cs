using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class CNSWServer
	{
		protected virtual DbConnection NewConnection()
		{
			return DbHelper.NewConnection();
		}

		public CNSWClientSetting GetSettingByMachineName(string machineName)
		{
			CNSWClientSetting result;
			if (string.IsNullOrWhiteSpace(machineName))
			{
				result = CNSWClientSetting.Empty;
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
		CNSWClientSetting GetSettingByMachineName(string machineName, DbConnection connection)
		{
			CNSWClientSetting result = null;

			foreach (var bytes in DbRegistryHelper.GetStmDataSettings(connection, "CNSWClientApplicationSetting"))
			{
				if (bytes.Length > 0)
				{
					var document = XDocument.Parse(Encoding.Unicode.GetString(bytes));
					var element = document
						.Descendants("CNSWClientSetting")
						.FirstOrDefault(x => x.Element("MachineName")?.Value?.Equals(machineName) ?? false);

					if (element != null)
					{
						result = CNSWClientSetting.New(element, connection);
						break;
					}
				}
			}

			return result ?? CNSWClientSetting.Empty;
		}
	}
}
