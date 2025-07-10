using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	class DummyBranchDepartmentProvider : IBranchDepartmentProvider, IFactoryProvider
	{
		readonly GlbBranch dummyBranch;
		readonly GlbDepartment dummyDepartment;

		public BusinessObjectFactory Factory { get; }

		internal DummyBranchDepartmentProvider(GlbBranch dummyBranch, GlbDepartment dummyDepartment, BusinessObjectFactory factory = null)
		{
			this.dummyBranch = dummyBranch;
			this.dummyDepartment = dummyDepartment;
			Factory = factory;
		}

		IGlbBranch IBranchDepartmentProvider.GetBranch(BusinessObjectFactory factory)
		{
			return dummyBranch;
		}

		IGlbDepartment IBranchDepartmentProvider.GetDepartment(BusinessObjectFactory factory)
		{
			return dummyDepartment;
		}
	}
}
