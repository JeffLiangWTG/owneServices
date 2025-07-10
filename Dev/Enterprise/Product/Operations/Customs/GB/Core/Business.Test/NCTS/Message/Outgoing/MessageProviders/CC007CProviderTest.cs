using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC007CProvider))]
	sealed class CC007CProviderTest : NctsHeaderProviderAbstractTest<CC007CProvider>
	{
		[TestDate(2023, 03, 15, 14, 31, 59)]
		public void TestArrivalNotificationDateAndTime() => CombineAssertions(() =>
		{
			AssertEquals("BM_ArrivalDate not set, ArrivalNotificationDateAndTime = today", new ZDateTime(2023, 03, 15, 14, 31, 59).ToDateTime(), Provider.ArrivalNotificationDateAndTime);

			nctsHeader.ArrivalMovementHeader.BM_ArrivalDate = new ZDateTime(1994, 2, 1, 21, 48, 20, 789);
			AssertEquals("BM_ArrivalDate set, ArrivalNotificationDateAndTime = BM_ArrivalDate", new ZDateTime(1994, 2, 1, 21, 48, 20).ToDateTime(), Provider.ArrivalNotificationDateAndTime);

			AssertEquals("No Milliseconds", 0, Provider.ArrivalNotificationDateAndTime.Millisecond);
		});

		public void TestSimplifiedProcedure()
		{
			nctsHeader.ArrivalMovementHeader.BM_GONumber = "1";
			AssertEquals(expected: false, Provider.SimplifiedProcedure);

			nctsHeader.ArrivalMovementHeader.BM_GONumber = ZString.Empty;
			AssertEquals(expected: false, Provider.SimplifiedProcedure);

			provider = (CC007CProvider)Activator.CreateInstance(typeof(CC007CProvider), nctsHeader);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_OA_AppliesTo = orgHeader.PK;
			authorizationHeader.CPH_Number = "7777";
			authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			authorizationHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			authorizationHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			nctsHeader.ArrivalMovementHeader.BM_GONumber = ZString.Empty;
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			AssertEquals(expected: false, provider.SimplifiedProcedure);

			var authorizationUsage1 = nctsHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_OH_Owner = orgHeader.PK;
			authorizationUsage1.AGC_Number = "usage1";
			authorizationUsage1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			provider = (CC007CProvider)Activator.CreateInstance(typeof(CC007CProvider), nctsHeader);
			AssertEquals(expected: true, provider.SimplifiedProcedure);
		}

		public void TestIncidentFlag()
		{
			nctsHeader.EnRouteIncidents.AddNew();
			AssertEquals(true, Provider.IncidentFlag);
		}

		public void TestTraderIdentificationNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			nctsHeader.DestinationTrader.Organisation.CustomsCodes.AddRange(Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "HolderID", Core.Constants.CountryCodes.UnitedKingdom));

			AssertEquals("GBHOLDERID", Provider.TraderIdentificationNumber);
		}

		public void TestTraderIdentificationNumber_With_XI_Prefix()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			nctsHeader.DestinationTrader.Organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI175521246821", Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("XI175521246821", Provider.TraderIdentificationNumber);
		}

		public void TestTraderCommunicationLanguage()
		{
			nctsHeader.BH_CommunicationLanguage = "EN";
			AssertEquals("EN", Provider.TraderCommunicationLanguage);
		}

		public void TestDischarge()
		{
			nctsHeader.ArrivalMovementHeader.BM_DischargeType = "A";
			AssertEquals("A", Provider.Discharge);
		}

		public void TestVoletPageNumber()
		{
			nctsHeader.ArrivalMovementHeader.BM_CarnetTotalPages = 2;
			AssertEquals("2", Provider.VoletPageNumber);
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
		}

		public void TestAuthorisations()
		{
			nctsHeader.CusAuthorizationUsages.AddNew();
			AssertEquals(1, Provider.Authorisations.Count);
		}

		public void TestMessageRecipient()
		{
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			AssertEquals("No ArrivalCustomsOffice", ZString.Empty, arrivalMovementHeader.DestinationCustomsOfficeCodeCountryForArrival);
			NCTSTestHelper.CreateCustomsOfficeForTest(arrivalMovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "GB000011", ZDateTime.Empty, true);
			AssertEquals("Has ArrivalCustomsOffice", "GB000011", arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);
			AssertEquals("NTA.GB", Provider.MessageRecipient);
		}
		protected override string MessageType => Constants.MessageTypes.CC007C;

		protected override string MovementType => NctsMovementType.Codes.Arrival;
	}
}
