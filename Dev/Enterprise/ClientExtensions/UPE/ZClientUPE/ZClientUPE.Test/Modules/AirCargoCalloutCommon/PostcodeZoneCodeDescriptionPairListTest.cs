using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class PostcodeZoneCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestZoneNameList()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			TestCaseHelper.ClearTable(RateTransportZoneItemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(RateTransportZonesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(RateTransportProviderSchema.Constants.TableName);
			RateTransportProvider decoyRateTransport = Factory.New<RateTransportProvider>();
			decoyRateTransport.TP_OH_RelatedParty = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			RateTransportProvider rateTransport1 = Factory.New<RateTransportProvider>();
			rateTransport1.TP_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			RateTransportProvider rateTransport2 = Factory.New<RateTransportProvider>();
			rateTransport2.TP_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			RateTransportZone decoyZone = decoyRateTransport.Zones.AddNew();
			decoyZone.TZ_ZoneName = "Decoy UPSZone";
			RateTransportZone zone1 = rateTransport1.Zones.AddNew();
			zone1.TZ_ZoneName = "UPSZone1";
			RateTransportZone zone2 = rateTransport1.Zones.AddNew();
			zone2.TZ_ZoneName = "UPSZone2";
			AssertEquals("The correct number of UPS transport zones should be returned", 5, ZoneNameList.Count);
			AssertEquals("Metro", UPEFilterConstants.MetroCountry.Metro, ZoneNameList[0].Code);
			AssertEquals("Metro", UPEFilterConstants.MetroCountry.Metro, ZoneNameList[0].Description);
			AssertEquals("Other", UPEFilterConstants.MetroCountry.Other, ZoneNameList[1].Code);
			AssertEquals("Other", UPEFilterConstants.MetroCountry.Other, ZoneNameList[1].Description);
			AssertEquals("Separator", "", ZoneNameList[2].Code);
			AssertEquals("Separator", "", ZoneNameList[2].Description);
			AssertEquals("First zone", "UPSZone1", ZoneNameList[3].Code);
			AssertEquals("First zone", "UPSZone1", ZoneNameList[3].Description);
			AssertEquals("Second zone", "UPSZone2", ZoneNameList[4].Code);
			AssertEquals("Second zone", "UPSZone2", ZoneNameList[4].Description);
		}

		PostcodeZoneCodeDescriptionPairList ZoneNameList
		{
			get
			{
				return new PostcodeZoneCodeDescriptionPairList(Factory);
			}
		}
	}
}
