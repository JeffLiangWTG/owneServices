using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SpecialCaseTax))]
	class SpecialCaseTaxTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var specialCaseTax = GetNewBusinessObject() as SpecialCaseTax;
			specialCaseTax.RateOrUnitValue = 10m;
			specialCaseTax.CurrencyCode = "BRL";
			specialCaseTax.UnitOfMeasure = "PKG";
			specialCaseTax.Quantity = 1m;
			specialCaseTax.LegalActType = "AD";
			specialCaseTax.LegalActIssuingBody = "123456";
			specialCaseTax.LegalActNumber = "123";
			specialCaseTax.LegalActYear = "2023";

			CombineAssertions(() =>
			{
				AssertEquals("TaxGroup", Constants.RateCodes.Antidumping, specialCaseTax.InvoiceLineTax.JLT_Type);
				AssertEquals("TaxGroup", RateCodes.Antidumping, specialCaseTax.QuantityPerUnit.CSI_SubType);
				AssertEquals("TaxType", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);
				AssertEquals("RateOrUnitValue", 10m, specialCaseTax.InvoiceLineTax.JLT_Rate);
				AssertEquals("Currency", "BRL", specialCaseTax.QuantityPerUnit.CSI_RX_NKCurrency);
				AssertEquals("UnitOfMeasure", "PKG", specialCaseTax.QuantityPerUnit.CSI_UnitOfQuantity);
				AssertEquals("Quantity", 1m, specialCaseTax.QuantityPerUnit.CSI_Quantity);
				AssertEquals("LegalActType", "AD", specialCaseTax.LegalAct.CSI_Code);
				AssertEquals("LegalActIssuingBody", "123456", specialCaseTax.LegalAct.CSI_IssuerType);
				AssertEquals("LegalActNumber", "123", specialCaseTax.LegalAct.CSI_ReferenceNumber);
				AssertEquals("LegalActYear", "2023", specialCaseTax.LegalAct.CSI_YearOfIssue);
			});
		}

		public void TestPropertiesReadOnly()
		{
			ReferenceTestDataHelper.CreateRefCusRateCodeAndType(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();

			CombineAssertions($"TaxGroup:{specialCaseTax.TaxGroup}, TaxType:{specialCaseTax.TaxType}", () =>
			{
				Assert("TaxGroupInfo should NOT be ReadOnly", !specialCaseTax.TaxGroupInfo.ReadOnly);
				Assert("TaxTypeInfo should NOT be ReadOnly", !specialCaseTax.TaxTypeInfo.ReadOnly);
				Assert("TaxRateInfo should NOT be ReadOnly", !specialCaseTax.RateOrUnitValueInfo.ReadOnly);
				Assert("CurrencyInfo should be ReadOnly", specialCaseTax.CurrencyCodeInfo.ReadOnly);
				Assert("UnitOfMeasureInfo should be ReadOnly", specialCaseTax.UnitOfMeasureInfo.ReadOnly);
				Assert("QuantityInfo should be ReadOnly", specialCaseTax.QuantityInfo.ReadOnly);
				Assert("LegalActTypeInfo should be ReadOnly", specialCaseTax.LegalActTypeInfo.ReadOnly);
				Assert("LegalActIssuingBodyInfo should be ReadOnly", specialCaseTax.LegalActIssuingBodyInfo.ReadOnly);
				Assert("LegalActNumberInfo should be ReadOnly", specialCaseTax.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYearInfo should be ReadOnly", specialCaseTax.LegalActYearInfo.ReadOnly);
			});

			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;

			CombineAssertions($"TaxGroup:{specialCaseTax.TaxGroup}, TaxType:{specialCaseTax.TaxType}", () =>
			{
				Assert("TaxGroupInfo should NOT be ReadOnly", !specialCaseTax.TaxGroupInfo.ReadOnly);
				Assert("TaxTypeInfo should NOT be ReadOnly", !specialCaseTax.TaxTypeInfo.ReadOnly);
				Assert("TaxRateInfo should NOT be ReadOnly", !specialCaseTax.RateOrUnitValueInfo.ReadOnly);
				Assert("CurrencyInfo should be ReadOnly", specialCaseTax.CurrencyCodeInfo.ReadOnly);
				Assert("UnitOfMeasureInfo should be ReadOnly", specialCaseTax.UnitOfMeasureInfo.ReadOnly);
				Assert("QuantityInfo should be ReadOnly", specialCaseTax.QuantityInfo.ReadOnly);
				Assert("LegalActTypeInfo should NOT be ReadOnly", !specialCaseTax.LegalActTypeInfo.ReadOnly);
				Assert("LegalActIssuingBodyInfo should NOT be ReadOnly", !specialCaseTax.LegalActIssuingBodyInfo.ReadOnly);
				Assert("LegalActNumberInfo should NOT be ReadOnly", !specialCaseTax.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYearInfo should NOT be ReadOnly", !specialCaseTax.LegalActYearInfo.ReadOnly);
			});

			specialCaseTax.TaxGroup = Constants.RateCodes.PIS;

			CombineAssertions($"TaxGroup:{specialCaseTax.TaxGroup}, TaxType:{specialCaseTax.TaxType}", () =>
			{
				Assert("TaxGroupInfo should NOT be ReadOnly", !specialCaseTax.TaxGroupInfo.ReadOnly);
				Assert("TaxTypeInfo should NOT be ReadOnly", !specialCaseTax.TaxTypeInfo.ReadOnly);
				Assert("TaxRateInfo should NOT be ReadOnly", !specialCaseTax.RateOrUnitValueInfo.ReadOnly);
				Assert("CurrencyInfo should be ReadOnly", specialCaseTax.CurrencyCodeInfo.ReadOnly);
				Assert("UnitOfMeasureInfo should be ReadOnly", specialCaseTax.UnitOfMeasureInfo.ReadOnly);
				Assert("QuantityInfo should be ReadOnly", specialCaseTax.QuantityInfo.ReadOnly);
				Assert("LegalActTypeInfo should be ReadOnly", specialCaseTax.LegalActTypeInfo.ReadOnly);
				Assert("LegalActIssuingBodyInfo should be ReadOnly", specialCaseTax.LegalActIssuingBodyInfo.ReadOnly);
				Assert("LegalActNumberInfo should be ReadOnly", specialCaseTax.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYearInfo should be ReadOnly", specialCaseTax.LegalActYearInfo.ReadOnly);
			});

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			CombineAssertions($"TaxGroup:{specialCaseTax.TaxGroup}, TaxType:{specialCaseTax.TaxType}", () =>
			{
				Assert("TaxGroupInfo should NOT be ReadOnly", !specialCaseTax.TaxGroupInfo.ReadOnly);
				Assert("TaxTypeInfo should NOT be ReadOnly", !specialCaseTax.TaxTypeInfo.ReadOnly);
				Assert("TaxRateInfo should NOT be ReadOnly", !specialCaseTax.RateOrUnitValueInfo.ReadOnly);
				Assert("CurrencyInfo should NOT be ReadOnly", !specialCaseTax.CurrencyCodeInfo.ReadOnly);
				Assert("UnitOfMeasureInfo should NOT be ReadOnly", !specialCaseTax.UnitOfMeasureInfo.ReadOnly);
				Assert("QuantityInfo should NOT be ReadOnly", !specialCaseTax.QuantityInfo.ReadOnly);
				Assert("LegalActTypeInfo should be ReadOnly", specialCaseTax.LegalActTypeInfo.ReadOnly);
				Assert("LegalActIssuingBodyInfo should be ReadOnly", specialCaseTax.LegalActIssuingBodyInfo.ReadOnly);
				Assert("LegalActNumberInfo should be ReadOnly", specialCaseTax.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYearInfo should be ReadOnly", specialCaseTax.LegalActYearInfo.ReadOnly);
			});

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.Reduction;

			CombineAssertions($"TaxGroup:{specialCaseTax.TaxGroup}, TaxType:{specialCaseTax.TaxType}", () =>
			{
				Assert("TaxGroupInfo should NOT be ReadOnly", !specialCaseTax.TaxGroupInfo.ReadOnly);
				Assert("TaxTypeInfo should NOT be ReadOnly", !specialCaseTax.TaxTypeInfo.ReadOnly);
				Assert("TaxRateInfo should NOT be ReadOnly", !specialCaseTax.RateOrUnitValueInfo.ReadOnly);
				Assert("CurrencyInfo should be ReadOnly", specialCaseTax.CurrencyCodeInfo.ReadOnly);
				Assert("UnitOfMeasureInfo should be ReadOnly", specialCaseTax.UnitOfMeasureInfo.ReadOnly);
				Assert("QuantityInfo should be ReadOnly", specialCaseTax.QuantityInfo.ReadOnly);
				Assert("LegalActTypeInfo should be ReadOnly", specialCaseTax.LegalActTypeInfo.ReadOnly);
				Assert("LegalActIssuingBodyInfo should be ReadOnly", specialCaseTax.LegalActIssuingBodyInfo.ReadOnly);
				Assert("LegalActNumberInfo should be ReadOnly", specialCaseTax.LegalActNumberInfo.ReadOnly);
				Assert("LegalActYearInfo should be ReadOnly", specialCaseTax.LegalActYearInfo.ReadOnly);
			});
		}

		public void TestTaxGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();

			AssertNull("QuantityPerUnit not created", specialCaseTax.QuantityPerUnit);
			AssertNull("LegalAct not created", specialCaseTax.LegalAct);

			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			var legalAct = specialCaseTax.LegalAct;
			AssertEquals("LegalAct added", invoiceLine, legalAct.Parent);
			AssertEquals("LegalAct Subject", AdditionalTaxTypeList.Codes.Antidumping, legalAct.CSI_SubType);

			specialCaseTax.TaxGroup = Constants.RateCodes.PIS;
			Assert("LegalAct deleted", legalAct.IsDeleted);
			AssertNull("LegalAct deleted", specialCaseTax.LegalAct);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			var quantityPerUnit = specialCaseTax.QuantityPerUnit;
			AssertEquals("QuantityPerUnit added", invoiceLine, specialCaseTax.QuantityPerUnit.Parent);
			AssertEquals("QuantityPerUnit RateCode", Constants.RateCodes.PIS, quantityPerUnit.CSI_SubType);

			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			AssertEquals("LegalAct added", invoiceLine, specialCaseTax.LegalAct.Parent);
			AssertEquals("InvoiceLineTax JLT_Type", Constants.RateCodes.Antidumping, specialCaseTax.InvoiceLineTax.JLT_Type);
			AssertEquals("QuantityPerUnit RateCode", Constants.RateCodes.Antidumping, quantityPerUnit.CSI_SubType);
		}

		public void TestTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();

			AssertNull("QuantityPerUnit not created", specialCaseTax.QuantityPerUnit);

			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			var quantityPerUnit = specialCaseTax.QuantityPerUnit;
			AssertEquals("QuantityPerUnit added", invoiceLine, quantityPerUnit.Parent);
			AssertEquals("QuantityPerUnit CSI_SubType", Constants.RateCodes.Antidumping, quantityPerUnit.CSI_SubType);
			AssertEquals("JLT_MethodOfCalculation updated", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.Reduction;
			Assert("QuantityPerUnit deleted", quantityPerUnit.IsDeleted);
			AssertNull("QuantityPerUnit deleted", specialCaseTax.QuantityPerUnit);
			AssertEquals("JLT_MethodOfCalculation updated", SpecialCaseTaxTypeList.Codes.Reduction, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			AssertNull("QuantityPerUnit deleted", specialCaseTax.QuantityPerUnit);
			AssertEquals("JLT_MethodOfCalculation updated", SpecialCaseTaxTypeList.Codes.AdValoremRate, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.Reduced;
			AssertNull("QuantityPerUnit deleted", specialCaseTax.QuantityPerUnit);
			AssertEquals("JLT_MethodOfCalculation updated", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.TariffAgreement;
			AssertNull("QuantityPerUnit deleted", specialCaseTax.QuantityPerUnit);
			AssertEquals("JLT_MethodOfCalculation updated", SpecialCaseTaxTypeList.Codes.TariffAgreement, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			quantityPerUnit = specialCaseTax.QuantityPerUnit;
			AssertEquals("QuantityPerUnit added", invoiceLine, quantityPerUnit.Parent);

			specialCaseTax.TaxType = "XXX";
			Assert("QuantityPerUnit deleted", quantityPerUnit.IsDeleted);
			AssertNull("QuantityPerUnit deleted", specialCaseTax.QuantityPerUnit);
		}

		public void TestDelete()
		{
			var specialCaseTax = GetNewBusinessObject() as SpecialCaseTax;
			specialCaseTax.Delete();
			CombineAssertions(() =>
			{
				Assert("SpecialCaseTax should be deleted", specialCaseTax.IsDeleted);
				Assert("JobComInvoiceLineTax should be deleted", specialCaseTax.InvoiceLineTax.IsDeleted);
				Assert("QuantityPerUnit should be deleted", specialCaseTax.QuantityPerUnit.IsDeleted);
				Assert("LegalAct should be deleted", specialCaseTax.LegalAct.IsDeleted);
			});
		}

		public void TestQuantityMaxLenght()
		{
			var specialCaseTax = GetNewBusinessObject() as SpecialCaseTax;
			specialCaseTax.Quantity = 1m;

			AssertEquals("Quantity MaxLength", 9, specialCaseTax.QuantityInfo.MaxLength);
			AssertHasDecimalPlacesAttribute("Quantity DecimalPlace", specialCaseTax.QuantityInfo, 0);
		}

		public void TestRateOrUnitValue()
		{
			var specialCaseTax = GetNewBusinessObject() as SpecialCaseTax;
			specialCaseTax.RateOrUnitValue = 200m;
			AssertEquals("RateOrUnitValue should be ", specialCaseTax.RateOrUnitValue, 200m);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			AssertEquals("RateOrUnitValue should be refreshed when TaxType is changed", specialCaseTax.RateOrUnitValue, 0m);
		}

		public void TestRateOrUnitValueDecimalPlaces()
		{
			var specialCaseTax = GetNewBusinessObject() as SpecialCaseTax;
			AssertHasDecimalPlacesAttribute("RateOrUnitValue DecimalPlace", specialCaseTax.RateOrUnitValueInfo, 5);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			AssertHasDecimalPlacesAttribute("RateOrUnitValue DecimalPlace", specialCaseTax.RateOrUnitValueInfo, 2);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.Reduced;
			AssertHasDecimalPlacesAttribute("RateOrUnitValue DecimalPlace", specialCaseTax.RateOrUnitValueInfo, 2);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.Reduction;
			AssertHasDecimalPlacesAttribute("RateOrUnitValue DecimalPlace", specialCaseTax.RateOrUnitValueInfo, 2);

			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.TariffAgreement;
			AssertHasDecimalPlacesAttribute("RateOrUnitValue DecimalPlace", specialCaseTax.RateOrUnitValueInfo, 2);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ReferenceTestDataHelper.CreateRefCusRateCodeAndType(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			return specialCaseTax;
		}
	}
}
