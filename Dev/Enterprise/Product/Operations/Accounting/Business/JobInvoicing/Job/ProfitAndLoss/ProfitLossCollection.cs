using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ProfitLossCollection : GlobalProfitLossCollection
	{
		public ProfitLossCollection(IJobProfitLoss profitLossParent, IJobHeaderParent parent)
			: base(profitLossParent, parent)
		{
		}

		public ProfitLossCollection(IJobProfitLoss profitLossParent, params ZGuid[] jobPKs)
			: base(profitLossParent, jobPKs)
		{
		}

		public override ZGuid[] JobPKs
		{
			get
			{
				ZGuid[] result = null;

				if (ManualJobPKs != null && ManualJobPKs.Length > 0)
				{
					return ManualJobPKs;
				}
				else if (Plugin != null)
				{
					result = new JobInvoicingDataAccessor(Factory).GetJobsFromShipment(Plugin.CostSupporter.ShipmentsList);
				}

				return result;
			}
		}

		#region Implementation

		protected override ZGuid CompanyFilter
		{
			get { return GlbCompany.CurrentCompany.PK; }
		}

		protected override string CompanyTotalsSQL
		{
			get { return String.Empty; }
		}

		protected override bool HasRecognizedChargesFilter
		{
			get { return true; }
		}

		protected override string CompanyTotalsFieldListSQL
		{
			get
			{
				return (NoResString)@"0 AS TotalRevenue,
						0 AS TotalWIP,
						0 AS TotalCost,
						0 AS TotalAccrual,
						0 AS TotalLineAmount,
						0 AS TotalRevenueRecognized,
						0 AS TotalWIPRecognized,
						0 AS TotalCostRecognized,
						0 AS TotalAccrualRecognized,
						0 AS TotalRevenueNotRecognized,
						0 AS TotalWIPNotRecognized, 
						0 AS TotalCostNotRecognized,
						0 AS TotalAccrualNotRecognized";
			}
		}
		#endregion
	}
}
