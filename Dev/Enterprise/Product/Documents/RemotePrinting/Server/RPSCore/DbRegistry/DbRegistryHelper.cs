using System.Collections.Generic;
using System.Xml.XPath;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Business.ConfigurationProvider;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public static class DbRegistryHelper
	{
		public static string GetNodeText(this IXPathNavigable node, string childNodeName)
		{
			return node.CreateNavigator().SelectSingleNode(childNodeName)?.Value;
		}

		public static IEnumerable<byte[]> GetStmDataSettings(DbConnection connection, string registryName)
		{
			var result = new List<byte[]>();
			using (var command = connection.Command($@"SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name=@registryName AND SD_BinaryValue IS NOT NULL"))
			{
				command.AddParameter("@registryName", System.Data.SqlDbType.VarChar, registryName);

				using (var resultReader = command.ExecuteReader())
				{
					while (resultReader.Read())
					{
						result.Add(resultReader[0] as byte[]);
					}
				}
				return result;
			}
		}

		public static string GetEhubGatewayServerAddress(DbConnection connection)
		{
			string result;

			var sendToTestServer = new EHubSendInterchangesToTestGatewayRegistryItem().LoadValue(connection);
			if (sendToTestServer)
			{
				result = new EHubTestGatewayServerAddressRegistryItem().LoadValue(connection);
			}
			else
			{
				result = new EhubGatewayServerAddressRegistryItem().LoadValue(connection);
			}
			return result;
		}

		public static int GetXTIdleConnectionRetryPause()
		{
			return DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.Value;
		}

		public static int GetXTIdleConnectionKeepAlive()
		{
			return DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.Value;
		}

		public static int GetInterchangeCountPerBatchOnReceiving()
		{
			return DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnReceiving.Value;
		}

		public static int GetXTServerMessageChunkSizeWhenSending()
		{
			return DirectxTMessagingRegistry.Instance.XTServerMessageChunkSizeWhenSending.Value;
		}

		public static string GetxTServerAddress(DbConnection connection)
		{
			var refDatabase = new ReferenceDatabaseUtils(connection);
			var key = DirectxTMessagingRegistry.Instance.ConnectionToXTServer.Value == ConnectionToXTServerOptions.XtProduction.Code ? XTServerServerProdKey : XTServerServerTestKey;
			return refDatabase.RefSysConfigLoader.Load(key, ZDateTime.Today)?.ZRC_StringValue ?? ZString.Empty;
		}

		public static string GetxTServerCertificate(DbConnection connection)
		{
			var refDatabase = new ReferenceDatabaseUtils(connection);
			var key = DirectxTMessagingRegistry.Instance.ConnectionToXTServer.Value == ConnectionToXTServerOptions.XtProduction.Code ? XTServerCertificateProdKey : XTServerCertificateTestKey;
			return refDatabase.RefSysConfigLoader.Load(key, ZDateTime.Today)?.ZRC_StringValue ?? ZString.Empty;
		}

		const string XTServerCertificateProdKey = "XTCAPROD";
		const string XTServerCertificateTestKey = "XTCATEST";
		const string XTServerServerProdKey = "XTSVRPROD";
		const string XTServerServerTestKey = "XTSVRTEST";
	}
}
