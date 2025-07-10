using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.Ajax.Testing
{
	sealed class AutoCompleteHelperGetterTest : TestCaseWithFactory
	{
		public void TestAutoCompleteHelperGetDefault()
		{
			AutoCompleteHelper fHelper = new OrgAddressAutoCompleteHelper(Factory);

			fHelper = fHelper.GetDefault(WebModuleIDs.OrganisationTracking);
			Assert(fHelper is OrgHeaderAutoCompleteHelper);
			fHelper = fHelper.GetDefault(WebModuleIDs.OrgAddress);
			Assert(fHelper is OrgAddressAutoCompleteHelper);
			fHelper = fHelper.GetDefault(WebModuleIDs.OrgContact);
			Assert(fHelper is OrgContactAutoCompleteHelper);
			fHelper = fHelper.GetDefault(WebModuleIDs.RefCountry);
			Assert(fHelper is CountryAutoCompleteHelper);
			fHelper = fHelper.GetDefault(WebModuleIDs.OrgReceivablesTracking);
			Assert(fHelper is OrgHeaderAutoCompleteHelper);
		}
	}
}
