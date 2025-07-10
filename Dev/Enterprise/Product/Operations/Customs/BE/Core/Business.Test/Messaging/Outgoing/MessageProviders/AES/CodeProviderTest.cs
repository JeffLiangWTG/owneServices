using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CodeProviderTest : Customs.Business.Testing.DataProviderTestCase<CodeProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals("999", Provider.SequenceNumber);
	}

	public void TestCode()
	{
		supplementaryCode.CY_Code = "Code";
		AssertEquals("Code", Provider.Code);
	}

	protected override CodeProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		supplementaryCode = Factory.NewWithValidTestData<SupplementaryCode>();
		provider = new CodeProvider(supplementaryCode, 999);
	}

	SupplementaryCode supplementaryCode;

	CodeProvider provider;
}
