using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargeTargetManager
	{
		readonly BaseCharge charge;
		readonly BusinessObjectFactory factory;

		public JobChargeTargetManager(BaseCharge charge)
		{
			this.charge = charge;
			this.factory = charge.Factory;
		}

		#region InvoiceTarget

		public ZString InvoiceTargetJobNumber
		{
			get
			{
				var job = factory.Load(InvoiceTargetTableCode, InvoiceTargetID) as IJobHeaderParent;

				if (job != null)
				{
					return job.JobNumber;
				}

				return invalidInvoiceTarget;
			}
			set
			{
				var jobInvoicingPlugin = charge.GetInvoiceTargetsIfEnabled().FirstOrDefault(x => x.JobNumber == value);		//FirstOrDefault because there is a validation to prevent duplicate selection

				if (jobInvoicingPlugin == null)
				{
					InvoiceTargetID = ZGuid.Empty;
					InvoiceTargetTableCode = ZString.Empty;
					DeleteIfEmpty();
					invalidInvoiceTarget = value;
				}
				else
				{
					invalidInvoiceTarget = ZString.Empty;
					InvoiceTargetID = jobInvoicingPlugin.PK;

					InvoiceTargetTableCode = jobInvoicingPlugin.TablePrefix();
				}
			}
		}
		ZString invalidInvoiceTarget = ZString.Empty;

		ZGuid InvoiceTargetID
		{
			get => JobChargeTarget?.JRT_InvoiceTargetID ?? ZGuid.Empty;
			set
			{
				if (value.IsEmpty && JobChargeTarget == null)
				{
					return;
				}

				GetOrCreate().JRT_InvoiceTargetID = value;
			}
		}

		ZString InvoiceTargetTableCode
		{
			get => JobChargeTarget?.JRT_InvoiceTargetTableCode ?? ZString.Empty;
			set
			{
				if (value.IsEmpty && JobChargeTarget == null)
				{
					return;
				}

				GetOrCreate().JRT_InvoiceTargetTableCode = value;
			}
		}

		#endregion

		#region RelatedJob

		public IJobInvoicingPlugIn RelatedJob => RelatedJobID.IsValid ? factory.GetCachedValue(RelatedJobID.ToStringKey(), GetRelatedJob) : null;

		IJobInvoicingPlugIn GetRelatedJob()
		{
			return factory.Load(RelatedJobTableCode, RelatedJobID) as IJobInvoicingPlugIn;
		}

		public ZString RelatedJobNumber
		{
			get
			{
				var job = RelatedJob;

				if (job != null)
				{
					return job.JobNumber;
				}

				return invalidRelatedJob;
			}
			set
			{
				var jobInvoicingPlugin = charge.RelatedShipments().SingleOrDefault(x => x.JobNumber == value);

				if (jobInvoicingPlugin == null)
				{
					RelatedJobID = ZGuid.Empty;
					RelatedJobTableCode = ZString.Empty;
					DeleteIfEmpty();
					invalidRelatedJob = value;
				}
				else
				{
					invalidRelatedJob = ZString.Empty;
					RelatedJobID = jobInvoicingPlugin.PK;
					RelatedJobTableCode = jobInvoicingPlugin.TablePrefix();
				}
			}
		}
		ZString invalidRelatedJob = ZString.Empty;

		internal ZGuid RelatedJobID
		{
			get => JobChargeTarget?.JRT_RelatedJobID ?? ZGuid.Empty;
			set
			{
				if (value.IsEmpty && JobChargeTarget == null)
				{
					return;
				}

				GetOrCreate().JRT_RelatedJobID = value;
			}
		}

		ZString RelatedJobTableCode
		{
			get => JobChargeTarget?.JRT_RelatedJobTableCode ?? ZString.Empty;
			set
			{
				if (value.IsEmpty && JobChargeTarget == null)
				{
					return;
				}

				GetOrCreate().JRT_RelatedJobTableCode = value;
			}
		}

		#endregion

		JobChargeTarget GetOrCreate()
		{
			if (JobChargeTarget != null)
			{
				return JobChargeTarget;
			}

			var result = factory.New<JobChargeTarget>();
			jobChargeTargetPK = result.PK;
			result.JRT_JR = charge.PK;
			return result;
		}

		void DeleteIfEmpty()
		{
			if (InvoiceTargetID.IsEmpty && RelatedJobID.IsEmpty)
			{
				JobChargeTarget?.Delete();
				jobChargeTargetPK = ZGuid.Empty;
			}
		}

		JobChargeTarget JobChargeTarget => !JobChargeTargetPK.IsEmpty ? factory.Load<JobChargeTarget>(JobChargeTargetPK) : null;

		ZGuid JobChargeTargetPK => jobChargeTargetPK ?? (jobChargeTargetPK = LoadJobChargeTargetByChargePK()).Value;
		ZGuid? jobChargeTargetPK;

		ZGuid LoadJobChargeTargetByChargePK()
		{
			var query = new ZQuery(JobChargeTargetSchema.JRT_JR, charge.PK);
			query.FetchOnlyFromLocalCache = !charge.IsInDatabase;

			return factory.LoadTop1<JobChargeTarget>(query)?.PK ?? ZGuid.Empty;
		}
	}
}
