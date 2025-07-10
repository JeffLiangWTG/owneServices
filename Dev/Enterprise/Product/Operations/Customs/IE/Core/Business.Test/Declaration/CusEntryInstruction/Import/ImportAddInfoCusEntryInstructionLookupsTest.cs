using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportAddInfoCusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcessingProcedureCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("ProcessingProcedureCode", "1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22", instruction.AddInfoLookups.ProcessingProcedureCode.CodesAsString);

			var anotherDeclaration = Factory.New<JobDeclaration>();
			anotherDeclaration.JE_MessageType = "IMP";
			AssertSame("ProcessingProcedureCode: Should cached.", instruction.AddInfoLookups.ProcessingProcedureCode, anotherDeclaration.CustomsEntryInstructions.AddNew().AddInfoLookups.ProcessingProcedureCode);
		}
	}
}
