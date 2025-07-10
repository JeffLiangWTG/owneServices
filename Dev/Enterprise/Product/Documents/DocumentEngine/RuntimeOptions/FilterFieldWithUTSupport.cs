using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class FilterFieldWithUTSupport : FilterField
	{
		public FilterFieldWithUTSupport(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected FilterFieldWithUTSupport(BaseFieldJsonData data) : base(data)
		{
		}

		#region DEBUG ONLY
#if DEBUG
		public abstract void ClearValueForUnitTest();
#endif
		#endregion
	}
}
