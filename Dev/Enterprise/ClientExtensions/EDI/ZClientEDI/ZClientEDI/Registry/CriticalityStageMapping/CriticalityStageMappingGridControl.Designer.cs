namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class CriticalityStageMappingGridControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.CriticalityStageMappingCollection);
			// 
			// CodeDescriptionBoolGrid
			// 
			zCheckBoxColumnStyleInfo2.Caption = "Is Default";
			zCheckBoxColumnStyleInfo2.ColumnName = "IsDefault";
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			// 
			// CriticalityStageMappingGridControl
			// 
			this.Name = "CriticalityStageMappingGridControl";
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
