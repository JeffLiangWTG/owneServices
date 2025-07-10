using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUNonLayoutImportSupplierHeaderUserControl : EUImportSupplierHeaderUserControl
	{
		public EUNonLayoutImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			var invoiceDetailUserControl = this.DetailsGroupBox.FindSingleOrDefault<ZUserControl>("InvoiceDetailUserControl");
			if (invoiceDetailUserControl != null)
			{
				DetailsGroupBox.Controls.Remove(invoiceDetailUserControl);
				invoiceDetailUserControl.Dispose();
			}
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = (NoResString)"GridLayoutuPuRmiBKZyWIsTINXw2I/g==";
		}

		protected void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			IncoTermDescriptionForm form = new IncoTermDescriptionForm(JZ_IncoTermBoundDropDownEdit.Text);
			ZFormModaliser.Show(form, ParentForm as ZForm);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			JZ_IncoTermInfo_ValueChanged(null, null);
		}

		void JZ_IncoTermInfo_ValueChanged(object sender, EventArgs e)
		{
			AgreedPlaceCodeFindBox.Visible = ((JobComInvoiceHeader)CurrentInvoiceHeader)?.AgreedPlaceCodeSupportAndVisible ?? false;
		}

		protected override void HookInvoiceHeaderEvents(JobComInvoiceHeader invoiceHeader)
		{
			if (invoiceHeader is JobComInvoiceHeader currentInvoiceHeader)
			{
				currentInvoiceHeader.JZ_IncoTermInfo.ValueChanged -= JZ_IncoTermInfo_ValueChanged;
				currentInvoiceHeader.JZ_IncoTermInfo.ValueChanged += JZ_IncoTermInfo_ValueChanged;
			}
		}

		protected override void UnHookInvoiceHeaderEvents(JobComInvoiceHeader invoiceHeader)
		{
			if (invoiceHeader is JobComInvoiceHeader currentInvoiceHeader)
			{
				currentInvoiceHeader.JZ_IncoTermInfo.ValueChanged -= JZ_IncoTermInfo_ValueChanged;
			}
		}

		protected override void UpdateCurrentInvoice()
		{
			base.UpdateCurrentInvoice();
			JZ_IncoTermInfo_ValueChanged(null, null);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveBuyerAddressColumn();
		}

		void RemoveBuyerAddressColumn()
		{
			if (JobDeclaration is JobDeclaration declaration && !declaration.IsUCC6)
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_BuyerAddress, "BuyerOrgPK");
			}
		}
	}
}
