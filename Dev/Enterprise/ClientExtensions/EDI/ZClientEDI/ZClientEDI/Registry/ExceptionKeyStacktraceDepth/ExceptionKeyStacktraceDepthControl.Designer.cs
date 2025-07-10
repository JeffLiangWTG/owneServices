namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ExceptionKeyStacktraceDepthControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ExceptionKeyStacktraceDepthGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionKeyStacktraceDepthGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.ExceptionKeyStacktraceDepth);
			// 
			// ExceptionKeyStacktraceDepthGrid
			// 
			this.ExceptionKeyStacktraceDepthGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExceptionKeyStacktraceDepthGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ExceptionKeyStacktraceDepth)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ExceptionKeyStacktraceDepth)(null)).ExceptionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.ExceptionKeyStacktraceDepth)(null)).StackDepth)));
			this.ExceptionKeyStacktraceDepthGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ExceptionType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "StackDepth";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.ExceptionKeyStacktraceDepthGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExceptionKeyStacktraceDepthGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ExceptionKeyStacktraceDepthGrid.CopySelectedRowsAllowed = true;
			this.ExceptionKeyStacktraceDepthGrid.GridId = "4B895ED7-3042-407B-A04E-456C20C7FFCD";
			this.ExceptionKeyStacktraceDepthGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExceptionKeyStacktraceDepthGrid.LayoutKey = "ExceptionKeyStacktraceDepthGrid";
			this.ExceptionKeyStacktraceDepthGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExceptionKeyStacktraceDepthGrid.Name = "ExceptionKeyStacktraceDepthGrid";
			this.ExceptionKeyStacktraceDepthGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.ExceptionKeyStacktraceDepthGrid.TabIndex = 0;
			// 
			// ExceptionKeyStacktraceDepthControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExceptionKeyStacktraceDepthGrid);
			this.Name = "ExceptionKeyStacktraceDepthControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionKeyStacktraceDepthGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid ExceptionKeyStacktraceDepthGrid;
	}
}
