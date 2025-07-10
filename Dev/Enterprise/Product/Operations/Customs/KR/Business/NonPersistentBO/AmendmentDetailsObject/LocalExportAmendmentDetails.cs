using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendmentDetails : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public LocalExportAmendmentDetails(ILocalExportAmendEntryHeader header, BusinessObjectFactory factory)
			: this(header, factory, ZString.Empty, ZGuid.Empty, ZString.Empty, null)
		{ }
		public LocalExportAmendmentDetails(ILocalExportAmendEntryHeader header, BusinessObjectFactory factory, ZString messageNum, ZGuid entryPK, ZString messageStatus, LocalExportAmendmentMessageDetails amendmentDetails)
			: base(factory)
		{
			this.header = header;
			this.amendmentMessageNumber = messageNum;

			this.entryPK = entryPK;
			this.amendmentDetails = amendmentDetails;

			MessageStatus = messageStatus;
		}
		readonly ZString amendmentMessageNumber;
		readonly ZGuid entryPK;
		readonly LocalExportAmendmentMessageDetails amendmentDetails;
		readonly ILocalExportAmendEntryHeader header;

		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;

		public LocalExportAmendItemWrapperCollection HeaderAmendedItems
		{
			get
			{
				if (headerAmendedItems == null)
				{
					headerAmendedItems = new LocalExportAmendItemWrapperCollection(header.AmendedItems, AmendedItemType.Header, Factory);
				}
				return headerAmendedItems;
			}
		}
		LocalExportAmendItemWrapperCollection headerAmendedItems;
		public LocalExportAmendItemWrapperCollection LineAmendedItems
		{
			get
			{
				if (lineAmendedItems == null)
				{
					lineAmendedItems = new LocalExportAmendItemWrapperCollection(header.AmendedItems, AmendedItemType.Line, Factory);
				}
				return lineAmendedItems;
			}
		}
		LocalExportAmendItemWrapperCollection lineAmendedItems;

		public LocalExportAmendItemWrapperCollection AllAmendedItems
		{
			get
			{
				if (allAmendedItems == null)
				{
					allAmendedItems = new LocalExportAmendItemWrapperCollection(header.AmendedItems, AmendedItemType.All, Factory);
				}
				return allAmendedItems;
			}
		}
		LocalExportAmendItemWrapperCollection allAmendedItems;

		[ResourceStringData("96E80C4A-0C39-4188-92EB-ED11D5C40581", Caption = "Message Status")]
		public ZString MessageStatus { get; }

		[ResourceStringData("272AEC8B-F4FA-4E65-BEB1-B061A0D4C837", Caption = "Message Status Desc.")]
		public ZString MessageStatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageStatus);

		public ZString DeclarationNumber => header.DeclarationNumber;
		[ResourceStringData("E5A6312F-3979-4695-829B-3685E1F2C1EA", Caption = "Declaration Number")]
		public ZString FormattedDeclarationNumber => MessageFunctions.DeclarationNumberFormat(DeclarationNumber);
		public ZString CustomsReceiptNumber => header.CustomsReceiptNumber;
		public ZString FormattedCustomsReceiptNumber => CustomsReceiptNumber.Length == 14 ? MessageFunctions.GetFormattedNumber(CustomsReceiptNumber, new int[] { 0, 3, 5, 7, 13 }) : CustomsReceiptNumber;
		public OrganizationDocWrapper Supplier => supplier ?? (supplier = new OrganizationDocWrapper(header.Supplier));
		OrganizationDocWrapper supplier;
		public ZString SupplierBusinessRegNo => header.Supplier?.GetRegistrationNumberFormattedIfRequired(IdentificationType.BusinessRegNo) ?? ZString.Empty;
		public ZString SupplierUnipassIDForOrganization => header.Supplier?.GetRegistrationNumberFormattedIfRequired(IdentificationType.UnipassIDForOrganization) ?? ZString.Empty;

		[ResourceStringData("B7C4D866-7BD7-48B6-A44A-5621AB62FFB7", Caption = "Amendment Type")]
		public ZString AmendType => amendmentDetails?.AmendmentType ?? ZString.Empty;
		[ResourceStringData("7BB51B4F-AB2F-430B-A657-E70AB83F2FB6", Caption = "Amendment Type Desc.")]
		public ZString AmendTypeDescription => Factory.GetCachedValue<LocalExportAmendmentTypeList>().GetDescriptionFromCode(AmendType);
		[ResourceStringData("92F15C4A-FF65-455E-9EE4-D3E4F0A824A9", Caption = "Reason Code")]
		public ZString ReasonCode => amendmentDetails?.ReasonCode ?? ZString.Empty;
		[ResourceStringData("D356B354-4485-4779-BF3D-40E556794E3F", Caption = "Reason Code Desc.")]
		public ZString ReasonCodeDescription => Factory.GetCachedValue<LocalExportAmendmentReasonCodeList>().GetDescriptionFromCode(ReasonCode);
		public ZString AmendReasonDescription => amendmentDetails?.AmendReasonDescription ?? ZString.Empty;
		[ResourceStringData("F3966F55-EC09-4031-A875-995DE158B0DB", Caption = "Submission Date")]
		public ZDateTime SubmissionDate => amendmentDetails?.SubmissionDate ?? ZDateTime.Empty;

		[ResourceStringData("0F648EB7-A693-4014-B367-D705634C14E1", Caption = "Customs Review Date")]
		public ZDateTime CustomsReviewDate => MessageDataRR3_5DP5DQ?.CustomsDateTime ?? ZDateTime.Empty;

		public ZString CustomsReferenceNumber => MessageDataR38?.ConfirmNumber ?? ZString.Empty;
		[ResourceStringData("6BF49C66-CD35-4074-85D0-3CD1CA3C156D", Caption = "Customs Reference Number")]
		public ZString FormattedCustomsReferenceNumber => MessageFunctions.GetFormattedNumber(CustomsReferenceNumber, new int[] { 0, 3, 5, 7, 13 });
		public ZDateTime AcceptanceDate => MessageDataR38?.AcceptDateTime ?? ZDateTime.Empty;

		public ZDateTime AmendAuthorisationDate => MessageDataRR3_5DR5DS?.CustomsDateTime ?? ZDateTime.Empty;
		public ZString CustomsOfficeAndDivision => MessageDataRR3_5DR5DS?.CustomsOfficeAndDivision ?? ZString.Empty;
		public ZString FormattedCustomsOfficeAndDivision => MessageFunctions.GetFormattedCustomsOfficeAndDivision(CustomsOfficeAndDivision);
		[ResourceStringData("3DB22C95-1792-4859-A590-B2D78C6F583E", Caption = "Review Result")]
		public ZString NoticeType => MessageDataRR3_5DR5DS?.ResultType ?? ZString.Empty;
		[ResourceStringData("5FA6A3E7-8EB6-49F9-BE40-F2B29EF9A698", Caption = "Review Result Desc.")]
		public ZString NoticeTypeDescription => Factory.GetCachedValue<LocalExportProcessResultTypeCodeList>().GetDescriptionFromCode(NoticeType);
		[ResourceStringData("BEA3D350-2B65-46F0-959F-D968A31E639C", Caption = "Customer Officer")]
		public ZString CustomerOfficer => MessageFunctions.GetCustomsOffice(Factory, CustomsOfficeAndDivision.SubstringSafe(0, 3));

		IGOVCBRR38MessageData MessageDataR38
		{
			get
			{
				if (messageDataR38 == null && entryPK.IsValid)
				{
					var messageR38 = entryPK.GetIncomingMessage(Factory, amendmentMessageNumber, ElectronicDocumentTypeList.Codes._R38);
					if (messageR38 != null)
					{
						using (var textReader = messageR38.GetEM_MessageTextReader())
						{
							messageDataR38 = new GOVCBRR38DataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageDataR38;
			}
		}
		IGOVCBRR38MessageData messageDataR38;

		IGOVCBRRR3MessageData MessageDataRR3_5DR5DS
		{
			get
			{
				if (messageDataRR3_5DR5DS == null && entryPK.IsValid)
				{
					var messageRR3_5DR5DS = entryPK.GetIncomingMessage(Factory, amendmentMessageNumber, ElectronicDocumentTypeList.Codes._RR3);
					if (messageRR3_5DR5DS != null)
					{
						using (var textReader = messageRR3_5DR5DS.GetEM_MessageTextReader())
						{
							messageDataRR3_5DR5DS = new GOVCBRRR3DataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageDataRR3_5DR5DS;
			}
		}
		IGOVCBRRR3MessageData messageDataRR3_5DR5DS;

		IGOVCBRRR3MessageData MessageDataRR3_5DP5DQ
		{
			get
			{
				if (messageDataRR3_5DP5DQ == null && entryPK.IsValid)
				{
					var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryPK);
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					query.AddToFilter(EDIMessageSchema.EM_MessageType, new string[] { ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Codes._5DQ });
					query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
					var message5DP5DQ = Factory.LoadTop1<EDIMessage>(query);
					if (message5DP5DQ != null)
					{
						var messageRR3_5DP5DQ = entryPK.GetIncomingMessage(Factory, message5DP5DQ.EM_MessageNum, ElectronicDocumentTypeList.Codes._RR3);
						if (messageRR3_5DP5DQ != null)
						{
							using (var textReader = messageRR3_5DP5DQ.GetEM_MessageTextReader())
							{
								messageDataRR3_5DP5DQ = new GOVCBRRR3DataProvider().GetMessageData(textReader);
							}
						}
					}
				}
				return messageDataRR3_5DP5DQ;
			}
		}
		IGOVCBRRR3MessageData messageDataRR3_5DP5DQ;
	}
}
