using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[TestedType(typeof(ChooseCompanyManager))]
	sealed class ChooseCompanyManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoginContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword("1234");
			Factory.Save();

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_PER = contact1.OC_PER;
			contact2.SetHashedPassword("abcd");

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_WebAccessEnabled = true;
			contact3.SetHashedPassword("8888");

			Factory.Save();

			var appInstance = new ZGlobalForTesting();
			var manager = new ChooseCompanyManager(appInstance, new[] { contact1.PK.ToGuid(), contact2.PK.ToGuid(), contact3.PK.ToGuid(), });
			AssertEquals("Should contain all designated contacts", 3, manager.LoginContacts.Count);
			AssertEquals("Should contain all designated contacts", true, manager.LoginContacts.Contains(contact1));
			AssertEquals("Should contain all designated contacts", true, manager.LoginContacts.Contains(contact2));
			AssertEquals("Should contain all designated contacts", true, manager.LoginContacts.Contains(contact3));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var appInstance = new ZGlobalForTesting();
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			Factory.Save();
			return new ChooseCompanyManager(appInstance, new[] { activeContact.PK.ToGuid() });
		}

		#endregion
	}
}
