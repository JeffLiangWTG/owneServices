using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FilterCompanyConstraint : IFilterConstraint
	{
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IFilterConstraint.Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FilterConstants.Company; }
		}

		public string SingularValueName
		{
			get { return Res.GetString("589d9d4d-ccb3-4cde-97f6-4b69e33798bc", "company"); }
		}

		public string PluralValueName
		{
			get { return Res.GetString("9e4a9dd2-6e8f-4720-8d9a-a81e7b3fb713", "companies"); }
		}

		public string Description
		{
			get
			{
				return Res.GetString("OperationalActionsFilter|CompanyConstraint|Description",
					"Matches on the company code of the current login company.\r\ne.g. 'Company == \"COM\"' will only match if the code of the current login company is 'COM'.");
			}
		}

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			return company == null ? null : company.GC_Code;
		}
	}
}
