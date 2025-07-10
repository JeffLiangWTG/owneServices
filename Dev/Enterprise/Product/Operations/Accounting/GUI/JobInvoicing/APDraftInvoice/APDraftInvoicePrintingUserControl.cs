using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APDraftInvoicePrintingUserControl : ZUserControl
	{
		public APDraftInvoicePrintingUserControl()
		{
			InitializeComponent();
			ControlDpiScalingHelper.SetTop(ref FindButton, TransactionTypeDropEdit.Top, false);
			ControlDpiScalingHelper.SetTop(ref ClearButton, TransactionTypeDropEdit.Top, false);
		}

		#region Binding

		JobDraftInvoicePrintingFilter InvoiceFilterObject;

		public void Bind(JobDraftInvoicePrintingFilter invoiceFilterObject)
		{
			this.InvoiceFilterObject = invoiceFilterObject;
			base.SetDataBinding(invoiceFilterObject, "");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Do Nothing. Handling manually.
		}

		#endregion

		#region Implementation

		void FindButton_Click(object sender, EventArgs e)
		{
			if (InvoiceFilterObject != null)
			{
				InvoiceFilterObject.RefreshInvoiceList();
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			if (InvoiceFilterObject != null)
			{
				InvoiceFilterObject.ResetInvoiceList();
			}
		}

		void UploadInvoiceButton_Click(object sender, EventArgs eventArgs)
		{
			if (InvoiceFilterObject?.JobParent == null)
			{
				return;
			}

			var parentTableCode = InvoiceFilterObject.JobParent.TablePrefix;
			var parentID = InvoiceFilterObject.JobParent.PK;

			var additionalQueryStrings = new[]
			{
				("parentTableCode", parentTableCode),
				("parentID", parentID.ToString())
			};
			GlowLinksHelper.OpenEnityInGlow(GlobalNotificationsWrapper.Instance, "Goto/UploadInvoice_33c172f64d7245f89c4da047f5a18fcc", InvoiceFilterObject.JobParent, additionalQueryStrings);
		}

		#endregion
	}
}
