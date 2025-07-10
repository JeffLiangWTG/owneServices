using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class ShipmentInspectionTypeControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.KnownShipperTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.KnownShipperTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ShipmentInspectionTypes);
			// 
			// KnownShipperTypesGrid
			// 
			this.KnownShipperTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.KnownShipperTypesGrid, "Types");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ShipmentInspectionTypes)(null)).Types)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ShipmentInspectionType)(((System.Collections.IList)(((Enterprise.Registry.Business.ShipmentInspectionTypes)(null)).Types)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ShipmentInspectionType)(((System.Collections.IList)(((Enterprise.Registry.Business.ShipmentInspectionTypes)(null)).Types)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ShipmentInspectionType)(((System.Collections.IList)(((Enterprise.Registry.Business.ShipmentInspectionTypes)(null)).Types)).SyncRoot)).AllowedOnPassengerFlights)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ShipmentInspectionType)(((System.Collections.IList)(((Enterprise.Registry.Business.ShipmentInspectionTypes)(null)).Types)).SyncRoot)).ShowInList)));
			this.KnownShipperTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("579dde21-d286-4cde-8a6c-22712294a3c9", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("954d4835-7bb7-421f-8995-df3af0c0a660", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d7b49a55-a8a5-4693-bdc0-7e8ec93838f2", "Passenger Flights", "Allowed on Passenger Flights", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "AllowedOnPassengerFlights";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9cf90f7f-85ae-4bd7-8802-698c2a3d724a", "Show in List");
			zCheckBoxColumnStyleInfo2.ColumnName = "ShowInList";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.KnownShipperTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.KnownShipperTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.KnownShipperTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.KnownShipperTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.KnownShipperTypesGrid.CopySelectedRowsAllowed = true;
			this.KnownShipperTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.KnownShipperTypesGrid.GridId = "f1dbdef4-0899-4811-863b-b5a67f17eda0";
			this.KnownShipperTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.KnownShipperTypesGrid.LayoutKey = "KnownShipperTypesGrid";
			this.KnownShipperTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.KnownShipperTypesGrid.Name = "KnownShipperTypesGrid";
			this.KnownShipperTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.KnownShipperTypesGrid.TabIndex = 0;
			// 
			// KnownShipperTypeControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.KnownShipperTypesGrid);
			this.Name = "KnownShipperTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.KnownShipperTypesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid KnownShipperTypesGrid;
	}
}
