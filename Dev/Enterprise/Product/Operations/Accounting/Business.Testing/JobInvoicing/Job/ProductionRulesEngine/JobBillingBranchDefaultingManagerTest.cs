using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class JobBillingBranchDefaultingManagerTest : JobBillingDefaultingManagerTest
	{
		protected override JobBillingDefaultingManager GetDefaultingManager(IFactLoaderProvider factLoaderProvider)
		{
			return new JobBillingBranchDefaultingManager(factLoaderProvider);
		}

		protected override string Context => "JBR";

		protected override Guid PK1 => Branch1.PK.ToGuid();
		protected override Guid PK2 => Branch2.PK.ToGuid();

		GlbBranch Branch1 => branch1 ?? (branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany));
		GlbBranch branch1;

		GlbBranch Branch2 => branch2 ?? (branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany));
		GlbBranch branch2;
	}
}
