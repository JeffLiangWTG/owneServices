using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromRatingHeader : FreightWrapper
	{
		public FreightWrapperFromRatingHeader(RatingHeader header, BusinessObjectFactory factory)
			: base(header, factory)
		{
			this.header = header;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("8dfc2c4c-e7ed-4c93-958d-76203317e1c0", "Quote No");
		}

		protected override ZString GetJobNumber()
		{
			return header.TH_QuoteNumber.TrimStart('0');
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return header.PK;
		}

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			if (header is Quote)
			{
				var salesAddress = header.Header?.Addresses.DefaultAddressOfType(OrgAddressType.Sales);
				if (salesAddress != null)
				{
					return new OrganisationWrapper(OrganisationUsageType.LocalClient, salesAddress, ContactType.LocalClient, Factory);
				}
			}

			return new OrganisationWrapper(OrganisationUsageType.LocalClient, header.Header, ContactType.LocalClient, Factory);
		}

		protected override RatingWrapper GetRating()
		{
			return RatingWrapper.New(header, Factory);
		}

		protected override StaffWrapper GetSalesRep()
		{
			OrgHeader client;
			GlbStaff staff;

			if (header.IsQuote())
			{
				staff = header.FirstSignatory;
				return staff == null ? null : new StaffWrapper(staff, Factory);
			}
			else if ((client = header.Header) != null)
			{
				staff = client.StaffAssignments.OverallSalesRepStaff;
				return staff == null ? null : new StaffWrapper(staff, Factory);
			}
			else
			{
				return null;
			}
		}

		#region Implementation

		readonly RatingHeader header;

		#endregion
	}
}
