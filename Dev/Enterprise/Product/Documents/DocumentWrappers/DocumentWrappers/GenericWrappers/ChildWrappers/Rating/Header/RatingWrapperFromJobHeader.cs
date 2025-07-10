using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingWrapperFromJobHeader : RatingWrapper
	{
		public RatingWrapperFromJobHeader(JobHeader header, BusinessObjectFactory factory) : base(header, factory)
		{
		}

		#region JobHeader

		JobHeader JobHeader
		{
			get { return (JobHeader)WrappedObject; }
		}

		#endregion

		#region Overrides

		protected override ZDateTime GetValidFrom()
		{
			return JobHeader.JH_A_JOP;
		}

		protected override ZDateTime GetValidUntil()
		{
			return (JobHeader.JH_A_JCL != ZDateTime.Empty) ? JobHeader.JH_A_JCL : DefaultQuoteEndDate;
		}

		ZDateTime DefaultQuoteEndDate
		{
			get { return GetDefaultEndDate(Env.Registry.Rating.QuoteValidityPeriod.Value); }
		}

		ZDateTime GetDefaultEndDate(int validityPeriod)
		{
			var result = ZDateTime.Empty;
			if (validityPeriod != 0)        // 0 means "no end date"
			{
				var startDate = JobHeader.JH_A_JOP;
				if (!startDate.IsValid || startDate.IsEmpty)
				{
					startDate = ZDateTime.Today;
				}

				if (validityPeriod < 0)     // negative value means "End of nth Month"
				{
					var endDate = startDate.AddMonths(-validityPeriod + 1);
					endDate = new ZDateTime(endDate.Year, endDate.Month, 1).AddDays(-1);
					result = endDate;
				}
				else
				{
					result = startDate.AddMonths(validityPeriod);
				}
			}

			return result;
		}

		protected override GlbCompany Company
		{
			get { return JobHeader.Company; }
		}

		protected override OrgHeader Client
		{
			get { return JobHeader.LocalCharges; }
		}

		#endregion

		#region Empty

		protected override ZString GetPrimarySource()
		{
			return ZString.Empty;
		}

		protected override ZString GetQuotationTitle()
		{
			return ZString.Empty;
		}

		protected override ZString GetCoverPageText()
		{
			return ZString.Empty;
		}

		protected override ZString GetCoverPageFooterText()
		{
			return ZString.Empty;
		}

		protected override ZBool GetIsReprint()
		{
			return false;
		}

		protected override StaffWrapper GetSecondSignatory()
		{
			return null;
		}

		protected override RatingOneOffShipmentWrapper GetOneOffShipment()
		{
			return null;
		}

		protected override ImageWrapperCollection GetTrailingPages()
		{
			return null;
		}

		protected override OrganisationWrapperCollection GetPublishedAirFreightAgents()
		{
			return null;
		}

		protected override OrganisationWrapperCollection GetPublishedSeaFreightAgents()
		{
			return null;
		}

		protected override PricingPageSetWrapperCollection GetPageSets()
		{
			return null;
		}

		protected override ZString GetQuotationAcceptText()
		{
			return ZString.Empty;
		}

		protected override ZString GetQuotationAcceptTooltip()
		{
			return ZString.Empty;
		}

		#endregion
	}
}
