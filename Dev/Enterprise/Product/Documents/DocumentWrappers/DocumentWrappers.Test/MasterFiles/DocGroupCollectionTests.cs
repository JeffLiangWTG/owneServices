using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocGroupCollection))]
	sealed class DocGroupCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocGroupCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var accGroups = Factory.New<AccGroups>();
			return DocGroup.New(accGroups, Factory);
		}

		protected override DocGroupCollection GetCollectionToTest()
		{
			return new DocGroupCollection(Factory);
		}
	}
}
