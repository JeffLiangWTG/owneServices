using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageBillWrapperTest : DocBaseWrapperTest
{
	public void TestMrn()
	{
		AssertEquals("Mrn", "", Wrapper.Mrn);

		var entryNumber = CusEntryNumber.LoadOrCreate(bill, "MRN", "IT");
		entryNumber.CE_EntryNum = "MRN123";
		AssertEquals("Mrn", "MRN123", Wrapper.Mrn);
	}

	new TemporaryStorageBillWrapper Wrapper => (TemporaryStorageBillWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStorageBillWrapper.New(bill, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<TemporaryStorageHeader>();
		bill = header.Bills.AddNew();
	}

	TemporaryStorageBill bill;
}
