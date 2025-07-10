using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPEUtilityTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		[TestDate(2016, 3, 7, 16, 15, 19, 123)]
		public void TestGetDuplicateShipmentQuery()
		{
			string shortNumber = "4AX585GPRZU";
			UPECusMAWB upeCusMAWB = Factory.New<UPECusMAWB>();
			upeCusMAWB.CM_MAWB = "08133333333";
			UPECusHAWB outDateCusHAWB = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			outDateCusHAWB.CS_HAWB = "1Z4AX5856644068241";
			outDateCusHAWB.WayBillShort = shortNumber;
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2016, 6, 8, 16, 15, 19, 123).ToDateTime();
			UPECusHAWB noMAWBCusHAWB = Factory.New<UPECusHAWB>();
			noMAWBCusHAWB.CS_HAWB = "1Z4AX5856644068242";
			noMAWBCusHAWB.WayBillShort = shortNumber;
			UPECusHAWB childShortCusHAWB = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			childShortCusHAWB.CS_HAWB = "1Z4AX5856644068243";
			JobRelatedWayBill wayBill = Factory.NewWithValidTestData<JobRelatedWayBill>(TestBusinessObjectKind.MinimumRequiredToSave);
			wayBill.EB_ParentID = childShortCusHAWB.PK;
			wayBill.EB_WaybillShortNumber = shortNumber;
			wayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			UPECusHAWB upeCusHAWB = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			upeCusHAWB.CS_HAWB = "1Z4AX5856644068244";
			upeCusHAWB.WayBillShort = shortNumber;
			Factory.Save();
			UPECusHAWB[] upeCusHAWBs = Factory.Load<UPECusHAWB>(UPEUtility.GetDuplicateShipmentQuery(shortNumber));
			AssertEquals("upeCusHAWBs.Length", 1, upeCusHAWBs.Length);
			AssertEquals("upeCusHAWBs[0].CS_HAWB", "1Z4AX5856644068244", upeCusHAWBs[0].CS_HAWB);
		}
	}
}
