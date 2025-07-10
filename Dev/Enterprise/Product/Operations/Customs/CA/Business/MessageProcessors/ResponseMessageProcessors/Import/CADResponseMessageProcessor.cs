using System;
using System.Globalization;
using System.Linq;
using System.Net;
using CargoWise.Common;
using CargoWise.Customs.CA.MessageDefinitions.CAD.Outbound;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class CADResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public CADResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new CADStatusCalculator(), MessageTypeList.Codes.CommercialAccountingDeclaration, Res.GetString("8E2A5333-E439-46C9-A151-6141AE20606F", "CAD Response"))
		{
		}

		protected new CADStatusCalculator StatusCalculator
		{
			get
			{
				return (CADStatusCalculator)base.StatusCalculator;
			}
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			base.DoPreProcessingReturningStatus(ediMessage);
			var message = (CADMessage)ediMessage;
			var documentMetaData = XmlObjectSerializer.Deserialize<DocumentMetaData>(message.EM_MessageText);

			if (documentMetaData?.Response == null)
			{
				Logger.Log(Res.GetString("DA569B89-DF6F-4DFA-86A9-18DD1AF04ECA", "Syntax error in CAD response message."), Integration.LogType.Error);
				throw new UnableToInterpretMessageException(message, this);
			}

			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				linkedObject = entryHeader;
				linkedObjectReference = entryHeader.CH_BGMReference;
			}
			else
			{
				var applicationReferenceID = documentMetaData.CommunicationMetaData.ApplicationReferenceId;
				if (applicationReferenceID != null)
				{
					linkedObjectReference = new ZString(applicationReferenceID.Value).SubstringSafe(0, 14);
				}
				var entry = ImportLinkedObjectManager.GetCusEntryHeaderByOriginalTransactionNo(message.Factory, linkedObjectReference, new string[] { MessageTypeList.Codes.CommercialAccountingDeclaration })
					?? ImportLinkedObjectManager.GetCusEntryHeaderByTransactionNumber(message.Factory, linkedObjectReference, new string[] { MessageTypeList.Codes.CommercialAccountingDeclaration });
				if (entry != null)
				{
					linkedObject = entry;
					linkedObject.Messages.Add(message);
					message.EM_GB = entry.RegistryBranchPK;
				}
			}

			if (linkedObject != null)
			{
				message.EM_MessageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);
				var response = documentMetaData.Response;

				SetupEntryDetails(response, message);

				using (DisposableEnvironment.ForBranch(message.Branch.PK.ToGuid()))
				{
					var responseStatus = response.Status?.NameCode?.Value ?? CADEntryStatusList.Codes.Unknown;
					if (responseStatus == CADEntryStatusList.Codes.Approved)
					{
						if (response.Declaration != null)
						{
							var receivedMessageStatus = StatusCalculator.GetMessageClearedStatus(message);
							message.EM_MessageSubType = receivedMessageStatus;
							if (!receivedMessageStatus.IsEmpty)
							{
								linkedObject.MessageStatus = receivedMessageStatus;
							}
							linkedObject.JobStatus = responseStatus;
							ReleaseCADEntry(response);
							var email = GetAcceptedEmailAndSetOnMessage(response, message, responseStatus);
							SendAcknowledgementReport(EmailResponseLinkedObject, email);
						}
						else
						{
							Logger.Log(Res.GetString("FD9473BE-0CD6-472B-B9C6-96A41AF36DFF", "Syntax error in CAD response message."), Integration.LogType.Error);
							throw new UnableToInterpretMessageException(message, this);
						}
					}
					else if (responseStatus == CADEntryStatusList.Codes.ApprovalPending)
					{
						linkedObject.MessageStatus = message.EM_MessageSubType = StatusCalculator.GetMessageAwaitingStatus(message);
						linkedObject.JobStatus = responseStatus;
						var email = GetApprovalPendingEmailAndSetOnMessage(response, message, responseStatus);
						SendImpedimentReport(EmailResponseLinkedObject, email);
					}
					else if (responseStatus == CADEntryStatusList.Codes.Acknowledged)
					{
						var receivedMessageStatus = StatusCalculator.GetMessageAcknowledgedStatus(message);
						if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
						{
							linkedObject.MessageStatus = receivedMessageStatus;
							linkedObject.JobStatus = responseStatus;
						}

						message.EM_MessageSubType = receivedMessageStatus;
						var email = GetFunctionalAcknowledgementEmailAndSetOnMessage(response, message, responseStatus);
						SendAcknowledgementReport(EmailResponseLinkedObject, email);
					}
					else
					{
						linkedObject.MessageStatus = message.EM_MessageSubType = StatusCalculator.GetMessageRejectedStatus(message);
						var email = GetErrorEmailAndSetOnMessage(response, message, responseStatus);
						SendErrorReport(EmailResponseLinkedObject, email);

						if (!IsErrorResponseStatus(responseStatus))
						{
							ErrorReporter.ReportOnce("UnknownCADMessage_UnknownResponseStatus", message.EM_MessageText);
						}

						if (CADResponseStatusNameCodeList.ContainsCode(responseStatus) || new CADEntryErrorResponseStatusList().ContainsCode(responseStatus))
						{
							linkedObject.JobStatus = responseStatus;
						}
						else
						{
							linkedObject.JobStatus = CADEntryStatusList.Codes.Unknown;
						}
					}
					message.EM_Status = EDIMessage.Status.Received;
				}
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				throw new CouldNotFindLinkedObjectException(linkedObjectReference, message, this);
			}

			message.EM_Status = EDIMessage.Status.Received;
			return message.EM_Status;
		}

		bool IsErrorResponseStatus(ZString responseStatus)
		{
			return responseStatus == CADEntryStatusList.Codes.InvalidSyntax
				|| responseStatus == CADEntryStatusList.Codes.Rejected
				|| responseStatus == CADEntryStatusList.Codes.SenderInfoInconsistent
				|| responseStatus == CADEntryStatusList.Codes.ProfileDoesNotExist
				|| responseStatus == CADEntryStatusList.Codes.Unknown
				|| responseStatus == CADEntryErrorResponseStatusList.Codes.GoodsShipmentWithAnonymousType
				|| responseStatus == CADEntryErrorResponseStatusList.Codes.FailedToEstablishABacksideConnection
				|| responseStatus == CADEntryErrorResponseStatusList.Codes.InternalError
				|| responseStatus == CADEntryErrorResponseStatusList.Codes.ConnectionError
				|| responseStatus == "40";
		}

		const string SequenceNumberFormat = "00000";

		void SetupEntryDetails(Response response, CADMessage message)
		{
			if (linkedObject is CusEntryHeader entry && response.Declaration != null)
			{
				var declaration = entry.Declaration;
				var responseDeclaration = response.Declaration;
				var responseVersionID = responseDeclaration.VersionId?.Value ?? ZString.Empty;
				ZShort versionID;
				if (ZShort.TryParse(responseVersionID, out versionID) && versionID > 0)
				{
					entry.CH_VersionID = versionID;
				}
				else
				{
					Logger.Log(Res.GetString("C35D70ED-4BDD-430F-98AE-CF951CAAA998", "Incorrect Version Id in response message."), Integration.LogType.Error);
					throw new UnableToInterpretMessageException(message, this);
				}

				SetupEntryHeaderAndRelevantData(responseDeclaration, entry);

				SetupEntryLineAndRelevantData(message, responseDeclaration, declaration, entry);

				if (entry.DutyFeeChangedSinceLastResponse)
				{
					declaration.CA_DeclarationException = CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy;
				}
				else if (declaration.CA_DeclarationException == CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy)
				{
					declaration.CA_DeclarationException = ZString.Empty;
				}
			}
		}

		void SetupEntryHeaderAndRelevantData(Declaration responseDeclaration, CusEntryHeader entry)
		{
			var charges = entry.ConfirmedCharges;
			foreach (var dutyTaxFee in responseDeclaration.DutyTaxFee)
			{
				var chargeType = dutyTaxFee.TypeCode.Value;
				var amount = (chargeType == CADDutyTaxFeeTypeCodes.Codes.TOT ? dutyTaxFee.AdValoremTaxBaseAmount?.Value : dutyTaxFee.Payment?.PaymentAmount?.Value) ?? decimal.Zero;
				var charge = charges.FirstOrDefault(x => x.C1_ChargeType == chargeType);
				if (charge == null)
				{
					charge = charges.AddNew();
					charge.C1_ChargeType = chargeType;
				}
				charge.C1_ChargeAmount = amount;
			}
		}

		void SetupEntryLineAndRelevantData(CADMessage message, Declaration responseDeclaration, JobDeclaration declaration, CusEntryHeader entry)
		{
			var responseGoodsShipment = responseDeclaration.GoodsShipment;
			if (responseGoodsShipment == null)
			{
				Logger.Log(Res.GetString("242B2567-442F-4D8F-8352-8CF32F787943", "Incorrect data. Response Goods Shipment should not be null."), Integration.LogType.Error);
				throw new UnableToInterpretMessageException(message, this);
			}
			var commodityCount = responseGoodsShipment.SelectMany(x => x.GovernmentAgencyGoodsItem.Select(y => y)).Count();
			var entryLines = entry.MergedLines.OrderBy(x => x.CL_LineNumber).ToList();
			var entryLinesCount = entryLines.Count;
			if (commodityCount != entryLinesCount)
			{
				if (responseDeclaration.TypeCode.Value != CADDeclaration.FType)
				{
					var messageCollector = new SendsMessagesToCustomsShutterUpperer(false);
					declaration.DoMerge(messageCollector);
					entryLines = entry.MergedLines.OrderBy(x => x.CL_LineNumber).ToList();
					entryLinesCount = entryLines.Count;
				}

				if (commodityCount != entryLinesCount)
				{
					message.EM_Status = EDIMessage.Status.Failed;
					Logger.Log(Res.GetString("FB0E6556-E000-4C09-A24E-FE93269D657C", "Different amount. Commodity amount: {0}, Entry line amount: {1}", commodityCount, entryLinesCount), Integration.LogType.Error);
					throw new UnableToInterpretMessageException(message, this);
				}
			}

			var goodsShipments = responseGoodsShipment.OrderBy(x => x.SequenceNumeric);

			foreach (var goodsShipment in goodsShipments)
			{
				var goodsSequence = goodsShipment.SequenceNumeric.ToString(SequenceNumberFormat, CultureInfo.InvariantCulture);
				var commodities = goodsShipment.GovernmentAgencyGoodsItem.OrderBy(x => x.SequenceNumeric);
				foreach (var commodity in commodities)
				{
					var commoditySequence = commodity.SequenceNumeric.ToString(SequenceNumberFormat, CultureInfo.InvariantCulture);
					var entryLine = entryLines.FirstOrDefault(x => x.CL_LineNumber.ToString(SequenceNumberFormat, CultureInfo.InvariantCulture) == commoditySequence);
					if (entryLine == null)
					{
						Logger.Log(Res.GetString("C1A3D3A4-3D3D-4A3D-8A3D-3D3D3D3D3D3D", "Incorrect data. Entry line should not be null."), Integration.LogType.Error);
						throw new UnableToInterpretMessageException(message, this);
					}
					entryLine.CL_GoodsShipmentSequence = ZShort.Parse(goodsSequence);
					entryLine.CL_CommoditySequence = ZShort.Parse(commoditySequence);

					var tot = commodity.DutyTaxFee.FirstOrDefault(x => x.TypeCode.Value == CADDutyTaxFeeTypeCodes.Codes.TOT)?.
						DutyTaxFeeAssessmentBasis.FirstOrDefault(y => y.AdValoremTaxBaseAmount.CurrencyId == Core.Constants.CurrencyCodes.Canada)?.AdValoremTaxBaseAmount.Value ?? ZDecimal.Zero;
					var vft = commodity.DutyTaxFee.FirstOrDefault(x => x.TypeCode.Value == CADDutyTaxFeeTypeCodes.Codes.VFT)?.Payment.TaxAssessedAmount.Value ?? ZDecimal.Zero;

					foreach (var commodityFee in commodity.DutyTaxFee)
					{
						var fees = entryLine.ConfirmedFees.Cast<CusEntryLineFee>();
						var chargeType = commodityFee.TypeCode.Value;
						var entryLineFee = fees.FirstOrDefault(x => x.CF_ChargeType == chargeType);
						if (entryLineFee == null)
						{
							entryLineFee = entryLine.ConfirmedFees.AddNew();
							entryLineFee.CF_ChargeType = chargeType;
						}
						entryLineFee.CF_ChargeAmount = commodityFee.Payment?.PaymentAmount?.Value ?? 0m;

						if (chargeType != CADDutyTaxFeeTypeCodes.Codes.TOT && chargeType != CADDutyTaxFeeTypeCodes.Codes.VFT)
						{
							var specificBaseValue = commodityFee.SpecificTaxBaseQuantity?.Value ?? ZDecimal.Zero;
							var advaloremBaseValue = ZDecimal.Zero;
							if (CADDutyTaxFeeTypeCodes.IsTaxCode(chargeType))
							{
								advaloremBaseValue = vft;
							}
							else
							{
								advaloremBaseValue = tot;
							}

							var diff = decimal.MaxValue;
							foreach (var rate in commodityFee.Rate)
							{
								var advaloremRate = rate.AdvaloremTaxBaseRateNumeric;
								var specificRate = rate.SpecificTaxRate;

								var calculatedTax = ZDecimal.Zero;
								if (advaloremRate != null)
								{
									calculatedTax = advaloremBaseValue * advaloremRate.Value * 0.01m;
									var calculatedDiff = Math.Abs(calculatedTax - entryLineFee.CF_ChargeAmount);
									if (calculatedDiff < diff)
									{
										diff = calculatedDiff;
										entryLineFee.CF_BaseValue = advaloremBaseValue;
										entryLineFee.CF_Rate = advaloremRate.Value;
										entryLineFee.CF_MethodOfCalculation = AmountTypes.Codes.Percent;
									}
								}
								else if (specificRate != null)
								{
									calculatedTax = specificBaseValue * specificRate.SpecificTaxBaseQuantity.Value;
									var calculatedDiff = Math.Abs(calculatedTax - entryLineFee.CF_ChargeAmount);
									if (calculatedDiff < diff)
									{
										diff = calculatedDiff;
										entryLineFee.CF_BaseValue = specificBaseValue;
										entryLineFee.CF_Rate = specificRate.SpecificTaxBaseQuantity.Value;
										entryLineFee.CF_MethodOfCalculation = specificRate.SpecificTaxBaseQuantity.UnitCode;
									}
								}
							}
						}
					}
				}
			}
		}

		void ReleaseCADEntry(Response response)
		{
			ZDateTime releaseDate;
			ZDateTime dueDateTime;
			var responseDeclaration = response.Declaration;
			var totDutyTaxFee = responseDeclaration.DutyTaxFee.FirstOrDefault(x => x.TypeCode.Value == CADDutyTaxFeeTypeCodes.Codes.TOT);

			if (ZDateTime.TryParseExact(responseDeclaration.AcceptanceDateTime.DateTimeString, out releaseDate, "yyyyMMddHHmmss"))
			{
				if (linkedObject is IB3MessageProcessorLinkedObject b3LinkedObject)
				{
					b3LinkedObject.EntryReleaseDate = releaseDate;
					b3LinkedObject.CancelScheduledB3Message();
					b3LinkedObject.CancelB3LateSendingWarningEvent();
				}

				var totPaymentAmount = totDutyTaxFee?.Payment?.PaymentAmount?.Value ?? 0m;
				if (totPaymentAmount == ZDecimal.Zero && linkedObject is IK84ReportAttachee k84ReportAttachee)
				{
					if (k84ReportAttachee.AccountingDate.IsEmpty)
					{
						k84ReportAttachee.AccountingDate = releaseDate;
					}

					if (k84ReportAttachee.StatementDate.IsEmpty)
					{
						k84ReportAttachee.StatementDate = releaseDate;
					}
				}
			}

			if (ZDateTime.TryParseExact(totDutyTaxFee?.Payment?.DueDateTime?.DateTimeString, out dueDateTime, "yyyyMMdd") && linkedObject is CusEntryHeader entry)
			{
				entry.Declaration.JE_ValuationDate = dueDateTime.Date;
			}
		}

		CADEntryStatusList CADResponseStatusNameCodeList
		{
			get
			{
				if (cadEntryStatusCodeList == null)
				{
					cadEntryStatusCodeList = new CADEntryStatusList();
				}
				return cadEntryStatusCodeList;
			}
		}
		CADEntryStatusList cadEntryStatusCodeList;

		EmailDef GetEmailAndSetOnMessage(Response response, EDIMessage message, ZString statusCode, string dynamicHtml2 = "", string dynamicHtml3 = "")
		{
			var statusDescription = CADResponseStatusNameCodeList.GetDescriptionFromCode(statusCode);
			var subject = GetEmailSubject(statusDescription).Trim();

			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.CADMessageResponse);
			emailBuilder.AddArgReplacementRange(LinkProvider.GetLink(linkedObject), linkedObjectReference, GetIssueDateString(response), StatusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, subject);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, dynamicHtml2);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml3, dynamicHtml3);
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		ZString GetEmailSubject(ZString messageType)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1} response has been received for {2}", messageType, StatusCalculator.MessageTypeDescription, linkedObjectReference);
		}

		EmailDef GetAcceptedEmailAndSetOnMessage(Response response, EDIMessage message, ZString statusCode)
		{
			var dynamicHtml2 = ZString.Empty;
			if (linkedObject is IB3MessageProcessorLinkedObject b3MessageProcessorLinkedObject && b3MessageProcessorLinkedObject.EntryReleaseDate.IsValid)
			{
				dynamicHtml2 = GetProcessingDateAndStatusCodeText(b3MessageProcessorLinkedObject.EntryReleaseDate.ToBestReadableDateTimeString(), response.Status.NameCode.Value);
			}
			return GetEmailAndSetOnMessage(response, message, statusCode, dynamicHtml2);
		}

		EmailDef GetApprovalPendingEmailAndSetOnMessage(Response response, EDIMessage message, ZString statusCode)
		{
			var dynamicHtml2 = @"Please query for an updated response in a bit to check the status of your CAD.<br />";
			var dynamicHtml3 = GetStatusCodeTextFromResponse(response);

			return GetEmailAndSetOnMessage(response, message, statusCode, dynamicHtml2, dynamicHtml3);
		}

		EmailDef GetFunctionalAcknowledgementEmailAndSetOnMessage(Response response, EDIMessage message, ZString statusCode)
		{
			return GetEmailAndSetOnMessage(response, message, statusCode, GetStatusCodeTextFromResponse(response));
		}

		EmailDef GetErrorEmailAndSetOnMessage(Response response, EDIMessage message, ZString statusCode)
		{
			return GetEmailAndSetOnMessage(response, message, statusCode, CombineProcessingDateAndStatusCodeAndErrorDetailsText(response));
		}

		string CombineProcessingDateAndStatusCodeAndErrorDetailsText(Response response)
		{
			var result = new ZStringBuilder();
			var processingDateAndStatusCodeText = GetStatusCodeTextFromResponse(response);
			var errorDetailsText = GetErrorDetailsTextFromResponse(response);
			if (!processingDateAndStatusCodeText.IsEmpty)
			{
				result.Append(processingDateAndStatusCodeText);
			}
			if (!errorDetailsText.IsEmpty)
			{
				if (!processingDateAndStatusCodeText.IsEmpty)
				{
					result.Append("<br />");
				}
				result.Append("<strong>Error Details:  </strong><br />");
				result.Append(errorDetailsText);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		ZString GetErrorDetailsTextFromResponse(Response response)
		{
			var result = ZString.Empty;
			if (response.Error.Count > 0)
			{
				if (response.Error[0].Pointer.Count > 0)
				{
					var errorDetailsTable = new HtmlTableCreator(new string[] { "Error Code", "Description", "Pointer" });
					foreach (var error in response.Error)
					{
						foreach (var pointer in error.Pointer)
						{
							errorDetailsTable.WriteRow(new string[] { error.ValidationCode.Value, error.Description.Value, pointer.Location.Value });
						}
					}
					result = errorDetailsTable.ToHtml();
				}
				else
				{
					var stringBuilder = new ZStringBuilder();
					foreach (var error in response.Error)
					{
						stringBuilder.AppendLine(error.Description.Value);
					}
					result = WebUtility.HtmlEncode(stringBuilder.ToString()).Replace("\r\n", "<br>").Replace("\n", "<br>");
				}
			}
			return result;
		}

		ZString GetIssueDateString(Response response)
		{
			var issueDateTimeString = response.IssueDateTime?.DateTimeString ?? ZString.Empty;
			if (ZDateTime.TryParseExact(issueDateTimeString, out ZDateTime issueDateTime, "yyyyMMddHHmmss"))
			{
				issueDateTimeString = issueDateTime.ToBestReadableDateTimeString();
			}
			return issueDateTimeString;
		}

		ZString GetStatusCodeTextFromResponse(Response response)
		{
			var statusCode = response.Status?.NameCode?.Value ?? CADEntryStatusList.Codes.Unknown;
			return GetProcessingDateAndStatusCodeText(ZString.Empty, statusCode);
		}

		ZString GetProcessingDateAndStatusCodeText(ZString processingDate, ZString statusCode)
		{
			var result = new ZStringBuilder();
			if (!processingDate.IsEmpty)
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, @"<strong>Processing Date:  </strong>{0}<br />", processingDate));
			}
			if (!statusCode.IsEmpty)
			{
				var description = CADResponseStatusNameCodeList.GetDescriptionFromCode(statusCode) ?? CADEntryStatusList.Descriptions.Unknown.ToString();
				result.Append(string.Format(CultureInfo.InvariantCulture, @"<strong>Status Code:  </strong>{0} {1}<br />", statusCode, description));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}
	}
}
