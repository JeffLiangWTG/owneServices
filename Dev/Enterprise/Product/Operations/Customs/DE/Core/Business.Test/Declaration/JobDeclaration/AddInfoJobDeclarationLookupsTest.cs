using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
	{
		public void TestDeferTypeList()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("SEL", "DEC", declaration.AddInfoLookups.DeferTypeList.CodesAsString);
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("IND", "DEC, DEF, RPP", declaration.AddInfoLookups.DeferTypeList.CodesAsString);
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("DIR", "DEC, DEF, REP", declaration.AddInfoLookups.DeferTypeList.CodesAsString);
				declaration.JE_DeclarantType = ZString.Empty;
				AssertEquals("Empty", "DEC, DEF, REP", declaration.AddInfoLookups.DeferTypeList.CodesAsString);
			});
		}

		public void TestVATAccountNumberList()
		{
			declaration.JE_OA_DeclarantAddress = Factory.CreateDeferralParty("10", "1111").MainAddress.PK;
			declaration.DefermentPartyDocAddress.OrganisationPK = Factory.CreateDeferralParty("10", "2222").PK;
			declaration.JE_OA_Representative = Factory.CreateDeferralParty("10", "3333").MainAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = Factory.CreateDeferralParty("10", "4444").MainAddress.PK;

			CombineAssertions(() =>
			{
				declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.Declarant;
				AssertEquals("Declarant-DEC", "1111", declaration.Lookups.VATAccountNumberList.CodesAsString);

				declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.DefermentParty;
				AssertEquals("DefermentParty-DEF", "2222", declaration.Lookups.VATAccountNumberList.CodesAsString);

				declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.Representative;
				AssertEquals("Representative-REP", "3333", declaration.Lookups.VATAccountNumberList.CodesAsString);

				declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.RepresentedParty;
				AssertEquals("RepresentedParty-RPP", "4444", declaration.Lookups.VATAccountNumberList.CodesAsString);
			});
		}

		public void TestBorderTransportMeansList_Import()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(ZString.Empty, ZString.Empty);
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Sea, "02, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Rail, "03, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Road, "01, 05, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Air, "04, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Mail, "06, 07");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.FixedTransportInstallations, "06, 07");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.InlandWaterwayTransport, "02, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.OwnPropulsion, "06, 07");
				AssertSame("Testing cache", lookups.BorderTransportMeansList, lookups.BorderTransportMeansList);
			});
		}

		public void TestBorderTransportMeansList_Export()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(ZString.Empty, ZString.Empty);
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Sea, "10, 11");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Rail, "21");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Road, "30");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Air, "40, 41");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Mail, "10, 11, 21, 30, 40, 41, 80, 81");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.FixedTransportInstallations, "10, 11, 21, 30, 40, 41, 80, 81");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.InlandWaterwayTransport, "80, 81");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.OwnPropulsion, "10, 11, 21, 30, 40, 41, 80, 81");
				AssertSame("Testing cache", lookups.BorderTransportMeansList, lookups.BorderTransportMeansList);
			});
		}

		public override void TestSpecificCircumstanceIndicatorList()
		{
			var codeList = declaration.AddInfoLookups.SpecificCircumstanceIndicatorList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", codeList, declaration.AddInfoLookups.SpecificCircumstanceIndicatorList);
				AssertEquals("A20", codeList.CodesAsString);
			});
		}

		public void TestMethodOfPayment()
		{
			var dateInFuture = ZDate.Today.AddDays(4);
			var dateInPast = ZDate.Today.AddDays(-4);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", parent: eun);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Payment Methods");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Valid by Date And Code", dateInPast, dateInFuture);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "B", "Valid by Code B", dateInPast, dateInFuture);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "S", "Not Valid by Code S", dateInPast, dateInFuture);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "Not Valid by Code L", dateInPast, dateInFuture);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "C", "Not Valid by Date", dateInPast, dateInPast);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "H", "Not Valid by Country", dateInPast, dateInFuture);

			Factory.Save();

			var paymentMethodList = declaration.AddInfoLookups.MethodOfPaymentList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "A, B", paymentMethodList.CodesAsString);
				AssertSame("Should be cached", paymentMethodList, declaration.AddInfoLookups.MethodOfPaymentList);
			});
		}

		public void TestAgreedPlaceCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "2", "222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var cached = declaration.AddInfoLookups.AgreedPlaceCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("1, 2", ((CodeDescriptionPairList)declaration.AddInfoLookups.AgreedPlaceCodeList).CodesAsString);
				AssertSame("Should be cached", cached, declaration.AddInfoLookups.AgreedPlaceCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		void AssertBorderMeansOfTransportListIsCorrectForDecTransport(ZString decTransport, ZString expectedBorderMeansOfTransportCodes)
		{
			declaration.JE_TransportMode = decTransport;
			AssertEquals(decTransport, expectedBorderMeansOfTransportCodes, declaration.AddInfoLookups.BorderTransportMeansList.CodesAsString);
		}
	}
}
