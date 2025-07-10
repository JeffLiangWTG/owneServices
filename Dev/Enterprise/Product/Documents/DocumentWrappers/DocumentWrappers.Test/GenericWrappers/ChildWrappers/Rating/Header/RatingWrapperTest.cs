using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	abstract class RatingWrapperTest<RatingT, WrapperT> : GenericWrapperTest
					where RatingT : BusinessObject
					where WrapperT : RatingWrapper
	{
		#region ExpectedFieldMap

		protected sealed override string ExpectedFieldMap
		{
			get
			{
				return @"
Rating Information
======================================================================
Name                                    Type
----------------------------------------------------------------------
OneOffShipment                          RatingOneOffShipment
SecondSignatory                         StaffMember
CoverPageFooterText                     String
CoverPageText                           String
InvoiceTermsText                        String
IsReprint                               Bool
PrimarySource                           String
QuotationAcceptText                     String
QuotationAcceptTooltip                  String
QuotationTitle                          String
ValidFrom                               DateTime
ValidUntil                              DateTime

TrailingPages                           Image Collection
PublishedAirFreightAgents               Organisation Collection
PublishedSeaFreightAgents               Organisation Collection
PageSets                                Pricing Page Set Collection
";
			}
		}

		#endregion

		#region TestLogo

		public void TestLogo()
		{
			var wrapperNoLogo = GetNewRatingWrapper();
			AssertNull(wrapperNoLogo.Logo);

			var wrapperCompanyLogo = GetNewRatingWrapper();
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(10, 10));
			AssertEquals(10, wrapperCompanyLogo.Logo.Size.Height);

			var wrapperQuotationDocumentLogo = GetNewRatingWrapper();
			Env.Registry.QuotationDocumentLogo = new Bitmap(20, 20);
			AssertEquals(20, wrapperQuotationDocumentLogo.Logo.Size.Height);

			Globals.IsWeb = true;
			DataRegistry.Instance.QuotationDocumentLogo = new Bitmap(30, 30);
			var wrapperWebLogo = GetNewRatingWrapper();
			AssertEquals(30, wrapperWebLogo.Logo.Size.Height);
			Globals.IsWeb = false; // reset env

			// DocumentBrandingImage
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(40, 40));
			BrandingTestHelperClass.SetClientBrandRegistryImage(); // logo has height of 1
			BrandingTestHelperClass.SetAgentBrandRegistryImage();
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var docWrapper = new DocImageSupportWrapperTests.DocBaseWrapperBaseWithImageSupportTestClass(dummy, Factory);
			var wrapper = GetNewRatingWrapper();
			AssertEquals("Setting Company Logo, doesn't affect Rating Logo", 30, wrapper.Logo.Size.Height);

			var contactOrg = Factory.NewWithValidTestData<OrgHeader>();
			var brandedOrg = Factory.NewWithValidTestData<OrgHeader>();
			contactOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
			docWrapper.SetTemplateConstants(constants);
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			var wrapperWithClientBranding = GetNewRatingWrapper();
			AssertEquals(1, wrapperWithClientBranding.Logo.Size.Height);
		}

		#endregion

		#region TestNew

		public void TestNew()
		{
			var wrapper = GetNewRatingWrapper();

			AssertType(typeof(WrapperT), wrapper);
			AssertSame(Header, wrapper.WrappedObject);
		}

		#endregion

		#region TestInvoiceTermsText

		public void TestInvoiceTermsText()
		{
			var expectedInvoiceTerms = new Dictionary<string, string>();
			expectedInvoiceTerms[ARInvoiceTermsList.CashOnDelivery.Code] = "Cash On Delivery";
			expectedInvoiceTerms[ARInvoiceTermsList.FromInvoiceDate.Code] = "0 Days From Date of Invoice";
			expectedInvoiceTerms[ARInvoiceTermsList.FromMonthEnd.Code] = "0 Days From End of Month";
			expectedInvoiceTerms[ARInvoiceTermsList.FromWeekEnd.Code] = "0 Days From End of Week";
			expectedInvoiceTerms[ARInvoiceTermsList.FromPeriodEnd.Code] = "0 Days From End of Period";
			expectedInvoiceTerms[ARInvoiceTermsList.FromShipmentDate.Code] = "0 Days From Date of Shipment";
			expectedInvoiceTerms[ARInvoiceTermsList.FromCustomsClearanceDate.Code] = "0 Days From Customs Clearance Date";
			expectedInvoiceTerms[ARInvoiceTermsList.PaymentInAdvance.Code] = "Payment in Advance";
			expectedInvoiceTerms[ARInvoiceTermsList.MonthsFromInvoiceCycleDate.Code] = "0 Months From Invoice Cycle Date";
			expectedInvoiceTerms[ARInvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code] = "0 Debtor Payment Cycle";
			expectedInvoiceTerms[ARInvoiceTermsList.LaterOfShipmentOrInvoiceDate.Code] = "0 Days Later of Shipment or Invoice Date";
			expectedInvoiceTerms[ARInvoiceTermsList.FromDeliveryOrPickupDate.Code] = "0 days from delivery or pickup date";

			foreach (CodeDescriptionPair term in new ARInvoiceTermsList())
			{
				string expectedText;
				Assert("Missing expected text for term " + term, expectedInvoiceTerms.TryGetValue(term.Code, out expectedText));
				AssertInvoiceTermsText(expectedText, term);
			}
		}

		protected virtual void AssertInvoiceTermsText(string expectedText, CodeDescriptionPair term)
		{
			var wrapper = GetNewRatingWrapper();
			var client = Factory.New<OrgHeader>();
			RatingHeader.TH_OH = client.PK;
			client.CompanyData.ARTerms[0].PY_InvoiceTerm = term.Code;
			client.CompanyData.ARTerms[0].PY_InvoiceDays = 0;
			RatingHeader.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).ARTerms[0].PY_InvoiceTerm = term.Code;
			AssertEquals("InvoiceTermsText for " + term, ExpectedInvoiceText(expectedText), wrapper.InvoiceTermsText);
		}

		public void TestInvoiceTermsText_MultipleInvoiceTerm()
		{
			AssertMultipleInvoiceTermsText(string.Format("Please be advised that our payment terms are as follows:\r\n1. {0}\r\nDisbursement item(s): \r\n1. {1}", "30 DAYS FROM DATE OF INVOICE", "For  Any Disbursement Type , invoice term is PAYMENT IN ADVANCE"));
		}

		protected virtual void AssertMultipleInvoiceTermsText(string expectedText)
		{
			var wrapper = GetNewRatingWrapper();
			var client = Factory.New<OrgHeader>();

			RatingHeader.TH_OH = client.PK;

			client.CompanyData.ARTerms[0].PY_InvoiceTerm = ARInvoiceTermsList.FromInvoiceDate.Code;
			client.CompanyData.ARTerms[0].PY_InvoiceDays = 30;
			client.CompanyData.ARTerms[0].PY_InvoiceClass = "ALL";

			client.CompanyData.ARTerms.AddNew();
			client.CompanyData.ARTerms[1].PY_InvoiceTerm = ARInvoiceTermsList.PaymentInAdvance.Code;
			client.CompanyData.ARTerms[1].PY_InvoiceDays = 1;
			client.CompanyData.ARTerms[1].PY_InvoiceClass = "DSB";
			RatingHeader.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).ARTerms[0].PY_InvoiceTerm = ARInvoiceTermsList.FromInvoiceDate.Code;
			AssertEquals("InvoiceTermsText for Multiple", expectedText , wrapper.InvoiceTermsText);
		}

		protected virtual ZString ExpectedInvoiceText(string expectedText)
		{
			return string.Format("Please be advised that our payment terms are as follows:\r\n1. {0}\r\nDisbursement item(s): \r\n1. {0}", expectedText.ToUpper());
		}

		#endregion

		#region Implementation

		#region Header

		protected RatingT Header
		{
			get { return header ?? (header = GetNewRatingHeader()); }
		}
		RatingT header;

		protected abstract RatingT GetNewRatingHeader();

		#endregion

		#region RatingHeader

		RatingHeader RatingHeader
		{
			get { return Header as RatingHeader; }
		}

		#endregion

		#region GetNewRatingWrapper

		protected abstract RatingWrapper GetNewRatingWrapper();

		#endregion

		#region GetNewDocumentWrapper

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return GetNewRatingWrapper();
		}

		#endregion

		#region GetSetupWrapperForDefaultFormatting

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			if (RatingHeader != null)
			{
				var secondSignatory = Factory.New<GlbStaff>();
				secondSignatory.GS_Code = "FRD";
				secondSignatory.GS_FullName = "Fread";
				RatingHeader.TH_GS_NKSecondSignatory = secondSignatory.GS_Code;
			}

			return GetNewRatingWrapper();
		}

		#endregion

		#endregion
	}
}
