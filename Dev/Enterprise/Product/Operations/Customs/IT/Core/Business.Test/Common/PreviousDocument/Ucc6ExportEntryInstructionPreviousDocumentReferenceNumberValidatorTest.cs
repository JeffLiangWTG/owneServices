using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportEntryInstructionPreviousDocumentReferenceNumberValidatorTest : TestCaseWithFactory
{
	public void TestCheckReferenceNumberFormat()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var previousDocument = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "123";
			AssertNoMessageErrors("For Ucc6 Export", previousDocument.CSI_ReferenceNumberInfo);
		}
	}
}
