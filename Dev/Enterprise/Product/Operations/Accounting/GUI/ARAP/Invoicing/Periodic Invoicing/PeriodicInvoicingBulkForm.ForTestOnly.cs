#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoicingBulkForm
	{
		public PeriodicInvoiceBulkControl PeriodicInvoiceBulkControl_ForTestOnly
		{
			get { return periodicInvoiceBulkControl; }
			set { periodicInvoiceBulkControl = value; }
		}

		public ContinueWithSave ShowPreSaveDialogs_ForTestOnly()
		{
			return ShowPreSaveDialogs();
		}
	}
}

#endif
