using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	sealed class MasterAndHouseBillTest : TestCaseWithFactory
	{
		public void TestGetMasterAndHouseBillsAsString()
		{
			MasterAndHouseBill bill1 = new MasterAndHouseBill("Master1", "House1");
			MasterAndHouseBill bill2 = new MasterAndHouseBill("Master2", "House2");
			MasterAndHouseBill bill3 = new MasterAndHouseBill("Master3", "");

			AssertEquals("Master1/House1", MasterAndHouseBill.GetMasterAndHouseBillsAsString(new MasterAndHouseBill[] { bill1 }));
			AssertEquals("Master1/House1, Master2/House2", MasterAndHouseBill.GetMasterAndHouseBillsAsString(new MasterAndHouseBill[] { bill1, bill2 }));
			AssertEquals("Master3", MasterAndHouseBill.GetMasterAndHouseBillsAsString(new MasterAndHouseBill[] { bill3 }));
		}
	}
}
