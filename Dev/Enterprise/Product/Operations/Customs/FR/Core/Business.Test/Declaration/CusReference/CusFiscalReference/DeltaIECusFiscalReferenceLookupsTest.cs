using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIECusFiscalReferenceLookupsTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			AssertEquals("The code list for UCC6 should feature EU codes + FR7.", "FR1, FR2, FR3, FR4, FR5, FR7", cusFiscalReference.Lookups.CodeList.CodesAsString);

			var declaration2 = Factory.New<JobDeclaration>();
			var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			var cusFiscalReference2 = entryInstruction2.FiscalReferences.AddNew();
			AssertEquals("The code list for non-UCC6 should feature EU codes.", "FR1, FR2, FR3, FR4, FR5", cusFiscalReference2.Lookups.CodeList.CodesAsString);
		}
	}
}
