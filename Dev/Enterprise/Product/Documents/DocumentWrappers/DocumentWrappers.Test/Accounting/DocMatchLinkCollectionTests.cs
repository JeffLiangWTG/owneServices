using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocMatchLinkCollection))]
	public class DocMatchLinkCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocMatchLinkCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AccTransactionMatchLink link = Factory.New<AccTransactionMatchLink>();
			return DocMatchLink.New(link, Factory);
		}

		protected override DocMatchLinkCollection GetCollectionToTest()
		{
			return new DocMatchLinkCollection(Factory);
		}
	}
}
