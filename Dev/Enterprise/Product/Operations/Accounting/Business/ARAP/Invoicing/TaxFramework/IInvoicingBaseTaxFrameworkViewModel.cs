using System;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IInvoicingBaseTaxFrameworkViewModel
	{
		ZString ValidateForPosting();
		void TrackHasChanges(bool shouldTrackHasChanges);
		IDisposable SuspendTrackingHasChanges { get; }
	}
}
