using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingWrapperFromCosting : RatingWrapper
	{
		public RatingWrapperFromCosting(Costing header, BusinessObjectFactory factory)
			: base(header, factory)
		{
			this.header = header;
		}

		protected override ZString GetPrimarySource()
		{
			if (header.IsStandardCostRate())
			{
				return Res.GetString("96a3d354-4de4-403c-9d02-bd15006d4db4", "Standard Costing");
			}
			else if (header.Header != null)
			{
				return Res.GetString("31b9d211-8b14-4d50-8bf8-b9e9378004e1", "{0} Costing", header.Header.OH_FullName);
			}
			else
			{
				return ZString.Empty;
			}
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

		readonly Costing header;

		#endregion
	}
}
