using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class JobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<JobDeclarationLookups, JobDeclaration>
	{
		public void TestPaymentPartyList()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("SEL", "DEC", lookups.PaymentPartyList.CodesAsString);
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("IND", "DEC, DEF, RPP", lookups.PaymentPartyList.CodesAsString);
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("DIR", "DEC, DEF, REP", lookups.PaymentPartyList.CodesAsString);
				jobDeclaration.JE_DeclarantType = ZString.Empty;
				AssertEquals("Empty", "DEC, DEF, REP", lookups.PaymentPartyList.CodesAsString);
			});
		}

		public void TestDefermentAccountNumberList()
		{
			jobDeclaration.JE_OA_DeclarantAddress = Factory.CreateDeferralParty("10", "1111").MainAddress.PK;
			jobDeclaration.DefermentPartyDocAddress.OrganisationPK = Factory.CreateDeferralParty("10", "2222").PK;
			jobDeclaration.JE_OA_Representative = Factory.CreateDeferralParty("10", "3333").MainAddress.PK;
			jobDeclaration.JE_OA_BuyingAgentAddress = Factory.CreateDeferralParty("10", "4444").MainAddress.PK;

			CombineAssertions(() =>
			{
				jobDeclaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
				AssertEquals("Declarant-DEC", "1111", lookups.DefermentAccountNumberList.CodesAsString);

				jobDeclaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.DefermentParty;
				AssertEquals("DefermentParty-DEF", "2222", lookups.DefermentAccountNumberList.CodesAsString);

				jobDeclaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Representative;
				AssertEquals("Representative-REP", "3333", lookups.DefermentAccountNumberList.CodesAsString);

				jobDeclaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.RepresentedParty;
				AssertEquals("RepresentedParty-RPP", "4444", lookups.DefermentAccountNumberList.CodesAsString);
			});
		}

		public void TestBuyingAgentAddressList()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.BuyingAgentAddressList);
		}

		public void TestStatisticStatusCodeList()
		{
			var codeList = lookups.StatisticStatusCodeList;

			AssertSame(Factory.GetCachedValue<StatisticStatusCodeList>(), codeList);
			AssertEquals("01, 04", codeList.CodesAsString);
		}

		public void TestIncoTermList_Import()
		{
			jobDeclaration.JE_MessageType = "IMP";
			var codeList = lookups.IncoTermList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.IncoTermList, codeList);
				AssertType<IncotermA1840CodeList>("Type", codeList);
			});
		}

		public void TestIncoTermList_Export()
		{
			jobDeclaration.JE_MessageType = "EXP";
			var codeList = lookups.IncoTermList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.IncoTermList, codeList);
				AssertType<IncoTermsCodeDescriptionPairList>("Type", codeList);
			});
		}

		public void TestDeclarantTypeList()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				AssertDeclarantTypeList("JE_MessageType MSC", "SEL, DIR, IND");

				jobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertDeclarantTypeList("JE_MessageType WAD", "SEL, DIR");
			});
		}

		public void TestTransportMeansList_Export()
		{
			CombineAssertions(() =>
			{
				AssertTransportMeansListIsCorrect(ZString.Empty, ZString.Empty);
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.Sea, "10, 11", "10");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.Rail, "20, 21");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.Road, "30", "30");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.Air, "40, 41", "40");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.InlandWaterwayTransport, "80, 81", "80");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.OwnPropulsion, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.Mail, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");
				AssertTransportMeansListIsCorrect(TransportTypeList.Codes.FixedTransportInstallations, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");

				AssertSame("Cached", lookups.TransportMeansList, lookups.TransportMeansList);
			});

			void AssertTransportMeansListIsCorrect(string transportModeInland, string expectedTransportMeansCodes, string expectedDefaultCode = null)
			{
				jobDeclaration.JE_TransportModeInland = transportModeInland;

				AssertEquals($"{transportModeInland} - Codes", expectedTransportMeansCodes, jobDeclaration.Lookups.TransportMeansList.CodesAsString);
				AssertEquals($"{transportModeInland} - DefaultCode", expectedDefaultCode, jobDeclaration.Lookups.TransportMeansList.DefaultCode);
			}
		}

		public void TestTransportMeansList_Import()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.TransportMeansList, lookups.TransportMeansList);
				AssertType<TransportMeansList>("Type", lookups.TransportMeansList);
			});
		}

		public void TestTransportMeansList_MiscellaneousCustoms()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;

			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.TransportMeansList, lookups.TransportMeansList);
				AssertType<TransportMeansList>("Type", lookups.TransportMeansList);
			});
		}

		void AssertDeclarantTypeList(string testCase, ZString expectedCodesAsString)
		{
			var list = (CodeDescriptionPairList)lookups.DeclarantTypeList;
			AssertEquals(testCase + "->CodesAsString", expectedCodesAsString, list.CodesAsString);
			AssertSame(testCase + "->Cached", list, lookups.DeclarantTypeList);
		}

		public void TestVATClaimBackList()
		{
			AssertEquals("N, Y", jobDeclaration.Lookups.VATClaimBackList.CodesAsString);
		}
	}
}
