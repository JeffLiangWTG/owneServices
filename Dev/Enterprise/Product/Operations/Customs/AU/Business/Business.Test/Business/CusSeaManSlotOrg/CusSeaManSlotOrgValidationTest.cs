using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManSlotOrgValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateABNOrCCID()
		{
			slotOrg.Validation.ValidateAll();
			AssertHasMessageErrors("by default", slotOrg.ABNOrCCIDInfo);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			slotOrg.BS_OH_SlotCharterer = org.PK;

			AssertHasMessageErrors("when org without abn or ccid present.", slotOrg.ABNOrCCIDInfo);

			org.PrimaryRegistrationNumber.Number = "12345678901";
			slotOrg.BS_OH_SlotCharterer = org.PK;
			AssertNoNotifications("when org with abn present", slotOrg.ABNOrCCIDInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CusSeaManTranHead tranHead = Factory.NewWithValidTestData<CusSeaManTranHead>();
			slotOrg = tranHead.SlotCharterers.AddNew();
		}

		CusSeaManSlotOrg slotOrg;

		#endregion
	}
}
