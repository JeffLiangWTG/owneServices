namespace Enterprise.Customs.GB.GUI
{
	partial class CredentialsDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zIssueDateColumn = new ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zExpiryDateColumn = new ZArchitecture.ZDateEditColumnStyleInfo();
			this.AccUserGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AccUserGrid)).BeginInit();
			this.AccUserGrid.SuspendLayout();
			this.CredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.GBGlbCompanyWrapper);
			// 
			// AccUserGrid
			// 
			this.AccUserGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AccUserGrid, "GBBPasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).Badge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).EORI)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).IsTokenForAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).IsTokenForCDS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).IsTokenForEMCS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).IsTokenForGVMS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).IsTokenForNCTS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).IsTokenForSnSGB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).StatusMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).GP_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.GB.Business.GlbExternalPassword_GB)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.GBGlbCompanyWrapper)(null)).GBBPasswordCollection)).SyncRoot)).GP_ExpiryDate)));
			this.AccUserGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a46ef58a-9bd1-4dc7-8659-c2c11d6885ef", "Profile");
			zDropEditColumnStyleInfo1.ColumnName = "Badge";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c013fcc5-c0a0-4354-a87c-022e3557e1dd", "EORI");
			zDropEditColumnStyleInfo2.ColumnName = "EORI";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8008c9f6-5925-4949-8ca3-653ed74ae02a", "Status");
			zTextBoxColumnStyleInfo1.ColumnName = "Status";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f8b9f4c0-03e6-40f4-91a7-f5a3aefc8120", "All");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsTokenForAll";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7de7a391-6c97-4e38-812a-9f4de1629ae5", "CDS");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsTokenForCDS";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3df3fc37-7dfc-4a45-b58f-7ac3e9d4d3ee", "EMCS");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsTokenForEMCS";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("927b09fc-4292-4078-8e8a-1c0dbed89ab6", "GVMS");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsTokenForGVMS";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c7f3cfc3-1540-4464-88ce-746e8e7e6a3c", "NCTS");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsTokenForNCTS";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("e59f2d85-b0f6-4fe1-83ee-58a0d7e933f4", "S&S GB");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsTokenForSnSGB";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0d333bc2-4ce5-4aec-acda-e43412e1bed5", "Status Message");
			zTextBoxColumnStyleInfo2.ColumnName = "StatusMessage";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zIssueDateColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("69e6a5b2-f31f-4e75-b64b-76f16d15ac2d", "Issue Date");
			zIssueDateColumn.ColumnName = "GP_IssueDate";
			zIssueDateColumn.IsReadOnly = true;
			zIssueDateColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zExpiryDateColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("343640d5-c024-47d6-be86-36ad8615774a", "Expiry Date");
			zExpiryDateColumn.ColumnName = "GP_ExpiryDate";
			zExpiryDateColumn.IsReadOnly = true;
			zExpiryDateColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AccUserGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AccUserGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AccUserGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AccUserGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AccUserGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.AccUserGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.AccUserGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.AccUserGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.AccUserGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.AccUserGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AccUserGrid.ColumnStyles.Add(zIssueDateColumn);
			this.AccUserGrid.ColumnStyles.Add(zExpiryDateColumn);
			this.AccUserGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccUserGrid.GridId = "d35b96df-79f1-483f-8770-3aa78d5597b1";
			this.AccUserGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccUserGrid.LayoutKey = "GBAccUserGrid";
			this.AccUserGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccUserGrid.Name = "GBAccUserGrid";
			this.AccUserGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.AccUserGrid.TabIndex = 1;
			//this.GBAccUserGrid.IsWholeRowSelectedOnClick = true;
			// 
			// CredentialsPanel
			// 
			this.CredentialsPanel.Controls.Add(this.AccUserGrid);
			this.CredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialsPanel.Name = "CredentialsPanel";
			this.CredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.CredentialsPanel.TabIndex = 5;
			// 
			// CredentialsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialsPanel);
			this.Name = "CredentialsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AccUserGrid)).EndInit();
			this.AccUserGrid.ResumeLayout(false);
			this.AccUserGrid.PerformLayout();
			this.CredentialsPanel.ResumeLayout(false);
			this.CredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel CredentialsPanel;
		internal Enterprise.ZArchitecture.ZGrid AccUserGrid;
	}
}
