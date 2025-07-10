using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocForwardingStandardPricingPageRollUpSortTest : BaseRunDocumentsTest
	{
		[TestDate(2020, 1, 1)]
		public void TestSort_Alphabetical()
			=> TestSort
			(
				DocRollupOrSortDisplayList.Codes.Alphabetical,
				@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[description1]   {AD}-[AUD]   {AH}-[1.00]
{C}-[description2]   {AD}-[AUD]   {AH}-[2.00]
{C}-[description3]   {AD}-[AUD]   {AH}-[3.00]






{C}-[END OF DOCUMENT]",
				message: "Should be sorted by RateLine Description"
			);

		[TestDate(2020, 1, 1)]
		public void TestSort_Sequence()
			=> TestSort
			(
				DocRollupOrSortDisplayList.Codes.Sequence,
				@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[description2]   {AD}-[AUD]   {AH}-[2.00]
{C}-[description1]   {AD}-[AUD]   {AH}-[1.00]
{C}-[description3]   {AD}-[AUD]   {AH}-[3.00]






{C}-[END OF DOCUMENT]",
				message: "Should be sorted by TL_LineOrder");

		[TestDate(2020, 1, 1)]
		void TestSort(string display, string expectedOutput, string message)
		{
			var client = TestHelper.NewOrgHeader(1);

			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, display);
			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Style, DocRollupOrSortStyleList.Codes.NoGrouping);

			Factory.Save();

			var quote = TestHelper.NewQuote(client);

			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			var rateLine1 = rateEntry1.AddFlatRateLine("BAF", 3m, description: "description3");
			rateLine1.TL_LineOrder = 3;
			var rateLine2 = rateEntry1.AddFlatRateLine("FRT", 2m, description: "description2");
			rateLine2.TL_LineOrder = 1;
			var rateLine3 = rateEntry1.AddFlatRateLine("WAR", 1m, description: "description1");
			rateLine3.TL_LineOrder = 2;

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Standard Pricing Page");

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: expectedOutput,
					message: message
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestRollUp_All()
			=> TestRollUp
			(
				DocRollupOrSortDisplayList.Codes.RollUpCharges,
				DocRollupOrSortStyleList.Codes.All,
				@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[All charges except Customs Duty and Tax]   {AD}-[AUD]   {AH}-[54.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]






{C}-[END OF DOCUMENT]"
			);

		[TestDate(2020, 1, 1)]
		public void TestRollUp_OriginLoadingFreightInsurance()
			=> TestRollUp
			(
				DocRollupOrSortDisplayList.Codes.RollUpCharges,
				DocRollupOrSortStyleList.Codes.OFD,
				@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[43.00]
{C}-[Freight and Insurance Charges]   {AD}-[AUD]   {AH}-[11.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]






{C}-[END OF DOCUMENT]"
			);

		[TestDate(2020, 1, 1)]
		public void TestRollUp_RollUpChargesAndSequence()
			=> TestRollUp
			(
				DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence,
				DocRollupOrSortStyleList.Codes.OFD,
			@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight and Insurance Charges]   {AD}-[AUD]   {AH}-[11.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]
{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[43.00]






{C}-[END OF DOCUMENT]"
			);

		void TestRollUp(string display, string style, string expectedOutput)
		{
			var client = TestHelper.NewOrgHeader(1);

			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, display);
			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Style, style);

			Factory.Save();

			var quote = TestHelper.NewQuote(client);

			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			var rateLine11 = rateEntry1.AddFlatRateLine("FRT", 11m, description: "description11");
			var rateLine12 = rateEntry1.AddUnitRateLine("WAR", 12m, description: "description12", lineUnit: "KG");
			var rateLine13 = rateEntry1.AddFlatRateLine("BAF", 13m, description: "description13", currency: "NZD");

			var rateEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "USLAX");
			var rateLine21 = rateEntry2.AddFlatRateLine("ODOC", 21m, description: "description21");
			var rateLine22 = rateEntry2.AddFlatRateLine("OFUMI", 22m, description: "description22");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Standard Pricing Page");

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: expectedOutput
				);
			}
		}

		[TestDate(2025, 4, 16)]
		public void TestRollUpWithPercentageCalculator_RateDoesNotContainCurrency()
		{
			var client = TestHelper.NewOrgHeader(1);

			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpCharges);
			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Style, DocRollupOrSortStyleList.Codes.All);

			Factory.Save();

			var charge = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();

			var quote = TestHelper.NewQuote(client);

			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			var rateLine11 = rateEntry1.AddFlatRateLine("FRT", 11m, description: "description11");
			var rateLine12 = rateEntry1.AddPercentageCharge("INSUR", charge, 10);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Standard Pricing Page");

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[16-Apr-25  -  16-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[All charges except Customs Duty and Tax]   {AD}-[AUD]   {AH}-[11.00]
{AH}-[10.0000]   {AM}-[% of Freight]






{C}-[END OF DOCUMENT]"
				);
			}
		}

		void TestRollUpWithRatingDocumentsChargeOrder(string rollUpStyle, string expectedOutput)
		{
			var client = TestHelper.NewOrgHeader(1);

			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, "RSQ");
			SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Style, rollUpStyle);

			var charge1 = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).FirstOrDefault();
			var charge2 = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF")).FirstOrDefault();
			var charge3 = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).FirstOrDefault();

			var chargeOrder = client.RatingDocumentsChargeOrders.AddNew();
			chargeOrder.RCO_AC_ChargeCode = charge1.PK;
			chargeOrder.RCO_PrintOrder = 1;
			chargeOrder.RCO_DocumentType = "All";

			var chargeOrder2 = client.RatingDocumentsChargeOrders.AddNew();
			chargeOrder2.RCO_AC_ChargeCode = charge2.PK;
			chargeOrder2.RCO_PrintOrder = 2;
			chargeOrder2.RCO_DocumentType = "All";

			var chargeOrder3 = client.RatingDocumentsChargeOrders.AddNew();
			chargeOrder3.RCO_AC_ChargeCode = charge3.PK;
			chargeOrder3.RCO_PrintOrder = 3;
			chargeOrder3.RCO_DocumentType = "All";

			Factory.Save();

			var quote = TestHelper.NewQuote(client);

			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			var rateLine11 = rateEntry1.AddFlatRateLine("FRT", 11m, description: "FRT");
			var rateLine12 = rateEntry1.AddUnitRateLine("INSUR", 12m, description: "INSUR", lineUnit: "KG");
			var rateLine13 = rateEntry1.AddFlatRateLine("BAF", 13m, description: "BAF", currency: "NZD");

			var rateEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "USLAX");
			var rateLine21 = rateEntry2.AddFlatRateLine("ODOC", 21m, description: "ODOC");
			var rateLine22 = rateEntry2.AddFlatRateLine("CAF", 22m, description: "CAF");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Standard Pricing Page");

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: expectedOutput
				);
			}
		}

		#region TestRollUpAndSequenceWithStyles

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_DEF()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"DEF",
		expectedOutput:
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT]   {AD}-[AUD]   {AH}-[11.00]
{C}-[BAF]   {AD}-[NZD]   {AH}-[13.00]
{C}-[ODOC]   {AD}-[AUD]   {AH}-[21.00]
{C}-[CAF]   {AD}-[AUD]   {AH}-[22.00]
{C}-[INSUR]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_NOG()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"NOG",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT]   {AD}-[AUD]   {AH}-[11.00]
{C}-[BAF]   {AD}-[NZD]   {AH}-[13.00]
{C}-[ODOC]   {AD}-[AUD]   {AH}-[21.00]
{C}-[CAF]   {AD}-[AUD]   {AH}-[22.00]
{C}-[INSUR]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_AEC()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"AEC",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin, Freight, Insurance and Destination Charges]   {AD}-[AUD]   {AH}-[54.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_OANDF()
			=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"O&F",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin, Freight and Insurance Charges]   {AD}-[AUD]   {AH}-[54.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_OFD()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"OFD",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight and Insurance Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]
{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[21.00]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_ORF()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"ORF",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin and Freight Charges]   {AD}-[AUD]   {AH}-[54.00]
{AD}-[NZD]   {AH}-[13.00]
{C}-[INSUR]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_FRT()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"FRT",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[NZD]   {AH}-[13.00]
{C}-[ODOC]   {AD}-[AUD]   {AH}-[21.00]
{C}-[INSUR]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_FANDD()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"F&D",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight, Insurance and Destination Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]
{C}-[ODOC]   {AD}-[AUD]   {AH}-[21.00]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_OFO()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"OFO",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight and Insurance Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]
{AD}-[NZD]   {AH}-[13.00]
{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[21.00]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_OFF()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"OFF",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[NZD]   {AH}-[13.00]
{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[21.00]
{C}-[Insurance Charges]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_OFI()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"OFI",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[NZD]   {AH}-[13.00]
{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[21.00]
{C}-[Insurance Charges]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_CCD()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"CCD",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT]   {AD}-[AUD]   {AH}-[11.00]
{C}-[BAF]   {AD}-[NZD]   {AH}-[13.00]
{C}-[ODOC]   {AD}-[AUD]   {AH}-[21.00]
{C}-[CAF]   {AD}-[AUD]   {AH}-[22.00]
{C}-[INSUR]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		[TestDate(2025, 4, 14)]
		public void TestRollUpAndSequenceWithStyle_CCG()
		=> TestRollUpWithRatingDocumentsChargeOrder
		(
		"CCG",
		expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[14-Apr-25  -  14-May-25]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Freight Charges]   {AD}-[AUD]   {AH}-[33.00]
{AD}-[NZD]   {AH}-[13.00]
{C}-[Origin Charges]   {AD}-[AUD]   {AH}-[21.00]
{C}-[Insurance Charges]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]"
		);

		#endregion

		public static void SetupOrganisation(OrgHeader org, string jobType, string propertyToSet, string value)
		{
			RatingDocumentsChargeGroupingOrRollup orgRatingDocRollupOrGroup = null;

			var orgRatingDocRollupOrGroupCollection = org.CompanyData.RatingDocRollupOrGroups;

			foreach (RatingDocumentsChargeGroupingOrRollup orgSetting in orgRatingDocRollupOrGroupCollection)
			{
				if (orgSetting.RCG_JobType == jobType)
				{
					orgRatingDocRollupOrGroup = orgSetting;
					break;
				}
			}

			if (orgRatingDocRollupOrGroup == null)
			{
				orgRatingDocRollupOrGroup = orgRatingDocRollupOrGroupCollection.AddNew();
			}

			orgRatingDocRollupOrGroup.RCG_Module = DocRollupOrSortModuleList.Codes.All;
			orgRatingDocRollupOrGroup.RCG_JobType = jobType;
			orgRatingDocRollupOrGroup.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;

			if (string.IsNullOrEmpty(orgRatingDocRollupOrGroup.RCG_Display))
			{
				orgRatingDocRollupOrGroup.RCG_Display = DocRollupOrSortDisplayList.Codes.Default;
			}

			if (string.IsNullOrEmpty(orgRatingDocRollupOrGroup.RCG_Style))
			{
				orgRatingDocRollupOrGroup.RCG_Style = DocRollupOrSortStyleList.Codes.Default;
			}

			orgRatingDocRollupOrGroup[propertyToSet] = value;
		}

		#region Implementation

		public override BusinessObject GetBusinessObject => Quote;

		public override BusinessContext BusinessContext => BusinessContext.Quotation;

		protected override void SetUp()
		{
			base.SetUp();
			var client = TestHelper.NewOrgHeader(1);
			Quote = TestHelper.NewQuote(client);
		}

		Quote Quote;

		TestHelper TestHelper => testHelper ??= new TestHelper(Factory);
		TestHelper testHelper;

		#endregion
	}
}
