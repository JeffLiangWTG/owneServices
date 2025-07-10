using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(AlternativeEvidenceCollection))]
sealed class AlternativeEvidenceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlternativeEvidenceCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new AlternativeEvidence(new JobDeclarationMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
	}

	protected override AlternativeEvidenceCollection GetCollectionToTest()
	{
		return new AlternativeEvidenceCollection(new JobDeclarationMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
	}
}
