using System.ComponentModel;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class ElectronicProcessingChargeDescriptionOverrideControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		IContainer components = null;

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

		internal Enterprise.ZArchitecture.ZGrid ElectronicProcessingChargeDescriptionOverrideGrid;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ElectronicProcessingChargeDescriptionOverrideGrid.ReadOnly = readOnly;
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo transportDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo containerDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo shipmentTypeEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo originDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo originDescriptionBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo destinationDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo destinationDescriptionBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo prefixSuffixDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo textBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo includeShipmentNumberCheckBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();

			this.ElectronicProcessingChargeDescriptionOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicProcessingChargeDescriptionOverrideGrid)).BeginInit();
			this.ElectronicProcessingChargeDescriptionOverrideGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverrideCollection);
			// 
			// ElectronicProcessingChargeDescriptionOverrideGrid
			// 
			this.ElectronicProcessingChargeDescriptionOverrideGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ElectronicProcessingChargeDescriptionOverrideGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).Transport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).Container)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).ShipmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).OriginDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).DestinationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).PrefixSuffix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).Text)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeDescriptionOverride)(null)).IncludeShipmentNumber)));

			transportDropEditColumnStyleInfo.ColumnName = "Transport";
			transportDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("67B4E9B2-0FF9-451D-86B9-98C48CD6B040", "Transport");
			transportDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			containerDropEditColumnStyleInfo.ColumnName = "Container";
			containerDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C3250C15-47CF-45DB-BEDD-F33C267C6D68", "Container");
			containerDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			shipmentTypeEditColumnStyleInfo.ColumnName = "ShipmentType";
			shipmentTypeEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3A62D844-C767-4A29-B5F5-A5A6C962F3E3", "Shipment Type");
			shipmentTypeEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			originDropEditColumnStyleInfo.ColumnName = "Origin";
			originDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B7F076D2-5EBC-4BB3-BB63-174B2573204B", "Origin");
			originDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			originDescriptionBoxColumnStyleInfo.ColumnName = "OriginDescription";
			originDescriptionBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C1187DED-8EDC-425F-8011-60C49C69DB55", "Origin Description");
			originDescriptionBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			destinationDropEditColumnStyleInfo.ColumnName = "Destination";
			destinationDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("98B8B380-4BC8-4B28-8BCB-F06B55EB0133", "Destination");
			destinationDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			destinationDescriptionBoxColumnStyleInfo.ColumnName = "DestinationDescription";
			destinationDescriptionBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A43E1064-0CE2-4D0F-8FC4-E89E7C1B59A8", "Destination Description");
			destinationDescriptionBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			prefixSuffixDropEditColumnStyleInfo.ColumnName = "PrefixSuffix";
			prefixSuffixDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5A467643-6FD8-471E-92C1-5C1BB9DE5D74", "Prefix/Suffix");
			prefixSuffixDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			textBoxColumnStyleInfo.ColumnName = "Text";
			textBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8CC13A05-F123-46F8-825A-0A9415958F6C", "Text");
			textBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			includeShipmentNumberCheckBoxColumnStyleInfo.ColumnName = "IncludeShipmentNumber";
			includeShipmentNumberCheckBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AE91D363-19D2-47C5-8E58-243E12D74ABF", "Include Shipment Number");
			includeShipmentNumberCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(transportDropEditColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(containerDropEditColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(shipmentTypeEditColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(originDropEditColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(originDescriptionBoxColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(destinationDropEditColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(destinationDescriptionBoxColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(prefixSuffixDropEditColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(textBoxColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ColumnStyles.Add(includeShipmentNumberCheckBoxColumnStyleInfo);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.CaptionVisible = false;
			this.ElectronicProcessingChargeDescriptionOverrideGrid.GridId = "91C8C696-3D7B-434A-BB40-8BA90515D26F";
			this.ElectronicProcessingChargeDescriptionOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ElectronicProcessingChargeDescriptionOverrideGrid.LayoutKey = "ElectronicProcessingChargeDescriptionOverrideGrid";
			this.ElectronicProcessingChargeDescriptionOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.Name = "ElectronicProcessingChargeDescriptionOverrideGrid";
			this.ElectronicProcessingChargeDescriptionOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 372, true);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.TabIndex = 0;
			this.ElectronicProcessingChargeDescriptionOverrideGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ElectronicProcessingChargeDescriptionOverrideControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ElectronicProcessingChargeDescriptionOverrideGrid);
			this.Name = "ElectronicProcessingChargeDescriptionOverrideControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicProcessingChargeDescriptionOverrideGrid)).EndInit();
			this.ElectronicProcessingChargeDescriptionOverrideGrid.ResumeLayout(false);
			this.ElectronicProcessingChargeDescriptionOverrideGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

