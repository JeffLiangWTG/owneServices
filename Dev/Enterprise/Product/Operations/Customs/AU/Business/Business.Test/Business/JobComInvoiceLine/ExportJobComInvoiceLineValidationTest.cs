using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ExportJobComInvoiceLineValidationTest : BaseJobComInvoiceLineValidationTest
	{
		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new ExportJobComInvoiceLineValidation(invoiceLine);
		}

		public void TestValidateDescriptionIsNotTooLong()
		{
			testInvoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testInvoiceLine.JI_Description = new string('a', 128);
			Assert("!HasMessageErrors", !testInvoiceLine.JI_DescriptionInfo.HasMessageErrors());
			Assert("!HasWarnings", !testInvoiceLine.JI_DescriptionInfo.HasWarnings());
			testInvoiceLine.JI_Description = new string('a', 129);
			Assert("!HasMessageErrors", !testInvoiceLine.JI_DescriptionInfo.HasMessageErrors());
			Assert("HasWarnings", testInvoiceLine.JI_DescriptionInfo.HasWarnings());
		}

		public void TestCountryOfOriginValidation()
		{
			testInvoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testInvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			Assert("MessageErrors", testInvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
			testInvoiceLine.JI_CountryOfOrigin = "NZ";
			Assert("NoMessageErrors", !testInvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
		}

		public void TestValidateGrossWeight()
		{
			testInvoiceLine.JI_InvoiceUQ = "MC";
			testInvoiceLine.JI_Weight = 0;
			testInvoiceLine.JI_WeightUQ = "KG";
			Assert("Gross Weight Should be Greater Than zero", testInvoiceLine.JI_WeightInfo.HasMessageErrors());
			testInvoiceLine.JI_Weight = 10;
			Assert("Gross Weight Should have no errors", !testInvoiceLine.JI_WeightInfo.HasMessageErrors());
		}

		public void TestValidateGrossWeightGreaterThenOrEqualToCustomsQuantity()
		{
			testInvoiceLine.JI_InvoiceUQ = "PCE";
			testInvoiceLine.JI_InvoiceQuantity = 100m;
			testInvoiceLine.JI_CustomsUnitQty = "KG";
			testInvoiceLine.JI_CustomsQuantity = 100m;

			testInvoiceLine.JI_WeightUQ = "KG";
			testInvoiceLine.JI_Weight = 10m;
			Assert("MessageErrorOnWeight", testInvoiceLine.JI_WeightInfo.HasMessageErrors());
			testInvoiceLine.JI_CustomsQuantity = 10m;
			Assert("NoMessageErrorOnWeight", !testInvoiceLine.JI_WeightInfo.HasMessageErrors());
		}

		public void TestValidateJI_Tariff()
		{
			testInvoiceLine.JI_Tariff = "1234567";
			Assert("Incomplete tariff should have message error", testInvoiceLine.JI_TariffInfo.HasMessageErrors());
			testInvoiceLine.JI_Tariff = "19999999";
			Assert("Invalid tariff should have warning", testInvoiceLine.JI_TariffInfo.HasMessageErrors());
		}

		public void TestValidateInvoiceUQ()
		{
			testInvoiceLine.JI_InvoiceUQ = "MC";//only in ExportTariff
			Assert("MC is valid as it is in Customs UQ list", !testInvoiceLine.JI_InvoiceUQInfo.HasNotifications());

			testInvoiceLine.JI_InvoiceUQ = "XX";
			Assert("XX is invalid", testInvoiceLine.JI_InvoiceUQInfo.HasNotifications());
		}

		public void TestCustomsUQForChapter99()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var testExpTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99011010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testExpTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testInvoiceLine.JI_Tariff = "99011010";
				AssertEquals(ZString.Empty, testInvoiceLine.JI_CustomsUnitQty);
			}
		}

		public void TestCustomsUQForChapter99_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				testInvoiceLine.JI_Tariff = "99011010";
				AssertEquals("NR", testInvoiceLine.JI_CustomsUnitQty);
			}
		}

		public void TestValidateJI_LinePrice()
		{
			testInvoiceLine.JI_LinePrice = 1m;
			Assert("Line price should not have message error", !testInvoiceLine.JI_LinePriceInfo.HasMessageErrors());
			testInvoiceLine.JI_LinePrice = 0m;
			Assert("Line price should have message error", testInvoiceLine.JI_LinePriceInfo.HasMessageErrors());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}

		#endregion
	}
}
