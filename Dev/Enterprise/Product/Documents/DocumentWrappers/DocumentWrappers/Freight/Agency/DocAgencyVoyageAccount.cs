using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocAgencyVoyageAccount : DocBaseWrapper
	{
		DocAgencyVoyageAccount(VoyageAccount account, BusinessObjectFactory factory)
			: base(account, factory) { }

		public static DocAgencyVoyageAccount New(VoyageAccount account, BusinessObjectFactory factory)
		{
			return account == null ? null : new DocAgencyVoyageAccount(account, factory);
		}

		public ZString JobNumber
		{
			get { return Account.NA_JobNumber; }
		}

		public DocOrganisation Principal
		{
			get { return DocOrganisation.New(Account.Header, Factory); }
		}

		public DocVoyage Voyage
		{
			get { return DocVoyage.New(Account.Voyage, Factory); }
		}

		public DocVoyageAccountDisbursementLineCollection DisbursmentLines
		{
			get
			{
				if (disbursmentLines == null)
				{
					Job job = new Job.Loader(Account).Load();
					Charge[] charges = (job == null) ? charges = System.Array.Empty<Charge>() : charges = job.Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
					disbursmentLines = DocVoyageAccountDisbursementLineCollection.New(charges, Factory);
				}
				return disbursmentLines;
			}
		}
		DocVoyageAccountDisbursementLineCollection disbursmentLines;

		#region Implementation

		VoyageAccount Account
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VoyageAccount)WrappedObject; }
		}

		#endregion
	}
}
