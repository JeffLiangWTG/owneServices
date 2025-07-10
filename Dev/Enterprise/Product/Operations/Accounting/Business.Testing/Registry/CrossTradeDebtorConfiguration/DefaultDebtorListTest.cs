using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class DefaultDebtorListTest : TestCase
	{
		public void TestCodeList()
		{
			var list = new DefaultDebtorList();
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode("PBP"));
			Assert(list.ContainsCode("CBP"));
			Assert(list.ContainsCode("CCP"));
			Assert(list.ContainsCode("CCC"));
		}

		public void TestDescriptionList()
		{
			var list = new DefaultDebtorList();
			AssertEquals(4, list.Count);
			AssertEquals("Prepaid Bill-To Party", list["PBP"].Description);
			AssertEquals("Collect Bill-To Party", list["CBP"].Description);
			AssertEquals("Job's Controlling Customer falling back to Prepaid Bill-To Party", list["CCP"].Description);
			AssertEquals("Job's Controlling Customer falling back to Collect Bill-To Party", list["CCC"].Description);
		}
	}
}
