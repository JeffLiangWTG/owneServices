using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCompetitorCollection))]
	sealed class DocCompetitorCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCompetitorCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocCompetitor.New(Factory.NewWithValidTestData<OrgCompetitor>(), Factory);
		}

		protected override DocCompetitorCollection GetCollectionToTest()
		{
			return new DocCompetitorCollection(Factory);
		}
	}
}
