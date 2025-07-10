using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DuimpTaxRegimeCollection))]
	class DuimpTaxRegimeCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<DuimpTaxRegime>
	{
		public void TestAddNewWithProfile()
		{
			invoiceLine.JI_Tariff = "12345678";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			var taxRegime = invoiceLine.DuimpTaxRegimes.AddNew(invoiceLine.GetRequiredTTProfiles().First(f => f.LegalCode == "P01"));
			CombineAssertions(() =>
			{
				AssertEquals("Parent", invoiceLine, taxRegime.Parent);
				AssertEquals("CSI_Procedure", "P01", taxRegime.CSI_Procedure);
				AssertEquals("CSI_SubType", "DTY", taxRegime.CSI_SubType);
				AssertEquals("CSI_Code", "Test 1", taxRegime.CSI_Code);
			});
		}

		public void TestUpdateMandatoryTaxRegimes()
		{
			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.DuimpTaxRegimes.Rebuild();
				AssertEquals(0, invoiceLine.DuimpTaxRegimes.Count);
				AssertEquals(0, invoiceLine.TaxRegimeAttributes.Count);

				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				AssertEquals(1, invoiceLine.DuimpTaxRegimes.Count);
				AssertEquals(2, invoiceLine.TaxRegimeAttributes.Count);

				invoiceLine.JI_Tariff = "87654321";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Angola;
				AssertEquals(0, invoiceLine.DuimpTaxRegimes.Count);
				AssertEquals(0, invoiceLine.TaxRegimeAttributes.Count);
			}
		}

		public void TestLoadCollection()
		{
			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				AssertEquals(1, invoiceLine.DuimpTaxRegimes.Count);
				AssertEquals(2, invoiceLine.TaxRegimeAttributes.Count);
				var taxRegime1 = invoiceLine.DuimpTaxRegimes.AddNew();
				taxRegime1.CSI_Procedure = "P10";
				var taxRegime2 = invoiceLine.DuimpTaxRegimes.AddNew();
				Factory.Save();
				AssertEquals(3, invoiceLine.DuimpTaxRegimes.Count);

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(JobComInvoiceLineSchema.PK, invoiceLine.PK);
				var invLineLoaded = newFactory.LoadTop1<JobComInvoiceLine>(query);
				AssertEquals(1, invLineLoaded.DuimpTaxRegimes.Count);
				AssertEquals("P01", invLineLoaded.DuimpTaxRegimes[0].CSI_Procedure);
				AssertNoExceptionThrown("No Exception Thrown", invLineLoaded.TaxRegimeAttributes.Rebuild);
				newFactory.Save();

				var attribute = profiles.First(f => f.QuestionCode == "ATT2").RefCusProfile.Attributes.First(f => f.XXY_Name == Constants.Profile.AttributeNames.Mandatory);
				attribute.XXY_Value = "True";
				Factory.Save();

				var newFactory2 = new BusinessObjectFactory();
				var invLineLoaded2 = newFactory2.LoadTop1<JobComInvoiceLine>(query);
				AssertEquals(2, invLineLoaded2.DuimpTaxRegimes.Count);
				AssertEquals(1, invLineLoaded2.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "P01").Count());
				AssertEquals(1, invLineLoaded2.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "P02").Count());
				AssertNoExceptionThrown("No Exception Thrown", invLineLoaded2.TaxRegimeAttributes.Rebuild);
			}
		}

		public void TestFindByLegalCode()
		{
			var collection = new DuimpTaxRegimeCollection(invoiceLine);
			Assert("Collection should NOT contain Legal Base equal P01", !collection.FindByLegalCode("P01").Any());

			var taxRegime1 = collection.AddNew(profiles.FirstOrDefault(f => f.QuestionCode == "ATT1"));
			AssertContainsExactElementsInAnyOrder(new[] { taxRegime1 }, collection.FindByLegalCode("P01"));
			Assert("Collection should NOT contain Legal Base equal P02", !collection.FindByLegalCode("P02").Any());

			var taxRegime2 = collection.AddNew(profiles.FirstOrDefault(f => f.QuestionCode == "ATT2"));
			AssertContainsExactElementsInAnyOrder(new[] { taxRegime1 }, collection.FindByLegalCode("P01"));
			AssertContainsExactElementsInAnyOrder(new[] { taxRegime2 }, collection.FindByLegalCode("P02"));

			var taxRegime3 = collection.AddNew(profiles.FirstOrDefault(f => f.QuestionCode == "ATT12"));
			AssertContainsExactElementsInAnyOrder(new[] { taxRegime2, taxRegime3 }, collection.FindByLegalCode("P02"));

			var taxRegime4 = collection.AddNew(profiles.FirstOrDefault(f => f.QuestionCode == "ATT13"));
			AssertContainsExactElementsInAnyOrder(new[] { taxRegime2, taxRegime3 }, collection.FindByLegalCode("P02"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			if (invoiceLine.JI_Tariff.IsEmpty)
			{
				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			}

			var taxRegime = Factory.New<DuimpTaxRegime>();
			taxRegime.CSI_Procedure = "P02";
			taxRegime.CSI_Code = "Test 2";
			taxRegime.CSI_SubType = "COF";
			return taxRegime;
		}

		protected override CusSupportingInfoCollection<DuimpTaxRegime> GetCusSupportingInfoCollection()
		{
			var collection = new CusSupportingInfoCollection<DuimpTaxRegime>(invoiceLine, CusSupportingInfoTypeList.Codes.DuimpTaxRegime, "COF");
			return collection;
		}

		protected override void SetUp()
		{
			base.SetUp();
			profiles = ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory).Select(TariffProfile.New).ToArray();
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		TariffProfile[] profiles;
		JobComInvoiceLine invoiceLine;
	}
}
