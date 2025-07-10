namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Integration;
	using Enterprise.MasterFiles.Business;

	internal class AccPayableOrderHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAPH_Stage()
		{
			AssertEquals("Stage has errors", false, BO.APH_StageInfo.HasErrors());
			BO.APH_Stage = "BLA";
			AssertEquals("Stage has errors", true, BO.APH_StageInfo.HasErrors());
			foreach (ICodeDescription x in BO.APH_Stage_List)
			{
				BO.APH_Stage = x.Code;
				AssertEquals("Stage has no errors", false, BO.APH_StageInfo.HasErrors());
			}
		}

		public void TestValidateAPH_Disposition()
		{
			AssertEquals("Disposition has errors", false, BO.APH_DispositionInfo.HasErrors());
			BO.APH_Disposition = "BLA";
			AssertEquals("Disposition has errors", true, BO.APH_DispositionInfo.HasErrors());
			foreach (ICodeDescription x in BO.APH_Disposition_List)
			{
				BO.APH_Disposition = x.Code;
				AssertEquals("Disposition has no errors", false, BO.APH_DispositionInfo.HasErrors());
			}
		}

		public void TestValidateAPH_Type()
		{
			AssertEquals("Type has errors", false, BO.APH_TypeInfo.HasErrors());
			BO.APH_Type = "BLA";
			AssertEquals("Type has errors", true, BO.APH_TypeInfo.HasErrors());
			foreach (ICodeDescription x in BO.APH_Type_List)
			{
				BO.APH_Type = x.Code;
				AssertEquals("Type has no errors", false, BO.APH_TypeInfo.HasErrors());
			}
		}

		public void TestValidateAPH_GoodsReceivedStatus()
		{
			AssertEquals("GoodsReceivedStatus has errors", false, BO.APH_GoodsReceivedStatusInfo.HasErrors());
			BO.APH_GoodsReceivedStatus = "BLA";
			AssertEquals("GoodsReceivedStatus has errors", true, BO.APH_GoodsReceivedStatusInfo.HasErrors());
			foreach (ICodeDescription x in BO.APH_GoodsReceivedStatus_List)
			{
				BO.APH_GoodsReceivedStatus = x.Code;
				AssertEquals("GoodsReceivedStatus has no errors", false, BO.APH_GoodsReceivedStatusInfo.HasErrors());
			}
		}

		public void TestValidateExchangeRate()
		{
			BO.APH_Calc_Currency.Currency = "USD";
			BO.APH_Calc_Currency.Rate = 0.65m;
			AssertNoErrors("Should be no error on Currency", BO.APH_RX_NKOrderCurrencyInfo);
			AssertNoErrors("Should be no error on exchange rate", BO.APH_EstimatedExchangeRateInfo);
			BO.APH_Calc_Currency.Rate = 0m;
			AssertNoErrors("Should be no error on currency", BO.APH_RX_NKOrderCurrencyInfo);
			AssertNoErrors("Should be no error on exchange rate", BO.APH_EstimatedExchangeRateInfo);
			BO.APH_Calc_Currency.Rate = 0.65m;
			BO.APH_Calc_Currency.Currency = "XXX";
			AssertHasErrors("Should be error on currency - invalid", BO.APH_RX_NKOrderCurrencyInfo);
			AssertNoErrors("Should be no error on exchange rate", BO.APH_EstimatedExchangeRateInfo);
		}

		protected AccPayableOrderHeader BO;
		protected override void SetUp()
		{
			base.SetUp();
			BO = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			BO.APH_OA_Buyer = Factory.NewWithValidTestData<OrgHeader>().PK;
			BO.SupplierDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
		}
	}
}