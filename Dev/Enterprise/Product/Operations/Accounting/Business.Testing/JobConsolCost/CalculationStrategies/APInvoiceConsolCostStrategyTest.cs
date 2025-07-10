using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ConsolCosting.JobConsolCost;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[TestedType(typeof(JobConsolCost))]
	public class APInvoiceConsolCostStrategyTest : JobConsolCostTest
	{
		protected override JobConsolCost GetCost(IJobCostingPlugIn consol)
		{
			return TestAPInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
		}

		public override void TestChequeNumberSetToPaddedNumberOnCharges()
		{
			Assert("Cheque Or Reference isn't used in AP Invoice context", true);
		}

		APInvoice fTestAPInvoice;
		APInvoice TestAPInvoice
		{
			get
			{
				if (fTestAPInvoice == null)
				{
					fTestAPInvoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
				}

				return fTestAPInvoice;
			}
		}

		public void TestAcquireMutexesOnAPInvoiceForm()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(Job)));

			var invoice = Factory.New<APInvoice>();
			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
				using (Job jobHeader = Job.CreateWithMutex(new BusinessObjectFactory(), shipment1))
				{
					AssertNull(jobHeader);
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestJobWithMutexesOnCosolCostIncludePreviouslyCreatedJobs()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			Job shipment1Job = new Job.Loader(Factory, shipment1).Load();
			AssertNull("Should be no job for Shipment1", shipment1Job);
			Job shipment2Job = new Job.Loader(Factory, shipment2).Load();
			AssertNull("Should be no job for Shipment2", shipment2Job);
			using (shipment1Job = new Job.Loader(shipment1).TryCreateWithMutex())
			{
				var invoice = Factory.New<APInvoice>();
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				try
				{
					AssertEquals("Two Jobs with Mutex", 2, cost.JobsWithMutexes.Count);
					AssertEquals("Shipment1 Job", shipment1Job, cost.JobsWithMutexes[0]);
					shipment2Job = new Job.Loader(Factory, shipment2).Load();
					AssertEquals("Shipment1 Job", shipment2Job, cost.JobsWithMutexes[1]);
				}
				finally
				{
					cost.Delete();
					invoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public void TestConsolChangedSuspenderSkipsCreatingNewJobsAndPopulatingDocAddresses()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			Job shipment1Job = new Job.Loader(Factory, shipment1).Load();
			AssertNull("Should be no job for Shipment1", shipment1Job);
			Job shipment2Job = new Job.Loader(Factory, shipment2).Load();
			AssertNull("Should be no job for Shipment2", shipment2Job);
			using (shipment1Job = new Job.Loader(shipment1).TryCreateWithMutex())
			{
				var invoice = Factory.New<APInvoice>();
				var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
				try
				{
					using (cost.GetConsolChangedSuspender())
					{
						var docAddressHitsBefore = Factory.GetTableHitCount(JobDocAddressSchema.Constants.TableName);
						cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
						var docAddressHitsAfter = Factory.GetTableHitCount(JobDocAddressSchema.Constants.TableName);
						AssertEquals("One Job with Mutex", 1, cost.JobsWithMutexes.Count);
						AssertEquals("Shipment1 Job", shipment1Job, cost.JobsWithMutexes[0]);
						shipment2Job = new Job.Loader(Factory, shipment2).Load();
						AssertNull("Should be no job for Shipment2 because of Suspender", shipment2Job);
						AssertEquals("No new hits of the JobDocAddress table because we also suspend setting JobDefaults", docAddressHitsBefore, docAddressHitsAfter);
					}
				}
				finally
				{
					cost.Delete();
					invoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public void TestSetIsUsedForApportionmentIsCalled()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			consol.Shipments.AddNew();
			var invoice = Factory.New<APInvoice>();
			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				Assert(cost.ApportionmentCharges[0].JR_IsUsedForApportionment);
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestSetIsUsedForApportionmentShouldNotChangePostedCost()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "AUSYD", "C0001");
			var shipment1 = ObjectCreator.CreateShipment("S0001", "AUSYD", "AUBNE", consol);
			shipment1.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			var shipment2 = ObjectCreator.CreateShipment("S0002", "AUSYD", "AUBNE", consol);
			shipment2.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			Factory.Save();
			var invoice = ObjectCreator.CreateInvoice(typeof(UAInvoice), "INV1", ObjectCreator.AUD, 1, ObjectCreator.Creditor1);
			invoice.SubmittedFromInvoicingForm = true;
			var cost = ObjectCreator.CreateConsolCost(invoice, consol, ObjectCreator.CC1, 200);
			cost.E6_PPDCLT = Constants.PaymentType.Prepaid;
			var charge1 = cost.ApportionmentCharges.FindChargeForJob(shipment1);
			var charge2 = cost.ApportionmentCharges.FindChargeForJob(shipment2);
			AssertEquals("Precondition: charge 1 JR_PrepaidCollect", Constants.PaymentType.Prepaid, charge1.JR_PrepaidCollect);
			AssertEquals("Precondition: charge 2 JR_PrepaidCollect", Constants.PaymentType.Collect, charge2.JR_PrepaidCollect);
			Assert("Precondition: charge 1 JR_IsUsedForApportionment", charge1.JR_IsUsedForApportionment);
			Assert("Precondition: charge 2 JR_IsUsedForApportionment", !charge2.JR_IsUsedForApportionment);
			cost.ApportionmentCharges.FindChargeForJob(shipment2).JR_IsUsedForApportionment = true;
			invoice.ImportAllApportionmentsFromCosting();
			Factory.Save();
			AssertEquals("Precondition: invoice Lines.Count", 2, invoice.Lines.Count);
			AssertEquals("Precondition: line 1 AL_OSExTaxAmount", 100m, invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Precondition: line 2 AL_OSExTaxAmount", 100m, invoice.Lines[1].AL_OSExTaxAmount);
			ReleaseFactory();
			invoice = Factory.Load<UAInvoice>(invoice.PK);
			invoice.ConsolCosting.ConsolCosts.Load();
			AssertEquals("Precondition: invoice ConsolCosts Count", 1, invoice.ConsolCosting.ConsolCosts.Count);
			cost = invoice.ConsolCosting.ConsolCosts[0];
			Assert("Precondition: cost is posted", cost.IsPosted);
			AssertEquals("ApportionmentCharges.Count", 2, cost.ApportionmentCharges.Count);
			AssertEquals("charge 1 JR_IsUsedForApportionment", true, cost.ApportionmentCharges.FindChargeForJob(shipment1).JR_IsUsedForApportionment);
			AssertEquals("charge 2 JR_IsUsedForApportionment", true, cost.ApportionmentCharges.FindChargeForJob(shipment2).JR_IsUsedForApportionment);
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
				var consolCost = Factory.New<JobConsolCost>();
				using (consolCost.ReportSettingParentSuspender.GetSuspender())
				{
					consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
				}

				consolCost.APInvoiceConsolCostingJobCreationError += (object sender, APInvoiceCostingJobCreationErrorEventArgs e) => mutexErrorMessage = e.ErrorMessage;
				string expectedError = "You have created the job S00001004 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S00001004 to continue.";
				var testStrategy = new InvoicingBaseConsolCostCalculationStrategy(consolCost);
				testStrategy.UpdateApportionmentChargesListing();
				AssertEquals("mutexErrorMessage", expectedError, mutexErrorMessage);
			}
		}

		public void TestReversingConsolCostAPInvoiceDoesNotCreateJobsForSubShipments()
		{
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var shipment = creator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			var jobHandler = new LocalClientJobHandler(shipment);
			jobHandler.Initialize();
			Assert(string.IsNullOrEmpty(jobHandler.InitializationMessage));

			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "ASM";
			factory.Save();

			AssertNotNull(shipment.Job);

			var subShipment1 = creator.CreateShipmentWithCoLoadMaster("S00001002", "AUSYD", "NZAKL", shipment);
			jobHandler = new LocalClientJobHandler(subShipment1);
			jobHandler.Initialize();
			AssertEquals("Automated creation of Job Header.", jobHandler.InitializationMessage);

			var subShipment2 = creator.CreateShipmentWithCoLoadMaster("S00001003", "AUSYD", "NZAKL", shipment);
			jobHandler = new LocalClientJobHandler(subShipment2);
			jobHandler.Initialize();
			AssertEquals("Automated creation of Job Header.", jobHandler.InitializationMessage);

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			consol.Shipments.Add(shipment);
			factory.Save();

			var consolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			consolCost.E6_OH_Creditor = creator.Creditor1.PK;
			consolCost.E6_AC_ChargeCode = creator.CC1.PK;
			consolCost.E6_OSCostAmount = 5483;
			consolCost.E6_PPDCLT = "ALL";
			consolCost.E6_ApportionmentMethod = AllocationMethod.TwentyFootEquivalentUnit;

			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[0].JR_E6 = consolCost.PK;
			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 5483;
			consolCost.E6_AT_TaxRate = creator.KDV18.PK;
			consolCost.E6_InvoiceNum = "APINV001";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			AddJobCharge(shipment.Job as Job, consolCost);

			factory.Save();

			var postManager = new ConsolInvoicingPostManager(factory, new Job[] { shipment.Job as Job }, consol);
			var result = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			factory.Save();

			var apInvoice = result.Values.ToList<APInvoice>().FirstOrDefault();

			var newFactory = new BusinessObjectFactory();
			var invoiceReverser = new ReversingFactory().NewReversing(newFactory.Load<APInvoice>(apInvoice.PK));
			invoiceReverser.Reverse();
			var apCreditNote = invoiceReverser.ReverseTransaction as APCreditNote;
			apCreditNote.AH_TransactionNum = "APCRD01";
			apCreditNote.Factory.Save();

			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			AssertNotNull(shipment.Job);

			subShipment1 = newFactory.Load<ForwardingShipment>(subShipment1.PK);
			AssertNull(subShipment1.Job);
			AssertHasRowWarning(subShipment1, "Automated creation of Job Header.");

			subShipment2 = newFactory.Load<ForwardingShipment>(subShipment2.PK);
			AssertNull(subShipment2.Job);
			AssertHasRowWarning(subShipment2, "Automated creation of Job Header.");
		}

		public void TestTraceMessageCollectedInsideUpdateApportionmentChargesListing()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = creator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job = new Job.Loader(shipment).TryCreateWithMutex();
			Factory.Save();

			var consolCost = Factory.New<JobConsolCost>();
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}

			var testStrategy = new InvoicingBaseConsolCostCalculationStrategy(consolCost);
			testStrategy.UpdateApportionmentChargesListing();

			var actualTraceMessage = string.Join("", dummyTracer.Traces);
			AssertContains("Traced @Enterprise.Accounting.Business.ConsolCosting.JobConsolCost.ShipmentsToApportion", actualTraceMessage);
			AssertContains("Traced @Enterprise.Accounting.Business.ConsolCosting.JobConsolCost+InvoicingBaseConsolCostCalculationStrategy.UpdateApportionmentChargesListing- after calling base.UpdateApportionmentChargesListing", actualTraceMessage);
			AssertContains("Traced @Enterprise.Accounting.Business.ConsolCosting.JobConsolCost+InvoicingBaseConsolCostCalculationStrategy.UpdateApportionmentChargesListing- after calling Cost.SplitApportionAmount", actualTraceMessage);
			AssertContains("Traced @Enterprise.Accounting.Business.ConsolCosting.JobConsolCost+InvoicingBaseConsolCostCalculationStrategy.UpdateApportionmentChargesListing- after calling Cost.SetIsUsedForApportionment", actualTraceMessage);
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
				var consolCost = Factory.New<JobConsolCost>();
				using (consolCost.ReportSettingParentSuspender.GetSuspender())
				{
					consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
				}

				var testStrategy = new InvoicingBaseConsolCostCalculationStrategy(consolCost);
				testStrategy.UpdateApportionmentChargesListing();
			}
		}

		void AddJobCharge(Job job, JobConsolCost cost)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_AC = cost.ChargeCode.PK;
		}

		public void TestOnE6_ParentIDConsolSetHandlesSingleMatchedConsolCostWithValidCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);
			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;
			var unpostedConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			unpostedConsolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			unpostedConsolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			unpostedConsolCost.E6_OSCostAmount = 100m;
			unpostedConsolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
			unpostedConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[0].JR_E6 = unpostedConsolCost.PK;
			unpostedConsolCost.ApportionmentCharges[0].JR_OSCostAmt = 90m;
			AddJobCharge(shipment1Job, unpostedConsolCost);
			unpostedConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[1].JR_E6 = unpostedConsolCost.PK;
			unpostedConsolCost.ApportionmentCharges[1].JR_OSCostAmt = 10m;
			AddJobCharge(shipment2Job, unpostedConsolCost);
			Factory.Save();
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;
			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			AssertEquals("Precondition", ZString.Empty, cost.E6_ApportionmentMethod);
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[1].JR_OSCostAmt);
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			AssertEquals("Postcondition - matched with charge code + creditor", "MAN", cost.E6_ApportionmentMethod);
			AssertEquals("Postcondition", 90m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Postcondition", 10m, cost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestOnE6_ParentIDConsolSetHandlesSingleMatchedConsolCostWithBlankCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);
			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;
			var unpostedConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			unpostedConsolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			unpostedConsolCost.E6_OSCostAmount = 100m;
			unpostedConsolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			unpostedConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[0].JR_E6 = unpostedConsolCost.PK;
			unpostedConsolCost.ApportionmentCharges[0].JR_OSCostAmt = 50m;
			AddJobCharge(shipment1Job, unpostedConsolCost);
			unpostedConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[1].JR_E6 = unpostedConsolCost.PK;
			unpostedConsolCost.ApportionmentCharges[1].JR_OSCostAmt = 50m;
			AddJobCharge(shipment2Job, unpostedConsolCost);
			Factory.Save();
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;
			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			AssertEquals("Precondition", ZString.Empty, cost.E6_ApportionmentMethod);
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[1].JR_OSCostAmt);
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			AssertEquals("Postcondition - matched with charge code + blank creditor", "SHP", cost.E6_ApportionmentMethod);
			AssertEquals("Postcondition", 50m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Postcondition", 50m, cost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestOnE6_ParentIDConsolSetHandlesMultipleMatchedConsolCostWithDifferentCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);
			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;
			var unpostedConsolCost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			unpostedConsolCost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			unpostedConsolCost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			unpostedConsolCost1.E6_OSCostAmount = 100m;
			unpostedConsolCost1.E6_ApportionmentMethod = AllocationMethod.Manual;
			unpostedConsolCost1.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			unpostedConsolCost1.ApportionmentCharges[0].JR_E6 = unpostedConsolCost1.PK;
			unpostedConsolCost1.ApportionmentCharges[0].JR_OSCostAmt = 90m;
			AddJobCharge(shipment1Job, unpostedConsolCost1);
			unpostedConsolCost1.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			unpostedConsolCost1.ApportionmentCharges[1].JR_E6 = unpostedConsolCost1.PK;
			unpostedConsolCost1.ApportionmentCharges[1].JR_OSCostAmt = 10m;
			AddJobCharge(shipment2Job, unpostedConsolCost1);
			var unpostedConsolCost2 = consol.GetApportionments().CostsCollection.TryAddNew();
			unpostedConsolCost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			unpostedConsolCost2.E6_OSCostAmount = 100m;
			unpostedConsolCost2.E6_ApportionmentMethod = AllocationMethod.Shipment;
			unpostedConsolCost2.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			unpostedConsolCost2.ApportionmentCharges[0].JR_E6 = unpostedConsolCost2.PK;
			unpostedConsolCost2.ApportionmentCharges[0].JR_OSCostAmt = 50m;
			AddJobCharge(shipment1Job, unpostedConsolCost2);
			unpostedConsolCost2.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			unpostedConsolCost2.ApportionmentCharges[1].JR_E6 = unpostedConsolCost2.PK;
			unpostedConsolCost2.ApportionmentCharges[1].JR_OSCostAmt = 50m;
			AddJobCharge(shipment2Job, unpostedConsolCost2);
			Factory.Save();
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;
			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			AssertEquals("Precondition", ZString.Empty, cost.E6_ApportionmentMethod);
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[1].JR_OSCostAmt);
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			AssertEquals("Postcondition - multiple matched, take matched creditor before the blank creditor", "MAN", cost.E6_ApportionmentMethod);
			AssertEquals("Postcondition", 90m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Postcondition", 10m, cost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestOnE6_ParentIDConsolSetHandlesMultipleMatchedConsolCostWithOneValidAndMultipleEmptyCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);

			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;

			var unpostedConsolCost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost1, AllocationMethod.Manual, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 90M, 10M);

			var unpostedConsolCost2 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost2, AllocationMethod.Shipment, ZGuid.Empty, shipment1Job, shipment2Job, 50M, 50M);

			var unpostedConsolCost3 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost3, AllocationMethod.ChargeableUnits, ZGuid.Empty, shipment1Job, shipment2Job, 190M, 10M);

			var unpostedConsolCost4 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost4, AllocationMethod.ChargeableUnits, ObjectCreator.ABIGAS.PK, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost5 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost5, AllocationMethod.ChargeableUnits, ZGuid.Empty, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost6 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost6, AllocationMethod.ChargeableUnits, ObjectCreator.Creditor1.PK, shipment1Job, shipment2Job, 290M, 10M);

			Factory.Save();

			var consolCostSelectionFormPoppedUp = false;
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			invoice.ConsolCosting.ConsolCosts.OnConsolChanged += (sender, e) => consolCostSelectionFormPoppedUp = true;

			AssertEquals("Precondition", ZString.Empty, cost.E6_ApportionmentMethod);
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[1].JR_OSCostAmt);

			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;

			AssertEquals("Postcondition - Consol Cost Selection Form pops up", false, consolCostSelectionFormPoppedUp);
			AssertEquals("Postcondition - multiple matched, take matched creditor before the AALSHI creditor", "MAN", cost.E6_ApportionmentMethod);
			AssertEquals("Postcondition", 90m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Postcondition", 10m, cost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestOnE6_ParentIDConsolSetHandlesMultipleMatchedConsolCostWithNoValidAndOneEmptyCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);

			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;

			var unpostedConsolCost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost1, AllocationMethod.Manual, ZGuid.Empty, shipment1Job, shipment2Job, 90M, 10M);

			var unpostedConsolCost2 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost2, AllocationMethod.Shipment, ObjectCreator.ABIGAS.PK, shipment1Job, shipment2Job, 50M, 50M);

			var unpostedConsolCost3 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost3, AllocationMethod.ChargeableUnits, ObjectCreator.ABIGAS.PK, shipment1Job, shipment2Job, 190M, 10M);

			var unpostedConsolCost4 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost4, AllocationMethod.ChargeableUnits, ObjectCreator.ABIGAS.PK, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost5 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost5, AllocationMethod.ChargeableUnits, ObjectCreator.ABIGAS.PK, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost6 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost6, AllocationMethod.ChargeableUnits, ObjectCreator.Creditor1.PK, shipment1Job, shipment2Job, 290M, 10M);

			Factory.Save();

			var consolCostSelectionFormPoppedUp = false;
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			invoice.ConsolCosting.ConsolCosts.OnConsolChanged += (sender, e) => consolCostSelectionFormPoppedUp = true;

			AssertEquals("Precondition", ZString.Empty, cost.E6_ApportionmentMethod);
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[1].JR_OSCostAmt);

			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;

			AssertEquals("Postcondition - Consol Cost Selection Form pops up", false, consolCostSelectionFormPoppedUp);
			AssertEquals("Postcondition - multiple matched, take matched creditor before the AALSHI creditor", "MAN", cost.E6_ApportionmentMethod);
			AssertEquals("Postcondition", 90m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Postcondition", 10m, cost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestOnE6_ParentIDConsolSetHandlesMultipleMatchedConsolCostWithBothValidAndEmptyCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);

			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;

			var unpostedConsolCost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost1, AllocationMethod.Manual, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 90M, 10M);

			var unpostedConsolCost2 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost2, AllocationMethod.Shipment, ZGuid.Empty, shipment1Job, shipment2Job, 50M, 50M);

			var unpostedConsolCost3 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost3, AllocationMethod.Manual, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 190M, 10M);

			var unpostedConsolCost4 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost4, AllocationMethod.ChargeableUnits, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost5 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost5, AllocationMethod.ChargeableUnits, ZGuid.Empty, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost6 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost6, AllocationMethod.ChargeableUnits, ObjectCreator.Creditor1.PK, shipment1Job, shipment2Job, 290M, 10M);

			Factory.Save();

			var consolCostSelectionFormPoppedUp = false;
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			invoice.ConsolCosting.ConsolCosts.OnConsolChanged += (sender, e) => consolCostSelectionFormPoppedUp = true;

			AssertEquals("Precondition", ZString.Empty, cost.E6_ApportionmentMethod);
			AssertEquals("Precondition", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Precondition", 0m, cost.ApportionmentCharges[1].JR_OSCostAmt);

			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;

			AssertEquals("Postcondition - Consol Cost Selection Form pops up", true, consolCostSelectionFormPoppedUp);
		}

		public void TestOnE6_ParentIDConsolSetHandlesMultipleMatchedConsolCost_SuspendAdditionallyForImport()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);

			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;

			var unpostedConsolCost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost1, AllocationMethod.Manual, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 90M, 10M);

			var unpostedConsolCost2 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost2, AllocationMethod.Shipment, ZGuid.Empty, shipment1Job, shipment2Job, 50M, 50M);

			var unpostedConsolCost3 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost3, AllocationMethod.Manual, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 190M, 10M);

			var unpostedConsolCost4 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost4, AllocationMethod.ChargeableUnits, ObjectCreator.AALSHI.PK, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost5 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost5, AllocationMethod.ChargeableUnits, ZGuid.Empty, shipment1Job, shipment2Job, 290M, 10M);

			var unpostedConsolCost6 = consol.GetApportionments().CostsCollection.TryAddNew();
			SetupConsolCost(unpostedConsolCost6, AllocationMethod.ChargeableUnits, ObjectCreator.Creditor1.PK, shipment1Job, shipment2Job, 290M, 10M);

			Factory.Save();

			var consolCostSelectionFormPoppedUp = false;
			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.AALSHI.PK;

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			invoice.ConsolCosting.ConsolCosts.OnConsolChanged += (sender, e) => consolCostSelectionFormPoppedUp = true;

			using (invoice.ConsolCosting.ConsolCosts.SuspendAdditionallyForImport())
			{
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			}

			AssertEquals("Postcondition - Consol Cost Selection Form pops up", false, consolCostSelectionFormPoppedUp);
		}

		void SetupConsolCost(JobConsolCost cst, ZString appMethod, ZGuid creditorPK, Job shipment1Job, Job shipment2Job, ZDecimal appAmount1, ZDecimal appAmount2)
		{
			cst.E6_OH_Creditor = creditorPK;
			cst.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cst.E6_OSCostAmount = appAmount1 + appAmount2;
			cst.E6_ApportionmentMethod = appMethod;

			cst.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			cst.ApportionmentCharges[0].JR_E6 = cst.PK;
			cst.ApportionmentCharges[0].JR_OSCostAmt = appAmount1;
			AddJobCharge(shipment1Job, cst);

			cst.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			cst.ApportionmentCharges[1].JR_E6 = cst.PK;
			cst.ApportionmentCharges[1].JR_OSCostAmt = appAmount2;
			AddJobCharge(shipment2Job, cst);
		}
	}
}
