using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Bi.Registration.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.GUI.Testing
{
	public class AnalysisServerControlTest : TestCaseWithFactory
	{
		public void TestIsInternalOrSupportField()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.IsInternalSystemForTest = false;

			try
			{
				using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				using (var control = new AnalysisServerControl())
				{
					CombineAssertions(() =>
					{
						Assert("precondition Support", !GlbStaff.CurrentUser.IsSupportUser);
						Assert("precondition Dev", !GlbStaff.CurrentUser.GS_IsDeveloper);

						Assert("UnKnownUser should not be internal or support", !control.IsInternalOrSupport);
					});
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				using (var control = new AnalysisServerControl())
				{
					CombineAssertions(() =>
						{
							Assert("precondition Support", !GlbStaff.CurrentUser.IsSupportUser);
							Assert("precondition Dev", !GlbStaff.CurrentUser.GS_IsDeveloper);

							Assert("WebUser should not be internal or support", !control.IsInternalOrSupport);
						});
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				using (var control = new AnalysisServerControl())
				{
					CombineAssertions(() =>
						{
							Assert("Support", GlbStaff.CurrentUser.IsSupportUser);
							Assert("Dev", GlbStaff.CurrentUser.GS_IsDeveloper);

							Assert("Is support user", control.IsInternalOrSupport);
						});
				}
			}
			finally
			{
				registration.KeyForTest.IsInternalSystemForTest = null;
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM | RequiredSoftware.SsasTabular2016OrLater)]
		public void TestActivateOnlyAccessibleToSupport()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.IsInternalSystemForTest = false;

			try
			{
				var bo = new BiMonitorBusinessObject
				{
					AnalysisServerInfo = new AnalysisServerInformation(Db.ServerName, Db.ServerName)
				};

				using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				using (var form = new BiManagerForm(bo))
				using (var control = GetAnalysisServerControlForTest(form))
				{
					Assert("Not a support user", !control.IsInternalOrSupport);
					var menuItems = GetRightClickMenuOnSsasCubeGrid(control);

					CombineAssertions("Activate, Deactivate, Redeploy, and Reprocess options should be invisible to non-support staff", () =>
					{
						Assert("Activate", !menuItems.ToList<string>().Any(i => i == "Activate"));
						Assert("Deactivate", !menuItems.ToList<string>().Any(i => i == "Deactivate"));
						Assert("Reprocess", !menuItems.ToList<string>().Any(i => i == "Reprocess"));
						Assert("Redeploy", !menuItems.ToList<string>().Any(i => i == "Redeploy"));
						Assert("Redeploy", !menuItems.ToList<string>().Any(i => i == "Cancel Redeploy"));
					});
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				using (var form = new BiManagerForm(bo))
				{
					var control = GetAnalysisServerControlForTest(form);
					Assert("Support user", control.IsInternalOrSupport);
					var menuItems = GetRightClickMenuOnSsasCubeGrid(control);

					CombineAssertions("Activate, Deactivate, and Reprocess options should all be visible to Support staff", () =>
					{
						Assert("Activate not in menu", menuItems[0].Text == "Activate");
						Assert("Deactivate not in menu", menuItems[1].Text == "Deactivate");
						Assert("Reprocess not in menu", menuItems[2].Text == "Reprocess");
						Assert("Redeploy not in menu", menuItems[3].Text == "Redeploy");
						Assert("Cancel Redeploy not in menu", menuItems[4].Text == "Cancel Redeploy");

						Assert("Activate not visible", menuItems[0].Visible);
						Assert("Deactivate not visible", menuItems[1].Visible);
						Assert("Reprocess not visible", menuItems[2].Visible);
						Assert("Redeploy not visible", menuItems[3].Visible);
						Assert("Cancel Redeploy not visible", menuItems[4].Visible);
					});
				}
			}
			finally
			{
				registration.KeyForTest.IsInternalSystemForTest = null;
			}
		}

		[UseSnapshotProtection]
		public void TestAnalysisServerInformationShowsAdminError()
		{
			var reportCredentials = new BiReportCredential();
			reportCredentials.UserName = "testUser246";
			reportCredentials.Password = "testPa$$word246!";
			reportCredentials.Domain = Environment.MachineName;
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName);
			SystemDataRegistry.Instance.BiReportUserCredential.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportCredentials);

			using (CreateLocalUser(reportCredentials.UserName, reportCredentials.Password))
			using (new BiReportUser().Impersonate())
			using (var connection = Db.NewAdminConnection())
			{
				var bo = new BiMonitorBusinessObject
				{
					AnalysisServerInfo = new AnalysisServerInformation(Db.ServerName, Db.ServerName)
				};
				AssertEquals(bo.AnalysisServerMode, AnalysisServerInformation.adminRightsRequired);
				AssertEquals(bo.AnalysisServerVersion, AnalysisServerInformation.adminRightsRequired);
			}
		}

		public void TestAnalysisServerInformationShowsNonAdminError()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var bo = new BiMonitorBusinessObject
				{
					AnalysisServerInfo = new AnalysisServerInformationForTest(Db.ServerName, Db.ServerName)
				};
				AssertEquals("Model Memory Usage Exception", bo.AnalysisServerMode); // Exception Message
				AssertEquals("Model Memory Usage Exception", bo.AnalysisServerVersion); // Exception Message
			}
		}

#if WINZOR
		[UseSnapshotProtection]
		public void TestRefreshAnalysisServerInformation()
		{
			var bo = new BiMonitorBusinessObject();

			var credential = new BiReportCredential
			{
				Domain = "TestDomain",
				UserName = "TestUserName",
				Password = "P$ssword"
			};

			using (var form = new BiManagerForm(bo))
			using (var control = GetAnalysisServerControlForTest(form))
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Environment.MachineName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, credential))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Environment.MachineName))
			{
				control.RefreshInfo();

				CombineAssertions("Winzor version should not load AnalysisServerInfo.", () =>
				{
					AssertEquals("Analysis Server Name", string.Empty, bo.AnalysisServerInfo.ServerName);
					AssertEquals("Analysis Server Version", string.Empty, bo.AnalysisServerInfo.ServerVersion);
					AssertEquals("Analysis Server Mode", string.Empty, bo.AnalysisServerInfo.ServerMode);
					AssertEquals("Analysis Server TotalEstimatedMemoryUsage", string.Empty, bo.AnalysisServerInfo.TotalEstimatedMemoryUsage);
				});
			}
		}
#endif

		#region Test Helpers

		AnalysisServerControl GetAnalysisServerControlForTest(BiManagerForm form)
		{
			form.SetSsasTabPageVisible();
			form.Show();
			System.Windows.Forms.Application.DoEvents();

			var tabControl = form.FindAll<ZTabControl>().First(c => c.Name == "zTabControl");
			tabControl.SelectTab(1);
			return tabControl.SelectedTab.FindAll<AnalysisServerControl>().First();
		}

		Menu.MenuItemCollection GetRightClickMenuOnSsasCubeGrid(AnalysisServerControl control)
		{
			control.ssasCubesGrid.SelectAllElements();
			control.ssasCubesGrid.PerformMouseDownForTest(0, 1);

			return control.ssasCubesGrid.ContextMenu.MenuItems;
		}

		IDisposable CreateLocalUser(string userName, string password)
		{
			bool userAlreadyExists = false;

			using (var context = new PrincipalContext(ContextType.Machine))
			{
				var user = UserPrincipal.FindByIdentity(context, userName);
				if (user == null)
				{
					user = new UserPrincipal(context);
					user.Name = userName;
					user.Enabled = true;
					user.SetPassword(password);
					user.Save();
				}
				else
				{
					userAlreadyExists = true;
				}
			}

			return userAlreadyExists ? null :
				new DisposableAction(() =>
				{
					using (var context = new PrincipalContext(ContextType.Machine))
					{
						var user = UserPrincipal.FindByIdentity(context, userName);
						if (user != null)
						{
							user.Delete();
						}
					}
				});
		}

		class AnalysisServerInformationForTest : AnalysisServerInformation
		{
			public AnalysisServerInformationForTest(string analysisServer, string dataWarehouseServer) : base(analysisServer, dataWarehouseServer)
			{
			}
			public override void RetrieveModelMemoryUsage(SsasServer server)
			{
				throw new SsasException("Model Memory Usage Exception"); // Exception Message
			}
		}

		#endregion // Test Helpers
	}
}
