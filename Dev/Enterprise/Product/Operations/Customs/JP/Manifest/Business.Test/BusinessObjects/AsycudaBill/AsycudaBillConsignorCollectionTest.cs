using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillConsignorCollection))]
	sealed class AsycudaBillConsignorCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestShipperTypeAndNumberAreSyncedWhenCreatingOrganization()
		{
			var collection = GetCollectionToTest();

			Bill.Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.ABL_ShipperRegNoType = OrgCusCode.JapanCodeTypes.LPC;
			Bill.ABL_ShipperRegNo = "1234567890123";
			Bill.ABL_ShipperStreet1 = "Test Street";

			var org = Factory.New<OrgHeader>();
			collection.SetupNewElementButDoNotAddIt(org, true);
			AssertEquals(1, org.CustomsCodes.Count);

			var customsCode = org.CustomsCodes.FirstOrDefault() as OrgCusCode;
			AssertEquals(Core.Constants.CountryCodes.Japan, customsCode.OK_RN_NKCodeCountry);
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, customsCode.OK_CodeType);
			AssertEquals("1234567890123", customsCode.SecuredCustomsRegNo);
			AssertEquals("Test Street", customsCode.PremisesAddress.OA_Code);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaBillConsignorCollection(Factory, Bill);

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
