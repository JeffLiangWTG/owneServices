using Enterprise.Customs.NL.Business.Declaration;
namespace Enterprise.Customs.NL.Business.Testing;

sealed class AlternativeEvidenceLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
{
	public void TestDocTypeList()
	{
		var docTypeList = lookups.DocTypeList;
		AssertContains("11, 12, 13, 14, 15, 16, 17", docTypeList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		var jobDeclarationMessageSendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		var alternativeEvidence = new AlternativeEvidence(jobDeclarationMessageSendingObject);
		lookups = alternativeEvidence.Lookups;
	}
	AlternativeEvidenceLookups lookups;
}
