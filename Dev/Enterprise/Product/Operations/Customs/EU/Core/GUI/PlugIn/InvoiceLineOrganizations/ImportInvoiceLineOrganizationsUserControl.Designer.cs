
namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class ImportInvoiceLineOrganizationsUserControl
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
			this.BuyerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SellerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SupplyChainActorReferencesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BuyerDocAddressControl.SuspendLayout();
			this.SellerDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SupplyChainActorReferencesUserControl
			// 
			this.SupplyChainActorReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
			// 
			// BuyerDocAddressControl
			// 
			this.BuyerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerDocAddressControl, "FilteredInvoiceLines.BuyerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).BuyerDocAddress)));
			this.BuyerDocAddressControl.BindToOrganisations = "FilteredInvoiceLines.Lookups+BuyerList";
			this.BuyerDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("847F45C0-2F16-4D0B-A593-13E608A0F1ED", "Buyer");
			this.BuyerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.BuyerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 68, true);
			this.BuyerDocAddressControl.Name = "BuyerDocAddressControl";
			this.BuyerDocAddressControl.ReadOnly = false;
			this.BuyerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 290;
			this.BuyerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.BuyerDocAddressControl.TabIndex = 2;
			this.BuyerDocAddressControl.ValidationJustForced = false;
			// 
			// SellerDocAddressControl
			// 
			this.SellerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerDocAddressControl, "FilteredInvoiceLines.SellerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SellerDocAddress)));
			this.SellerDocAddressControl.BindToOrganisations = "FilteredInvoiceLines.Lookups+SellerList";
			this.SellerDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("0C1CAB0C-CB53-460A-9A9D-51AC10E77F9B", "Seller");
			this.SellerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.SellerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 93, true);
			this.SellerDocAddressControl.Name = "SellerDocAddressControl";
			this.SellerDocAddressControl.ReadOnly = false;
			this.SellerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 290;
			this.SellerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.SellerDocAddressControl.TabIndex = 3;
			this.SellerDocAddressControl.ValidationJustForced = false;
			// 
			// ImportInvoiceLineOrganizationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BuyerDocAddressControl);
			this.Controls.Add(this.SellerDocAddressControl);
			this.Name = "ImportInvoiceLineOrganizationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 327, true);
			this.Controls.SetChildIndex(this.SupplyChainActorReferencesUserControl, 0);
			this.Controls.SetChildIndex(this.SellerDocAddressControl, 0);
			this.Controls.SetChildIndex(this.BuyerDocAddressControl, 0);
			this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
			this.SupplyChainActorReferencesUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BuyerDocAddressControl.ResumeLayout(true);
			this.BuyerDocAddressControl.PerformLayout();
			this.SellerDocAddressControl.ResumeLayout(true);
			this.SellerDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public MasterFiles.GUI.ZDocAddressControl BuyerDocAddressControl;
		public MasterFiles.GUI.ZDocAddressControl SellerDocAddressControl;
	}
}
