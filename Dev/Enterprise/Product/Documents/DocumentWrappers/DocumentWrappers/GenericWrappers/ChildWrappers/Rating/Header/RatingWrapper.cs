using System;
using System.Drawing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Rating Information")]
	public abstract class RatingWrapper : GenericWrapper
	{
		#region New

		public static RatingWrapper New(RatingHeader ratingheader, BusinessObjectFactory factory)
		{
			Costing costing = ratingheader as Costing;
			if (costing != null)
			{
				return new RatingWrapperFromCosting(costing, factory);
			}

			ClientRate clientRate = ratingheader as ClientRate;
			if (clientRate != null)
			{
				return new RatingWrapperFromClientRate(clientRate, factory);
			}

			CompanyTariff tariff = ratingheader as CompanyTariff;
			if (tariff != null)
			{
				return new RatingWrapperFromTariff(tariff, factory);
			}

			Quote quote = ratingheader as Quote;
			if (quote != null)
			{
				return new RatingWrapperFromQuotation(quote, factory);
			}

			return null;
		}

		public static RatingWrapper New(JobHeader jobheader, BusinessObjectFactory factory)
		{
			return jobheader != null ? new RatingWrapperFromJobHeader(jobheader, factory) : null;
		}

		#endregion

		#region Constructor

		protected RatingWrapper(RatingHeader ratingheader, BusinessObjectFactory factory) : base(ratingheader, factory) { }

		protected RatingWrapper(JobHeader jobHeader, BusinessObjectFactory factory) : base(jobHeader, factory) { }

		#endregion

		public ZString PrimarySource
		{
			get { return GetPrimarySource(); }
		}
		protected abstract ZString GetPrimarySource();

		public ZString QuotationTitle
		{
			get { return GetQuotationTitle(); }
		}
		protected abstract ZString GetQuotationTitle();

		public ZString CoverPageText
		{
			get { return GetCoverPageText(); }
		}
		protected abstract ZString GetCoverPageText();

		public ZString QuotationAcceptText
		{
			get { return GetQuotationAcceptText(); }
		}

		protected abstract ZString GetQuotationAcceptText();

		public ZString QuotationAcceptTooltip
		{
			get { return GetQuotationAcceptTooltip(); }
		}

		protected abstract ZString GetQuotationAcceptTooltip();

		public ZString CoverPageFooterText
		{
			get { return GetCoverPageFooterText(); }
		}
		protected abstract ZString GetCoverPageFooterText();

		public ZString InvoiceTermsText
		{
			get
			{
				var result = ZString.Empty;

				if (Header != null)
				{
					#region CodeStringFinder Hint
#if DEBUG
					new InvoiceTermsListWithShortDescription();
					new ARInvoiceTermsList();
#endif
					#endregion

					var client = Header.Header;
					var clientCompanyData = client?.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
					if (clientCompanyData != null)
					{
						var invoiceTermText = clientCompanyData.GetOrgARTermsAsLongText(x => !x.IsDisbursementTerm || x.IsDefaultTerm);
						var disbursementInvoiceTermText = clientCompanyData.GetOrgARTermsAsLongText(x => x.IsDisbursementTerm);
						if (string.IsNullOrEmpty(disbursementInvoiceTermText))
						{
							disbursementInvoiceTermText = clientCompanyData.GetOrgARTermsAsLongText(x => x.IsDefaultTerm);
						}

						if (!string.IsNullOrWhiteSpace(invoiceTermText) || !string.IsNullOrWhiteSpace(disbursementInvoiceTermText))
						{
							var builder = new ZStringBuilder();
							builder.Append(Res.GetString("75703789-e4b4-4b7b-b0c3-17179314d31f", "Please be advised that our payment terms are as follows:"));
							builder.AppendIfNotEmpty(invoiceTermText);
							if (!string.IsNullOrWhiteSpace(disbursementInvoiceTermText))
							{
								builder.Append(Res.GetString("a8b8d9d9-67c4-4e3b-b950-b5ba160d2f4c", "Disbursement item(s): "));
								builder.Append(disbursementInvoiceTermText);
							}

							result = builder.ToStringWithNewLineBetweenAppends();
						}
					}
				}

				return result;
			}
		}

		public ZDateTime ValidFrom
		{
			get { return GetValidFrom(); }
		}
		protected abstract ZDateTime GetValidFrom();

		public ZDateTime ValidUntil
		{
			get { return GetValidUntil(); }
		}
		protected abstract ZDateTime GetValidUntil();

		public ZBool IsReprint
		{
			get { return GetIsReprint(); }
		}
		protected abstract ZBool GetIsReprint();

		public StaffWrapper SecondSignatory
		{
			get { return secondSignatory ?? (secondSignatory = GetSecondSignatory()); }
		}
		protected abstract StaffWrapper GetSecondSignatory();
		StaffWrapper secondSignatory;

		public Image Logo
		{
			get
			{
				if (logo == null || logo.IsDisposed())
				{
					logo = DocumentBrandingImage;

					if (logo == null || logo.IsDisposed())
					{
						if (Env.Instance.IsWeb)
						{
							logo = GetWebLogo();
						}
						else
						{
							logo = Env.Registry.QuotationDocumentLogo;
						}
					}

					if (logo == null || logo.IsDisposed())
					{
						logo = SystemDataRegistry.Instance.CompanyLogo.Value;
					}
				}

				return logo;
			}
		}
		Image logo;

		public virtual RatingHeader RatingHeader
		{
			get { return Header; }
		}

		public RatingOneOffShipmentWrapper OneOffShipment
		{
			get { return oneOffShipment ?? (oneOffShipment = GetOneOffShipment()); }
		}
		protected abstract RatingOneOffShipmentWrapper GetOneOffShipment();
		RatingOneOffShipmentWrapper oneOffShipment;

		public ImageWrapperCollection TrailingPages
		{
			get { return trailingPages ?? (trailingPages = GetTrailingPages()); }
		}
		protected abstract ImageWrapperCollection GetTrailingPages();
		ImageWrapperCollection trailingPages;

		public OrganisationWrapperCollection PublishedAirFreightAgents
		{
			get { return publishedAirFreightAgents ?? (publishedAirFreightAgents = GetPublishedAirFreightAgents()); }
		}
		protected abstract OrganisationWrapperCollection GetPublishedAirFreightAgents();
		OrganisationWrapperCollection publishedAirFreightAgents;

		public OrganisationWrapperCollection PublishedSeaFreightAgents
		{
			get { return publishedSeaFreightAgents ?? (publishedSeaFreightAgents = GetPublishedSeaFreightAgents()); }
		}
		protected abstract OrganisationWrapperCollection GetPublishedSeaFreightAgents();
		OrganisationWrapperCollection publishedSeaFreightAgents;

		public PricingPageSetWrapperCollection PageSets
		{
			get { return pageSets ?? (pageSets = GetPageSets()); }
		}
		PricingPageSetWrapperCollection pageSets;

		protected virtual PricingPageSetWrapperCollection GetPageSets()
		{
			return new PricingPageSetWrapperCollection(Header, MenuTitle, Factory);
		}

		#region Implementation

		RatingHeader Header
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return WrappedObject as RatingHeader; }
		}

		protected virtual GlbCompany Company
		{
			get { return Header.Company; }
		}

		protected virtual OrgHeader Client
		{
			get { return Header.Header; }
		}

		Image GetWebLogo()
		{
			OrgHeader client = Client;
			GlbCompany company = Company;
			Guid companyPK = company != null ? company.PK.ToGuid() : Guid.Empty;

			OrgCompanyData companyData;
			Guid branchPK;

			if (client == null || (companyData = client.GetCompanyDataForGlbCompany(company)) == null)
			{
				branchPK = Guid.Empty;
			}
			else if (companyData.ControllingBranch != null && companyPK == companyData.ControllingBranch.GB_GC)
			{
				branchPK = companyData.ControllingBranch.PK.ToGuid();
			}
			else
			{
				branchPK = Guid.Empty;
			}

			return Env.Registry.GetQuotationDocumentLogo(companyPK, branchPK);
		}

		#endregion
	}
}
