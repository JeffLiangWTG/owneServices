using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBPreviousAdministrativeReferenceWrapperTest : SADPreviousAdministrativeReferenceWrapperTest
{
	public override void TestCustomsOffice()
	{
		var wrapper = new NBPreviousAdministrativeReferenceWrapper("A3", "1", "A", ZDate.Today, "X", "IT137100", 1);
		AssertEquals("CustomsOffice", "137100", wrapper.CustomsOffice);
	}

	public override void TestItemNumber()
	{
		var wrapper = new NBPreviousAdministrativeReferenceWrapper("A3", "1", "A", ZDate.Today, "X", "IT137100", 1);
		AssertEquals("ItemNumber", 1, wrapper.ItemNumber);

		wrapper = new NBPreviousAdministrativeReferenceWrapper("A3", "1", "A", ZDate.Today, "X", "IT137100", 0);
		AssertEquals("ItemNumber", null, wrapper.ItemNumber);
	}
}
