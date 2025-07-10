using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.LifeCycleManagement;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class AccDraftInvoiceStatusChangeValidatorTest : TestCaseWithFactory
	{
		public void TestCanSetToAnalysingState()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");

			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;
			var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Analyzing);
			Assert(result.CanUpdate);
			AssertNull(result.ValidationErrors);

			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except([AccDraftInvoiceHeaderStatus.Draft, AccDraftInvoiceHeaderStatus.Analyzing]))
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Analyzing);
				Assert(!result.CanUpdate);
				AssertEquals(FormattableString.Invariant($"Cannot set to an invoice to analyzing state as current status is {statusCode}"), result.ValidationErrors[0]);
			}
		}

		public void TestCanMarkAsDraft()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");

			mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Analyzing)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Analyzing;

			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;
			var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Draft);
			Assert(result.CanUpdate);
			AssertNull(result.ValidationErrors);

			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except([AccDraftInvoiceHeaderStatus.Analyzing, AccDraftInvoiceHeaderStatus.Draft]))
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Draft);
				Assert(!result.CanUpdate);
				AssertEquals(FormattableString.Invariant($"Cannot mark an invoice as draft when current status is {statusCode}"), result.ValidationErrors[0]);
			}
		}

		public void TestCanApproveForPosting()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);
			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");

			string[] statusThatAllowTransition = [AccDraftInvoiceHeaderStatus.Draft, AccDraftInvoiceHeaderStatus.InDispute, AccDraftInvoiceHeaderStatus.AwaitingApproval, AccDraftInvoiceHeaderStatus.Processed];

			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except(statusThatAllowTransition))
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.ApprovedForPosting);
				Assert(!result.CanUpdate);
				AssertEquals("Cannot approve a draft invoice for posting if reconciliation has not been completed yet", result.ValidationErrors[0]);
				AssertEquals(FormattableString.Invariant($"Cannot approve a draft invoice for posting when current status is {statusCode}"), result.ValidationErrors[1]);
			}

			foreach (var statusCode in statusThatAllowTransition)
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;
				draftInvoice.HasReconciliationRun = true;

				var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.ApprovedForPosting);
				Assert(result.CanUpdate);
				AssertNull(result.ValidationErrors);
			}
		}

		public void TestCanApproveForPostingWhenUnPosted()
		{
			var mockStatusValidator = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mockStatusValidator
				.Setup(v => v.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(),
					AccDraftInvoiceHeaderStatus.Processed))
				.Returns(new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = true });
			mockStatusValidator
				.Setup(v => v.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(),
					AccDraftInvoiceHeaderStatus.ApprovedForPosting))
				.Returns(new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = false });
			ObjectFactory.Substitute(mockStatusValidator.Object);

			var objectCreator = new TestObjectCreator(Factory);
			var invoice = objectCreator.CreateAPInvoice<APInvoice>("AP001", objectCreator.AUD, 1.0M, 200M, 0M, 0M, 200M, 0M, 0M);
			Factory.Save();

			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");
			draftInvoice.AIH_AH_PostedTransactionHeader = invoice.PK;
			AssertEquals("Pre-condition: The draft invoice is marked as processed.",
				AccDraftInvoiceHeaderStatus.Processed, draftInvoice.AIH_Status);

			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;
			var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.ApprovedForPosting);
			Assert(!result.CanUpdate);
			AssertEquals("Cannot approve a draft invoice for posting if it is already posted.", result.ValidationErrors[0]);

			draftInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;

			result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.ApprovedForPosting);
			Assert(result.CanUpdate);
			AssertNull(result.ValidationErrors);
		}

		public void TestCanSendForApproval()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);
			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");

			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except([AccDraftInvoiceHeaderStatus.Draft]))
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				var res = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.AwaitingApproval);
				Assert(!res.CanUpdate);
				AssertEquals(FormattableString.Invariant($"Cannot send a draft invoice for approval when current status is {statusCode}"), res.ValidationErrors[0]);
			}

			mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Draft)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Draft;

			var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.AwaitingApproval);
			Assert(result.CanUpdate);
			AssertNull(result.ValidationErrors);
		}

		public void TestCanDiscard()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);
			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");

			string[] statusesThatDoNotAllowTransition = [AccDraftInvoiceHeaderStatus.ApprovedForPosting, AccDraftInvoiceHeaderStatus.Processed];

			foreach (var statusCode in statusesThatDoNotAllowTransition)
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Discarded);
				Assert(!result.CanUpdate);
				AssertEquals(FormattableString.Invariant($"Cannot discard a draft invoice when current status is {statusCode}"), result.ValidationErrors[0]);
			}

			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except(statusesThatDoNotAllowTransition))
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Discarded);
				Assert(result.CanUpdate);
				AssertNull(result.ValidationErrors);
			}
		}

		public void TestCanMarkAsInDispute()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);
			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");

			string[] statusesThatDoNotAllowTransition = [AccDraftInvoiceHeaderStatus.ApprovedForPosting, AccDraftInvoiceHeaderStatus.Processed];

			foreach (var statusCode in statusesThatDoNotAllowTransition)
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.InDispute);
				Assert(!result.CanUpdate);
				AssertEquals(FormattableString.Invariant($"Cannot mark a draft invoice as disputed when current status is {statusCode}"), result.ValidationErrors[0]);
			}

			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except(statusesThatDoNotAllowTransition))
			{
				mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, statusCode)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
				draftInvoice.AIH_Status = statusCode;

				var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.InDispute);
				Assert(result.CanUpdate);
				AssertNull(result.ValidationErrors);
			}
		}

		public void TestCanMarkAsProcessed()
		{
			var mockStatusValidator = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mockStatusValidator
				.Setup(v => v.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(),
					AccDraftInvoiceHeaderStatus.Processed))
				.Returns(new AccDraftInvoiceStatusUpdateValidationResult() { CanUpdate = false });
			ObjectFactory.Substitute(mockStatusValidator.Object);

			var objectCreator = new TestObjectCreator(Factory);
			var invoice = objectCreator.CreateAPInvoice<APInvoice>("AP001", objectCreator.AUD, 1.0M, 200M, 0M, 0M, 200M, 0M, 0M);
			Factory.Save();

			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");
			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;
			var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Processed);
			Assert(!result.CanUpdate);
			AssertEquals("Cannot mark a draft invoice as processed if it is not posted yet.", result.ValidationErrors[0]);

			draftInvoice.AIH_AH_PostedTransactionHeader = invoice.PK;

			result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.Processed);
			Assert(result.CanUpdate);
			AssertNull(result.ValidationErrors);
		}

		public void TestCanSendToReviewStateDueToFailureToPost()
		{
			var mockHandler = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			ObjectFactory.Substitute(mockHandler.Object);
			var handler = new AccDraftInvoiceStatusChangeValidator() as IAccDraftInvoiceStatusChangeValidator;

			var objectCreator = new TestObjectCreator(Factory);
			var draftInvoice = objectCreator.CreateDraftInvoice("INV001", "INTR001", objectCreator.Creditor1.PK, 230M, 0M, "AUD");
			foreach (var statusCode in typeof(AccDraftInvoiceHeaderStatus).GetConstantValues().Except([AccDraftInvoiceHeaderStatus.ApprovedForPosting, AccDraftInvoiceHeaderStatus.InReview]))
			{
				var res = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.InReview);
				Assert(!res.CanUpdate);
				AssertEquals("Cannot send a draft invoice for review until reconciliation is complete.", res.ValidationErrors[0]);
			}

			mockHandler.Setup(h => h.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.ApprovedForPosting)).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.ApprovedForPosting;

			var result = handler.CanChangeStatusTo(draftInvoice, AccDraftInvoiceHeaderStatus.InReview);
			Assert(result.CanUpdate);
			AssertNull(result.ValidationErrors);
		}
	}
}
