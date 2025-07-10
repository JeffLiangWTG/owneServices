using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.GUI.Testing
{
	public class BiManagerFormTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestClosingFormBeforeLoadingInformation()
		{
			var bo = new BiMonitorBusinessObject();
			using (var form = new BiManagerForm(bo))
			{
				AssertNoExceptionThrown(() =>
				{
					form.Show();
				});

#if WINZOR
				var credential = new Enterprise.Registry.Business.BiReportCredential
				{
					Domain = "TestDomain",
					UserName = "TestUserName",
					Password = "P$ssword"
				};

				using (Enterprise.Registry.Business.SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, System.Environment.MachineName))
				using (Enterprise.Registry.Business.SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, credential))
				using (Enterprise.Registry.Business.SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, System.Environment.MachineName))
				using (Enterprise.Registry.Business.SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, System.Environment.MachineName))
				{
					CombineAssertions("Winzor version should not load AnalysisServerInfo and PowerBiInfo", () =>
					{
						AssertEquals("Analysis Server Name", string.Empty, bo.AnalysisServerInfo.ServerName);
						AssertEquals("Analysis Server Version", string.Empty, bo.AnalysisServerInfo.ServerVersion);
						AssertEquals("Analysis Server Mode", string.Empty, bo.AnalysisServerInfo.ServerMode);
						AssertEquals("Analysis Server TotalEstimatedMemoryUsage", string.Empty, bo.AnalysisServerInfo.TotalEstimatedMemoryUsage);

						AssertNull("PowerBi Web Portal Url", bo.PowerBiServerInfo.PowerBiWebPortalUrl);
						AssertNull("PowerBi Server Version", bo.PowerBiServerInfo.PowerBiServerVersion);
						AssertNull("PowerBi Reports Status", bo.PowerBiServerInfo.PowerBiReportsStatus);
						AssertNull("PowerBi Reports Version", bo.PowerBiServerInfo.PowerBiReportsVersion);
					});
				}
#endif
			}
		}

		[UseSnapshotProtection]
		public void TestInvalidAuditOnBiManagerShouldThrowExceptionAndDisplayError()
		{
			var bo = new BiMonitorBusinessObject
			{
				AnalysisServerInfo = new AnalysisServerInformation(Db.ServerName, Db.ServerName)
			};
			using (var form = new BiManagerFormForTest(bo))
			{
				AssertEquals("Audit server error", ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}
	}
}
