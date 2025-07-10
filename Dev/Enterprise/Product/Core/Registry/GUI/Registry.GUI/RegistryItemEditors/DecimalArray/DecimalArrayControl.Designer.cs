namespace Enterprise.Registry.GUI
{
	partial class DecimalArrayControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DecimalGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DecimalGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Internal.DecimalLine);
			// 
			// DecimalGrid
			// 
			this.DecimalGrid.AllowNavigation = false;
			this.DecimalGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.DecimalGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Internal.DecimalLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Internal.DecimalLine)(null)).DecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Internal.DecimalLine)(null)).Number)));
			this.DecimalGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "DecimalPlaces";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DecimalArrayControl|865886c8-6509-41ca-8b13-40fdd12f1696", "Number");
			zCalcEditColumnStyleInfo1.ColumnName = "Number";
			this.DecimalGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DecimalGrid.GridId = "fb25eadd-6fb1-4c5e-8c14-0096a7d618aa";
			this.DecimalGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DecimalGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DecimalGrid.LayoutKey = "NumericGrid";
			this.DecimalGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DecimalGrid.Name = "DecimalGrid";
			this.DecimalGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 195, true);
			this.DecimalGrid.TabIndex = 0;
			// 
			// DecimalArrayControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DecimalGrid);
			this.Name = "DecimalArrayControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 195, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DecimalGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid DecimalGrid;
	}
}
