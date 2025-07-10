using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationDeclarantTypeDefaultingTest : TestCaseWithFactory
{
	public void TestDoNotDefaultJE_DeclarantType_OnJobCreation()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}

	public void TestDoNotDefaultJE_DeclarantType_OnImporterChangeWithMatchingDeclarantEori()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("EOR", "0123456789");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.JE_OH_Importer = orgHeader.PK;
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}

	public void TestDoNotDefaultJE_DeclarantType_OnSupplierChangeWithMatchingDeclarantEori()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("EOR", "0123456789");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.JE_OH_Supplier = orgHeader.PK;
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}

	public void TestDoNotDefaultJE_DeclarantType_OnImporterChangeWithNoMatchingDeclarantEori()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew("EOR", "0123456789");
		var importer = Factory.New<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_OH_Importer = importer.PK;
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}
}
