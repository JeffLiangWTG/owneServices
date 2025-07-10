#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoicingForm
	{
		public PeriodicInvoiceControl PeriodicInvoiceControl_ForTestOnly
		{
			get { return periodicInvoiceControl; }
			set { periodicInvoiceControl = value; }
		}

		public ZGuidFindBox TaxBranchFindBox_ForTestOnly => TaxBranchFindBox;

		public ContinueWithSave CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly()
		{
			return CreatePeriodicInvoiceAndCheckSecurityRights();
		}

		public Business.JobInvoicing.PeriodicInvoicePostManager LastPostManagerForTestOnly_ForTestOnly
		{
			get { return LastPostManagerForTestOnly; }
			set { LastPostManagerForTestOnly = value; }
		}

		public void HandleSaveException_ForTestOnly(Exception e)
		{
			HandleSaveException(e);
		}

		protected internal virtual IBusiness BusinessEntityForValidation_ForTestOnly => BusinessEntityForValidation;
	}
}

#endif
