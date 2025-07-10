using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	public class JobBatchInvoicingPostManagerGUIWrapperTest : BaseBatchInvoicingPostManagerGUIWrapperTest
	{
		public void TestBatchPrintJobInvoicing()
		{
			var testARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "TESTINV001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testARInvoice1.IsSelfBillingInvoice = true;
			testARInvoice1.AH_ConsolidatedInvoiceRef = "JOBNUMBER1";
			var testARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "TESTINV002", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testARInvoice2.IsSelfBillingInvoice = true;
			testARInvoice2.AH_ConsolidatedInvoiceRef = "JOBNUMBER2";
			Factory.Save();

			AssertEquals("InvoiceNumber as expected", "JOBNUMBER1", testARInvoice1.InvoiceNumber);
			AssertEquals("InvoiceNumber as expected", "JOBNUMBER2", testARInvoice2.InvoiceNumber);

			Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Add(testARInvoice2.PK);
			Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Add(testARInvoice1.PK);
			JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(Wrapper.BulkPostingDataCollector_ForTestOnly);
			Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			string message = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertStartsWith("The last message should ask user if they wish to print AR Invoice", "Do you want to print the following invoices?", message);
			AssertContains("Invoice number should be in printing message", testARInvoice1.InvoiceNumber, message);
			AssertContains("Invoice number should be in printing message", testARInvoice2.InvoiceNumber, message);
			Assert("Invoices must be in their PKs order", message.IndexOf(testARInvoice2.InvoiceNumber) < message.IndexOf(testARInvoice1.InvoiceNumber));
			Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var testAPInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TESTINV003", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testAPInvoice1.IsSelfBillingInvoice = true;
			var testAPInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TESTINV004", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testAPInvoice2.IsSelfBillingInvoice = true;
			Factory.Save();
			Wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.Add(testAPInvoice2.PK);
			Wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.Add(testAPInvoice1.PK);
			JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(Wrapper.BulkPostingDataCollector_ForTestOnly);
			Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			message = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertStartsWith("The last message should ask user if they wish to print Self Billing AP Invoice", "Do you want to print the following Self Billing Invoices?", message);
			AssertContains("Invoice number should be in printing message", testAPInvoice1.InvoiceNumber, message);
			AssertContains("Invoice number should be in printing message", testAPInvoice2.InvoiceNumber, message);
			Assert("Invoices must be in their PKs order", message.IndexOf(testAPInvoice2.InvoiceNumber) < message.IndexOf(testAPInvoice1.InvoiceNumber));
			Wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			try
			{
				AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var testAPInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TESTINV005", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
				testAPInvoice3.IsSelfBillingInvoice = false;
				var testAPInvoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TESTINV006", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
				testAPInvoice4.IsSelfBillingInvoice = false;
				Factory.Save();
				Wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.Add(testAPInvoice3.PK);
				Wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.Add(testAPInvoice4.PK);
				JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(Wrapper.BulkPostingDataCollector_ForTestOnly);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("The last message should ask user if they wish to print Self Billing AP Invoice", "Do you want to print Cost Confirmation Documents?", message);
				Wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestBatchPrintJobARInvoicingWithNonJobBasedInvoiceNumber()
		{
			using (AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var testARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "TESTINV001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
				testARInvoice1.IsSelfBillingInvoice = true;
				testARInvoice1.AH_ConsolidatedInvoiceRef = "JOBNUMBER1";
				var testARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "TESTINV002", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
				testARInvoice2.IsSelfBillingInvoice = true;
				testARInvoice2.AH_ConsolidatedInvoiceRef = "JOBNUMBER2";
				Factory.Save();

				AssertNotEquals("InvoiceNumber is not Job Invoice Number", "JOBNUMBER1", testARInvoice1.InvoiceNumber);
				AssertNotEquals("InvoiceNumber  is not Job Invoice Number", "JOBNUMBER2", testARInvoice2.InvoiceNumber);

				Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Add(testARInvoice2.PK);
				Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Add(testARInvoice1.PK);
				JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(Wrapper.BulkPostingDataCollector_ForTestOnly);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				string messageAR = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertStartsWith("The last message should ask user if they wish to print AR Invoice", "Do you want to print the following invoices?", messageAR);
				AssertContains("Invoice number should be in printing message", testARInvoice1.InvoiceNumber, messageAR);
				AssertContains("Invoice number should be in printing message", testARInvoice2.InvoiceNumber, messageAR);
				Assert("Invoices must be in their PKs order", messageAR.IndexOf(testARInvoice2.InvoiceNumber) < messageAR.IndexOf(testARInvoice1.InvoiceNumber));
				Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestBatchPrintJobARInvoicingWithReprintingErrorMessage()
		{
			using (AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var testARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "TESTINV001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
				testARInvoice1.IsSelfBillingInvoice = true;
				testARInvoice1.AH_ConsolidatedInvoiceRef = "JOBNUMBER1";
				var testARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "TESTINV002", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
				testARInvoice2.IsSelfBillingInvoice = true;
				testARInvoice2.AH_ConsolidatedInvoiceRef = "JOBNUMBER2";
				Factory.Save();

				AssertNotEquals("InvoiceNumber is not Job Invoice Number", "JOBNUMBER1", testARInvoice1.InvoiceNumber);
				AssertNotEquals("InvoiceNumber is not Job Invoice Number", "JOBNUMBER2", testARInvoice2.InvoiceNumber);

				AssertEquals("AR Invoice1 should not have any document in EDocs", 0, testARInvoice1.DocManagerInfo.AllEDocs.Count);
				AssertEquals("AR Invoice2 should not have any document in EDocs", 0, testARInvoice2.DocManagerInfo.AllEDocs.Count);
				Assert("CanPrint Invoice1", testARInvoice1.CheckCanPrintPostedInvoicingBase().Result);
				Assert("CanPrint Invoice2", testARInvoice2.CheckCanPrintPostedInvoicingBase().Result);

				Assert("Attaching Invoice1 should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(testARInvoice1, new NotificationBuffer()));
				Assert("Attaching Invoice2 should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(testARInvoice2, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice1 should have invoice attached in EDocs", 1, testARInvoice1.DocManagerInfo.AllEDocs.Count);
				AssertEquals("AR Invoice2 should have invoice attached in EDocs", 1, testARInvoice2.DocManagerInfo.AllEDocs.Count);

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Add(testARInvoice2.PK);
				Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Add(testARInvoice1.PK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(Wrapper.BulkPostingDataCollector_ForTestOnly);

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(AccountingConstants.ReprintingInvoiceMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestConstructor()
		{
			JobInvoicingPostingOption expectedPostingOption = JobInvoicingPostingOption.All;
			BusinessObjectFactory expectedFactory = new BusinessObjectFactory();
			IJobInvoicingPlugIn[] expectedShipments = new IJobInvoicingPlugIn[] { ShipmentA, ShipmentB };
			ZString expectedPostingOptionName = "Selected Posting Option Name";
			ZString expectedPostedObjectName = "Posted Object Name";

			base.Wrapper = new JobBatchInvoicingPostManagerGUIWrapper(expectedPostingOption, expectedShipments, expectedPostingOptionName, expectedPostedObjectName);

			AssertEquals(expectedPostingOption, Wrapper.PostingOption_ForTestOnly);
			AssertEquals("PlugInFactory_ForTestOnly.RefreshEnabled should be disabled for performance reasons", false, Wrapper.PlugInFactory_ForTestOnly.RefreshEnabled);
			AssertEquals(expectedShipments.Length, Wrapper.JobCollection_ForTestOnly.Length);
			AssertEquals(expectedPostingOptionName, Wrapper.SelectedPostingOptionName);
			AssertEquals(expectedPostedObjectName, Wrapper.PostedObjectName);

			foreach (Job job in Wrapper.JobCollection_ForTestOnly)
			{
				AssertNotNull(job.Parent);
				AssertEquals("Should not initialize the job default values", false, job.DefaultValuesHasBeenAssigned);
			}
		}

		public void TestPostedObjectName()
		{
			ZString specifiedName = "This is the Name of Posted Business Object";
			BaseBatchInvoicingPostManagerGUIWrapper wrapperWithNameSpecified;
			wrapperWithNameSpecified = GetGuiWrapperWithTwoObjectsToPost(specifiedName);
			AssertEquals("PostedObject name should be set correctly", specifiedName, wrapperWithNameSpecified.PostedObjectName);

			BaseBatchInvoicingPostManagerGUIWrapper wrapperWithDefaultName;
			wrapperWithDefaultName = GetGuiWrapperWithTwoObjectsToPost("");
			AssertEquals("DefaultPostedObjectName_ForTestOnly should be used", Wrapper.DefaultPostedObjectName_ForTestOnly, wrapperWithDefaultName.PostedObjectName);
		}

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetGuiWrapperWithOneObjectToPost(string postedBusinessObjectName)
		{
			IJobInvoicingPlugIn[] jobInvoicingJobs = new IJobInvoicingPlugIn[] { ShipmentA };
			var wrapper = new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, jobInvoicingJobs, "Selected Posting Option Name", postedBusinessObjectName);
			wrapper.DoTestPostTransactions = true;
			return wrapper;
		}

		protected override IJobInvoicingPlugIn GUIWrapperPlugIn
		{
			get
			{
				return ShipmentA;
			}
		}

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetGuiWrapperWithTwoObjectsToPost(string postedBusinessObjectName, bool isDebtorValid = true)
		{
			JobInvoicingJobs = (isDebtorValid) ? new IJobInvoicingPlugIn[] { ShipmentA, ShipmentB } : new IJobInvoicingPlugIn[] { Shipment1, Shipment2 };
			return new TestJobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, JobInvoicingJobs, "Selected Posting Option Name", postedBusinessObjectName);
		}

		public override void TestCurrentObjectCode()
		{
			Wrapper.Post();
			AssertEquals("CurrentObjectCode_ForTestOnly should be set to the Job code", "S00001002", Wrapper.CurrentObjectCode_ForTestOnly);
		}

		public override void TestDefaultPostedObjectName()
		{
			AssertEquals("Default Posted Object Name", "Job", Wrapper.DefaultPostedObjectName_ForTestOnly);
		}

		public void TestGetPostManagerValidation()
		{
			PostManagerValidation validation = Wrapper.GetPostManagerValidation_ForTestOnly(Wrapper.Jobs_ForTestOnly, JobInvoicingPostingOption.All, Wrapper.OriginalJobs_ForTestOnly);
			AssertEquals("Validation object should be of correct type", validation.GetType(), typeof(PostManagerValidation));
			Assert("IsBulkPosting value should be true", validation.IsBulkPosting_ForTestOnly);
		}

		[TestDate(2005, 9, 10)]
		public void TestCannotPostInvoicesFromJobWithWorkOnHold()
		{
			TestCaseHelper.ClearTable(AccTransactionHeader.Schema.TableName);

			JobA.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			JobB.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			Wrapper.Post();

			var baseQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			var query = new ZQuery(baseQuery);
			query.AddToFilter(AccTransactionHeaderSchema.AH_JH, JobA.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JobA.JH_GC);
			ARInvoice[] jobAInvoices = Factory.Load<ARInvoice>(query);
			query = new ZQuery(baseQuery);
			query.AddToFilter(AccTransactionHeaderSchema.AH_JH, JobB.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JobB.JH_GC);
			ARInvoice[] jobBInvoices = Factory.Load<ARInvoice>(query);

			AssertEquals("Job A Invoices (Status: Invoicing On Hold)", 0, jobAInvoices.Length);
			AssertEquals("Job B Invoices (Status: Working)", 1, jobBInvoices.Length);

			AssertEquals("Posted Invoices", 1, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			AssertEquals("Posted Invoice Job should be Job B (Status = Working)", JobB.PK, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].Job.PK);
		}

		protected override void AssertOverrideTransactionDescription(bool shouldOverride)
		{
			int overrideTransactionDescriptionFormShowedCount = 0;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
			{
				OverrideTransactionDescriptionForm form = (OverrideTransactionDescriptionForm)formOrDialog;
				OverrideTransactionDescriptionHelper bizo = (OverrideTransactionDescriptionHelper)form.BusinessEntity;
				form.Shown += new EventHandler((sender, e) =>
				{
					foreach (InvoicingBase invoice in bizo.WrappedObjects)
					{
						invoice.AH_Desc = "My test description";
					}
					form.FireSaveButton();
				});
				overrideTransactionDescriptionFormShowedCount++;
			});
			Wrapper.Post();

			AssertEquals("Should have created 2 invoices", 2, Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Count);
			if (shouldOverride)
			{
				AssertType("LastFormShownDialogForTest", typeof(OverrideTransactionDescriptionForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("OverrideTransactionDescriptionForm should be shown one time per posting.", 1, overrideTransactionDescriptionFormShowedCount);
				var allPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs));
				AssertEquals("Should have created one invoice", "My test description", allPostedInvoices[0].AH_Desc);
				AssertEquals("Should have created one invoice", "My test description", allPostedInvoices[1].AH_Desc);
			}
			else
			{
				AssertEquals("LastFormShownDialogForTest should not be shouwn", null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestOverrideTransactionDescriptionDoesntAppearIfNothingPosted()
		{
			JobA.Charges.RemoveAndDeleteAll();
			JobB.Charges.RemoveAndDeleteAll();
			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			GUIWrapper.Post();

			AssertEquals("Should have created no invoices", 0, Wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs.Count);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
		}

		public override void TestCreditLimitEmailsAreSentWhenPosting()
		{
			SetupForCreditLimitEmails();

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			ForwardingShipment shipment1 = SetupShipmentWithMultipleInvoices(TestObjectCreator.ABIGAS.PK);
			ForwardingShipment shipment2 = SetupShipmentWithMultipleInvoices(TestObjectCreator.ABIGAS.PK);

			Factory.Save();
			IJobInvoicingPlugIn[] jobs = new IJobInvoicingPlugIn[] { shipment1, shipment2 };

			JobBatchInvoicingPostManagerGUIWrapper wrapper = new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, jobs, "AAA", "BBB");
			Factory.Save();
			wrapper.Post();

			AssertEquals("4 Emails should be sent", 4, Env.OutgoingMailManager.EmailsCreated.Count);
			var aPPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs));
			var aRPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs));
			AssertEquals("Should have created 12 invoices", 12, aPPostedInvoices.Length + aRPostedInvoices.Length);
		}

		public void TestBatchJobInvoicingPost_NotModifyingExistingJobs()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			var voyageRate = voyage.ExRates.AddNew();
			voyageRate.E8_RX_NKExCurrency = "USD";
			voyageRate.E8_VoyageExchangeRate = 1.34m;
			voyage.GenerateSailings();

			Factory.Save();

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			var job = TestObjectCreator.CreateJob(billOfLading, false);
			billOfLading.JS_JX = voyage.Sailings[0].PK;
			job.PlugInData = billOfLading;

			var jobRate = job.ExchangeRates.FindByRefCurrency(voyageRate.ExCurrency);
			AssertNotNull("Generic Job Exchange Rate should be found", jobRate);
			AssertEquals("JF_BaseRate", voyageRate.E8_VoyageExchangeRate, jobRate.JF_BaseRate);
			Assert("Precondition: No OrgType", jobRate.JF_OrgType.IsEmpty);
			Assert("Precondition: No Org", jobRate.JF_OH_Org.IsEmpty);

			jobRate.JF_OH_Org = TestObjectCreator.ABIGAS.PK;
			jobRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			Assert("OrgType", !jobRate.JF_OrgType.IsEmpty);
			Assert("Org", !jobRate.JF_OH_Org.IsEmpty);

			var chargeA = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Description", AUD, 100m, TestObjectCreator.AALSHI, USD, 150m, TestObjectCreator.ABIGAS);
			chargeA.JR_APInvoiceNum = "INV12";
			chargeA.JR_APInvoiceDate = ZDateTime.Today;
			chargeA.JR_InvoiceType = AgencyInvoiceTypesList.Codes.Misc;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().Addresses[0].PK;
			job.JH_OA_AgentCollectAddr = Factory.NewWithValidTestData<OrgHeader>().Addresses[0].PK;
			Factory.Save();

			IJobInvoicingPlugIn[] jobInvoicingJobs = new IJobInvoicingPlugIn[] { billOfLading };
			Assert("Job HasChanges is false", !job.HasChanges);
			var wrapper = new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, jobInvoicingJobs, "All", "BillOfLading");
			wrapper.Post();

			Assert("After Posting Job HasChanges should be false", !wrapper.OriginalJobs_ForTestOnly.FirstOrDefault().HasChanges);
			Assert("Should create an invoice", wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count == 1);
			AssertEquals("The error message should be empty", ZString.Empty, wrapper.CurrentObjectMessageStack_ForTestOnly);
		}

		#region InvoicePostingExchangeRateOption
		[TestDate(2015, 5, 10)]
		public override void TestPostWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupForInvoicePostingExchangeRateOption();

			var shipmentPK1 = CreateShipmentForInvoicePostingExchangeRateOption();
			var shipmentPK2 = CreateShipmentForInvoicePostingExchangeRateOption();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment1 = newFactory.Load<ForwardingShipment>(shipmentPK1);
			var shipment2 = newFactory.Load<ForwardingShipment>(shipmentPK2);

			IJobInvoicingPlugIn[] jobs = new IJobInvoicingPlugIn[] { shipment1, shipment2 };
			JobBatchInvoicingPostManagerGUIWrapper wrapper = new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, jobs, "AAA", "BBB");
			wrapper.Post();

			AssertShipmentAndAPInvoiceForInvoicePostingExchangeRateOption(shipment1.PK);
			AssertShipmentAndAPInvoiceForInvoicePostingExchangeRateOption(shipment2.PK);

			//AR
			newFactory = new BusinessObjectFactory();
			shipment1 = newFactory.Load<ForwardingShipment>(shipmentPK1);
			shipment2 = newFactory.Load<ForwardingShipment>(shipmentPK2);

			jobs = new IJobInvoicingPlugIn[] { shipment1, shipment2 };
			wrapper = new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, jobs, "AAA", "BBB");
			wrapper.Post();
			AssertShipmentAndARInvoiceForInvoicePostingExchangeRateOption(shipmentPK1);
			AssertShipmentAndARInvoiceForInvoicePostingExchangeRateOption(shipmentPK2);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public override void TestBackDatingARAPInvoiceWithInvoicePostingExchangeRateOption()
		{
			Assert("N/a", true);
		}

		#endregion

		protected override ZString GetExpectedMessageForJobOnHold()
		{
			return @"You cannot post because this job is on hold.
To post, change the job status from 'WHL'.";
		}

		#region Implementation

		IJobInvoicingPlugIn[] JobInvoicingJobs;

		protected override void AssertParentIDIsSetToParentJob(ZString description, ZGuid parentId)
		{
			AssertEquals(description, parentId, ((ForwardingShipment)(JobInvoicingJobs[0])).Job.PK);
		}

		protected override void AssertParentTableCode(ZString description, ZString actualTableCode)
		{
			AssertEquals(description, JobHeaderSchema.Constants.Prefix, actualTableCode);
		}

		new JobBatchInvoicingPostManagerGUIWrapper Wrapper
		{
			get { return base.Wrapper as JobBatchInvoicingPostManagerGUIWrapper; }
		}

		class TestJobBatchInvoicingPostManagerGUIWrapper : JobBatchInvoicingPostManagerGUIWrapper
		{
			public TestJobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption,
IJobInvoicingPlugIn[] jobCollection,
ZString selectedPostingOptionName,
ZString postedObjectName)
					: base(postingOption, jobCollection, selectedPostingOptionName, postedObjectName)
			{
				DoTestPostTransactions = true;
			}

			protected override void PrintInvoices(TransactionCreatorHashtable transactions)
			{
			}
		}

		#endregion
	}
}
