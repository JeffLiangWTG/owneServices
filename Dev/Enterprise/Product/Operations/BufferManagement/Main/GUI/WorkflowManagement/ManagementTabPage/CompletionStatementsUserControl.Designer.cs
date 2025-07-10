namespace Enterprise.BufferManagement.GUI
{
	partial class CompletionStatementsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo zDynamicMultilineTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CompletionStatementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CompletionStatementsGrid)).BeginInit();
			this.CompletionStatementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTaskCollectionView);
			// 
			// CompletionStatementsGrid
			// 
			this.CompletionStatementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CompletionStatementsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).DisplayOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_NotesAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GS_NKAssignedStaffMember)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).IsEffectAchieved)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_FH_ProcessHeader)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_G4_RequiredCapability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstimateVariationFactor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).StandardEstimateHours)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).HighEstimatedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_CardNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_SE_NKTaskCompletionEvent)));
			this.CompletionStatementsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DisplayOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDynamicMultilineTextBoxColumnStyleInfo1.ColumnName = "P9_NotesAsString";
			zDynamicMultilineTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "P9_GS_NKAssignedStaffMember";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsEffectAchieved";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidDropEditColumnStyleInfo1.ColumnName = "P9_FH_ProcessHeader";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "P9_G4_RequiredCapability";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "P9_Status";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "P9_EstDuration";
			zTimeEditExColumnStyleInfo1.IsVisible = false;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "P9_EstimateVariationFactor";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "StandardEstimateHours";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo2.AllowNegative = false;
			zTimeEditExColumnStyleInfo2.ColumnName = "HighEstimatedDuration";
			zTimeEditExColumnStyleInfo2.IsVisible = false;
			zTimeEditExColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "P9_CardNote";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "P9_SE_NKTaskCompletionEvent";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CompletionStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CompletionStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CompletionStatementsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo2);
			this.CompletionStatementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CompletionStatementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CompletionStatementsGrid.CopySelectedRowsAllowed = true;
			this.CompletionStatementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompletionStatementsGrid.GridId = "31a9fabb-de70-421d-993c-eab47434a5af";
			this.CompletionStatementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CompletionStatementsGrid.LayoutKey = "CompletionStatementsGrid";
			this.CompletionStatementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompletionStatementsGrid.Name = "CompletionStatementsGrid";
			this.CompletionStatementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 244, true);
			this.CompletionStatementsGrid.TabIndex = 0;
			// 
			// CompletionStatementsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CompletionStatementsGrid);
			this.Name = "CompletionStatementsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 244, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CompletionStatementsGrid)).EndInit();
			this.CompletionStatementsGrid.ResumeLayout(false);
			this.CompletionStatementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid CompletionStatementsGrid;
	}
}
