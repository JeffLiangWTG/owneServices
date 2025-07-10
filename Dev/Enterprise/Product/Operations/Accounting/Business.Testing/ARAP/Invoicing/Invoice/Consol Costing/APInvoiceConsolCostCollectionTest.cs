using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolCostCollection))]
	public class APInvoiceConsolCostCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			APInvoice inv = Factory.New<APInvoice>();
			return new APInvoiceConsolCostCollection(Factory, inv);
		}

		public void TestAllowNew()
		{
			InvoicingBase testInvoicingBase = Factory.New<UAInvoice>();
			testInvoicingBase.AH_TransactionNum = "00001000";
			Factory.Save();
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			APInvoiceConsolCostCollection testcollection = new APInvoiceConsolCostCollection(Factory, testInvoicingBase);
			Assert(testcollection.AllowNew);

			testInvoicingBase = converter.ConvertToAP(testInvoicingBase, false);

			testcollection = new APInvoiceConsolCostCollection(Factory, testInvoicingBase);
			Assert(!testcollection.AllowNew);

			testInvoicingBase = Factory.New<APInvoice>();
			testcollection = new APInvoiceConsolCostCollection(Factory, testInvoicingBase);
			Assert(testcollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			InvoicingBase testInvoicingBase = Factory.New<UAInvoice>();
			testInvoicingBase.AH_TransactionNum = "00001000";
			Factory.Save();
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			APInvoiceConsolCostCollection testcollection = new APInvoiceConsolCostCollection(Factory, testInvoicingBase);
			Assert(testcollection.AllowRemove);

			testInvoicingBase = converter.ConvertToAP(testInvoicingBase, false);

			testcollection = new APInvoiceConsolCostCollection(Factory, testInvoicingBase);
			Assert(!testcollection.AllowRemove);

			testInvoicingBase = Factory.New<APInvoice>();
			testcollection = new APInvoiceConsolCostCollection(Factory, testInvoicingBase);
			Assert(testcollection.AllowRemove);
		}

		public void TestIsFinalDefaultedToRegistrySettingWhenConsolCostCreated()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = creator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			factory.Save();
			using (var job = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var testCollection = (APInvoiceConsolCostCollection)GetCollectionToTest();

				AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var consolCost1 = testCollection.AddNew();
				AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var consolCost2 = testCollection.AddNew();

				AssertEquals(true, consolCost1.IsFinal);
				AssertEquals(false, consolCost2.IsFinal);
			}
		}

		public void TestE6_IsTaxAmountOverriddenIsTrue()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = creator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			factory.Save();
			using (var job = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var testCollection = (APInvoiceConsolCostCollection)GetCollectionToTest();
				var consolCost1 = testCollection.AddNew();
				AssertEquals(true, consolCost1.E6_IsTaxAmountOverridden);
			}
		}

		public void TestMutexErrorProcessingWithMessageSubscription()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = creator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			factory.Save();
			using (var job = new Job.Loader(shipment).TryCreateWithMutex())
			{
				AssertNotNull("Precondition: job should be created in another factory.", job);

				string mutexErrorMessage = "";
				int mutexErrorEventRaisedCount = 0;
				var testCollection = (APInvoiceConsolCostCollection)GetCollectionToTest();
				testCollection.OnJobCreationError += (object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e) => { mutexErrorMessage = e.ErrorMessage; mutexErrorEventRaisedCount++; };
				var consolCost = testCollection.TryAddNewForConsol_ForTestOnly(consol);
				AssertType("Precondition: CalculationStrategy type", typeof(JobConsolCost.InvoicingBaseConsolCostCalculationStrategy), consolCost.CalculationStrategy);
				string expectedError = "You have created the job S00001004 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S00001004 to continue.";
				AssertEquals("mutexErrorMessage", expectedError, mutexErrorMessage);
				AssertEquals("mutexErrorEventRaisedCount", 1, mutexErrorEventRaisedCount);

				mutexErrorMessage = "";
				mutexErrorEventRaisedCount = 0;
				testCollection.RemoveAll();
				var anotherTestCollection = (APInvoiceConsolCostCollection)GetCollectionToTest();
				anotherTestCollection.Add(consolCost);
				string anotherMutexErrorMessage = "";
				int anotherMutexErrorEventRaisedCount = 0;
				anotherTestCollection.OnJobCreationError += (object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e) => { anotherMutexErrorMessage = e.ErrorMessage; anotherMutexErrorEventRaisedCount++; };
				AssertType("Precondition: CalculationStrategy type", typeof(JobConsolCost.InvoicingBaseConsolCostCalculationStrategy), consolCost.CalculationStrategy);
				using (consolCost.ReportSettingParentSuspender.GetSuspender())
				{
					consolCost.E6_ParentID = ZGuid.Empty;
					consolCost.E6_ParentID = consol.PK;
				}
				AssertEquals("mutexErrorMessage should not populated because that collection was cleared.", "", mutexErrorMessage);
				AssertEquals("mutexErrorEventRaisedCount", 0, mutexErrorEventRaisedCount);
				AssertEquals("anotherMutexErrorMessage should be populated even if consol cost was added by Add method, not only AddNew.", expectedError, anotherMutexErrorMessage);
				AssertEquals("anotherMutexErrorEventRaisedCount", 1, anotherMutexErrorEventRaisedCount);
			}
		}

		[ExpectException(typeof(JobCreationException))]
		public void TestMutexErrorProcessingWithoutMessageSubscription()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = creator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			factory.Save();
			using (var job = new Job.Loader(shipment).TryCreateWithMutex())
			{
				AssertNotNull("Precondition: job should be created in another factory.", job);

				var testCollection = (APInvoiceConsolCostCollection)GetCollectionToTest();
				var consolCost = testCollection.TryAddNewForConsol_ForTestOnly(consol);
				AssertType("Precondition: CalculationStrategy type", typeof(JobConsolCost.InvoicingBaseConsolCostCalculationStrategy), consolCost.CalculationStrategy);
			}
		}

		public void TestSetDefaultsForNewChild_SupplierCostReference()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_ChequeOrReference = "ABC";
			APInvoiceConsolCostCollection testCollection = new APInvoiceConsolCostCollection(Factory, invoice);
			var consolCost = testCollection.AddNew();
			AssertEquals("Job Consol Cost should have same Supplier Cost Reference as parent AP Invoice", invoice.AH_ChequeOrReference, consolCost.E6_CostReference);
		}

		public void TestSetDefaultsForNewChild_DocumentReceivedDate()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_DocumentReceivedDate = ZDateTime.Today;
			var testCollection = new APInvoiceConsolCostCollection(Factory, invoice);
			var consolCost = testCollection.AddNew();
			AssertEquals("Job Consol Cost should have same Document Received Date as parent AP Invoice", invoice.AH_DocumentReceivedDate, consolCost.E6_DocumentReceivedDate);
		}

		public void TestSetDefaultsForNewChild_ApportionmentMethod()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceConsolCostCollection testCollection = new APInvoiceConsolCostCollection(Factory, invoice);
			var consolCost = testCollection.AddNew();
			AssertNull("PreCondition", consolCost.Consol);
			AssertEquals("Apportionment Method should keep empty when consol is not set", ZString.Empty, consolCost.E6_ApportionmentMethod);

			var newConfig = ConsolCostDefaultApportionmentMethodConfiguration.Create_ForTestOnly(AllocationMethod.GrossWeight);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);
			var consolCost2 = testCollection.AddNew();
			var consol = TestObjectCreator.CreateConsol();
			consolCost2.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			consolCost2.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			AssertNotNull("PreCondition", consolCost2.Consol);
			AssertNotNull("PreCondition", consolCost2.ChargeCode);
			AssertEquals("Job Consol Cost should have same Apportionment Method as Registry", AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.Value.ConsolCostDefaultApportionmentMethodCollection[0].Apportionment, consolCost2.E6_ApportionmentMethod);
		}

		public void TestSetDefaultsForNewChild_CostTaxBranch()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;

			var testCollection = new APInvoiceConsolCostCollection(Factory, invoice);
			var consolCost = testCollection.AddNew();
			AssertNull("PreCondition", consolCost.Consol);
			AssertEquals(GlbBranch.CurrentBranch.PK, consolCost.E6_GB_CostTaxBranch);
		}

		public void TestDefaultingConsolFromPreviousLine()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var collection = GetCollectionToTest();
			var consolCost1 = (JobConsolCost)collection.AddNew();
			var expectedParentId = consol.PK;
			var expectedParentTableCode = consol.TablePrefix;
			consolCost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(expectedParentId, expectedParentTableCode);

			var consolCost2 = (JobConsolCost)collection.AddNew();
			AssertEquals("Single operation: E6_ParentID", expectedParentId, consolCost2.E6_ParentID);
			AssertEquals("Single operation: E6_ParentTableCode", expectedParentTableCode, consolCost2.E6_ParentTableCode);

			using (collection.SuspendListChanged())
			{
				var consolCost3 = (JobConsolCost)collection.AddNew();
				AssertEquals("Bulk operation: E6_ParentID", ZGuid.Empty, consolCost3.E6_ParentID);
				AssertEquals("Bulk operation: E6_ParentTableCode", "", consolCost3.E6_ParentTableCode);
			}
		}

		public void TestSuspendAdditionallyForImport()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var collection = GetCollectionToTest() as APInvoiceConsolCostCollection;
			Assert(!collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateSuspender.IsSuspended);
			AssertEquals(1, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateCount_ForTestOnly);

			using (collection.SuspendAdditionallyForImport())
			{
				var consolCost1 = collection.ParentAPInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				consolCost1.E6_LocalCostAmount = 50m;
				var consolCost2 = collection.ParentAPInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				consolCost2.E6_LocalCostAmount = 100m;

				Assert(collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateSuspender.IsSuspended);
				AssertEquals(1, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateCount_ForTestOnly);
				AssertEquals(0, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.Count);
			}

			Assert(!collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateSuspender.IsSuspended);
			AssertEquals(2, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateCount_ForTestOnly);
			AssertEquals(1, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.Count);
			AssertEquals(150m, collection.ParentAPInvoice.ConsolCosting.ConsolSummary[0].LocalTotalAmount);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
