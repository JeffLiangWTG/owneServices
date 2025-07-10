using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBConsigneeConsignorMatchApprovalTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			AssertEquals("Parent should be loaded correctly", hAWB.PK, matchApproval.Parent.PK);
		}

		public void TestParentReference()
		{
			Factory.Save();
			AssertEquals("ParentReference (which populates to P2_Reference) should come from CS_HAWB", "HouseBill", matchApproval.P2_Reference);
		}

		public void TestMasterBill()
		{
			Factory.Save();
			AssertEquals("ParentReference (which is calculated from MasterBill) should come from CS_MAWB", "MasterBill", matchApproval.MasterBill);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName correct", "Organisation Match Approval - MAWB='MasterBill' HAWB='HouseBill'", matchApproval.HumanReadableName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new OrgMatchApproval.Loader(Factory);
			mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "MasterBill";
			hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "HouseBill";
			matchApproval = (CusHAWBConsigneeMatchApproval)loader.LoadOrCreate(hAWB.PK, OrgMatchApprovalType.AirCargoConsignee);
		}

		OrgMatchApproval.Loader loader;
		CusMAWB mAWB;
		CusHAWB hAWB;
		CusHAWBConsigneeMatchApproval matchApproval;
	}
}
