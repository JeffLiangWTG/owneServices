using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocCusContainerCollection))]
	sealed class DocCusContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseCusContainer cusContainer = Factory.New<BaseCusContainer>();
			return DocCusContainer.New(cusContainer, Factory);
		}

		protected override DocCusContainerCollection GetCollectionToTest()
		{
			return new DocCusContainerCollection(Factory);
		}
	}
}
