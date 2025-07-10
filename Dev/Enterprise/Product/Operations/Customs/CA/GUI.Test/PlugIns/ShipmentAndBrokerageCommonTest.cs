using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using SupervisorOverrides = Enterprise.Customs.CA.Business.SupervisorOverrides;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ShipmentAndBrokerageCommonTest : Customs.GUI.PlugIn.Testing.BaseShipmentAndBrokerageCommonTest
	{
		[TestDate(2017, 03, 02)]
		public override void TestIsSupervisorApproved()
		{
			var decl = Factory.New<JobDeclaration>();
			decl.JE_EntryAuthorisationDate = new ZDateTime(2017, 03, 02);

			var security = new Security.SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			security.CAAccountingDates.IsAllowed = false;

			var common = new ShipmentAndBrokerageCommon(decl);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				Assert(common.IsSupervisorApproved() == ContinueWithSave.Yes);
				decl.CA_K84AccountingDate = new ZDateTime(2017, 03, 03);
				Factory.Save();
				decl.CA_K84AccountingDate = new ZDateTime(2017, 10, 15);
				var supervisor = (SupervisorOverrides)common.GetSupervisorOverrides();
				Assert(supervisor.AccountDateHasChanged(decl));
				Assert(common.IsSupervisorApproved() == ContinueWithSave.No);
				AssertEquals("You may not change the Accounting date without supervisor approval. No supervisors have been setup so these dates may not be changed. To grant supervisor override capability to a supervisor user go to the users Security Rights->Operate->Customs->Supervisor Override, and grant the CA Accounting dates right.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			security.CAAccountingDates.IsAllowed = true;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				common = new ShipmentAndBrokerageCommon(decl);
				Assert(common.IsSupervisorApproved() == ContinueWithSave.Yes);
			}
		}
	}
}
