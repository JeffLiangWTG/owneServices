using System;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public class JASInvoicingFormHelper
	{
		public JASInvoicingFormHelper(IJXCExportForm form)
		{
			this.Form = form;
			CheckForNullArgumentAndInvalidOperation();
			AddExportToJXCFinancialMessageMenuItem();
		}

		void AddExportToJXCFinancialMessageMenuItem()
		{
			if (Form.BusinessEntity.IsInDatabaseIncludingChildren)
			{
				ZForm zForm = Form as ZForm;
				if (zForm != null)
				{
					ZFormMenuStrategy.AddActionsMenuItem(zForm, MenuItemName, new EventHandler(ExportJXCMenuItem_Clicked));
				}
			}
		}

		void ExportJXCMenuItem_Clicked(object sender, EventArgs args)
		{
			HandleJXCMenuItem();
		}

		void HandleJXCMenuItem()
		{
			InvoiceWrapper invoiceWrapper = new InvoiceWrapper(JASInvoicingBase);
			INVCDTMessageExporter exporter = new INVCDTMessageExporter(invoiceWrapper);
			JXCMessageGUIExportDirector director = new JXCMessageGUIExportDirector(exporter, Form);
			if (director.EnsureMessageCanBeExported())
			{
				director.Export();
			}
		}

		void CheckForNullArgumentAndInvalidOperation()
		{
			if (Form == null)
			{
				throw new ArgumentNullException("Form");
			}

			if (JASInvoicingBase == null)
			{
				throw new InvalidOperationException("Should be passing a IJXCExportForm with IJASInvoicingBase as BusinessEntity");
			}
		}

		IJASInvoicingBase JASInvoicingBase
		{
			get { return Form.BusinessEntity as IJASInvoicingBase; }
		}

		public readonly IJXCExportForm Form;
		const string MenuItemName = "Export JXC Financial Message";
	}
}

#region Implementation
#region class JASARCreditNoteForTest
#endregion
#endregion
