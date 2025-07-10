using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container_Wrappers
{
	[TestedType(typeof(DocContainerCollection))]
	internal class DocContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var containerBizO = Factory.New<CommonContainer>();
			return DocContainer.New(containerBizO, Factory);
		}

		protected override DocContainerCollection GetCollectionToTest()
		{
			return new DocContainerCollection(Factory);
		}
	}
}
