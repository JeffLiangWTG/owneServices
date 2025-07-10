#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using CargoWise.Main.Startup.Login;
using CargoWise.Main.Startup.MainForm;
using CargoWise.Main.Startup.SqlSecurity;
using CargoWise.Main.Startup.Tools;
using CargoWise.Main.Startup.Tools.JetBrains;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Core.Modules;
using Enterprise.DataPurge;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Diagnostics;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.BarcodeParsing;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterData.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.SqlSecurity;
using Enterprise.Startup.Tools;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.OpenIDConnect.Token;
using Res = CargoWise.Main.Res;
using ResString = CargoWise.Main.ResString;

#if WINZOR
using Microsoft.JSInterop;
using WinzorFramework.RemoteClientServices;
using WTG.OpenIDConnect.Login;
#endif
#endregion Usings

namespace Enterprise.Startup
{
	public partial class MainForm
	{
		#region Menu Items

		#region File

		void AddAdditionalFileMenuItemsForAppropriateUsers()
		{
			if (GlbStaff.CurrentUser.GS_IsDeveloper || GlbStaff.CurrentUser.GS_IsController || !GlbStaff.CurrentUser.GS_IsOperational)
			{
				var purgeItem = new ZToolStripMenuItem(ResString.GetMultilingualString("01904726-FB54-48C0-BCBB-C3476D69A280", "Purge Data (Admin Users)"), (s, e) => ZFormModaliser.ShowDialogAndDispose(new DataPurgeUserForm()));
				SettingsMenuButton.DropDownItems.Insert(0, purgeItem);
				TypeDescriptor.AddAttributes(SettingsMenuButton.DropDownItems[0], new SuppressFormsLocalizedTestAttribute());
				SettingsMenuButton.DropDownItems.Insert(1, new ToolStripSeparator());
			}
		}

#if DEBUG

		void CreateFastUserSwitchingMenu()
		{
			var fastUserSwitchingMenuItem = new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.FastUserSwitching", "Fast User Switching (Debug Only)"));
			TypeDescriptor.AddAttributes(fastUserSwitchingMenuItem, new SuppressFormsLocalizedTestAttribute());
			fastUserSwitchingMenuItem.DropDownOpening += FastUserSwitchingMenuItem_DropDownOpening;
			SettingsMenuButton.DropDownItems.Insert(4, fastUserSwitchingMenuItem);
		}

		void FastUserSwitchingMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			var fastUserSwitchingMenuItem = (ZToolStripMenuItem)sender;
			if (fastUserSwitchingMenuItem.DropDownItems.Count == 0)
			{
				AddAllUsers(fastUserSwitchingMenuItem);
			}
		}

		void AddAllUsers(ZToolStripMenuItem fastUserSwitchingMenuItem)
		{
			var activeOperationalStaffQuery = new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True);
			activeOperationalStaffQuery.AddToFilter(GlbStaffSchema.GS_IsOperational, ZBool.True);
			var staffList = new GlbStaffCollection(new BusinessObjectFactory() { RefreshEnabled = false }, activeOperationalStaffQuery)
				.OrderBy(GetWesternName, StringComparer.OrdinalIgnoreCase)
				.ToArray();

			AddUsers(staffList, fastUserSwitchingMenuItem, "");
		}

		static string GetWesternName(GlbStaff staff)
		{
			return staff.GS_LoginName.IsWesternEuropeanOrEmpty ? (string)staff.GS_LoginName.Trim() : (string)staff.GS_Code + " (" + staff.GS_LoginName.Trim() + ')';
		}

		void AddUsers(IEnumerable<GlbStaff> staffList, ZToolStripMenuItem parentItem, string prefix)
		{
			var n = staffList.Count();
			const int MaxUsersPerDropDown = 50;
			if (n > MaxUsersPerDropDown)
			{
				string lastPrefix = null;
				var subList = new List<GlbStaff>();
				foreach (var staff in staffList)
				{
					var name = GetWesternName(staff);
					var currentPrefix = name.Substring(0, Math.Min(name.Length, prefix.Length + 1));
					if (currentPrefix.Length == prefix.Length)
					{
						AddUser(staff, name, parentItem);
					}
					else
					{
						if (lastPrefix == null || !string.Equals(lastPrefix, currentPrefix, StringComparison.OrdinalIgnoreCase))
						{
							AddUserSubMenu(subList, parentItem, lastPrefix);
							subList.Clear();
							lastPrefix = currentPrefix;
						}
						subList.Add(staff);
					}
				}
				AddUserSubMenu(subList, parentItem, lastPrefix);
			}
			else if (n > 0)
			{
				foreach (var staff in staffList)
				{
					AddUser(staff, GetWesternName(staff), parentItem);
				}
			}
		}

		void AddUserSubMenu(IEnumerable<GlbStaff> staffList, ZToolStripMenuItem parentItem, string prefix)
		{
			var n = staffList.Count();
			if (n == 1)
			{
				AddUsers(staffList, parentItem, prefix);
			}
			else if (n > 0)
			{
				var item = new ZToolStripMenuItem(prefix.ToUpperInvariant());
				TypeDescriptor.AddAttributes(item, new SuppressFormsLocalizedTestAttribute());
				parentItem.DropDownItems.Add(item);
				AddUsers(staffList, item, prefix);
			}
		}

		void AddUser(GlbStaff staff, string name, ZToolStripMenuItem parentItem)
		{
			var staffItem = new ZToolStripMenuItem(name, OnSwitchUser);
			staffItem.Tag = (string)staff.GS_LoginName;
			parentItem.DropDownItems.Add(staffItem);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Debug")]
		internal void OnSwitchUser(object sender, EventArgs args)
		{
			var item = (ToolStripItem)sender;
			var loginName = (string)(item.Tag);
			Env.SetUserContext(new UserContext(loginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
#if !WINZOR
			var nextNavigationBar = NavigationBar as NextNavigation;
			if (nextNavigationBar is null)
			{
				return;
			}
			nextNavigationBar.HomeControl.UserName = GlbStaff.CurrentUser.GS_FullName;
			nextNavigationBar.HomeControl.Branch = GlbBranch.CurrentBranch.GB_BranchName;
			nextNavigationBar.HomeControl.Company = GlbCompany.CurrentCompany.GC_Name;
			nextNavigationBar.HomeControl.Department = GlbDepartment.CurrentDepartment.GE_Desc;
#endif
		}

#endif

		internal void ExitMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.CurrentCompany is not null)
			{
				WindowPersister.ClearUrlsFromRegistry();
			}
			Close();
		}

		void ExitAndRememberMenuItem_Click(object sender, EventArgs e)
		{
			WindowPersister.SaveUrlsToRegistry();
			Close();
		}

		void FileChangePasswordMenuItem_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "Change Password" };
			var staff = factory.Load<GlbStaff>(Env.CurrentUser.PK);
			factory.ReloadAllSafe<GlbStaff>();

			var shouldSave = PasswordDialogs.Instance.ShowChangePasswordDialog(staff);
			if (shouldSave)
			{
				factory.Save();
			}
		}

		#endregion File

		#region Diagnostics

		void DiagnosticsEmailMenuItem_Click(object sender, EventArgs e)
		{
			new EmailDiagnosticsForm().Show();
		}

		void DiagnosticsEHubMenuItem_Click(object sender, EventArgs e)
		{
			new eHubDiagnosticsForm().Show();
		}

		void DiagnosticsDxTMenuItem_Click(object sender, EventArgs e)
		{
			new DirectxTDiagnosticsForm().Show();
		}

		void DirectxTConnectionTestMenuItem_Click(object sender, EventArgs e)
		{
			var errMsg = string.Empty;
			var result = new DirectxTDiagnostics().CheckConnection(out errMsg);
			string message = result ? Res.GetString("e08bb500-b5f7-42d1-9ddd-977556fad2b9", "The connection is working.") : Res.GetString("5eaf7d80-edd0-47a0-9b87-4bc6001a31d2", "The connection is not working");

			var caption = Res.GetString("CF6FF8A4-BE39-49DD-812A-7244AADEAC16", "Direct xT server connection results");
			var icon = result ? MessageBoxIcon.Information : MessageBoxIcon.Error;
			Globals.Message.Show(errMsg, caption, MessageBoxButtons.OK, icon, DialogResult.OK);
		}

		void DiagnosticsHAWBMenuItem_Click(object sender, EventArgs e)
		{
			new TestDocumentProducer().RunHAWBTestDocument();
		}

		void DiagnosticsCoverSheetMenuItem_Click(object sender, EventArgs e)
		{
			new TestDocumentProducer().RunCoverSheetDocument();
		}

		[SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizersOrGetTotalMemory", Justification = "They are Diagnostic Code Only")]
		void CompactHeapMenuItem_Click(object sender, EventArgs e)
		{
			var pi = typeof(GCSettings).GetProperty("LargeObjectHeapCompactionMode", BindingFlags.Static | BindingFlags.Public);
			if (pi != null)
			{
				pi.SetValue(null, 2, Array.Empty<object>());
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Globals.Message.Show(Res.GetString("0aa9d456-6c77-4e93-80b2-66ebd04de45b", "Memory has been compacted and garbage collected."));
			}
			else
			{
				Globals.Message.Show(Res.GetString("326a6679-d4e2-4e8b-a45c-8db0d3ad9b85", "Compaction is only possible when running .Net 4.5.1 or greater"));
			}
		}

		void DiagnosticsPrintTestMenuItem_Click(object sender, EventArgs e)
		{
			new TestDocumentProducer().RunPrintTestDocument();
		}

		void PerformanceMenuItem_Click(object sender, EventArgs e)
		{
			new ZPerformanceForm(new PerformanceStatisticWithForms()).Show();
		}

		void DiagnosticsProfilePerformanceSamplingMenuItem_Click(object sender, EventArgs e)
		{
			DiagnosticsProfilePerformanceMenuItem_Click(sender, e, ProfilingType.SAMPLING);
		}

		void DiagnosticsProfilePerformanceTimelineMenuItem_Click(object sender, EventArgs e)
		{
			DiagnosticsProfilePerformanceMenuItem_Click(sender, e, ProfilingType.TIMELINE);
		}

		void DiagnosticsProfilePerformanceMenuItem_Click(object sender, EventArgs e, ProfilingType profilingType)
		{
			if (!CanRunProfiler())
			{
				return;
			}

			var dialogService = new DialogService(this);
			var profilePerformanceModel = new ProfilePerformanceModel(dialogService, profilingType);
			profilePerformanceModel.Init();
		}

		void DiagnosticsProfileMemoryMenuItem_Click(object sender, EventArgs e)
		{
			if (!CanRunProfiler())
			{
				return;
			}

			var dialogService = new DialogService(this);
			var profileMemoryModel = new ProfileMemoryModel(dialogService);
			profileMemoryModel.Init();
		}

		bool CanRunProfiler()
		{
			if (ProfileModel.IsSessionStarted)
			{
				Globals.Message.ShowInformation(
					Res.GetString("58693970-e0f8-49f4-96ff-9e3fb8077df1", "Another profiling sessions is running."),
					ProfileModel.ProfilerDialogTitle);

				return false;
			}

			return true;
		}

		void AddressValidationMenuItem_Click(object sender, EventArgs e)
		{
			if (addressValidationMessageMonitorForm != null)
			{
				addressValidationMessageMonitorForm.Invoke(new Action(delegate
				{ addressValidationMessageMonitorForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						addressValidationMessageMonitorForm = new AddressValidationMessageMonitorForm();
						addressValidationMessageMonitorForm.ShowDialog();
						addressValidationMessageMonitorForm = null;
					}
				}, threadName: nameof(AddressValidationMessageMonitorForm));
			}
		}

		void TraceMonitorMenuItem_Click(object sender, EventArgs e)
		{
			if (traceForm != null)
			{
				traceForm.Invoke(new Action(delegate
				{ traceForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						traceForm = new TraceMonitorForm(new TraceMonitor());
						traceForm.ShowDialog();
						traceForm = null;
					}
				}, threadName: nameof(TraceMonitorForm));
			}
		}

		void DeliberatelyLeakForms_Click(object sender, EventArgs e)
		{
			var cache = OpenedFormCache.GetInstance();
			cache.deliberatelyLeakedForms = new System.Collections.Concurrent.ConcurrentBag<Form>();
			if (cache.IsDeliberatelyLeakingForms)
			{
				cache.IsDeliberatelyLeakingForms = false;
				Globals.Message.Show(Res.GetString("8c573116-bcd3-40c3-870b-aa7bd5a04c5e", "No longer deliberately leaking forms."));
			}
			else
			{
				cache.IsDeliberatelyLeakingForms = true;
				Globals.Message.Show(Res.GetString("174dae8b-232b-4d62-aec3-075ca8934a43", "Now deliberately leaking forms."));
			}
		}

		void TestHardwareTokenSignature_Click(object sender, EventArgs e)
		{
			Accounting.Business.EInvoicing.HardwareTokenSigning.ElectronicInvoicingDiagnostics.HandleTestHardwareTokenSignature(sender, e);
		}

		void HotKeyMonitorMenuItem_Click(object sender, EventArgs e)
		{
			if (hotKeyMonitorForm != null)
			{
				hotKeyMonitorForm.Invoke(new Action(delegate
				{ hotKeyMonitorForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						hotKeyMonitorForm = new HotKeyMonitorForm();
						hotKeyMonitorForm.ShowDialog();
						HotKeyMonitorProvider.GetHotKeyMonitor().ClearAll();
						hotKeyMonitorForm = null;
					}
				},
				threadName: nameof(HotKeyMonitorForm));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static HotKeyMonitorForm hotKeyMonitorForm;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static TraceMonitorForm traceForm;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static AddressValidationMessageMonitorForm addressValidationMessageMonitorForm;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static DeduplicationMonitoringForm deDuplicationResultMonitorForm;

		void DeduplicationMenuItem_Click(object sender, EventArgs e)
		{
			if (deDuplicationResultMonitorForm != null)
			{
				deDuplicationResultMonitorForm.Invoke(new Action(delegate
				{ deDuplicationResultMonitorForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						deDuplicationResultMonitorForm = new DeduplicationMonitoringForm();
						deDuplicationResultMonitorForm.ShowDialog();
						deDuplicationResultMonitorForm = null;
					}
				}, threadName: nameof(DeduplicationMonitoringForm));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static UXMLMatchingDiagnosticToolForm uxmlMatchingDiagnosticToolForm;

		void UXMLMatchingDiagnosticMenuItem_Click(object sender, EventArgs e)
		{
			if (uxmlMatchingDiagnosticToolForm != null)
			{
				uxmlMatchingDiagnosticToolForm.Invoke(new Action(delegate
				{ uxmlMatchingDiagnosticToolForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						uxmlMatchingDiagnosticToolForm = new UXMLMatchingDiagnosticToolForm();
						uxmlMatchingDiagnosticToolForm.ShowDialog();
						uxmlMatchingDiagnosticToolForm = null;
					}
				}, threadName: nameof(UXMLMatchingDiagnosticToolForm));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static DeniedPartyScreeningMonitoringForm deniedPartyScreeningMonitoringForm;

		void DeniedPartyScreeningMonitorMenuItem_Click(object sender, EventArgs e)
		{
			if (deniedPartyScreeningMonitoringForm != null)
			{
				deniedPartyScreeningMonitoringForm.Invoke(new Action(delegate
				{ deniedPartyScreeningMonitoringForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						deniedPartyScreeningMonitoringForm = new DeniedPartyScreeningMonitoringForm();
						deniedPartyScreeningMonitoringForm.ShowDialog();
						deniedPartyScreeningMonitoringForm = null;
					}
				}, threadName: nameof(DeniedPartyScreeningMonitoringForm));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static SecurityCheckpointMonitoringForm securityCheckpointMonitoringForm;

		void SecurityCheckpointMonitorMenuItem_Click(object sender, EventArgs e)
		{
			if (securityCheckpointMonitoringForm != null)
			{
				securityCheckpointMonitoringForm.Invoke(new Action(delegate
				{ securityCheckpointMonitoringForm.Activate(); }));
			}
			else
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						securityCheckpointMonitoringForm = new SecurityCheckpointMonitoringForm();
						securityCheckpointMonitoringForm.ShowDialog();
						securityCheckpointMonitoringForm = null;
					}
				}, threadName: nameof(SecurityCheckpointMonitoringForm));
			}
		}

#if !WINZOR
		void NetworkMonitorMenuItem_Click(object sender, EventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				using (var networkMonitorForm = new NetworkMonitorForm())
				{
					networkMonitorForm.ShowDialog(this);
				}
			}
		}

		void DragDropTrackingInfoMenuItem_Click(object sender, EventArgs e)
		{
			EnterpriseChannel.Instance?.ShowDragDropTrackingInfoForm();
		}
#endif

		void ThreadMonitorMenuItem_Click(object sender, EventArgs e)
		{
			if (threadMonitorForm != null)
			{
				threadMonitorForm.Invoke(new Action(delegate
				{
					threadMonitorForm.RestoreLastWindowStateFromMinimized();
					threadMonitorForm.Activate();
				}));
				return;
			}

			threadMonitorForm = new ThreadMonitorForm(new ThreadMonitor(Thread.CurrentThread));

			try
			{
				DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						threadMonitorForm.ShowDialog();
						threadMonitorForm = null;
					}
				},
				threadName: nameof(ThreadMonitor));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static ThreadMonitorForm threadMonitorForm;

		void DiagnosticsScanningMenuItem_Click(object sender, EventArgs e)
		{
			var form = new ScanningDiagnosticsForm();
			form.Show();
		}

		void DiagnosticsBarcodeParsingMenuItem_Click(object sender, EventArgs e)
		{
			((ZForm)ObjectFactory.New<IBarcodeParsingDiagnosticsForm>()).Show();
		}

		void CartonisationDiagnosticMenuItem_Click(object sender, EventArgs e)
		{
			((ZForm)ObjectFactory.New<ICartonisationDiagnosticForm>()).Show();
		}

		void GetAndVerifyB2CAccessTokenMenuItem_Click(object sender, EventArgs e)
		{
			var systemToSystemTrustService = new SystemToSystemTrustService();
			var token = string.Empty;
			var verifySucceeded = false;
			Exception exception = null;

			try
			{
				token = systemToSystemTrustService.GetAccessToken();

				var systemInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
				var tenantId = systemInfo.TenantId;
				var authorityUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/";
				var audience = SystemDataRegistry.Instance.EDIClientID.Value;

				var result = TokenValidator.ValidateAccessToken(authorityUrl, audience, token, ConfigurationHelper.ConfigurationManagerCache, null, CancellationToken.None)
					.ConfigureAwait(false).GetAwaiter().GetResult();
				verifySucceeded = result != null;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				exception = ex;
			}
			finally
			{
				var verifyResult = verifySucceeded ? (NoResString)"Succeeded" : $"Failed. {exception?.Message}";
				var message = $@"B2C Access Token:
{token}

Verify Result: {verifyResult}";

				Globals.Message.ShowInformation(message);
			}
		}

		void CaptureBusinessObjectStackTraceMenuItem_Click(object sender, EventArgs e)
		{
			var item = (ZToolStripMenuItem)sender;
			var enable = !BusinessObjectCreationStackTraceRecorder.IsEnabled;
			BusinessObjectCreationStackTraceRecorder.SetEnable(enable);
			item.Checked = enable;
		}

		void ShowQueryStackTraceMenuItem_Click(object sender, EventArgs e)
		{
			var item = (ZToolStripMenuItem)sender;
			QueryStackTraceRecorder.Instance.Enabled = !QueryStackTraceRecorder.Instance.Enabled;
			item.Checked = QueryStackTraceRecorder.Instance.Enabled;
		}

		void ShowLoadedFetchHintsMenuItem_Click(object sender, EventArgs e)
		{
			var item = (ZToolStripMenuItem)sender;
			RowFactory.LoadedFetchHintRecordingEnabled = !RowFactory.LoadedFetchHintRecordingEnabled;
			item.Checked = RowFactory.LoadedFetchHintRecordingEnabled;
		}

		void ShowMemoryUsageMenuItem_Click(object sender, EventArgs e)
		{
			Globals.Message.Show(Enterprise.ZArchitecture.Core.TopLevelExceptionHandler.ResourcesMessage());
		}

		#endregion Diagnostics

		#region Help

		void SetupHelpMenu()
		{
			HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.CargoWiseWebName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowUserPortal()));
			var wtaDropDownMenuItem = new ZToolStripMenuItem(ZHelpMenu.WiseTechAcademyName);
			wtaDropDownMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.WiseTechAcademyMyLearningName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowWiseTechAcademy()));
			wtaDropDownMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.WiseTechAcademyContentAndSupportName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowWiseTechAcademy(ZHelpMenu.WiseTechAcademyContentAndSupportPath, ZHelpMenu.WiseTechAcademyContentAndSupportTarget)));
			wtaDropDownMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.ReleaseNotesName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowWiseTechAcademy(ZHelpMenu.WiseTechAcademyContentAndSupportPath, ZHelpMenu.WiseTechAcademyUpdateNotesTarget)));
			HelpMenuButton.DropDownItems.Add(wtaDropDownMenuItem);
			HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.BorderWiseLogin, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowBorderWiseWebApp()));
			SetupCargoWiseWebPortalsMenuItemIfAvailable();

			HelpMenuButton.DropDownItems.Add(new ToolStripSeparator());

			ZTrainingModeMenuItem.AddTrainingModeMenuItem(HelpMenuButton);

			ServiceRequestMenuItem.AddServiceRequestMenuItem(HelpMenuButton, this);
			HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.eRequestPortalName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowERequestPortal()));

			Enterprise.ProductRegistration.GUI.RegisterProductMenuItem.AddRegisterProductMenuItem(HelpMenuButton, SetCaption);

			HelpMenuButton.DropDownItems.Add(new ToolStripSeparator());

			if (GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsDeveloper)
			{
				var stlBillingItem = new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.StlBilling", "STL Billing"));
				var stlBillingSubItems = new ToolStripItem[]
				{
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.CollectSTLRetrospectiveData", "Collect Retrospective Data"), CollectSTLRetrospectiveDataMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.StlCollectors", "Dynamic Collector Definitions"), StlCollectorsMenuItem_Click),
				};

				stlBillingItem.DropDownItems.AddRange(stlBillingSubItems);
				HelpMenuButton.DropDownItems.Add(stlBillingItem);
				HelpMenuButton.DropDownItems.Add(new ToolStripSeparator());
			}

			if (GlbStaff.CurrentUser.HasDatabaseAccess && (EnvProxy.IsHostedWithCargowise || !GlbStaff.CurrentUser.IsADIntegrationEnabled))
			{
				HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.SetSQLPassword", "Set SQL password"), SetSQLPasswordMenuItem_Click));
				HelpMenuButton.DropDownItems.Add(new ToolStripSeparator());
			}

			SetupDatabaseMenuItemsForAppropriateUsers();

			HelpMenuButton.DropDownItems.Add(GetHelpDiagnosticsMenuItem());

#if !WINZOR
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			if (Globals.IsDebugMode || productRegistration.IsWiseTechGlobalInternalUATSystem() || productRegistration.IsWiseTechGlobalInternalDeveloperSystem() || productRegistration.IsWiseTechGlobalInternalTrainingSystem())
			{
				HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.DeveloperFeatureControlOverride, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowDeveloperFeatureControlOverride()));
			}
#endif
			HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.HotKeyHelpName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowHotKeyHelp(this)));

			SetupSecurityOverrideTokenMenuItemIfAvailable();

			if (IsCWNext)
			{
				HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.AboutName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowAboutForCWNext()));
			}
			else
			{
				HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.AboutName, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowAbout()));
			}
			var newHelpMenuItems = (ArrayList)ObjectFactory.Get("ClientSpecificHelpMenuItems");
			if (newHelpMenuItems.Count == 0)
			{
				return;
			}

			foreach (ToolStripMenuItem item in newHelpMenuItems)
			{
				HelpMenuButton.DropDownItems.Add(new ToolStripSeparator());
				HelpMenuButton.DropDownItems.Add(item);
			}
		}

		void SetupSecurityOverrideTokenMenuItemIfAvailable()
		{
			if (SystemDataRegistry.Instance.EnableSecurityOverrideToken.Value && ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled)
			{
				HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.SecurityOverrideToken", "Security Override Token"), (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowSecurityOverrideToken()));
			}
		}

		void SetupCargoWiseWebPortalsMenuItemIfAvailable()
		{
			if (ObjectFactory.Get<IHelpMenuProvider>().IsCargoWiseWebPortalsConfigured())
			{
				HelpMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ZHelpMenu.CargoWiseWebPortals, (sender, args) => ObjectFactory.Get<IHelpMenuProvider>().ShowCargoWiseWebPortals()));
			}
		}

		ZToolStripMenuItem GetHelpDiagnosticsMenuItem()
		{
			var menuItem = new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Diagnostics", "Diagnostics"));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.CaptureBusinessObjectStackTrace", "Capture Business Object Stack Trace"), CaptureBusinessObjectStackTraceMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsEmail", "Send/receive test email..."), DiagnosticsEmailMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsEHub", "Send/receive test message via eHub..."), DiagnosticsEHubMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsDxT", "Send/receive test message via Direct xT..."), DiagnosticsDxTMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsxT", "Test connection with xT server..."), DirectxTConnectionTestMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsHAWB", "Print test Air Waybill..."), DiagnosticsHAWBMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsCoverSheet", "Print test Shipment Cover Sheet..."), DiagnosticsCoverSheetMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsPrintTest", "Print test printer scaling page..."), DiagnosticsPrintTestMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("Compact Heap", "Compact Heap and Garbage Collect"), CompactHeapMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsBarcodeParsingRules", "Run Barcode Parsing Rules"), DiagnosticsBarcodeParsingMenuItem_Click));
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.CartonisationDiagnostic", "Cartonization Algorithm Diagnostics"), CartonisationDiagnosticMenuItem_Click));
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.GetAndVerifyB2CAccessToken", "Get && Verify B2C Access Token"), GetAndVerifyB2CAccessTokenMenuItem_Click));
			}
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsScanning", "Run Wedge Scanner Diagnostics"), DiagnosticsScanningMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Performance", "Performance"), PerformanceMenuItem_Click));
			if (!EnvProxy.IsHostedWithCargowise || GlbStaff.CurrentUser.IsSupportUser || SystemDataRegistry.Instance.AllowNonSupportDiagnosticsProfiling.Value)
			{
				menuItem.DropDownItems.Add(GetProfilePerformanceDotTraceMenuItem());
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DiagnosticsProfileMemory", "Profile Memory (dotMemory)"), DiagnosticsProfileMemoryMenuItem_Click));
			}
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ThreadMonitor", "Thread Monitor (Hang Debugging)"), ThreadMonitorMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ShowQueryStackTrace", "Show Query Stack Trace"), ShowQueryStackTraceMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ShowLoadedFetchHints", "Show Loaded Fetch Hints"), ShowLoadedFetchHintsMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ShowMemoryUsage", "Show Memory Usage"), ShowMemoryUsageMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DatabaseInfoMenuItem", "Database and S3 Storage Info"), DatabaseInfoMenuItem_Click));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DocumentSectionsMenuItem", "Document Sections"), DocumentSectionsMenuItem_Click));

#if !WINZOR

			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession && InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.CheckNetworkRoundLoopDelay))
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.NetworkMonitor", "Network Monitor"), NetworkMonitorMenuItem_Click));
			}

			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DragDropTrackingInfo", "Drag and Drop Tracking Info"), DragDropTrackingInfoMenuItem_Click));
			}

#endif

			if (Env.Instance.Registry.EnableAddressValidationWebService)
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.AddressValidationMessageMonitor", "Address Validation Message Monitor"), AddressValidationMenuItem_Click));
			}

			if (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value)
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DeduplicationMonitor", "De-duplication Result Monitor"), DeduplicationMenuItem_Click));
			}

			if (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.Value)
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.UXMLMatchingDiagnosticTool", "UXML Matching Diagnostic Tool"), UXMLMatchingDiagnosticMenuItem_Click));
			}

			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DeniedPartyScreeningMonitor", "Denied Party Screening Monitor"), DeniedPartyScreeningMonitorMenuItem_Click));

			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Security Checkpoint Monitor", "Security Checkpoint Monitor"), SecurityCheckpointMonitorMenuItem_Click));

			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.TraceMonitor", "Trace Monitor"), TraceMonitorMenuItem_Click));

			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("3eba8fd8-6ada-4b31-976a-b622f9261b22", "Test Hardware Token Signature"), TestHardwareTokenSignature_Click));

			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.DeliberatelyLeakForms", "Deliberately Leak Forms"), DeliberatelyLeakForms_Click));
			}

			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.HotKeyMonitor", "Hot Key Monitor"), HotKeyMonitorMenuItem_Click));

			return menuItem;
		}

		ZToolStripMenuItem GetProfilePerformanceDotTraceMenuItem()
		{
			var menuItem = new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Diagnostics.ProfilePerformanceDotTrace", "Profile Performance (dotTrace)"));
			menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Diagnostics.ProfilePerformanceDotTrace.Sampling", "Sampling"), DiagnosticsProfilePerformanceSamplingMenuItem_Click));
			if (ProfilePerformanceModel.IsETWServiceRunning())
			{
				menuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Diagnostics.ProfilePerformanceDotTrace.Timeline", "Timeline"), DiagnosticsProfilePerformanceTimelineMenuItem_Click));
			}
			return menuItem;
		}

		void SetupDatabaseMenuItemsForAppropriateUsers()
		{
			if (GlbStaff.CurrentUser.GS_IsDeveloper || GlbStaff.CurrentUser.GS_IsController || GlbStaff.CurrentUser.IsDatabaseDeveloper)
			{
				var dbAdminItem = new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin", "Database Administration"));

				var dbAdminSubItems = new ToolStripItem[]
				{
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.ConfigureServer", "Execute Configure-Server Procedure"), ExecuteConfigureServerProcedure_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.SqlSystemConfigurations", "SQL Server System Configurations"), SqlServerSystemConfigurations_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.UserRepositoryConsole", "Customize User Repository Objects"), UserRepositoryConsoleMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.RecreateDbSynonym", "Recreate Database Synonyms"), RecreateDbSynonymMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.ReferenceData", "Reference Data"), RefDataUpdateMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.RestoreDbReaderRole", "Restore Reader Role Permissions"), RestoreDbReaderRoleMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.RestoreDbLogins", "Restore Database Logins"), RestoreDbLoginsMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.SynchroniseStaffDbLogins", "Synchronize Staff Database Logins"), SynchroniseStaffDbLoginsMenuItem_Click),
					new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.BuildSqlSecurity", "Output SQL security build info"), OutputSqlSecurityBuildInfoMenuItem_Click),
				};

				dbAdminItem.DropDownItems.AddRange(dbAdminSubItems);
				if (EnvProxy.Instance.Registry.UseAlwaysOnReplicaCache)
				{
					dbAdminItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Help.DbAdmin.RefreshAlwaysOnCache", "Reset Always On Cache"), ResetAlwaysOnCacheMenuItem_Click));
				}

				HelpMenuButton.DropDownItems.Add(dbAdminItem);
			}
		}

		#endregion Help

		#endregion Menu Items

		#region Database and S3 Storage Info

		void DatabaseInfoMenuItem_Click(object sender, EventArgs e)
		{
			DatabaseAndS3InfoForm.ShowDialogAndDispose();
		}

		#endregion Database and S3 Storage Info

		#region Document Sections

		void DocumentSectionsMenuItem_Click(object sender, EventArgs e)
		{
			new DocumentSectionsForm().Show();
		}

		#endregion Document Sections

		#region Change User/Branch/Department

		void FileLoginMenuItem_Click(object sender, EventArgs e)
		{
			var checkOpenedFormsMessage = GetOpenedFormsName();
			if (!checkOpenedFormsMessage.IsNullOrEmpty())
			{
				checkOpenedFormsMessage = System.Environment.NewLine + checkOpenedFormsMessage;
				Globals.Message.Show(Res.GetString("bcc0bfb6-74e1-4a08-8fa0-b810bd8a675c", "Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:") + checkOpenedFormsMessage, Res.GetString("ed92fddf-f48a-43dd-86c8-563ee5e0dfbe", "Some forms are still open"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (ClientHookLoader.Instance.ClientHook?.HasCompanySpecificOverrides is true)
			{
				ClientHookLoader.Instance.ClientHook.Uninitialise();
				Globals.Message.Show(Res.GetString("346ee334-3de1-497c-b417-8562d4720364", "Some company specific functionality may work incorrectly after switching between two different companies. You must log out and in again after changing companies."), Res.GetString("0dd41ce2-b652-474e-9881-eb8179f566ac", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
#if WINZOR
			if (ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled)
			{
				ReLaunchWinzorAppServer(OIDCLoginRequestMessage.LoginPrompt.SelectAccount);
				return;
			}
#endif
			ShowLoginUserControl();
			Env.LoginController.Logout();
#if !WINZOR
			if (GlbStaff.CurrentUser.CheckWinzorEnabledForUser())
			{
				ObjectFactory.Get<IProgramRestarter>().ShutdownEnterpriseWithMessage(Res.GetString("73353EDE-366F-4C1B-A2F4-42CA612C7CC7", "CargoWise Winzor testers must restart The Application before changing user."));
			}
#endif
		}

		void SupportLoginMenuItem_Click(object sender, EventArgs e)
		{
			var checkOpenedFormsMessage = GetOpenedFormsName();
			if (!checkOpenedFormsMessage.IsNullOrEmpty())
			{
				checkOpenedFormsMessage = System.Environment.NewLine + checkOpenedFormsMessage;
				Globals.Message.Show(Res.GetString("bcc0bfb6-74e1-4a08-8fa0-b810bd8a675c", "Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:") + checkOpenedFormsMessage, Res.GetString("ed92fddf-f48a-43dd-86c8-563ee5e0dfbe", "Some forms are still open"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				if (ClientHookLoader.Instance.ClientHook != null && ClientHookLoader.Instance.ClientHook.HasCompanySpecificOverrides)
				{
					ClientHookLoader.Instance.ClientHook.Uninitialise();
					Globals.Message.Show(Res.GetString("346ee334-3de1-497c-b417-8562d4720364", "Some company specific functionality may work incorrectly after switching between two different companies. You must log out and in again after changing companies."), Res.GetString("0dd41ce2-b652-474e-9881-eb8179f566ac", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				ShowSupportLoginUserControl();
				Env.LoginController.Logout();
			}
		}

#if WINZOR
		void ClientSettingsMenuItem_Click(object sender, EventArgs e)
		{
			CargoWiseClientInvoker.Invoke((clientServices) =>
			{
				return clientServices.WindowService.ConfigureClientSettingsAsync();
			});
		}
#endif

		void ChangeBranchDepartmentCore()
		{
			RefreshPopupMenus();
			var checkOpenedFormsMessage = GetOpenedFormsName();
			if (!checkOpenedFormsMessage.IsNullOrEmpty())
			{
				checkOpenedFormsMessage = System.Environment.NewLine + checkOpenedFormsMessage;
				Globals.Message.Show(Res.GetString("bcc0bfb6-74e1-4a08-8fa0-b810bd8a675c", "Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:") + checkOpenedFormsMessage, Res.GetString("ed92fddf-f48a-43dd-86c8-563ee5e0dfbe", "Some forms are still open"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				if (ClientHookLoader.Instance.ClientHook != null && ClientHookLoader.Instance.ClientHook.HasCompanySpecificOverrides)
				{
					ClientHookLoader.Instance.ClientHook.Uninitialise();
					Globals.Message.Show(Res.GetString("346ee334-3de1-497c-b417-8562d4720364", "Some company specific functionality may work incorrectly after switching between two different companies. You must log out and in again after changing companies."), Res.GetString("0dd41ce2-b652-474e-9881-eb8179f566ac", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				ShowLoginLocationControl();
			}
		}

		void ChangeBranchDepartmentDefault_Click(object sender, EventArgs e)
		{
			SaveChangeBranchDepartmentSettingIfNeeded(ZString.Empty);
			SetChangeBranchDepartmentMenuItemText();
			SetChangeBranchDepartmentShortcutKeys();
			ChangeBranchDepartmentCore();
		}

		void ChangeBranchDepartmentClose_Click(object sender, EventArgs e)
		{
			SaveChangeBranchDepartmentSettingIfNeeded(ChangeBranchDepartmentSettingClose);
			SetChangeBranchDepartmentMenuItemText();
			SetChangeBranchDepartmentShortcutKeys();

			foreach (var form in ZApplication.GetOpenForms())
			{
				if (this == form
					|| form is not ZForm zForm
					|| zForm.BusinessEntity?.HasChanges == true
					|| zForm.BusinessEntity?.IsInDatabase == false)
				{
					continue;
				}

				form.Close();
			}

			ChangeBranchDepartmentCore();
		}

		void ChangeBranchDepartmentSaveAndClose_Click(object sender, EventArgs e)
		{
			SaveChangeBranchDepartmentSettingIfNeeded(ChangeBranchDepartmentSettingSave);
			SetChangeBranchDepartmentMenuItemText();
			SetChangeBranchDepartmentShortcutKeys();

			foreach (var form in ZApplication.GetOpenForms())
			{
				if (this == form)
				{
					continue;
				}

				if (form is ZForm { BusinessEntity: not null } zForm && (zForm.BusinessEntity.HasChanges || !zForm.BusinessEntity.IsInDatabase))
				{
					zForm.ValidateChildren();
					if (zForm.BusinessEntity.HasErrors() || zForm.FireSaveButton() != ContinueWithSave.Yes)
					{
						continue;
					}
				}

				form.Close();
			}

			ChangeBranchDepartmentCore();
		}

		#endregion Change User/Branch/Department

		#region Nav Bar Menu

		internal ZToolStripMenuItem SettingsMenuButton;
		internal ZToolStripMenuItem HelpMenuButton;

		void InitializeNavBarButtons()
		{
			navBarToolStrip = new ZToolStrip { Name = "NavBarToolStrip", GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Bottom };
			navBarToolStrip.Font = new Font(OFont.NavigationMenuFontName, 9F);
			AddSettingsMenu();
#if DEBUG
#if !WINZOR
			navBarToolStrip.Items.Add(new TestingMenuBuilder(this, NavigationBar.navigationViewModel).GetMenu());
#endif
#endif
			AddHelpMenu();

			NavigationBar.Controls.Add(navBarToolStrip);
			NavigationBar.CategoryChanged += NavigationBar_CategoryChanged;
		}

		void AddHelpMenu()
		{
			HelpMenuButton = new ZToolStripMenuItem(NavigationBar.navigationViewModel.HelpMenuTitle) { Name = "HelpToolStripMenuItem" };
			HelpMenuButton.Image = Icons.GetImage(IconTypes.HelpButtonRest);
			HelpMenuButton.Alignment = ToolStripItemAlignment.Right;
			HelpMenuButton.TextImageRelation = TextImageRelation.TextBeforeImage;
			SetupHelpMenu();
			navBarToolStrip.Items.Add(HelpMenuButton);
		}

		[SuppressMessage("Decruftification", "WTG3012:Avoid combining bool literals in larger boolean expressions.", Justification = "Testing")]
		void AddSettingsMenu()
		{
			SettingsMenuButton = new ZToolStripMenuItem { Name = "SettingsToolStripMenuItem" };
			SettingsMenuButton.Text = NavigationBar.navigationViewModel.OptionMenuTitle;
			SettingsMenuButton.Image = Icons.GetImage(IconTypes.Login);

			if (GlbStaff.CurrentUser.AllowPasswordChange && !ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled)
			{
				SettingsMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.FileChangePassword", "&Change Password"), FileChangePasswordMenuItem_Click));
			}

			SettingsMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.FileLogin", "&Login as New User"), FileLoginMenuItem_Click));

			if (ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled
				&& (EnvProxy.IsHostedWithCargowise || SystemDataRegistry.Instance.EnableSupportUserLogin.Value))
			{
				SettingsMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.SupportLogin", "&Support Login"), SupportLoginMenuItem_Click));
			}

			AddChangeBranchDepartmentMenuItems();

			SettingsMenuButton.DropDownItems.Add(new ToolStripSeparator());

			var netVersionSwichProvider = new CargoWise.Main.Startup.DotNetVersionSwitch.DotNetVersionSwitchManager(isRunningTest: Globals.IsTest);
			foreach (var switchMenuItem in netVersionSwichProvider.GetSwitchVersionMenuItems())
			{
				SettingsMenuButton.DropDownItems.Add(switchMenuItem);
			}

#if WINZOR
			SettingsMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.Settings", "&Settings"), ClientSettingsMenuItem_Click));
			SettingsMenuButton.DropDownItems.Add(new ToolStripSeparator());
#endif

			SettingsMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.FileExit", "E&xit"), ExitMenuItem_Click));
			SettingsMenuButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.FileExitAndRemember", "Exit and &remember open forms"), ExitAndRememberMenuItem_Click));

			navBarToolStrip.Items.Add(SettingsMenuButton);
		}

		const string ChangeBranchDepartmentSettingName = "ChangeBranchDepartmentSetting";
		string changeBranchDepartmentSetting;
		string ChangeBranchDepartmentSetting => changeBranchDepartmentSetting ??= LoadChangeBranchDepartmentSetting(new BusinessObjectFactory())?.SD_BinaryValue.ToAscii().ToString() ?? string.Empty;
		internal ZToolStripMenuItem ChangeBranchDepartmentMenuItem;
		const string ChangeBranchDepartmentSettingClose = "C";
		const string ChangeBranchDepartmentSettingSave = "S";

		void AddChangeBranchDepartmentMenuItems()
		{
			ChangeBranchDepartmentMenuItem = new ZToolStripMenuItem();
			ChangeBranchDepartmentMenuItem.Click += ChangeBranchDepartmentMenuItem_Click;
			SetChangeBranchDepartmentMenuItemText();

			ChangeBranchDepartmentMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartment.Close", "&Close Open Forms"), ChangeBranchDepartmentClose_Click));
			ChangeBranchDepartmentMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartment.SaveAndClose", "&Save and Close Open Forms"), ChangeBranchDepartmentSaveAndClose_Click));
			ChangeBranchDepartmentMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartment.Default", "&Manually Close Forms"), ChangeBranchDepartmentDefault_Click));

			SettingsMenuButton.DropDownItems.Add(ChangeBranchDepartmentMenuItem);
			SetChangeBranchDepartmentShortcutKeys();
		}

		void SetChangeBranchDepartmentMenuItemText()
		{
			if (ChangeBranchDepartmentSetting == ChangeBranchDepartmentSettingClose)
			{
				ChangeBranchDepartmentMenuItem.Text = ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartmentClose", "Change Company/&Branch/Dept. (Close Forms)");
			}
			else if (ChangeBranchDepartmentSetting == ChangeBranchDepartmentSettingSave)
			{
				ChangeBranchDepartmentMenuItem.Text = ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartmentSaveAndClose", "Change Company/&Branch/Dept. (Save/Close Forms)");
			}
			else
			{
				ChangeBranchDepartmentMenuItem.Text = ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartment", "Change Company/&Branch/Dept. (Manually Close Forms)");
			}

			var popupMenu = FindPopupMenu(ChangeBranchDepartmentMenuItem);
			if (popupMenu != null)
			{
				popupMenu.Title = ChangeBranchDepartmentMenuItem.Text;
			}
		}

		void SetChangeBranchDepartmentShortcutKeys()
		{
			var menuItemWithShortcut = (ZToolStripMenuItem)ChangeBranchDepartmentMenuItem.DropDownItems[ChangeBranchDepartmentSetting switch
			{
				ChangeBranchDepartmentSettingClose => 0,
				ChangeBranchDepartmentSettingSave => 1,
				_ => 2
			}];

			foreach (ZToolStripMenuItem menuItem in ChangeBranchDepartmentMenuItem.DropDownItems)
			{
				menuItem.ShortcutKeys = Keys.None;

				var popupMenu = FindPopupMenu(menuItem);
				if (popupMenu != null)
				{
					popupMenu.InputGestureText = menuItem.GetInputGestureText();
				}
			}

			menuItemWithShortcut.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
			var popupMenuWithShortcut = FindPopupMenu(menuItemWithShortcut);
			if (popupMenuWithShortcut != null)
			{
				popupMenuWithShortcut.InputGestureText = menuItemWithShortcut.GetInputGestureText();
			}
		}

		void ChangeBranchDepartmentMenuItem_Click(object sender, EventArgs e)
		{
			if (ChangeBranchDepartmentSetting == ChangeBranchDepartmentSettingClose)
			{
				ChangeBranchDepartmentClose_Click(sender, e);
			}
			else if (ChangeBranchDepartmentSetting == ChangeBranchDepartmentSettingSave)
			{
				ChangeBranchDepartmentSaveAndClose_Click(sender, e);
			}
			else
			{
				ChangeBranchDepartmentDefault_Click(sender, e);
			}
		}

		void RefreshPopupMenus()
		{
			foreach (var menu in rootPopupMenus)
			{
				menu.PopupMenuEvent.RefreshPopupMenus();
			}
		}

		StmData LoadChangeBranchDepartmentSetting(BusinessObjectFactory factory)
		{
			var query = new ZQuery(StmDataSchema.SD_Name, ChangeBranchDepartmentSettingName)
				.AddToFilter(StmDataSchema.SD_Owner, GlbStaff.CurrentUser.PK);
			query.OrderBy = StmDataSchema.Constants.SD_SystemCreateTimeUtc + OrderByClause.Ascending;
			return factory.LoadTop1<StmData>(query);
		}

		internal void SaveChangeBranchDepartmentSettingIfNeeded(ZString value)
		{
			rootPopupMenus.Clear();
			if (ChangeBranchDepartmentSetting == value)
			{
				return;
			}

			changeBranchDepartmentSetting = value;
			var factory = new BusinessObjectFactory();
			var settingData = LoadChangeBranchDepartmentSetting(factory);

			if (value.IsEmpty)
			{
				settingData?.Delete();
			}
			else
			{
				if (settingData is null)
				{
					settingData = factory.New<StmData>();
					settingData.SD_Name = ChangeBranchDepartmentSettingName;
					settingData.SD_Owner = GlbStaff.CurrentUser.PK;
				}

				settingData.SD_BinaryValue = ZBlob.FromAscii(value);
			}

			factory.Save();
		}

		void NavigationBar_CategoryChanged(object sender, EventArgs e)
		{
			navBarToolStrip.Visible = NavigationBar.SelectedCategory?.Name == ModuleTreeLoaderConstant.Category.Jump.Name;
		}

		readonly List<PopupMenu> rootPopupMenus = new List<PopupMenu>();
		PopupMenu FindPopupMenu(ToolStripItem item)
		{
			var menus = NavigationBar.navigationViewModel?.PopupMenus;
			if (menus != null)
			{
				foreach (var menu in menus)
				{
					var popupMenu = menu.FindPopupMenuByRelatedToolStripItem(item);
					if (popupMenu != null)
					{
						if (!rootPopupMenus.Contains(menu))
						{
							rootPopupMenus.Add(menu);
						}
						return popupMenu;
					}
				}
			}

			return null;
		}

		ZToolStrip navBarToolStrip;

		#endregion Nav Bar Menu

		#region Recreate Reference Database Synonyms

		void RecreateDbSynonymMenuItem_Click(object sender, EventArgs e)
		{
			using (var connection = Db.NewAdminConnection())
			{
				var director = ObjectFactory.New<BaseRefDbSynonymSynchroniser>(new UpgradeContext(), connection);

				var didRecreateForUserRepository = false;
				using (var progressForm = new ProgressForm())
				{
					SetUpRecreateDbSynonymProgressForm(progressForm, director);
					director.SynchroniseReferenceDbSynonyms();
					if (!director.IsCancelled && new DbUserRepository().SynchroniseSynonyms(connection))
					{
						didRecreateForUserRepository = true;
					}
				}

				SystemDataRegistry.Instance.RefDbNamesCacheIsDirty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var message = GetRecreateDbSynonymOutput(director, didRecreateForUserRepository);
				var caption = Res.GetString("e1a68d35-6cfe-45b2-9846-93ec5ad60669", "Completed recreating synonyms.");
				var icon = director.HasErrors() ? MessageBoxIcon.Error : MessageBoxIcon.Information;
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, icon, DialogResult.OK);
			}
		}

		void SetUpRecreateDbSynonymProgressForm(ProgressForm progressForm, BaseRefDbSynonymSynchroniser director)
		{
			var baseStatus = Res.GetString("eaffd2c7-f7b9-47c3-9392-9936f12b935a", "Recreating database synonyms...");
			progressForm.Status = baseStatus;
			director.StatusChanged += new EventHandler<BaseRefDbSynonymSynchroniser.StatusChangedEventArgs>(
				(progressEventSender, progressEventArgs) =>
				{
					progressForm.PercentComplete = progressEventArgs.PercentComplete;
					switch (progressEventArgs.Code)
					{
						case BaseRefDbSynonymSynchroniser.StatusCode.CreatingDatabase:
							progressForm.Status = Res.GetString("1f1f1a27-cbc2-4ac0-898f-16fc6018369c", "Creating database...\r\n{0}", progressEventArgs.Database);
							break;

						case BaseRefDbSynonymSynchroniser.StatusCode.SynchronisingSynonyms:
							progressForm.Status = string.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", baseStatus, progressEventArgs.Database);
							break;

						case BaseRefDbSynonymSynchroniser.StatusCode.Cancelling:
							progressForm.Status = Res.GetString("3b39fe34-3917-49c4-8aad-8ac0d7455b3e", "Stopping...");
							break;
					}
				});

			progressForm.Cancelled += director.Cancelled;
			progressForm.ShowProgressBar = true;
			progressForm.ShowCancelButton = true;
			progressForm.CancelProgressButtonText = Res.GetString("0f09c2df-f415-46bf-9948-f189b1cb0aa5", "Stop");
			progressForm.ShowModalTo(this);
			progressForm.Invalidate();
			progressForm.Update();
		}

		string GetRecreateDbSynonymOutput(BaseRefDbSynonymSynchroniser director, bool didRecreateForUserRepository)
		{
			var output = new StringBuilder();
			if (director.HasSuccesses())
			{
				output.AppendLine(Res.GetString("8818145a-b726-4743-903e-a13e3bd9ddb3", "Synonyms were successfully recreated for the following databases:"));
				output.AppendLine();
				foreach (var success in director.Successes)
				{
					output.Append(success);
					if (director.NewReferenceDbs.Contains(success))
					{
						output.Append(" - " + Res.GetString("6b0c37d5-a4ea-46e1-9ba8-713f9d92b5ec", "Database has been created."));
					}
					output.AppendLine();
				}

				if (didRecreateForUserRepository)
				{
					output.AppendLine(Res.GetString("d6696a88-3f3e-4ad0-ad18-0305353a071b", "User Repository [{0}{1}]", Db.DatabaseName, DbUserRepository.RepositoryDbSuffix));
				}
			}

			if (director.HasErrors())
			{
				if (director.HasSuccesses())
				{
					output.AppendLine();
					output.AppendLine();
				}
				output.AppendLine(Res.GetString("bb43b6fc-be8d-4d10-b29d-a5b9acc9e6dd", "Errors occurred when recreating synonyms for the following databases:"));
				output.AppendLine();
				output.AppendLine(string.Join(System.Environment.NewLine, director.Errors));
			}

			if (director.IsCancelled)
			{
				if (director.HasErrors() || director.HasSuccesses())
				{
					output.AppendLine();
					output.AppendLine();
				}
				output.AppendLine(Res.GetString("854ca4a2-b19f-40e0-8b39-2965a339fbcc", "Because this action stopped, some actions may not have been completed."));
			}

			return output.ToString();
		}

		#endregion Recreate Reference Database Synonyms

		#region Reference Data Update

		void RefDataUpdateMenuItem_Click(object sender, EventArgs e)
		{
			new RefDataForm().Show();
		}

		#endregion

		#region Customize User Repository Objects

		void UserRepositoryConsoleMenuItem_Click(object sender, EventArgs e)
		{
			if (!GlbStaff.CurrentUser.IsDatabaseDeveloper)
			{
				Globals.Message.ShowError(Res.GetString(
					"99877ABA-CB3B-4E1B-A90A-B2DBC79986CD",
					"Your staff record is not configured for database developer access. Contact your system administrator."));
				return;
			}

			string password = null;

			if (!ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled)
			{
				password = QueryUserSqlPassword();
				if (password.IsNullOrEmpty())
				{
					return;
				}
			}

			var userRepository = new UserRepository(new BusinessObjectFactory(), password);
			string message = null;
			if (userRepository.ValidateLogin(out message))
			{
				var form = new UserRepositoryConsoleForm(userRepository);
				AddToOpenedUserRepositoryConsoleFormList(form);
				form.Show();
			}
			else
			{
				if (!string.IsNullOrEmpty(message))
				{
					Globals.Message.ShowError(message);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("42C0CB73-EB31-4732-B5D4-0DE9F6003CAD", "Your SQL Login Password is incorrect"));
				}
			}

			string QueryUserSqlPassword() => Globals.Message.QueryUserResponse(
				new UserResponseArgument
				{
					Caption = Res.GetString("EC6FD329-1B84-43A3-B607-CA8E42BD0102", "SQL Login"),
					Message = Res.GetString("A73D7C4E-C8C0-4989-8485-D454AAC26998", "Please enter your SQL Login Password"),
					Buttons = ZMessageBoxButtons.OKCancel,
					DefaultButton = ZMessageBoxDefaultButton.Button1,
					MinimumResponseLength = 0,
					Icon = ZMessageBoxIcon.None,
					UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal,
					PasswordChar = '*'
				});
		}

		protected virtual void AddToOpenedUserRepositoryConsoleFormList(UserRepositoryConsoleForm form)
		{
		}

		#endregion Customize User Repository Objects

		#region Set SQL Password

		internal void SetSQLPasswordMenuItem_Click(object sender, EventArgs e)
		{
			string caption = Res.GetString("A721AED2-0313-483D-8631-D6E3846329D8", "Set SQL Password");
			if (!GlbStaff.CurrentUser.HasDatabaseAccess)
			{
				Globals.Message.ShowInformation(Res.GetString("69CAA4D1-E632-4518-B164-AD2137826AE2", "You staff record is not configured for SQL Server access. Contact your system administrator."), caption);
				return;
			}

			var factory = new BusinessObjectFactory { NameForDebugging = "Set SQL Password" };
			var staff = factory.Load<GlbStaff>(Env.CurrentUser.PK);
			factory.ReloadAllSafe<GlbStaff>();
			if (ZFormModaliser.ShowDialogAndDispose(new SetSQLPasswordForm(staff)) == System.Windows.Forms.DialogResult.OK)
			{
				factory.Save();
			}
		}

		#endregion Set SQL Password

		#region STL Billing

		void CollectSTLRetrospectiveDataMenuItem_Click(object sender, EventArgs e)
		{
			new StlRetrieverForm().Show();
		}

		void StlCollectorsMenuItem_Click(object sender, EventArgs e)
		{
			new RefStlScriptGeneratorForm().Show();
		}

		#endregion STL Billing

		#region Restore Reader Role Permissions

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
		void RestoreDbReaderRoleMenuItem_Click(object sender, EventArgs e)
		{
			string caption = Res.GetString("585b4f11-78b7-4324-b855-0472492acc08", "Restore database roles and permissions");

			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					var dbSecurity = new DbSecurity();

					dbSecurity.RefreshDbReaderRolePermissions(connection, msg => { Log(msg); });
					dbSecurity.RefreshSchemaDbRoles(connection, msg => { Log(msg); });
				}

				Globals.Message.ShowInformation(
					Res.GetString("7865c71a-f13e-4c6d-9055-e8427a67adb0", "Completed setting database roles and permissions."),
					caption);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, caption);
			}
		}

		static void Log(string msg)
		{
			if (!string.IsNullOrWhiteSpace(msg))
			{
				Globals.Message.ShowInformation(msg);
			}
		}

		#endregion Restore Reader Role Permissions

		#region Restore Database Logins

		ProgressWithDetailesForm restoreDbLoginsForm;

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
		internal void RestoreDbLoginsMenuItem_Click(object sender, EventArgs e)
		{
			restoreDbLoginsForm = new ProgressWithDetailesForm();
			restoreDbLoginsForm.CaptionResourceString = Res.GetData("174CA2F3-858E-441C-9E08-52E22932A154", "Restore database logins");
			restoreDbLoginsForm.ShowProgressBar = true;
			restoreDbLoginsForm.CancelProgressButtonText = Res.GetString("8BDD4F1B-0E4B-4B85-9451-3B625DAAD751", "Wait ...");
			restoreDbLoginsForm.EnableCancelButton = false;
			restoreDbLoginsForm.SleepBetweenRefreshMilliseconds = 0;
			restoreDbLoginsForm.Show();

			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					var totalLogins = connection.Logins.Length;
					var loginStepPercent = 100f / (totalLogins * 2);
					var i = 0;
					foreach (var login in connection.Logins)
					{
						login.EnableLogin(msg => restoreDbLoginsForm.AddLog(msg));
						restoreDbLoginsForm.PercentComplete = (int)(++i * loginStepPercent);

						login.EnsureLoginCorrectlyMappedToAllDatabases(msg => restoreDbLoginsForm.AddLog(msg));
						restoreDbLoginsForm.PercentComplete = (int)(++i * loginStepPercent);
					}
				}

				restoreDbLoginsForm.SetStatusAndPercentComplete(Res.GetString("2a2d5072-6e97-46fd-86e9-9830a8babf37", "Completed setting database logins."), 100);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				restoreDbLoginsForm.AddLog(ex.Message);
			}

			restoreDbLoginsForm.ActivateCloseButton();
		}

		#endregion Restore Database Logins

		#region Synchronise Staff Database Logins

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
		void SynchroniseStaffDbLoginsMenuItem_Click(object sender, EventArgs e)
		{
			string caption = Res.GetString("658AB724-84F1-428C-9746-F85C1E53A742", "Synchronize Staff Database Logins");

			using (var connection = Db.NewAdminConnection())
			{
				var userManager = new DbUserManager();
				userManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(connection);
			}

			Globals.Message.ShowInformation(Res.GetString("1C8CA43D-8FB3-43C4-B19D-704453877DE9", "Process completed successfully."), caption);
		}

		#endregion Synchronise Staff Database Logins

		#region Build Sql security
		ProgressWithDetailesForm outputSqlSecurityBuildInfoForm;
		BackgroundWorker backgroundWorker;

		void OutputSqlSecurityBuildInfoMenuItem_Click(object sender, EventArgs e)
		{
			outputSqlSecurityBuildInfoForm = new ProgressWithDetailesForm();
			outputSqlSecurityBuildInfoForm.CaptionResourceString = Res.GetData("A1D30D0F-D51A-4546-8A91-AB878839C04A", "Output SQL security build info");
			outputSqlSecurityBuildInfoForm.ShowProgressBar = true;
			outputSqlSecurityBuildInfoForm.CancelProgressButtonText = Res.GetString("8BDD4F1B-0E4B-4B85-9451-3B625DAAD751", "Wait ...");
			outputSqlSecurityBuildInfoForm.EnableCancelButton = false;
			outputSqlSecurityBuildInfoForm.SleepBetweenRefreshMilliseconds = 0;

			backgroundWorker = new BackgroundWorker();
			backgroundWorker.DoWork += BuildSecurityOutputBackgroundWorker_DoWork;
			backgroundWorker.RunWorkerCompleted += BuildSecurityOutputBackgroundWorker_RunWorkerCompleted;
			backgroundWorker.RunWorkerAsync();

			outputSqlSecurityBuildInfoForm.Show();
		}

		void BuildSecurityOutputBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				if (e.Error.IsCriticalException())
				{
					throw e.Error;
				}
				else
				{
					outputSqlSecurityBuildInfoForm.AddLog(e.Error.Message);
				}
			}

			outputSqlSecurityBuildInfoForm.SetStatusAndPercentComplete(Res.GetString("EC1FC6F6-6900-44D8-BC21-9EFBF78B8ACE", "Completed building security output"), 100);
			outputSqlSecurityBuildInfoForm.ActivateCloseButton();
		}

		void SetStatusAndPercentCompleteToOutputSqlSecurityBuildInfoForm(string status, int percentComplete)
		{
			var setPercentCompleteDelegate = new Action(() =>
			{
				outputSqlSecurityBuildInfoForm.SetStatusAndPercentComplete(status, percentComplete);
			});

			if (outputSqlSecurityBuildInfoForm.InvokeRequired)
			{
				outputSqlSecurityBuildInfoForm.Invoke(setPercentCompleteDelegate);
			}
			else
			{
				setPercentCompleteDelegate();
			}
		}

		void AddLogToOutputSqlSecurityBuildInfoForm(string message)
		{
			var addLogDelegate = new Action(() =>
			{
				outputSqlSecurityBuildInfoForm.AddLog(message);
			});

			if (outputSqlSecurityBuildInfoForm.InvokeRequired)
			{
				outputSqlSecurityBuildInfoForm.Invoke(addLogDelegate);
			}
			else
			{
				addLogDelegate();
			}
		}

		void BuildSecurityOutputBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var messageToProgressMapper = new SqlSecurityMessageToProgressMapper();

			var logger = new SqlSecurityBuildOutputCallbackLogger((logType, message, exception) =>
			{
				if (messageToProgressMapper.Progress(message))
				{
					SetStatusAndPercentCompleteToOutputSqlSecurityBuildInfoForm(message, messageToProgressMapper.CurrentProgress);
				}

				AddLogToOutputSqlSecurityBuildInfoForm($"{message}{System.Environment.NewLine}{exception?.ToString()}");
			});

			var sqlSecurityManager = new SqlSecurityManager(logger, Db.DatabaseName);
			using (var adminConnection = Db.NewAdminConnection())
			{
				sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None, trialRun: true);
			}
		}

		#endregion Build Sql security

		#region Refresh AlwaysOn Cache

		internal void ResetAlwaysOnCacheMenuItem_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("62009084-7C5D-4BB3-B838-9DE54D666638", "Reset Always On Cache");

			var registry = EnvProxy.Instance.Registry;
			registry.AlwaysOnReplicaCachedInfos = Array.Empty<AlwaysOnReplicaInfo>();
			registry.AvailabilityGroupInfo = Array.Empty<string>();

			Globals.Message.ShowInformation(Res.GetString("CF60A61C-4FF7-4931-A93E-4F0C000A32F8", "Cache has been reset."), caption);
		}

		#endregion Refresh AlwaysOn Cache

		#region SQL Server System Configurations

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
		internal void ExecuteConfigureServerProcedure_Click(object sender, EventArgs e)
		{
			ExecuteConfigureServerProcedure(new ServerConfigurationUtils());
		}

		internal void ExecuteConfigureServerProcedure(ServerConfigurationUtils serverConfigurationUtils)
		{
			var title = Res.GetString("D436E28A-0237-46B0-9A63-240036EA243A", "Execute Configure-Server Procedure");
			if (Globals.Message.ShowConfirmation(Res.GetString("B7676290-39B4-47F5-8609-3B3DD0917251", "The system will execute the Configure-Server procedure."), title, Res.GetString("82f9527b-10ab-4330-863c-cf76c85f391a", "Please type in the following to confirm and proceed."), Res.GetString("59db8d38-a8ac-49bd-81fe-5e7052253af0", "confirm"), MessageBoxIcon.Warning) == DialogResult.OK)
			{
				try
				{
					using var connection = Db.NewAdminConnection();
					serverConfigurationUtils.EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(connection, out var replicaExceptions);
					if (replicaExceptions != null && replicaExceptions.Count > 0)
					{
						var secondaryErrors = string.Join("\r\n", replicaExceptions.Select(x => x.Message).ToArray());
						Globals.Message.ShowWarning(Res.GetString("02BA201E-00C8-49FD-A10E-CA4BC3839891", "The Configure-Server procedure executed successfully on the primary server. However, the following errors occurred when attempting to configure the secondaries: \r\n{0}", secondaryErrors), title);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("F0319307-3746-49FA-A03C-25F78977C283", "Configure-Server procedure executed successfully."), title);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message, title);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
		internal void SqlServerSystemConfigurations_Click(object sender, EventArgs e)
		{
			try
			{
				ZFormModaliser.ShowDialogAndDispose(new SqlSystemConfigurationsForm());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, (sender as ZToolStripMenuItem)?.Text);
			}
		}

		#endregion
	}
}
