using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class OthersProviderTest : DataProviderTestCase<OthersProvider>
	{
		public void TestCalculationOfTheAmountOfTheImportDuty()
		{
			instruction.ZG_Article86_3_UCC = Customs.Business.YesNoList.Codes.Yes;
			AssertEquals("CalculationOfTheAmountOfTheImportDuty", true, Provider.CalculationOfTheAmountOfTheImportDuty);

			instruction.ZG_Article86_3_UCC = Customs.Business.YesNoList.Codes.No;
			var newProvider = new OthersProvider(instruction);
			AssertEquals("CalculationOfTheAmountOfTheImportDuty", false, newProvider.CalculationOfTheAmountOfTheImportDuty);
		}

		public void TestAdditionalInformation()
		{
			instruction.AdditionalInformation = "Add Information";
			AssertEquals("AdditionalInformation", "Add Information", Provider.AdditionalInformation);
		}

		protected override OthersProvider GetProvider() => new OthersProvider(instruction);

		protected override void SetUp()
		{
			base.SetUp();
			instruction = Factory.New<CusEntryInstruction>();
		}

		CusEntryInstruction instruction;
	}
}
