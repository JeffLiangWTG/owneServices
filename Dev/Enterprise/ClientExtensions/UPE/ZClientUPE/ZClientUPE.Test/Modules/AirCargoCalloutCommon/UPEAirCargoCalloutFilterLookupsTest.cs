using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class UPEAirCargoCalloutFilterLookupsTest : TestCaseWithFactory
	{
		#region Code Description Pair Lists
		public void TestQueueNames_List()
		{
			AssertEquals("QueueNames_List", typeof(CommercialQueueCodeDescriptionPairList), Lookups.QueueNames_List.GetType());
		}

		public void TestZoneNameList()
		{
			TestCaseHelper.ClearTable(RateTransportZoneItemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(RateTransportZonesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(RateTransportProviderSchema.Constants.TableName);
			RateTransportProvider decoyRateTransport = Factory.NewWithValidTestData<RateTransportProvider>();
			decoyRateTransport.TP_OH_RelatedParty = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			RateTransportProvider rateTransport1 = Factory.NewWithValidTestData<RateTransportProvider>();
			rateTransport1.TP_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			RateTransportProvider rateTransport2 = Factory.NewWithValidTestData<RateTransportProvider>();
			rateTransport2.TP_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			RateTransportZone decoyZone = decoyRateTransport.Zones.AddNew();
			decoyZone.TZ_ZoneName = "Decoy UPSZone";
			RateTransportZone zone1 = rateTransport1.Zones.AddNew();
			zone1.TZ_ZoneName = "UPSZone1";
			RateTransportZone zone2 = rateTransport1.Zones.AddNew();
			zone2.TZ_ZoneName = "UPSZone2";
			Factory.Save();
			AssertEquals("The correct number of UPS transport zones should be returned", 5, Lookups.ZoneNameList.Count);
			AssertEquals("Metro", UPEFilterConstants.MetroCountry.Metro, Lookups.ZoneNameList[0].Code);
			AssertEquals("Metro", UPEFilterConstants.MetroCountry.Metro, Lookups.ZoneNameList[0].Description);
			AssertEquals("Other", UPEFilterConstants.MetroCountry.Other, Lookups.ZoneNameList[1].Code);
			AssertEquals("Other", UPEFilterConstants.MetroCountry.Other, Lookups.ZoneNameList[1].Description);
			AssertEquals("Separator", "", Lookups.ZoneNameList[2].Code);
			AssertEquals("Separator", "", Lookups.ZoneNameList[2].Description);
			AssertEquals("First zone", "UPSZone1", Lookups.ZoneNameList[3].Code);
			AssertEquals("First zone", "UPSZone1", Lookups.ZoneNameList[3].Description);
			AssertEquals("Second zone", "UPSZone2", Lookups.ZoneNameList[4].Code);
			AssertEquals("Second zone", "UPSZone2", Lookups.ZoneNameList[4].Description);
		}

		#endregion
		#region FindBox Lists
		public void TestTaskAssignedToStaff_List()
		{
			AssertNotNull("TaskAssignedToStaff_List", Lookups.TaskAssignedToStaff_List);
		}

		public void TestPortList()
		{
			AssertNotNull("PortList", Lookups.PortList);
		}

		public void TestServiceLevelList()
		{
			AssertNotNull("ServiceLevelList", Lookups.ServiceLevelList);
		}

		public void TestAccountClassList()
		{
			AssertNotNull("AccountClassList", Lookups.AccountClassList);
		}

		#endregion
		#region Implementation
		class TestUPEAirCargoCalloutFilterLookups : UPEAirCargoCalloutFilterLookups
		{
			public TestUPEAirCargoCalloutFilterLookups(UPEAirCargoFilterBusinessObject filterBizO) : base(filterBizO)
			{
			}

			protected override DefaultQueueCodeDescriptionPairList NewQueueNamesList()
			{
				return new CommercialQueueCodeDescriptionPairList();
			}

			public new UPEAirCargoFilterBusinessObject FilterBizO
			{
				get
				{
					return (UPEAirCargoFilterBusinessObject)base.FilterBizO;
				}
			}
		}

		TestUPEAirCargoCalloutFilterLookups Lookups;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Lookups = new TestUPEAirCargoCalloutFilterLookups(new UPEAirCargoFilterBusinessObject());
		}
		#endregion
	}
}
