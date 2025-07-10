using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SpecialCaseTaxCollection))]
	class SpecialCaseTaxCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SpecialCaseTaxCollection>
	{
		public void TestLoadForImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.Taxes.AddNew(Constants.RateCodes.ImportDuty);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.IPI);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.PIS);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.Cofins);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.Antidumping);

			AssertEquals(0, invoiceLine.SpecialCaseTaxes.Count);

			var tax1 = invoiceLine.Taxes.AddNew(Constants.RateCodes.ImportDuty);
			tax1.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			var tax2 = invoiceLine.Taxes.AddNew(Constants.RateCodes.IPI);
			tax2.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			var tax3 = invoiceLine.Taxes.AddNew(Constants.RateCodes.PIS);
			tax3.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduction;
			var tax4 = invoiceLine.Taxes.AddNew(Constants.RateCodes.Cofins);
			tax4.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			var tax5 = invoiceLine.Taxes.AddNew(Constants.RateCodes.Antidumping);
			tax5.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			var unitInfo2 = invoiceLine.QuantityPerUnitInfos.AddNew(Constants.RateCodes.IPI);
			var unitInfo3 = invoiceLine.QuantityPerUnitInfos.AddNew(Constants.RateCodes.PIS);
			var unitInfo4 = invoiceLine.QuantityPerUnitInfos.AddNew(Constants.RateCodes.Cofins);
			var unitInfo5 = invoiceLine.QuantityPerUnitInfos.AddNew(Constants.RateCodes.Antidumping);

			var legalAct2 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.ExIPITariff);
			var legalAct3 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.IPITaxBenefit);
			var legalAct5 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping);

			AssertEquals(0, invoiceLine.SpecialCaseTaxes.Count);

			invoiceLine.SpecialCaseTaxes.Rebuild();
			AssertEquals(5, invoiceLine.SpecialCaseTaxes.Count);

			var specialCaseTaxes = invoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().OrderBy(x => x.TaxGroup).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("ImportDuty TaxGroup", Constants.RateCodes.ImportDuty, specialCaseTaxes[0].TaxGroup);
				AssertEquals("ImportDuty TaxType", SpecialCaseTaxTypeList.Codes.AdValoremRate, specialCaseTaxes[0].TaxType);
				AssertEquals("ImportDuty QuantityPerUnit", null, specialCaseTaxes[0].QuantityPerUnit);
				AssertEquals("ImportDuty LegalAct", null, specialCaseTaxes[0].LegalAct);

				AssertEquals("IPI TaxGroup", Constants.RateCodes.IPI, specialCaseTaxes[1].TaxGroup);
				AssertEquals("IPI TaxType", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTaxes[1].TaxType);
				AssertEquals("IPI QuantityPerUnit", unitInfo2, specialCaseTaxes[1].QuantityPerUnit);
				AssertEquals("IPI LegalAct", null, specialCaseTaxes[1].LegalAct);

				AssertEquals("Antidumping TaxGroup", Constants.RateCodes.Antidumping, specialCaseTaxes[2].TaxGroup);
				AssertEquals("Antidumping TaxType", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTaxes[2].TaxType);
				AssertEquals("Antidumping QuantityPerUnit", unitInfo5, specialCaseTaxes[2].QuantityPerUnit);
				AssertEquals("Antidumping LegalAct", legalAct5, specialCaseTaxes[2].LegalAct);

				AssertEquals("PIS TaxGroup", Constants.RateCodes.PIS, specialCaseTaxes[3].TaxGroup);
				AssertEquals("PIS TaxType", SpecialCaseTaxTypeList.Codes.Reduction, specialCaseTaxes[3].TaxType);
				AssertEquals("PIS QuantityPerUnit", null, specialCaseTaxes[3].QuantityPerUnit);
				AssertEquals("PIS LegalAct", null, specialCaseTaxes[3].LegalAct);

				AssertEquals("Cofins TaxGroup", Constants.RateCodes.Cofins, specialCaseTaxes[4].TaxGroup);
				AssertEquals("Cofins TaxType", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTaxes[4].TaxType);
				AssertEquals("Cofins QuantityPerUnit", unitInfo4, specialCaseTaxes[4].QuantityPerUnit);
				AssertEquals("Cofins LegalAct", null, specialCaseTaxes[4].LegalAct);
			});
		}

		public void TestLoadForImportSiscomex()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.Taxes.AddNew(Constants.RateCodes.ImportDuty);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.IPI);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.PIS);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.Cofins);
			invoiceLine.Taxes.AddNew(Constants.RateCodes.Antidumping);

			AssertEquals(0, invoiceLine.SpecialCaseTaxes.Count);

			var tax1 = invoiceLine.Taxes.AddNew(Constants.RateCodes.ImportDuty);
			tax1.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			var tax2 = invoiceLine.Taxes.AddNew(Constants.RateCodes.IPI);
			tax2.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			var tax3 = invoiceLine.Taxes.AddNew(Constants.RateCodes.PIS);
			tax3.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduction;
			var tax4 = invoiceLine.Taxes.AddNew(Constants.RateCodes.Cofins);
			tax4.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduction;
			var tax5 = invoiceLine.Taxes.AddNew(Constants.RateCodes.Antidumping);
			tax5.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			var unitInfo5 = invoiceLine.QuantityPerUnitInfos.AddNew(Constants.RateCodes.Antidumping);
			var legalAct5 = invoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping);

			AssertEquals(0, invoiceLine.SpecialCaseTaxes.Count);

			invoiceLine.SpecialCaseTaxes.Rebuild();
			AssertEquals(3, invoiceLine.SpecialCaseTaxes.Count);

			var specialCaseTaxes = invoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().OrderBy(x => x.TaxGroup).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Antidumping TaxGroup", Constants.RateCodes.Antidumping, specialCaseTaxes[0].TaxGroup);
				AssertEquals("Antidumping TaxType", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTaxes[0].TaxType);
				AssertEquals("Antidumping QuantityPerUnit", unitInfo5, specialCaseTaxes[0].QuantityPerUnit);
				AssertEquals("Antidumping LegalAct", legalAct5, specialCaseTaxes[0].LegalAct);

				AssertEquals("PIS TaxGroup", Constants.RateCodes.PIS, specialCaseTaxes[1].TaxGroup);
				AssertEquals("PIS TaxType", SpecialCaseTaxTypeList.Codes.Reduction, specialCaseTaxes[1].TaxType);
				AssertEquals("PIS QuantityPerUnit", null, specialCaseTaxes[1].QuantityPerUnit);
				AssertEquals("PIS LegalAct", null, specialCaseTaxes[1].LegalAct);

				AssertEquals("Cofins TaxGroup", Constants.RateCodes.Cofins, specialCaseTaxes[2].TaxGroup);
				AssertEquals("Cofins TaxType", SpecialCaseTaxTypeList.Codes.Reduction, specialCaseTaxes[2].TaxType);
				AssertEquals("Cofins QuantityPerUnit", null, specialCaseTaxes[2].QuantityPerUnit);
				AssertEquals("Cofins LegalAct", null, specialCaseTaxes[2].LegalAct);
			});
		}

		public void TestUpdateOrAddAndDeleteReductionRate()
		{
			var collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);

			collection.UpdateOrAddReductionRate(Constants.RateCodes.IPI);
			AssertEquals(1, collection.Count);
			AssertEquals("TaxGroup", Constants.RateCodes.IPI, collection[0].TaxGroup);
			AssertEquals("TaxType", SpecialCaseTaxTypeList.Codes.Reduced, collection[0].TaxType);

			collection[0].TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			collection.UpdateOrAddReductionRate(Constants.RateCodes.IPI);
			AssertEquals(1, collection.Count);
			AssertEquals("TaxGroup", Constants.RateCodes.IPI, collection[0].TaxGroup);
			AssertEquals("TaxType", SpecialCaseTaxTypeList.Codes.Reduced, collection[0].TaxType);

			collection.UpdateOrAddReductionRate(Constants.RateCodes.PIS);
			AssertEquals(2, collection.Count);
			AssertEquals("TaxGroup", Constants.RateCodes.PIS, collection[1].TaxGroup);
			AssertEquals("TaxType", SpecialCaseTaxTypeList.Codes.Reduced, collection[1].TaxType);

			collection.DeleteReductionRate(Constants.RateCodes.IPI);
			AssertEquals("IPI Tax deleted", 1, collection.Count);
			AssertEquals("TaxGroup", Constants.RateCodes.PIS, collection[0].TaxGroup);

			collection.DeleteReductionRate(Constants.RateCodes.IPI);
			AssertEquals("No Tax deleted", 1, collection.Count);

			collection.DeleteReductionRate(Constants.RateCodes.PIS);
			AssertEquals("PIS Tax deleted", 0, collection.Count);
		}

		public void TestFindByRateCodeAndTaxType()
		{
			var collection = GetCollectionToTest();
			collection.UpdateOrAddReductionRate(Constants.RateCodes.IPI);
			collection.UpdateOrAddReductionRate(Constants.RateCodes.PIS);
			collection.UpdateOrAddReductionRate(Constants.RateCodes.Cofins);
			collection.UpdateOrAddReductionRate(Constants.RateCodes.Antidumping);

			AssertNull(collection.FindByRateCodeAndTaxType(Constants.RateCodes.IPI, SpecialCaseTaxTypeList.Codes.QuantityPerUnit));
			AssertNull(collection.FindByRateCodeAndTaxType(Constants.RateCodes.PIS, SpecialCaseTaxTypeList.Codes.QuantityPerUnit));
			AssertNull(collection.FindByRateCodeAndTaxType(Constants.RateCodes.Cofins, SpecialCaseTaxTypeList.Codes.QuantityPerUnit));
			AssertNull(collection.FindByRateCodeAndTaxType(Constants.RateCodes.Antidumping, SpecialCaseTaxTypeList.Codes.QuantityPerUnit));

			var ipi = collection.FindByRateCodeAndTaxType(Constants.RateCodes.IPI, SpecialCaseTaxTypeList.Codes.Reduced);
			var pis = collection.FindByRateCodeAndTaxType(Constants.RateCodes.PIS, SpecialCaseTaxTypeList.Codes.Reduced);
			var cofins = collection.FindByRateCodeAndTaxType(Constants.RateCodes.Cofins, SpecialCaseTaxTypeList.Codes.Reduced);
			var antidumping = collection.FindByRateCodeAndTaxType(Constants.RateCodes.Antidumping, SpecialCaseTaxTypeList.Codes.Reduced);

			CombineAssertions(() =>
			{
				AssertEquals("IPI", Constants.RateCodes.IPI, ipi.TaxGroup);
				AssertEquals("IPI TaxType", SpecialCaseTaxTypeList.Codes.Reduced, ipi.TaxType);

				AssertEquals("PIS", Constants.RateCodes.PIS, pis.TaxGroup);
				AssertEquals("PIS TaxType", SpecialCaseTaxTypeList.Codes.Reduced, pis.TaxType);

				AssertEquals("Cofins", Constants.RateCodes.Cofins, cofins.TaxGroup);
				AssertEquals("Cofins TaxType", SpecialCaseTaxTypeList.Codes.Reduced, cofins.TaxType);

				AssertEquals("Antidumping", Constants.RateCodes.Antidumping, antidumping.TaxGroup);
				AssertEquals("Antidumping TaxType", SpecialCaseTaxTypeList.Codes.Reduced, antidumping.TaxType);
			});
		}

		protected override SpecialCaseTaxCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return invoiceLine.SpecialCaseTaxes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax = invoiceLine.Taxes.AddNew();
			return new SpecialCaseTax(tax, null, null);
		}
	}
}
