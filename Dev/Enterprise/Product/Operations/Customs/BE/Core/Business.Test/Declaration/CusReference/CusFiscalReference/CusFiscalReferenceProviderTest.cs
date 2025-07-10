using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class CusFiscalReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetNewLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();

		CombineAssertions(() =>
		{
			AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			AssertType<ImportCusFiscalReferenceLookups>(cusFiscalReference.Lookups);
		});
	}
}
