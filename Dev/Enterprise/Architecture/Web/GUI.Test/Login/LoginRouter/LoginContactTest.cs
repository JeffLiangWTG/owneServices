using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	sealed class LoginContactTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "barj@marj.com";
			Factory.Save();

			var loginContact = LoginContact.New(contact);
			AssertEquals(contact.OrganisationCode, loginContact.OrganisationCode);
			AssertEquals(contact.WorkingAddressCompanyName, loginContact.OrganisationName);
			AssertEquals(contact.OC_Email, loginContact.Email);
			AssertEquals(contact.OC_IsPrimaryContact.ToString(), loginContact.PrimaryWorkplace);
			Assert(loginContact.LinkedSystems.IsEmpty);
		}
	}
}
