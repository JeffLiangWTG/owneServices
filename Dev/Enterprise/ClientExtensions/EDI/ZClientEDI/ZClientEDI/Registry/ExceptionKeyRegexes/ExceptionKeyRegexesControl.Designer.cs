namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ExceptionKeyRegexesControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExceptionKeyMatchingRegexGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionKeyMatchingRegexGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.ExceptionKeyRegexCollection);
			// 
			// ExceptionKeyMatchingRegexGrid
			// 
			this.ExceptionKeyMatchingRegexGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExceptionKeyMatchingRegexGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ExceptionKeyRegex)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ExceptionKeyRegex)(null)).Regex)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ExceptionKeyRegex)(null)).Description)));
			this.ExceptionKeyMatchingRegexGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Regex";
			zTextBoxColumnStyleInfo1.ColumnName = "Regex";
			zTextBoxColumnStyleInfo2.Caption = "Description";
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ExceptionKeyMatchingRegexGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExceptionKeyMatchingRegexGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ExceptionKeyMatchingRegexGrid.CopySelectedRowsAllowed = true;
			this.ExceptionKeyMatchingRegexGrid.GridId = "4B895ED7-3042-407B-A04E-456C20C7FFCD";
			this.ExceptionKeyMatchingRegexGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExceptionKeyMatchingRegexGrid.LayoutKey = "ExceptionKeyMatchingRegexGrid";
			this.ExceptionKeyMatchingRegexGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExceptionKeyMatchingRegexGrid.Name = "ExceptionKeyMatchingRegexGrid";
			this.ExceptionKeyMatchingRegexGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.ExceptionKeyMatchingRegexGrid.TabIndex = 0;
			// 
			// ExceptionKeyMatchingRegexControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExceptionKeyMatchingRegexGrid);
			this.Name = "ExceptionKeyMatchingRegexControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionKeyMatchingRegexGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid ExceptionKeyMatchingRegexGrid;
	}
}
