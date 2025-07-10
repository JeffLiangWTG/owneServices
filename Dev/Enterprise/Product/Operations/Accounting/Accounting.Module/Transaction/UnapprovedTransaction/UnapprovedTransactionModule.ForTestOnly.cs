#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedTransactionModule
	{
		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}

		public ZQuery ExportQuery_ForTestOnly => ExportQuery;

		public IFilterControl GetNewFilterControl_ForTestOnly()
		{
			return GetNewFilterControl();
		}

		public ZController GetNewControllerFromCreator_ForTestOnly(AccTransactionHeader selectedHeader)
		{
			return GetNewControllerFromCreator(selectedHeader);
		}

		public MenuItem[] GetNewStandardMenuItems_ForTestOnly()
		{
			return GetNewStandardMenuItems();
		}

		public MultilingualString NewCreditNoteMenuText_ForTestOnly => NewCreditNoteMenuText;

		public MultilingualString NewInvoiceMenuText_ForTestOnly => NewInvoiceMenuText;

		public void HandleAlreadyPosted_ForTestOnly(object sender, EventArgs e)
		{
			HandleAlreadyPosted(sender, e);
		}

		public void UAInvoicesModule_Approve_Click_ForTestOnly(object sender, EventArgs e)
		{
			UAInvoicesModule_Approve_Click(sender, e);
		}

		public Business.ARAP.Invoicing.UnapprovedTransactionConverter GetUnapprovedTransactionConverter_ForTestOnly(BusinessObject[] selectedObjects)
		{
			return GetUnapprovedTransactionConverter(selectedObjects);
		}
	}
}

#endif
