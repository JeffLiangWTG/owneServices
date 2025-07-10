using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[TestClass]
	[TestExcludeBusinessObjectsAllHaveTestCases]
	class DummyFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}

		public override ZQuery Filter
		{
			get { return new ZQuery(); }
		}
	}
}
