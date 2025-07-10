using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Testing
{
	class UPEToolsTest : TestCaseWithFactory
	{
		public void TestLogQueueMovement()
		{
			AssertEquals("should be zero", 0, hawb.Logs.LogsNotInDB.Length);

			string msg = "because i like it";
			UPETools.Instance.LogQueueMovement(hawb.Logs, "blah", msg);
			Factory.Save();

			StmALog log = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, msg));
			AssertNotNull(log);
		}

		public void TestUPECustomisationBranches()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			AssertEquals(0, UPETools.Instance.UPECustomisationBranches(false).Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			AssertEquals(1, UPETools.Instance.UPECustomisationBranches(false).Count);
			AssertEquals(Env.CurrentBranch.PK, UPETools.Instance.UPECustomisationBranches(false).First().PK);

			AssertEquals(3, UPETools.Instance.UPECustomisationBranches(true).Count);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany.CurrentCompany.Branches.Add(branch);
			Factory.Save();
			AssertEquals(4, UPETools.Instance.UPECustomisationBranches(true).Count);

			UPEDataRegistry.Instance.BranchToUseForUPECustomisations = branch.PK.ToGuid();
			Factory.Save();
			AssertEquals(1, UPETools.Instance.UPECustomisationBranches(false).Count);
			AssertEquals(branch.PK, UPETools.Instance.UPECustomisationBranches(false).First().PK);

			AssertEquals(4, UPETools.Instance.UPECustomisationBranches(true).Count);
			AssertEquals(branch.PK, UPETools.Instance.UPECustomisationBranches(true).First().PK);

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			UPEDataRegistry.Instance.EnableUPECustomisationsItem.SetValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var anotherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompany.Branches.Add(anotherCompanyBranch);
			UPEDataRegistry.Instance.BranchToUseForUPECustomisations = anotherCompanyBranch.PK.ToGuid();
			Factory.Save();
			AssertEquals(2, UPETools.Instance.UPECustomisationBranches(false).Count);
		}

		public void TestSendTimeoutEmail()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			UPEDataRegistry.Instance.SftpServerTimeoutItemNotificationGroup = Guid.Empty;
			Factory.Save();
			UPETools.Instance.SendTimeoutEmail("JAY");
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "JLI";
			staff.GS_EmailAddress = "jay.li@whatever.com";
			UPEDataRegistry.Instance.SftpServerTimeoutItemNotificationGroup = group.PK.ToGuid();
			Factory.Save();
			UPETools.Instance.SendTimeoutEmail("JAY");
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Service Task 'JAY' has timed out", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		UPECusHAWB hawb;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			hawb = Factory.NewWithValidTestData<UPECusHAWB>();

			base.SetUp();
		}
	}
}


