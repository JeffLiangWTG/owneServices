using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class RequiredBranchFilterValidator : FilterCollectionValidator
	{
		public override bool IsValid(FilterField filterToValidate)
		{
			return FilterValidationHelper.FilterContainsOnlyAllowed(filterToValidate, GlbBranch.CurrentBranch);
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			string result = "";

			MultipleSelectionLookup multipleSelectionLookup = filterToValidate as MultipleSelectionLookup;

			if (multipleSelectionLookup != null && multipleSelectionLookup.IsHidden)
			{
				return result;
			}

			if (multipleSelectionLookup != null && multipleSelectionLookup.BindToList.Count > 1)
			{
				result = Res.GetString("0553afae-ac3a-486b-a088-aedfa72f0aa3",
					"'{0}' should have only the login branch. This is required because you have not been granted the security right ({1}).",
					filterToValidate.DisplayNameLocalized, Env.Security.ReportsExternalBranchFilter.DisplayTextPathToSecurityRight);
			}
			else
			{
				result = Res.GetString("e2546f0e-e87b-43b5-96c9-8667db74a745",
					"'{0}' should have the login branch. This is required because you have not been granted the security right ({1}).",
					filterToValidate.DisplayNameLocalized, Env.Security.ReportsExternalBranchFilter.DisplayTextPathToSecurityRight);
			}

			return result;
		}
	}
}
