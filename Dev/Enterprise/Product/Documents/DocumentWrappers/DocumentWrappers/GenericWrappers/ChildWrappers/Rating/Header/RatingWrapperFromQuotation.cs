using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingWrapperFromQuotation : RatingWrapper
	{
		public RatingWrapperFromQuotation(Quote header, BusinessObjectFactory factory)
			: base(header, factory)
		{
			this.header = header;
		}

		protected override ZString GetPrimarySource()
		{
			if (header.Header != null)
			{
				return Res.GetString("0a5110c4-4bc6-46b9-900f-6b6d45fac757", "Quotation for {0}", header.Header.OH_FullName);
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZString GetQuotationTitle()
		{
			return header.TH_OneTimeQuote ? Env.Registry.Rating.OneOffQuoteTitleText : Env.Registry.Rating.QuoteTitleText;
		}

		protected override ZString GetCoverPageText()
		{
			StmNote[] quoteCoverPageNotes = header.Notes.FindByDescription(PredefinedNoteTypes.Instance.QuoteCoverPageText.Description);
			ZString result;

			if (quoteCoverPageNotes != null && quoteCoverPageNotes.Length > 0)
			{
				result = quoteCoverPageNotes[0].ST_NoteDataAsText;
			}
			else
			{
				result = header.CoverPageText;
			}

			return result;
		}

		protected override ZString GetQuotationAcceptText()
		{
			return header.QuotationAcceptText;
		}

		protected override ZString GetQuotationAcceptTooltip()
		{
			return header.QuotationAcceptTooltip;
		}

		protected override ZString GetCoverPageFooterText()
		{
			return Env.Registry.Rating.QuoteCoverPageFooterText;
		}

		protected override ZBool GetIsReprint()
		{
			return header.DocumentPrintMode == QuotationDocumentMode.Reprint;
		}

		protected override ZDateTime GetValidFrom()
		{
			return header.TH_QuoteDate;
		}

		protected override ZDateTime GetValidUntil()
		{
			return header.TH_QuoteEndDate;
		}

		protected override StaffWrapper GetSecondSignatory()
		{
			GlbStaff staff;

			if ((staff = header.SecondSignatory) != null)
			{
				return new StaffWrapper(staff, Factory);
			}
			else
			{
				return null;
			}
		}

		protected override RatingOneOffShipmentWrapper GetOneOffShipment()
		{
			return header.TH_OneTimeQuote ? new RatingOneOffShipmentWrapper(header, Factory) : null;
		}

		protected override ImageWrapperCollection GetTrailingPages()
		{
			return new ImageWrapperCollection(header.TrailingPageImages, Factory);
		}

		protected override OrganisationWrapperCollection GetPublishedAirFreightAgents()
		{
			OrganisationWrapperCollection results = new OrganisationWrapperCollection(Factory);

			foreach (OrgAddress orgAddress in header.PublishedAirFreightAgents)
			{
				results.Add(new OrganisationWrapper(OrganisationUsageType.PublishedAirFreightAgent, orgAddress, ContactType.FreightAgent, Factory));
			}

			return results;
		}

		protected override OrganisationWrapperCollection GetPublishedSeaFreightAgents()
		{
			OrganisationWrapperCollection results = new OrganisationWrapperCollection(Factory);

			foreach (OrgAddress orgAddress in header.PublishedSeaFreightAgents)
			{
				results.Add(new OrganisationWrapper(OrganisationUsageType.PublishedSeaFreightAgent, orgAddress, ContactType.FreightAgent, Factory));
			}

			return results;
		}

		#region Implementation

		readonly Quote header;

		#endregion
	}
}
