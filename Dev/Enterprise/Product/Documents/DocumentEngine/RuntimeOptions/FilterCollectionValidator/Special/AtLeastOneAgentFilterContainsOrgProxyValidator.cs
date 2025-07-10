using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class AtLeastOneAgentFilterContainsOrgProxyValidator : AtLeastOneFilterNotEmptyValidator
	{
		protected override bool IsAdditionalValidationSuccessful(FilterField filterToValidate)
		{
			return FilterValidationHelper.FilterContainsOnlyAllowed(filterToValidate, GlbCompany.CurrentCompany.OrgProxy, GlbBranch.CurrentBranch.OrgProxy);
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			string result = (Filters.Count == 1 ? "'" + Filters[0].DisplayNameLocalized + "'" : Res.GetString("7fdf8c0b-5d61-4379-93fc-aace9f7fb902", "At least one of the {0}", FieldNamesJoined));

			result += " " + Res.GetString("ebec4515-7b12-45d2-b03e-8981b093630e",
				"should be set to only the current login company's organization proxy ({0}){1}. This is required because you have not been granted the security right ({2}).",
				GlbCompany.CurrentCompany.OrgProxy.OH_Code,
				GlbBranch.CurrentBranch.OrgProxy == null ? "" : " " + Res.GetString("6931c924-4704-40f6-8804-45be4a29c675", "or current login branch's organization proxy ({0})", GlbBranch.CurrentBranch.OrgProxy.OH_Code),
				Env.Security.ReportsExternalAgentsFilter.DisplayTextPathToSecurityRight);

			return result;
		}
	}
}
