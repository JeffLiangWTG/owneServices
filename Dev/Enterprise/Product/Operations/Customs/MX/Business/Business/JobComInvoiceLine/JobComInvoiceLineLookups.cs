using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public CodeDescriptionPairList VehicleMileageUQList => Parent.FirstVehicle.Lookups.MileageUQList;
	}
}
