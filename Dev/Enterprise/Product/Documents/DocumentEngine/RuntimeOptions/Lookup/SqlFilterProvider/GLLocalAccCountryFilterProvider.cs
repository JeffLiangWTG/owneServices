using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class GLLocalAccCountryFilterProvider : SQLFilterProvider
	{
		public GLLocalAccCountryFilterProvider(LookupFilterFieldBase masterFilter, LookupFilterFieldBase detailFilter)
			: base(masterFilter, detailFilter)
		{
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery filter = new ZQuery();

				CodeLookupField lookupField = MasterFilter as CodeLookupField;
				if (lookupField != null)
				{
					filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, lookupField.Value ?? "");
				}

				return filter;
			}
		}

		public override void ValidateMasterAndDetailFilterTypes() { }
	}
}
