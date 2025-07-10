using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class JobBillingDepartmentDefaultingManagerTest : JobBillingDefaultingManagerTest
	{
		protected override JobBillingDefaultingManager GetDefaultingManager(IFactLoaderProvider factLoaderProvider)
		{
			return new JobBillingDepartmentDefaultingManager(factLoaderProvider);
		}

		protected override string Context => "JDE";
		protected override Guid PK1 => Department1.PK.ToGuid();
		protected override Guid PK2 => Department2.PK.ToGuid();

		GlbDepartment Department1 => department1 ?? (department1 = TestObjectCreator.CreateDepartment("XXX"));
		GlbDepartment department1;

		GlbDepartment Department2 => department2 ?? (department2 = TestObjectCreator.CreateDepartment("XYZ"));
		GlbDepartment department2;
	}
}
