using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	class WorkflowFilterStripsHelperJPAFRTest : WorkflowFilterStripsHelperTest
	{
		public void TestSetAlternativeParentColumn()
		{
			var filterBizo = new JPAFRFilterStrip();
			var filter = (TasksModuleFilter)filterBizo.ModuleFilters["Tasks"];
			AssertEquals("AlternativeParentColumn", JPAFRHeaderSchema.JPH_ParentId, filter.AlternativeParentColumn);
		}
	}
}
