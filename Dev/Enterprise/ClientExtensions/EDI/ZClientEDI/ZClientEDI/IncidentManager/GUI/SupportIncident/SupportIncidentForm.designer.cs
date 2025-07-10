using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class SupportIncidentForm
	{
		#region Windows Form Designer generated code

		private ZCodeFindBox businessConsultantCodeFindBox;
		private ZTextBox zTextBox7;
		private ZTemplateTabControl zTabControl1;
		private ZTabPage zTabPage2;
		private ZGuidFindBox ParentFeatureRequestFindBox;
		private EDIWorkflowTabPage WorkflowTabPage;
		private KSplitter splitter2;
		private KSplitContainer splitContainer2;
		protected ZGroupBox zGroupBox7;
		private KSplitContainer splitContainer1;
		private ZGroupBox zGroupBox8;
		private ZTextBox zTextBox12;
		private ZGroupBox zGroupBox2;
		private ZTextBox zTextBox10;
		protected ZDropEdit CriticalityDropEdit;
		protected ZLabel zLabel14;
		protected ZDropEdit defectMenuSectionDropEdit;
		protected ZDropEdit defectCr8ModuleDropEdit;
		protected ZDropEdit defectCr9ModuleDropEdit;
		protected ZDropEdit zDropEdit9;
		private ZTextBox zTextBox8;
		private ZLabel zLabel65;
		private ZLabel zLabel66;
		protected ZLabel zLabel67;
		private ZLabel zLabel68;
		private ZLabel zLabel69;
		protected ZLabel zLabel70;
		protected ZLabel zLabel71;
		private ZPanel zPanel2;
		private ZPanel FeatureCustomFieldsPanel;
		protected ZGroupBox zGroupBox1;
		protected ModuleSelectionControl DefectRelatedWorkItemsGrid;
		protected ZGroupBox zGroupBox3;
		protected ZLabel zLabel22;
		private ZCheckBox zCheckBox3;
		private ZCheckBox zCheckBox2;
		private ZLabel zLabel63;
		protected ZLabel zLabel64;
		private ZDropEdit zDropEdit3;
		private ZGuidFindBox zGuidFindBox2;
		protected ZTabPage RelatedItemsTabPage;
		protected ZGuidFindBox relatedProjectGuidFindBox;
		protected ZGroupBox featureWorkItemsGroupBox;
		private ModuleSelectionControl FeatureRequestWorkItemsGrid;
		private KSplitter splitter1;
		private ZPanel zPanel3;
		private ZModuleButtonGrid RelatedFeatureRequestsGrid;
		private ZPanel zPanel4;
		protected ZGroupBox ClientAndContactDetailsGroupBox;
		private ZDropEdit LanguageDropEdit;
		private ZLabel isContactAccreditedLabel;
		private ZLabel SRNumLabel;
		private ZTextBox SRNumValue;
		private ZButton ShowAccreditationButton;
		private ZLabel LocalTimeLabel;
		private ZLabel LocalTimeValueLabel;
		protected ZGuidFindBox ContactGuidFindBox;
		protected ZLabel ContractLabel;
		internal ZLabel ContractStatusLabel;
		private ZPanel zPanel5;
		private ZPanel statePanel;
		private ZGroupBox IncidentLogGroupBox;
		protected ZGroupBox IncidentDetailsGroupBox;
		protected ZPanel IncidentDetailsPanel;
		protected ZLabel SystemVersionBoundLabel;
		protected ZLabel OverallAssignedToCodeLabel;
		protected ZLabel OverallAssignedToDescriptionLabel;
		protected ZDropEdit menuSectionDropEdit;
		protected ZDropEdit cr8ModuleDropEdit;
		protected ZDropEdit cr9ModuleDropEdit;
		protected ZDropEdit zDropEdit1;
		protected ZLabel SystemVersionLabel;
		protected ZDropEdit sourceDropEdit;
		protected ZLabel AssignedToLabel;
		protected ZDropEdit RequestedPriorityDropEdit;
		protected ZGroupBox StatusGroupBox;
		private ZTextBox IncidentNumberTextBox;
		private ZLabel eRequestStatusLabel;
		protected ZLabel StatusDescription;
		protected ZLabel StatusLabel;
		private ZGroupBox groupBox2;
		private ZTextBox zTextBox2;
		private ZTextBox zTextBox1;
		protected ZGroupBox featureAnalysisGroupBox;
		protected ZGroupBox zGroupBox6;
		private ZLabel zLabel48;
		private ZLabel zLabel49;
		protected ZLabel OverallAssignedToLabel;
		private ZLabel zLabel51;
		private ZLabel zLabel52;
		protected ZLabel zLabel53;
		protected ZLabel zLabel54;
		private ZLabel EstimatedDateLabel;
		private ZLabel EstimatedDateCaption;
		private ZTabPage DefectManagementTabPage;
		private ZTabPage FeatureRequestTabPage;
		private ZTabPage featureRequestAnalysisPage;
		private ZTabControl ProjectRelatedIncidentNoteTabControl;
		private ZTabPage zTabPage5;
		private ZTabPage zTabPage6;
		private ZTabPage zTabPage7;
		private ZTabPage zTabPage8;
		private ZRichTextBox BusinessRequirementRichTextBox;
		private ZRichTextBox TechnicalSpecificationRichTextBox;
		private ZRichTextBox FeatureRequestPrerequisitesRichTextBox;
		private ZRichTextBox InternalNotesRichTextBox;
		private KSplitContainer featureSplitContainer;
		private ZGuidFindBox DefectCausedByWIGuidFindBox;
		private ZLabel ClientSizeDescriptionLabel;
		protected ZLabel ClientSizeLabel;
		protected ZGroupBox IncidentCommentGroupBox;
		private ZTextBox IncidentCommentTextBox;
		internal ZButton TriageAssistButton;
		internal ZButton OverrideSourceModuleButton;
		protected ZDropEdit CountryDropEdit;
		protected ZLabel menuItemCaption;
		protected ZLabel triageAssistCaption;
		protected ZLabel serviceTypeCaption;
		protected ZDropEdit serviceTypeDropEdit;
		private ZLabel sourceModuleLabel;
		private ZLabel triageAssistLabel;
		protected ZDropEdit ProductAreaDropEdit;
		protected ZLabel eRequestStatusLabelText;
		protected ZLabel CurrentTaskLabel;
		protected ZLabel CurrentTaskLabelText;
		private ZLabel zLabel6;
		protected ZLabel zLabel7;
		protected ZDropEdit zDropEdit5;
		private ZLabel zLabel8;
		protected ZLabel zLabel17;
		private ZLabel stageLabelText;
		private ZLabel ServiceStatusLabelText;
		private ZLabel OutageDurationLabelText;
		private ZLabel stageLabel;
		protected ZLabel ServiceStatusLabel;
		protected ZLabel OutageDurationLabel;
		private ZLabel RelationshipManagerCaption;
		private ZLabel HostedLocationLabel;
		private ZLabel HostedLocationCaption;
		private ZLabel WorkplaceLabel;
		private ZLabel WorkplaceCaption;
		protected ZLabel WorkplaceLabel2;
		private ZLabel WorkplaceCaption2;
		private ZLabel RelMgrLabel;
		internal ZButton SendMessageButton;
		protected ZButton CloseIncidentButton;
		internal protected ZButton AwaitingResponseButton;
		internal ZTextBox ConversationMessageTextBox;
		protected KSplitContainer splitContainer5;
		protected IncidentContactPhoneDiallerUserControl ContactPhoneDiallerUserControl;
		internal ZButton CancelCurrentTaskButton;
		internal ZButton CloseCurrentTaskButton;
		internal ZButton SuspendCurrentTaskButton;
		internal ZButton WorkOnCurrentTaskButton;
		private ZGroupBox FeatureRequestClientGroupBox;
		private ZAddressControl FeatureRequestClientAddressControl;
		protected ZGuidFindBox FeatureRequestContactFindBox;
		private ZLabel FeatureRequestClientRegNo;
		private ZLabel FeatureRequestClientRegType;
		private KSplitContainer featureDetailsClientSplitContainer;
		private ZGroupBox CustomFieldsGroupBox;
		private ZGroupBox EstGroupBox;
		private ZDropEdit EstPaymentTermsDropEdit;
		private ZDateEdit EstRequestDate;
		private ZDateEdit EstExpiryDate;
		private ZDateEdit EstSentDate;
		private ZDateEdit EstExpressDelCutOff;
		private ZCalcEdit EstCancellationFee;
		private ZCalcEdit EstMaxOneOff;
		private ZCalcEdit EstMinOneOff;
		private ZCalcEdit EstMaxMonthly;
		private ZCalcEdit EstMinMonthly;
		private ZCodeFindBox EstCurrencyBox;
		private ZCalcEdit EstMaxDevHours;
		private ZCalcEdit EstMinDevHours;
		private ZGroupBox QteGroupBox;
		private ZDropEdit QtePaymentTermsDropEdit;
		private ZDateEdit QteAcceptedDate;
		private ZDateEdit QteExpiryDate;
		private ZDateEdit QteSentDate;
		private ZDateEdit QteDateDelivered;
		private ZCheckBox QteExpressDeliveryIncludedCheckBox;
		private ZCalcEdit QteHeadStartSurcharge;
		private ZCheckBox QteHeadStartIncludedCheckBox;
		private ZCalcEdit QteOneOffUpfront;
		private ZCalcEdit QteAmount;
		private ZCalcEdit QteExpressDeliverySurcharge;
		private ZCodeFindBox QteCurrencyBox;
		private ZCalcEdit QteMaxDevHours;
		private ZCalcEdit QteMinDevHours;
		private ZCalcEdit QteCancellationFee;
		private ZDropEdit QtePaymentTypeDropEdit;
		private ProcessTemplateCustomFieldsControl CustomFieldsControl;
		private ZTabPage zTabPage1;
		private ZRichTextBox SoftwareChangeRichTextBox;
		internal Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit SupportCompanyDropEdit;
		private Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit supportDatabaseDropEdit;
		private ZGuidFindBox supportEnterpriseGuidFindBox;
		private ZCodeFindBox supportEnterpriseCodeFindBox;
		private ZGuidFindBox supportClientGuidFindBox;
		protected ZLabel supportClientNameLabel;
		internal ZCollapsiblePanel panelSimilarIncidents;
		internal KSplitter splitterSimilarIncidents;
		private SimilarIncidentsUserControl similarIncidentsUserControl;
		private System.ComponentModel.IContainer components = null;
		internal ZDateEdit ClientRequiredDate;
		private ZGroupBox workflowStateGroupBox;
		protected ZLabel resolutionMethodLabelText;
		private KSplitContainer statusStateSplitContainer;
		private ZDropEdit closureResolutionDropEdit;

		#region PreviousNextControl

		private ZButton nextButton;
		private ZButton previousButton;
		private ZCalcEdit numberOfResultsCalcEdit;
		private ZCalcEdit currentResultCalcEdit;

		#endregion

		/// <summary>
		/// Don't make this non-virtual. It'll break the TestContractStatus test.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			this.DefectManagementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox7 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zDropEdit5 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox8 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox12 = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox10 = new Enterprise.ZArchitecture.ZTextBox();
			this.CriticalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.defectMenuSectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.defectCr8ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.defectCr9ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit9 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox8 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel65 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel66 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel67 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel68 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel69 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel70 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel71 = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefectRelatedWorkItemsGrid = new Enterprise.Client.EDI.IncidentManager.GUI.ModuleSelectionControl();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefectCausedByWIGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zCheckBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel63 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel64 = new Enterprise.ZArchitecture.ZLabel();
			this.zDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGuidFindBox2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.splitter2 = new CargoWise.Windows.UI.KSplitter();
			this.FeatureRequestTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.featureSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.featureAnalysisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.featureRequestAnalysisPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProjectRelatedIncidentNoteTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.zTabPage5 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BusinessRequirementRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SoftwareChangeRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zTabPage6 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TechnicalSpecificationRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zTabPage7 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FeatureRequestPrerequisitesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zTabPage8 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InternalNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RelatedFeatureRequestsGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ParentFeatureRequestFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.featureWorkItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeatureRequestWorkItemsGrid = new Enterprise.Client.EDI.IncidentManager.GUI.ModuleSelectionControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.FeatureCustomFieldsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.featureDetailsClientSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox6 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.ClientSizeDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ClientSizeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EstimatedDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EstimatedDateCaption = new Enterprise.ZArchitecture.ZLabel();
			this.businessConsultantCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.relatedProjectGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel48 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel49 = new Enterprise.ZArchitecture.ZLabel();
			this.OverallAssignedToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel51 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel52 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel53 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel54 = new Enterprise.ZArchitecture.ZLabel();
			this.ClientRequiredDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FeatureRequestClientGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeatureRequestClientRegType = new Enterprise.ZArchitecture.ZLabel();
			this.FeatureRequestClientRegNo = new Enterprise.ZArchitecture.ZLabel();
			this.WorkplaceCaption = new Enterprise.ZArchitecture.ZLabel();
			this.WorkplaceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FeatureRequestClientAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FeatureRequestContactFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.EstGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EstPaymentTermsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EstRequestDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstExpiryDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstSentDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstExpressDelCutOff = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstCancellationFee = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstMaxOneOff = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstMinOneOff = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstMaxMonthly = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstMinMonthly = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstCurrencyBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EstMaxDevHours = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstMinDevHours = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QteGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QteCancellationFee = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QtePaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QtePaymentTermsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QteDateDelivered = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QteAcceptedDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QteExpiryDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QteSentDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QteExpressDeliveryIncludedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QteHeadStartSurcharge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QteHeadStartIncludedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QteOneOffUpfront = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QteAmount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QteExpressDeliverySurcharge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QteCurrencyBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QteMaxDevHours = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QteMinDevHours = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WorkflowTabPage = new Enterprise.Client.EDI.IncidentManager.GUI.EDIWorkflowTabPage(this.components);
			this.RelatedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.groupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.IncidentCommentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncidentCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientAndContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.supportClientNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SupportCompanyDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			this.supportDatabaseDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			this.supportEnterpriseGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.supportEnterpriseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.supportClientGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ContactPhoneDiallerUserControl = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentContactPhoneDiallerUserControl();
			this.RelMgrLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HostedLocationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HostedLocationCaption = new Enterprise.ZArchitecture.ZLabel();
			this.WorkplaceLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.WorkplaceCaption2 = new Enterprise.ZArchitecture.ZLabel();
			this.RelationshipManagerCaption = new Enterprise.ZArchitecture.ZLabel();
			this.isContactAccreditedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SRNumLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SRNumValue = new Enterprise.ZArchitecture.ZTextBox();
			this.ShowAccreditationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LocalTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocalTimeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ContractLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContractStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel5 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.IncidentLogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainer5 = new CargoWise.Windows.UI.KSplitContainer();
			this.CloseIncidentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AwaitingResponseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConversationMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IncidentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncidentDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverrideSourceModuleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TriageAssistButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.triageAssistCaption = new Enterprise.ZArchitecture.ZLabel();
			this.menuItemCaption = new Enterprise.ZArchitecture.ZLabel();
			this.serviceTypeCaption = new Enterprise.ZArchitecture.ZLabel();
			this.serviceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.triageAssistLabel = new Enterprise.ZArchitecture.ZLabel();
			this.sourceModuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SystemVersionBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.menuSectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.cr8ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.cr9ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SystemVersionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.sourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestedPriorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.statePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.statusStateSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.resolutionMethodLabelText = new Enterprise.ZArchitecture.ZLabel();
			this.stageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ServiceStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OutageDurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.stageLabelText = new Enterprise.ZArchitecture.ZLabel();
			this.ServiceStatusLabelText = new Enterprise.ZArchitecture.ZLabel();
			this.OutageDurationLabelText = new Enterprise.ZArchitecture.ZLabel();
			this.eRequestStatusLabelText = new Enterprise.ZArchitecture.ZLabel();
			this.IncidentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eRequestStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.workflowStateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OverallAssignedToDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverallAssignedToCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssignedToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CurrentTaskLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CurrentTaskLabelText = new Enterprise.ZArchitecture.ZLabel();
			this.StatusDescription = new Enterprise.ZArchitecture.ZLabel();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseCurrentTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelCurrentTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SuspendCurrentTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.WorkOnCurrentTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.panelSimilarIncidents = new Enterprise.ZArchitecture.GUI.ZCollapsiblePanel();
			this.similarIncidentsUserControl = new Enterprise.Client.EDI.IncidentManager.GUI.SimilarIncidentsUserControl();
			this.splitterSimilarIncidents = new CargoWise.Windows.UI.KSplitter();
			this.closureResolutionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DefectManagementTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.zGroupBox7.SuspendLayout();
			this.zDropEdit5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox8.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.CriticalityDropEdit.SuspendLayout();
			this.defectMenuSectionDropEdit.SuspendLayout();
			this.defectCr8ModuleDropEdit.SuspendLayout();
			this.defectCr9ModuleDropEdit.SuspendLayout();
			this.zDropEdit9.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DefectRelatedWorkItemsGrid.InnerGrid)).BeginInit();
			this.DefectRelatedWorkItemsGrid.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.DefectCausedByWIGuidFindBox.SuspendLayout();
			this.zDropEdit3.SuspendLayout();
			this.zGuidFindBox2.SuspendLayout();
			this.FeatureRequestTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.featureSplitContainer)).BeginInit();
			this.featureSplitContainer.Panel1.SuspendLayout();
			this.featureSplitContainer.Panel2.SuspendLayout();
			this.featureSplitContainer.SuspendLayout();
			this.featureAnalysisGroupBox.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.featureRequestAnalysisPage.SuspendLayout();
			this.ProjectRelatedIncidentNoteTabControl.SuspendLayout();
			this.zTabPage5.SuspendLayout();
			this.BusinessRequirementRichTextBox.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.SoftwareChangeRichTextBox.SuspendLayout();
			this.zTabPage6.SuspendLayout();
			this.TechnicalSpecificationRichTextBox.SuspendLayout();
			this.zTabPage7.SuspendLayout();
			this.FeatureRequestPrerequisitesRichTextBox.SuspendLayout();
			this.zTabPage8.SuspendLayout();
			this.InternalNotesRichTextBox.SuspendLayout();
			this.zTabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedFeatureRequestsGrid.InnerGrid)).BeginInit();
			this.RelatedFeatureRequestsGrid.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.ParentFeatureRequestFindBox.SuspendLayout();
			this.featureWorkItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeatureRequestWorkItemsGrid.InnerGrid)).BeginInit();
			this.FeatureRequestWorkItemsGrid.SuspendLayout();
			this.FeatureCustomFieldsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.featureDetailsClientSplitContainer)).BeginInit();
			this.featureDetailsClientSplitContainer.Panel1.SuspendLayout();
			this.featureDetailsClientSplitContainer.Panel2.SuspendLayout();
			this.featureDetailsClientSplitContainer.SuspendLayout();
			this.zGroupBox6.SuspendLayout();
			this.businessConsultantCodeFindBox.SuspendLayout();
			this.relatedProjectGuidFindBox.SuspendLayout();
			this.ClientRequiredDate.SuspendLayout();
			this.FeatureRequestClientGroupBox.SuspendLayout();
			this.FeatureRequestClientAddressControl.SuspendLayout();
			this.FeatureRequestContactFindBox.SuspendLayout();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.CustomFieldsControl.SuspendLayout();
			this.EstGroupBox.SuspendLayout();
			this.EstPaymentTermsDropEdit.SuspendLayout();
			this.EstRequestDate.SuspendLayout();
			this.EstExpiryDate.SuspendLayout();
			this.EstSentDate.SuspendLayout();
			this.EstExpressDelCutOff.SuspendLayout();
			this.EstCurrencyBox.SuspendLayout();
			this.QteGroupBox.SuspendLayout();
			this.QtePaymentTypeDropEdit.SuspendLayout();
			this.QtePaymentTermsDropEdit.SuspendLayout();
			this.QteDateDelivered.SuspendLayout();
			this.QteAcceptedDate.SuspendLayout();
			this.QteExpiryDate.SuspendLayout();
			this.QteSentDate.SuspendLayout();
			this.QteCurrencyBox.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.IncidentCommentGroupBox.SuspendLayout();
			this.ClientAndContactDetailsGroupBox.SuspendLayout();
			this.SupportCompanyDropEdit.SuspendLayout();
			this.supportDatabaseDropEdit.SuspendLayout();
			this.supportEnterpriseGuidFindBox.SuspendLayout();
			this.supportEnterpriseCodeFindBox.SuspendLayout();
			this.supportClientGuidFindBox.SuspendLayout();
			this.ContactPhoneDiallerUserControl.SuspendLayout();
			this.LanguageDropEdit.SuspendLayout();
			this.ContactGuidFindBox.SuspendLayout();
			this.zPanel5.SuspendLayout();
			this.IncidentLogGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
			this.splitContainer5.Panel1.SuspendLayout();
			this.splitContainer5.SuspendLayout();
			this.IncidentDetailsGroupBox.SuspendLayout();
			this.IncidentDetailsPanel.SuspendLayout();
			this.CountryDropEdit.SuspendLayout();
			this.serviceTypeDropEdit.SuspendLayout();
			this.ProductAreaDropEdit.SuspendLayout();
			this.menuSectionDropEdit.SuspendLayout();
			this.cr8ModuleDropEdit.SuspendLayout();
			this.cr9ModuleDropEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.sourceDropEdit.SuspendLayout();
			this.RequestedPriorityDropEdit.SuspendLayout();
			this.statePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusStateSplitContainer)).BeginInit();
			this.statusStateSplitContainer.Panel1.SuspendLayout();
			this.statusStateSplitContainer.Panel2.SuspendLayout();
			this.statusStateSplitContainer.SuspendLayout();
			this.StatusGroupBox.SuspendLayout();
			this.workflowStateGroupBox.SuspendLayout();
			this.panelSimilarIncidents.SuspendLayout();
			this.similarIncidentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.DefectManagementTabPage);
			this.MainTabControl.Controls.Add(this.FeatureRequestTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.RelatedItemsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 613, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Selecting);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RelatedItemsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.FeatureRequestTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DefectManagementTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Customer Service");
			this.MainTabPage.Controls.Add(this.zPanel5);
			this.MainTabPage.Controls.Add(this.splitterSimilarIncidents);
			this.MainTabPage.Controls.Add(this.panelSimilarIncidents);
			this.MainTabPage.Controls.Add(this.zPanel4);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 586, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 586, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 613, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 1498;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncident);
			// 
			// DefectManagementTabPage
			// 
			this.DefectManagementTabPage.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|36f8e5d2-0123-459b-b159-1561e9bd7b34", "Defect");
			this.DefectManagementTabPage.Controls.Add(this.splitter2);
			this.DefectManagementTabPage.Controls.Add(this.splitContainer2);
			this.DefectManagementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DefectManagementTabPage.Name = "DefectManagementTabPage";
			this.DefectManagementTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DefectManagementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 586, true);
			this.DefectManagementTabPage.TabIndex = 3;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.zGroupBox7);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.zPanel2);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 579, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(295);
			this.splitContainer2.TabIndex = 23;
			// 
			// zGroupBox7
			// 
			this.zGroupBox7.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|bdff32c3-51e3-4d1c-81ca-f17d22acde4b", "Details");
			this.zGroupBox7.Controls.Add(this.zLabel6);
			this.zGroupBox7.Controls.Add(this.zLabel7);
			this.zGroupBox7.Controls.Add(this.zDropEdit5);
			this.zGroupBox7.Controls.Add(this.splitContainer1);
			this.zGroupBox7.Controls.Add(this.CriticalityDropEdit);
			this.zGroupBox7.Controls.Add(this.defectMenuSectionDropEdit);
			this.zGroupBox7.Controls.Add(this.defectCr8ModuleDropEdit);
			this.zGroupBox7.Controls.Add(this.defectCr9ModuleDropEdit);
			this.zGroupBox7.Controls.Add(this.zDropEdit9);
			this.zGroupBox7.Controls.Add(this.zTextBox8);
			this.zGroupBox7.Controls.Add(this.zLabel65);
			this.zGroupBox7.Controls.Add(this.zLabel66);
			this.zGroupBox7.Controls.Add(this.zLabel67);
			this.zGroupBox7.Controls.Add(this.zLabel68);
			this.zGroupBox7.Controls.Add(this.zLabel69);
			this.zGroupBox7.Controls.Add(this.zLabel70);
			this.zGroupBox7.Controls.Add(this.zLabel71);
			this.zGroupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox7.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 229, true);
			this.zGroupBox7.Name = "zGroupBox7";
			this.zGroupBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 295, true);
			this.zGroupBox7.TabIndex = 1;
			this.zGroupBox7.TabStop = false;
			// 
			// zLabel6
			// 
			this.zLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel6, "CurrentTask+P9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).CurrentTask.P9_Description)));
			this.zLabel6.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel6, false);
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 62, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20, true);
			this.zLabel6.TabIndex = 25;
			this.zLabel6.Text = "<Current Task Disposition>";
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 64, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.zLabel7.TabIndex = 24;
			this.zLabel7.Text = "Current Task:";
			this.zLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zDropEdit5
			// 
			this.zDropEdit5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit5, "ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.FilteredProductAreaList)));
			this.zDropEdit5.BindToList = "Lookups+FilteredProductAreaList";
			this.zDropEdit5.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|606104d2-f62b-427a-821b-6ce7fede1922", "Area", "Area", "Product Area", "");
			this.zDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 63, true);
			this.zDropEdit5.MaxItemsToShowInDropDown = 20;
			this.zDropEdit5.Name = "zDropEdit5";
			this.zDropEdit5.PreBoundMaxLength = 3;
			this.zDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.zDropEdit5.TabIndex = 23;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 115, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.zGroupBox8);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBox2);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 177, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(568);
			this.splitContainer1.TabIndex = 22;
			// 
			// zGroupBox8
			// 
			this.zGroupBox8.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|b138bf55-2f0b-4faa-8a1a-1af716164d27", "Resolution Comment");
			this.zGroupBox8.Controls.Add(this.zTextBox12);
			this.zGroupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox8.Name = "zGroupBox8";
			this.zGroupBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 177, true);
			this.zGroupBox8.TabIndex = 21;
			this.zGroupBox8.TabStop = false;
			// 
			// zTextBox12
			// 
			this.zTextBox12.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox12, "ResolutionNoteText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ResolutionNoteText)));
			this.zTextBox12.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox12.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox12, false);
			this.zTextBox12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox12.Multiline = true;
			this.zTextBox12.Name = "zTextBox12";
			this.zTextBox12.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 158, true);
			this.zTextBox12.TabIndex = 6;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|243dc2f4-2145-44c1-8739-90a9a9b63c07", "Problem Description");
			this.zGroupBox2.Controls.Add(this.zTextBox10);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 177, true);
			this.zGroupBox2.TabIndex = 26;
			this.zGroupBox2.TabStop = false;
			// 
			// zTextBox10
			// 
			this.zTextBox10.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox10, "DetailNoteText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).DetailNoteText)));
			this.zTextBox10.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox10, false);
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox10.Multiline = true;
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 158, true);
			this.zTextBox10.TabIndex = 7;
			// 
			// CriticalityDropEdit
			// 
			this.CriticalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CriticalityDropEdit, "IM_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Priority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.CriticalityList)));
			this.CriticalityDropEdit.BindToList = "Lookups+CriticalityList";
			this.CriticalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 89, true);
			this.CriticalityDropEdit.MaxItemsToShowInDropDown = 20;
			this.CriticalityDropEdit.Name = "CriticalityDropEdit";
			this.CriticalityDropEdit.PreBoundMaxLength = 3;
			this.CriticalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CriticalityDropEdit.TabIndex = 5;
			// 
			// defectMenuSectionDropEdit
			// 
			this.defectMenuSectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defectMenuSectionDropEdit, "IM_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ModuleDescription)));
			this.defectMenuSectionDropEdit.BindToForDescription = "IM_ModuleDescription";
			this.defectMenuSectionDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("51531051-9f7b-4382-9c56-a8ae3c3b6bcd", "Section", "Menu Section");
			this.defectMenuSectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 89, true);
			this.defectMenuSectionDropEdit.MaxItemsToShowInDropDown = 20;
			this.defectMenuSectionDropEdit.Name = "defectMenuSectionDropEdit";
			this.defectMenuSectionDropEdit.PreBoundMaxLength = 3;
			this.defectMenuSectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.defectMenuSectionDropEdit.TabIndex = 4;
			this.defectMenuSectionDropEdit.Visible = false;
			// 
			// defectCr8ModuleDropEdit
			// 
			this.defectCr8ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defectCr8ModuleDropEdit, "IM_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ModuleDescription)));
			this.defectCr8ModuleDropEdit.BindToForDescription = "IM_ModuleDescription";
			this.defectCr8ModuleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("87ad0d56-5b6f-4a9a-b0b7-85ca935ff1a2", "Requirement");
			this.defectCr8ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 89, true);
			this.defectCr8ModuleDropEdit.MaxItemsToShowInDropDown = 20;
			this.defectCr8ModuleDropEdit.Name = "defectCr8ModuleDropEdit";
			this.defectCr8ModuleDropEdit.PreBoundMaxLength = 3;
			this.defectCr8ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.defectCr8ModuleDropEdit.TabIndex = 4;
			this.defectCr8ModuleDropEdit.Visible = false;
			// 
			// defectCr9ModuleDropEdit
			// 
			this.defectCr9ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defectCr9ModuleDropEdit, "IM_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ModuleDescription)));
			this.defectCr9ModuleDropEdit.BindToForDescription = "IM_ModuleDescription";
			this.defectCr9ModuleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("e303e32a-2b94-4614-8ef2-5a76b04f8a75", "Service");
			this.defectCr9ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 89, true);
			this.defectCr9ModuleDropEdit.MaxItemsToShowInDropDown = 20;
			this.defectCr9ModuleDropEdit.Name = "defectCr9ModuleDropEdit";
			this.defectCr9ModuleDropEdit.PreBoundMaxLength = 3;
			this.defectCr9ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.defectCr9ModuleDropEdit.TabIndex = 4;
			this.defectCr9ModuleDropEdit.Visible = false;
			// 
			// zDropEdit9
			// 
			this.zDropEdit9.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit9, "IM_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.ProductList)));
			this.zDropEdit9.BindToList = "Lookups+ProductList";
			this.zDropEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 38, true);
			this.zDropEdit9.MaxItemsToShowInDropDown = 20;
			this.zDropEdit9.Name = "zDropEdit9";
			this.zDropEdit9.PreBoundMaxLength = 3;
			this.zDropEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.zDropEdit9.TabIndex = 3;
			// 
			// zTextBox8
			// 
			this.zTextBox8.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox8, "IM_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Description)));
			this.zTextBox8.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|8066fcf2-3c67-46c6-bd5f-4715747f655e", "Summary");
			this.zTextBox8.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 15, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 20, true);
			this.zTextBox8.TabIndex = 1;
			// 
			// zLabel65
			// 
			this.BindingSource.SetBindingMember(this.zLabel65, "OverallAssignedToCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToCode)));
			this.zLabel65.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel65, false);
			this.zLabel65.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 61, true);
			this.zLabel65.Name = "zLabel65";
			this.zLabel65.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 20, true);
			this.zLabel65.TabIndex = 11;
			this.zLabel65.Text = "XXX";
			// 
			// zLabel66
			// 
			this.BindingSource.SetBindingMember(this.zLabel66, "OverallAssignedToDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToDescription)));
			this.zLabel66.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel66, false);
			this.zLabel66.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 65, true);
			this.zLabel66.Name = "zLabel66";
			this.zLabel66.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 35, true);
			this.zLabel66.TabIndex = 12;
			this.zLabel66.Text = "<Staff Fullname>";
			this.zLabel66.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zLabel67
			// 
			this.BindingSource.SetBindingMember(this.zLabel67, "OverallAssignedToLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToLabelText)));
			this.zLabel67.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 65, true);
			this.zLabel67.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zLabel67.Name = "zLabel67";
			this.zLabel67.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 35, true);
			this.zLabel67.TabIndex = 10;
			this.zLabel67.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// zLabel68
			// 
			this.zLabel68.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel68, "IM_ResolutionCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ResolutionCodeDescription)));
			this.zLabel68.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel68, false);
			this.zLabel68.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 37, true);
			this.zLabel68.Name = "zLabel68";
			this.zLabel68.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 23, true);
			this.zLabel68.TabIndex = 14;
			this.zLabel68.Text = "<Disposition>";
			// 
			// zLabel69
			// 
			this.BindingSource.SetBindingMember(this.zLabel69, "IM_StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_StatusDescription)));
			this.zLabel69.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel69, false);
			this.zLabel69.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 36, true);
			this.zLabel69.Name = "zLabel69";
			this.zLabel69.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.zLabel69.TabIndex = 9;
			this.zLabel69.Text = "<Status>";
			// 
			// zLabel70
			// 
			this.zLabel70.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|529cac10-06e7-487f-8815-a6a1059ea634", "Disposition:");
			this.zLabel70.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 38, true);
			this.zLabel70.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zLabel70.Name = "zLabel70";
			this.zLabel70.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.zLabel70.TabIndex = 13;
			this.zLabel70.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel71
			// 
			this.zLabel71.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|391ce402-5043-4513-88d9-4166f5b3b4c7", "Status:");
			this.zLabel71.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 36, true);
			this.zLabel71.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zLabel71.Name = "zLabel71";
			this.zLabel71.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
			this.zLabel71.TabIndex = 8;
			this.zLabel71.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.zGroupBox1);
			this.zPanel2.Controls.Add(this.zGroupBox3);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 183, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 280, true);
			this.zPanel2.TabIndex = 4;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|2c1cd004-d9b8-45bb-907e-011b362aa230", "Work Item Details");
			this.zGroupBox1.Controls.Add(this.DefectRelatedWorkItemsGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 113, true);
			this.zGroupBox1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 70, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 167, true);
			this.zGroupBox1.TabIndex = 3;
			this.zGroupBox1.TabStop = false;
			// 
			// DefectRelatedWorkItemsGrid
			// 
			this.DefectRelatedWorkItemsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefectRelatedWorkItemsGrid, "RelatedWorkItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).RelatedWorkItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.WorkItems)));
			this.DefectRelatedWorkItemsGrid.BindToFindBoxList = "Lookups+WorkItems";
			zTextBoxColumnStyleInfo1.ColumnName = "WKI_WorkItemNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "WKI_Summary";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|75daa53f-59fd-4ca2-b0e3-87d3061b26d8", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "WorkItemType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "WKI_Status";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|4a227a68-80ab-430e-8871-11bf77b48ae3", "Disposition");
			zTextBoxColumnStyleInfo5.ColumnName = "DispositionDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|a83f9f22-0c73-428f-9538-1a0ed88b8dcf", "Assigned");
			zTextBoxColumnStyleInfo6.ColumnName = "AssignedToStaff+GS_Code";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|7b233b38-41bd-4e8c-9abf-5ba224fd6bdc", "Current Task");
			zTextBoxColumnStyleInfo7.ColumnName = "CurrentOrNextTask+P9_Type";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|de57971c-c6a3-41aa-98e4-8c367991a7a6", "Task Est. Start date");
			zDateEditColumnStyleInfo1.ColumnName = "CurrentOrNextTask+P9_ScheduledDateForBinding";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|0960b02f-7c01-4997-b508-7ed65d4b5e2b", "Task Act. Start date");
			zDateEditColumnStyleInfo2.ColumnName = "CurrentOrNextTask+P9_ActualDateForBinding";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|78043c67-5c6e-4591-ace6-95734f3a4106", "Task Est. Duration");
			zTimeEditExColumnStyleInfo1.ColumnName = "CurrentOrNextTask+P9_EstDuration";
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo2.AllowNegative = false;
			zTimeEditExColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|d3076b76-13c5-47c2-b2ee-71ad143acb31", "Task Act. Duration");
			zTimeEditExColumnStyleInfo2.ColumnName = "CurrentOrNextTask+P9_ActualDuration";
			zTimeEditExColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.DefectRelatedWorkItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo2);
			this.DefectRelatedWorkItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefectRelatedWorkItemsGrid.GridId = "d41b79f9-4d69-401b-bbd5-5af25ad35bed";
			// 
			// 
			// 
			this.DefectRelatedWorkItemsGrid.InnerGrid.AllowNavigation = false;
			this.DefectRelatedWorkItemsGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DefectRelatedWorkItemsGrid.InnerGrid.CaptionVisible = false;
			this.DefectRelatedWorkItemsGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.DefectRelatedWorkItemsGrid.InnerGrid.GridId = null;
			this.DefectRelatedWorkItemsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefectRelatedWorkItemsGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.DefectRelatedWorkItemsGrid.InnerGrid.LayoutKey = "Grid";
			this.DefectRelatedWorkItemsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DefectRelatedWorkItemsGrid.InnerGrid.Name = "Grid";
			this.DefectRelatedWorkItemsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1166, 110, true);
			this.DefectRelatedWorkItemsGrid.InnerGrid.TabIndex = 0;
			this.DefectRelatedWorkItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DefectRelatedWorkItemsGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WorkItem;
			this.DefectRelatedWorkItemsGrid.Name = "DefectRelatedWorkItemsGrid";
			this.DefectRelatedWorkItemsGrid.NameOfAGridElement = ZClientEDI.Res.GetData("A18974E3-20A1-44DC-9995-428FABEB517C", "Work Item");
			this.DefectRelatedWorkItemsGrid.ReadOnly = false;
			this.DefectRelatedWorkItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 148, true);
			this.DefectRelatedWorkItemsGrid.TabIndex = 12;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|6967e0e4-496d-40df-8a4b-64fb090ac633", "Defect Analysis");
			this.zGroupBox3.Controls.Add(this.DefectCausedByWIGuidFindBox);
			this.zGroupBox3.Controls.Add(this.zCheckBox3);
			this.zGroupBox3.Controls.Add(this.zCheckBox2);
			this.zGroupBox3.Controls.Add(this.zLabel63);
			this.zGroupBox3.Controls.Add(this.zLabel64);
			this.zGroupBox3.Controls.Add(this.zDropEdit3);
			this.zGroupBox3.Controls.Add(this.zGuidFindBox2);
			this.zGroupBox3.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox3.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 113, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 113, true);
			this.zGroupBox3.TabIndex = 1;
			this.zGroupBox3.TabStop = false;
			// 
			// DefectCausedByWIGuidFindBox
			// 
			this.DefectCausedByWIGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefectCausedByWIGuidFindBox, "DefectCausedByWorkItemPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).DefectCausedByWorkItemPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.WorkItems)));
			this.DefectCausedByWIGuidFindBox.BindToList = "Lookups.WorkItems";
			this.DefectCausedByWIGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|d8ef04f9-853b-41b7-b27b-6a31f857406d", "Caused By Work Item");
			this.DefectCausedByWIGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 18, true);
			this.DefectCausedByWIGuidFindBox.Name = "DefectCausedByWIGuidFindBox";
			this.DefectCausedByWIGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.DefectCausedByWIGuidFindBox.TabIndex = 8;
			// 
			// zCheckBox3
			// 
			this.zCheckBox3.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox3, "IM_ReproducedInPatchRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ReproducedInPatchRelease)));
			this.zCheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 90, true);
			this.zCheckBox3.Name = "zCheckBox3";
			this.zCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
			this.zCheckBox3.TabIndex = 13;
			this.zCheckBox3.UseVisualStyleBackColor = true;
			// 
			// zCheckBox2
			// 
			this.zCheckBox2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox2, "IM_ReproducedInAlphaRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ReproducedInAlphaRelease)));
			this.zCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 90, true);
			this.zCheckBox2.Name = "zCheckBox2";
			this.zCheckBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 17, true);
			this.zCheckBox2.TabIndex = 12;
			this.zCheckBox2.UseVisualStyleBackColor = true;
			// 
			// zLabel63
			// 
			this.BindingSource.SetBindingMember(this.zLabel63, "UpgradeDeployDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).UpgradeDeployDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel63, false);
			this.zLabel63.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(666, 15, true);
			this.zLabel63.Name = "zLabel63";
			this.zLabel63.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 23, true);
			this.zLabel63.TabIndex = 10;
			this.zLabel63.Text = "<UpgradeDeployDate>";
			// 
			// zLabel64
			// 
			this.zLabel64.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|b831e4a4-688c-4d40-a16d-bb004759074e", "Deployed Date:");
			this.zLabel64.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.zLabel64.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 15, true);
			this.zLabel64.Name = "zLabel64";
			this.zLabel64.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.zLabel64.TabIndex = 9;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "IM_ClientBugSeverity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ClientBugSeverity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.BugSeverities)));
			this.zDropEdit3.BindToList = "Lookups+BugSeverities";
			this.zDropEdit3.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|d43b409b-9fc7-4afa-8eae-e11fe80a9991", "Severity");
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.zDropEdit3.TabIndex = 10;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "IM_GG_Team");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_GG_Team)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.Teams)));
			this.zGuidFindBox2.BindToList = "Lookups+Teams";
			this.zGuidFindBox2.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|e90987a2-044f-41d9-b88f-dce69242437b", "Caused By Team");
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 41, true);
			this.zGuidFindBox2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.zGuidFindBox2.TabIndex = 9;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 1, true);
			this.splitter2.TabIndex = 4;
			this.splitter2.TabStop = false;
			// 
			// FeatureRequestTabPage
			// 
			this.FeatureRequestTabPage.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|64c68c95-2200-485e-88b3-6467fd996d6d", "Feature Request");
			this.FeatureRequestTabPage.Controls.Add(this.splitter1);
			this.FeatureRequestTabPage.Controls.Add(this.featureSplitContainer);
			this.FeatureRequestTabPage.Controls.Add(this.FeatureCustomFieldsPanel);
			this.FeatureRequestTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FeatureRequestTabPage.Name = "FeatureRequestTabPage";
			this.FeatureRequestTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FeatureRequestTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 586, true);
			this.FeatureRequestTabPage.TabIndex = 4;
			// 
			// featureSplitContainer
			// 
			this.featureSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.featureSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 384, true);
			this.featureSplitContainer.Name = "featureSplitContainer";
			this.featureSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// featureSplitContainer.Panel1
			// 
			this.featureSplitContainer.Panel1.Controls.Add(this.featureAnalysisGroupBox);
			// 
			// featureSplitContainer.Panel2
			// 
			this.featureSplitContainer.Panel2.Controls.Add(this.featureWorkItemsGroupBox);
			this.featureSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 199, true);
			this.featureSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			this.featureSplitContainer.TabIndex = 4;
			// 
			// featureAnalysisGroupBox
			// 
			this.featureAnalysisGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|6d74a057-f768-4483-8cd9-03ccb9dd573a", "Feature Request Analysis");
			this.featureAnalysisGroupBox.Controls.Add(this.zTabControl1);
			this.featureAnalysisGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.featureAnalysisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.featureAnalysisGroupBox.Name = "featureAnalysisGroupBox";
			this.featureAnalysisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 128, true);
			this.featureAnalysisGroupBox.TabIndex = 1;
			this.featureAnalysisGroupBox.TabStop = false;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.featureRequestAnalysisPage);
			this.zTabControl1.Controls.Add(this.zTabPage2);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 109, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// featureRequestAnalysisPage
			// 
			this.featureRequestAnalysisPage.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|d0c21ad8-fa1a-4b25-adc0-1bc8ae3424b6", "Project Analysis");
			this.featureRequestAnalysisPage.Controls.Add(this.ProjectRelatedIncidentNoteTabControl);
			this.featureRequestAnalysisPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.featureRequestAnalysisPage.Name = "featureRequestAnalysisPage";
			this.featureRequestAnalysisPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.featureRequestAnalysisPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 82, true);
			this.featureRequestAnalysisPage.TabIndex = 1;
			// 
			// ProjectRelatedIncidentNoteTabControl
			// 
			this.ProjectRelatedIncidentNoteTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ProjectRelatedIncidentNoteTabControl.Controls.Add(this.zTabPage5);
			this.ProjectRelatedIncidentNoteTabControl.Controls.Add(this.zTabPage1);
			this.ProjectRelatedIncidentNoteTabControl.Controls.Add(this.zTabPage6);
			this.ProjectRelatedIncidentNoteTabControl.Controls.Add(this.zTabPage7);
			this.ProjectRelatedIncidentNoteTabControl.Controls.Add(this.zTabPage8);
			this.ProjectRelatedIncidentNoteTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectRelatedIncidentNoteTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProjectRelatedIncidentNoteTabControl.Name = "ProjectRelatedIncidentNoteTabControl";
			this.ProjectRelatedIncidentNoteTabControl.SelectedIndex = 0;
			this.ProjectRelatedIncidentNoteTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1158, 76, true);
			this.ProjectRelatedIncidentNoteTabControl.TabIndex = 0;
			// 
			// zTabPage5
			// 
			this.zTabPage5.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|20f9aaaf-4d15-440f-bef9-f739a9324044", "Business Requirement");
			this.zTabPage5.Controls.Add(this.BusinessRequirementRichTextBox);
			this.zTabPage5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage5.Name = "zTabPage5";
			this.zTabPage5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.zTabPage5.TabIndex = 0;
			// 
			// BusinessRequirementRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.BusinessRequirementRichTextBox, "BusinessRequirementsAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).BusinessRequirementsAsBlob)));
			this.BusinessRequirementRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BusinessRequirementRichTextBox, false);
			this.BusinessRequirementRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BusinessRequirementRichTextBox.MaxLength = 10000000;
			this.BusinessRequirementRichTextBox.Name = "BusinessRequirementRichTextBox";
			this.BusinessRequirementRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.BusinessRequirementRichTextBox.TabIndex = 0;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = ZClientEDI.Res.GetData("1c6b283c-7429-4c28-a477-bceea0049933", "Suggested Software Change");
			this.zTabPage1.Controls.Add(this.SoftwareChangeRichTextBox);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.zTabPage1.TabIndex = 4;
			this.zTabPage1.Text = "Suggested Software Change";
			// 
			// SoftwareChangeRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.SoftwareChangeRichTextBox, "SoftwareChangeAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).SoftwareChangeAsBlob)));
			this.SoftwareChangeRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SoftwareChangeRichTextBox, false);
			this.SoftwareChangeRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SoftwareChangeRichTextBox.MaxLength = 10000000;
			this.SoftwareChangeRichTextBox.Name = "SoftwareChangeRichTextBox";
			this.SoftwareChangeRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.SoftwareChangeRichTextBox.TabIndex = 1;
			// 
			// zTabPage6
			// 
			this.zTabPage6.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|9e945bbe-56f4-483b-9140-0d1f17b47044", "Technical Specification");
			this.zTabPage6.Controls.Add(this.TechnicalSpecificationRichTextBox);
			this.zTabPage6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage6.Name = "zTabPage6";
			this.zTabPage6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.zTabPage6.TabIndex = 1;
			// 
			// TechnicalSpecificationRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.TechnicalSpecificationRichTextBox, "TechnicalSpecificationAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).TechnicalSpecificationAsBlob)));
			this.TechnicalSpecificationRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TechnicalSpecificationRichTextBox, false);
			this.TechnicalSpecificationRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TechnicalSpecificationRichTextBox.MaxLength = 10000000;
			this.TechnicalSpecificationRichTextBox.Name = "TechnicalSpecificationRichTextBox";
			this.TechnicalSpecificationRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.TechnicalSpecificationRichTextBox.TabIndex = 0;
			// 
			// zTabPage7
			// 
			this.zTabPage7.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|9cf585a6-4f3e-4260-b746-5ba485806a3e", "Feature Request Pre-requisites");
			this.zTabPage7.Controls.Add(this.FeatureRequestPrerequisitesRichTextBox);
			this.zTabPage7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage7.Name = "zTabPage7";
			this.zTabPage7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.zTabPage7.TabIndex = 2;
			// 
			// FeatureRequestPrerequisitesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.FeatureRequestPrerequisitesRichTextBox, "FeatureRequestPrerequisitesAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).FeatureRequestPrerequisitesAsBlob)));
			this.FeatureRequestPrerequisitesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FeatureRequestPrerequisitesRichTextBox, false);
			this.FeatureRequestPrerequisitesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeatureRequestPrerequisitesRichTextBox.MaxLength = 10000000;
			this.FeatureRequestPrerequisitesRichTextBox.Name = "FeatureRequestPrerequisitesRichTextBox";
			this.FeatureRequestPrerequisitesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.FeatureRequestPrerequisitesRichTextBox.TabIndex = 0;
			// 
			// zTabPage8
			// 
			this.zTabPage8.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|8e8817b7-367b-4ba4-9a74-8260e01e25a6", "Internal Notes");
			this.zTabPage8.Controls.Add(this.InternalNotesRichTextBox);
			this.zTabPage8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage8.Name = "zTabPage8";
			this.zTabPage8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.zTabPage8.TabIndex = 3;
			// 
			// InternalNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalNotesRichTextBox, "FeatureRequestInternalNoteAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).FeatureRequestInternalNoteAsBlob)));
			this.InternalNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InternalNotesRichTextBox, false);
			this.InternalNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InternalNotesRichTextBox.MaxLength = 10000000;
			this.InternalNotesRichTextBox.Name = "InternalNotesRichTextBox";
			this.InternalNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 49, true);
			this.InternalNotesRichTextBox.TabIndex = 0;
			// 
			// zTabPage2
			// 
			this.zTabPage2.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|a9ce3cf6-e076-4fe2-bb5b-7df75660b434", "Related Feature Requests");
			this.zTabPage2.Controls.Add(this.RelatedFeatureRequestsGrid);
			this.zTabPage2.Controls.Add(this.zPanel3);
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 82, true);
			this.zTabPage2.TabIndex = 2;
			// 
			// RelatedFeatureRequestsGrid
			// 
			this.RelatedFeatureRequestsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedFeatureRequestsGrid, "RelatedFeatureRequests");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).RelatedFeatureRequests)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.FeatureRequests)));
			this.RelatedFeatureRequestsGrid.BindToFindBoxList = "Lookups+FeatureRequests";
			zTextBoxColumnStyleInfo8.ColumnName = "IM_IncidentNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "IM_Description";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo10.CaptionResourceString = ZClientEDI.Res.GetData("ZModuleButtonGrid|7ac329f2-a52e-4498-aded-3f1fac3c5e16", "Overall Status");
			zTextBoxColumnStyleInfo10.ColumnName = "IM_Status";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = ZClientEDI.Res.GetData("ZModuleButtonGrid|981c531e-784e-4bae-b2be-a33af6f01c28", "Disposition");
			zTextBoxColumnStyleInfo11.ColumnName = "IM_ResolutionCodeDescription";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.CaptionResourceString = ZClientEDI.Res.GetData("ZModuleButtonGrid|37413b2b-cfff-4bc0-b2f9-15c834837dfd", "Assigned");
			zTextBoxColumnStyleInfo12.ColumnName = "OverallAssignedToCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedFeatureRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.RelatedFeatureRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.RelatedFeatureRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.RelatedFeatureRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.RelatedFeatureRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.RelatedFeatureRequestsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedFeatureRequestsGrid.GridId = "dd4f7068-f3c0-4889-849d-55b80816de5c";
			// 
			// 
			// 
			this.RelatedFeatureRequestsGrid.InnerGrid.AllowNavigation = false;
			this.RelatedFeatureRequestsGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedFeatureRequestsGrid.InnerGrid.CaptionVisible = false;
			this.RelatedFeatureRequestsGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.RelatedFeatureRequestsGrid.InnerGrid.GridId = null;
			this.RelatedFeatureRequestsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedFeatureRequestsGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.RelatedFeatureRequestsGrid.InnerGrid.LayoutKey = "Grid";
			this.RelatedFeatureRequestsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RelatedFeatureRequestsGrid.InnerGrid.Name = "Grid";
			this.RelatedFeatureRequestsGrid.InnerGrid.ReadOnly = true;
			this.RelatedFeatureRequestsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1152, 9, true);
			this.RelatedFeatureRequestsGrid.InnerGrid.TabIndex = 0;
			this.RelatedFeatureRequestsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 32, true);
			this.RelatedFeatureRequestsGrid.Name = "RelatedFeatureRequestsGrid";
			this.RelatedFeatureRequestsGrid.NameOfAGridElement = ZClientEDI.Res.GetData("6A7B0B95-6CA4-448B-8CAD-7AF07656F995", "Feature Request");
			this.RelatedFeatureRequestsGrid.ReadOnly = true;
			this.RelatedFeatureRequestsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1158, 47, true);
			this.RelatedFeatureRequestsGrid.TabIndex = 19;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.ParentFeatureRequestFindBox);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1158, 29, true);
			this.zPanel3.TabIndex = 18;
			// 
			// ParentFeatureRequestFindBox
			// 
			this.ParentFeatureRequestFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentFeatureRequestFindBox, "ParentFeatureRequestPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ParentFeatureRequestPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.FeatureRequests)));
			this.ParentFeatureRequestFindBox.BindToList = "Lookups+FeatureRequests";
			this.ParentFeatureRequestFindBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|0a451384-cf79-4646-945b-2bb56cf21969", "Parent Feature Request");
			this.ParentFeatureRequestFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 3, true);
			this.ParentFeatureRequestFindBox.Name = "ParentFeatureRequestFindBox";
			this.ParentFeatureRequestFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 20, true);
			this.ParentFeatureRequestFindBox.TabIndex = 17;
			// 
			// featureWorkItemsGroupBox
			// 
			this.featureWorkItemsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|2c7151f2-09ca-4de7-9457-77a5f77258b4", "Work Item Details");
			this.featureWorkItemsGroupBox.Controls.Add(this.FeatureRequestWorkItemsGrid);
			this.featureWorkItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.featureWorkItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.featureWorkItemsGroupBox.Name = "featureWorkItemsGroupBox";
			this.featureWorkItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 67, true);
			this.featureWorkItemsGroupBox.TabIndex = 3;
			this.featureWorkItemsGroupBox.TabStop = false;
			// 
			// FeatureRequestWorkItemsGrid
			// 
			this.FeatureRequestWorkItemsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FeatureRequestWorkItemsGrid, "RelatedWorkItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).RelatedWorkItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.WorkItems)));
			this.FeatureRequestWorkItemsGrid.BindToFindBoxList = "Lookups+WorkItems";
			zTextBoxColumnStyleInfo13.ColumnName = "WKI_WorkItemNumber";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "WKI_Summary";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo15.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|b5431e1b-301e-4ff5-860d-c36ad60dc612", "Type");
			zTextBoxColumnStyleInfo15.ColumnName = "WorkItemType";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo16.ColumnName = "WKI_Status";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|34831ebf-bb47-47c0-bb14-87a4dff2fe38", "Disposition");
			zTextBoxColumnStyleInfo17.ColumnName = "DispositionDescription";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo18.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|3bcfea1e-8b52-458b-ad45-87ff14f3d517", "Assigned");
			zTextBoxColumnStyleInfo18.ColumnName = "AssignedToStaff+GS_Code";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|a285f6ae-a688-462a-9a36-5f05e401a2d6", "Current Task");
			zTextBoxColumnStyleInfo19.ColumnName = "CurrentOrNextTask+P9_Type";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|259b7abf-11f1-4139-85df-1f7f8890b387", "Task Est. Start date");
			zDateEditColumnStyleInfo3.ColumnName = "CurrentOrNextTask+P9_ScheduledDateForBinding";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|7d8374e4-dbdd-483b-9fc6-9099f739cc5c", "Task Act. Start date");
			zDateEditColumnStyleInfo4.ColumnName = "CurrentOrNextTask+P9_ActualDateForBinding";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo3.AllowNegative = false;
			zTimeEditExColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|30761f7e-bea5-4892-8855-ddd2f80ba7de", "Task Est. Duration");
			zTimeEditExColumnStyleInfo3.ColumnName = "CurrentOrNextTask+P9_EstDuration";
			zTimeEditExColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo4.AllowNegative = false;
			zTimeEditExColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("ModuleSelectionControl|cb0e8c89-9658-4414-944f-f9a553a580a7", "Task Act. Duration");
			zTimeEditExColumnStyleInfo4.ColumnName = "CurrentOrNextTask+P9_ActualDuration";
			zTimeEditExColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo3);
			this.FeatureRequestWorkItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo4);
			this.FeatureRequestWorkItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeatureRequestWorkItemsGrid.GridId = "d3e71b5c-b87a-4e37-a4d2-36dfe3a9452c";
			// 
			// 
			// 
			this.FeatureRequestWorkItemsGrid.InnerGrid.AllowNavigation = false;
			this.FeatureRequestWorkItemsGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FeatureRequestWorkItemsGrid.InnerGrid.CaptionVisible = false;
			this.FeatureRequestWorkItemsGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.FeatureRequestWorkItemsGrid.InnerGrid.GridId = null;
			this.FeatureRequestWorkItemsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeatureRequestWorkItemsGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.FeatureRequestWorkItemsGrid.InnerGrid.LayoutKey = "Grid";
			this.FeatureRequestWorkItemsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FeatureRequestWorkItemsGrid.InnerGrid.Name = "Grid";
			this.FeatureRequestWorkItemsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1166, 10, true);
			this.FeatureRequestWorkItemsGrid.InnerGrid.TabIndex = 0;
			this.FeatureRequestWorkItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FeatureRequestWorkItemsGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WorkItem;
			this.FeatureRequestWorkItemsGrid.Name = "FeatureRequestWorkItemsGrid";
			this.FeatureRequestWorkItemsGrid.NameOfAGridElement = ZClientEDI.Res.GetData("A18974E3-20A1-44DC-9995-428FABEB517C", "Work Item");
			this.FeatureRequestWorkItemsGrid.ReadOnly = false;
			this.FeatureRequestWorkItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 48, true);
			this.FeatureRequestWorkItemsGrid.TabIndex = 13;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 381, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 3, true);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// FeatureCustomFieldsPanel
			// 
			this.FeatureCustomFieldsPanel.Controls.Add(this.featureDetailsClientSplitContainer);
			this.FeatureCustomFieldsPanel.Controls.Add(this.CustomFieldsGroupBox);
			this.FeatureCustomFieldsPanel.Controls.Add(this.EstGroupBox);
			this.FeatureCustomFieldsPanel.Controls.Add(this.QteGroupBox);
			this.FeatureCustomFieldsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FeatureCustomFieldsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FeatureCustomFieldsPanel.Name = "FeatureCustomFieldsPanel";
			this.FeatureCustomFieldsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 378, true);
			this.FeatureCustomFieldsPanel.TabIndex = 28;
			// 
			// featureDetailsClientSplitContainer
			// 
			this.featureDetailsClientSplitContainer.Dock = System.Windows.Forms.DockStyle.Left;
			this.featureDetailsClientSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.featureDetailsClientSplitContainer.Name = "featureDetailsClientSplitContainer";
			this.featureDetailsClientSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// featureDetailsClientSplitContainer.Panel1
			// 
			this.featureDetailsClientSplitContainer.Panel1.Controls.Add(this.zGroupBox6);
			// 
			// featureDetailsClientSplitContainer.Panel2
			// 
			this.featureDetailsClientSplitContainer.Panel2.Controls.Add(this.FeatureRequestClientGroupBox);
			this.featureDetailsClientSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 378, true);
			this.featureDetailsClientSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			this.featureDetailsClientSplitContainer.SplitterWidth = 1;
			this.featureDetailsClientSplitContainer.TabIndex = 11;
			// 
			// zGroupBox6
			// 
			this.zGroupBox6.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|cdee01bd-4a81-4dee-8d82-4b79616d0ed2", "Details");
			this.zGroupBox6.Controls.Add(this.zLabel8);
			this.zGroupBox6.Controls.Add(this.zLabel17);
			this.zGroupBox6.Controls.Add(this.ClientSizeDescriptionLabel);
			this.zGroupBox6.Controls.Add(this.ClientSizeLabel);
			this.zGroupBox6.Controls.Add(this.EstimatedDateLabel);
			this.zGroupBox6.Controls.Add(this.EstimatedDateCaption);
			this.zGroupBox6.Controls.Add(this.businessConsultantCodeFindBox);
			this.zGroupBox6.Controls.Add(this.relatedProjectGuidFindBox);
			this.zGroupBox6.Controls.Add(this.zTextBox7);
			this.zGroupBox6.Controls.Add(this.zLabel48);
			this.zGroupBox6.Controls.Add(this.zLabel49);
			this.zGroupBox6.Controls.Add(this.OverallAssignedToLabel);
			this.zGroupBox6.Controls.Add(this.zLabel51);
			this.zGroupBox6.Controls.Add(this.zLabel52);
			this.zGroupBox6.Controls.Add(this.zLabel53);
			this.zGroupBox6.Controls.Add(this.zLabel54);
			this.zGroupBox6.Controls.Add(this.ClientRequiredDate);
			this.zGroupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox6.Name = "zGroupBox6";
			this.zGroupBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 270, true);
			this.zGroupBox6.TabIndex = 0;
			this.zGroupBox6.TabStop = false;
			// 
			// zLabel8
			// 
			this.zLabel8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel8, "CurrentTask+P9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).CurrentTask.P9_Description)));
			this.zLabel8.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel8, false);
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 63, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.zLabel8.TabIndex = 27;
			this.zLabel8.Text = "<Current Task Description>";
			// 
			// zLabel17
			// 
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 63, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.zLabel17.TabIndex = 26;
			this.zLabel17.Text = "Current Task:";
			this.zLabel17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ClientSizeDescriptionLabel
			// 
			this.ClientSizeDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientSizeDescriptionLabel, "ClientSizeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientSizeDescription)));
			this.ClientSizeDescriptionLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientSizeDescriptionLabel, false);
			this.ClientSizeDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 121, true);
			this.ClientSizeDescriptionLabel.Name = "ClientSizeDescriptionLabel";
			this.ClientSizeDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.ClientSizeDescriptionLabel.TabIndex = 18;
			this.ClientSizeDescriptionLabel.Text = "<ClientSize>";
			// 
			// ClientSizeLabel
			// 
			this.ClientSizeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 121, true);
			this.ClientSizeLabel.Name = "ClientSizeLabel";
			this.ClientSizeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.ClientSizeLabel.TabIndex = 17;
			this.ClientSizeLabel.Text = "Client Size:";
			this.ClientSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// EstimatedDateLabel
			// 
			this.BindingSource.SetBindingMember(this.EstimatedDateLabel, "CurrentTaskEstimatedDateAsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).CurrentTaskEstimatedDateAsText)));
			this.EstimatedDateLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EstimatedDateLabel, false);
			this.EstimatedDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 97, true);
			this.EstimatedDateLabel.Name = "EstimatedDateLabel";
			this.EstimatedDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 19, true);
			this.EstimatedDateLabel.TabIndex = 16;
			this.EstimatedDateLabel.Text = "<Current Task Estimated Date>";
			// 
			// EstimatedDateCaption
			// 
			this.EstimatedDateCaption.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|ffb47ddb-be25-4c2e-9845-cddeb6a12f3a", "Current Task Estimated Date:");
			this.EstimatedDateCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 83, true);
			this.EstimatedDateCaption.Name = "EstimatedDateCaption";
			this.EstimatedDateCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 35, true);
			this.EstimatedDateCaption.TabIndex = 15;
			this.EstimatedDateCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// businessConsultantCodeFindBox
			// 
			this.businessConsultantCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.businessConsultantCodeFindBox, "IM_GS_NKSpecifiedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_GS_NKSpecifiedBy)));
			this.businessConsultantCodeFindBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|1ed633ef-6f2a-414b-80ae-1db86ba7cd4c", "Business Consultant");
			this.businessConsultantCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 268, true);
			this.businessConsultantCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.businessConsultantCodeFindBox.Name = "businessConsultantCodeFindBox";
			this.businessConsultantCodeFindBox.PreBoundMaxLength = 3;
			this.businessConsultantCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 20, true);
			this.businessConsultantCodeFindBox.TabIndex = 9;
			// 
			// relatedProjectGuidFindBox
			// 
			this.relatedProjectGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relatedProjectGuidFindBox, "RelatedProjectPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).RelatedProjectPK)));
			this.relatedProjectGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|28cf6dea-4cdd-478f-a495-a5d93c947d92", "Project");
			this.relatedProjectGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 244, true);
			this.relatedProjectGuidFindBox.Name = "relatedProjectGuidFindBox";
			this.relatedProjectGuidFindBox.PreBoundMaxLength = 10;
			this.relatedProjectGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 20, true);
			this.relatedProjectGuidFindBox.TabIndex = 8;
			// 
			// zTextBox7
			// 
			this.zTextBox7.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox7, "IM_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Description)));
			this.zTextBox7.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|cdbdafcb-7c31-48c3-81f3-db8f581a396a", "Summary");
			this.zTextBox7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 196, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 20, true);
			this.zTextBox7.TabIndex = 2;
			// 
			// zLabel48
			// 
			this.BindingSource.SetBindingMember(this.zLabel48, "OverallAssignedToCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToCode)));
			this.zLabel48.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel48, false);
			this.zLabel48.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 171, true);
			this.zLabel48.Name = "zLabel48";
			this.zLabel48.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.zLabel48.TabIndex = 11;
			this.zLabel48.Text = "WWW";
			// 
			// zLabel49
			// 
			this.BindingSource.SetBindingMember(this.zLabel49, "OverallAssignedToDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToDescription)));
			this.zLabel49.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel49, false);
			this.zLabel49.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 171, true);
			this.zLabel49.Name = "zLabel49";
			this.zLabel49.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.zLabel49.TabIndex = 12;
			this.zLabel49.Text = "<Staff Fullname>";
			// 
			// OverallAssignedToLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallAssignedToLabel, "OverallAssignedToLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToLabelText)));
			this.OverallAssignedToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 171, true);
			this.OverallAssignedToLabel.Name = "OverallAssignedToLabel";
			this.OverallAssignedToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.OverallAssignedToLabel.TabIndex = 10;
			this.OverallAssignedToLabel.Text = "<AssignedToLabel>";
			this.OverallAssignedToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel51
			// 
			this.zLabel51.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zLabel51, "IM_ResolutionCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ResolutionCodeDescription)));
			this.zLabel51.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel51, false);
			this.zLabel51.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 40, true);
			this.zLabel51.Name = "zLabel51";
			this.zLabel51.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 18, true);
			this.zLabel51.TabIndex = 14;
			this.zLabel51.Text = "<Disposition>";
			// 
			// zLabel52
			// 
			this.BindingSource.SetBindingMember(this.zLabel52, "IM_StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_StatusDescription)));
			this.zLabel52.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel52, false);
			this.zLabel52.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 147, true);
			this.zLabel52.Name = "zLabel52";
			this.zLabel52.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.zLabel52.TabIndex = 9;
			this.zLabel52.Text = "<Status>";
			// 
			// zLabel53
			// 
			this.zLabel53.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|8de61b29-4077-4c00-bab0-120e844fc218", "Disposition:");
			this.zLabel53.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 39, true);
			this.zLabel53.Name = "zLabel53";
			this.zLabel53.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zLabel53.TabIndex = 13;
			this.zLabel53.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel54
			// 
			this.zLabel54.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|150bf270-ef14-4ef9-b976-47ec6dbae1bb", "Status:");
			this.zLabel54.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 147, true);
			this.zLabel54.Name = "zLabel54";
			this.zLabel54.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zLabel54.TabIndex = 9;
			this.zLabel54.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ClientRequiredDate
			// 
			this.ClientRequiredDate.AllowDrop = true;
			this.ClientRequiredDate.AutoCompleteMonthThreshold = 1;
			this.ClientRequiredDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ClientRequiredDate, "IM_RequiredBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_RequiredBy)));
			this.ClientRequiredDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 220, true);
			this.ClientRequiredDate.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|B90CD5C3-2649-4E3B-B570-C519956066F8", "Client Required Date");
			this.ClientRequiredDate.Name = "ClientRequiredDate";
			this.ClientRequiredDate.TabIndex = 7;
			// 
			// FeatureRequestClientGroupBox
			// 
			this.FeatureRequestClientGroupBox.Controls.Add(this.FeatureRequestClientRegType);
			this.FeatureRequestClientGroupBox.Controls.Add(this.FeatureRequestClientRegNo);
			this.FeatureRequestClientGroupBox.Controls.Add(this.WorkplaceCaption);
			this.FeatureRequestClientGroupBox.Controls.Add(this.WorkplaceLabel);
			this.FeatureRequestClientGroupBox.Controls.Add(this.FeatureRequestClientAddressControl);
			this.FeatureRequestClientGroupBox.Controls.Add(this.FeatureRequestContactFindBox);
			this.FeatureRequestClientGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeatureRequestClientGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeatureRequestClientGroupBox.Name = "FeatureRequestClientGroupBox";
			this.FeatureRequestClientGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 107, true);
			this.FeatureRequestClientGroupBox.TabIndex = 1;
			this.FeatureRequestClientGroupBox.TabStop = false;
			this.FeatureRequestClientGroupBox.Text = "Organisation";
			// 
			// FeatureRequestClientRegType
			// 
			this.BindingSource.SetBindingMember(this.FeatureRequestClientRegType, "FeatureRequestClientRegType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).FeatureRequestClientRegType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FeatureRequestClientRegType, false);
			this.FeatureRequestClientRegType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 81, true);
			this.FeatureRequestClientRegType.Name = "FeatureRequestClientRegType";
			this.FeatureRequestClientRegType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.FeatureRequestClientRegType.TabIndex = 10;
			this.FeatureRequestClientRegType.Text = "<RegType>";
			this.FeatureRequestClientRegType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FeatureRequestClientRegNo
			// 
			this.BindingSource.SetBindingMember(this.FeatureRequestClientRegNo, "FeatureRequestClientRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).FeatureRequestClientRegNo)));
			this.FeatureRequestClientRegNo.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FeatureRequestClientRegNo, false);
			this.FeatureRequestClientRegNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 81, true);
			this.FeatureRequestClientRegNo.Name = "FeatureRequestClientRegNo";
			this.FeatureRequestClientRegNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 18, true);
			this.FeatureRequestClientRegNo.TabIndex = 10;
			this.FeatureRequestClientRegNo.Text = "<ClientRegNo>";
			// 
			// WorkplaceLabel
			// 
			this.BindingSource.SetBindingMember(this.WorkplaceLabel, "FeatureRequestContactPrimaryWorkplace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ContactPrimaryWorkplace)));
			this.WorkplaceLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WorkplaceLabel, false);
			this.WorkplaceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 62, true);
			this.WorkplaceLabel.Name = "WorkplaceLabel";
			this.WorkplaceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 18, true);
			this.WorkplaceLabel.TabIndex = 10;
			// 
			// WorkplaceCaption
			// 
			this.WorkplaceCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 62, true);
			this.WorkplaceCaption.Name = "WorkplaceCaption";
			this.WorkplaceCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.WorkplaceCaption.TabIndex = 10;
			this.WorkplaceCaption.Text = "Workplace:";
			this.WorkplaceCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FeatureRequestClientAddressControl
			// 
			this.FeatureRequestClientAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FeatureRequestClientAddressControl, "FeatureRequestClientAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).FeatureRequestClientAddressPK)));
			this.FeatureRequestClientAddressControl.BindToOrgList = "Lookups.Clients";
			this.FeatureRequestClientAddressControl.CaptionResourceString = ZClientEDI.Res.GetData("a90c00d9-f43f-4213-9b9b-0fbbe56dd9a6", "Organization");
			this.FeatureRequestClientAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.FeatureRequestClientAddressControl.Name = "FeatureRequestClientAddressControl";
			this.FeatureRequestClientAddressControl.PopupCaption = "";
			this.FeatureRequestClientAddressControl.ShowAddress = false;
			this.FeatureRequestClientAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.FeatureRequestClientAddressControl.TabIndex = 8;
			// 
			// FeatureRequestContactFindBox
			// 
			this.FeatureRequestContactFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FeatureRequestContactFindBox, "FeatureRequestContactPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).FeatureRequestContactPK)));
			this.FeatureRequestContactFindBox.CaptionResourceString = ZClientEDI.Res.GetData("c1fb9aca-53a7-4cef-82d8-83b119f32211", "Contact");
			this.FeatureRequestContactFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 38, true);
			this.FeatureRequestContactFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.FeatureRequestContactFindBox.Name = "FeatureRequestContactFindBox";
			this.FeatureRequestContactFindBox.PopupCaption = "Select the Contact";
			this.FeatureRequestContactFindBox.PreBoundMaxLength = 25;
			this.FeatureRequestContactFindBox.ShowDescriptionBox = false;
			this.FeatureRequestContactFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.FeatureRequestContactFindBox.TabIndex = 9;
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CustomFieldsGroupBox.Controls.Add(this.CustomFieldsControl);
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(991, 0, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 378, true);
			this.CustomFieldsGroupBox.TabIndex = 1;
			this.CustomFieldsGroupBox.TabStop = false;
			this.CustomFieldsGroupBox.Text = "Custom Fields";
			// 
			// CustomFieldsControl
			// 
			this.CustomFieldsControl.AllowDrop = true;
			this.CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.NothingSetupMessageLabelText = "To make use of this tab, please setup incident custom fields in Workflow Manager." +
	"";
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 359, true);
			this.CustomFieldsControl.TabIndex = 1;
			// 
			// EstGroupBox
			// 
			this.EstGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.EstGroupBox.Controls.Add(this.EstPaymentTermsDropEdit);
			this.EstGroupBox.Controls.Add(this.EstRequestDate);
			this.EstGroupBox.Controls.Add(this.EstExpiryDate);
			this.EstGroupBox.Controls.Add(this.EstSentDate);
			this.EstGroupBox.Controls.Add(this.EstExpressDelCutOff);
			this.EstGroupBox.Controls.Add(this.EstCancellationFee);
			this.EstGroupBox.Controls.Add(this.EstMaxOneOff);
			this.EstGroupBox.Controls.Add(this.EstMinOneOff);
			this.EstGroupBox.Controls.Add(this.EstMaxMonthly);
			this.EstGroupBox.Controls.Add(this.EstMinMonthly);
			this.EstGroupBox.Controls.Add(this.EstCurrencyBox);
			this.EstGroupBox.Controls.Add(this.EstMaxDevHours);
			this.EstGroupBox.Controls.Add(this.EstMinDevHours);
			this.EstGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 0, true);
			this.EstGroupBox.Name = "EstGroupBox";
			this.EstGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 378, true);
			this.EstGroupBox.TabIndex = 1;
			this.EstGroupBox.TabStop = false;
			this.EstGroupBox.Text = "Estimate";
			// 
			// EstPaymentTermsDropEdit
			// 
			this.EstPaymentTermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EstPaymentTermsDropEdit, "Estimate.CIE_PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_PaymentTerms)));
			this.EstPaymentTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 146, true);
			this.EstPaymentTermsDropEdit.MaxItemsToShowInDropDown = 20;
			this.EstPaymentTermsDropEdit.Name = "EstPaymentTermsDropEdit";
			this.EstPaymentTermsDropEdit.PreBoundMaxLength = 3;
			this.EstPaymentTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.EstPaymentTermsDropEdit.TabIndex = 7;
			// 
			// EstRequestDate
			// 
			this.EstRequestDate.AllowDrop = true;
			this.EstRequestDate.AutoCompleteMonthThreshold = 1;
			this.EstRequestDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstRequestDate, "Estimate.CIE_QuoteRequestedLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_QuoteRequestedLocal)));
			this.EstRequestDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 120, true);
			this.EstRequestDate.Name = "EstRequestDate";
			this.EstRequestDate.TabIndex = 6;
			// 
			// EstExpiryDate
			// 
			this.EstExpiryDate.AllowDrop = true;
			this.EstExpiryDate.AutoCompleteMonthThreshold = 1;
			this.EstExpiryDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstExpiryDate, "Estimate.CIE_EstimateExpiryDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_EstimateExpiryDateLocal)));
			this.EstExpiryDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 94, true);
			this.EstExpiryDate.Name = "EstExpiryDate";
			this.EstExpiryDate.TabIndex = 5;
			// 
			// EstSentDate
			// 
			this.EstSentDate.AllowDrop = true;
			this.EstSentDate.AutoCompleteMonthThreshold = 1;
			this.EstSentDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstSentDate, "Estimate.CIE_EstimateSentDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_EstimateSentDateLocal)));
			this.EstSentDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 68, true);
			this.EstSentDate.Name = "EstSentDate";
			this.EstSentDate.TabIndex = 4;
			// 
			// EstExpressDelCutOff
			// 
			this.EstExpressDelCutOff.AllowDrop = true;
			this.EstExpressDelCutOff.AutoCompleteMonthThreshold = 1;
			this.EstExpressDelCutOff.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstExpressDelCutOff, "Estimate.CIE_ExpressDeliveryOptionCutOffDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_ExpressDeliveryOptionCutOffDateLocal)));
			this.EstExpressDelCutOff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 327, true);
			this.EstExpressDelCutOff.Name = "EstExpressDelCutOff";
			this.EstExpressDelCutOff.TabIndex = 14;
			// 
			// EstCancellationFee
			// 
			this.BindingSource.SetBindingMember(this.EstCancellationFee, "Estimate.CIE_CancellationFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_CancellationFee)));
			this.EstCancellationFee.DecimalPlaces = 2;
			this.EstCancellationFee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 301, true);
			this.EstCancellationFee.Name = "EstCancellationFee";
			this.EstCancellationFee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.EstCancellationFee.TabIndex = 13;
			this.EstCancellationFee.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstMaxOneOff
			// 
			this.BindingSource.SetBindingMember(this.EstMaxOneOff, "Estimate.CIE_MaxEstimateOneoff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_MaxEstimateOneoff)));
			this.EstMaxOneOff.DecimalPlaces = 2;
			this.EstMaxOneOff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 275, true);
			this.EstMaxOneOff.Name = "EstMaxOneOff";
			this.EstMaxOneOff.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.EstMaxOneOff.TabIndex = 12;
			this.EstMaxOneOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstMinOneOff
			// 
			this.BindingSource.SetBindingMember(this.EstMinOneOff, "Estimate.CIE_MinEstimateOneoff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_MinEstimateOneoff)));
			this.EstMinOneOff.DecimalPlaces = 2;
			this.EstMinOneOff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 249, true);
			this.EstMinOneOff.Name = "EstMinOneOff";
			this.EstMinOneOff.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.EstMinOneOff.TabIndex = 11;
			this.EstMinOneOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstMaxMonthly
			// 
			this.BindingSource.SetBindingMember(this.EstMaxMonthly, "Estimate.CIE_MaxEstimateMonthly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_MaxEstimateMonthly)));
			this.EstMaxMonthly.DecimalPlaces = 2;
			this.EstMaxMonthly.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 223, true);
			this.EstMaxMonthly.Name = "EstMaxMonthly";
			this.EstMaxMonthly.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.EstMaxMonthly.TabIndex = 10;
			this.EstMaxMonthly.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstMinMonthly
			// 
			this.BindingSource.SetBindingMember(this.EstMinMonthly, "Estimate.CIE_MinEstimateMonthly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_MinEstimateMonthly)));
			this.EstMinMonthly.DecimalPlaces = 2;
			this.EstMinMonthly.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 197, true);
			this.EstMinMonthly.Name = "EstMinMonthly";
			this.EstMinMonthly.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.EstMinMonthly.TabIndex = 9;
			this.EstMinMonthly.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstCurrencyBox
			// 
			this.EstCurrencyBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EstCurrencyBox, "Estimate.CIE_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_RX_NKCurrency)));
			this.EstCurrencyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 171, true);
			this.EstCurrencyBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.EstCurrencyBox.Name = "EstCurrencyBox";
			this.EstCurrencyBox.PreBoundMaxLength = 3;
			this.EstCurrencyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.EstCurrencyBox.TabIndex = 8;
			// 
			// EstMaxDevHours
			// 
			this.BindingSource.SetBindingMember(this.EstMaxDevHours, "Estimate.CIE_MaxDevelopmentHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_MaxDevelopmentHours)));
			this.EstMaxDevHours.DecimalPlaces = 2;
			this.EstMaxDevHours.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 42, true);
			this.EstMaxDevHours.Name = "EstMaxDevHours";
			this.EstMaxDevHours.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.EstMaxDevHours.TabIndex = 3;
			this.EstMaxDevHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstMinDevHours
			// 
			this.BindingSource.SetBindingMember(this.EstMinDevHours, "Estimate.CIE_MinDevelopmentHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Estimate.CIE_MinDevelopmentHours)));
			this.EstMinDevHours.DecimalPlaces = 2;
			this.EstMinDevHours.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 16, true);
			this.EstMinDevHours.Name = "EstMinDevHours";
			this.EstMinDevHours.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.EstMinDevHours.TabIndex = 2;
			this.EstMinDevHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QteGroupBox
			// 
			this.QteGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.QteGroupBox.Controls.Add(this.QteCancellationFee);
			this.QteGroupBox.Controls.Add(this.QtePaymentTypeDropEdit);
			this.QteGroupBox.Controls.Add(this.QtePaymentTermsDropEdit);
			this.QteGroupBox.Controls.Add(this.QteDateDelivered);
			this.QteGroupBox.Controls.Add(this.QteAcceptedDate);
			this.QteGroupBox.Controls.Add(this.QteExpiryDate);
			this.QteGroupBox.Controls.Add(this.QteSentDate);
			this.QteGroupBox.Controls.Add(this.QteExpressDeliveryIncludedCheckBox);
			this.QteGroupBox.Controls.Add(this.QteHeadStartSurcharge);
			this.QteGroupBox.Controls.Add(this.QteHeadStartIncludedCheckBox);
			this.QteGroupBox.Controls.Add(this.QteOneOffUpfront);
			this.QteGroupBox.Controls.Add(this.QteAmount);
			this.QteGroupBox.Controls.Add(this.QteExpressDeliverySurcharge);
			this.QteGroupBox.Controls.Add(this.QteCurrencyBox);
			this.QteGroupBox.Controls.Add(this.QteMaxDevHours);
			this.QteGroupBox.Controls.Add(this.QteMinDevHours);
			this.QteGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 0, true);
			this.QteGroupBox.Name = "QteGroupBox";
			this.QteGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 378, true);
			this.QteGroupBox.TabIndex = 1;
			this.QteGroupBox.TabStop = false;
			this.QteGroupBox.Text = "Quote";
			// 
			// QteCancellationFee
			// 
			this.BindingSource.SetBindingMember(this.QteCancellationFee, "Quote.CIQ_CancellationFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_CancellationFee)));
			this.QteCancellationFee.DecimalPlaces = 2;
			this.QteCancellationFee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 301, true);
			this.QteCancellationFee.Name = "QteCancellationFee";
			this.QteCancellationFee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.QteCancellationFee.TabIndex = 13;
			this.QteCancellationFee.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QtePaymentTypeDropEdit
			// 
			this.QtePaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QtePaymentTypeDropEdit, "Quote.CIQ_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_Type)));
			this.QtePaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 171, true);
			this.QtePaymentTypeDropEdit.MaxItemsToShowInDropDown = 20;
			this.QtePaymentTypeDropEdit.Name = "QtePaymentTypeDropEdit";
			this.QtePaymentTypeDropEdit.PreBoundMaxLength = 3;
			this.QtePaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.QtePaymentTypeDropEdit.TabIndex = 8;
			// 
			// QtePaymentTermsDropEdit
			// 
			this.QtePaymentTermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QtePaymentTermsDropEdit, "Quote.CIQ_PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_PaymentTerms)));
			this.QtePaymentTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 197, true);
			this.QtePaymentTermsDropEdit.MaxItemsToShowInDropDown = 20;
			this.QtePaymentTermsDropEdit.Name = "QtePaymentTermsDropEdit";
			this.QtePaymentTermsDropEdit.PreBoundMaxLength = 3;
			this.QtePaymentTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.QtePaymentTermsDropEdit.TabIndex = 9;
			// 
			// QteDateDelivered
			// 
			this.QteDateDelivered.AllowDrop = true;
			this.QteDateDelivered.AutoCompleteMonthThreshold = 1;
			this.QteDateDelivered.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QteDateDelivered, "Quote.CIQ_DeliveredDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_DeliveredDateLocal)));
			this.QteDateDelivered.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 146, true);
			this.QteDateDelivered.Name = "QteDateDelivered";
			this.QteDateDelivered.TabIndex = 7;
			// 
			// QteAcceptedDate
			// 
			this.QteAcceptedDate.AllowDrop = true;
			this.QteAcceptedDate.AutoCompleteMonthThreshold = 1;
			this.QteAcceptedDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QteAcceptedDate, "Quote.CIQ_QuoteAcceptedDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_QuoteAcceptedDateLocal)));
			this.QteAcceptedDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 120, true);
			this.QteAcceptedDate.Name = "QteAcceptedDate";
			this.QteAcceptedDate.TabIndex = 6;
			// 
			// QteExpiryDate
			// 
			this.QteExpiryDate.AllowDrop = true;
			this.QteExpiryDate.AutoCompleteMonthThreshold = 1;
			this.QteExpiryDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QteExpiryDate, "Quote.CIQ_QuoteExpiryDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_QuoteExpiryDateLocal)));
			this.QteExpiryDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 94, true);
			this.QteExpiryDate.Name = "QteExpiryDate";
			this.QteExpiryDate.TabIndex = 5;
			// 
			// QteSentDate
			// 
			this.QteSentDate.AllowDrop = true;
			this.QteSentDate.AutoCompleteMonthThreshold = 1;
			this.QteSentDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QteSentDate, "Quote.CIQ_QuoteSentDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_QuoteSentDateLocal)));
			this.QteSentDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 68, true);
			this.QteSentDate.Name = "QteSentDate";
			this.QteSentDate.TabIndex = 4;
			// 
			// QteExpressDeliveryIncludedCheckBox
			// 
			this.QteExpressDeliveryIncludedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.QteExpressDeliveryIncludedCheckBox, "Quote.CIQ_ExpressDeliveryOptionIncluded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_ExpressDeliveryOptionIncluded)));
			this.QteExpressDeliveryIncludedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.QteExpressDeliveryIncludedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QteExpressDeliveryIncludedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 356, true);
			this.QteExpressDeliveryIncludedCheckBox.Name = "QteExpressDeliveryIncludedCheckBox";
			this.QteExpressDeliveryIncludedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.QteExpressDeliveryIncludedCheckBox.TabIndex = 16;
			this.QteExpressDeliveryIncludedCheckBox.UseVisualStyleBackColor = true;
			// 
			// QteHeadStartSurcharge
			// 
			this.BindingSource.SetBindingMember(this.QteHeadStartSurcharge, "Quote.CIQ_HeadStartSurcharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_HeadStartSurcharge)));
			this.QteHeadStartSurcharge.DecimalPlaces = 2;
			this.QteHeadStartSurcharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 327, true);
			this.QteHeadStartSurcharge.Name = "QteHeadStartSurcharge";
			this.QteHeadStartSurcharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.QteHeadStartSurcharge.TabIndex = 15;
			this.QteHeadStartSurcharge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QteHeadStartIncludedCheckBox
			// 
			this.QteHeadStartIncludedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.QteHeadStartIncludedCheckBox, "Quote.CIQ_HeadStartOptionIncluded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_HeadStartOptionIncluded)));
			this.QteHeadStartIncludedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.QteHeadStartIncludedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QteHeadStartIncludedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 330, true);
			this.QteHeadStartIncludedCheckBox.Name = "QteHeadStartIncludedCheckBox";
			this.QteHeadStartIncludedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.QteHeadStartIncludedCheckBox.TabIndex = 14;
			this.QteHeadStartIncludedCheckBox.UseVisualStyleBackColor = true;
			// 
			// QteOneOffUpfront
			// 
			this.BindingSource.SetBindingMember(this.QteOneOffUpfront, "Quote.CIQ_OneoffUpfront");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_OneoffUpfront)));
			this.QteOneOffUpfront.DecimalPlaces = 2;
			this.QteOneOffUpfront.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 275, true);
			this.QteOneOffUpfront.Name = "QteOneOffUpfront";
			this.QteOneOffUpfront.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.QteOneOffUpfront.TabIndex = 12;
			this.QteOneOffUpfront.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QteAmount
			// 
			this.BindingSource.SetBindingMember(this.QteAmount, "Quote.CIQ_QuoteAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_QuoteAmount)));
			this.QteAmount.DecimalPlaces = 2;
			this.QteAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 249, true);
			this.QteAmount.Name = "QteAmount";
			this.QteAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.QteAmount.TabIndex = 11;
			this.QteAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QteExpressDeliverySurcharge
			// 
			this.BindingSource.SetBindingMember(this.QteExpressDeliverySurcharge, "Quote.CIQ_ExpressDeliverySurcharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_ExpressDeliverySurcharge)));
			this.QteExpressDeliverySurcharge.DecimalPlaces = 2;
			this.QteExpressDeliverySurcharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 353, true);
			this.QteExpressDeliverySurcharge.Name = "QteExpressDeliverySurcharge";
			this.QteExpressDeliverySurcharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.QteExpressDeliverySurcharge.TabIndex = 17;
			this.QteExpressDeliverySurcharge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QteCurrencyBox
			// 
			this.QteCurrencyBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QteCurrencyBox, "Quote.CIQ_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_RX_NKCurrency)));
			this.QteCurrencyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 223, true);
			this.QteCurrencyBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.QteCurrencyBox.Name = "QteCurrencyBox";
			this.QteCurrencyBox.PreBoundMaxLength = 3;
			this.QteCurrencyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.QteCurrencyBox.TabIndex = 10;
			// 
			// QteMaxDevHours
			// 
			this.BindingSource.SetBindingMember(this.QteMaxDevHours, "Quote.CIQ_MaxDevelopmentHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_MaxDevelopmentHours)));
			this.QteMaxDevHours.DecimalPlaces = 2;
			this.QteMaxDevHours.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 42, true);
			this.QteMaxDevHours.Name = "QteMaxDevHours";
			this.QteMaxDevHours.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.QteMaxDevHours.TabIndex = 3;
			this.QteMaxDevHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QteMinDevHours
			// 
			this.BindingSource.SetBindingMember(this.QteMinDevHours, "Quote.CIQ_MinDevelopmentHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Quote.CIQ_MinDevelopmentHours)));
			this.QteMinDevHours.DecimalPlaces = 2;
			this.QteMinDevHours.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 16, true);
			this.QteMinDevHours.Name = "QteMinDevHours";
			this.QteMinDevHours.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.QteMinDevHours.TabIndex = 2;
			this.QteMinDevHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 5;
			// 
			// RelatedItemsTabPage
			// 
			this.RelatedItemsTabPage.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|bb706b34-b64a-4823-8bfe-e07835462a29", "Related Items");
			this.RelatedItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedItemsTabPage.Name = "RelatedItemsTabPage";
			this.RelatedItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 586, true);
			this.RelatedItemsTabPage.TabIndex = 6;
			// 
			// zPanel4
			// 
			this.zPanel4.Controls.Add(this.groupBox2);
			this.zPanel4.Controls.Add(this.IncidentCommentGroupBox);
			this.zPanel4.Controls.Add(this.ClientAndContactDetailsGroupBox);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Left;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 586, true);
			this.zPanel4.TabIndex = 6;
			// 
			// groupBox2
			// 
			this.groupBox2.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|bf53d688-edaa-4e1b-a279-a9f6a6f44cf3", "Problem Description");
			this.groupBox2.Controls.Add(this.zTextBox2);
			this.groupBox2.Controls.Add(this.zTextBox1);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 168, true);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			// 
			// zTextBox2
			// 
			this.zTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zTextBox2.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox2, "DetailNoteText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).DetailNoteText)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox2, false);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 45, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 118, true);
			this.zTextBox2.TabIndex = 2;
			// 
			// zTextBox1
			// 
			this.zTextBox1.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox1, "IM_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Description)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 19, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 20, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// IncidentCommentGroupBox
			// 
			this.IncidentCommentGroupBox.Controls.Add(this.IncidentCommentTextBox);
			this.IncidentCommentGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.IncidentCommentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 496, true);
			this.IncidentCommentGroupBox.Name = "IncidentCommentGroupBox";
			this.IncidentCommentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 90, true);
			this.IncidentCommentGroupBox.TabIndex = 7;
			this.IncidentCommentGroupBox.TabStop = false;
			this.IncidentCommentGroupBox.Text = "eRequest Comment";
			// 
			// IncidentCommentTextBox
			// 
			this.IncidentCommentTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.IncidentCommentTextBox, "IncidentComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IncidentComment)));
			this.IncidentCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IncidentCommentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IncidentCommentTextBox, false);
			this.IncidentCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IncidentCommentTextBox.Multiline = true;
			this.IncidentCommentTextBox.Name = "IncidentCommentTextBox";
			this.IncidentCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.IncidentCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 71, true);
			this.IncidentCommentTextBox.TabIndex = 3;
			// 
			// ClientAndContactDetailsGroupBox
			// 
			this.ClientAndContactDetailsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|0398a93d-aced-439f-8115-3da93878d41d", "Organization");
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.supportClientNameLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.SupportCompanyDropEdit);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.supportDatabaseDropEdit);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.supportEnterpriseGuidFindBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.supportEnterpriseCodeFindBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.supportClientGuidFindBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactPhoneDiallerUserControl);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.RelMgrLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.HostedLocationLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.HostedLocationCaption);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.WorkplaceLabel2);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.WorkplaceCaption2);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.RelationshipManagerCaption);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.isContactAccreditedLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.SRNumLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.SRNumValue);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ShowAccreditationButton);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.LocalTimeLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.LocalTimeValueLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.LanguageDropEdit);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactGuidFindBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContractLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContractStatusLabel);
			this.ClientAndContactDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClientAndContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientAndContactDetailsGroupBox.Name = "ClientAndContactDetailsGroupBox";
			this.ClientAndContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 328, true);
			this.ClientAndContactDetailsGroupBox.TabIndex = 0;
			this.ClientAndContactDetailsGroupBox.TabStop = false;
			// 
			// supportClientNameLabel
			// 
			this.BindingSource.SetBindingMember(this.supportClientNameLabel, "ClientName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientName)));
			this.supportClientNameLabel.AutoSize = true;
			this.supportClientNameLabel.IsFontBold = true;
			this.supportClientNameLabel.UseMnemonic = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.supportClientNameLabel, false);
			this.supportClientNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 16, true);
			this.supportClientNameLabel.Name = "supportClientNameLabel";
			this.supportClientNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 39, true);
			this.supportClientNameLabel.TabIndex = 22;
			this.supportClientNameLabel.Text = "<Client Name>";
			this.supportClientNameLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SupportCompanyDropEdit
			// 
			this.SupportCompanyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportCompanyDropEdit, "ClientCompanyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientCompanyCode)));
			this.SupportCompanyDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("c3e1a5fc-713f-4714-808d-b0d1a938a981", "DB Company");
			this.SupportCompanyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 93, true);
			this.SupportCompanyDropEdit.MaxItemsToShowInDropDown = 30;
			this.SupportCompanyDropEdit.Name = "SupportCompanyDropEdit";
			this.SupportCompanyDropEdit.PreBoundMaxLength = 3;
			this.SupportCompanyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.SupportCompanyDropEdit.TabIndex = 4;
			// 
			// supportDatabaseDropEdit
			// 
			this.supportDatabaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.supportDatabaseDropEdit, "DatabaseServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).DatabaseServerCode)));
			this.supportDatabaseDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("a3b07036-1fc9-4083-a312-6e8c12cc4c16", "Database");
			this.supportDatabaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 66, true);
			this.supportDatabaseDropEdit.MaxItemsToShowInDropDown = 30;
			this.supportDatabaseDropEdit.Name = "supportDatabaseDropEdit";
			this.supportDatabaseDropEdit.PreBoundMaxLength = 3;
			this.supportDatabaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.supportDatabaseDropEdit.TabIndex = 3;
			// 
			// supportEnterpriseGuidFindBox
			// 
			this.supportEnterpriseGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.supportEnterpriseGuidFindBox, "EnterprisePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).EnterprisePK)));
			this.supportEnterpriseGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("0396ae2f-1a8d-4964-b219-fb07e8cff945", "Enterprise ID");
			this.supportEnterpriseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 40, true);
			this.supportEnterpriseGuidFindBox.Name = "supportEnterpriseGuidFindBox";
			this.supportEnterpriseGuidFindBox.ShowDescriptionBox = false;
			this.supportEnterpriseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.supportEnterpriseGuidFindBox.TabIndex = 2;
			// 
			// supportEnterpriseCodeFindBox
			// 
			this.supportEnterpriseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.supportEnterpriseCodeFindBox, "EnterpriseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).EnterpriseCode)));
			this.supportEnterpriseCodeFindBox.CaptionResourceString = ZClientEDI.Res.GetData("d58ead6b-3035-4b44-99b6-1c7577206acb", "Enterprise Code");
			this.supportEnterpriseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 40, true);
			this.supportEnterpriseCodeFindBox.Name = "supportEnterpriseCodeFindBox";
			this.supportEnterpriseCodeFindBox.PreBoundMaxLength = 3;
			this.supportEnterpriseCodeFindBox.ShouldResize = true;
			this.supportEnterpriseCodeFindBox.ShowDescriptionBox = false;
			this.supportEnterpriseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.supportEnterpriseCodeFindBox.TabIndex = 2;
			// 
			// supportClientGuidFindBox
			// 
			this.supportClientGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.supportClientGuidFindBox, "IM_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_OH_Client)));
			this.supportClientGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("f3bda37f-eecc-47a8-9775-077296732b44", "Organization");
			this.supportClientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.supportClientGuidFindBox.Name = "supportClientGuidFindBox";
			this.supportClientGuidFindBox.ShowDescriptionBox = false;
			this.supportClientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.supportClientGuidFindBox.TabIndex = 1;
			// 
			// ContactPhoneDiallerUserControl
			// 
			this.ContactPhoneDiallerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactPhoneDiallerUserControl, "IM_OC_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_OC_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_OH_Client)));
			this.ContactPhoneDiallerUserControl.BindToOrg = "IM_OH_Client";
			this.ContactPhoneDiallerUserControl.CurrentOrg = null;
			this.ContactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 172, true);
			this.ContactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.Name = "ContactPhoneDiallerUserControl";
			this.ContactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.TabIndex = 6;
			// 
			// RelMgrLabel
			// 
			this.BindingSource.SetBindingMember(this.RelMgrLabel, "RelationshipStaffCodeAndName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).RelationshipStaffCodeAndName)));
			this.RelMgrLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RelMgrLabel, false);
			this.RelMgrLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 292, true);
			this.RelMgrLabel.Name = "RelMgrLabel";
			this.RelMgrLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 18, true);
			this.RelMgrLabel.TabIndex = 21;
			this.RelMgrLabel.Text = "<RM Code and Name>";
			// 
			// HostedLocationLabel
			// 
			this.BindingSource.SetBindingMember(this.HostedLocationLabel, "LicenceDatabaseHostedLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).LicenceDatabaseHostedLocation)));
			this.HostedLocationLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HostedLocationLabel, false);
			this.HostedLocationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 118, true);
			this.HostedLocationLabel.Name = "HostedLocationLabel";
			this.HostedLocationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 19, true);
			this.HostedLocationLabel.TabIndex = 19;
			this.HostedLocationLabel.Text = "<Hosted Location>";
			// 
			// HostedLocationCaption
			// 
			this.HostedLocationCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 118, true);
			this.HostedLocationCaption.Name = "HostedLocationCaption";
			this.HostedLocationCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 19, true);
			this.HostedLocationCaption.TabIndex = 18;
			this.HostedLocationCaption.Text = "Hosted:";
			this.HostedLocationCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WorkplaceLabel2
			// 
			this.BindingSource.SetBindingMember(this.WorkplaceLabel2, "ContactPrimaryWorkplace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ContactPrimaryWorkplace)));
			this.WorkplaceLabel2.IsFontBold = true;
			this.WorkplaceLabel2.UseMnemonic = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WorkplaceLabel2, false);
			this.WorkplaceLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 195, true);
			this.WorkplaceLabel2.Name = "WorkplaceLabel2";
			this.WorkplaceLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 18, true);
			this.WorkplaceLabel2.TabIndex = 19;
			// 
			// WorkplaceCaption2
			// 
			this.WorkplaceCaption2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 195, true);
			this.WorkplaceCaption2.Name = "WorkplaceCaption2";
			this.WorkplaceCaption2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.WorkplaceCaption2.TabIndex = 18;
			this.WorkplaceCaption2.Text = "Workplace:";
			this.WorkplaceCaption2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// RelationshipManagerCaption
			// 
			this.BindingSource.SetBindingMember(this.RelationshipManagerCaption, "RelationshipManagerOrCoordinatorLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).RelationshipManagerOrCoordinatorLabel)));
			this.RelationshipManagerCaption.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|7146af5c-49fd-45d0-8ade-0fdc034fd3ca", "Project Managed By:");
			this.RelationshipManagerCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 292, true);
			this.RelationshipManagerCaption.Name = "RelationshipManagerCaption";
			this.RelationshipManagerCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.RelationshipManagerCaption.TabIndex = 20;
			this.RelationshipManagerCaption.Text = "<Rel Staff:>";
			// 
			// isContactAccreditedLabel
			// 
			this.BindingSource.SetBindingMember(this.isContactAccreditedLabel, "ContactAccreditationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ContactAccreditationStatus)));
			this.isContactAccreditedLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.isContactAccreditedLabel, false);
			this.isContactAccreditedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 218, true);
			this.isContactAccreditedLabel.Name = "isContactAccreditedLabel";
			this.isContactAccreditedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 19, true);
			this.isContactAccreditedLabel.TabIndex = 9;
			this.isContactAccreditedLabel.Text = "<ContactIsAccredited>";
			// 
			// SRNumLabel
			// 
			this.SRNumLabel.AutoSize = true;
			this.SRNumLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|0baf341d-d450-4be2-9efd-109210aad1cf", "Cust. SR Num.:");
			this.SRNumLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SRNumLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 246, true);
			this.SRNumLabel.Name = "SRNumLabel";
			this.SRNumLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.SRNumLabel.TabIndex = 12;
			// 
			// SRNumValue
			// 
			this.BindingSource.SetBindingMember(this.SRNumValue, "Request.INC_ClientReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution.
			// If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Request.INC_ClientReference)));
			this.SRNumValue.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|9da04503-ed6d-4646-a388-0fab605481da", "Client Reference Num.");
			this.SRNumValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 246, true);
			this.SRNumValue.Name = "SRNumValue";
			this.SRNumValue.ReadOnly = true;
			this.SRNumValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 13, true);
			// 
			// ShowAccreditationButton
			// 
			this.ShowAccreditationButton.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|380ded0f-4624-4421-a19a-2b4fe9985fb6", "Show Accreditation Info");
			this.ShowAccreditationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 218, true);
			this.ShowAccreditationButton.Name = "ShowAccreditationButton";
			this.ShowAccreditationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 19, true);
			this.ShowAccreditationButton.TabIndex = 8;
			this.ShowAccreditationButton.UseVisualStyleBackColor = true;
			this.ShowAccreditationButton.Click += new System.EventHandler(this.ShowAccreditationButton_Click);
			// 
			// LocalTimeLabel
			// 
			this.LocalTimeLabel.AutoSize = true;
			this.LocalTimeLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|112e0f36-2c34-4aaf-97c2-bef143c677ee", "Client Time:");
			this.LocalTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 246, true);
			this.LocalTimeLabel.Name = "LocalTimeLabel";
			this.LocalTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.LocalTimeLabel.TabIndex = 10;
			this.LocalTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LocalTimeValueLabel
			// 
			this.LocalTimeValueLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LocalTimeValueLabel, "ClientLocalTimeString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientLocalTimeString)));
			this.LocalTimeValueLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|f5290e36-fb04-40a7-b155-1cb23bb89092", "Client Time");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalTimeValueLabel, false);
			this.LocalTimeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 246, true);
			this.LocalTimeValueLabel.Name = "LocalTimeValueLabel";
			this.LocalTimeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 13, true);
			this.LocalTimeValueLabel.TabIndex = 11;
			this.LocalTimeValueLabel.Text = "<Local Time>";
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "IM_Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Language)));
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 145, true); // JNG: add 27 to location y of above control
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.LanguageDropEdit.TabIndex = 4;
			// 
			// ContactGuidFindBox
			// 
			this.ContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactGuidFindBox, "IM_OC_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_OC_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.ContactList)));
			this.ContactGuidFindBox.BindToList = "Lookups+ContactList";
			this.ContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 172, true);
			this.ContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.ContactGuidFindBox.Name = "ContactGuidFindBox";
			this.ContactGuidFindBox.PopupCaption = "Select the Contact";
			this.ContactGuidFindBox.PreBoundMaxLength = 25;
			this.ContactGuidFindBox.ShowDescriptionBox = false;
			this.ContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.ContactGuidFindBox.TabIndex = 5;
			// 
			// ContractLabel
			// 
			this.ContractLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|bd502d5d-4502-442c-aad0-e71fef5ae0ab", "Contract:");
			this.ContractLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 265, true);
			this.ContractLabel.Name = "ContractLabel";
			this.ContractLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 22, true);
			this.ContractLabel.TabIndex = 14;
			this.ContractLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ContractStatusLabel
			// 
			this.ContractStatusLabel.AccessibleDescription = "";
			this.BindingSource.SetBindingMember(this.ContractStatusLabel, "IM_ClientContractStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ClientContractStatus)));
			this.ContractStatusLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|ce9bc867-8058-4c35-bb5d-e385e5483450", "Contract");
			this.ContractStatusLabel.ForeColor = System.Drawing.Color.Red;
			this.ContractStatusLabel.IsFontBold = true;
			this.ContractStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 268, true);
			this.ContractStatusLabel.Name = "ContractStatusLabel";
			this.ContractStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 16, true);
			this.ContractStatusLabel.TabIndex = 1;
			this.ContractStatusLabel.Text = "<Client Contract Status>";
			// 
			// zPanel5
			// 
			this.zPanel5.Controls.Add(this.IncidentLogGroupBox);
			this.zPanel5.Controls.Add(this.IncidentDetailsGroupBox);
			this.zPanel5.Controls.Add(this.statePanel);
			this.zPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 0, true);
			this.zPanel5.Name = "zPanel5";
			this.zPanel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 586, true);
			this.zPanel5.TabIndex = 20;
			// 
			// IncidentLogGroupBox
			// 
			this.IncidentLogGroupBox.Controls.Add(this.splitContainer5);
			this.IncidentLogGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentLogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 301, true);
			this.IncidentLogGroupBox.Name = "IncidentLogGroupBox";
			this.IncidentLogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 285, true);
			this.IncidentLogGroupBox.TabIndex = 6;
			this.IncidentLogGroupBox.TabStop = false;
			this.IncidentLogGroupBox.Text = "eConversation";
			// 
			// splitContainer5
			// 
			this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainer5.Name = "splitContainer5";
			this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer5.Panel1
			// 
			this.splitContainer5.Panel1.Controls.Add(this.CloseIncidentButton);
			this.splitContainer5.Panel1.Controls.Add(this.AwaitingResponseButton);
			this.splitContainer5.Panel1.Controls.Add(this.ConversationMessageTextBox);
			this.splitContainer5.Panel1.Controls.Add(this.SendMessageButton);
			this.splitContainer5.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			this.splitContainer5.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.splitContainer5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 266, true);
			this.splitContainer5.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			this.splitContainer5.SplitterWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(8);
			this.splitContainer5.TabIndex = 3;
			// 
			// CloseIncidentButton
			// 
			this.CloseIncidentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseIncidentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 3, true);
			this.CloseIncidentButton.Name = "CloseIncidentButton";
			this.CloseIncidentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 45, true);
			this.CloseIncidentButton.TabIndex = 4;
			this.CloseIncidentButton.Text = "Close As";
			this.CloseIncidentButton.UseVisualStyleBackColor = true;
			this.CloseIncidentButton.Click += new System.EventHandler(this.CloseIncidentButton_Click);
			// 
			// AwaitingResponseButton
			// 
			this.AwaitingResponseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AwaitingResponseButton.Enabled = true;
			this.AwaitingResponseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 3, true);
			this.AwaitingResponseButton.Name = "AwaitingResponseButton";
			this.AwaitingResponseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 45, true);
			this.AwaitingResponseButton.TabIndex = 2;
			this.AwaitingResponseButton.Text = "Awaiting Response";
			this.AwaitingResponseButton.UseVisualStyleBackColor = true;
			// 
			// ConversationMessageTextBox
			// 
			this.ConversationMessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ConversationMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConversationMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConversationMessageTextBox.Multiline = true;
			this.ConversationMessageTextBox.CaptionResourceString = ZClientEDI.Res.GetData("a1df9963-650b-464e-a86f-018c15b62ad1", "Conversation Message Text Box");
			this.ConversationMessageTextBox.Name = "ConversationMessageTextBox";
			this.ConversationMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ConversationMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 45, true);
			this.ConversationMessageTextBox.TabIndex = 1;
			// 
			// SendMessageButton
			// 
			this.SendMessageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendMessageButton.Enabled = false;
			this.SendMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 3, true);
			this.SendMessageButton.Name = "SendMessageButton";
			this.SendMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 45, true);
			this.SendMessageButton.TabIndex = 3;
			this.SendMessageButton.Text = "Send";
			this.SendMessageButton.UseVisualStyleBackColor = true;
			// 
			// IncidentDetailsGroupBox
			// 
			this.IncidentDetailsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|fe23408c-dd95-45f5-a487-e44f7301aaed", "Incident Details");
			this.IncidentDetailsGroupBox.Controls.Add(this.IncidentDetailsPanel);
			this.IncidentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.IncidentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
			this.IncidentDetailsGroupBox.Name = "IncidentDetailsGroupBox";
			this.IncidentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 216, true);
			this.IncidentDetailsGroupBox.TabIndex = 3;
			this.IncidentDetailsGroupBox.TabStop = false;
			// 
			// IncidentDetailsPanel
			// 
			this.IncidentDetailsPanel.Controls.Add(this.CountryDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.OverrideSourceModuleButton);
			this.IncidentDetailsPanel.Controls.Add(this.TriageAssistButton);
			this.IncidentDetailsPanel.Controls.Add(this.triageAssistCaption);
			this.IncidentDetailsPanel.Controls.Add(this.menuItemCaption);
			this.IncidentDetailsPanel.Controls.Add(this.serviceTypeCaption);
			this.IncidentDetailsPanel.Controls.Add(this.serviceTypeDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.triageAssistLabel);
			this.IncidentDetailsPanel.Controls.Add(this.sourceModuleLabel);
			this.IncidentDetailsPanel.Controls.Add(this.ProductAreaDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.SystemVersionBoundLabel);
			this.IncidentDetailsPanel.Controls.Add(this.menuSectionDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.cr8ModuleDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.cr9ModuleDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.zDropEdit1);
			this.IncidentDetailsPanel.Controls.Add(this.SystemVersionLabel);
			this.IncidentDetailsPanel.Controls.Add(this.sourceDropEdit);
			this.IncidentDetailsPanel.Controls.Add(this.RequestedPriorityDropEdit);
			this.IncidentDetailsPanel.AutoScroll = true;
			this.IncidentDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncidentDetailsPanel.Name = "IncidentDetailsPanel";
			this.IncidentDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 216, true);
			this.IncidentDetailsPanel.TabIndex = 0;
			this.IncidentDetailsPanel.TabStop = false;
			// 
			// CountryDropEdit
			// 
			this.CountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryDropEdit, "IM_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_RN_NKCountry)));
			this.CountryDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("a08a06ff-b151-4e2b-bfc0-ee0631fb7933", "Country/Region");
			this.CountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 103, true);
			this.CountryDropEdit.MaxItemsToShowInDropDown = 20;
			this.CountryDropEdit.Name = "CountryDropEdit";
			this.CountryDropEdit.PreBoundMaxLength = 2;
			this.CountryDropEdit.ShowDescriptionBox = false;
			this.CountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.CountryDropEdit.TabIndex = 8;
			// 
			// OverrideSourceModuleButton
			// 
			this.OverrideSourceModuleButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.OverrideSourceModuleButton.CaptionResourceString = ZClientEDI.Res.GetData("4add3bab-9b0d-47c0-8077-b32c639cb5f6", "Override");
			this.OverrideSourceModuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 158, true);
			this.OverrideSourceModuleButton.Name = "OverrideSourceModuleButton";
			this.OverrideSourceModuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OverrideSourceModuleButton.TabIndex = 9;
			this.OverrideSourceModuleButton.UseVisualStyleBackColor = true;
			this.OverrideSourceModuleButton.Click += new System.EventHandler(this.OverrideMenuItemButton_Click);
			// 
			// TriageAssistButton
			// 
			this.TriageAssistButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TriageAssistButton.CaptionResourceString = ZClientEDI.Res.GetData("1befd1d8-a75d-4675-9492-abb1f0684496", "Triage Assist");
			this.TriageAssistButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 136, true);
			this.TriageAssistButton.Name = "TriageAssistButton";
			this.TriageAssistButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.TriageAssistButton.TabIndex = 8;
			this.TriageAssistButton.UseVisualStyleBackColor = true;
			this.TriageAssistButton.Click += new System.EventHandler(this.TriageAssistButton_Click);
			// 
			// triageAssistCaption
			// 
			this.triageAssistCaption.CaptionResourceString = ZClientEDI.Res.GetData("b915d7af-50be-4c28-83e9-19f2af98cb17", "Triage Node:");
			this.triageAssistCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.triageAssistCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 127, true);
			this.triageAssistCaption.Name = "triageAssistCaption";
			this.triageAssistCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.triageAssistCaption.TabIndex = 7;
			this.triageAssistCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.triageAssistCaption.UseMnemonic = false;
			// 
			// menuItemCaption
			// 
			this.menuItemCaption.CaptionResourceString = ZClientEDI.Res.GetData("d07e051f-4f1b-4980-b2e1-a7648d26d50a", "Menu Item:");
			this.menuItemCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 157, true);
			this.menuItemCaption.Name = "menuItemCaption";
			this.menuItemCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.menuItemCaption.TabIndex = 7;
			this.menuItemCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// serviceTypeCaption
			// 
			this.serviceTypeCaption.CaptionResourceString = ZClientEDI.Res.GetData("9962751f-388c-4e89-8e12-c92056484392", "Service Type:");
			this.serviceTypeCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 127, true);
			this.serviceTypeCaption.Name = "serviceTypeCaption";
			this.serviceTypeCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.serviceTypeCaption.TabIndex = 7;
			this.serviceTypeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// serviceTypeDropEdit
			// 
			this.serviceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serviceTypeDropEdit, "IM_ServiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ServiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ServiceTypeDescription)));
			this.serviceTypeDropEdit.BindToForDescription = "IM_ServiceTypeDescription";
			this.serviceTypeDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("f5b489ee-4f57-42b6-9b53-f9b626e86ec0", "Service Type");
			this.serviceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 130, true);
			this.serviceTypeDropEdit.MaxItemsToShowInDropDown = 20;
			this.serviceTypeDropEdit.Name = "serviceTypeDropEdit";
			this.serviceTypeDropEdit.PreBoundMaxLength = 3;
			this.serviceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.serviceTypeDropEdit.TabIndex = 5;
			// 
			// triageAssistLabel
			// 
			this.BindingSource.SetBindingMember(this.triageAssistLabel, "TriageDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).TriageDescription)));
			this.triageAssistLabel.CaptionResourceString = ZClientEDI.Res.GetData("fd79f5fe-42d5-468f-ace8-95da81cbc30d", "Triage Node:");
			this.triageAssistLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.triageAssistLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 131, true);
			this.triageAssistLabel.Name = "triageAssistLabel";
			this.triageAssistLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 15, true);
			this.triageAssistLabel.TabIndex = 8;
			this.triageAssistLabel.Text = "<Triage Node:>";
			this.triageAssistLabel.UseMnemonic = false;
			this.triageAssistLabel.AutoEllipsis = true;
			// 
			// sourceModuleLabel
			// 
			this.BindingSource.SetBindingMember(this.sourceModuleLabel, "SourceModuleWithPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).SourceModuleWithPath)));
			this.sourceModuleLabel.CaptionResourceString = ZClientEDI.Res.GetData("fda17183-be79-4ade-b2f2-5e20d89c0a34", "Menu Item");
			this.sourceModuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 161, true);
			this.sourceModuleLabel.Name = "sourceModuleLabel";
			this.sourceModuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 15, true);
			this.sourceModuleLabel.TabIndex = 8;
			this.sourceModuleLabel.Text = "<Menu Item>";
			this.sourceModuleLabel.UseMnemonic = false;
			// 
			// ProductAreaDropEdit
			// 
			this.ProductAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductAreaDropEdit, "ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.FilteredProductAreaList)));
			this.ProductAreaDropEdit.BindToList = "Lookups+FilteredProductAreaList";
			this.ProductAreaDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|4b6008bd-cb6e-42e7-8633-701382ed77c1", "Product Area");
			this.ProductAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 77, true);
			this.ProductAreaDropEdit.MaxItemsToShowInDropDown = 20;
			this.ProductAreaDropEdit.Name = "ProductAreaDropEdit";
			this.ProductAreaDropEdit.PreBoundMaxLength = 3;
			this.ProductAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.ProductAreaDropEdit.TabIndex = 5;
			// 
			// SystemVersionBoundLabel
			// 
			this.BindingSource.SetBindingMember(this.SystemVersionBoundLabel, "ClientReportedOnVersion+FullDisplayText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientReportedOnVersion.FullDisplayText)));
			this.SystemVersionBoundLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|fa484139-68d8-4e16-859a-8bcfbba223ec", "System Version");
			this.SystemVersionBoundLabel.IsFontBold = true;
			this.SystemVersionBoundLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SystemVersionBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 0, true);
			this.SystemVersionBoundLabel.Name = "SystemVersionBoundLabel";
			this.SystemVersionBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 23, true);
			this.SystemVersionBoundLabel.AutoEllipsis = true;
			this.SystemVersionBoundLabel.TabIndex = 1;
			this.SystemVersionBoundLabel.Text = "<Version>";
			// 
			// menuSectionDropEdit
			// 
			this.menuSectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.menuSectionDropEdit, "IM_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ModuleDescription)));
			this.menuSectionDropEdit.BindToForDescription = "IM_ModuleDescription";
			this.menuSectionDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("f14cac47-e12a-40b0-bd6d-c104e6baef8f", "Section", "Menu Section");
			this.menuSectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 103, true);
			this.menuSectionDropEdit.MaxItemsToShowInDropDown = 20;
			this.menuSectionDropEdit.Name = "menuSectionDropEdit";
			this.menuSectionDropEdit.PreBoundMaxLength = 3;
			this.menuSectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.menuSectionDropEdit.TabIndex = 6;
			this.menuSectionDropEdit.Visible = false;
			// 
			// cr8ModuleDropEdit
			// 
			this.cr8ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cr8ModuleDropEdit, "IM_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ModuleDescription)));
			this.cr8ModuleDropEdit.BindToForDescription = "IM_ModuleDescription";
			this.cr8ModuleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("9dfef4b5-04c2-4ff9-afe1-9f3a6814adc4", "Requirement");
			this.cr8ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 103, true);
			this.cr8ModuleDropEdit.MaxItemsToShowInDropDown = 20;
			this.cr8ModuleDropEdit.Name = "cr8ModuleDropEdit";
			this.cr8ModuleDropEdit.PreBoundMaxLength = 3;
			this.cr8ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.cr8ModuleDropEdit.TabIndex = 7;
			this.cr8ModuleDropEdit.Visible = false;
			// 
			// cr9ModuleDropEdit
			// 
			this.cr9ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cr9ModuleDropEdit, "IM_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ModuleDescription)));
			this.cr9ModuleDropEdit.BindToForDescription = "IM_ModuleDescription";
			this.cr9ModuleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("f97eee53-4c70-4bdf-8700-3b5fb9ea5d09", "Service");
			this.cr9ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 103, true);
			this.cr9ModuleDropEdit.MaxItemsToShowInDropDown = 20;
			this.cr9ModuleDropEdit.Name = "cr9ModuleDropEdit";
			this.cr9ModuleDropEdit.PreBoundMaxLength = 3;
			this.cr9ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.cr9ModuleDropEdit.TabIndex = 7;
			this.cr9ModuleDropEdit.Visible = false;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "IM_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.ProductList)));
			this.zDropEdit1.BindToList = "Lookups+ProductList";
			this.zDropEdit1.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|c2093cd1-8d92-43b2-9376-f53292cca55c", "Product");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 77, true);
			this.zDropEdit1.MaxItemsToShowInDropDown = 20;
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.zDropEdit1.TabIndex = 4;
			// 
			// SystemVersionLabel
			// 
			this.SystemVersionLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|1c17294f-7005-4b80-8764-0587ede8d962", "System Version:");
			this.SystemVersionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 0, true);
			this.SystemVersionLabel.Name = "SystemVersionLabel";
			this.SystemVersionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.SystemVersionLabel.TabIndex = 0;
			this.SystemVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// sourceDropEdit
			// 
			this.sourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sourceDropEdit, "IM_Source");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.SourceList)));
			this.sourceDropEdit.BindToList = "Lookups+SourceList";
			this.sourceDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|06702627-5720-4716-b531-edc376db5364", "Source");
			this.sourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 25, true);
			this.sourceDropEdit.MaxItemsToShowInDropDown = 20;
			this.sourceDropEdit.Name = "sourceDropEdit";
			this.sourceDropEdit.PreBoundMaxLength = 3;
			this.sourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 20, true);
			this.sourceDropEdit.TabIndex = 2;
			// 
			// RequestedPriorityDropEdit
			// 
			this.RequestedPriorityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestedPriorityDropEdit, "IM_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_Priority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Lookups.CriticalityList)));
			this.RequestedPriorityDropEdit.BindToList = "Lookups+CriticalityList";
			this.RequestedPriorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.RequestedPriorityDropEdit.Name = "RequestedPriorityDropEdit";
			this.RequestedPriorityDropEdit.PreBoundMaxLength = 3;
			this.RequestedPriorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 20, true);
			this.RequestedPriorityDropEdit.TabIndex = 3;
			// 
			// statePanel
			// 
			this.statePanel.Controls.Add(this.statusStateSplitContainer);
			this.statePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.statePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.statePanel.Name = "statePanel";
			this.statePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 115, true);
			this.statePanel.TabIndex = 2;
			// 
			// statusStateSplitContainer
			// 
			this.statusStateSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusStateSplitContainer.Location = new System.Drawing.Point(0, 0);
			this.statusStateSplitContainer.Name = "statusStateSplitContainer";
			// 
			// statusStateSplitContainer.Panel1
			// 
			this.statusStateSplitContainer.Panel1.Controls.Add(this.StatusGroupBox);
			// 
			// statusStateSplitContainer.Panel2
			// 
			this.statusStateSplitContainer.Panel2.Controls.Add(this.workflowStateGroupBox);
			this.statusStateSplitContainer.Size = new System.Drawing.Size(1401, 287);
			this.statusStateSplitContainer.SplitterDistance = 768;
			this.statusStateSplitContainer.TabIndex = 15;
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|ecfdcfb4-0839-4567-b1cc-4bbe48cce1b0", "eRequest Status");
			this.StatusGroupBox.Controls.Add(this.closureResolutionDropEdit);
			this.StatusGroupBox.Controls.Add(this.resolutionMethodLabelText);
			this.StatusGroupBox.Controls.Add(this.stageLabel);
			this.StatusGroupBox.Controls.Add(this.ServiceStatusLabel);
			this.StatusGroupBox.Controls.Add(this.OutageDurationLabel);
			this.StatusGroupBox.Controls.Add(this.stageLabelText);
			this.StatusGroupBox.Controls.Add(this.ServiceStatusLabelText);
			this.StatusGroupBox.Controls.Add(this.OutageDurationLabelText);
			this.StatusGroupBox.Controls.Add(this.eRequestStatusLabelText);
			this.StatusGroupBox.Controls.Add(this.IncidentNumberTextBox);
			this.StatusGroupBox.Controls.Add(this.eRequestStatusLabel);
			this.StatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 115, true);
			this.StatusGroupBox.TabIndex = 2;
			this.StatusGroupBox.TabStop = false;
			// 
			// closureResolutionLabel
			// 
			this.closureResolutionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.closureResolutionDropEdit, "IM_ClosureResolutionForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ClosureResolution)));
			this.closureResolutionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 55, true);
			this.closureResolutionDropEdit.MaxItemsToShowInDropDown = 30;
			this.closureResolutionDropEdit.Name = "closureResolutionDropEdit";
			this.closureResolutionDropEdit.PreBoundMaxLength = 3;
			this.closureResolutionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.closureResolutionDropEdit.TabIndex = 24;
			// 
			// resolutionMethodLabelText
			// 
			this.resolutionMethodLabelText.CaptionResourceString = ZClientEDI.Res.GetData("43190596-6b2a-4d3d-80c7-03a3c4b40daa", "Resolution Method:");
			this.resolutionMethodLabelText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.resolutionMethodLabelText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 55, true);
			this.resolutionMethodLabelText.Name = "resolutionMethodLabelText";
			this.resolutionMethodLabelText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 15, true);
			this.resolutionMethodLabelText.TabIndex = 13;
			this.resolutionMethodLabelText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.resolutionMethodLabelText.UseMnemonic = false;
			// 
			// stageLabel
			// 
			this.BindingSource.SetBindingMember(this.stageLabel, "Stage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Stage)));
			this.stageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.stageLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.stageLabel, false);
			this.stageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 19, true);
			this.stageLabel.Name = "stageLabel";
			this.stageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 15, true);
			this.stageLabel.TabIndex = 11;
			this.stageLabel.Text = "<Stage>";
			this.stageLabel.UseMnemonic = false;
			// 
			// ServiceStatusLabel
			// 
			this.BindingSource.SetBindingMember(this.ServiceStatusLabel, "ServiceStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ServiceStatus)));
			this.ServiceStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ServiceStatusLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ServiceStatusLabel, false);
			this.ServiceStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 73, true);
			this.ServiceStatusLabel.Name = "ServiceStatusLabel";
			this.ServiceStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 15, true);
			this.ServiceStatusLabel.TabIndex = 11;
			this.ServiceStatusLabel.Text = "<ServiceStatusLabel>";
			this.ServiceStatusLabel.UseMnemonic = false;
			// 
			// OutageDurationLabel
			// 
			this.BindingSource.SetBindingMember(this.OutageDurationLabel, "OutageDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OutageDuration)));
			this.OutageDurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OutageDurationLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutageDurationLabel, false);
			this.OutageDurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 91, true);
			this.OutageDurationLabel.Name = "OutageDurationLabel";
			this.OutageDurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 15, true);
			this.OutageDurationLabel.TabIndex = 11;
			this.OutageDurationLabel.Text = "<OutageDuration>";
			this.OutageDurationLabel.UseMnemonic = false;
			// 
			// stageLabelText
			// 
			this.stageLabelText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.stageLabelText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 19, true);
			this.stageLabelText.Name = "stageLabelText";
			this.stageLabelText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.stageLabelText.TabIndex = 10;
			this.stageLabelText.Text = "Stage:";
			this.stageLabelText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.stageLabelText.UseMnemonic = false;
			// 
			// ServiceStatusLabelText
			// 
			this.ServiceStatusLabelText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServiceStatusLabelText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 73, true);
			this.ServiceStatusLabelText.Name = "ServiceStatusLabelText";
			this.ServiceStatusLabelText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 15, true);
			this.ServiceStatusLabelText.TabIndex = 11;
			this.ServiceStatusLabelText.Text = "Service Status:";
			this.ServiceStatusLabelText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ServiceStatusLabelText.UseMnemonic = false;
			// 
			// OutageDurationLabelText
			// 
			this.OutageDurationLabelText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OutageDurationLabelText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 91, true);
			this.OutageDurationLabelText.Name = "OutageDurationLabelText";
			this.OutageDurationLabelText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 15, true);
			this.OutageDurationLabelText.TabIndex = 12;
			this.OutageDurationLabelText.Text = "Outage Duration:";
			this.OutageDurationLabelText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OutageDurationLabelText.UseMnemonic = false;
			// 
			// eRequestStatusLabelText
			// 
			this.eRequestStatusLabelText.CaptionResourceString = ZClientEDI.Res.GetData("e6d76bd5-be62-4849-8c2a-bb0f9219c51a", "eRequest Status:");
			this.eRequestStatusLabelText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.eRequestStatusLabelText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.eRequestStatusLabelText.Name = "eRequestStatusLabelText";
			this.eRequestStatusLabelText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 15, true);
			this.eRequestStatusLabelText.TabIndex = 7;
			this.eRequestStatusLabelText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.eRequestStatusLabelText.UseMnemonic = false;
			// 
			// IncidentNumberTextBox
			// 
			this.IncidentNumberTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IncidentNumberTextBox, "IM_IncidentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_IncidentNumber)));
			this.IncidentNumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.IncidentNumberTextBox.ForeColor = System.Drawing.Color.MediumBlue;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IncidentNumberTextBox, false);
			this.IncidentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 0, true);
			this.IncidentNumberTextBox.Name = "IncidentNumberTextBox";
			this.IncidentNumberTextBox.ReadOnly = true;
			this.IncidentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 12, true);
			this.IncidentNumberTextBox.TabIndex = 4;
			this.IncidentNumberTextBox.Text = "<INCIDENTNUMBER>";
			// 
			// eRequestStatusLabel
			// 
			this.eRequestStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.eRequestStatusLabel, "IM_ResolutionCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_ResolutionCodeDescription)));
			this.eRequestStatusLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|e909ac57-e84c-4123-a866-03f9e33cb9a8", "Disposition");
			this.eRequestStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.eRequestStatusLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.eRequestStatusLabel, false);
			this.eRequestStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 36, true);
			this.eRequestStatusLabel.Name = "eRequestStatusLabel";
			this.eRequestStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 18, true);
			this.eRequestStatusLabel.TabIndex = 3;
			this.eRequestStatusLabel.Text = "<Disposition Description>";
			this.eRequestStatusLabel.UseMnemonic = false;
			// 
			// workflowStateGroupBox
			// 
			this.workflowStateGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("634aef00-5f99-4825-9185-cbfae1a6f803", "Workflow State");
			this.workflowStateGroupBox.Controls.Add(this.OverallAssignedToDescriptionLabel);
			this.workflowStateGroupBox.Controls.Add(this.OverallAssignedToCodeLabel);
			this.workflowStateGroupBox.Controls.Add(this.AssignedToLabel);
			this.workflowStateGroupBox.Controls.Add(this.CurrentTaskLabel);
			this.workflowStateGroupBox.Controls.Add(this.CurrentTaskLabelText);
			this.workflowStateGroupBox.Controls.Add(this.StatusDescription);
			this.workflowStateGroupBox.Controls.Add(this.StatusLabel);
			this.workflowStateGroupBox.Controls.Add(this.CloseCurrentTaskButton);
			this.workflowStateGroupBox.Controls.Add(this.CancelCurrentTaskButton);
			this.workflowStateGroupBox.Controls.Add(this.SuspendCurrentTaskButton);
			this.workflowStateGroupBox.Controls.Add(this.WorkOnCurrentTaskButton);
			this.workflowStateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workflowStateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.workflowStateGroupBox.Name = "workflowStateGroupBox";
			this.workflowStateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 115, true);
			this.workflowStateGroupBox.TabIndex = 3;
			this.workflowStateGroupBox.TabStop = false;
			// 
			// OverallAssignedToDescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallAssignedToDescriptionLabel, "OverallAssignedToDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToDescription)));
			this.OverallAssignedToDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OverallAssignedToDescriptionLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverallAssignedToDescriptionLabel, false);
			this.OverallAssignedToDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 61, true);
			this.OverallAssignedToDescriptionLabel.Name = "OverallAssignedToDescriptionLabel";
			this.OverallAssignedToDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 15, true);
			this.OverallAssignedToDescriptionLabel.TabIndex = 3;
			this.OverallAssignedToDescriptionLabel.Text = "<Staff Fullname>";
			this.OverallAssignedToDescriptionLabel.UseMnemonic = false;
			this.OverallAssignedToDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			// 
			// OverallAssignedToCodeLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallAssignedToCodeLabel, "OverallAssignedToCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToCode)));
			this.OverallAssignedToCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OverallAssignedToCodeLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverallAssignedToCodeLabel, false);
			this.OverallAssignedToCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 61, true);
			this.OverallAssignedToCodeLabel.Name = "OverallAssignedToCodeLabel";
			this.OverallAssignedToCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 15, true);
			this.OverallAssignedToCodeLabel.TabIndex = 2;
			this.OverallAssignedToCodeLabel.Text = "XXX";
			this.OverallAssignedToCodeLabel.UseMnemonic = false;
			// 
			// AssignedToLabel
			// 
			this.BindingSource.SetBindingMember(this.AssignedToLabel, "OverallAssignedToLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).OverallAssignedToLabelText)));
			this.AssignedToLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AssignedToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.AssignedToLabel.Name = "AssignedToLabel";
			this.AssignedToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 15, true);
			this.AssignedToLabel.TabIndex = 1;
			this.AssignedToLabel.Text = "Assigned To:";
			this.AssignedToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AssignedToLabel.UseMnemonic = false;
			// 
			// currentTaskLabel
			// 
			this.BindingSource.SetBindingMember(this.CurrentTaskLabel, "CurrentTask+P9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).CurrentTask.P9_Description)));
			this.CurrentTaskLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CurrentTaskLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CurrentTaskLabel, false);
			this.CurrentTaskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 43, true);
			this.CurrentTaskLabel.Name = "currentTaskLabel";
			this.CurrentTaskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 15, true);
			this.CurrentTaskLabel.TabIndex = 9;
			this.CurrentTaskLabel.Text = "<Current Task Description>";
			this.CurrentTaskLabel.UseMnemonic = false;
			this.CurrentTaskLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			// 
			// currentTaskLabelText
			// 
			this.BindingSource.SetBindingMember(this.CurrentTaskLabelText, "CurrentTaskDescriptionLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).CurrentTaskDescriptionLabelText)));
			this.CurrentTaskLabelText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CurrentTaskLabelText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 42, true);
			this.CurrentTaskLabelText.Name = "currentTaskLabelText";
			this.CurrentTaskLabelText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.CurrentTaskLabelText.TabIndex = 8;
			this.CurrentTaskLabelText.Text = "Current Task:";
			this.CurrentTaskLabelText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CurrentTaskLabelText.UseMnemonic = false;
			// 
			// statusDescription
			// 
			this.BindingSource.SetBindingMember(this.StatusDescription, "IM_StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_StatusDescription)));
			this.StatusDescription.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|a25e86af-f551-4dee-a182-6730bbf41e7a", "Status");
			this.StatusDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.StatusDescription.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusDescription, false);
			this.StatusDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 23, true);
			this.StatusDescription.Name = "statusDescription";
			this.StatusDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.StatusDescription.TabIndex = 1;
			this.StatusDescription.Text = "<Status>";
			this.StatusDescription.UseMnemonic = false;
			// 
			// StatusLabel
			// 
			this.StatusLabel.CaptionResourceString = ZClientEDI.Res.GetData("SupportIncidentForm|d7443539-bf06-4041-b6f3-b46ef238d2fe", "Overall Status:");
			this.StatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 23, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 15, true);
			this.StatusLabel.TabIndex = 0;
			this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.StatusLabel.UseMnemonic = false;
			// 
			// CloseCurrentTaskButton
			// 
			this.CloseCurrentTaskButton.BackColor = System.Drawing.SystemColors.Control;
			this.CloseCurrentTaskButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CloseCurrentTaskButton, false);
			this.CloseCurrentTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 87, true);
			this.CloseCurrentTaskButton.Name = "CloseCurrentTaskButton";
			this.CloseCurrentTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.CloseCurrentTaskButton.TabIndex = 37;
			this.CloseCurrentTaskButton.ToolTipCaption = null;
			this.CloseCurrentTaskButton.UseVisualStyleBackColor = false;
			this.CloseCurrentTaskButton.Click += new System.EventHandler(this.CloseCurrentTaskButton_Click);
			this.CloseCurrentTaskButton.MouseEnter += new System.EventHandler(this.CloseCurrentTaskButton_MouseEnter);
			this.CloseCurrentTaskButton.MouseLeave += new System.EventHandler(this.CloseCurrentTaskButton_MouseLeave);
			// 
			// CancelCurrentTaskButton
			// 
			this.CancelCurrentTaskButton.BackColor = System.Drawing.SystemColors.Control;
			this.CancelCurrentTaskButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CancelCurrentTaskButton, false);
			this.CancelCurrentTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 87, true);
			this.CancelCurrentTaskButton.Name = "CancelCurrentTaskButton";
			this.CancelCurrentTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.CancelCurrentTaskButton.TabIndex = 38;
			this.CancelCurrentTaskButton.UseVisualStyleBackColor = false;
			this.CancelCurrentTaskButton.Click += new System.EventHandler(this.CancelCurrentTaskButton_Click);
			this.CancelCurrentTaskButton.MouseEnter += new System.EventHandler(this.CancelCurrentTaskButton_MouseEnter);
			this.CancelCurrentTaskButton.MouseLeave += new System.EventHandler(this.CancelCurrentTaskButton_MouseLeave);
			// 
			// SuspendCurrentTaskButton
			// 
			this.SuspendCurrentTaskButton.BackColor = System.Drawing.SystemColors.Control;
			this.SuspendCurrentTaskButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SuspendCurrentTaskButton, false);
			this.SuspendCurrentTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 87, true);
			this.SuspendCurrentTaskButton.Name = "SuspendCurrentTaskButton";
			this.SuspendCurrentTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.SuspendCurrentTaskButton.TabIndex = 36;
			this.SuspendCurrentTaskButton.ToolTipCaption = null;
			this.SuspendCurrentTaskButton.UseVisualStyleBackColor = false;
			this.SuspendCurrentTaskButton.Click += new System.EventHandler(this.SuspendCurrentTaskButton_Click);
			this.SuspendCurrentTaskButton.MouseEnter += new System.EventHandler(this.SuspendCurrentTaskButton_MouseEnter);
			this.SuspendCurrentTaskButton.MouseLeave += new System.EventHandler(this.SuspendCurrentTaskButton_MouseLeave);
			// 
			// WorkOnCurrentTaskButton
			// 
			this.WorkOnCurrentTaskButton.BackColor = System.Drawing.SystemColors.Control;
			this.WorkOnCurrentTaskButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WorkOnCurrentTaskButton, false);
			this.WorkOnCurrentTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 87, true);
			this.WorkOnCurrentTaskButton.Name = "WorkOnCurrentTaskButton";
			this.WorkOnCurrentTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.WorkOnCurrentTaskButton.TabIndex = 35;
			this.WorkOnCurrentTaskButton.ToolTipCaption = null;
			this.WorkOnCurrentTaskButton.UseVisualStyleBackColor = false;
			this.WorkOnCurrentTaskButton.Click += new System.EventHandler(this.WorkOnCurrentTaskButton_Click);
			this.WorkOnCurrentTaskButton.MouseEnter += new System.EventHandler(this.WorkOnCurrentTaskButton_MouseEnter);
			this.WorkOnCurrentTaskButton.MouseLeave += new System.EventHandler(this.WorkOnCurrentTaskButton_MouseLeave);
			// 
			// panelSimilarIncidents
			// 
			this.panelSimilarIncidents.Controls.Add(this.similarIncidentsUserControl);
			this.panelSimilarIncidents.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelSimilarIncidents.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(984, 0, true);
			this.panelSimilarIncidents.Name = "panelSimilarIncidents";
			this.panelSimilarIncidents.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 586, true);
			this.panelSimilarIncidents.TabIndex = 22;
			this.panelSimilarIncidents.Text = "Similar Incidents";
			// 
			// similarIncidentsUserControl
			// 
			this.similarIncidentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.similarIncidentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.similarIncidentsUserControl.Name = "similarIncidentsUserControl";
			this.similarIncidentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 586, true);
			this.similarIncidentsUserControl.TabIndex = 0;
			// 
			// splitterSimilarIncidents
			// 
			this.splitterSimilarIncidents.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitterSimilarIncidents.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(980, 0, true);
			this.splitterSimilarIncidents.Name = "splitterSimilarIncidents";
			this.splitterSimilarIncidents.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 586, true);
			this.splitterSimilarIncidents.MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.splitterSimilarIncidents.TabIndex = 21;
			// 
			// SupportIncidentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 669, true);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncident);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 700, true);
			this.Name = "SupportIncidentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DefectManagementTabPage.ResumeLayout(false);
			this.DefectManagementTabPage.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.zGroupBox7.ResumeLayout(false);
			this.zGroupBox7.PerformLayout();
			this.zDropEdit5.ResumeLayout(true);
			this.zDropEdit5.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.zGroupBox8.ResumeLayout(false);
			this.zGroupBox8.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.CriticalityDropEdit.ResumeLayout(true);
			this.CriticalityDropEdit.PerformLayout();
			this.defectMenuSectionDropEdit.ResumeLayout(true);
			this.defectMenuSectionDropEdit.PerformLayout();
			this.defectCr8ModuleDropEdit.ResumeLayout(true);
			this.defectCr8ModuleDropEdit.PerformLayout();
			this.defectCr9ModuleDropEdit.ResumeLayout(true);
			this.defectCr9ModuleDropEdit.PerformLayout();
			this.zDropEdit9.ResumeLayout(true);
			this.zDropEdit9.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DefectRelatedWorkItemsGrid.InnerGrid)).EndInit();
			this.DefectRelatedWorkItemsGrid.ResumeLayout(true);
			this.DefectRelatedWorkItemsGrid.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.DefectCausedByWIGuidFindBox.ResumeLayout(true);
			this.DefectCausedByWIGuidFindBox.PerformLayout();
			this.zDropEdit3.ResumeLayout(true);
			this.zDropEdit3.PerformLayout();
			this.zGuidFindBox2.ResumeLayout(true);
			this.zGuidFindBox2.PerformLayout();
			this.FeatureRequestTabPage.ResumeLayout(false);
			this.FeatureRequestTabPage.PerformLayout();
			this.featureSplitContainer.Panel1.ResumeLayout(false);
			this.featureSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.featureSplitContainer)).EndInit();
			this.featureSplitContainer.ResumeLayout(false);
			this.featureSplitContainer.PerformLayout();
			this.featureAnalysisGroupBox.ResumeLayout(false);
			this.featureAnalysisGroupBox.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.featureRequestAnalysisPage.ResumeLayout(false);
			this.featureRequestAnalysisPage.PerformLayout();
			this.ProjectRelatedIncidentNoteTabControl.ResumeLayout(false);
			this.ProjectRelatedIncidentNoteTabControl.PerformLayout();
			this.zTabPage5.ResumeLayout(false);
			this.zTabPage5.PerformLayout();
			this.BusinessRequirementRichTextBox.ResumeLayout(true);
			this.BusinessRequirementRichTextBox.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.SoftwareChangeRichTextBox.ResumeLayout(true);
			this.SoftwareChangeRichTextBox.PerformLayout();
			this.zTabPage6.ResumeLayout(false);
			this.zTabPage6.PerformLayout();
			this.TechnicalSpecificationRichTextBox.ResumeLayout(true);
			this.TechnicalSpecificationRichTextBox.PerformLayout();
			this.zTabPage7.ResumeLayout(false);
			this.zTabPage7.PerformLayout();
			this.FeatureRequestPrerequisitesRichTextBox.ResumeLayout(true);
			this.FeatureRequestPrerequisitesRichTextBox.PerformLayout();
			this.zTabPage8.ResumeLayout(false);
			this.zTabPage8.PerformLayout();
			this.InternalNotesRichTextBox.ResumeLayout(true);
			this.InternalNotesRichTextBox.PerformLayout();
			this.zTabPage2.ResumeLayout(false);
			this.zTabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedFeatureRequestsGrid.InnerGrid)).EndInit();
			this.RelatedFeatureRequestsGrid.ResumeLayout(true);
			this.RelatedFeatureRequestsGrid.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.ParentFeatureRequestFindBox.ResumeLayout(true);
			this.ParentFeatureRequestFindBox.PerformLayout();
			this.featureWorkItemsGroupBox.ResumeLayout(false);
			this.featureWorkItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeatureRequestWorkItemsGrid.InnerGrid)).EndInit();
			this.FeatureRequestWorkItemsGrid.ResumeLayout(true);
			this.FeatureRequestWorkItemsGrid.PerformLayout();
			this.FeatureCustomFieldsPanel.ResumeLayout(false);
			this.FeatureCustomFieldsPanel.PerformLayout();
			this.featureDetailsClientSplitContainer.Panel1.ResumeLayout(false);
			this.featureDetailsClientSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.featureDetailsClientSplitContainer)).EndInit();
			this.featureDetailsClientSplitContainer.ResumeLayout(false);
			this.featureDetailsClientSplitContainer.PerformLayout();
			this.zGroupBox6.ResumeLayout(false);
			this.zGroupBox6.PerformLayout();
			this.businessConsultantCodeFindBox.ResumeLayout(true);
			this.businessConsultantCodeFindBox.PerformLayout();
			this.relatedProjectGuidFindBox.ResumeLayout(true);
			this.relatedProjectGuidFindBox.PerformLayout();
			this.ClientRequiredDate.ResumeLayout(true);
			this.ClientRequiredDate.PerformLayout();
			this.FeatureRequestClientGroupBox.ResumeLayout(false);
			this.FeatureRequestClientGroupBox.PerformLayout();
			this.FeatureRequestClientAddressControl.ResumeLayout(true);
			this.FeatureRequestClientAddressControl.PerformLayout();
			this.FeatureRequestContactFindBox.ResumeLayout(true);
			this.FeatureRequestContactFindBox.PerformLayout();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.CustomFieldsGroupBox.PerformLayout();
			this.CustomFieldsControl.ResumeLayout(true);
			this.CustomFieldsControl.PerformLayout();
			this.EstGroupBox.ResumeLayout(false);
			this.EstGroupBox.PerformLayout();
			this.EstPaymentTermsDropEdit.ResumeLayout(true);
			this.EstPaymentTermsDropEdit.PerformLayout();
			this.EstRequestDate.ResumeLayout(true);
			this.EstRequestDate.PerformLayout();
			this.EstExpiryDate.ResumeLayout(true);
			this.EstExpiryDate.PerformLayout();
			this.EstSentDate.ResumeLayout(true);
			this.EstSentDate.PerformLayout();
			this.EstExpressDelCutOff.ResumeLayout(true);
			this.EstExpressDelCutOff.PerformLayout();
			this.EstCurrencyBox.ResumeLayout(true);
			this.EstCurrencyBox.PerformLayout();
			this.QteGroupBox.ResumeLayout(false);
			this.QteGroupBox.PerformLayout();
			this.QtePaymentTypeDropEdit.ResumeLayout(true);
			this.QtePaymentTypeDropEdit.PerformLayout();
			this.QtePaymentTermsDropEdit.ResumeLayout(true);
			this.QtePaymentTermsDropEdit.PerformLayout();
			this.QteDateDelivered.ResumeLayout(true);
			this.QteDateDelivered.PerformLayout();
			this.QteAcceptedDate.ResumeLayout(true);
			this.QteAcceptedDate.PerformLayout();
			this.QteExpiryDate.ResumeLayout(true);
			this.QteExpiryDate.PerformLayout();
			this.QteSentDate.ResumeLayout(true);
			this.QteSentDate.PerformLayout();
			this.QteCurrencyBox.ResumeLayout(true);
			this.QteCurrencyBox.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.IncidentCommentGroupBox.ResumeLayout(false);
			this.IncidentCommentGroupBox.PerformLayout();
			this.ClientAndContactDetailsGroupBox.ResumeLayout(false);
			this.ClientAndContactDetailsGroupBox.PerformLayout();
			this.SupportCompanyDropEdit.ResumeLayout(true);
			this.SupportCompanyDropEdit.PerformLayout();
			this.supportDatabaseDropEdit.ResumeLayout(true);
			this.supportDatabaseDropEdit.PerformLayout();
			this.supportEnterpriseGuidFindBox.ResumeLayout(true);
			this.supportEnterpriseGuidFindBox.PerformLayout();
			this.supportEnterpriseCodeFindBox.ResumeLayout(true);
			this.supportEnterpriseCodeFindBox.PerformLayout();
			this.supportClientGuidFindBox.ResumeLayout(true);
			this.supportClientGuidFindBox.PerformLayout();
			this.ContactPhoneDiallerUserControl.ResumeLayout(true);
			this.ContactPhoneDiallerUserControl.PerformLayout();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.ContactGuidFindBox.ResumeLayout(true);
			this.ContactGuidFindBox.PerformLayout();
			this.zPanel5.ResumeLayout(false);
			this.zPanel5.PerformLayout();
			this.IncidentLogGroupBox.ResumeLayout(false);
			this.IncidentLogGroupBox.PerformLayout();
			this.splitContainer5.Panel1.ResumeLayout(false);
			this.splitContainer5.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
			this.splitContainer5.ResumeLayout(false);
			this.splitContainer5.PerformLayout();
			this.IncidentDetailsGroupBox.ResumeLayout(false);
			this.IncidentDetailsGroupBox.PerformLayout();
			this.IncidentDetailsPanel.ResumeLayout(false);
			this.IncidentDetailsPanel.PerformLayout();
			this.CountryDropEdit.ResumeLayout(true);
			this.CountryDropEdit.PerformLayout();
			this.serviceTypeDropEdit.ResumeLayout(true);
			this.serviceTypeDropEdit.PerformLayout();
			this.ProductAreaDropEdit.ResumeLayout(true);
			this.ProductAreaDropEdit.PerformLayout();
			this.menuSectionDropEdit.ResumeLayout(true);
			this.menuSectionDropEdit.PerformLayout();
			this.cr8ModuleDropEdit.ResumeLayout(true);
			this.cr8ModuleDropEdit.PerformLayout();
			this.cr9ModuleDropEdit.ResumeLayout(true);
			this.cr9ModuleDropEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.sourceDropEdit.ResumeLayout(true);
			this.sourceDropEdit.PerformLayout();
			this.RequestedPriorityDropEdit.ResumeLayout(true);
			this.RequestedPriorityDropEdit.PerformLayout();
			this.statePanel.ResumeLayout(false);
			this.statePanel.PerformLayout();
			this.statusStateSplitContainer.Panel1.ResumeLayout(false);
			this.statusStateSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.statusStateSplitContainer)).EndInit();
			this.statusStateSplitContainer.ResumeLayout(false);
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.workflowStateGroupBox.ResumeLayout(false);
			this.workflowStateGroupBox.PerformLayout();
			this.panelSimilarIncidents.ResumeLayout(false);
			this.panelSimilarIncidents.PerformLayout();
			this.similarIncidentsUserControl.ResumeLayout(true);
			this.similarIncidentsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
