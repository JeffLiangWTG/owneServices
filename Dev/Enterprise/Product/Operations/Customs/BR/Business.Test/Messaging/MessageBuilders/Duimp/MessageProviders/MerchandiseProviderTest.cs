using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class MerchandiseProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(MerchandiseProvider.New(null));

			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.Invoices[0].InvoiceLines.AddNew();
			AssertType<MerchandiseProvider>(MerchandiseProvider.New(declaration.InvoiceLines.Cast<JobComInvoiceLine>()));
		}

		public void TestMerchandise()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var dataProvider = MerchandiseProvider.New(invoice.InvoiceLines.Cast<JobComInvoiceLine>());
			CombineAssertions(() =>
			{
				Assert("ValuationMethodCode should be Empty", dataProvider.ApplicationTypeCode.IsEmpty());
				Assert("Condition should be Empty", dataProvider.Condition.IsEmpty());
				Assert("CommercialUnit should be Empty", dataProvider.CommercialUnit.IsEmpty());
				AssertEquals("CommercialQuantity should be Zero", 0.0, dataProvider.CommercialQuantity);
				AssertEquals("QuantityStatisticalMeasure should be Zero", 0.0, dataProvider.QuantityStatisticalMeasure);
				AssertEquals("NetWeight should be Zero", 0.0, dataProvider.NetWeight);
				Assert("Currency should be Empty", dataProvider.Currency.IsEmpty());
				AssertEquals("TradeCurrencyUnitValue should be", 0.0, dataProvider.UnitPrice);
				Assert("Description should be Empty", dataProvider.Description.IsEmpty());
			});

			invoice.JZ_IncoTerm = BRIncoTermList.Codes.CIF;
			invoice.JZ_InvoiceAmount = 1412.00m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			invoiceLine1.JI_GoodsApplication = ImportGoodsApplicationTypeList.Codes.Consumption;
			invoiceLine1.JI_GoodsCondition = ImportGoodsConditionTypeList.Codes.Used;
			invoiceLine1.JI_LinePrice = 1200m;
			invoiceLine1.JI_InvoiceUQ = "BOX";
			invoiceLine1.JI_InvoiceQuantity = 10.123456m;
			invoiceLine1.JI_CustomsQuantity = 20.323456m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceLine1.ComplementaryDescription = "PANO";

			var charge1 = invoiceLine1.Charges.AddNew();
			charge1.J7_ChargeType = Common.CustomsChargeTypeList.Codes.AdditionCharge;
			charge1.J7_Amount = 50m;
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge1.J7_IsDutiable = false;
			charge1.J7_Calc_IsIncludedInInvoiceAmount = true;
			declaration.ResumeApportionment();

			dataProvider = MerchandiseProvider.New(invoice.InvoiceLines.Cast<JobComInvoiceLine>());
			CombineAssertions(() =>
			{
				AssertEquals("ApplicationTypeCode", "CONSUMO", dataProvider.ApplicationTypeCode);
				AssertEquals("Condition", "USADA", dataProvider.Condition);
				AssertEquals("CommercialUnit", "Caixa", dataProvider.CommercialUnit);
				AssertEquals("CommercialQuantity", 10.12346, dataProvider.CommercialQuantity);
				AssertEquals("QuantityStatisticalMeasure", 20.32346, dataProvider.QuantityStatisticalMeasure);
				AssertEquals("NetWeight", 22.67962, dataProvider.NetWeight);
				AssertEquals("Currency", "USD", dataProvider.Currency);
				AssertEquals("TradeCurrencyUnitValue should be", 123.4755706, dataProvider.UnitPrice);
				AssertEquals("Description", "PANO", dataProvider.Description);
			});

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_GoodsApplication = ImportGoodsApplicationTypeList.Codes.Consumption;
			invoiceLine2.JI_GoodsCondition = ImportGoodsConditionTypeList.Codes.Used;
			invoiceLine2.JI_LinePrice = 700m;
			invoiceLine2.JI_InvoiceUQ = "BOX";
			invoiceLine2.JI_InvoiceQuantity = 30.2m;
			invoiceLine2.JI_CustomsQuantity = 40.3m;
			invoiceLine2.JI_NetWeight = 80m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
			invoiceLine2.ComplementaryDescription = "PANO";

			var charge2 = invoiceLine2.Charges.AddNew();
			charge2.J7_ChargeType = Common.CustomsChargeTypeList.Codes.AdditionCharge;
			charge2.J7_Amount = 20m;
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge2.J7_IsDutiable = false;
			charge2.J7_Calc_IsIncludedInInvoiceAmount = true;
			declaration.ResumeApportionment();

			dataProvider = MerchandiseProvider.New(invoice.InvoiceLines.Cast<JobComInvoiceLine>());
			CombineAssertions(() =>
			{
				AssertEquals("ApplicationTypeCode", "CONSUMO", dataProvider.ApplicationTypeCode);
				AssertEquals("Condition", "USADA", dataProvider.Condition);
				AssertEquals("CommercialUnit", "Caixa", dataProvider.CommercialUnit);
				AssertEquals("CommercialQuantity", 40.32346, dataProvider.CommercialQuantity);
				AssertEquals("QuantityStatisticalMeasure", 60.62346, dataProvider.QuantityStatisticalMeasure);
				AssertEquals("NetWeight", 24.94758, dataProvider.NetWeight);
				AssertEquals("Currency", "USD", dataProvider.Currency);
				AssertEquals("TradeCurrencyUnitValue should be", 48.8549346, dataProvider.UnitPrice);
				AssertEquals("Description", "PANO", dataProvider.Description);
			});

			invoiceLine1.JI_InvoiceQuantity = 0m;
			invoiceLine2.JI_InvoiceQuantity = 0m;
			dataProvider = MerchandiseProvider.New(invoice.InvoiceLines.Cast<JobComInvoiceLine>());
			AssertEquals("TradeCurrencyUnitValue should be", 0.0, dataProvider.UnitPrice);
		}
	}
}
