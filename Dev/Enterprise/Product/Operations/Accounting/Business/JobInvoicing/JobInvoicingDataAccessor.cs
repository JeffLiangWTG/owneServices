using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobInvoicingDataAccessor
	{
		public JobInvoicingDataAccessor(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public JobInvoicingDataAccessor()
			: this(new BusinessObjectFactory())
		{
		}

		public ZGuid GetJobFromForeignKey(IJobHeaderParent shipment)
		{
			return GetJobFromForeignKey(shipment, true);
		}

		public ZGuid GetJobFromForeignKey(IJobHeaderParent shipment, bool restrictToThisCompany)
		{
			IJobHeaderParent[] shipments = { shipment };
			ZGuid[] result = GetJobsFromShipment(shipments, restrictToThisCompany, false);
			return result.Length == 1 ? result[0] : ZGuid.Empty;
		}

		public ZGuid[] GetJobsFromShipment(IJobHeaderParent[] shipments)
		{
			return GetJobsFromShipment(shipments, true);
		}

		public ZGuid[] GetJobsFromShipment(IJobHeaderParent[] shipments, bool restrictToThisCompany)
		{
			return GetJobsFromShipment(shipments, restrictToThisCompany, true);
		}

		public ZGuid[] GetJobsFromShipment(IJobHeaderParent[] shipments, bool restrictToThisCompany, bool includeRelatedJobs)
		{
			List<ZGuid> result = new List<ZGuid>();

			if (shipments.Length > 0)
			{
				ZQuery jobPKFilter = new ZQuery();
				List<ZGuid> parentPKs = new List<ZGuid>();

				foreach (IJobHeaderParent shipment in shipments)
				{
					if (shipment != null)
					{
						parentPKs.Add(shipment.PK);
						if (includeRelatedJobs && shipment is IJobInvoicingPlugInAdditionalJobs)
						{
							foreach (IJobInvoicingPlugIn plugIn in ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor)
							{
								parentPKs.Add(plugIn.PK);
							}
						}
					}
				}

				ZQuery parentJobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, parentPKs.ToArray());
				if (restrictToThisCompany)
				{
					parentJobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				}
				else
				{
					parentJobQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				}

				IEnumerable<Job> parentJobs = Factory.Load<Job>(new JobCollection(Factory, parentJobQuery).CompleteFilter);
				foreach (Job job in parentJobs)
				{
					if (job.Parent == null)
					{
						job.InitializeParentFromGenericJobWithoutSettingDefaults();
					}
					result.Add(job.PK);
				}

				if (includeRelatedJobs && result.Count > 0)
				{
					ZQuery childJobQuery = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, result.ToArray());
					if (restrictToThisCompany)
					{
						childJobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					}
					Job[] childJobs = Factory.Load<Job>(childJobQuery);
					foreach (Job job in childJobs)
					{
						if (job.Parent == null)
						{
							job.InitializeParentFromGenericJobWithoutSettingDefaults();
						}
						result.Add(job.PK);
					}
				}
			}

			return result.ToArray();
		}

		readonly BusinessObjectFactory Factory;
	}
}
