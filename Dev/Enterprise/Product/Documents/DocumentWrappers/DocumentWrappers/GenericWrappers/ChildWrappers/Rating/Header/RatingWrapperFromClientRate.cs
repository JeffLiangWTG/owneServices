using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingWrapperFromClientRate : RatingWrapper
	{
		public RatingWrapperFromClientRate(ClientRate header, BusinessObjectFactory factory)
			: base(header, factory)
		{
			this.header = header;
		}

		protected override ZString GetPrimarySource()
		{
			if (header.Header != null)
			{
				return Res.GetString("6d1666fd-27af-44bb-8eea-87e59eba3399", "{0} Client Rate", header.Header.OH_FullName);
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZString GetQuotationTitle()
		{
			return Res.GetString("c0b577d2-b180-4811-8f98-0a77785abde2", "Rate Update Notification");
		}

		protected override ZString GetCoverPageText()
		{
			return Env.Registry.Rating.GRIUpdateCoverPageText;
		}

		protected override ZString GetCoverPageFooterText()
		{
			return Env.Registry.Rating.GRIUpdateFooterPageText;
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

		readonly ClientRate header;

		#endregion
	}
}
