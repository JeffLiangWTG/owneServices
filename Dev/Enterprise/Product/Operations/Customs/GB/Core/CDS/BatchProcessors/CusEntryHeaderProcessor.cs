using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.GB.CDS.CDSResponse.ResponseFunction;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	static class CusEntryHeaderProcessor
	{
		public static void ProcessCusEntryHeader(BusinessObjectFactory factory, CusEntryHeader cusEntryHeader, CDSResponseEDIMessage cdsEDIMessage, LoggingInformation logger)
		{
			using (DisposableEnvironment.ForBranch(cusEntryHeader.Branch.PK.ToGuid()))
			{
				var messageDataObject = cdsEDIMessage.MessageDataObject;
				var responseParameters = new ResponseParameters();
				var issueDateTime = cdsEDIMessage.MessageDataObject.IssueDateTime?.Item?.ToZDateTime() ?? ZDateTime.Empty;
				if (issueDateTime.IsValid)
				{
					responseParameters.EventDateTime = issueDateTime.UtcToDateTimeOffset();
				}

				var outgoingMessage = cusEntryHeader.GetLastOutgoingMessage(cdsEDIMessage.EM_ApplicationReference);
				var responseFunction = cdsEDIMessage.ResponseFunction;
				cdsEDIMessage.EM_MessageSubType = responseFunction?.ThreeCharFunctionCode ?? cdsEDIMessage.EM_MessageSubType;
				UpdateEntryNumberIfNeeded(factory, cusEntryHeader, responseFunction, messageDataObject.GetMovementReferenceNumber, messageDataObject.GetIssueDate, cdsEDIMessage.EM_MessageNum, logger);
				UpdateEntryStatusIfNeeded(factory, cusEntryHeader, responseFunction, cdsEDIMessage, outgoingMessage, logger);
				AttachMessage(cusEntryHeader, cdsEDIMessage);

				if (outgoingMessage != null)
				{
					UpdateStatus(cusEntryHeader, responseFunction, outgoingMessage, cdsEDIMessage);
				}

				PopulateChallengesIfNeeded(cusEntryHeader, cdsEDIMessage);
				SaveDutiesAndFees(cusEntryHeader, messageDataObject);
				ProcessExitOfGoods(cusEntryHeader, responseFunction, messageDataObject, responseParameters);

				var isPostponedVatViaFiscalReference = cusEntryHeader.EntryInstruction?.IsPostponedVatViaFiscalReference ?? false;

				if (responseFunction != null && (HasCashMethodOfPaymentForAnyFees(cusEntryHeader) || isPostponedVatViaFiscalReference))
				{
					CreateOrUpdatePayInfo(cdsEDIMessage, responseFunction, cusEntryHeader, messageDataObject, isPostponedVatViaFiscalReference);
				}

				GBBondedWarehouseHelper.SetupForBondedWarehousing(cusEntryHeader, outgoingMessage, responseFunction.NumericFunctionCode, logger);
				MarkInvoiceLinesAsErroneousYellowIfNeeded(cusEntryHeader, responseFunction, messageDataObject, outgoingMessage);

				AdditionalUpdateByResponseType(cusEntryHeader, responseFunction, messageDataObject, responseParameters);
				AddEntryStatusLog(cusEntryHeader, responseFunction, messageDataObject, responseParameters);
				SendEntryDocs(factory, cusEntryHeader, responseFunction, logger);
				SendEmailToNotificationGroup(cdsEDIMessage, cusEntryHeader.Messages.LastOutgoingNonSystemAndNonNullUserMessage, factory, logger);
			}
		}

		static void UpdateEntryNumberIfNeeded(BusinessObjectFactory factory, CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, Func<ZString> movementReferenceNumberGetter, Func<ZDateTime> issueDateGetter, ZString messageNumber, LoggingInformation logger)
		{
			if (responseFunction.ShouldUpdateEntryNumber(factory, ((IMessageAttachee)cusEntryHeader).DataGroupingCode ?? ZString.Empty))
			{
				StoreMovementReferenceNumberIfNeeded(cusEntryHeader, movementReferenceNumberGetter?.Invoke() ?? ZString.Empty, issueDateGetter?.Invoke() ?? ZDateTime.Empty, responseFunction, messageNumber, logger);
			}
		}

		static void StoreMovementReferenceNumberIfNeeded(CusEntryHeader cusEntryHeader, ZString movementReferenceNumber, ZDateTime issueDate, ResponseFunction responseFunction, ZString messageNumber, LoggingInformation logger)
		{
			if (!movementReferenceNumber.IsEmpty && !issueDate.IsEmpty)
			{
				var mrnEntryNumber = CusEntryNumber.Load(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				if (mrnEntryNumber == null)
				{
					mrnEntryNumber = CusEntryNumber.New(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
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

		static void UpdateEntryStatusIfNeeded(BusinessObjectFactory factory, CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, CDSResponseEDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			var orginalEntryStatus = cusEntryHeader.CH_EntryStatus;

			var (needed, newStatus) = responseFunction.ShouldUpdateCustomsStatus(factory, orginalEntryStatus, ((IMessageAttachee)cusEntryHeader).DataGroupingCode ?? ZString.Empty);

			if (needed && responseFunction.ThreeCharFunctionCode == Constants.ThreeCharFunctionCodes.MessageRejected)
			{
				needed &= (outgoingMessage?.EM_MessageType ?? ZString.Empty) == CDSEDIMessageTypeList.Codes.NewDeclaration;
			}

			if (needed)
			{
				cusEntryHeader.CH_EntryStatus = newStatus;

				if (cusEntryHeader.CH_EntryStatus == EntryStatusList.Codes.Cancelled)
				{
					Customs.Business.PermitHelper.RollbackPermitTransactions(incomingMessage, outgoingMessage, null, GBPermitHelper.GetPermitAppIdForMessage, ZString.Empty, Core.Constants.CountryCodes.UnitedKingdom, false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
				}
			}

			var messageType = outgoingMessage == null ? ZString.Empty : outgoingMessage.EM_MessageType;
			var messageNum = outgoingMessage == null ? ZString.Empty : outgoingMessage.EM_MessageNum;
			logger.Log($"Entry {((IMessageAttachee)cusEntryHeader).JobNumber}, message #{incomingMessage.EM_MessageNum} ({responseFunction.ThreeCharFunctionCode}), original message #{messageNum} ({messageType}), original status = {orginalEntryStatus}, status update needed = {needed}, new status = {newStatus}, final status = {cusEntryHeader.CH_EntryStatus}");
		}

		static void AttachMessage(CusEntryHeader cusEntryHeader, CDSResponseEDIMessage cdsEDIMessage)
		{
			cdsEDIMessage.EM_LinkedObject = cusEntryHeader;
		}

		static void UpdateStatus(CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
			ZString status = ZString.Empty;

			switch (responseFunction)
			{
				case MessageRejected _:
					if (ShouldProcessReject(cusEntryHeader, outgoingMessage))
					{
						status = CDSMessageStatusCalculator.GetMessageRejectedStatus(outgoingMessage);
						Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, GBPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
						ProcessRejection(cusEntryHeader, outgoingMessage);
						incomingMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
						cusEntryHeader.Factory.Save();
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

			if (cusEntryHeader != null && !status.IsEmpty)
			{
				cusEntryHeader.CH_Status = status;
			}
		}

		static bool ShouldProcessReject(CusEntryHeader cusEntryHeader, CDSEDIMessage outgoingMessage)
		{
			return !(outgoingMessage.EM_MessageType == CDSEDIMessageTypeList.Codes.NewDeclaration && cusEntryHeader.CH_EntryStatus == EntryStatusList.Codes.Cancelled);
		}

		static void ProcessRejection(CusEntryHeader cusEntryHeader, CDSEDIMessage outgoingMessage)
		{
			if (outgoingMessage.EM_MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration)
			{
				var namMessage = cusEntryHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_ApplicationReference == GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outgoingMessage));

				if (namMessage != null)
				{
					namMessage.EM_Status = EDIMessageStatusList.Codes.Discarded;
				}
			}

			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Rejected;
		}

		static void PopulateChallengesIfNeeded(CusEntryHeader cusEntryHeader, CDSResponseEDIMessage message)
		{
			if (message.MessageDataObject.AdditionalInformation != null)
			{
				var errors = message.Prettier.GetAllFriendlyErrors().ToList();
				var smartErrors = message.MessageDataObject.AdditionalInformation.Where(info => info.StatementCode?.Value == "smartErrorMsg");
				foreach (var smartError in smartErrors)
				{
					var error = FindFriendlyErrorCode(smartError, errors);
					var cusEntryLine = cusEntryHeader.AllEntryLines.FindByLineNumber(ZInt.ParseSafe(error?.LineNumber, ZInt.Zero));
					if (cusEntryLine != null)
					{
						switch (smartError.StatementDescription?.Value)
						{
							case "Value per kilo appears too low for this commodity":
							case "Value per kilo appears too high for this commodity":
							case "Value does not appear credible for commodity weight":
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_NettMass);
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_Price);
								break;
							case "Supplementary unit does not appear credible for commodity value":
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_Price);
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_Supp);
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_SuppUQ);
								break;
							case "Supplementary unit does not appear credible for commodity weight":
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_NettMass);
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_Supp);
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_SuppUQ);
								break;
							case "Weight appears too low per item":
							case "Weight appears too high per item":
								AddOrUpdateChallenge(cusEntryHeader.FECChallenges, cusEntryLine, FECChallengeFields.Codes.JI_NettMass);
								break;
						}
					}
				}
			}
		}

		static FriendlyCodeWithPointers FindFriendlyErrorCode(ResponseAdditionalInformation info, IList<FriendlyCodeWithPointers> errorCodes)
		{
			FriendlyCodeWithPointers result = null;
			// "07B/53A" represents "//Response/Error" in XML
			if (info.Pointer.Length == 2 && info.Pointer[0].DocumentSectionCode?.Value == "07B" && info.Pointer[1].DocumentSectionCode?.Value == "53A")
			{
				var index = (int)info.Pointer[1].SequenceNumeric;
				if (index > 0 && index <= errorCodes.Count)
				{
					result = errorCodes[index - 1];
				}
			}
			return result;
		}

		static void AddOrUpdateChallenge(FECChallengeCollection collection, CusEntryLine entryLine, ZString code)
		{
			var parentTableCode = CusEntryLineSchema.Constants.Prefix;

			var challenge = collection.Cast<FECChallenge>().FirstOrDefault(
				x => x.CY_ParentTableCode == parentTableCode && x.CY_ParentID == entryLine.PK && x.CY_Code == code
			);
			if (challenge == null)
			{
				challenge = collection.AddNew();
				challenge.CY_ParentTableCode = parentTableCode;
				challenge.CY_ParentID = entryLine.PK;
				challenge.CY_Code = code;
				challenge.CY_Order = entryLine.CL_LineNumber;
				challenge.CY_IsOverridden = false;
			}
			challenge.CY_Data = challenge.NewValue;
		}

		static void SaveDutiesAndFees(CusEntryHeader cusEntryHeader, Response messageDataObject)
		{
			var goodsItems = messageDataObject.GetGovernmentAgencyGoodsItem();

			if (goodsItems != null)
			{
				DeleteConfirmedFees(cusEntryHeader);

				foreach (var goodsItem in goodsItems)
				{
					var entryLine = cusEntryHeader.AllEntryLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == goodsItem.SequenceNumeric);

					if (entryLine != null)
					{
						foreach (var dutyTaxFree in goodsItem.Commodity)
						{
							var typeCode = dutyTaxFree.TypeCode?.Value ?? ZString.Empty;
							var payment = dutyTaxFree.Payment;
							var paymentAmount = payment?.PaymentAmount?.Value ?? 0;

							if (typeCode != Constants.TaxTypeCodes.Subsidy)
							{
								var calculatedFees = entryLine.Fees.OfType<CusEntryLineFee.CusEntryLineFee>().Where(f => f.CF_ChargeType == typeCode);
								var calculatedFee = calculatedFees.FirstOrDefault();
								var paymentAssessedAmount = payment?.TaxAssessedAmount?.Value ?? 0;

								var confirmedFee = entryLine.ConfirmedFees.AddOrUpdate(typeCode, paymentAmount);
								confirmedFee.CF_Rate = dutyTaxFree.TaxRateNumeric;

								if (dutyTaxFree.AdValoremTaxBaseAmount != null)
								{
									confirmedFee.CF_BaseValue = dutyTaxFree.AdValoremTaxBaseAmount.Value;
								}
								else if (calculatedFees.Count() == 1)
								{
									confirmedFee.CF_MethodOfCalculation = calculatedFee.CF_MethodOfCalculation;
									confirmedFee.CF_BaseValue = calculatedFee.CF_BaseValue;
								}

								if (string.IsNullOrEmpty(calculatedFee?.CF_MethodOfPayment))
								{
									if ((typeCode == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat || typeCode == EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland)
										&& (cusEntryHeader.EntryInstruction?.IsPostponedVatViaFiscalReference ?? false))
									{
										if (paymentAmount == 0)
										{
											confirmedFee.CF_MethodOfPayment = ZString.Empty;
										}
									}
									else if (dutyTaxFree.TaxRateNumeric > 0)
									{
										confirmedFee.CF_MethodOfPayment = entryLine.RandomLine.ZG_MethodOfPayment;
									}
								}
								else if (!string.IsNullOrEmpty(calculatedFee?.CF_MethodOfPayment))
								{
									confirmedFee.CF_MethodOfPayment = calculatedFee.CF_MethodOfPayment;
								}

								if (paymentAmount == 0 && paymentAssessedAmount != 0)
								{
									confirmedFee.CF_ChargeAmount = paymentAssessedAmount;
									confirmedFee.CF_IsLandedCostOnly = true;
								}
								else
								{
									confirmedFee.CF_ChargeAmount = paymentAmount;
								}
							}
						}
					}
				}
			}
		}

		static void DeleteConfirmedFees(CusEntryHeader header)
		{
			header.ConfirmedCharges.RemoveAndDeleteAll();
			foreach (CusEntryLine entryLine in header.AllEntryLines)
			{
				entryLine.ConfirmedFees.RemoveAndDeleteAll();
			}
		}

		static void ProcessExitOfGoods(CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, Response messageDataObject, ResponseParameters responseParameters)
		{
			switch (responseFunction.ThreeCharFunctionCode)
			{
				case Constants.ThreeCharFunctionCodes.GoodsExitedCustomsUnion:
					ProcessGoodsExitedCustomsUnion(cusEntryHeader, messageDataObject, responseParameters);
					break;
				case Constants.ThreeCharFunctionCodes.ExitOfGoodsFromEUNotConfirmed:
					cusEntryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived;
					break;
			}
		}

		static void ProcessGoodsExitedCustomsUnion(CusEntryHeader cusEntryHeader, Response messageDataObject, ResponseParameters responseParameters)
		{
			var statement = messageDataObject.AdditionalInformation?.FirstOrDefault(x => x.StatementTypeCode?.Value == CDSResponseStatementTypeList.Codes.ClearanceInstructionsForExport);
			if (statement != null)
			{
				var value = statement.StatementDescription?.Value;
				cusEntryHeader.CH_ExitActualOffice = value;
				responseParameters.EventParameters.Add(new KeyValuePair<string, string>("LOC", value));
			}

			ResponseStatus status = null;
			if (messageDataObject.Status != null)
			{
				status = messageDataObject.Status.FirstOrDefault(x => x.NameCode?.Value == CDSResponseStatementCodeList.Codes.SatisfactoryAllDeclarationsWhichHaveNotResultedInA2OrB1
					|| x.NameCode?.Value == CDSResponseStatementCodeList.Codes.ConsideredSatisfactoryThereAreNoControlTasksForTheDeclaration);
				if (status != null)
				{
					cusEntryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;
				}
				else
				{
					status = messageDataObject.Status.FirstOrDefault(x => x.NameCode?.Value == CDSResponseStatementCodeList.Codes.NotSatisfactoryThereIsAMajorDiscrepancyControlResultForTheDeclaration);
					cusEntryHeader.CH_ExitedStatus = status != null ? ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory : ExportExitStatus.Codes.UnknownOrNotReported;
				}

				cusEntryHeader.CH_ExitDate = status?.EffectiveDateTime?.Item?.ToZDateTime() ?? ZDateTime.Empty;

				status ??= messageDataObject.Status.FirstOrDefault(x => !string.IsNullOrEmpty(x.NameCode?.Value));
				responseParameters.EventParameters.Add(new KeyValuePair<string, string>("TYP", status?.NameCode?.Value ?? string.Empty));
			}
		}

		static bool HasCashMethodOfPaymentForAnyFees(CusEntryHeader header)
		{
			return header.AllEntryLines.Cast<CusEntryLine>().Any(
				line => line.ConfirmedFees.Cast<Business.Declaration.CusEntryLineFee>().Any(
					fee => MethodOfPaymentCodes.CDS.ImmediateCashMethodsOfPayment.Contains(fee.CF_MethodOfPayment)));
		}

		static void CreateOrUpdatePayInfo(CDSResponseEDIMessage cdsEDIMessage, ResponseFunction responseFunction, CusEntryHeader cusEntryHeader, Response messageDataObject, bool pvaInUse)
		{
			var declarationDutyTaxFee = messageDataObject.Declaration?.DutyTaxFee;
			var cashPaymentInfo = cusEntryHeader.EntryPayInfos.FirstOrDefault(x => x.C9_TransactionType == PaymentTransactionTypeList.Codes.Cash);
			var pvaPaymentInfo = cusEntryHeader.EntryPayInfos.FirstOrDefault(x => x.C9_TransactionType == PaymentTransactionTypeList.Codes.PostponedVATAccounting);

			if (declarationDutyTaxFee?.Length == 1)
			{
				if (cashPaymentInfo == null && responseFunction is DutiesTaxesCalculatedAndDue)
				{
					cashPaymentInfo = cusEntryHeader.EntryPayInfos.AddNew();
					cashPaymentInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.Cash;

					var payment = declarationDutyTaxFee[0].Payment;

					cashPaymentInfo.C9_IncomingPayResponseNo = cdsEDIMessage.EM_MessageNum;
					cashPaymentInfo.C9_PaymentAmount = payment.PaymentAmount.Value;
					cashPaymentInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.Cash;

					if (responseFunction is DutiesTaxesCalculatedAndDue)
					{
						var statusCode = SetStatusForPayInfo(messageDataObject, cashPaymentInfo);

						if (cashPaymentInfo.C9_ReceiptDate.IsEmpty && statusCode == Constants.StatusNameCode.FinalCustomsDebt)
						{
							cashPaymentInfo.C9_ReceiptDate = messageDataObject.GetIssueDate().Date;
						}

						cashPaymentInfo.C9_PaymentReference = payment.ReferenceID.Value;
					}
				}
			}

			if (pvaInUse)
			{
				if (pvaPaymentInfo == null)
				{
					pvaPaymentInfo = cusEntryHeader.EntryPayInfos.AddNew();
					pvaPaymentInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.PostponedVATAccounting;
				}

				var pvaDetails = messageDataObject.Declaration?.GoodsShipment?.
								SelectMany(x => x.Commodity).
								Select(x => x).
								Where(x => (new List<ZString> { FeeTypeList.Codes.B00, EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland }).Contains(x.TypeCode.Value));

				if (pvaDetails?.Any() ?? false)
				{
					var pvaTotal = pvaDetails.Sum(x => x.Payment.TaxAssessedAmount.Value);

					pvaPaymentInfo.C9_PaymentAmount = pvaTotal;
					pvaPaymentInfo.C9_PaymentReference = cusEntryHeader.EntryInstruction.PVAFiscalReference;
					pvaPaymentInfo.C9_IncomingPayResponseNo = cdsEDIMessage.EM_MessageNum;
					var statusCode = SetStatusForPayInfo(messageDataObject, pvaPaymentInfo);
					if (pvaPaymentInfo.C9_ReceiptDate.IsEmpty && statusCode == Constants.StatusNameCode.FinalCustomsDebt)
					{
						pvaPaymentInfo.C9_ReceiptDate = messageDataObject.GetIssueDate().Date;
					}
				}
			}

			if (cashPaymentInfo != null && responseFunction is DeclarationCleared)
			{
				ProcessPayInfoForClearance(cdsEDIMessage, messageDataObject, cashPaymentInfo);
			}

			if (pvaPaymentInfo != null && responseFunction is DeclarationCleared)
			{
				ProcessPayInfoForClearance(cdsEDIMessage, messageDataObject, pvaPaymentInfo);
			}
		}

		static ZString SetStatusForPayInfo(Response messageDataObject, Customs.Business.CusEntryPayInfo paymentInfo)
		{
			var statusCode = messageDataObject.GetStatusNameCode();
			if (statusCode == Constants.StatusNameCode.ProvisionalCustomsDebt)
			{
				paymentInfo.C9_PaymentStatus = PaymentStatusList.Codes.ProvisionalUnpaid;
			}
			else if (statusCode == Constants.StatusNameCode.FinalCustomsDebt)
			{
				paymentInfo.C9_PaymentStatus = PaymentStatusList.Codes.FinalUnpaid;
			}

			return statusCode;
		}

		static void ProcessPayInfoForClearance(CDSResponseEDIMessage cdsEDIMessage, Response messageDataObject, Customs.Business.CusEntryPayInfo paymentInfo)
		{
			paymentInfo.C9_IncomingPayResponseNo = cdsEDIMessage.EM_MessageNum;
			paymentInfo.C9_PaymentDate = messageDataObject.GetIssueDate();
			paymentInfo.C9_PaymentStatus = PaymentStatusList.Codes.FinalPaid;
		}

		static void MarkInvoiceLinesAsErroneousYellowIfNeeded(CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, Response messageDataObject, CDSEDIMessage outgoingMessage)
		{
			if (responseFunction is MessageRejected && outgoingMessage != null)
			{
				var rejectedInvoiceLineNumbers = GetRejectedEntryLineNumbers(messageDataObject, outgoingMessage).ToArray();
				if (rejectedInvoiceLineNumbers.Any())
				{
					cusEntryHeader.MarkInvoiceLinesAsErroneousYellow(rejectedInvoiceLineNumbers);
				}
			}
		}

		static IEnumerable<ZInt> GetRejectedEntryLineNumbers(Response messageDataObject, CDSEDIMessage outgoingMessage)
		{
			var outgoingMessageText = outgoingMessage.EM_MessageText;
			if (!outgoingMessageText.IsEmpty)
			{
				var friendlyErrors = messageDataObject.GetAllFriendlyErrorsForRequestAndRejection(XElement.Parse(outgoingMessageText));
				return friendlyErrors.Select(x => ZInt.ParseSafe(x.LineNumber, ZInt.Zero));
			}

			return Enumerable.Empty<ZInt>();
		}

		static void AdditionalUpdateByResponseType(CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, Response messageDataObject, ResponseParameters responseParameters)
		{
			switch (responseFunction)
			{
				case DeclarationAccepted:
					if (cusEntryHeader.EntryInstruction != null && messageDataObject.Declaration?.AcceptanceDateTime != null)
					{
						var acceptanceDateTime = messageDataObject.Declaration.AcceptanceDateTime.Item.ToZDateTime();
						cusEntryHeader.EntryInstruction.CEI_DateForDuty = acceptanceDateTime;
						responseParameters.EventDateTime = new ZDateTimeOffset(acceptanceDateTime);
					}
					break;
				case DutiesTaxesCalculatedAndDue:
					if (cusEntryHeader.Declaration != null && messageDataObject.GetStatusNameCode() == Constants.StatusNameCode.FinalCustomsDebt)
					{
						cusEntryHeader.Declaration.ResumeApportionment();
					}
					break;
			}
		}

		static void AddEntryStatusLog(CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, Response messageDataObject, ResponseParameters responseParameters)
		{
			var responseFunctionCode = responseFunction?.ThreeCharFunctionCode ?? ZString.Empty;
			cusEntryHeader.Logs.AddNew(Events.CustomsEntryStatus, responseFunctionCode, responseParameters.EventDateTime, responseParameters.EventParameters.ToArray());
		}

		static void SendEntryDocs(BusinessObjectFactory factory, CusEntryHeader cusEntryHeader, ResponseFunction responseFunction, LoggingInformation logger)
		{
			var declaration = cusEntryHeader.Declaration;
			if (declaration != null && responseFunction.ShouldSendEntryDocs(factory, declaration.GetDefaultDataGroupingCode()))
			{
				try
				{
					var documentFactory = declaration.DocManagerInfo.MasterFactory;
					var storageMain = documentFactory.RetrieveExistingOrCreateStorageMain(declaration, Core.Constants.DocManagerCodes.JobDeclaration);
					var template = ExcelTemplateRetriever.GetTemplate("CDSEntryDocument", Business.Declaration.CusEntryHeaderDocumentSupporter.GbCDSEntryHeader, factory);
					var docDataProvider = BODocDataProvider.Get(DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.GB.DocumentWrappers.DocCDSEntryHeaderWrapper, Enterprise.Customs.GB.DocumentWrappers", cusEntryHeader));
					using (var report = new Report(null, template, docDataProvider, template.TemplateName, null, DocumentDirection.ANY, false))
					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						var binaryData = DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ColourDepth.BlackAndWhite);
						var edoc = storageMain.AddFileOrDocument(binaryData, " CDS Entry Document - " + ((IMessageAttachee)cusEntryHeader).JobNumber + ".pdf", Core.Constants.RefDocTypes.EntryPrint);
						edoc.Description = Core.Constants.RefDocTypeDescriptions.EntryPrint;
						cusEntryHeader.Logs.AddNew(Events.DocumentAllocated,
							string.Concat(Core.Constants.RefDocTypes.EntryPrint, "|", edoc.UniqueKey));
						documentFactory.Save();
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					logger.Log("The CDS Entry documents have failed to be generated due to the following error : " + e.Message); // Log entries are not localized
				}
			}
		}

		static void SendEmailToNotificationGroup(CDSResponseEDIMessage responseMessage, EDIMessage outgoingMessage, BusinessObjectFactory factory, LoggingInformation logger)
		{
			if (!responseMessage.EM_MessageInterpretation.IsEmpty)
			{
				var notificationEmailGroupPk = Guid.Empty;
				IRegistryItem notificationEmailGroupRegistry = null;
				var declaration = responseMessage.LinkedEntry.Declaration;
				var email = new HtmlEmailDef();
				email.Subject = "A CDS response has been received for job " + declaration.JE_DeclarationReference;
				email.Body = responseMessage.EM_MessageInterpretation + "\n\nClick here to open the job: " + EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference);
				var originalSender = outgoingMessage?.UserWhoQueuedThisRecord ?? declaration.CusAgent;
				var notificationCode = responseMessage.ResponseFunction.ThreeCharFunctionCode.ToString();

				if (responseMessage.ResponseFunction.Category == Constants.ResponseFunctionCategory.PositiveReplies)
				{
					notificationEmailGroupPk = GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCDSPositiveReplies, notificationCode, Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
					notificationEmailGroupRegistry = GBCustomsDataRegistry.Instance.GetNotificationItem(notificationCode, "", GBCustomsDataRegistry.Instance.NotificationCDSPositiveReplies);
				}
				else if (responseMessage.ResponseFunction.Category == Constants.ResponseFunctionCategory.Rejections)
				{
					notificationEmailGroupPk = GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCDSRejections, notificationCode, Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
					notificationEmailGroupRegistry = GBCustomsDataRegistry.Instance.GetNotificationItem(notificationCode, "", GBCustomsDataRegistry.Instance.NotificationCDSRejections);
				}
				else if (responseMessage.ResponseFunction.Category == Constants.ResponseFunctionCategory.UnsolicitedUpdates)
				{
					notificationEmailGroupPk = GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCDSUnsolicitedUpdates, notificationCode, Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
					notificationEmailGroupRegistry = GBCustomsDataRegistry.Instance.GetNotificationItem(notificationCode, "", GBCustomsDataRegistry.Instance.NotificationCDSUnsolicitedUpdates);
				}
				else
				{
					notificationEmailGroupPk = GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCDS, "", Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
					notificationEmailGroupRegistry = GBCustomsDataRegistry.Instance.GetNotificationItem("", "", GBCustomsDataRegistry.Instance.NotificationCDS);
				}

				new EmailSender(logger).SendNotification(
						email,
						originalSender,
						GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
						notificationEmailGroupPk,
						notificationEmailGroupRegistry,
						factory);
			}
		}

		public static CDSEDIMessage GetLastOutgoingMessage(this CusEntryHeader cusEntryHeader, ZString conversationID)
		{
			var orderedFilteredMessages = cusEntryHeader?.Messages.OfType<CDSEDIMessage>().Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && m.EM_MessageType != CDSEDIMessageTypeList.Codes.NewAmendment)
				.OrderByDescending(x => x.EM_SystemCreateTimeUtc);
			return orderedFilteredMessages.FirstOrDefault(m => !m.EM_ApplicationReference.IsEmpty && m.EM_ApplicationReference == conversationID)
				?? orderedFilteredMessages.FirstOrDefault(m => m.EM_ApplicationReference.IsEmpty)
				?? orderedFilteredMessages.FirstOrDefault();
		}
	}
}
