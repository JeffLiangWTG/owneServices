using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(EntryInstructionProvider))]
sealed class EntryInstructionProviderTest : TestCaseWithFactory
{
	public void TestCustomsEntryInstructions()
	{
		var declaration = Factory.New<JobDeclaration>();
		var provider = new EntryInstructionProvider(declaration);
		AssertType<CusEntryInstructionCollection>(provider.CustomsEntryInstructions);
	}
}
