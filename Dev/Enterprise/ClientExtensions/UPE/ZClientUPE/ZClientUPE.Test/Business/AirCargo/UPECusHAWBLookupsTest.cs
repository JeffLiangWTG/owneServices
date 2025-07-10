using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECusHAWBLookupsTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestPrepaidCollectList()
		{
			AssertEquals("Invalid CodeDescriptionPairList", typeof(BillingTermsCodeDescriptionPairList), Lookups.PrepaidCollectList.GetType());
			AssertEquals(7, Lookups.PrepaidCollectList.Count);
		}

		public void TestPrepaidCollectListForValidation()
		{
			string expected = new CusHAWBLookups(HouseBill).PrepaidCollectList.CodesAsString;
			AssertEquals(expected, Lookups.PrepaidCollectListForValidation.CodesAsString);
		}

		public void TestShipmentTypeList()
		{
			AssertEquals("Invalid CodeDescriptionPairList", typeof(ShipmentTypeCodeDescriptionPairList), Lookups.ShipmentTypeList.GetType());
			AssertEquals(3, Lookups.ShipmentTypeList.Count);
		}

		public void TestDutyTypeList()
		{
			AssertEquals("Invalid CodeDescriptionPairList", typeof(DutyTypeCodeDescriptionPairList), Lookups.DutyTypeList.GetType());
			AssertEquals(4, Lookups.DutyTypeList.Count);
		}

		public void TestHoldForCollectDepotList()
		{
			AssertEquals("Invalid CodeDescriptionPairList", typeof(HFCDepotCodeDescriptionPairList), Lookups.HoldForCollectDepotList.GetType());
			AssertEquals(6, Lookups.HoldForCollectDepotList.Count);
		}

		UPECusHAWBLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new UPECusHAWBLookups(HouseBill);
				}

				return fLookups;
			}
		}

		UPECusHAWB HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = Factory.New<UPECusHAWB>();
				}

				return fHouseBill;
			}
		}

		UPECusHAWBLookups fLookups;
		UPECusHAWB fHouseBill;
	}
}
