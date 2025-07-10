using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ZClientEDI.Business.Licencing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public interface ILicenceDatabaseViewController
	{
		void OnSendNewSystemShutdownDate(LicenceDatabase db);
	}

	public partial class LicenceDatabaseForm : ZTemplateForm, IProcessStatus
	{
		public LicenceDatabaseForm(LicenceDatabase businessEntity, ILicenceDatabaseViewController licenceDatabaseViewController)
			: base(businessEntity)
		{
			InitializeComponent();

			DisableNewAction();
			ResetHeartbeatButton.Enabled = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;
			SetClientSpecificModuleIDs();
			this.EnabledChanged += LicenceDatabaseForm_EnabledChanged;
			ProductionDatabaseDropEdit.EnabledChanged += LicenceDatabaseForm_EnabledChanged;
			ProductionDatabaseDropEdit.ReadOnlyChanged += LicenceDatabaseForm_EnabledChanged;
			this.StaffCodeFindBox.ReadOnly = Database.IsInDatabase && !Database.LicEnterprise.LE_IsInternal;
			ViewController = licenceDatabaseViewController;
			tableLayoutPanel1.AutoScroll = false;  // need to disable AutoScroll, otherwise disabling the horizontal scrollbar doesn't work
			tableLayoutPanel1.HorizontalScroll.Enabled = false;
			tableLayoutPanel1.AutoScroll = true;
			PlugIns.Add(ControllerIDs.Audit);
		}

		public LicenceDatabaseForm(LicenceDatabase businessEntity, ILicenceDatabaseViewController licenceDatabaseViewController, IClientOrgLicenceProvider licenceProvider)
			: this(businessEntity, licenceDatabaseViewController)
		{
			this.LicenceProvider = licenceProvider;
		}

		readonly ILicenceDatabaseViewController ViewController;

		void LicenceDatabaseForm_EnabledChanged(object sender, EventArgs e)
		{
			return;
		}

		readonly IClientOrgLicenceProvider LicenceProvider;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetControlsVisibility();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var db = Database;
			if (db != null)
			{
				db.LD_LicenceTypeInfo.ValueChanged -= LD_LicenceTypeInfo_ValueChanged;
				db.LD_OH_WebAccessOrgInfo.ValueChanged -= LD_OH_WebAccessOrgInfo_ValueChanged;
				db.LD_ProductInfo.ValueChanged -= LD_ProductInfo_ValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			db = Database;
			if (db != null)
			{
				db.LD_LicenceTypeInfo.ValueChanged += LD_LicenceTypeInfo_ValueChanged;
				db.LD_OH_WebAccessOrgInfo.ValueChanged += LD_OH_WebAccessOrgInfo_ValueChanged;
				db.LD_ProductInfo.ValueChanged += LD_ProductInfo_ValueChanged;
			}
		}

		protected override bool SupportsEDocs => false;

		void LD_LicenceTypeInfo_ValueChanged(object sender, EventArgs e) => SetControlsVisibility();

		void LD_ProductInfo_ValueChanged(object sender, EventArgs e) => SetControlsVisibility();

		public LicenceDatabase Database
		{
			get { return (LicenceDatabase)base.BusinessEntity; }
		}

		void SetClientSpecificModuleIDs()
		{
			CurrentSentVersionGuidFindBox.ModuleID = ClientModuleRegistration.ReleaseBuild;
			CurrentRunningVersionGuidFindBox.ModuleID = ClientModuleRegistration.ReleaseBuild;
		}

		void SetControlsVisibility()
		{
			ProductionDatabaseDropEdit.Visible = !Database.LD_LicenceType.IsEmpty && Database.LD_LicenceType != DatabaseTypes.Codes.Production;
			TenantIDTextBox.Visible = !Database.IsEnterpriseFamilyDatabase;
		}

		public override string FormCaption
		{
			get { return "Database Installation"; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes &&
				Database.IsMultiTenantDatabase &&
				(Database.TrustedSystem?.HasChanges ?? false))
			{
				var messageResult = Globals.Message.Show(
					(NoResString)"The changes on 'Trusted Messaging' will affect all LicenceDatabases for this Product.\r\nAre you sure you want to save the changes?",
					(NoResString)"Trusted Messaging",
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				result = messageResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;

				if (result == ContinueWithSave.Yes)
				{
					Database.UpdateMessagingConfigOnAllMultiTenantDatabases();
				}
			}
			else if (result == ContinueWithSave.Yes && Database.IsInDatabase &&
				(Database.LD_DatabaseConfigInfo.HasChanges || (Database.TrustedSystem?.HasChanges ?? false)))
			{
				var messageResult = Globals.Message.Show(
					(NoResString)"The changes on 'Trusted Messaging' may affect the communication between client system and MyAccount.\r\nAre you sure you want to save the changes?",
					(NoResString)"Trusted Messaging",
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				result = messageResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (!Database.IsInDatabase && Database.EDIWebAccessOrg != null && Database.LD_LE.IsEmpty)
			{
				Database.SetEnterpriseServerCode();
			}

			return base.ValidateAndSave();
		}

		#region Request Version Report

		protected void RequestVersionReportButton_Click(object sender, EventArgs e)
		{
			try
			{
				RequestVersionReport();
				Globals.Message.Show("A request for a Version Report has been sent to the client's batch processor.\n\nTheir system will respond with a Version Report and ediProd will be automatically updated in a few minutes.", "Version Report Requested", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ShowCannotRequestVersionReport();
			}
		}

		void ShowCannotRequestVersionReport()
		{
			Globals.Message.Show("A request for a Version Report could not be sent to the client.\n\nPlease check that they are running a recent version of the software, and that the Public Email Address is correctly specified for the database if the version is before 16.10.20.0.", "Cannot Request Version Report", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		protected virtual void RequestVersionReport()
		{
			var licDatabase = ((LicenceDatabase)BusinessEntity);
			if (licDatabase.CanRequestVersionReportFromLegacySystem)
			{
				licDatabase.RequestVersionReportFromLegacySystem();
			}
			else
			{
				ShowCannotRequestVersionReport();
			}
		}

		#endregion

		#region Reset Heartbeat

		protected void ResetHeartbeatButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.ShowConfirmation(QueryResetHeartbeatMessage, QueryResetHeartbeatCaption, "To continue, please type ", "Yes", MessageBoxIcon.Exclamation) == DialogResult.OK)
			{
				try
				{
					if (ResetHeartbeat())
					{
						Globals.Message.Show(HeartbeatResetSuccessMessage, HeartbeatResetSuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						List<string> listOfLegacyProducts = new List<string>();
						foreach (SystemProduct product in EDIDataRegistry.Instance.SystemProductMappings.Value)
						{
							listOfLegacyProducts.Add(product.Code);
						}

						Globals.Message.Show(string.Format(
							CultureInfo.CurrentCulture,
							HeartbeatCannotBeResetMessage,
							LicenceDatabase.DateFromWhichDatabaseHeartbeatCanBeReset.ToShortDateString(),
							string.Join(", ", listOfLegacyProducts.ToArray())),
							HeartbeatCannotBeResetCaption,
							MessageBoxButtons.OK,
							MessageBoxIcon.Stop);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.Show(HeartbeatErrorInRequestingVersionReportMessage, HeartbeatErrorInRequestingVersionReportCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		protected virtual bool ResetHeartbeat()
		{
			return ((LicenceDatabase)BusinessEntity).ResetHeartbeat();
		}

		const string HeartbeatCannotBeResetMessage =
@"The Heartbeat information could not be reset.

Please check the following details:
- The Public Email Address must be filled, in order to send an email to the client's batch processor
- The client's Current Version must be on or after {0} for this feature to work.
- The Licence Type must NOT be set to one of the Legacy Database Types ({1}).
- The Registration must be not be REG - Registered.
- The software must be earlier than version 16.10.20.0.

Please check that these details are correct before trying again.";

		const string HeartbeatCannotBeResetCaption = "Cannot Reset Heartbeat";
		const string HeartbeatResetSuccessMessage = "The Heartbeat information has been reset for this database.";
		const string HeartbeatResetSuccessCaption = "Heartbeat Reset";
		const string HeartbeatErrorInRequestingVersionReportMessage = "There was an error in reqesting a version report. Please check that the ediProd batch processor is running, or your SMTP mail settings are correct.";
		const string HeartbeatErrorInRequestingVersionReportCaption = "Cannot Request Version Report";
		const string QueryResetHeartbeatMessage = "This procedure will reset all heartbeat information collected from the client system and request new information from the client.\n\nPlease ensure the client has their e-mail batch processor running and configured correctly.";
		const string QueryResetHeartbeatCaption = "Reset Client Heartbeat Information";

		#endregion

		#region Request Licence Usage

		protected void RequestLicenceUsageButton_Click(object sender, EventArgs e)
		{
			RequestLicenceUsage();
		}

		protected void RequestLicenceUsage()
		{
			LicenceUsageRequest request = new LicenceUsageRequest(((LicenceDatabase)BusinessEntity));
			LicenceUsageRequestForm form = new LicenceUsageRequestForm(request);
			ZFormModaliser.Show(form, this);
		}

		protected void RequestLicenceDatabaseLogsButton_Click(object sender, EventArgs e)
		{
			RequestLicenceDatabaseLogs();
		}

		protected void RequestLicenceDatabaseLogs()
		{
			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(((LicenceDatabase)BusinessEntity));
			if (LicenceProvider != null)
			{
				request.IncidentNumber = LicenceProvider.ReferenceNumber;
			}
			LicenceDatabaseLogsRequestForm form = new LicenceDatabaseLogsRequestForm(request);
			ZFormModaliser.Show(form, this);
		}

		#endregion

		#region Request Staff Report

		protected void RequestStaffReportButton_Click(object sender, EventArgs e)
		{
			var database = BusinessEntity as LicenceDatabase;
			if (database != null)
			{
				ClientStaffReportRequest.Send(database);
				Globals.Message.Show("A request for Staff List has been sent to the client.");
			}
		}

		#endregion

		#region Actions

		void LicenceDatabaseForm_Load(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed)
			{
				this.ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
				this.ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6D3B55F7-91C8-484E-BEF6-5371CF523788", "Send New System Shutdown Date"), new EventHandler(SendNewSystemShutdownDate)));
				this.ActionsMenuItem.MenuItems.Add(new ZMenuItem((NoResString)"Remove Modules and Convert to STL", new EventHandler(RemoveModulesSTL)));
				this.ActionsMenuItem.MenuItems.Add(new ZMenuItem((NoResString)"Remove Modules and Convert to OnDemand", new EventHandler(RemoveModulesODM)));
			}
		}

		void SendNewSystemShutdownDate(object sender, EventArgs e)
		{
			ViewController?.OnSendNewSystemShutdownDate((LicenceDatabase)BusinessEntity);
		}

		void RemoveModulesSTL(object sender, EventArgs e)
		{
			ConvertToCW1(LicenceAdvStdOthList.Codes.SeatTransaction);
		}

		void RemoveModulesODM(object sender, EventArgs e)
		{
			ConvertToCW1(LicenceAdvStdOthList.Codes.OnDemand);
		}

		void ConvertToCW1(string edition)
		{
			if (DialogResult.OK == Globals.Message.Show(
				"This will remove all licence modules and update the Edition for all organizations attached to this database.\r\n" +
				"Only do this if the system has been upgraded to CW1 and all ediEnterprise billing is done.\r\n\r\n" +
				"Remove all modules?", "Remove Modules", MessageBoxButtons.OKCancel, DialogResult.OK))
			{
				Database.RegisterEditableChildObject(Database.LicHeadersForAllCompanies);

				int modulesRemoved;

				using (new ZWaitCursorChanger(this))
				{
					modulesRemoved = Database.RemoveAllModules();
				}

				foreach (LicenceHeader licHeader in Database.LicHeadersForAllCompanies)
				{
					licHeader.LA_LicenceAdvStdOth = edition;
				}

				Globals.Message.Show(modulesRemoved == 0
					? "No modules were found."
					: modulesRemoved + " module(s) found and removed successfully.");
			}
		}

		#endregion

		#region Web Access Org

		bool runContactCloner;

		void LD_OH_WebAccessOrgInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Database.ShouldCloneContactOnWebAccessOrgChanged
				&& Database.LD_OH_WebAccessOrgInfo.HasChanges
				&& !Database.LD_OH_WebAccessOrgInfo.OriginalValue.IsEmpty)
			{
				var message = "You are about to change master organisation. This will affect contacts for MyAccount and eRequest login. Contacts will be copied to another organisation. Are you sure you want to proceed?";
				var result = UserNotification.Instance.ShowConfirmation(message, "Change Master Organisation Warning", "confirm", MessageBoxIcon.Warning);
				if (result == DialogResult.OK)
				{
					runContactCloner = true;
				}
				else
				{
					Database.RevertWebAccessOrgChange();
				}
			}
			else
			{
				runContactCloner = false;
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			if (runContactCloner)
			{
				using (ProgressMediator = new SaveProgressMediator())
				{
					ProgressMediator.ShowModalProgressForm(Bounds, "Cloning Contacts...", 0);
					Database.CloneContacts(runContactCloner, this);
					ProgressMediator.UpdateStatus("Saving to the database... This might take a while.", 0);
					base.Save(factories);
					runContactCloner = false;
					ProgressMediator.HideForm();
					ProgressMediator = null;
				}
			}
			else
			{
				base.Save(factories);
			}

			var errorMessage = Database.MergeContactsPerson();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				UserNotification.Instance.ShowError(errorMessage, "Failed to Merge Person");
			}
		}

		public virtual void UpdateStatus(string status, int progressValue) => ProgressMediator?.UpdateStatus(status, progressValue);

		SaveProgressMediator ProgressMediator;

		#endregion
	}
}

