using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class FilterRuleTestSet
	{
		public string FilterName { get; }
		public Func<StmModuleFilter> FilterGetter { get; }
		public string ExpectedTypeOfFilterStripBusinessObject { get; }

		public FilterRuleTestSet(string filterName, Func<StmModuleFilter> filterGetter, string expectedTypeOfFilterStripBusinessObject)
		{
			FilterName = filterName;
			FilterGetter = filterGetter;
			ExpectedTypeOfFilterStripBusinessObject = expectedTypeOfFilterStripBusinessObject;
		}
	}
}
