using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class OrgContactAutoCompleteHelperTest : DependentBizOAutoCompleteHelperTest
	{
		#region Overrides

		protected override AutoCompleteHelper GetHelper()
		{
			return new OrgContactAutoCompleteHelper(Factory);
		}

		protected override BusinessObject GetBusinessObject()
		{
			OrgContact orgContact = (OrgContact)base.GetBusinessObject();
			orgContact.OC_OH = ((DependentBizOAutoCompleteHelper)Helper).ParentPK;
			return orgContact;
		}

		protected override void SetUp()
		{
			base.SetUp();
			((DependentBizOAutoCompleteHelper)Helper).ParentPK = Factory.NewWithValidTestData<OrgHeader>().PK;
		}
		#endregion

		#region Test Inactive Contact

		public void TestInactiveContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			OrgContact activeContact = org.Contacts.AddNew();
			activeContact.OC_IsActive = true;
			activeContact.OC_ContactName = "Contact Active";

			OrgContact inActiveContact = org.Contacts.AddNew();
			inActiveContact.OC_IsActive = false;
			inActiveContact.OC_ContactName = "Contact Inactive";

			Factory.Save();

			var autoCompleteHelper = new OrgContactAutoCompleteHelper(null);
			autoCompleteHelper = new OrgContactAutoCompleteHelper(Factory) { ParentPK = org.PK };

			var res = autoCompleteHelper.GetList("Contact");
			AssertEquals("Inactive Contact should not be returned", 1, res.Count);

			AssertEquals("active Contact should contains", true, res[0].Contains("Contact Active"));
		}

		#endregion
	}
}
