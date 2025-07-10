using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TariffWrapperTest : TestCaseWithFactory
	{
		public void TestConstructorWithValue()
		{
			var message = "Value cannot be null.";
			var paramName = "invoiceLine";
#if NETFRAMEWORK
			var errorMessage = $"{message}{System.Environment.NewLine}Parameter name: {paramName}";
#else
			var errorMessage = $"{message} (Parameter '{paramName}')";
#endif
			_ = AssertExceptionThrown<ArgumentNullException>("Null InvoiceLine", errorMessage,
				() => new TariffWrapper(null));
		}

		public void TestTariffCode()
		{
			AssertEquals("Tariff code should be set.", "3333.33.33", tariff.Code);
		}

		public void TestTariffDescription()
		{
			AssertEquals("Tariff Description should be set.", "Show the description.", tariff.Description);
		}

		public void TestTariffUQ1()
		{
			AssertEquals("Tariff UQ1 should be set.", Core.Constants.Weight.Kilograms, tariff.UQ1);
		}

		public void TestTariffUQ2()
		{
			AssertEquals("Tariff UQ2 should be set.", string.Empty, tariff.UQ2);
		}

		public void TestTariffUQ3()
		{
			AssertEquals("Tariff UQ3 should be set.", string.Empty, tariff.UQ3);
		}

		public void TestTariffUQ4()
		{
			AssertEquals("Tariff UQ4 should be set.", string.Empty, tariff.UQ4);
		}

		public void TestTariffUQ5()
		{
			AssertEquals("Tariff UQ5 should be set.", string.Empty, tariff.UQ5);
		}

		public void TestTariffWrapper_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var ahecc = Factory.New<AUCAHECC>();
				ahecc.UA_AHECC = "3333.22.11";
				ahecc.UA_LongDescription = "Show the description.";
				ahecc.UA_UQ = "Kg";

				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = ahecc.UA_AHECC;
				tariff = new TariffWrapper(invoiceLine);

				CombineAssertions("Old AUCAHECC Code", () =>
				{
					AssertEquals("Tariff code should be set.", "3333.22.11", tariff.Code);
					AssertEquals("Tariff Description should be set.", "Show the description.", tariff.Description);
					AssertEquals("Tariff UQ1 should be Kilograms.", Core.Constants.Weight.Kilograms, tariff.UQ1);
					AssertEquals("Tariff UQ2 should be Empty.", string.Empty, tariff.UQ2);
					AssertEquals("Tariff UQ3 should be Empty.", string.Empty, tariff.UQ3);
					AssertEquals("Tariff UQ4 should be Empty.", string.Empty, tariff.UQ4);
					AssertEquals("Tariff UQ5 should be Empty.", string.Empty, tariff.UQ5);
				});
			}
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "33333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "Show the description."
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3333.33.33";
			tariff = new TariffWrapper(invoiceLine);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;
		ITariff tariff;
	}
}
