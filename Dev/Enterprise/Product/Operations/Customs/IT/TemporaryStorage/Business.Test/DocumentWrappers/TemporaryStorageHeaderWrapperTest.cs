using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageHeaderWrapperTest : DocBaseWrapperTest
{
	public void TestBills()
	{
		AssertType<TemporaryStorageBillWrapperCollection>(Wrapper.Bills);
	}

	new TemporaryStorageHeaderWrapper Wrapper => (TemporaryStorageHeaderWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStorageHeaderWrapper.New(header, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;
}
