using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using IMessageManageableBizObj = Enterprise.Customs.Business.IMessageManageableBizObj;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoMasterFormAmendmentCheckerTest : Customs.GUI.Testing.AmendmentDetectionOnSavingBaseTest
	{
		protected override IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend() => GetAmendableMAWB();

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager() => GetAmedableMAWBWithChanges(false);

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors()
		{
			var mawb = GetAmedableMAWBWithChanges(true);
			mawb.ChildBills[0].CS_GoodsDescription = "";
			return mawb;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager()
		{
			var mawb = GetAmedableMAWBWithChanges(true);
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "";
			return mawb;
		}

		protected override IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj) => new AirCargoMasterForm((CusMAWB)bizObj);

		CusMAWB GetAmendableMAWB(bool shouldSetUpCertificate)
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
			ZTestHelper helper = new ZTestHelper(Factory);
			CusHAWB hAWB = helper.CreateTestHouseBill();
			CusMAWB mAWB = hAWB.MAWB;
			hAWB.CS_JS = ZGuid.Empty;
			mAWB.CM_JK = ZGuid.Empty;
			mAWB.CM_HouseMessageIsSent = true;
			hAWB.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			if (shouldSetUpCertificate)
			{
				helper.SetupCertificates();
			}

			Factory.Save();
			return mAWB;
		}

		CusMAWB GetAmendableMAWB() => GetAmendableMAWB(true);

		CusMAWB GetAmedableMAWBWithChanges(bool shouldSetUpCertificate)
		{
			CusMAWB mawb = GetAmendableMAWB(shouldSetUpCertificate);
			CusHAWB hawb = mawb.ChildBills[0];
			hawb.CS_GoodsDescription = "changed changed";
			return mawb;
		}
	}
}
