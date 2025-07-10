namespace Enterprise.Registry.GUI
{
	partial class UOMPackTypeControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackTypeGrid)).BeginInit();
			this.PackTypeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.UOMPackTypeCollection);
			// 
			// PackTypeGrid
			// 
			this.PackTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackTypeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.UOMPackType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.UOMPackType)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.UOMPackType)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.UOMPackType)(null)).NumberOfLabels)));
			this.PackTypeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "NumberOfLabels";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PackTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackTypeGrid.CopySelectedRowsAllowed = true;
			this.PackTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackTypeGrid.GridId = "fbdd8782-3fea-41bf-bd5d-f6fb839b583f";
			this.PackTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackTypeGrid.LayoutKey = "PackTypeGrid";
			this.PackTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackTypeGrid.Name = "PackTypeGrid";
			this.PackTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 242, true);
			this.PackTypeGrid.TabIndex = 0;
			// 
			// UOMPackTypeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackTypeGrid);
			this.Name = "UOMPackTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 242, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackTypeGrid)).EndInit();
			this.PackTypeGrid.ResumeLayout(false);
			this.PackTypeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid PackTypeGrid;
	}
}

