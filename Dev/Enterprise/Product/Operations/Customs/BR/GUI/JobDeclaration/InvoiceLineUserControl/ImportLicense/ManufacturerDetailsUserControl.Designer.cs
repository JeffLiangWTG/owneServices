using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ManufacturerDetailsUserControl
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
			this.ManufacturerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JI_CountryOfOriginBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ManufacturerIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManufacturerContactDetailDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManufacturerPanel.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.ManufacturerIndicatorDropEdit.SuspendLayout();
			this.ManufacturerContactDetailDocAddress.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// ManufacturerPanel
			// 
			this.ManufacturerPanel.Controls.Add(this.JI_CountryOfOriginBoundFindBox);
			this.ManufacturerPanel.Controls.Add(this.ManufacturerIndicatorDropEdit);
			this.ManufacturerPanel.Controls.Add(this.ManufacturerContactDetailDocAddress);
			this.ManufacturerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManufacturerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManufacturerPanel.Name = "ManufacturerPanel";
			this.ManufacturerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 403, true);
			this.ManufacturerPanel.TabIndex = 0;
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CountryOfOriginBoundFindBox, "JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_CountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).Lookups.CountryList)));
			this.JI_CountryOfOriginBoundFindBox.BindToList = "Lookups+CountryList";
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 217, true);
			this.JI_CountryOfOriginBoundFindBox.Name = "JI_CountryOfOriginBoundFindBox";
			this.JI_CountryOfOriginBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.JI_CountryOfOriginBoundFindBox.ParentType = null;
			this.JI_CountryOfOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.JI_CountryOfOriginBoundFindBox.TabIndex = 3;
			// 
			// ManufacturerIndicatorDropEdit
			// 
			this.ManufacturerIndicatorDropEdit.AllowDrop = true;
			this.ManufacturerIndicatorDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ManufacturerIndicatorDropEdit, "JI_ManufacturerIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ManufacturerIndicator)));
			this.ManufacturerIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 3, true);
			this.ManufacturerIndicatorDropEdit.Name = "ManufacturerIndicatorDropEdit";
			this.ManufacturerIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ManufacturerIndicatorDropEdit.TabIndex = 1;
			// 
			// ManufacturerContactDetailDocAddress
			// 
			this.ManufacturerContactDetailDocAddress.AddressValidationProcessCmdKey = null;
			this.ManufacturerContactDetailDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerContactDetailDocAddress, "ManufacturerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ManufacturerDocAddress)));
			this.ManufacturerContactDetailDocAddress.BindToOrganisations = "FilteredInvoiceLines.Lookups.SupplierList";
			this.ManufacturerContactDetailDocAddress.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2CDD0C68-119C-44DE-8FC4-FBCA4EF63B7C", "Manufacturer");
			this.ManufacturerContactDetailDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.ManufacturerContactDetailDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 29, true);
			this.ManufacturerContactDetailDocAddress.Name = "ManufacturerContactDetailDocAddress";
			this.ManufacturerContactDetailDocAddress.ReadOnly = true;
			this.ManufacturerContactDetailDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.ManufacturerContactDetailDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ManufacturerContactDetailDocAddress.TabIndex = 2;
			this.ManufacturerContactDetailDocAddress.ValidationJustForced = false;
			// 
			// ManufacturerDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManufacturerPanel);
			this.Name = "ManufacturerDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 403, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManufacturerPanel.ResumeLayout(false);
			this.ManufacturerPanel.PerformLayout();
			this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
			this.JI_CountryOfOriginBoundFindBox.PerformLayout();
			this.ManufacturerIndicatorDropEdit.ResumeLayout(true);
			this.ManufacturerIndicatorDropEdit.PerformLayout();
			this.ManufacturerContactDetailDocAddress.ResumeLayout(true);
			this.ManufacturerContactDetailDocAddress.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel ManufacturerPanel;
		internal ZDocAddressControl ManufacturerContactDetailDocAddress;
		internal ZArchitecture.GUI.ZDropEdit ManufacturerIndicatorDropEdit;
		internal ZCodeFindBox JI_CountryOfOriginBoundFindBox;
	}
}
