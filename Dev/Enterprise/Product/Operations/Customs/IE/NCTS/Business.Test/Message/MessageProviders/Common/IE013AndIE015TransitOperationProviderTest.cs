using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013AndIE015TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015TransitOperationProvider>
	{
		public void TestLRN()
		{
			AssertEquals("LRN", NCTSOutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}

		public void TestDeclarationType()
		{
			movementHeader.BM_InBondEntryType = "TYP1";
			AssertEquals("Declaration Type", "TYP1", Provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			movementHeader.BM_AdditionalDeclarationType = "A";
			AssertEquals("Additional Declaration Type", "A", Provider.AdditionalDeclarationType);
		}

		public void TestTIRCarnetNumber()
		{
			movementHeader.TirCarnetNumber = "NO1234";
			AssertEquals("TIR Carnet Number", "NO1234", Provider.TIRCarnetNumber);
		}

		public void TestPresentationOfTheGoodsDateAndTime()
		{
			movementHeader.BM_PresentationDateTime = new DateTime(2023, 5, 5, 8, 30, 15);
			AssertEquals(new DateTime(2023, 5, 5, 8, 30, 15), Provider.PresentationOfTheGoodsDateAndTime);
		}

		public void TestSecurity()
		{
			CombineAssertions(() =>
			{
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals(GetMessage(), "0", Provider.Security);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				AssertEquals(GetMessage(), "1", Provider.Security);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				AssertEquals(GetMessage(), "2", Provider.Security);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				AssertEquals(GetMessage(), "3", Provider.Security);

				movementHeader.BM_TypeOfSecurity = "123";
				AssertEquals(GetMessage(), "0", Provider.Security);
			});

			string GetMessage() => $"{nameof(movementHeader.BM_TypeOfSecurity)} = {movementHeader.BM_TypeOfSecurity}";
		}

		public void TestReducedDatasetIndicator()
		{
			movementHeader.BM_ReducedDatasetIndicator = ZBool.False;
			AssertEquals(false, Provider.ReducedDatasetIndicator);

			movementHeader.BM_ReducedDatasetIndicator = ZBool.True;
			AssertEquals(true, Provider.ReducedDatasetIndicator);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals(GetMessage(), string.Empty, GetProvider().SpecificCircumstanceIndicator);

				movementHeader.BM_SpecificCircumstance = "ABC";
				AssertEquals(GetMessage(), "ABC", GetProvider().SpecificCircumstanceIndicator);
			});

			string GetMessage() => $"{nameof(movementHeader.BM_SpecificCircumstance)} = {movementHeader.BM_SpecificCircumstance}";
		}

		public void TestCommunicationLanguageAtDeparture()
		{
			AssertEquals("IE", Provider.CommunicationLanguageAtDeparture);
		}

		public void TestBindingItinerary()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No country added", false, Provider.BindingItinerary);
				var country1 = nctsHeader.CountriesOfRouting.AddNew();
				AssertEquals("Country with no data added", false, Provider.BindingItinerary);
				country1.CY_Data = "FR";
				AssertEquals("Country with data", true, Provider.BindingItinerary);
			});
		}

		public void TestLimitDate()
		{
			movementHeader.BM_ExportDate = ZDateTime.BrettsBirthday;
			AssertEquals(new DateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Unspecified), Provider.LimitDate);
		}

		protected override IE013AndIE015TransitOperationProvider GetProvider() => new IE013AndIE015TransitOperationProviderForTest(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}

	class IE013AndIE015TransitOperationProviderForTest : IE013AndIE015TransitOperationProvider
	{
		public IE013AndIE015TransitOperationProviderForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}
	}
}
