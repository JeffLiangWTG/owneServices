using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using IMessageManageableBizObj = Enterprise.Customs.Business.IMessageManageableBizObj;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoConsolPlugInAmendmentCheckerTest : Customs.GUI.Testing.AmendmentDetectionOnSavingBaseTest
	{
		protected override IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend() => TestDataSetter.CreateAmendableOceanBill(true);

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager() => TestDataSetter.CreateAmendableOceanBillWithChanges(false);

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors()
		{
			var result = TestDataSetter.CreateAmendableOceanBillWithChanges(true);
			result.CB_RL_NKPortOfDischarge = "";
			result.RunPreSaveValidation();
			AssertEquals("No errors", false, result.HasErrors);
			AssertEquals("has message error", true, result.CB_RL_NKPortOfDischargeInfo.HasMessageErrors());
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager()
		{
			var result = TestDataSetter.CreateAmendableOceanBillWithChanges(true);
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "";
			return result;
		}

		protected override IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj)
		{
			var bill = (CusSCAOceanBill)bizObj;
			bill.Consol.RegisterEditableChildObject(bill);
			var plugin = new SeaCargoConsolPlugIn(bill.Consol);
			plugin.Enabled = true;
			return plugin;
		}

		SeaCargoTestDataSetter testDataSetter;
		SeaCargoTestDataSetter TestDataSetter => testDataSetter ?? (testDataSetter = new SeaCargoTestDataSetter(Factory));
	}
}
