using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.IdentitySecurity;
using static CargoWise.Definitions.Authentication.SupportLogonRole;
using static Enterprise.Client.EDI.EDISecurityCheckpoints.Constants;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public interface ILicenceViewController
	{
		void OnGenerateLicenceKey(Form form, LicenceDatabase db, EDIOrgHeader org);
		void OnUpdateLicenceRemotely(Form form, LicenceDatabase db, EDIOrgHeader org);
		void OnGenerateCompanyNativeXmlEmail(EDIOrgHeader org);
		ILicenceDatabaseViewController GetLicenceDatabaseViewController();
	}

	public partial class LicenceKeyBuilderControl : EDIOrganisationSecurityContainerControl
	{
		[Obsolete("designer only")]
		public LicenceKeyBuilderControl()
		{
			InitializeComponent();
		}

		public LicenceKeyBuilderControl(ILicenceViewController viewController)
		{
			InitializeComponent();
			ViewController = viewController;
			DatabasesModuleButtonGrid.LicDatabaseViewController = viewController?.GetLicenceDatabaseViewController();
			SetupConnectionContextMenu();
			if (!DesignModeFinder.IsDesigning)
			{
				DatabasesModuleButtonGrid.ParentControl = this;
				DatabasesModuleButtonGrid.ModuleID = ClientModuleRegistration.LicenceDatabase;
				DatabasesModuleButtonGrid.SetButtonsReadOnly(!EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed);

				InitializeLicenceHeaderBinding();
			}
			InactiveCountBox.TextAlign = HorizontalAlignment.Left;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

#if !WINZOR
			notifyIcon?.Dispose();
#endif

			base.Dispose(disposing);
		}

		public ILicenceViewController ViewController { get; set; }

		public ZLabel LicenceKeyInSyncLabel { get { return licenceModulesControl.LicenceKeyInSyncLabel; } }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SplitterState.Persist(DbSplitter);
		}

		public IClientOrgLicenceProvider ContextBusinessEntity { get; set; }

		#region Bound Organisation

		internal EDIOrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = (EDIOrgHeader)this.BindingSource.DataSource;
					if (fOrganisation == null)
					{
						fOrganisation = ((ZForm)FindForm()).BusinessEntity as EDIOrgHeader;
					}
				}

				return fOrganisation;
			}
		}

		EDIOrgHeader fOrganisation;

		#endregion

		#region Licence Key Generation

		protected void GenerateLicenceKeyButton_Click(object sender, EventArgs e)
		{
			ViewController?.OnGenerateLicenceKey(FindForm(), SelectedLicenceDatabase, Organisation);
		}

		protected void EmailCompanyButton_Click(object sender, EventArgs e)
		{
			Organisation.LicCompany.Validation.ValidateLC_CompanyCode();

			if (Organisation.LicCompany.LC_CompanyCodeInfo.HasWarnings() || Organisation.LicCompany.LC_CompanyCodeInfo.HasErrors())
			{
				Globals.Message.ShowError(Organisation.LicCompany.LC_CompanyCodeInfo.Notifications.GetHighestSeverityNotification().Message);
				return;
			}

			ViewController?.OnGenerateCompanyNativeXmlEmail(Organisation);
		}

		protected void AutoDeployLicenceButton_Click(object sender, EventArgs e)
		{
			ViewController?.OnUpdateLicenceRemotely(FindForm(), SelectedLicenceDatabase, Organisation);
		}

		#region Get Directory To Save Licence Key

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected bool GetDirectoryForLicenceKeyFile(ref string directory)
		{
			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.Description = Res.GetString("5c16797b-de8e-4523-8567-92cf7b3c071d", "Please select the folder into which the License Key File should be saved:");

				if (EDIDataRegistry.Instance.LicenceKeyDirectoryPath.Length > 0)
				{
					browser.SelectedPath = EDIDataRegistry.Instance.LicenceKeyDirectoryPath;
				}

				if (ShowDialog(browser) == DialogResult.OK)
				{
					directory = GetPath(browser);
					EDIDataRegistry.Instance.LicenceKeyDirectoryPath = directory;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual DialogResult ShowDialog(ZFolderBrowserDialog browser)
		{
			browser.RequireMappablePath = true;
			return browser.ShowDialog();
		}

		protected virtual string GetPath(ZFolderBrowserDialog browser)
		{
			return browser.MappedSelectedPath;
		}

		#endregion

		#endregion

		#region Upgrade Package

		protected void SendButton_Click(object sender, EventArgs e)
		{
			UpgradeRequestCollectionContainer upgrader = new UpgradeRequestCollectionContainer(new BusinessObjectFactory(), Organisation);
			if (ContextBusinessEntity != null)
			{
				upgrader.NotificationSubjectPrefix = ContextBusinessEntity.ReferenceNumber + ": ";
			}
			ZFormModaliser.Show(new UpgradeForm(upgrader), ParentForm);
		}

		#endregion

		#region Connect to Client

		const string ConnectToConnectionMenuItemName = "ConnectToConnection";

		void SetupConnectionContextMenu()
		{
			var menuItems = ConnectionsGrid.ContextMenu.MenuItems;
			menuItems.Add("-");
			var connectItem = menuItems.Add("Connect", ConnectToClient);

			connectItem.Name = ConnectToConnectionMenuItemName;

			menuItems.Add("-");
		}

		void ConnectToClientOnDoubleClick(object sender, EventArgs e)
		{
			Point p = MousePosition;
			DataGrid.HitTestInfo hit = ConnectionsGrid.HitTest(ConnectionsGrid.PointToClient(p));

			if (hit.Type == DataGrid.HitTestType.ColumnHeader || hit.Type == DataGrid.HitTestType.ColumnResize)
			{
				return;
			}
			ConnectToClient(sender, e);
		}

		protected void ConnectToClient(object sender, EventArgs e)
		{
			var currentConnection = ConnectionsGrid.ListManager.GetCurrent() as LicenceConnection;

			if (ConnectionsGrid.List.Count == 0 && currentConnection == null)
			{
				return;
			}

			switch (currentConnection.LK_RemoteAccessMethod)
			{
				case DefaultRemoteAccessMethods.Codes.RemoteDesktop:
					ConnectToRemoteDesktop(currentConnection);
					break;

				case DefaultRemoteAccessMethods.Codes.Vnc:
					ConnectToVNC(currentConnection);
					break;

				case DefaultRemoteAccessMethods.Codes.GlowDesktopClient:
					LaunchConnectionLauncher(new GlowClientLauncher(currentConnection));
					break;

				case DefaultRemoteAccessMethods.Codes.WebBrowser:
					LaunchConnectionLauncher(new WebBrowserLauncher(currentConnection));
					break;

				default:
					Globals.Message.Show(Res.GetString("a0bc78cf-ff11-442e-a46d-c734369947f8", "This type of connection must be established manually."));
					break;
			}
		}

		#region IConnectionLauncher Handling

		void LaunchConnectionLauncher(IConnectionLauncher launcher)
		{
			Action<Upgrades.Progress> launchClient = p => launcher.Launch(p);
			if (launcher.ShowProgressForm)
			{
				ExecuteWithProgressForm(launchClient);
			}
			else
			{
				launchClient(null);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1049:Using Application.DoEvents", Justification = "old internal only code")]
		void ExecuteWithProgressForm(Action<Upgrades.Progress> action)
		{
			var progressForm = new ProgressForm();
			progressForm.ShowModalTo(ParentForm);
			Application.DoEvents();

			var progress = LauncherProgress(progressForm);
			action(progress);

			progressForm.Hide();
		}

		Upgrades.Progress LauncherProgress(ProgressForm progressForm)
		{
			return (status, percentComplete) =>
			{
				progressForm.SetStatusAndPercentComplete(status, percentComplete);
				return true;
			};
		}

		#endregion

		#region Remote Desktop

		void ConnectToRemoteDesktop(LicenceConnection connection)
		{
			string validationErrorText;
			var batchCommands = connection.BuildRemoteDesktopBatchCommands(out validationErrorText);
			if (string.IsNullOrEmpty(validationErrorText))
			{
				using (TempFile tempFile = TempFile.NewWithExtension("bat"))
				{
					File.WriteAllText(tempFile.Filename, batchCommands);
					FileOpener.Open(tempFile.Filename);
				}
			}
			else
			{
				Globals.Message.Show(validationErrorText);
			}
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is the default install dir")]
		const string ultraVNCFile = @"c:\program files\ultravnc\vncviewer.exe";
		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is the default install dir")]
		const string realVNCFile = @"c:\program files\realvnc\vncviewer.exe";

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, launching VNC not opening a file or url")]
		void ConnectToVNC(LicenceConnection connection)
		{
			string fileName = null;
			if (File.Exists(ultraVNCFile))
			{
				fileName = ultraVNCFile;
			}
			else if (File.Exists(realVNCFile))
			{
				fileName = realVNCFile;
			}
			if (fileName != null)
			{
				if (!connection.LK_RemoteAccessPassWord.IsEmpty && !connection.LK_RemoteAccessAddress.IsEmpty)
				{
					Process.Start(fileName, connection.LK_RemoteAccessAddress + " -password \"" + connection.LK_RemoteAccessPassWord + "\"");
				}
				else
				{
					Globals.Message.Show(Res.GetString("ad3bd3cb-bfa6-4418-b890-620e849f9b8b", "You must specify a password and connection address before attempting connection using Remote Desktop."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("97a965d0-7ac6-417c-8fae-60099bb3682b", "Please install VNC in either RealVNC or UltraVNC directory"));
			}
		}

		#endregion

		#region Projects

		void ProjectsGrid_DoubleClick(object sender, EventArgs e)
		{
			DisplayProject();
		}

		void DisplayProject()
		{
			if (ProjectsGrid.SelectedElements.Length > 0)
			{
				EDIProject project = ProjectsGrid.SelectedElements[0] as EDIProject;
				if (project != null)
				{
					ZController controller = ZControllerFactory.Create(ControllerIDs.Project);
					controller.SetFormsModalTo(FindForm());
					controller.ShowEditForm(project);
				}
			}
		}

		#endregion

		#region EDIOrganisationForm

		public void SelectPriceItemTab()
		{
			LicenceTabControl.SelectedTab = BillingPricesTabPage;
		}

		public void SelectPriceHeadersGrid(ClientLicencePriceItem priceItem)
		{
			this.BillingPricesControl.SelectPriceHeadersGrid(priceItem);
		}

		#endregion

		#region Database Module Button Grid

		protected void GenerateLoginTokenButton_Click(object sender, EventArgs e)
		{
			GenerateLoginToken(UserType.CW1);
		}

		protected void GenerateDiagnosticsLoginTokenButton_Click(object sender, EventArgs e)
		{
			GenerateLoginToken(UserType.Diagnostic);
		}

		protected void GenerateLoginTokenWithOldEntCodeButton_Click(object sender, EventArgs e)
		{
			GenerateLoginToken(UserType.CW1, true);
		}

		void GenerateLoginToken(UserType userType, bool usingOldEnterpriseCode = false)
		{
			var form = ParentForm as ZForm;
			if (form.BusinessEntity?.HasChanges == true)
			{
				Globals.Message.Show(Res.GetString("89E8A7AB-A9FE-4E9A-A37B-D25A34A187B2", "Cannot generate a token before saving."));
				return;
			}

			if (SelectedLicenceDatabase == null)
			{
				Globals.Message.Show(Res.GetString("962d3ec1-5bce-47a7-999e-cd993bbdbda6", "Please select a license database."));
				return;
			}

			var privateKeyBytes = EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.Value;
			if (privateKeyBytes == null || privateKeyBytes.Length == 0)
			{
				Globals.Message.Show(Res.GetString("944c77ea-56a9-4e23-b28e-aefb695e962d", "Please upload the private key in the registry item '{0}' to sign the login token.", EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.HumanReadableRegistryPath()));
				return;
			}

			var privateKeyPem = Encoding.UTF8.GetString(privateKeyBytes);
			var privateKey = RSAKeyProvider.ImportPrivateKey(privateKeyPem);

			var centuryBegin = new DateTime(1970, 1, 1);
			var exp = new TimeSpan(ZDateTime.UtcNow.AddSeconds(90).Ticks - centuryBegin.Ticks).TotalSeconds;
			var now = new TimeSpan(ZDateTime.UtcNow.Ticks - centuryBegin.Ticks).TotalSeconds;

			var systemCode = $"{GetEnterpriseCode(usingOldEnterpriseCode)}{SelectedLicenceDatabase.LD_ServerCode}";
			var user = Env.CurrentUser.Initials;
			var incidentNumber = ContextBusinessEntity?.ReferenceNumber.ToString();

			string[] userRoles;
			if (userType == UserType.Diagnostic)
			{
				systemCode = $"{systemCode}_{nameof(UserType.Diagnostic)}";
				userRoles = EDISecurityCheckPointRoleMapper.GetExternalDiagnosticUserRoles();
			}
			else
			{
				userRoles = EDISecurityCheckPointRoleMapper.GetExternalSuperUserRoles();
			}

			var payload = new JwtPayload
			{
				{ "iat", (long)now },
				{ "exp", (long)exp },
				{ "aud",  systemCode },
				{ "sub", user },
				{ "incident", incidentNumber },
				{ "name", Env.CurrentUser.FullName },
				{ "roles",  userRoles },
				{ "jti", ZGuid.NewZGuid().ToString() }
			};

			var certificateData = DataRegistry.Instance.CWSupportLoginTokenCertificate;
			var cert = new X509Certificate2(certificateData);

			var loginToken = JwtSecurity.GenerateSignedJwt(privateKey, cert, payload);
			if (!SafeClipboard.SetText(loginToken))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
			else
			{
				ContextBusinessEntity?.LicenceOrganisation.Logs.AddNew(AutoEvents.SupportLoginTokenGenerated, "Support Login Token Generated", new[]
				{
						new KeyValuePair<string, string>("SystemCode", systemCode),
						new KeyValuePair<string, string>("UserInitial", user),
						new KeyValuePair<string, string>("Incident", incidentNumber),
				});
#if !WINZOR
				var msg = Res.GetString("f49b3771-3fa9-4d59-918b-28e8879ff018", "Your Support Login Token has been copied to the clipboard.");
				NotifyIcon.ShowBalloon("Support Login Token", msg, ZNotifyIconEx.NotifyInfoFlags.Info, 2000);
#endif
			}
		}

		string GetEnterpriseCode(bool usingOldEnterpriseCode)
		{
			if (usingOldEnterpriseCode)
			{
				var stmALog = GetStmALog(SelectedLicenceDatabase);

				var match = FieldChangeEventParser.Match(stmALog.SL_Reference);
				return match.Groups["From"].Value;
			}

			return SelectedLicenceDatabase.EnterpriseCode;
		}

#if !WINZOR
		ZNotifyIconEx NotifyIcon
		{
			get
			{
				if (notifyIcon == null)
				{
					notifyIcon = new ZNotifyIconEx();
					notifyIcon.Icon = BrandingFactory.Instance.ProductIcon;

					notifyIcon.Click += delegate
					{
						notifyIcon.Dispose();
						notifyIcon = null;
					};

					notifyIcon.BalloonClick += delegate
					{
						notifyIcon.Dispose();
						notifyIcon = null;
					};
				}

				return notifyIcon;
			}
		}

		ZNotifyIconEx notifyIcon;
#endif

		readonly Regex FieldChangeEventParser = new Regex(@"LicenceDataBase enterpriseCode change from '(?<From>[^']+)' to '(?<To>[^']+)'");

		#endregion

		#region ToggleButtonEnabledState

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			LicenceDatabasesListManager = (CurrencyManager)GetBindingManager("LicCompany+ActiveOrAllLicDatabases");
		}

		public CurrencyManager LicenceDatabasesListManager
		{
			get { return licenceDatabasesListManager; }
			set
			{
				if (licenceDatabasesListManager != null)
				{
					licenceDatabasesListManager.CurrentChanged -= new EventHandler(DatabasesGrid_CurrentChanged);
				}
				licenceDatabasesListManager = value;
				if (licenceDatabasesListManager != null)
				{
					licenceDatabasesListManager.CurrentChanged += new EventHandler(DatabasesGrid_CurrentChanged);
					UpdateDatabaseControls(LicenceDatabasesListManager);
				}
			}
		}
		CurrencyManager licenceDatabasesListManager;

		void DatabasesGrid_CurrentChanged(object sender, EventArgs e)
		{
			UpdateDatabaseControls((CurrencyManager)sender);
		}

		void UpdateDatabaseControls(CurrencyManager listManager)
		{
			var selectedDatabase = listManager != null && listManager.Count > 0 ? (LicenceDatabase)listManager.GetCurrent() : null;
			var selectedLicHeader = selectedDatabase != null ? Organisation.LicCompany.GetHeader(selectedDatabase) : null;

			UpdateLicenceHeaderBinding(selectedLicHeader);
			UpdateLicenceHeaderControlValues(selectedLicHeader);
			UpdateGenerateLoginTokenButtonVisibility(selectedLicHeader?.Database);
		}

		void InitializeLicenceHeaderBinding()
		{
			licenceHeaderBindingSource = new ZBindingSource();
			licenceHeaderBindingSource.DataSourceType = typeof(LicenceHeader);

			const string DummyLicHeaderForBindingMember = "DummyLicHeaderForBinding";
			int dummyLength = DummyLicHeaderForBindingMember.Length;

			foreach (var binding in BindingSource.FullBindingMembers.ToList())
			{
				var oldBindingMember = binding.Value;
				var indexOfLicenceHeaderPart = oldBindingMember.IndexOf(DummyLicHeaderForBindingMember, StringComparison.OrdinalIgnoreCase);
				if (indexOfLicenceHeaderPart >= 0)
				{
					BindingSource.SetBindingMember(binding.Key, null);
					((IDefaultBindingSettings)BindingSource).ExcludeFromDefaultBinding(binding.Key);

					indexOfLicenceHeaderPart += dummyLength;
					if (oldBindingMember.Length > indexOfLicenceHeaderPart)
					{
						var nextChar = oldBindingMember[indexOfLicenceHeaderPart];
						if (nextChar == '.' || nextChar == '+')
						{
							++indexOfLicenceHeaderPart;
						}
					}

					var newBindingMember = indexOfLicenceHeaderPart == oldBindingMember.Length
						? "."
						: oldBindingMember.Substring(indexOfLicenceHeaderPart);

					licenceHeaderBindingSource.SetBindingMember(binding.Key, newBindingMember);
				}
			}
		}

		ZBindingSource licenceHeaderBindingSource;

		void UpdateLicenceHeaderBinding(LicenceHeader selectedHeader)
		{
			licenceHeaderBindingSource.DataSource = selectedHeader;
		}

		void UpdateLicenceHeaderControlValues(LicenceHeader selectedHeader)
		{
			var selectedDatabase = selectedHeader?.Database;

			GenerateLicenceKeyButton.Enabled = AutoDeployLicenceButton.Enabled =
				EDISecurityCheckpoints.OrgLicenceCreateAndEmailLicenceKey.IsAllowed
				&& selectedDatabase != null
				&& selectedDatabase.HasCompanyLicence();

			SendButton.Enabled =
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed
				&& selectedDatabase != null
				&& selectedHeader != null
				&& selectedDatabase.IsEnterpriseFamilyDatabase;

			productBox.Text = selectedDatabase != null ? selectedDatabase.Lookups.ProductTypeList.GetDescriptionFromCode(selectedDatabase.LD_Product) : "";

			string editionComment = "";
			if (selectedDatabase != null)
			{
				var futureStlDate = selectedDatabase.FutureStlBillingModelDate;
				if (!futureStlDate.IsEmpty)
				{
					editionComment = "- STL from " + futureStlDate.ToDateTime().ToShortDateString();
				}
				else if (selectedHeader != null && selectedHeader.Edition != selectedHeader.LA_LicenceAdvStdOth)
				{
					editionComment = "- automatic from pricelist";
				}
			}
			editionCommentBox.Text = editionComment;
		}

		public bool AreTokenButtonsVisible
		{
			get => areTokenButtonsVisible;
			set
			{
				areTokenButtonsVisible = value;
				UpdateGenerateLoginTokenButtonVisibility(SelectedLicenceDatabase);
			}
		}

		bool areTokenButtonsVisible;

		void UpdateGenerateLoginTokenButtonVisibility(LicenceDatabase licenceDatabase)
		{
			if (licenceDatabase != null && areTokenButtonsVisible)
			{
				if (SelectedLicenceDatabase.LD_LicenceType == DatabaseTypes.Codes.Production)
				{
					GenerateLoginTokenButton.Visible = Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsSuperUser).IsAllowed;
					GenerateDiagnosticsLoginTokenButton.Visible = Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser).IsAllowed;
					GenerateLoginTokenWithOldEntCodeButton.Visible = Env.Security.FindCheckPoint(GlowSupportLogonToExternalProductionSystemAsSuperUser).IsAllowed && CheckIfStmALogIsNotNullAndHasNotExpired(licenceDatabase);
				}
				else
				{
					GenerateLoginTokenButton.Visible = Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsSuperUser).IsAllowed;
					GenerateDiagnosticsLoginTokenButton.Visible = Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser).IsAllowed;
					GenerateLoginTokenWithOldEntCodeButton.Visible = Env.Security.FindCheckPoint(GlowSupportLogonToExternalNonProductionSystemAsSuperUser).IsAllowed && CheckIfStmALogIsNotNullAndHasNotExpired(licenceDatabase);
				}
			}
			else
			{
				GenerateLoginTokenButton.Visible = false;
				GenerateDiagnosticsLoginTokenButton.Visible = false;
				GenerateLoginTokenWithOldEntCodeButton.Visible = false;
			}
		}

		bool CheckIfStmALogIsNotNullAndHasNotExpired(LicenceDatabase licenceDatabase)
		{
			var log = GetStmALog(licenceDatabase);

			return log != null && log.SL_EventTime.AddDays(EDIDataRegistry.Instance.ValidDaysOfGenerateLoginTokenWithOldEntCodeButton.Value) > ZDateTime.Now;
		}

		StmALog GetStmALog(LicenceDatabase licenceDatabase) => licenceDatabase.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.MessageStatusChangeCode && FieldChangeEventParser.IsMatch(x.SL_Reference)).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();

		#endregion

		#region GenerateReopenPeriodKeyButton

		void GenerateReopenPeriodKeyButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(new ReopenPeriodKeyGeneratorForm(new ReopenPeriodKeyBusinessObject(Organisation)), ParentForm);
		}

		#endregion

		#region Licence Active

		void LicenceActiveCheckBox_Click(object sender, EventArgs e)
		{
			// Force the change to be committed immediately the user clicks so the database grid is in sync
			ShowInactiveDatabasesCheckBox.Select();
			LicenceActiveCheckBox.Select();
		}

		#endregion

		#region Selection

		public LicenceDatabase SelectedLicenceDatabase
		{
			get { return (LicenceDatabase)LicenceDatabasesListManager.GetCurrent(); }
			set
			{
				if (value != null)
				{
					var collection = DatabasesModuleButtonGrid.InnerGrid.ListManager.List as ActiveOrAllLicenceDatabaseCollection;
					if (collection != null)
					{
						var element = collection.Select((licence, index) => new { licence, index })
							.FirstOrDefault(node => node.licence.PK == value.PK);
						if (element != null)
						{
							DatabasesModuleButtonGrid.InnerGrid.ListManager.Position = element.index;
						}
					}
				}
			}
		}

		public LicenceHeader SelectedLicenceHeader
		{
			get
			{
				var db = SelectedLicenceDatabase;
				return db != null ? Organisation?.LicCompany.GetHeader(db) : null;
			}
			set
			{
				SelectedLicenceDatabase = value?.Database;
			}
		}

		#endregion
	}
}

