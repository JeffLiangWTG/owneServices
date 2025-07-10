using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ImportAmendmentDetails : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public ImportAmendmentDetails(IImport5FEHeader header, BusinessObjectFactory factory)
			: this(header, factory, ZString.Empty, ZGuid.Empty, ZString.Empty, null)
		{ }
		public ImportAmendmentDetails(IImport5FEHeader header, BusinessObjectFactory factory, ZString messageNum, ZGuid entryPK, ZString messageStatus, ImportAmendmentMessageDetails amendmentDetails)
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
		readonly ImportAmendmentMessageDetails amendmentDetails;
		readonly IImport5FEHeader header;

		IGOVCBR5FKMessageData MessageData5FK
		{
			get
			{
				if (messageData5FK == null && entryPK.IsValid)
				{
					var message5FK = entryPK.GetIncomingMessage(Factory, amendmentMessageNumber, ElectronicDocumentTypeList.Codes._5FK);
					if (message5FK != null)
					{
						using (var textReader = message5FK.GetEM_MessageTextReader())
						{
							messageData5FK = new GOVCBR5FKDataProvider().GetMessageData(Factory, textReader);
						}
					}
				}
				return messageData5FK;
			}
		}
		IGOVCBR5FKMessageData messageData5FK;

		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;

		[ResourceStringData("7736E43F-70BC-4338-AF16-A337A2B42486", Caption = "Submission Date")]
		public ZDateTime SubmissionDate => amendmentDetails?.SubmissionDate ?? ZDate.Empty;

		[ResourceStringData("31AEE440-8730-4CBC-861E-94A53989AE80", Caption = "Amend Sequence No")]
		public ZInt AmendSequenceNo => amendmentDetails?.AmendmentVersionNo ?? 0;

		[ResourceStringData("96E80C4A-0C39-4188-92EB-ED11D5C40581", Caption = "Message Status")]
		public ZString MessageStatus { get; }

		[ResourceStringData("272AEC8B-F4FA-4E65-BEB1-B061A0D4C837", Caption = "Message Status Desc.")]
		public ZString MessageStatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageStatus);

		[ResourceStringData("89EE6033-14D2-4A76-A1C5-218F240DA799", Caption = "Amendment Type 1 Desc.")]
		public ZString AmendType1Description => Factory.GetCachedValue<ImportAmedmentTypeList>().GetDescriptionFromCode(AmendType1);

		[ResourceStringData("F706973C-3553-498F-A96F-4D71AC759372", Caption = "Amendment Type 2 Desc.")]
		public ZString AmendType2Description => Factory.GetCachedValue<ImportAmedmentTypeList>().GetDescriptionFromCode(AmendType2);

		[ResourceStringData("41591DAB-1BBC-4333-97E8-3A2AA77C7E11", Caption = "Fault Party Desc.")]
		public ZString FaultPartyDescription => FaultParty == ImputationReasonCodeList.Codes._99 ? amendmentDetails?.FaultPartyOtherDescription ?? ZString.Empty : Factory.GetCachedValue<ImputationReasonCodeList>().GetDescriptionFromCode(FaultParty) ?? ZString.Empty;

		[ResourceStringData("DA7B1CC3-BA2C-4B83-BEE7-831714902209", Caption = "Reason Code Desc.")]
		public ZString ReasonCodeDescription => Factory.GetCachedValue<ImportDeclarationModifyReasonCodeList>().GetDescriptionFromCode(ReasonCode);

		[ResourceStringData("E0136888-F3CD-463E-A88F-2544B907B2A4", Caption = "Review Result Desc.")]
		public ZString NoticeTypeDescription => Factory.GetCachedValue<ImportAmendmentResultList>().GetDescriptionFromCode(NoticeType);

		[ResourceStringData("1150CAD1-ED5C-4F2A-9D27-E92754095709", Caption = "Customs Disbursement Bill #")]
		public ZString CustomsDisbursementBillNumber => MessageFunctions.NoticeNumberFormat(MessageData5FK?.NoticeNumber ?? ZString.Empty);

		[ResourceStringData("970D2F48-4A4A-422A-B597-6198061AD71E", Caption = "Review Date")]
		public ZDateTime DecisionDate => MessageData5FK?.NoticeDate ?? ZDateTime.Empty;

		[ResourceStringData("F10FB393-00C6-467C-A122-1C27F4B1CBF2", Caption = "Customer Officer")]
		public ZString CustomerOfficer => MessageData5FK?.CustomerOfficer ?? ZString.Empty;

		[ResourceStringData("41E18E2D-C0F8-4AC3-9025-353B8FF2AD95", Caption = "Payment Amount")]
		public ZDecimal PaymentAmount => MessageData5FK?.InDepositAmount ?? ZDecimal.Zero;

		[ResourceStringData("AFCDFAC8-A9C9-4534-9632-6CCFE1FF934F", Caption = "Delay Payment Amount")]
		public ZDecimal DelayPaymentAmount => MessageData5FK?.DelayPaymentAmount ?? ZDecimal.Zero;

		[ResourceStringData("DB0906A0-4896-49D5-9585-220369A08340", Caption = "Amend Penalty Payable")]
		public ZDecimal AmendPenaltyPayable => MessageData5FK?.TotalInterestAndPenalty ?? ZDecimal.Zero;

		[ResourceStringData("79F7A226-E3B9-4469-A61C-ABD24981B50F", Caption = "Before Total Duty Tax")]
		public ZDecimal BeforeTotalDutyTax => header?.BeforeTotalDutyTaxAmount ?? ZDecimal.Zero;

		[ResourceStringData("A52065A9-006A-4502-B9C9-A6B776F99FE5", Caption = "After Total Duty Tax")]
		public ZDecimal AfterTotalDutyTax => header?.AfterTotalDutyTaxAmount ?? ZDecimal.Zero;

		[ResourceStringData("0CF03256-D8C4-475E-BED3-0C8DDD01B866", Caption = "Duty Tax Difference")]
		public ZDecimal DutyTaxDifference => header?.DutyTaxDifference ?? ZDecimal.Zero;

		[ResourceStringData("82BF0441-D124-4782-8A71-B0F8B8D741B9", Caption = "Before Customs Value")]
		public ZDecimal BeforeCustomsValue => header?.BeforeCustomsValue ?? ZDecimal.Zero;

		[ResourceStringData("2C83BAC5-F303-4E1A-AC4C-663479B2FE1C", Caption = "After Customs Value")]
		public ZDecimal AfterCustomsValue => header?.AfterCustomsValue ?? ZDecimal.Zero;

		[ResourceStringData("81AFED08-A121-4F46-9C95-0A70690DA7D3", Caption = "Customs Value Difference")]
		public ZDecimal CustomsValueDifference => header?.CustomsValueDifference ?? ZDecimal.Zero;

		[ResourceStringData("002539F4-E322-4A50-9F10-C8939F56EBC8", Caption = "Amendment Type 1")]
		public ZString AmendType1 => amendmentDetails?.AmendmentType.SubstringSafe(0, 1) ?? ZString.Empty;

		[ResourceStringData("02275A10-8B79-4CD6-93DD-BC4D73D8C314", Caption = "Amendment Type 2")]
		public ZString AmendType2 => amendmentDetails?.AmendmentType.SubstringSafe(1, 1) ?? ZString.Empty;

		[ResourceStringData("9C06C8AF-47F4-4F6B-BF9A-963DB6AB1C94", Caption = "Fault Party")]
		public ZString FaultParty => amendmentDetails?.FaultParty ?? ZString.Empty;

		[ResourceStringData("AA62C080-0E8A-46B4-825F-FC70B9780B75", Caption = "Reason Code")]
		public ZString ReasonCode => amendmentDetails?.ReasonCode ?? ZString.Empty;

		[ResourceStringData("F826A26E-5ADE-4090-940B-BFCA63F8FE5A", Caption = "Review Result")]
		public ZString NoticeType => MessageData5FK?.NoticeCode ?? ZString.Empty;
	}
}
