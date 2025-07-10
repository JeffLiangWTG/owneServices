using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTask.Test
{
	public class ServiceTaskHelperTest : TestCaseWithFactory
	{
		public void TestGetBranchForEDIServiceTasks()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;

			Factory.Save();
			EDIDataRegistry.Instance.BranchForEDIServiceTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, branch.PK.ToGuid());

			var branchForServiceTask1 = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			AssertEquals(branch.PK, branchForServiceTask1.PK);

			var query = new ZQuery(GlbBranchSchema.GB_IsActive, true);
			query.OrderBy = GlbBranch.Schema.GB_SystemCreateTimeUtc;
			var defaultBranch = Factory.LoadTop1<GlbBranch>(query);
			branch.GB_IsActive = false;
			Factory.Save();

			var branchForServiceTask2 = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			AssertEquals(defaultBranch.PK, branchForServiceTask2.PK);
		}
	}
}
