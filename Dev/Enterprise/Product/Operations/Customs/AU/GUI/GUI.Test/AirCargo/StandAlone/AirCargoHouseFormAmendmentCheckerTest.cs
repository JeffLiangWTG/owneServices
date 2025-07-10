using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using IMessageManageableBizObj = Enterprise.Customs.Business.IMessageManageableBizObj;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	class AirCargoHouseFormAmendmentCheckerTest : Customs.GUI.Testing.AmendmentDetectionOnSavingBaseTest
	{
		protected override IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend() => GetAmendableHAWB();

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager()
		{
			var result = GetAmendableHAWB(false);
			result.CS_GoodsDescription = "Changed";
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors()
		{
			var result = GetAmendableHAWB();
			result.CS_GoodsDescription = "";
			AssertEquals("MessageError", true, result.CS_GoodsDescriptionInfo.HasMessageErrors());
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager()
		{
			var result = GetAmendableHAWB();
			result.CS_GoodsDescription = "changed changed";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "";
			return result;
		}

		protected override IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj)
		{
			var hawb = (CusHAWB)bizObj;
			if (hawb.Shipment != null)
			{
				hawb.CS_JS = ZGuid.Empty;
			}

			return new AirCargoHouseForm(hawb);
		}

		ZTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new ZTestHelper(Factory);
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
		}

		CusHAWB GetAmendableHAWB(bool shouldSetUpCertificate)
		{
			var result = helper.CreateTestHouseBill();
			result.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();

			if (shouldSetUpCertificate)
			{
				helper.SetupCertificates();
			}
			return result;
		}

		CusHAWB GetAmendableHAWB() => GetAmendableHAWB(true);
	}
}
