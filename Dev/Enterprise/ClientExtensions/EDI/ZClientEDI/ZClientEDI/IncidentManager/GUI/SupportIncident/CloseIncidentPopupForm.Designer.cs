namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class CloseIncidentPopupForm
	{
		protected Enterprise.ZArchitecture.ZTextBox resolutionCommentTextBox;
		protected Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.ZLabel zLabel1;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CriticalityDropEdit;
		Enterprise.ZArchitecture.GUI.Tools.SpellChecker spellChecker;
		private Enterprise.ZArchitecture.ZLabel ProductAreaLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ProductAreaDropEdit;
		private Enterprise.ZArchitecture.ZLabel ModuleLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ModuleDropEdit;

		protected override void InitializeComponent()
		{
			this.resolutionCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.CriticalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductAreaLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit1.SuspendLayout();
			this.CriticalityDropEdit.SuspendLayout();
			this.ProductAreaDropEdit.SuspendLayout();
			this.ModuleDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// MessageLabel
			//
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 28, true);
			this.MessageLabel.Text = "Please specify a resolution method. Optionally, specify some resolution comments." +
	"";
			//
			// CloseButton
			//
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 258, true);
			this.CloseButton.TabIndex = 15;
			//
			// CancelButtonX
			//
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 258, true);
			this.CancelButtonX.TabIndex = 16;
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 286, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 24, true);
			this.MainStatusBar.TabIndex = 17;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction);
			//
			// resolutionCommentTextBox
			//
			this.resolutionCommentTextBox.AcceptsReturn = true;
			this.resolutionCommentTextBox.AcceptsTab = true;
			this.resolutionCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.resolutionCommentTextBox, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).Comment)));
			this.resolutionCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.resolutionCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 137, true);
			this.resolutionCommentTextBox.Multiline = true;
			this.resolutionCommentTextBox.Name = "resolutionCommentTextBox";
			this.resolutionCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.resolutionCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 116, true);
			this.resolutionCommentTextBox.TabIndex = 12;
			this.resolutionCommentTextBox.TextChanged += new System.EventHandler(this.resolutionCommentTextBox_TextChanged);
			//
			// zLabel1
			//
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 44, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 14, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Method:";
			this.zLabel1.UseMnemonic = false;
			//
			// zLabel2
			//
			this.zLabel2.AutoSize = true;
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 121, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 14, true);
			this.zLabel2.TabIndex = 11;
			this.zLabel2.Text = "Customer-facing resolution message:";
			this.zLabel2.UseMnemonic = false;
			//
			// zDropEdit1
			//
			this.zDropEdit1.AllowDrop = true;
			this.zDropEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEdit1, "ResolutionMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).ResolutionMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).ActiveCloseStatusDispositionList)));
			this.zDropEdit1.BindToList = "ActiveCloseStatusDispositionList";
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 41, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 18, true);
			this.zDropEdit1.TabIndex = 2;
			//
			// zLabel3
			//
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 66, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 21, true);
			this.zLabel3.TabIndex = 13;
			this.zLabel3.Text = "Criticality:";
			this.zLabel3.UseMnemonic = false;
			//
			// CriticalityDropEdit
			//
			this.CriticalityDropEdit.AllowDrop = true;
			this.CriticalityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CriticalityDropEdit, "Criticality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).Criticality)));
			this.CriticalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 67, true);
			this.CriticalityDropEdit.Name = "CriticalityDropEdit";
			this.CriticalityDropEdit.PreBoundMaxLength = 3;
			this.CriticalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 18, true);
			this.CriticalityDropEdit.TabIndex = 14;
			//
			// ProductAreaLabel
			//
			this.ProductAreaLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ProductAreaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 94, true);
			this.ProductAreaLabel.Name = "ProductAreaLabel";
			this.ProductAreaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.ProductAreaLabel.TabIndex = 20;
			this.ProductAreaLabel.Text = "Product Area:";
			this.ProductAreaLabel.UseMnemonic = false;
			//
			// ProductAreaDropEdit
			//
			this.ProductAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductAreaDropEdit, "ProductArea");
			this.ProductAreaDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).ProductArea)));
			this.ProductAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 94, true);
			this.ProductAreaDropEdit.Name = "ProductAreaDropEdit";
			this.ProductAreaDropEdit.PreBoundMaxLength = 3;
			this.ProductAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 18, true);
			this.ProductAreaDropEdit.TabIndex = 21;
			//
			// ModuleLabel
			//
			this.ModuleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ModuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 94, true);
			this.ModuleLabel.Name = "ModuleLabel";
			this.ModuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 20, true);
			this.ModuleLabel.TabIndex = 18;
			this.ModuleLabel.Text = "Section:";
			this.ModuleLabel.UseMnemonic = false;
			//
			// ModuleDropEdit
			//
			this.ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModuleDropEdit, "SectionRequirementService");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).SectionRequirementService)));
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 94, true);
			this.ModuleDropEdit.Name = "ModuleDropEdit";
			this.ModuleDropEdit.PreBoundMaxLength = 3;
			this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 18, true);
			this.ModuleDropEdit.TabIndex = 19;
			//
			// CloseIncidentPopupForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 310, true);
			this.Controls.Add(this.ProductAreaLabel);
			this.Controls.Add(this.ProductAreaDropEdit);
			this.Controls.Add(this.ModuleLabel);
			this.Controls.Add(this.ModuleDropEdit);
			this.Controls.Add(this.CriticalityDropEdit);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.resolutionCommentTextBox);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 310, true);
			this.Name = "CloseIncidentPopupForm";
			this.Text = "Close Incident";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.resolutionCommentTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.CriticalityDropEdit, 0);
			this.Controls.SetChildIndex(this.ModuleDropEdit, 0);
			this.Controls.SetChildIndex(this.ModuleLabel, 0);
			this.Controls.SetChildIndex(this.ProductAreaDropEdit, 0);
			this.Controls.SetChildIndex(this.ProductAreaLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.CriticalityDropEdit.ResumeLayout(true);
			this.CriticalityDropEdit.PerformLayout();
			this.ProductAreaDropEdit.ResumeLayout(true);
			this.ProductAreaDropEdit.PerformLayout();
			this.ModuleDropEdit.ResumeLayout(true);
			this.ModuleDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
