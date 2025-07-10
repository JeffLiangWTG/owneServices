using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillNotifyPartyCollection))]
	sealed class AsycudaBillNotifyPartyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNotifyPartyTypeAndNumberAreSyncedWhenCreatingOrganization()
		{
			var collection = GetCollectionToTest();

			Bill.Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.LPC;
			Bill.ABL_NotifyPartyRegNo = "1234567890123";
			Bill.ABL_NotifyPartyStreet1 = "Test Street";

			var org = Factory.New<OrgHeader>();
			collection.SetupNewElementButDoNotAddIt(org, true);
			AssertEquals(1, org.CustomsCodes.Count);

			var customsCode = org.CustomsCodes.FirstOrDefault() as OrgCusCode;
			AssertEquals(Core.Constants.CountryCodes.Japan, customsCode.OK_RN_NKCodeCountry);
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, customsCode.OK_CodeType);
			AssertEquals("1234567890123", customsCode.SecuredCustomsRegNo);
			AssertEquals("Test Street", customsCode.PremisesAddress.OA_Code);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaBillNotifyPartyCollection(Factory, Bill);

		AsycudaBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					bill = header.Bills.AddNew();
				}
				return bill;
			}
		}
		AsycudaBill bill;
	}
}
