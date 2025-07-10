using System;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIDECLineProvider))]
	sealed class SCIDECLineProviderTest : ImportDecLineProviderAbstractTest<SCIDECLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIDECLineProvider(null));
		}

		public void TestArticleNumber()
		{
			invoiceLine.JI_PartNo = "ABC123";
			AssertEquals("ABC123", Provider.ArticleNumber);
		}

		public void TestInwardMovementAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_BondedWhsQuantity = 10.5;
			invoiceLine.JI_BondedWhsUnitQty = "KGMA";
			invoiceLine2.JI_BondedWhsQuantity = 5.1;
			invoiceLine2.JI_BondedWhsUnitQty = "KGMA";
			var amount = Provider.InwardMovementAmount;

			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 15.6m, amount.Quantity);
				AssertEquals("MeasurementUnit", "KGM", amount.MeasurementUnit);
				AssertEquals("Qualifier", "A", amount.Qualifier);
			});
		}

		public void TestEconomicConditions()
		{
			invoiceLine.ZG_EconomicConditions = "01";
			AssertEquals("01", Provider.EconomicConditions);
		}

		public void TestProducts_NoInwardProcessingProducts()
		{
			AssertEquals("No products", 0, Provider.Products.Count);
		}

		public void TestProducts()
		{
			invoiceLine.InwardProcessingProducts.AddNew();
			invoiceLine.InwardProcessingProducts.AddNew();
			AssertEquals(2, Provider.Products.Count);
		}

		public void TestIdentificationMeans()
		{
			invoiceLine.ZG_IdentificationMeansType = IdentificationMeansTypeList.Codes.SN;
			invoiceLine.JI_ExtraInfoForClassification = "ABC";
			var provider = Provider.IdentificationMeans;
			CombineAssertions(() =>
			{
				AssertEquals(IdentificationMeansTypeList.Codes.SN, provider.Type);
				AssertEquals("ABC", provider.Description);
			});
		}

		public void TestRequestedPreferentialTreatment()
		{
			invoiceLine.JI_PrimaryPreference = "150";
			AssertEquals("150", Provider.RequestedPreferentialTreatment);
		}

		public void TestAssessmentCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals("IsHighValueOvrdValidForAssessmentCustomsValue is always true", 502.58m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_DeclarationDV1()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals("IsHighValueOvrdValidForAssessmentCustomsValue is always true", 502.58m, Provider.AssessmentCustomsValue);
		}

		protected override SCIDECLineProvider GetProvider() => new SCIDECLineProvider(entryLine);
	}
}
