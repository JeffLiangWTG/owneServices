namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SingleAccountingPeriodUserControl
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
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FieldPeriodEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FieldPeriodEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.SingleAccountingPeriodField);
			// 
			// FieldLabel
			// 
			this.FieldLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SingleAccountingPeriodUserControl|1c658855-aee5-4422-a488-288a38a431c2", "Field");
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.FieldLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.FieldLabel.TabIndex = 0;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// FieldPeriodEdit
			// 
			this.FieldPeriodEdit.AllowDrop = true;
			this.FieldPeriodEdit.BindTo = "SinglePeriod";
			this.FieldPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 0, true);
			this.FieldPeriodEdit.Name = "FieldPeriodEdit";
			this.FieldPeriodEdit.ReadOnly = false;
			this.FieldPeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.FieldPeriodEdit.TabIndex = 1;
			// 
			// SingleAccountingPeriodUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldPeriodEdit);
			this.Controls.Add(this.FieldLabel);
			this.Name = "SingleAccountingPeriodUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FieldPeriodEdit.ResumeLayout(true);
			this.FieldPeriodEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZLabel FieldLabel;
		internal SchedulablePeriodEdit FieldPeriodEdit;
	}
}
