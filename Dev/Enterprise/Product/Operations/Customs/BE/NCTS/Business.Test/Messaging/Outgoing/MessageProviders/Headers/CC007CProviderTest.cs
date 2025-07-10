using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC007CProvider))]
	sealed class CC007CProviderTest : NctsHeaderProviderAbstractTest<CC007CProvider>
	{
		[TestDate(2023, 03, 15, 14, 31, 00)]
		public void TestArrivalNotificationDateAndTime() => CombineAssertions(() =>
		{
			AssertEquals("BM_ArrivalDate not set, ArrivalNotificationDateAndTime = today", ZDateTime.UtcNow.ToUniversalBranchTime(), Provider.ArrivalNotificationDateAndTime);

			nctsHeader.ArrivalMovementHeader.BM_ArrivalDate = new DateTime(2022, 04, 01, 12, 34, 00);
			AssertEquals(new ZDateTime(2022, 04, 01, 12, 34, 00).ToUniversalBranchTime(), provider.ArrivalNotificationDateAndTime);
		});

		public void TestSimplifiedProcedure()
		{
			nctsHeader.CusAuthorizationUsages.Clear();
			CombineAssertions(() =>
			{
				AssertEquals("Simplified Procedure should be 0", false, Provider.SimplifiedProcedure);

				var cau = nctsHeader.CusAuthorizationUsages.AddNew();
				cau.AGC_ParentID = nctsHeader.PK;
				AssertEquals("Simplified Procedure should be 1 when ParentID is existing", true,
					Provider.SimplifiedProcedure);

				nctsHeader.CusAuthorizationUsages.Clear();
				cau = nctsHeader.CusAuthorizationUsages.AddNew();
				cau.AGC_Code = "TST";
				AssertEquals("Simplified Procedure should be 1 when AGC_Code is filled", true,
					Provider.SimplifiedProcedure);

				nctsHeader.CusAuthorizationUsages.Clear();
				cau = nctsHeader.CusAuthorizationUsages.AddNew();
				cau.AGC_Number = "TEST_AGC_NUMBER";
				AssertEquals("Simplified Procedure should be 1 when AGC_Number is filled", true,
					Provider.SimplifiedProcedure);
			});
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
			nctsHeader.DestinationTrader.Organisation.CustomsCodes.AddRange(Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "HolderID", Core.Constants.CountryCodes.Belgium));

			AssertEquals("BEHolderID", Provider.TraderIdentificationNumber);
		}

		public void TestTraderCommunicationLanguage()
		{
			nctsHeader.BH_CommunicationLanguage = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, Provider.TraderCommunicationLanguage);
		}

		public void TestDischarge()
		{
			nctsHeader.ArrivalMovementHeader.BM_DischargeType = "A";
			AssertEquals("A", Provider.Discharge);
		}

		public void TestVoletPageNumber()
		{
			// Add check for voletPageNumber depends on discharge mapped
			nctsHeader.ArrivalMovementHeader.BM_CarnetTotalPages = 2;
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_DischargeType = "";
				AssertEquals("VoletPageNumber should be empty when Discharge not mapped", "", Provider.VoletPageNumber);

				nctsHeader.ArrivalMovementHeader.BM_DischargeType = "A";
				AssertEquals("VoletPageNumber should be 2", "2", Provider.VoletPageNumber);
			});
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

		protected override string MessageType => Constants.MessageTypes.CC007C;

		protected override string MovementType => NctsMovementType.Codes.Arrival;
	}
}
