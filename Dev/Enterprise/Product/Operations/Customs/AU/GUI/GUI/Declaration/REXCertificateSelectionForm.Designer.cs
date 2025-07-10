namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class REXCertificateSelectionForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CertificatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificatesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			this.CertificatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 198, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.IsCaptionOverridden = true;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 167, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.Text = "OK";
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Cancel_Button.IsCaptionOverridden = true;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(661, 167, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.Text = "Cancel";
			this.Cancel_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// CertificatesGroupBox
			// 
			this.CertificatesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CertificatesGroupBox.Controls.Add(this.CertificatesGrid);
			this.CertificatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CertificatesGroupBox.Name = "CertificatesGroupBox";
			this.CertificatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 158, true);
			this.CertificatesGroupBox.TabIndex = 1;
			this.CertificatesGroupBox.TabStop = false;
			this.CertificatesGroupBox.Text = "Select Certificate(s)";
			// 
			// CertificatesGrid
			// 
			this.CertificatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CertificatesGrid, "CertificateReissueRequests");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader)(null)).CertificateReissueRequests)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueRequest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader)(null)).CertificateReissueRequests)).SyncRoot)).CertificateNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueRequest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader)(null)).CertificateReissueRequests)).SyncRoot)).Certificates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueRequest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader)(null)).CertificateReissueRequests)).SyncRoot)).ReissueReason)));
			this.CertificatesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Certificates";
			zDropEditColumnStyleInfo1.Caption = "Name";
			zDropEditColumnStyleInfo1.ColumnName = "CertificateNumber";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.Caption = "Reissue Reason";
			zTextBoxColumnStyleInfo1.ColumnName = "ReissueReason";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.CertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CertificatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGrid.GridId = "20FE1AC1-76C4-4663-AFA9-FC66DCE1FB05";
			this.CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificatesGrid.LayoutKey = "CertificatesGrid";
			this.CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CertificatesGrid.Name = "CertificatesGrid";
			this.CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 139, true);
			this.CertificatesGrid.TabIndex = 0;
			// 
			// REXCertificateSelectionForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 222, true);
			this.Controls.Add(this.CertificatesGroupBox);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CertificateReissueHeader";
			this.MinimizeBox = false;
			this.Name = "REXCertificateSelectionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Certificates";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.CertificatesGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificatesGroupBox.ResumeLayout(false);
			this.CertificatesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			this.CertificatesGrid.ResumeLayout(false);
			this.CertificatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		private ZArchitecture.GUI.ZGroupBox CertificatesGroupBox;
		private ZArchitecture.ZGrid CertificatesGrid;
	}
}
