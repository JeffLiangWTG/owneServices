using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(TransitOperationProvider))]
	sealed class TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<TransitOperationProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null nctsHeader",
#if NETFRAMEWORK
					@"Value cannot be null.
Parameter name: nctsHeader",
#else
					"Value cannot be null. (Parameter 'nctsHeader')",
#endif
					() => new TransitOperationProvider(null));
				AssertExceptionThrown<ArgumentNullException>("Null depHeader",
#if NETFRAMEWORK
					@"Value cannot be null.
Parameter name: depHeader",
#else
					"Value cannot be null. (Parameter 'depHeader')",
#endif
					() => new TransitOperationProvider(GetArrivalHeader()));
			});

			NctsHeader GetArrivalHeader()
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				return header;
			}
		}

		public void TestLRN()
		{
			header.MovementHeader.BM_PaperlessInbondNum = "LRNTEST";
			AssertEquals("LRNTEST", provider.LRN);
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
		}

		public void TestCommunicationLanguageAtDeparture()
		{
			header.BH_CommunicationLanguage = "EN";
			AssertEquals("EN", provider.CommunicationLanguageAtDeparture);
		}

		public void TestBindingItinerary()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Routing itinerary defined", false, provider.BindingItinerary);
				header.CountriesOfRouting.AddNew();
				AssertEquals("Routing itinerary defined", false, provider.BindingItinerary);
			});
		}

		public void TestLimitDate()
		{
			header.MovementHeader.IsSimplifiedNctsProcedure = true;

			CombineAssertions(() =>
			{
				header.MovementHeader.BM_ExportDate = System.DateTime.FromOADate(1234);
				AssertEquals(System.DateTime.FromOADate(1234), provider.LimitDate);

				header.MovementHeader.BM_ExportDate = ZDateTime.Empty;
				AssertNull("LimitDate should be null from Empty", provider.LimitDate);

				header.MovementHeader.BM_ExportDate = new ZDateTime(DateTime.MinValue);
				AssertNull("LimitDate should be null from MinValue", provider.LimitDate);
			});
		}

		public void TestMRN()
		{
			AssertEquals(null, provider.MRN);
		}

		public void TestAmendmentTypeFlag()
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.BM_CustomsStatus = "GIV";
				AssertEquals("is GIV", true, provider.AmendmentTypeFlag);
				header.MovementHeader.BM_CustomsStatus = "NOT";
				AssertEquals("is not GIV", false, provider.AmendmentTypeFlag);
			});
		}

		public void TestPresentationDateAndTime()
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.BM_ArrivalDate = new DateTime(2022, 04, 01, 12, 34, 00);
				AssertEquals(new ZDateTime(2022, 04, 01, 12, 34, 00).ToUniversalBranchTime(), provider.PresentationDateAndTime);

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
