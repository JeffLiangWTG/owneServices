using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationInstructionDocumentProviderTest : TestCaseWithFactory
	{
		public void TestInstructionDocumentProviderTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var refNumber = declaration.DispatchInstructionNumbers.AddNew();
			refNumber.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;
			refNumber.CE_EntryNum = "FAT00001";

			var instructionsDocument = new DeclarationInstructionDocumentProvider(refNumber);

			AssertEquals("ReferenceTypeCode should be", DispatchInstructionDocumentTypes.Codes._01, instructionsDocument.ReferenceTypeCode);
			AssertEquals("ReferenceNumber should be", "FAT00001", instructionsDocument.ReferenceNumber);
		}
	}
}

