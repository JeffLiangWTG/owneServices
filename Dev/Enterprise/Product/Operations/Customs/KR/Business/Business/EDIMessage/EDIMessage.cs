using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class EDIMessage : Enterprise.Messaging.Business.EDIMessage, Integration.Customs.KR.IEDIMessage
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string RefundEntryNumberPlaceHolder = "<<*REFUNDNo*>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Place Holder")]
		public const string RefundEntryNumberPlaceHolderHtml = "&lt;&lt;*REFUNDNo*&gt;&gt;";
		public EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public override void OnSaving()
		{
			base.OnSaving();
			if (EM_MessageNum.IsEmpty && !IsInDatabase)
			{
				PopulateMessageNumber();
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				EM_MessageNum = ZString.Empty;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "CW1", EM_ApplicationCode).GetNextFormatted(Factory);
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;

		public ZString MessageOrEntryStatus
		{
			get
			{
				if (!messageOrEntryStatus.HasValue)
				{
					messageOrEntryStatus = CustomsReviewMessage?.EM_MessageOwner ?? MessageStatus;
				}
				return messageOrEntryStatus.Value;
			}
		}
		ZString? messageOrEntryStatus;

		public ZString MessageStatus
		{
			get
			{
				if (messageStatus.IsEmpty)
				{
					messageStatus = ZString.Empty;

					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						var incomingMessage = ResponseMessage;
						if (incomingMessage != null)
						{
							var isRejected = incomingMessage.EM_MessageType == ElectronicDocumentTypeList.Codes._R20;
							switch (incomingMessage.EM_MessageType)
							{
								case ElectronicDocumentTypeList.Codes._R20:
								case ElectronicDocumentTypeList.Codes._R99:
								case ElectronicDocumentTypeList.Codes._5AF:
								case ElectronicDocumentTypeList.Codes._R38:
									if (ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(EM_MessageType))
									{
										messageStatus = isRejected ? CustomsMessageStatusTypeList.Codes.OriginalRejected : CustomsMessageStatusTypeList.Codes.OriginalAccepted;
									}
									else if (ElectronicDocumentTypeList.IsCancellation(EM_MessageType) && (!ElectronicDocumentTypeList.IsLocalExportAmendmentOrCancellationMessage(EM_MessageType) || EM_MessageSubType == MessageSubTypeLocalExport.Cancellation))
									{
										messageStatus = isRejected ? CustomsMessageStatusTypeList.Codes.CancellationRejected : CustomsMessageStatusTypeList.Codes.CancellationAccepted;
									}
									else if (ElectronicDocumentTypeList.IsAmendment(EM_MessageType) && (!ElectronicDocumentTypeList.IsLocalExportAmendmentOrCancellationMessage(EM_MessageType) || EM_MessageSubType == MessageSubTypeLocalExport.Amendment))
									{
										messageStatus = isRejected ? CustomsMessageStatusTypeList.Codes.AmendmentRejected : CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
									}
									break;
							}
						}
					}
				}
				return messageStatus;
			}
		}
		ZString messageStatus;

		EDIMessage ResponseMessage
		{
			get
			{
				if (responseMessage == null)
				{
					var query = GetCustomsMessageQuery(new string[] { ElectronicDocumentTypeList.Codes._R20, ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5AF, ElectronicDocumentTypeList.Codes._R38 });
					responseMessage = Factory.LoadTop1<EDIMessage>(query);
				}
				return responseMessage;
			}
		}
		EDIMessage responseMessage;

		EDIMessage CustomsReviewMessage
		{
			get
			{
				if (customsReviewMessage == null)
				{
					var query = GetCustomsMessageQuery(new string[] { ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(EM_MessageType) });
					customsReviewMessage = Factory.LoadTop1<EDIMessage>(query);
				}
				return customsReviewMessage;
			}
		}
		EDIMessage customsReviewMessage;

		ZQuery GetCustomsMessageQuery(string[] messageTypes)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, EM_MessageNum);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, EM_LinkTable);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, EM_LinkUniqueID);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, messageTypes);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
			return query;
		}

		protected override string GetEntryNumber()
		{
			var result = ZString.Empty;
			if (EM_LinkTable == CusMiscRequestHeader.Schema.TableName)
			{
				var cusMiscRequestHeader = EM_LinkedObject as CusMiscRequestHeader;
				if (cusMiscRequestHeader != null)
				{
					cusMiscRequestHeader.SaveCusEntryNumber();
					result = cusMiscRequestHeader.CusEntryNumber.CE_EntryNum;
				}
			}
			else if (EM_LinkTable == CusEntryHeader.Schema.TableName)
			{
				var cusEntryHeader = EM_LinkedObject as CusEntryHeader;
				if (cusEntryHeader != null)
				{
					if (cusEntryHeader.IsLocalExport)
					{
						result = cusEntryHeader.GetEntryNumberForLEXMessage();
					}
					else if (HasRefundEntryNumberPlaceHolder)
					{
						result = cusEntryHeader.GetRefundNumber(CustomsDisbursementBill);
						if (EM_MessageType == ElectronicDocumentTypeList.Codes._5UL)
						{
							EM_MessageOwner = result;
							if (!EM_ApplicationReference.IsEmpty)
							{
								var refundSessionalData = cusEntryHeader.GetOrCreateRefundSessionalDataOriginalSendable(EM_ApplicationReference);
								if (refundSessionalData != null)
								{
									refundSessionalData.CSI_ReferenceNumber = result;
								}
							}
						}
					}
					else
					{
						result = cusEntryHeader.CusEntryNumber.CE_EntryNum;
					}
				}
			}
			return result;
		}
		ZBool HasRefundEntryNumberPlaceHolder => EM_MessageText.IndexOf(RefundEntryNumberPlaceHolder) != -1 || EM_MessageText.IndexOf(RefundEntryNumberPlaceHolderHtml) != -1;
		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			if (HasRefundEntryNumberPlaceHolder)
			{
				EM_MessageText = EM_MessageText.Replace(RefundEntryNumberPlaceHolder, GetEntryNumber());
			}

			ReplaceMessageInterpretationPlaceHolders();
		}

		void ReplaceMessageInterpretationPlaceHolders()
		{
			if (EM_MessageText.IndexOf(EntryNumberPlaceHolderHtml) != -1 || EM_MessageText.IndexOf(RefundEntryNumberPlaceHolderHtml) != -1)
			{
				EM_MessageText = EM_MessageText
					.Replace(EntryNumberPlaceHolderHtml, GetEntryNumber())
					.Replace(RefundEntryNumberPlaceHolderHtml, GetEntryNumber());
			}
		}

		public string CustomsDisbursementBill { get; set; }
		public ZBool IsDLTMessage => EM_MessageType == Constants.EDIInterchangeType.DLT;
		public CusPollingTransactionCollection CusPollingTransactions
		{
			get
			{
				if (cusPollingTransactions == null)
				{
					cusPollingTransactions = new CusPollingTransactionCollection(this);
				}
				return cusPollingTransactions;
			}
		}
		CusPollingTransactionCollection cusPollingTransactions;
		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ResourceStringData("8D3F430B-989D-467A-B0E0-DA01EAD03F1D", Caption = "Sub Type Name")]
		public override ZString EM_MessageSubTypeDescription => base.EM_MessageSubTypeDescription;

		[ResourceStringData("748F97E1-66AE-49D2-87AA-22399C4A0E77", Caption = "Direction Name")]
		public ZString EM_ReceiveTransmitDescription => Factory.GetCachedValue<ReceiveTransmitList>().GetDescriptionFromCode(EM_ReceiveTransmit);

		[ResourceStringData("BE4326E3-4F89-40A8-BEAF-4C83C17DE388", Caption = "Message Type Name")]
		public ZString EM_MessageTypeDescription => Factory.GetCachedValue<ElectronicDocumentTypeList>().GetDescriptionFromCode(EM_MessageType);

		[ResourceStringData("252A7A83-A56A-4629-9E12-E2FCCC7EEFC9", Caption = "Status Name")]
		public ZString EM_StatusDescription => Factory.GetCachedValue<EDIMessageStatusList>().GetDescriptionFromCode(EM_Status);

		protected override IStreamFormatter MessageStreamFormatter => new EDIMessageStreamFormatterForXml();
	}
}
