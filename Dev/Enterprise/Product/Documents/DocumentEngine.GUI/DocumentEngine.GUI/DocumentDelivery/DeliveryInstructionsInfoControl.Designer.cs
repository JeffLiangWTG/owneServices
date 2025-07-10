namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	partial class DeliveryInstructionsInfoControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IndividualDocPackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.recipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.documentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CoverNotePage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.includeCoverNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.coverNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainPage.SuspendLayout();
			this.IndividualDocPackGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.recipientsGrid)).BeginInit();
			this.DocumentsTabPage.SuspendLayout();
			this.DocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.documentsGrid)).BeginInit();
			this.CoverNotePage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.PrintTaskSettings);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.MainTabControl);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 409, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainPage);
			this.MainTabControl.Controls.Add(this.DocumentsTabPage);
			this.MainTabControl.Controls.Add(this.CoverNotePage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 391, true);
			this.MainTabControl.TabIndex = 2;
			// 
			// MainPage
			// 
			this.MainPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|3b7d8bb1-901a-4b4b-ad76-3f367e907a3f", "Destination");
			this.MainPage.Controls.Add(this.IndividualDocPackGroupBox);
			this.MainPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainPage.Name = "MainPage";
			this.MainPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 364, true);
			this.MainPage.TabIndex = 0;
			// 
			// IndividualDocPackGroupBox
			// 
			this.IndividualDocPackGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|50428135-c4da-40b8-a8c7-6780049f980a", "Recipients");
			this.IndividualDocPackGroupBox.Controls.Add(this.recipientsGrid);
			this.IndividualDocPackGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IndividualDocPackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IndividualDocPackGroupBox.Name = "IndividualDocPackGroupBox";
			this.IndividualDocPackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 364, true);
			this.IndividualDocPackGroupBox.TabIndex = 0;
			this.IndividualDocPackGroupBox.TabStop = false;
			// 
			// recipientsGrid
			// 
			this.recipientsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.recipientsGrid, "DocPacksDeliveryInstructions.Recipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).OrgHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).Salutation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).DeliveryMethodDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).Recipients)).SyncRoot)).DeliveryAddress)));
			this.recipientsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|99d75b98-13e7-4a41-9055-c82cea5877df", "Organization");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OrgHeaderPK";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|43e5209f-6d64-435d-a6c8-100ee6f3f3fb", "Company Name");
			zTextBoxColumnStyleInfo6.ColumnName = "CompanyName";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|413501a5-6f88-4c35-a62e-eeeb7c0b0f43", "Salutation");
			zTextBoxColumnStyleInfo7.ColumnName = "Salutation";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|dd1829bb-9d83-4fcc-8888-6e9bc409628c", "Name");
			zDropEditColumnStyleInfo4.ColumnName = "Name";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|de7ffed1-0446-4733-a8a9-ea9f5a730adb", "Delivery Method");
			zDropEditColumnStyleInfo5.ColumnName = "DeliveryMethodDescription";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|2f0edf08-7443-40fc-a6cf-d24a42268f6c", "Attachment Type");
			zDropEditColumnStyleInfo6.ColumnName = "AttachmentType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|f332f9d9-a579-48fe-91ad-1c4a9c6ff703", "E-Mail Address / Fax");
			zTextBoxColumnStyleInfo8.ColumnName = "DeliveryAddress";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			this.recipientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.recipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.recipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.recipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.recipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.recipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.recipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.recipientsGrid.GridId = "03e01a32-5ff6-4045-ab12-bcf22bf55dbf";
			this.recipientsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.recipientsGrid.LayoutKey = "zGrid1";
			this.recipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.recipientsGrid.Name = "recipientsGrid";
			this.recipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 345, true);
			this.recipientsGrid.TabIndex = 0;
			// 
			// DocumentsTabPage
			// 
			this.DocumentsTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|fe5e2c02-621d-4414-b99e-fe54936a6e83", "Documents to Send");
			this.DocumentsTabPage.Controls.Add(this.DocumentsGroupBox);
			this.DocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DocumentsTabPage.Name = "DocumentsTabPage";
			this.DocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 364, true);
			this.DocumentsTabPage.TabIndex = 1;
			// 
			// DocumentsGroupBox
			// 
			this.DocumentsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|09c0e22a-2e6d-4057-8896-cddf915ea791", "Documents");
			this.DocumentsGroupBox.Controls.Add(this.documentsGrid);
			this.DocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentsGroupBox.Name = "DocumentsGroupBox";
			this.DocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 364, true);
			this.DocumentsGroupBox.TabIndex = 2;
			this.DocumentsGroupBox.TabStop = false;
			// 
			// documentsGrid
			// 
			this.documentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.documentsGrid, "DocPacksDeliveryInstructions.DeliverablesToBePrinted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).DeliverablesToBePrinted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).DeliverablesToBePrinted)).SyncRoot)).IncludedInPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).DeliverablesToBePrinted)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).DeliverablesToBePrinted)).SyncRoot)).DeliveryMode)));
			this.documentsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|7e36b133-f5c9-4c50-86ee-ca3bc7e9cb4b", "Include");
			zCheckBoxColumnStyleInfo2.ColumnName = "IncludedInPrint";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|83533616-1461-4d75-9633-1138b9b99c8e", "Document Name");
			zTextBoxColumnStyleInfo9.ColumnName = "Name";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|eb935dda-6ff5-4987-8ca5-7d320517dc47", "Delivery Mode");
			zTextBoxColumnStyleInfo10.ColumnName = "DeliveryMode";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			this.documentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.documentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.documentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.documentsGrid.GridId = "b5635d41-57b8-4369-b777-f92bd3e1db63";
			this.documentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.documentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.documentsGrid.LayoutKey = "DocumentsGrid";
			this.documentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.documentsGrid.Name = "documentsGrid";
			this.documentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.documentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 345, true);
			this.documentsGrid.TabIndex = 1;
			// 
			// CoverNotePage
			// 
			this.CoverNotePage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|e59ecebf-8835-4339-a43b-59ef3cd21891", "Cover Note");
			this.CoverNotePage.Controls.Add(this.includeCoverNoteCheckBox);
			this.CoverNotePage.Controls.Add(this.coverNoteTextBox);
			this.CoverNotePage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CoverNotePage.Name = "CoverNotePage";
			this.CoverNotePage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 364, true);
			this.CoverNotePage.TabIndex = 2;
			// 
			// includeCoverNoteCheckBox
			// 
			this.includeCoverNoteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.includeCoverNoteCheckBox, "DocPacksDeliveryInstructions.IncludeCoverNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).IncludeCoverNote)));
			this.includeCoverNoteCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DeliveryInstructionsInfoControl|0038e8dd-5406-48ca-81e8-5fd08445457d", "Include Cover Note", "Include a Cover Note for E-Mail and Fax Recipients", "");
			this.includeCoverNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.includeCoverNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 11, true);
			this.includeCoverNoteCheckBox.Name = "includeCoverNoteCheckBox";
			this.includeCoverNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.includeCoverNoteCheckBox.TabIndex = 0;
			this.includeCoverNoteCheckBox.UseVisualStyleBackColor = false;
			// 
			// coverNoteTextBox
			// 
			this.coverNoteTextBox.AcceptsReturn = true;
			this.coverNoteTextBox.AcceptsTab = true;
			this.coverNoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.coverNoteTextBox, "DocPacksDeliveryInstructions.CoverNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).CoverNote)));
			this.coverNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.coverNoteTextBox, false);			
			this.coverNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 32, true);
			this.coverNoteTextBox.Multiline = true;
			this.coverNoteTextBox.Name = "coverNoteTextBox";
			this.coverNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 327, true);
			this.coverNoteTextBox.TabIndex = 1;
			// 
			// DeliveryInstructionsInfoControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "DeliveryInstructionsInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 409, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.MainTabControl.ResumeLayout(false);
			this.MainPage.ResumeLayout(false);
			this.IndividualDocPackGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.recipientsGrid)).EndInit();
			this.DocumentsTabPage.ResumeLayout(false);
			this.DocumentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.documentsGrid)).EndInit();
			this.CoverNotePage.ResumeLayout(false);
			this.CoverNotePage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox IndividualDocPackGroupBox;
		private Enterprise.ZArchitecture.ZGrid recipientsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTabPage DocumentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DocumentsGroupBox;
		private Enterprise.ZArchitecture.ZGrid documentsGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage CoverNotePage;
		private Enterprise.ZArchitecture.GUI.ZCheckBox includeCoverNoteCheckBox;
		private Enterprise.ZArchitecture.ZTextBox coverNoteTextBox;

	}
}
