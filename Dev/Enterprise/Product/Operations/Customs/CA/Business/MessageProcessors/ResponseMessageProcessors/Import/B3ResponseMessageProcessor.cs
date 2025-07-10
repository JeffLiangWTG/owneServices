using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class B3ResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public B3ResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new B3ImportStatusCalculator(), MessageTypeList.Codes.B3CUSDEC, Res.GetString("9d3525ec-a8f2-463e-bbb1-4c8cb42d1a1f", "B3/B3X Response"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			base.DoPreProcessingReturningStatus(ediMessage);
			ediMessage.Factory.Saved -= Factory_Saved;
			bondedWarehouseEntryInfos = new List<BondedWarehouseEntryInfo>();
			var resultStatus = ZString.Empty;
			var cusresMessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			if (cusresMessage != null)
			{
				if (cusresMessage.Group4.Count > 0)
				{
					linkObjects = new Dictionary<ZGuid, LinkedObjectInfo>();
					for (int index = 0; index < cusresMessage.Group4.Count; index++)
					{
						GetLinkedObjectAndItsReference(cusresMessage, (EDIMessage)ediMessage, index);
					}

					for (int i = 0; i < linkObjects.Count; i++)
					{
						B3Message message = (B3Message)(i == linkObjects.Count - 1 ? ediMessage : ediMessage.Clone());
						var info = linkObjects.ElementAt(i);
						var indexes = info.Value.Group4Indexes;
						SetLinkedObjectAndItsReference(info.Value);
						linkedObject.Messages.Add(message);

						var declaration = JobDeclaration;
						if (declaration != null && declaration.SupportsBondedWarehousing
								&& (declaration.HasWHSTransaction || declaration.IsOutwardBondedWarehousingEnabled || declaration.IsInwardBondedWarehousingEnabled))
						{
							bondedWarehouseEntryInfos.Add(new BondedWarehouseEntryInfo(linkedObject as BusinessObject, message));
						}

						message.EM_MessageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);

						using (DisposableEnvironment.ForBranch(GetMessageBranchAndSetOnMessage(linkedObject, message).PK.ToGuid()))
						{
							var group4 = cusresMessage.Group4[indexes[0]];
							if (IsEntryError(cusresMessage.Group4, indexes))
							{
								ChangeStatusToError(cusresMessage, message);
							}
							else if (IsEntryAccepted(group4))
							{
								var email = GetAcceptedResponseEmailAndSetOnMessage(cusresMessage, message, GetMessageTypeDescription(ediMessage));
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								message.EM_MessageSubType = EntryStatusList.Codes.Clear;
								if (linkedObject is IB3MessageProcessorLinkedObject b3LinkedObject)
								{
									b3LinkedObject.EntryReleaseDate = message.RNSProcessingDate;
									b3LinkedObject.CancelScheduledB3Message();
									b3LinkedObject.CancelB3LateSendingWarningEvent();
									b3LinkedObject.AddDocumentsToGeneratorQueue();
								}
								SendAcknowledgementReport(EmailResponseLinkedObject, email);
							}
							else if (IsEntryConfirmed(group4))
							{
								var email = GetConfirmedResponseEmailAndSetOnMessage(cusresMessage, message, GetMessageTypeDescription(ediMessage));
								if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
								{
									linkedObject.MessageStatus = MessageStatusList.Codes.AcknowledgedOriginal;
								}
								message.EM_MessageSubType = B3EntryStatusList.Codes.Confirmed;

								if (HasI99AndI11(cusresMessage.Group4, indexes))
								{
									linkedObject.MessageStatus = MessageStatusList.Codes.ClearOriginal;
								}
								SendErrorReport(EmailResponseLinkedObject, email);
							}
							else
							{
								ChangeStatusToError(cusresMessage, message);
							}

							linkedObject.JobStatus = ((B3ImportStatusCalculator)StatusCalculator).CalculatedJobStatus(linkedObject, indexes);
							resultStatus = EDIMessage.Status.Received;
							message.EM_Status = resultStatus;
						}

						if (bondedWarehouseEntryInfos != null && bondedWarehouseEntryInfos.Any())
						{
							ediMessage.Factory.Saved -= Factory_Saved;
							ediMessage.Factory.Saved += Factory_Saved;
						}
					}

					if (linkObjects.Count == 0)
					{
						throw new CouldNotFindLinkedObjectException(linkedObjectReference, ediMessage, this);
					}
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}

			return resultStatus;
		}

		void ChangeStatusToError(CUSRESMessage cusresMessage, B3Message message)
		{
			var email = GetErrorResponseEmailAndSetOnMessage(cusresMessage, message, GetMessageTypeDescription(message));
			if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
			{
				linkedObject.MessageStatus = StatusCalculator.GetMessageRejectedStatus(message);
			}
			message.EM_MessageSubType = EntryStatusList.Codes.Error;
			SendErrorReport(EmailResponseLinkedObject, email);
		}

		#region Implementation

		ZString GetMessageTypeDescription(Enterprise.Messaging.Business.EDIMessage message)
		{
			return message.EM_MessageType == MessageTypeList.Codes.XTypeEntry ? MessageTypeList.Descriptions.XTypeEntry : ZString.Empty;
		}

		#region SetLinkedObjectAndItsReference

		Dictionary<ZGuid, LinkedObjectInfo> linkObjects;

		class LinkedObjectInfo
		{
			public IEDIFACTMessageAttachee LinkedObject;
			public ZString LinkedObjectReference;
			public List<int> Group4Indexes;

			public LinkedObjectInfo(IEDIFACTMessageAttachee linkedObject, ZString linkedObjectReference)
			{
				LinkedObject = linkedObject;
				LinkedObjectReference = linkedObjectReference;
				Group4Indexes = new List<int>();
			}
		}

		void GetLinkedObjectAndItsReference(CUSRESMessage cusresMessage, EDIMessage message, int index)
		{
			IEDIFACTMessageAttachee messageAttachee = null;

			linkedObjectReference = GetLinkedObjectReference(cusresMessage, message, index);

			if (!linkedObjectReference.IsEmpty)
			{
				var matchingDeclarations = ImportLinkedObjectManager.LoadDeclarationsWithCusEntryHeaderReference(message.Factory, linkedObjectReference, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration }).ToList();

				if (!matchingDeclarations.Any())
				{
					var decfromCusEntryNum = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(message.Factory, linkedObjectReference, ZString.Empty);
					if (decfromCusEntryNum != null)
					{
						matchingDeclarations.Add(decfromCusEntryNum);
					}
				}

				var declarationCount = matchingDeclarations.Count;
				if (declarationCount == 1)
				{
					var dec = matchingDeclarations.First();
					if (dec.IsB3X)
					{
						messageAttachee = dec;
						message.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
					}
					else
					{
						messageAttachee = dec.B3EntryHeader;
					}
				}
				else if (declarationCount > 1)
				{
					var b3EntryHeaders = matchingDeclarations.Select(x => x.B3EntryHeader).Where(x => x.Messages.Cast<EDIMessage>().Any(msg => msg.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && msg.BatchNumber == message.BatchNumber));
					var countOfB3EntryHeaders = b3EntryHeaders.Count();
					if (countOfB3EntryHeaders == 1)
					{
						messageAttachee = b3EntryHeaders.First();
					}
					else if (countOfB3EntryHeaders > 1)
					{
						var entryWaitingResponse = b3EntryHeaders.Where(x => StatusCalculator.IsAwaitingReply(x.CH_Status));
						countOfB3EntryHeaders = entryWaitingResponse.Count();
						if (countOfB3EntryHeaders == 1)
						{
							messageAttachee = entryWaitingResponse.First();
						}
						else if (countOfB3EntryHeaders > 1)
						{
							Logger.LogWarning("There are more than one job which are waiting for response with the same transaction sequence number, the response message will not be linked to any of them.");
							throw new CouldNotFindLinkedObjectException(linkedObjectReference, message, this);
						}
					}
				}
			}

			if (messageAttachee != null)
			{
				LinkedObjectInfo info = null;
				var pk = ((BusinessObject)messageAttachee).PK;
				if (linkObjects.ContainsKey(pk))
				{
					info = linkObjects[pk];
				}
				else
				{
					info = new LinkedObjectInfo(messageAttachee, linkedObjectReference);
					linkObjects.Add(pk, info);
				}
				info.Group4Indexes.Add(index);
			}
		}

		ZString GetLinkedObjectReference(CUSRESMessage cusresMessage, EDIMessage message, int index)
		{
			var result = ZString.Empty;
			var transactionNumber = GetTransactionNumber(cusresMessage, index);
			if (string.IsNullOrEmpty(transactionNumber))
			{
				result = GetReferenceByBatchNumber(cusresMessage, message, index);
			}
			else
			{
				var accountSecurityNo = cusresMessage.Group3.Count > 0 && cusresMessage.Group3[0].RFF.Count > 0
					? cusresMessage.Group3[0].RFF[0].Reference.ReferenceIdentifier.PadLeft(5, '0') : string.Empty;
				result = accountSecurityNo + transactionNumber;
			}
			return result;
		}

		void SetLinkedObjectAndItsReference(LinkedObjectInfo info)
		{
			linkedObject = info.LinkedObject;
			linkedObjectReference = info.LinkedObjectReference;
		}

		static string GetTransactionNumber(CUSRESMessage cusresMessage, int index)
		{
			var transactionNumberQualifiers =
				new[]
					{
						B3EntryComponentTypes.Codes.I11,
						B3EntryComponentTypes.Codes.I16,
						B3EntryComponentTypes.Codes.I71,
						B3EntryComponentTypes.Codes.I91,
						B3EntryComponentTypes.Codes.I99
					};

			return (from SegmentGroup4 group4 in (from SegmentGroup4 group4 in cusresMessage.Group4 select group4).Skip(index)
					where group4.ERP.Count > 0 && group4.RFF.Count > 0
							&& transactionNumberQualifiers.Contains(group4.ERP[0].ErrorPointDetails.MessageItemNumber)
					select group4.RFF[0].Reference.ReferenceIdentifier.PadLeft(9, '0')).FirstOrDefault();
		}

		ZString GetReferenceByBatchNumber(CUSRESMessage cusresMessage, EDIMessage message, int index)
		{
			var batchNumber = (from SegmentGroup4 group4 in (from SegmentGroup4 group4 in cusresMessage.Group4 select group4).Skip(index)
							   where group4.ERP.Count > 0 && group4.RFF.Count > 0
										   && group4.ERP[0].ErrorPointDetails.MessageItemNumber == B3EntryComponentTypes.Codes.X11
							   select group4.RFF[0].Reference.ReferenceIdentifier).FirstOrDefault();
			if (!string.IsNullOrEmpty(batchNumber))
			{
				var query = new ZDBOnlyQuery(typeof(EDIMessage));
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAIMP);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.B3CUSDEC);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, message.EM_SystemCreateTimeUtc);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddDays(-2));
				query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				var bestMatchMessage = (from EDIMessage sentMessage in message.Factory.Load<EDIMessage>(query)
										where sentMessage.BatchNumber == batchNumber
										select sentMessage).FirstOrDefault();
				if (bestMatchMessage != null)
				{
					return bestMatchMessage.TransactionNumber;
				}
			}
			return string.Empty;
		}

		#endregion

		#region GetAcceptedResponseEmailAndSetOnMessage

		protected override string GetAcceptedMessageText(SegmentGroup cusresMessage, EDIMessage message)
		{
			var cusres = (CUSRESMessage)cusresMessage;
			ZDateTime processingDate;
			if (cusres.DTM.Count > 0 && ZDateTime.TryParseExact(cusres.DTM[0].DateTimePeriod.DateTimePeriodValue, out processingDate, "yyyyMMdd"))
			{
				return Res.GetString("045c16df-6d7f-452b-82f6-bffba26cc26b", "<strong>Processing Date:  </strong>{0}<br />", processingDate.ToShortDateString());
			}
			return string.Empty;
		}

		#endregion

		#region GetErrorResponseEmailAndSetOnMessage

		CAErrorCodesDescriptionHelper ErrorDescriptionHelper
		{
			get
			{
				if (errorDescriptionHelper == null)
				{
					errorDescriptionHelper = new CAErrorCodesDescriptionHelper();
				}
				return errorDescriptionHelper;
			}
		}
		CAErrorCodesDescriptionHelper errorDescriptionHelper;

		protected override string GetErrorMessageText1(SegmentGroup cusresMessage, EDIMessage message)
		{
			HtmlTableCreator errorTable = null;
			var entryComponentTypes = new B3EntryComponentTypes();
			errorTable = new HtmlTableCreator(new[] { Res.GetString("014a39b0-0f98-4568-a22f-49ca8ef2ad00", "Error Code"), Res.GetString("17e67fbc-1384-4391-bd19-384a64ea3c21", "Error Message"), Res.GetString("80b28330-0f69-4e1a-a743-dbc24ff4bfce", "Entry Component"), Res.GetString("e175e007-c1dc-41cf-8c6c-6c8be29c7e7e", "Reference") });

			var indexes = linkObjects[((BusinessObject)linkedObject).PK].Group4Indexes;
			foreach (var index in indexes)
			{
				var group4 = ((CUSRESMessage)cusresMessage).Group4[index];
				var componentType = group4.ERP[0].ErrorPointDetails.MessageItemNumber;
				var entryComponent = string.Format("{0} - {1}", componentType, entryComponentTypes.GetDescriptionFromCode(componentType));
				var reference = string.Format("{0}: '{1}'", GetReferenceName(componentType), group4.RFF[0].Reference.ReferenceIdentifier);
				var errorCode = group4.ERC[0].ApplicationErrorDetail.ApplicationErrorIdentification;

				errorTable.WriteRow(errorCode, ErrorDescriptionHelper.GetDescriptionFromMsgNoCode(message.Factory, errorCode), entryComponent, reference);
			}
			return errorTable != null ? errorTable.ToHtml() : string.Empty;
		}

		static string GetReferenceName(string componentType)
		{
			switch (componentType)
			{
				case B3EntryComponentTypes.Codes.X11:
					return Res.GetString("8d559c1e-4451-408a-b0ab-63c0539e650a", "Batch Number");
				case B3EntryComponentTypes.Codes.I11:
				case B3EntryComponentTypes.Codes.I16:
				case B3EntryComponentTypes.Codes.I71:
				case B3EntryComponentTypes.Codes.I91:
				case B3EntryComponentTypes.Codes.I99:
					return Res.GetString("d5c5659e-41cd-409c-81ef-bd2e8444fcbe", "Transaction Number");
				case B3EntryComponentTypes.Codes.I21:
					return Res.GetString("4e9607f4-cedd-4f78-83ef-09b628882c46", "Sub-Header Number");
				case B3EntryComponentTypes.Codes.I31:
				case B3EntryComponentTypes.Codes.I41:
				case B3EntryComponentTypes.Codes.I51:
				case B3EntryComponentTypes.Codes.I61:
				case B3EntryComponentTypes.Codes.I66:
					return Res.GetString("c870de0d-39cb-4c6f-b7e0-586aa62d2856", "Line Number");
				default:
					return Res.GetString("113a1bc6-7502-436b-b4d0-2594032564be", "Unknown");
			}
		}

		#endregion

		#region GetConfirmedResponseEmailAndSetOnMessage
		protected EmailDef GetConfirmedResponseEmailAndSetOnMessage(SegmentGroup cusresMessage, EDIMessage message, string messageTypeDescription = "")
		{
			if (string.IsNullOrEmpty(messageTypeDescription))
			{
				messageTypeDescription = StatusCalculator.MessageTypeDescription;
			}

			var subject = Res.GetString("77ab2747-8e6e-4f59-8f58-494fbbeeb7b2", "Confirmed {0} Response for {1}", messageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_MessageText.Replace("'", "\r\n"), EmailDefBuilder.HtmlTemplates.ConfirmedResponse);
			emailBuilder.AddArgReplacementRange(LinkProvider.GetLink(linkedObject), linkedObjectReference, messageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetErrorMessageText1(cusresMessage, message));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, GetErrorMessageText2(cusresMessage, message));
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		#endregion

		public const string EntryAccepted = "942992";
		public const string EntryConfirmed = "942861";
		public const string EntryError = "942991";

		internal static bool IsEntryAccepted(SegmentGroup4 group4)
		{
			return group4.ERP.Count > 0 && group4.ERP[0].ErrorPointDetails.MessageItemNumber == B3EntryComponentTypes.Codes.I99
				&& group4.ERC.Count > 0 && group4.ERC[0].ApplicationErrorDetail.ApplicationErrorIdentification == B3ResponseMessageProcessor.EntryAccepted;
		}

		internal static bool IsEntryConfirmed(SegmentGroup4 group4)
		{
			return group4.ERC.Cast<ERCSegment>().Any(erc => erc.ApplicationErrorDetail.ApplicationErrorIdentification == EntryConfirmed);
		}

		static bool HasGroup4ErrorNumberAndID(SegmentGroup4 group4, string entryComponentType, string errorIdentification)
		{
			return group4.ERP.Cast<ERPSegment>().Any(erp => erp.ErrorPointDetails.MessageItemNumber == entryComponentType)
					   && group4.ERC.Cast<ERCSegment>().Any(erc => erc.ApplicationErrorDetail.ApplicationErrorIdentification == errorIdentification);
		}

		internal static bool IsEntryError(SegmentGroup4MessageSection segmentGroup4, List<int> group4Indexes)
		{
			if (group4Indexes == null)
			{
				group4Indexes = new List<int>();
			}
			if (group4Indexes.Count == 0)
			{
				for (var index = 0; index < segmentGroup4.Count; index++)
				{
					group4Indexes.Add(index);
				}
			}

			return HasI99(segmentGroup4, group4Indexes) && !HasI99AndI11(segmentGroup4, group4Indexes);
		}

		static bool HasI99(SegmentGroup4MessageSection segmentGroup4, List<int> group4Indexes)
		{
			foreach (var index in group4Indexes)
			{
				SegmentGroup4 group4 = segmentGroup4[index];
				if (HasI99(group4))
				{
					return true;
				}
			}
			return false;
		}

		static bool HasI99(SegmentGroup4 group4)
		{
			return group4.ERP.Count > 0 && group4.ERP[0].ErrorPointDetails.MessageItemNumber == B3EntryComponentTypes.Codes.I99
					   && group4.ERC.Count > 0 && group4.ERC[0].ApplicationErrorDetail.ApplicationErrorIdentification == B3ResponseMessageProcessor.EntryError;
		}

		static bool HasI99AndI11(SegmentGroup4MessageSection segmentGroup4, List<int> group4Indexs)
		{
			var hasI11 = false;
			var hasErrI99 = false;
			foreach (var index in group4Indexs)
			{
				SegmentGroup4 group4 = segmentGroup4[index];

				if (HasGroup4ErrorNumberAndID(group4, B3EntryComponentTypes.Codes.I99, EntryError))
				{
					hasErrI99 = true;
				}
				else if (HasGroup4ErrorNumberAndID(group4, B3EntryComponentTypes.Codes.I11, EntryConfirmed))
				{
					hasI11 = true;
				}

				if (hasErrI99 && hasI11)
				{
					return true;
				}
			}
			return false;
		}

		JobDeclaration JobDeclaration
		{
			get { return linkedObject?.TopLevelBusinessObject as JobDeclaration; }
		}

		#region Bonded Warehouse

		List<BondedWarehouseEntryInfo> bondedWarehouseEntryInfos;

		class BondedWarehouseEntryInfo
		{
			public BusinessObject LinkedObject;
			public EDIMessage Message;
			public EmailDef EmailReportThatHasBeenDelayed;

			public BondedWarehouseEntryInfo(BusinessObject linkedObject, EDIMessage message)
			{
				LinkedObject = linkedObject;
				Message = message;
			}
		}

		protected override void SendReport(EmailDef email)
		{
			var info = bondedWarehouseEntryInfos?.FirstOrDefault(i => i.LinkedObject == linkedObject);
			if (info != null && info.EmailReportThatHasBeenDelayed == null)
			{
				info.EmailReportThatHasBeenDelayed = email;
			}
			else
			{
				base.SendReport(email);
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
			}
			if (bondedWarehouseEntryInfos != null)
			{
				foreach (var info in bondedWarehouseEntryInfos)
				{
					if (info.LinkedObject != null && info.Message != null)
					{
						new BondedWarehouseMessageProcessor(info.Message.PK, info.EmailReportThatHasBeenDelayed, SendEmail).ProcessAfterSaved(savedSuccessfully);
					}
				}
			}
			bondedWarehouseEntryInfos = null;
		}

		void SendEmail(EmailDef email)
		{
			base.SendReport(email);
		}

		#endregion

		#endregion
	}
}
