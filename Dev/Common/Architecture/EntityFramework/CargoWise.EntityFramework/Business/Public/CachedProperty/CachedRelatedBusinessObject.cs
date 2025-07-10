using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class CachedRelatedBusinessObject<T> : RecalculableCachedValue<T>
		where T : BusinessObject
	{
		public CachedRelatedBusinessObject(ZPropertyInfoGuid propertyInfo, GetValueDelegate<T> getRelatedBusinessObjectDelegate)
			: base(getRelatedBusinessObjectDelegate)
		{
			PropertyInfo = propertyInfo;
		}

		readonly ZPropertyInfoGuid PropertyInfo;

		protected override bool NeedRecalc() => base.NeedRecalc() || (value?.IsDeleted ?? false) || PropertyInfo.Value != (value?.PK ?? ZGuid.Empty);
	}
}
