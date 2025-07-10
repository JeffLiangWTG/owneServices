using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	public class ManageInvoiceRequestModelTestBase : TestCaseWithFactory
	{
		public void TestConstructorThrows()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when transaction batch is null.", () => GetModelForTest(null, new HungaryTransactionExtraInfo(), "1234", Notifications));
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when batch number is null.", () => GetModelForTest(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance), new HungaryTransactionExtraInfo(), null, Notifications));
			AssertExceptionThrown<ArgumentException>("Exception should be thrown when batch number is empty.", () => GetModelForTest(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance), null, "", Notifications));
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when notifications is null.", () => GetModelForTest(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance), new HungaryTransactionExtraInfo(), "1234", null));
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when notifications is null.", () => GetModelForTest(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance), null, "1234", Notifications));
		}

		[TestDate(2020, 5, 7, 15, 26, 32, 253)]
		[TestUtcOffset(10, 0, 0)]
		public void TestConstructorBasedProperties()
		{
			var model = GetModelForTest(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance), new HungaryTransactionExtraInfo(), "1234", Notifications);

			AssertEquals("BatchNumber is from constructor", "1234", model.BatchNumber);
			AssertEquals("Timestamp is from ZDateTime.UtcNow", new DateTime(2020, 5, 7, 15, 26, 32, 253), model.UtcTimestamp);
			AssertEquals("Timestamp is UTC", DateTimeKind.Utc, model.UtcTimestamp.Kind);

			Assert("There should be no errors logged", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestIdentityProperties()
		{
			var info = CreateTransactionBatchForTest(isReciprocal: false);
			var model = GetModelForTest(info.transactionInfo, info.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			AssertEquals("BranchCode is from Universal Transaction / Database", Branch.GB_Code, model.BranchCode);
			AssertEquals("CompanyCode is from Universal Transaction / Database", Company.GC_Code, model.CompanyCode);
			AssertEquals("CompanyIsReciprocal is from Universal Transaction / Database", Company.GC_IsReciprocal, model.CompanyIsReciprocal);

			Assert("There should be no errors logged", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestIdentityProperties_ForIsReciprocal()
		{
			var info = CreateTransactionBatchForTest(isReciprocal: true);
			var model = GetModelForTest(info.transactionInfo, info.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			AssertEquals("BranchCode is from Universal Transaction / Database", Branch.GB_Code, model.BranchCode);
			AssertEquals("CompanyCode is from Universal Transaction / Database", Company.GC_Code, model.CompanyCode);
			AssertEquals("CompanyIsReciprocal is from Universal Transaction / Database", Company.GC_IsReciprocal, model.CompanyIsReciprocal);

			Assert("There should be no errors logged", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestErrorLogged_WhenNoBranchInUniversalTransaction()
		{
			var model = GetModelForTest(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance), new HungaryTransactionExtraInfo(), "1234", Notifications);
			model.LoadData(Factory);

			AssertMultilineASCIIEquals("Unable to determine Branch from Universal Transaction Batch. This transaction will not be processed.", Notifications.AsString);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestErrorLogged_WhenUnknownBranchInUniversalTransaction()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "...", Name = "Unknown Branch" }
			};

			var model = GetModelForTest(transactionInfo, new HungaryTransactionExtraInfo(), "1234", Notifications);
			model.LoadData(Factory);

			AssertMultilineASCIIEquals("Unable to load Branch based on Universal Transaction Batch. This transaction will not be processed.", Notifications.AsString);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestInvoiceCategory()
		{
			var existingInvoiceTypes = typeof(InvoiceTypesList.Codes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();

			var normalInvoiceCategoryTypes = new string[] {
				InvoiceTypesList.Codes.FinalInvoice,
				InvoiceTypesList.Codes.ForeignCurrencyInvoice,
				InvoiceTypesList.Codes.FreightInvoice,
				InvoiceTypesList.Codes.DestinationChargesInvoice,
				InvoiceTypesList.Codes.InvoicePerTaxCode,
				InvoiceTypesList.Codes.SelfBillingInvoice,
				InvoiceTypesList.Codes.DisbursementInForeignCurrency,
				InvoiceTypesList.Codes.DisbursementInvoice };

			var aggregateInvoiceCategoryTypes = new string[] {
				InvoiceTypesList.Codes.FinalInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInvoice_Batching,
				InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
				InvoiceTypesList.Codes.FreightInvoice_Batching,
				InvoiceTypesList.Codes.InvoicePerTaxCode_Batching,
				InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
				InvoiceTypesList.Codes.SelfBillingInvoice_Batching };

			var invoiceTypeToIgnore = new string[] { InvoiceTypesList.Codes.DoNotPost };

			foreach (var invoiceType in existingInvoiceTypes)
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfo.Category = invoiceType;
				var model = GetModelForTest(transactionInfo, new HungaryTransactionExtraInfo(), "1234", Notifications);

				var result = model.GetInvoiceDataModel().InvoiceCategory;
				var expectedCategory = normalInvoiceCategoryTypes.Contains(invoiceType) ? "NORMAL"
					: aggregateInvoiceCategoryTypes.Contains(invoiceType) ? "AGGREGATE"
					: invoiceTypeToIgnore.Contains(invoiceType) ? string.Empty
					: "Error - the invoice type doesn't have any category";

				AssertEquals("You must update the InvoiceDataModel.NormalInvoiceCategories or InvoiceDataModel.AggregateInvoiceCategories to add the missing invoice type",
					expectedCategory, result);
			}
		}

		public void TestNoErrorLogged_WhenNoCredentialsForCompany()
		{
			var batch = CreateTransactionBatchForTest(includeCredentials: false);
			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			Assert("There should be no error", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestNoErrorLogged_WhenBlankCredentialsForCompany()
		{
			var batch = CreateTransactionBatchForTest();
			Credentials.Login = "";
			Credentials.PasswordHash = "";
			Credentials.SignatureKey = "";
			Credentials.ReplacementKey = "";
			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			Assert("There should be no error", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestNoErrorLogged_WhenFullCredentialSuppliedForCompany()
		{
			var batch = CreateTransactionBatchForTest();
			AssertNotNullOrEmpty("Credential supplied", Credentials.Login);

			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			Assert("There should be no error", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestCredentialsProperties()
		{
			var batch = CreateTransactionBatchForTest();
			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			AssertEquals("LoginId is from Database Credentials", Credentials.Login, model.LoginId);
			AssertEquals("PasswordHash is from Database Credentials", Credentials.PasswordHash, model.PasswordHash);
			AssertEquals("SignatureKey is from Database Credentials", Credentials.SignatureKey, model.SignatureKey);
			AssertEquals("ReplacementKey is from Database Credentials", Credentials.ReplacementKey, model.ReplacementKey);

			Assert("There should be no errors logged", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestEnterpriseEnvironmentBasedProperties()
		{
			var batch = CreateTransactionBatchForTest();
			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);

			var expectedSoftwareVersion = ReleaseInfo.Instance.VersionNumber.ToString();
			AssertEquals("SoftwareVersion is from ReleaseInfo", expectedSoftwareVersion, model.SoftwareVersion);

			Assert("There should be no errors logged", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestTaxPayerRegistrationNumber()
		{
			var batch = CreateTransactionBatchForTest();
			var regNumbers = batch.transactionInfo.BranchAddress.RegistrationNumberCollection;
			var regNumber = regNumbers.First(x => x.Type.Code.GetValueOrDefault() == "VAT");

			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);
			AssertEquals("TaxPayerRegistrationNumber is from Universal XML BranchAddress", "12036024", model.TaxPayerRegistrationNumber);

			regNumber.Value = "HU72947287";
			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);
			AssertEquals("TaxPayerRegistrationNumber removes 'HU' prefix", "72947287", model.TaxPayerRegistrationNumber);

			regNumber.Value = "36738592421";
			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);
			AssertEquals("TaxPayerRegistrationNumber trims to 8 digits", "36738592", model.TaxPayerRegistrationNumber);

			regNumber.CountryOfIssue.Code = "AU";
			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);
			AssertEquals("TaxPayerRegistrationNumber must be in HU country", null, model.TaxPayerRegistrationNumber);
			regNumber.CountryOfIssue.Code = "HU";

			regNumber.Type.Code = "GST";
			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);
			AssertEquals("TaxPayerRegistrationNumber must be of type VAT", null, model.TaxPayerRegistrationNumber);
			regNumber.Type.Code = "VAT";

			regNumbers.Clear();
			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			model.LoadData(Factory);
			AssertEquals("TaxPayerRegistrationNumber is empty if nothing in XML", null, model.TaxPayerRegistrationNumber);

			Assert("There should be no errors logged", !Notifications.HasErrors);
			Assert("There should be no warnings logged", !Notifications.HasWarnings);
		}

		public void TestPreviousInvoiceInfoPopulated_WhenOriginalTransactionIsNotSubmitted()
		{
			Branch = TestObjectCreator.CreateHungaryBranchWithAddressAndTaxNumber();
			Company = Branch.Company;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoiceOriginal = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceOriginal, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "second line", 101m);
				arInvoiceOriginal.Lines[0].AL_Sequence = 7;
				arInvoiceOriginal.Lines[1].AL_Sequence = 10;
				arInvoiceOriginal.Factory.Save();

				var arCreditNote = (ARCreditNote)((IAmending)arInvoiceOriginal).GenerateAmendingTransaction(TransactionTypes.CreditNote);
				TestObjectCreator.AddLineToCreditNote(arCreditNote, ZGuid.Empty, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, 10);
				arCreditNote.Lines[0].AL_Sequence = 14;
				arCreditNote.Lines[1].AL_Sequence = 16;
				arCreditNote.Lines[2].AL_Sequence = 7;
				arCreditNote.Factory.Save();

				Factory.Save();

				var batch = CreateTransactionBatchForTest(invoiceNumber: arCreditNote.AH_TransactionNum, originalTransactionNumber: arInvoiceOriginal.AH_TransactionNum);
				var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
				model.LoadData(Factory);

				var expectedInvoices = new[]
				{
					new PreviousInvoice() { AH_TransactionNum = arInvoiceOriginal.AH_TransactionNum, WasSubmittedSuccessfully = false, MaximumLineSequence = 0 },
				};
				AssertArrayEqualsByElements("PreviousInvoices should be failed INV", expectedInvoices, model.PreviousInvoices.ToArray());

				Assert("There should be no errors logged", !Notifications.HasErrors);
				Assert("There should be no warnings logged", !Notifications.HasWarnings);
			}
		}

		[TestDate(2020, 5, 1)]
		[TestDateIncremental(hours: 1)]
		public void TestPreviousInvoices()
		{
			Branch = TestObjectCreator.CreateHungaryBranchWithAddressAndTaxNumber();
			Company = Branch.Company;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoiceSent = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceSent, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "second line", 101m);
				arInvoiceSent.Lines[0].AL_Sequence = 7;
				arInvoiceSent.Lines[1].AL_Sequence = 10;
				arInvoiceSent.Factory.Save();

				var batchSent = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivotSent = TestObjectCreator.CreateEInvoicingTransactionPivot(batchSent, arInvoiceSent, Core.Constants.EInvoicingPivotState.Succeed, actionType: Core.Constants.EInvoicingPivotActionType.Submit);
				pivotSent.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;

				var arCreditFailed = (ARCreditNote)((IAmending)arInvoiceSent).GenerateAmendingTransaction(TransactionTypes.CreditNote);
				TestObjectCreator.AddLineToCreditNote(arCreditFailed, ZGuid.Empty, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, 10);
				arCreditFailed.Lines[0].AL_Sequence = 14;
				arCreditFailed.Lines[1].AL_Sequence = 16;
				arCreditFailed.Lines[2].AL_Sequence = 7;
				arCreditFailed.Factory.Save();

				var batchFailed = TestObjectCreator.CreateEInvoicingBatch(101, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivotFailed = TestObjectCreator.CreateEInvoicingTransactionPivot(batchFailed, arCreditFailed, Core.Constants.EInvoicingPivotState.Failed, actionType: Core.Constants.EInvoicingPivotActionType.Submit);
				pivotFailed.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;

				var arInvoiceCurrent = (ARInvoice)((IAmending)arInvoiceSent).GenerateAmendingTransaction(TransactionTypes.Invoice);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceCurrent, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "number 2", 200m);
				arInvoiceCurrent.Lines[0].AL_Sequence = 1;
				arInvoiceCurrent.Lines[1].AL_Sequence = 2;
				arInvoiceCurrent.Lines[2].AL_Sequence = 5;
				arInvoiceCurrent.Factory.Save();

				var batchCurrent = TestObjectCreator.CreateEInvoicingBatch(102, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivotCurrent = TestObjectCreator.CreateEInvoicingTransactionPivot(batchCurrent, arInvoiceCurrent, Core.Constants.EInvoicingPivotState.Batched, actionType: Core.Constants.EInvoicingPivotActionType.Submit);
				pivotCurrent.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;

				Factory.Save();

				var batch = CreateTransactionBatchForTest(invoiceNumber: arInvoiceCurrent.AH_TransactionNum);
				var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
				model.LoadData(Factory);

				var expectedInvoices = new[]
				{
					new PreviousInvoice() { AH_TransactionNum = arInvoiceSent.AH_TransactionNum, AH_TransactionType = "INV", WasSubmittedSuccessfully = true, MaximumLineSequence = 2 },
					new PreviousInvoice() { AH_TransactionNum = arCreditFailed.AH_TransactionNum, AH_TransactionType = "CRD", WasSubmittedSuccessfully = false, MaximumLineSequence = 3 },
				};
				AssertArrayEqualsByElements("PreviousInvoices should be successful INV and failed CRD", expectedInvoices, model.PreviousInvoices.ToArray());

				Assert("There should be no errors logged", !Notifications.HasErrors);
				Assert("There should be no warnings logged", !Notifications.HasWarnings);
			}
		}

		[TestDate(2020, 5, 1)]
		[TestDateIncremental(hours: 1)]
		public void TestPreviousInvoices_WhenOriginalInvoiceWasDiscarded()
		{
			Branch = TestObjectCreator.CreateHungaryBranchWithAddressAndTaxNumber();
			Company = Branch.Company;
			Factory.Save();

			AssertNotNull("Precondition: Branch has HomePort with TimeZoneSet", Branch.HomePort?.TimeZoneSet);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoiceDiscardedWhenEInvoicingWasDisabled = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceDiscardedWhenEInvoicingWasDisabled, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "second line", 101m);
				arInvoiceDiscardedWhenEInvoicingWasDisabled.Lines[0].AL_Sequence = 7;
				arInvoiceDiscardedWhenEInvoicingWasDisabled.Lines[1].AL_Sequence = 10;
				arInvoiceDiscardedWhenEInvoicingWasDisabled.Factory.Save();

				var pivotDiscarded = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoiceDiscardedWhenEInvoicingWasDisabled, status: Core.Constants.EInvoicingPivotState.Discarded, actionType: Core.Constants.EInvoicingPivotActionType.Submit);
				pivotDiscarded.AIP_ErrorDescription = "E-Reporting/E-Invoicing is disabled for this login company.";
				pivotDiscarded.AIP_LastResponseReceivedUtc = ZDateTime.Empty;

				var arInvoiceCurrent = (ARInvoice)((IAmending)arInvoiceDiscardedWhenEInvoicingWasDisabled).GenerateAmendingTransaction(TransactionTypes.Invoice);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceCurrent, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "number 2", 200m);
				arInvoiceCurrent.Lines[0].AL_Sequence = 1;
				arInvoiceCurrent.Lines[1].AL_Sequence = 2;
				arInvoiceCurrent.Lines[2].AL_Sequence = 5;
				arInvoiceCurrent.Factory.Save();

				var batchCurrent = TestObjectCreator.CreateEInvoicingBatch(102, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivotCurrent = TestObjectCreator.CreateEInvoicingTransactionPivot(batchCurrent, arInvoiceCurrent, Core.Constants.EInvoicingPivotState.Batched, actionType: Core.Constants.EInvoicingPivotActionType.Submit);
				pivotCurrent.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;

				Factory.Save();

				var batch = CreateTransactionBatchForTest(invoiceNumber: arInvoiceCurrent.AH_TransactionNum);
				var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
				model.LoadData(Factory);

				var expectedInvoices = new[]
				{
					new PreviousInvoice() { AH_TransactionNum = arInvoiceDiscardedWhenEInvoicingWasDisabled.AH_TransactionNum, AH_TransactionType = "INV", WasSubmittedSuccessfully = false, MaximumLineSequence = 2 },
				};
				AssertArrayEqualsByElements("PreviousInvoices should be DCD transaction only", expectedInvoices, model.PreviousInvoices.ToArray());

				Assert("There should be no errors logged", !Notifications.HasErrors);
				Assert("There should be no warnings logged", !Notifications.HasWarnings);
			}
		}

		[TestDate(2020, 5, 1)]
		[TestDateIncremental(hours: 1)]
		public void TestPreviousInvoices_WhenOriginalInvoiceHasNoPivot()
		{
			Branch = TestObjectCreator.CreateHungaryBranchWithAddressAndTaxNumber();
			Company = Branch.Company;
			Factory.Save();

			AssertNotNull("Precondition: Branch has HomePort with TimeZoneSet", Branch.HomePort?.TimeZoneSet);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoiceWithoutPivotCreatedPriorToEInvoicing = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceWithoutPivotCreatedPriorToEInvoicing, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "second line", 101m);
				arInvoiceWithoutPivotCreatedPriorToEInvoicing.Lines[0].AL_Sequence = 7;
				arInvoiceWithoutPivotCreatedPriorToEInvoicing.Lines[1].AL_Sequence = 10;
				arInvoiceWithoutPivotCreatedPriorToEInvoicing.Factory.Save();

				var arInvoiceCurrent = (ARInvoice)((IAmending)arInvoiceWithoutPivotCreatedPriorToEInvoicing).GenerateAmendingTransaction(TransactionTypes.Invoice);
				TestObjectCreator.CreateARInvoiceLine(arInvoiceCurrent, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "number 2", 200m);
				arInvoiceCurrent.Lines[0].AL_Sequence = 1;
				arInvoiceCurrent.Lines[1].AL_Sequence = 2;
				arInvoiceCurrent.Lines[2].AL_Sequence = 5;
				arInvoiceCurrent.Factory.Save();

				var batchCurrent = TestObjectCreator.CreateEInvoicingBatch(102, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivotCurrent = TestObjectCreator.CreateEInvoicingTransactionPivot(batchCurrent, arInvoiceCurrent, Core.Constants.EInvoicingPivotState.Batched, actionType: Core.Constants.EInvoicingPivotActionType.Submit);
				pivotCurrent.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;

				Factory.Save();

				var batch = CreateTransactionBatchForTest(invoiceNumber: arInvoiceCurrent.AH_TransactionNum);
				var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
				model.LoadData(Factory);

				var expectedInvoices = new[]
				{
					new PreviousInvoice() { AH_TransactionNum = arInvoiceWithoutPivotCreatedPriorToEInvoicing.AH_TransactionNum, AH_TransactionType = "INV", WasSubmittedSuccessfully = false, MaximumLineSequence = 2 },
				};
				AssertArrayEqualsByElements("PreviousInvoices should be original AR Transaction only", expectedInvoices, model.PreviousInvoices.ToArray());

				Assert("There should be no errors logged", !Notifications.HasErrors);
				Assert("There should be no warnings logged", !Notifications.HasWarnings);
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestDeliveryDate()
		{
			var batch = CreateTransactionBatchForTest();
			var model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			var journal1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			var journal2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			model.Transaction.PostingJournalCollection.Add(journal1);
			model.Transaction.PostingJournalCollection.Add(journal2);
			journal1.TaxDate = new ZDate(2021, 5, 20);
			journal2.TaxDate = new ZDate(2021, 5, 25);

			batch.transactionInfo.TransactionDate = new ZDateTime(2021, 5, 15);
			AssertEquals("invoice date before journal tax dates, we expect delivery date to use invoice date", new ZDateTime(2021, 5, 15), model.DeliveryDate);

			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			batch.transactionInfo.TransactionDate = new ZDateTime(2021, 5, 22);
			AssertEquals("invoice date between journal tax dates, we expect delivery date to use invoice date", new ZDateTime(2021, 5, 22), model.DeliveryDate);

			model = GetModelForTest(batch.transactionInfo, batch.extraInfo, "1234", Notifications);
			batch.transactionInfo.TransactionDate = new ZDateTime(2021, 5, 30);
			AssertEquals("invoice date after journal tax dates, we expect delivery date to use latest journal tax date", new ZDateTime(2021, 5, 25), model.DeliveryDate);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Notifications = new NotificationBuffer();
		}

		ManageInvoiceRequestModel GetModelForTest(TransactionInfo transactionInfo, HungaryTransactionExtraInfo transactionExtraInfo, string batchNumber, INotifications notifications)
		{
			return new ManageInvoiceRequestModel(transactionInfo, transactionExtraInfo, batchNumber, notifications);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		NotificationBuffer Notifications;

		GlbBranch Branch;
		GlbCompany Company;
		GlbCompanyExternalPasswordHUI Credentials;

		(TransactionInfo transactionInfo, HungaryTransactionExtraInfo extraInfo) CreateTransactionBatchForTest(
			bool includeCredentials = true,
			string registrationNumber = "29573262",
			string registrationCountry = "HU",
			string registrationType = "VAT",
			string invoiceNumber = "1000",
			string originalTransactionNumber = "",
			bool isReciprocal = false
			)
		{
			Branch = Branch ?? TestObjectCreator.CreateHungaryBranchWithAddressAndTaxNumber();
			Company = Company ?? Branch.Company;
			Company.GC_IsReciprocal = isReciprocal;
			if (includeCredentials)
			{
				Credentials = TestObjectCreator.CreateHungaryEInvoicingCredentials(Company);
			}

			var result = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var address1 = new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "Tropicana Las Vegas Casino",
				Address1 = "Vigadó u. 2",
				City = "Budapest",
				State = "BU",
				Postcode = "1051",
				Country = new UniversalDataBuss.DataObjects.Universal.Country() { Name = "Hungary", Code = "HU" },
			};
			address1.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
			{
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "VAT" },
					Value = "12036024",
				},
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "GBR" },
					Value = "81237290",
				}
			});
			var address2 = new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "Középkori Romkert (Medieval Ruin Garden)",
				Address1 = "Koronázó tér 1",
				City = "Székesfehérvár",
				Postcode = "8000",
				Country = new UniversalDataBuss.DataObjects.Universal.Country() { Name = "Hungary", Code = "HU" },
			};
			address2.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
			{
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = registrationCountry },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = registrationType },
					Value = registrationNumber
				}
			});
			var info = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Number = invoiceNumber,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionType.INV,
				TransactionDate = ZDateTime.Today,
				Branch = new Branch() { Code = Branch.GB_Code, Name = Branch.GB_BranchName },
				Category = "FIN",
				LocalCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
				OSCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
				ExchangeRate = 1.0m,
				BranchAddress = address1,
				OrganizationAddress = address2,
				OriginalReference = string.IsNullOrEmpty(originalTransactionNumber) ? null : new OriginalReference() { OriginalTransactionNumber = originalTransactionNumber }
			};
			info.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = 100m,
					OSGSTVATAmount = 27m,
					OSTotalAmount = 127m,
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
					ChargeExchangeRate = 1.0m,
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					OSAmount = 200m,
					OSGSTVATAmount = 0m,
					OSTotalAmount = 200m,
					LocalAmount = 200m,
					LocalGSTVATAmount = 0m,
					LocalTotalAmount = 200m,
					VATTaxID = new TaxID() { TaxCode = "FREEVAT", TaxRate = 0, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
					TaxMessageID = new TaxMessageID() { Description = "Some reason why this was excluded from paying tax" },
					ChargeExchangeRate = 1.0m,
				},
			});

			return (info, new HungaryTransactionExtraInfo());
		}

		#endregion
	}
}
