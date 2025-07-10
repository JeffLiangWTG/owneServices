using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroupConstants;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentManagementGroupDetailsUserControl : ZUserControl, IConversationView
	{
		public IncidentManagementGroupDetailsUserControl() : base()
		{
			InitializeComponent();
			conversationViewController = new IncidentManagementEConversationViewController();
		}

		protected IncidentManagementGroup IncidentManagementGroup
		{
			get { return ((ZForm)FindForm())?.BusinessEntity as IncidentManagementGroup; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource == null)
			{
				StagesGrid.ColourDeciding -= StagesGrid_ColourDeciding;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				StagesGrid.ColourDeciding += StagesGrid_ColourDeciding;
				UpdateButtonEnabled();
				((ZForm)FindForm()).Saved += UpdateButtonEnabled;
				SetupCriticalityDropDown();

				conversationViewController.Initialize(dataSource as IncidentManagementGroup, this);
				((IncidentManagementGroup)dataSource).EConversation.MessageCountChanged += (o, e) =>
				{
					eConversationMessageListUserControl1.RefreshMessages();
				};
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning && IncidentManagementGroup != null)
			{
				SetModuleDropEditVisibility();
				SetSourceModuleOverrideVisibility();
				SetServiceTypeVisible();
				IncidentManagementGroup.ING_ProductInfo.ValueChanged += ING_ProductInfo_ValueChanged;
				SetServiceOutageLabelColor();
				IncidentManagementGroup.ING_ServiceOutageInfo.ValueChanged += ING_ServiceOutageInfo_ValueChanged;
				AddShowSourceModuleFinderPopupEventHandlers();

				outageDurationRefreshTimer = new ProxyWindowsTimer();
				outageDurationRefreshTimer.Interval = (int)new TimeSpan(0, 1, 0).TotalMilliseconds;
				outageDurationRefreshTimer.Tick += new EventHandler(OutageDurationRefreshTimer_Tick);
				outageDurationRefreshTimer.Enabled = true;
			}
		}

		IWindowsTimer outageDurationRefreshTimer;

		void ING_ProductInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSourceModuleOverrideVisibility();
			SetModuleDropEditVisibility();
		}

		internal bool ShouldAllowSourceModuleOverride
		{
			get
			{
				return IncidentManagementGroup.ModuleType != ModuleListType.Unspecified;
			}
		}

		void SetSourceModuleOverrideVisibility()
		{
			OverrideSourceModuleButton.Visible = ShouldAllowSourceModuleOverride;
		}

		void SetModuleDropEditVisibility()
		{
			menuSectionDropEdit.Visible = IncidentManagementGroup.ModuleType == ModuleListType.MenuSection && !IncidentManagementGroup.ING_Product.IsEmpty;
			cr8ModuleDropEdit.Visible = IncidentManagementGroup.ModuleType == ModuleListType.Cr8;
			cr9ModuleDropEdit.Visible = IncidentManagementGroup.ModuleType == ModuleListType.Cr9;
		}

		void OutageDurationRefreshTimer_Tick(object sender, EventArgs e)
		{
			outageDurationTimeLabel.DataBindings?[nameof(outageDurationTimeLabel.Text)]?.ReadValue();
		}

		#region Criticality

		void SetupCriticalityDropDown()
		{
			IncidentManagementGroup.ING_PriorityInfo.ValueChanged += new EventHandler(ING_PriorityInfo_ValueChanged);
		}

		void ING_PriorityInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSourceModuleOverrideVisibility();
			SetModuleDropEditVisibility();
		}

		#endregion

		#region Stages Grid

		void StagesGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			var configuration = (IncidentGroupStatusConfiguration)e.ObjectAtRow;
			if (configuration != null && configuration.Code.EqualsIgnoringCase(IncidentManagementGroup.ING_Status))
			{
				e.Colour = Color.LightGreen;
			}
		}

		protected void PreviousButton_Click(object sender, EventArgs e)
		{
			var previousStatus = IncidentManagementGroup.GetPreviousStatus();

			if (previousStatus == null)
			{
				Globals.Message.ShowError("There is no previous stage.");
				return;
			}

			if (IncidentManagementGroup.NowStage.IsReversible)
			{
				TryUpdateAndSaveStatus(previousStatus);
			}
		}

		protected void NextButton_Click(object sender, EventArgs e)
		{
			var nextStatus = IncidentManagementGroup.GetNextStatus();

			if (nextStatus == null)
			{
				Globals.Message.ShowError("There is no next stage.");
				return;
			}

			IncidentManagementGroup.RefreshStages();
			var stageCodeCollection = IncidentManagementGroup.Stages.Cast<IncidentGroupStatusConfiguration>().Select(stage => stage.Code).ToList();
			if (!stageCodeCollection.Contains(nextStatus.Code))
			{
				Globals.Message.ShowError("Stage has been deleted or disabled since opening this form.");
				return;
			}

			var appendMessage = string.Empty;
			if (!nextStatus.IsReversible)
			{
				appendMessage = "\r\n" + Res.GetString("8A51CC2A-DEBE-45C4-AFA3-15D5188FF679", "This action is irreversible.");
			}

			TryUpdateAndSaveStatus(nextStatus, appendMessage);
		}

		void TryUpdateAndSaveStatus(IncidentGroupStatusConfiguration newStatus, string appendMessage = "")
		{
			var message = Res.GetString("fbaeddd9-8d74-41e0-9dfd-3162be26f0ed", "Updating the Group Stage will cause the form to be saved. Would you like to continue?");
			if (!string.IsNullOrEmpty(appendMessage))
			{
				message += appendMessage;
			}

			if (Globals.Message.Show(message, "Save Form", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
			{
				return;
			}

			if (SaveChangesBeforeUpdatingStatus())
			{
				TryUpdateStatus(newStatus, appendMessage);
				TrySaveStatusChanges();
			}

			this.StagesGrid.Refresh();
			UpdateButtonEnabled();
		}

		string GetStatusConcurrencyErrorMessage(IPropertyRecord record)
		{
			var databaseStage = this.IncidentManagementGroup.Stages.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code.Equals(record.DatabaseValue));
			return Res.GetString("2F74B55F-932C-48DA-9C12-D9F20D9F011D", "Operation canceled. {0} has updated the stage of this Group to {1}. Please reload form and try again.", record.LastModified, databaseStage?.DescriptionOnGroup ?? record.DatabaseValue);
		}
		static string statusConcurrencyErrorMessageCaption => Res.GetString("0D421D28-7413-47C5-A57C-7B24079823D9", "Save Concurrency Error");

		IEnumerable<IPropertyRecord> GetConcurrencyErrorProperties()
		{
			var groupChangedObject = this.IncidentManagementGroup.Factory.GetChanges().GetChangedObjects().FirstOrDefault(x => x.SessionInstance.PK == this.IncidentManagementGroup.PK);
			if (groupChangedObject != null)
			{
				return groupChangedObject.NonMergeableProperties.Concat(groupChangedObject.MergeableProperties);
			}

			return Array.Empty<IPropertyRecord>();
		}

		bool SaveChangesBeforeUpdatingStatus()
		{
			if (this.IncidentManagementGroup.HasChanges)
			{
				if ((this.FindForm() as ZForm).FireSaveButton() == ContinueWithSave.No)
				{
					var ingStatus = GetConcurrencyErrorProperties().FirstOrDefault(x => x.ColumnName == IncidentManagementGroup.Schema.ING_Status);
					if (ingStatus != null)
					{
						Globals.Message.ShowError(GetStatusConcurrencyErrorMessage(ingStatus), statusConcurrencyErrorMessageCaption);
					}
					else
					{
						var message = Res.GetString("2FAE933D-9F54-4DB0-B2C4-CC8F874A4E74", "Stage change aborted as conflicting changes require attention. Please review the form and try again.");
						Globals.Message.ShowWarning(message);
					}

					return false;
				}
			}
			else
			{
				var oldStatusBeforeReloading = this.IncidentManagementGroup.ING_Status;
				this.IncidentManagementGroup.Reload();
				if (oldStatusBeforeReloading != this.IncidentManagementGroup.ING_Status)
				{
					var lastEditUser = this.IncidentManagementGroup.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, this.IncidentManagementGroup.ING_SystemLastEditUser);
					var message = Res.GetString("C18D7BE5-4640-488B-A3C4-E71151C65410", "Operation canceled. {0} has updated the stage of this Group to {1}. Please review the form.", lastEditUser?.GS_FullName ?? IncidentManagementGroup.ING_SystemLastEditUser, this.IncidentManagementGroup.NowStage.DescriptionOnGroup);
					Globals.Message.ShowError(message);
					return false;
				}
			}

			return true;
		}

		void TryUpdateStatus(IncidentGroupStatusConfiguration newStatus, string appendMessage = "")
		{
			var oldStatus = IncidentManagementGroup.ING_Status;

			if (newStatus.ApprovalGate)
			{
				if (Globals.Message.Show(Res.GetString("725dc1cf-1431-4bb8-b998-4300717f14c9", "Approval is required to change the group stage from {0} to {1}. Do you wish to proceed with the change?", oldStatus, newStatus.Code), "Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
				{
					return;
				}
			}

			if ((newStatus.Code == IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code) || (newStatus.Code == IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code))
			{
				var publishedMsgExists = false;
				var publishedMessages = IncidentManagementGroup
								.IncidentManagementGroupMessages
								.Where(m => m.IGM_IsPublished);
				var msgTypeDesc = "";
				if (newStatus.Code == IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code)
				{
					msgTypeDesc = IncidentManagementGroupMessageTypePairList.Descriptions.Opening;
					publishedMsgExists = publishedMessages.Any(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Opening);
				}
				else
				{
					msgTypeDesc = IncidentManagementGroupMessageTypePairList.Descriptions.Closing;
					publishedMsgExists = publishedMessages.Any(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Closing);
				}

				if (!publishedMsgExists && newStatus.ControlIncidents)
				{
					Globals.Message.Show($"No {msgTypeDesc.ToLower()} broadcast message published", "Published Message Approval Gate", MessageBoxButtons.OK, DialogResult.OK);
					return;
				}
			}

			if ((oldStatus != IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code) && (newStatus.Code == IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code) && newStatus.ControlIncidents)
			{
				if (Globals.Message.Show("Confirm broadcast of opening message", "Broadcast Opening Message Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
				{
					return;
				}
			}

			if ((oldStatus != IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code) && (newStatus.Code == IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code) && newStatus.ControlIncidents)
			{
				if (Globals.Message.Show("Confirm broadcast of closing message", "Broadcast Closing Message Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
				{
					return;
				}
			}
			if ((IncidentManagementGroup.NowStage.ControlIncidents == false) && (newStatus.ControlIncidents == true))
			{
				if (Globals.Message.Show("Confirm incident control enabled", "Control Incidents to True Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
				{
					return;
				}
			}

			if ((IncidentManagementGroup.NowStage.ControlIncidents == true) && (newStatus.ControlIncidents == false))
			{
				if (Globals.Message.Show("Confirm incident control disabled", "Control Incidents to False Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
				{
					return;
				}

				if (newStatus.IncidentCompleted == false)
				{
					foreach (IncidentManagementLink linkedIncident in IncidentManagementGroup.LinkedIncidents)
					{
						linkedIncident.ReopenIncidentTasksClosedByGroup(appendMessage);
					}
				}
			}

			if ((IncidentManagementGroup.NowStage.GroupCompleted == false) && (newStatus.GroupCompleted == true))
			{
				if (IncidentManagementGroup.ING_ServiceOutage == ServiceOutageCodes.Active)
				{
					if (Globals.Message.Show("Confirm group completion", "Group Completed to True Approval Gate", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
					{
						return;
					}
				}
			}

			IncidentManagementGroup.ING_Status = newStatus.Code;
		}

		void TrySaveStatusChanges()
		{
			if (!this.IncidentManagementGroup.HasChanges)
			{
				return;
			}

			if ((this.FindForm() as ZForm).FireSaveButton() == ContinueWithSave.No)
			{
				var ingStatus = GetConcurrencyErrorProperties().FirstOrDefault(x => x.ColumnName == IncidentManagementGroup.Schema.ING_Status);
				if (ingStatus != null)
				{
					Globals.Message.ShowError(GetStatusConcurrencyErrorMessage(ingStatus), statusConcurrencyErrorMessageCaption);
				}
			}
		}

		void UpdateButtonEnabled(object sender, EventArgs e)
		{
			var previousStatus = IncidentManagementGroup.GetPreviousStatus();
			var enablePreviousButton = previousStatus != null && IncidentManagementGroup.NowStage.IsReversible;

			PreviousButton.Enabled = enablePreviousButton;
			NextButton.Enabled = IncidentManagementGroup.GetNextStatus() != null;

			PreviousButton.ToolTipCaption = previousStatus != null && !IncidentManagementGroup.NowStage.IsReversible ?
												ResString.GetMultilingualString("0688E5EB-281F-435B-A874-F0C5B619F8D7", "Previous stage no longer available")
												: null;
		}

		void UpdateButtonEnabled()
		{
			UpdateButtonEnabled(null, EventArgs.Empty);
		}

		#endregion

		void OverrideSourceModuleButton_Click(object sender, EventArgs e)
		{
			ShowSourceModuleFinderPopup("");
		}

		void ShowSourceModuleFinderPopup(ZString findReason)
		{
			var finder = new SourceModuleFinder(IncidentManagementGroup.ING_Product, IncidentManagementGroup.ModuleType, findReason, IncidentManagementGroup.Factory);
			finder.ProductAreaFilter = IncidentManagementGroup.ING_ProductArea;
			finder.ModuleFilter = IncidentManagementGroup.ING_Module;

			var form = new SourceModuleFinderForm(finder);
			form.ModuleMappingWithSourceModuleSelected += (sender, e) =>
			{
				RemoveShowSourceModuleFinderPopupEventHandlers();
				IncidentManagementGroup.ING_ProductArea = e.ModuleMappingWithSourceModule.ProductArea;
				IncidentManagementGroup.ING_Module = e.ModuleMappingWithSourceModule.ModuleCode;
				IncidentManagementGroup.ING_SourceModuleId = e.ModuleMappingWithSourceModule.SourceModuleCode;
				AddShowSourceModuleFinderPopupEventHandlers();

				SetSourceModuleLabelColor();
			};

			form.FormClosing += (sender, e) =>
			{
				IncidentManagementGroup.RecalculateProductArea();
				productAreaDropEdit.Text = IncidentManagementGroup.ING_ProductArea;
			};

			ZFormModaliser.ShowDialogAndDispose(form);
		}

		void SetSourceModuleLabelColor()
		{
			sourceModuleLabel.ForeColor = IncidentManagementGroup.IsSourceModuleOverriden ? Color.Green : Color.Black;
		}

		void AddShowSourceModuleFinderPopupEventHandlers()
		{
			IncidentManagementGroup.ING_ModuleInfo.ValueChanged += ING_ModuleInfo_ValueChanged;
		}

		void RemoveShowSourceModuleFinderPopupEventHandlers()
		{
			IncidentManagementGroup.ING_ModuleInfo.ValueChanged -= ING_ModuleInfo_ValueChanged;
		}

		void ING_ModuleInfo_ValueChanged(object sender, EventArgs e)
		{
			SetServiceTypeVisible();
		}

		void SetServiceTypeVisible()
		{
			var visibility = IncidentManagementGroup.Lookups.ServiceTypeList.Count > 0;
			serviceTypeCaption.Visible = visibility;
			serviceTypeDropEdit.Visible = visibility;

			if (!visibility && !IncidentManagementGroup.ING_ServiceType.IsEmpty)
			{
				IncidentManagementGroup.ING_ServiceType = ZString.Empty;
			}
		}

		void ServiceOutageStartMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(NewRaiseEventLogForm("SVS", "STA", "OUT"), ParentForm);
		}

		void ServiceOutageDowngradedMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(NewRaiseEventLogForm("SVM", "STA", "DWN"), ParentForm);
		}

		void ServiceRestoredMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(NewRaiseEventLogForm("SVM", "STA", "RES"), ParentForm);
		}

		ZStmALogAddForm NewRaiseEventLogForm(string eventCode, string key, string value)
		{
			var view = new StmALogCollectionView(IncidentManagementGroup);
			var form = new ZStmALogAddForm(view, IncidentManagementGroup.HasChanges, true);
			var newLog = (BaseStmALog)form.BusinessEntity;
			newLog.Master = IncidentManagementGroup;
			newLog.SL_SE_NKEvent = eventCode;
			newLog.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength("", new[] { new KeyValuePair<string, string>(key, value) });
			form.FormClosed += RefreshOutageDuration;
			return form;
		}

		void RefreshOutageDuration(object sender, EventArgs e)
		{
			IncidentManagementGroup?.OutageDurationInfo.RefreshBinding();
		}

		void ING_ServiceOutageInfo_ValueChanged(object sender, EventArgs e)
		{
			SetServiceOutageLabelColor();
		}

		void SetServiceOutageLabelColor()
		{
			switch (IncidentManagementGroup.ING_ServiceOutage)
			{
				case ServiceOutageCodes.Investigating:
					serviceOutageDropEdit.ForeColor = Color.Black;
					outageDurationTimeLabel.ForeColor = Color.Black;
					break;
				case ServiceOutageCodes.Active:
					serviceOutageDropEdit.ForeColor = Color.Red;
					outageDurationTimeLabel.ForeColor = Color.Red;
					break;
				case ServiceOutageCodes.Downgraded:
					serviceOutageDropEdit.ForeColor = Color.Orange;
					outageDurationTimeLabel.ForeColor = Color.Orange;
					break;
				case ServiceOutageCodes.Restored:
					serviceOutageDropEdit.ForeColor = Color.Green;
					outageDurationTimeLabel.ForeColor = Color.Green;
					break;
			}
		}

		#region IConversationView

		readonly IncidentManagementEConversationViewController conversationViewController;

		public JobConversation Conversation => IncidentManagementGroup.EConversation.Conversation;

		public TextBoxBase MessageTextBox => conversationMessageTextBox;

		public ZButton SendButton => sendMessageButton;

		public ZButton AddInternalCommentButton => null;

		public ZButton BroadcastButton => null;

		#endregion

		void TriageAssistButton_Click(object sender, EventArgs e)
		{
			if (TriageForm != null)
			{
				TriageForm.Focus();
				return;
			}

			if (!IncidentManagementGroup.IsInDatabase)
			{
				Globals.Message.ShowError("You must save the form before using Triage Assist.");
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var bizObjReloaded = newFactory.Load<IncidentManagementGroup>(IncidentManagementGroup.PK);
			var assistObject = new TriageAssistBusinessObject(bizObjReloaded);
			TriageForm = new TriageAssistForm(assistObject);
			
			TriageForm.FormClosed += (_, x_) => TriageForm = null;

			assistObject.OnTriageAssistSaved += (_, x_) =>
			{
				if (assistObject.Parent.Factory.HasContext(SupportIncident.Context.OnSecondFactorySave))
				{
					return;
				}

				TriageAssistHelper.TriageAssistOnSaved(new TriageAssistBusinessObject(IncidentManagementGroup), IncidentManagementGroup);
			};
			TriageForm.Show();
		}

		public TriageAssistForm TriageForm { get; private set; }
	}
}
