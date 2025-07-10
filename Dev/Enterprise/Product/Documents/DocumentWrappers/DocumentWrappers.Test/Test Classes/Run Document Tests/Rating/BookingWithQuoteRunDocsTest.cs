using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
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
	sealed class BookingWithQuoteRunDocsTest : BaseRunDocumentsTest
	{
		public BookingWithQuoteRunDocsTest() { }

		#region Cover Page

		[TestDate(2020, 1, 1)]
		public void TestDocument_CoverPage_ShouldNotShowQuotationClientReplyLinks()
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var chargeCode1 = chargeCodeFactory.New("CHG1", "Charge Code 1 Description", FlatCalculator.Code);

			var oneOffQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

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

{G}-[*** NO ORGANIZATION DETAILS FOUND ***]   {AE}-[BOOKING]   {AO}-[S00001000]

{AE}-[QUOTE NO.]   {AO}-[1001]

{AE}-[DATE]   {AO}-[01-Jan-20 00:00]





{C}-[ S00001000 ]








{C}-[Yours Sincerely,]






{C}-[EAGLE DATAMATION INTERNATIONAL]





{C}-[END OF DOCUMENT]");
				}
			}
		}

		#endregion

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
