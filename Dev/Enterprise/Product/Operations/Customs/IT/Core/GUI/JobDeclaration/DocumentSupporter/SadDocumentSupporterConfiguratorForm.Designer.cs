
namespace Enterprise.Customs.IT.GUI
{
	partial class SadDocumentSupporterConfiguratorForm
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
			this.bGMReferenceToPrintDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.layoutStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.acceptButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bGMReferenceToPrintDropEdit.SuspendLayout();
			this.layoutStyleDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclarationSadDocumentSupporter);
			// 
			// bGMReferenceToPrintDropEdit
			// 
			this.bGMReferenceToPrintDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bGMReferenceToPrintDropEdit, "BGMReferenceToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclarationSadDocumentSupporter)(null)).BGMReferenceToPrint)));
			this.bGMReferenceToPrintDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("96B0D841-B8BE-4CEE-9385-8198EA11A3B2", "Entry");
			this.bGMReferenceToPrintDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 25, true);
			this.bGMReferenceToPrintDropEdit.Name = "bGMReferenceToPrintDropEdit";
			this.bGMReferenceToPrintDropEdit.ShouldResizeByMaxLength = true;
			this.bGMReferenceToPrintDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.bGMReferenceToPrintDropEdit.TabIndex = 0;
			// 
			// layoutStyleDropEdit
			// 
			this.layoutStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.layoutStyleDropEdit, "LayoutStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclarationSadDocumentSupporter)(null)).LayoutStyle)));
			this.layoutStyleDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("58D42670-E121-4B5E-B4CE-F11536A89923", "Layout Style");
			this.layoutStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 51, true);
			this.layoutStyleDropEdit.Name = "layoutStyleDropEdit";
			this.layoutStyleDropEdit.ShouldResizeByMaxLength = true;
			this.layoutStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.layoutStyleDropEdit.TabIndex = 1;
			// 
			// acceptButton
			// 
			this.acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.acceptButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("27a28894-b0ae-4cd3-99ce-5315943e6d6e", "&Accept");
			this.acceptButton.IsCaptionOverridden = false;
			this.acceptButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 96, true);
			this.acceptButton.Name = "acceptButton";
			this.acceptButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.acceptButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.acceptButton.TabIndex = 2;
			this.acceptButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.acceptButton.ToolTipCaption = null;
			this.acceptButton.Click += new System.EventHandler(this.AcceptButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("7ccb5f81-1e2e-42e1-9cad-a0befcd47aad", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 96, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			// 
			// SadDocumentSupporterConfiguratorForm
			// 
			this.AcceptButton = this.acceptButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 149, true);
			this.Controls.Add(this.bGMReferenceToPrintDropEdit);
			this.Controls.Add(this.layoutStyleDropEdit);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.acceptButton);
			this.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclarationSadDocumentSupporter);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SadDocumentSupporterConfiguratorForm";
			this.Text = "SadDocumentSupporterConfiguratorForm";
			this.Controls.SetChildIndex(this.acceptButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.layoutStyleDropEdit, 0);
			this.Controls.SetChildIndex(this.bGMReferenceToPrintDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bGMReferenceToPrintDropEdit.ResumeLayout(true);
			this.bGMReferenceToPrintDropEdit.PerformLayout();
			this.layoutStyleDropEdit.ResumeLayout(true);
			this.layoutStyleDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZDropEdit bGMReferenceToPrintDropEdit;
		private ZArchitecture.GUI.ZDropEdit layoutStyleDropEdit;
		private ZArchitecture.GUI.ZButton acceptButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		#endregion
	}
}
