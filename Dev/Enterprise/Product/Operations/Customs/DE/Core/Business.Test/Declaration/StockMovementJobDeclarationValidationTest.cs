using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class StockMovementJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_TransportMode()
		{
			jobDeclaration.Validation.ValidateJE_TransportMode();
			AssertNoNotifications(jobDeclaration.JE_TransportModeInfo);
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoNotifications(jobDeclaration.JE_RL_NKOriginInfo);
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertNoNotifications(jobDeclaration.JE_RL_NKFinalDestinationInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
		}
		protected JobDeclaration jobDeclaration;
	}
}
