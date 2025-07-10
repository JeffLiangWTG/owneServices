namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EscalateIncidentPopupForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}

		protected Enterprise.ZArchitecture.ZTextBox CommentTextBox;

		protected override void InitializeComponent()
		{
			this.CommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CriticalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CriticalityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductAreaLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MenuItemLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MenuItemPathLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StageMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessageLabel
			// 
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 146, true);
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.MessageLabel.TabIndex = 11;
			this.MessageLabel.Text = "Internal comment:";
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 258, true);
			this.CloseButton.TabIndex = 13;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 258, true);
			this.CancelButtonX.TabIndex = 14;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 287, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(221);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(221);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction);
			// 
			// CommentTextBox
			// 
			this.CommentTextBox.AcceptsReturn = true;
			this.CommentTextBox.AcceptsTab = true;
			this.CommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CommentTextBox, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction)(null)).Comment)));
			this.CommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 166, true);
			this.CommentTextBox.Multiline = true;
			this.CommentTextBox.Name = "CommentTextBox";
			this.CommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 76, true);
			this.CommentTextBox.TabIndex = 12;
			// 
			// StageDropEdit
			// 
			this.StageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StageDropEdit, "EscalationStage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction)(null)).EscalationStage)));
			this.StageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 41, true);
			this.StageDropEdit.Name = "StageDropEdit";
			this.StageDropEdit.PreBoundMaxLength = 3;
			this.StageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.StageDropEdit.TabIndex = 2;
			// 
			// ModuleDropEdit
			// 
			this.ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModuleDropEdit, "SectionRequirementService");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction)(null)).SectionRequirementService)));
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 94, true);
			this.ModuleDropEdit.Name = "ModuleDropEdit";
			this.ModuleDropEdit.PreBoundMaxLength = 3;
			this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ModuleDropEdit.TabIndex = 6;
			// 
			// StageLabel
			// 
			this.StageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 41, true);
			this.StageLabel.Name = "StageLabel";
			this.StageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.StageLabel.TabIndex = 1;
			this.StageLabel.Text = "Stage:";
			// 
			// ModuleLabel
			// 
			this.ModuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 94, true);
			this.ModuleLabel.Name = "ModuleLabel";
			this.ModuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.ModuleLabel.TabIndex = 5;
			this.ModuleLabel.Text = "Menu Section:";
			// 
			// CriticalityDropEdit
			// 
			this.CriticalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CriticalityDropEdit, "EscalationCriticality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction)(null)).EscalationCriticality)));
			this.CriticalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 67, true);
			this.CriticalityDropEdit.Name = "CriticalityDropEdit";
			this.CriticalityDropEdit.PreBoundMaxLength = 3;
			this.CriticalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.CriticalityDropEdit.TabIndex = 4;
			// 
			// CriticalityLabel
			// 
			this.CriticalityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 67, true);
			this.CriticalityLabel.Name = "CriticalityLabel";
			this.CriticalityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CriticalityLabel.TabIndex = 3;
			this.CriticalityLabel.Text = "Criticality:";
			// 
			// ProductAreaDropEdit
			// 
			this.ProductAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductAreaDropEdit, "ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction)(null)).ProductArea)));
			this.ProductAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 94, true);
			this.ProductAreaDropEdit.Name = "ProductAreaDropEdit";
			this.ProductAreaDropEdit.PreBoundMaxLength = 3;
			this.ProductAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ProductAreaDropEdit.TabIndex = 8;
			// 
			// ProductAreaLabel
			// 
			this.ProductAreaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 94, true);
			this.ProductAreaLabel.Name = "ProductAreaLabel";
			this.ProductAreaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.ProductAreaLabel.TabIndex = 7;
			this.ProductAreaLabel.Text = "Product Area:";
			// 
			// MenuItemLabel
			// 
			this.MenuItemLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 122, true);
			this.MenuItemLabel.Name = "MenuItemLabel";
			this.MenuItemLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.MenuItemLabel.TabIndex = 9;
			this.MenuItemLabel.Text = "Menu Item:";
			// 
			// MenuItemPathLabel
			// 
			this.BindingSource.SetBindingMember(this.MenuItemPathLabel, "MenuItemWithPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction)(null)).MenuItemWithPath)));
			this.MenuItemPathLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 122, true);
			this.MenuItemPathLabel.Name = "MenuItemPathLabel";
			this.MenuItemPathLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.MenuItemPathLabel.TabIndex = 10;
			// 
			// StageMessageLabel
			// 
			this.StageMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StageMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.StageMessageLabel.Name = "StageMessageLabel";
			this.StageMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 29, true);
			this.StageMessageLabel.TabIndex = 0;
			this.StageMessageLabel.Text = "Please specify a stage. Optionally, specify some comments.";
			// 
			// EscalateIncidentPopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 311, true);
			this.Controls.Add(this.StageMessageLabel);
			this.Controls.Add(this.MenuItemPathLabel);
			this.Controls.Add(this.MenuItemLabel);
			this.Controls.Add(this.ProductAreaLabel);
			this.Controls.Add(this.ProductAreaDropEdit);
			this.Controls.Add(this.CriticalityLabel);
			this.Controls.Add(this.CriticalityDropEdit);
			this.Controls.Add(this.ModuleLabel);
			this.Controls.Add(this.StageLabel);
			this.Controls.Add(this.ModuleDropEdit);
			this.Controls.Add(this.StageDropEdit);
			this.Controls.Add(this.CommentTextBox);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentEscalateAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 350, true);
			this.Name = "EscalateIncidentPopupForm";
			this.Text = "Escalate Incident";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.CommentTextBox, 0);
			this.Controls.SetChildIndex(this.StageDropEdit, 0);
			this.Controls.SetChildIndex(this.ModuleDropEdit, 0);
			this.Controls.SetChildIndex(this.StageLabel, 0);
			this.Controls.SetChildIndex(this.ModuleLabel, 0);
			this.Controls.SetChildIndex(this.CriticalityDropEdit, 0);
			this.Controls.SetChildIndex(this.CriticalityLabel, 0);
			this.Controls.SetChildIndex(this.ProductAreaDropEdit, 0);
			this.Controls.SetChildIndex(this.ProductAreaLabel, 0);
			this.Controls.SetChildIndex(this.MenuItemLabel, 0);
			this.Controls.SetChildIndex(this.MenuItemPathLabel, 0);
			this.Controls.SetChildIndex(this.StageMessageLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit StageDropEdit;
		private ZArchitecture.GUI.ZDropEdit ModuleDropEdit;
		private ZArchitecture.ZLabel StageLabel;
		private ZArchitecture.ZLabel ModuleLabel;
		private ZArchitecture.GUI.ZDropEdit CriticalityDropEdit;
		private ZArchitecture.ZLabel CriticalityLabel;
		private ZArchitecture.GUI.ZDropEdit ProductAreaDropEdit;
		private ZArchitecture.ZLabel ProductAreaLabel;
		private ZArchitecture.ZLabel MenuItemLabel;
		private ZArchitecture.ZLabel MenuItemPathLabel;
		private ZArchitecture.ZLabel StageMessageLabel;
	}
}