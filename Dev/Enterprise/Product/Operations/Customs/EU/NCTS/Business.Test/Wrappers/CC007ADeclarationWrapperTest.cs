using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CC007ADeclarationWrapper))]
	class CC007ADeclarationWrapperTest : DeclarationWrapperAbstractTest<CC007ADeclarationWrapper>
	{
		public void TestDestination()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "GO SPORT FRANCE";
			header.DestinationTrader.OrganisationPK = org.PK;
			AssertEquals("GO SPORT FRANCE", wrapper.Destination.Name);
		}

		public void TestArrivalNotificationPlace()
		{
			AssertEquals("Brisbane", wrapper.ArrivalNotificationPlace);
		}

		public void TestArrivalNotificationPlaceLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.ArrivalNotificationPlaceLanguage);
		}

		public void TestArrivalAgreedLocationOfGoodsCode()
		{
			header.ArrivalMovementHeader.BM_LocationOfGoodsCode = "123";
			AssertEquals("123", wrapper.ArrivalAgreedLocationOfGoodsCode);
		}

		public void TestArrivalAgreedLocationOfGoodsLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.ArrivalAgreedLocationOfGoodsLanguage);
		}

		public void TestDialogLanguageIndicatorAtDestination()
		{
			AssertEquals(ZString.Empty, wrapper.DialogLanguageIndicatorAtDestination);
		}

		[TestDate(2020, 12, 8)]
		public void TestArrivalNotificationDate()
		{
			AssertEquals("20201208", wrapper.ArrivalNotificationDate);

			header.ArrivalMovementHeader.BM_EntryDate = new ZDateTime(2021, 3, 4);
			AssertEquals("20210304", wrapper.ArrivalNotificationDate);
		}

		public void TestIsSimplifiedArrivalProcedure()
		{
			CombineAssertions(() =>
			{
				header.ArrivalMovementHeader.BM_GONumber = NctsControlResult.Codes.AuthorizedTrader;
				AssertEquals("Simplified", ZBool.True, wrapper.IsSimplifiedArrivalProcedure);
				header.ArrivalMovementHeader.BM_GONumber = "TT";
				AssertEquals("Not Simplified", ZBool.False, wrapper.IsSimplifiedArrivalProcedure);
			});
		}

		public void TestCustomsPresentationOfficeRefNumber()
		{
			header.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "FR008";
			AssertEquals("FR008", wrapper.CustomsPresentationOfficeRefNumber);
		}

		public void TestCustomsSubPlace()
		{
			header.ArrivalMovementHeader.BM_CustomsSubPlace = "DOVER ERTS";
			AssertEquals("DOVER ERTS", wrapper.CustomsSubPlace);
		}

		public void TestDeclarantTIN_IsArrivalFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "2222222222";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR2222222222", wrapper.DeclarantTIN);
		}

		public void TestEnRouteEvents()
		{
			header.EnRouteIncidents.AddNew();
			header.EnRouteTransshipments.AddNew();
			header.EnRouteTransshipments.AddNew();
			header.EnRouteSeals.AddNew();
			header.EnRouteSeals.AddNew();
			header.EnRouteSeals.AddNew();
			AssertEquals(3, wrapper.EnRouteEvents.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			wrapper = new CC007ADeclarationWrapper(header);
		}
		NctsHeader header;
		CC007ADeclarationWrapper wrapper;

		protected override CC007ADeclarationWrapper GetProvider()
		{
			header.DestinationTrader.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			return new CC007ADeclarationWrapper(header);
		}
	}
}
