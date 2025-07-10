#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction
{
	public partial class APIncompleteInvoicesModule
	{
		public bool CanBeCopied_ForTestOnly()
		{
			return CanBeCopied();
		}

		public MenuItem[] GetNewStandardMenuItems_ForTestOnly()
		{
			return GetNewStandardMenuItems();
		}

		public ResourceStringData CancelIncompleteInvoiceMenuText_ForTestOnly => CancelIncompleteInvoiceMenuText;

		public IBusinessObjectCollection GetNewGridCollection_ForTestOnly()
		{
			return GetNewGridCollection();
		}

		public void HandleCancel_ForTestOnly(object sender, EventArgs e)
		{
			HandleCancel(sender, e);
		}

		public ZController GetNewController_ForTestOnly(BusinessObject selectedBusinessObject)
		{
			return GetNewController(selectedBusinessObject);
		}

		public ZQuery ExportQuery_ForTestOnly => ExportQuery;
	}
}

#endif
