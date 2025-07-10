using System;
using CargoWise.Data;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business.Testing
{
	class PowerBiServerInformationTest : TransactionedTestCase
	{
		public void TestPowerBiServerInformationHasError()
		{
			var reportServer = "TestPowerBiReportsUrl";
			var reportCredentials = BiReportCredentialRegistryItem.BiReportCredentialTestValue();

			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, reportCredentials))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, reportServer))
			{
				var serverInfo = new PowerBiServerInformation(reportServer);
				AssertEquals("Report Server", reportServer, serverInfo.PowerBiWebPortalUrl);
				AssertEquals("Reports Version", null, serverInfo.PowerBiReportsVersion);
			}
		}
	}
}
