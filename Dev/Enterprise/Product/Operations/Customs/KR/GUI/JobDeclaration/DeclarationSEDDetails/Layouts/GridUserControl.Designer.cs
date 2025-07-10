namespace Enterprise.Customs.KR.GUI
{
	partial class GridUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StevedoresGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StevedoresGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OtherTransportMeansGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OtherTransportMeansGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StevedoresGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StevedoresGrid)).BeginInit();
			this.StevedoresGrid.SuspendLayout();
			this.OtherTransportMeansGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherTransportMeansGrid)).BeginInit();
			this.OtherTransportMeansGrid.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// StevedoresGroupBox
			// 
			this.StevedoresGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("11466485-e06a-4005-ab3f-33c81664d221", "Stevedores");
			this.StevedoresGroupBox.Controls.Add(this.StevedoresGrid);
			this.StevedoresGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StevedoresGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StevedoresGroupBox.Name = "StevedoresGroupBox";
			this.StevedoresGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 135, true);
			this.StevedoresGroupBox.TabIndex = 0;
			this.StevedoresGroupBox.TabStop = false;
			// 
			// StevedoresGrid
			// 
			this.StevedoresGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StevedoresGrid, "Persons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Persons)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Persons)).SyncRoot)).CPN_PER_Person)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Persons)).SyncRoot)).PersonBirthDate)));
			this.StevedoresGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CPN_PER_Person";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "PersonBirthDate";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.StevedoresGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.StevedoresGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StevedoresGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StevedoresGrid.GridId = "7a01fbce-ffdd-477d-96d8-c9ea78fbef4b";
			this.StevedoresGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StevedoresGrid.LayoutKey = "StevedoresGrid";
			this.StevedoresGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.StevedoresGrid.Name = "StevedoresGrid";
			this.StevedoresGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 116, true);
			this.StevedoresGrid.TabIndex = 0;
			// 
			// OtherTransportMeansGroupBox
			// 
			this.OtherTransportMeansGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("62e4fce5-23d3-4dd3-a1c2-25980bc11059", "Other Transport Means");
			this.OtherTransportMeansGroupBox.Controls.Add(this.OtherTransportMeansGrid);
			this.OtherTransportMeansGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherTransportMeansGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherTransportMeansGroupBox.Name = "OtherTransportMeansGroupBox";
			this.OtherTransportMeansGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 135, true);
			this.OtherTransportMeansGroupBox.TabIndex = 0;
			this.OtherTransportMeansGroupBox.TabStop = false;
			// 
			// OtherTransportMeansGrid
			// 
			this.OtherTransportMeansGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherTransportMeansGrid, "TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).TransportMeans)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.TransportMeans)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).TransportMeans)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.TransportMeans)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).TransportMeans)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.TransportMeans)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).TransportMeans)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.TransportMeans)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).TransportMeans)).SyncRoot)).CY_Data)));
			this.OtherTransportMeansGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			this.OtherTransportMeansGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OtherTransportMeansGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OtherTransportMeansGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OtherTransportMeansGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OtherTransportMeansGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherTransportMeansGrid.GridId = "2237b19f-cbe6-4ca2-b52c-68fa41535bc6";
			this.OtherTransportMeansGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherTransportMeansGrid.LayoutKey = "OtherTransportMeansGrid";
			this.OtherTransportMeansGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OtherTransportMeansGrid.Name = "OtherTransportMeansGrid";
			this.OtherTransportMeansGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 116, true);
			this.OtherTransportMeansGrid.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.StevedoresGroupBox);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 135, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 135, true);
			this.LeftPanel.TabIndex = 1;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.OtherTransportMeansGroupBox);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 0, true);
			this.RightPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 135, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 135, true);
			this.RightPanel.TabIndex = 3;
			// 
			// Panel
			// 
			this.Panel.Controls.Add(this.LeftPanel);
			this.Panel.Controls.Add(this.RightPanel);
			this.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Panel.Name = "Panel";
			this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 135, true);
			this.Panel.TabIndex = 0;
			// 
			// GridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Panel);
			this.Name = "GridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StevedoresGroupBox.ResumeLayout(false);
			this.StevedoresGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StevedoresGrid)).EndInit();
			this.StevedoresGrid.ResumeLayout(false);
			this.StevedoresGrid.PerformLayout();
			this.OtherTransportMeansGroupBox.ResumeLayout(false);
			this.OtherTransportMeansGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherTransportMeansGrid)).EndInit();
			this.OtherTransportMeansGrid.ResumeLayout(false);
			this.OtherTransportMeansGrid.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.Panel.ResumeLayout(false);
			this.Panel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox OtherTransportMeansGroupBox;
		internal ZArchitecture.GUI.ZGroupBox StevedoresGroupBox;
		private ZArchitecture.ZGrid StevedoresGrid;
		private ZArchitecture.ZGrid OtherTransportMeansGrid;
		private ZArchitecture.GUI.ZPanel LeftPanel;
		private ZArchitecture.GUI.ZPanel RightPanel;
		private ZArchitecture.GUI.ZPanel Panel;
	}
}
