using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingWrapperFromTariff : RatingWrapper
	{
		public RatingWrapperFromTariff(CompanyTariff header, BusinessObjectFactory factory)
			: base(header, factory)
		{
			this.header = header;
		}

		protected override ZString GetPrimarySource()
		{
			return Res.GetString("913ed270-5f57-40fd-b6c4-c14d31a97a52", "Level {0} Company Tariff - {1}", header.TH_GlobalRateLevel, header.TH_GlobalRateDescriptionMultilingual);
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

		protected override ZDateTime GetValidFrom()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetValidUntil()
		{
			return ZDateTime.Empty;
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
			return new ImageWrapperCollection(Factory);
		}

		protected override OrganisationWrapperCollection GetPublishedAirFreightAgents()
		{
			return new OrganisationWrapperCollection(Factory);
		}

		protected override OrganisationWrapperCollection GetPublishedSeaFreightAgents()
		{
			return new OrganisationWrapperCollection(Factory);
		}

		protected override ZString GetQuotationAcceptText()
		{
			return ZString.Empty;
		}

		protected override ZString GetQuotationAcceptTooltip()
		{
			return ZString.Empty;
		}

		#region Implementation

		readonly CompanyTariff header;

		#endregion
	}
}
