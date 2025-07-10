using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Client.EDI.IncidentManager.GUI.ReopenIncidentPopup;
using MenuItem = System.Windows.Forms.MenuItem;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class SupportIncidentForm : ZTemplateForm, IConversationView
	{
		public SupportIncidentForm(SupportIncident supportIncident)
			: base(supportIncident)
		{
			this.CaptionRenderingEnabled = true;

			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ClientControllerRegistration.SupportIncidentClientOrgLicence);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ClientControllerRegistration.IncidentEConversationPlugIn);
			PlugIns.Add(ControllerIDs.Audit);

			SetControlModuleIDs();
			SetupActionMenuItemsEvent();

			IncidentNumberTextBox.Font = new Font(IncidentNumberTextBox.Font, FontStyle.Bold);

			WorkflowTabPage.Initialize(supportIncident);

			SetupLicenceKeyBuilderPlugin();
			SetupJobInvoicingPlugIn();
			SetupMenuItemEvent();
			SetupStaffAssignmentLabelsEvent();
			SetupQuickActionButtonsEvent();
			SetupCriticalityDropDown();
			SetupEConversationControlsAndCloseControlsEvent();
			SetupTriageAssist();
			SetupClosureResolution();

			spellChecker = SpellChecker.InitialiseSpellcheck(ConversationMessageTextBox, "SupportIncidentForm_ConversationMessageTextBox");
			_ = SpellChecker.InitialiseSpellcheck(BusinessRequirementRichTextBox, "SupportIncidentForm_BusinessRequirementRichTextBox");
			UpdateKnownNames();

			conversationViewController = new IncidentConversationViewController();
			conversationViewController.Initialize(this);

			if (EDIDataRegistry.Instance.EnableIncidentSimilarityFunctionality.Value)
			{
				panelSimilarIncidents.LinkSplitter(splitterSimilarIncidents);
			}
			else
			{
				splitterSimilarIncidents.Dispose();
				splitterSimilarIncidents = null;
				panelSimilarIncidents.Dispose();
				panelSimilarIncidents = null;
			}

			splitter2.AllowOverlap(splitContainer2);
			splitter1.AllowOverlap(featureSplitContainer);
		}

		void SetupClosureResolution()
		{
			SetupClosureResolutionVisible(null, EventArgs.Empty);
			BusinessEntity.IM_ResolutionCodeInfo.ValueChanged += SetupClosureResolutionVisible;
		}

		void SetupClosureResolutionVisible(object sender, EventArgs e)
		{
			closureResolutionDropEdit.Visible = BusinessEntity?.IsCurrentResolutionCodeClosedOrResolved ?? false;
		}

		void SetupLicenceKeyBuilderPlugin()
		{
			if (BusinessEntity != null)
			{
				licenceKeyBuilderControl = PlugIns.GetPlugIn(ClientControllerRegistration.SupportIncidentClientOrgLicence).UserControl as LicenceKeyBuilderControl;

				SetupTokenButtonsVisibility();

				BusinessEntity.IM_StatusInfo.ValueChanged += new EventHandler(TokenButton_ValueChanged);
				BusinessEntity.IM_ResolutionCodeInfo.ValueChanged += new EventHandler(TokenButton_ValueChanged);
			}
		}

		void SetupTokenButtonsVisibility()
		{
			if (BusinessEntity.IsClosedOrCancelled)
			{
				licenceKeyBuilderControl.AreTokenButtonsVisible = BusinessEntity.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
				return;
			}
			licenceKeyBuilderControl.AreTokenButtonsVisible = true;
		}

		void TokenButton_ValueChanged(object sender, EventArgs e)
		{
			SetupTokenButtonsVisibility();
		}

		LicenceKeyBuilderControl licenceKeyBuilderControl;

		internal protected SpellChecker spellChecker;

		void UpdateKnownNames()
		{
			var knownNames = new List<string>();

			knownNames.Add(BusinessEntity?.Contact?.Name);
			BusinessEntity?.EConversation.ExistingConversation?.Staff.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			BusinessEntity?.EConversation.ExistingConversation?.RelatedParties.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			BusinessEntity?.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(task => knownNames.Add(task.StaffName));

			spellChecker.UpdateWordsToIgnore(knownNames.Where(name => !string.IsNullOrWhiteSpace(name)));
		}

		readonly IncidentConversationViewController conversationViewController;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning && BusinessEntity != null)
			{
				SplitterState.Persist(this.splitContainer5);
				SetCompanyControlsVisibility();
				SetModuleDropEditVisibility();
				SetSourceModuleOverrideVisibility();
				SetSourceModuleLabelColor();
				SetupProjectRelatedFeatureRequestProperties();
				SetSourceDropEditReadOnlyIfNeeded();
				SetupQuickActionButtons(this, EventArgs.Empty);
				SetServiceTypeVisible();

				// Textbox isn't binded to a property so can manually set its MaxLength here. ZTextBox hides this property in designer view.
				ConversationMessageTextBox.MaxLength = 32000;
				conversationControl = new EConversationMessageListUserControl(false);
				conversationControl.messagesLayoutPanel.SuspendLayout();
				conversationControl.SetDataBinding(BusinessEntity, "EConversation");
				conversationControl.Dock = DockStyle.Fill;
				splitContainer5.Panel2.Controls.Add(conversationControl);

				BusinessEntity.EConversation.MessageCountChanged += RefreshEConversationControls;
				BusinessEntity.IM_ProductInfo.ValueChanged += IM_ProductInfo_ValueChanged;
				BusinessEntity.IM_LDInfo.ValueChanged += IM_LDInfo_ValueChanged;
				BusinessEntity.OnCloseIncident += BusinessEntity_OnCloseIncident;
				AddShowSourceModuleFinderPopupEventHandlers();

				SetupTaskButtons();
				BusinessEntity.OnCurrentOrNextTaskStatusChange += SetupTaskButtonsAndEConversationControlsAndCloseIncidentControls;
				BusinessEntity.WorkflowItems.Tasks.CountChanged += SetupTaskButtonsAndEConversationControlsAndCloseIncidentControls;
				SetupEstimateAndQuoteFields();
				conversationControl.messagesLayoutPanel.ResumeLayout(false);
				conversationControl.messagesLayoutPanel.PerformLayout();

				outageDurationRefreshTimer = new ProxyWindowsTimer();
				outageDurationRefreshTimer.Interval = (int)new TimeSpan(0, 0, 1).TotalMilliseconds;
				outageDurationRefreshTimer.Tick += new EventHandler(OutageDurationRefreshTimer_Tick);
				outageDurationRefreshTimer.Enabled = true;
				RecalculateServiceOutageStatusMenuItems();

				if (closureResolutionDropEdit.Visible && !string.IsNullOrEmpty(closureResolutionDropEdit.CodeBox.Text) && string.IsNullOrEmpty(closureResolutionDropEdit.DescriptionBox.Text))
				{
					var desc = BusinessEntity.Lookups.AllCloseStatusDispositionList.GetDescriptionFromCode(closureResolutionDropEdit.CodeBox.Text);
					closureResolutionDropEdit.DescriptionBox.Text = desc;
				}

				SetupSystemVersionBoundLabel();
			}
		}

		IWindowsTimer outageDurationRefreshTimer;

		void OutageDurationRefreshTimer_Tick(object sender, EventArgs e)
		{
			RecalculateServiceOutageStatusMenuItems();
		}

		void SetupEstimateAndQuoteFields()
		{
			SetCaptionAndDescription(EstMinDevHours, EDIDataRegistry.Instance.EstMinDevHoursLabel.Value);
			SetCaptionAndDescription(EstMaxDevHours, EDIDataRegistry.Instance.EstMaxDevHoursLabel.Value);
			SetCaptionAndDescription(EstSentDate, EDIDataRegistry.Instance.EstSentDateLabel.Value);
			SetCaptionAndDescription(EstExpiryDate, EDIDataRegistry.Instance.EstExpiryDateLabel.Value);
			SetCaptionAndDescription(EstRequestDate, EDIDataRegistry.Instance.EstRequestDateLabel.Value);
			SetCaptionAndDescription(EstPaymentTermsDropEdit, EDIDataRegistry.Instance.PaymentTermsLabel.Value);
			SetCaptionAndDescription(EstCurrencyBox, EDIDataRegistry.Instance.CurrencyLabel.Value);
			SetCaptionAndDescription(EstMinMonthly, EDIDataRegistry.Instance.EstMinMonthlyLabel.Value);
			SetCaptionAndDescription(EstMaxMonthly, EDIDataRegistry.Instance.EstMaxMonthlyLabel.Value);
			SetCaptionAndDescription(EstMinOneOff, EDIDataRegistry.Instance.EstMinOneOffLabel.Value);
			SetCaptionAndDescription(EstMaxOneOff, EDIDataRegistry.Instance.EstMaxOneOffLabel.Value);
			SetCaptionAndDescription(EstCancellationFee, EDIDataRegistry.Instance.CancellationFeeLabel.Value);
			SetCaptionAndDescription(EstExpressDelCutOff, EDIDataRegistry.Instance.EstExpressDelCutOffLabel.Value);

			SetCaptionAndDescription(QteMinDevHours, EDIDataRegistry.Instance.QteMinDevHoursLabel.Value);
			SetCaptionAndDescription(QteMaxDevHours, EDIDataRegistry.Instance.QteMaxDevHoursLabel.Value);
			SetCaptionAndDescription(QteSentDate, EDIDataRegistry.Instance.QteSentDateLabel.Value);
			SetCaptionAndDescription(QteExpiryDate, EDIDataRegistry.Instance.QteExpiryDateLabel.Value);
			SetCaptionAndDescription(QteAcceptedDate, EDIDataRegistry.Instance.QteAcceptedDateLabel.Value);
			SetCaptionAndDescription(QteDateDelivered, EDIDataRegistry.Instance.QteDateDeliveredLabel.Value);
			SetCaptionAndDescription(QtePaymentTermsDropEdit, EDIDataRegistry.Instance.PaymentTermsLabel.Value);
			SetCaptionAndDescription(QteCurrencyBox, EDIDataRegistry.Instance.CurrencyLabel.Value);
			SetCaptionAndDescription(QtePaymentTypeDropEdit, EDIDataRegistry.Instance.QtePaymentTypeLabel.Value);
			SetCaptionAndDescription(QteAmount, EDIDataRegistry.Instance.QteAmountLabel.Value);
			SetCaptionAndDescription(QteOneOffUpfront, EDIDataRegistry.Instance.QteOneOffUpfrontLabel.Value);
			SetCaptionAndDescription(QteCancellationFee, EDIDataRegistry.Instance.CancellationFeeLabel.Value);
			SetCaptionAndDescription(QteHeadStartIncludedCheckBox, EDIDataRegistry.Instance.QteHeadStartIncludedLabel.Value);
			SetCaptionAndDescription(QteHeadStartSurcharge, EDIDataRegistry.Instance.SurchargeLabel.Value);
			SetCaptionAndDescription(QteExpressDeliveryIncludedCheckBox, EDIDataRegistry.Instance.QteExpressDeliveryIncludedLabel.Value);
			SetCaptionAndDescription(QteExpressDeliverySurcharge, EDIDataRegistry.Instance.SurchargeLabel.Value);
		}

		void SetCaptionAndDescription(Control control, string value)
		{
			control.GetExtension<LabelCaptionRenderer>().Caption = value;
			control.GetExtension<HintExtension>().Description = value;
		}

		EConversationMessageListUserControl conversationControl;

		void SetupJobInvoicingPlugIn()
		{
			ZPlugIn plugIn = PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			if (plugIn.UserControl is JobInvoicingUserControl)
			{
				LocalClientControl = ((JobInvoicingUserControl)plugIn.UserControl).JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard;
			}

			if (BusinessEntity.IM_InvoicingLocalClientHasInvoicingPreferencesNote)
			{
				if (LocalClientControl != null)
				{
					LocalClientControl.ForeColor = Color.Red;
				}
			}

			BusinessEntity.IM_OA_BranchAddressInfo.ValueChanged += new EventHandler(Client_ValueChanged);
		}

		public new SupportIncident BusinessEntity
		{
			get { return (SupportIncident)base.BusinessEntity; }
		}

		#region Business Entity Event Handlers

		void BusinessEntity_OnCloseIncident(object sender, SupportIncident.TaskStatusChangeEventArgs e)
		{
			var incident = BusinessEntity;
			if (incident != null && !incident.SuspendTriggerCloseIncident && (!incident.IsPendingUpgrade && !CloseIncident()))
			{
				if (!e.IgnoreStatusRollback && sender is SupportIncidentProcessTask task)
				{
					incident.OnCloseIncident -= BusinessEntity_OnCloseIncident;
					task.P9_Status = e.OriginalTaskStatus;
					incident.CalculateWorkflowDependentProperties(shouldOverrideDisposition: true);
					incident.OnCloseIncident += BusinessEntity_OnCloseIncident;
				}
			}
		}

		void SetupTaskButtonsAndEConversationControlsAndCloseIncidentControls(object sender, EventArgs e)
		{
			if (!BusinessEntity.WorkflowItems.IsLoading)
			{
				SetupTaskButtons();
				SetupEConversationControlsAndCloseControls();
			}
		}

		void RefreshEConversationControls(object sender, EventArgs e)
		{
			conversationControl.RefreshMessages();
		}

		#endregion

		#region Project Related Feature Request Properties

		void SetupProjectRelatedFeatureRequestProperties()
		{
			SetupProjectRelatedFeatureRequesPropertiesVisibility();
			BusinessEntity.RelatedProjectPKInfo.ValueChanged += new EventHandler(delegate
			{ SetupProjectRelatedFeatureRequesPropertiesVisibility(); });
			BusinessEntity.IM_SourceInfo.ValueChanged += new EventHandler(delegate
			{ SetupProjectRelatedFeatureRequesPropertiesVisibility(); });
			this.featureDetailsClientSplitContainer.SplitterDistance = zGroupBox6.Bottom;
		}

		void SetupProjectRelatedFeatureRequesPropertiesVisibility()
		{
			if (BusinessEntity != null)
			{
				this.businessConsultantCodeFindBox.Visible = BusinessEntity.IsProjectRelatedIncident;
				this.featureRequestAnalysisPage.Text = BusinessEntity.IsProjectRelatedIncident ? "Project Analysis" : "Analysis";
			}
		}

		#endregion

		#region Database

		void IM_LDInfo_ValueChanged(object sender, EventArgs e)
		{
			SetCompanyControlsVisibility();
		}

		void SetCompanyControlsVisibility()
		{
			bool shouldShowCompanyControls = false;

			if (BusinessEntity.IsInDatabase)
			{
				shouldShowCompanyControls = !BusinessEntity.IM_LCC.IsEmpty || BusinessEntity.ClientCompanyCode_Visible;
			}
			else
			{
				shouldShowCompanyControls = BusinessEntity.IM_LD.IsEmpty || BusinessEntity.ClientCompanyCode_Visible;
			}
			SupportCompanyDropEdit.Visible = shouldShowCompanyControls;
		}

		#endregion

		#region Product

		void IM_ProductInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSourceModuleOverrideVisibility();
			SetModuleDropEditVisibility();
		}

		#endregion

		void SetSourceDropEditReadOnlyIfNeeded()
		{
			sourceDropEdit.ReadOnly =
				BusinessEntity.IM_Source == SupportIncidentLookups.SourceListConstants.ERequestPortal
				|| BusinessEntity.IM_Source == SupportIncidentLookups.SourceListConstants.IssueManagerReported
				|| BusinessEntity.IM_Source == SupportIncidentLookups.SourceListConstants.CreatedFromProject
				|| BusinessEntity.IM_Source == SupportIncidentLookups.SourceListConstants.APIInboundInternal
				|| BusinessEntity.IM_Source == SupportIncidentLookups.SourceListConstants.APIInboundExternal;
		}

		#region Criticality

		void SetupCriticalityDropDown()
		{
			BusinessEntity.IM_PriorityInfo.ValueChanged += new EventHandler(IM_PriorityInfo_ValueChanged);
		}

		void IM_PriorityInfo_ValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity.IsInDatabase && BusinessEntity.IM_Priority != lastCriticality && !BusinessEntity.IsSynchronisingCriticalityFromAction)
			{
				BaseIncidentPopupForm popupForm = null;

				var stageCriticalityMapping = BusinessEntity.Lookups.ActiveStageCriticalityMapping;
				if (BusinessEntity.Lookups.CriticalityList.ContainsCode(BusinessEntity.IM_Priority)
					&& stageCriticalityMapping.ContainsKey(BusinessEntity.IM_Category)
					&& !stageCriticalityMapping[BusinessEntity.IM_Category].Contains(BusinessEntity.IM_Priority))
				{
					var defaultStageMapping = BusinessEntity.Lookups.CriticalityDefaultStageMapping;
					var action = new SupportIncidentEscalateAction(BusinessEntity, lastCriticality);
					if (defaultStageMapping.ContainsKey(BusinessEntity.IM_Priority))
					{
						action.EscalationStage = defaultStageMapping[BusinessEntity.IM_Priority];
					}
					action.EscalationCriticality = BusinessEntity.IM_Priority;
					if (CanPopupEscalateIncidentForm)
					{
						popupForm = new EscalateIncidentPopupForm(action);
					}
				}
				else
				{
					var validCodeList = BusinessEntity.Lookups.ClosedSupportStatusDispositionList;
					if (!string.IsNullOrEmpty(BusinessEntity.IM_ResolutionCode)
						&& !validCodeList.ContainsCode(BusinessEntity.IM_ResolutionCode)
						&& BusinessEntity.IM_Status == IncidentMainLookups.Status.Closed
						&& BusinessEntity.IM_Category == SupportIncidentCategoriesList.Codes.Support)
					{
						if (!CloseIncident())
						{
							BusinessEntity.IM_Priority = lastCriticality;
							CriticalityDropEdit.Text = lastCriticality;
							RequestedPriorityDropEdit.Text = lastCriticality;
						}
					}
					else
					{
						BusinessEntity.Validation.ValidateIM_Priority();
						if (!BusinessEntity.IM_PriorityInfo.HasErrors())
						{
							if (BusinessEntity.IM_Status != IncidentMainLookups.Status.Closed)
							{
								BusinessEntity.TriggerCriticalityChangeEvent();
							}
						}
					}
				}

				if (popupForm != null)
				{
					if (ShowPopupForm(popupForm) != DialogResult.OK)
					{
						BusinessEntity.IM_Priority = lastCriticality;
						CriticalityDropEdit.Text = lastCriticality;
						RequestedPriorityDropEdit.Text = lastCriticality;
					}
				}
			}

			if (BusinessEntity.IM_Priority.Equals(Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest))
			{
				this.AwaitingResponseButton.Image = global::Enterprise.Client.EDI.IncidentManager.GUI.Properties.Resources.arrow_drop1;
				this.AwaitingResponseButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			}
			else
			{
				this.AwaitingResponseButton.Image = null;
			}

			SetSourceModuleOverrideVisibility();
			SetModuleDropEditVisibility();
			SetupActionMenuItems();
		}

		ZString lastCriticality => (ZString)BusinessEntity.IM_PriorityInfo.OriginalValue;

		#endregion

		#region Module

		void SetModuleDropEditVisibility()
		{
			menuSectionDropEdit.Visible = BusinessEntity.ModuleType == ModuleListType.MenuSection && !BusinessEntity.IM_Product.IsEmpty;
			cr8ModuleDropEdit.Visible = BusinessEntity.ModuleType == ModuleListType.Cr8;
			cr9ModuleDropEdit.Visible = BusinessEntity.ModuleType == ModuleListType.Cr9;

			defectMenuSectionDropEdit.Visible = BusinessEntity.ModuleType == ModuleListType.MenuSection && !BusinessEntity.IM_Product.IsEmpty;
			defectCr8ModuleDropEdit.Visible = BusinessEntity.ModuleType == ModuleListType.Cr8;
			defectCr9ModuleDropEdit.Visible = BusinessEntity.ModuleType == ModuleListType.Cr9;
		}

		#endregion

		#region SourceModule

		internal bool ShouldAllowSourceModuleOverride
		{
			get
			{
				return BusinessEntity.ModuleType != ModuleListType.Unspecified;
			}
		}

		void SetSourceModuleOverrideVisibility()
		{
			OverrideSourceModuleButton.Visible = ShouldAllowSourceModuleOverride;
		}

		void SetSourceModuleLabelColor()
		{
			sourceModuleLabel.ForeColor = BusinessEntity.IsSourceModuleOverriden ? Color.Green : Color.Black;
		}

		#endregion

		#region MenuItem

		void SetupMenuItemEvent()
		{
			SetupMenuItemProperties();
			BusinessEntity.WorkflowItems.CountChanged += new CollectionCountChangedEventHandler(delegate
			{ SetupMenuItemProperties(); });
			BusinessEntity.IM_StatusInfo.ValueChanged += new EventHandler(delegate
			{ SetupMenuItemProperties(); });
			BusinessEntity.IM_ResolutionCodeInfo.ValueChanged += new EventHandler(delegate
			{ SetupActionMenuItems(); });
			BusinessEntity.IM_CategoryInfo.ValueChanged += new EventHandler(delegate
			{ UpdateUPOAndUDOMenuItemStatus(); });
			BusinessEntity.RelatedWorkItems.CountChanged += (e, x) => { UpdateUPOAndUDOMenuItemStatus(); };

			if (revertToAwaitingResponseItem != null)
			{
				revertToAwaitingResponseItem.Enabled = BusinessEntity.CanRevertToAwaitingResponse;
			}

			if (closeOnBehalfOfClientItem != null)
			{
				closeOnBehalfOfClientItem.Enabled = BusinessEntity.CanCloseOnBehalfOfClient;
			}

			BusinessEntity.IM_ResolutionCodeInfo.ValueChanged += (e, x) =>
			{
				if (BusinessEntity != null)
				{
					if (revertToAwaitingResponseItem != null)
					{
						revertToAwaitingResponseItem.Enabled = BusinessEntity.CanRevertToAwaitingResponse;
					}

					if (closeOnBehalfOfClientItem != null)
					{
						closeOnBehalfOfClientItem.Enabled = BusinessEntity.CanCloseOnBehalfOfClient;
					}
				}
			};
		}

		void SetupMenuItemProperties()
		{
			if (BusinessEntity != null)
			{
				bool hasNoWorkflowTasks = (BusinessEntity.WorkflowItems.Tasks.Count == 0);

				if (assignedToMenuItem != null)
				{
					assignedToMenuItem.Enabled = hasNoWorkflowTasks;
				}

				if (investigateMenuItem != null)
				{
					investigateMenuItem.Enabled = hasNoWorkflowTasks;
				}

				if (reOpenMenuItem != null)
				{
					reOpenMenuItem.Enabled = BusinessEntity.CanReOpen;
				}

				UpdateUPOAndUDOMenuItemStatus();
			}
		}

		#endregion

		#region Staff Assignment Labels

		void SetupStaffAssignmentLabelsEvent()
		{
			SetupStaffAssignmentAndTaskStatusLabels();
			BusinessEntity.IM_StatusInfo.ValueChanged += new EventHandler(delegate
			{ SetupStaffAssignmentAndTaskStatusLabels(); });
		}

		#endregion

		void SetupStaffAssignmentAndTaskStatusLabels()
		{
			if (BusinessEntity != null)
			{
				if (BusinessEntity.IM_Status == SupportIncidentLookups.Status.Closed)
				{
					ControlDpiScalingHelper.SetWidth(ref StatusLabel, 107, true);
					ControlDpiScalingHelper.SetWidth(ref AssignedToLabel, 115, true);
				}
				else
				{
					ControlDpiScalingHelper.SetWidth(ref StatusLabel, 82, true);
					ControlDpiScalingHelper.SetWidth(ref AssignedToLabel, 90, true);
				}

				StatusDescription.Location = ControlDpiScalingHelper.NewScaledPoint(AssignedToLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), StatusLabel.Top, false);
				CurrentTaskLabel.Location = ControlDpiScalingHelper.NewScaledPoint(AssignedToLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), CurrentTaskLabelText.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				OverallAssignedToCodeLabel.Location = ControlDpiScalingHelper.NewScaledPoint(AssignedToLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), OverallAssignedToCodeLabel.Top, false);
				OverallAssignedToDescriptionLabel.Location = ControlDpiScalingHelper.NewScaledPoint(OverallAssignedToCodeLabel.Right, OverallAssignedToCodeLabel.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			}
		}

		#region EConversation

		void SetupEConversationControlsAndCloseControlsEvent()
		{
			this.ConversationMessageTextBox.SetDataBinding(BusinessEntity.EConversation.Conversation, nameof(BusinessEntity.EConversation.Conversation.NextMessagePlainText));
			BusinessEntity.IM_StatusInfo.ValueChanged += new EventHandler(delegate
			{ SetupEConversationControlsAndCloseControls(); });
			SetupEConversationControlsAndCloseControls();
		}

		void SetupEConversationControlsAndCloseControls()
		{
			if (BusinessEntity == null)
			{
				return;
			}

			var isAllowed = CalculateEConversationTextBoxIsAllowed();

			if (BusinessEntity.ShouldShowClosedDispositions)
			{
				SetCloseIncidentControlsEnabled(false);
				DisableEConversation("eConversation is disabled for this eRequest Status. Please go to Actions > Reopen to enable eConversation");
			}
			else if (!BusinessEntity.IsInDatabase)
			{
				if (isAllowed)
				{
					DisableEConversation("eConversation is not available until incident is saved");
					SetCloseIncidentControlsEnabled(true);
				}
				else
				{
					DisableEConversation("eConversation is not available until incident is saved. Please mark a task as Working before closing the incident");
					SetCloseIncidentControlsEnabled(false);
				}
			}
			else if (!isAllowed)
			{
				DisableEConversation("Please mark a task as Working before communicating with the client or closing the incident");
				SetCloseIncidentControlsEnabled(false);
			}
			else
			{
				if (!ConversationMessageTextBox.Enabled)
				{
					ConversationMessageTextBox.Text = string.Empty;
					ConversationMessageTextBox.Enabled = true;
					this.AwaitingResponseButton.Enabled = true;

					SetCloseIncidentControlsEnabled(true);
				}
				SetupEConversationTabPage(true, ZString.Empty);
			}
		}

		ProcessTask userLastWorkingTask;

		bool CalculateEConversationTextBoxIsAllowed()
		{
			if (BusinessEntity == null)
			{
				return false;
			}

			if (EDISecurityCheckpoints.CustomerServiceIncidentAllowSendEConvAndCloseIncident.IsAllowed)
			{
				return true;
			}

			var hasWorkItemWithWorkingStatus = BusinessEntity.IM_Status != SupportIncidentLookups.Status.Closed
				&& BusinessEntity.RelatedWorkItems.Cast<NewWorkItem>()
				.Any(x => x.WKI_Status != ProcessTaskStatusCodeList.Codes.Closed && x.WKI_Status != ProcessTaskStatusCodeList.Codes.Cancelled);
			if (hasWorkItemWithWorkingStatus)
			{
				return true;
			}

			var currentUserTasks = BusinessEntity.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Where(x => x.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code);
			if (currentUserTasks.IsNullOrEmpty())
			{
				return false;
			}

			var firstActiveTask = currentUserTasks.FirstOrDefault(x => x.P9_Status == ProcessTaskStatusCodeList.Codes.Working);
			if (firstActiveTask != null)
			{
				userLastWorkingTask = firstActiveTask;
				return true;
			}

			var cancelAction = EnsureNoUnsentEConversationMessage().isEConversationActionCancelled;
			if (cancelAction && userLastWorkingTask != null)
			{
				userLastWorkingTask.P9_Status = SupportIncidentLookups.Status.Working;
				return true;
			}
			else
			{
				return false;
			}
		}

		void DisableEConversation(ZString reason)
		{
			AwaitingResponseButton.Enabled = false;
			SendMessageButton.Enabled = false;
			ConversationMessageTextBox.Enabled = false;
			ConversationMessageTextBox.Text = reason;

			SetupEConversationTabPage(false, reason);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = EnsureNoUnsentEConversationMessage().SaveAction;
			}

			return result;
		}

		void SetupEConversationTabPage(bool enabled, ZString reason)
		{
			var eConvView = PlugIns.Instances.OfType<IncidentConversationPlugin>().FirstOrDefault()?.UserControl as IConversationView;
			if (eConvView != null)
			{
				eConversationTabTextBox = eConvView.MessageTextBox;
				eConversationTabTextBox.Enabled = enabled;
				eConversationTabTextBox.Text = reason;

				var eConv = (BusinessEntity as IConversationProvider)?.eConversation;
				if (eConv != null)
				{
					eConv.NextMessage = reason.IsEmpty ? ZBlob.Empty : (ZBlob)ORtfTextUtil.TextToRtfBytes(reason);
					eConv.NextMessagePlainText = reason.IsEmpty ? ZString.Empty : reason;
				}

				if (eConvView is IncidentConversationTabControl incidentConversationTabControl)
				{
					eConversationTabViewController = incidentConversationTabControl.Controller as IncidentConversationViewController;
				}
			}
		}

		IncidentConversationViewController eConversationTabViewController;
		TextBoxBase eConversationTabTextBox;

		void SetCloseIncidentControlsEnabled(bool enabled)
		{
			CloseIncidentButton.Enabled = enabled;
			if (closeMenuItem != null)
			{
				closeMenuItem.Enabled = enabled;
			}
		}

		(ContinueWithSave SaveAction, bool isEConversationActionCancelled) EnsureNoUnsentEConversationMessage()
		{
			if (!ConversationMessageTextBox.Enabled)
			{
				return (ContinueWithSave.Yes, false);
			}

			var hasUnsentMessageOnCustomerServiceTab = !string.IsNullOrEmpty(ConversationMessageTextBox.Text);
			var hasUnsentMessageInEConversationPlugin = eConversationTabTextBox != null && !string.IsNullOrEmpty(eConversationTabTextBox.Text);

			if (BusinessEntity.IsInDatabase
					&& BusinessEntity.IM_Priority != Constants.CustomerService.CriticalityCodes.CR1_SystemDown
					&& (ZString)BusinessEntity.IM_PriorityInfo.OriginalValue != Constants.CustomerService.CriticalityCodes.CR1_SystemDown)
			{
				if (hasUnsentMessageOnCustomerServiceTab && hasUnsentMessageInEConversationPlugin)
				{
					var discardUnsentMessageResult = Globals.Message.Show("You have unsent messages in multiple eConversation text boxes. If you continue with save, the contents will be discarded. Continue with save?", "Discard unsent messages?", MessageBoxButtons.YesNo, DialogResult.No);
					if (discardUnsentMessageResult == DialogResult.Yes)
					{
						conversationViewController.ClearMessage();
						eConversationTabViewController?.ClearMessage();
						return (ContinueWithSave.Yes, false);
					}
					else
					{
						return (ContinueWithSave.No, false);
					}
				}
				else if (hasUnsentMessageOnCustomerServiceTab || hasUnsentMessageInEConversationPlugin)
				{
					var awaitingResponseButtonVisible = BusinessEntity.CanCloseAwaitingResponse;
					using (var messageForm = CreateSendEConversationForm(hasUnsentMessageOnCustomerServiceTab, awaitingResponseButtonVisible))
					{
						ZFormModaliser.ShowDialogWithoutDispose(messageForm);
						var controller = hasUnsentMessageInEConversationPlugin ? eConversationTabViewController : conversationViewController;
						var isCancelAction = messageForm.Action == SendEConversationForm.PerformAction.Cancel;

						switch (messageForm.Action)
						{
							case SendEConversationForm.PerformAction.Send:
								return (controller?.SendMessage(messageForm.MessageText) ?? true ? ContinueWithSave.Yes : ContinueWithSave.No, isCancelAction);
							case SendEConversationForm.PerformAction.AwaitingResponse:
								return (controller?.AwaitingResponse(messageForm.MessageText) ?? true ? ContinueWithSave.Yes : ContinueWithSave.No, isCancelAction);
							case SendEConversationForm.PerformAction.Discard:
								controller?.ClearMessage();
								return (ContinueWithSave.Yes, isCancelAction);
							case SendEConversationForm.PerformAction.Cancel:
								return (ContinueWithSave.No, isCancelAction);
							default:
								break;
						}
					}
				}
			}

			return (ContinueWithSave.Yes, false);
		}

		protected virtual SendEConversationForm CreateSendEConversationForm(bool hasUnsentMessageOnCustomerServiceTab, bool awaitingResponseButtonVisible)
		{
			return hasUnsentMessageOnCustomerServiceTab ?
						new SendEConversationForm(ConversationMessageTextBox.Text, shouldShowAwaitingResponse: awaitingResponseButtonVisible) :
						new SendEConversationForm(BusinessEntity.EConversation.Conversation.NextMessage, shouldShowAwaitingResponse: awaitingResponseButtonVisible);
		}

		public bool IsEConversationEnabled => ConversationMessageTextBox.Enabled;

		#endregion

		#region Stage Change

		void SetupQuickActionButtonsEvent()
		{
			BusinessEntity.IM_CategoryInfo.ValueChanged += SetupQuickActionButtons;
			BusinessEntity.IM_ResolutionCodeInfo.ValueChanged += SetupQuickActionButtons;
		}

		void SetupQuickActionButtons(object sender, EventArgs e)
		{
			bool awaitingResponseButtonVisible = false;
			bool closeIncidentButtonVisible = false;
			if (BusinessEntity != null)
			{
				awaitingResponseButtonVisible = BusinessEntity.CanCloseAwaitingResponse;
				closeIncidentButtonVisible = (BusinessEntity.IM_Category == SupportIncidentCategoriesList.Codes.Support);
			}
			CloseIncidentButton.Visible = closeIncidentButtonVisible;
			AwaitingResponseButton.Visible = awaitingResponseButtonVisible;

			int padding = ControlDpiScalingHelper.ScaleToCurrentDpiX(4);

			if (!closeIncidentButtonVisible)
			{
				SendMessageButton.Location = ControlDpiScalingHelper.NewScaledPoint(CloseIncidentButton.Location.X + CloseIncidentButton.Width - SendMessageButton.Width, SendMessageButton.Location.Y, false);
			}
			else
			{
				SendMessageButton.Location = ControlDpiScalingHelper.NewScaledPoint(CloseIncidentButton.Location.X - SendMessageButton.Width - padding, SendMessageButton.Location.Y, false);
			}

			AwaitingResponseButton.Location = ControlDpiScalingHelper.NewScaledPoint(SendMessageButton.Location.X - AwaitingResponseButton.Width - padding, AwaitingResponseButton.Location.Y, false);

			if (awaitingResponseButtonVisible)
			{
				ControlDpiScalingHelper.SetWidth(ref ConversationMessageTextBox, AwaitingResponseButton.Location.X - padding - ConversationMessageTextBox.Location.X, false);
			}
			else
			{
				ControlDpiScalingHelper.SetWidth(ref ConversationMessageTextBox, SendMessageButton.Location.X - padding - ConversationMessageTextBox.Location.X, false);
			}
		}

		#endregion

		#region Task buttons

		protected virtual void SetupTaskButtons()
		{
			WorkOnCurrentTaskButton.Enabled = false;
			SuspendCurrentTaskButton.Enabled = false;
			CloseCurrentTaskButton.Enabled = false;
			CancelCurrentTaskButton.Enabled = false;

			WorkOnCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.WorkOnTask;
			SuspendCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.SuspendTask;
			CloseCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.CloseTask;
			CancelCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.CancelTask;

			var currentOrNextTask = BusinessEntity.CurrentOrNextTask;

			if (currentOrNextTask != null)
			{
				if (currentOrNextTask.P9_Status == SupportIncidentLookups.Status.Working)
				{
					SuspendCurrentTaskButton.Enabled = true;
					CloseCurrentTaskButton.Enabled = true;
					WorkOnCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.WorkOnTask_Active;
				}
				else if (currentOrNextTask.P9_Status == SupportIncidentLookups.Status.Suspended)
				{
					WorkOnCurrentTaskButton.Enabled = true;
					CloseCurrentTaskButton.Enabled = true;
					SuspendCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.SuspendTask_Active;
				}
				else
				{
					WorkOnCurrentTaskButton.Enabled = true;
					CancelCurrentTaskButton.Enabled = true;
				}
			}
			else if (!BusinessEntity.IsCurrentResolutionCodeClosedResolvedOrAwaitingCustomer)
			{
				WorkOnCurrentTaskButton.Enabled = true;
			}
		}

		void UpdateStatus(string newStatus)
		{
			if (BusinessEntity != null)
			{
				var currentOrNextTask = BusinessEntity.CurrentOrNextTask;
				if (currentOrNextTask != null)
				{
					if ((newStatus == ProcessTaskStatusCodeList.Codes.Working || newStatus == ProcessTaskStatusCodeList.Codes.Closed) && currentOrNextTask.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						currentOrNextTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					}
					currentOrNextTask.P9_Status = newStatus;
				}
				BusinessEntity.RefreshBinding();
			}
		}

		#endregion

		#region Validate And Save

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				BusinessEntity.CustomerNotifier.SendQueuedEmails();
				SetupEConversationControlsAndCloseControls();
			}

			RecalculateServiceOutageStatusMenuItems();
			UpdateKnownNames();

			return result;
		}

		#endregion

		#region Form Notification Handler

		internal class SupportIncidentNotificationHandler : INotificationHandler
		{
			public SupportIncidentNotificationHandler(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				NotificationHandler.Instance.ReportError(message, caption);
			}

			void INotificationHandler.ReportInformation(string message, string caption)
			{
				if (factory != null)
				{
					var factoryChanges = factory.GetChanges();
					var criticalChange = factoryChanges.GetChangedObjects().FirstOrDefault(x =>
					{
						var isNoteWithCriticalChanges = x.IsExistsInDatabase && x.IsModifiedInDatabase && !x.CanMerge() && x.DatabaseInstance is StmNote;

						if (!isNoteWithCriticalChanges)
						{
							return false;
						}

						var hasNoNonMergeableProperties = !x.NonMergeableProperties.Any(y => y.HasChangedInDatabase);
						var mergeableProperties = x.MergeableProperties.Where(y => y.HasChangedInDatabase).ToArray();

						return hasNoNonMergeableProperties && mergeableProperties.Length == 1 && mergeableProperties[0].ColumnName == StmNoteSchema.Constants.ST_NoteText;
					});

					if (criticalChange != null)
					{
						var databaseInstance = (StmNote)criticalChange.DatabaseInstance;
						var errorMessage = FormattableString.Invariant($"Note PK: {databaseInstance.PK}\r\n Note Parent: {databaseInstance.ST_Table} {databaseInstance.ST_ParentID}\r\n Note Description in Database: {databaseInstance.ST_Description}\r\n Note Type in Database: {databaseInstance.ST_NoteType}\r\n Note Text in Database: {databaseInstance.ST_NoteText}");
						ErrorReporter.ReportOnce("Invalid note delete with critical changes", errorMessage, null);
					}
				}

				NotificationHandler.Instance.ReportInformation(message, caption);
			}
		}

		protected override INotificationHandler FormNotificationHandler => new SupportIncidentNotificationHandler(DataSource?.Factory);

		#endregion

		#region Cilent Selected / Updated

		void IM_OH_ClientInfo_ValueChanged(object sender, EventArgs e)
		{
			SetContractStatusColor();
			PopupLatestCreatedIncidents();
		}

		void ContractStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			SetContractStatusColor();
		}

		void SetContractStatusColor()
		{
			Color colour = Color.Red;
			if (BusinessEntity.IM_ClientContractStatus.Contains(LicenceHeaderLookups.SupportModeConstants.Descriptions.Standard))
			{
				colour = Color.Green;
			}
			else if (BusinessEntity.IM_ClientContractStatus.Contains(LicenceHeaderLookups.SupportModeConstants.Descriptions.Hour24))
			{
				colour = Color.Blue;
			}

			ContractStatusLabel.ForeColor = colour;
		}

		void PopupLatestCreatedIncidents()
		{
			if (!BusinessEntity.IsInDatabase)
			{
				EDIOrgHeader client = BusinessEntity.Client as EDIOrgHeader;
				if (client != null)
				{
					using (new ZWaitCursorChanger(this))
					{
						var eventTemplateOrgPk = EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.Value;
						if (eventTemplateOrgPk == ZGuid.Empty || (client.PK != eventTemplateOrgPk && lastClientPk != eventTemplateOrgPk))
						{
							ZString lastTenIncidentsAsText = BusinessEntity.LastTenIncidentsForSameEnterpriseAsText;
							if (!lastTenIncidentsAsText.IsEmpty)
							{
								ZString displayName;
								if (client.LicCompany != null && client.LicCompany.LicEnterprise != null && !client.LicCompany.LicEnterprise.OrganisationName.IsEmpty)
								{
									displayName = client.LicCompany.LicEnterprise.OrganisationName;
								}
								else
								{
									displayName = BusinessEntity.ClientName;
								}

								Globals.Message.ShowInformation("Last ten incidents created for " + displayName + " :\r\n\r\n" + lastTenIncidentsAsText);
							}
						}
					}
				}
				lastClientPk = client != null ? client.PK : ZGuid.Empty;
			}
		}
		ZGuid lastClientPk = ZGuid.Empty;

		#endregion

		#region Contact Accredtitaion Status

		void ContactAccreditationStatus_ValueChanged(object sender, EventArgs e)
		{
			isContactAccreditedLabel.ForeColor = BusinessEntity.ContactAccreditationStatus.Contains(Res.GetString("8a8bfecc-19cc-4af4-9ca9-404215b87482", "Not accredited for"))
				? Color.Red
				: Color.Green;
		}

		#endregion

		#region Actions

		#region Menu

		void SetupActionMenuItemsEvent()
		{
			ActionsMenuItem.MenuItems.AddRange(GenericMenuItems.ToArray());
			ActionsMenuItem.MenuItems.AddRange(SupportMenuItems.ToArray());
			ActionsMenuItem.MenuItems.AddRange(FeatureRequestMenuItems.ToArray());

			TopLevelTabControl.SelectedIndexChanged += new EventHandler(TopLevelTabControl_SelectedIndexChanged);
			SetupActionMenuItems();
		}

		void TopLevelTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupActionMenuItems();
		}

		void SetupActionMenuItems()
		{
			if (BusinessEntity == null)
			{
				return;
			}

			foreach (MenuItem menuItem in ActionsMenuItem.MenuItems)
			{
				if (GenericMenuItems.Contains(menuItem))
				{
					menuItem.Visible = true;
					if (SyncDetailsERequestMenuText.Equals(menuItem.Text) && NeedDisableSyncDetailMenuItem())
					{
						menuItem.Enabled = false;
					}
				}
				else if (SupportMenuItems.Contains(menuItem))
				{
					menuItem.Visible = BusinessEntity.IM_Category == SupportIncidentCategoriesList.Codes.Support;
				}
				else if (FeatureRequestMenuItems.Contains(menuItem))
				{
					menuItem.Visible = (BusinessEntity.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest) && IsFeatureRequestMenuItemVisible(menuItem.Text);
				}

				if (menuItem.Name == ZFormMenuStrategy.ResetFormSizeToDefaultName)
				{
					menuItem.Click -= (s, e) => { splitContainer5.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52); };
					menuItem.Click += (s, e) => { splitContainer5.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52); };
				}
			}
		}

		void RecalculateServiceOutageStatusMenuItems()
		{
			if (BusinessEntity == null)
			{
				return;
			}
			var lastServiceLog = BusinessEntity.GetLastServiceLog();

			switch (lastServiceLog)
			{
				case null:
					SetLabelsVisible(false);
					SetMenuItemsEnabled(true, false, false);
					ResetServiceStatusAndOutageDuration();
					break;

				case AutoEvents.ServiceSuspendedCode:
					SetLabelsVisible(true);
					SetMenuItemsEnabled(false, true, true);
					RecalculateServiceStatusAndOutageDuration(Color.Red);
					break;

				case AutoEvents.ServiceCommencedCode:
					SetLabelsVisible(true);
					SetMenuItemsEnabled(true, false, true);
					RecalculateServiceStatusAndOutageDuration(Color.Black);
					break;
			}
		}

		void SetLabelsVisible(bool visible)
		{
			ServiceStatusLabel.Visible = visible;
			ServiceStatusLabelText.Visible = visible;
			OutageDurationLabel.Visible = visible;
			OutageDurationLabelText.Visible = visible;
		}

		void SetMenuItemsEnabled(bool startEventEnabled, bool restoredEventEnabled, bool clearEventsEnabled)
		{
			addServiceOutageStartEventMenuItem.Enabled = startEventEnabled;
			addServiceRestoredEventMenuItem.Enabled = restoredEventEnabled;
			clearAllServiceEventsMenuItem.Enabled = clearEventsEnabled;
		}

		void ResetServiceStatusAndOutageDuration()
		{
			BusinessEntity.ServiceStatus = "";
			BusinessEntity.OutageDuration = "";
		}

		void RecalculateServiceStatusAndOutageDuration(Color color)
		{
			ServiceStatusLabel.ForeColor = color;
			OutageDurationLabel.ForeColor = color;
			BusinessEntity.RecalculateServiceStatus();
			BusinessEntity.RecalculateOutageDuration();
		}

		bool NeedDisableSyncDetailMenuItem()
		{
			var staff = Env.CurrentUser as GlbStaff;
			var isCreateUser = Env.CurrentUser.Initials.Equals(BusinessEntity.IM_SystemCreateUser);
			var isContract = staff?.GS_EmailAddress.Equals(BusinessEntity.Contact?.Email) ?? false;
			var createStaff = BusinessEntity.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, BusinessEntity.IM_SystemCreateUser));
			return (createStaff?.GS_IsSystemAccount ?? false) || (!isCreateUser && !isContract);
		}

		#region Support

		List<MenuItem> SupportMenuItems
		{
			get
			{
				if (fSupportMenuItems == null)
				{
					fSupportMenuItems = new List<MenuItem>();

					fSupportMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|4F88887A-A990-4f32-8F0B-35E0CC787A95", "-")));

					investigateMenuItem = new ZMenuItem("Working / &Investigating", Working_Click);
					investigateMenuItem.Shortcut = Shortcut.CtrlShiftI;
					investigateMenuItem.ShowShortcut = true;
					fSupportMenuItems.Add(investigateMenuItem);

					var menuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|78110831-8526-41f8-B29E-95A91C6389B9", "Re-send Email &Notification"), EmailNotification_Click);
					menuItem.Shortcut = Shortcut.CtrlShiftN;
					menuItem.ShowShortcut = true;
					fSupportMenuItems.Add(menuItem);
				}

				return fSupportMenuItems;
			}
		}

		List<MenuItem> fSupportMenuItems;

		#endregion

		#region Feature Request

		List<MenuItem> FeatureRequestMenuItems
		{
			get
			{
				if (featureRequestMenuItems == null)
				{
					featureRequestMenuItems = new List<MenuItem>();
					featureRequestMenuItems.Add(new ZMenuItem("-"));

					featureRequestMenuItems.Add(new ZMenuItem(SendSoftwareEstimateMenuText, SendSofwareEstimate_Click));
					featureRequestMenuItems.Add(new ZMenuItem(ReSendSoftwareEstimateMenuText, SendSofwareEstimate_Click));
					featureRequestMenuItems.Add(new ZMenuItem(ReIssueSoftwareEstimateRequestMenuText, ReissueSofwareEstimateRequest_Click));
					featureRequestMenuItems.Add(new ZMenuItem(SendSoftwareQuoteMenuText, SendSoftwareQuote_Click));
					featureRequestMenuItems.Add(new ZMenuItem(ReSendSoftwareQuoteMenuText, SendSoftwareQuote_Click));
					featureRequestMenuItems.Add(new ZMenuItem(ReIssueSoftwareQuoteRequestMenuText, ReissueSofwareQuoteRequest_Click));
				}

				return featureRequestMenuItems;
			}
		}
		List<MenuItem> featureRequestMenuItems;

		bool IsFeatureRequestMenuItemVisible(string menuItemText)
		{
			if (BusinessEntity.IM_Priority == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
			{
				ZString disposition = BusinessEntity.IM_ResolutionCode;
				bool supportsAutomaticCR7Workflow = BusinessEntity.ClientSystemSupportsBiDirectionUpdate && !BusinessEntity.IM_ClientIncidentReference.IsEmpty;

				switch (menuItemText)
				{
					case SendSoftwareEstimateMenuText:
						return disposition == SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate
								|| (!supportsAutomaticCR7Workflow && disposition != SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided);

					case ReSendSoftwareEstimateMenuText:
						return disposition == SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;

					case ReIssueSoftwareEstimateRequestMenuText:
						return disposition != SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate
								&& BusinessEntity.HasDispositionChangedToEventLog(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided);

					case SendSoftwareQuoteMenuText:
						return disposition == SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation
								|| (!supportsAutomaticCR7Workflow && disposition == SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided);

					case ReSendSoftwareQuoteMenuText:
						return disposition == SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;

					case ReIssueSoftwareQuoteRequestMenuText:
						return disposition != SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation
								&& BusinessEntity.HasDispositionChangedToEventLog(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided);

					case "-":
						return true;

					default:
						return false;
				}
			}
			else
			{
				return false;
			}
		}

		const string SendSoftwareEstimateMenuText = "Send Software Estimate";
		const string ReSendSoftwareEstimateMenuText = "Re-Send Software Estimate";
		const string ReIssueSoftwareEstimateRequestMenuText = "Re-issue Software Estimate Request";
		const string SendSoftwareQuoteMenuText = "Send Software Quote";
		const string ReSendSoftwareQuoteMenuText = "Re-Send Software Quote";
		const string ReIssueSoftwareQuoteRequestMenuText = "Re-issue Software Quote Request";
		const string SyncDetailsERequestMenuText = "Sync Details to eRequest";
		const string LaunchContentFinderMenuText = "Launch Content Finder";
		const string ServiceOutageStatusMenuText = "Service Outage Status";
		const string AddServiceOutageStartEventMenuText = "Add service outage start event";
		const string AddServiceRestoredEventMenuText = "Add service restored event";
		const string ClearAllServiceEventsMenuText = "Clear all service events";
		const string MuteAllOutboundEmailNotificationsMenuText = "Mute all outbound email notifications";
		#endregion

		#region Generic

		List<MenuItem> GenericMenuItems
		{
			get
			{
				if (genericMenuItems == null)
				{
					genericMenuItems = new List<MenuItem>();

					genericMenuItems.Add(new ZMenuItem("-"));

					assignedToMenuItem = new ZMenuItem("&Assign To", AssignToStaff_Click);
					assignedToMenuItem.Shortcut = Shortcut.CtrlShiftA;
					assignedToMenuItem.ShowShortcut = true;
					genericMenuItems.Add(assignedToMenuItem);

					var menuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|58F5B922-98E6-415D-892E-F395DA080610", "Escala&te"), Escalate_Click);
					menuItem.Shortcut = Shortcut.CtrlShiftT;
					menuItem.ShowShortcut = true;
					genericMenuItems.Add(menuItem);

					closeMenuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|1EC8538B-220F-4cf3-B59E-B60F6AF9D748", "&Close"), Close_Click);
					genericMenuItems.Add(closeMenuItem);

					if (BusinessEntity.CanRevertToAwaitingResponse || BusinessEntity.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
					{
						revertToAwaitingResponseItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentFrm|0FA60F7D-0BE6-4006-8D85-1EFC81CD60A9", "Revert to Awaiting Response"), RevertToAwaitingResponse_Click);
						genericMenuItems.Add(revertToAwaitingResponseItem);
					}
					//else -> Hidden for Clarity trial period, to be reverted at a later date
					//{
					//revertToClosedResolvedMenuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentFrm|0D32C491-8F82-443A-939A-FCD898748702", "Revert to Closed/Resolved"), RevertToClosedResolved_Click);
					//revertToClosedResolvedMenuItem.Enabled = true;
					//genericMenuItems.Add(revertToClosedResolvedMenuItem);
					//}*/

					closeOnBehalfOfClientItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentFrm|7B1B08A4-464F-4933-98A0-174D59767447", "Close on behalf of Client (Confirmed Resolved)"), CloseOnBehalfOfClient_Click);
					genericMenuItems.Add(closeOnBehalfOfClientItem);

					reOpenMenuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|70228FB7-81AD-441E-9B84-163A2148DBB2", "&Re-Open"), ReOpen_Click, Shortcut.CtrlShiftO);
					genericMenuItems.Add(reOpenMenuItem);

					setToUPOMenuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|BE4E8A9C-F3C2-4452-BA27-1B802605C238", "&Set eRequest Status to Awaiting Auto Upgrade Deployment"), SetToUPOMenuItem_Click);
					genericMenuItems.Add(setToUPOMenuItem);

					setToUDOMenuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|84B56D87-7C85-46BD-BFD5-39542919BA2F", "&Set eRequest Status to Upgrade Delayed"), SetToUDOMenuItem_Click);
					genericMenuItems.Add(setToUDOMenuItem);

					genericMenuItems.Add(new ZMenuItem("-"));

					var addToLogMenuItem = new ZMenuItem("Add Internal &Log", AddLog_Click, Shortcut.CtrlShiftL);
					genericMenuItems.Add(addToLogMenuItem);

					var serviceOutageStatusMenuItem = new ZMenuItem(ServiceOutageStatusMenuText);

					addServiceOutageStartEventMenuItem = new ZMenuItem(AddServiceOutageStartEventMenuText, AddServiceOutageStartEvent_Click);
					addServiceRestoredEventMenuItem = new ZMenuItem(AddServiceRestoredEventMenuText, AddServiceRestoredEvent_Click);
					clearAllServiceEventsMenuItem = new ZMenuItem(ClearAllServiceEventsMenuText, ClearAllServiceEvents_Click);

					serviceOutageStatusMenuItem.MenuItems.Add(addServiceOutageStartEventMenuItem);
					serviceOutageStatusMenuItem.MenuItems.Add(addServiceRestoredEventMenuItem);
					serviceOutageStatusMenuItem.MenuItems.Add(clearAllServiceEventsMenuItem);

					genericMenuItems.Add(serviceOutageStatusMenuItem);

					var syncDetailMenuItem = new ZMenuItem(SyncDetailsERequestMenuText, SyncDetails_Click);
					genericMenuItems.Add(syncDetailMenuItem);

					var launchContentFinderMenuItem = new ZMenuItem(LaunchContentFinderMenuText, LaunchContentFinder_Click);
					launchContentFinderMenuItem.Enabled = EDIDataRegistry.Instance.EnableContentFinder.Value;
					genericMenuItems.Add(launchContentFinderMenuItem);

					var muteAllOutboundEmailNotificationsMenuItem = new ZMenuItem(MuteAllOutboundEmailNotificationsMenuText, MuteAllOutboundEmailNotifications_Click);
					muteAllOutboundEmailNotificationsMenuItem.Checked = SupportIncidentEmailTriggeringRules.IsAllSuppressed(BusinessEntity);
					muteAllOutboundEmailNotificationsMenuItem.Enabled = EDISecurityCheckpoints.CustomerServiceMuteOutboundEmailNotifications.IsAllowed;

					genericMenuItems.Add(muteAllOutboundEmailNotificationsMenuItem);

					if (LicenceDatabase.CanSupportBiDirectionIncidentMessage(BusinessEntity.ClientReportedOnVersion) != Enterprise.Customs.Business.TriState.True
						&& !BusinessEntity.IsWebRequest)
					{
						menuItem = new ZMenuItem(ResString.GetMultilingualString("SupportIncidentForm|18D09A0B-F024-4b38-BAF0-17526C433AF3", "Email &Update"), EmailCorrespondence_Click, Shortcut.CtrlShiftU);
						genericMenuItems.Add(menuItem);
					}
				}
				return genericMenuItems;
			}
		}

		protected List<MenuItem> genericMenuItems;
		MenuItem assignedToMenuItem;
		MenuItem reOpenMenuItem;
		//MenuItem revertToClosedResolvedMenuItem = null; -> Hidden for Clarity trial period, to be reverted at a later date
		MenuItem revertToAwaitingResponseItem;
		MenuItem closeOnBehalfOfClientItem;
		protected MenuItem closeMenuItem;
		MenuItem investigateMenuItem;
		protected MenuItem addServiceOutageStartEventMenuItem;
		protected MenuItem addServiceRestoredEventMenuItem;
		protected MenuItem clearAllServiceEventsMenuItem;
		MenuItem setToUPOMenuItem;
		MenuItem setToUDOMenuItem;

		#endregion

		#endregion

		#region Logic

		#region Set eRequest Status to UPO & UDO

		bool CanSetToUPOorUDO()
		{
			if (BusinessEntity == null)
			{
				return false;
			}

			if (BusinessEntity.RelatedWorkItems.Count == 0)
			{
				Globals.Message.Show(Res.GetString("189ef916-9884-4164-9b07-92242f468b98", "Operation Canceled: No work item is attached to this incident."));
				return false;
			}
			else if (BusinessEntity.RelatedWorkItems.Any(x => !(x as WorkItem).IsClosedOrCancelled))
			{
				Globals.Message.Show(Res.GetString("16ae80a6-8c02-47af-89a9-55d331842347", "Operation Canceled: The incident has unclosed work item(s)."));
				return false;
			}

			return true;
		}

		void SetToUPOMenuItem_Click(object sender, EventArgs e)
		{
			if (!CanSetToUPOorUDO())
			{
				return;
			}

			BusinessEntity.WaitForUpgrade();
			BusinessEntity.AddSystemMessageToCustomer(BusinessEntity.IM_ResolutionCodeDescription);
		}

		void SetToUDOMenuItem_Click(object sender, EventArgs e)
		{
			if (!CanSetToUPOorUDO())
			{
				return;
			}

			BusinessEntity.DelayUpgrade();
			BusinessEntity.AddSystemMessageToCustomer(BusinessEntity.IM_ResolutionCodeDescription);
		}

		void UpdateUPOAndUDOMenuItemStatus()
		{
			if (BusinessEntity == null)
			{
				return;
			}

			var showUPO = false;
			var showUDO = false;
			if (BusinessEntity.RelatedWorkItems.Count > 0)
			{
				switch (BusinessEntity.IM_Category)
				{
					case SupportIncidentCategoriesList.Codes.Support:
						break;
					case SupportIncidentCategoriesList.Codes.Defect:
						showUDO = true;
						showUPO = true;
						break;
					default:
						showUPO = true;
						break;
				}
			}

			if (setToUPOMenuItem != null)
			{
				setToUPOMenuItem.Enabled = showUPO;
			}

			if (setToUDOMenuItem != null)
			{
				setToUDOMenuItem.Enabled = showUDO;
			}
		}

		#endregion

		#region Action Validation

		enum ActionTypes
		{
			Support,
			DefectManagement,
			FeatureRequestManagement
		}

		bool CheckValidMenuOption(ActionTypes type)
		{
			bool result = CheckSecurityForTab(type);
			if (result)
			{
				if (type == ActionTypes.Support &&
					(BusinessEntity.IM_Category != SupportIncidentCategoriesList.Codes.Support || (BusinessEntity.IM_Category == SupportIncidentCategoriesList.Codes.Support && BusinessEntity.IM_Status == SupportIncidentLookups.Status.Closed)))
				{
					Globals.Message.Show(Res.GetString("09217d5a-f50d-4f7b-91ac-d70bdb3c8de6", "This incident is escalated to other stages. If you want to change details, please escalated the incident back to support first."));
					result = false;
				}
			}

			return result;
		}

		bool CheckSecurityForTab(ActionTypes type)
		{
			bool result = true;

			if (type == ActionTypes.DefectManagement)
			{
				if (!EDISecurityCheckpoints.CustomerServiceIncidentEditDefectManagement.IsAllowed)
				{
					EDISecurityCheckpoints.CustomerServiceIncidentEditDefectManagement.ShowError();
					result = false;
				}
			}
			else if (type == ActionTypes.FeatureRequestManagement)
			{
				if (!EDISecurityCheckpoints.CustomerServiceIncidentEditFeatureManagement.IsAllowed)
				{
					EDISecurityCheckpoints.CustomerServiceIncidentEditFeatureManagement.ShowError();
					result = false;
				}
			}

			return result;
		}

		#endregion

		#region Assign

		void AssignToStaff_Click(object sender, EventArgs e)
		{
			ShowPopupForm(new AssignIncidentPopupForm(new SupportIncidentAssignStaffAction(BusinessEntity)));
		}

		#endregion

		#region Escalate

		void Escalate_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.HasErrors)
			{
				var caption = Res.GetString("cde586be-1de1-4692-9565-ce1ad6b5179c", "Escalate failed");
				var message = Res.GetString("edfb2537-16ab-4916-ac2e-8d55dc03da27", "There are errors that need to be corrected before this Incident can be Escalated.");
				ZFormModaliser.ShowDialogAndDispose(GetEscalateErrorMessageBox(message, caption));
				return;
			}

			ShowPopupForm(new EscalateIncidentPopupForm(new SupportIncidentEscalateAction(BusinessEntity, BusinessEntity.IM_Priority)));
		}

		protected virtual ZErrorMessageBox GetEscalateErrorMessageBox(string message, string caption)
		{
			return new ZErrorMessageBox(BusinessEntity, message, caption);
		}

		#endregion

		#region Working

		void Working_Click(object sender, EventArgs e)
		{
			if (CheckValidMenuOption(ActionTypes.Support))
			{
				BusinessEntity.InvestigateInSupport();
			}
		}

		#endregion

		#region Close

		void Close_Click(object sender, EventArgs e)
		{
			CloseIncident(true);
		}

		bool CloseIncident(bool isActionClose = false)
		{
			BusinessEntity.CloseIncidentViaPopupForm = true;
			var shouldOnlyUpdateStatus = BusinessEntity.IsInDatabase && (BusinessEntity.IsClosedDisposition((ZString)BusinessEntity.IM_ResolutionCodeInfo.OriginalValue)
				|| ((SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided.Equals(BusinessEntity.IM_ResolutionCode)
				|| SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided.Equals(BusinessEntity.IM_ResolutionCode)) && !isActionClose));

			if (shouldOnlyUpdateStatus)
			{
				BusinessEntity.IM_Status = IncidentMainLookups.Status.Closed;
				return true;
			}
			else if (BusinessEntity.IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR5_Training && BusinessEntity.IM_Category == SupportIncidentCategoriesList.Codes.Support && BusinessEntity.IsResolutionWizardEnabled)
			{
				return ShowResolutionWizardIncidentPopup() == DialogResult.OK;
			}
			else
			{
				return ShowCloseIncidentPopup() == DialogResult.OK;
			}
		}

		SupportIncidentResolutionWizardAction RestoringWizardForm()
		{
			if (!string.IsNullOrEmpty(BusinessEntity.ResolutionWizardOptionText))
			{
				var serializer = ZXmlSerializer.New(typeof(SupportIncidentResolutionWizardAction));
				var reader = new StringReader(BusinessEntity.ResolutionWizardOptionText);
				var resolutionWizardActionObj = (SupportIncidentResolutionWizardAction)serializer.Deserialize(reader);
				resolutionWizardActionObj.SetIncident(BusinessEntity);
				return resolutionWizardActionObj;
			}
			else
			{
				return new SupportIncidentResolutionWizardAction(BusinessEntity);
			}
		}

		DialogResult ShowCloseIncidentPopup(bool closeOnBehalfOfClient = false)
		{
			var action = new SupportIncidentCloseAction(BusinessEntity);
			if (closeOnBehalfOfClient)
			{
				BusinessEntity.PopulateCloseAction(action);
				BusinessEntity.CloseViaCloseOnBehalfOfClient = true;
			}
			ZString unsentEConversationMessage = ZString.Empty;
			if (ConversationMessageTextBox.Enabled)
			{
				unsentEConversationMessage = ConversationMessageTextBox.Text;
				ConversationMessageTextBox.Text = string.Empty;
			}

			action.Comment = unsentEConversationMessage;
			var result = ShowPopupForm(new CloseIncidentPopupForm(action));

			if (ConversationMessageTextBox.Enabled && result != DialogResult.OK)
			{
				ConversationMessageTextBox.Text = unsentEConversationMessage;
			}

			return result;
		}

		DialogResult ShowResolutionWizardIncidentPopup()
		{
			var restoringAction = RestoringWizardForm();
			var unsentEConversationMessage = ZString.Empty;
			if (ConversationMessageTextBox.Enabled)
			{
				unsentEConversationMessage = ConversationMessageTextBox.Text;
				ConversationMessageTextBox.Text = string.Empty;
			}

			restoringAction.Comment = unsentEConversationMessage;

			var form = new ResolutionWizardForm(restoringAction);
			var result = ShowPopupForm(form);

			if (ConversationMessageTextBox.Enabled && result != DialogResult.OK)
			{
				ConversationMessageTextBox.Text = unsentEConversationMessage;
			}

			return result;
		}

		#endregion

		#region Resend Notification

		void EmailNotification_Click(object sender, EventArgs e)
		{
			if (CheckValidMenuOption(ActionTypes.Support))
			{
				SendEmail(BusinessEntity.GetEmailObjectForNotification());
			}
		}

		#endregion

		#region General - ReOpen / Revert To Closed / Revert To Awaiting Response / CloseOnBehalfOfClient

		void ReOpen_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.CanReOpen)
			{
				var form = GetReopenIncidentPopup();
				ZFormModaliser.ShowDialogAndDispose(form, this);

				if (form.DialogResult != ReopenIncidentAction.Cancel)
				{
					BusinessEntity.RunPreSaveValidation();
					if (BusinessEntity.HasErrors)
					{
						ShowErrorsDialog();
						return;
					}

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var incidentReloaded = newFactory.Load<SupportIncident>(BusinessEntity.PK);
					if (!BusinessEntity.HasChanges)
					{
						BusinessEntity.Reload();
					}
					else if (BusinessEntity.IM_SystemLastEditTimeUtc != incidentReloaded.IM_SystemLastEditTimeUtc)
					{
						Globals.Message.Show(Res.GetString("ee0c96a5-119d-4b39-a80d-349b936981aa", "While you have been working with this form, another user has made changes which cannot be merged. Please save and resolve any conflicts or reload this form before trying to reopen the incident again."));
						return;
					}

					BusinessEntity.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
					this.AwaitingResponseButton.Enabled = true;

					var newTask = BusinessEntity.CurrentOrNextTask;
					if (newTask != null && form.DialogResult == ReopenIncidentAction.AssignToSelf)
					{
						newTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					}

					ValidateAndSave();
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("dfc2175c-ffdb-4498-ab38-79db3ed00150", "The current status of the incident can not Re-Open."));
			}
		}

		protected virtual ReopenIncidentPopup GetReopenIncidentPopup()
		{
			return new ReopenIncidentPopup();
		}

		void RevertToClosedResolved_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.IsCurrentResolutionCodeClosedOrResolved || EDIDataRegistry.Instance.IncidentClosureDispositions.Value.ContainsCode(BusinessEntity.IM_ResolutionCode))
			{
				Globals.Message.ShowInformation(Res.GetString("8069af78-6cf5-44f9-a4b6-9b60e4fbf215", "The operation has been canceled because the eRequest Status is already Closed/Resolved."));
				return;
			}

			switch (BusinessEntity.FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(BusinessEntity.IM_ResolutionCodeInfo, out var previousCode))
			{
				case LoadingPreviousFieldValueResult.Success:
					var isPreviousClosedOrResolved = BusinessEntity.IsResolutionCodeClosedOrResolved(previousCode);
					if (!isPreviousClosedOrResolved)
					{
						if (string.IsNullOrEmpty(previousCode) || !EDIDataRegistry.Instance.IncidentClosureDispositions.Value.ContainsCode(previousCode))
						{
							Globals.Message.ShowInformation(Res.GetString("15eaa1f0-6971-43d2-a32e-8e4771087afc", "The operation has been canceled because the previous eRequest Status is not Closed/Resolved."));
							return;
						}
					}

					var resolution = isPreviousClosedOrResolved ? BusinessEntity.GetPreviousClosureResolution() : previousCode;
					var clsResolutionList = BusinessEntity.Lookups.GetClosureDispositionList(activeOnly: false, BusinessEntity.IM_Category, BusinessEntity.IM_Priority, BusinessEntity.IM_Product);
					if (clsResolutionList.ContainsCode(resolution))
					{
						var caption = Res.GetString("91ba0f7c-8ece-42f3-8fe2-15e45da3b2ec", "Revert eRequest Status");
						var message = Res.GetString("3490900a-c512-46c6-b2ed-3748e6612654", "Do you wish to revert the eRequest Status to {0} - {1}.", resolution, clsResolutionList.GetDescriptionFromCode(resolution));
						if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
						{
							BusinessEntity.OnRevertingToClosedOrResolved(resolution, isPreviousClosedOrResolved, previousCode);
						}
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("f4dc7063-de88-4603-9c2f-8f82ae782836", "The operation has been canceled because the previous eRequest status cannot be applied to the current details."));
					}

					break;
				case LoadingPreviousFieldValueResult.ParentCouldNotBeLoadedFromDatabase:
					Globals.Message.ShowInformation(Res.GetString("8d75ad36-7e8a-4bd2-b4c0-bcdd8e5a5c20", "Please save the form before reverting."));
					break;
				case LoadingPreviousFieldValueResult.ParentHasBeenDeleted:
				case LoadingPreviousFieldValueResult.ParentDataOutDated:
					Globals.Message.ShowInformation(Res.GetString("b5fb9e3e-e6af-4cf6-8a64-1dd58c41e803", "Changes by another user have been saved during your session. Please refresh the form."));
					break;
				case LoadingPreviousFieldValueResult.NoLog:
					break;
				default:
					var messageCaption = Res.GetString("BCA934C6-063E-40F3-A1CD-9DB23E90D23C", "Canceled");
					Globals.Message.ShowWarning(Res.GetString("5A8DCEC7-F047-4613-BB79-D024EE41DBF2", "Unable to revert as this Incident appears to be in an inconsistent state. An error report has been sent for review."), messageCaption);
					break;
			}
		}

		void RevertToAwaitingResponse_Click(object sender, EventArgs e)
		{
			var result = BusinessEntity.RevertIncidentToAwaitingResponse();
			var messageCaption = Res.GetString("D64DDAE8-95CB-4878-A18D-43628357A330", "Canceled");
			switch (result)
			{
				case SupportIncident.RevertResult.CurrentStatusIsClosed:
					Globals.Message.ShowWarning(Res.GetString("5EE5B651-CB03-4F5C-A4E7-052415BA3D57", "The operation has been canceled as the incident has been closed."), messageCaption);
					break;
				case SupportIncident.RevertResult.OldStatusNotMatch:
					Globals.Message.ShowWarning(Res.GetString("48358A35-BA41-436F-9F6D-A1F492F62E1E", "The operation has been canceled because the previous status does not match the current incident details."), messageCaption);
					break;
				default:
					break;
			}
		}

		void CloseOnBehalfOfClient_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.CanCloseOnBehalfOfClient)
			{
				ShowCloseIncidentPopup(true);
			}
			else
			{
				Globals.Message.Show(Res.GetString("2F374916-8C4C-41AF-82C5-9CE84D63442F", "The current status of the incident can not Close On Behalf Of Client."));
			}
		}

		#endregion

		#region General - Add Incident Log

		void AddLog_Click(object sender, EventArgs e)
		{
			ShowChildForm(new AddIncidentLogPopupForm(new SupportIncidentLogCommentAction(BusinessEntity), this));
		}

		public int LogPopupFormCount
		{
			get { return logPopupFormCount; }
			set
			{
				if (logPopupFormCount != value)
				{
					logPopupFormCount = value;
					OnLogPopupFormCountChanged();
				}
			}
		}

		int logPopupFormCount;

		#endregion

		#region Service Outage Status

		void AddServiceOutageStartEvent_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(CreateEventLogForm(AutoEvents.ServiceSuspendedCode, "DES", "Service outage has started"), ParentForm);
		}

		void AddServiceRestoredEvent_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(CreateEventLogForm(AutoEvents.ServiceCommencedCode, "DES", "Service has been restored"), ParentForm);
		}

		void ClearAllServiceEvents_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("20029c96-20ba-4c5b-ac53-1d4423134e18", "Confirm - Clear Service Events");
			var message = Res.GetString("da30bec5-66ef-4e8f-9e48-a875644ea37c", "This action will cancel all service events previously logged on this incident. Do you wish to continue?");
			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);

			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			SetLabelsVisible(false);
			BusinessEntity.CancelAllServiceLogs();
		}

		protected ZStmALogAddForm CreateEventLogForm(string eventCode, string key, string value)
		{
			var view = new StmALogCollectionView(BusinessEntity);
			var form = new ZStmALogAddForm(view, BusinessEntity.HasChanges, true);
			var newLog = (BaseStmALog)form.BusinessEntity;
			newLog.Master = BusinessEntity;
			newLog.SL_SE_NKEvent = eventCode;
			newLog.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength("", new[] { new KeyValuePair<string, string>(key, value) });
			return form;
		}

		#endregion

		#region sync details to eRequest

		void SyncDetails_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("0fd2874b-e0e4-4e03-8938-8429c76ee6b5", "Sync Details to eRequest");
			var message = Res.GetString("44b72d11-de72-4e31-97de-69a55ab1d177", "Incident Summary and Details will be copied to eRequest. This operation cannot be undone. Continue?");
			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
			if (dialogResult == DialogResult.Cancel)
			{
				return;
			}
			BusinessEntity.SyncDetailsToeRequest();
		}

		#endregion

		#region Launch Content Finder

		void LaunchContentFinder_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("58858BA4-112E-4EA7-B3B3-D395E6D659E7", "Launch Content Finder");
			var message = Res.GetString("FDABE297-6636-4BF9-9AB6-2EFF84108FB8", "A browser to the Incident Support Tool / Content Suggester webpage will be open. Continue?");

			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
			if (dialogResult == DialogResult.OK)
			{
				var url = EDIDataRegistry.Instance.ContentFinderUrl.Value;
				if (!string.IsNullOrEmpty(url))
				{
					try
					{
						var jwtToken = BusinessEntity.CreateContentFinderJwtToken();
						WebUrlLauncher.Launch($"{url}?jwt={jwtToken}");
					}
					catch (Win32Exception ex)
					{
						ErrorReporter.ReportOnce("SupportIncidentForm|LaunchContentFinder_Click", "Failed to open web address for: " + url, ex);
						Globals.Message.ShowInformation(Res.GetString("96D8DF57-C041-429C-AD2E-D3F0A29DDF8D", "The web address for '{0}' could not be opened. Please try copy and pasting it into your web browser instead.", url));
					}
				}
			}
		}

		#endregion

		#region General - Mute All Outbound Email Notifications

		void MuteAllOutboundEmailNotifications_Click(object sender, EventArgs e)
		{
			var currentValue = (sender as MenuItem).Checked;
			if (currentValue)
			{
				BusinessEntity.UnmuteEmailNotification();
			}
			else
			{
				if (BusinessEntity.MuteEmailNotification())
				{
					var caption = "Mute mode enabled";
					var message = Res.GetString("SupportIncidentForm|041b462c-2efb-44ea-9a7e-79085baf95da", "This eRequest has mute mode enabled due to potential email loop detection. All outbound notifications will be blocked.");
					Globals.Message.ShowInformation(message, caption);
				}
				else
				{
					Globals.Message.Show(Res.GetString("SupportIncidentForm|8c7f06ef-5c57-400f-9903-f0fb1c523448", "Failed to update the email notification suppression status. Please check if the BLN tag group has ALL code enabled."));
					return;
				}
			}

			(sender as MenuItem).Checked = !currentValue;
		}

		#endregion
		#region General - Email Updates

		void EmailCorrespondence_Click(object sender, EventArgs e)
		{
			SendEmail(BusinessEntity.GetEmailObjectForCorrespondence());
		}

		void SendEmail(SupportIncidentEmail email)
		{
			if (email != null)
			{
				email.UserCanEdit = true;
				email.BusinessObjectSendingEmail.CustomerNotifier.SendEmailNow(email);
			}
		}

		void SupportIncident_EmailPreconditionFailure(object sender, SupportIncident.EmailPreconditionFailureArgs e)
		{
			Globals.Message.ShowWarning(e.FailureReason);
		}

		#endregion

		#region Send Sofware Estimate

		void SendSofwareEstimate_Click(object sender, EventArgs e)
		{
			var action = new SupportIncidentCloseAction(BusinessEntity);
			action.SendDevelopmentEstimate = true;
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;
			action.Comment = "Please find estimate attached.";
			PromptForFileAndSend(action, "Send Software Estimate");
		}

		void ReissueSofwareEstimateRequest_Click(object sender, EventArgs e)
		{
			BusinessEntity.IM_Status = SupportIncidentLookups.Status.Working;
			BusinessEntity.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;
			IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.DevelopmentEstimateRequested, BusinessEntity);
		}

		#endregion

		#region Send Sofware Quote

		void SendSoftwareQuote_Click(object sender, EventArgs e)
		{
			var action = new SupportIncidentCloseAction(BusinessEntity);
			action.SendSoftwareQuote = true;
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;
			action.Comment = "Please find quote attached.";
			PromptForFileAndSend(action, "Send Software Quote");
		}

		void ReissueSofwareQuoteRequest_Click(object sender, EventArgs e)
		{
			BusinessEntity.IM_Status = SupportIncidentLookups.Status.Working;
			BusinessEntity.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation;
			IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FormalQuotationRequested, BusinessEntity);
		}

		#endregion

		void PromptForFileAndSend(SupportIncidentCloseAction action, string caption)
		{
			ForceActivateEDocsPlugin();
			ZFormModaliser.ShowDialogAndDispose(new AttachDocumentForm(action, caption));
		}

		void ForceActivateEDocsPlugin()
		{
			// If edocs plugin is not activated
			// then it won't add it's factory during saving
			// and edocs won't get saved.
			// See ZForm.SaveInternal ... PlugIns.FactoriesToBeSaved
			foreach (var plugin in PlugIns.Instances)
			{
				if (plugin is eDocsPlugIn)
				{
					var edocs = ((eDocsPlugIn)plugin);
					edocs.SelectTabPage();
					break;
				}
			}
		}

		#endregion

		DialogResult ShowPopupForm(BaseIncidentPopupForm form)
		{
			return ZFormModaliser.ShowDialogAndDispose(form, this);
		}

		protected void ShowChildForm(BaseIncidentPopupForm form)
		{
			form.Show();
		}

		#endregion

		#region Form Setup

		void SetControlModuleIDs()
		{
			RelatedFeatureRequestsGrid.ModuleID = ClientModuleRegistration.SupportIncident;
			ParentFeatureRequestFindBox.ModuleID = ClientModuleRegistration.SupportIncident;
		}

		public override string FormCaption
		{
			get
			{
				var caption = new StringBuilder();
				caption.Append("Customer Service Incident");
				if (BusinessEntity != null)
				{
					if (BusinessEntity.IM_IncidentNumber != string.Empty)
					{
						caption.Clear();
						caption.Append(BusinessEntity.IM_IncidentNumber);
						if (BusinessEntity.IM_Description != string.Empty)
						{
							caption.Append(" - ");
							caption.Append(BusinessEntity.IM_Description);
						}
					}

					if (BusinessEntity.BranchAddress != null)
					{
						caption.Append(" - ");
						caption.Append(BusinessEntity.BranchAddress.Header.OH_FullName);
					}
				}

				return caption.ToString();
			}
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		void MainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == RelatedItemsTabPage && RelatedItemsTabPage.Controls.Count == 0)
			{
				InitializeRelatedItemsTabPage(RelatedItemsTabPage);
			}
		}

		protected virtual void InitializeRelatedItemsTabPage(ZTabPage relatedItemsTabPage)
		{
			var control = new EDIWorkTaskRelatedItemUserControl(IsViewOrDeleteMode);
			control.Dock = DockStyle.Fill;
			relatedItemsTabPage.Controls.Add(control);
		}

		#endregion

		#region IConversationView

		JobConversation IConversationView.Conversation => DataSource.EConversation.ExistingConversation;
		TextBoxBase IConversationView.MessageTextBox => ConversationMessageTextBox;
		ZButton IConversationView.SendButton => SendMessageButton;
		ZButton IConversationView.AddInternalCommentButton => null;
		ZButton IConversationView.BroadcastButton => null;

		#endregion

		#region IDataBoundControl Members

		new SupportIncident DataSource
		{
			get { return (SupportIncident)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.IM_OH_ClientInfo.ValueChanged -= new EventHandler(IM_OH_ClientInfo_ValueChanged);
				DataSource.IM_LDInfo.ValueChanged -= new EventHandler(ContractStatusInfo_ValueChanged);
				DataSource.ContactAccreditationStatusInfo.ValueChanged -= new EventHandler(ContactAccreditationStatus_ValueChanged);
				DataSource.EmailPreconditionFailure -= new EventHandler<SupportIncident.EmailPreconditionFailureArgs>(SupportIncident_EmailPreconditionFailure);
				DataSource.IM_PriorityInfo.ValueChanged -= new EventHandler(IM_PriorityInfo_ValueChanged);
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				BusinessEntity.IM_OH_ClientInfo.ValueChanged += new EventHandler(IM_OH_ClientInfo_ValueChanged);
				BusinessEntity.IM_LDInfo.ValueChanged += new EventHandler(ContractStatusInfo_ValueChanged);
				if (ContractStatusLabel != null)
				{
					IM_OH_ClientInfo_ValueChanged(this, EventArgs.Empty);
				}

				BusinessEntity.ContactAccreditationStatusInfo.ValueChanged += new EventHandler(ContactAccreditationStatus_ValueChanged);
				if (isContactAccreditedLabel != null)
				{
					ContactAccreditationStatus_ValueChanged(this, EventArgs.Empty);
				}

				DataSource.EmailPreconditionFailure += new EventHandler<SupportIncident.EmailPreconditionFailureArgs>(SupportIncident_EmailPreconditionFailure);
			}
		}

		#endregion

		void CacheControls()
		{
			var bottomPanel = Controls.Find("BottomPanel", false).FirstOrDefault();
			var prevNextControl = bottomPanel?.Controls.OfType<ZPreviousNextControl>().FirstOrDefault();
			if (prevNextControl != null)
			{
				nextButton = (ZButton)prevNextControl?.Controls.Find("NextButton", false).FirstOrDefault();
				previousButton = (ZButton)prevNextControl?.Controls.Find("PreviousButton", false).FirstOrDefault();
				numberOfResultsCalcEdit = (ZCalcEdit)prevNextControl?.Controls.Find("NumberOfResults", false).FirstOrDefault();
				currentResultCalcEdit = (ZCalcEdit)prevNextControl?.Controls.Find("CurrentRecordNumberCalcEdit", false).FirstOrDefault();
			}
		}

		void OnLogPopupFormCountChanged()
		{
			if (new object[] { nextButton, previousButton, numberOfResultsCalcEdit, currentResultCalcEdit }.Any(item => item == null))
			{
				CacheControls();
			}

			var shouldEnabled = LogPopupFormCount == 0;
			var results = numberOfResultsCalcEdit?.Text ?? string.Empty;
			var currentResult = currentResultCalcEdit?.Text ?? string.Empty;

			if (currentResultCalcEdit != null)
			{
				currentResultCalcEdit.Enabled = shouldEnabled;
			}

			if (previousButton != null)
			{
				previousButton.Enabled = shouldEnabled && currentResult != "1";
			}

			if (nextButton != null)
			{
				nextButton.Enabled = shouldEnabled && currentResult != results;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (LogPopupFormCount > 0)
			{
				Globals.Message.ShowWarning("Incident cannot be closed while child dialogs are open.");
				e.Cancel = true;
				return;
			}

			base.OnClosing(e);
		}

		#region ChangeClientLabelColor

		ZOrgAddressControl LocalClientControl;

		void Client_ValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				if (supportClientGuidFindBox != null)
				{
					var foreColor = BusinessEntity.IM_ClientHasInvoicingPreferencesNote ? Color.Red : Color.Black;
					supportClientGuidFindBox.Extensions.Get<ZLabelCaptionRenderer>().ForeColor = foreColor;
				}

				if (LocalClientControl != null)
				{
					var foreColor = BusinessEntity.IM_InvoicingLocalClientHasInvoicingPreferencesNote ? Color.Red : Color.Black;
					LocalClientControl.ForeColor = foreColor;
				}
			}
		}

		#endregion

		#region Button Event Handlers

		void CloseIncidentButton_Click(object sender, EventArgs e)
		{
			CloseIncident();
		}

		void ShowAccreditationButton_Click(object sender, EventArgs e)
		{
			EDIOrgContact contact = (EDIOrgContact)BusinessEntity.Contact;
			if (contact != null && contact.RelatedCertificateApplicant != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new HRJobApplicantAccreditationInfoForm(contact.RelatedCertificateApplicant));
			}
			else if (contact != null)
			{
				Globals.Message.ShowInformation(Res.GetString("da419393-2dc8-4d8c-8b43-ecf89ce2048d", "No accreditation info for {0}.", contact.OC_ContactName));
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("b352d43e-8ca8-4df0-9d9b-7bbe1ba7a298", "Incident contact is not specified."));
			}
		}

		void WorkOnCurrentTaskButton_Click(object sender, EventArgs e)
		{
			var incident = BusinessEntity;
			if (incident != null && incident.CurrentOrNextTask == null)
			{
				ProcessTask newTask;
				if (incident.IM_Status == SupportIncidentLookups.Status.Closed)
				{
					incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen, true);
					newTask = incident.CurrentOrNextTask;
				}
				else
				{
					newTask = new SupportIncidentClientReopenEvent(BusinessEntity).CloneLastClosedOrCancelledTask();
					if (newTask != null)
					{
						//We truncate to fit. We use maxlength - 1 because that's what's done in ProcessTask.SetDescriptionFromType
						newTask.P9_Description = newTask.TypeDescription.Substring(0, newTask.P9_DescriptionInfo.MaxLength - 1);
					}
				}

				if (newTask != null)
				{
					newTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				}
				BusinessEntity.RefreshBinding();
			}
			else
			{
				UpdateStatus(ProcessTaskStatusCodeList.Codes.Working);
			}

			ConversationMessageTextBox.Focus();
		}

		void WorkOnCurrentTaskButton_MouseEnter(object sender, EventArgs e)
		{
			WorkOnCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.WorkOnTask_Active;
		}

		internal void WorkOnCurrentTaskButton_MouseLeave(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				var currentOrNextTask = BusinessEntity.CurrentOrNextTask;
				if (currentOrNextTask == null || currentOrNextTask.P9_Status != ProcessTaskStatusCodeList.Codes.Working)
				{
					WorkOnCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.WorkOnTask;
				}
			}
		}

		void SuspendCurrentTaskButton_Click(object sender, EventArgs e)
		{
			UpdateStatus(ProcessTaskStatusCodeList.Codes.Suspended);
		}

		void SuspendCurrentTaskButton_MouseEnter(object sender, EventArgs e)
		{
			SuspendCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.SuspendTask_Active;
		}

		internal void SuspendCurrentTaskButton_MouseLeave(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				var currentOrNextTask = BusinessEntity.CurrentOrNextTask;
				if (currentOrNextTask == null || currentOrNextTask.P9_Status != ProcessTaskStatusCodeList.Codes.Suspended)
				{
					SuspendCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.SuspendTask;
				}
			}
		}

		void CloseCurrentTaskButton_Click(object sender, EventArgs e)
		{
			UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
		}

		void CloseCurrentTaskButton_MouseEnter(object sender, EventArgs e)
		{
			CloseCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.CloseTask_Active;
		}

		void CloseCurrentTaskButton_MouseLeave(object sender, EventArgs e)
		{
			CloseCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.CloseTask;
		}

		void CancelCurrentTaskButton_Click(object sender, EventArgs e)
		{
			UpdateStatus(ProcessTaskStatusCodeList.Codes.Cancelled);
		}

		void CancelCurrentTaskButton_MouseEnter(object sender, EventArgs e)
		{
			CancelCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.CancelTask_Active;
		}

		void CancelCurrentTaskButton_MouseLeave(object sender, EventArgs e)
		{
			CancelCurrentTaskButton.BackgroundImage = Enterprise.Client.EDI.Properties.Resources.CancelTask;
		}

		#endregion

		#region SourceModule Override

		void ProductAreaInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowSourceModuleFinderPopupIfRequired();
		}

		void IM_ModuleInfo_ValueChanged(object sender, EventArgs e)
		{
			NotifyIfInternalItem();
			ShowSourceModuleFinderPopupIfRequired();
			SetServiceTypeVisible();
		}

		void ShowSourceModuleFinderPopupIfRequired()
		{
			if (BusinessEntity == null || BusinessEntity.ProductArea.IsEmpty || BusinessEntity.HasContext(SupportIncident.Context.OnOverrideProductClassification))
			{
				return;
			}

			BusinessEntity.Validation.ValidateIM_Module();
			if (BusinessEntity.IM_ModuleInfo.HasErrors())
			{
				return;
			}

			if (BusinessEntity.IncidentTriage != null && BusinessEntity.ProductArea == BusinessEntity.IncidentTriage.IMT_ProductArea)
			{
				return;
			}

			var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(BusinessEntity.ModuleType);
			if (productAreaModuleMappingsRegistryItem == null)
			{
				return;
			}

			var mapping = productAreaModuleMappingsRegistryItem.Value.GetMapping(BusinessEntity.IM_Product, BusinessEntity.IM_Module);
			if (mapping == null || mapping.SourceModuleMappings.Count == 0)
			{
				return;
			}

			var recalculatedProductArea = BusinessEntity.GetRecalculatedProductArea();
			if (recalculatedProductArea != BusinessEntity.ProductArea)
			{
				var findReason = string.Format(@"You have selected a '{0}' that is shared by multiple Product Areas.
The current Menu Item does not belong to the currently selected Product Area ({1}), please select the correct Menu Item to determine the correct Product Area.", EnumExtensions.GetCaption(BusinessEntity.ModuleType), BusinessEntity.ProductAreaDescription);

				ShowSourceModuleFinderPopup(findReason);
			}
		}

		void NotifyIfInternalItem()
		{
			if (BusinessEntity == null || BusinessEntity.ProductArea.IsEmpty
				|| !BusinessEntity.IsInDatabase || !BusinessEntity.IM_ModuleInfo.HasChanges)
			{
				return;
			}

			BusinessEntity.Validation.ValidateIM_Module();
			if (BusinessEntity.IM_ModuleInfo.HasErrors())
			{
				return;
			}

			var addedBy = BusinessEntity.AddedBy;
			if (addedBy == null || !addedBy.GS_IsSystemAccount)
			{
				return;
			}

			var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(BusinessEntity.ModuleType);
			if (productAreaModuleMappingsRegistryItem == null)
			{
				return;
			}

			var mapping = productAreaModuleMappingsRegistryItem.Value.GetMapping(BusinessEntity.IM_Product, BusinessEntity.IM_Module);
			if (mapping == null || !mapping.IsInternal)
			{
				return;
			}

			if (Globals.Message.Show(
						Res.GetString("5c0d98e3-ed22-45e2-9918-507a0cc5e38a", "You are changing the {0} to an Internal Item. Click Yes to proceed.", EnumExtensions.GetCaption(BusinessEntity.ModuleType)),
						Res.GetString("af7cfdb2-718f-4d79-9493-29be4b278e36", "Internal Item"),
						MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
			{
				BusinessEntity.IM_ModuleInfo.Value = BusinessEntity.IM_ModuleInfo.OriginalValue;
			}
		}

		void SetupTriageAssist()
		{
			if (!EDIDataRegistry.Instance.EnableTriageEngineModule.Value)
			{
				TriageAssistButton.Visible = false;
				triageAssistCaption.Visible = false;
				triageAssistLabel.Visible = false;
			}
		}

		void TriageAssistButton_Click(object sender, EventArgs e)
		{
			if (TriageForm != null)
			{
				TriageForm.Focus();
				return;
			}

			if (!BusinessEntity.IsInDatabase)
			{
				Globals.Message.ShowError("You must save the form before using Triage Assist.");
				return;
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var incidentReloaded = newFactory.Load<SupportIncident>(BusinessEntity.PK);
			var assistObject = new TriageAssistBusinessObject(incidentReloaded);
			TriageForm = new TriageAssistForm(assistObject, this);

			TriageForm.FormClosed += (_, x_) =>
			{
				LogPopupFormCount--;
				TriageForm = null;
			};

			assistObject.OnTriageAssistSaved += (_, x_) =>
			{
				if (assistObject.Parent.Factory.HasContext(SupportIncident.Context.OnSecondFactorySave))
				{
					return;
				}

				if (BusinessEntity.HasChanges)
				{
					if (BusinessEntity.IM_IMT_Triage != assistObject.Parent.TriagePK)
					{
						BusinessEntity.IM_IMT_Triage = assistObject.Parent.TriagePK;
					}
				}
				else
				{
					BusinessEntity.Reload();
				}

				CanPopupEscalateIncidentForm = false;
				TriageAssistHelper.TriageAssistOnSaved(new TriageAssistBusinessObject(BusinessEntity), BusinessEntity);
				CanPopupEscalateIncidentForm = true;
			};
			LogPopupFormCount++;
			TriageForm.Show();
		}

		#region CanPopupEscalateIncidentForm

		protected ZBool canPopupEscalateIncidentForm = true;

		protected ZBool CanPopupEscalateIncidentForm
		{
			get { return canPopupEscalateIncidentForm; }
			set
			{
				canPopupEscalateIncidentForm = value;
			}
		}

		#endregion

		public TriageAssistForm TriageForm { get; private set; }

		void OverrideMenuItemButton_Click(object sender, EventArgs e)
		{
			ShowSourceModuleFinderPopup("");
		}

		void ShowSourceModuleFinderPopup(ZString findReason)
		{
			var finder = new SourceModuleFinder(BusinessEntity.IM_Product, BusinessEntity.ModuleType, findReason, BusinessEntity.Factory);
			finder.ProductAreaFilter = BusinessEntity.ProductArea;
			finder.ModuleFilter = BusinessEntity.IM_Module;

			var form = new SourceModuleFinderForm(finder);
			form.ModuleMappingWithSourceModuleSelected += (sender, e) =>
			{
				RemoveShowSourceModuleFinderPopupEventHandlers();
				BusinessEntity.ProductArea = e.ModuleMappingWithSourceModule.ProductArea;
				BusinessEntity.IM_Module = e.ModuleMappingWithSourceModule.ModuleCode;
				BusinessEntity.IM_SourceModuleId = e.ModuleMappingWithSourceModule.SourceModuleCode;
				AddShowSourceModuleFinderPopupEventHandlers();

				SetSourceModuleLabelColor();
			};

			form.FormClosing += (sender, e) =>
			{
				BusinessEntity.RecalculateProductArea();
				ProductAreaDropEdit.Text = BusinessEntity.ProductArea;
			};

			ZFormModaliser.ShowDialogAndDispose(form);
		}

		void AddShowSourceModuleFinderPopupEventHandlers()
		{
			BusinessEntity.ProductAreaInfo.ValueChanged += ProductAreaInfo_ValueChanged;
			BusinessEntity.IM_ModuleInfo.ValueChanged += IM_ModuleInfo_ValueChanged;
		}

		void RemoveShowSourceModuleFinderPopupEventHandlers()
		{
			BusinessEntity.ProductAreaInfo.ValueChanged -= ProductAreaInfo_ValueChanged;
			BusinessEntity.IM_ModuleInfo.ValueChanged -= IM_ModuleInfo_ValueChanged;
		}

		void SetServiceTypeVisible()
		{
			var visibility = BusinessEntity.Lookups.ServiceTypeList.Count > 0;
			serviceTypeCaption.Visible = visibility;
			serviceTypeDropEdit.Visible = visibility;

			if (!visibility && !BusinessEntity.IM_ServiceType.IsEmpty)
			{
				BusinessEntity.IM_ServiceType = ZString.Empty;
			}
		}

		void SetupSystemVersionBoundLabel()
		{
			this.SystemVersionBoundLabel.ContextMenuStrip = new ContextMenuStrip();
			var copyMenuItem = new ZToolStripMenuItem("Copy");
			copyMenuItem.Click += (s, ev) =>
			{
				SafeClipboard.SetText(this.SystemVersionBoundLabel.Text);
			};
			this.SystemVersionBoundLabel.ContextMenuStrip.Items.Add(copyMenuItem);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (outageDurationRefreshTimer != null)
			{
				outageDurationRefreshTimer.Tick -= new EventHandler(OutageDurationRefreshTimer_Tick);
				outageDurationRefreshTimer.Stop();
			}

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				UnsubscribeEventHandlers();
			}
			base.Dispose(disposing);
		}

		void UnsubscribeEventHandlers()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.IM_CategoryInfo.ValueChanged -= SetupQuickActionButtons;
				BusinessEntity.IM_ProductInfo.ValueChanged -= IM_ProductInfo_ValueChanged;
				BusinessEntity.IM_ResolutionCodeInfo.ValueChanged -= SetupQuickActionButtons;
				BusinessEntity.IM_ResolutionCodeInfo.ValueChanged -= SetupClosureResolutionVisible;
				BusinessEntity.OnCloseIncident -= BusinessEntity_OnCloseIncident;
				BusinessEntity.OnCurrentOrNextTaskStatusChange -= SetupTaskButtonsAndEConversationControlsAndCloseIncidentControls;
				BusinessEntity.EConversation.MessageCountChanged -= RefreshEConversationControls;
				BusinessEntity.WorkflowItems.Tasks.CountChanged -= SetupTaskButtonsAndEConversationControlsAndCloseIncidentControls;

				RemoveShowSourceModuleFinderPopupEventHandlers();
				BusinessEntity.IM_OA_BranchAddressInfo.ValueChanged -= new EventHandler(Client_ValueChanged);
				BusinessEntity.IM_StatusInfo.ValueChanged -= new EventHandler(TokenButton_ValueChanged);
				BusinessEntity.IM_ResolutionCodeInfo.ValueChanged -= new EventHandler(TokenButton_ValueChanged);
			}
		}

		#endregion

		#region For Testing
#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

#endif
		#endregion
	}
}
