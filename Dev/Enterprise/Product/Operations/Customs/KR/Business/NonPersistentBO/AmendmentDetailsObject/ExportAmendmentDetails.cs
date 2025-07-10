using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ExportAmendmentDetails : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public ExportAmendmentDetails(IExportAmendmentHeader header, BusinessObjectFactory factory, ZGuid entryPK)
			: this(header, factory, ZString.Empty, entryPK, ZString.Empty, ZString.Empty, null)
		{ }
		public ExportAmendmentDetails(IExportAmendmentHeader header, BusinessObjectFactory factory, ZString messageNum5AS, ZGuid entryPK, ZString messageStatus, ZString messageOrEntryStatus, ExportAmendmentMessageDetails amendmentDetails)
			: base(factory)
		{
			this.header = header;
			this.messageNum5AS = messageNum5AS;
			this.entryPK = entryPK;
			this.amendmentDetails = amendmentDetails;
			MessageStatus = messageStatus;
			MessageOrEntryStatus = messageOrEntryStatus;
		}
		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
		readonly IExportAmendmentHeader header;
		readonly ZString messageNum5AS;
		readonly ZGuid entryPK;
		readonly ExportAmendmentMessageDetails amendmentDetails;

		[ResourceStringData("D7B78EFF-6B70-4A96-9A7D-09B7CF6FA2BA", Caption = "Message Status")]
		public ZString MessageStatus { get; }
		public ZString MessageOrEntryStatus { get; }
		public ZString StatusDescription => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(MessageOrEntryStatus);
		public ZBool IsRejected
		{
			get
			{
				var result = ZBool.False;
				switch (MessageOrEntryStatus)
				{
					case CustomsEntryStatusTypeList.Codes.DMS:
					case CustomsMessageStatusTypeList.Codes.CancellationRejected:
					case CustomsMessageStatusTypeList.Codes.AmendmentRejected:
						result = ZBool.True;
						break;
				}
				return result;
			}
		}

		[ResourceStringData("25421D03-E3CC-44EB-B67B-A0B6505FDB9D", Caption = "Amend Sequence No")]
		public ZInt AmendSequenceNo => amendmentDetails?.AmendmentVersionNo ?? 0;
		[ResourceStringData("4EDAD7A1-3350-44E7-84D1-0A61EECB967F", Caption = "Submission Date")]
		public ZDateTime SubmissionDate => amendmentDetails?.SubmissionDate ?? ZDateTime.Empty;
		[ResourceStringData("680D7090-CBCF-45B2-B308-9A820030D738", Caption = "Amendment Type")]
		public ZString AmendmentType => amendmentDetails?.AmendmentType ?? ZString.Empty;
		[ResourceStringData("6744A6C1-1BA1-44DF-A29C-F8780632BC50", Caption = "Amendment Type Desc.")]
		public ZString AmendmentTypeDescription => amendmentDetails?.AmendmentTypeDescription ?? ZString.Empty;
		[ResourceStringData("D6992B98-773B-4093-A39A-411AD440BBD9", Caption = "Fault Party")]
		public ZString FaultParty => amendmentDetails?.FaultParty ?? ZString.Empty;
		[ResourceStringData("515A8964-589D-4E4D-ABA3-7A63C77E9BD3", Caption = "Fault Party Desc.")]
		public ZString FaultPartyOtherDescription => amendmentDetails?.FaultPartyOtherDescription ?? ZString.Empty;
		[ResourceStringData("9901B236-BE21-4125-91C4-A8F65BE09455", Caption = "Reason Code")]
		public ZString ReasonCode => amendmentDetails?.ReasonCode ?? ZString.Empty;
		[ResourceStringData("AFB588AD-5995-4019-92F7-F56FAC0D26D8", Caption = "Reason Code Desc.")]
		public ZString AmendReasonDescription => amendmentDetails?.AmendReasonDescription ?? ZString.Empty;
		public ZString Details => amendmentDetails?.Details ?? ZString.Empty;

		[ResourceStringData("87D6AA11-5657-4472-9AAE-1A26E8E0914D", Caption = "Review Result")]
		public ZString NoticeType => MessageData5DT?.NoticeType ?? ZString.Empty;
		[ResourceStringData("A0853949-87ED-498C-8BF7-7248A872E3A4", Caption = "Review Result Desc.")]
		public ZString NoticeDescription => (ZString)Factory.GetCachedValue<ExportNotificationTypeList>().GetDescriptionFromCode(NoticeType);
		[ResourceStringData("A4A89366-5CA3-47A9-97FC-62A482C9CA44", Caption = "Approval Number")]
		public ZString ApprovalNo => MessageData5DT?.ApprovalNo ?? ZString.Empty;
		public ZString AuthorisationNumber => MessageFunctions.RequestDocumentNumber(ApprovalNo);
		[ResourceStringData("B1A2C524-C245-4C53-A194-B4C6DBC720A9", Caption = "Review Date")]
		public ZDateTime DecisionDate => MessageData5DT?.DecisionDate ?? ZDateTime.Empty;
		public ZString CustomsOfficerID => MessageData5DT?.CustomsPersonID ?? ZString.Empty;
		public ZString CustomsOfficerName => MessageData5DT?.CustomsPersonName ?? ZString.Empty;

		[ResourceStringData("13DA0D95-DA98-4026-9E87-489AC45C4C8C", Caption = "Customer Officer")]
		public ZString CustomerOfficerIDAndName => MessageData5AF != null ? ZString.Format("{0}/{1}", MessageData5AF.CustomsPersonID, MessageData5AF.CustomsPersonName) : ZString.Empty;

		public ZDecimal BeforeTotalCustomsValue { get; set; }
		public ZDecimal AfterTotalCustomsValue { get; set; }
		public ZDecimal BeforeTotalCustomsValueUSD { get; set; }
		public ZDecimal AfterTotalCustomsValueUSD { get; set; }
		public ZString FormattedBeforeTotalCustomsValue => BeforeTotalCustomsValue.ToString("#,##0") + BeforeTotalCustomsValueUSD.ToString("($#,##0)");
		public ZString FormattedAfterTotalCustomsValue => AfterTotalCustomsValue.ToString("#,##0") + AfterTotalCustomsValueUSD.ToString("($#,##0)");
		public ZBool IsCustomsValueEqual => BeforeTotalCustomsValue == AfterTotalCustomsValue;

		public ZDateTime DeclarationDate { get; set; }
		public ZDateTime ReleaseDate { get; set; }
		public ZString BrokerCompanyName { get; set; }
		public ZString BrokerCompanyRepresentative { get; set; }
		public ZString SupplierAddress { get; set; }
		public ZString SupplierCompanyName { get; set; }
		public ZString SupplierCompanyID { get; set; }
		public ZString FormattedSupplierCompanyID => MessageFunctions.GetFormattedUnipassIDForOrganization(SupplierCompanyID);

		public ZString FormattedExportDeclarationNumber => MessageFunctions.DeclarationNumberFormat(header?.ExportDeclarationNumber ?? ZString.Empty);
		public ZString DeclarationCustomsOffice => header?.DeclarationCustomsOffice ?? ZString.Empty;
		public ZString DeclarationCustomsOfficeDescription => Factory.GetCachedValue(DeclarationCustomsOffice, () => MessageFunctions.GetCustomsOffice(Factory, DeclarationCustomsOffice));
		public ZString DeclarationCustomsDivision => header?.DeclarationCustomsDivision ?? ZString.Empty;

		IGOVCBR5AFMessageData MessageData5AF
		{
			get
			{
				if (messageData5AF == null && entryPK.IsValid)
				{
					var message5AF = entryPK.GetIncomingMessage(Factory, messageNum5AS, ElectronicDocumentTypeList.Codes._5AF);
					if (message5AF != null)
					{
						using (var textReader = message5AF.GetEM_MessageTextReader())
						{
							messageData5AF = new GOVCBR5AFDataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageData5AF;
			}
		}
		IGOVCBR5AFMessageData messageData5AF;

		IGOVCBR5DTMessageData MessageData5DT
		{
			get
			{
				if (messageData5DT == null && entryPK.IsValid)
				{
					var message5DT = entryPK.GetIncomingMessage(Factory, messageNum5AS, ElectronicDocumentTypeList.Codes._5DT);
					if (message5DT != null)
					{
						using (var textReader = message5DT.GetEM_MessageTextReader())
						{
							messageData5DT = new GOVCBR5DTDataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageData5DT;
			}
		}
		IGOVCBR5DTMessageData messageData5DT;

		public Export5ASItemWrapperCollection HeaderAmendedItems
		{
			get
			{
				if (headerAmendedItems == null)
				{
					var headerItem = header?.AmendmentItems?.Cast<Export5ASItem>().Where(x => x.IsHeaderItem()).OrderBy(x => x.AmendDataItemID);
					headerAmendedItems = new Export5ASItemWrapperCollection(headerItem, Factory);
				}
				return headerAmendedItems;
			}
		}
		Export5ASItemWrapperCollection headerAmendedItems;

		public Export5ASItemWrapperCollection LineAmendedItems
		{
			get
			{
				if (lineAmendedItems == null)
				{
					var lineItem = header?.AmendmentItems?.Cast<Export5ASItem>().Where(x => !x.IsHeaderItem()).OrderBy(x => x.AmendDataItemID);
					lineAmendedItems = new Export5ASItemWrapperCollection(lineItem, Factory);
				}
				return lineAmendedItems;
			}
		}
		Export5ASItemWrapperCollection lineAmendedItems;

		public Export5ASItemWrapperCollection AllAmendedItems
		{
			get
			{
				if (allAmendedItems == null)
				{
					allAmendedItems = new Export5ASItemWrapperCollection(header?.AmendmentItems.Cast<Export5ASItem>().OrderBy(x => x.AmendDataItemID), Factory);
				}
				return allAmendedItems;
			}
		}
		Export5ASItemWrapperCollection allAmendedItems;
	}
}
