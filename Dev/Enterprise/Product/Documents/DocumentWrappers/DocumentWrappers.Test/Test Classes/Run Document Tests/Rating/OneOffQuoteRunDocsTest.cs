using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class OneOffQuoteRunDocsTest : BaseRunDocumentsTest
	{
		public OneOffQuoteRunDocsTest() { }

		#region OneOffQuoteDetails & OneOffQuoteDetailsWithLocalCurrency DocStrip

		public void TestTemplate_DocStrip_OneOffQuoteDetails() => TestTemplate_DocStrip
		(
			sectionItemName: "One Off Quote Details",
			expectedOutput: @"{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details]

{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details (cont.)]

{C}-[<AutoHeight><Charges.Description>]   {AG}-[<Charges.OSSell.WithoutTax>]
{C}-[<HideRowIfCellIsEmpty><AutoHeight><Charges.CalculationDescription>]


{C}-[Sub-total (<Charges.OSSell.WithoutTax.Currency.Code>)]   {AE}-[<FormatNumber(<Total Charges.OSSell.WithoutTax.Amount>,<Charges.OSSell.WithoutTax.Currency.Code>)>]"
		);

		public void TestTemplate_DocStrip_OneOffQuoteDetailsWithLocalCurrency() => TestTemplate_DocStrip
		(
			sectionItemName: "One Off Quote Details with Local Currency",
			expectedOutput: @"{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details]
{AS}-[Quote Currency]

{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details (cont.)]
{AS}-[Quote Currency]

{C}-[<AutoHeight><Charges.Description><If(""<Charges.OSSell.Tax>"" != """",""*"","""")>]   {AE}-[<AutoHeight><Charges.OSSell.WithoutTax>]   {AM}-[<AutoHeight><If(""<RegistryItem(DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage)>""==""True""&&""<Charges.OSSell.WithoutTax.Currency.Code>"" != ""<Charges.LocalSell.WithoutTax.Currency.Code>"", ""@<Charges.SellRate>"","""")>]   {AS}-[<AutoHeight><If(""<RegistryItem(DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage)>""==""True"",""<Charges.LocalSell.WithoutTax>"","""")>]
{C}-[<HideRowIfCellIsEmpty><AutoHeight><Charges.CalculationDescription>]


{C}-[Sub-total (<Charges.OSSell.WithoutTax.Currency.Code>)]   {AE}-[<AutoHeight><FormatNumber(<Total Charges.OSSell.WithoutTax.Amount>,<Charges.OSSell.WithoutTax.Currency.Code>)>]





{C}-[TOTAL CHARGES:]   {U}-[<Charges.LocalSell.Currency.Code>]   {AE}-[<AutoHeight><FormatNumber(<Total Charges.LocalSell.WithoutTax.Amount>,<Charges.LocalSell.Currency.Code>)>]


{C}-[A <TaxCode> charge may apply to all items marked with an asterisk (*).]"
		);

		void TestTemplate_DocStrip(string sectionItemName, string expectedOutput)
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Test Menu");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var businessObject = (IDocumentSupportable)GetBusinessObject;
				CreateDocumentCommand(Factory, businessObject, sectionItemName);

				AssertRunDocument(businessObject, expectedOutput);
			}
		}

		static void CreateDocumentCommand(BusinessObjectFactory factory, IDocumentSupportable documentSupportable, string sectionItemName)
		{
			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = documentSupportable;
			documentCommand.SU_MenuName = "Test Menu";
			documentCommand.SU_BusinessContext = "Quotation";
			var templateUsed = documentCommand.Documents.AddNew();
			templateUsed.SI_SU = documentCommand.PK;
			var template = factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, "System Document Elements"));
			templateUsed.SI_SO = template.PK;
			var config = templateUsed.DocConfigs.AddNew();
			config.S3_SI = templateUsed.PK;

			var configItem1 = config.ConfigItems.AddNew();
			configItem1.S4_PrintOrder = 1;
			configItem1.S4_SectionItemName = sectionItemName;
			configItem1.S4_SectionType = "BEX";
		}

		public void TestDocument_DocStrip_OneOffQuoteDetails()
			=> TestDocument_OneOffQuoteDetailFilterStrip
			(
				sectionItemName: "One Off Quote Details",
				expectedOutput: @"{C}-[Quotation Details]
{C}-[Charge Code 1 Description]   {AG}-[1,234.57 USD]

{C}-[Sub-total (USD)]   {AE}-[1,234.57]"
			);

		public void TestDocument_DocStrip_OneOffQuoteDetailsWithLocalCurrency()
			=> TestDocument_OneOffQuoteDetailFilterStrip
			(
				sectionItemName: "One Off Quote Details with Local Currency",
				expectedOutput: @"{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Charge Code 1 Description]   {AE}-[1,234.57 USD]   {AM}-[@1.000000]   {AS}-[1,234.57 ERN]

{C}-[Sub-total (USD)]   {AE}-[1,234.57]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[1,234.57]"
			);

		void TestDocument_OneOffQuoteDetailFilterStrip(string sectionItemName, string expectedOutput)
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var chargeCode1 = chargeCodeFactory.New("CHG1", "Charge Code 1 Description", FlatCalculator.Code);
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			Factory.Save();

			oneOffQuote.TryLoadOrCreateJob();
			using (var job = (Job)oneOffQuote.Job)
			{
				var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");

				testObjectCreator.CreateCharge(job, chargeCode1, sellCurrency: usdCurrency, osSellAmt: 1234.5678m, creditor: null);

				RunDocumentWithAllSections = ZBool.False;

				CreateDocumentCommand(Factory, oneOffQuote, sectionItemName);

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Test Menu");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(oneOffQuote, expectedOutput);
				}
			}
		}

		#endregion

		[TestDate(2020, 1, 1)]
		public void TestDocument_GivenCoverPage_ThenShouldNotShowQuotationClientReplyLinks()
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var chargeCode1 = chargeCodeFactory.New("CHG1", "Charge Code 1 Description", FlatCalculator.Code);

			var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			Factory.Save();

			oneOffQuote.TryLoadOrCreateJob();
			using (var job = (Job)oneOffQuote.Job)
			{
				var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");

				testObjectCreator.CreateCharge(job, chargeCode1, sellCurrency: usdCurrency, osSellAmt: 10m, creditor: null);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Page");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(oneOffQuote, @"{C}-[Cover Page]   {AO}-[Page 1 of 1]

{G}-[*** NO ORGANIZATION DETAILS FOUND ***]   {AE}-[QUOTE NO.]   {AO}-[1001]

{AE}-[DATE]   {AO}-[01-Jan-20 00:00]






{C}-[QUOTATION 1001 ]








{C}-[Yours Sincerely,]






{C}-[EAGLE DATAMATION INTERNATIONAL]



{C}-[This quotation is subject to our Standard Terms and Conditions which are available on request.]

{C}-[END OF DOCUMENT]");
				}
			}
		}

		#region Implementation

		public override BusinessObject GetBusinessObject => QuotedBooking;

		public override BusinessContext BusinessContext => BusinessContext.Quotation;

		protected override void SetUp()
		{
			base.SetUp();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
		}

		QuotedBooking QuotedBooking;

		#endregion
	}
}
