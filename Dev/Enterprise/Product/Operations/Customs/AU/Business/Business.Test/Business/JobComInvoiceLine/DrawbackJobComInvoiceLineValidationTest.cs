using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DrawbackJobComInvoiceLineValidationTest : BaseJobComInvoiceLineValidationTest
	{
		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new DrawbackJobComInvoiceLineValidation(invoiceLine);
		}

		public void TestTariffNotMandatory()
		{
			testInvoiceLine.JI_Tariff = "";
			Assert("Has No MessageErrors", !testInvoiceLine.JI_TariffInfo.HasMessageErrors());
		}

		public void TestTariffValidation_WithoutStatClassification()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "AU";
			helper.LoadOrCreateNewTariff("AU", tariffType.PK, "1234567899", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");

			var newFactory = new BusinessObjectFactory();
			var auTariff = newFactory.New<AUCClass>();
			auTariff.UJ_Code = "1234.56.78 90";
			newFactory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1234.56.78 90";
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");

				using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					invoiceLine.JI_Tariff = "1234.56.78 99";
					AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");
				}
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			jobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		#endregion
	}
}
