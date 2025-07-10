using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CPQAAttacheeWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			DummyCPQAAttachee dummyAttachee = Factory.New<DummyCPQAAttachee>();

			CPQAAttacheeWrapper wrapper = new CPQAAttacheeWrapper(dummyAttachee);
			AssertEquals("FKName", CusEntryCPDecSchema.ON_ParentID, wrapper.FKColumnInCusEntryCPDecTable);

			AssertNotNull("Questions", wrapper.Questions);
			AssertEquals("Registered as Edifitible child", true, dummyAttachee.IsRegisteredEditableChildObject(wrapper.Questions));

			AssertEquals("SelectionDate", ZDateTime.Today, wrapper.SelectionDate);
		}
	}
}
