using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class GenericChargeConfigurationControl
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

		Enterprise.ZArchitecture.ZGrid GenericChargeConfigurationControlGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.GenericChargeConfigurationControlGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GenericChargeConfigurationControlGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GenericChargeConfiguration);
			// 
			// DirectDebitFileURLGrid
			// 
			this.GenericChargeConfigurationControlGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GenericChargeConfigurationControlGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GenericChargeConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GenericChargeConfiguration)(null)).ChargePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GenericChargeConfiguration)(null)).ChargeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GenericChargeConfiguration)(null)).ChargeDescription)));
			this.GenericChargeConfigurationControlGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DoNotQueueInvoicesContainingSpecificChargesForTransmission|19A44353-4D71-416D-970D-5AB8C99B0E82", "Charge");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargePK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DoNotQueueInvoicesContainingSpecificChargesForTransmission|5EE21A3A-29E1-413A-9CAA-35A10AABF8A3", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.GenericChargeConfigurationControlGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.GenericChargeConfigurationControlGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GenericChargeConfigurationControlGrid.GridId = "41C1EAF8-00BF-4F89-A9F0-F104DFE4F1F2";
			this.GenericChargeConfigurationControlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenericChargeConfigurationControlGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GenericChargeConfigurationControlGrid.LayoutKey = "zGrid1";
			this.GenericChargeConfigurationControlGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GenericChargeConfigurationControlGrid.Name = "GenericChargeConfigurationControlGrid";
			this.GenericChargeConfigurationControlGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			this.GenericChargeConfigurationControlGrid.TabIndex = 0;
			// 
			// GenericChargeConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GenericChargeConfigurationControlGrid);
			this.Name = "GenericChargeConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GenericChargeConfigurationControlGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GenericChargeConfigurationControlGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}
