using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.AirCargo.Testing
{
	internal partial class UPECusHAWBTest : TestCaseWithFactory
	{
		public void TestCreateHeldForPaymentLogToSendToBISIObsolete_ChargesAboveCODConfirmPaymentThreshold()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(UPEDataRegistry.Instance.CODConfirmPaymentThreshold + 1);

			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should NOT complete commercial queue for this test", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", localHAWB, string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, string.Empty);
		}

		public void TestCreateHeldForPaymentLogToSendToBISIObsolete_ChargesBelowCODConfirmPaymentThreshold_AccountClass10()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New<UPEOrgHeader>().PK;
			localHAWB.Declaration.Importer.CompanyData.OB_OJ_ARDebtorGroup = Factory.New<OrgDebtorGroup>().PK;
			localHAWB.Declaration.Importer.CompanyData.ARDebtorGroup.OJ_Code = "10";

			localHAWB.SetTotalLocalCharges(UPEDataRegistry.Instance.CODConfirmPaymentThreshold + 1);

			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should NOT complete commercial queue for this test", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			AssertLastCommercialQueueChangeLog("Should add the held for payment queue log", localHAWB, string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, string.Empty);
		}
	}
}
