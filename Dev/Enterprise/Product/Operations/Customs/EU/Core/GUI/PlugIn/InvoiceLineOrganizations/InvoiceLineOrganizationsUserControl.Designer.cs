
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class InvoiceLineOrganizationsUserControl
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
			this.ConsignorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SupplyChainActorReferencesUserControl = new Enterprise.Customs.EU.GUI.PlugIn.SupplyChainActorReferencesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsignorAddressControl.SuspendLayout();
			this.ConsigneeAddressControl.SuspendLayout();
			this.SupplyChainActorReferencesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// ConsignorAddressControl
			// 
			this.ConsignorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorAddressControl, "FilteredInvoiceLines.JI_OA_ExporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OA_ExporterAddress)));
			this.ConsignorAddressControl.BindToOrgList = "FilteredInvoiceLines.Lookups+ConsignorList";
			this.ConsignorAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("0AAE1CAE-410C-4C4A-8EF8-F3DAB5BFF035", "Consignor");
			this.ConsignorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 18, true);
			this.ConsignorAddressControl.Name = "ConsignorAddressControl";
			this.ConsignorAddressControl.PopupCaption = "";
			this.ConsignorAddressControl.ReadOnly = false;
			this.ConsignorAddressControl.ShowAddress = false;
			this.ConsignorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsignorAddressControl.TabIndex = 0;
			// 
			// ConsigneeAddressControl
			// 
			this.ConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeAddressControl, "FilteredInvoiceLines.JI_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OA_ConsigneeAddress)));
			this.ConsigneeAddressControl.BindToOrgList = "FilteredInvoiceLines.Lookups+Consignees";
			this.ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 43, true);
			this.ConsigneeAddressControl.Name = "ConsigneeAddressControl";
			this.ConsigneeAddressControl.PopupCaption = "";
			this.ConsigneeAddressControl.ReadOnly = false;
			this.ConsigneeAddressControl.ShowAddress = false;
			this.ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsigneeAddressControl.TabIndex = 1;
			// 
			// SupplyChainActorReferencesUserControl
			// 
			this.SupplyChainActorReferencesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplyChainActorReferencesUserControl, "FilteredInvoiceLines.CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusSupplyChainActorReferences)));
			this.SupplyChainActorReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.SupplyChainActorReferencesUserControl.Name = "SupplyChainActorReferencesUserControl";
			this.SupplyChainActorReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 195, true);
			this.SupplyChainActorReferencesUserControl.TabIndex = 4;
			// 
			// InvoiceLineOrganizationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplyChainActorReferencesUserControl);
			this.Controls.Add(this.ConsignorAddressControl);
			this.Controls.Add(this.ConsigneeAddressControl);
			this.Name = "InvoiceLineOrganizationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 271, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsignorAddressControl.ResumeLayout(true);
			this.ConsignorAddressControl.PerformLayout();
			this.ConsigneeAddressControl.ResumeLayout(true);
			this.ConsigneeAddressControl.PerformLayout();
			this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
			this.SupplyChainActorReferencesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public ZAddressControl ConsignorAddressControl;
		public ZAddressControl ConsigneeAddressControl;
		protected SupplyChainActorReferencesUserControl SupplyChainActorReferencesUserControl;

		#endregion
	}
}
