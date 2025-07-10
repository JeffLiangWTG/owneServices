namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class DocumentsUserControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DescriptionWordWrapingTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			this.ReferenceWordWrapingTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			this.CertificatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			this.DocumentsGrid.SuspendLayout();
			this.TypeCodeFindBox.SuspendLayout();
			this.CertificatesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.DocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocumentsGrid, "Documents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)).SyncRoot)).CSI_ReferenceNumber)));
			this.DocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_SubType";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocumentsGrid.GridId = "e0afb17c-6d43-49c8-a6d5-ae03948aa164";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "DocumentsGrid";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 120, true);
			this.DocumentsGrid.TabIndex = 0;
			// 
			// TypeCodeFindBox
			// 
			this.TypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeCodeFindBox, "Documents.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)).SyncRoot)).CSI_SubType)));
			this.TypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 146, true);
			this.TypeCodeFindBox.Name = "TypeCodeFindBox";
			this.TypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TypeCodeFindBox.ParentType = null;
			this.TypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.TypeCodeFindBox.TabIndex = 1;
			// 
			// DescriptionWordWrapingTextBox
			// 
			this.DescriptionWordWrapingTextBox.AcceptsReturn = true;
			this.DescriptionWordWrapingTextBox.AllowDrop = true;
			this.DescriptionWordWrapingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionWordWrapingTextBox, "Documents.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)).SyncRoot)).CSI_Description)));
			this.DescriptionWordWrapingTextBox.CaptionResourceString = null;
			this.DescriptionWordWrapingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.DescriptionWordWrapingTextBox, 4);
			this.DescriptionWordWrapingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 172, true);
			this.DescriptionWordWrapingTextBox.Multiline = true;
			this.DescriptionWordWrapingTextBox.Name = "DescriptionWordWrapingTextBox";
			this.DescriptionWordWrapingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionWordWrapingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 58, true);
			this.DescriptionWordWrapingTextBox.TabIndex = 2;
			// 
			// ReferenceWordWrapingTextBox
			// 
			this.ReferenceWordWrapingTextBox.AcceptsReturn = true;
			this.ReferenceWordWrapingTextBox.AllowDrop = true;
			this.ReferenceWordWrapingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReferenceWordWrapingTextBox, "Documents.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Documents)).SyncRoot)).CSI_ReferenceNumber)));
			this.ReferenceWordWrapingTextBox.CaptionResourceString = null;
			this.ReferenceWordWrapingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.ReferenceWordWrapingTextBox, 4);
			this.ReferenceWordWrapingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 236, true);
			this.ReferenceWordWrapingTextBox.Multiline = true;
			this.ReferenceWordWrapingTextBox.Name = "ReferenceWordWrapingTextBox";
			this.ReferenceWordWrapingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ReferenceWordWrapingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 58, true);
			this.ReferenceWordWrapingTextBox.TabIndex = 3;
			// 
			// CertificatesGroupBox
			// 
			this.CertificatesGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("7B8CACAC-81DC-48C0-8EFA-AC056F4B772D", "Certificates");
			this.CertificatesGroupBox.Controls.Add(this.DocumentsGrid);
			this.CertificatesGroupBox.Controls.Add(this.TypeCodeFindBox);
			this.CertificatesGroupBox.Controls.Add(this.DescriptionWordWrapingTextBox);
			this.CertificatesGroupBox.Controls.Add(this.ReferenceWordWrapingTextBox);
			this.CertificatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificatesGroupBox.Name = "CertificatesGroupBox";
			this.CertificatesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.CertificatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 310, true);
			this.CertificatesGroupBox.TabIndex = 0;
			this.CertificatesGroupBox.TabStop = false;
			// 
			// DocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CertificatesGroupBox);
			this.Name = "DocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			this.DocumentsGrid.ResumeLayout(false);
			this.DocumentsGrid.PerformLayout();
			this.TypeCodeFindBox.ResumeLayout(true);
			this.TypeCodeFindBox.PerformLayout();
			this.CertificatesGroupBox.ResumeLayout(false);
			this.CertificatesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid DocumentsGrid;
		private ZArchitecture.GUI.ZGroupBox CertificatesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox TypeCodeFindBox;
		private Enterprise.Customs.GUI.WordWrappingTextBox DescriptionWordWrapingTextBox;
		private Enterprise.Customs.GUI.WordWrappingTextBox ReferenceWordWrapingTextBox;
	}
}
