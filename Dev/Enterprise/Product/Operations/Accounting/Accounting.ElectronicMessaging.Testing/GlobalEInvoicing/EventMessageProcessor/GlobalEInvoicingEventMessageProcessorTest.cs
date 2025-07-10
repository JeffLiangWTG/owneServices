using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	public class GlobalEInvoicingEventMessageProcessorTest_AR : GlobalEInvoicingEventMessageProcessorTest
	{
		protected override Type TestInvoiceType =>  typeof(ARInvoice);
	}

	public class GlobalEInvoicingEventMessageProcessorTest_AP : GlobalEInvoicingEventMessageProcessorTest
	{
		protected override Type TestInvoiceType =>  typeof(APInvoice);
	}

	public abstract class GlobalEInvoicingEventMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2021, 1, 26, 10, 46, 05)]
		public void TestIAKMessage()
		{
			AssertIAKMessageBatchStatus(false);
		}

		public void TestIAKMessage_Discarded()
		{
			batch.AIB_Status = EInvoicingBatchState.Discarded;
			Factory.Save();

			AssertIAKMessageBatchStatus(true);
		}

		public void AssertIAKMessageBatchStatus(bool isDiscardedBatch)
		{
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedNumber_Legacy = "72cf24e4-366e-11eb-adc1-0242ac120002",
				Reason = "Some message to inform",
			});

			var eventData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, batch);
			var processor = new GlobalEInvoicingEventMessageProcessor(eventData, countryEInvoicingObjectFactory);

			AssertEquals("Precondition", 0, Logger.Logs.Count());
			AssertEquals("Precondition", 1, batch.TransactionPivots.Count);
			AssertEquals("Precondition", isDiscardedBatch ? EInvoicingBatchState.Discarded : EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			AssertEquals("Postcondition", 0, Logger.Logs.Count());
			AssertEquals("Postcondition", 1, batch.TransactionPivots.Count);
			if (isDiscardedBatch)
			{
				AssertEquals("Postcondition", EInvoicingBatchState.Discarded, batch.AIB_Status);
				AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Postcondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
				AssertEquals("Postcondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			}
			else
			{
				AssertEquals("Postcondition", EInvoicingBatchState.Sent, batch.AIB_Status);
				AssertEquals("Postcondition", "72cf24e4-366e-11eb-adc1-0242ac120002", batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Postcondition", EInvoicingPivotState.Succeed, pivot.AIP_Status);
				AssertEquals("Postcondition", ZDateTime.Now, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition", "Some message to inform", pivot.AIP_ErrorDescription);
			}
		}

		[TestDate(2021, 1, 26, 10, 46, 05)]
		public void TestIAKMessagePivotStatusSucceed()
		{
			batch.AIB_Status = EInvoicingBatchState.Sent;
			batch.AIB_GovernmentAllocatedNumber = "SomePreviousNumber";
			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.Now;
			pivot.AIP_ErrorDescription = ZString.Empty;
			Factory.Save();

			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedNumber_Legacy = "72cf24e4-366e-11eb-adc1-0242ac120002",
				Reason = "Some message to inform",
			});

			var eventData = new EventMessageProcessorData(null, null, Logger, ediMessage, universalEvent, batch);
			var processor = new GlobalEInvoicingEventMessageProcessor(eventData, countryEInvoicingObjectFactory);

			AssertEquals("Precondition", 0, Logger.Logs.Count());
			AssertEquals("Precondition", 1, batch.TransactionPivots.Count);
			AssertEquals("Precondition", EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Precondition", "SomePreviousNumber", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Precondition", ZDateTime.Now, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();
			AssertEquals("Postcondition", 1, Logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Warning && x.Message == $"No update performed due to transaction pivot having 'SUC' status for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."));
			AssertEquals("Postcondition", 1, batch.TransactionPivots.Count);
			AssertEquals("Postcondition", EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Postcondition", "SomePreviousNumber", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Postcondition", ZDateTime.Now, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
		}

		[TestDate(2021, 1, 26, 10, 46, 05)]
		public void TestIRJMessageStatus()
		{
			AssertIRJMessageBatchStatus(false);
		}

		public void TestIRJMessageStatus_Discarded()
		{
			batch.AIB_Status = EInvoicingBatchState.Discarded;
			Factory.Save();

			AssertIRJMessageBatchStatus(true);
		}

		public void AssertIRJMessageBatchStatus(bool isDiscardedBatch)
		{
			var (ediMessage, universalEvent) = GetEDIMessageAndIRJUniversalEvent();
			var eventData = new EventMessageProcessorData(null, null, Logger, ediMessage, universalEvent, batch);
			var processor = new GlobalEInvoicingEventMessageProcessor(eventData, countryEInvoicingObjectFactory);

			AssertEquals("Precondition", 0, Logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("Precondition", 1, batch.TransactionPivots.Count);
			AssertEquals("Precondition", isDiscardedBatch ? EInvoicingBatchState.Discarded : EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			if (isDiscardedBatch)
			{
				AssertEquals("Postcondition", 0, Logger.Logs.Count());
				AssertEquals("Postcondition", ZString.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Postcondition", 1, batch.TransactionPivots.Count);
				AssertEquals("Postcondition", EInvoicingBatchState.Discarded, batch.AIB_Status);
				AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Postcondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
				AssertEquals("Postcondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			}
			else
			{
				AssertEquals("Postcondition", 1, Logger.Logs.Count());
				AssertEquals("Postcondition", emailSendingUnsuccessfulMessage, Logger.Logs.ElementAt(0).Message);
				AssertEquals("Postcondition", 1, batch.TransactionPivots.Count);
				AssertEquals("Postcondition", EInvoicingBatchState.Sent, batch.AIB_Status);
				AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Postcondition", EInvoicingPivotState.Failed, pivot.AIP_Status);
				AssertEquals("Postcondition", ZDateTime.Now, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition", "Failed to send message to government", pivot.AIP_ErrorDescription);
			}
		}

		public void TestEInvoiceBatchHasNoTransactionPivots()
		{
			pivot.Delete();
			Factory.Save();

			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedNumber_Legacy = "AnyOldNumberWillDoHere",
				Reason = "Some message to inform",
			});

			var eventData = new EventMessageProcessorData(null, null, Logger, ediMessage, universalEvent, batch);
			var processor = new GlobalEInvoicingEventMessageProcessor(eventData, countryEInvoicingObjectFactory);

			AssertEquals("Precondition", 0, Logger.Logs.Count());
			AssertEquals("Precondition", 0, batch.TransactionPivots.Count);
			AssertEquals("Precondition", EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_Status);

			processor.Process();

			AssertEquals("Postcondition", "GlobalEInvoicingEventMessageProcessor_PivotNotFound", ErrorReporter.LastKeyReported);
			AssertEquals("Postcondition", 1, Logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "No update performed due to transaction pivot not found for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", 0, batch.TransactionPivots.Count);
			AssertEquals("Postcondition", EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_Status);
			ErrorReporter.Clear();
		}

		#region TestMessage_GovernmentAllocatedNumber

		public void TestIAKMessage_GovernmentAllocatedNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedNumber_Legacy = "36edcb89-672b-45e8-92fe-389dae895336",
				AIB_GovernmentAllocatedNumber_Post = "36edcb89-672b-45e8-92fe-389dae895336",
			});
		}

		public void TestIRJMessage_GovernmentAllocatedNumber()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage,
				AIB_GovernmentAllocatedNumber_Legacy = "36edcb89-672b-45e8-92fe-389dae895336",
				AIB_GovernmentAllocatedNumber_Post = "36edcb89-672b-45e8-92fe-389dae895336",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedBatchNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedBatchNumber = "NewAndShinyBatchNumber",
				AIB_GovernmentAllocatedNumber_Post = "NewAndShinyBatchNumber",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedNumber_AndBatchNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedBatchNumber = "NewAndShinyBatchNumber",
				AIB_GovernmentAllocatedNumber_Legacy = "OldAndCrustyBatchNumber",
				AIB_GovernmentAllocatedNumber_Post = "NewAndShinyBatchNumber",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedTransactionNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_GovernmentAllocatedId = "ARealPerTransactionRefNumber!",
				AH_GovernmentAllocatedId_Post = "ARealPerTransactionRefNumber!",
			});
		}

		public void TestIRJMessage_GovernmentAllocatedTransactionNumber()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage,
				AH_GovernmentAllocatedId = "ARealPerTransactionRefNumber!",
				AH_GovernmentAllocatedId_Post = ZString.Empty,
			});
		}

		public void TestIAKMessage_GovernmentAllocatedTransactionNumber_SameAsExistingValue()
		{
			CurrentTransaction.AH_GovernmentAllocatedID = "APreviousPerTransactionRefNumber";
			Factory.Save();
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_GovernmentAllocatedId_Pre = "APreviousPerTransactionRefNumber",
				AH_GovernmentAllocatedId = "APreviousPerTransactionRefNumber",
				AH_GovernmentAllocatedId_Post = "APreviousPerTransactionRefNumber",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedTransactionNumber_DifferentThanExistingValue()
		{
			CurrentTransaction.AH_GovernmentAllocatedID = "APreviousPerTransactionRefNumber";
			Factory.Save();
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_GovernmentAllocatedId_Pre = "APreviousPerTransactionRefNumber",
				AH_GovernmentAllocatedId = "000ARealPerTransactionRefNumber000",
				AH_GovernmentAllocatedId_Post = "APreviousPerTransactionRefNumber",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Government Allocated ID can't be overridden. Existing value: 'APreviousPerTransactionRefNumber' & New value: '000ARealPerTransactionRefNumber000' for invoice batch {batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {batch.Company.CompanyName}.",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedTransactionNumber_ExistingValueAndEmpty()
		{
			CurrentTransaction.AH_GovernmentAllocatedID = "APreviousPerTransactionRefNumber";
			Factory.Save();
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_GovernmentAllocatedId_Pre = "APreviousPerTransactionRefNumber",
				AH_GovernmentAllocatedId = ZString.Empty,
				AH_GovernmentAllocatedId_Post = "APreviousPerTransactionRefNumber",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedBatchNumber_AndTransactionNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedBatchNumber = "NewAndShinyBatchNumber",
				AIB_GovernmentAllocatedNumber_Post = "NewAndShinyBatchNumber",

				AH_GovernmentAllocatedId = "ARealPerTransactionRefNumber!",
				AH_GovernmentAllocatedId_Post = "ARealPerTransactionRefNumber!",
			});
		}

		public void TestIAKMessage_GovernmentAllocatedNumber_AndTransactionNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedNumber_Legacy = "OldAndCrustyBatchNumber",
				AIB_GovernmentAllocatedNumber_Post = "OldAndCrustyBatchNumber",

				AH_GovernmentAllocatedId = "ARealPerTransactionRefNumber!",
				AH_GovernmentAllocatedId_Post = "ARealPerTransactionRefNumber!",
			});
		}

		#endregion TestMessage_GovernmentAllocatedNumber

		#region TestMessage_EHubAllocatedNumber

		public void TestIAKMessage_EHubAllocatedNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_EHubAllocatedNumber = "6cfb13bd-106f-409c-b986-7f3645490327"
			});
		}

		public void TestIRJMessage_EHubAllocatedNumber()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage,
				AIB_EHubAllocatedNumber = "6cfb13bd-106f-409c-b986-7f3645490327"
			});
		}

		#endregion TestMessage_EHubAllocatedNumber

		#region TestMessage_TransactionReference

		public void TestIAKMessage_TransactionReference()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_TransactionReference_Pre = ZString.Empty,
				AH_TransactionReference = "CN00373948",
				AH_TransactionReference_Post = "CN00373948"
			});
		}

		public void TestIRJMessage_TransactionReference()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage,
				AH_TransactionReference_Pre = ZString.Empty,
				AH_TransactionReference = "CN00373948",
				AH_TransactionReference_Post = ZString.Empty
			});
		}

		public void TestIAKMessage_TransactionReference_SameAsExistingValue()
		{
			CurrentTransaction.AH_TransactionReference = "CN00373948";
			Factory.Save();
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_TransactionReference_Pre = "CN00373948",
				AH_TransactionReference = "CN00373948",
				AH_TransactionReference_Post = "CN00373948"
			});
		}

		public void TestIAKMessage_TransactionReference_DifferentThanExistingValue()
		{
			CurrentTransaction.AH_TransactionReference = "CN11223344";
			Factory.Save();
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_TransactionReference_Pre = "CN11223344",
				AH_TransactionReference = "CN00373948",
				AH_TransactionReference_Post = "CN11223344",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Compliance Number can't be overridden. Existing value: 'CN11223344' & New value: 'CN00373948' for invoice batch {batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {batch.Company.CompanyName}."
			});
		}

		public void TestIAKMessage_TransactionReference_ExistingValueAndEmpty()
		{
			CurrentTransaction.AH_TransactionReference = "CN00373948";
			Factory.Save();
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH_TransactionReference_Pre = "CN00373948",
				AH_TransactionReference = "",
				AH_TransactionReference_Post = "CN00373948"
			});
		}

		#endregion TestMessage_TransactionReference

		#region TestIAKMessage_AuthorisationRecord

		public void TestIAKMessage_AuthorisationRecord_IfAllNullExpectNotExists()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = false,
				AHF_Counter = null,
				AHF_Number = null,
				AHF_DateTime = null,
				AHF_IDType = null,
			});
		}

		public void TestIAKMessage_AuthorisationRecord_IfNullCounterNumberDateTimeIdTypeFields()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = true,
				AHF_Counter = null,
				AHF_Number = null,
				AHF_DateTime = null,
				AHF_IDType = null,
				AHF_VerificationUrl = "https://xyz.com/abc"
			});
		}

		public void TestIAKMessage_AuthorisationRecord_IfInvalidDateFormatSupplied()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-13-08T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_DateTime as '2021-13-08T15:46:05.0000000+00:00' cannot be converted to date and time for invoice batch {batch.AIB_BatchNumber}, transaction {this.InvoiceTitle000}, in {batch.Company.CompanyName}."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_IfRecordAlreadyExists_AndFieldsAreBlankOrPlaceholder()
		{
			var authorizationRecord = batch.Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_RecordType = countryEInvoicingObjectFactory.AuthorizationRecordType;
			authorizationRecord.AHF_ParentId = CurrentTransaction.PK;
			authorizationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authorizationRecord.AHF_IssuerCertificateIdentifier = "20001000000300022824";
			authorizationRecord.AHF_IssuerAuthorizationData = Convert.FromBase64String("HR+ryY2RCvDxOpw0w4BdmbqEq+3Qn6oddkyjSiCQuJ2yCaxiQMJiQru0tmZmgtLvH1ZuGiq+Arn3jk2d4eqmH8NjhezpyMCAR0euiYgx0Og5ksJXXP9lzbaZevxubdgAxHK98kCHhRxsfVO94ZSjjNU21XvXY17PjX3BYyejK+JrOJ0NBBXB4g9XLBdxCP+vY5zlwMCXeJlXKNS/gRm/HMnMnDWOj9T81Bthi35YTaG68VoBib4XcWRjNbVoj8aVx+58g4MYv2ljsfKT0CjG8HizWsFn3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntwa7v6A7O8GnELL+1szLaw==");
			authorizationRecord.AHF_DebtorNumber = "P01";
			authorizationRecord.AHF_PlaceOfIssue = "10200";
			Factory.Save();

			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = true,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_AuthorisationData = "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
				AHF_IDNumber = "APXPU3617F",
				AHF_ITransactionHash = "SVRYTjM5NDgxMjQ5WE0=",
				AHF_PublicKey = "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
				AHF_VerificationUrl = "https://xyz.com/abc",
				AHF_IssuerCertificateIdentifier = "20001000000300022824",
				AHF_IssuerAuthorizationData = "HR+ryY2RCvDxOpw0w4BdmbqEq+3Qn6oddkyjSiCQuJ2yCaxiQMJiQru0tmZmgtLvH1ZuGiq+Arn3jk2d4eqmH8NjhezpyMCAR0euiYgx0Og5ksJXXP9lzbaZevxubdgAxHK98kCHhRxsfVO94ZSjjNU21XvXY17PjX3BYyejK+JrOJ0NBBXB4g9XLBdxCP+vY5zlwMCXeJlXKNS/gRm/HMnMnDWOj9T81Bthi35YTaG68VoBib4XcWRjNbVoj8aVx+58g4MYv2ljsfKT0CjG8HizWsFn3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntwa7v6A7O8GnELL+1szLaw==",
				AHF_DebtorNumber = "P01",
				AHF_PlaceOfIssue = "10200"
			});
		}

		public void TestIAKMessage_AuthorisationRecord_IfRecordAlreadyExists_AndFieldsAreSet()
		{
			var authorizationRecord = batch.Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_Counter = "11/66NS";
			authorizationRecord.AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM";
			authorizationRecord.AHF_DateTime = ZDateTimeOffset.UtcNow;
			authorizationRecord.AHF_IDType = "GVT";
			authorizationRecord.AHF_AuthorisationData = Convert.FromBase64String("RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ==");
			authorizationRecord.AHF_IDNumber = "AOIVD8801";
			authorizationRecord.AHF_ITransactionHash = Convert.FromBase64String("S0pBU0hENDkzODI3NDk4MzJTQUtK");
			authorizationRecord.AHF_PublicKey = Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM=");
			authorizationRecord.AHF_VerificationUrl = "https://server.gov.au/verify/fnh0982i3jnjilojs09";
			authorizationRecord.AHF_RecordType = countryEInvoicingObjectFactory.AuthorizationRecordType;
			authorizationRecord.AHF_ParentId = CurrentTransaction.PK;
			authorizationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authorizationRecord.AHF_IssuerCertificateIdentifier = "20001000000300022824";
			authorizationRecord.AHF_IssuerAuthorizationData = Convert.FromBase64String("HR+ryY2RCvDxOpw0w4BdmbqEq+3Qn6oddkyjSiCQuJ2yCaxiQMJiQru0tmZmgtLvH1ZuGiq+Arn3jk2d4eqmH8NjhezpyMCAR0euiYgx0Og5ksJXXP9lzbaZevxubdgAxHK98kCHhRxsfVO94ZSjjNU21XvXY17PjX3BYyejK+JrOJ0NBBXB4g9XLBdxCP+vY5zlwMCXeJlXKNS/gRm/HMnMnDWOj9T81Bthi35YTaG68VoBib4XcWRjNbVoj8aVx+58g4MYv2ljsfKT0CjG8HizWsFn3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntwa7v6A7O8GnELL+1szLaw==");
			authorizationRecord.AHF_DebtorNumber = "P01";
			authorizationRecord.AHF_PlaceOfIssue = "10200";
			Factory.Save();

			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "1",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "QQQ",
				AHF_AuthorisationData = "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
				AHF_IDNumber = "APXPU3617F",
				AHF_ITransactionHash = "SVRYTjM5NDgxMjQ5WE0=",
				AHF_PublicKey = "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
				AHF_VerificationUrl = "https://xyz.com/abc",
				AHF_IssuerCertificateIdentifier = "200010239822824",
				AHF_IssuerAuthorizationData = "HR+ryY2RCvDxOpw0w4BdmbqEq+3Qn6oddkyjSiCQuJ2yCaxiQMJiQru0tmZmgtLvH1ZuGiq+Arn3jk2d4eqmH8NjhezpyMCAR0euiYgx0OgL+1szLaw==",
				AHF_DebtorNumber = "WWI",
				AHF_PlaceOfIssue = "12233",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Authorization record field(s) EINV_AuthorisationData,EINV_Counter,EINV_DateTime,EINV_IDNumber,EINV_IDType,EINV_ITransactionHash,EINV_Number,EINV_PublicKey,EINV_VerificationURL,EINV_IssuerCertificateIdentifier,EINV_IssuerAuthorisationData,EINV_DebtorNumber,EINV_PlaceOfIssue have already been set for invoice batch {batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {batch.Company.CompanyName}. You may only set AccTransactionHeaderAuthorisationRecord fields once."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_IfRecordAlreadyExists_IsIdempotent()
		{
			var authorizationRecord = batch.Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_Counter = "11/66NS";
			authorizationRecord.AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM";
			authorizationRecord.AHF_DateTime = ZDateTimeOffset.UtcNow;
			authorizationRecord.AHF_IDType = "GVT";
			authorizationRecord.AHF_AuthorisationData = Convert.FromBase64String("RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ==");
			authorizationRecord.AHF_IDNumber = "AOIVD8801";
			authorizationRecord.AHF_ITransactionHash = Convert.FromBase64String("S0pBU0hENDkzODI3NDk4MzJTQUtK");
			authorizationRecord.AHF_PublicKey = Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM=");
			authorizationRecord.AHF_VerificationUrl = "https://server.gov.au/verify/fnh0982i3jnjilojs09";
			authorizationRecord.AHF_IssuerCertificateIdentifier = "20001000000300022824";
			authorizationRecord.AHF_IssuerAuthorizationData = Convert.FromBase64String("HR+ryY2RCvDxOpw0w4BdmbqEq+3Qn6oddkyjSiCQuJ2yCaxiQMJiQru0tmZmgtLvH1ZuGiq+Arn3jk2d4eqmH8NjhezpyMCAR0euiYgx0Og5ksJXXP9lzbaZevxubdgAxHK98kCHhRxsfVO94ZSjjNU21XvXY17PjX3BYyejK+JrOJ0NBBXB4g9XLBdxCP+vY5zlwMCXeJlXKNS/gRm/HMnMnDWOj9T81Bthi35YTaG68VoBib4XcWRjNbVoj8aVx+58g4MYv2ljsfKT0CjG8HizWsFn3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntwa7v6A7O8GnELL+1szLaw==");
			authorizationRecord.AHF_DebtorNumber = "P01";
			authorizationRecord.AHF_PlaceOfIssue = "10200";
			authorizationRecord.AHF_RecordType = countryEInvoicingObjectFactory.AuthorizationRecordType;
			authorizationRecord.AHF_ParentId = CurrentTransaction.PK;
			authorizationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			Factory.Save();

			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = true,
				AHF_Counter = "11/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = GetFormattedDateTimeOffset(authorizationRecord.AHF_DateTime),
				AHF_IDType = "GVT",
				AHF_AuthorisationData = "RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ==",
				AHF_IDNumber = "AOIVD8801",
				AHF_ITransactionHash = "S0pBU0hENDkzODI3NDk4MzJTQUtK",
				AHF_PublicKey = "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM=",
				AHF_VerificationUrl = "https://server.gov.au/verify/fnh0982i3jnjilojs09",
				AHF_IssuerCertificateIdentifier = "20001000000300022824",
				AHF_IssuerAuthorizationData = "HR+ryY2RCvDxOpw0w4BdmbqEq+3Qn6oddkyjSiCQuJ2yCaxiQMJiQru0tmZmgtLvH1ZuGiq+Arn3jk2d4eqmH8NjhezpyMCAR0euiYgx0Og5ksJXXP9lzbaZevxubdgAxHK98kCHhRxsfVO94ZSjjNU21XvXY17PjX3BYyejK+JrOJ0NBBXB4g9XLBdxCP+vY5zlwMCXeJlXKNS/gRm/HMnMnDWOj9T81Bthi35YTaG68VoBib4XcWRjNbVoj8aVx+58g4MYv2ljsfKT0CjG8HizWsFn3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntwa7v6A7O8GnELL+1szLaw==",
				AHF_DebtorNumber = "P01",
				AHF_PlaceOfIssue = "10200"
			});
		}

		public void TestIAKMessage_AuthorisationRecord_WithAllFields()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = true,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_AuthorisationData = "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
				AHF_IDNumber = "APXPU3617F",
				AHF_ITransactionHash = "SVRYTjM5NDgxMjQ5WE0=",
				AHF_PublicKey = "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
				AHF_VerificationUrl = "https://xyz.com/abc"
			});
		}

		#endregion TestIAKMessage_AuthorisationRecord

		#region TestMessage_TransactionNotFound

		public void TestMessage_TransactionNotFound()
		{
			invoice = ObjectCreator.CreateInvoiceWithLine(TestInvoiceType, "00001001", ObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			batch = ObjectCreator.CreateEInvoicingBatch(2, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			pivot = ObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent);

			invoice.Delete();

			AssertMessage(new Settings(commonSettingsIAK)
			{
				InvalidTransactionCheck = true,
				ShouldUpdateValues = false,
				ExpectedLogCount = 1,
				ExpectedValidationError = $"No update performed due to related transaction was not found for pivot for invoice batch {batch.AIB_BatchNumber} in {batch.Company.CompanyName}."
			});
		}

		#endregion

		#region TestMessage_PivotStatus

		public void TestIAKMessage_PivotStatusIfSucceed()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Succeed,
				AIP_Status_Post = EInvoicingPivotState.Succeed
			});
		}

		public void TestIAKMessage_PivotStatusIfNotProvided()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status_Post = EInvoicingPivotState.Succeed
			});
		}

		public void TestIAKMessage_PivotStatusIfDelivered()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Delivered,
				AIP_Status_Post = EInvoicingPivotState.Delivered
			});
		}

		public void TestIAKMessage_PivotStatusIfDiscarded()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Discarded,
				AIP_Status_Post = EInvoicingPivotState.Discarded
			});
		}

		public void TestIAKMessage_PivotStatusIfFailed()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Failed,
				AIP_Status_Post = EInvoicingPivotState.Sent,
				ShouldUpdateValues = false,
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"No update performed as IAK can't have EINV_PivotStatus as 'FAL' status for invoice batch {batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {batch.Company.CompanyName}."
			});
		}

		public void TestIRJMessage_PivotStatusIfFailed()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Failed,
				AIP_Status_Post = EInvoicingPivotState.Failed,
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage
			});
		}

		public void TestIRJMessage_PivotStatusIfNotProvided()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status_Post = EInvoicingPivotState.Failed,
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage
			});
		}

		public void TestIRJMessage_PivotStatusIfDelivered()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Delivered,
				AIP_Status_Post = EInvoicingPivotState.Delivered,
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage
			});
		}

		public void TestIRJMessage_PivotStatusIfDiscarded()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Discarded,
				AIP_Status_Post = EInvoicingPivotState.Discarded,
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage
			});
		}

		public void TestIRJMessage_PivotStatusIfSucceed()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Succeed,
				AIP_Status_Post = EInvoicingPivotState.Sent,
				ShouldUpdateValues = false,
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"No update performed as IRJ can't have EINV_PivotStatus as 'SUC' status for invoice batch {batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {batch.Company.CompanyName}."
			});
		}

		#endregion TestMessage_PivotStatus

		#region TestMessage_WithAllElements

		public void TestIAKMessage_WithAllElements()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AIB_GovernmentAllocatedBatchNumber = "8c0274ef584e403f4761f46825f3ebeaabe2dfd5",
				AIB_GovernmentAllocatedNumber_Legacy = "36edcb89-672b-45e8-92fe-389dae895336",
				AIB_GovernmentAllocatedNumber_Post = "8c0274ef584e403f4761f46825f3ebeaabe2dfd5",
				AIB_EHubAllocatedNumber = "6cfb13bd-106f-409c-b986-7f3645490327",
				AH_GovernmentAllocatedId_Pre = ZString.Empty,
				AH_GovernmentAllocatedId = "ade4502a403918349e",
				AH_GovernmentAllocatedId_Post = "ade4502a403918349e",
				AH_TransactionReference_Pre = ZString.Empty,
				AH_TransactionReference = "CN00373948",
				AH_TransactionReference_Post = "CN00373948",
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Delivered,
				AIP_Status_Post = EInvoicingPivotState.Delivered,
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = true,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_AuthorisationData = "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
				AHF_IDNumber = "APXPU3617F",
				AHF_ITransactionHash = "SVRYTjM5NDgxMjQ5WE0=",
				AHF_PublicKey = "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
				AHF_VerificationUrl = "https://xyz.com/abc"
			});

			Factory.Save();
		}

		public void TestIRJMessage_WithAllElements()
		{
			AssertMessage(new Settings(commonSettingsIRJ)
			{
				ExpectedLogCount = 1,
				ExpectedInformation = emailSendingUnsuccessfulMessage,
				AIB_GovernmentAllocatedBatchNumber = "8c0274ef584e403f4761f46825f3ebeaabe2dfd5",
				AIB_GovernmentAllocatedNumber_Legacy = "36edcb89-672b-45e8-92fe-389dae895336",
				AIB_GovernmentAllocatedNumber_Post = "8c0274ef584e403f4761f46825f3ebeaabe2dfd5",
				AIB_EHubAllocatedNumber = "6cfb13bd-106f-409c-b986-7f3645490327",
				AH_GovernmentAllocatedId_Pre = ZString.Empty,
				AH_GovernmentAllocatedId = "ade4502a403918349e",
				AH_GovernmentAllocatedId_Post = String.Empty,
				AH_TransactionReference_Pre = ZString.Empty,
				AH_TransactionReference = "CN00373948",
				AH_TransactionReference_Post = ZString.Empty,
				AIP_Status_Pre = EInvoicingPivotState.Sent,
				AIP_Status = EInvoicingPivotState.Delivered,
				AIP_Status_Post = EInvoicingPivotState.Delivered
			});

			Factory.Save();
		}

		#endregion TestMessage_WithAllElements

		#region ValidateAuthorisationRecord

		public void TestIAKMessage_AuthorisationRecord_Valid_PublicKey()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_PublicKey = "HR+ryY2RCvDxOpw0w4adsadsad3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntszLaw==",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_PublicKey as it is not a valid Base64 string for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_Valid_AuthorisationData()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_AuthorisationData = "HR+ryY2RCvDxOpw0w4adsadsad3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntszLaw==",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_AuthorisationData as it is not a valid Base64 string for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_Valid_ITransactionHash()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_ITransactionHash = "HR+ryY2RCvDxOpw0w4adsadsad3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntszLaw==",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_ITransactionHash as it is not a valid Base64 string for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_ValidLength_IssuerCertificateIdentifier()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_IssuerCertificateIdentifier = "200010000003000228242000100000030002282420001000000300022824",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_IssuerCertificateIdentifier as its length more than 50: invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_Valid_IssuerAuthorizationData()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_IssuerAuthorizationData = "HR+ryY2RCvDxOpw0w4adsadsad3xaFpUGs0EprOZTVBdLvdRvl2RckRvTHGXhntszLaw==",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_IssuerAuthorisationData as it is not a valid Base64 string for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_ValidLength_DebtorNumber()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_DebtorNumber = "200010000ASFKASFKASJFKAFASKFJASFK20001000000ASJKASDJKDHJDSFHSDFHJSDFKJSDFKJSDFKSDKSDJF00022824",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_DebtorNumber as its length more than 50: invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		public void TestIAKMessage_AuthorisationRecord_ValidLength_PlaceOfIssue()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				IsAuthorisationCheckRequired = true,
				ShouldUpdateAuthorisationRecord = false,
				AHF_Counter = "62/66NS",
				AHF_Number = "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				AHF_DateTime = "2021-02-18T15:46:05.0000000+00:00",
				AHF_IDType = "GVT",
				AHF_PlaceOfIssue = "1023232HASDJASDJASDJADJASJDASJDJASDJASD36767123200",
				ExpectedLogCount = 1,
				ExpectedValidationWarning = $"Failed to set EINV_PlaceOfIssue as its length more than 10: invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International."
			});
		}

		#endregion

		#region TestIAKMessage_ComplianceSubTypeAndComplianceDocumentStatus

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessage_ComplianceSubTypeAndComplianceDocumentStatus()
		{
			AssertMessage(new Settings(commonSettingsIAK)
			{
				AH1_ComplianceDocumentStatus = "CDD",
				AH_ComplianceSubType = "TXA"
			});
		}

		#endregion

		#region Helper Methods

		void AssertMessage(Settings settings)
		{
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(settings);
			var eventData = new EventMessageProcessorData(null, null, Logger, ediMessage, universalEvent, batch);
			var processor = new GlobalEInvoicingEventMessageProcessor(eventData, countryEInvoicingObjectFactory);

			AssertEquals("Precondition:LogCount", 0, Logger.Logs.Count());
			AssertEquals("Precondition:ValidationError", ZString.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Precondition:AIB_Status", EInvoicingBatchState.Sent, batch.AIB_Status);

			AssertEquals("Precondition:AIP_LastResponseReceivedUtc", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition:AIP_Status", settings.AIP_Status_Pre, pivot.AIP_Status);
			AssertEquals("Precondition:AIP_ErrorDescription", ZString.Empty, pivot.AIP_ErrorDescription);

			AssertEquals("Precondition:AIB_GovernmentAllocatedNumber", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition:AIB_EHubAllocatedNumber", ZString.Empty, batch.AIB_EHubAllocatedNumber);

			if (!settings.InvalidTransactionCheck)
			{
				AssertEquals("Precondition:AH_GovernmentAllocatedId", settings.AH_GovernmentAllocatedId_Pre, CurrentTransaction.AH_GovernmentAllocatedID);
				AssertEquals("Precondition:AH_TransactionReference", settings.AH_TransactionReference_Pre, CurrentTransaction.AH_TransactionReference);
				AssertEquals("Precondition:AH_ComplianceSubType", ZString.Empty, CurrentTransaction.AH_ComplianceSubType);
				AssertEquals("Precondition:AH1_ComplianceDocumentStatus", ZString.Empty, CurrentTransaction.ComplianceDocumentStatus);
			}

			var authRecordBefore = GetAuthorisationRecord(CurrentTransaction);

			processor.Process();

			AssertEquals("Postcondition:LogCount", settings.ExpectedLogCount, Logger.Logs.Count());
			AssertEquals("Postcondition:ValidationError", settings.ExpectedValidationError, ErrorReporter.LastMessageReported);

			if (!settings.ExpectedValidationWarning.IsEmpty)
			{
				AssertEquals("Postcondition:ValidationWarning", settings.ExpectedValidationWarning, Logger.Logs.First(l => l.Type == Enterprise.Integration.LogType.Warning).Message);
			}

			if (!settings.ExpectedInformation.IsEmpty)
			{
				AssertEquals("Postcondition:Information", settings.ExpectedInformation, Logger.Logs.First(l => l.Type == Enterprise.Integration.LogType.Information).Message);
			}

			if (settings.ShouldUpdateValues)
			{
				AssertEquals("Postcondition:AIP_LastResponseReceivedUtc", GetFormattedDateTime(settings.EventTime), GetFormattedDateTime(pivot.AIP_LastResponseReceivedUtc));
				AssertEquals("Postcondition:AIP_Status", settings.AIP_Status_Post, pivot.AIP_Status);
				AssertEquals("Postcondition:AIP_ErrorDescription", settings.Reason, pivot.AIP_ErrorDescription);

				AssertEquals("Postcondition:AIB_GovernmentAllocatedNumber", settings.AIB_GovernmentAllocatedNumber_Post, batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Postcondition:AIB_EHubAllocatedNumber", settings.AIB_EHubAllocatedNumber, batch.AIB_EHubAllocatedNumber);
				AssertEquals("Postcondition:AH_GovernmentAllocatedId", settings.AH_GovernmentAllocatedId_Post, CurrentTransaction.AH_GovernmentAllocatedID);
				AssertEquals("Postcondition:AH_TransactionReference", settings.AH_TransactionReference_Post, CurrentTransaction.AH_TransactionReference);
				AssertEquals("Postcondition:AH_ComplianceSubType", settings.AH_ComplianceSubType, CurrentTransaction.AH_ComplianceSubType);
				AssertEquals("Postcondition:AH1_ComplianceDocumentStatus", settings.AH1_ComplianceDocumentStatus, CurrentTransaction.ComplianceDocumentStatus);

				if (settings.IsAuthorisationCheckRequired)
				{
					var authorisationRecord = GetAuthorisationRecord(CurrentTransaction);
					if (settings.ShouldUpdateAuthorisationRecord)
					{
						AssertNotNull("Postcondition:AuthorisationRecord", authorisationRecord);
						AssertEquals("Postcondition:AHF_AuthorisationData", GetFromBase64String(settings.AHF_AuthorisationData), authorisationRecord.AHF_AuthorisationData.ToUTF8());
						AssertEquals("Postcondition:AHF_Counter", settings.AHF_Counter, authorisationRecord.AHF_Counter);
						AssertEquals("Postcondition:AHF_DateTime", settings.AHF_DateTime, GetFormattedDateTimeOffset(authorisationRecord.AHF_DateTime));
						AssertEquals("Postcondition:AHF_IDNumber", settings.AHF_IDNumber, authorisationRecord.AHF_IDNumber);
						AssertEquals("Postcondition:AHF_IDType", settings.AHF_IDType, authorisationRecord.AHF_IDType);
						AssertEquals("Postcondition:AHF_ITransactionHash", GetFromBase64String(settings.AHF_ITransactionHash), authorisationRecord.AHF_ITransactionHash.ToUTF8());
						AssertEquals("Postcondition:AHF_Number", settings.AHF_Number, authorisationRecord.AHF_Number);
						AssertEquals("Postcondition:AHF_PublicKey", GetFromBase64String(settings.AHF_PublicKey), authorisationRecord.AHF_PublicKey.ToUTF8());
						AssertEquals("Postcondition:AHF_VerificationUrl", settings.AHF_VerificationUrl, authorisationRecord.AHF_VerificationUrl);
						AssertEquals("Postcondition:AHF_IssuerCertificateIdentifier", settings.AHF_IssuerCertificateIdentifier, authorisationRecord.AHF_IssuerCertificateIdentifier);
						AssertEquals("Postcondition:AHF_IssuerAuthorizationData", GetFromBase64String(settings.AHF_IssuerAuthorizationData), authorisationRecord.AHF_IssuerAuthorizationData.ToUTF8());
						AssertEquals("Postcondition:AHF_DebtorNumber", settings.AHF_DebtorNumber, authorisationRecord.AHF_DebtorNumber);
						AssertEquals("Postcondition:AHF_PlaceOfIssue", settings.AHF_PlaceOfIssue, authorisationRecord.AHF_PlaceOfIssue);
					}
					else
					{
						if (authorisationRecord == null)
						{
							AssertNull("Postcondition:AuthorisationRecord", authRecordBefore);
						}
						else
						{
							AssertEquals("Postcondition:AHF_AuthorisationData", authRecordBefore.AHF_AuthorisationData.ToUTF8(), authorisationRecord.AHF_AuthorisationData.ToUTF8());
							AssertEquals("Postcondition:AHF_Counter", authRecordBefore.AHF_Counter, authorisationRecord.AHF_Counter);
							AssertEquals("Postcondition:AHF_DateTime", GetFormattedDateTimeOffset(authRecordBefore.AHF_DateTime), GetFormattedDateTimeOffset(authorisationRecord.AHF_DateTime));
							AssertEquals("Postcondition:AHF_IDNumber", authRecordBefore.AHF_IDNumber, authorisationRecord.AHF_IDNumber);
							AssertEquals("Postcondition:AHF_IDType", authRecordBefore.AHF_IDType, authorisationRecord.AHF_IDType);
							AssertEquals("Postcondition:AHF_ITransactionHash", authRecordBefore.AHF_ITransactionHash.ToUTF8(), authorisationRecord.AHF_ITransactionHash.ToUTF8());
							AssertEquals("Postcondition:AHF_Number", authRecordBefore.AHF_Number, authorisationRecord.AHF_Number);
							AssertEquals("Postcondition:AHF_PublicKey", authRecordBefore.AHF_PublicKey.ToUTF8(), authorisationRecord.AHF_PublicKey.ToUTF8());
							AssertEquals("Postcondition:AHF_VerificationUrl", authRecordBefore.AHF_VerificationUrl, authorisationRecord.AHF_VerificationUrl);
							AssertEquals("Postcondition:AHF_IssuerCertificateIdentifier", authRecordBefore.AHF_IssuerCertificateIdentifier, authorisationRecord.AHF_IssuerCertificateIdentifier);
							AssertEquals("Postcondition:AHF_IssuerAuthorizationData", authRecordBefore.AHF_IssuerAuthorizationData.ToUTF8(), authorisationRecord.AHF_IssuerAuthorizationData.ToUTF8());
							AssertEquals("Postcondition:AHF_DebtorNumber", authRecordBefore.AHF_DebtorNumber, authorisationRecord.AHF_DebtorNumber);
							AssertEquals("Postcondition:AHF_PlaceOfIssue", authRecordBefore.AHF_PlaceOfIssue, authorisationRecord.AHF_PlaceOfIssue);
						}
					}
				}
			}
			else
			{
				AssertEquals("Postcondition:AIP_LastResponseReceivedUtc", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition:AIP_Status", settings.AIP_Status_Pre, pivot.AIP_Status);
				AssertEquals("Postcondition:AIP_ErrorDescription", settings.Reason, pivot.AIP_ErrorDescription);

				AssertEquals("Postcondition:AIB_GovernmentAllocatedNumber", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Postcondition:AIB_EHubAllocatedNumber", ZString.Empty, batch.AIB_EHubAllocatedNumber);

				if (!settings.InvalidTransactionCheck)
				{
					AssertEquals("Postcondition:AH_GovernmentAllocatedId", settings.AH_GovernmentAllocatedId_Pre, CurrentTransaction.AH_GovernmentAllocatedID);
					AssertEquals("Postcondition:AH_TransactionReference", settings.AH_TransactionReference_Pre, CurrentTransaction.AH_TransactionReference);
					AssertEquals("Postcondition:AH_ComplianceSubType", settings.AH_ComplianceSubType, CurrentTransaction.AH_ComplianceSubType);
					AssertEquals("Postcondition:AH1_ComplianceDocumentStatus", settings.AH1_ComplianceDocumentStatus, CurrentTransaction.ComplianceDocumentStatus);
				}
			}

			ErrorReporter.Clear();
		}

		[TestDate(2021, 1, 26, 10, 46, 05)]
		(EDIMessage ediMessage, UniversalEvent universalEvent) GetEDIMessageAndIRJUniversalEvent()
		{
			var universalEventText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{batch.AIB_BatchNumber}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>{ZDateTime.Now}</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>MX</MessageType>
			<MessageSubType/>
			<Reason>Failed to send message to government</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>{GlbCompany.CurrentCompany.GC_Code}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var (ediMessage, universalEvent) = GetEDIMessage(universalEventText);

			return (ediMessage, universalEvent);
		}

		(EDIMessage ediMessage, UniversalEvent universalEvent) GetEDIMessageAndUniversalEvent(Settings settings)
		{
#pragma warning disable CS0618 // Type or member is obsolete: backwards compatibility of Legacy_GovernmentAllocatedNumber / EINV_GovtAllocatedRefNumber
			var universalEventText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{settings.AIB_BatchNumber}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>{GetFormattedDateTime(settings.EventTime)}</EventTime>
		<EventType>{settings.EventType}</EventType>
		<EventParameters>
			<MessageType>{settings.MessageType}</MessageType>
			<MessageSubType>{settings.MessageSubType}</MessageSubType>{GetReasonIfNeeded(settings.Reason)}
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>{settings.CompanyCode}</Value>
			</Context>
{GetContextNodeIfNeeded(EventContextTypeCode.AIB_GovernmentAllocatedNumber_Explicit, settings.AIB_GovernmentAllocatedBatchNumber) +
GetContextNodeIfNeeded(EventContextTypeCode.Legacy_GovernmentAllocatedNumber, settings.AIB_GovernmentAllocatedNumber_Legacy) +
GetContextNodeIfNeeded(EventContextTypeCode.AIB_EHubAllocatedNumber, settings.AIB_EHubAllocatedNumber) +
GetContextNodeIfNeeded(EventContextTypeCode.AH_GovernmentAllocatedID, settings.AH_GovernmentAllocatedId) +
GetContextNodeIfNeeded(EventContextTypeCode.AH_TransactionReference, settings.AH_TransactionReference) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_AuthorisationData, settings.AHF_AuthorisationData) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_Counter, settings.AHF_Counter) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_DateTime, settings.AHF_DateTime) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_IDNumber, settings.AHF_IDNumber) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_IDType, settings.AHF_IDType) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_ITransactionHash, settings.AHF_ITransactionHash) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_Number, settings.AHF_Number) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_PublicKey, settings.AHF_PublicKey) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_VerificationUrl, settings.AHF_VerificationUrl) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_IssuerCertificateIdentifier, settings.AHF_IssuerCertificateIdentifier) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_IssuerAuthorizationData, settings.AHF_IssuerAuthorizationData) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_DebtorNumber, settings.AHF_DebtorNumber) +
GetContextNodeIfNeeded(EventContextTypeCode.AHF_PlaceOfIssue, settings.AHF_PlaceOfIssue) +
GetContextNodeIfNeeded(EventContextTypeCode.AIP_Status, settings.AIP_Status) +
GetContextNodeIfNeeded(EventContextTypeCode.AH_ComplianceSubType, settings.AH_ComplianceSubType) +
GetContextNodeIfNeeded(EventContextTypeCode.AH1_ComplianceDocumentStatus, settings.AH1_ComplianceDocumentStatus)}
		</ContextCollection>
	</Event>
</UniversalEvent>";
#pragma warning restore CS0618 // Type or member is obsolete
			var (ediMessage, universalEvent) = GetEDIMessage(universalEventText);

			return (ediMessage, universalEvent);

			string GetContextNodeIfNeeded(string type, ZString value)
			{
				if (!value.IsEmpty)
				{
					return $@"
			<Context>
				<Type>{type}</Type>
				<Value>{value}</Value>
			</Context>";
				}
				return string.Empty;
			}

			string GetReasonIfNeeded(ZString value)
			{
				if (!value.IsEmpty)
				{
					return $@"
			<Reason>{value}</Reason>";
				}
				return string.Empty;
			}
		}

		string GetFormattedDateTime(ZDateTime zDateTime)
		{
			return zDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
		}

		string GetFormattedDateTimeOffset(ZDateTimeOffset zDateTimeOffset)
		{
			return zDateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
		}

		ZString GetFromBase64String(ZString str)
		{
			if (str.IsEmpty)
			{
				return ZString.Empty;
			}
			return Encoding.UTF8.GetString(Convert.FromBase64String(str));
		}

		InvoicingBase CurrentTransaction => batch.Factory.Load<InvoicingBase>(pivot.AIP_ParentID);

		AccTransactionHeaderAuthorisationRecord GetAuthorisationRecord(InvoicingBase transaction)
		{
			if (transaction == null)
			{
				return null;
			}

			var filter = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, SQLComparisonOperator.Equal, transaction.PK);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, SQLComparisonOperator.Equal, countryEInvoicingObjectFactory.AuthorizationRecordType);
			return batch.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(filter);
		}

		(EDIMessage ediMessage, UniversalEvent universalEvent) GetEDIMessage(string universalEventText)
		{
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_MessageText = universalEventText;
			var universalEvent = ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();

			return (ediMessage, universalEvent);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			ObjectCreator = new TestObjectCreator(Factory);
			invoice = ObjectCreator.CreateInvoiceWithLine(TestInvoiceType, "00001000", ObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			batch = ObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			pivot = ObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent);
			countryEInvoicingObjectFactory = new DummyCountryEInvoicingObjectFactory();

			commonSettingsIAK = new CommonSettings()
			{
				AIB_BatchNumber = batch.AIB_BatchNumber,
				EventTime = ZDateTime.UtcNow,
				EventType = "IAK",
				MessageType = "IN",
				MessageSubType = "GEN",
				CompanyCode = GlbCompany.CurrentCompany.GC_Code
			};

			commonSettingsIRJ = new CommonSettings()
			{
				AIB_BatchNumber = batch.AIB_BatchNumber,
				EventTime = ZDateTime.UtcNow,
				EventType = "IRJ",
				MessageType = "IN",
				MessageSubType = "GEN",
				CompanyCode = GlbCompany.CurrentCompany.GC_Code
			};

			Factory.Save();
		}

		string emailSendingUnsuccessfulMessage => $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Transaction {InvoiceTitle000} [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
		IXmlSessionTracker Logger;
		TestObjectCreator ObjectCreator;
		InvoicingBase invoice;
		AccEInvoicingBatch batch;
		AccEInvoicingTransactionPivot pivot;
		ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory;
		CommonSettings commonSettingsIAK;
		CommonSettings commonSettingsIRJ;

		string InvoiceTitle000 => $"{InvoiceTypeName} INV 00001000";
		string InvoiceTypeName => TestInvoiceType.Name.Substring(0, 2);
		protected abstract Type TestInvoiceType { get; }

		class CommonSettings
		{
			public ZInt AIB_BatchNumber { get; set; }
			public ZDateTime EventTime { get; set; }
			public ZString EventType { get; set; }
			public ZString MessageType { get; set; }
			public ZString MessageSubType { get; set; }
			public ZString Reason { get; set; }
			public ZString CompanyCode { get; set; }
		}

		class Settings : CommonSettings
		{
			public Settings(CommonSettings commonSettings)
			{
				AIB_BatchNumber = commonSettings.AIB_BatchNumber;
				EventTime = commonSettings.EventTime;
				EventType = commonSettings.EventType;
				MessageType = commonSettings.MessageType;
				MessageSubType = commonSettings.MessageSubType;
				CompanyCode = commonSettings.CompanyCode;

				if (EventType == "IAK")
				{
					AIP_Status_Post = EInvoicingPivotState.Succeed;
				}
				else if (EventType == "IRJ")
				{
					AIP_Status_Post = EInvoicingPivotState.Failed;
				}
			}
			public int ExpectedLogCount { get; set; }
			public bool ShouldUpdateValues { get; set; } = true;
			public ZString ExpectedValidationError { get; set; }
			public ZString ExpectedValidationWarning { get; set; }
			public ZString ExpectedInformation { get; set; }

			public ZString AIB_GovernmentAllocatedNumber_Legacy { get; set; }
			public ZString AIB_GovernmentAllocatedBatchNumber { get; set; }
			public ZString AIB_GovernmentAllocatedNumber_Post { get; set; }
			public ZString AIB_EHubAllocatedNumber { get; set; }

			public bool InvalidTransactionCheck { get; set; }
			public ZString AH_TransactionReference_Pre { get; set; }
			public ZString AH_TransactionReference { get; set; }
			public ZString AH_TransactionReference_Post { get; set; }

			public ZString AH_GovernmentAllocatedId_Pre { get; set; }
			public ZString AH_GovernmentAllocatedId { get; set; }
			public ZString AH_GovernmentAllocatedId_Post { get; set; }

			public bool IsAuthorisationCheckRequired { get; set; }
			public bool ShouldUpdateAuthorisationRecord { get; set; }
			public ZString AHF_AuthorisationData { get; set; }
			public ZString AHF_Counter { get; set; }
			public ZString AHF_DateTime { get; set; }
			public ZString AHF_IDNumber { get; set; }
			public ZString AHF_IDType { get; set; }
			public ZString AHF_ITransactionHash { get; set; }
			public ZString AHF_Number { get; set; }
			public ZString AHF_PublicKey { get; set; }
			public ZString AHF_VerificationUrl { get; set; }
			public ZString AHF_IssuerCertificateIdentifier { get; set; }
			public ZString AHF_IssuerAuthorizationData { get; set; }
			public ZString AHF_DebtorNumber { get; set; }
			public ZString AHF_PlaceOfIssue { get; set; }
			public ZString AIP_Status_Pre { get; set; } = EInvoicingPivotState.Sent;
			public ZString AIP_Status { get; set; }
			public ZString AIP_Status_Post { get; set; }
			public ZString AH_ComplianceSubType { get; set; }
			public ZString AH1_ComplianceDocumentStatus { get; set; }
		}

		class DummyCountryEInvoicingObjectFactory : CountryEInvoicingObjectFactory
		{
			protected override ZString CountryCode => Core.Constants.CountryCodes.India;

			protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
				=> new DummyUniversalEventFunctionalityProvider(this);
		}

		class DummyUniversalEventFunctionalityProvider : GlobalXUEFunctionalityProvider
		{
			public DummyUniversalEventFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
				: base(countryEInvoicingObjectFactory)
			{
			}

			protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
				=> (eventType == "IAK" && (messageSubType == "GEN")) || eventType == "IRJ";

			protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			{
				if (IsBatchEventMessageProcessSupported(eventMessageProcessorData.EventType, eventMessageProcessorData.MessageSubType))
				{
					return new GlobalEInvoicingEventMessageProcessor(eventMessageProcessorData, base.CountryEInvoicingObjectFactory);
				}

				return null;
			}
		}

		#endregion Helper Methods
	}
}
