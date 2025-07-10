using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Certification.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.IdentitySecurity;
using static CargoWise.Definitions.Authentication.SupportLogonRole;
using static Enterprise.Client.EDI.EDISecurityCheckpoints.Constants;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using static Enterprise.Client.EDI.IncidentManager.GUI.ReopenIncidentPopup;
using LoggerForTest = Enterprise.Integration.LoggerForTest;

#if WINZOR
using Microsoft.JSInterop;
using WinzorFramework;
#endif

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(SupportIncidentForm))]
	public class SupportIncidentFormTest : ZFormBasherTest
	{
		public void TestClosureResolutionDropEditVisible()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			Assert(!incident.IsCurrentResolutionCodeClosedOrResolved);
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				var closureResolutionDropEdit = form.Controls.Find("closureResolutionDropEdit", true)[0] as ZDropEdit;
				AssertNotNull(closureResolutionDropEdit);
				Assert(!closureResolutionDropEdit.Visible);
				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
				Assert(closureResolutionDropEdit.Visible);
				Factory.Save();

				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
				Assert(!closureResolutionDropEdit.Visible);
				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
				Assert(closureResolutionDropEdit.Visible);
			}
		}

		public void TestClosureResolutionDropEditShouldShowDisabledCloseStatusDescription()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			incident.CloseIncident(resolvedCode, "");
			Factory.Save();

			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				var closureResolutionDropEdit = form.Controls.Find("closureResolutionDropEdit", true)[0] as ZDropEdit;
				AssertNotNull(closureResolutionDropEdit);
			}

			var zzzClosureDisposition = registryValue.Cast<IncidentClosureDisposition>().FirstOrDefault(x => x.Code == resolvedCode);
			zzzClosureDisposition.Bool = false;
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident2 = factory2.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			using (var form = new SupportIncidentFormForTest(incident2))
			{
				form.Show();
				var closureResolutionDropEdit = form.Controls.Find("closureResolutionDropEdit", true)[0] as ZDropEdit;
				AssertNotNull(closureResolutionDropEdit);
				AssertEquals("closureResolutionDropEdit code should be ZZZ", resolvedCode, closureResolutionDropEdit.CodeBox.Text);
				AssertEquals("closureResolutionDropEdit description should be ZZZ Description", resolvedDecription, closureResolutionDropEdit.DescriptionBox.Text);
			}
		}

		public void TestRelatedItemsTabShouldContainUnifiedControlsForRelatedItems()
		{
			using (var form = (SupportIncidentForm)GetFormToBashCore())
			{
				form.Show();
				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				form.TopLevelTabControl_Exposed.SelectedTab = relatedTab;
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("RelatedItemGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("NetworkDiagramGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ParentWorkflowGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ChildWorkflowGroupBox"));
			}
		}

		public void TestPlayButtonFocus()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertEquals(false, form.ConversationMessageTextBox.Focused);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals(true, form.ConversationMessageTextBox.Focused);
			}
		}

		public void TestGenerateLoginToken_SecurityCheckpointDenied()
		{
			InitializeData(out var db, out var lic, out var incident);

			Factory.Save();

			CheckGenerateLoginTokenButtonVisible(incident, false, db.LD_LicenceType == DatabaseTypes.Codes.Production);
			CheckGenerateLoginTokenButtonVisible(incident, true, db.LD_LicenceType == DatabaseTypes.Codes.Production);

			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			CheckGenerateLoginTokenButtonVisible(incident, false, db.LD_LicenceType == DatabaseTypes.Codes.Production);
			CheckGenerateLoginTokenButtonVisible(incident, true, db.LD_LicenceType == DatabaseTypes.Codes.Production);
		}

		void CheckGenerateLoginTokenButtonVisible(SupportIncident incident, bool visible, bool production)
		{
			if (production)
			{
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsSuperUser).IsAllowed = visible;
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser).IsAllowed = visible;
			}
			else
			{
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsSuperUser).IsAllowed = visible;
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser).IsAllowed = visible;
			}

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;
				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", searchAllChildren: true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", searchAllChildren: true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", searchAllChildren: true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertNotNull(generateDiagnosticsButton);
				AssertNotNull(generateLoginTokenWithOldEntCodeButton);
				AssertEquals(visible, generateButton.Visible);
				AssertEquals(visible, generateDiagnosticsButton.Visible);
				AssertEquals(visible, generateLoginTokenWithOldEntCodeButton.Visible);
				Balloon.Instance.Hide();
			}
		}

		[ExpectNoExceptions]
		public void TestGenerateLoginToken_NoLicenseDatabase()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			incident.IM_OH_Client = licCompany.Header.PK;
			using (var form = new SupportIncidentForm(incident))
			{
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(0, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(false, generateButton.Visible);
				AssertEquals(false, generateDiagnosticsButton.Visible);
				AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);
			}
		}

		[TestDate]
		[DeveloperOnlyTest]
		public void TestGenerateLoginToken()
		{
			var currentDate = DateTime.UtcNow;
			TestDateAttribute.Date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);

			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var db1 = licCompany.LicDatabases.AddNew();
			db1.LD_HostedLocation = "NCW";
			db1.LD_ServerCode = "DB1";

			var db2 = licCompany.LicDatabases.AddNew();
			db2.LD_HostedLocation = "PRD";
			db2.LD_ServerCode = "DB2";
			db2.LD_LicenceType = DatabaseTypes.Codes.Test;

			var lic1 = licCompany.GetHeader(db1);
			var currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_Code = "TUR";
			currentUser.GS_FullName = "FullName";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = lic1.Company.Header.PK;
			incident.IM_IncidentNumber = "CS00001";
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CheckLoginToken(incident, lic1, UserType.CW1, 1, "cw1exttest");
				CheckLoginToken(incident, lic1, UserType.Diagnostic, 1, "diagexttest");
				CheckLoginToken(incident, lic1, UserType.CW1, 0, "cw1extprod");
				CheckLoginToken(incident, lic1, UserType.Diagnostic, 0, "diagextprod");
			}
		}

		[TestDate]
		[DeveloperOnlyTest]
		public void TestGenerateLoginTokenWithOldEnterpriseCode()
		{
			var currentDate = DateTime.UtcNow;
			TestDateAttribute.Date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);

			InitializeData(out var db, out var lic, out var incident);

			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

#if WINZOR
				// Mock clipboard in winzor
				const string ClipboardJS = "/_content/WinzorFramework/js/module/clipboard.js";
				var mockIJSRuntime = new Mock<IJSRuntime>();
				mockIJSRuntime.Setup(runtime => runtime.InvokeAsync<IJSObjectReference>("import", new object[] { ClipboardJS })).Returns(new ValueTask<IJSObjectReference>(new Mock<IJSObjectReference>().Object));
				var cwcs = WinzorDispatcher.Current.CurrentContext.Form?.CargoWiseClientServices;
				if (cwcs != null)
				{
					cwcs.JSRuntime = mockIJSRuntime.Object;
				}

#endif

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);

				var serverCode = ((LicenceDatabase)databasesModuleButtonGrid.InnerGrid.ListManager.Current).LD_ServerCode;
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				generateButton.PerformClick();
				var expectedErrorMessage = "Please upload the private key in the registry item 'WiseTech Global Client Extensions -> Customer Service Incidents -> CWSupport Account Login Token Private Key' to sign the login token.";
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				var privateKeyBytes = Encoding.UTF8.GetBytes(PrivateKey);
				using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, privateKeyBytes))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					generateButton.PerformClick();
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				}

#if !WINZOR

				// comment out in winzor as it doesn't support SafeClipboard.GetDataObject(),
				// test that the generation token function is working except clipboard in winzor version

				var copiedData = SafeClipboard.GetDataObject().GetData(typeof(string));
				var token = copiedData.ToString();
				AssertNotNullOrEmpty(token);

				var log = lic.Company.Header.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SupportLoginTokenGeneratedCode).LastOrDefault();
				var audience = $"ENT{serverCode}";
				AssertNotNull(log);
				AssertEquals($"Support Login Token Generated|Incident=CS00001|SystemCode={audience}|UserInitial=E", log.SL_Reference);

				var cert = new X509Certificate2(Encoding.UTF8.GetBytes(CertificateString));
				var securityToken = JwtSecurity.VerifySignedJwt(cert.GetRSAPublicKey(), token);
				var jwtToken = (JwtSecurityToken)securityToken;

				AssertEquals(audience, jwtToken.Audiences.FirstOrDefault());
#endif
			}
		}

		[TestDate]
		public void TestGenerateLoginTokenWithOldEnterpriseCode_DeniedWithValidDaysOfGenerateLoginTokenWithOldEntCodeButtonRegistry()
		{
			InitializeData(out var db, out var lic, out var incident);

			Factory.Save();

			var currentDate = DateTime.UtcNow.AddDays(8);
			TestDateAttribute.Date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);

			AssertGenerateLoginTokenWithOldEntCodeButtonVisibility(incident);
		}

		public void TestGenerateLoginTokenWithOldEnterpriseCode_DeniedWithNoOldEnterpriseCodeLog()
		{
			InitializeData(out var db, out var lic, out var incident, ifAddLog: false);

			Factory.Save();
			AssertGenerateLoginTokenWithOldEntCodeButtonVisibility(incident);
		}

		void AssertGenerateLoginTokenWithOldEntCodeButtonVisibility(SupportIncident incident)
		{
			using (var form = new SupportIncidentForm(incident))
			{
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", searchAllChildren: true).FirstOrDefault();
				AssertNotNull(generateLoginTokenWithOldEntCodeButton);
				AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);
			}
		}

		public void TestGenerateLoginToken_IncidentClosed()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			InitializeData(out var db, out var lic, out var incident);
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(false, generateButton.Visible);
				AssertEquals(false, generateDiagnosticsButton.Visible);
				AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);
			}
		}

		public void TestGenerateLoginToken_IncidentClosedWithClosedAwaitingClientResponse()
		{
			InitializeData(out var db, out var lic, out var incident);
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(true, generateButton.Visible);
				AssertEquals(true, generateDiagnosticsButton.Visible);
				AssertEquals(true, generateLoginTokenWithOldEntCodeButton.Visible);
			}
		}

		public void TestGenerateLoginToken_ClosedAwaitingClientResponseWhenIncidentAlreadyClosed()
		{
			InitializeData(out var db, out var lic, out var incident);

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				AssertEquals(false, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(false, generateButton.Visible);
				AssertEquals(false, generateDiagnosticsButton.Visible);
				AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);

				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);

				var generateButton2 = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton2 = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton2 = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton2);
				AssertEquals(true, generateButton2.Visible);
				AssertEquals(true, generateDiagnosticsButton2.Visible);
				AssertEquals(true, generateLoginTokenWithOldEntCodeButton2.Visible);
			}
		}

		public void TestGenerateLoginToken_IncidentFromOpenToClosed()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var db1 = licCompany.LicDatabases.AddNew();
			db1.LD_HostedLocation = "NCW";
			db1.LD_ServerCode = "DB1";
			db1.Logs.AddNew(Events.MessageStatusChange, "LicenceDataBase enterpriseCode change from 'ENT' to 'TST'");
			var lic1 = licCompany.GetHeader(db1);
			var incident = GetIncidentForEConversationTest();
			incident.IM_OH_Client = lic1.Company.Header.PK;
			Factory.Save();
			var sendPermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;

			try
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
					form.Show();

					bool isPendingEConversationPopupShown = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
					{
						var closeIncidentForm = dialog as CloseIncidentPopupForm;
						if (closeIncidentForm != null)
						{
							var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
							action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
							action.SynchroniseToIncident();
						}

						var pendingEConversationForm = dialog as SendEConversationForm;
						if (pendingEConversationForm != null)
						{
							isPendingEConversationPopupShown = true;
						}
					});
					form.ConversationMessageTextBox.Text = "blah blah";
					form.CloseAsButtonForTest.PerformClick();

					Assert("Should be no pending eConversation message", !isPendingEConversationPopupShown);
					AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
					AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
					AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident.IM_ClosureResolution);

					var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
					var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
					mainTabControl.SelectedTab = licenseTabPage;

					var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
					AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

					var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
					var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
					var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
					AssertNotNull(generateButton);
					AssertEquals(false, generateButton.Visible);
					AssertEquals(false, generateDiagnosticsButton.Visible);
					AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);
				}
			}
			finally
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendPermissions;
			}
		}

		public void TestGenerateLoginToken_IncidentFromClosedToOpen()
		{
			InitializeData(out var db, out var lic, out var incident);

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				AssertEquals(false, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(false, generateButton.Visible);
				AssertEquals(false, generateDiagnosticsButton.Visible);
				AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);

				incident.IM_Status = SupportIncidentLookups.Status.Open;
				incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;

				AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);

				var generateButton2 = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton2 = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton2 = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton2);
				AssertEquals(true, generateButton2.Visible);
				AssertEquals(true, generateDiagnosticsButton2.Visible);
				AssertEquals(true, generateLoginTokenWithOldEntCodeButton2.Visible);
			}
		}

		public void TestGenerateLoginToken_SecurityCheckpointDeniedWhenSetAreTokenButtonsVisible()
		{
			InitializeData(out var db, out var lic, out var incident);

			incident.IM_Status = SupportIncidentLookups.Status.Open;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsSuperUser).IsAllowed = false;
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser).IsAllowed = false;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(false, generateButton.Visible);
				AssertEquals(false, generateDiagnosticsButton.Visible);
				AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);
			}
		}

		public void TestGenerateLoginToken_PopMessageBeforeSavingIncident()
		{
			InitializeData(out var db, out var lic, out var incident);

			Factory.Save();

			var unitTestUserNotification = UnitTestUserNotification.Instance;
			using (var form = new SupportIncidentForm(incident))
			{
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				form.Show();

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
				var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(true, generateButton.Visible);
				AssertEquals(true, generateDiagnosticsButton.Visible);
				AssertEquals(true, generateLoginTokenWithOldEntCodeButton.Visible);

				incident.IM_Status = SupportIncidentLookups.Status.Closed;
				Factory.Save();
				incident.IM_Status = SupportIncidentLookups.Status.Open;
				generateButton.PerformClick();
				var generateButtonMessage = unitTestUserNotification.LastMessage.Text;
				AssertEquals("Cannot generate a token before saving.", generateButtonMessage);
			}
		}

		public void TestGenerateLoginToken_WorkflowItemsClose()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var db1 = licCompany.LicDatabases.AddNew();
			db1.LD_HostedLocation = "NCW";
			db1.LD_ServerCode = "DB1";
			db1.Logs.AddNew(Events.MessageStatusChange, "LicenceDataBase enterpriseCode change from 'ENT' to 'TST'");
			var lic1 = licCompany.GetHeader(db1);

			var incident = GetIncidentForEConversationTest();
			incident.IM_OH_Client = lic1.Company.Header.PK;
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			var sendPermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;

			try
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
				EDISecurityCheckpoints.CWSupportLogonToExternalProductionSystemAsSuperUser.IsAllowed = true;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
					form.Show();

					bool isPendingEConversationPopupShown = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
					{
						var closeIncidentForm = dialog as CloseIncidentPopupForm;
						if (closeIncidentForm != null)
						{
							var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
							action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
							action.SynchroniseToIncident();
						}

						var pendingEConversationForm = dialog as SendEConversationForm;
						if (pendingEConversationForm != null)
						{
							isPendingEConversationPopupShown = true;
						}
					});
					form.ConversationMessageTextBox.Text = "blah blah";
					form.CloseAsButtonForTest.PerformClick();

					Assert("Should be no pending eConversation message", !isPendingEConversationPopupShown);
					AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

					var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
					var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
					mainTabControl.SelectedTab = licenseTabPage;

					var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
					AssertEquals(1, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

					var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
					var generateDiagnosticsButton = (ZButton)licenseTabPage.Controls.Find("GenerateDiagnosticsLoginTokenButton", true).FirstOrDefault();
					var generateLoginTokenWithOldEntCodeButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenWithOldEntCodeButton", true).FirstOrDefault();
					AssertNotNull(generateButton);
					AssertEquals(false, generateButton.Visible);
					AssertEquals(false, generateDiagnosticsButton.Visible);
					AssertEquals(false, generateLoginTokenWithOldEntCodeButton.Visible);

					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

					AssertEquals(true, generateButton.Visible);
					AssertEquals(true, generateDiagnosticsButton.Visible);
					AssertEquals(true, generateLoginTokenWithOldEntCodeButton.Visible);
				}
			}
			finally
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendPermissions;
			}
		}

		void CheckLoginToken(SupportIncident incident, LicenceHeader lic, UserType userType, int databaseIndex, string expectedRole)
		{
			var forDiagnosticUser = userType == UserType.Diagnostic;
			var buttonName = forDiagnosticUser ? "GenerateDiagnosticsLoginTokenButton" : "GenerateLoginTokenButton";

			using (var form = new SupportIncidentForm(incident))
			{
				var production = databaseIndex == 0;

				Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsSuperUser).IsAllowed = production;
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser).IsAllowed = production;
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsSuperUser).IsAllowed = !production;
				Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser).IsAllowed = !production;

				form.Show();

#if WINZOR
				// Mock clipboard in winzor
				const string ClipboardJS = "/_content/WinzorFramework/js/module/clipboard.js";
				var mockIJSRuntime = new Mock<IJSRuntime>();
				mockIJSRuntime.Setup(runtime => runtime.InvokeAsync<IJSObjectReference>("import", new object[] { ClipboardJS })).Returns(new ValueTask<IJSObjectReference>(new Mock<IJSObjectReference>().Object));
				var cwcs = WinzorDispatcher.Current.CurrentContext.Form?.CargoWiseClientServices;
				if (cwcs != null)
				{
					cwcs.JSRuntime = mockIJSRuntime.Object;
				}

#endif

				var licenseTabPage = form.Controls.Find("LicenseTabPage", true).FirstOrDefault() as ZTabPage;
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedTab = licenseTabPage;

				var databasesModuleButtonGrid = licenseTabPage.Controls.Find("DatabasesModuleButtonGrid", true).FirstOrDefault() as ModuleButtonGridForLicencing;
				AssertEquals(2, databasesModuleButtonGrid.InnerGrid.ListManager.List.Count);

				var generateButton = (ZButton)licenseTabPage.Controls.Find(buttonName, true).FirstOrDefault();
				AssertNotNull(generateButton);

				databasesModuleButtonGrid.InnerGrid.ListManager.Position = databaseIndex;
				var serverCode = ((LicenceDatabase)databasesModuleButtonGrid.InnerGrid.ListManager.Current).LD_ServerCode;
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				generateButton.PerformClick();
				var expectedErrorMessage = "Please upload the private key in the registry item 'WiseTech Global Client Extensions -> Customer Service Incidents -> CWSupport Account Login Token Private Key' to sign the login token.";
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				var privateKeyBytes = Encoding.UTF8.GetBytes(PrivateKey);
				using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, privateKeyBytes))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					generateButton.PerformClick();
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				}

#if !WINZOR

				// comment out in winzor as it doesn't support SafeClipboard.GetDataObject(),
				// test that the generation token function is working except clipboard in winzor version

				var copiedData = SafeClipboard.GetDataObject().GetData(typeof(string));
				var token = copiedData.ToString();
				AssertNotNullOrEmpty(token);

				var log = lic.Company.Header.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SupportLoginTokenGeneratedCode).LastOrDefault();
				var audience = forDiagnosticUser ? $"ENT{serverCode}_{nameof(UserType.Diagnostic)}" : $"ENT{serverCode}";
				AssertNotNull(log);
				AssertEquals($"Support Login Token Generated|Incident=CS00001|SystemCode={audience}|UserInitial=TUR", log.SL_Reference);

				var cert = new X509Certificate2(Encoding.UTF8.GetBytes(CertificateString));
				var securityToken = JwtSecurity.VerifySignedJwt(cert.GetRSAPublicKey(), token);
				var jwtToken = (JwtSecurityToken)securityToken;

				AssertNotNull(jwtToken);
				AssertNotNull(jwtToken.Payload.Jti);
				AssertEquals("TUR", jwtToken.Subject);
				AssertEquals(audience, jwtToken.Audiences.FirstOrDefault());
				AssertEquals(TestDateAttribute.Date, jwtToken.IssuedAt);
				AssertEquals(TestDateAttribute.Date.AddSeconds(90), jwtToken.ValidTo);
				AssertEquals("FullName", jwtToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value);

				var payload = jwtToken.Payload;
				var incidentNumber = payload["incident"].ToString();
				AssertEquals("CS00001", incidentNumber);

				var userRoles = jwtToken.Claims.Where(c => c.Type == "roles").Select(claim => claim.Value).ToArray();
				AssertEquals(1, userRoles.Length);
				AssertEquals(true, userRoles.Contains(expectedRole));

#endif
			}
		}

		void InitializeData(out LicenceDatabase db, out LicenceHeader lic, out SupportIncident incident, bool ifAddLog = true)
		{
			incident = Factory.NewWithValidTestData<SupportIncident>();
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			db = licCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "DB1";
			if (ifAddLog)
			{
				db.Logs.AddNew(Events.MessageStatusChange, "LicenceDataBase enterpriseCode change from 'ENT' to 'TST'");
			}
			lic = licCompany.GetHeader(db);
			incident.IM_OH_Client = lic.Company.Header.PK;
			incident.IM_IncidentNumber = "CS00001";
		}

		const string PrivateKey = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDnv8pQZ3Zgettj
WJWJJfAYJZ7xSy/HE8+w0DkOXOMfWkDLo4lvl1ILxI8ievnFAK/B/frSJ/deuRII
mS8mXH1GFweCmdBB8uK69Ugd9tlAcxEvv46rgA/N01eSdcExJ1SONz15IGOnFGEM
2p8vEdwmVLMZlk0kKMueOcveq0rlYE2CO7uD5k+58woFF8ByI2UAlBYwr8IsUK22
WUf3C4UFHwhhjqivN98nWehx3TCV7mVw2DyKGCwZKiqAxxHfZlTeX4w0TyoNwuKV
XY5pbRqWi7Yret3emrxY61lOAYYw4dpMQ4EBeqBdlBvOC6JcR7qKOgsGDCbOSCse
rDomAZ59AgMBAAECggEAC5sz1H4HVC1NonaTxUgZ86OislPrzc0PXZFit3ZDG6qx
9wuMJ64M5QZQOCUE2u8TXk72yk1HxWiAX4ozGHk7qZB1XFQ8Cql8MxUd/6jWrYnL
EQ3yD7hnUjgPjogI3Qoqtmqhe1j6FKqKcmcv317GHJdTrD2LmdAp47/iQWxpC6lZ
d0g5F6lmdztkZdQhm7njhxExJ+CdU68rnJeks3GPYQzCZA4V7tkxP98Y5XaBWcdT
/dK1vcXS1SrInEDZvli/cXIc8sAf6LqDB9yqdcPw++oI+aUFLGmOnImCouPx4jA5
MXLdf0UXsLJbPgteBCtTxBeWene76Ym3CkyS3n0TMwKBgQDvzsABwAx7rXpIVwbL
ff3a11T2JQVHG8Ts1TvWtxFYBcLt0QThTMve4VENDQUk/il3Ltn+lLgjx+5Vyyaa
3UDYuZmDnbrlq4DfePZa7VNWj6TBFNRChMzxZG3MWUHP7qda8NcFM6pEZJpyFSeQ
8ij5Wu7PuywW5D3rkoc0sDOyKwKBgQD3Zb6KQr1wvCHy0GH86FSNYB0P3mS45KWM
JU3AdlwndtCVlKyDuhtlcaxK2OlZZiLU0aKnvLR+y77OPu5YawcEdGrZmwGerVYd
fzKDzSJuEn8MM13+GcAJlFlsoklUaeu3bHmAGKUw3ph1B6Ix0c2/HHFkjHelbT9b
t2Kc8GGl9wKBgEEzqrsPF5XNDjF7EArmH86Pu7cNS8kQwNNQCuwPbHTNZDm7GiOT
+N6JzrrIrnxnaqjQIU956jM4WhIToVR8EfSbSiUiDr4BipG4VutUGdOwTLB+1FOd
vgdoMf5cymsZzYEJeL0eVg4weFnKbK6ZWRCra8EpeAxlVHyno4Fs4zFvAoGBAOrQ
y3V3u09RgfdyCk9+RSKa43q4X2mOvAK1NYND1FwwzfHr14KAFpjGt/2ivHl6E/1j
rLsAxWDECirAWIHbtCFqTjCUi4kMhPwiStQG1HMdYzE1YDVaQ4fUIryVnHxevLiw
YPJQchpcbOBHio82z85hNM928+k0NDrdaOAE2OopAoGAaMsqGcNzwXZmYI+GNH6Z
zZPH0t4Cg+m3L1HliI+8FeMxwL6sWRxxL1qGG73xj7JGUjxCIPX52oGCoUob/x1Y
VE5Exfnv+O555Sy5hksOvTepn1fiZWVoDQc3KntZegRFKGEuozqQ1eKCkLieQ0/m
k8VXc3uhpFenM2ZGK0TedMY=
-----END PRIVATE KEY-----";

#if !WINZOR
		const string CertificateString = @"-----BEGIN CERTIFICATE-----
MIID2jCCAsKgAwIBAgIQR0RemLM7Lo27g7CJGUVSRzANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwNTA4MDYxNDAzWhcNMjQwNTA3MDcxNDAz
WjBjMQswCQYDVQQGEwJBVTEMMAoGA1UECAwDU1lEMQwwCgYDVQQHDANTWUQxDDAK
BgNVBAoMA1dURzEMMAoGA1UECwwDV1RHMRwwGgYDVQQDDBN3aXNldGVjaC5nbG9i
YWwuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA57/KUGd2YHrb
Y1iViSXwGCWe8UsvxxPPsNA5DlzjH1pAy6OJb5dSC8SPInr5xQCvwf360if3XrkS
CJkvJlx9RhcHgpnQQfLiuvVIHfbZQHMRL7+Oq4APzdNXknXBMSdUjjc9eSBjpxRh
DNqfLxHcJlSzGZZNJCjLnjnL3qtK5WBNgju7g+ZPufMKBRfAciNlAJQWMK/CLFCt
tllH9wuFBR8IYY6orzffJ1nocd0wle5lcNg8ihgsGSoqgMcR32ZU3l+MNE8qDcLi
lV2OaW0alou2K3rd3pq8WOtZTgGGMOHaTEOBAXqgXZQbzguiXEe6ijoLBgwmzkgr
Hqw6JgGefQIDAQABo3wwejAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFMUZFOQcnWCL
2XW0xLCOqDhZ7wLiMB0GA1UdDgQWBBRgVXKxahXMIirYYm0846gHnJeY5jAOBgNV
HQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMA0GCSqG
SIb3DQEBCwUAA4IBAQBcRs+X0iH/K5dOTQkZ5/v13huLOXb3QFExTJW2+tKmRYe1
e3CoQVE6LQJ+cCQ+Tl6UmbyeyTIqEuFpTmm6Yhl9agu41tlgiobP1+YQ/VMjasgh
Vhgr34KA09iVzpLsIlROdNW5Q5rfjRh1WuBAEPcABKKNFgaqVvS2BKzd/a6aXaId
Zu+YuRV232OXOUZP07DkaLhax6wfSf+tfkNLQLvoVNcJmcF6mNgzg2HULuvia77u
HPpykW02IbnSAr3jDVGHezVNLctFrpHpCWRlTUMulW56xc74ZiONSf+N/2WZJo0x
wNakFhJRZGZJpKCx8xrIO4D9y7jt0WGP7ZCvcXeQ
-----END CERTIFICATE-----";
#endif

		public void TestNoExceptionsWhenBusinessEntityIsNull()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "123";
			using (var form = new IncidentFormWithNullEntity(incident))
			{
				(form as KForm).DataSource = null;
				AssertNoExceptionThrown(delegate
				{
					form.Show();
				});
			}
		}

		class IncidentFormWithNullEntity : SupportIncidentForm
		{
			public IncidentFormWithNullEntity(SupportIncident supportIncident) : base(supportIncident)
			{
			}

			public new SupportIncident BusinessEntity
			{
				get
				{
					return null;
				}
			}
		}

		public void TestShouldAllowSourceModuleOverride()
		{
			var incident = Factory.New<SupportIncident>();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_Product = "";
				incident.IM_Priority = "";
				AssertEquals(false, form.OverrideSourceModuleButton.Visible);
				incident.IM_Product = "ENT";
				AssertEquals(false, form.OverrideSourceModuleButton.Visible);
				incident.IM_Priority = "CR2";
				AssertEquals(true, form.OverrideSourceModuleButton.Visible);
				incident.IM_Priority = "CR7";
				AssertEquals(true, form.OverrideSourceModuleButton.Visible);
				incident.IM_Priority = "CR8";
				AssertEquals(true, form.OverrideSourceModuleButton.Visible);
				incident.IM_Priority = "CR9";
				AssertEquals(true, form.OverrideSourceModuleButton.Visible);
				incident.IM_Product = "SPH";
				AssertEquals(true, form.OverrideSourceModuleButton.Visible);
			}
		}

		public void TestSetCompanyControlsVisibility()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			var database1 = Factory.New<LicenceDatabase>();
			database1.LD_ServerCode = "SRV";
			database1.LD_LE = enterprise.PK;
			database1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = database1.PK;
			clientCompany.LCC_Code = "MEO";
			clientCompany.LCC_OH = org.PK;
			var database2 = Factory.New<LicenceDatabase>();
			database2.LD_ServerCode = "TLX";
			database2.LD_LE = enterprise.PK;
			database2.LD_Product = "SPH";
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertEquals("Client company drop box must be visible if no database is selected", true, form.SupportCompanyDropEdit.Visible);
				incident.IM_LD = database2.PK;
				AssertEquals("Client company drop box must be not visible if database product is not ENT/CW1", false, form.SupportCompanyDropEdit.Visible);
				incident.IM_LD = database1.PK;
				incident.IM_LCC = clientCompany.PK;
				AssertEquals("Client company drop box must be visible if database product is ENT/CW1", true, form.SupportCompanyDropEdit.Visible);
				Factory.Save();
				incident.IM_Product = "WIW";
				AssertEquals("Client company drop box must be visible for other products if incident client company is in database", true, form.SupportCompanyDropEdit.Visible);
			}
		}

		public void TestIncidentDetailsAreUpdatedIfThereIsNoMenuSelected()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			productAreas.AddPair("PA3", "Product Area 3");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			var moduleMapping = product.ModuleMappings.AddNew("YYY", "YYY Description", "PA2", true);
			moduleMapping.SourceModuleMappings.AddNew("SourceModule1", "PA1");
			moduleMapping.SourceModuleMappings.AddNew("SourceModule2", "PA2");
			product.ModuleMappings.AddNew("ZZZ", "XXX Description", "PA3", true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "Menu Item A", "", ModuleListType.Cr9, "PA1", true, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var moduleList = collection.GetModuleList("ENT", "PA1");

			var incident = Factory.New<SupportIncident>();
			using (ZFormModaliser.SuspendDispose())
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				incident.IM_Module = "YYY";
				incident.ProductArea = "PA2";
				AssertNull("Should not show SourceModuleFinderPopup because Product Area is correct", ZFormModaliser.LastFormShownDialogForTest);
				incident.ProductArea = "PA1";
				AssertNotNull("Should show SourceModuleFinderPopup because module shared in multiple product areas", ZFormModaliser.LastFormShownDialogForTest);
				using (var shownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(SourceModuleFinderForm), shownForm);
					var sourceModuleFinderForm = (SourceModuleFinderForm)shownForm;
					var sourceModuleFinder = (SourceModuleFinder)sourceModuleFinderForm.LastDataSourceForTest;
					AssertNotNull("sourceModuleFinder", sourceModuleFinder);
					sourceModuleFinder.ProductAreaFilter = "PA3";
					sourceModuleFinder.ModuleFilter = "ZZZ";
					sourceModuleFinderForm.OkButton_Click(this, EventArgs.Empty);
					AssertEquals("ZZZ", incident.IM_Module);
					AssertEquals("PA3", incident.ProductArea);
				}
			}
		}

		public void TestShowSourceModuleFinderPopupIfRequired()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var xxxMapping = product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			xxxMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var yyyMapping = product.ModuleMappings.AddNew("YYY", "YYY Description", "PA1", true);
			yyyMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "Menu Item A", "", ModuleListType.MenuSection, "PA1", true, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var incident = Factory.New<SupportIncident>();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
				incident.IM_Module = "XXX";
				incident.ProductArea = "PA1";
				AssertNull("Should not show SourceModuleFinderPopup because Product Area is correct", ZFormModaliser.LastFormShownDialogForTest);
				incident.ProductArea = "PA2";
				AssertNotNull("Should show SourceModuleFinderPopup because Product Area is incorrect", ZFormModaliser.LastFormShownDialogForTest);
				using (var shownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(SourceModuleFinderForm), shownForm);
					var sourceModuleFinderForm = (SourceModuleFinderForm)shownForm;
					var sourceModuleFinder = (SourceModuleFinder)sourceModuleFinderForm.LastDataSourceForTest;
					AssertNotNull("sourceModuleFinder", sourceModuleFinder);
					CombineAssertions("SourceModuleFinder properties should be correct", () =>
					{
						AssertEquals("ProductCode", ProductTypes.Codes.Enterprise, sourceModuleFinder.ProductCode);
						AssertEquals("ModuleType", ModuleListType.MenuSection, sourceModuleFinder.ModuleType);
						AssertMultilineASCIIEquals("FindReason", @"You have selected a 'Menu Section' that is shared by multiple Product Areas.
The current Menu Item does not belong to the currently selected Product Area (Product Area 2), please select the correct Menu Item to determine the correct Product Area.", sourceModuleFinder.FindReason);
						AssertEquals("ProductAreaFilter", "PA2", sourceModuleFinder.ProductAreaFilter);
						AssertEquals("ModuleFilter", "XXX", sourceModuleFinder.ModuleFilter);
					});
				}

				ZFormModaliser.LastFormShownDialogForTest = null;
				incident.ProductArea = "";
				AssertNull("Should not show SourceModuleFInderPopup when Product Area is empty", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestShowSourceModuleFinderPopupIfRequired_ShouldNotShowIfSettingProductAreaToMatchTriageNode()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("CCC", "Carrot");
			productAreasList.AddPair("PA1", "Product Area 1");
			productAreasList.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			var redModuleMapping = product.ModuleMappings.AddNew("RED", "Red", "PA1", isModuleReadOnly: false);
			redModuleMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "Menu Item A", "", ModuleListType.MenuSection, "PA1", isSelectableForOverride: true, isSearchable: true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "CCC";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = "CR4";
			incident.IM_Module = "RED";

			Factory.Save();
			AssertNoErrors("Precondition", incident.IM_ProgramAreaInfo);
			AssertNoErrors("Precondition", incident.IM_ModuleInfo);

			incident.IM_IMT_Triage = triage.PK;
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.ProductArea = triage.IMT_ProductArea;
				AssertNull("Should not show SourceModuleFinderPopup because Product Area matches triage", ZFormModaliser.LastFormShownDialogForTest);

				incident.ProductArea = "PA2";
				AssertNotNull("Should show SourceModuleFinderPopup because Product Area does not match the triage and there's no selected menu item", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestEConversationSplitterReset()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithERequestV2();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				SplitContainer splitContainer = form.GetPersistedSplitContainer();
				int defaultDistance = splitContainer.SplitterDistance;
				int newDistance = defaultDistance + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				splitContainer.SplitterDistance = newDistance;
				AssertEquals("Check new splitter distance", newDistance, splitContainer.SplitterDistance);
				GetActionsMenuItem(form, "Reset Form Size and Layout to Default").PerformClick();
				AssertEquals("Check splitter distance is default", defaultDistance, splitContainer.SplitterDistance);
			}
		}

		public void TestInvalidXmlCharactersAreRemovedFromConversationMessageTextBox()
		{
			var incident = Factory.New<SupportIncident>();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.ConversationMessageTextBox.Text = string.Format("Hello {0}Richard", (char)0x02);
				AssertEquals("Hello Richard", form.ConversationMessageTextBox.Text);
				form.ConversationMessageTextBox.AppendText((char)0xDFFF + " Smith");
				AssertEquals("Hello Richard Smith", form.ConversationMessageTextBox.Text);
				AssertEquals("Hello Richard Smith".Length, form.ConversationMessageTextBox.SelectionStart);
			}
		}

		public void TestCriticalityChangeAndStageChange()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR5";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				incident.IM_Priority = "CR4";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				incident.IM_Priority = "CR6";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				incident.IM_Priority = "CR4";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				incident.IM_Priority = "CR7";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				incident.IM_Priority = "CR4";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				incident.IM_Priority = "CR9";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				incident.IM_Priority = "CR4";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
		public void TestCriticalityChangeAndStageNotChange()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR5";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				incident.IM_Priority = "CR4";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);

				incident.IM_Priority = "CR3";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);

				incident.IM_Priority = "CR8";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);

				incident.IM_Priority = "CR3";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);

				incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
				incident.IM_Priority = "CR4";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestCriticalityChangeCreatesPopupWithCorrectCriticalities()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR5";
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				incident.IM_Priority = "CR4";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);
				incident.IM_Priority = "CR3";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);
				incident.SetCriticalityWithoutLoggingReason("CR2");
				incident.IM_Priority = "CR1";
				AssertEquals("Should not show popup when stage not changed", null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestCriticalityChangeWillOpenClosureFormIfSelectionNotValidForNewCriticality()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR3";
			incident.IM_Status = "CLS";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = "TXT";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				incident.IM_Priority = "CR5";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertEquals("Should popup when selection not valid", typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestClosureForm_Cancel_ShouldRevertERequestStatusAndClosureResolution()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR3";
			incident.IM_Status = "CLS";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = DispositionList.Constants.Working.WorkInProgress;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			Factory.Save();
			AssertEquals("Precondition", DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			AssertEquals("Precondition", string.Empty, incident.IM_ClosureResolution);

			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				AssertEquals("Precondition: Should show close incident popup", null, ZFormModaliser.LastFormShownDialogForTest);

				var task = incident.WorkflowItems.AddNew();
				task.P9_Description = "Task 1";
				task.P9_Type = "UDF";
				task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				AssertEquals("Precondition: Should show close incident popup", typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should have reverted to non-closed", DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);
				AssertEquals("Should not be set", string.Empty, incident.IM_ClosureResolution);
			}
		}

		[ExpectNoExceptions]
		public void TestMouseLeaveEvents()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR5";
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.SetDataBinding(null, "");
				form.WorkOnCurrentTaskButton_MouseLeave(null, null);
				form.SuspendCurrentTaskButton_MouseLeave(null, null);
			}
		}

		public void TestCriticalityChangeCreatesPopupWithCorrectCriticalitiesForInvalidCriticality()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR6";
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				incident.IM_Priority = "AAA";
				AssertEquals("Should not show popup when criticality is invalid", null, ZFormModaliser.LastFormShownDialogForTest);
				incident.IM_Priority = "CR1";
				AssertEquals("ZFormModaliser.LastFormShownDialogForTest.GetType()", typeof(EscalateIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var escalateForm = ZFormModaliser.LastFormShownDialogForTest as EscalateIncidentPopupForm;
				var escalateAction = escalateForm.LastDataSourceForTest as SupportIncidentEscalateAction;
				AssertEquals("escalateAction.EscalationStage", "SUP", escalateAction.EscalationStage);
				AssertEquals("escalateAction.EscalationCriticality", "CR1", escalateAction.EscalationCriticality);
				incident.IM_Priority = "CR7";
				AssertEquals("Should not show popup when stage not changed", escalateForm, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestConversationMessageTextBoxMaxLength()
		{
			using (MockSupportIncidentForm form = new MockSupportIncidentForm(Factory.New<SupportIncident>()))
			{
				form.Show();
				AssertEquals("Max length", 32000, form.GetConversationMessageTextBox().MaxLength);
			}
		}

		public void TestContractStatus()
		{
			OrgHeader orgCreditOnHold = Factory.NewWithValidTestData<OrgHeader>();
			orgCreditOnHold.CompanyData.OB_AROnCreditHold = true;
			OrgHeader orgSTD = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org24H = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgNOS = Factory.NewWithValidTestData<OrgHeader>();
			LicenceHeader licenceSTD = SetupLicenceForOrg(orgSTD, LicenceHeaderLookups.SupportModeConstants.Codes.Standard);
			LicenceHeader licence24H = SetupLicenceForOrg(org24H, LicenceHeaderLookups.SupportModeConstants.Codes.Hour24);
			LicenceHeader licenceNOS = SetupLicenceForOrg(orgNOS, LicenceHeaderLookups.SupportModeConstants.Codes.NoSupport);
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_OH_Client = orgCreditOnHold.PK;
				AssertEquals("Contract invalid - credit on hold", Color.Red, form.ContractStatusLabel.ForeColor);
				incident.IM_LD = licenceSTD.LA_LD;
				incident.IM_LCC = licenceSTD.ClientCompany.PK;
				incident.IM_OH_Client = orgSTD.PK;
				AssertEquals("Licence with standard support ", Color.Green, form.ContractStatusLabel.ForeColor);
				incident.IM_LD = licence24H.LA_LD;
				incident.IM_LCC = licence24H.ClientCompany.PK;
				incident.IM_OH_Client = org24H.PK;
				AssertEquals("Licence with 24/7 support", Color.Blue, form.ContractStatusLabel.ForeColor);
				incident.IM_LD = licenceNOS.LA_LD;
				incident.IM_LCC = licenceNOS.ClientCompany.PK;
				incident.IM_OH_Client = orgNOS.PK;
				AssertEquals("Licence with no support", Color.Red, form.ContractStatusLabel.ForeColor);
				incident.IM_LD = licence24H.LA_LD;
				incident.IM_LCC = licence24H.ClientCompany.PK;
				incident.IM_OH_Client = org24H.PK;
				Factory.Save();
				AssertEquals("New form with 24/7 support", Color.Blue, form.ContractStatusLabel.ForeColor);

				incident.IM_LD = licenceSTD.LA_LD;
				AssertEquals("Licence with no support", Color.Red, form.ContractStatusLabel.ForeColor);
			}
		}

		public void TestLastCreatedIncidentsPopupDialog()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "Demo Company";
			OrgHeader parent = Factory.NewWithValidTestData<OrgHeader>();
			parent.OH_FullName = "Parent Company";
			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_OH = parent.PK;
			LicenceCompany company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_LE = enterprise1.PK;
			company1.LC_OH = client.PK;
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "CS00054601";
			incident1.IM_Description = "Test Incident 546";
			incident1.IM_OH_Client = client.PK;
			Factory.Save();
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			using (SupportIncidentForm form = new SupportIncidentForm(incident2))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				incident2.IM_OH_Client = client.PK;
				AssertContains("Last ten incidents created for", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Parent Company", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("CS00054601", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Test Incident 546", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (SupportIncidentForm form = new SupportIncidentForm(incident2))
			{
				form.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (SupportIncidentForm form = new SupportIncidentForm(incident3))
			{
				form.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var eventTemplateOrg = Factory.New<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eventTemplateOrg.PK.ToGuid());
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			enterprise1.LE_OH = ZGuid.Empty;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (SupportIncidentForm form = new SupportIncidentForm(incident4))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				incident4.IM_OH_Client = client.PK;
				AssertContains("Last ten incidents created for", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Demo Company", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				Assert("Pre-condition: last message is cleared", UnitTestUserNotification.Instance.LastMessage.WasNone);
				incident4.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
				Assert("Last incident popup should not show again", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#region ActionMenuItems

		public void TestActionMenuItems_MuteAllOutboundEmailNotifications()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var jobHeader = ProcessJobHeader.GetForParent(incident, incident.Factory);
			AssertNotNull(jobHeader);
			Assert(!jobHeader.Tags.Any());
			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";

			var magnitude = blnGroup.Magnitudes.AddNew();
			magnitude.TGM_Code = SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification;
			magnitude.TGM_IsActive = false;
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Mute all outbound email notifications");
				AssertNotNull(menuItem);
				Assert(!menuItem.Checked);

				menuItem.PerformClick();
				Assert(!menuItem.Checked);
				AssertEquals("Failed to update the email notification suppression status. Please check if the BLN tag group has ALL code enabled.", UnitTestUserNotification.Instance.LastMessage.Text);

				magnitude.TGM_IsActive = true;
				Factory.Save();

				menuItem.PerformClick();

				Assert("Should not post message before saving", !incident.EConversation.GetNewLocalMessages().Any(x => x.Body == "We have disabled outbound email notifications for this eRequest."));
				Factory.Save();
				Assert(menuItem.Checked);
				var allTagLink = jobHeader.Tags.Single(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
				AssertNotNull(allTagLink);
				Assert("An internal message should be posted", incident.EConversation.ExistingConversation.Messages.Any(x => x.Body == "We have disabled outbound email notifications for this eRequest."));
				AssertEquals("This eRequest has mute mode enabled due to potential email loop detection. All outbound notifications will be blocked.", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				var email = SupportIncidentEmail.New(incident, new SupportIncidentWorkItemCreatedEmailContentBuilder(incident, true, "ENT"), out var allowed);
				Assert(!allowed);
				AssertNull(email);

				menuItem.PerformClick();

				Assert("Should not post message before saving", !incident.EConversation.GetNewLocalMessages().Any(x => x.Body == "We have re-enabled outbound email notifications for this eRequest."));
				Factory.Save();
				Assert(!menuItem.Checked);
				Assert("ALL tag should be removed", !jobHeader.Tags.Any(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification));
				Assert("An internal message should be posted", incident.EConversation.ExistingConversation.Messages.Any(x => x.Body == "We have re-enabled outbound email notifications for this eRequest."));

				var email2 = SupportIncidentEmail.New(incident, new SupportIncidentWorkItemCreatedEmailContentBuilder(incident, true, "ENT"), out var allowed2);
				Assert(allowed2);
				AssertNotNull(email2);
			}
		}

		public void TestActionMenuItems_SetERequestStatusToUPOAndUDO_AttachWorkItem()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;

			using (var form = new SupportIncidentForm(incident))
			{
				incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
				AssertEquals(0, incident.RelatedWorkItems.Count);

				form.Show();
				var upoItem = GetActionsMenuItem(form, "&Set eRequest Status to Awaiting Auto Upgrade Deployment", true);
				var udoItem = GetActionsMenuItem(form, "&Set eRequest Status to Upgrade Delayed", true);

				AssertNotNull(upoItem);
				AssertNotNull(udoItem);

				Assert("UPO item should not show if incident has no attached work item", !upoItem.Enabled);
				Assert("UDO item should not show if incident has no attached work item", !udoItem.Enabled);

				incident.RelatedWorkItems.Add(workItem);
				AssertEquals(1, incident.RelatedWorkItems.Count);
				Assert(upoItem.Enabled);
				Assert(udoItem.Enabled);
			}
		}

		public void TestActionMenuItems_SetERequestStatusToUPOAndUDO()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.RelatedWorkItems.Add(workItem);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var upoItem = GetActionsMenuItem(form, "&Set eRequest Status to Awaiting Auto Upgrade Deployment", true);
				var udoItem = GetActionsMenuItem(form, "&Set eRequest Status to Upgrade Delayed", true);

				AssertNotNull(upoItem);
				AssertNotNull(udoItem);

				Assert("UPO item should not show if incident's stage is support", !upoItem.Enabled);
				Assert("UDO item should not show if incident's stage is support", !udoItem.Enabled);

				incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
				Assert("UPO item should show if incident's stage is defect", upoItem.Enabled);
				Assert("UDO item should show if incident's stage is defect", udoItem.Enabled);

				upoItem.PerformClick();
				AssertEquals(DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
				Factory.Save();
				AssertEquals("ResolutionCode should not change after saving", DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);

				udoItem.PerformClick();
				AssertEquals(DispositionList.Constants.Closed.UpgradeDelayed, incident.IM_ResolutionCode);
				Factory.Save();
				AssertEquals("ResolutionCode should not change after saving", DispositionList.Constants.Closed.UpgradeDelayed, incident.IM_ResolutionCode);

				incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
				Assert("UPO item should show if incident's stage is feature request", upoItem.Enabled);
				Assert("UDO item should show if incident's stage is feature request", !udoItem.Enabled);

				upoItem.PerformClick();
				AssertEquals(DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
				Factory.Save();
				AssertEquals("ResolutionCode should not change after saving", DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			}
		}

		public void TestActionsMenuItems_DefaultMenuItemsStillVisible()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertNotNull("ActionsMenuItem: Copy Form to Clipboard", GetActionsMenuItem(form, "Copy Form to Clipboard"));
				AssertNotNull("ActionsMenuItem: Reset Form Size and Layout to Default", GetActionsMenuItem(form, "Reset Form Size and Layout to Default"));
			}
		}

		public void TestActionsMenuItemsValidation()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_OA_BranchAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			incident.IM_OC_Contact = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;
			Factory.Save();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_Status = SupportIncidentLookups.Status.Closed;
				GetActionsMenuItem(form, "Working / &Investigating").PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("This incident is escalated to other stages. If you want to change details, please escalated the incident back to support first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSupportActionsMenuItems()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			incident.IM_OC_Contact = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;
			var sender = new EmailOnlyCustomerNotificationSender();
			incident.CustomerNotifier = new InteractiveIncidentCustomerNotifier(incident, sender);
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var item = GetActionsMenu(form);
				AssertVisibleMenuItems(form, 31);
				GetActionsMenuItem(form, "&Assign To").PerformClick();
				AssertEquals("Assign Incident", ZFormModaliser.LastFormShownDialogForTest.Text);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
				GetActionsMenuItem(form, "Working / &Investigating").PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				incident.CloseIncident("", "");
				incident.IM_GS_NKAssignedToCurrent = staff2.GS_Code;
				var reopenMenuItem = GetActionsMenuItem(form, "&Re-Open");
				Assert(!reopenMenuItem.Enabled);
				incident.IM_Status = SupportIncidentLookups.Status.Open;
				GetActionsMenuItem(form, "&Close").PerformClick();
				AssertEquals("Close Incident", ZFormModaliser.LastFormShownDialogForTest.Text);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
				incident.IM_Status = SupportIncidentLookups.Status.Open;
				Factory.Save();
				var resendNotification = GetActionsMenuItem(form, "Re-send Email &Notification");
				AssertEquals(Shortcut.CtrlShiftN, resendNotification.Shortcut);
				Assert(resendNotification.ShowShortcut);
				resendNotification.PerformClick();
				AssertEquals(typeof(CustomerServiceEmailForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertContains("Notification of Incident:", ((SupportIncidentEmail)ZFormModaliser.LastIBusinessShownOnDialogForTest).Subject);
			}
		}

		public void TestFeatureRequestActionsMenuItems()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertNotNull(GetActionsMenuItem(form, "Send Software Estimate"));
				AssertNull(GetActionsMenuItem(form, "Re-Send Software Estimate"));
				AssertNull(GetActionsMenuItem(form, "Re-issue Software Estimate Request"));
			}

			incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, "");
			incident.Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertNull(GetActionsMenuItem(form, "Send Software Estimate"));
				AssertNotNull(GetActionsMenuItem(form, "Re-Send Software Estimate"));
				AssertNotNull(GetActionsMenuItem(form, "Re-issue Software Estimate Request"));
				AssertNotNull(GetActionsMenuItem(form, "Send Software Quote"));
				AssertNull(GetActionsMenuItem(form, "Re-Send Software Quote"));
				AssertNull(GetActionsMenuItem(form, "Re-issue Software Quote Request"));
			}

			incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "");
			incident.Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertNotNull(GetActionsMenuItem(form, "Send Software Estimate"));
				AssertNull(GetActionsMenuItem(form, "Re-Send Software Estimate"));
				AssertNotNull(GetActionsMenuItem(form, "Re-issue Software Estimate Request"));
				AssertNull(GetActionsMenuItem(form, "Send Software Quote"));
				AssertNotNull(GetActionsMenuItem(form, "Re-Send Software Quote"));
				AssertNotNull(GetActionsMenuItem(form, "Re-issue Software Quote Request"));
			}
		}

		public void TestReOpenMenuItemShouldBeEnabledWhenERequsetInSomeStatus()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.IM_Description = "Test Incident";
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BUF");
			var bMSystem = Factory.NewWithValidTestData<BMSystem>();
			var bMSystemWorkflowDeterminer = Factory.NewWithValidTestData<BMSystemWorkflowDeterminer>();
			bMSystemWorkflowDeterminer.FSW_WorkflowType = incident.WorkflowType;
			bMSystemWorkflowDeterminer.FSW_FS_System = bMSystem.PK;
			bMSystemWorkflowDeterminer.FSW_IsActive = true;

			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty);
			Factory.Save();

			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			AssertEquals(LegacyStatusCodes.Closed, incident.IM_Status);

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "test reopen menu task";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, incident.CurrentTask.P9_Status);

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "INC";
			var header1 = workflowTemplate.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "RCW-AXT";
			var templateTask = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			templateTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			templateTask.P9_Description = "template task";
			templateTask.P9_FH_ProcessHeader = header1.PK;
			workflowTemplate.Factory.Save();
			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { workflowTemplate }));
			Factory.Save();

			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert("this incident's CanReOpen should be true", incident.CanReOpen);
				var reOpenMenuItem = GetActionsMenuItem(form, "&Re-Open");
				Assert("the Re-Open menu item should be enabled", reOpenMenuItem.Enabled);
				form.ReopenIncidentPopup_DefaultAnswer = ReopenIncidentAction.AssignToSelf;
				reOpenMenuItem.PerformClick();
				form.FireSaveButton();
			}

			Factory.Save();
			var incident2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident2.IM_Status);
			AssertNotEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			var task = incident2.WorkflowItems.Cast<ProcessTask>().First(t => t.P9_Description == "test reopen menu task");
			Assert(task.IsClosed);
			Assert(incident2.WorkflowItems.Cast<ProcessTask>().Any(t => t.IsOpen));
		}

		public void TestReOpenButton_AssignToSelf()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "New Staff";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.IM_Description = "Test Incident";
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BUF");
			var bMSystem = Factory.NewWithValidTestData<BMSystem>();
			var bMSystemWorkflowDeterminer = Factory.NewWithValidTestData<BMSystemWorkflowDeterminer>();
			bMSystemWorkflowDeterminer.FSW_WorkflowType = incident.WorkflowType;
			bMSystemWorkflowDeterminer.FSW_FS_System = bMSystem.PK;
			bMSystemWorkflowDeterminer.FSW_IsActive = true;

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "test menu task";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "INC";
			var header1 = workflowTemplate.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "RCW-AXT";
			var templateTask = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			templateTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			templateTask.P9_Description = "template task";
			templateTask.P9_FH_ProcessHeader = header1.PK;
			workflowTemplate.Factory.Save();
			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates([workflowTemplate]));
			Factory.Save();

			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty);
			Factory.Save();

			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			AssertEquals(LegacyStatusCodes.Closed, incident.IM_Status);

			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert("this incident's CanReOpen should be true", incident.CanReOpen);
				var reOpenMenuItem = GetActionsMenuItem(form, "&Re-Open");
				Assert("the Re-Open menu item should be enabled", reOpenMenuItem.Enabled);
				form.ReopenIncidentPopup_DefaultAnswer = ReopenIncidentAction.AssignToSelf;
				reOpenMenuItem.PerformClick();
				form.FireSaveButton();
			}

			Factory.Save();
			var loadedIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, loadedIncident.IM_Status);
			AssertNotEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			var createdTask = loadedIncident.WorkflowItems.Cast<ProcessTask>().First(t => t.P9_Description == "template task" && t.IsOpen);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, createdTask.P9_GS_NKAssignedStaffMember);
		}

		public void TestReOpenButton_AssignToCapability()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "New Staff";

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Capability desc";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.IM_Description = "Test Incident";
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BUF");
			var bMSystem = Factory.NewWithValidTestData<BMSystem>();
			var bMSystemWorkflowDeterminer = Factory.NewWithValidTestData<BMSystemWorkflowDeterminer>();
			bMSystemWorkflowDeterminer.FSW_WorkflowType = incident.WorkflowType;
			bMSystemWorkflowDeterminer.FSW_FS_System = bMSystem.PK;
			bMSystemWorkflowDeterminer.FSW_IsActive = true;

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "test menu task";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "INC";
			var header1 = workflowTemplate.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "RCW-AXT";
			var templateTask = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			templateTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			templateTask.P9_Description = "template task";
			templateTask.P9_FH_ProcessHeader = header1.PK;
			templateTask.P9_G4_RequiredCapability = capability.PK;
			workflowTemplate.Factory.Save();
			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates([workflowTemplate]));
			Factory.Save();

			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty);
			Factory.Save();

			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			AssertEquals(LegacyStatusCodes.Closed, incident.IM_Status);

			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert("this incident's CanReOpen should be true", incident.CanReOpen);
				var reOpenMenuItem = GetActionsMenuItem(form, "&Re-Open");
				Assert("the Re-Open menu item should be enabled", reOpenMenuItem.Enabled);
				form.ReopenIncidentPopup_DefaultAnswer = ReopenIncidentAction.AssignToSelf;
				reOpenMenuItem.PerformClick();
				form.FireSaveButton();
			}

			Factory.Save();
			var loadedIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, loadedIncident.IM_Status);
			AssertNotEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			var createdTask = loadedIncident.WorkflowItems.Cast<ProcessTask>().First(t => t.P9_Description == "template task" && t.IsOpen);
			AssertEquals("Capability desc", createdTask.CapabilityName);
		}

		public void TestShouldNotReOpenWhenHasValidationErrors()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "New Staff";

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Capability desc";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.IM_Description = "Test Incident";
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BUF");
			var bMSystem = Factory.NewWithValidTestData<BMSystem>();
			var bMSystemWorkflowDeterminer = Factory.NewWithValidTestData<BMSystemWorkflowDeterminer>();
			bMSystemWorkflowDeterminer.FSW_WorkflowType = incident.WorkflowType;
			bMSystemWorkflowDeterminer.FSW_FS_System = bMSystem.PK;
			bMSystemWorkflowDeterminer.FSW_IsActive = true;

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "test menu task";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Sequence = 1;

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "INC";
			var header1 = workflowTemplate.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "RCW-AXT";
			var templateTask = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			templateTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			templateTask.P9_Description = "template task";
			templateTask.P9_FH_ProcessHeader = header1.PK;
			templateTask.P9_G4_RequiredCapability = capability.PK;
			workflowTemplate.Factory.Save();
			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates([workflowTemplate]));
			Factory.Save();

			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty);
			Factory.Save();

			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			AssertEquals(LegacyStatusCodes.Closed, incident.IM_Status);

			incident.IM_Source = "";
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert("this incident's CanReOpen should be true", incident.CanReOpen);
				var reOpenMenuItem = GetActionsMenuItem(form, "&Re-Open");
				Assert("the Re-Open menu item should be enabled", reOpenMenuItem.Enabled);
				form.ReopenIncidentPopup_DefaultAnswer = ReopenIncidentAction.AssignToSelf;
				reOpenMenuItem.PerformClick();
				Assert(incident.HasErrors());
				AssertEquals("Expected Message is incorrect", "Please enter a Source.", incident.IM_SourceInfo.GetErrors().GetFirstMessage());
				form.FireSaveButton();
			}

			Factory.Save();
			var loadedIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			AssertEquals("IM_Status should remain CLS since there were validation errors.", SupportIncidentLookups.Status.Closed, loadedIncident.IM_Status);
			AssertEquals("IM_RequestStatus should remain CWR since there were validation errors.", DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_RequestStatus);
			var hasOpenTask = loadedIncident.WorkflowItems.Cast<ProcessTask>().Any(t => t.IsOpen);
			Assert(!hasOpenTask);
		}

		public void TestRevertToAwaitingResponseMenuItem()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, string.Empty);
			Factory.Save();
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			AssertNotEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertNotEquals(DispositionList.Constants.Closed.SelfResolved, incident.IM_ResolutionCode);
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanRevertToAwaitingResponse should be false", !incident.CanRevertToAwaitingResponse);
				var actionMenu1 = GetActionsMenuItem(form, "Revert to Awaiting Response");
				AssertNull(actionMenu1);
				//var revertToClosedMenu = GetActionsMenuItem(form, "Revert to Closed/Resolved"); -> Hidden for Clarity trial period, to be reverted at a later date
				//AssertNotNull(revertToClosedMenu);
			}

			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty, true);
			Factory.Save();
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanRevertToAwaitingResponse should be true", incident.CanRevertToAwaitingResponse);
				var actionMenu2 = GetActionsMenuItem(form, "Revert to Awaiting Response");
				AssertNotNull(actionMenu2);
				//var revertToClosedMenu = GetActionsMenuItem(form, "Revert to Closed/Resolved"); -> Hidden for Clarity trial period, to be reverted at a later date
				//AssertNull(revertToClosedMenu);
				actionMenu2.PerformClick();
				AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				AssertEquals(true, incident.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "Client response is not genuine. Revert status to Awaiting Response."));
				var latestEConversationMessage = incident.EConversation.LastAddedMessageForTest;
				AssertEquals(true, latestEConversationMessage.Body == "Awaiting Client Response" && latestEConversationMessage.JCM_IsSystem);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestCanRevertToAwaitingResponse_WhenShowIncidentForm()
		{
			#region Creating DataSource

			var factory = new BusinessObjectFactory(Db.NewExtraConnectionToMainDb());
			var client = factory.NewWithValidTestData<EDIOrgHeader>();
			factory.Save();
			client.MainAddress.OA_Address1 = "Colin Street";
			client.OH_Code = "ORC131";
			factory.Save();
			var enterprise = factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "AD1";
			enterprise.LE_OH = client.PK;
			var build = factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = factory.New<LicenceDatabase>();
			database.LD_ServerCode = "CC8";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = factory.New<ClientCompany>();
			clientCompany.LCC_Code = "EE8";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			factory.Save();

			var incident = factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_IncidentNumber = Guid.NewGuid().ToString().Substring(0, 19);
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.ClientCompanyCode = clientCompany.LCC_Code;

			#endregion

			var flows = incident.Workflows.AddNew();
			flows.FH_WorkflowType = "WKI";

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Sequence = 1;
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_FH_ProcessHeader = flows.PK;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Sequence = 2;
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = "~BP";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_FH_ProcessHeader = flows.PK;

			incident.Factory.Save();
			AssertEquals(DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			try
			{
				var incident1 = (new BusinessObjectFactory(Db.NewExtraConnectionToMainDb()) { RefreshEnabled = false }).LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
				using (var form = new SupportIncidentFormForTest(incident1))
				{
					form.Show();
					incident1.IM_Description = "Log Concurrency Error";
					form.FireSaveButton();
					AssertEquals(DispositionList.Constants.Open.AssignedAwaitingAction, incident1.IM_ResolutionCode);

					UnitTestUserNotification.Instance.ClearMessages();
					var incident2 = (new BusinessObjectFactory(Db.NewExtraConnectionToMainDb()) { RefreshEnabled = false }).LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
					using (var form2 = new SupportIncidentFormForTest(incident2))
					{
						form2.Show();

						incident1.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 2).P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
						form.FireSaveButton();
						AssertEquals(DispositionList.Constants.Open.AssignedAwaitingAction, incident1.IM_ResolutionCode);

						UnitTestUserNotification.Instance.ClearMessages();
						incident2.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
						incident2.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 2).P9_Status = ProcessTaskStatusCodeList.Codes.Working;

						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
						form2.FireSaveButton();
						AssertContains("After you click 'OK', the form will merge your changes with changes made by other user.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(incident2.Logs.Find(x => x.ReferenceFreeText.StartsWith($"Disposition - {DispositionList.Constants.Open.AssignedAwaitingAction} to {DispositionList.Constants.Suspended.Deferred}")).Count() == 1);

						UnitTestUserNotification.Instance.ClearMessages();
						incident2.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

						form2.FireSaveButton();
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}

				incident.Reload();
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					form.Close();
				}

				AssertNotContains("The value in the latest field changed log doesn't match the latest value", ErrorReporter.LastKeyReported);
			}
			finally
			{
				incident.Reload();
				incident.WorkflowItems.Reload(true);
				incident.Request.Reload();
				incident.Request.Delete();
				incident.Delete();

				contact.Delete();
				clientCompany.Delete();
				database.Delete();
				build.Delete();
				enterprise.Delete();
				client.Delete();

				factory.Save();
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestCanRevertToAwaitingResponse_WhenSetResolutionCode()
		{
			#region Creating DataSource

			var factory = new BusinessObjectFactory();
			var client = factory.NewWithValidTestData<EDIOrgHeader>();
			factory.Save();
			client.MainAddress.OA_Address1 = "Colin Street";
			client.OH_Code = "ORC131";
			factory.Save();
			var enterprise = factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "AD1";
			enterprise.LE_OH = client.PK;
			var build = factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = factory.New<LicenceDatabase>();
			database.LD_ServerCode = "CC8";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = factory.New<ClientCompany>();
			clientCompany.LCC_Code = "EE8";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			factory.Save();

			var incident = factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_IncidentNumber = Guid.NewGuid().ToString().Substring(0, 19);
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.ClientCompanyCode = clientCompany.LCC_Code;

			#endregion
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.Factory.Save();
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			var flows = incident.Workflows.AddNew();
			flows.FH_WorkflowType = "WKI";

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Sequence = 1;
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_FH_ProcessHeader = flows.PK;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Description = "Task 2";
			task2.P9_Sequence = 2;
			task2.P9_Type = "UDF";
			task2.P9_GS_NKAssignedStaffMember = "~BP";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_FH_ProcessHeader = flows.PK;

			incident.Factory.Save();
			AssertEquals(DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			try
			{
				var incident1 = (new BusinessObjectFactory() { RefreshEnabled = false }).LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
				using (var form = new SupportIncidentFormForTest(incident1))
				{
					form.Show();
					incident1.IM_Description = "Log Concurrency Error";
					form.FireSaveButton();
					AssertEquals(DispositionList.Constants.Open.AssignedAwaitingAction, incident1.IM_ResolutionCode);

					UnitTestUserNotification.Instance.ClearMessages();
					var incident2 = (new BusinessObjectFactory() { RefreshEnabled = false }).LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
					using (var form2 = new SupportIncidentFormForTest(incident2))
					{
						form2.Show();

						incident2.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Working;
						form2.FireSaveButton();
						AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident2.IM_ResolutionCode);

						incident1.Reload();
						AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident1.IM_ResolutionCode);
						incident1.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
						form.FireSaveButton();
						AssertEquals(DispositionList.Constants.Suspended.Deferred, incident1.IM_ResolutionCode);

						AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident2.IM_ResolutionCode);
						incident2.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
						form2.FireSaveButton();
						AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(incident2.Logs.Find(x => x.ReferenceFreeText.StartsWith($"Disposition - {DispositionList.Constants.Working.WorkInProgress} to {DispositionList.Constants.Open.AssignedAwaitingAction}")).Count() == 1);

						UnitTestUserNotification.Instance.ClearMessages();
						incident2.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Working;

						form2.FireSaveButton();
						AssertNotContains("The value in the latest field changed log doesn't match the latest value", ErrorReporter.LastKeyReported);
					}
				}
			}
			finally
			{
				incident.Reload();
				incident.WorkflowItems.Reload(true);
				incident.Request.Reload();
				incident.Request.Delete();
				incident.Delete();

				contact.Delete();
				clientCompany.Delete();
				database.Delete();
				build.Delete();
				enterprise.Delete();
				client.Delete();

				factory.Save();
				ErrorReporter.Instance.Clear();
			}
		}

		/*public void TestRevertToResolvedOrClosedMenuItem() -> Hidden for Clarity trial period, to be reverted at a later date
		{
			var closedDispositions = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("YYY", (NoResString)"YYYDescription", supportParent, false);
			var defectParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("XXX", (NoResString)"XXXDescription", defectParent, true);
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, closedDispositions);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = "ADD";
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Revert to Closed/Resolved");
				AssertNotNull(menuItem);

				Assert(!incident.IsInDatabase);
				Assert(menuItem.Enabled);
				AssertEquals(LoadingPreviousFieldValueResult.ParentCouldNotBeLoadedFromDatabase, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out var oldCode));
				menuItem.PerformClick();
				AssertEquals("Please save the form before reverting.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertNullOrEmpty("From _empty to ADD", oldCode);
				menuItem.PerformClick();
				AssertEquals("From _empty to ADD", "The operation has been canceled because the previous eRequest Status is not Closed/Resolved.", UnitTestUserNotification.Instance.LastMessage.Text);

				incident.IM_ResolutionCode = "WRK";
				Factory.Save();
				AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertEquals("From ADD to WRK", "ADD", oldCode);
				menuItem.PerformClick();
				AssertEquals("From ADD to WRK", "The operation has been canceled because the previous eRequest Status is not Closed/Resolved.", UnitTestUserNotification.Instance.LastMessage.Text);

				incident.CloseIncident("YYY", "No");
				Factory.Save();
				AssertEquals("CLS", incident.IM_ResolutionCode);
				AssertEquals("YYY", incident.IM_ClosureResolution);

				AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertEquals("From WRK to CLS", "WRK", oldCode);
				menuItem.PerformClick();
				AssertEquals("From WRK to CLS", "The operation has been canceled because the eRequest Status is already Closed/Resolved.", UnitTestUserNotification.Instance.LastMessage.Text);

				incident.IM_ResolutionCode = "ADD";
				AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertEquals("From CLS to ADD(Has Changes)", "CLS", oldCode);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();
				AssertEquals("From CLS to ADD(Has Changes)", "Do you wish to revert the eRequest Status to YYY - YYYDescription.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Incident should be reverted back to YYY", "CLS", incident.IM_ResolutionCode);
				AssertEquals("Incident should be reverted back to YYY", "YYY", incident.IM_ClosureResolution);

				incident.IM_ResolutionCode = "ADD";
				Factory.Save();
				AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertEquals("From CLS to ADD", "CLS", oldCode);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();
				AssertEquals("From CLS to ADD", "Do you wish to revert the eRequest Status to YYY - YYYDescription.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				AssertEquals("Incident should be reverted back to YYY", "CLS", incident.IM_ResolutionCode);
				AssertEquals("Incident should be reverted back to YYY", "YYY", incident.IM_ClosureResolution);

				incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "message");
				incident.IM_ResolutionCode = "ADD";
				Factory.Save();
				AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertEquals("From CLS to ADD", "CLS", oldCode);
				menuItem.PerformClick();
				AssertEquals("From CLS to ADD", "The operation has been canceled because the previous eRequest status cannot be applied to the current details.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRevertToResolvedOrClosedMenuItem_Concurrency() -> Hidden for Clarity trial period, to be reverted at a later date
		{
			var closedDispositions = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("YYY", (NoResString)"YYYDescription", supportParent, false);
			var defectParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("XXX", (NoResString)"XXXDescription", defectParent, true);
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, closedDispositions);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = "ADD";
			Factory.Save();

			AssertEquals(LoadingPreviousFieldValueResult.Success, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out var oldCode));
			AssertNullOrEmpty("From _empty to ADD", oldCode);

			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Revert to Closed/Resolved");
				AssertNotNull(menuItem);

				var incidentFromAnotherSession = (new BusinessObjectFactory() { RefreshEnabled = false }).LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
				incidentFromAnotherSession.IM_ResolutionCode = "ABC";
				incidentFromAnotherSession.Factory.Save();

				AssertEquals(LoadingPreviousFieldValueResult.ParentDataOutDated, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out oldCode));
				AssertEquals("ADD", oldCode);

				menuItem.PerformClick();
				AssertEquals("Changes by another user have been saved during your session. Please refresh the form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRevertToResolvedOrClosedMenuItem_AbnormalCase() -> Hidden for Clarity trial period, to be reverted at a later date
		{
			var closedDispositions = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("YYY", (NoResString)"YYYDescription", supportParent, false);
			var defectParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("XXX", (NoResString)"XXXDescription", defectParent, true);
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, closedDispositions);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = "ADD";
			Factory.Save();

			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Revert to Closed/Resolved");
				AssertNotNull(menuItem);
				incident.Logs.AddNew(AutoEvents.StatusChange, $"{SupportIncident.ChangedFieldDescription.IM_ResolutionCode} 2222 2222 2222");
				Factory.Save();
				AssertEquals(LoadingPreviousFieldValueResult.CannotParseLog, incident.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out _));

				menuItem.PerformClick();
				AssertEquals("Unable to revert as this Incident appears to be in an inconsistent state. An error report has been sent for review.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("A field changed log could not be parsed when getting previous value", ErrorReporter.LastKeyReported);
			}
			ErrorReporter.Instance.Clear();
		}*/

		public void TestCloseOnBehalfOfClientItem()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, string.Empty);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanCloseOnBehalfOfClient should be false", !incident.CanCloseOnBehalfOfClient);
				var actionMenu = GetActionsMenuItem(form, "Close on behalf of Client (Confirmed Resolved)");
				Assert(!actionMenu.Enabled);
			}

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanCloseOnBehalfOfClient should be true", incident.CanCloseOnBehalfOfClient);
				var actionMenu = GetActionsMenuItem(form, "Close on behalf of Client (Confirmed Resolved)");
				Assert(actionMenu.Enabled);
				actionMenu.PerformClick();
				AssertEquals(typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestCloseOnBehalfOfClientItem_CancelAction()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, string.Empty);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_ResolutionCode);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_Status);
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanCloseOnBehalfOfClient should be true", incident.CanCloseOnBehalfOfClient);
				var actionMenu = GetActionsMenuItem(form, "Close on behalf of Client (Confirmed Resolved)");
				Assert(actionMenu.Enabled);
				actionMenu.PerformClick();
				AssertEquals(typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_Status);
				AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseOnBehalfOfClientItem_PerformAction_PreviousCWR()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13, 12, 0, 0);
			incident.IM_Priority = "CR4";
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var logs = incident.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(0, logs.Count(log => log.SL_SE_NKEvent == "IRS"));
			AssertEquals(0, logs.Count(log => log.SL_SE_NKEvent == "MIS"));
			AssertEquals(0, logs.Count(log => log.SL_SE_NKEvent == "STC" && log.SL_Reference == "Disposition - WRK to CLS"));
			AssertEquals(false, logs.Any(x => x.SL_SE_NKEvent == "STU" && x.SL_Reference.Contains("|NEW=CLS|OLD=WRK")));

			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_Status);
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			var irsEventTimeUtcOverride = new ZDateTime(2024, 6, 13, 12, 0, 0);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
			{
				var closeIncidentForm = dialog as CloseIncidentPopupForm;
				if (closeIncidentForm != null)
				{
					var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
					action.PostIRSEvent = true;
					action.IRSEventTimeUtcOverride = irsEventTimeUtcOverride;
					action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
					action.SynchroniseToIncident();
				}
			});

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanCloseOnBehalfOfClient should be true", incident.CanCloseOnBehalfOfClient);
				var actionMenu = GetActionsMenuItem(form, "Close on behalf of Client (Confirmed Resolved)");
				Assert(actionMenu.Enabled);
				actionMenu.PerformClick();
				AssertEquals(typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				form.FireSaveButton();
			}

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadIncident = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals(1, irsLogs.Count());
			var irsLog = irsLogs.First();
			AssertEquals(irsEventTimeUtcOverride, irsLog.SL_EventTimeUtc);
			AssertEquals(1, loadIncidentLogs.Find(log => log.SL_SE_NKEvent == "MIS" && log.SL_Reference == "This eRequest has been deemed resolved").Count());
			AssertEquals(1, loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "STC" && x.SL_Reference == "Disposition - WRK to CLS").Count());
			AssertEquals(1, loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "STU" && x.SL_Reference.Contains("|NEW=CLS|OLD=WRK")).Count());

			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, loadIncident.IM_ClosureResolution);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseOnBehalfOfClientItem_PerformAction_PreviousCLS()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR4";
			incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, string.Empty);
			incident.Logs.AddNew(Events.IncidentClosed, Array.Empty<KeyValuePair<string, string>>());
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(IncidentClosureDisposition.ResolvedAndClosedCode, incident.IM_ResolutionCode);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var logs = incident.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(1, logs.Count(log => log.SL_SE_NKEvent == "IRS"));
			AssertEquals(0, logs.Count(log => log.SL_SE_NKEvent == "MIS"));
			AssertEquals(0, logs.Count(log => log.SL_SE_NKEvent == "STC" && log.SL_Reference == "Disposition - WRK to CLS"));
			AssertEquals(0, logs.Count(x => x.SL_SE_NKEvent == "STU" && x.SL_Reference.Contains("|NEW=CLS|OLD=WRK")));

			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_Status);
			AssertEquals(DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
			{
				var closeIncidentForm = dialog as CloseIncidentPopupForm;
				if (closeIncidentForm != null)
				{
					var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
					action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
					action.SynchroniseToIncident();
				}
			});

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanCloseOnBehalfOfClient should be true", incident.CanCloseOnBehalfOfClient);
				var actionMenu = GetActionsMenuItem(form, "Close on behalf of Client (Confirmed Resolved)");
				Assert(actionMenu.Enabled);
				actionMenu.PerformClick();
				AssertEquals(typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				form.FireSaveButton();
			}

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadIncident = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals("should not add another IRS event", 1, irsLogs.Count());
			AssertEquals(1, loadIncidentLogs.Find(log => log.SL_SE_NKEvent == "MIS").Count());
			AssertEquals(1, loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "STC" && x.SL_Reference == "Disposition - WRK to CLS").Count());
			AssertEquals(1, loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "STU" && x.SL_Reference.Contains("|NEW=CLS|OLD=WRK")).Count());

			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, loadIncident.IM_ClosureResolution);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseOnBehalfOfClientItem_PerformAction_NoIRS_NoIWR()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR4";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, string.Empty);
			var irsEvent = incident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "IRS");
			incident.Logs.AddNew(Events.IncidentClosed, Array.Empty<KeyValuePair<string, string>>());
			irsEvent.Delete();
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var logs = incident.Logs.GetAllLogs().Cast<StmALog>();

			AssertEquals("Should be no IRS", 0, logs.Count(log => log.SL_SE_NKEvent == "IRS"));
			AssertEquals("Should be no IWR", 0, logs.Count(log => log.SL_SE_NKEvent == "IWR"));
			AssertEquals("Should be an ICL", 1, logs.Count(log => log.SL_SE_NKEvent == "ICL"));
			var closeEvent = logs.FirstOrDefault(log => log.SL_SE_NKEvent == "ICL");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
			{
				var closeIncidentForm = dialog as CloseIncidentPopupForm;
				if (closeIncidentForm != null)
				{
					var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
					action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
					action.SynchroniseToIncident();
				}
			});

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Assert("incident CanCloseOnBehalfOfClient should be true", incident.CanCloseOnBehalfOfClient);
				var actionMenu = GetActionsMenuItem(form, "Close on behalf of Client (Confirmed Resolved)");
				Assert(actionMenu.Enabled);
				actionMenu.PerformClick();
				AssertEquals(typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				form.FireSaveButton();
			}

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadIncident = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals("Should add another IRS event", 1, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, loadIncident.IM_ClosureResolution);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		#endregion
		#region Generic
		void AssertVisibleMenuItems(SupportIncidentForm form, int expected)
		{
			int count = 0;
			MenuItem actionsMenu = GetActionsMenu(form);
			foreach (MenuItem item in actionsMenu.MenuItems)
			{
				if (item.Visible)
				{
					count++;
				}
			}

			AssertEquals(expected, count);
		}

		#endregion
		#region Email
		public void TestSendEmailAnonymously()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			incident.IM_OC_Contact = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;
			var sender = new EmailOnlyCustomerNotificationSender();
			incident.CustomerNotifier = new InteractiveIncidentCustomerNotifier(incident, sender);
			Factory.Save();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				GetEmailNotificationMenuItem(form).PerformClick();
				var email = (SupportIncidentEmail)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("EmailContactForm.BusinessEntity.BusinessObjectSendingEmail", incident, email.BusinessObjectSendingEmail);
			}
		}

		public void TestSendEmailAnonymouslyShowsWarning()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_Status = IncidentMainLookups.Status.Working;
				AssertEquals("Precondition: Incident should have changes.", true, incident.HasChanges);
				GetEmailNotificationMenuItem(form).PerformClick();
				AssertEquals("LastMessage", "Changes have been made to this record. You must save before sending an Email.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendNotification()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			incident.IM_OC_Contact = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;
			var sender = new EmailOnlyCustomerNotificationSender();
			incident.CustomerNotifier = new InteractiveIncidentCustomerNotifier(incident, sender);
			Factory.Save();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				GetEmailNotificationMenuItem(form).PerformClick();
				var email = (SupportIncidentEmail)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("Notification email should have been obtained.", true, email.Subject.StartsWith("Notification of"));
				GetEmailCorrespondenceMenuItem(form).PerformClick();
				email = (SupportIncidentEmail)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("Correspondence email should have been obtained.", true, email.Subject.StartsWith("Update on"));
			}
		}

		MenuItem GetEmailNotificationMenuItem(Form form)
		{
			return GetActionsMenuItem(form, "Re-send Email &Notification");
		}

		MenuItem GetEmailCorrespondenceMenuItem(Form form)
		{
			return GetActionsMenuItem(form, "Email &Update");
		}

		MenuItem GetActionsMenuItem(Form form, string text, bool allResult = false)
		{
			MenuItem actionsMenu = GetActionsMenu(form);
			foreach (MenuItem actionItem in actionsMenu.MenuItems)
			{
				if (actionItem.Text == text && (actionItem.Visible || allResult))
				{
					return actionItem;
				}
			}

			return null;
		}

		MenuItem GetActionsMenu(Form form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item.Text == "Actio&ns")
				{
					return item;
				}
			}

			return null;
		}

		#endregion
		public void TestFormCaption()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Organisation";
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS12345";
			incident.IM_Description = "Some Summary";
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				AssertEquals("CS12345 - Some Summary", form.FormCaption);
				incident.IM_OA_BranchAddress = org.MainAddress.PK;
				AssertEquals("CS12345 - Some Summary - Some Organisation", form.FormCaption);
			}
		}

		public void TestNotesTab()
		{
			bool notesFound = false;
			SupportIncident incident = Factory.New<SupportIncident>();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				foreach (ZTabPage page in ((ZTabControl)form.Controls[0].Controls[0]).TabPages)
				{
					if (page.Text.Contains("Notes"))
					{
						notesFound = true;
						break;
					}
				}
			}

			Assert(!notesFound);
		}

		public void TestShowAccreditation()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			FieldInfo field = typeof(SupportIncidentForm).GetField("ShowAccreditationButton", BindingFlags.NonPublic | BindingFlags.Instance);
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((ZButton)field.GetValue(form)).PerformClick();
				AssertEquals("Incident contact is not specified.", UnitTestUserNotification.Instance.LastMessage.Text);
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Demo_Company";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.MainAddress.OA_Phone = "111111111";
				org.MainAddress.OA_Address1 = "100 Fake St";
				OrgContact contact = org.Contacts.AddNew();
				contact.OC_Email = "newuser@cargowise.com";
				contact.OC_ContactName = "John Smith";
				incident.IM_OA_BranchAddress = org.MainAddress.PK;
				incident.IM_OC_Contact = contact.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((ZButton)field.GetValue(form)).PerformClick();
				AssertEquals("No accreditation info for John Smith.", UnitTestUserNotification.Instance.LastMessage.Text);
				CertificateApplicant applicant = Factory.NewWithValidTestData<CertificateApplicant>();
				applicant.HA_EmailAddress = "newuser@cargowise.com";
				applicant.RelatedOrgContactPK = contact.PK;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((ZButton)field.GetValue(form)).PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Accreditation form", typeof(HRJobApplicantAccreditationInfoForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(applicant.PK, ((IBusiness)((HRJobApplicantAccreditationInfoForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest).Identifier);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
			}
		}

		public void TestFormHasPlugIns()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS12345";
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				AssertNotNull("The form should contain the Invoicing PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull("The form should contain the Documents PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull("The form should contain the Licence PlugIn", form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence));
			}
		}

		public void TestLicenceKeyBuilderPlugin_IncidentClosed()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS12345";
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertNotNull("The form should contain the Licence PlugIn", form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence));
				AssertEquals(true, (form.PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl).AreTokenButtonsVisible);
			}
		}

		public void TestSetupBusinessConsultantVisibility()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS00098432";
			FieldInfo field = typeof(SupportIncidentForm).GetField("businessConsultantCodeFindBox", BindingFlags.NonPublic | BindingFlags.Instance);
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				AssertEquals("Not visible", false, ((ZCodeFindBox)field.GetValue(form)).Visible);
			}

			incident = Factory.New<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "Testing");
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectedTab = ((ZTabPage)form.TopLevelTabControl_Exposed.TabPages[2]);
				AssertEquals("Should be visible", true, ((ZCodeFindBox)field.GetValue(form)).Visible);
			}

			incident = Factory.New<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "Testing");
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			incident.RelatedProjectPK = project.PK;
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectedTab = ((ZTabPage)form.TopLevelTabControl_Exposed.TabPages[2]);
				AssertEquals("Should be visible", true, ((ZCodeFindBox)field.GetValue(form)).Visible);
			}
		}

		public void TestNoDeleteOnRelatedIncidents()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00098432";
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "Testing");
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			var attachedIncident = incident.RelatedFeatureRequests.AddNew();
			attachedIncident.IM_IncidentNumber = "CS00098433";
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				var control = form.Controls.Find("RelatedFeatureRequestsGrid", true).First();
				AssertEquals("Should be readonly", true, control.GetReadOnly());
				form.Dispose();
			}
		}

		#region EConversation
		public void TestEConversationAndCloseActions_NotAvailableIfIncidentNotSavedOrCR1()
		{
			var incident = GetIncidentForEConversationTest();
			var sendAndClosePermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			try
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					Assert(!form.SendMessageButtonForTest.Enabled);
					Assert(!form.ConversationMessageTextBox.Enabled);
					AssertEquals("eConversation is not available until incident is saved", form.ConversationMessageTextBox.Text);
					Assert(form.CloseAsButtonForTest.Enabled);
					Assert(form.CloseMenuItemForTest.Enabled);
				}

				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					Assert(!form.SendMessageButtonForTest.Enabled);
					Assert(!form.ConversationMessageTextBox.Enabled);
					AssertEquals("eConversation is not available until incident is saved. Please mark a task as Working before closing the incident", form.ConversationMessageTextBox.Text);
					Assert(!form.CloseAsButtonForTest.Enabled);
					Assert(!form.CloseMenuItemForTest.Enabled);
					incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
					form.FireSaveButton();
					AssertNotEquals("Warning for unsent messages shouldn't be triggered", "You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!form.SendMessageButtonForTest.Enabled);
					Assert(!form.ConversationMessageTextBox.Enabled);
					AssertEquals("Please mark a task as Working before communicating with the client or closing the incident", form.ConversationMessageTextBox.Text);
					Assert(!form.CloseAsButtonForTest.Enabled);
					Assert(!form.CloseMenuItemForTest.Enabled);
					var task = incident.WorkflowItems.AddNew();
					task.P9_Description = "Task 1";
					task.P9_Type = "UDF";
					task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					Assert(!form.SendMessageButtonForTest.Enabled);
					Assert(form.ConversationMessageTextBox.Enabled);
					AssertEquals(string.Empty, form.ConversationMessageTextBox.Text);
					Assert(form.CloseAsButtonForTest.Enabled);
					Assert(form.CloseMenuItemForTest.Enabled);
				}

				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
				var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
				using (var form = new SupportIncidentFormForTest(loadedIncident))
				{
					form.Show();
					Assert(!form.SendMessageButtonForTest.Enabled);
					Assert(form.ConversationMessageTextBox.Enabled);
					AssertEquals(string.Empty, form.ConversationMessageTextBox.Text);
					Assert(form.CloseAsButtonForTest.Enabled);
					Assert(form.CloseMenuItemForTest.Enabled);
				}
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendAndClosePermissions;
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestEConversationAndCloseActions_DisableIfCurrentTaskNotWorking()
		{
			var incident = GetIncidentForEConversationTest();
			var sendAndClosePermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			try
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(false, form.ConversationMessageTextBox.Enabled);
					AssertEquals("eConversation is not available until incident is saved. Please mark a task as Working before closing the incident", form.ConversationMessageTextBox.Text);
					AssertEquals(false, form.CloseAsButtonForTest.Enabled);
					AssertEquals(false, form.CloseMenuItemForTest.Enabled);
					UnitTestUserNotification.Instance.ClearMessages();
					form.FireSaveButton();
					AssertNotEquals("Warning for unsent messages shouldn't be triggered", "You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);
					var msg = "Please mark a task as Working before communicating with the client or closing the incident";
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(false, form.ConversationMessageTextBox.Enabled);
					AssertEquals(msg, form.ConversationMessageTextBox.Text);
					AssertEquals(false, form.CloseAsButtonForTest.Enabled);
					AssertEquals(false, form.CloseMenuItemForTest.Enabled);
					UnitTestUserNotification.Instance.ClearMessages();
					form.FireSaveButton();
					AssertNotEquals("Warning for unsent messages shouldn't be triggered", "You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);
					var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
					eConvPlugin.SelectTabPage();
					Application.DoEvents();
					var eConvView = eConvPlugin.UserControl as IConversationView;
					var eConv = (incident as IConversationProvider).eConversation;
					AssertEquals(false, eConvView.MessageTextBox.Enabled);
					AssertEquals(false, eConvView.SendButton.Enabled);
					AssertEquals(msg, eConvView.MessageTextBox.Text);
					AssertEquals(eConv.NextMessage, (ZBlob)ORtfTextUtil.TextToRtfBytes(msg));
					var task1 = incident.WorkflowItems.AddNew();
					task1.P9_Description = "Task 1";
					task1.P9_Type = "UDF";
					task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					task1.P9_Sequence = 1;
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(true, form.ConversationMessageTextBox.Enabled);
					AssertEquals(true, form.CloseAsButtonForTest.Enabled);
					AssertEquals(true, form.CloseMenuItemForTest.Enabled);
					AssertEquals(true, eConvView.MessageTextBox.Enabled);
					AssertEquals("", eConvView.MessageTextBox.Text);
					AssertEquals(eConv.NextMessage, ZBlob.Empty);
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(false, form.ConversationMessageTextBox.Enabled);
					AssertEquals(msg, form.ConversationMessageTextBox.Text);
					AssertEquals(false, form.CloseAsButtonForTest.Enabled);
					AssertEquals(false, form.CloseMenuItemForTest.Enabled);
					AssertEquals(false, eConvView.MessageTextBox.Enabled);
					AssertEquals(msg, eConvView.MessageTextBox.Text);
					AssertEquals(eConv.NextMessage, (ZBlob)ORtfTextUtil.TextToRtfBytes(msg));
					EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					form.FireSaveButton();
					AssertNotEquals("Warning for unsent messages shouldn't be triggered", "You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(true, form.ConversationMessageTextBox.Enabled);
					AssertEquals(true, form.CloseAsButtonForTest.Enabled);
					AssertEquals(true, form.CloseMenuItemForTest.Enabled);
					AssertEquals(true, eConvView.MessageTextBox.Enabled);
					AssertEquals("", eConvView.MessageTextBox.Text);
					AssertEquals(eConv.NextMessage, ZBlob.Empty);
					EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(false, form.ConversationMessageTextBox.Enabled);
					AssertEquals(false, form.CloseAsButtonForTest.Enabled);
					AssertEquals(false, form.CloseMenuItemForTest.Enabled);
					AssertEquals(false, eConvView.MessageTextBox.Enabled);
					AssertEquals(msg, eConvView.MessageTextBox.Text);
					AssertEquals(eConv.NextMessage, (ZBlob)ORtfTextUtil.TextToRtfBytes(msg));
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(true, form.ConversationMessageTextBox.Enabled);
					AssertEquals(true, form.CloseAsButtonForTest.Enabled);
					AssertEquals(true, form.CloseMenuItemForTest.Enabled);
					AssertEquals(true, eConvView.MessageTextBox.Enabled);
					AssertEquals("", eConvView.MessageTextBox.Text);
					AssertEquals(eConv.NextMessage, ZBlob.Empty);
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
					task1.P9_GS_NKAssignedStaffMember = "AAA";
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(false, form.ConversationMessageTextBox.Enabled);
					AssertEquals(false, form.CloseAsButtonForTest.Enabled);
					AssertEquals(false, form.CloseMenuItemForTest.Enabled);
					AssertEquals(false, eConvView.MessageTextBox.Enabled);
					var task2 = incident.WorkflowItems.AddNew();
					task2.P9_Description = "Task 2";
					task2.P9_Type = "UDF";
					task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					task2.P9_Sequence = 2;
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(true, form.ConversationMessageTextBox.Enabled);
					AssertEquals(true, form.CloseAsButtonForTest.Enabled);
					AssertEquals(true, form.CloseMenuItemForTest.Enabled);
					AssertEquals(true, eConvView.MessageTextBox.Enabled);
					incident.WorkflowItems.RemoveAndDelete(task1);
					incident.WorkflowItems.RemoveAndDelete(task2);
					AssertEquals(false, form.SendMessageButtonForTest.Enabled);
					AssertEquals(false, form.ConversationMessageTextBox.Enabled);
					AssertEquals(false, form.CloseAsButtonForTest.Enabled);
					AssertEquals(false, form.CloseMenuItemForTest.Enabled);
					AssertEquals(false, eConvView.MessageTextBox.Enabled);
				}
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendAndClosePermissions;
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseActions_DisableIfCurrentTaskWorkingAndResolutionCodeIsClosed()
		{
			var incident = GetIncidentForEConversationTest();
			Factory.Save();
			var sendAndClosePermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
			try
			{
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					AssertEquals("Pre-condition: Should be OPN", "OPN", incident.IM_Status);
					AssertEquals("Pre-condition: Should be ADD:", "ADD", incident.IM_ResolutionCode);
					Assert("The SendMessageButton should be disabled", !form.SendMessageButtonForTest.Enabled);
					Assert("The ConversationMessageTextBox should be enabled", form.ConversationMessageTextBox.Enabled);
					Assert("The CloseAsButton should be enabled", form.CloseAsButtonForTest.Enabled);
					Assert("The Actions -> CloseMenuItem should be enabled", form.CloseMenuItemForTest.Enabled);
					Assert("The AwaitingResponseButton should be enabled", form.AwaitingResponseButtonForTest.Enabled);

					bool isPendingEConversationPopupShown = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
					{
						var closeIncidentForm = dialog as CloseIncidentPopupForm;
						if (closeIncidentForm != null)
						{
							var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
							action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
							action.SynchroniseToIncident();
						}

						var pendingEConversationForm = dialog as SendEConversationForm;
						if (pendingEConversationForm != null)
						{
							isPendingEConversationPopupShown = true;
						}
					});
					form.ConversationMessageTextBox.Text = "blah blah";
					form.CloseAsButtonForTest.PerformClick();

					Assert("Should be no pending eConversation message", !isPendingEConversationPopupShown);
					AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
					AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
					form.FireSaveButton();
					Assert("The SendMessageButton should be disabled", !form.SendMessageButtonForTest.Enabled);
					Assert("The ConversationMessageTextBox should be disabled", !form.ConversationMessageTextBox.Enabled);
					Assert("The CloseAsButton should be disabled", !form.CloseAsButtonForTest.Enabled);
					Assert("The Actions -> CloseMenuItem should be disabled", !form.CloseMenuItemForTest.Enabled);
					Assert("The AwaitingResponseButton should be disabled", !form.AwaitingResponseButtonForTest.Enabled);

					var task1 = incident.WorkflowItems.AddNew();
					task1.P9_Description = "Task 1";
					task1.P9_Type = "UDF";
					task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					task1.P9_Sequence = 1;
					AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
					AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
					Assert("The SendMessageButton should be disabled", !form.SendMessageButtonForTest.Enabled);
					Assert("The ConversationMessageTextBox should be disabled", !form.ConversationMessageTextBox.Enabled);
					Assert("The CloseAsButton should be disabled", !form.CloseAsButtonForTest.Enabled);
					Assert("The Actions -> CloseMenuItem should be disabled", !form.CloseMenuItemForTest.Enabled);
					Assert("The AwaitingResponseButton should be disabled", !form.AwaitingResponseButtonForTest.Enabled);
				}
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendAndClosePermissions;
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
		}

		public void TestEConversation_EnableSaveButton()
		{
			var incident = GetIncidentForEConversationTest();
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				AssertEquals("Pre-condition", false, incident.HasChanges);
				AssertEquals("Pre-condition", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				form.ConversationMessageTextBox.Focus();
				form.ConversationMessageTextBox.Text = "blah blah";
				form.Controls[0].Focus();
				AssertEquals("Incident has changes", true, incident.HasChanges);
				AssertEquals("Save button is enabled", true, ((IPostingButtonsProvider)form).CommandButtonApply.Enabled);
				AssertEquals("Save button has correct text", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				form.FireSaveButton();
				AssertEquals("Pending eConversation Message", ZFormModaliser.LastFormShownDialogForTest.Text);
				ZFormModaliser.LastFormShownDialogForTest.Close();
				AssertEquals(string.Empty, form.ConversationMessageTextBox.Text);
				AssertEquals("Incident has no changes", false, incident.HasChanges);
				AssertEquals("Save button is disabled after save", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				form.ConversationMessageTextBox.Text = "blah blah";
				form.SendMessageButtonForTest.PerformClick();
				AssertEquals("Incident has changes", true, incident.HasChanges);
				AssertEquals("Save button is enabled when send button is clicked", true, ((IPostingButtonsProvider)form).CommandButtonApply.Enabled);
				AssertEquals("Save button has correct text", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestDisableEConversation_WhenERequestStatusEqualsResolvedOrClosed()
		{
			var incident = GetIncidentForEConversationTest();
			Factory.Save();
			var sendAndClosePermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
			try
			{
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					AssertEquals("Pre-condition: Should be OPN", "OPN", incident.IM_Status);
					AssertEquals("Pre-condition: Should be ADD:", "ADD", incident.IM_ResolutionCode);
					Assert("The SendMessageButton should be disabled", !form.SendMessageButtonForTest.Enabled);
					Assert("The ConversationMessageTextBox should be enabled", form.ConversationMessageTextBox.Enabled);
					Assert("The CloseAsButton should be enabled", form.CloseAsButtonForTest.Enabled);
					Assert("The Actions -> CloseMenuItem should be enabled", form.CloseMenuItemForTest.Enabled);
					Assert("The AwaitingResponseButton should be enabled", form.AwaitingResponseButtonForTest.Enabled);

					bool isPendingEConversationPopupShown = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
					{
						var closeIncidentForm = dialog as CloseIncidentPopupForm;
						if (closeIncidentForm != null)
						{
							var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
							action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
							action.SynchroniseToIncident();
						}

						var pendingEConversationForm = dialog as SendEConversationForm;
						if (pendingEConversationForm != null)
						{
							isPendingEConversationPopupShown = true;
						}
					});
					form.ConversationMessageTextBox.Text = "blah blah";
					form.CloseAsButtonForTest.PerformClick();

					Assert("Should be no pending eConversation message", !isPendingEConversationPopupShown);
					AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
					AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
					form.FireSaveButton();
					Assert("The SendMessageButton should be disabled", !form.SendMessageButtonForTest.Enabled);
					Assert("The ConversationMessageTextBox should be disabled", !form.ConversationMessageTextBox.Enabled);
					Assert("The CloseAsButton should be disabled", !form.CloseAsButtonForTest.Enabled);
					Assert("The Actions -> CloseMenuItem should be disabled", !form.CloseMenuItemForTest.Enabled);
					Assert("The AwaitingResponseButton should be disabled", !form.AwaitingResponseButtonForTest.Enabled);
				}
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendAndClosePermissions;
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
		}

		public void TestClickSaveAfterSendingMessagesIncidentHasNoChanges()
		{
			var incident = GetIncidentForEConversationTest();
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.ConversationMessageTextBox.Text = "blah blah";
				form.SendMessageButtonForTest.PerformClick();
				incident.WorkflowItems.RemoveAll();
				AssertEquals("Incident has changes", true, incident.HasChanges);
				form.Controls[0].Focus();
				AssertEquals("Save button is enabled", true, ((IPostingButtonsProvider)form).CommandButtonApply.Enabled);
				AssertEquals("Incident has changes", true, incident.HasChanges);
				AssertEquals("Incident has no error", false, incident.HasErrors);
				form.FireSaveButton();
				AssertEquals("Incident has no changes", false, incident.HasChanges);
			}
		}

		public void TestEConversation_ChangeTaskStatusDoesNotClearUnsentMessage()
		{
			var incident = GetIncidentForEConversationTest();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			var sendPermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			try
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = true;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					Assert("Pre-condition: eConversation text box is enabled", form.ConversationMessageTextBox.Enabled);
					form.ConversationMessageTextBox.Text = "Hello world!";
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					Assert("eConversation text box is still enabled", form.ConversationMessageTextBox.Enabled);
					Assert("eConversation text box should not be cleared", !string.IsNullOrEmpty(form.ConversationMessageTextBox.Text));
				}
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendPermissions;
			}
		}

		public void TestEConversation_CloseIncidentComment()
		{
			var incident = GetIncidentForEConversationTest();
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			var sendPermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			try
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					bool isPendingEConversationPopupShown = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
					{
						var closeIncidentForm = dialog as CloseIncidentPopupForm;
						if (closeIncidentForm != null)
						{
							var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
							action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
							action.SynchroniseToIncident();
						}

						var pendingEConversationForm = dialog as SendEConversationForm;
						if (pendingEConversationForm != null)
						{
							isPendingEConversationPopupShown = true;
						}
					});
					form.ConversationMessageTextBox.Text = "blah blah";
					form.CloseAsButtonForTest.PerformClick();
					Assert("Should be no pending eConversation message", !isPendingEConversationPopupShown);
					AssertEquals("Unsent eConversation is copied to close incident comment", "blah blah", ((SupportIncidentAction)ZFormModaliser.LastIBusinessShownOnDialogForTest).Comment);
					AssertEquals("Unsent eConversation is cleared after close incident action is confirmed", "eConversation is disabled for this eRequest Status. Please go to Actions > Reopen to enable eConversation", form.ConversationMessageTextBox.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					form.ConversationMessageTextBox.Text = "some comment";
					form.CloseAsButtonForTest.PerformClick();
					AssertEquals("Unsent eConversation is not cleared if close incident action is cancelled", "some comment", form.ConversationMessageTextBox.Text);
				}
			}
			finally
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendPermissions;
			}
		}

		public void TestEConversation_SaveAndSendDefaultAction()
		{
			var incident = GetIncidentForEConversationTest();
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				AssertEquals("Pre-condition", false, incident.HasChanges);
				AssertEquals("Pre-condition", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				form.ConversationMessageTextBox.Focus();
				form.ConversationMessageTextBox.Text = "blahblah";
				form.Controls[0].Focus();//Bound fields are refreshed when the control loses focus
				AssertEquals("Incident has changes", true, incident.HasChanges);
				AssertEquals("Save button is enabled", true, ((IPostingButtonsProvider)form).CommandButtonApply.Enabled);
				AssertEquals("Save button has correct text", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				form.FireSaveButton();
				AssertEquals("Pending eConversation Message", ZFormModaliser.LastFormShownDialogForTest.Text);
				ZFormModaliser.LastFormShownDialogForTest.Close();
				AssertEquals(string.Empty, form.ConversationMessageTextBox.Text);
				AssertEquals("Incident has no changes", false, incident.HasChanges);
				AssertEquals("Save button is disabled after save", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				AssertEquals(string.Empty, form.ConversationMessageTextBox.Text);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestEConversation_SaveWithChangesInBothEConversationBoxes()
		{
			var incident = GetIncidentForEConversationTest();
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				AssertEquals("Pre-condition", false, incident.HasChanges);
				AssertEquals("Pre-condition", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);

				form.EConversationTabControlForTest.MessageTextBox.Focus();
				form.EConversationTabControlForTest.MessageTextBox.Text = "blahblah";
				form.ConversationMessageTextBox.Focus();
				form.ConversationMessageTextBox.Text = "nahnah";
				form.Controls[0].Focus();//Bound fields are refreshed when the control loses focus
				AssertEquals("Incident has changes", true, incident.HasChanges);
				AssertEquals("Save button is enabled", true, ((IPostingButtonsProvider)form).CommandButtonApply.Enabled);
				AssertEquals("Save button has correct text", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();
				AssertEquals("You have unsent messages in multiple eConversation text boxes. If you continue with save, the contents will be discarded. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have cleared value", "blahblah", form.EConversationTabControlForTest.MessageTextBox.Text);
				AssertEquals("Should not have cleared value", "nahnah", form.ConversationMessageTextBox.Text);
				AssertEquals("Incident should not have saved", true, incident.HasChanges);
				AssertEquals("Save button should still be enabled", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("Should have discarded value", string.Empty, form.EConversationTabControlForTest.MessageTextBox.Text);
				AssertEquals("Should have discarded value", string.Empty, form.ConversationMessageTextBox.Text);
				AssertEquals("Incident has no changes", false, incident.HasChanges);
				AssertEquals("Save button is disabled after save", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestEConversation_RelatedWorkItems()
		{
			var incident = GetIncidentForEConversationTest();
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask workItemTask = workItem.WorkflowItems.AddNew();
			workItemTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				AssertEquals(true, form.ConversationMessageTextBox.Enabled);
				form.ConversationMessageTextBox.Text = "User message";
				AssertEquals(true, form.SendMessageButtonForTest.Enabled);
			}
		}

		SupportIncident GetIncidentForEConversationTest()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			Factory.Save();
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			incident.ClientCompanyCode = clientCompany.LCC_Code;
			return incident;
		}

		class SupportIncidentFormForTest : SupportIncidentForm
		{
			public SupportIncidentFormForTest(SupportIncident incident) : base(incident)
			{
			}

			public IncidentContactPhoneDiallerUserControl ContactPhoneDiallerUserControlForTest
			{
				get
				{
					return ContactPhoneDiallerUserControl;
				}
			}

			public ZButton SendMessageButtonForTest
			{
				get
				{
					return SendMessageButton;
				}
			}

			public ZButton AwaitingResponseButtonForTest
			{
				get
				{
					return AwaitingResponseButton;
				}
			}

			public ZButton CloseAsButtonForTest
			{
				get
				{
					return CloseIncidentButton;
				}
			}

			public MenuItem CloseMenuItemForTest
			{
				get
				{
					return closeMenuItem;
				}
			}

			public IConversationView EConversationTabControlForTest
			{
				get
				{
					return PlugIns.Instances.OfType<IncidentConversationPlugin>().FirstOrDefault()?.UserControl as IConversationView;
				}
			}

			public ZLabel ServiceStatusLabelForTest
			{
				get
				{
					return ServiceStatusLabel;
				}
			}

			public ZLabel OutageDurationLabelForTest
			{
				get
				{
					return OutageDurationLabel;
				}
			}

			public MenuItem AddServiceOutageStartEventMenuItemForTest
			{
				get
				{
					return addServiceOutageStartEventMenuItem;
				}
			}

			public MenuItem AddServiceRestoredEventMenuItemForTest
			{
				get
				{
					return addServiceRestoredEventMenuItem;
				}
			}

			public MenuItem ClearAllServiceEventsMenuItemForTest
			{
				get
				{
					return clearAllServiceEventsMenuItem;
				}
			}

			public new ZStmALogAddForm CreateEventLogForm(string eventCode, string key, string value)
			{
				return base.CreateEventLogForm(eventCode, key, value);
			}

			protected override SendEConversationForm CreateSendEConversationForm(bool hasUnsentMessageOnCustomerServiceTab, bool awaitingResponseButtonVisible)
			{
				return new SendEConversationFormForTest(ConversationMessageTextBox.Text, shouldShowAwaitingResponse: awaitingResponseButtonVisible);
			}
		}

		class SendEConversationFormForTest : SendEConversationForm
		{
			public SendEConversationFormForTest(ZString eConversationMessage, bool shouldShowAwaitingResponse)
				: base(eConversationMessage, shouldShowAwaitingResponse)
			{
			}

			public void ClickCancel()
			{
				Action = PerformAction.Cancel;
				CancelButton.PerformClick();
			}

			public void ClickSend()
			{
				Action = PerformAction.Send;
				SendButton.PerformClick();
			}
		}

		#endregion

		#region Task Buttons
		public void TestTaskButtons()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "RCW Investigate";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
			Factory.Save();
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "DES";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Type = "INV";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task1.P9_Status);
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				form.SuspendCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
				form.SuspendCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
				form.CloseCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.CancelCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
				AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
				task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				var workitem = Factory.NewWithValidTestData<NewWorkItem>();
				workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
				incident.RelatedItems.Add(workitem);
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				incident.IM_Status = SupportIncidentLookups.Status.Working;
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals(3, incident.WorkflowItems.Count);
				var task3 = incident.WorkflowItems.Cast<ProcessTask>().OrderBy(x => x.P9_Sequence).Last();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, task3.P9_GS_NKAssignedStaffMember);
				AssertEquals(task3.Lookups.Types.GetDescriptionFromCode(task3.P9_Type), task3.P9_Description);
				Assert(!incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Incident Re-opened"));
				form.CloseCurrentTaskButton.PerformClick();
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
				task3.P9_Status = SupportIncidentLookups.Status.Closed;
				workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
				incident.IM_Status = SupportIncidentLookups.Status.Closed;
				form.WorkOnCurrentTaskButton.PerformClick();
				Assert(incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Incident Re-opened"));
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestWorkOnCurrentTaskButton_P9_DescriptionMaxLength()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "RCW Investigate";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
			Factory.Save();
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "UDF";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, string.Empty);
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);

				incident.IM_Status = IncidentMainLookups.Status.Open;
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Precondition: Should have 1 task", 1, incident.WorkflowItems.Count);

				AssertNoExceptionThrown(() => form.WorkOnCurrentTaskButton.PerformClick());
				AssertEquals("Should have cloned the existing task", 2, incident.WorkflowItems.Count);
				var task2 = incident.WorkflowItems.Cast<ProcessTask>().FirstOrDefault(x => x.IsOpen);
				AssertEquals("Should have a truncated version of the registry description", task2.TypeDescription.Substring(0, task2.P9_DescriptionInfo.MaxLength - 1), task2.P9_Description);

				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
			}
		}

		[ExpectNoExceptions]
		public void TestTaskButtons_DataRefreshBusPublish()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = "ASN";
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				form.Close();
			}

			var factory2 = new BusinessObjectFactory();
			var loadedTask = factory2.Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = "CLS";
			factory2.Save();
		}

		public void TestSendEConversationMessageCreateTask()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NAN";
			staff.GS_FullName = "Name test";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "INV";
			task1.P9_Description = "Task 1";
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.TaskProperties.ActualDate = ZDateTimeOffset.UtcNow.AddMinutes(-1);
			task1.P9_CompletedTime = ZDateTimeOffset.UtcNow;
			task1.P9_ActualDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 0, 1, 0);
			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Type = "DES";
			task2.P9_Description = "Task 2";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals(2, incident.WorkflowItems.Count);
				form.ConversationMessageTextBox.Text = "Message local user";
				AssertEquals(true, form.SendMessageButtonForTest.Enabled);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendMessageButtonForTest.PerformClick();
				AssertEquals(3, incident.WorkflowItems.Count);
				AssertEquals("Client Communication", incident.WorkflowItems[2].P9_Description);
				AssertEquals(TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0)), incident.WorkflowItems[2].P9_EstDuration);
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestIgnoreKnownNames()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NAA";
			staff.GS_FullName = "ababab aba";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "NAB";
			staff2.GS_FullName = "bcbcbc bcb";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "INV";
			task1.P9_Description = "Task 1";
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.TaskProperties.ActualDate = ZDateTimeOffset.UtcNow.AddMinutes(-1);
			task1.P9_CompletedTime = ZDateTimeOffset.UtcNow;
			task1.P9_ActualDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 0, 1, 0);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.EConversation.Conversation.Staff.AddNewParticipant(staff2);
			Factory.Save();
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				var checkSpellingMenuItem = form.ConversationMessageTextBox.ContextMenuStrip.Items.Find("checkSpelling", false)[0];
				form.ConversationMessageTextBox.Text = "I am ababab, I am aba. You are bcbcbc, you are bcb.";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					checkSpellingMenuItem.PerformClick();
					AssertEquals("No errors found.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				var staff3 = Factory.NewWithValidTestData<GlbStaff>();
				staff3.GS_Code = "NAC";
				staff3.GS_FullName = "cdcdcd cdc";
				var staff4 = Factory.NewWithValidTestData<GlbStaff>();
				staff4.GS_Code = "NAD";
				staff4.GS_FullName = "dedede ded";
				var task2 = incident.WorkflowItems.AddNew();
				task2.P9_Sequence = 2;
				task2.P9_Type = "DES";
				task2.P9_Description = "Task 2";
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				task2.P9_GS_NKAssignedStaffMember = staff3.GS_Code;
				incident.EConversation.Conversation.Staff.AddNewParticipant(staff4);
				form.FireSaveButton();
				form.ConversationMessageTextBox.Text = "I am cdcdcd, I am cdc. You are dedede, you are ded.";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					checkSpellingMenuItem.PerformClick();
					AssertEquals("No errors found.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		SpellCheckFormAction Change(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, null);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_Closed()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, shouldEnableWorkingButton: false);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_Resolved()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)"ZZZ Description", supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(resolvedCode, shouldEnableWorkingButton: false);
			}
		}

		public void TestWorkOnCurrentTaskButtonEnabled_ClosedAwaitingClientResponse()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, shouldEnableWorkingButton: false);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_WaitingUpgrade()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, shouldEnableWorkingButton: false);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_UpgradeDelayed()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, shouldEnableWorkingButton: false);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_FormalQuotationProvided()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, shouldEnableWorkingButton: false);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_DevelopmentEstimateProvided()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, shouldEnableWorkingButton: false);
		}

		public void TestWorkOnCurrentTaskButtonEnabled_FeatureAccepted()
		{
			AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, shouldEnableWorkingButton: true);
		}

		void AssertWorkOnCurrentTaskButtonEnabled_IM_ResolutionCode(string disposition, bool shouldEnableWorkingButton)
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "RCW Investigate";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
			Factory.Save();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "DES";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, task1.P9_Status);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);

				incident.CloseIncident(disposition, string.Empty);

				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);

				AssertEquals(shouldEnableWorkingButton, form.WorkOnCurrentTaskButton.Enabled);
				var task2 = incident.WorkflowItems.AddNew();
				task2.P9_Sequence = 2;
				task2.P9_Type = "DES";
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				AssertEquals(true, form.WorkOnCurrentTaskButton.Enabled);
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestWorkOnCurrentTaskButtonEnabled()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "RCW Investigate";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
			Factory.Save();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "DES";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, task1.P9_Status);
				AssertEquals("Work on current task should be enabled when there is an open task not working", true, form.WorkOnCurrentTaskButton.Enabled);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
				AssertEquals("Work on current task should be disabled when tasks are working", false, form.WorkOnCurrentTaskButton.Enabled);
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestWorkOnCurrentTaskButton_Click_ShouldNotReopen()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "RCW Investigate";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
			Factory.Save();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "DES";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Type = "INV";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task1.P9_Status);
				form.WorkOnCurrentTaskButton.PerformClick();
				form.CloseCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.CancelCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
				AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
				task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				var workitem = Factory.NewWithValidTestData<NewWorkItem>();
				workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
				incident.RelatedItems.Add(workitem);
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				incident.IM_Status = SupportIncidentLookups.Status.Working;
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals(3, incident.WorkflowItems.Count);
				var task3 = incident.WorkflowItems.Cast<ProcessTask>().OrderBy(x => x.P9_Sequence).Last();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, task3.P9_GS_NKAssignedStaffMember);
				AssertEquals(task3.Lookups.Types.GetDescriptionFromCode(task3.P9_Type), task3.P9_Description);
				Assert(!incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Incident Re-opened"));
				AssertEquals("Should not change the resolution code", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

				form.CloseCurrentTaskButton.PerformClick();
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
				task3.P9_Status = SupportIncidentLookups.Status.Closed;
				workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
				incident.IM_Status = SupportIncidentLookups.Status.Closed;
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, string.Empty);

				AssertEquals("Precondition: Incident should be closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Precondition: Incident should be closed", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
				var task4 = incident.WorkflowItems.AddNew();
				task4.P9_Sequence = 22;
				task4.P9_Type = "INV";
				task4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals("Incident disposition should remain closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Incident disposition should remain closed", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
				AssertEquals("Should set to working", ProcessTaskStatusCodeList.Codes.Working, task4.P9_Status);
				AssertEquals("Should set to working", SupportIncidentLookups.Status.Working, incident.IM_Status);
				Assert(!incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Incident Re-opened"));
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestWorkOnCurrentTaskButton_Click_FeatureAccepted_ClosedWorkflowStatus_ShouldReopen()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = client.PK;
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "RCW Investigate";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
			Factory.Save();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OA_BranchAddress = client.Addresses[0].PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "DES";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Type = "INV";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab(0);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task1.P9_Status);
				form.WorkOnCurrentTaskButton.PerformClick();
				form.CloseCurrentTaskButton.PerformClick();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, string.Empty);
				AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, incident.IM_ResolutionCode);
				AssertEquals("Precondition", SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
				form.WorkOnCurrentTaskButton.PerformClick();
				AssertEquals(3, incident.WorkflowItems.Count);
				var task3 = incident.WorkflowItems.Cast<ProcessTask>().OrderBy(x => x.P9_Sequence).Last();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, task3.P9_GS_NKAssignedStaffMember);
				AssertNotEquals("Should reopen the incident, recalculating the resolution code", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
				Assert("Should reopen incident", incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Incident Re-opened"));
			}

			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		#endregion

		public void TestChangeClientLabelColor()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.SetRelatedParty(parentOrg, RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR);
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_LD = database.PK;
			Factory.Save();
			var controlFieldInfo1 = typeof(SupportIncidentForm).GetField("supportClientGuidFindBox", BindingFlags.Instance | BindingFlags.NonPublic);
			var controlFieldInfo2 = typeof(SupportIncidentForm).GetField("LocalClientControl", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				incident.IM_OH_Client = org.PK;
				incident.IM_OC_Contact = incident.Client.Contacts.AddNew().PK;
				incident.IM_LCC = clientCompany.PK;
				var supportClientControl = controlFieldInfo1.GetValue(form) as ZGuidFindBox;
				AssertEquals(Color.Black, supportClientControl.Extensions.Get<ZLabelCaptionRenderer>().ForeColor);
				var localClientControl = controlFieldInfo2.GetValue(form) as ZOrgAddressControl;
				AssertEquals(Color.Black, localClientControl.ForeColor);
			}

			org.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.InvoicingPreferences.Description, "");
			incident = Factory.New<SupportIncident>();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				incident.IM_OH_Client = org.PK;
				incident.IM_OC_Contact = incident.Client.Contacts.AddNew().PK;
				incident.IM_LCC = clientCompany.PK;
				var supportClientControl = controlFieldInfo1.GetValue(form) as ZGuidFindBox;
				AssertEquals(Color.Red, supportClientControl.Extensions.Get<ZLabelCaptionRenderer>().ForeColor);
				var localClientControl = controlFieldInfo2.GetValue(form) as ZOrgAddressControl;
				AssertEquals(Color.Black, localClientControl.ForeColor);
			}
		}

		public void TestShowCloseIncidentPopup()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			int closeIncidentEventHandlerCallCount = 0;
			incident.OnCloseIncident += (s, e) =>
			{
				closeIncidentEventHandlerCallCount++;
				if (closeIncidentEventHandlerCallCount > 1)
				{
					Fail("Close incident form should only popup once");
				}
			};
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Close incident event handler should only be called once", 1, closeIncidentEventHandlerCallCount);
			}
		}

		public void TestShouldNotShowCloseIncidentPopupIfResolutionIsWaitingUpgradeOrUpgradeDelayed()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			var taskA = incident1.WorkflowItems.AddNew();
			taskA.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed;
			var taskB = incident2.WorkflowItems.AddNew();
			taskB.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var taskC = incident3.WorkflowItems.AddNew();
			taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			using (var form = new SupportIncidentForm(incident1))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				taskA.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Should not show close incident popup since the resolution code is WaitingUpgrade", null, ZFormModaliser.LastFormShownDialogForTest);
			}

			using (var form = new SupportIncidentForm(incident2))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				taskB.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Should not show close incident popup since the resolution code is WaitingUpgrade", null, ZFormModaliser.LastFormShownDialogForTest);
			}

			using (var form = new SupportIncidentForm(incident3))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Should show close incident popup", typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestShouldShowResolutionWizardForm()
		{
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			var mockData = new List<string> { "ENT" };

			mockFeatureControlManager.Setup(m => m.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CR5ResolutionWizard, CancellationToken.None))
									 .Returns(Task.FromResult(mockFeatureData.Object)).Verifiable();

			mockFeatureData.Setup(m => m.TryDeserializeParameterAsJson(out It.Ref<IEnumerable<string>>.IsAny))
						   .Returns(true)
						   .Callback((out IEnumerable<string> result) =>
						   {
							   result = mockData;
						   });

			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			using (ObjectFactory.Substitute(mockFeatureData.Object))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				var incident = Factory.New<SupportIncident>();
				incident.IM_Priority = "CR5";
				incident.IM_Product = "ENT";
				var taskC = incident.WorkflowItems.AddNew();
				taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				Factory.Save();
				AssertEquals(true, incident.IsResolutionWizardEnabled);

				using (var form = new SupportIncidentForm(incident))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					AssertEquals("Should show ResolutionWizardForm", typeof(ResolutionWizardForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}

		public void TestShouldNotShowResolutionWizardForm_WhenStageIsCNT()
		{
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			var mockData = new List<string> { "ENT" };

			mockFeatureControlManager.Setup(m => m.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CR5ResolutionWizard, CancellationToken.None))
									 .Returns(Task.FromResult(mockFeatureData.Object)).Verifiable();

			mockFeatureData.Setup(m => m.TryDeserializeParameterAsJson(out It.Ref<IEnumerable<string>>.IsAny))
						   .Returns(true)
						   .Callback((out IEnumerable<string> result) =>
						   {
							   result = mockData;
						   });

			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			using (ObjectFactory.Substitute(mockFeatureData.Object))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				var incident = Factory.New<SupportIncident>();
				incident.IM_Priority = "CR5";
				incident.IM_Product = "ENT";
				incident.IM_Category = "CNT";
				var taskC = incident.WorkflowItems.AddNew();
				taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				Factory.Save();
				AssertEquals(true, incident.IsResolutionWizardEnabled);

				using (var form = new SupportIncidentForm(incident))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					AssertEquals("Should not show ResolutionWizardForm", typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}

		public void TestShouldNotShowCloseIncidentPopupWhenIncidentManagementGroupSaving_WithATCMilestone()
		{
			var controlStage = "ZZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(controlStage, "desc", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "PYN";
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncidentForTest>();
			var task = incident.WorkflowItems.Tasks.AddNew();
			task.P9_Sequence = 10;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_GS_NKAssignedStaffMember = "PYN";

			var milestone = incident.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Auto-Close 'Create Work item'";
			milestone.TriggerConditions.TriggerEventCode = Events.AttachedCode;
			milestone.TriggerConditions.TriggerFiredCountdown = 100;
			MilestoneCompletionHelper.SetCompletionMilestone(task, milestone);

			var jobHeader = ProcessJobHeader.GetForParent(incident, Factory);
			var processHeader = jobHeader.ProcessHeaders.AddNew();
			MilestoneCompletionHelper.SetCompletionMilestone(processHeader, milestone);
			Factory.Save();

			AssertEquals("Precondition:", DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);
			AssertEquals("Precondition:", string.Empty, incident.IM_ClosureResolution);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "ING00000001";
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = controlStage;

			using (var form = new IncidentManagementGroupForm(group))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				var link = Factory.New<IncidentManagementLink>();
				link.INL_IM_Incident = incident.PK;
				link.INL_ING_Group = group.PK;
				link.INL_IsGroupControlled = false;

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
				Factory.Save();

				AssertEquals("Should not show close incident popup when IncidentManagementGroup saving", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestPromptShouldShowWhenCloseIncidentWithOpenInternalLog()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			using (var incidentForm = new MockSupportIncidentForm(incident))
			{
				AssertEquals("init incidentForm LogPopupFormCount should be zero", 0, incidentForm.LogPopupFormCount);
				var logForm = new AddIncidentLogPopupForm(new SupportIncidentLogCommentAction(incident), incidentForm);
				incidentForm.Show();
				incidentForm.ShowChildFormTest(logForm);
				AssertEquals("when popup one logForm, incidentForm's LogPopupFormCount should be one", 1, incidentForm.LogPopupFormCount);

				var bottomPanel = incidentForm.Controls.Find("BottomPanel", false).FirstOrDefault();
				var prevNextControl = bottomPanel?.Controls.OfType<ZPreviousNextControl>().FirstOrDefault();
				var nextButton = (ZButton)prevNextControl?.Controls.Find("NextButton", false).FirstOrDefault();
				var previousButton = (ZButton)prevNextControl?.Controls.Find("PreviousButton", false).FirstOrDefault();
				var currentResultCalcEdit = (ZCalcEdit)prevNextControl?.Controls.Find("CurrentRecordNumberCalcEdit", false).FirstOrDefault();

				Assert(!nextButton.Enabled);
				Assert(!previousButton.Enabled);
				Assert(!currentResultCalcEdit.Enabled);

				incidentForm.Close();
				Assert(incidentForm.HasCancelledClosingAction);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Incident cannot be closed while child dialogs are open.");
				logForm.Close();
				AssertEquals("when close the logForm, incidentForm's LogPopupFormCount should be zero", 0, incidentForm.LogPopupFormCount);
			}
		}

		public void TestSetupStaffAssignmentAndTaskStatusLabels()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			Factory.Save();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				var assignedLabel = form.GetAssignedToLabel();
				var overallLabel = form.GetOverallAssignedToCodeLabel();
				var overallDescriptionLabel = form.GetOverallAssignedToDescriptionLabel();
				var statusLabel = form.GetStatusLabel();
				var statusDescriptionLabel = form.GetStatusDescriptionLabel();
				var currentTaskLabel = form.GetCurrentTaskLabel();
				var currentTaskLabelText = form.GetCurrentTaskLabelText();
				var closedAssignedLabelWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
				var closedStatusLabelWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
				AssertEquals(AnchorStyles.Left | AnchorStyles.Right, currentTaskLabel.Anchor);
				AssertEquals(AnchorStyles.Left | AnchorStyles.Right, overallDescriptionLabel.Anchor);
				AssertEquals("Width of assigned to label when incident is closed", closedAssignedLabelWidth, assignedLabel.Width);
				AssertEquals("Width of status label when incident is closed", closedStatusLabelWidth, statusLabel.Width);
				AssertEquals("Location of status description label when incident is closed", new Point(assignedLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), statusLabel.Location.Y), statusDescriptionLabel.Location);
				AssertEquals("Location of current task label text when incident is closed", new Point(assignedLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), currentTaskLabelText.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(1)), currentTaskLabel.Location);
				AssertEquals("Location of overall assigned staff code label when incident is closed", new Point(assignedLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), overallLabel.Location.Y), overallLabel.Location);
				AssertEquals("Location of overall assigned staff name label when incident is closed", new Point(overallLabel.Right, overallDescriptionLabel.Location.Y), overallDescriptionLabel.Location);
				var nonClosedAssignedLabelWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				var nonClosedStatusLabelWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
				incident.IM_Status = SupportIncidentLookups.Status.Open;
				AssertEquals("Width of assigned to label when incident is not closed", nonClosedAssignedLabelWidth, form.GetAssignedToLabel().Width);
				AssertEquals("Width of status label when incident is not closed", nonClosedStatusLabelWidth, statusLabel.Width);
				AssertEquals("Location of status description label when incident is not closed", new Point(assignedLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), statusLabel.Location.Y), statusDescriptionLabel.Location);
				AssertEquals("Location of current task label text when incident is not closed", new Point(assignedLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), currentTaskLabelText.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(1)), currentTaskLabel.Location);
				AssertEquals("Location of overall assigned staff code label when incident is not closed", new Point(assignedLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), overallLabel.Location.Y), overallLabel.Location);
				AssertEquals("Location of overall assigned staff name label when incident is not closed", new Point(overallLabel.Right, overallDescriptionLabel.Location.Y), overallDescriptionLabel.Location);
				form.Dispose();
				AssertNoExceptionThrown("Nothing happens if the status changes after the form has closed", () => incident.IM_Status = SupportIncidentLookups.Status.Closed);
			}
		}

		public void TestMenuSectionCannotSelectAll()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				AssertEquals(0, form.Controls.Find("menuSectionDropEdit", true)[0].Controls.Find("ALL", true).Length);
				AssertEquals(0, form.Controls.Find("defectMenuSectionDropEdit", true)[0].Controls.Find("ALL", true).Length);
			}
		}

		public void TestDefectWorkItemGridSort()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var ediProject = Factory.NewWithValidTestData<Project>();
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var link = Factory.NewWithValidTestData<GenPivot>();
			link.XX_Relation1ID = ediProject.PK;
			link.XX_Relation1TableCode = "WKP";
			link.XX_Relation2ID = incident.PK;
			link.XX_Relation2TableCode = "IM";
			link.XX_RelationType = "WRK";
			var link2 = Factory.NewWithValidTestData<GenPivot>();
			link2.XX_Relation1ID = incident.PK;
			link2.XX_Relation1TableCode = "IM";
			link2.XX_Relation2ID = workItem.PK;
			link2.XX_Relation2TableCode = "WKI";
			link2.XX_RelationType = "WRK";
			Factory.Save();
			var incidentInMainFactory = Factory.Load<SupportIncident>(incident.PK);
			using (var form = new MockSupportIncidentForm(incidentInMainFactory))
			{
				form.Show();
				GetWorkItemGrid("DefectRelatedWorkItemsGrid", form, incidentInMainFactory);
				AssertEquals("shouldn't throw exception", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestPanelSimilarIncidentsIsControlledByRegistry()
		{
			EDIDataRegistry.Instance.EnableIncidentSimilarityFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new SupportIncidentForm(Factory.NewWithValidTestData<SupportIncident>()))
			{
				AssertNull(form.panelSimilarIncidents);
				AssertNull(form.splitterSimilarIncidents);
			}

			EDIDataRegistry.Instance.EnableIncidentSimilarityFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new SupportIncidentForm(Factory.NewWithValidTestData<SupportIncident>()))
			{
				AssertNotNull(form.panelSimilarIncidents);
				AssertNotNull(form.splitterSimilarIncidents);
			}
		}

		public void TestSplitterIsLinkedToPanelSimilarIncidents()
		{
			EDIDataRegistry.Instance.EnableIncidentSimilarityFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new SupportIncidentForm(Factory.NewWithValidTestData<SupportIncident>()))
			{
				form.Show();
				Application.DoEvents();
				form.panelSimilarIncidents.IsCollapsed = false;
				Assert(form.splitterSimilarIncidents.Visible);
				form.panelSimilarIncidents.IsCollapsed = true;
				Assert(!form.splitterSimilarIncidents.Visible);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestClientRequiredDate()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var date = new ZDateTime(2019, 11, 17);
			incident.IM_RequiredBy = date;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectedTab = ((ZTabPage)form.TopLevelTabControl_Exposed.TabPages[2]);
				AssertEquals(date.ToString("dd-MMM-yy").ToUpper(), form.ClientRequiredDate.Text);
				var dateNew = new ZDateTime(2019, 11, 18);
				form.ClientRequiredDate.Text = dateNew.ToString();
				form.ClientRequiredDate.Focus(); // to trigger the property set
				AssertEquals(dateNew, form.BusinessEntity.IM_RequiredBy);
			}
		}

		public void TestClientFullNameContainsAmpersandShouldBeKept()
		{
			var incident = Factory.New<SupportIncident>();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				var workplaceLabel = form.GetWorkplaceLabel();
				var supportClientNameLabel = form.GetSupportClientNameLabel();
				Assert("workplaceLabel should not use mnemonic", !workplaceLabel.UseMnemonic);
				Assert("supportClientNameLabel should not use mnemonic", !supportClientNameLabel.UseMnemonic);
			}
		}

		public void TestRelatedItemsRefresh_NewWorkItemInDefectRelatedWorkItemsGrid()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Something";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.Factory.Save();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();

				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				var relatedItemGrid = relatedTab.FindSingleOrDefault<ZGrid>("RelatedItemGrid");
				AssertNotNull(relatedItemGrid);
				AssertEquals(0, relatedItemGrid.VisibleRowCount);

				var workItemGrid = GetWorkItemGrid("DefectRelatedWorkItemsGrid", form, incident);
				workItemGrid.FireNewButtonClick();
				AssertNotNull(workItemGrid.LastShownZForm);
				using (var lastShownZForm = (NewWorkItemForm)workItemGrid.LastShownZForm)
				{
					var bizo = lastShownZForm.BusinessEntity as NewWorkItem;
					bizo.FillWithValidTestData();
					bizo.WKI_Summary = "Summary child 1";
					lastShownZForm.FireSaveButton();
				}
				form.FireSaveButton();

				AssertEquals(1, relatedItemGrid.VisibleRowCount);
			}
		}

		public void TestRelatedItemsRefresh_NewWorkItemInFeatureRequestWorkItemsGrid()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Something";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.Factory.Save();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();

				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				var relatedItemGrid = relatedTab.FindSingleOrDefault<ZGrid>("RelatedItemGrid");
				AssertNotNull(relatedItemGrid);
				AssertEquals(0, relatedItemGrid.VisibleRowCount);

				var workItemGrid = GetWorkItemGrid("FeatureRequestWorkItemsGrid", form, incident);
				workItemGrid.FireNewButtonClick();
				AssertNotNull(workItemGrid.LastShownZForm);
				using (var lastShownZForm = (NewWorkItemForm)workItemGrid.LastShownZForm)
				{
					var bizo = lastShownZForm.BusinessEntity as NewWorkItem;
					bizo.FillWithValidTestData();
					bizo.WKI_Summary = "Summary FeatureRequest";
					lastShownZForm.FireSaveButton();
				}
				form.FireSaveButton();

				AssertEquals(1, relatedItemGrid.VisibleRowCount);
			}
		}

		ModuleSelectionControl GetWorkItemGrid(string gridName, MockSupportIncidentForm form, SupportIncident incident)
		{
			var workItemGrid = form.Controls.Find(gridName, true)[0] as ModuleSelectionControl;
			if (workItemGrid != null)
			{
				foreach (var columnStyle in workItemGrid.ColumnStyles)
				{
					var column = columnStyle as ZTextBoxColumnStyleInfo;
					workItemGrid.InnerGrid.Columns.Add(column);
				}

				using (var stream = form.GetLayoutStream(workItemGrid.InnerGrid))
				{
					var bothVisible = Factory.New<StmModuleFilter>();
					bothVisible.S9_ModuleID = "Test";
					bothVisible.S9_ColumnLayoutData = form.GetLayoutStream(workItemGrid.InnerGrid).ToArray();
					bothVisible.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
					bothVisible.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
					bothVisible.S9_FilterName = "BothVisible";
					bothVisible.S9_SaveColumnLayout = true;
					workItemGrid.InnerGrid.CurrentColumnLayout = bothVisible;
					workItemGrid.SetDataBinding(incident, "RelatedWorkItems");
				}
			}

			return workItemGrid;
		}

		#region DB Hits

		public void TestRelatedItemsTab_DbHits_ProjectsOfWorkItems()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INC", "WKI", "WKP");
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			for (var i = 0; i < 20; i++)
			{
				var workItem = Factory.NewWithValidTestData<NewWorkItem>();
				incident.RelatedItems.Add(workItem);
				var project = Factory.NewWithValidTestData<EDIProject>();
				workItem.RelatedItems.Add(project);
				var incidentParent = Factory.NewWithValidTestData<SupportIncident>();
				incidentParent.RelatedItems.Add(incident);
			}

			Factory.Save();

			var expectedHits = new Dictionary<string, int> {
					{ GenPivotSchema.Constants.TableName, 4 },
					{ IncidentMainSchema.Constants.TableName, 20 },
					{ OrgAddressSchema.Constants.TableName, 0 },
					{ OrgHeaderSchema.Constants.TableName, 0 },
					{ ProcessHeaderSchema.Constants.TableName, 3 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ WorkItemSchema.Constants.TableName, 20 },
					{ WorkItemRequestLinkSchema.Constants.TableName, 0 },
					{ WorkProjectSchema.Constants.TableName, 0 },
					{ StmDataSchema.Constants.TableName, 8 },
			};

			var newFactory = new BusinessObjectFactory();
			var loadedIncident = newFactory.Load<SupportIncident>(incident.PK);
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true))
			{
				using (var form = new SupportIncidentForm(loadedIncident))
				{
					form.Show();
					Application.DoEvents();
					form.TopLevelTabControl_Exposed.SelectTab("RelatedItemsTabPage");
					Application.DoEvents();
				}
			}
		}

		public void TestRelatedItemsTab_DbHits_WorkItemsOfProjects()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INC", "WKI", "WKP");
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			for (var i = 0; i < 20; i++)
			{
				var project = Factory.NewWithValidTestData<EDIProject>();
				incident.RelatedItems.Add(project);
				var workItem = Factory.NewWithValidTestData<NewWorkItem>();
				project.RelatedItems.Add(workItem);
				var incidentParent = Factory.NewWithValidTestData<SupportIncident>();
				incidentParent.RelatedItems.Add(incident);
			}

			Factory.Save();

			var expectedHits = new Dictionary<string, int> {
					{ GenPivotSchema.Constants.TableName, 4 },
					{ IncidentMainSchema.Constants.TableName, 20 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgHeaderSchema.Constants.TableName, 3 },
					{ ProcessHeaderSchema.Constants.TableName, 3 },
					{ ProcessTasksSchema.Constants.TableName, 4 },
					{ WorkItemSchema.Constants.TableName, 0 },
					{ WorkItemRequestLinkSchema.Constants.TableName, 0 },
					{ WorkProjectSchema.Constants.TableName, 20 },
					{ StmDataSchema.Constants.TableName, 8 },
				};

			var newFactory = new BusinessObjectFactory();
			var loadedIncident = newFactory.Load<SupportIncident>(incident.PK);
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true))
			{
				using (var form = new SupportIncidentForm(loadedIncident))
				{
					form.Show();
					Application.DoEvents();
					form.TopLevelTabControl_Exposed.SelectTab("RelatedItemsTabPage");
					Application.DoEvents();
				}
			}
		}

		#endregion

		public void TestSetupNewIncidentMenuItem()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();
				var control = form.RelatedItemsTabPage_Exposed.Controls[0] as EDIWorkTaskRelatedItemUserControl;
				AssertNotNull(control.NewSupportIncidentMenuItem_Exposed.DropDownItems);
				control.AddUniversalCopyButton_Exposed();
				var newIncidentMenuItem = control.NewSupportIncidentMenuItem_Exposed;
				var newIncidentSubMenuItems = newIncidentMenuItem.DropDownItems.Cast<ZToolStripMenuItem>();
				AssertArrayEqualsByElements(new[] { "New", "Universal Copy" }, newIncidentSubMenuItems.Select(x => x.Text).ToArray());
			}
		}

		public void TestNotifyIfInternalItem()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "NAC";
			staff1.GS_FullName = "cdcdcd cdc";
			Factory.Save();
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var xxxMapping = product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			xxxMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			var yyyMapping = product.ModuleMappings.AddNew("YYY", "YYY Description", "PA1", true);
			yyyMapping.SourceModuleMappings.AddNew("SourceModule2", "PA2");
			yyyMapping.IsInternal = true;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var incident = Factory.New<SupportIncident>();
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
				incident.IM_Module = "XXX";
				incident.ProductArea = "PA1";
				incident.IM_SystemCreateUser = "NAC";
				Factory.Save();
				incident.IM_Module = "YYY";
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				incident.IM_Module = "XXX";
				incident.IM_SystemCreateUser = "~BP";
				Factory.Save();
				incident.IM_Module = "YYY";
				AssertEquals("You are changing the Menu Section to an Internal Item. Click Yes to proceed.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("XXX", incident.IM_Module);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				incident.IM_Module = "YYY";
				AssertEquals("You are changing the Menu Section to an Internal Item. Click Yes to proceed.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("YYY", incident.IM_Module);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSetupTaskButtonsAndEConversationControlsAndCloseIncidentControls_Performance()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, incident.WorkflowType);
			var jobHeader = helper.GetJobHeaderForParent(incident, Factory, addDefaultProcessHeaderIfNone: false);
			var maxWorkflowCount = 4;
			var maxTaskCount = 50;
			for (int i = 0; i < maxWorkflowCount; i++)
			{
				var workflow = helper.CreateWorkflow(jobHeader, $"Workflow {i}");
				for (int j = 0; j < maxTaskCount; j++)
				{
					var task = incident.WorkflowItems.Tasks.AddNew();
					task.P9_Sequence = i * 100 + j;
					task.P9_Description = $"Task {task.P9_Sequence}";
					task.P9_FH_ProcessHeader = workflow.PK;
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				}
			}

			Factory.Save();
			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			using (var form = new MockSupportIncidentForm(loadedIncident))
			{
				form.ControllerID = ClientControllerRegistration.SupportIncident;
				form.Show();
				form.TopLevelTabControl_Exposed.SelectTab("WorkflowTabPage");
				Application.DoEvents();
				loadedIncident.WorkflowItems.Tasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				loadedIncident.Factory.Save();
				AssertEquals("Should be only two calls, one on load and one for task status change", 2, form.SetupTaskButtonsCallCount);
			}
		}

		public void TestActionsMenuItems_Escalate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var incident = GetIncidentForEConversationTest();
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				var menu = GetActionsMenuItem(form, "Escala&te");
				menu.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				incident.IM_Priority = "XXX";
				AssertHasErrors(incident.IM_PriorityInfo);
				menu.PerformClick();

				var errorMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZErrorMessageBoxForTest;
				AssertNotNull(errorMessageBox);
				AssertEquals("There are errors that need to be corrected before this Incident can be Escalated.", errorMessageBox.MessageMultilingual.ToString());
				Assert(errorMessageBox.DetailsTextWithColumnNames_Exposed.Contains("Enter a valid Criticality"));
			}
		}

		[TestDate]
		public void TestActionsMenuItems_ServiceOutageStatus()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00000001";

			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				form.FireSaveButton();
				var serviceOutageStatusMenu = GetActionsMenuItem(form, "Service Outage Status");
				AssertEquals(3, serviceOutageStatusMenu.MenuItems.Count);

				AssertServiceLogsLengthAndLastServiceLog(0, null, form);
				AssertMenusEnabled(true, false, false, form);
				AssertServiceStatusAndOutageDuration("", "", form);

				TestDateAttribute.Date = new DateTime(2023, 9, 26, 12, 0, 0);
				using (var eventLogForm = form.CreateEventLogForm(AutoEvents.ServiceSuspendedCode, "DES", "Service outage has started"))
				{
					ShowFormAndSave(f => form.CreateEventLogForm(AutoEvents.ServiceSuspendedCode, "DES", "Service outage has started"), form);

					AssertServiceLogsLengthAndLastServiceLog(1, AutoEvents.ServiceSuspendedCode, form);
					AssertMenusEnabled(false, true, true, form);
					AssertLabelsColor(Color.Red, Color.Red, form);

					form.FireSaveButton();
					AssertServiceStatusAndOutageDuration("Active Service Outage", "00:00:00", form);
				}

				TestDateAttribute.Date = new DateTime(2023, 9, 26, 12, 1, 0);
				using (var eventLogForm = form.CreateEventLogForm(AutoEvents.ServiceCommencedCode, "DES", "Service has been restored"))
				{
					ShowFormAndSave(f => form.CreateEventLogForm(AutoEvents.ServiceCommencedCode, "DES", "Service outage has started"), form);

					AssertServiceLogsLengthAndLastServiceLog(2, AutoEvents.ServiceCommencedCode, form);
					AssertMenusEnabled(true, false, true, form);
					AssertLabelsColor(Color.Black, Color.Black, form);

					form.FireSaveButton();
					AssertServiceStatusAndOutageDuration("Service Restored", "00:01:00", form);
				}

				TestDateAttribute.Date = new DateTime(2023, 9, 26, 12, 1, 30);
				using (var eventLogForm = form.CreateEventLogForm(AutoEvents.ServiceSuspendedCode, "DES", "Service outage has started"))
				{
					ShowFormAndSave(f => form.CreateEventLogForm(AutoEvents.ServiceSuspendedCode, "DES", "Service outage has started"), form);

					AssertServiceLogsLengthAndLastServiceLog(3, AutoEvents.ServiceSuspendedCode, form);
					AssertMenusEnabled(false, true, true, form);
					AssertLabelsColor(Color.Red, Color.Red, form);

					form.FireSaveButton();
					AssertServiceStatusAndOutageDuration("Active Service Outage", "00:01:00", form);
				}

				form.ClearAllServiceEventsMenuItemForTest.PerformClick();
				form.FireSaveButton();

				AssertServiceLogsLengthAndLastServiceLog(0, null, form);
				AssertMenusEnabled(true, false, false, form);

				form.FireSaveButton();
				AssertServiceStatusAndOutageDuration("", "", form);
			}
		}

		void ShowFormAndSave(Func<SupportIncidentFormForTest, Form> createFormAction, SupportIncidentFormForTest form)
		{
			using (var eventLogForm = createFormAction(form) as ZStmALogAddForm)
			{
				eventLogForm.Show();
				var addEventButton = (ZButton)eventLogForm.Controls["FlowLayoutPanel"].Controls["AddButton"];
				addEventButton.PerformClick();
				form.FireSaveButton();
			}
		}

		void AssertServiceLogsLengthAndLastServiceLog(int expectedLogLength, string expectedLastServiceLog, SupportIncidentFormForTest form)
		{
			var allServiceLogs = form.BusinessEntity.GetAllServiceLogs();
			var lastServiceLog = form.BusinessEntity.GetLastServiceLog();

			AssertEquals("Service log length is incorrect", expectedLogLength, allServiceLogs.Length);
			AssertEquals("Last service log is incorrect", expectedLastServiceLog, lastServiceLog);
		}

		void AssertMenusEnabled(bool expectedAddServiceOutageStartEventEnabled, bool expectedAddServiceRestoredEventEnabled, bool expectedClearAllServiceEventsEnabled, SupportIncidentFormForTest form)
		{
			AssertEquals("Add service outage start event menu enabled state is incorrect", expectedAddServiceOutageStartEventEnabled, form.AddServiceOutageStartEventMenuItemForTest.Enabled);
			AssertEquals("Add service restored event menu enabled state is incorrect", expectedAddServiceRestoredEventEnabled, form.AddServiceRestoredEventMenuItemForTest.Enabled);
			AssertEquals("Clear all service events menu enabled state is incorrect", expectedClearAllServiceEventsEnabled, form.ClearAllServiceEventsMenuItemForTest.Enabled);
		}

		void AssertLabelsColor(Color expectedServiceStatusLabelColor, Color expectedOutageDurationLabelColor, SupportIncidentFormForTest form)
		{
			AssertEquals("ServiceStatusLabel color is incorrect", expectedServiceStatusLabelColor, form.ServiceStatusLabelForTest.ForeColor);
			AssertEquals("OutageDurationLabel color is incorrect", expectedOutageDurationLabelColor, form.OutageDurationLabelForTest.ForeColor);
		}

		void AssertServiceStatusAndOutageDuration(string expectedServiceStatus, string expectedOutageDuration, SupportIncidentFormForTest form)
		{
			AssertEquals("ServiceStatus is incorrect", expectedServiceStatus, form.BusinessEntity.ServiceStatus);
			AssertEquals("OutageDuration is incorrect", expectedOutageDuration, form.BusinessEntity.OutageDuration);
		}

		public void TestServiceType_Visibility()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			areas.AddPair("ARC", "ARC");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);
			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			product1.ModuleMappings.AddNew("SAA", "XXX Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			var module = product.ServiceTypeModuleMappings.AddNew("SAA", "Module XRM", "XRM", false);
			module.ServiceTypeMappings.AddNew("SIM");
			EDIDataRegistry.Instance.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.ProductArea = "XRM";
			incident.IM_Module = "SAA";
			incident.IM_ServiceType = "SIM";
			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert(form.ServiceTypeCaption_Exposed.Visible);
				Assert(form.ServiceTypeDropEdit_Exposed.Visible);
				Assert(!incident.IM_ServiceType.IsEmpty);
				incident.IM_Module = "AAA";
				Assert(!form.ServiceTypeCaption_Exposed.Visible);
				Assert(!form.ServiceTypeDropEdit_Exposed.Visible);
				Assert(incident.IM_ServiceType.IsEmpty);
			}
		}

		public void TestSRNumValue_TextBox_ReadOnly()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			using (SupportIncidentForm form = new SupportIncidentForm(incident))
			{
				Assert(((TextBox)form.Controls.Find("SRNumValue", true)[0]).GetReadOnly());
			}
		}

		public void TestSource_DropEdit_ReadOnly()
		{
			SupportIncident incidentINC = Factory.New<SupportIncident>();
			incidentINC.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			using (SupportIncidentForm form = new SupportIncidentForm(incidentINC))
			{
				form.Show();
				Assert("sourceDropEdit should be readOnly when source is eRequest Portal", ((ZDropEdit)form.Controls.Find("sourceDropEdit", true)[0]).GetReadOnly());
				form.Dispose();
			}
			SupportIncident incidentISS = Factory.New<SupportIncident>();
			incidentISS.IM_Source = SupportIncidentLookups.SourceListConstants.IssueManagerReported;
			using (SupportIncidentForm form = new SupportIncidentForm(incidentISS))
			{
				form.Show();
				Assert("sourceDropEdit should be readOnly when source is Created From Issue Manager", ((ZDropEdit)form.Controls.Find("sourceDropEdit", true)[0]).GetReadOnly());
				form.Dispose();
			}
			SupportIncident incidentPRJ = Factory.New<SupportIncident>();
			incidentPRJ.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			using (SupportIncidentForm form = new SupportIncidentForm(incidentPRJ))
			{
				form.Show();
				Assert("sourceDropEdit should be readOnly when source is Created From Project", ((ZDropEdit)form.Controls.Find("sourceDropEdit", true)[0]).GetReadOnly());
				form.Dispose();
			}
			SupportIncident incidentAPI = Factory.New<SupportIncident>();
			incidentAPI.IM_Source = SupportIncidentLookups.SourceListConstants.APIInboundInternal;
			using (SupportIncidentForm form = new SupportIncidentForm(incidentAPI))
			{
				form.Show();
				Assert("sourceDropEdit should be readOnly when source is API Inbound (Internal)", ((ZDropEdit)form.Controls.Find("sourceDropEdit", true)[0]).GetReadOnly());
				form.Dispose();
			}
			SupportIncident incidentAPE = Factory.New<SupportIncident>();
			incidentAPE.IM_Source = SupportIncidentLookups.SourceListConstants.APIInboundExternal;
			using (SupportIncidentForm form = new SupportIncidentForm(incidentAPE))
			{
				form.Show();
				Assert("sourceDropEdit should be readOnly when source is API Inbound (External)", ((ZDropEdit)form.Controls.Find("sourceDropEdit", true)[0]).GetReadOnly());
				form.Dispose();
			}
			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.IM_Source = SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd;
			using (SupportIncidentForm form = new SupportIncidentForm(incident2))
			{
				form.Show();
				Assert("sourceDropEdit should not be readOnly when source is WTG Internal via ediProd", !((ZDropEdit)form.Controls.Find("sourceDropEdit", true)[0]).GetReadOnly());
				form.Dispose();
			}
		}

		public void TestSyncDetailToERequestMenuItemShouldNotBeEnabledForExternalRequest()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			var externalLic = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "DB2");
			var reportingContact = externalLic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = staff1.GS_FullName;
			reportingContact.OC_Email = staff1.GS_EmailAddress;

			var externalRequest = Factory.New<IncidentRequest>();
			externalRequest.INC_OC_ReportedBy = reportingContact.PK;
			externalRequest.INC_Criticality = "CR4";
			externalRequest.INC_Summary = "stuff happened again";
			externalRequest.INC_Details = "how stuff keep happening?";
			externalRequest.INC_OC_ApprovedBy = reportingContact.PK;
			externalRequest.INC_Type = "ENT";
			externalRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			externalRequest.INC_SubType = "RCB";
			externalRequest.INC_Area = "AREnquiry";
			externalRequest.INC_ProductLicence = "EN2CO2DB2";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			SupportIncident externalIncident =
				Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request,
					externalRequest.PK));

			using (Env.SetTemporaryUserContext(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (var form = new SupportIncidentForm(externalIncident))
				{
					form.Show();
					var menu = GetActionsMenuItem(form, "Sync Details to eRequest");
					Assert(!menu.Enabled);
				}
			}
		}

		public void TestSyncDetailToERequestMenuItemShouldNotBeEnabledForUsersOtherThanAuthor()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var internalLic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			internalLic.Database.LicEnterprise.LE_IsInternal = true;
			var contact = internalLic.Company.Header.Contacts.AddNew();
			contact.OC_ContactName = staff1.GS_FullName;
			contact.OC_Email = staff1.GS_EmailAddress;

			var internalRequest = Factory.New<IncidentRequest>();
			internalRequest.INC_OC_ReportedBy = contact.PK;
			internalRequest.INC_Criticality = "CR5";
			internalRequest.INC_Summary = "stuff happened";
			internalRequest.INC_Details = "how stuff happen?";
			internalRequest.INC_OC_ApprovedBy = contact.PK;
			internalRequest.INC_Type = "ENT";
			internalRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			internalRequest.INC_SubType = "REF";
			internalRequest.INC_Area = "Organisation";
			internalRequest.INC_ProductLicence = "ENTCOMDB1";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			SupportIncident supportIncident =
				Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request,
					internalRequest.PK));

			using (Env.SetTemporaryUserContext(staff2.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (var form = new SupportIncidentForm(supportIncident))
				{
					form.Show();
					var menu = GetActionsMenuItem(form, "Sync Details to eRequest");
					Assert(!menu.Enabled);
				}
			}
		}

		public void TestSyncDetailToERequestMenuItemWillSynchronise()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "joe@test.org";
			staff1.GS_FullName = "Joe Staff";
			staff1.GS_Code = "JOE";

			var internalLic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			internalLic.Database.LicEnterprise.LE_IsInternal = true;
			var contact = internalLic.Company.Header.Contacts.AddNew();
			contact.OC_ContactName = staff1.GS_FullName;
			contact.OC_Email = staff1.GS_EmailAddress;

			var internalRequest = Factory.New<IncidentRequest>();
			internalRequest.INC_OC_ReportedBy = contact.PK;
			internalRequest.INC_Criticality = "CR5";
			internalRequest.INC_Summary = "stuff happened";
			internalRequest.INC_Details = "how stuff happen?";
			internalRequest.INC_OC_ApprovedBy = contact.PK;
			internalRequest.INC_Type = "ENT";
			internalRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			internalRequest.INC_SubType = "REF";
			internalRequest.INC_Area = "Organisation";
			internalRequest.INC_ProductLicence = "ENTCOMDB1";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();
			SupportIncident supportIncident =
					Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request,
						internalRequest.PK));

			using (Env.SetTemporaryUserContext(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					   GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using (var form = new SupportIncidentForm(supportIncident))
				{
					form.Show();
					var menu = GetActionsMenuItem(form, "Sync Details to eRequest");
					Assert(menu.Enabled);
					supportIncident.IM_Description = "New description";
					supportIncident.DetailNoteText = "New Details";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					menu.PerformClick();
					form.FireSaveButton();
					var updateIncidentRequest = Factory.LoadTop1<IncidentRequest>(new ZQuery(IncidentRequestSchema.PK,
						internalRequest.PK));
					AssertEquals("New description", updateIncidentRequest.INC_Summary);
					AssertEquals("New Details", updateIncidentRequest.INC_Details);
				}
			}
		}

		public void TestCheckSpellingItemCount()
		{
			var incident = Factory.New<SupportIncident>();
			Factory.Save();
			var controller = new IncidentConversationViewController(false);
			using (var form = new SupportIncidentForm(incident))
			{
				controller.Initialize(form);
				controller.TextBoxForTest.Text = "helloooo world";

				AssertEquals("Only has one Check Spell Item", 1, controller.TextBoxForTest.ContextMenuStrip.Items.Find("checkSpelling", false).Length);
			}
		}

		public void TestLaunchContentFinderMenuItem()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "newuser2";
			staff.GS_Code = "NE2";
			var url = "https://weburl.com.au";
			var supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			incident.IM_Description = "New incident description 1";
			incident.DetailNoteText = "Detail note 2";
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = ProductAreaList.Codes.ARC;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily("newuser2"))
			{
				using (EDIDataRegistry.Instance.ContentFinderUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url))
				{
					using (EDIDataRegistry.Instance.EnableContentFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						using (var form = new SupportIncidentForm(incident))
						{
							form.Show();
							var menu = GetActionsMenuItem(form, "Launch Content Finder");
							Assert(!menu.Enabled);
							form.Dispose();
						}
					}

					using (EDIDataRegistry.Instance.EnableContentFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (var form = new SupportIncidentForm(incident))
					{
						form.Show();
						var menu = GetActionsMenuItem(form, "Launch Content Finder");
						Assert(menu.Enabled);

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						menu.PerformClick();
						form.FireSaveButton();

						Assert(WebUrlLauncher.LastUrlLaunched.StartsWith($"{url}?jwt="));

						var token = WebUrlLauncher.LastUrlLaunched.Replace($"{url}?jwt=", "");
						var securityToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

						var payload = securityToken.Payload;
						AssertEquals("Payload should contains AssignedStaffCode", payload["userStaffCode"], "NE2");
						AssertEquals("Payload should contains IM_IncidentNumber", payload["incidentNumber"], incident.IM_IncidentNumber);
						AssertEquals("Payload should contains IM_Description", payload["summary"], incident.IM_Description);
						AssertEquals("Payload should contains DetailNoteText", payload["details"], incident.DetailNoteText);
						AssertEquals("Payload should contains IM_Product", payload["product"], incident.IM_Product);
						AssertEquals("Payload should contains ProductArea", payload["productArea"], incident.ProductArea);
						AssertEquals("Payload should contains Criticality", payload["criticality"], incident.Criticality);
						AssertEquals("Payload should contains IM_Module", payload["module"], incident.IM_Module);
						AssertEquals("Payload should contains IM_IncidentType", payload["incidentType"], incident.IM_IncidentType);
						AssertEquals("Payload should contains IM_Language", payload["language"], incident.IM_Language);
					}
				}
			}
		}

		public void TestTriageAssistButtonVisibility()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			using (EDIDataRegistry.Instance.EnableTriageEngineModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new SupportIncidentForm(incident))
				{
					form.Show();
					var triageAssistButton = form.Controls.Find("TriageAssistButton", true).FirstOrDefault();
					AssertEquals("Should not show button since triage engine is not enabled", false, triageAssistButton.Visible);
					form.Dispose();
				}
			}

			using (EDIDataRegistry.Instance.EnableTriageEngineModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new SupportIncidentForm(incident))
				{
					form.Show();
					var triageAssistButton = form.Controls.Find("TriageAssistButton", true).FirstOrDefault();
					AssertEquals("Should show button since triage engine is enabled", true, triageAssistButton.Visible);
					form.Dispose();
				}
			}
		}

		public void TestTriageLabelVisibility()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			using (EDIDataRegistry.Instance.EnableTriageEngineModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new SupportIncidentForm(incident))
				{
					form.Show();
					var triageAssistCaption = form.Controls.Find("triageAssistCaption", true).FirstOrDefault();
					var triageAssistLabel = form.Controls.Find("triageAssistLabel", true).FirstOrDefault();
					AssertEquals("Should not show triageAssistCaption since triage engine is not enabled", false, triageAssistCaption.Visible);
					AssertEquals("Should not show triageAssistLabel since triage engine is not enabled", false, triageAssistLabel.Visible);
				}
			}

			using (EDIDataRegistry.Instance.EnableTriageEngineModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new SupportIncidentForm(incident))
				{
					form.Show();
					var triageAssistCaption = form.Controls.Find("triageAssistCaption", true).FirstOrDefault();
					var triageAssistLabel = form.Controls.Find("triageAssistLabel", true).FirstOrDefault();
					AssertEquals("Should show triageAssistCaption since triage engine is enabled", true, triageAssistCaption.Visible);
					AssertEquals("Should show triageAssistLabel since triage engine is enabled", true, triageAssistLabel.Visible);
				}
			}
		}

		public void TestTriageAssistButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var triageAssistButton = (ZButton)form.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(form.TriageForm);
				triageAssistButton.PerformClick();
				AssertNotNull(form.TriageForm);
				AssertEquals(true, form.TriageForm.Visible);
				form.TriageForm.Close();
				form.Dispose();
			}
		}

		public void TestOverrideMenuItemButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var overrideSourceModuleButton = (ZButton)form.Controls.Find("OverrideSourceModuleButton", true).FirstOrDefault();
				overrideSourceModuleButton.PerformClick();
				AssertEquals("Should show source module finder form", typeof(SourceModuleFinderForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				form.Dispose();
			}
		}

		public void TestCloseIncident_DispositionPreviouslyClosed_ShouldNotShowCloseIncidentPopupForm()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident1.IM_Status = IncidentMainLookups.Status.Closed;
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident2.IM_Status = IncidentMainLookups.Status.Closed;
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.DuplicateIncident;
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident3.IM_Status = IncidentMainLookups.Status.Closed;
			incident3.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident4.IM_Status = IncidentMainLookups.Status.Closed;

			var closureDispositions = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			Assert("Precondition: Disposition should be closed", closureDispositions.ContainsCode(incident1.IM_ResolutionCode));
			Assert("Precondition: Disposition should be closed", closureDispositions.ContainsCode(incident2.IM_ResolutionCode));

			var newTask1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident1.WorkflowItems.Add(newTask1);
			newTask1.P9_ParentID = incident1.PK;
			newTask1.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident2.WorkflowItems.Add(newTask2);
			newTask2.P9_ParentID = incident2.PK;
			newTask2.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			var newTask3 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask3.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident3.WorkflowItems.Add(newTask3);
			newTask3.P9_ParentID = incident3.PK;
			newTask3.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			var newTask4 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask4.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident4.WorkflowItems.Add(newTask4);
			newTask4.P9_ParentID = incident4.PK;
			newTask4.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Closed", IncidentMainLookups.Status.Closed, incident1.IM_Status);
			AssertEquals("Precondition: Should be Closed", IncidentMainLookups.Status.Closed, incident2.IM_Status);
			AssertEquals("Precondition: Should be Closed", IncidentMainLookups.Status.Closed, incident3.IM_Status);
			AssertEquals("Precondition: Should still be open", IncidentMainLookups.Status.Open, incident4.IM_Status);
			AssertEquals("Precondition: Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, incident1.IM_ResolutionCode);
			AssertEquals("Precondition: Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.DuplicateIncident, incident2.IM_ResolutionCode);
			AssertEquals("Precondition: Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident3.IM_ResolutionCode);
			AssertEquals("Precondition: Disposition should not be closed", SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident4.IM_ResolutionCode);

			using (var form = new SupportIncidentForm(incident1))
			{
				form.Show();
				newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Should not popup when incident disposition was already closed internal", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, incident1.IM_ResolutionCode);
				AssertEquals("Status should be updated to Closed", IncidentMainLookups.Status.Closed, incident1.IM_Status);
				form.Dispose();
			}

			using (var form = new SupportIncidentForm(incident2))
			{
				form.Show();
				newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Should not popup when incident disposition was already closed awaiting client response", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.DuplicateIncident, incident2.IM_ResolutionCode);
				AssertEquals("Status should be updated to Closed", IncidentMainLookups.Status.Closed, incident2.IM_Status);
				form.Dispose();
			}

			using (var form = new SupportIncidentForm(incident3))
			{
				form.Show();
				newTask3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Should not popup when incident disposition was already resolved", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident3.IM_ResolutionCode);
				AssertEquals("Status should be updated to Closed", IncidentMainLookups.Status.Closed, incident3.IM_Status);
				form.Dispose();
			}

			using (var form = new SupportIncidentForm(incident4))
			{
				form.Show();
				newTask4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertEquals("Should popup since disposition was not closed", typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				form.Dispose();
			}
		}

		public void TestCloseIncident_DeletingLastActiveWorkFlowTask_CompletePopupForm_ShouldClose()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.WorkflowItems.Add(newTask);
			newTask.P9_ParentID = incident.PK;
			newTask.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertNullOrEmpty("Precondition: Should not have closure reason", incident.IM_ClosureResolution);
			AssertEquals("Precondition: Should have one task", 1, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var closeIncidentForm = dialog as CloseIncidentPopupForm;
					if (closeIncidentForm != null)
					{
						var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
						action.PostIRSEvent = true;
						action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
						action.SynchroniseToIncident();
						closeIncidentForm.Close();
					}
				});
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals(1, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, loadIncident.IM_ClosureResolution);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseIncident_DeletinglastActiveWorkFlowTask_CancelPopupForm_ShouldNotClose()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(newTask);
			newTask.P9_ParentID = incident.PK;
			newTask.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertNullOrEmpty("Precondition: Should not have closure reason", incident.IM_ClosureResolution);
			AssertEquals("Precondition: Should have one task", 1, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals(0, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Open, loadIncident.IM_Status);
			AssertNullOrEmpty(loadIncident.IM_ClosureResolution);
			AssertEquals(1, loadIncident.WorkflowItems.Count);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseIncident_DeletingLastActiveWorkFlowTaskWithManyTasks_CompletePopupForm_ShouldClose()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.WorkflowItems.Add(newTask1);
			newTask1.P9_ParentID = incident.PK;
			newTask1.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.WorkflowItems.Add(newTask2);
			newTask2.P9_ParentID = incident.PK;
			newTask2.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertNullOrEmpty("Precondition: Should not have closure reason", incident.IM_ClosureResolution);
			AssertEquals("Precondition: Should have two tasks", 2, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var closeIncidentForm = dialog as CloseIncidentPopupForm;
					if (closeIncidentForm != null)
					{
						var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
						action.PostIRSEvent = true;
						action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
						action.SynchroniseToIncident();
						closeIncidentForm.Close();
					}
				});
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask1);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals(1, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, loadIncident.IM_ClosureResolution);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseIncident_DeletingLastActiveWorkFlowTaskWithManyTasks_CancelPopupForm_ShouldNotClose()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.WorkflowItems.Add(newTask1);
			newTask1.P9_ParentID = incident.PK;
			newTask1.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.WorkflowItems.Add(newTask2);
			newTask2.P9_ParentID = incident.PK;
			newTask2.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertNullOrEmpty("Precondition: Should not have closure reason", incident.IM_ClosureResolution);
			AssertEquals("Precondition: Should have two tasks", 2, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask1);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals(0, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Open, loadIncident.IM_Status);
			AssertNullOrEmpty(loadIncident.IM_ClosureResolution);
			AssertEquals(2, loadIncident.WorkflowItems.Count);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseIncident_DeletingWorkFlowTaskWithManyOpenTasks_ShouldNotShowCloseIncidentPopup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(newTask1);
			newTask1.P9_ParentID = incident.PK;
			newTask1.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(newTask2);
			newTask2.P9_ParentID = incident.PK;
			newTask2.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertNullOrEmpty("Precondition: Should not have closure reason", incident.IM_ClosureResolution);
			AssertEquals("Precondition: Should have two tasks", 2, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask1);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			AssertEquals(SupportIncidentLookups.Status.Open, loadIncident.IM_Status);
			AssertNullOrEmpty(loadIncident.IM_ClosureResolution);

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("Should not show close incident popup since there is another open task", null, ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestCloseIncident_DeletingClosedWorkFlowTask_ShouldNotShowCloseIncidentPopup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.WorkflowItems.Add(newTask1);
			newTask1.P9_ParentID = incident.PK;
			newTask1.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.WorkflowItems.Add(newTask2);
			newTask2.P9_ParentID = incident.PK;
			newTask2.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertNullOrEmpty("Precondition: Should not have closure reason", incident.IM_ClosureResolution);
			AssertEquals("Precondition: Should have two tasks", 2, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask1);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			AssertEquals(SupportIncidentLookups.Status.Open, loadIncident.IM_Status);
			AssertNullOrEmpty(loadIncident.IM_ClosureResolution);

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("Should not show close incident popup since there is another open task", null, ZFormModaliser.LastFormShownDialogForTest);

			var loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals(0, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Open, loadIncident.IM_Status);
			AssertNullOrEmpty(loadIncident.IM_ClosureResolution);
			AssertEquals(1, loadIncident.WorkflowItems.Count);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestDeletingWorkFlowTask_IncidentAlreadyClosed_ShouldNotShowCloseIncidentPopup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var newTask1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.WorkflowItems.Add(newTask1);
			newTask1.P9_ParentID = incident.PK;
			newTask1.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			incident.WorkflowItems.Add(newTask2);
			newTask2.P9_ParentID = incident.PK;
			newTask2.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Precondition: Should be Open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertEquals("Precondition: Should have two tasks", 2, incident.WorkflowItems.Count);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var closeIncidentForm = dialog as CloseIncidentPopupForm;
					if (closeIncidentForm != null)
					{
						var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
						action.PostIRSEvent = true;
						action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
						action.SynchroniseToIncident();
					}
				});
				newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			AssertEquals("Precondition: Should be Closed", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Precondition: Should have two tasks", 2, incident.WorkflowItems.Count);

			var loadIncidentLogs = incident.Logs.GetAllLogs();
			var irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals("Precondition: irs logs should be sent", 1, irsLogs.Count());

			AssertEquals("Precondition: Popup should be shown", typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ZFormModaliser.LastFormShownDialogForTest = null;

			AssertEquals("Precondition: LastForm should be rest to null to test delete", null, ZFormModaliser.LastFormShownDialogForTest);

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask1);
				incident.WorkflowItems.Tasks.RemoveAndDelete(newTask2);
			}

			Factory.Save();

			var loadIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));

			AssertEquals("Should not show close incident popup since incident is already closed", null, ZFormModaliser.LastFormShownDialogForTest);

			loadIncidentLogs = loadIncident.Logs.GetAllLogs();
			irsLogs = loadIncidentLogs.Find(x => x.SL_SE_NKEvent == "IRS");
			AssertEquals("irs logs should not be sent again", 1, irsLogs.Count());

			AssertEquals(SupportIncidentLookups.Status.Closed, loadIncident.IM_Status);
			AssertEquals(0, loadIncident.WorkflowItems.Count);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestSetupEConversationControlsAndCloseControls_CurrentUserTasksStatus()
		{
			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			var task2 = incident.WorkflowItems.Tasks.AddNew();

			task1.P9_Sequence = 1;
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = "CAN";

			task2.P9_Sequence = 2;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = "WRK";
			Factory.Save();

			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert(incident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().FirstOrDefault(x => x.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code).P9_Status == "CAN");
				Assert(incident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Any(x => x.P9_Status == ProcessTaskStatusCodeList.Codes.Working && x.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code));

				Assert("eConversation should be enabled because current user has active task", form.GetConversationMessageTextBox().Enabled);
			}

			task2.P9_Status = "CAN";
			Factory.Save();

			using (var form = new MockSupportIncidentForm(incident))
			{
				form.Show();
				Assert(incident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().FirstOrDefault(x => x.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code).P9_Status == "CAN");
				Assert(!incident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Any(x => x.P9_Status == ProcessTaskStatusCodeList.Codes.Working && x.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code));

				Assert("eConversation should be enabled because current user has no active task", !form.GetConversationMessageTextBox().Enabled);
			}
		}

		public void TestEConversation_CancelShouldRevertAction()
		{
			var incident = GetIncidentForEConversationTest();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Description = "Task 1";
			task.P9_Type = "UDF";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				AssertEquals("Pre-condition", false, incident.HasChanges);
				AssertEquals("Pre-condition", "&New", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
				AssertEquals("Pre-condition", false, form.ConversationMessageTextBox.Enabled);

				form.BusinessEntity.WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				AssertEquals("Conversation TextBox should be enable", true, form.ConversationMessageTextBox.Enabled);

				form.ConversationMessageTextBox.Text = "blah blah";
				AssertEquals("Incident has changes", true, incident.HasChanges);
				AssertEquals("Save button is enabled", true, ((IPostingButtonsProvider)form).CommandButtonApply.Enabled);
				AssertEquals("Save button has correct text", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var pendingEConversationForm = dialog as SendEConversationFormForTest;
					if (pendingEConversationForm != null)
					{
						pendingEConversationForm.ClickCancel();
					}
				});

				AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);

				form.BusinessEntity.WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

				AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
				AssertEquals("Conversation TextBox should be enable once status reverted to WRK", true, form.ConversationMessageTextBox.Enabled);
				AssertType(typeof(SendEConversationFormForTest), ZFormModaliser.LastFormShownDialogForTest);

				AssertEquals("Should roll back to WRK if the conversation form is cancelled", SupportIncidentLookups.Status.Working, incident.IM_Status);
				AssertEquals("Should roll back to WRK if the conversation form is cancelled", ProcessTaskStatusCodeList.Codes.Working, form.BusinessEntity.WorkflowItems[0].P9_Status);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestShowClosePopupForm_WhenLastWRKTaskIsClosed_ShouldNotPostingCloseAsMsgTwice()
		{
			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;
			var incident = GetIncidentForEConversationTest();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incident.IM_LD = Guid.Empty;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd;
			incident.IM_Product = ProductTypes.Codes.BorderWise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Other;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;

			var task = incident.WorkflowItems.AddNew();
			task.P9_Description = "Task 1";
			task.P9_Type = "UDF";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = "WRK";
			Factory.Save();

			bool isPendingEConversationPopupShown = false;
			using (var form = new SupportIncidentFormForTest(incident))
			{
				form.Show();
				Assert(incident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Any(x => x.P9_Status == ProcessTaskStatusCodeList.Codes.Working && x.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code));
				Assert("eConversation should be enabled because current user has active task", form.ConversationMessageTextBox.Enabled);

				form.ConversationMessageTextBox.Text = "blah blah";
				AssertEquals("Incident has changes", true, incident.HasChanges);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var pendingEConversationForm = dialog as SendEConversationFormForTest;
					if (pendingEConversationForm != null)
					{
						isPendingEConversationPopupShown = true;
						pendingEConversationForm.ClickSend();
					}

					var closeIncidentForm = dialog as CloseIncidentPopupForm;
					if (closeIncidentForm != null)
					{
						var action = ((SupportIncidentCloseAction)closeIncidentForm.BusinessEntity);
						action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.NotCustomerServiceRequest;
						action.SynchroniseToIncident();
						closeIncidentForm.Close();
					}
				});

				AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);

				task.P9_Status = "CLS";
				Assert("Should be pending eConversation message", isPendingEConversationPopupShown);

				AssertEquals("Closing the last task should not publish a system message", 0, incident.EConversation.GetTimeOrderedMessages().Count(x => x.Body == "Closed As Completed"));
				AssertEquals("Only closing the Incident should publish a system message", 1, incident.EConversation.GetTimeOrderedMessages().Count(x => x.Body == "Closed As Not a Service Request"));
				AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertType(typeof(CloseIncidentPopupForm), ZFormModaliser.LastFormShownDialogForTest);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
		}

		public void TestShowCloseIncidentPopup_NullRef()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			int closeIncidentEventHandlerCallCount = 0;
			incident.OnCloseIncident += (s, e) =>
			{
				closeIncidentEventHandlerCallCount++;
				if (closeIncidentEventHandlerCallCount > 1)
				{
					Fail("Close incident form should only popup once");
				}
			};
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("Close incident event handler should only be called once", 1, closeIncidentEventHandlerCallCount);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			}
		}

		public void TestOnTriageAssistSaved_OverrideProductClassification()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Module = "ADV";
			incident.IM_Priority = "CR7";
			incident.IM_Product = "ENT";
			incident.ProductArea = "MDM";
			incident.IM_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = "SPT";
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				parentIncidentForm.TriageAssistButton.PerformClick();
				using (var triageForm = parentIncidentForm.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals(incident.IM_IMT_Triage, triage1.PK);
					AssertEquals(incident.ProductArea, triage1.IMT_ProductArea);
					AssertEquals(incident.IM_Product, triage1.IMT_Product);
					AssertEquals(incident.IM_Module, triage1.IMT_Module);
				}
			}
		}

		public void TestOnTriageAssistSaved_OverrideProductClassification_SourceModuleFinder_DefaultMapping()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR8";
			incident.ProductArea = "ARC";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			triage1.IMT_Module = "AAA";
			triage1.IMT_Product = "ENT";
			triage1.IMT_ProductArea = "";
			triage1.IMT_SetProductAreaByMenuItem = true;
			Factory.Save();

			incident.ProductArea = "";

			var defaultMapping = EDIDataRegistry.Instance.SystemProductMappings.Value.GetProductByCode("ENT")
				.ModuleMappings.OfType<ProductAreaModuleMapping>().FirstOrDefault(x => x.ModuleCode == "AAA" && x.IsEnabled);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				parentIncidentForm.TriageAssistButton.PerformClick();
				using (var triageForm = parentIncidentForm.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertNull(ZFormModaliser.LastFormShownDialogForTest);

					AssertEquals(incident.IM_IMT_Triage, triage1.PK);
					AssertEquals(incident.ProductArea, defaultMapping.ProductArea);
					AssertEquals(incident.IM_Product, triage1.IMT_Product);
					AssertEquals(incident.IM_Module, triage1.IMT_Module);
					AssertEquals(incident.IM_SourceModuleId, "N/A");
				}
			}
		}

		public void TestOnTriageAssistSaved_OverrideProductClassification_SourceModuleFinder()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			productAreas.AddPair("PA3", "Product Area 3");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			var moduleMapping = product.ModuleMappings.AddNew("YYY", "YYY Description", "PA2", true);
			moduleMapping.SourceModuleMappings.AddNew("SourceModule1", "PA1");
			moduleMapping.SourceModuleMappings.AddNew("SourceModule2", "PA2");
			product.ModuleMappings.AddNew("ZZZ", "XXX Description", "PA3", true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "Menu Item A", "", ModuleListType.Cr9, "PA1", true, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Service;
			triage1.IMT_Module = "YYY";
			triage1.IMT_Product = "ENT";
			triage1.IMT_ProductArea = "";
			triage1.IMT_SetProductAreaByMenuItem = true;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();

			using (ZFormModaliser.SuspendDispose())
			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				incident.IM_Module = "";
				incident.ProductArea = "";

				parentIncidentForm.TriageAssistButton.PerformClick();
				using (var triageForm = parentIncidentForm.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					using (var shownForm = ZFormModaliser.LastFormShownDialogForTest)
					{
						AssertType(typeof(SourceModuleFinderForm), shownForm);
						var sourceModuleFinderForm = (SourceModuleFinderForm)shownForm;
						var sourceModuleFinder = (SourceModuleFinder)sourceModuleFinderForm.LastDataSourceForTest;
						AssertNotNull("sourceModuleFinder", sourceModuleFinder);

						AssertEquals(1, sourceModuleFinder.SourceModules.Count);
						sourceModuleFinderForm.Show();
						sourceModuleFinderForm.SourceModuleGrid.Refresh();

						sourceModuleFinderForm.SourceModuleGrid.Select(0);
						sourceModuleFinderForm.OkButton_Click(this, EventArgs.Empty);
						AssertEquals("YYY", incident.IM_Module);
						AssertEquals("PA1", incident.ProductArea);
						AssertEquals("SourceModule1", incident.IM_SourceModuleId);
					}
				}
			}
		}

		public void TestOnTriageAssistSaved_OverrideProductClassification_IM_SourceModuleId()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR8";
			incident.ProductArea = "ARC";
			incident.IM_SourceModuleId = "SourceModule2";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Service;
			triage1.IMT_Module = "AAA";
			triage1.IMT_Product = "ENT";
			triage1.IMT_ProductArea = "";
			triage1.IMT_SetProductAreaByMenuItem = true;
			Factory.Save();

			var defaultMapping = EDIDataRegistry.Instance.SystemProductMappings.Value.GetProductByCode("ENT")
				.ModuleMappings.OfType<ProductAreaModuleMapping>().FirstOrDefault(x => x.ModuleCode == "AAA" && x.IsEnabled);

			var query = new ZQuery(StmALogSchema.SL_Parent, incident.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "STC");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Menu Item");
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var log = Factory.LoadTop1<StmALog>(query);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "ABC";
				Factory.Save();
			}

			Assert(!incident.IM_SourceModuleId.IsEmpty);
			Assert(!incident.IsSourceModuleOverriden);

			using (ZFormModaliser.SuspendDispose())
			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.ProductArea = "";

				parentIncidentForm.TriageAssistButton.PerformClick();
				using (var triageForm = parentIncidentForm.TriageForm)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);  //no = allow override

					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);

					AssertEquals(@"The following Menu Item already exists on the eRequest and was populated via the client using the F1 Help functionality:

SourceModule2

Do you want to finalize this triage node using the existing Menu Item to determine Product Area (selecting no will allow you to override)?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(incident.IM_IMT_Triage, triage1.PK);
					AssertEquals(incident.ProductArea, defaultMapping.ProductArea);
					AssertEquals(incident.IM_Product, triage1.IMT_Product);
					AssertEquals(incident.IM_Module, triage1.IMT_Module);
					AssertEquals(incident.IM_SourceModuleId, "N/A");
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestOnTriageAssistSaved_OverrideProductClassification_IM_SourceModuleId_SkipOverride()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Service;
			triage1.IMT_Module = "YYY";
			triage1.IMT_Product = "ENT";
			triage1.IMT_ProductArea = "";
			triage1.IMT_SetProductAreaByMenuItem = true;

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR8";
			incident.ProductArea = "ARC";
			incident.IM_SourceModuleId = "SourceModule2";
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, incident.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "STC");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Menu Item");
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var log = Factory.LoadTop1<StmALog>(query);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "ABC";
				Factory.Save();
			}

			Assert(!incident.IM_SourceModuleId.IsEmpty);
			Assert(!incident.IsSourceModuleOverriden);

			incident.ProductArea = "";
			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				parentIncidentForm.TriageAssistButton.PerformClick();
				using (var triageForm = parentIncidentForm.TriageForm)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes = skip override
					triageForm.Show();

					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals(@"The following Menu Item already exists on the eRequest and was populated via the client using the F1 Help functionality:

SourceModule2

Do you want to finalize this triage node using the existing Menu Item to determine Product Area (selecting no will allow you to override)?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals(incident.IM_IMT_Triage, triage1.PK);
					AssertEquals("stay unchanged", "", incident.ProductArea);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestEConversationCriticalityChangeMessageOnlyOnSave()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				incident.IM_Priority = "CR3";
				var unSavedEConvoMessages = incident.EConversation.GetNewMessagesAndClear();
				AssertEquals("There should be no new eConversation messages between saves", 0, unSavedEConvoMessages.Length);
				incident.IM_Priority = "CR2";
				Factory.Save();
				var savedEConvoMessages = incident.EConversation.GetNewMessagesAndClear();
				AssertEquals(1, savedEConvoMessages.Length);
				AssertEquals("Criticality change message should be from last saved value to new value", savedEConvoMessages[0].Body, "Criticality changed from CR4 to CR2");
			}
		}

		public void TestAwaitingResponseButtonShouldEnableWhenHaveWorkingTask()
		{
			var incident = GetIncidentForEConversationTest();
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			var sendPermissions = EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed;
			EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = false;

			try
			{
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					Assert(!form.AwaitingResponseButtonForTest.Enabled);
				}

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				using (var form = new SupportIncidentFormForTest(incident))
				{
					form.Show();
					Assert(form.AwaitingResponseButtonForTest.Enabled);
				}
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed = sendPermissions;
			}
		}

		public void TestTriageFormShouldNotReloadIncident()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident.ProductArea = "ARC";
			incident.IM_SourceModuleId = "SourceModule2";

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = "SPT";
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (ZFormModaliser.SuspendDispose())
			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				try
				{
					parentIncidentForm.Show();

					incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;

					parentIncidentForm.TriageAssistButton.PerformClick();
					AssertNotNull(parentIncidentForm.TriageForm);

					var bizObj = (TriageAssistBusinessObject)parentIncidentForm.TriageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					Assert("Should not reload the main incident factory", incident.IM_PriorityInfo.HasChanges);
					AssertEquals(incident.IM_Priority, Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
				}
				finally
				{
					parentIncidentForm.TriageForm?.Dispose();
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestSystemVersionBoundLabel()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = new ZDateTime(2006, 1, 12, 15, 10, 0);
			build.VersionNumber = new VersionNumber(1, 2, 3, 4);
			incident.IM_HL_ClientReportedOnVersion = build.PK;
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var fullDisplayText = $"ediEnterprise - {build.ReleaseDisplayText} - 1.2.3.4 - 12-Jan-06 15:10";
				var systemVersionBoundLabel = form.Controls.Find("SystemVersionBoundLabel", true)[0] as ZLabel;
				AssertEquals(fullDisplayText, systemVersionBoundLabel.Text);
				SafeClipboard.Clear();
				var copyMenuItem = systemVersionBoundLabel.ContextMenuStrip.Items.Find("Copy", false)[0];
				AssertNotNull("copyMenuItem should not be null", copyMenuItem);
				copyMenuItem.PerformClick();
				var clipboard = SafeClipboard.GetText();
				AssertEquals(fullDisplayText, clipboard);
			}
		}

		public void TestIncidentFormCannotBeClosedWhenTriageFormIsOpen()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var triageAssistButton = (ZButton)form.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				triageAssistButton.PerformClick();
				AssertNotNull(form.TriageForm);
				AssertEquals("Triage form should be open.", true, form.TriageForm.Visible);

				var bottomPanel = form.Controls.Find("BottomPanel", false).FirstOrDefault();
				var prevNextControl = bottomPanel?.Controls.OfType<ZPreviousNextControl>().FirstOrDefault();
				var nextButton = (ZButton)prevNextControl?.Controls.Find("NextButton", false).FirstOrDefault();
				var previousButton = (ZButton)prevNextControl?.Controls.Find("PreviousButton", false).FirstOrDefault();
				var currentResultCalcEdit = (ZCalcEdit)prevNextControl?.Controls.Find("CurrentRecordNumberCalcEdit", false).FirstOrDefault();

				Assert("Should not enable prevNext buttons.", !nextButton.Enabled);
				Assert("Should not enable prevNext buttons.", !previousButton.Enabled);
				Assert("Should not enable prevNext buttons.", !currentResultCalcEdit.Enabled);

				form.Close();
				AssertEquals("Should show warning message.", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Should show warning message.", "Incident cannot be closed while child dialogs are open.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Incident form should not be closed.", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.TriageForm.Close();

				Assert("Should enable prevNext buttons.", nextButton.Enabled);
				Assert("Should enable prevNext buttons.", previousButton.Enabled);
				Assert("Should enable prevNext buttons.", currentResultCalcEdit.Enabled);

				form.Dispose();
			}
		}

		public void TestShouldNotPopupCloseFormWhenCreateClientCommunicationTask()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncidentForTest>();
			incident1.IM_Product = "ENT";
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			Factory.Save();

			using (var form = new SupportIncidentForm(incident1))
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.Show();
				AssertEquals("HasSuspendTriggeredCloseIncidentByWorkflowTaskChange should be false before CreateClientCommunicationTaskIfNeeded", false, incident1.HasSuspendTriggeredCloseIncidentByWorkflowTaskChange);
				incident1.CreateClientCommunicationTaskIfNeeded();
				AssertEquals("HasSuspendTriggeredCloseIncidentByWorkflowTaskChange should be true after CreateClientCommunicationTaskIfNeeded", true, incident1.HasSuspendTriggeredCloseIncidentByWorkflowTaskChange);
				AssertEquals("Should have added a Client Communication Task", 1, incident1.WorkflowItems.Tasks.Count);
				AssertNull("Should not popup close form", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#region Overrides
		protected override Form GetFormToBashCore()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			SupportIncidentForm form = new SupportIncidentForm(incident);
			form.ControllerID = ClientControllerRegistration.SupportIncident;
			return form;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "allMessagesRadioButton" || control.Name == "userMessagesRadioButton")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}

		#endregion

		#region Implementation

		LicenceHeader SetupLicenceForOrg(OrgHeader org, ZString supportMode)
		{
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = supportMode;
			enterprise.LE_OH = org.PK;
			LicenceCompany company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = supportMode;
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();
			licence.LA_LC = company.PK;
			licence.LA_LD = database.PK;
			licence.LA_SupportMode = supportMode;
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;
			return licence;
		}

		#region class MockSupportIncidentForm

		class MockSupportIncidentForm : SupportIncidentForm
		{
			public bool HasCancelledClosingAction { get; set; }

			public ReopenIncidentAction ReopenIncidentPopup_DefaultAnswer { get; set; }

			public MockSupportIncidentForm(SupportIncident supportIncident) : base(supportIncident)
			{
			}

			protected override ContinueWithSave ShowPreSaveDialogs()
			{
				return ContinueWithSave.Yes;
			}

			public ZTextBox GetConversationMessageTextBox()
			{
				return ConversationMessageTextBox;
			}

			public SplitContainer GetPersistedSplitContainer()
			{
				return splitContainer5;
			}

			public ZLabel GetAssignedToLabel()
			{
				return AssignedToLabel;
			}

			public ZLabel GetStatusLabel()
			{
				return StatusLabel;
			}

			public ZLabel GetStatusDescriptionLabel()
			{
				return StatusDescription;
			}

			public ZLabel GetCurrentTaskLabel()
			{
				return CurrentTaskLabel;
			}

			public ZLabel GetCurrentTaskLabelText()
			{
				return CurrentTaskLabelText;
			}

			public ZLabel GetWorkplaceLabel()
			{
				return WorkplaceLabel2;
			}

			public ZLabel GetOverallAssignedToCodeLabel()
			{
				return OverallAssignedToCodeLabel;
			}

			public ZLabel GetSupportClientNameLabel()
			{
				return supportClientNameLabel;
			}

			public ZLabel GetOverallAssignedToDescriptionLabel()
			{
				return OverallAssignedToDescriptionLabel;
			}

			public void ShowChildFormTest(BaseIncidentPopupForm form)
			{
				base.ShowChildForm(form);
			}

			protected override void OnClosing(CancelEventArgs e)
			{
				HasCancelledClosingAction = false;
				base.OnClosing(e);
				HasCancelledClosingAction = e.Cancel;
			}

			public ZTabPage RelatedItemsTabPage_Exposed => RelatedItemsTabPage;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public MemoryStream GetLayoutStream(ZGrid grid)
			{
				MemoryStream result = new MemoryStream();
				DataTable dtLayout = new DataTable("OGridColumnSettings");
				dtLayout.Columns.Add("MappingName", typeof(string));
				dtLayout.Columns.Add("Width", typeof(int));
				dtLayout.Columns.Add("IsVisible", typeof(bool));
				foreach (ZGridColumn col in grid.Columns)
				{
					dtLayout.Rows.Add(new object[] { col.ColumnStyle.MappingName, col.ColumnStyle.Width, col.IsVisible });
				}

				DataSet ds = new DataSet();
				ds.Tables.Add(dtLayout);
				DataTable dtSort = null;
				dtSort = new DataTable("OGridSortSettings");
				dtSort.Columns.Add("SortPropertyName", typeof(string));
				dtSort.Columns.Add("SortDirection", typeof(ListSortDirection));
				DataRow sortSettingsRow = dtSort.NewRow();
				sortSettingsRow["SortPropertyName"] = "WKI_Summary";
				sortSettingsRow["SortDirection"] = ListSortDirection.Ascending;
				dtSort.Rows.Add(sortSettingsRow);
				ds.Tables.Add(dtSort);
				ds.WriteXml(result, XmlWriteMode.IgnoreSchema);
				result.Position = 0;
				return result;
			}

			protected override void SetupTaskButtons()
			{
				base.SetupTaskButtons();
				SetupTaskButtonsCallCount++;
			}

			public int SetupTaskButtonsCallCount { get; private set; }
			public ZLabel ServiceTypeCaption_Exposed => serviceTypeCaption;
			public ZDropEdit ServiceTypeDropEdit_Exposed => serviceTypeDropEdit;

			protected override ZErrorMessageBox GetEscalateErrorMessageBox(string message, string caption)
			{
				return new ZErrorMessageBoxForTest(BusinessEntity, message, caption);
			}

			protected override ReopenIncidentPopup GetReopenIncidentPopup()
			{
				var popup = base.GetReopenIncidentPopup();
				popup.DialogResult = ReopenIncidentPopup_DefaultAnswer;
				return popup;
			}
		}

		class SupportIncidentForTest : SupportIncident
		{
			public SupportIncidentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool HasSuspendTriggeredCloseIncidentByWorkflowTaskChange;

			public override bool SuspendTriggerCloseIncident
			{
				get => base.SuspendTriggerCloseIncident;
				set
				{
					if (base.SuspendTriggerCloseIncident != value)
					{
						base.SuspendTriggerCloseIncident = value;
						HasSuspendTriggeredCloseIncidentByWorkflowTaskChange = true;
					}
				}
			}

			protected override bool CanShowCloseIncidentForm => true;
		}

		class ZErrorMessageBoxForTest : ZErrorMessageBox
		{
			public ZErrorMessageBoxForTest(BusinessObject businessObject, string message, string caption)
				: base(businessObject, message, caption)
			{
			}

			public string DetailsTextWithColumnNames_Exposed => DetailsTextWithColumnNames;
		}

		#endregion

		#endregion
	}
}
