using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals
{
	public static class AutoCompleteHelperGetter
	{
		static readonly object Locker = new object();
		public static AutoCompleteHelper GetDefault(this AutoCompleteHelper helper, WebModuleID moduleID)
		{
			AutoCompleteHelper result = null;
			lock (Locker)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				if (moduleID == WebModuleIDs.Organisation ||
					moduleID == WebModuleIDs.OrganisationTracking ||
					moduleID == WebModuleIDs.OrgConsigneeTracking ||
					moduleID == WebModuleIDs.OrgConsignorTracking ||
					moduleID == WebModuleIDs.OrgSupplierTracking ||
					moduleID == WebModuleIDs.OrgReceivablesTracking)
				{
					result = new OrgHeaderAutoCompleteHelper(factory);
				}
				else if (moduleID == WebModuleIDs.OrgAddress)
				{
					result = new OrgAddressAutoCompleteHelper(factory);
				}
				else if (moduleID == WebModuleIDs.OrgContact)
				{
					result = new OrgContactAutoCompleteHelper(factory);
				}
				else if (moduleID == WebModuleIDs.RefCountry)
				{
					result = new CountryAutoCompleteHelper(factory);
				}
			}
			return result;
		}
	}
}
