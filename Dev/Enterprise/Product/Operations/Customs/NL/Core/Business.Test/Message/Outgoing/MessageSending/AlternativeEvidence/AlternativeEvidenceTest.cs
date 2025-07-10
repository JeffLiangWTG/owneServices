using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(AlternativeEvidence))]
sealed class AlternativeEvidenceTest : NonPersistentBusinessObjectTestCase
{
	public void TestDocTypeDescription()
	{
		CombineAssertions(() =>
		{
			alternativeEvidence.DocType = "11";
			AssertEquals("Valid", "KOPIE PAKBON ONDERTEKEND OF GEWAARMERKT DOOR GEADRESSEERDE", alternativeEvidence.DocTypeDescription);

			alternativeEvidence.DocType = "14";
			AssertEquals("Valid-Another value", "PAKBON ONDERTEKEND OF GEWAARMERKT DOOR MARKTDEELNEMER", alternativeEvidence.DocTypeDescription);

			alternativeEvidence.DocType = "XX";
			AssertEquals("Invalid", "", alternativeEvidence.DocTypeDescription);
		});
	}

	public void TestDocTypeMaxLength()
	{
		AssertEquals(2, alternativeEvidence.DocTypeInfo.MaxLength);
	}

	public void TestLookups() => AssertType<AlternativeEvidenceLookups>(new AlternativeEvidence(jobDeclarationMessageSendingObject).Lookups);

	protected override BusinessObject GetNewBusinessObject() => new AlternativeEvidence(jobDeclarationMessageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		jobDeclarationMessageSendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		alternativeEvidence = new AlternativeEvidence(jobDeclarationMessageSendingObject);
	}

	JobDeclarationMessageSendingObject jobDeclarationMessageSendingObject;
	AlternativeEvidence alternativeEvidence;
}
