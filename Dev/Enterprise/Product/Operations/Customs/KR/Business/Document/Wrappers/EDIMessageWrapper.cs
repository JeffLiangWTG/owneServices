using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;

namespace Enterprise.Customs.KR.Business
{
	public class EDIMessageWrapper : DocumentWrapper
	{
		public EDIMessageWrapper(EDIMessage message) : base(message, message?.Factory)
		{
			if (ReferenceEquals(message, null))
			{
				throw new ArgumentNullException(nameof(message), "Message");
			}

			this.message = message;
		}

		readonly EDIMessage message;
		[ResourceStringData("BC142F2E-52DE-435E-A1FD-63FE1D082262", Caption = "Version No.")]
		public ZString ApplicationReference => message.EM_ApplicationReference;
		[ResourceStringData("6ae7eda6-9b85-44d1-b012-624fbb78daa3", Caption = "Message Status")]
		public ZString MessageStatus => message.MessageOrEntryStatus;
		[ResourceStringData("0f02b9c5-a9bc-4298-bd4c-1a49bb2eaf06", Caption = "Message Status Desc.")]
		public ZString MessageStatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageStatus) ?? Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(MessageStatus);

		public CusEntryHeader Entry => message.EM_LinkedObject as CusEntryHeader;

		public OrgHeaderWrapper Payer => payer ?? (payer = OrgHeaderWrapper.New(Entry?.Declaration?.DutyPayer));
		OrgHeaderWrapper payer;

		public ZDateTime SentDateTimeInLocalTimeZone
		{
			get
			{
				return Env.Time.GetLocalTimeFromUtc(message.EM_SystemCreateTimeUtc.ToDateTime());
			}
		}

		public EarlyReleaseMiscMessageSendingObject MessageSendingObject5BD
		{
			get
			{
				if (messageSendingObject5BD == null)
				{
					messageSendingObject5BD = new EarlyReleaseMiscMessageSendingObject(Entry);
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5BD)
					{
						messageSendingObject5BD.Populate(message);
					}
				}
				return messageSendingObject5BD;
			}
		}
		EarlyReleaseMiscMessageSendingObject messageSendingObject5BD;

		public ExtendReExportDateMessageSendingObject MessageSendingObjectD72
		{
			get
			{
				if (messageSendingObjectD72 == null)
				{
					if (message.EM_LinkedObject is CusEntryHeader entry)
					{
						messageSendingObjectD72 = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
						if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._D72)
						{
							messageSendingObjectD72.PopulateD72(message);
						}
					}
				}
				return messageSendingObjectD72;
			}
		}
		ExtendReExportDateMessageSendingObject messageSendingObjectD72;

		public CancellationMessageSendingObject MessageSendingObject5BF
		{
			get
			{
				if (messageSendingObject5BF == null)
				{
					messageSendingObject5BF = new CancellationMessageSendingObject(Entry);
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5BF)
					{
						messageSendingObject5BF.Populate(message);
					}
				}
				return messageSendingObject5BF;
			}
		}
		CancellationMessageSendingObject messageSendingObject5BF;

		public GOVCBR5BEMessageData MessageData5BE
		{
			get
			{
				if (messageData5BE == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5BE)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5BE = new GOVCBR5BEDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5BE;
			}
		}
		GOVCBR5BEMessageData messageData5BE;

		public GOVCBR5GVMessageData MessageData5GV
		{
			get
			{
				if (messageData5GV == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5GV)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5GV = new GOVCBR5GVDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5GV;
			}
		}
		GOVCBR5GVMessageData messageData5GV;

		public GOVCBR5UBMessageData MessageData5UB
		{
			get
			{
				if (messageData5UB == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5UB)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5UB = new GOVCBR5UBDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5UB;
			}
		}
		GOVCBR5UBMessageData messageData5UB;

		public GOVCBR5TWMessageData MessageData5TW
		{
			get
			{
				if (messageData5TW == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5TW)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5TW = new GOVCBR5TWDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5TW;
			}
		}
		GOVCBR5TWMessageData messageData5TW;

		public GOVCBR5TVMessageData MessageData5TV
		{
			get
			{
				if (messageData5TV == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5TV)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5TV = new GOVCBR5TVDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5TV;
			}
		}
		GOVCBR5TVMessageData messageData5TV;

		public GOVCBR5UOMessageData MessageData5UO
		{
			get
			{
				if (messageData5UO == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5UO)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5UO = new GOVCBR5UODataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5UO;
			}
		}
		GOVCBR5UOMessageData messageData5UO;

		public GOVCBR5WNMessageData MessageData5WN
		{
			get
			{
				if (messageData5WN == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5WN)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5WN = new GOVCBR5WNDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5WN;
			}
		}
		GOVCBR5WNMessageData messageData5WN;

		public GOVCBR5GUMessageData MessageData5GU
		{
			get
			{
				if (messageData5GU == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5GU)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5GU = new GOVCBR5GUDataProvider().GetMessageData(Factory, reader, message);
						}
					}
				}
				return messageData5GU;
			}
		}
		GOVCBR5GUMessageData messageData5GU;

		public GOVCBR5FVMessageData MessageData5FV
		{
			get
			{
				if (messageData5FV == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5FV)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5FV = new GOVCBR5FVDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5FV;
			}
		}
		GOVCBR5FVMessageData messageData5FV;

		public FinalPriceReportByDateExtensionHeader MessageData5SG
		{
			get
			{
				if (messageData5SG == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5SG)
					{
						messageData5SG = new FinalPriceReportByDateExtensionHeader(Factory);
						messageData5SG.PopulateFromMessage(message);
					}
				}
				return messageData5SG;
			}
		}
		FinalPriceReportByDateExtensionHeader messageData5SG;

		public ImportD72Details GOVCBRD72Message
		{
			get
			{
				if (govcbrD72Message == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._D72)
					{
						govcbrD72Message  = new ImportD72Details(Factory, message);
					}
				}
				return govcbrD72Message;
			}
		}
		ImportD72Details govcbrD72Message;

		public Import5BBDetails GOVCBR5BBMessage
		{
			get
			{
				if (govcbr5BBMessage == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5BB)
					{
						govcbr5BBMessage = new Import5BBDetails(message);
					}
				}
				return govcbr5BBMessage;
			}
		}
		Import5BBDetails govcbr5BBMessage;

		public ExemptionRequestOfPenalty MessageData5UA
		{
			get
			{
				if (messageData5UA == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA)
					{
						messageData5UA = new ExemptionRequestOfPenalty(message);
					}
				}
				return messageData5UA;
			}
		}
		ExemptionRequestOfPenalty messageData5UA;

		public RefundRequest MessageData5UL
		{
			get
			{
				if (messageData5UL == null)
				{
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._5UL)
					{
						messageData5UL = new RefundRequest(message);
					}
				}
				return messageData5UL;
			}
		}
		RefundRequest messageData5UL;
	}
}
