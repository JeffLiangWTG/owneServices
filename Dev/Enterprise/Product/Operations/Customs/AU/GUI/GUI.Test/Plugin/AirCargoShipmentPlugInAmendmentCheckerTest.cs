using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using IMessageManageableBizObj = Enterprise.Customs.Business.IMessageManageableBizObj;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoShipmentPlugInAmendmentCheckerTest : Customs.GUI.Testing.AmendmentDetectionOnSavingBaseTest
	{
		protected override IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend() => GetAmendableHAWB(true);

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager()
		{
			var result = GetAmendableHAWB(false);
			result.CS_GoodsDescription = "Changed";
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors()
		{
			var result = GetAmendableHAWB(true);
			result.CS_GoodsDescription = "";
			AssertEquals("MessageError", true, result.CS_GoodsDescriptionInfo.HasMessageErrors());
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager()
		{
			var result = GetAmendableHAWB(true);
			result.CS_GoodsDescription = "changed changed";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "";
			return result;
		}

		protected override IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj)
		{
			var hawb = (CusHAWB)bizObj;
			hawb.Shipment.RegisterEditableChildObject(hawb);
			var result = new AirCargoShipmentPlugIn(hawb.Shipment);
			result.Enabled = true;
			return result;
		}

		CusHAWB GetAmendableHAWB(bool shouldSetUpCertificate)
		{
			CusHAWB result = helper.CreateTestHouseBill();
			result.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			if (shouldSetUpCertificate)
			{
				helper.SetupCertificates();
			}

			return result;
		}

		ZTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new ZTestHelper(Factory);
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
		}
	}
}
