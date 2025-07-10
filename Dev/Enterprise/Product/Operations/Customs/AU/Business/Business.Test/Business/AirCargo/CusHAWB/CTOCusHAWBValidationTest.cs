using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusHAWBValidationTest : CusHAWBValidationAbstractTest
	{
		public override void TestValidateHAWB()
		{
			HAWB.CS_HAWB = "1234";
			AssertHasMessageError(HAWB.CS_HAWBInfo, "MAWB should be at least 11 characters long.");

			HAWB.CS_HAWB = "";
			AssertHasMessageError(HAWB.CS_HAWBInfo, "MAWB is required for messaging");

			HAWB.CS_HAWB = "123456789012353";
			AssertNoMessageErrors(HAWB.CS_HAWBInfo);
		}

		public void TestCheckCS_IsSelfAssessedClearance()
		{
			HAWB.MAWB.CM_RL_NKDischargePort = "";
			HAWB.CS_IsSelfAssessedClearance = true;
			AssertNoMessageErrors(HAWB.CS_IsSelfAssessedClearanceInfo);

			HAWB.MAWB.CM_RL_NKDischargePort = "AUSYD";
			HAWB.CS_IsSelfAssessedClearance = true;
			AssertNoMessageErrors(HAWB.CS_IsSelfAssessedClearanceInfo);

			HAWB.MAWB.CM_RL_NKDischargePort = "USLAX";
			HAWB.CS_IsSelfAssessedClearance = true;
			AssertHasMessageError(HAWB.CS_IsSelfAssessedClearanceInfo, "SAC is not valid for transit cargo");

			HAWB.CS_IsSelfAssessedClearance = false;
			AssertNoMessageErrors(HAWB.CS_IsSelfAssessedClearanceInfo);
		}

		public void TestCheckCS_RL_NKLoadPort()
		{
			RefUNLOCO port = GetNewPort();

			HAWB.Validation.ValidateCS_RL_NKLoadPort();
			AssertHasMessageErrors("by default", HAWB.CS_RL_NKLoadPortInfo);

			HAWB.CS_RL_NKLoadPort = "~~~";
			AssertHasMessageErrors("when invalid", HAWB.CS_RL_NKLoadPortInfo);

			HAWB.CS_RL_NKLoadPort = port.RL_Code;
			AssertNoNotifications("when entered", HAWB.CS_RL_NKLoadPortInfo);

			port.RL_HasAirport = false;
			HAWB.CS_RL_NKLoadPort = port.RL_Code;
			AssertHasWarnings("when not air port", HAWB.CS_RL_NKLoadPortInfo);
		}

		public void TestCheckCS_Weight()
		{
			HAWB.Validation.ValidateCS_Weight();
			AssertHasMessageErrors("by default", HAWB.CS_WeightInfo);

			HAWB.CS_Weight = 1;
			AssertNoNotifications("when valid", HAWB.CS_WeightInfo);

			HAWB.CS_Weight = -1;
			AssertHasMessageErrors("when invalid", HAWB.CS_WeightInfo);
		}

		public void TestCheckCS_WeightUQ()
		{
			HAWB.Validation.ValidateCS_WeightUQ();
			AssertHasMessageErrors("by default", HAWB.CS_WeightUQInfo);

			HAWB.CS_WeightUQ = CMRGrossWeightCodes.Codes.Kilograms;
			AssertNoNotifications("when valid", HAWB.CS_WeightUQInfo);

			HAWB.CS_WeightUQ = "~~";
			AssertHasMessageErrors("when invalid", HAWB.CS_WeightUQInfo);
		}

		public void TestCheckCS_GoodsValue()
		{
			HAWB.Validation.ValidateCS_GoodsValue();
			AssertHasWarnings("by default", HAWB.CS_GoodsValueInfo);

			HAWB.CS_GoodsValue = 1;
			AssertNoNotifications("when entered", HAWB.CS_GoodsValueInfo);

			HAWB.CS_GoodsValue = -1;
			AssertHasErrors("when invalid", HAWB.CS_GoodsValueInfo);
		}

		public void TestCheckCS_RX_NKGoodsCurrency()
		{
			HAWB.Validation.ValidateCS_RX_NKGoodsCurrency();
			AssertNoNotifications("by default", HAWB.CS_RX_NKGoodsCurrencyInfo);

			HAWB.CS_GoodsValue = 1;
			HAWB.Validation.ValidateCS_RX_NKGoodsCurrency();
			AssertHasMessageErrors("when goods value is positive", HAWB.CS_RX_NKGoodsCurrencyInfo);

			HAWB.CS_RX_NKGoodsCurrency = (Factory.NewWithValidTestData<RefCurrency>()).RX_Code;
			AssertNoNotifications("when valid", HAWB.CS_RX_NKGoodsCurrencyInfo);

			HAWB.CS_RX_NKGoodsCurrency = "~~";
			AssertHasErrors("when invalid", HAWB.CS_RX_NKGoodsCurrencyInfo);
		}

		public void TestCheckCS_PiecesManifested()
		{
			HAWB.Validation.ValidateCS_PiecesManifested();
			AssertHasMessageErrors("by default", HAWB.CS_PiecesManifestedInfo);

			HAWB.CS_PiecesManifested = 1;
			AssertNoNotifications("when set", HAWB.CS_PiecesManifestedInfo);
		}

		public void TestValidateFreightPrepaidCollect_Mandatory()
		{
			const string mandatoryMessage = "Method of Payment is required.";
			CombineAssertions(() =>
			{
				HAWB.Validation.ValidateCS_FreightPrepaidCollect();
				AssertHasMessageError("Mandatory", HAWB.CS_FreightPrepaidCollectInfo, mandatoryMessage);

				HAWB.CS_FreightPrepaidCollect = "~~";
				AssertNoMessageError("Entered", HAWB.CS_FreightPrepaidCollectInfo, mandatoryMessage);
			});
		}

		protected override CusHAWBBase GetNewHAWB()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			return mawb.ChildBills.AddNew();
		}
	}
}
