namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class SailingUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.SailingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshStatisticsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingStatisticsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SailingContainersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SailingBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainersOnHeaderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EditSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SailingDischargeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SailingETDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SailingPortOfLadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SailingATDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingGroupBox.SuspendLayout();
			this.SailingStatisticsPanel.SuspendLayout();
			this.SailingETADateEdit.SuspendLayout();
			this.SailingETDDateEdit.SuspendLayout();
			this.SailingATDDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// SailingGroupBox
			// 
			this.SailingGroupBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("7CC4DDBF-1E58-4C23-B02D-F2B988452E8B", "Sailing");
			this.SailingGroupBox.Controls.Add(this.RefreshStatisticsButton);
			this.SailingGroupBox.Controls.Add(this.SailingStatisticsPanel);
			this.SailingGroupBox.Controls.Add(this.EditSailingButton);
			this.SailingGroupBox.Controls.Add(this.ClearSailingButton);
			this.SailingGroupBox.Controls.Add(this.SelectSailingButton);
			this.SailingGroupBox.Controls.Add(this.SailingETADateEdit);
			this.SailingGroupBox.Controls.Add(this.SailingDischargeTextBox);
			this.SailingGroupBox.Controls.Add(this.SailingETDDateEdit);
			this.SailingGroupBox.Controls.Add(this.SailingPortOfLadingTextBox);
			this.SailingGroupBox.Controls.Add(this.SailingATDDateEdit);
			this.SailingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SailingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SailingGroupBox.Name = "SailingGroupBox";
			this.SailingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 76, true);
			this.SailingGroupBox.TabIndex = 0;
			this.SailingGroupBox.TabStop = false;
			// 
			// RefreshStatisticsButton
			// 
			this.RefreshStatisticsButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("5AA54D61-4C7A-47FF-A4A2-CE66F244FE22", "Refresh Statistics");
			this.RefreshStatisticsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 43, true);
			this.RefreshStatisticsButton.Name = "RefreshStatisticsButton";
			this.RefreshStatisticsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshStatisticsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.RefreshStatisticsButton.TabIndex = 24;
			this.RefreshStatisticsButton.ToolTipCaption = null;
			this.RefreshStatisticsButton.UseVisualStyleBackColor = true;
			this.RefreshStatisticsButton.Click += new System.EventHandler(this.RefreshStatisticsButton_Click);
			// 
			// SailingStatisticsPanel
			// 
			this.SailingStatisticsPanel.Controls.Add(this.BillsCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.SailingContainersCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.SailingBillsCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.ContainersOnHeaderCalcEdit);
			this.SailingStatisticsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 14, true);
			this.SailingStatisticsPanel.Name = "SailingStatisticsPanel";
			this.SailingStatisticsPanel.Visible = false;
			this.SailingStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 56, true);
			this.SailingStatisticsPanel.TabIndex = 23;
			// 
			// BillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BillsCalcEdit, "AMA_NoOfBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_NoOfBills)));
			this.BillsCalcEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("439A9A2E-5614-4E5C-824F-94524A74C6BF", "Bills on this Manifest");
			this.BillsCalcEdit.DecimalPlaces = 2;
			this.BillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 5, true);
			this.BillsCalcEdit.Name = "BillsCalcEdit";
			this.BillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.BillsCalcEdit.TabIndex = 18;
			this.BillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SailingContainersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingContainersCalcEdit, "AMA_NoOfSailingContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_NoOfSailingContainers)));
			this.SailingContainersCalcEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("9089AFE4-8A5D-4F20-92A3-F6BDAB21AC85", "Containers related to Sailing");
			this.SailingContainersCalcEdit.DecimalPlaces = 2;
			this.SailingContainersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 31, true);
			this.SailingContainersCalcEdit.Name = "SailingContainersCalcEdit";
			this.SailingContainersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.SailingContainersCalcEdit.TabIndex = 21;
			this.SailingContainersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SailingBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingBillsCalcEdit, "AMA_NoOfSailingBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_NoOfSailingBills)));
			this.SailingBillsCalcEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("3B8EF236-607A-4F0D-AAD8-C240FE176C71", "Bills related to Sailing");
			this.SailingBillsCalcEdit.DecimalPlaces = 2;
			this.SailingBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 5, true);
			this.SailingBillsCalcEdit.Name = "SailingBillsCalcEdit";
			this.SailingBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.SailingBillsCalcEdit.TabIndex = 19;
			this.SailingBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContainersOnHeaderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainersOnHeaderCalcEdit, "AMA_NoOfContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_NoOfContainers)));
			this.ContainersOnHeaderCalcEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("A852808F-021D-4BF1-8B66-944DD3A4D7DB", "Containers on this Manifest");
			this.ContainersOnHeaderCalcEdit.DecimalPlaces = 2;
			this.ContainersOnHeaderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 31, true);
			this.ContainersOnHeaderCalcEdit.Name = "ContainersOnHeaderCalcEdit";
			this.ContainersOnHeaderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ContainersOnHeaderCalcEdit.TabIndex = 20;
			this.ContainersOnHeaderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EditSailingButton
			// 
			this.EditSailingButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("A69FC4A7-2FBC-4582-B664-59F754F161DB", "Edit Sailing");
			this.EditSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 43, true);
			this.EditSailingButton.Name = "EditSailingButton";
			this.EditSailingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.EditSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.EditSailingButton.TabIndex = 7;
			this.EditSailingButton.ToolTipCaption = null;
			this.EditSailingButton.UseVisualStyleBackColor = true;
			this.EditSailingButton.Click += new System.EventHandler(this.EditSailingButton_Click);
			// 
			// ClearSailingButton
			// 
			this.ClearSailingButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("B75E52DC-DC29-402C-8FCB-C85D93585F06", "Clear Sailing");
			this.ClearSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 17, true);
			this.ClearSailingButton.Name = "ClearSailingButton";
			this.ClearSailingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.ClearSailingButton.TabIndex = 6;
			this.ClearSailingButton.ToolTipCaption = null;
			this.ClearSailingButton.UseVisualStyleBackColor = true;
			this.ClearSailingButton.Click += new System.EventHandler(this.ClearSailingButton_Click);
			// 
			// SelectSailingButton
			// 
			this.SelectSailingButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("4EB9126E-5A93-458A-AE27-1A044E6FCF5F", "Select Sailing");
			this.SelectSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 17, true);
			this.SelectSailingButton.Name = "SelectSailingButton";
			this.SelectSailingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.SelectSailingButton.TabIndex = 5;
			this.SelectSailingButton.ToolTipCaption = null;
			this.SelectSailingButton.UseVisualStyleBackColor = true;
			this.SelectSailingButton.Click += new System.EventHandler(this.SelectSailingButton_Click);
			// 
			// SailingETADateEdit
			// 
			this.SailingETADateEdit.AllowDrop = true;
			this.SailingETADateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingETADateEdit, "Sailings.JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Sailings)).SyncRoot)).JX_JB_E_ARV)));
			this.SailingETADateEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("90646AAC-005B-477A-AF87-C219567BB92A", "ETA");
			this.SailingETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 45, true);
			this.SailingETADateEdit.Name = "SailingETADateEdit";
			this.SailingETADateEdit.TabIndex = 4;
			// 
			// SailingDischargeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SailingDischargeTextBox, "Sailings.JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Sailings)).SyncRoot)).JX_JB_RL_NKPortOfDischarge)));
			this.SailingDischargeTextBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("FB5306CA-6BF1-41FA-8FB2-E06112E45E5A", "Discharge");
			this.SailingDischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 19, true);
			this.SailingDischargeTextBox.Name = "SailingDischargeTextBox";
			this.SailingDischargeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SailingDischargeTextBox.TabIndex = 2;
			// 
			// SailingETDDateEdit
			// 
			this.SailingETDDateEdit.AllowDrop = true;
			this.SailingETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingETDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingETDDateEdit, "Sailings.JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Sailings)).SyncRoot)).JX_JA_E_DEP)));
			this.SailingETDDateEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("EECA35AC-DAE1-4EB5-8A40-F96ADF48B045", "ETD");
			this.SailingETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 45, true);
			this.SailingETDDateEdit.Name = "SailingETDDateEdit";
			this.SailingETDDateEdit.TabIndex = 1;
			// 
			// SailingPortOfLadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.SailingPortOfLadingTextBox, "Sailings.JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Sailings)).SyncRoot)).JX_JA_RL_NKPortOfLoading)));
			this.SailingPortOfLadingTextBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("1B95D270-F848-4B87-BB3D-A6A564A2FCED", "Loading");
			this.SailingPortOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 19, true);
			this.SailingPortOfLadingTextBox.Name = "SailingPortOfLadingTextBox";
			this.SailingPortOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SailingPortOfLadingTextBox.TabIndex = 0;
			// 
			// SailingATDDateEdit
			// 
			this.SailingATDDateEdit.AllowDrop = true;
			this.SailingATDDateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingATDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingATDDateEdit, "Sailings.JX_JA_A_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Sailings)).SyncRoot)).JX_JA_A_DEP)));
			this.SailingATDDateEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("E5BCD2FF-026A-43AD-B60D-7B00717B5CFF", "ATD");
			this.SailingATDDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SailingATDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 45, true);
			this.SailingATDDateEdit.Name = "SailingATDDateEdit";
			this.SailingATDDateEdit.TabIndex = 3;
			// 
			// SailingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingGroupBox);
			this.Name = "SailingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingGroupBox.ResumeLayout(false);
			this.SailingGroupBox.PerformLayout();
			this.SailingStatisticsPanel.ResumeLayout(false);
			this.SailingStatisticsPanel.PerformLayout();
			this.SailingETADateEdit.ResumeLayout(true);
			this.SailingETADateEdit.PerformLayout();
			this.SailingETDDateEdit.ResumeLayout(true);
			this.SailingETDDateEdit.PerformLayout();
			this.SailingATDDateEdit.ResumeLayout(true);
			this.SailingATDDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox SailingGroupBox;
		ZArchitecture.ZTextBox SailingPortOfLadingTextBox;
		ZArchitecture.GUI.ZDateEdit SailingETDDateEdit;
		ZArchitecture.ZTextBox SailingDischargeTextBox;
		ZArchitecture.GUI.ZDateEdit SailingETADateEdit;
		ZArchitecture.GUI.ZButton EditSailingButton;
		ZArchitecture.GUI.ZButton ClearSailingButton;
		ZArchitecture.GUI.ZButton SelectSailingButton;
		ZArchitecture.ZCalcEdit BillsCalcEdit;
		ZArchitecture.ZCalcEdit SailingContainersCalcEdit;
		ZArchitecture.ZCalcEdit SailingBillsCalcEdit;
		ZArchitecture.ZCalcEdit ContainersOnHeaderCalcEdit;
		ZArchitecture.GUI.ZPanel SailingStatisticsPanel;
		ZArchitecture.GUI.ZButton RefreshStatisticsButton;
		ZArchitecture.GUI.ZDateEdit SailingATDDateEdit;
	}
}
