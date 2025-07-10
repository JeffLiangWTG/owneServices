using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class FilterStripBusinessObjectForTestWithParamlessConstructor : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for test that uses reflection to look for a signature")]
		public FilterStripBusinessObjectForTestWithParamlessConstructor(object o)
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			throw new NotImplementedException();
		}
	}
}
