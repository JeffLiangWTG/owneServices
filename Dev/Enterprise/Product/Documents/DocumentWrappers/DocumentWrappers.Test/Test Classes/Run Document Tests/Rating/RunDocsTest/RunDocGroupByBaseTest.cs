using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.RunDocsSectionGroupingTest
{
	abstract class RunDocGroupByBaseTest : BaseRunDocumentsTest
	{
		#region Similar Destination and Freight Rates with Different Destination

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyCarrierServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination
			(
				setValue: (rateEntry, value) => rateEntry.TI_PL_NKCarrierServiceLevel = value,
				value1: "AM",
				value2: "",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyCarrierServiceLevel_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyCarrierServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination
			(
				setValue: (rateEntry, value) => rateEntry.TI_RS_NKServiceLevel_NI = value,
				value1: "AM",
				value2: "",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyServiceLevel_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyServiceLevel_ExpectedOutput { get; }

		void TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination<T>(Action<RateEntry, T> setValue, T value1, T value2, string expectedOutput)
		{
			var ratingHeader = (RatingHeader)GetBusinessObject;
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 10m);
			// Emptying commodity would make DST rates part of AIR rate page i.e. commodity isn't being compared due to empty AIR commodity in CanAddToPricingPage > IsPrintedByFreightRelatedRateLineListList
			rateEntry.TI_RH_NKCommodityCode = "";

			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "NZAKL", "DDOC", 20m);
			rateEntry1.TI_RH_NKCommodityCode = "";
			setValue(rateEntry1, value1);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "NZ", "DDOC", 21m);
			rateEntry2.TI_RH_NKCommodityCode = "";
			setValue(rateEntry2, value2);
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					ratingHeader,
					expectedOutput: expectedOutput
				);
			}
		}

		#endregion

		#region Similar Destination and Freight Rates with Different Origin

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyCarrierServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin
			(
				setValue: (rateEntry, value) => rateEntry.TI_PL_NKCarrierServiceLevel = value,
				value1: "AM",
				value2: "",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyCarrierServiceLevel_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyCarrierServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin
			(
				setValue: (rateEntry, value) => rateEntry.TI_RS_NKServiceLevel_NI = value,
				value1: "AM",
				value2: "",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyServiceLevel_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_SameCommodity()
			=> TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin
			(
				setValue: (rateEntry, value) => rateEntry.TI_RH_NKCommodityCode = value,
				value1: "HAZ",
				value2: "HAZ",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_SameCommodity_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_SameCommodity_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_DifferentCommodity()
			=> TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin
			(
				setValue: (rateEntry, value) => rateEntry.TI_RH_NKCommodityCode = value,
				value1: "HAZ",
				value2: "ALUM",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_DifferentCommodity_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_DifferentCommodity_ExpectedOutput { get; }

		void TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin<T>(Action<RateEntry, T> setValue, T value1, T value2, string expectedOutput)
		{
			var ratingHeader = (RatingHeader)GetBusinessObject;
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 10m);
			// Emptying commodity would make DST rates part of AIR rate page i.e. commodity isn't being compared due to empty AIR commodity in CanAddToPricingPage > IsPrintedByFreightRelatedRateLineListList
			rateEntry.TI_RH_NKCommodityCode = "";

			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "NZAKL", "DDOC", 20m);
			rateEntry1.TI_RH_NKCommodityCode = "";
			setValue(rateEntry1, value1);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "NZAKL", "DDOC", 21m);
			rateEntry2.TI_RH_NKCommodityCode = "";
			setValue(rateEntry2, value2);
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					ratingHeader,
					expectedOutput: expectedOutput
				);
			}
		}

		#endregion

		#region Similar Destination and Freight Rates

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_DifferentOrigin()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_OriginLRC = value,
				value1: "AUEC",
				value2: "AUSR",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_DifferentOrigin_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_DifferentOrigin_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SameTransportProvider()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_OH_TransportProvider = value,
				value1: Carrier1.PK,
				value2: Carrier1.PK,
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SameTransportProvider_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SameTransportProvider_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_DifferentTransportProvider()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_OH_TransportProvider = value,
				value1: Carrier1.PK,
				value2: Carrier2.PK,
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_DifferentTransportProvider_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_DifferentTransportProvider_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SameServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_RS_NKServiceLevel_NI = value,
				value1: "AM",
				value2: "AM",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SameServiceLevel_ExpectedOutput
		);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SameServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_DifferentServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_RS_NKServiceLevel_NI = value,
				value1: "AM",
				value2: "XX",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_DifferentServiceLevel_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_DifferentServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SameCarrierServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_PL_NKCarrierServiceLevel = value,
				value1: "AM",
				value2: "AM",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SameCarrierServiceLevel_ExpectedOutput
		);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SameCarrierServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_DifferentCarrierServiceLevel()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_PL_NKCarrierServiceLevel = value,
				value1: "AM",
				value2: "XX",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_DifferentCarrierServiceLevel_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_DifferentCarrierServiceLevel_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_SameCommodity()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_RH_NKCommodityCode = value,
				value1: "HAZ",
				value2: "HAZ",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_SameCommodity_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_SameCommodity_ExpectedOutput { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocBuilder_SimilarDestinationFreightRates_DifferentCommodity()
			=> TestDocBuilder_SimilarDestinationFreightRates
			(
				setValue: (rateEntry, value) => rateEntry.TI_RH_NKCommodityCode = value,
				value1: "GEN",
				value2: "ALUM",
				expectedOutput: TestDocBuilder_SimilarDestinationFreightRates_DifferentCommodity_ExpectedOutput
			);

		protected abstract string TestDocBuilder_SimilarDestinationFreightRates_DifferentCommodity_ExpectedOutput { get; }

		void TestDocBuilder_SimilarDestinationFreightRates<T>(Action<RateEntry, T> setValue, T value1, T value2, string expectedOutput)
		{
			var ratingHeader = (RatingHeader)GetBusinessObject;
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 10m);
			// Emptying commodity would make DST rates part of AIR rate page i.e. commodity isn't being compared due to empty AIR commodity in CanAddToPricingPage > IsPrintedByFreightRelatedRateLineListList
			rateEntry.TI_RH_NKCommodityCode = "";

			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "NZAKL", "DDOC", 20m);
			rateEntry1.TI_RH_NKCommodityCode = "";
			setValue(rateEntry1, value1);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AU", "NZAKL", "DDOC", 21m);
			rateEntry2.TI_RH_NKCommodityCode = "";
			setValue(rateEntry2, value2);

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					ratingHeader,
					expectedOutput: expectedOutput
				);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Carrier1 = TestHelper.NewOrgHeader();
			Carrier2 = TestHelper.NewOrgHeader();

			Factory.Save();
		}

		OrgHeader Carrier1;
		OrgHeader Carrier2;

		protected TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
