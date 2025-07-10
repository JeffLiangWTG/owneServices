using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class ZoneFilterBuilderTest : TestCaseWithFactory
	{
		const string UPSTestZoneName = "UPSZone";
		public void TestAllMetroOrCountryZoneFilter()
		{
			var cusMawb = Factory.NewWithValidTestData<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			var decoyHawb = cusMawb.ChildBills.AddNew();
			var sydneyHawb = cusMawb.ChildBills.AddNew();
			var brisbaneHawb = cusMawb.ChildBills.AddNew();
			var cusHawbNotInAnyPostcodeRange = cusMawb.ChildBills.AddNew();
			cusHawb.CS_ConsigneePostcode = "100";
			decoyHawb.CS_ConsigneePostcode = "150";
			sydneyHawb.CS_ConsigneePostcode = "200";
			brisbaneHawb.CS_ConsigneePostcode = "250";
			cusHawbNotInAnyPostcodeRange.CS_ConsigneePostcode = "300";
			var postcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "100", "100");
			var decoyPostcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "150", "150");
			var sydneyPostcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "200", "200");
			var brisbanePostcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "250", "250");
			postcodeRange.Zone.TZ_ZoneName = "Metro";
			decoyPostcodeRange.Zone.TZ_ZoneName = "Decoy NSW Country";
			sydneyPostcodeRange.Zone.TZ_ZoneName = "Sydney Metro";
			brisbanePostcodeRange.Zone.TZ_ZoneName = "MetroQLD";
			Factory.Save();
			AssertMatches("All metro", UPEFilterConstants.MetroCountry.Metro, cusHawb, sydneyHawb, brisbaneHawb);
			Factory.Save();
			AssertMatches("All country", UPEFilterConstants.MetroCountry.Other, decoyHawb, cusHawbNotInAnyPostcodeRange);
		}

		public void TestZoneFilterWithPostcodeManuallyEnteredOnHawb()
		{
			CusMAWB mawb = Factory.New<CusMAWB>();
			CusHAWB lowerBoundHawb = mawb.ChildBills.AddNew(typeof(CusHAWB));
			CusHAWB upperBoundHawb = mawb.ChildBills.AddNew(typeof(CusHAWB));
			lowerBoundHawb.CS_ConsigneePostcode = "300";
			upperBoundHawb.CS_ConsigneePostcode = "2000";
			TestZoneFilter(lowerBoundHawb, upperBoundHawb);
		}

		public void TestZoneFilterWithPostcodeFromLinkedConsigneeOrg()
		{
			CusMAWB mawb = Factory.New<CusMAWB>();
			CusHAWB lowerBoundHawb = mawb.ChildBills.AddNew(typeof(CusHAWB));
			CusHAWB upperBoundHawb = mawb.ChildBills.AddNew(typeof(CusHAWB));
			lowerBoundHawb.CS_OA_ConsigneeAddress = CreateOrganisationWithPostcode("300").MainAddress.PK;
			lowerBoundHawb.CS_ConsigneePostcode = "1"; // decoy
			upperBoundHawb.CS_OA_ConsigneeAddress = CreateOrganisationWithPostcode("2000").MainAddress.PK;
			upperBoundHawb.CS_ConsigneePostcode = "1000000"; // decoy
			TestZoneFilter(lowerBoundHawb, upperBoundHawb);
		}

		void TestZoneFilter(CusHAWB lowerBoundHawb, CusHAWB upperBoundHawb)
		{
			RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, Factory.NewWithValidTestData(typeof(OrgHeader)).PK, "1", "1000000");
			var postcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "300", "2000");
			Factory.Save();
			AssertMatches("Both HAWBs should match", UPSTestZoneName, lowerBoundHawb, upperBoundHawb);
			var tempPostCode = Factory.New<RefPostCode>();
			tempPostCode.RK_CityTownPostCode = "199Z";
			postcodeRange.TQ_ToPostCode = tempPostCode.RK_CityTownPostCode;
			Factory.Save();
			AssertMatches("Only the HAWB with the postcode lower bound should match", UPSTestZoneName, lowerBoundHawb);
			tempPostCode = Factory.New<RefPostCode>();
			tempPostCode.RK_CityTownPostCode = "30A";
			postcodeRange.TQ_FromPostCode = tempPostCode.RK_CityTownPostCode;
			tempPostCode = Factory.New<RefPostCode>();
			tempPostCode.RK_CityTownPostCode = "2000";
			postcodeRange.TQ_ToPostCode = tempPostCode.RK_CityTownPostCode;
			Factory.Save();
			AssertMatches("Only the HAWB with the postcode upper bound should match", UPSTestZoneName, upperBoundHawb);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		OrgHeader CreateOrganisationWithPostcode(ZString postcode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.MainAddress.OA_PostCode = postcode;

			OrgAddress decoyAddress = result.Addresses.AddNew();
			decoyAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			decoyAddress.OA_Address1 = "Address1";
			decoyAddress.OA_PostCode = "1"; // decoy address
			AssertEquals("This newly created office address should not be the default", false, decoyAddress.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
			return result;
		}

		void AssertMatches(string message, string zoneName, params CusHAWB[] expectedMatches)
		{
			ZoneFilterBuilder builder = new ZoneFilterBuilder(typeof(CusHAWB), zoneName, CusHAWBSchema.CS_ConsigneePostcode);
			AssertMatches(message, builder, expectedMatches);
		}

		void AssertMatches(string message, ZoneFilterBuilder builder, params CusHAWB[] expectedMatches)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusHAWB));
			builder.AddToFilter(query);
			CusHAWB[] matches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			AssertEquals(message + "; Correct number of matches", expectedMatches.Length, matches.Length);
			foreach (CusHAWB expectedMatch in expectedMatches)
			{
				AssertEquals(message + "; Should match on the correct CusHAWB(s)", true, ArrayContainsPK(matches, expectedMatch));
			}
		}

		bool ArrayContainsPK(BusinessObject[] list, BusinessObject expectedItemInList)
		{
			foreach (BusinessObject next in list)
			{
				if (next.PK == expectedItemInList.PK)
				{
					return true;
				}
			}

			return false;
		}
	}
}
