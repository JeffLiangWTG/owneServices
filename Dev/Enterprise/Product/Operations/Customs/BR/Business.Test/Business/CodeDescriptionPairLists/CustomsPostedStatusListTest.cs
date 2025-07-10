using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CustomsPostedStatusListTest : TestCase
	{
		public void TestCustomsPostedStatusList()
		{
			AssertContainsExactElementsInExactOrder(new[] { "ACT", "DPD", "DLT", "ACC", "UPD" }, new CustomsPostedStatusList().GetAllCodes());
		}

		public void TestNeedsToSendMessage()
		{
			AssertEquals("ACT", true, new ZString(CustomsPostedStatusList.Codes.Active).NeedsToSendMessage());
			AssertEquals("DPD", true, new ZString(CustomsPostedStatusList.Codes.DeletePending).NeedsToSendMessage());
			AssertEquals("DLT", false, new ZString(CustomsPostedStatusList.Codes.Deleted).NeedsToSendMessage());
			AssertEquals("ACC", false, new ZString(CustomsPostedStatusList.Codes.Accepted).NeedsToSendMessage());
			AssertEquals("UPD", true, new ZString(CustomsPostedStatusList.Codes.UpdatePending).NeedsToSendMessage());
		}

		public void TestIsAccepted()
		{
			AssertEquals("ACT", false, new ZString(CustomsPostedStatusList.Codes.Active).IsAccepted());
			AssertEquals("DPD", false, new ZString(CustomsPostedStatusList.Codes.DeletePending).IsAccepted());
			AssertEquals("DLT", false, new ZString(CustomsPostedStatusList.Codes.Deleted).IsAccepted());
			AssertEquals("ACC", true, new ZString(CustomsPostedStatusList.Codes.Accepted).IsAccepted());
			AssertEquals("UPD", false, new ZString(CustomsPostedStatusList.Codes.UpdatePending).IsAccepted());
		}

		public void TestIsDeleted()
		{
			AssertEquals("ACT", false, new ZString(CustomsPostedStatusList.Codes.Active).IsDeleted());
			AssertEquals("DPD", false, new ZString(CustomsPostedStatusList.Codes.DeletePending).IsDeleted());
			AssertEquals("DLT", true, new ZString(CustomsPostedStatusList.Codes.Deleted).IsDeleted());
			AssertEquals("ACC", false, new ZString(CustomsPostedStatusList.Codes.Accepted).IsDeleted());
			AssertEquals("UPD", false, new ZString(CustomsPostedStatusList.Codes.UpdatePending).IsDeleted());
		}

		public void TestIsDeletePending()
		{
			AssertEquals("ACT", false, new ZString(CustomsPostedStatusList.Codes.Active).IsDeletePending());
			AssertEquals("DPD", true, new ZString(CustomsPostedStatusList.Codes.DeletePending).IsDeletePending());
			AssertEquals("DLT", false, new ZString(CustomsPostedStatusList.Codes.Deleted).IsDeletePending());
			AssertEquals("ACC", false, new ZString(CustomsPostedStatusList.Codes.Accepted).IsDeletePending());
			AssertEquals("UPD", false, new ZString(CustomsPostedStatusList.Codes.UpdatePending).IsDeletePending());
		}

		public void TestIsUpdatePending()
		{
			AssertEquals("ACT", false, new ZString(CustomsPostedStatusList.Codes.Active).IsUpdatePending());
			AssertEquals("DPD", false, new ZString(CustomsPostedStatusList.Codes.DeletePending).IsUpdatePending());
			AssertEquals("DLT", false, new ZString(CustomsPostedStatusList.Codes.Deleted).IsUpdatePending());
			AssertEquals("ACC", false, new ZString(CustomsPostedStatusList.Codes.Accepted).IsUpdatePending());
			AssertEquals("UPD", true, new ZString(CustomsPostedStatusList.Codes.UpdatePending).IsUpdatePending());
		}
	}
}
