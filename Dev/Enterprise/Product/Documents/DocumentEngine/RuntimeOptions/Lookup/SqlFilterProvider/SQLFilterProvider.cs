using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal abstract class SQLFilterProvider
	{
		public SQLFilterProvider(LookupFilterFieldBase masterFilter, LookupFilterFieldBase detailFilter)
		{
			this.DetailFilter = detailFilter;
			this.MasterFilter = masterFilter;
			ValidateMasterAndDetailFilterTypes();
		}

		protected LookupFilterFieldBase MasterFilter;
		protected LookupFilterFieldBase DetailFilter;

		public abstract ZQuery Filter { get; }
		public abstract void ValidateMasterAndDetailFilterTypes();
	}
}
