using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonMRNWrapperTest : WrapperHelperTest<ImportH1CommonMRNWrapper>
{
	public void TestMRN()
	{
		entryHeader.MovementReferenceNumber = "TEST001JPB";
		AssertEquals("Expected filled MRN", "TEST001JPB", wrapper.MRN);
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader = Factory.New<CusEntryHeader>();
		wrapper = GetWrapper(entryHeader);
	}
	CusEntryHeader entryHeader;
	ImportH1CommonMRNWrapper wrapper;

	ImportH1CommonMRNWrapper GetWrapper(CusEntryHeader entryHeader) => new ImportH1CommonMRNWrapper(entryHeader);

	protected override ImportH1CommonMRNWrapper GetProvider() => wrapper;
}
