using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SADPreviousAdministrativeReferenceWrapperTest : TestCaseWithFactory
{
	public void TestSADPreviousAdministrativeReferenceWrapper()
	{
		wrapper = new SADPreviousAdministrativeReferenceWrapper("A3", "1", "A", new ZDate(2020, 01, 01), "X", "IT137100", 1);
		CombineAssertions("SAD Previous Administrative Reference", () =>
		{
			AssertEquals("Register should be", "A3", wrapper.Register);
			AssertEquals("Reference should be", "1", wrapper.ReferenceNumber);
			AssertEquals("ReferenceCIN should be", "A", wrapper.ReferenceCIN);
			AssertEquals("Date should be", new ZDate(2020, 01, 01), wrapper.Date);
			AssertEquals("Series should be", "X", wrapper.Series);
			AssertEquals("CustomsOffice should be", "IT137100", wrapper.CustomsOffice);
			AssertEquals("ItemNumber should be", 1, wrapper.ItemNumber);
		});
	}

	public virtual void TestCustomsOffice()
	{
		wrapper = new SADPreviousAdministrativeReferenceWrapper("A3", "1", "A", new ZDate(2020, 01, 01), "X", "IT137100", 1);
		AssertEquals("CustomsOffice", "IT137100", wrapper.CustomsOffice);
	}

	public virtual void TestItemNumber()
	{
		wrapper = new SADPreviousAdministrativeReferenceWrapper("A3", "1", "A", new ZDate(2020, 01, 01), "X", "IT137100", 1);
		AssertEquals("CustomsOffice", 1, wrapper.ItemNumber);

		wrapper = new SADPreviousAdministrativeReferenceWrapper("A3", "1", "A", new ZDate(2020, 01, 01), "X", "IT137100", 0);
		AssertEquals("CustomsOffice", 0, wrapper.ItemNumber);
	}

	public void TestEmptySADPreviousAdministrativeReferenceWrapper()
	{
		wrapper = SADPreviousAdministrativeReferenceWrapper.Empty();
		CombineAssertions("Empty SAD Previous Administrative Reference", () =>
		{
			AssertEquals("Register should be", "", wrapper.Register);
			AssertEquals("Reference should be", "", wrapper.ReferenceNumber);
			AssertEquals("ReferenceCIN should be", "", wrapper.ReferenceCIN);
			AssertEquals("Date should be", ZDate.Empty, wrapper.Date);
			AssertEquals("Series should be", "", wrapper.Series);
			AssertEquals("CustomsOffice should be", "", wrapper.CustomsOffice);
			AssertNull("ItemNumber should be", wrapper.ItemNumber);
		});
	}

	SADPreviousAdministrativeReferenceWrapper wrapper;
}
