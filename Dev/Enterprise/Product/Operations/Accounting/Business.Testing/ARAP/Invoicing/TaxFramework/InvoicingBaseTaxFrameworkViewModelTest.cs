using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class InvoicingBaseTaxFrameworkViewModelTest : TestCaseWithFactory
	{
		public void TestTrackHasChanges()
		{
			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.DeleteTaxesNotInDB(taxRecordParent)).Callback<ITaxRecordParent>(p => invoice.AH_RequisitionStatus = "XYZ");

			objForTest.TrackHasChanges(true);
			using (objForTest.SuspendTrackingHasChanges)
			{
				invoice.AH_OSExTaxAmount = 155m;
				CombineAssertions(() =>
				{
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Never);
				});
			}

			invoice.Logs.AddNew();
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Never);
			});

			using (invoice.Factory.SetTempContext(BusinessContext.MakingChangesToOtherTaxes))
			{
				invoice.AH_OSExTaxAmount = 205m;
				CombineAssertions(() =>
				{
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Never);
				});
			}

			using (invoice.Factory.SetTempContext(BusinessContext.MakingChangesNotAffectingTaxRecordParent))
			{
				invoice.AH_OSExTaxAmount = 255m;
				CombineAssertions(() =>
				{
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Never);
				});
			}

			invoice.Lines[0].AL_OSAmount = 22m;
			CombineAssertions(() =>
			{
				AssertEquals("InvoicingBaseTaxFrameworkViewModel_TrackHasChanges_1", ErrorReporter.LastKeyReported);
				AssertContains(FormattableString.Invariant($@"Unexpected changes are detected on Invoice. Invoice changes are not allowed after Tax Transactions are calculated as they can make Tax Transactions calculations invalid.
	PK = {invoice.PK}
	Type = APInvoice
	Types around row = APInvoice"), ErrorReporter.LastMessageReported);
				taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Once);
			});

			ErrorReporter.Clear();
			using (objForTest.SuspendTrackingHasChanges)
			{
				invoice.AH_RequisitionStatus = "ABC";
			}
			invoice.Lines[0].Delete();
			CombineAssertions(() =>
			{
				AssertEquals("Callback must have been executed", "XYZ", invoice.AH_RequisitionStatus);
				AssertEquals("InvoicingBaseTaxFrameworkViewModel_TrackHasChanges_1", ErrorReporter.LastKeyReported);
				AssertContains(FormattableString.Invariant($@"Unexpected changes are detected on Invoice. Invoice changes are not allowed after Tax Transactions are calculated as they can make Tax Transactions calculations invalid.
	PK = {invoice.PK}
	Type = APInvoice
	Types around row = APInvoice"), ErrorReporter.LastMessageReported);
				taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Exactly(5), "Line.Delete() calls Invoice.OnHasChangesChanged() 4 times. Hence verifying additional 4 method calls. DeleteTaxesNotInDB() setup callback should not execute recursive OnHasChangesChanged() calls as tracking HasChanges should have been suspended");
			});

			ErrorReporter.Clear();
			taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Exactly(5), "Precondition");
			invoice.AH_OSExTaxAmount = 158m;
			CombineAssertions(() =>
			{
				AssertEquals("InvoicingBaseTaxFrameworkViewModel_TrackHasChanges_1", ErrorReporter.LastKeyReported);
				AssertContains(FormattableString.Invariant($@"Unexpected changes are detected on Invoice. Invoice changes are not allowed after Tax Transactions are calculated as they can make Tax Transactions calculations invalid.
	PK = {invoice.PK}
	Type = APInvoice
	Types around row = APInvoice"), ErrorReporter.LastMessageReported);
				taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Exactly(8), "AH_OSExTaxAmount calls HasChanges thrice after Precondition");
			});

			objForTest.TrackHasChanges(false);
			ErrorReporter.Clear();
			invoice.AH_OSExTaxAmount = 158m;
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Exactly(8));
			});
		}

		public void TestErrorReportShouldNotRaise_WhenEventArgumentChangeInEditableChildCollectionIsTrue()
		{
			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.DeleteTaxesNotInDB(taxRecordParent));

			objForTest.TrackHasChanges(true);
			Assert("InvoicingBase", invoice is InvoicingBase);

			ErrorReporter.Clear();

			var child = Factory.New<DummyBusinessObject>();
			child.HasChanges = true;

			HasChangesChangedEventArgs eventArgs = null;
			invoice.HasChangesChanged += (s, e) =>
			{
				eventArgs = e;
			};

			invoice.RegisterEditableChildObject(child);
			AssertEquals("Postcondition: ChangeInEditableChildCollection", true, eventArgs.ChangeInEditableChildCollection);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Never);
			});

			invoice.AH_RequisitionStatus = "ABC";
			AssertEquals("Postcondition: ChangeInEditableChildCollection", false, eventArgs.ChangeInEditableChildCollection);

			AssertEquals("InvoicingBaseTaxFrameworkViewModel_TrackHasChanges_1", ErrorReporter.LastKeyReported);
			AssertContains(FormattableString.Invariant($@"Unexpected changes are detected on Invoice. Invoice changes are not allowed after Tax Transactions are calculated as they can make Tax Transactions calculations invalid.
	PK = {invoice.PK}
	Type = APInvoice
	Types around row = APInvoice"), ErrorReporter.LastMessageReported);
			taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Once);

			ErrorReporter.Clear();
		}

		public void TestValidateForPosting_StopsTrackingHasChangesNoIfErrorMessage()
		{
			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.DeleteTaxesNotInDB(taxRecordParent));

			taxframeworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsPayable.Code, true);
			Assert(taxRecordParent.ShouldCalculateTaxTransactions);

			objForTest.TrackHasChanges(true);
			AssertNotNullOrEmpty(objForTest.ValidateForPosting());
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			invoice.AH_RequisitionStatus = "ABC";
			AssertContains(FormattableString.Invariant($@"Unexpected changes are detected on Invoice. Invoice changes are not allowed after Tax Transactions are calculated as they can make Tax Transactions calculations invalid.
	PK = {invoice.PK}
	Type = APInvoice
	Types around row = APInvoice"), ErrorReporter.LastMessageReported);
			taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Once);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			AssertNullOrEmpty(objForTest.ValidateForPosting());
			ErrorReporter.Clear();
			invoice.AH_RequisitionStatus = "XYZ";
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			taxProcessorMock.Verify(t => t.DeleteTaxesNotInDB(taxRecordParent), Times.Once);
		}

		public void TestValildateIsOtherTaxesCalculatedBeforePosting()
		{
			Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
			Assert(!taxRecordParent.ShouldCalculateTaxTransactions);
			var errorMessage = objForTest.ValidateForPosting();
			AssertNullOrEmpty(errorMessage);

			taxframeworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			taxframeworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsPayable.Code, true);
			Assert(taxRecordParent.ShouldCalculateTaxTransactions);
			errorMessage = objForTest.ValidateForPosting();
			AssertEquals("Tax Transactions have to be calculated on the invoice before posting. Please retry posting after calculating Tax Transactions.", errorMessage);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			errorMessage = objForTest.ValidateForPosting();
			AssertNullOrEmpty(errorMessage);
		}

		public void TestType()
		{
			AssertType<InvoicingBaseTaxFrameworkViewModel>(objForTest);
		}

		protected override void SetUp()
		{
			testObjectCreator = new TestObjectCreator(Factory);
			taxframeworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			invoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "", testObjectCreator.AUD, 1, 10, 0, 10, 0);
			objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseTaxFrameworkViewModel(invoice);
			taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
		}

		IInvoicingBaseTaxFrameworkViewModel objForTest;
		InvoicingBase invoice;
		InvoicingBaseTaxRecordParent taxRecordParent;
		TestObjectCreator testObjectCreator;
		TaxFrameworkTestObjectCreator taxframeworkTestObjectCreator;
	}
}
