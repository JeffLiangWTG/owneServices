using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADSpecialMentionEoriInfoWrapperTest : TestCaseWithFactory
{
	public void TestSADSpecialMentionEoriInfoWrapper()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		wrapper = new SADSpecialMentionEoriInfoWrapper(entryHeader);
		CombineAssertions("EORI", () =>
		{
			AssertEquals("FirstEoriCode should be", "", wrapper.FirstEoriCode);
			AssertEquals("SecondEoriCode should be", "", wrapper.SecondEoriCode);
			AssertNull("PreviousInvoiceAmount should be", wrapper.PreviousInvoiceAmount);
		});
	}

	SADSpecialMentionEoriInfoWrapper wrapper;
}
