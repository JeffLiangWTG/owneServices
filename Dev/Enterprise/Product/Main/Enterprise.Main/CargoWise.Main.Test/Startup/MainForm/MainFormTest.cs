using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using CargoWise.Main.Startup.Tools.JetBrains;
using CargoWise.Types;
using Enterprise.BlazorWinFormsInterop;
using Enterprise.Core.GUI.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Startup.Testing.MainFormTestCase;

namespace Enterprise.Startup.Testing
{
	sealed class MainFormTest : TransactionedTestCase
	{
		[RequiresSTA]
		public void TestSupportLoginMenuItemShouldNotShowWhenOIDCDisabled()
		{
			var oidcConfig = new Mock<IOIDCConfig>();
			oidcConfig.Setup(config => config.IsOIDCEnabled).Returns(false);

			using (ObjectFactory.Substitute(oidcConfig.Object))
			{
				EnvProxy.SetHostedLocationForTest("SYD");

				SystemDataRegistry.Instance.EnableSupportUserLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				using (var form = new TestMainForm())
				{
					form.Show();
					AssertEquals("The menuitem should not show as oidc disabled.", false, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Support Login"));
				}

				SystemDataRegistry.Instance.EnableSupportUserLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (var form = new TestMainForm())
				{
					form.Show();
					AssertEquals("The menuitem should not show as oidc disabled.", false, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Support Login"));
				}
			}
		}

		[RequiresSTA]
		public void TestSupportLoginMenuItemShowWhenOIDCEnabled()
		{
			var oidcConfig = new Mock<IOIDCConfig>();
			oidcConfig.Setup(config => config.IsOIDCEnabled).Returns(true);

			using (ObjectFactory.Substitute(oidcConfig.Object))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				using (var form = new TestMainForm())
				{
					form.Show();
					AssertEquals("The menuitem is shown for hosted system.", true, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Support Login"));
				}

				EnvProxy.SetHostedLocationForTest("NCW");

				SystemDataRegistry.Instance.EnableSupportUserLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				using (var form = new TestMainForm())
				{
					form.Show();
					AssertEquals("The menuitem is not shown for self hosted system if EnableSupportUserLogin is not enabled.", false, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Support Login"));
				}

				SystemDataRegistry.Instance.EnableSupportUserLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (var form = new TestMainForm())
				{
					form.Show();
					AssertEquals("The menuitem is shown for self hosted system if EnableSupportUserLogin is enabled.", true, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Support Login"));
				}
			}
		}

		[RequiresSTA]
		public void TestMainFormWithOpenModuleInProgress()
		{
			try
			{
				using (var form = new TestMainFormWithOpenModuleInProgress())
				{
					form.Show();
					AssertNoExceptionThrown(() => form.OpenModule(new MainFormModule(ModuleIDs.Containers), true));
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[RequiresSTA]
		public void TestCloseHybridModeWhenCloseMainForm()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;

			var factory = new BusinessObjectFactory();
			var userGroup = factory.NewWithValidTestData<GlbGroup>();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.Groups.Add(userGroup);

			var winzorFeatureTest = factory.NewWithValidTestData<StmFeatureTest>();
			winzorFeatureTest.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;
			winzorFeatureTest.SFT_IsActive = true;
			winzorFeatureTest.SFT_GG_Group = userGroup.PK;

			var allWinzorFeatureTest = factory.NewWithValidTestData<StmFeatureTest>();
			allWinzorFeatureTest.SFT_FeatureName = StmFeatureTest.WinzorAllFeaturesCode;
			allWinzorFeatureTest.SFT_IsActive = true;
			allWinzorFeatureTest.SFT_GG_Group = userGroup.PK;

			factory.Save();
			var listener = new Mock<IWinFormsListener>();
			ObjectFactory.Substitute(listener.Object);
			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var form = new TestMainForm())
			{
				form.Show();
				form.Close();
				Assert("The main form should be closed", form.IsDisposed);
			}
#if !WINZOR
			listener.Verify(o => o.ExitHybridMode(), Times.Once);
#else
			listener.Verify(o => o.ExitHybridMode(), Times.Never);
#endif
		}

		[RequiresSTA]
		public void TestFindWithOptionRecompile()
		{
			// if the query does not have like
			SearchFromFindScreen("E");
			Assert("If a query does not have like, it should not have OPTION (RECOMPILE)", !SqlEventTracker.Instance.LastSqlQuery.Contains("OPTION (RECOMPILE)"));

			// if the query does not come from find screen
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Contains, "CO");
				var factory = new BusinessObjectFactory();
				factory.Load<DummyBusinessObject>(query);

				Assert("LIKE parameter should be parameterised", SqlEventTracker.Instance.LastSqlEvent.Contains("like @"));
				Assert("The query should not have OPTION (RECOMPILE)", !SqlEventTracker.Instance.LastSqlEvent.Contains("OPTION (RECOMPILE)"));
			}

			// if the query comes from find screen, and ApplyOptionRecompile is false
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ApplyOptionRecompile = false;

				SearchFromFindScreen("S");

				Assert("LIKE parameter should be parameterised", SqlEventTracker.Instance.ThirdToLastSqlQuery.Contains("like @"));
				Assert("The query should not have OPTION (RECOMPILE)", !SqlEventTracker.Instance.ThirdToLastSqlQuery.Contains("OPTION (RECOMPILE)"));
			}

			// if the query comes from find screen, and ApplyOptionRecompile is true
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ApplyOptionRecompile = true;

				SearchFromFindScreen("S");

				Assert("The query should have OPTION (RECOMPILE)", SqlEventTracker.Instance.ThirdToLastSqlQuery.Contains("OPTION (RECOMPILE)"));
			}
		}

		void SearchFromFindScreen(string comparisonOperator)
		{
			using (var form = new TestMainFormWithHotkeyOverride())
			{
				form.Show();
				var containersMainFormModule = new MainFormModule(ModuleIDs.Containers);
				form.OpenModule(containersMainFormModule, true);

				ZFormTest.SetFilterStrip(form, 0, "se\t", comparisonOperator, "a");

				var filterControls = (ZFilterStripControl)(((ZEmbeddedModule)(form.PreviousEmbeddedModule)).EmbeddedControl);
				filterControls.Find();
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestShowLoginUserControl_NoExceptionThrown()
		{
			using (var form = new TestMainForm())
			{
				form.ShowLoginUserControl();
				form.ShowLoginLocationControl();
				form.Show();
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestShowNextLoginUserControl_NoExceptionThrown()
		{
			using (var form = new TestMainForm(isCWNext: true))
			{
				form.ShowLoginUserControl();
				form.ShowLoginLocationControl();
				form.Show();
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[RequiresSTA]
		public void TestSwitchUser_ClearsGlowUserData()
		{
			// Arrange
			var glowServiceClientFactoryMock = new Mock<ZArchitecture.GlowInterop.IGlowServiceClientFactory>(MockBehavior.Strict) { CallBase = true };

			using (ObjectFactory.Substitute(glowServiceClientFactoryMock.Object))
			using (var form = new TestMainFormOnSwitchUser())
			{
				glowServiceClientFactoryMock.Setup(factory => factory.ClearUserData());
				// Act
				form.SwitchUser();

				// Assert
				glowServiceClientFactoryMock.VerifyAll();
				glowServiceClientFactoryMock.Verify(factory => factory.ClearUserData(), Times.Exactly(1));
			}
		}

		#region Global Search
		[RequiresSTA]
		public void TestGlobalSearchInitialised()
		{
			using (var form = new MainForm())
			using (var nav = new TileNavigationBar())
			{
				AssertNull("globalSearch should be null until initialisation", form.globalSearch);
				AssertNotNull("GlobalSearch should not be null", form.GlobalSearch);
				AssertNotNull("globalSearch has been initialised and should not be null", form.globalSearch);
			}
		}

		[RequiresSTA]
		public void TestGlobalSearchQueryThrowsWithNoNavBar()
		{
			using (var form = new MainForm())
			using (var nav = new TileNavigationBar())
			{
				AssertExceptionThrown(typeof(InvalidOperationException), () => form.ModuleOpener(null));
				form.NavigationBar = nav;
				// throws a different exception, meaning the nav bar was set correctly and query 'ran'
				AssertExceptionThrown(typeof(NullReferenceException), () => form.ModuleOpener(null));
			}
		}

		#endregion

		public void TestAppTitleBarTextSizeIsCorrect()
		{
			using (var form = new MainForm())
			{
#if WINZOR
				AssertEquals(10.15F, form.AppTitleText.Font.Size);
#else
				AssertEquals(10F, form.AppTitleText.Font.Size);
#endif
			}
		}

		[RequiresSTA]
		public void TestTimelineProfilingMenuItem()
		{
			bool etwServiceIsRunning = ProfilePerformanceModel.IsETWServiceRunning();
			using (var form = new TestMainForm())
			{
				form.Show();
				ZToolStripMenuItem diagnosticsMenu = (ZToolStripMenuItem)form.HelpMenuButton.DropDownItems["Diagnostics"];
				AssertNotNull("Diagnostics menu not found", diagnosticsMenu);
				ZToolStripMenuItem profilePerformanceDotTraceMenuItem = (ZToolStripMenuItem)diagnosticsMenu.DropDownItems["Profile Performance (dotTrace)"];
				AssertNotNull("Profile Performance (dotTrace) menu item not found", profilePerformanceDotTraceMenuItem);
				if (etwServiceIsRunning)
				{
					AssertNotNull("Timeline menu item not found when JetBrains ETW service running", profilePerformanceDotTraceMenuItem.DropDownItems["Timeline"]);
				}
				else
				{
					AssertNull("Timeline menu item found when JetBrains ETW service not running", profilePerformanceDotTraceMenuItem.DropDownItems["Timeline"]);
				}
			}
		}

		[RequiresSTA]
		public void TestSamplingProfilingMenuItem()
		{
			using (var form = new TestMainForm())
			{
				form.Show();
				ZToolStripMenuItem diagnosticsMenu = (ZToolStripMenuItem)form.HelpMenuButton.DropDownItems["Diagnostics"];
				AssertNotNull("Diagnostics menu not found", diagnosticsMenu);
				ZToolStripMenuItem profilePerformanceDotTraceMenuItem = (ZToolStripMenuItem)diagnosticsMenu.DropDownItems["Profile Performance (dotTrace)"];
				AssertNotNull("Profile Performance (dotTrace) menu item not found", profilePerformanceDotTraceMenuItem);
				AssertNotNull("Sampling menu item not found", profilePerformanceDotTraceMenuItem.DropDownItems["Sampling"]);
			}
		}

		[RequiresSTA]
		public void TestDoInitializeAfterLogin_PurgeAllRegistry()
		{
			var item = new StringRegistryItem("DUMMY_ITEM", null, null, null, RegistryStorageFlags.Company);
			RegistryItemDictionary.Instance.Add(item);

			AssertNotNull("'RegistryItemDictionary' should contain this registry item.", RegistryItemDictionary.Instance.GetItem(item.Name));

			using (var form = new TestMainForm())
			{
				AssertNull("After running 'DoInitializeAfterLogin()', the registry item in the 'RegistryItemDictionary' should be purged.", RegistryItemDictionary.Instance.GetItem(item.Name));
			}
		}

		[RequiresSTA]
		public void TestFastUserSwitchShouldNotThrowException()
		{
			var activeOperationalStaffQuery = new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True);
			activeOperationalStaffQuery.AddToFilter(GlbStaffSchema.GS_IsOperational, ZBool.True);
			var staffList = new GlbStaffCollection(new BusinessObjectFactory() { RefreshEnabled = false }, activeOperationalStaffQuery).ToList();
			AssertNotEquals(staffList.Count, 0);
			var firstStaff = staffList.First();
			using var testCargoWiseOneForm = new TestMainForm(false);

			var toolStripItem = new ZToolStripMenuItem("Switch User");
			toolStripItem.Tag = (string)firstStaff.GS_LoginName;
			AssertNoExceptionThrown(() => testCargoWiseOneForm.OnSwitchUser(toolStripItem, null));

			using var testCargoWiseNextForm = new TestMainForm(true);
			AssertNoExceptionThrown(() => testCargoWiseNextForm.OnSwitchUser(toolStripItem, null));
		}
	}
}
