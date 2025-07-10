namespace Enterprise.Registry.GUI
{
	partial class AddressListControl
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AddressListGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressListGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AddressListElement);
			// 
			// AddressListGrid
			// 
			this.AddressListGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressListGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AddressListElement)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AddressListElement)(null)).AddressType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AddressListElement)(null)).ControllerName)));
			this.AddressListGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AddressType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "ControllerName";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AddressListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AddressListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AddressListGrid.CopySelectedRowsAllowed = true;
			this.AddressListGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressListGrid.GridId = "25ca433f-bc07-4f9a-a2ed-afd2eea52616";
			this.AddressListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressListGrid.LayoutKey = "AddressListGrid";
			this.AddressListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.AddressListGrid.Name = "AddressListGrid";
			this.AddressListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 165, true);
			this.AddressListGrid.TabIndex = 0;
			// 
			// AddressListControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressListGrid);
			this.Name = "AddressListControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressListGrid)).EndInit();
			this.ResumeLayout(false);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		}

		Enterprise.ZArchitecture.ZGrid AddressListGrid;

		#endregion

		#endregion
	}
}
