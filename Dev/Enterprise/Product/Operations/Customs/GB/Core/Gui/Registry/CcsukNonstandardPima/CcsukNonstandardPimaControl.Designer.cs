using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GUI.Registry
{
	partial class CcsukNonstandardPimaControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AddressesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressesGrid)).BeginInit();
			this.AddressesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Registry.CcsukNonstandardPimaSetting);
			// 
			// AddressesGrid
			// 
			this.AddressesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.CcsukNonstandardPimaSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukNonstandardPimaSetting)(null)).AirportAndShed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukNonstandardPimaSetting)(null)).MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukNonstandardPimaSetting)(null)).TypeBPima)));
			this.AddressesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9cde1e29-263e-4e96-af30-007b1e1b11ed", "Airport and Shed");
			zTextBoxColumnStyleInfo1.ColumnName = "AirportAndShed";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f105c712-d9ce-422e-b738-e5b414812766", "Message Type");
			zDropEditColumnStyleInfo1.ColumnName = "MessageType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("675e8817-63d3-460c-b33c-52ad0341c578", "Type B PIMA");
			zTextBoxColumnStyleInfo2.ColumnName = "TypeBPima";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AddressesGrid.CopySelectedRowsAllowed = true;
			this.AddressesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressesGrid.GridId = "c3e6dec3-9fd8-41e6-8def-c25aab6efd60";
			this.AddressesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressesGrid.LayoutKey = "TaxCodesGrid";
			this.AddressesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressesGrid.Name = "AddressesGrid";
			this.AddressesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.AddressesGrid.TabIndex = 0;
			// 
			// CcsukNonstandardPimaControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressesGrid);
			this.Name = "CcsukNonstandardPimaControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressesGrid)).EndInit();
			this.AddressesGrid.ResumeLayout(false);
			this.AddressesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZGrid AddressesGrid;

	}
}
