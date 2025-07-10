using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobCostingPlugInDataRetriever
	{
		public JobCostingPlugInDataRetriever(IJobCostingPlugIn hostPlugIn, BusinessObjectFactory factory)
		{
			this.HostPlugIn = hostPlugIn;
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;
		readonly IJobCostingPlugIn HostPlugIn;

		public ApportionmentListing GetApportionments()
		{
			return new ApportionmentListing(Factory, HostPlugIn);
		}

		public IEnumerable<Job> Jobs
		{
			get
			{
				if (fJobs == null)
				{
					fJobs = Factory.Load<Job>(new JobCollection(Factory, JobsQuery).CompleteFilter);
				}

				return fJobs;
			}
		}
		IEnumerable<Job> fJobs;

		#region Jobs Query

		ZQuery JobsQuery
		{
			get
			{
				ZQuery query = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(JobHeaderSchema.JH_ParentID, ShipmentPKs);
				return query;
			}
		}

		List<ZGuid> ShipmentPKs
		{
			get
			{
				List<ZGuid> pKs = new List<ZGuid>();

				foreach (IJobInvoicingPlugIn shipment in HostPlugIn.CostSupporter.ShipmentsList)
				{
					pKs.Add(shipment.PK);
				}

				return pKs;
			}
		}

		#endregion
	}
}

