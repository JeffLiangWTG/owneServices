using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocPackingGroupCollection))]
	sealed class DocPackingGroupCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<DocPackingGroupCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var hBLContainerMultiPack = Factory.New<PackingGroup>();
			return DocPackingGroup.New(hBLContainerMultiPack, Factory);
		}

		protected override DocPackingGroupCollection GetCollectionToTest()
		{
			return new DocPackingGroupCollection(Factory);
		}
	}
}
