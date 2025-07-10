using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.GUI.Testing.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(InvoicePreviewForm))]
	public class InvoicePreviewFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ChargePoster poster = new ChargePoster(Factory);
			var previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.All);
			return new InvoicePreviewForm(previewer);
		}

		public void TestInvoicePreviewForm_ClosedBeforeInvoicePreview()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;

			var poster = new ChargePoster(Factory);
			poster.PostedInvoices.Add(invoice);

			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice);

			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);

			using (var testForm = new InvoicePreviewForm(previewer))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);
				testForm.FormCannotBeClosedUntilProcessingCompleted = true;

				using (var task = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true }))
				{
					testForm.Close();

					AssertEquals(true, testForm.Visible);
					AssertEquals("This form cannot be closed until invoice processing is complete.", UnitTestUserNotification.Instance.LastMessage.Text);

					task.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.All);
				}

				testForm.FormCannotBeClosedUntilProcessingCompleted = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testForm.Close();
				AssertEquals(false, testForm.Visible);
			}

			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHideAndRepositionPreviewAndDeliveryButton()
		{
			ChargePoster poster = new ChargePoster(Factory);
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job.PK;
			invoice2.AH_JH = job.PK;

			poster.PostedInvoices.Add(invoice1);
			poster.PostedInvoices.Add(invoice2);
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice1);
			transactions.AddARInvoice(invoice2);
			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				testForm.Show();
				Assert(!testForm.PreviewAndDeliverButton_ForTestOnly.Visible);
				AssertEquals(testForm.PreviewAndDeliverButton_ForTestOnly.Location, testForm.PreviewOnlyButton_ForTestOnly.Location);
			}

			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, true))
			{
				testForm.Show();
				Assert(testForm.PreviewAndDeliverButton_ForTestOnly.Visible);
				AssertNotEquals(testForm.PreviewAndDeliverButton_ForTestOnly.Location, testForm.PreviewOnlyButton_ForTestOnly.Location);
			}
		}

		public void TestARInvoicePreviewAndDeliveryAndPreviewOnly()
		{
			AssertInvoicePreviewAndDeliveryAndPreviewOnly(LedgerTypes.AccountsReceivable);
		}

		public void TestInvoice_PreviewAndDelivery_PreviewOnly_SecurityCheck()
		{
			ChargePoster poster = new ChargePoster(Factory);
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job.PK;

			poster.PostedInvoices.Add(invoice1);
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice1);

			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue, securityHelper);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, true))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);

				var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);
				var previewAndDeliverCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewAndDeliver);

				previewOnlyCheckpoint.IsAllowed = false;
				testForm.PreviewOnlyButton_ForTestOnly.PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewOnlyCheckpoint.ErrorMessageForNotAllowed);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				previewOnlyCheckpoint.IsAllowed = true;
				testForm.PreviewOnlyButton_ForTestOnly.PerformClick();
				AssertNotEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewOnlyCheckpoint.ErrorMessageForNotAllowed);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				previewAndDeliverCheckpoint.IsAllowed = false;
				testForm.PreviewAndDeliverButton_ForTestOnly.PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewAndDeliverCheckpoint.ErrorMessageForNotAllowed);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				previewAndDeliverCheckpoint.IsAllowed = true;
				testForm.PreviewAndDeliverButton_ForTestOnly.PerformClick();
				AssertNotEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewAndDeliverCheckpoint.ErrorMessageForNotAllowed);

				PreviewFormTestHelper.CloseOpenedForms();
			}
		}

		public void TestAPInvoicePreviewAndDeliveryAndPreviewOnly()
		{
			AssertInvoicePreviewAndDeliveryAndPreviewOnly(LedgerTypes.AccountsPayable);
		}

		public void AssertInvoicePreviewAndDeliveryAndPreviewOnly(string ledgerType)
		{
			ChargePoster poster = new ChargePoster(Factory);
			InvoicingBase invoice1, invoice2;
			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			}
			else
			{
				invoice1 = Factory.NewWithValidTestData<APInvoice>();
				invoice2 = Factory.NewWithValidTestData<APInvoice>();
				invoice1.AH_OH = invoice2.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
			}
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job.PK;
			invoice2.AH_JH = job.PK;

			poster.PostedInvoices.Add(invoice1);
			poster.PostedInvoices.Add(invoice2);
			var transactions = new TransactionCreatorHashtable();
			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				transactions.AddARInvoice(invoice1);
				transactions.AddARInvoice(invoice2);
			}
			else
			{
				transactions.AddAPInvoice((APInvoice)invoice1, invoice1.Header.OH_Code, invoice1.InvoiceNumber);
				transactions.AddAPInvoice((APInvoice)invoice2, invoice2.Header.OH_Code, invoice2.InvoiceNumber);
			}

			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);
				testForm.InvoicesGrid_ForTestOnly.Select(1);
				AssertEquals("TestForm.InvoicesGrid_ForTestOnly.SelectedElements length should be 2", 2, testForm.InvoicesGrid_ForTestOnly.SelectedElements.Length);
				AssertEquals(ledgerType, ((InvoicingBase)testForm.InvoicesGrid_ForTestOnly.SelectedElements[0]).AH_Ledger);
				AssertEquals(ledgerType, ((InvoicingBase)testForm.InvoicesGrid_ForTestOnly.SelectedElements[1]).AH_Ledger);
				ZFormModaliser.LastFormShownDialogForTest = null;
				testForm.Run_ForTestOnly(true);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);

				List<Form> previewForms = new List<Form>();
				foreach (var form in Application.OpenForms)
				{
					var previewForm = form as DocumentEngine.GUI.XLSPreviewForm;
					if (previewForm != null)
					{
						previewForms.Add(previewForm);
					}
				}
				AssertEquals("There should be two preview forms opened", 2, previewForms.Count);
				previewForms[0].Close();
				previewForms[1].Close();

				ZFormModaliser.LastFormShownDialogForTest = null;
				testForm.Run_ForTestOnly(false);
				AssertEquals(typeof(DocumentEngine.GUI.DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[ExpectNoExceptions]
		public void TestInvoicePreviewFormRunWillCreateProFormaTaskOnlyAndNoException()
		{
			ChargePoster poster = new ChargePoster(Factory);
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job.PK;
			invoice2.AH_JH = job.PK;

			poster.PostedInvoices.Add(invoice1);
			poster.PostedInvoices.Add(invoice2);
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice1);
			transactions.AddARInvoice(invoice2);
			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);
				testForm.InvoicesGrid_ForTestOnly.Select(1);
				AssertEquals("TestForm.InvoicesGrid_ForTestOnly.SelectedElements length should be 2", 2, testForm.InvoicesGrid_ForTestOnly.SelectedElements.Length);
				testForm.Run_ForTestOnly(false);

				PreviewFormTestHelper.CloseOpenedForms();
			}
		}

		public void TestNoException_WhenInvoiceIsDeleted()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;

			var poster = new ChargePoster(Factory);
			poster.PostedInvoices.Add(invoice);

			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice);

			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);

			using (var testForm = new InvoicePreviewFormForTest(previewer))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);

				using (var task = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true }))
				{
					testForm.Run_ForTestOnly(true);

					AssertEquals("An error has occurred. Please close the Document Form before the Invoice Preview Form.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		class InvoicePreviewFormForTest : InvoicePreviewForm
		{
			public InvoicePreviewFormForTest(InvoicesPreviewer invoicesPreviewer)
				: base(invoicesPreviewer)
			{ }

			protected override InvoicePrintTask GetNewInvoicePrintTask(InvoicingBase invoice)
			{
				var result = base.GetNewInvoicePrintTask(invoice);
				invoice.Delete();

				return result;
			}
		}

		public void TestInvoicePreviewHasNoErrorsWithCreditOnHold()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader orgheader = creator.AALSHI;
			orgheader.OH_Code = "ABCDEF";
			orgheader.MiscServ.OM_AROnCreditHold = true;
			orgheader.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			ChargePoster poster = new ChargePoster(Factory);
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job.PK;
			invoice2.AH_JH = job.PK;
			invoice1.AH_OH = orgheader.PK;
			invoice2.AH_OH = invoice1.AH_OH;
			orgheader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			invoice1.Validation.ValidateAll();
			invoice2.Validation.ValidateAll();

			AssertHasError(invoice1.AH_OHInfo, "This account is on credit hold and cannot be billed to");

			poster.PostedInvoices.Add(invoice1);
			poster.PostedInvoices.Add(invoice2);
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice1);
			transactions.AddARInvoice(invoice2);
			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);
				testForm.InvoicesGrid_ForTestOnly.Select(1);
				foreach (InvoicingBase bizo in testForm.InvoicesGrid_ForTestOnly.SelectedElements)
				{
					AssertNoError(bizo.AH_OHInfo, "This account is on credit hold and cannot be billed to");
				}
				testForm.Run_ForTestOnly(false);

				PreviewFormTestHelper.CloseOpenedForms();
			}
		}

		public void TestInvoicePreviewHasNoCriticalErrors()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			ChargePoster chargePoster = new ChargePoster(Factory);
			OrgHeader debtor = testObjectCreator.ABIGAS;
			Factory.Save();

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARInvoice), testObjectCreator.USD, 1.0m, debtor);
			InvoicingLineBase line = (InvoicingLineBase)invoice1.Lines.AddNew();

			using (invoice1.Lines[0].GetLocalAmountCalculationSuspender())
			using (invoice1.Lines[0].GetOSAmountCalculationSuspender())
			{
				if (invoice1.IsInDatabase)
				{
					invoice1.AH_FullyPaidDate = ZDateTime.Empty;
				}

				line.AL_RX_NKTransactionCurrency = testObjectCreator.AUD.RX_Code;
				line.AL_ExchangeRate = 1.0m;
				line.AL_OSExTaxAmount = 2.0m;
				line.AL_OSTaxAmount = 0.0m;
				line.AL_LocalExTaxAmount = 1.0m;
				line.AL_LocalTaxAmount = 0.0m;
				line.AL_Desc = "tee he he";
				line.AL_GB = GlbBranch.CurrentBranch.PK;
			}

			invoice1.MarkAsNeedingValidation();
			invoice1.RunPreSaveValidation();

			AssertHasError(invoice1.Lines[0].AL_LocalExTaxAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

			chargePoster.PostedInvoices.Add(invoice1);
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice1);
			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer))
			{
				testForm.Show();
				testForm.InvoicesGrid_ForTestOnly.Select(0);
				foreach (InvoicingBase bizo in testForm.InvoicesGrid_ForTestOnly.SelectedElements)
				{
					bizo.MarkAsNeedingValidation();
					bizo.RunPreSaveValidation();
					AssertNoError(invoice1.Lines[0].AL_LocalExTaxAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");
				}
				testForm.Run_ForTestOnly(false);

				PreviewFormTestHelper.CloseOpenedForms();
			}
		}

		public void TestFormCaption()
		{
			var previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.ConsolCosts);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				AssertEquals("Preview All Transactions", testForm.FormCaption);
			}

			previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				AssertEquals("Preview All Transactions", testForm.FormCaption);
			}

			previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.Costs);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				AssertEquals("Preview All Transactions", testForm.FormCaption);
			}
		}

		public void TestInvoicesGridColumns()
		{
			var previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.ConsolCosts);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				testForm.Show();
				AssertNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_TransactionCategory"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_TransactionNum"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_InvoiceDate"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_DueDate"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_Ledger"));
			}

			previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.Revenue);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				testForm.Show();
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_TransactionCategory"));
				AssertNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_TransactionNum"));
				AssertNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_InvoiceDate"));
				AssertNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_DueDate"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_Ledger"));
			}

			previewer = new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.Costs);
			using (InvoicePreviewForm testForm = new InvoicePreviewForm(previewer, false))
			{
				testForm.Show();
				AssertNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_TransactionCategory"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_TransactionNum"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_InvoiceDate"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_DueDate"));
				AssertNotNull(testForm.InvoicesGrid_ForTestOnly.GetColumnStyle("AH_Ledger"));
			}
		}

		public void TestModifyPreviewInvoiceDoesNotSaveInvoiceToDatabase()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			var sellCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var sellAccount = Factory.NewWithValidTestData<OrgHeader>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			dept.GE_IsActive = true;
			dept.GE_Air = true;
			dept.GE_Misc = false;

			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			sellCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			sellCompanyData.OB_OH = sellAccount.PK;
			sellCompanyData.OB_IsDebtor = true;

			sellAccount.OH_IsActive = true;
			sellAccount.OH_IsDebtor = true;
			sellAccount.OH_FullName = "Test Account";
			sellAccount.OH_RL_NKClosestPort = "AUSYD";

			chargeCode.AC_IsActive = true;
			chargeCode.AC_ChargeType = "REV";

			var addedCharge = job.Charges.AddNew();

			addedCharge.JR_AC = chargeCode.PK;
			addedCharge.JR_OSSellAmt = 300;
			addedCharge.JR_OH_SellAccount = sellAccount.PK;
			addedCharge.JR_Desc = "Required Description";
			addedCharge.JR_GE = dept.PK;
			addedCharge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice;

			Factory.Save();

			sellCompanyData.SetARTaxApplicable(false);

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Revenue);

			var invoice = transactions.Values.Cast<ARInvoice>().FirstOrDefault();
			var task = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true });

			using (new InvoicePreviewForm(previewer, true))
			{
				var instructions = new DeliveryInstructions(task.GetFirstDocumentPack());
				DocPackVisualiserManager manager = new DocPackVisualiserManager(instructions.DocPack, instructions.DeliverablesToBePrinted);
				foreach (InvoiceLine invoiceLine in invoice.Lines)
				{
					invoiceLine.AL_Desc = "test description";
				}

				invoice.AH_Desc = "test description";

				manager.SaveData();
			}

			Assert("Invoice should not have been saved to the database", !invoice.IsInDatabase);
			Assert("Invoice lines should not have been saved to the database", !invoice.Lines.Any(line => line.IsInDatabase));

			AssertNoExceptionThrown(() => invoice.Factory.Save());

			Assert("Invoice should not have been saved to the database", !invoice.IsInDatabase);
			Assert("Invoice lines should not have been saved to the database", !invoice.Lines.Any(line => line.IsInDatabase));
		}
	}
}
