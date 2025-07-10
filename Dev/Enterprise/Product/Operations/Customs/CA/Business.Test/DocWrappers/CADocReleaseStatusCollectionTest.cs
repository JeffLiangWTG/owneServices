using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocReleaseStatusCollection))]
	sealed class CADocReleaseStatusCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocReleaseStatusCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocReleaseStatus.New(CADocReleaseStatusTest.GetReleaseStatus(Factory), Factory);
		}

		protected override DocReleaseStatusCollection GetCollectionToTest()
		{
			return new DocReleaseStatusCollection(Factory);
		}
	}
}
