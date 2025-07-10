using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUNonLayoutExportSupplierHeaderUserControl : EUExportSupplierHeaderUserControl
	{
		public EUNonLayoutExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			RemoveAndDisposeInvoiceDetailUserControl();
		}

		void RemoveAndDisposeInvoiceDetailUserControl()
		{
			var invoiceDetailUserControl = DetailsGroupBox.FindSingleOrDefault<ZUserControl>("InvoiceDetailUserControl");
			if (invoiceDetailUserControl != null)
			{
				DetailsGroupBox.Controls.Remove(invoiceDetailUserControl);
				invoiceDetailUserControl.Dispose();
			}
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

			var declaration = JobDeclaration;
			if (declaration != null)
			{
				var isMiscellaneous = declaration.IsMiscellaneous;
				TransportChargesMethodOfPaymentDropEdit.Visible = !isMiscellaneous;
				JZ_InvoiceCurrLandedCostExRateCalcEdit.Visible = isMiscellaneous;
			}
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
	}
}
