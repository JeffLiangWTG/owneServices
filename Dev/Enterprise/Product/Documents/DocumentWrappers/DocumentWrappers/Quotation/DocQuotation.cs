using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Quotation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public abstract class DocQuotation : DocBaseWrapper, IDocTypeCode
	{
		protected DocQuotation(PricingPage page, BusinessObjectFactory factoryToWrap)
			: base(page, factoryToWrap)
		{
			DocTypeCode = DocStorageTypeCode;
			if (page != null)
			{
				FirstEntry = page.RateEntries.FirstOrDefault();
			}
		}

		public static DocQuotation New(PricingPage page, BusinessObjectFactory factoryToWrap)
		{
			if (page == null)
			{
				return null;
			}

			switch (page.Style)
			{
				case PricingPageStyle.Standard:
					return DocEntryQuotation.New(page, factoryToWrap);
				case PricingPageStyle.Landscape:
					return DocTableQuotation.New(page, factoryToWrap);

				default:
					return null;
			}
		}

		public override string ToString()
		{
			return QuoteNumberAndClientFullName;
		}

		public RatingHeader GetParentHeader()
		{
			return Header;
		}

		public ZBool IsQuote
		{
			get { return Header != null && Header.IsQuote(); }
		}

		public DocOrganisation Client
		{
			get
			{
				if (Quote != null && Quote.QuotationClientAddress != null && Quote.QuotationClientAddress.IsValidAddress)
				{
					return DocOrganisation.New(Quote.QuotationClientAddress, Factory);
				}

				if (Header.TH_OH.IsValid)
				{
					return DocOrganisation.New(Header.Header, Factory);
				}

				return null;
			}
		}

		public ZString ClientName
		{
			get { return Client != null ? Client.Name : ZString.Empty; }
		}

		public ZString LocalPort
		{
			get { return FirstEntry.LocalPort == null ? ZString.Empty : FirstEntry.LocalPort.Description; }
		}

		public ZString QuoteNumber
		{
			get { return Quote != null ? Quote.TH_QuoteNumber.TrimStart('0') : ZString.Empty; }
		}

		public ZString QuoteNumberAndClientCode
		{
			get { return Quote != null && Quote.Header != null ? (ZString)(Quote.TH_QuoteNumber.TrimStart('0') + " - " + Quote.Header.OH_Code) : ZString.Empty; }
		}

		public ZString QuoteNumberAndClientFullName
		{
			get { return Quote != null ? (ZString)(Quote.TH_QuoteNumber.TrimStart('0') + " - " + ClientName) : ZString.Empty; }
		}

		public ZString ClientFullNameUpper
		{
			get { return Quote != null ? ClientName.ToUpper() : ZString.Empty; }
		}

		public ZString ValidFrom
		{
			get { return Header != null ? (ZString)Header.TH_QuoteDate.ToString("d MMM yyyy") : ZString.Empty; }
		}

		public ZString ValidUntil
		{
			get { return Header != null ? (ZString)Header.TH_QuoteEndDate.ToString("d MMM yyyy") : ZString.Empty; }
		}

		public ZString Date
		{
			get { return ZDateTime.Today.ToString("d MMM yyyy"); }
		}

		public ZString CommodityCode
		{
			get { return FirstEntry.CommodityCode != null ? FirstEntry.CommodityCode.RH_DescriptionMultilingual : ZString.Empty; }
		}

		public ZString CommodityCodeCode
		{
			get { return FirstEntry.CommodityCode != null ? FirstEntry.CommodityCode.RH_Code : ZString.Empty; }
		}

		public ZString ServiceLevel
		{
			get
			{
				if (FirstEntry != null)
				{
					if (FirstEntry.IsCosting())
					{
						return FirstEntry.CarrierServiceLevel != null ? FirstEntry.CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual : ZString.Empty;
					}

					return FirstEntry.ServiceLevel_NI != null ? FirstEntry.ServiceLevel_NI.RS_DescriptionMultilingual : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		public ZString ServiceLevelCode
		{
			get
			{
				if (FirstEntry != null)
				{
					return FirstEntry.IsCosting() ? FirstEntry.TI_PL_NKCarrierServiceLevel : FirstEntry.TI_RS_NKServiceLevel_NI;
				}

				return ZString.Empty;
			}
		}

		public ZBool IsDoorToDoor
		{
			get { return FirstEntry.ServiceLevel_NI != null ? FirstEntry.ServiceLevel_NI.RS_IsDoorToDoor : ZBool.False; }
		}

		public ZString Mode
		{
			get { return FirstEntry.FreightType; }
		}

		public ZBool IsCFSRate
		{
			get { return FirstEntry != null && FirstEntry.IsCFS(); }
		}

		public ZBool IsImport
		{
			get { return FirstEntry != null && FirstEntry.IsImport(); }
		}

		public ZBool IsExport
		{
			get { return FirstEntry != null && FirstEntry.IsExport(); }
		}

		public ZBool IsCrossTrade
		{
			get { return FirstEntry != null && (FirstEntry.IsCrossTrade() || FirstEntry.TI_IsCrossTrade); }
		}

		public ZBool IsDomestic
		{
			get { return FirstEntry != null && FirstEntry.IsDomestic(); }
		}

		public ZBool IsSupplementary
		{
			get { return FirstEntry != null && FirstEntry.IsSupplementaryEntry(); }
		}

		public ZString InvoiceTermsText
		{
			get
			{
				var result = string.Empty;
				var client = Header.Header;
				var clientCompanyData = client?.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
				if (clientCompanyData != null)
				{
					string invoiceTermText = clientCompanyData.GetOrgARTermsAsLongText(x => !x.IsDisbursementTerm || x.IsDefaultTerm);
					string disbursementInvoiceTermText = clientCompanyData.GetOrgARTermsAsLongText(x => x.IsDisbursementTerm || x.IsDefaultTerm);

					if (!string.IsNullOrWhiteSpace(invoiceTermText) || !string.IsNullOrWhiteSpace(disbursementInvoiceTermText))
					{
						ZStringBuilder builder = new ZStringBuilder();
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

				return result;
			}
		}

		public ZString QuotationTitle
		{
			get
			{
				ZString result = "";

				if (Header != null)
				{
					if (Header.IsQuote())
					{
						result = Quote.TH_OneTimeQuote ? Env.Registry.Rating.OneOffQuoteTitleText : Env.Registry.Rating.QuoteTitleText;
					}
					else if (Header.IsClientRate())
					{
						result = Res.GetString("c05b0d2f-87ff-420b-b9b0-b3ef90e4b9f2", "Rate Update Notification");
					}
				}

				return result;
			}
		}

		public ZString QuotationTitleUpper
		{
			get { return QuotationTitle.ToUpper(); }
		}

		public ZString PageOpeningText
		{
			get
			{
				ZString textOnQuote = FirstEntry.TI_PageOpeningText;
				ZString result = textOnQuote.IsEmpty ? DocumentsDataRegistry.Instance.QuoteOpeningText.Value : textOnQuote;

				return result.Replace("\r", "");
			}
		}

		public ZString PageClosingText
		{
			get
			{
				ZString textOnQuote = FirstEntry.TI_PageClosingText;
				ZString result = textOnQuote.IsEmpty ? DocumentsDataRegistry.Instance.QuoteClosingText.Value : textOnQuote;

				if (IsGSTApplicable)
				{
					if (!result.IsEmpty)
					{
						result += "\n\n";
					}

					var taxDescription = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
					var taxMessage = IsFreightGSTApplicable
						? Res.GetString("27eff35b-eccd-47bd-9b99-0ffc12cd3b4d", "A local Value Added Tax charge (equivalent to {0}) may apply to freight and all items marked with an asterisk (*).", taxDescription)
						: Res.GetString("d84035fb-16d9-4403-a76c-716f82c7aaad", "A local Value Added Tax charge (equivalent to {0}) may apply to all items marked with an asterisk (*).", taxDescription);

					result += taxMessage;
				}

				return result.Replace("\r", "");
			}
		}

		public DocCFXCollection CFX
		{
			get
			{
				DocCFXCollection cfxCollection = new DocCFXCollection(Factory);

				if (Header != null && Env.Registry.Rating.IncludeCFXOnQuote)
				{
					if (IsImport)
					{
						if (FirstEntry.IsAir())
						{
							cfxCollection.Add(new DocCFX(Res.GetString("1930563b-bad0-412e-a716-e3cf708a9dc3", "Import - Air"), Header.TH_AirCFX));
						}

						if (FirstEntry.IsSea())
						{
							cfxCollection.Add(new DocCFX(Res.GetString("ebebdf93-04a5-4142-bd90-ee6575c14827", "Import - Sea"), Header.TH_SeaCFX));
						}
					}

					if (IsExport)
					{
						if (FirstEntry.IsAir())
						{
							cfxCollection.Add(new DocCFX(Res.GetString("8125beab-e7c6-43d8-a785-6edaccd22ce1", "Export - Air"), Header.TH_ExportAirCFX));
						}

						if (FirstEntry.IsSea())
						{
							cfxCollection.Add(new DocCFX(Res.GetString("80698416-6103-456a-8d15-31e9c9f2a9bc", "Export - Sea"), Header.TH_ExportSeaCFX));
						}
					}
				}

				return cfxCollection;
			}
		}

		public ZString DraftStatus
		{
			get { return Quote != null && Quote.DocumentPrintMode == QuotationDocumentMode.Draft ? (ZString)"DRAFT" : ZString.Empty; }
		}

		public ZString DocumentReprintStatus
		{
			get { return Quote != null && Quote.DocumentPrintMode == QuotationDocumentMode.Reprint ? (ZString)"REPRINT" : ZString.Empty; }
		}

		public DocIndexEntryCollection IndexEntries
		{
			get { return Quote == null ? new DocIndexEntryCollection(Factory) : GetIndexEntries(); }
		}

		DocIndexEntryCollection GetIndexEntries()
		{
			if (Quote == null)
			{
				return null;
			}

			var collection = new PricingPageCollection(Quote);
			var shouldLoadLandscapePricingPage = Quote
				.SelectedPages
				.Cast<RateAttachment>()
				.Any(p => p.CurrentAttachment.TS_TemplateType == RatingConstants.DocTemplateTypes.TableFormatPricingPage);

			var strategy = shouldLoadLandscapePricingPage
				? PricingPaginationStrategy.LandscapeComplexStyle
				: PricingPaginationStrategy.StandardStyle;

			collection.Load(strategy);

			return new DocIndexEntryCollection(collection, Factory);
		}

		public DocQuotationLineCollection LocalDocRateLineItems
		{
			get { return IsImport ? DestinationDocRateLineItems : OriginDocRateLineItems; }
		}

		public DocQuotationLineCollection OverseasDocRateLineItems
		{
			get { return IsImport ? OriginDocRateLineItems : DestinationDocRateLineItems; }
		}

		public DocQuotationLineCollection OriginDocRateLineItems
		{
			get
			{
				if (originDocRateLineItems == null)
				{
					originDocRateLineItems = new DocQuotationLineCollection(this, new PricingPageRateLineFactory(EntryTypes.Origin), Page.RateEntries, Factory);

					if (FirstEntry != null && Header != null)
					{
						originDocRateLineItems.Load();
					}
				}

				return originDocRateLineItems;
			}
		}
		DocQuotationLineCollection originDocRateLineItems;

		public DocQuotationLineCollection DestinationDocRateLineItems
		{
			get
			{
				if (destinationDocRateLineItems == null)
				{
					destinationDocRateLineItems = new DocQuotationLineCollection(this, new PricingPageRateLineFactory(EntryTypes.Destination), Page.RateEntries, Factory);

					if (FirstEntry != null && Header != null)
					{
						destinationDocRateLineItems.Load();
					}
				}

				return destinationDocRateLineItems;
			}
		}
		DocQuotationLineCollection destinationDocRateLineItems;

		public Image Logo
		{
			get
			{
				Image result = DocumentBrandingImage;

				if (result == null)
				{
					if (Env.Instance.IsWeb)
					{
						var branchForLogo = GetWebBranchForLogo();
						result = Env.Registry.GetQuotationDocumentLogo(branchForLogo.Company.PK.ToGuid(), branchForLogo.PK.ToGuid());
					}
					else
					{
						result = Env.Registry.QuotationDocumentLogo;
					}
				}

				if (result == null)
				{
					result = SystemDataRegistry.Instance.CompanyLogo.Value;
				}

				return result;
			}
		}

		public DocStaff SalesRep
		{
			get
			{
				if (IsQuote && Header != null && Header.FirstSignatory != null)
				{
					return DocStaff.New(Header.FirstSignatory, Factory);
				}

				if (StaffAssignments.OverallSalesRepStaff != null)
				{
					var docStaff = DocStaff.New(StaffAssignments.OverallSalesRepStaff, Factory);
					docStaff.CustomRelationshipToOrganisation = CommonResourceStrings.OverallRepresentative;

					return docStaff;
				}

				return null;
			}
		}

		#region IDocTypeCode

		ZString DocTypeCode;

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}

		#endregion

		#region Implementation

		const string DocStorageTypeCode = "QUO";

		protected RatingHeader Header { get; private set; }
		protected RateEntry FirstEntry
		{
			get { return firstEntry; }
			private set
			{
				firstEntry = value;
				if (firstEntry != null)
				{
					Header = FirstEntry.Parent;
				}
			}
		}
		RateEntry firstEntry;

		Quote Quote
		{
			get { return quote ?? (quote = Header as Quote); }
		}
		Quote quote;

		internal List<RefContainer> ContainerList => containerList ?? (containerList = Page.ContainerSet.ToList());
		List<RefContainer> containerList;

		protected abstract bool IsGSTApplicable
		{
			get;
		}

		protected virtual bool IsFreightGSTApplicable
		{
			get { return false; }
		}

		protected override ClientTariffAndLevel TariffAndLevelRegistry
		{
			get
			{
				if (Header != null && Header.Header != null && (Header.IsQuote() || Header.IsClientRate()))
				{
					var tariffLevelCode = Header.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).RateTariffLevels.DefaultLevel.ToString(Culture.Invariant);

					return (ClientTariffAndLevel)DocumentsDataRegistry.Instance.ClientTariffAndLevels.Value.FindByCode(tariffLevelCode);
				}

				return base.TariffAndLevelRegistry;
			}
		}

		public PricingPage Page
		{
			get { return (PricingPage)WrappedObject; }
		}

		OrgStaffAssignmentsCollection StaffAssignments
		{
			get
			{
				if (staffAssignments == null && Header != null && Header.Header != null)
				{
					staffAssignments = Header.HeaderStaffAssignments;
				}

				return staffAssignments;
			}
		}
		OrgStaffAssignmentsCollection staffAssignments;

		GlbBranch GetWebBranchForLogo()
		{
			OrgCompanyData companyData = Header.Header.GetCompanyDataForGlbCompany(Header.Company);
			GlbBranch branchForLogo;

			if (companyData.ControllingBranch != null && Header.Company.Branches.Contains(companyData.ControllingBranch))
			{
				branchForLogo = companyData.ControllingBranch;
			}
			else if (GlbBranch.CurrentBranch != null && Header.Company.Branches.Contains(GlbBranch.CurrentBranch))
			{
				branchForLogo = GlbBranch.CurrentBranch;
			}
			else
			{
				branchForLogo = Header.Company.Branches[0];
			}

			return branchForLogo;
		}

		#endregion
	}
}
