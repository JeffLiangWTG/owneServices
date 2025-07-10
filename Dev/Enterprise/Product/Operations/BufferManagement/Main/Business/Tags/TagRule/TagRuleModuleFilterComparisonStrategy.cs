using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class TagRuleModuleFilterComparisonStrategy : ModuleFilterComparisonStrategy
	{
		public override int CompareModuleFilters(IModuleFilter filter1, IModuleFilter filter2)
		{
			var result = base.CompareModuleFilters(filter1, filter2);

			if (result == 0)
			{
				result = filter1.OriginalCode.CompareTo(filter2.OriginalCode);
			}

			if (result == 0)
			{
				result = filter1.Query.FilterString.CompareTo(filter2.Query.FilterString);
			}
			return result;
		}
	}
}
