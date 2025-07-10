using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AlternativeEvidenceCollection))]
	class AlternativeEvidenceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlternativeEvidenceCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AlternativeEvidence(Factory);
		}

		protected override AlternativeEvidenceCollection GetCollectionToTest()
		{
			return new AlternativeEvidenceCollection(Factory);
		}
	}
}
