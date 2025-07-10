using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Products
{
	public partial class ExportProductsToCSVForm : KForm
	{
		Enterprise.ZArchitecture.ZLabel SupplierLabel;
		internal ZGuidFindBox SupplierFindBox;
		internal ZGuidFindBox ImporterFindBox;
		Enterprise.ZArchitecture.ZLabel ImporterLabel;
		CargoWise.Windows.UI.KCheckBox IncludeManuallyAddedCheckBox;
		CargoWise.Windows.UI.KCheckBox CloseTemporaryRecordsCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZButton OKBoundButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelBoundButton;

		protected void InitializeComponent()
		{
			this.SupplierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SupplierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImporterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IncludeManuallyAddedCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.CloseTemporaryRecordsCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.OKBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// SupplierLabel
			//
			this.SupplierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 48, true);
			this.SupplierLabel.Name = "SupplierLabel";
			this.SupplierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 24, true);
			this.SupplierLabel.TabIndex = 38;
			this.SupplierLabel.Text = "Supplier:";
			//
			// SupplierFindBox
			//
			this.SupplierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 48, true);
			this.SupplierFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.SupplierFindBox.Name = "SupplierFindBox";
			this.SupplierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 21, true);
			this.SupplierFindBox.TabIndex = 31;
			//
			// ImporterFindBox
			//
			this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 24, true);
			this.ImporterFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ImporterFindBox.Name = "ImporterFindBox";
			this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 21, true);
			this.ImporterFindBox.TabIndex = 30;
			//
			// ImporterLabel
			//
			this.ImporterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 24, true);
			this.ImporterLabel.Name = "ImporterLabel";
			this.ImporterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.ImporterLabel.TabIndex = 37;
			this.ImporterLabel.Text = "Importer:";
			//
			// IncludeManuallyAddedCheckBox
			//
			this.IncludeManuallyAddedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 80, true);
			this.IncludeManuallyAddedCheckBox.Name = "IncludeManuallyAddedCheckBox";
			this.IncludeManuallyAddedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.IncludeManuallyAddedCheckBox.TabIndex = 32;
			this.IncludeManuallyAddedCheckBox.Text = "Include manually added Products";
			//
			// CloseTemporaryRecordsCheckBox
			//
			this.CloseTemporaryRecordsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 111, true);
			this.CloseTemporaryRecordsCheckBox.Name = "CloseTemporaryRecordsCheckBox";
			this.CloseTemporaryRecordsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 24, true);
			this.CloseTemporaryRecordsCheckBox.TabIndex = 33;
			this.CloseTemporaryRecordsCheckBox.Text = "Close Temporary Records";
			//
			// OKBoundButton
			//
			this.OKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 162, true);
			this.OKBoundButton.Name = "OKBoundButton";
			this.OKBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKBoundButton.TabIndex = 34;
			this.OKBoundButton.Text = "OK";
			this.OKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			//
			// CancelBoundButton
			//
			this.CancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 162, true);
			this.CancelBoundButton.Name = "CancelBoundButton";
			this.CancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBoundButton.TabIndex = 35;
			this.CancelBoundButton.Text = "Cancel";
			this.CancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			//
			// ExportProductsToCSVForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 214, true);
			this.Controls.Add(this.OKBoundButton);
			this.Controls.Add(this.CancelBoundButton);
			this.Controls.Add(this.CloseTemporaryRecordsCheckBox);
			this.Controls.Add(this.IncludeManuallyAddedCheckBox);
			this.Controls.Add(this.SupplierLabel);
			this.Controls.Add(this.SupplierFindBox);
			this.Controls.Add(this.ImporterFindBox);
			this.Controls.Add(this.ImporterLabel);
			this.Name = "ExportProductsToCSVForm";
			this.Text = "Export Products - Austin csv-file";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing )
		{
			if (disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing );
		}
	}
}
