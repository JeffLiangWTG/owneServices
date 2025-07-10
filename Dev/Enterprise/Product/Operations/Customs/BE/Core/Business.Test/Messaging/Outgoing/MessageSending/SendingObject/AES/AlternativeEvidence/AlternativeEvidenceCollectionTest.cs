using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(AlternativeEvidenceCollection))]
class AlternativeEvidenceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlternativeEvidenceCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => new AlternativeEvidence(Factory);

	protected override AlternativeEvidenceCollection GetCollectionToTest() => new AlternativeEvidenceCollection(Factory);
}
