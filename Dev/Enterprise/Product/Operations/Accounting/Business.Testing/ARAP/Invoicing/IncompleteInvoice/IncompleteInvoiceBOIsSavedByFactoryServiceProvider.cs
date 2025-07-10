using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class IncompleteInvoiceBOIsSavedByFactoryServiceProviderTest : TestCaseWithFactory
	{
		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestIncompleteInvoiceSavingDoesNotSaveUnwantedObejcts()
		{
			var bizoList = new List<BusinessObject>();

			var charge = Factory.NewWithValidTestData<JobCharge>();
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			Factory.Save();

			var chargeAttrib = charge.JobChargeAttributes.AddNew();
			chargeAttrib.EC_Name = "INV";

			var costAttrib = cost.Attributes.AddNew();
			costAttrib.E6A_Name = "INV";

			bizoList.Add(Factory.NewWithValidTestData<AccTransactionHeader>());
			bizoList.Add(Factory.NewWithValidTestData<AccTransactionLines>());
			bizoList.Add(Factory.NewWithValidTestData<JobCharge>());
			bizoList.Add(Factory.NewJobWithValidTestDataForTesting<JobHeader>());
			bizoList.Add(Factory.NewWithValidTestData<JobChargeRevRecognition>());
			bizoList.Add(Factory.NewWithValidTestData<JobConsolCost>());
			bizoList.Add(Factory.NewWithValidTestData<ExchangeRate>());

			var testObjectCreator = new TestObjectCreator(Factory);
			var invoiceToSave = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv1", testObjectCreator.AUD, 1, 100, 0, 100, 0, testObjectCreator.Creditor1, testObjectCreator.GLHeader1.PK);

			invoiceToSave.SaveAsIncomplete();

			CombineAssertions(() =>
				{
					Assert("invoiceToSave", invoiceToSave.IsInDatabase);
					foreach (var line in invoiceToSave.Lines)
					{
						Assert("invoiceToSave lines", !line.IsInDatabase);
					}
					foreach (var bizo in bizoList)
					{
						Assert("excluded bizos", !bizo.IsInDatabase);
					}

					Assert("included bizos - JobChargeAttrib", chargeAttrib.IsInDatabase);
					Assert("included bizos - JobConsolCostAttrib", costAttrib.IsInDatabase);
				});

			var invoiceToSave2 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv2", testObjectCreator.AUD, 1, 100, 0, 100, 0, testObjectCreator.Creditor1, testObjectCreator.GLHeader1.PK);
			invoiceToSave.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			CombineAssertions(() =>
				{
					Assert("invoiceToSave2", invoiceToSave2.IsInDatabase);
					foreach (var line in invoiceToSave2.Lines)
					{
						Assert("invoiceToSave2 lines", line.IsInDatabase);
					}
					foreach (var line in invoiceToSave.Lines)
					{
						Assert("invoiceToSave lines", line.IsInDatabase);
					}
					foreach (var bizo in bizoList)
					{
						Assert("excluded bizos", bizo.IsInDatabase);
					}
				});
		}
	}
}
