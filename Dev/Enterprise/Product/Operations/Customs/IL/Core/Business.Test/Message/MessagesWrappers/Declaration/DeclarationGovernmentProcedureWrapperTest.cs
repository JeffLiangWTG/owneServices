using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class DeclarationGovernmentProcedureWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGovernmentProcedure>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationGovernmentProcedureWrapper.NewOrNull(null));
			AssertNotNull(Provider);
		}

		public void TestCurrentCode()
		{
			AssertEquals("1234567", Provider.CurrentCode.Value);
		}

		protected override IDeclarationGovernmentProcedure GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entryInstruction.CEI_FormattedProcedure = "1234567";
			declaration.CustomsEntryInstructions.Add(entryInstruction);
			return DeclarationGovernmentProcedureWrapper.NewOrNull(entryInstruction);
		}
	}
}
