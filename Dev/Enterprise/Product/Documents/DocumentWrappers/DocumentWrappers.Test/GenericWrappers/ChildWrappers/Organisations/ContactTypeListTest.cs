using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class ContactTypeListTest : TestCaseWithFactory
	{
		public void TestGetContactType()
		{
			ContactTypeList list = new ContactTypeList();
			AssertEquals("list.GetContactType(ContactTypeList.Codes.LocalTransport)", ContactType.LocalTransport, list.GetContactType(ContactTypeList.Codes.LocalTransport));
			AssertEquals("list.GetContactType(ContactTypeList.Codes.TransitWarehouse)", ContactType.TransitWarehouse, list.GetContactType(ContactTypeList.Codes.TransitWarehouse));
			AssertEquals("list.GetContactType(ContactTypeList.Codes.ExportDepot)", ContactType.ExportDepot, list.GetContactType(ContactTypeList.Codes.ExportDepot));
			AssertEquals("list.GetContactType(ContactTypeList.Codes.All)", ContactType.All, list.GetContactType(ContactTypeList.Codes.All));
			AssertEquals("list.GetContactType('ZXYXYZ')", null, list.GetContactType("ZXYXYZ"));
			AssertEquals("list.GetContactType(ContactTypeList.Codes.VerifiedGrossWeightContact)", ContactType.VerifiedGrossWeightContact, list.GetContactType(ContactTypeList.Codes.VerifiedGrossWeightContact));
			AssertEquals("list.GetContactType(ContactTypeList.Codes.ControllingCustomer)", ContactType.ControllingCustomer, list.GetContactType(ContactTypeList.Codes.ControllingCustomer));
			AssertEquals("list.GetContactType(ContactTypeList.Codes.ControllingAgent)", ContactType.ControllingAgent, list.GetContactType(ContactTypeList.Codes.ControllingAgent));
		}
	}
}
