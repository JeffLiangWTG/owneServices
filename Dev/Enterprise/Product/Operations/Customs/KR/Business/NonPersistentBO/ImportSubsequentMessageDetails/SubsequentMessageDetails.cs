using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class SubsequentMessageDetails : NonPersistentBusinessObject
	{
		public SubsequentMessageDetails(CusEntryHeader entry) : base(entry.Factory)
		{
			this.entry = entry;
		}
		readonly CusEntryHeader entry;

		#region FTA
		CusEntryNumber FTAEntryNum
		{
			get { return ftaEntryNum ?? (ftaEntryNum = GetEntryNumber(entry.GetOriginalFTAType())); }
		}
		CusEntryNumber ftaEntryNum;

		[List(nameof(CusEntryHeaderLookups) + "." + nameof(KR.Business.CusEntryHeaderLookups.MessageStatusList))]
		[ResourceStringData("04563D75-4067-428D-9A31-24639B0A6FE1", Caption = "Message Status")]
		public ZString FTAMessageStatus => FTAEntryNum?.CE_EntryStatus ?? ZString.Empty;
		[ResourceStringData("DB605684-9C8D-4B66-8400-B62C5C55DD4F", Caption = "Accepted Date")]
		public ZDateTime FTAAcceptedDate => FTAEntryNum?.CE_IssueDate ?? ZDateTime.Empty;
		[ResourceStringData("26B754A3-08C6-4271-BE2C-E0A2720C4CF7", Caption = "Law Code Description", ShortCaption = "Law Code Desc.")]
		public ZString LawCodeDescription => Factory.GetCachedValue<FTALawCodeList>().GetDescriptionFromCode(entry.EntryInstruction?.CEI_FTARelationArticleCode ?? ZString.Empty);

		public FTAAmendmentDetailsCollection FTAAmendments
		{
			get
			{
				if (ftaAmendments == null)
				{
					ftaAmendments = new FTAAmendmentDetailsCollection(entry);
				}
				return ftaAmendments;
			}
		}
		FTAAmendmentDetailsCollection ftaAmendments;
		#endregion
		#region 934
		CusEntryNumber EntryNum934
		{
			get { return entryNum934 ?? (entryNum934 = GetEntryNumber(ElectronicDocumentTypeList.Codes._934)); }
		}
		CusEntryNumber entryNum934;
		[ResourceStringData("8A7956E7-92B9-4DBA-BBC7-0E7F53C83C19", Caption = "Message Status")]
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.MessageStatusList))]
		public ZString MessageStatus934 => EntryNum934?.CE_EntryStatus ?? ZString.Empty;
		[ResourceStringData("F957E6CB-0A6E-4BCB-BCF8-6166ABFB2CD4", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate934 => EntryNum934?.CE_IssueDate ?? ZDateTime.Empty;
		[ResourceStringData("CAB17046-032A-43EA-9F76-AE95A027EA8C", Caption = "Estimated Date Of Final Price")]
		public ZDateTime EstimatedDateOfFinalPrice => EntryNum934?.CE_ExpiryDate ?? ZDateTime.Empty;
		[ResourceStringData("38CA6EE8-D684-4300-8222-FDBF2E643069", Caption = "Provisional Valuation Dec. No.")]
		public ZString NoticeNumber934 => MessageFunctions.GetFormattedNumber(EntryNum934?.CE_EntryNum ?? ZString.Empty, new int[] { 0, 3, 5, 7 });
		[ResourceStringData("0DF5F16F-6C5F-4019-8194-105B48E77907", Caption = "Valuation Method")]
		[List(nameof(JobComInvoiceHeaderLookups) + "." + nameof(Business.JobComInvoiceHeaderLookups.ValuationCodeList))]
		public ZString ValuationMethod => entry.RandomHeader.JZ_ValuationCode;
		#endregion
		#region 5BD
		CusEntryNumber EntryNum5BD
		{
			get { return entryNum5BD ?? (entryNum5BD = GetEntryNumber(ElectronicDocumentTypeList.Codes._5BD)); }
		}
		CusEntryNumber entryNum5BD;
		[ResourceStringData("E2DE05A6-E5BD-4B82-8B9B-733D4D43832B", Caption = "Message Status")]
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.MessageStatusList))]
		public ZString MessageStatus5BD => EntryNum5BD?.CE_EntryStatus ?? ZString.Empty;
		[ResourceStringData("7658AAE1-FCAE-4B37-8B6A-67D9B956BD3B", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate5BD => EntryNum5BD?.CE_IssueDate ?? ZDateTime.Empty;
		[ResourceStringData("6FA87BFB-11AA-4AC7-B4D5-67E9BC48C178", Caption = "Review Date")]
		public ZDateTime ReviewDate5BD => MessageWrapper5BE?.NoticeDateTime ?? ZDateTime.Empty;

		public EDIMessage Message5BD => message5BD ?? (message5BD = entry.GetAcceptedOutgoingMessage(ElectronicDocumentTypeList.Codes._5BD));
		EDIMessage message5BD;

		public EDIMessage Message5BE => message5BE ?? (message5BE = FindIncomingMessage(ElectronicDocumentTypeList.Codes._5BE, ElectronicDocumentTypeList.Codes._5BD, Message5BD?.EM_MessageNum));
		EDIMessage message5BE;

		GOVCBR5BEMessageData MessageWrapper5BE
		{
			get
			{
				if (Message5BE != null && messageWrapper5BE == null)
				{
					messageWrapper5BE = new EDIMessageWrapper(Message5BE).MessageData5BE;
				}
				return messageWrapper5BE;
			}
		}
		GOVCBR5BEMessageData messageWrapper5BE;

		[ResourceStringData("ED97E12F-7B96-40CD-A2E4-E76252902B14", Caption = "Review Result")]
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.CH_EntryStatusList))]
		public ZString ReviewResult5BD => Message5BE?.EM_MessageOwner ?? ZString.Empty;
		[ResourceStringData("B65AA72A-6B98-4C03-9874-A20CE5C34B3A", Caption = "Request Reason")]
		public ZString RequestReason => Declaration5BD?.Reason.Value ?? ZString.Empty;
		[ResourceStringData("C831A6AC-115D-4D29-8AC5-70C0219E23A6", Caption = "Security Type")]
		[List(nameof(EarlyReleaseMiscMessageSendingObjectLookups) + "." + nameof(Business.EarlyReleaseMiscMessageSendingObjectLookups.SecurityTypeList))]
		public ZString SecurityType => Declaration5BD?.ObligationGuarantee.SecurityDetailsCode.Value ?? ZString.Empty;
		[ResourceStringData("89BA5CF3-8C29-4045-9A73-086B341759F7", Caption = "Security Period")]
		public ZDateTime SecurityPeriodStartDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var date = Declaration5BD?.ObligationGuarantee.SecurityEffectiveDateTime;
				if (DateTime.TryParseExact(date, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					result = dt;
				}
				return result;
			}
		}
		public ZDateTime SecurityPeriodEndDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var date = Declaration5BD?.ObligationGuarantee.LpcoExpirationDateTime;
				if (DateTime.TryParseExact(date, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					result = dt;
				}
				return result;
			}
		}
		[ResourceStringData("7CC21E40-70AD-44A6-B626-7DEC33E7BDE5", Caption = "Security Amount")]
		public ZDecimal SecurityAmount => Declaration5BD?.ObligationGuarantee.SecurityAmount.Value ?? 0m;
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BD.Declaration Declaration5BD
		{
			get
			{
				if (declaration5BD == null && Message5BD != null)
				{
					using (var reader = Message5BD.GetEM_MessageTextReader())
					{
						declaration5BD = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BD.Declaration>(reader);
					}
				}
				return declaration5BD;
			}
		}
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BD.Declaration declaration5BD;
		#endregion
		#region 5BF
		[ResourceStringData("0429731B-BEF5-4CBC-940C-13C5C5A148CD", Caption = "Message Status")]
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.MessageStatusList))]
		public ZString MessageStatus5BF => Message5BF == null ? ZString.Empty : entry.CH_Status;

		public EDIMessage Message5BF => message5BF ?? (message5BF = entry.GetAcceptedOutgoingMessage(ElectronicDocumentTypeList.Codes._5BF));
		EDIMessage message5BF;

		[ResourceStringData("652FF12B-C8D9-406D-A555-07990342A1EC", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate5BF
		{
			get
			{
				var messageR99 = entry.GetLatestR99Message(ElectronicDocumentTypeList.Codes._5BF);
				return messageR99?.EM_SystemCreateTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
			}
		}
		[ResourceStringData("DF94B7F2-4CF2-4197-B942-7EE38AD81D17", Caption = "Approval Date")]
		public ZDateTime ApprovalDate5BF => ImportEntryNum?.CE_ExpiryDate ?? ZDateTime.Empty;
		[ResourceStringData("B39E740E-9B54-4CF9-B9AA-17B73C84D1BF", Caption = "Cancellation Reason")]
		public ZString CancellationReason => Declaration5BF?.Reason.Value ?? ZString.Empty;
		[ResourceStringData("EF696977-6703-4C67-8783-23538704B1D4", Caption = "Review Result")]
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.CH_EntryStatusList))]
		public ZString ReviewResult5BF
		{
			get
			{
				var message5BG = FindIncomingMessage(ElectronicDocumentTypeList.Codes._5BG, ElectronicDocumentTypeList.Codes._5BF, Message5BF?.EM_MessageNum);
				return message5BG?.EM_MessageOwner ?? ZString.Empty;
			}
		}
		[ResourceStringData("3B6D246B-E501-46E9-AFFA-7A0C2F7C9BAD", Caption = "Review Date")]
		public ZDateTime ReviewDate5BF => GetEntryNumber(KRJobMessageTypeList.Codes.Import)?.CE_ExpiryDate ?? ZDateTime.Empty;
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BF.Declaration Declaration5BF
		{
			get
			{
				if (declaration5BF == null)
				{
					var message = GetLastOutgoingMessage(ElectronicDocumentTypeList.Codes._5BF);
					if (message != null)
					{
						using var reader = message.GetEM_MessageTextReader();
						declaration5BF = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BF.Declaration>(reader);
					}
				}
				return declaration5BF;
			}
		}
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BF.Declaration declaration5BF;
		#endregion
		#region 5TM
		CusEntryNumber EntryNum5TM
		{
			get { return entryNum5TM ?? (entryNum5TM = GetEntryNumber(ElectronicDocumentTypeList.Codes._5TM)); }
		}
		CusEntryNumber entryNum5TM;
		[ResourceStringData("C3650079-1BE4-48F5-A1FA-B97526F1B729", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate5TM => EntryNum5TM?.CE_IssueDate ?? ZDateTime.Empty;
		[ResourceStringData("390CF231-E834-4620-8F79-DA6D8BDC8FD1", Caption = "Message Status")]
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.MessageStatusList))]
		public ZString MessageStatus5TM => EntryNum5TM?.CE_EntryStatus ?? ZString.Empty;
		#endregion

		#region 5FN
		public MessageSendingEntryLineObjectCollection GOVCBR5FNMessages
		{
			get
			{
				if (govcbr5FNMessages == null)
				{
					govcbr5FNMessages = new MessageSendingEntryLineObjectCollection(entry);
					govcbr5FNMessages.PopulateElementsFrom5FNMessages(null);
				}
				return govcbr5FNMessages;
			}
		}
		MessageSendingEntryLineObjectCollection govcbr5FNMessages;
		#endregion
		#region D72
		public EDIMessageWrapperCollection GOVCBRD72Messages
		{
			get
			{
				if (govcbrD72Messages == null)
				{
					var messages = new List<EDIMessage>();
					var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._D72);
					if (entryNum != null)
					{
						var entryLineReference = ZInt.ParseSafe(entryNum.CE_EntryLineReference, 0);
						var orderedMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._D72).OrderByDescending(x => x.EM_SystemCreateTimeUtc);
						for (var i = 1; i <= entryLineReference + 1; i++)
						{
							var lastMessage = orderedMessages.FirstOrDefault(x => x.EM_ApplicationReference == i.ToString());
							if (lastMessage != null)
							{
								messages.Add(lastMessage);
							}
						}
					}
					govcbrD72Messages = new EDIMessageWrapperCollection(messages, Factory);
				}
				return govcbrD72Messages;
			}
		}
		EDIMessageWrapperCollection govcbrD72Messages;
		#endregion
		#region 5BA, 5BB
		CusEntryNumber EntryNum5BA
		{
			get { return entryNum5BA ?? (entryNum5BA = GetEntryNumber(ElectronicDocumentTypeList.Codes._5BA)); }
		}
		CusEntryNumber entryNum5BA;
		[List(nameof(CusEntryHeaderLookups) + "." + nameof(Business.CusEntryHeaderLookups.MessageStatusList))]
		[ResourceStringData("A5315293-1A68-4C67-81B4-A2DAC35BD3E5", Caption = "Message Status")]
		public ZString MessageStatus5BA => EntryNum5BA?.CE_EntryStatus ?? ZString.Empty;
		[ResourceStringData("5549ADD7-F4DA-4202-8604-C0DDC840D181", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate5BA => EntryNum5BA?.CE_IssueDate ?? ZDateTime.Empty;
		public EDIMessageWrapperCollection GOVCBR5BBMessages
		{
			get
			{
				if (govcbr5BBMessages == null)
				{
					var messages = new List<EDIMessage>();
					if (EntryNum5BA != null)
					{
						var entryLineReference = ZInt.ParseSafe(EntryNum5BA.CE_EntryLineReference, 0);
						var orderedMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5BB).OrderByDescending(x => x.EM_SystemCreateTimeUtc);
						for (var i = 1; i <= entryLineReference + 1; i++)
						{
							var lastMessage = orderedMessages.FirstOrDefault(x => x.EM_ApplicationReference == i.ToString());
							if (lastMessage != null)
							{
								messages.Add(lastMessage);
							}
						}
					}
					govcbr5BBMessages = new EDIMessageWrapperCollection(messages, Factory);
				}
				return govcbr5BBMessages;
			}
		}
		EDIMessageWrapperCollection govcbr5BBMessages;
		#endregion
		#region 5UA
		
		public EDIMessageWrapperCollection GOVCBR5UAMessages
		{
			get
			{
				if (govcbr5UAMessages == null)
				{
					var messages = new List<EDIMessage>();
					var entryNum5UAWithMaxVersionNo = entry.EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA).OrderByDescending(x => x.CE_EntryLineReference).FirstOrDefault();

					if (entryNum5UAWithMaxVersionNo != null)
					{
						var orderedMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA).OrderByDescending(x => x.EM_SystemCreateTimeUtc);

						for (var i = 1; i <= ZInt.ParseSafe(entryNum5UAWithMaxVersionNo.CE_EntryLineReference, 0); i++)
						{
							var lastMessage = orderedMessages.FirstOrDefault(x => x.EM_ApplicationReference == i.ToString());

							if (lastMessage != null)
							{
								messages.Add(lastMessage);
							}
						}
					}

					govcbr5UAMessages = new EDIMessageWrapperCollection(messages, Factory);
				}
				return govcbr5UAMessages;
			}
		}
		EDIMessageWrapperCollection govcbr5UAMessages;
		#endregion
		#region 5UL

		public EDIMessageWrapperCollection GOVCBR5ULMessages
		{
			get
			{
				if (govcbr5ULMessages == null)
				{
					var messages = new List<EDIMessage>();

					var orderedMessages = entry.Messages.Cast<EDIMessage>()
											.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UL)
											.GroupBy(x => x.EM_MessageOwner)
											.Select(group => group.OrderByDescending(msg => msg.EM_SystemCreateTimeUtc).First());
					govcbr5ULMessages = new EDIMessageWrapperCollection(orderedMessages, Factory);
				}
				return govcbr5ULMessages;
			}
		}
		EDIMessageWrapperCollection govcbr5ULMessages;
		#endregion

		CusEntryNumber ImportEntryNum
		{
			get { return importEntryNum ?? (importEntryNum = GetEntryNumber(KRJobMessageTypeList.Codes.Import)); }
		}
		CusEntryNumber importEntryNum;

		CusEntryNumber GetEntryNumber(string messageType)
		{
			return entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == messageType);
		}
		EDIMessage GetLastOutgoingMessage(string messageType)
		{
			return entry.Messages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault(x => x.EM_MessageType == messageType);
		}
		EDIMessage FindIncomingMessage(string messageType, string messageSubType, string applicationReference)
		{
			return entry.PK.GetIncomingMessage(Factory, applicationReference, messageType, messageSubType);
		}

		public CusEntryHeaderLookups CusEntryHeaderLookups => entry.Lookups;
		public JobComInvoiceHeaderLookups JobComInvoiceHeaderLookups => entry.RandomHeader.Lookups;
		public EarlyReleaseMiscMessageSendingObjectLookups EarlyReleaseMiscMessageSendingObjectLookups => new (new EarlyReleaseMiscMessageSendingObject(entry));
	}
}
