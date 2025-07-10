using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportAmendItemWrapperCollection))]
	sealed class LocalExportAmendItemWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LocalExportAmendItemWrapperCollection>
	{
		protected override LocalExportAmendItemWrapperCollection GetCollectionToTest() => new LocalExportAmendItemWrapperCollection(System.Array.Empty<LocalExportAmendItem>(), AmendedItemType.Header, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new LocalExportAmendItemWrapper(new LocalExportAmendItem(), Factory);
	}
}
