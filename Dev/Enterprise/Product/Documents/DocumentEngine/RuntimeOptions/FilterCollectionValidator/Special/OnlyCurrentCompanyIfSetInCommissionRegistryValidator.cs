using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OnlyCurrentCompanyIfSetInCommissionRegistryValidator : FilterCollectionValidator
	{
		public OnlyCurrentCompanyIfSetInCommissionRegistryValidator()
			: base()
		{
		}

		public override bool IsValid(FilterField filterToValidate)
		{
			if (OnlyAllowCurrentLoginCompany)
			{
				var lookupField = filterToValidate as LookupField;
				if (lookupField != null && lookupField.Value != GlbCompany.CurrentCompany.PK)
				{
					return false;
				}
			}

			return base.IsValid(filterToValidate);
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			if (OnlyAllowCurrentLoginCompany)
			{
				if (filterToValidate is LookupField)
				{
					return
						Res.GetString("88df1ed0-d86c-43b9-85d1-c6d6f62a98aa", "Must be the current login company ({0}) as the registry item '{1}' is currently set to true.",
							GlbCompany.CurrentCompany.GC_Code,
							OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Caption);
				}
			}

			return base.GetErrorMessage(filterToValidate);
		}

		public bool OnlyAllowCurrentLoginCompany
		{
			get { return OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value; }
		}
	}
}
