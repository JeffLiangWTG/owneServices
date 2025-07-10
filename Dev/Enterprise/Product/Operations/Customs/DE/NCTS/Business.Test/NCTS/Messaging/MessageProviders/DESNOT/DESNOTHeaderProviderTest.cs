using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DESNOTHeaderProvider))]
	sealed class DESNOTHeaderProviderTest : NCTSHeaderProviderAbstractTest<DESNOTHeaderProvider>
	{
		public void TestAuthorisationNumber()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "123456789", Core.Constants.CountryCodes.Germany);
			AssertEquals("123456789", Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_CurrentBranchOrgProxyIsNull()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals(string.Empty, Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_FromRegistry()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0123456789012345678901234"))
			{
				AssertEquals("0123456789012345678901234", Provider.AuthorisationNumber);
			}
		}

		public void TestMRN()
		{
			nctsHeader.ArrivalMrnFromUser = "MRN123";
			AssertEquals("MRN123", Provider.MRN);
		}

		[TestDate(2023, 02, 15, 17, 22, 47)]
		public void TestArrivalNotificationDateAndTime()
		{
			CombineAssertions(() =>
			{
				movementHeader.BM_ArrivalDate = ZDateTime.Empty;
				AssertEquals("BM_ArrivalDate is invalid", ZDateTime.Empty, Provider.ArrivalNotificationDateAndTime);

				movementHeader.BM_ArrivalDate = ZDateTime.Now;
				AssertEquals("BM_ArrivalDate is valid", new DateTime(2023, 02, 15, 16, 22, 47),
					Provider.ArrivalNotificationDateAndTime);
			});
		}

		[TestDate(2023, 02, 15, 17, 22, 47, 123)]
		public void TestArrivalNotificationDateAndTime_MillisecondsRemoved()
		{
			CombineAssertions(() =>
			{
				movementHeader.BM_ArrivalDate = ZDateTime.Now;
				AssertEquals("BM_ArrivalDate with Millisecond part removed", new DateTime(2023, 02, 15, 16, 22, 47), Provider.ArrivalNotificationDateAndTime);
			});
		}

		public void Test_ArrivalNotificationDate_ShouldBeInUTC()
		{
			movementHeader.BM_ArrivalDate = ZDateTime.Now;
			AssertEquals("BM_ArrivalDate is in UTC", DateTimeKind.Utc, Provider.ArrivalNotificationDateAndTime.Kind);
		}

		public void TestIncidentFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BH_ExportFlag is empty", false, Provider.IncidentFlag);

				nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertEquals("BH_ExportFlag is Y", true, Provider.IncidentFlag);

				nctsHeader.BH_ExportFlag = EventFlagList.Codes.No;
				AssertEquals("BH_ExportFlag is N", false, Provider.IncidentFlag);
			});
		}

		public void TestAuthorisations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "C520", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			movementHeader.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			movementHeader.AuthorizationNumber = "Num123";
			var authorisation = Provider.Authorisations.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Type", "C520", authorisation.Type);
				AssertEquals("ReferenceNumber", "Num123", authorisation.ReferenceNumber);
			});
		}

		public void TestAuthorisations_Empty()
		{
			AssertEquals(0, Provider.Authorisations.Count);
		}

		public void TestCustomsOfficeOfDestinationActualReferenceNumber()
		{
			movementHeader.DestinationCustomsOfficeCodeForArrival = "DE000011";
			AssertEquals("DE000011", Provider.CustomsOfficeOfDestinationActualReferenceNumber);
		}

		public void TestTraderAtDestination()
		{
			using (Factory.SetTemporaryCurrentUser(fullName: "UserName"))
			{
				DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = orgHeader.Addresses.AddNew();
				var orgContact = orgHeader.Contacts.AddNew();
				orgContact.OC_ContactName = "ContactName";
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Greece);

				nctsHeader.DestinationTrader.E2_OA_Address = orgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals("Name", "UserName", Provider.TraderAtDestination.Name);
					AssertEquals("EoriNumber", "GR123456789", Provider.TraderAtDestination.EoriNumber);
				});
			}
		}

		public void TestLocationOfGoodsAdditionalIdentifier()
		{
			movementHeader.GoodsLocation.CGL_AdditionalIdentifier = "0000";
			AssertEquals("0000", Provider.LocationOfGoodsAdditionalIdentifier);
		}

		public void TestLocationOfGoodsContact()
		{
			movementHeader.GoodsLocation.Address.E2_Contact = "Jason";
			AssertEquals("Jason", Provider.LocationOfGoodsContact.Name);
		}

		public void TestIncidents()
		{
			nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident1 = nctsHeader.EnRouteIncidents.AddNew();
			incident1.BN_EventCountryCode = Core.Constants.CountryCodes.Germany;
			var incident2 = nctsHeader.EnRouteIncidents.AddNew();
			incident2.BN_EventCountryCode = Core.Constants.CountryCodes.Australia;
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Australia }, Provider.Incidents.Select(x => x.Country));
		}

		public void TestIncidents_IncidentFlagIsFalse()
		{
			nctsHeader.BH_ExportFlag = EventFlagList.Codes.No;
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_EventCountryCode = Core.Constants.CountryCodes.Germany;
			AssertEquals(0, Provider.Incidents.Count);
		}

		protected override DESNOTHeaderProvider GetHeaderProvider() => new DESNOTHeaderProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			movementHeader = nctsHeader.ArrivalMovementHeader;
		}
		NctsHeader nctsHeader;
		NctsArrivalMovementHeader movementHeader;
	}
}
