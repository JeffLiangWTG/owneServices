using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class FilterStripBusinessObjectForTest : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			throw new NotImplementedException();
		}

		public string DefinedStatusAllCodeForTest = DefinedStatusAllCode;
		public string DefinedStatusNotSystemCodeForTest = DefinedStatusNotSystemCode;
		public string DefinedStatusSystemCodeForTest = DefinedStatusSystemCode;
	}
}
