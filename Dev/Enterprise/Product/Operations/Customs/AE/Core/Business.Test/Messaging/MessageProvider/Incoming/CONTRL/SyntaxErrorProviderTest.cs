namespace Enterprise.Customs.AE.Business.Testing;

sealed class SyntaxErrorProviderTest : Customs.Business.Testing.DataProviderTestCase<SyntaxErrorProvider, ISyntaxErrorProvider>
{
	public void TestSyntaxErrorCode() => AssertEquals(ExpectedSyntaxErrorCode, Provider.SyntaxErrorCode);

	public void TestErrorDataElementPosition() => AssertEquals(ExpectedDataElementPosition, Provider.ErrorDataElementPosition);

	public void TestErrorDataElementComponentPosition() => AssertEquals(ExpectedComponentPosition, Provider.ErrorDataElementComponentPosition);

	protected override SyntaxErrorProvider GetProvider()
	{
		return new SyntaxErrorProvider(ExpectedSyntaxErrorCode, ExpectedDataElementPosition, ExpectedComponentPosition);
	}

	const string ExpectedSyntaxErrorCode = "1";
	const string ExpectedDataElementPosition = "2";
	const string ExpectedComponentPosition = "3";
}
