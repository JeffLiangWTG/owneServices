using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SupplierPartOwnerFilterProvider : SQLFilterProvider
	{
		public SupplierPartOwnerFilterProvider(LookupFilterFieldBase masterFilter, LookupFilterFieldBase detailFilter) : base(masterFilter, detailFilter)
		{
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery filter = new ZQuery();

				LookupField lookupField = MasterFilter as LookupField;
				if (lookupField != null)
				{
					filter = new ZQuery(OrgHeaderSchema.PK, lookupField.Value);
				}
				return filter;
			}
		}

		public override void ValidateMasterAndDetailFilterTypes() { }
	}
}
