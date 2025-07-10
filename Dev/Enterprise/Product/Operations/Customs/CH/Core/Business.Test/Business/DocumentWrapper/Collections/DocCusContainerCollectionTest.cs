using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocCusContainerCollection))]
sealed class DocCusContainerCollectionTests : DocBaseCusContainerCollectionTest<DocCusContainerCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return DocCusContainer.New(Factory.New<CusContainer>(), Factory);
	}

	protected override DocCusContainerCollection GetCollectionToTest()
	{
		return new DocCusContainerCollection(Factory);
	}
}
