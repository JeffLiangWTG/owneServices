using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocContactsCollection))]
	sealed class DocContactsCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocContactsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgContact = Factory.New<OrgContact>();
			return DocContacts.New(orgContact, Factory);
		}

		protected override DocContactsCollection GetCollectionToTest()
		{
			return new DocContactsCollection(Factory);
		}
	}
}
