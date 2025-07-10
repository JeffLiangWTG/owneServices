using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocCusContainerCollection))]
	sealed class DocCusContainerCollectionTests : DocBaseCusContainerCollectionTest<DocCusContainerCollection>
	{
		protected override DocCusContainerCollection GetCollectionToTest() => new DocCusContainerCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusContainer = Factory.New<CusContainer>();
			return DocCusContainer.New(cusContainer, Factory);
		}
	}
}
