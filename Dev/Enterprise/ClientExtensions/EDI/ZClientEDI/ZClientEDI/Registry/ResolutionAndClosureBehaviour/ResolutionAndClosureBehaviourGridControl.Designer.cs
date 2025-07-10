namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ResolutionAndClosureBehaviourGridControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.ResolutionAndClosureBehaviourCollection);
			// 
			// CodeDescriptionBoolGrid
			// 
			zCheckBoxColumnStyleInfo1.Caption = "Allow Self Resolve";
			zCheckBoxColumnStyleInfo1.ColumnName = "AllowSelfResolve";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.Caption = "Resolved to Closed Days";
			zCalcEditColumnStyleInfo1.ColumnName = "DaysResolvedToClosed";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.Caption = "Pending to Closed Days";
			zCalcEditColumnStyleInfo2.ColumnName = "DaysPendingCustomerToClosed";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.Caption = "Closed Reopen Rule";
			zDropEditColumnStyleInfo1.ColumnName = "ClosedReopenRule";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			// 
			// ResolutionAndClosureBehaviourGridControl
			// 
			this.Name = "ResolutionAndClosureBehaviourGridControl";
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
