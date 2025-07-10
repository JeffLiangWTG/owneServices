using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection
{
	public partial class AgentPostingOptionSelectionForm
	{


		#region Designer generated code

		new void InitializeComponent()
		{
			this.CancelPostingButton = new ZArchitecture.GUI.ZButton();
			this.PostingInstructionLabel = new ZArchitecture.ZLabel();
			this.AgentBeingPostedFindBox = new MasterFiles.GUI.ZOrganisationFindBox();
			this.ContinueButton = new ZArchitecture.GUI.ZButton();
			this.zCalcEdit1 = new ZArchitecture.ZCalcEdit();
			this.CurrencySelectedDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.zLabel3 = new ZArchitecture.ZLabel();
			this.zLabel4 = new ZArchitecture.ZLabel();
			this.InstructionsPanel = new ZArchitecture.GUI.ZPanel();
			this.PostingConsolAgentsInvoiceLabel = new ZArchitecture.ZLabel();
			this.PostingOptionsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.ContactDropEdit = new ZArchitecture.GUI.ZGuidDropEdit();
			this.AddressGuidDropEdit = new ZArchitecture.GUI.ZGuidDropEdit();
			this.zDropEdit1 = new ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgentBeingPostedFindBox.SuspendLayout();
			this.CurrencySelectedDropEdit.SuspendLayout();
			this.InstructionsPanel.SuspendLayout();
			this.PostingOptionsGroupBox.SuspendLayout();
			this.ContactDropEdit.SuspendLayout();
			this.AddressGuidDropEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 313, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AgentPostingOptionSelector);
			// 
			// CancelPostingButton
			// 
			this.CancelPostingButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|7932b659-b7f3-4833-ac04-0b2adef4ea50", "C&ancel Posting", "Cancel Posting.");
			this.CancelPostingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 285, true);
			this.CancelPostingButton.Name = "CancelPostingButton";
			this.CancelPostingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 22, true);
			this.CancelPostingButton.TabIndex = 4;
			this.CancelPostingButton.ToolTipCaption = null;
			this.CancelPostingButton.Click += new System.EventHandler(this.CancelPostingButton_Click);
			// 
			// PostingInstructionLabel
			// 
			this.PostingInstructionLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|25b6565d-6f71-4152-9504-2cfced9acc1b", "Please nominate the currency and exchange rate, addresses and contact for posting the agent invoice.");
			this.PostingInstructionLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PostingInstructionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.PostingInstructionLabel.Name = "PostingInstructionLabel";
			this.PostingInstructionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 15, true);
			this.PostingInstructionLabel.TabIndex = 3;
			// 
			// AgentBeingPostedFindBox
			// 
			this.AgentBeingPostedFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentBeingPostedFindBox, "AgentBeingPosted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AgentPostingOptionSelector)(null)).AgentBeingPosted)));
			this.AgentBeingPostedFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|dcabf44d-eb5e-43dc-85ef-2ed5d70fabeb", "Agent being posted");
			this.AgentBeingPostedFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.AgentBeingPostedFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 22, true);
			this.AgentBeingPostedFindBox.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.AgentBeingPostedFindBox.Name = "AgentBeingPostedFindBox";
			this.AgentBeingPostedFindBox.ShouldResize = true;
			this.AgentBeingPostedFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 17, true);
			this.AgentBeingPostedFindBox.TabIndex = 1;
			// 
			// ContinueButton
			// 
			this.ContinueButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|ad4a6d3f-9c23-4b21-a247-ee31080acc79", "&Continue");
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 285, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.ContinueButton.TabIndex = 3;
			this.ContinueButton.ToolTipCaption = null;
			this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// zCalcEdit1
			// 
			this.zCalcEdit1.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AgentPostingOptionSelector)(null)).ExchangeRate)));
			this.zCalcEdit1.BindToDecimalPlaces = "ExchangeRateDecimalPlaces";
			this.zCalcEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|6f4325a9-7bab-4571-9f16-9735e2254606", "Exchange Rate");
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 92, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.zCalcEdit1.TabIndex = 4;
			this.zCalcEdit1.Text = "0.00";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrencySelectedDropEdit
			// 
			this.CurrencySelectedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencySelectedDropEdit, "Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AgentPostingOptionSelector)(null)).Currency)));
			this.CurrencySelectedDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|3408fd8b-656e-4332-a07b-8120802b1a5b", "Currency");
			this.CurrencySelectedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 45, true);
			this.CurrencySelectedDropEdit.Name = "CurrencySelectedDropEdit";
			this.CurrencySelectedDropEdit.PreBoundMaxLength = 3;
			this.CurrencySelectedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 17, true);
			this.CurrencySelectedDropEdit.TabIndex = 2;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|f9c1d5fe-1911-4ae2-bc7a-2fe31e5032bf", "Currently, charges are saved in a mix of currencies, addresses and contacts.");
			this.zLabel2.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 34, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 15, true);
			this.zLabel2.TabIndex = 3;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|5fce46d6-5184-4983-bb11-e6369859b9b6", "You must select a common currency and address for posting these charges. Contact is optional.");
			this.zLabel3.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 49, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 15, true);
			this.zLabel3.TabIndex = 3;
			// 
			// zLabel4
			// 
			this.zLabel4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|dbe586b9-1e9d-42cf-b389-8ae7d7bc4942", "This application will convert the prepared charges into the nominated posting currency, address and contact.");
			this.zLabel4.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 64, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 15, true);
			this.zLabel4.TabIndex = 3;
			// 
			// InstructionsPanel
			// 
			this.InstructionsPanel.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionsPanel.Controls.Add(this.zLabel4);
			this.InstructionsPanel.Controls.Add(this.zLabel2);
			this.InstructionsPanel.Controls.Add(this.zLabel3);
			this.InstructionsPanel.Controls.Add(this.PostingInstructionLabel);
			this.InstructionsPanel.Controls.Add(this.PostingConsolAgentsInvoiceLabel);
			this.InstructionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.InstructionsPanel.Name = "InstructionsPanel";
			this.InstructionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 80, true);
			this.InstructionsPanel.TabIndex = 1;
			// 
			// PostingConsolAgentsInvoiceLabel
			// 
			this.PostingConsolAgentsInvoiceLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|7fdbe46a-a750-45e1-b373-747377b665df", "You are posting an invoice for the Consol Receiving Agent.");
			this.PostingConsolAgentsInvoiceLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PostingConsolAgentsInvoiceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 5, true);
			this.PostingConsolAgentsInvoiceLabel.Name = "PostingConsolAgentsInvoiceLabel";
			this.PostingConsolAgentsInvoiceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 14, true);
			this.PostingConsolAgentsInvoiceLabel.TabIndex = 3;
			// 
			// PostingOptionsGroupBox
			// 
			this.PostingOptionsGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PostingOptionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|39f2bc6f-eda0-4eff-ba94-646a5cc0cd0b", "Posting Options");
			this.PostingOptionsGroupBox.Controls.Add(this.ContactDropEdit);
			this.PostingOptionsGroupBox.Controls.Add(this.AddressGuidDropEdit);
			this.PostingOptionsGroupBox.Controls.Add(this.zDropEdit1);
			this.PostingOptionsGroupBox.Controls.Add(this.zCalcEdit1);
			this.PostingOptionsGroupBox.Controls.Add(this.CurrencySelectedDropEdit);
			this.PostingOptionsGroupBox.Controls.Add(this.AgentBeingPostedFindBox);
			this.PostingOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 93, true);
			this.PostingOptionsGroupBox.Name = "PostingOptionsGroupBox";
			this.PostingOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 174, true);
			this.PostingOptionsGroupBox.TabIndex = 2;
			this.PostingOptionsGroupBox.TabStop = false;
			// 
			// ContactDropEdit
			// 
			this.ContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactDropEdit, "Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AgentPostingOptionSelector)(null)).Contact)));
			this.ContactDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9801cf44-450a-44e1-b387-912613939676", "Contact");
			this.ContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 144, true);
			this.ContactDropEdit.Name = "ContactDropEdit";
			this.ContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 17, true);
			this.ContactDropEdit.TabIndex = 29;
			// 
			// AddressGuidDropEdit
			// 
			this.AddressGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressGuidDropEdit, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AgentPostingOptionSelector)(null)).Address)));
			this.AddressGuidDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f36fbedb-26f7-4e30-9a94-90a0a3695734", "Address");
			this.AddressGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 118, true);
			this.AddressGuidDropEdit.Name = "AddressGuidDropEdit";
			this.AddressGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 17, true);
			this.AddressGuidDropEdit.TabIndex = 28;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "ExchangeRateCalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AgentPostingOptionSelector)(null)).ExchangeRateCalculationMethod)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|30bcdf53-f55d-497e-865e-71b1bc590543", "Method", "Calculation Method", "Exchange Rate Calculation Method", "");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 68, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 17, true);
			this.zDropEdit1.TabIndex = 3;
			// 
			// AgentPostingOptionSelectionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AgentPostingOptionSelectionForm|7e942097-b68d-4eb5-aadf-8ad814259067", "Agent Invoice Option Selection");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 335, true);
			this.Controls.Add(this.PostingOptionsGroupBox);
			this.Controls.Add(this.InstructionsPanel);
			this.Controls.Add(this.ContinueButton);
			this.Controls.Add(this.CancelPostingButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(AgentPostingOptionSelector);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingCurrencySelection" +
	".AgentPostingCurrencySelector";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 312, true);
			this.Name = "AgentPostingOptionSelectionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelPostingButton, 0);
			this.Controls.SetChildIndex(this.ContinueButton, 0);
			this.Controls.SetChildIndex(this.InstructionsPanel, 0);
			this.Controls.SetChildIndex(this.PostingOptionsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgentBeingPostedFindBox.ResumeLayout(true);
			this.AgentBeingPostedFindBox.PerformLayout();
			this.CurrencySelectedDropEdit.ResumeLayout(true);
			this.CurrencySelectedDropEdit.PerformLayout();
			this.InstructionsPanel.ResumeLayout(false);
			this.InstructionsPanel.PerformLayout();
			this.PostingOptionsGroupBox.ResumeLayout(false);
			this.PostingOptionsGroupBox.PerformLayout();
			this.ContactDropEdit.ResumeLayout(true);
			this.ContactDropEdit.PerformLayout();
			this.AddressGuidDropEdit.ResumeLayout(true);
			this.AddressGuidDropEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}