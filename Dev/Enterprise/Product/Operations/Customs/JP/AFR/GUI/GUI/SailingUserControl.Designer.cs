namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class SailingUserControl
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
			this.SailingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SailingStatisticsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AFRBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
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
			this.RefreshStatisticsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingGroupBox.SuspendLayout();
			this.SailingStatisticsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			// 
			// SailingGroupBox
			// 
			this.SailingGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("813DB091-907D-4CFC-B787-654F44B58BEF", "Sailing");
			this.SailingGroupBox.Controls.Add(this.RefreshStatisticsButton);
			this.SailingGroupBox.Controls.Add(this.SailingStatisticsPanel);
			this.SailingGroupBox.Controls.Add(this.EditSailingButton);
			this.SailingGroupBox.Controls.Add(this.ClearSailingButton);
			this.SailingGroupBox.Controls.Add(this.SelectSailingButton);
			this.SailingGroupBox.Controls.Add(this.SailingETADateEdit);
			this.SailingGroupBox.Controls.Add(this.SailingDischargeTextBox);
			this.SailingGroupBox.Controls.Add(this.SailingETDDateEdit);
			this.SailingGroupBox.Controls.Add(this.SailingPortOfLadingTextBox);
			this.SailingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SailingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SailingGroupBox.Name = "SailingGroupBox";
			this.SailingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 76, true);
			this.SailingGroupBox.TabIndex = 0;
			this.SailingGroupBox.TabStop = false;
			// 
			// SailingStatisticsPanel
			// 
			this.SailingStatisticsPanel.Controls.Add(this.AFRBillsCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.SailingContainersCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.SailingBillsCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.ContainersOnHeaderCalcEdit);
			this.SailingStatisticsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 14, true);
			this.SailingStatisticsPanel.Name = "SailingStatisticsPanel";
			this.SailingStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 56, true);
			this.SailingStatisticsPanel.TabIndex = 23;
			// 
			// AFRBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AFRBillsCalcEdit, "JPH_NoOfAFRBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_NoOfAFRBills)));
			this.AFRBillsCalcEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("9EABD5E0-CDC8-49D7-B14D-004C81B36C0A", "Bills on this AFR");
			this.AFRBillsCalcEdit.DecimalPlaces = 2;
			this.AFRBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 5, true);
			this.AFRBillsCalcEdit.Name = "AFRBillsCalcEdit";
			this.AFRBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.AFRBillsCalcEdit.TabIndex = 18;
			this.AFRBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SailingContainersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingContainersCalcEdit, "JPH_NoOfSailingContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_NoOfSailingContainers)));
			this.SailingContainersCalcEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("B4E7BC0E-471E-459E-8D6F-2B908F520826", "Containers related to Sailing");
			this.SailingContainersCalcEdit.DecimalPlaces = 2;
			this.SailingContainersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 31, true);
			this.SailingContainersCalcEdit.Name = "SailingContainersCalcEdit";
			this.SailingContainersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.SailingContainersCalcEdit.TabIndex = 21;
			this.SailingContainersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SailingBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingBillsCalcEdit, "JPH_NoOfSailingBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_NoOfSailingBills)));
			this.SailingBillsCalcEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("E53F7A4B-A356-4872-A92C-BAC0AE290D8C", "Containerized Bills related to Sailing");
			this.SailingBillsCalcEdit.DecimalPlaces = 2;
			this.SailingBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 5, true);
			this.SailingBillsCalcEdit.Name = "SailingBillsCalcEdit";
			this.SailingBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.SailingBillsCalcEdit.TabIndex = 19;
			this.SailingBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContainersOnHeaderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainersOnHeaderCalcEdit, "JPH_NoOfAFRContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).JPH_NoOfAFRContainers)));
			this.ContainersOnHeaderCalcEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("27CBF055-3726-4D46-ADF1-24F44B5B0401", "Containers on this AFR");
			this.ContainersOnHeaderCalcEdit.DecimalPlaces = 2;
			this.ContainersOnHeaderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 31, true);
			this.ContainersOnHeaderCalcEdit.Name = "ContainersOnHeaderCalcEdit";
			this.ContainersOnHeaderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ContainersOnHeaderCalcEdit.TabIndex = 20;
			this.ContainersOnHeaderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EditSailingButton
			// 
			this.EditSailingButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("FF88503B-A806-42AE-94FC-192F67BA015D", "Edit Sailing");
			this.EditSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 43, true);
			this.EditSailingButton.Name = "EditSailingButton";
			this.EditSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.EditSailingButton.TabIndex = 6;
			this.EditSailingButton.UseVisualStyleBackColor = true;
			this.EditSailingButton.Click += new System.EventHandler(this.EditSailingButton_Click);
			// 
			// ClearSailingButton
			// 
			this.ClearSailingButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4396AA00-E6A0-442A-9D3A-9D019799DEE4", "Clear Sailing");
			this.ClearSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 17, true);
			this.ClearSailingButton.Name = "ClearSailingButton";
			this.ClearSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.ClearSailingButton.TabIndex = 5;
			this.ClearSailingButton.UseVisualStyleBackColor = true;
			this.ClearSailingButton.Click += new System.EventHandler(this.ClearSailingButton_Click);
			// 
			// SelectSailingButton
			// 
			this.SelectSailingButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("2251E961-21C7-4E59-8351-25BB098EAFD5", "Select Sailing");
			this.SelectSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 17, true);
			this.SelectSailingButton.Name = "SelectSailingButton";
			this.SelectSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.SelectSailingButton.TabIndex = 4;
			this.SelectSailingButton.UseVisualStyleBackColor = true;
			this.SelectSailingButton.Click += new System.EventHandler(this.SelectSailingButton_Click);
			// 
			// SailingETADateEdit
			// 
			this.SailingETADateEdit.AllowDrop = true;
			this.SailingETADateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingETADateEdit, "Sailings.JX_JB_E_ARV");
			this.SailingETADateEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("DCD8A25D-7D09-4471-BC3B-49041A0A06FB", "ETA");
			this.SailingETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 45, true);
			this.SailingETADateEdit.Name = "SailingETADateEdit";
			this.SailingETADateEdit.TabIndex = 3;
			// 
			// SailingDischargeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SailingDischargeTextBox, "Sailings.JX_JB_RL_NKPortOfDischarge");
			this.SailingDischargeTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("33F4BB88-427C-4715-AC78-BF2EE63E13C1", "Discharge");
			this.SailingDischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 19, true);
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
			this.SailingETDDateEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("C397B37E-1C2F-4872-A1A9-C920FD42E8FB", "ETD");
			this.SailingETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 45, true);
			this.SailingETDDateEdit.Name = "SailingETDDateEdit";
			this.SailingETDDateEdit.TabIndex = 1;
			// 
			// SailingPortOfLadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.SailingPortOfLadingTextBox, "Sailings.JX_JA_RL_NKPortOfLoading");
			this.SailingPortOfLadingTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("11998265-3DE2-4722-B07D-421FA5C18EF9", "Loading");
			this.SailingPortOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 19, true);
			this.SailingPortOfLadingTextBox.Name = "SailingPortOfLadingTextBox";
			this.SailingPortOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SailingPortOfLadingTextBox.TabIndex = 0;
			// 
			// RefreshStatisticsButton
			// 
			this.RefreshStatisticsButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("9E2E3C95-6274-4470-885D-2AE5E086F8EA", "Refresh Statistics");
			this.RefreshStatisticsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 43, true);
			this.RefreshStatisticsButton.Name = "RefreshStatisticsButton";
			this.RefreshStatisticsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.RefreshStatisticsButton.TabIndex = 24;
			this.RefreshStatisticsButton.UseVisualStyleBackColor = true;
			this.RefreshStatisticsButton.Click += new System.EventHandler(this.RefreshStatisticsButton_Click);
			// 
			// SailingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingGroupBox);
			this.Name = "SailingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingGroupBox.ResumeLayout(false);
			this.SailingGroupBox.PerformLayout();
			this.SailingStatisticsPanel.ResumeLayout(false);
			this.SailingStatisticsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SailingGroupBox;
		private ZArchitecture.ZTextBox SailingPortOfLadingTextBox;
		private ZArchitecture.GUI.ZDateEdit SailingETDDateEdit;
		private ZArchitecture.ZTextBox SailingDischargeTextBox;
		private ZArchitecture.GUI.ZDateEdit SailingETADateEdit;
		private ZArchitecture.GUI.ZButton EditSailingButton;
		private ZArchitecture.GUI.ZButton ClearSailingButton;
		private ZArchitecture.GUI.ZButton SelectSailingButton;
		private ZArchitecture.ZCalcEdit AFRBillsCalcEdit;
		private ZArchitecture.ZCalcEdit SailingContainersCalcEdit;
		private ZArchitecture.ZCalcEdit SailingBillsCalcEdit;
		private ZArchitecture.ZCalcEdit ContainersOnHeaderCalcEdit;
		internal ZArchitecture.GUI.ZPanel SailingStatisticsPanel;
		internal ZArchitecture.GUI.ZButton RefreshStatisticsButton;
	}
}
