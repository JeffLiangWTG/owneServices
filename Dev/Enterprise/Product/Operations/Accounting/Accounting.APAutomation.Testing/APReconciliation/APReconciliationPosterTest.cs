using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class APInvoiceConverterFromDraftInvoiceTest : APReconciliationPosterTest<APInvoice>
	{
		protected override string TransactionType => TransactionTypes.Invoice;

		protected override string ExpectedHumanReadableTransactionType => "invoice";

		protected override bool ExpectedShouldShowOriginalInvoiceReferenceFields => false;

		protected override AccDraftInvoiceHeader CreateValidDraftInvoice()
		{
			return GetValidDraftInvoice(250m, 250m, 25m);
		}
	}

	public class APCreditNoteConverterFromDraftInvoiceTest : APReconciliationPosterTest<APCreditNote>
	{
		protected override string TransactionType => TransactionTypes.CreditNote;

		protected override string ExpectedHumanReadableTransactionType => "credit note";

		protected override bool ExpectedShouldShowOriginalInvoiceReferenceFields => true;

		protected override AccDraftInvoiceHeader CreateValidDraftInvoice()
		{
			return GetValidDraftInvoice(-250m, 250m, 25m);
		}
	}

	[TestedType(typeof(APReconciliationPoster))]
	public abstract class APReconciliationPosterTest<T> : TestCaseWithFactory
		where T : InvoicingBase
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(InvoiceConverter);
			AssertType<APReconciliationPoster>(InvoiceConverter);
		}

		protected abstract string TransactionType { get; }

		protected abstract string ExpectedHumanReadableTransactionType { get; }

		protected abstract bool ExpectedShouldShowOriginalInvoiceReferenceFields { get; }

		#region PostFromDraftInvoice

		[TestDate(2024, 03, 22)]
		public void TestPostFromDraftInvoice_ConversionWithDefaultValue()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			CombineAssertions("PreCondition, Draft Invoice", () =>
			{
				AssertEquals("AIH_GC_Company", GlbCompany.CurrentCompany.PK, draftInvoice.AIH_GC_Company);
				AssertEquals("AIH_GB_Branch", TestObjectCreator.NonCurrentBranch.PK, draftInvoice.AIH_GB_Branch);
				AssertEquals("AIH_GE_Department", TestObjectCreator.NonCurrentDepartment.PK, draftInvoice.AIH_GE_Department);
				AssertEquals("AIH_TransactionNumber", ZString.Empty, draftInvoice.AIH_TransactionNumber);
				AssertEquals("AIH_OH_Creditor", TestObjectCreator.Creditor4.PK, draftInvoice.AIH_OH_Creditor);
				AssertEquals("AIH_Description", "AP Desc", draftInvoice.AIH_Description);
				AssertEquals("AIH_OA_CreditorAddress", ZGuid.Empty, draftInvoice.AIH_OA_CreditorAddress);
				AssertEquals("AIH_OC_CreditorContact", ZGuid.Empty, draftInvoice.AIH_OC_CreditorContact);
				AssertEquals("AIH_RX_NKTransactionCurrency", TestObjectCreator.AUD.Code, draftInvoice.AIH_RX_NKTransactionCurrency);
				AssertEquals("AIH_PostDate", ZDate.Empty, draftInvoice.AIH_PostDate);
				AssertEquals("AIH_DueDate", ZDate.Empty, draftInvoice.AIH_DueDate);
				AssertEquals("AIH_TransactionDate", ZDate.Empty, draftInvoice.AIH_TransactionDate);
				AssertEquals("AIH_DocumentReceivedDate", ZDate.Empty, draftInvoice.AIH_DocumentReceivedDate);
				AssertEquals("AIH_ExpectedOSExTaxAmount", 0m, draftInvoice.AIH_ExpectedOSExTaxAmount);
				AssertEquals("AIH_ExpectedOSTaxAmount", 0m, draftInvoice.AIH_ExpectedOSTaxAmount);
				AssertEquals("AIH_ExpectedOSTotalAmount", 0m, draftInvoice.AIH_ExpectedOSTotalAmount);
				AssertEquals("AIH_InternalReference", "00001001", draftInvoice.AIH_InternalReference);
				AssertEquals("AIH_AH_OriginalTransaction", ZGuid.Empty, draftInvoice.AIH_AH_OriginalTransaction);
				AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting, draftInvoice.AIH_Status);
			});

			using var disableReconciliation = DisableReconciliation();
			var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
			Assert(validationError == default);
			Assert(reconciliationError == default);
			CombineAssertions("AP Transaction Default", () =>
			{
				AssertEquals("ShouldShowOriginalInvoiceReferenceFields", ExpectedShouldShowOriginalInvoiceReferenceFields, newAP.ShouldShowOriginalInvoiceReferenceFields);
				AssertEquals("SubmittedFromInvoicingForm", expected: true, newAP.SubmittedFromInvoicingForm);
				AssertEquals("AH_Ledger", LedgerTypes.AccountsPayable, newAP.AH_Ledger);
				AssertEquals("AH_TransactionType", TransactionType, newAP.AH_TransactionType);
				AssertEquals("AH_GC", GlbCompany.CurrentCompany.PK, newAP.AH_GC);
				AssertEquals("AH_GB", GlbBranch.CurrentBranch.PK, newAP.AH_GB);
				AssertEquals("AH_GE", GlbDepartment.CurrentDepartment.PK, newAP.AH_GE);
				AssertEquals("AH_OH", TestObjectCreator.Creditor4.PK, newAP.AH_OH);
				AssertEquals("AH_Desc", "AP Desc", newAP.AH_Desc);
				AssertEquals("AH_TransactionNum", ZString.Empty, newAP.AH_TransactionNum);
				AssertEquals("AH_OA_InvoiceAddressOverride", ZGuid.Empty, newAP.AH_OA_InvoiceAddressOverride);
				AssertEquals("AH_OC_InvoiceContactOverride", ZGuid.Empty, newAP.AH_OC_InvoiceContactOverride);
				AssertEquals("AH_RX_NKTransactionCurrency", TestObjectCreator.AUD.Code, newAP.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_PostDate", new ZDate(2024, 03, 22), newAP.AH_PostDate);
				AssertEquals("AH_DueDate", new ZDate(2024, 03, 22), newAP.AH_DueDate);
				AssertEquals("AH_InvoiceDate", new ZDate(2024, 03, 22), newAP.AH_InvoiceDate);
				AssertEquals("AH_DocumentReceivedDate", ZDate.Empty, newAP.AH_DocumentReceivedDate);
				AssertEquals("IsExpectedTaxTotalVisible", expected: true, newAP.IsExpectedTaxTotalVisible);
				AssertEquals("ValidateExpectedInvoiceTotal", expected: true, newAP.ValidateExpectedInvoiceTotal);
				AssertEquals("ExpectedInvoiceExclTaxTotal", 0m, newAP.ExpectedInvoiceExclTaxTotal);
				AssertEquals("ExpectedInvoiceTaxTotal", 0m, newAP.ExpectedInvoiceTaxTotal);
				AssertEquals("ExpectedInvoiceTotal", 0m, newAP.ExpectedInvoiceTotal);
				AssertEquals("AH_TransactionReference", ZString.Empty, newAP.AH_TransactionReference);
				AssertEquals("OriginalTransactionReference", ZGuid.Empty, newAP.OriginalTransactionReference);
				AssertEquals("AH_OriginalTransactionNum", ZString.Empty, newAP.AH_OriginalTransactionNum);
				AssertEquals("AH_OriginalInvoiceDate", ZDate.Empty, newAP.AH_OriginalInvoiceDate);
				AssertEquals("DraftInvoiceHeaderPK", draftInvoice.PK, newAP.DraftInvoiceHeaderPK);

				var sourceDraftInvoice = newAP.Factory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);
				AssertEquals("AIH_AH_PostedTransactionHeader", newAP.PK, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
				AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.Processed, sourceDraftInvoice.AIH_Status);
			});
		}

		[TestDate(2024, 03, 22)]
		public void TestPostFromDraftInvoice_ConversionWithOverriddenValue_RealOriginalTransaction()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor4);
			AssertEquals("PreCondtion, Original InvoiceDate", new ZDateTime(2024, 03, 22), apInvoice.AH_InvoiceDate);
			AssertEquals("PreCondtion, Original Transaction Line Count", 1, apInvoice.Lines.Count);
			AssertTransactionLine("PreCondtion, Original Transaction Line", apInvoice.Lines[0], true);
			Factory.Save();

			AssertConversionWithOverriddenValue(
				draftInvoice => draftInvoice.AIH_AH_OriginalTransaction = apInvoice.PK
				, newAP =>
				{
					AssertEquals("ShouldShowOriginalInvoiceReferenceFields", ExpectedShouldShowOriginalInvoiceReferenceFields, newAP.ShouldShowOriginalInvoiceReferenceFields);
					if (newAP.ShouldShowOriginalInvoiceReferenceFields)
					{
						CombineAssertions("When user set AIH_AH_OriginalTransaction with existing invoice, OriginalTransactionReference would be populated, and AH_OriginalInvoiceDate Date & AH_OriginalTransactionNum would be empty.", () =>
						{
							AssertEquals("OriginalTransactionReference", apInvoice.PK, newAP.OriginalTransactionReference);
							AssertEquals("Mapped lines count should be same to Original Transaction", 1, newAP.Lines.Count);
							AssertTransactionLine("Mapped line should be set value from Original Transaction,", newAP.Lines[0], false);

							AssertEquals("AH_OriginalInvoiceDate", new ZDateTime(2024, 03, 22), newAP.AH_OriginalInvoiceDate);
							AssertEquals("AH_OriginalTransactionNum", "AP100001", newAP.AH_OriginalTransactionNum);

							newAP.AH_TransactionNum = "NEWAP100001";
							newAP.Factory.Save();

							var newAP_reload = new BusinessObjectFactory().Load<AccTransactionHeader>(newAP.PK);
							AssertEquals("OriginalTransactionReference in DB", apInvoice.PK, newAP_reload.AH_TransactionBelongsToGroup);
							AssertEquals("AH_OriginalInvoiceDate should not be stored to DB", ZDateTime.Empty, newAP_reload.AH_OriginalInvoiceDate);
							AssertEquals("AH_OriginalTransactionNum should not be stored to DB", ZString.Empty, newAP_reload.AH_OriginalTransactionNum);
						});
					}
					else
					{
						AssertWhenNotAssignOriginalTransactionInfo(newAP);
					}
				}
			);

			void AssertTransactionLine(string comment, AccTransactionLines line, bool invertSigns)
			{
				AssertEquals($"{comment} AL_GC", GlbCompany.CurrentCompany.PK, line.AL_GC);
				AssertEquals($"{comment} AL_RX_NKTransactionCurrency", TestObjectCreator.AUD.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals($"{comment} AL_ExchangeRate", 1m, line.AL_ExchangeRate);
				AssertEquals($"{comment} AL_Desc", "tee he he", line.AL_Desc);
				AssertEquals($"{comment} AL_AG", TestObjectCreator.GLHeader1.PK, line.AL_AG);

				AssertEquals($"{comment} AL_LineAmount", invertSigns ? -250m : 250m, line.AL_LineAmount);
				AssertEquals($"{comment} AL_GSTVAT", invertSigns ? -25m : 25m, line.AL_GSTVAT);
				AssertEquals($"{comment} AL_OSAmount", invertSigns ? -275m : 275m, line.AL_OSAmount);

				AssertEquals($"{comment} AL_OH", ZGuid.Empty, line.AL_OH);
				AssertEquals($"{comment} AL_AC", ZGuid.Empty, line.AL_AC);
				AssertEquals($"{comment} AL_AT", ZGuid.Empty, line.AL_AT);
				AssertEquals($"{comment} AL_A9_VATClass", ZGuid.Empty, line.AL_A9_VATClass);
			}
		}

		[TestDate(2024, 03, 22)]
		public void TestPostFromDraftInvoice_ConversionWithOverriddenValue_CustomizedOriginalTransactionNum()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
			AssertEquals("PreCondtion, Original InvoiceDate", new ZDateTime(2024, 03, 22), apInvoice.AH_InvoiceDate);
			Factory.Save();

			AssertConversionWithOverriddenValue(
				draftInvoice =>
				{
					draftInvoice.AIH_OriginalTransactionNum = apInvoice.AH_TransactionNum;
					draftInvoice.AIH_OriginalInvoiceDate = apInvoice.AH_InvoiceDate.Date;
				}
				, newAP =>
				{
					AssertEquals("ShouldShowOriginalInvoiceReferenceFields", ExpectedShouldShowOriginalInvoiceReferenceFields, newAP.ShouldShowOriginalInvoiceReferenceFields);
					if (newAP.ShouldShowOriginalInvoiceReferenceFields)
					{
						CombineAssertions("When user set AIH_OriginalTransactionNum & AIH_OriginalInvoiceDate instead of AIH_AH_OriginalTransaction , AH_OriginalInvoiceDate Date & AH_OriginalTransactionNum would be reocrded to DB.", () =>
						{
							AssertEquals("OriginalTransactionReference", ZGuid.Empty, newAP.OriginalTransactionReference);
							AssertEquals("We should not get any transaction line.", 0, newAP.Lines.Count);

							AssertEquals("AH_OriginalInvoiceDate", new ZDateTime(2024, 03, 22), newAP.AH_OriginalInvoiceDate);
							AssertEquals("AH_OriginalTransactionNum", "AP100001", newAP.AH_OriginalTransactionNum);

							newAP.AH_TransactionNum = "NEWAP100001";
							TestObjectCreator.CreateInvoiceLine(newAP
								, TestObjectCreator.AUD, 1m
								, 200m, 0m, 0m
								, 200m, 0m, 0m);
							newAP.Factory.Save();

							var newAPCRD_reload = new BusinessObjectFactory().Load<AccTransactionHeader>(newAP.PK);
							AssertEquals("OriginalTransactionReference should not be stored to DB", ZGuid.Empty, newAPCRD_reload.AH_TransactionBelongsToGroup);
							AssertEquals("AH_OriginalInvoiceDate in DB", new ZDateTime(2024, 03, 22), newAPCRD_reload.AH_OriginalInvoiceDate);
							AssertEquals("AH_OriginalTransactionNum in DB", "AP100001", newAPCRD_reload.AH_OriginalTransactionNum);
						});
					}
					else
					{
						AssertWhenNotAssignOriginalTransactionInfo(newAP);
					}
				}
			);
		}

		public void TestPostFromDraftInvoice_WithLocalCurrencyDefaultExchangeRate()
		{
			AssertEquals("PreCondition, local currency code.", TestObjectCreator.AUD.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertPostFromDraftInvoice_ExchangeRate(TestObjectCreator.AUD.RX_Code, 1m);
		}

		public void TestPostFromDraftInvoice_WithForeignCurrencyDefaultExchangeRate()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AED, 1.525m);
			AssertPostFromDraftInvoice_ExchangeRate(TestObjectCreator.AED.RX_Code, 1.525m);
		}

		public void TestPostFromDraftInvoice_IncorrectTransactionCompany()
		{
			var nonCurrentCompany = TestObjectCreator.CreateNewCompany("TST", TestObjectCreator.CreditorTR);
			nonCurrentCompany.CompanyName = "Company Name AAAAAAAA";

			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_GC_Company = nonCurrentCompany.PK;
			Factory.Save();

			var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
			Assert(reconciliationError == default);
			AssertEquals($@"You are trying to post a draft {ExpectedHumanReadableTransactionType} to company TST - Company Name AAAAAAAA.
You are either not logged into CargoWise, or logged in to a different company.
Please login to company TST - Company Name AAAAAAAA before attempting to post the draft {ExpectedHumanReadableTransactionType}."
				, validationError.Msg);
			AssertEquals($"Draft {ExpectedHumanReadableTransactionType} cannot be posted", validationError.Caption);
			AssertNull(newAP);
			AssertDraftInvoiceStatusAndLinkUnchanged(draftInvoice);
		}

		public void TestPostFromDraftInvoice_DraftTransactionAlreadyPosted()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			Factory.Save();

			var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
			Assert(reconciliationError == default);
			AssertEquals($"This draft {ExpectedHumanReadableTransactionType} is already either posted as an AP {ExpectedHumanReadableTransactionType}, an AP {ExpectedHumanReadableTransactionType} awaiting approval, or AP {ExpectedHumanReadableTransactionType} has been saved as incomplete."
				, validationError.Msg);
			AssertEquals($"Draft {ExpectedHumanReadableTransactionType} already posted"
				, validationError.Caption);
			AssertNull(newAP);
		}

		public void TestPostFromDraftInvoice_ReconcileDraftInvoiceSuccess()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;
			var dummyAccruals = new[] {
				new APReconciliationLine() { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new ZGuid("A7A0E470-47F6-4699-AAF0-E05954FD3C17") }
				, new APReconciliationLine() { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new ZGuid("729BBF4A-C3AF-43DF-BB07-BB4407D9567E") }
				, new APReconciliationLine() { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new ZGuid("C91B0E13-3877-4E33-8831-F49CF859A4F8") }
				, new APReconciliationLine() { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new ZGuid("A132BC49-11A8-474A-B7AA-614483684C15") }
			};

			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice))
				.Returns(() => new APReconciliationProcessingResult
				{
					Result = APReconciliationResultTypes.Success,
					FailureReason = "Dummy Fail Reason that would not be used",
					ReconciliableAccruals = dummyAccruals
				});

			var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);
			mockReconciliationLineConverter.Setup(x => x.ImportReconciliationLinesToInvoice(
				It.Is<InvoicingBase>(x => x.PK == draftInvoice.AIH_AH_PostedTransactionHeader && draftInvoice.AIH_AH_PostedTransactionHeader != ZGuid.Empty)
				, dummyAccruals)
			);

			using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
			using (ObjectFactory.Substitute(mockReconciliationLineConverter.Object))
			{
				var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
				AssertNotNull(draftInvoice);
				Assert(validationError == default);
				Assert(reconciliationError == default);
			}

			mockReconciliationProcessor.Verify(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>())
				, Times.Exactly(1));
			mockReconciliationLineConverter.Verify(x => x.ImportReconciliationLinesToInvoice(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<IReconciliationLineIdentifier>>())
				, Times.Exactly(1));
		}

		public void TestPostFromDraftInvoice_DoNotCallReconcileWhenHavingSelectedAccrual()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;
			var dummySelectedAccruals = new PosterConfigurationDTO.Accruals[] {
				new () { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new Guid("A7A0E470-47F6-4699-AAF0-E05954FD3C17") }
				, new () { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new Guid("729BBF4A-C3AF-43DF-BB07-BB4407D9567E") }
				, new () { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new Guid("C91B0E13-3877-4E33-8831-F49CF859A4F8") }
				, new () { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new Guid("A132BC49-11A8-474A-B7AA-614483684C15") }
			};

			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice));

			var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);
			mockReconciliationLineConverter.Setup(x => x.ImportReconciliationLinesToInvoice(
				It.Is<InvoicingBase>(x => x.PK == draftInvoice.AIH_AH_PostedTransactionHeader && draftInvoice.AIH_AH_PostedTransactionHeader != ZGuid.Empty)
				, dummySelectedAccruals)
			);

			using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
			using (ObjectFactory.Substitute(mockReconciliationLineConverter.Object))
			{
				var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice
					, new PosterConfigurationDTO
					{
						SelectedAccruals = dummySelectedAccruals
					}
					, out var validationError, out var reconciliationError);
				AssertNotNull(draftInvoice);
				Assert(validationError == default);
				Assert(reconciliationError == default);
			}

			mockReconciliationProcessor.Verify(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>())
				, Times.Exactly(0));
			mockReconciliationLineConverter.Verify(x => x.ImportReconciliationLinesToInvoice(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<IReconciliationLineIdentifier>>())
				, Times.Exactly(1));
		}

		public void TestPostFromDraftInvoice_ShouldCallReconcileWhenNoSelectedAccrual()
		{
			var draftInvoicePK = GetBusinessObjectThatIsInTheDatabase().PK;

			AssertShouldCallReconcileWhenNoSelectedAccrual(null);
			AssertShouldCallReconcileWhenNoSelectedAccrual(Array.Empty<PosterConfigurationDTO.Accruals>());

			void AssertShouldCallReconcileWhenNoSelectedAccrual(PosterConfigurationDTO.Accruals[] emptySelectedAccruals)
			{
				var newFactory = new BusinessObjectFactory();
				var draftInvoice = newFactory.Load<AccDraftInvoiceHeader>(draftInvoicePK);

				draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;
				var dummyReconciliableAccruals = new APReconciliationLine[] {
					new () { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new ZGuid("A7A0E470-47F6-4699-AAF0-E05954FD3C17") }
					, new () { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new ZGuid("729BBF4A-C3AF-43DF-BB07-BB4407D9567E") }
					, new () { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new ZGuid("C91B0E13-3877-4E33-8831-F49CF859A4F8") }
					, new () { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new ZGuid("A132BC49-11A8-474A-B7AA-614483684C15") }
				};

				var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
				mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice))
					.Returns(() => new APReconciliationProcessingResult
					{
						Result = APReconciliationResultTypes.Success,
						FailureReason = "Dummy Fail Reason that would not be used",
						ReconciliableAccruals = dummyReconciliableAccruals
					});

				var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);
				mockReconciliationLineConverter.Setup(x => x.ImportReconciliationLinesToInvoice(
					It.Is<InvoicingBase>(x => x.PK == draftInvoice.AIH_AH_PostedTransactionHeader && draftInvoice.AIH_AH_PostedTransactionHeader != ZGuid.Empty)
					, dummyReconciliableAccruals)
				);

				using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
				using (ObjectFactory.Substitute(mockReconciliationLineConverter.Object))
				{
					InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice
						, new PosterConfigurationDTO
						{
							SelectedAccruals = emptySelectedAccruals
						}
						, out var validationError, out var reconciliationError);
					AssertNotNull(draftInvoice);
					Assert(validationError == default);
					Assert(reconciliationError == default);

					mockReconciliationProcessor.Verify(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>())
						, Times.Exactly(1));
					mockReconciliationLineConverter.Verify(x => x.ImportReconciliationLinesToInvoice(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<IReconciliationLineIdentifier>>())
						, Times.Exactly(1));
				}
			}
		}

		public void TestPostFromDraftInvoice_DraftTransactionIsAwaitingApproval()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.AwaitingApproval;
			Factory.Save();

			InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
			Assert(reconciliationError == default);
			AssertEquals("This invoice has been marked as Awaiting Approval. Please approve the invoice prior to posting.", validationError.Msg);
			AssertEquals("Transactions Awaiting Approved cannot be posted.", validationError.Caption);
		}

		public void TestPostFromDraftInvoice_ReconcileDraftInvoiceFail()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice))
				.Returns(() => new APReconciliationProcessingResult { Result = APReconciliationResultTypes.Failed, FailureReason = "Dummy Fail Reason" });

			var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
			using (ObjectFactory.Substitute(mockReconciliationLineConverter.Object))
			{
				var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
				AssertNotNull(draftInvoice);
				Assert(validationError == default);
				CombineAssertions("Reconciliation Fail Reason", () =>
				{
					AssertEquals("Caption", $"Unable to reconcile the {ExpectedHumanReadableTransactionType}", reconciliationError.Caption);
					AssertEquals("Message", "Dummy Fail Reason", reconciliationError.Msg);
				});
			}

			mockReconciliationProcessor.Verify(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>())
				, Times.Exactly(1));
			mockReconciliationLineConverter.Verify(x => x.ImportReconciliationLinesToInvoice(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<IReconciliationLineIdentifier>>())
				, Times.Exactly(0));
		}

		[TestDate(2024, 03, 20)]
		public void TestPostFromDraftInvoice_WithoutTaxFrameworkButWithValidDraftInvoice()
		{
			var draftInvoice = CreateValidDraftInvoice();
			var mockTaxProcessor = new Mock<ITaxProcessor>(MockBehavior.Strict);
			mockTaxProcessor.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns(string.Empty);
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var invoicingBase = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);

			AssertEquals(true, validationError == default);
			AssertEquals(false, invoicingBase.HasErrors);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Never);
		}

		public void TestPostFromDraftInvoice_WithTaxFrameworkAndInvalidDraftInvoice()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			var mockTaxProcessor = GetTaxProcessorByTaxFramework(string.Empty);

			var invoicingBase = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);

			CombineAssertions("Validation Fail Reason", () =>
			{
				AssertEquals("There are Accounting transaction errors to be corrected before calculating tax records.", validationError.Msg);
				AssertEquals("Fail to calculate tax records", validationError.Caption);
			});
			Assert(invoicingBase.HasErrors);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Never);
		}

		[TestDate(2024, 03, 20)]
		public void TestPostFromDraftInvoice_WithTaxFrameworkAndValidDraftInvoice_SuccessfullyProcessed()
		{
			var draftInvoice = CreateValidDraftInvoice();
			var mockTaxProcessor = GetTaxProcessorByTaxFramework(string.Empty);

			var invoicingBase = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);

			AssertEquals(true, validationError == default);
			AssertEquals(false, invoicingBase.HasErrors);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(1));
		}

		[TestDate(2024, 03, 20)]
		public void TestPostFromDraftInvoice_WithTaxFrameworkAndValidDraftInvoice_FailToProcess()
		{
			var draftInvoice = CreateValidDraftInvoice();
			var mockTaxProcessor = GetTaxProcessorByTaxFramework(taxProcessErrorMsg: "Dummy tax process Error.");

			var invoicingBase = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);

			CombineAssertions("Validation Fail Reson", () =>
			{
				AssertEquals("Dummy tax process Error.", validationError.Msg);
				AssertEquals("Fail to calculate tax records", validationError.Caption);
			});
			AssertEquals(false, invoicingBase.HasErrors);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(1));
		}

		void AssertPostFromDraftInvoice_ExchangeRate(string currencyCode, ZDecimal expectedExchangeRate)
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_RX_NKTransactionCurrency = currencyCode;
			Factory.Save();

			using var disableReconciliation = DisableReconciliation();
			var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);
			Assert(validationError == default);
			Assert(reconciliationError == default);
			CombineAssertions("AP Transaction Default", () =>
			{
				AssertEquals("AH_RX_NKTransactionCurrency", currencyCode, newAP.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_ExchangeRate", expectedExchangeRate, newAP.AH_ExchangeRate);
			});
		}

		void AssertConversionWithOverriddenValue(Action<AccDraftInvoiceHeader> additionalDataSet, Action<T> additionalAssertion)
		{
			var draftInvoiceOverride = GetBusinessObjectThatIsInTheDatabase();
			var creditorContact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor4, "Kim", "010 -0000-0000");
			var creditorAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor4, OrgAddressType.Payables, isMain: true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
			draftInvoiceOverride.AIH_OA_CreditorAddress = creditorAddress.PK;
			draftInvoiceOverride.AIH_OC_CreditorContact = creditorContact.PK;
			draftInvoiceOverride.AIH_PostDate = new ZDate(2024, 03, 20);
			draftInvoiceOverride.AIH_DueDate = new ZDate(2024, 03, 21);
			draftInvoiceOverride.AIH_TransactionDate = new ZDate(2024, 03, 19);
			draftInvoiceOverride.AIH_DocumentReceivedDate = new ZDate(2024, 03, 15);
			draftInvoiceOverride.AIH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			draftInvoiceOverride.AIH_ExpectedOSExTaxAmount = 250m;
			draftInvoiceOverride.AIH_ExpectedOSTaxAmount = 25m;
			draftInvoiceOverride.AIH_ExpectedOSTotalAmount = 275m;
			draftInvoiceOverride.AIH_Description = "Desc {444CF273-B6EF-468D-A9D5-59C9606CEB27}";
			draftInvoiceOverride.AIH_TransactionNumber = "AAAA_0001001";

			additionalDataSet?.Invoke(draftInvoiceOverride);
			Factory.Save();

			using var disableReconciliation = DisableReconciliation();
			var newAP = InvoiceConverter.PostFromDraftInvoice<T>(draftInvoiceOverride, null, out var validationError, out var reconciliationError);
			Assert(validationError == default);
			Assert(reconciliationError == default);
			AssertEquals("SubmittedFromInvoicingForm", expected: true, newAP.SubmittedFromInvoicingForm);
			AssertEquals("AH_TransactionType", TransactionType, newAP.AH_TransactionType);
			AssertEquals("AH_GC", GlbCompany.CurrentCompany.PK, newAP.AH_GC);
			AssertEquals("AH_GB", GlbBranch.CurrentBranch.PK, newAP.AH_GB);
			AssertEquals("AH_GE", GlbDepartment.CurrentDepartment.PK, newAP.AH_GE);
			AssertEquals("AH_OH", TestObjectCreator.Creditor4.PK, newAP.AH_OH);
			AssertEquals("AH_Desc", "Desc {444CF273-B6EF-468D-A9D5-59C9606CEB27}", newAP.AH_Desc);
			AssertEquals("AH_TransactionNum", "AAAA_0001001", newAP.AH_TransactionNum);
			AssertEquals("AH_OA_InvoiceAddressOverride", creditorAddress.PK, newAP.AH_OA_InvoiceAddressOverride);
			AssertEquals("AH_OC_InvoiceContactOverride", creditorContact.PK, newAP.AH_OC_InvoiceContactOverride);
			AssertEquals("AH_RX_NKTransactionCurrency", TestObjectCreator.AUD.Code, newAP.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_ExchangeRate", 1m, newAP.AH_ExchangeRate);
			AssertEquals("AH_PostDate", new ZDate(2024, 03, 20), newAP.AH_PostDate);
			AssertEquals("AH_DueDate", new ZDate(2024, 03, 21), newAP.AH_DueDate);
			AssertEquals("AH_InvoiceDate", new ZDate(2024, 03, 19), newAP.AH_InvoiceDate);
			AssertEquals("AH_DocumentReceivedDate", new ZDate(2024, 03, 15), newAP.AH_DocumentReceivedDate);
			AssertEquals("IsExpectedTaxTotalVisible", expected: true, newAP.IsExpectedTaxTotalVisible);
			AssertEquals("ValidateExpectedInvoiceTotal", expected: true, newAP.ValidateExpectedInvoiceTotal);
			AssertEquals("ExpectedInvoiceExclTaxTotal", 250m, newAP.ExpectedInvoiceExclTaxTotal);
			AssertEquals("ExpectedInvoiceTaxTotal", 25m, newAP.ExpectedInvoiceTaxTotal);
			AssertEquals("ExpectedInvoiceTotal", 275m, newAP.ExpectedInvoiceTotal);
			AssertEquals("AH_TransactionReference", ZString.Empty, newAP.AH_TransactionReference);
			AssertEquals("DraftInvoiceHeaderPK", draftInvoiceOverride.PK, newAP.DraftInvoiceHeaderPK);

			var sourceDraftInvoice = newAP.Factory.Load<AccDraftInvoiceHeader>(draftInvoiceOverride.PK);
			AssertEquals("AIH_AH_PostedTransactionHeader", newAP.PK, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
			AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.Processed, sourceDraftInvoice.AIH_Status);
		}

		#endregion

		#region AutoReconcileAndPost

		[TestDate(2024, 03, 22)]
		public void TestAutoReconcileAndPost_ReconciliationFail()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			CombineAssertions("PreCondition, Draft Invoice", () =>
			{
				AssertEquals("AIH_GC_Company", GlbCompany.CurrentCompany.PK, draftInvoice.AIH_GC_Company);
				AssertEquals("AIH_GB_Branch", TestObjectCreator.NonCurrentBranch.PK, draftInvoice.AIH_GB_Branch);
				AssertEquals("AIH_GE_Department", TestObjectCreator.NonCurrentDepartment.PK, draftInvoice.AIH_GE_Department);
				AssertEquals("AIH_TransactionNumber", ZString.Empty, draftInvoice.AIH_TransactionNumber);
				AssertEquals("AIH_OH_Creditor", TestObjectCreator.Creditor4.PK, draftInvoice.AIH_OH_Creditor);
				AssertEquals("AIH_Description", "AP Desc", draftInvoice.AIH_Description);
				AssertEquals("AIH_OA_CreditorAddress", ZGuid.Empty, draftInvoice.AIH_OA_CreditorAddress);
				AssertEquals("AIH_OC_CreditorContact", ZGuid.Empty, draftInvoice.AIH_OC_CreditorContact);
				AssertEquals("AIH_RX_NKTransactionCurrency", TestObjectCreator.AUD.Code, draftInvoice.AIH_RX_NKTransactionCurrency);
				AssertEquals("AIH_PostDate", ZDate.Empty, draftInvoice.AIH_PostDate);
				AssertEquals("AIH_DueDate", ZDate.Empty, draftInvoice.AIH_DueDate);
				AssertEquals("AIH_TransactionDate", ZDate.Empty, draftInvoice.AIH_TransactionDate);
				AssertEquals("AIH_DocumentReceivedDate", ZDate.Empty, draftInvoice.AIH_DocumentReceivedDate);
				AssertEquals("AIH_ExpectedOSExTaxAmount", 0m, draftInvoice.AIH_ExpectedOSExTaxAmount);
				AssertEquals("AIH_ExpectedOSTaxAmount", 0m, draftInvoice.AIH_ExpectedOSTaxAmount);
				AssertEquals("AIH_ExpectedOSTotalAmount", 0m, draftInvoice.AIH_ExpectedOSTotalAmount);
				AssertEquals("AIH_InternalReference", "00001001", draftInvoice.AIH_InternalReference);
				AssertEquals("AIH_AH_OriginalTransaction", ZGuid.Empty, draftInvoice.AIH_AH_OriginalTransaction);
				AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting, draftInvoice.AIH_Status);
			});

			Factory.SuspendValidation();
			using var disableReconciliation = DisableReconciliation();
			var newAP = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);
			Assert(errorMessage == default);
			CombineAssertions("AP Transaction Default", () =>
			{
				AssertEquals("ShouldShowOriginalInvoiceReferenceFields", ExpectedShouldShowOriginalInvoiceReferenceFields, newAP.ShouldShowOriginalInvoiceReferenceFields);
				AssertEquals("SubmittedFromInvoicingForm", expected: true, newAP.SubmittedFromInvoicingForm);
				AssertEquals("AH_Ledger", LedgerTypes.AccountsPayable, newAP.AH_Ledger);
				AssertEquals("AH_TransactionType", TransactionType, newAP.AH_TransactionType);
				AssertEquals("AH_GC", GlbCompany.CurrentCompany.PK, newAP.AH_GC);
				AssertEquals("AH_GB", GlbBranch.CurrentBranch.PK, newAP.AH_GB);
				AssertEquals("AH_GE", GlbDepartment.CurrentDepartment.PK, newAP.AH_GE);
				AssertEquals("AH_OH", TestObjectCreator.Creditor4.PK, newAP.AH_OH);
				AssertEquals("AH_Desc", "AP Desc", newAP.AH_Desc);
				AssertEquals("AH_TransactionNum", ZString.Empty, newAP.AH_TransactionNum);
				AssertEquals("AH_OA_InvoiceAddressOverride", ZGuid.Empty, newAP.AH_OA_InvoiceAddressOverride);
				AssertEquals("AH_OC_InvoiceContactOverride", ZGuid.Empty, newAP.AH_OC_InvoiceContactOverride);
				AssertEquals("AH_RX_NKTransactionCurrency", TestObjectCreator.AUD.Code, newAP.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_PostDate", new ZDate(2024, 03, 22), newAP.AH_PostDate);
				AssertEquals("AH_DueDate", new ZDate(2024, 03, 22), newAP.AH_DueDate);
				AssertEquals("AH_InvoiceDate", new ZDate(2024, 03, 22), newAP.AH_InvoiceDate);
				AssertEquals("AH_DocumentReceivedDate", ZDate.Empty, newAP.AH_DocumentReceivedDate);
				AssertEquals("IsExpectedTaxTotalVisible", expected: true, newAP.IsExpectedTaxTotalVisible);
				AssertEquals("ValidateExpectedInvoiceTotal", expected: true, newAP.ValidateExpectedInvoiceTotal);
				AssertEquals("ExpectedInvoiceExclTaxTotal", 0m, newAP.ExpectedInvoiceExclTaxTotal);
				AssertEquals("ExpectedInvoiceTaxTotal", 0m, newAP.ExpectedInvoiceTaxTotal);
				AssertEquals("ExpectedInvoiceTotal", 0m, newAP.ExpectedInvoiceTotal);
				AssertEquals("AH_TransactionReference", ZString.Empty, newAP.AH_TransactionReference);
				AssertEquals("OriginalTransactionReference", ZGuid.Empty, newAP.OriginalTransactionReference);
				AssertEquals("AH_OriginalTransactionNum", ZString.Empty, newAP.AH_OriginalTransactionNum);
				AssertEquals("AH_OriginalInvoiceDate", ZDate.Empty, newAP.AH_OriginalInvoiceDate);
				AssertEquals("DraftInvoiceHeaderPK", draftInvoice.PK, newAP.DraftInvoiceHeaderPK);

				var sourceDraftInvoice = newAP.Factory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);
				AssertEquals("AIH_AH_PostedTransactionHeader", newAP.PK, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
				AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.Processed, sourceDraftInvoice.AIH_Status);
			});
			Factory.ResumeValidation();
		}

		public void TestAutoReconcileAndPost_ReconciliationSuccess_PostingFail()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;
			var accruals = new List<APReconciliationLine>() {
				new() { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new Guid("A7A0E470-47F6-4699-AAF0-E05954FD3C17") },
				new() { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new Guid("A132BC49-11A8-474A-B7AA-614483684C15") }
			};

			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice))
				.Returns(() => new APReconciliationProcessingResult()
				{
					Result = APReconciliationResultTypes.Success,
					ReconciliableAccruals = accruals
				});

			var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);
			mockReconciliationLineConverter.Setup(x => x.ImportReconciliationLinesToInvoice(
				It.IsAny<InvoicingBase>(), accruals)
			);

			using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
			using (ObjectFactory.Substitute(mockReconciliationLineConverter.Object))
			{
				var newAP = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);
				AssertNotNull(draftInvoice);
				AssertNull(newAP);
				Assert(errorMessage != default);
				AssertEquals(ZGuid.Empty, draftInvoice.AIH_AH_PostedTransactionHeader);
				AssertEquals(AccDraftInvoiceHeaderStatus.ApprovedForPosting, draftInvoice.AIH_Status);
			}

			mockReconciliationLineConverter.Verify(
				x => x.ImportReconciliationLinesToInvoice(It.IsAny<InvoicingBase>(),
					It.IsAny<IEnumerable<IReconciliationLineIdentifier>>()), Times.Exactly(1));
		}

		public void TestAutoReconcileAndPost_IncorrectTransactionCompany()
		{
			var nonCurrentCompany = TestObjectCreator.CreateNewCompany("TST", TestObjectCreator.CreditorTR);
			nonCurrentCompany.CompanyName = "Company Name AAAAAAAA";

			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_GC_Company = nonCurrentCompany.PK;
			Factory.Save();

			var newAP = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);
			AssertEquals($@"You are trying to post a draft {ExpectedHumanReadableTransactionType} to company TST - Company Name AAAAAAAA.
You are either not logged into CargoWise, or logged in to a different company.
Please login to company TST - Company Name AAAAAAAA before attempting to post the draft {ExpectedHumanReadableTransactionType}."
				, errorMessage);
			AssertNull(newAP);
			AssertDraftInvoiceStatusAndLinkUnchanged(draftInvoice);
		}

		public void TestAutoReconcileAndPost_DraftTransactionAlreadyPosted()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			Factory.Save();

			var newAP = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);
			AssertEquals($"This draft {ExpectedHumanReadableTransactionType} is already either posted as an AP {ExpectedHumanReadableTransactionType}, an AP {ExpectedHumanReadableTransactionType} awaiting approval, or AP {ExpectedHumanReadableTransactionType} has been saved as incomplete."
				, errorMessage);
			AssertNull(newAP);
		}

		public void TestAutoReconcileAndPost_DraftTransactionIsAwaitingApproval()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.AwaitingApproval;
			Factory.Save();

			InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice,  out var errorMessage);
			AssertEquals("This invoice has been marked as Awaiting Approval. Please approve the invoice prior to posting.", errorMessage);
		}

		public void TestAutoReconcileAndPost_WithLocalCurrencyDefaultExchangeRate()
		{
			AssertEquals("PreCondition, local currency code.", TestObjectCreator.AUD.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			Factory.Save();

			Factory.SuspendValidation();
			using var disableReconciliation = DisableReconciliation();
			var newAP = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);
			Assert(errorMessage == default);
			CombineAssertions("AP Transaction Default", () =>
			{
				AssertEquals("AH_RX_NKTransactionCurrency", TestObjectCreator.AUD.RX_Code, newAP.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_ExchangeRate", 1m, newAP.AH_ExchangeRate);
			});
			Factory.ResumeValidation();
		}

		public void TestAutoReconcileAndPost_WithForeignCurrencyDefaultExchangeRate()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AED, 1.525m);

			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_RX_NKTransactionCurrency = TestObjectCreator.AED.RX_Code;
			Factory.Save();

			Factory.SuspendValidation();
			using var disableReconciliation = DisableReconciliation();
			var newAP = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);
			Assert(errorMessage == default);
			CombineAssertions("AP Transaction Default", () =>
			{
				AssertEquals("AH_RX_NKTransactionCurrency", TestObjectCreator.AED.RX_Code, newAP.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_ExchangeRate", 1.525m, newAP.AH_ExchangeRate);
			});
			Factory.ResumeValidation();
		}

		[TestDate(2024, 03, 20)]
		public void TestAutoReconcileAndPost_WithoutTaxFrameworkButWithValidDraftInvoice()
		{
			var draftInvoice = CreateValidDraftInvoice();
			var mockTaxProcessor = new Mock<ITaxProcessor>(MockBehavior.Strict);
			mockTaxProcessor.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns(string.Empty);
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			var invoicingBase = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);

			AssertEquals(true, errorMessage == default);
			AssertEquals(false, invoicingBase.HasErrors);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Never);
		}

		public void TestAutoReconcileAndPost_WithTaxFrameworkAndInvalidDraftInvoice()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			var mockTaxProcessor = GetTaxProcessorByTaxFramework(string.Empty);

			using var disableReconciliation = DisableReconciliation();
			var invoicingBase = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);

			AssertEquals("There are Accounting transaction errors to be corrected before calculating tax records.", errorMessage);
			AssertNull(invoicingBase);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Never);
		}

		[TestDate(2024, 03, 20)]
		public void TestAutoReconcileAndPost_WithTaxFrameworkAndValidDraftInvoice_SuccessfullyProcessed()
		{
			var draftInvoice = CreateValidDraftInvoice();
			var mockTaxProcessor = GetTaxProcessorByTaxFramework(string.Empty);

			var invoicingBase = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);

			AssertEquals(true, errorMessage == default);
			AssertEquals(false, invoicingBase.HasErrors);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(1));
		}

		[TestDate(2024, 03, 20)]
		public void TestAutoReconcileAndPost_WithTaxFrameworkAndValidDraftInvoice_FailToProcess()
		{
			var draftInvoice = CreateValidDraftInvoice();
			var mockTaxProcessor = GetTaxProcessorByTaxFramework(taxProcessErrorMsg: "Dummy tax process Error.");

			var invoicingBase = InvoiceConverter.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);

			AssertEquals("Dummy tax process Error.", errorMessage);
			AssertNull(invoicingBase);
			mockTaxProcessor.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(1));
		}

		#endregion

		void AssertDraftInvoiceStatusAndLinkUnchanged(AccDraftInvoiceHeader draftInvoice)
		{
			var sourceDraftInvoice = Factory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);
			AssertEquals("AIH_AH_PostedTransactionHeader", ZGuid.Empty, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
			AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting, sourceDraftInvoice.AIH_Status);
		}

		void AssertWhenNotAssignOriginalTransactionInfo(T newAP)
		{
			CombineAssertions("OriginalTransactionReference & AH_OriginalInvoiceDate Date & AH_OriginalTransactionNum keep empty and no lines will be created.", () =>
			{
				AssertEquals("OriginalTransactionReference", ZGuid.Empty, newAP.OriginalTransactionReference);
				AssertEquals("AH_OriginalInvoiceDate should not be stored to DB", ZDateTime.Empty, newAP.AH_OriginalInvoiceDate);
				AssertEquals("AH_OriginalTransactionNum should not be stored to DB", ZString.Empty, newAP.AH_OriginalTransactionNum);
				AssertEquals("We should not get any transaction line.", 0, newAP.Lines.Count);
			});
		}

		#region ReconcileFromDraftInvoice

		public void TestReconcileFromDraftInvoice()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;

			var dummyReconciliationResult = new APReconciliationProcessingResult
			{
				Result = APReconciliationResultTypes.Success,
				FailureReason = "Dummy Fail Reason that should be empty or null when result is Success.",
				ReconciliableAccruals = new[] {
					new APReconciliationLine() { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new ZGuid("A7A0E470-47F6-4699-AAF0-E05954FD3C17") }
					, new APReconciliationLine() { LineType = APReconciliationLineTypes.JobCharge, LineIdentifier = new ZGuid("729BBF4A-C3AF-43DF-BB07-BB4407D9567E") }
					, new APReconciliationLine() { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new ZGuid("C91B0E13-3877-4E33-8831-F49CF859A4F8") }
					, new APReconciliationLine() { LineType = APReconciliationLineTypes.ConsolCost, LineIdentifier = new ZGuid("A132BC49-11A8-474A-B7AA-614483684C15") }
				}
			};
			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice))
				.Returns(() => dummyReconciliationResult);

			using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
			{
				var reconciliationResult = InvoiceConverter.ReconcileFromDraftInvoice<T>(draftInvoice, out var validationError);
				AssertNotNull(reconciliationResult);
				Assert(validationError == default);
				AssertEquals(dummyReconciliationResult, reconciliationResult);
			}

			mockReconciliationProcessor.Verify(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>())
				, Times.Exactly(1));
		}

		public void TestPostFromDraftInvoice_CreateCreditorInvoicedExchangeRateProvider()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("Precondition", draftInvoice.Factory.ServiceContainer.GetService<CreditorInvoicedExchangeRateProvider>());

			InvoiceConverter.PostFromDraftInvoice<T>(draftInvoice, null, out var validationError, out var reconciliationError);

			AssertNotNull(draftInvoice.Factory.ServiceContainer.GetService<CreditorInvoicedExchangeRateProvider>());
		}

		public void TestReconcileFromDraftInvoice_ReconcileDraftInvoiceFail()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor.Setup(x => x.Reconcile(draftInvoice))
				.Returns(() => new APReconciliationProcessingResult { Result = APReconciliationResultTypes.Failed, FailureReason = "Dummy Fail Reason" });

			var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(mockReconciliationProcessor.Object))
			using (ObjectFactory.Substitute(mockReconciliationLineConverter.Object))
			{
				var reconciliationResult = InvoiceConverter.ReconcileFromDraftInvoice<T>(draftInvoice, out var validationError);
				AssertNotNull(reconciliationResult);
				Assert(validationError == default);
				CombineAssertions("Reconciliation Fail Reason", () =>
				{
					AssertEquals("Message", "Dummy Fail Reason", reconciliationResult.FailureReason);
					AssertEquals(APReconciliationResultTypes.Failed, reconciliationResult.Result);
					AssertNull(reconciliationResult.ReconciliableAccruals);
				});
			}

			mockReconciliationProcessor.Verify(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>())
				, Times.Exactly(1));
			mockReconciliationLineConverter.Verify(x => x.ImportReconciliationLinesToInvoice(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<APReconciliationLine>>())
				, Times.Exactly(0));
		}

		public void TestReconcileFromDraftInvoice_IncorrectTransactionCompany()
		{
			var nonCurrentCompany = TestObjectCreator.CreateNewCompany("TST", TestObjectCreator.CreditorTR);
			nonCurrentCompany.CompanyName = "Company Name AAAAAAAA";

			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_GC_Company = nonCurrentCompany.PK;
			Factory.Save();

			var reconciliationResult = InvoiceConverter.ReconcileFromDraftInvoice<T>(draftInvoice, out var validationError);
			AssertEquals($@"You are trying to post a draft {ExpectedHumanReadableTransactionType} to company TST - Company Name AAAAAAAA.
You are either not logged into CargoWise, or logged in to a different company.
Please login to company TST - Company Name AAAAAAAA before attempting to post the draft {ExpectedHumanReadableTransactionType}."
				, validationError.Msg);
			AssertEquals($"Draft {ExpectedHumanReadableTransactionType} cannot be posted", validationError.Caption);
			AssertNull(reconciliationResult);
		}

		public void TestReconcileFromDraftInvoice_DraftTransactionHadBeenAllocated()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase();
			draftInvoice.AIH_AH_PostedTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			Factory.Save();

			var reconciliationResult = InvoiceConverter.ReconcileFromDraftInvoice<T>(draftInvoice, out var validationError);
			AssertEquals($"This draft {ExpectedHumanReadableTransactionType} is already either posted as an AP {ExpectedHumanReadableTransactionType}, an AP {ExpectedHumanReadableTransactionType} awaiting approval, or AP {ExpectedHumanReadableTransactionType} has been saved as incomplete."
				, validationError.Msg);
			AssertEquals($"Draft {ExpectedHumanReadableTransactionType} already posted"
				, validationError.Caption);
			AssertNull(reconciliationResult);
		}

		#endregion

		protected AccDraftInvoiceHeader GetBusinessObjectThatIsInTheDatabase()
		{
			var mockStatusUpdater = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mockStatusUpdater.Setup(h => h.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<string>())).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
			ObjectFactory.Substitute(mockStatusUpdater.Object);

			var draftInvoice = Factory.New<AccDraftInvoiceHeader>();
			draftInvoice.AIH_TransactionType = TransactionType;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			draftInvoice.AIH_GB_Branch = TestObjectCreator.NonCurrentBranch.PK;
			draftInvoice.AIH_GE_Department = TestObjectCreator.NonCurrentDepartment.PK;
			draftInvoice.AIH_InternalReference = "00001001";
			draftInvoice.AIH_OH_Creditor = TestObjectCreator.Creditor4.PK;
			draftInvoice.AIH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			draftInvoice.AIH_Description = "AP Desc";
			draftInvoice.AIH_Status = Core.Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting;

			Factory.Save();
			return draftInvoice;
		}

		IDisposable DisableReconciliation()
		{
			var mockReconciliationProcessor = new Mock<IAPReconciliationProcessor>(MockBehavior.Strict);
			mockReconciliationProcessor
				.Setup(x => x.Reconcile(It.IsAny<AccDraftInvoiceHeader>()))
				.Returns(() => new APReconciliationProcessingResult
				{
					Result = APReconciliationResultTypes.Success,
					FailureReason = "Dummy Fail Reason that would not be used",
					ReconciliableAccruals = Array.Empty<APReconciliationLine>()
				});

			var mockReconciliationLineConverter = new Mock<IReconciliationLineConverter>(MockBehavior.Strict);
			mockReconciliationLineConverter.Setup(x => x.ImportReconciliationLinesToInvoice(
				It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<IReconciliationLineIdentifier>>())
			);

			return new DisposableList(new[] {
				ObjectFactory.Substitute(mockReconciliationProcessor.Object),
				ObjectFactory.Substitute(mockReconciliationLineConverter.Object),
			});
		}

		Mock<ITaxProcessor> GetTaxProcessorByTaxFramework(string taxProcessErrorMsg)
		{
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			var mockTaxProcessor = new Mock<ITaxProcessor>(MockBehavior.Strict);
			mockTaxProcessor.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns(taxProcessErrorMsg);
			ObjectFactory.Substitute(mockTaxProcessor.Object);

			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			return mockTaxProcessor;
		}

		protected abstract AccDraftInvoiceHeader CreateValidDraftInvoice();

		protected AccDraftInvoiceHeader GetValidDraftInvoice(decimal osCostAmt, decimal osExTaxAmount, decimal osTaxAmount)
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2024, 3, 1));
			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			Factory.Save();

			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: osCostAmt, creditor: testObjectCreator.Creditor4);

			Factory.Save();

			var draftInvoice = testObjectCreator.CreateDraftWithJob(TransactionType, "TestINV001", testObjectCreator.Creditor4.PK, osExTaxAmount, osTaxAmount, "AUD", shipment);
			draftInvoice.AIH_PostDate = new ZDate(2024, 03, 20);

			return draftInvoice;
		}

		IAPReconciliationPoster InvoiceConverter => invoiceConverter ??= ObjectFactory.Get<IAPReconciliationPoster>();
		IAPReconciliationPoster invoiceConverter;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
