using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIFilteredContactsCollection))]
	class EDIFilteredContactsCollectionTest : BusinessObjectCollectionViewTestCase<EDIFilteredContactsCollection>
	{
		protected override EDIFilteredContactsCollection GetCollectionToTest()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			return org.FilteredContacts;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<EDIOrgContact>();
		}
	}
}
