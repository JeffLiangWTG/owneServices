using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManSlotOrg))]
	public class CusSeaManSlotOrgTest : Customs.Business.Testing.CusSeaManSlotOrgTest
	{
		public new void TestHeader()
		{
			AssertNotNull("nullness", slotOrg.Header);
			AssertEquals("type", typeof(CusSeaManTranHead), slotOrg.Header.GetType());
		}

		public void TestABNOrCCID()
		{
			AssertEquals("by default", ZString.Empty, slotOrg.ABNOrCCID);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			slotOrg.BS_OH_SlotCharterer = org.PK;

			AssertEquals("when org without abn or ccid present.", ZString.Empty, slotOrg.ABNOrCCID);

			org.PrimaryRegistrationNumber.Number = "12345678901";
			AssertEquals("when org with abn and no ccid present", "12345678901", slotOrg.ABNOrCCID);

			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "12345");
			AssertEquals("when org with abn and ccid present", "12345678901", slotOrg.ABNOrCCID);

			org.PrimaryRegistrationNumber.Number = ZString.Empty;
			AssertEquals("when org without abn and ccid present", "12345", slotOrg.ABNOrCCID);
		}

		public void TestABNOrCCIDInfo()
		{
			AssertNotNull("nullness", slotOrg.ABNOrCCIDInfo);
		}

		public void TestSlotChartererWrapper()
		{
			AssertNull("by default is null", slotOrg.SlotChartererWrapper);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			slotOrg.BS_OH_SlotCharterer = org.PK;

			AssertNotNull("not null when org present", slotOrg.SlotChartererWrapper);
			AssertEquals("type", typeof(OrgHeaderWrapper), slotOrg.SlotChartererWrapper.GetType());
		}

		public void TestValidation()
		{
			AssertNotNull("nullness", slotOrg.Validation);
			AssertEquals("type", typeof(CusSeaManSlotOrgValidation), slotOrg.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CusSeaManTranHead tranHead = Factory.NewWithValidTestData<CusSeaManTranHead>();
			return tranHead.SlotCharterers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			tranHead = Factory.NewWithValidTestData<CusSeaManTranHead>();
			slotOrg = tranHead.SlotCharterers.AddNew();
		}

		CusSeaManTranHead tranHead;
		CusSeaManSlotOrg slotOrg;
		#endregion
	}
}
