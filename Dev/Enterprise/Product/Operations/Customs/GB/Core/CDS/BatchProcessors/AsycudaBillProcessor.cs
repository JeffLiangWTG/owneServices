using System;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.GB.CDS.CDSResponse.ResponseFunction;

namespace Enterprise.Customs.GB.CDS
{
	static class AsycudaBillProcessor
	{
		public static void ProcessAsycudaBill(BusinessObjectFactory factory, AsycudaBill bill, CDSResponseEDIMessage cdsEDIMessage, LoggingInformation logger)
		{
			using (DisposableEnvironment.ForBranch(((IMessageAttachee)bill).Branch.PK.ToGuid()))
			{
				var messageDataObject = cdsEDIMessage.MessageDataObject;
				var responseParameters = new ResponseParameters();
				var issueDateTime = cdsEDIMessage.MessageDataObject.IssueDateTime?.Item?.ToZDateTime() ?? ZDateTime.Empty;
				if (issueDateTime.IsValid)
				{
					responseParameters.EventDateTime = issueDateTime.UtcToDateTimeOffset();
				}

				var outgoingMessage = bill.GetLastOutgoingMessage(cdsEDIMessage.EM_ApplicationReference);
				var responseFunction = cdsEDIMessage.ResponseFunction;
				cdsEDIMessage.EM_MessageSubType = responseFunction?.ThreeCharFunctionCode ?? cdsEDIMessage.EM_MessageSubType;
				UpdateEntryNumberIfNeeded(factory, bill, responseFunction, messageDataObject.GetMovementReferenceNumber, messageDataObject.GetIssueDate, cdsEDIMessage.EM_MessageNum, logger);
				UpdateEntryStatusIfNeeded(factory, bill, responseFunction, cdsEDIMessage, outgoingMessage, logger);
				AttachMessage(bill, cdsEDIMessage);

				if (outgoingMessage != null)
				{
					UpdateStatus(bill, responseFunction, outgoingMessage, cdsEDIMessage);
				}

				AddEntryStatusLog(bill, responseFunction, messageDataObject, responseParameters);
			}
		}

		static void UpdateEntryNumberIfNeeded(BusinessObjectFactory factory, AsycudaBill bill, ResponseFunction responseFunction, Func<ZString> movementReferenceNumberGetter, Func<ZDateTime> issueDateGetter, ZString messageNumber, LoggingInformation logger)
		{
			if (responseFunction.ShouldUpdateEntryNumber(factory, ((IMessageAttachee)bill).DataGroupingCode ?? ZString.Empty))
			{
				StoreMovementReferenceNumberIfNeeded(bill, movementReferenceNumberGetter?.Invoke() ?? ZString.Empty, issueDateGetter?.Invoke() ?? ZDateTime.Empty, responseFunction, messageNumber, logger);
			}
		}

		static void StoreMovementReferenceNumberIfNeeded(AsycudaBill bill, ZString movementReferenceNumber, ZDateTime issueDate, ResponseFunction responseFunction, ZString messageNumber, LoggingInformation logger)
		{
			if (!movementReferenceNumber.IsEmpty && !issueDate.IsEmpty)
			{
				var mrnEntryNumber = CusEntryNumber.Load(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				if (mrnEntryNumber == null)
				{
					mrnEntryNumber = CusEntryNumber.New(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					mrnEntryNumber.CE_EntryNum = movementReferenceNumber;
					mrnEntryNumber.CE_IssueDate = issueDate;
				}
				else if (movementReferenceNumber != mrnEntryNumber.CE_EntryNum)
				{
					logger.Log(string.Format(CultureInfo.CurrentCulture, "We're about to change the MRN from {0} to {1} while processing message number {2}.", mrnEntryNumber.CE_EntryNum, movementReferenceNumber, messageNumber));
					mrnEntryNumber.CE_EntryNum = movementReferenceNumber;
					mrnEntryNumber.CE_IssueDate = issueDate;
				}
			}
		}

		static void UpdateEntryStatusIfNeeded(BusinessObjectFactory factory, AsycudaBill bill, ResponseFunction responseFunction, CDSResponseEDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			var orginalEntryStatus = bill.ABL_BillStatus;

			var (needed, newStatus) = responseFunction.ShouldUpdateCustomsStatus(factory, orginalEntryStatus, ((IMessageAttachee)bill).DataGroupingCode);

			if (needed && responseFunction.ThreeCharFunctionCode == Constants.ThreeCharFunctionCodes.MessageRejected)
			{
				needed &= (outgoingMessage?.EM_MessageType ?? ZString.Empty) == CDSEDIMessageTypeList.Codes.NewDeclaration;
			}

			if (needed)
			{
				bill.ABL_BillStatus = newStatus;

				if (bill.ABL_BillStatus == EntryStatusList.Codes.Cancelled)
				{
					Customs.Business.PermitHelper.RollbackPermitTransactions(incomingMessage, outgoingMessage, null, GBPermitHelper.GetPermitAppIdForMessage, ZString.Empty, Core.Constants.CountryCodes.UnitedKingdom, false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
				}
			}

			var messageType = outgoingMessage == null ? ZString.Empty : outgoingMessage.EM_MessageType;
			var messageNum = outgoingMessage == null ? ZString.Empty : outgoingMessage.EM_MessageNum;
			logger.Log($"Entry {((IMessageAttachee)bill).JobNumber}, message #{incomingMessage.EM_MessageNum} ({responseFunction.ThreeCharFunctionCode}), original message #{messageNum} ({messageType}), original status = {orginalEntryStatus}, status update needed = {needed}, new status = {newStatus}, final status = {bill.ABL_BillStatus}");
		}

		static void AttachMessage(AsycudaBill bill, CDSResponseEDIMessage cdsEDIMessage)
		{
			cdsEDIMessage.EM_LinkedObject = bill;
		}

		static void UpdateStatus(AsycudaBill bill, ResponseFunction responseFunction, CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
			ZString status = ZString.Empty;

			switch (responseFunction)
			{
				case MessageRejected _:
					if (ShouldProcessReject(bill, outgoingMessage))
					{
						status = CDSMessageStatusCalculator.GetMessageRejectedStatus(outgoingMessage);
						Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, GBPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
						ProcessRejection(bill, outgoingMessage);
						incomingMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
						bill.Factory.Save();
					}
					break;
				case DeclarationCleared clearFunc:
					status = CDSMessageStatusCalculator.GetMessageClearStatus(outgoingMessage);
					Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, GBPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);

					break;
				default:
					status = CDSMessageStatusCalculator.GetMessageAcknowledgedStatus(outgoingMessage);
					break;
			}

			if (bill != null && !status.IsEmpty)
			{
				bill.ABL_MessageStatus = status;
			}
		}

		static bool ShouldProcessReject(AsycudaBill bill, CDSEDIMessage outgoingMessage)
		{
			return !(outgoingMessage.EM_MessageType == CDSEDIMessageTypeList.Codes.NewDeclaration && bill.ABL_BillStatus == EntryStatusList.Codes.Cancelled);
		}

		static void ProcessRejection(AsycudaBill bill, CDSEDIMessage outgoingMessage)
		{
			if (outgoingMessage.EM_MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration)
			{
				var namMessage = bill.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_ApplicationReference == GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outgoingMessage));

				if (namMessage != null)
				{
					namMessage.EM_Status = EDIMessageStatusList.Codes.Discarded;
				}
			}

			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Rejected;
		}

		static void AddEntryStatusLog(AsycudaBill bill, ResponseFunction responseFunction, Response messageDataObject, ResponseParameters responseParameters)
		{
			var responseFunctionCode = responseFunction?.ThreeCharFunctionCode ?? ZString.Empty;
			bill.Logs.AddNew(Events.CustomsEntryStatus, responseFunctionCode, responseParameters.EventDateTime, responseParameters.EventParameters.ToArray());
		}

		static CDSEDIMessage GetLastOutgoingMessage(this AsycudaBill bill, ZString conversationID)
		{
			var orderedFilteredMessages = bill?.Messages.OfType<CDSEDIMessage>().Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && m.EM_MessageType != CDSEDIMessageTypeList.Codes.NewAmendment)
				.OrderByDescending(x => x.EM_SystemCreateTimeUtc);
			return orderedFilteredMessages.FirstOrDefault(m => !m.EM_ApplicationReference.IsEmpty && m.EM_ApplicationReference == conversationID)
				?? orderedFilteredMessages.FirstOrDefault(m => m.EM_ApplicationReference.IsEmpty)
				?? orderedFilteredMessages.FirstOrDefault();
		}
	}
}
