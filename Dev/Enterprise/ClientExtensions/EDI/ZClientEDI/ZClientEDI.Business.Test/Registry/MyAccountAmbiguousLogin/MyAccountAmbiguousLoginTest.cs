using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(MyAccountAmbiguousLogin))]
	public class MyAccountAmbiguousLoginTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var ambiguousLogin = new MyAccountAmbiguousLogin(new[] { contact1, contact2 });
			AssertContainsExactElementsInAnyOrder(new[] { contact1, contact2 }, ambiguousLogin.Contacts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MyAccountAmbiguousLogin(System.Array.Empty<OrgContact>());
		}
	}
}
