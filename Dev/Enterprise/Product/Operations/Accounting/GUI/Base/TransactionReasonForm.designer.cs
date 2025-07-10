using Enterprise.Core;
using Enterprise.Environment;

namespace Enterprise.Accounting.GUI.Base
{
	partial class TransactionReasonForm
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
		new void InitializeComponent()
		{
			this.OKReasonButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelReasonButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReversingReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SupportingDocumentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupportingDocumentNumLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsAmendInFullCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 129, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder);
			// 
			// OKReasonButton
			// 
			this.OKReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKReasonButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|638a3fe3-cd9f-4b54-a56a-33172994aabe", "OK", "&OK", "");
			this.OKReasonButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 121, true);
			this.OKReasonButton.Name = "OKReasonButton";
			this.OKReasonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKReasonButton.TabIndex = 6;
			this.OKReasonButton.Click += new System.EventHandler(this.OKReasonButton_Click);
			// 
			// CancelReasonButton
			// 
			this.CancelReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelReasonButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|4256b02a-9e2a-4a0a-aba4-802bd648d732", "Cancel", "Cancel", "");
			this.CancelReasonButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 121, true);
			this.CancelReasonButton.Name = "CancelReasonButton";
			this.CancelReasonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelReasonButton.TabIndex = 7;
			this.CancelReasonButton.Click += new System.EventHandler(this.CancelReasonButton_Click);
			// 
			// ReasonTextBox
			// 
			this.ReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).Reason)));
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 58, true);
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 17, true);
			this.ReasonTextBox.TabIndex = 3;
			// 
			// ReversingReasonCodeDropEdit
			// 
			this.ReversingReasonCodeDropEdit.AllowDrop = true;
			this.ReversingReasonCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReversingReasonCodeDropEdit, "Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).Code)));
			this.ReversingReasonCodeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|0dc15545-b582-4cb6-8d79-e4ff2df86fda", "Code", "Code", "");
			this.ReversingReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 34, true);
			this.ReversingReasonCodeDropEdit.Name = "ReversingReasonCodeDropEdit";
			this.ReversingReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 17, true);
			this.ReversingReasonCodeDropEdit.TabIndex = 2;
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 4, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 23, true);
			this.ReasonLabel.TabIndex = 1;
			// 
			// SupportingDocumentNumberTextBox
			// 
			this.SupportingDocumentNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupportingDocumentNumberTextBox, "SupportingDocumentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).SupportingDocumentNumber)));
			this.SupportingDocumentNumberTextBox.CaptionResourceString = null;
			this.SupportingDocumentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 79, true);
			this.SupportingDocumentNumberTextBox.Name = "SupportingDocumentNumberTextBox";
			this.SupportingDocumentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 17, true);
			this.SupportingDocumentNumberTextBox.TabIndex = 5;
			this.SupportingDocumentNumberTextBox.Visible = false;
			// 
			// SupportingDocumentNumLabel
			// 
			this.SupportingDocumentNumLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|FF48A28F-38F5-410F-B9F8-91A529238808", "Supp. Doc Num.", "Supporting Document Number");
			this.SupportingDocumentNumLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SupportingDocumentNumLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 77, true);
			this.SupportingDocumentNumLabel.Name = "SupportingDocumentNumLabel";
			this.SupportingDocumentNumLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.SupportingDocumentNumLabel.TabIndex = 4;
			this.SupportingDocumentNumLabel.Visible = false;
			// 
			// AmendInFullCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsAmendInFullCheckBox, "IsAmendInFull");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).IsAmendInFull)));
			this.IsAmendInFullCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|2519B82E-E27C-432B-BDB8-D0F25AD177D8", "Amend in Full", "");
			this.IsAmendInFullCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 97, true);
			this.IsAmendInFullCheckBox.Name = "IsAmendInFullCheckBox";
			this.IsAmendInFullCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.IsAmendInFullCheckBox.TabIndex = 5;
			this.IsAmendInFullCheckBox.Visible = false;
			// 
			// TransactionReasonForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 129, true);
			this.ControlBox = false;
			this.Controls.Add(this.SupportingDocumentNumLabel);
			this.Controls.Add(this.SupportingDocumentNumberTextBox);
			this.Controls.Add(this.ReasonLabel);
			this.Controls.Add(this.ReversingReasonCodeDropEdit);
			this.Controls.Add(this.ReasonTextBox);
			this.Controls.Add(this.OKReasonButton);
			this.Controls.Add(this.CancelReasonButton);
			this.Controls.Add(this.IsAmendInFullCheckBox);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 187, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 187, true);
			this.Name = "ReversingReasonForm";
			this.Text = "ReversingReasonForm";
			this.Controls.SetChildIndex(this.CancelReasonButton, 0);
			this.Controls.SetChildIndex(this.OKReasonButton, 0);
			this.Controls.SetChildIndex(this.ReasonTextBox, 0);
			this.Controls.SetChildIndex(this.ReversingReasonCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.ReasonLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SupportingDocumentNumberTextBox, 0);
			this.Controls.SetChildIndex(this.SupportingDocumentNumLabel, 0);
			this.Controls.SetChildIndex(this.IsAmendInFullCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton OKReasonButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelReasonButton;
		private Enterprise.ZArchitecture.ZTextBox ReasonTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ReversingReasonCodeDropEdit;
		private ZArchitecture.ZLabel ReasonLabel;
		private ZArchitecture.ZTextBox SupportingDocumentNumberTextBox;
		private ZArchitecture.ZLabel SupportingDocumentNumLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsAmendInFullCheckBox;
	}
}
