using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDefermentAccountCollection))]
	sealed class DocDefermentAccountCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocDefermentAccountCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => DocDefermentAccount.New(new DefermentAccount(Factory.New<OrgHeader>(), "XXX"), Factory);

		protected override DocDefermentAccountCollection GetCollectionToTest() => new DocDefermentAccountCollection(Factory);
	}
}
