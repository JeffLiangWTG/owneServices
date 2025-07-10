using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecordCollectionValidatorTest : TestCaseWithFactory
	{
		public void TestIsUsagedAsDependency()
		{
			AssertType<TaxRecordCollectionValidator>(new TaxRecordCreator().TaxRecordCollectionValidator_ExposedForTestOnly);
		}

		public void TestGetDuplicatedTaxRecordsForSinglePostingTaxSystem()
		{
			var company = GlbCompany.CurrentCompany;
			using (company.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.TaxSystems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaxSystemsConfigurationCollection())) // To achieve "No tax system setup."
			{
				var taxRecord1 = Factory.New<AccTaxTransaction>();
				taxRecord1.ATT_TaxSystemCode = "ISS";
				taxRecord1.ATT_GC = company.PK;
				var taxRecord2 = Factory.New<AccTaxTransaction>();
				taxRecord2.ATT_TaxSystemCode = "ISS1";
				taxRecord2.ATT_GC = company.PK;
				var taxRecord3 = Factory.New<AccTaxTransaction>();
				taxRecord3.ATT_TaxSystemCode = "ISS";
				taxRecord3.ATT_GC = company.PK;

				var taxRecords = new[] { taxRecord1, taxRecord2, taxRecord3 };

				ITaxRecordCollectionValidator validator = new TaxRecordCollectionValidator(new TaxFrameworkConfigurationHelper());

				var duplicates = validator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(taxRecords);
				AssertEquals("No tax system setup.", 0, duplicates.Count);

				var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("ISS");
				taxSystem1.Country = Core.Constants.CountryCodes.Brazil;
				var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("ISS1");
				taxSystem2.Country = Core.Constants.CountryCodes.Brazil;
				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.AddRange(new[] { taxSystem1, taxSystem2 });
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

				duplicates = validator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(taxRecords);
				AssertContainsExactElementsInAnyOrder(new[] { taxRecord1, taxRecord3 }, duplicates);

				taxRecord1.ATT_TaxSystemCode = "ISS1";
				duplicates = validator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(taxRecords);
				AssertEquals("Only tax system with ISS code can't have duplicates at the moment of test writing.", 0, duplicates.Count);
			}
		}

		public void TestGetDuplicatedTaxRecordsForSinglePostingTaxSystem_WithEmptyTaxRecordList()
		{
			ITaxRecordCollectionValidator validator = new TaxRecordCollectionValidator(new TaxFrameworkConfigurationHelper());
			var duplicates = validator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(new List<AccTaxTransaction>());
			AssertEquals("No tax system setup.", 0, duplicates.Count);
		}

		public void TestArgumentNullExceptionWhenTaxFrameworkConfigurationHelperIsNUll()
		{
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: taxFrameworkConfigurationHelper", () => new TaxRecordCollectionValidator(null));

			AssertNoExceptionThrown(() => new TaxRecordCollectionValidator(new TaxFrameworkConfigurationHelper()));
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
