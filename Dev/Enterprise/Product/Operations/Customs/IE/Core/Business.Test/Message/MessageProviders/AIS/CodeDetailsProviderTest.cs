using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class CodeDetailsProviderTest : DataProviderTestCase<CodeDetailsProvider>
	{
		public void TestCode()
		{
			instruction.ZG_IdOfGoodCode = "A";
			AssertEquals("Code", "A", Provider.Code);
		}

		public void TestText()
		{
			instruction.IdentificationofGoodsDetails = "Sample";
			AssertEquals("Details", "Sample", Provider.Text);
		}

		protected override CodeDetailsProvider GetProvider() => new CodeDetailsProvider(instruction);

		protected override void SetUp()
		{
			base.SetUp();

			instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_Style = "H1";
		}

		CusEntryInstruction instruction;
	}
}
