using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FilterCountryConstraint : IFilterConstraint
	{
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IFilterConstraint.Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FilterConstants.Country; }
		}

		public string SingularValueName
		{
			get { return Res.GetString("8fce183d-5470-4823-ab53-806bd3f6261b", "country/region"); }
		}

		public string PluralValueName
		{
			get { return Res.GetString("def8e218-0660-4d50-b060-c39662c3ab18", "countries/regions"); }
		}

		public string Description
		{
			get
			{
				return Res.GetString("OperationalActionsFilter|CountryConstraint|Description",
					"Matches on the country code of the current login company.\r\ne.g. 'Country == \"AU\"' will only match if the current login company is in Australia.");
			}
		}

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			RefCountry country;
			return company == null || (country = company.Country) == null ? null : country.RN_Code;
		}
	}
}
