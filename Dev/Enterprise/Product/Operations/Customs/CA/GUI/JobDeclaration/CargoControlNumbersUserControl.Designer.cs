namespace Enterprise.Customs.CA.GUI
{
	partial class CargoControlNumbersUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ReleaseStatusesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseStatusesGrid)).BeginInit();
			this.ReleaseStatusesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// ReleaseStatusesGrid
			// 
			this.ReleaseStatusesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReleaseStatusesGrid, "ReleaseStatuses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_CargoControlNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_Bill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).Bills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ProcessingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseOffice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_WarehouseCode)));
			this.ReleaseStatusesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "RL_CargoControlNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo1.BindToList = "Bills";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1a5e7d56-04f1-482b-9217-b2f6c72de7d1", "Bill");
			zGuidDropEditColumnStyleInfo1.ColumnName = "RL_Bill";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "RL_ReleaseStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "RL_ReleaseStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.ColumnName = "RL_ProcessingDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "RL_ReleaseDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RL_ReleaseOffice";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "RL_WarehouseCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ReleaseStatusesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ReleaseStatusesGrid.CopySelectedRowsAllowed = true;
			this.ReleaseStatusesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReleaseStatusesGrid.GridId = "0984351e-b7a0-4ac9-b1cf-d6da8ebbfa0e";
			this.ReleaseStatusesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseStatusesGrid.LayoutKey = "ReleaseStatusesGrid";
			this.ReleaseStatusesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReleaseStatusesGrid.Name = "ReleaseStatusesGrid";
			this.ReleaseStatusesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1121, 141, true);
			this.ReleaseStatusesGrid.TabIndex = 0;
			// 
			// CargoControlNumbersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReleaseStatusesGrid);
			this.Name = "CargoControlNumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1121, 141, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseStatusesGrid)).EndInit();
			this.ReleaseStatusesGrid.ResumeLayout(false);
			this.ReleaseStatusesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ReleaseStatusesGrid;

	}

}
