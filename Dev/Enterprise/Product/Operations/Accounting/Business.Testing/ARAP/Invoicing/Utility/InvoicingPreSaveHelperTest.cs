using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ARAP.Invoicing.Testing
{
	public class InvoicingPreSaveHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPreSaveActions_ManuallyAmendedCreditNote()
		{
			var invoicingPreSaveHelper = new InvoicingPreSaveHelper();

			var arCreditNote = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			arCreditNote.AH_OriginalTransactionNum = "AAA111";
			Assert(arCreditNote.IsAmendingTransaction);
			Assert(arCreditNote.IsAmendingTransaction_SoftReference);
			AssertNull(arCreditNote.OriginalTransaction);

			var outcome = invoicingPreSaveHelper.PreSaveActions(arCreditNote, null, false);
			Assert("Expect PreSave succeed", outcome.CanProceed);
			Assert("Expect no error message", string.IsNullOrEmpty(outcome.ErrorMessage));
		}

		public void TestInvoiceRoundingLineCreatorType()
		{
			var invoicingPreSaveHelper = new InvoicingPreSaveHelper();
			AssertType<InvoiceRoundingLineCreator>(invoicingPreSaveHelper.InvoiceRoundingLineCreator_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestPreSaveActions_AddRoundingLine()
		{
			var invoiceRoundingLineCreator = new Mock<IInvoiceRoundingLineCreator>(MockBehavior.Strict);

			var invoicingPreSaveHelper = new InvoicingPreSaveHelper();
			invoicingPreSaveHelper.SubstituteInvoiceRoundingLineCreator_ForTestOnly(invoiceRoundingLineCreator.Object);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M);

			invoiceRoundingLineCreator.Setup(x => x.AddRoundingLine(It.IsAny<InvoicingBase>()));

			invoicingPreSaveHelper.PreSaveActions(arInvoice, null, false);
			invoiceRoundingLineCreator.Verify(x => x.AddRoundingLine(arInvoice), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestPreSaveActionsForNonJobBillingPosting_AddRoundingLine()
		{
			var invoiceRoundingLineCreator = new Mock<IInvoiceRoundingLineCreator>(MockBehavior.Strict);

			var invoicingPreSaveHelper = new InvoicingPreSaveHelper();
			invoicingPreSaveHelper.SubstituteInvoiceRoundingLineCreator_ForTestOnly(invoiceRoundingLineCreator.Object);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M);

			invoiceRoundingLineCreator.Setup(x => x.AddRoundingLine(It.IsAny<InvoicingBase>()));

			((IInvoicingPreSaveHelper)invoicingPreSaveHelper).PreSaveActionsForNonJobBillingPosting(arInvoice);
			invoiceRoundingLineCreator.Verify(x => x.AddRoundingLine(arInvoice), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestPreSaveActions_AddSurchargeLine()
		{
			var surchargeLineCreator = new Mock<ISurchargeLineCreator>(MockBehavior.Strict);

			var invoicingPreSaveHelper = new InvoicingPreSaveHelper();
			ObjectFactory.Substitute(surchargeLineCreator.Object);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M);

			surchargeLineCreator.Setup(x => x.AddSurchargeLine(It.IsAny<InvoicingBase>()));
			invoicingPreSaveHelper.PreSaveActions(arInvoice, null, false);
			surchargeLineCreator.Verify(x => x.AddSurchargeLine(arInvoice), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestPreSaveActionsForNonJobBillingPosting_AddSurchargeLine()
		{
			var surchargeLineCreator = new Mock<ISurchargeLineCreator>(MockBehavior.Strict);

			var invoicingPreSaveHelper = new InvoicingPreSaveHelper();
			ObjectFactory.Substitute(surchargeLineCreator.Object);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M);

			surchargeLineCreator.Setup(x => x.AddSurchargeLine(It.IsAny<InvoicingBase>()));

			((IInvoicingPreSaveHelper)invoicingPreSaveHelper).PreSaveActionsForNonJobBillingPosting(arInvoice);
			surchargeLineCreator.Verify(x => x.AddSurchargeLine(arInvoice), Times.Once);
		}

		public void TestPreSaveActions_WhenJobsNeedToBeReopened()
		{
			creator.Job1.JH_Status = JobHeaderStatus.Closed.Code;
			creator.Job2.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var apInvoice = creator.CreateAPInvoice<APInvoice>("I0002", creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			creator.CreateAPInvoiceLine(apInvoice, creator.Job1, creator.CC1, creator.AUD, 1M, null, 100M);

			AssertContainsExactElementsInAnyOrder("PreCondtion, and set cached value to RelatedJobsForReversing."
				, new[] { creator.Job1 }
				, apInvoice.RelatedJobsForReversing
			);

			creator.CreateAPInvoiceLine(apInvoice, creator.Job2, creator.CC1, creator.AUD, 1M, null, 100M);
			AssertContainsExactElementsInAnyOrder("PreCondtion, the RelatedJobsForReversing is cached that only having Job1."
				, new[] { creator.Job1 }
				, apInvoice.RelatedJobsForReversing
			);

			var testProvider = new DummySecurityOverrideProvider(apInvoice);
			((ISecurityOverrideProviderSource)apInvoice).Provider = testProvider;

			new InvoicingPreSaveHelper().PreSaveActions(apInvoice, null, false);
			AssertEquals("The RelatedJobsForReversing should be refreshed when running PreSaveActions."
				, "Closed Job(s) :Z00001000,Z00001001"
				, testProvider.LastSecurityMessage
			);
		}

		class DummySecurityOverrideProvider : SecurityOverrideProvider
		{
			public DummySecurityOverrideProvider(APInvoice apInvoice)
			{
				ApInvoice = apInvoice;
			}

			APInvoice ApInvoice { get; }

			public string LastSecurityMessage { get; private set; }

			protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
			{
				if (checkPoint == Env.Security.ReopenJob)
				{
					LastSecurityMessage = $"Closed Job(s) :{string.Join(",", ApInvoice.RelatedJobsForReversing.Select(x => x.JH_JobNum))}";

					return SecurityCertificate.Denied;
				}

				throw new NotImplementedException();
			}

			protected override bool ShouldPromptForGranted => true;

			protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
			{
				throw new NotImplementedException();
			}
		}

		[TestDate(2022, 11, 07)]
		public void TestAddErrorIfInvoiceDateIsInTheFuture()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_InvoiceDate = new ZDateTime(2022, 11, 07);
			var result = InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(invoice.AH_GC, invoice.AH_InvoiceDateInfo, invoice.GetType());
			AssertEquals(false, result);

			AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.SetValue(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, false);

			invoice.AH_InvoiceDate = new ZDateTime(2022, 11, 08);
			result = InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(invoice.AH_GC, invoice.AH_InvoiceDateInfo, invoice.GetType());
			AssertEquals(false, result);

			AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.SetValue(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
			result = InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(invoice.AH_GC, invoice.AH_InvoiceDateInfo, invoice.GetType());
			AssertEquals(true, result);
		}

		#region Implementation

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
		}

		#endregion
	}
}
