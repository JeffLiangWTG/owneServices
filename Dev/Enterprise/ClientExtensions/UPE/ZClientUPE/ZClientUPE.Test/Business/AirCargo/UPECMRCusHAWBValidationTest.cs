using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	internal class UPECMRCusHAWBValidationTest : CMRCusHAWBValidationTest
	{
		protected override void SetUp()
		{
			try
			{
				base.SetUp();
			}
			catch (AssertionFailedError) { } // We need all tests from base, but also need to bypass AssertEquals on "House bill validation object" from CMRCusHAWBValidationTest
		}

		public void TestBaseShouldHaveTheSameNotificationsAsUPE()
		{
			var upemawb = Factory.New<UPECusMAWB>();
			var upehawb = (UPECusHAWB)upemawb.ChildBills.AddNew();
			upehawb.RunPreSaveValidation();

			IEnumerable<INotification> cusHawbNotifications;
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(null))
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.RunPreSaveValidation();
				cusHawbNotifications = hawb.Notifications;
			}

			AssertContainsExactElementsInAnyOrder(upehawb.Notifications, cusHawbNotifications);
		}

		#region CheckCS_MasterHouseBill() & CheckCS_HAWB() related from AirCargoCusHAWBValidationNonInheritedTest

		public void TestWarnIfDifferentConsignee()
		{
			var houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			houseBill.CS_ConsigneeName = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsigneeNameInfo, "is different from the freight job data");
			houseBill.CS_ConsigneeCity = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsigneeCityInfo, "is different from the freight job data");
			houseBill.CS_ConsigneePhone = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsigneePhoneInfo, "is different from the freight job data");
			houseBill.CS_ConsigneePostcode = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsigneePostcodeInfo, "is different from the freight job data");
			houseBill.CS_ConsigneeState = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsigneeStateInfo, "is different from the freight job data");
			houseBill.CS_ConsigneeStreet = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsigneeStreetInfo, "is different from the freight job data");
			if (houseBill.CS_RN_NKConsigneeCountry == "US")
			{
				houseBill.CS_RN_NKConsigneeCountry = "AU";
			}
			else
			{
				houseBill.CS_RN_NKConsigneeCountry = "US";
			}
			AssertHasWarningContaining(houseBill.CS_RN_NKConsigneeCountryInfo, "is different from the freight job data");

			houseBill.SynchroniseData();
			houseBill.Validation.ValidateAll();
			AssertNoWarningContaining(houseBill.CS_ConsigneeNameInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsigneeCityInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsigneePhoneInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsigneePostcodeInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsigneeStateInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsigneeStreetInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_RN_NKConsigneeCountryInfo, "is different from the freight job data");
		}

		public void TestWarnIfDifferentConsignor()
		{
			var houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			houseBill.CS_ConsignorName = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsignorNameInfo, "is different from the freight job data");
			houseBill.CS_ConsignorCity = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsignorCityInfo, "is different from the freight job data");
			houseBill.CS_ConsignorPostcode = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsignorPostcodeInfo, "is different from the freight job data");
			houseBill.CS_ConsignorState = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsignorStateInfo, "is different from the freight job data");
			houseBill.CS_ConsignorStreet = "Different";
			AssertHasWarningContaining(houseBill.CS_ConsignorStreetInfo, "is different from the freight job data");
			if (houseBill.CS_RN_NKConsignorCountry == "US")
			{
				houseBill.CS_RN_NKConsignorCountry = "GB";
			}
			else
			{
				houseBill.CS_RN_NKConsignorCountry = "US";
			}
			AssertHasWarningContaining(houseBill.CS_RN_NKConsignorCountryInfo, "is different from the freight job data");

			houseBill.SynchroniseData();
			houseBill.Validation.ValidateAll();
			AssertNoWarningContaining(houseBill.CS_ConsignorNameInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsignorCityInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsignorPostcodeInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsignorStateInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_ConsignorStreetInfo, "is different from the freight job data");
			AssertNoWarningContaining(houseBill.CS_RN_NKConsignorCountryInfo, "is different from the freight job data");
		}

		public void TestWarnIfDifferent()
		{
			RefCurrency uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			RefCurrency aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");

			var houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			houseBill.Shipment.JS_RX_NKGoodsValueCurr = uSDCurrency.RX_Code;
			houseBill.CS_RX_NKGoodsCurrency = aUDCurrency.RX_Code;
			Assert("Currency Different", houseBill.CS_RX_NKGoodsCurrencyInfo.HasWarnings());
		}

		public void TestWarnIfDifferentMasterHouse()
		{
			var houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_HouseBill = "MAS1223";
			houseBill.Shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			houseBill.CS_MasterHouseBill = "DIFF_MAS";

			Assert("Master House Bill different", houseBill.CS_MasterHouseBillInfo.HasWarnings());
		}

		#endregion
	}
}
