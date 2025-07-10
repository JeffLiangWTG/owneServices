using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	class EDIOrgContactsFilterBusinessObject : OrgContactsFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddFlagFilters(filters);
			AddUserAgreementAcceptanceLogFilters(filters);
			return filters;
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var description = Res.GetString("A3FCACC4-1813-4191-83B3-2466918A3B6E", "Has Client Staff");
			var learningCentreUserFilter = filters.AddFlagsFilter("Has Client Staff", new string[] { description },  new GetFlagsQuery[] { HasUserAccountFilter });
			learningCentreUserFilter.MultilingualDescription = ResString.GetMultilingualString("24EBA5F6-EEE0-4486-9BBE-CC0A07EDD3BD", "Has Client Staff");
		}

		ZQuery HasUserAccountFilter(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(EDIOrgContact));
			var userAccountSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, !value);
			query.AddSubQuery(userAccountSubQuery, JoinCondition.And);

			return query;
		}

		void AddUserAgreementAcceptanceLogFilters(ModuleFilterCollection filters)
		{
			var contactFilter = new AddUserAgreementAcceptanceLogModuleFilter(Modules.ClientModuleRegistration.UserAgreementAcceptances, OrgContactSchema.PK, Factory, typeof(EdiUserAgreementAcceptanceLog));
			contactFilter.MultilingualDescription = ResString.GetMultilingualString("31D2046C-8927-4139-8AE8-1DC9D976CEF0", "User Agreement Acceptance Logs (Multiple)");
			filters.AddFilter(contactFilter);
		}
	}
}
