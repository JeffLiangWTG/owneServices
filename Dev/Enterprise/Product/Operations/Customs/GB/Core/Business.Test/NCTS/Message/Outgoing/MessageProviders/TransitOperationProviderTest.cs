using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class TransitOperationProviderTest : DataProviderTestCase<TransitOperationProvider>
	{
		public void TestConstructor()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null nctsHeader", () => new TransitOperationProvider(null));
				AssertExceptionThrown<ArgumentNullException>("Null depHeader", () => new TransitOperationProvider(header));
			});
		}

		public void TestLRN()
		{
			header.MovementHeader.BM_PaperlessInbondNum = "LRN";
			header.MovementHeader.BM_EntryDate = ZDateTime.Empty;
			AssertEquals("LRN", provider.LRN);

			header.MovementHeader.BM_PaperlessInbondNum = ZString.Empty;
			AssertEquals(null, provider.LRN);

			header.MovementHeader.BM_PaperlessInbondNum = "LRN";
			header.MovementHeader.BM_EntryDate = ZDateTime.Now;
			AssertEquals(null, provider.LRN);

			header.MovementHeader.BM_Phase = GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration;
			AssertEquals("LRN", provider.LRN);
		}

		public void TestDeclarationType()
		{
			header.MovementHeader.BM_InBondEntryType = "TIR";
			AssertEquals("TIR", provider.DeclarationType);
		}

		public void TestTIRCarnetNumber()
		{
			header.MovementHeader.BM_InBondEntryType = "TIR";
			header.MovementHeader.TirCarnetNumber = "TirC";
			AssertEquals("TirC", provider.TIRCarnetNumber);
			header.MovementHeader.TirCarnetNumber = ZString.Empty;
			AssertNull("TIRCarnetNumber", provider.TIRCarnetNumber);
		}

		public void TestSecurity()
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals(GetMessage(), 0, provider.Security);

				header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				AssertEquals(GetMessage(), 1, provider.Security);

				header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				AssertEquals(GetMessage(), 2, provider.Security);

				header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				AssertEquals(GetMessage(), 3, provider.Security);

				header.MovementHeader.BM_TypeOfSecurity = "123";
				AssertEquals(GetMessage(), null, provider.Security);
			});

			string GetMessage() => $"{nameof(header.MovementHeader.BM_TypeOfSecurity)} = {header.MovementHeader.BM_TypeOfSecurity}";
		}

		public void TestAdditionalDeclarationType()
		{
			const string additionalDeclarationTypeValue = "4";
			header.MovementHeader.BM_AdditionalDeclarationType = additionalDeclarationTypeValue;

			AssertEquals(additionalDeclarationTypeValue, provider.AdditionalDeclarationType);
		}

		public void TestReducedDatasetIndicator()
		{
			header.MovementHeader.BM_ReducedDatasetIndicator = false;
			AssertEquals(false, provider.ReducedDatasetIndicator);

			header.MovementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals(true, provider.ReducedDatasetIndicator);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			const string specificCircumstanceValue = "456";
			header.MovementHeader.BM_SpecificCircumstance = specificCircumstanceValue;

			AssertEquals(specificCircumstanceValue, provider.SpecificCircumstanceIndicator);

			header.MovementHeader.BM_SpecificCircumstance = ZString.Empty;
			AssertNull("SpecificCircumstanceIndicator", provider.SpecificCircumstanceIndicator);
		}

		public void TestCommunicationLanguageAtDeparture()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = "en-gb";
			AssertEquals("en", provider.CommunicationLanguageAtDeparture);
		}

		public void TestBindingItinerary()
		{
			header.CountriesOfRouting.AddNew();
			AssertEquals(true, provider.BindingItinerary);
		}

		public void TestLimitDate()
		{
			header.MovementHeader.IsSimplifiedNctsProcedure = true;

			CombineAssertions(() =>
			{
				header.MovementHeader.BM_ExportDate = DateTime.FromOADate(1234);
				AssertEquals(DateTime.FromOADate(1234), provider.LimitDate);

				header.MovementHeader.BM_ExportDate = ZDateTime.Empty;
				AssertNull("LimitDate should be null from Empty", provider.LimitDate);

				header.MovementHeader.BM_ExportDate = new ZDateTime(DateTime.MinValue);
				AssertNull("LimitDate should be null from MinValue", provider.LimitDate);

				header.MovementHeader.BM_ExportDate = new DateTime(2024, 12, 11, 10, 9, 8, 765);
				var limitDate = ((DateTime)Provider.LimitDate);
				AssertEquals(new ZDateTime(2024, 12, 11, 10, 9, 8).ToString(@"yyyy-MM-dd'T'hh:mm:ss", CultureInfo.InvariantCulture), limitDate.ToString(@"yyyy-MM-dd'T'hh:mm:ss", CultureInfo.InvariantCulture));
				AssertEquals("no milliseconds", 0, limitDate.Millisecond);
			});
		}

		public void TestMRN()
		{
			header.ArrivalMrnFromUser = "MRN";
			header.MovementHeader.BM_EntryDate = ZDateTime.Empty;
			AssertEquals(null, provider.MRN);

			header.MovementHeader.BM_EntryDate = ZDateTime.Today;
			AssertEquals("MRN", provider.MRN);

			header.ArrivalMrnFromUser = ZString.Empty;
			AssertEquals(null, provider.MRN);
		}

		public void TestAmendmentTypeFlag()
		{
			AssertEquals(expected: false, provider.AmendmentTypeFlag);

			var depHeader = header.MovementHeader;
			depHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
			depHeader.BM_Phase = GB_NCTS5DeparturePhaseList.Codes.Amendment;
			depHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
			provider = new TransitOperationProvider(header);
			AssertEquals(expected: true, provider.AmendmentTypeFlag);
		}

		public void TestPresentationDateAndTime()
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.BM_ArrivalDate = new DateTime(2022, 04, 01, 12, 34, 56);
				AssertEquals(new ZDateTime(2022, 04, 01, 12, 34, 56), provider.PresentationDateAndTime);

				header.MovementHeader.BM_ArrivalDate = ZDateTime.Empty;
				AssertNull("PresentationDateAndTime should be null from Empty", provider.PresentationDateAndTime);

				header.MovementHeader.BM_ArrivalDate = new ZDateTime(DateTime.MinValue);
				AssertNull("PresentationDateAndTime should be null from MinValue", provider.PresentationDateAndTime);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			provider = new TransitOperationProvider(header);
		}

		TransitOperationProvider provider;
		NctsHeader header;

		protected override TransitOperationProvider GetProvider() => provider;
	}
}
