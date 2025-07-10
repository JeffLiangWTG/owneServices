using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class AsycudaBillValidationForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_BillIssueDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_MasterBillIssueDate = ZDate.Empty;
			AssertHasMessageErrorContaining(header.AMA_MasterBillIssueDateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MasterBillIssueDate = ZDate.Today;
			AssertNoMessageErrors(header.AMA_MasterBillIssueDateInfo);
		}

		public void TestCheckABL_RL_NKPortOfDischarge()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_RL_NKPortOfLoading = "COCCC";
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_RL_NKPortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);

			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = "MXCCC";
			port.RL_RW = ZGuid.Empty;

			header.AMA_RL_NKPortOfDischarge = port.RL_Code;
			AssertHasMessageErrorContaining(header.AMA_RL_NKPortOfDischargeInfo, "The selected Discharge Port should have State entered");

			port.RL_RW = ZGuid.BrettsGuid;
			header.AMA_RL_NKPortOfDischarge = port.RL_Code;
			AssertNoNotifications(header.AMA_RL_NKPortOfDischargeInfo);
		}
	}
}
