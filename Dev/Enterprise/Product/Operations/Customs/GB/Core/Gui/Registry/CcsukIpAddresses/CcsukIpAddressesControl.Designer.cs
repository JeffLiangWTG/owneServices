using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GUI.Registry
{
	partial class CcsukIpAddressesControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AddressesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressesGrid)).BeginInit();
			this.AddressesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting);
			// 
			// AddressesGrid
			// 
			this.AddressesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting)(null)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting)(null)).LocalIpAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting)(null)).CcsukParticipantIpAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting)(null)).FriendlyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CcsukIpaddressesSetting)(null)).Transport)));
			this.AddressesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3500e55c-2f1d-48f5-8f2c-ee07a3074db7", "Sequence");
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0d4edec7-8972-45f3-9e04-d225ca61eefb", "Local IP for binding");
			zTextBoxColumnStyleInfo1.ColumnName = "LocalIpAddress";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("baf898f4-c191-41c0-b261-8dba97855caf", "Participant IP");
			zTextBoxColumnStyleInfo2.ColumnName = "CcsukParticipantIpAddress";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9cee328a-b86c-4bde-9518-15fba300288d", "Friendly Name");
			zTextBoxColumnStyleInfo3.ColumnName = "FriendlyName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2aa0f2aa-4cba-45ee-9da9-d5f0fe4bd9ce", "Transport");
			zDropEditColumnStyleInfo1.ColumnName = "Transport";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AddressesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AddressesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AddressesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressesGrid.GridId = "c3e6dec3-9fd8-41e6-8def-c25aab6efd60";
			this.AddressesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressesGrid.LayoutKey = "TaxCodesGrid";
			this.AddressesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressesGrid.Name = "AddressesGrid";
			this.AddressesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.AddressesGrid.TabIndex = 0;
			// 
			// CcsukIpAddressesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressesGrid);
			this.Name = "CcsukIpAddressesControl";
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
