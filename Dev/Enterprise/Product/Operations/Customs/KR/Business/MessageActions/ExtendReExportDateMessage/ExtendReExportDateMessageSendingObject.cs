using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRD72;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR43;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendReExportDateMessageSendingObject : JobDeclarationMiscMessageSendingObjectCore
	{
		#region Schema
		public new static class Schema
		{
			public const string ReasonDescription = "ReasonDescription";
			public const int ReasonDescriptionMaxLength = 500;
			public const string NewReExportDate = "NewReExportDate";
		}
		#endregion

		public ExtendReExportDateMessageSendingObject(CusEntryHeader entry, ZDateTime currentReExportScheduledDate) : base(entry, ElectronicDocumentTypeList.Codes._D72)
		{
			CurrentReExportScheduledDate = currentReExportScheduledDate;
		}

		public MessageSendingInvoiceLineCollection MessageSendingInvoiceLines
		{
			get
			{
				if (messageSendingInvoiceLines == null)
				{
					messageSendingInvoiceLines = new MessageSendingInvoiceLineCollection(this.Factory);
					messageSendingInvoiceLines.PopulateElements(this, CurrentReExportScheduledDate);
					RegisterEditableChildObject(messageSendingInvoiceLines);
				}
				return messageSendingInvoiceLines;
			}
		}
		MessageSendingInvoiceLineCollection messageSendingInvoiceLines;

		public MessageSendingEntryLineObjectCollection D72EntryLines
		{
			get
			{
				if (d72EntryLines == null)
				{
					d72EntryLines = new MessageSendingEntryLineObjectCollection(Header);
					d72EntryLines.PopulateElementsFromMergedLines(x => x.RandomLine.JI_ScheduledReExportDate == CurrentReExportScheduledDate, ElectronicDocumentTypeList.Codes._D72);
					RegisterEditableChildObject(d72EntryLines);
				}
				return d72EntryLines;
			}
		}
		MessageSendingEntryLineObjectCollection d72EntryLines;

		[ResourceStringData("8E8E0230-8A6C-4D02-9E61-027E773A8F6A", Caption = "Current Re-Export Scheduled Date")]
		public ZDateTime CurrentReExportScheduledDate { get; private set; }
		public ZString CompanyNameForDocument { get; private set; }
		public ZString RepresentativeNameForDocument { get; private set; }
		public ZString FormattedNoForDocument { get; private set; }
		public ZString AddressDetailsForDocument { get; private set; }
		ZString CustomsOffice { get; set; }
		public ZString CustomsOfficeName => MessageFunctions.GetCustomsOffice(Factory, CustomsOffice);
		public ZDateTime SentDateTimeInLocalTimeZone { get; private set; }
		public override ZString AmendmentType => ZString.Empty;
		public override ZString AmendmentTypeDescription => ZString.Empty;
		public ZString MessageOrEntryStatus { get; private set; }
		public ZString MessageOrEntryStatusDescription { get; private set; }
		public ZDateTime EffectiveDateTime { get; private set; }

		public new ExtendReExportDateMessageSendingObjectValidation Validation => (ExtendReExportDateMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new ExtendReExportDateMessageSendingObjectValidation(this);

		#region ReasonDescription
		[MaxLength(Schema.ReasonDescriptionMaxLength)]
		[ResourceStringData("108382A6-90C3-48B1-86F1-0982AC0AFC87", Caption = "Reason Description")]
		public virtual ZString ReasonDescription
		{
			get
			{
				return reasonDescription;
			}
			set
			{
				CheckMaximumLength(ReasonDescriptionInfo, value);
				SetNonPersistentPropertyValue(ReasonDescriptionInfo, ref reasonDescription, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonDescription();
				}
			}
		}
		public virtual ZPropertyInfo ReasonDescriptionInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.ReasonDescription);
			}
		}
		ZString reasonDescription;
		#endregion

		#region NewReExportDate
		[ResourceStringData("3BB43950-C3E6-4316-89F8-5FC795D9B112", Caption = "New Re-Export Scheduled Date")]
		public virtual ZDateTime NewReExportDate
		{
			get
			{
				return newReExportDate;
			}
			set
			{
				SetNonPersistentPropertyValue(NewReExportDateInfo, ref newReExportDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNewReExportDate();
				}
			}
		}
		public virtual ZPropertyInfo NewReExportDateInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.NewReExportDate);
			}
		}
		ZDateTime newReExportDate;
		#endregion

		public void PopulateD72(EDIMessage message)
		{
			if (message.EM_MessageType != ElectronicDocumentTypeList.Codes._D72)
			{
				throw new ArgumentException("expected to receive an EDIMessage of D72");
			}

			Declaration declaration = null;
			using (var reader = message.GetEM_MessageTextReader())
			{
				declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Declaration>(reader);
			}

			SentDateTimeInLocalTimeZone = Env.Time.GetLocalTimeFromUtc(message.EM_SystemCreateTimeUtc.ToDateTime());
			MessageOrEntryStatus = message.MessageOrEntryStatus;
			MessageOrEntryStatusDescription = Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageOrEntryStatus) ?? Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(MessageOrEntryStatus);

			using (GetValidationSuspender())
			{
				CustomsOffice = declaration.DeclarationOfficeId.Value;
				ReasonDescription = declaration.Reason.Value;

				CompanyNameForDocument = declaration.Submitter.Name?.Value ?? ZString.Empty;
				RepresentativeNameForDocument = declaration.Submitter.Contact.Name.Value;

				var roleCode = declaration.Submitter.RoleCode.Value;
				if (roleCode == IOrganizationExtensionMethods.RegNoForResidentKRC)
				{
					FormattedNoForDocument = MessageFunctions.GetFormattedNumber(declaration.Submitter.Id.Value, new int[] { 0, 6 });
				}
				else if (roleCode == IOrganizationExtensionMethods.BusinessNoTypeForKRC)
				{
					FormattedNoForDocument = MessageFunctions.GetFormattedNumber(declaration.Submitter.Id.Value, new int[] { 0, 3, 5 });
				}
				else
				{
					FormattedNoForDocument = declaration.Submitter.Id.Value;
				}

				var strBuilder = new ZStringBuilder();
				ZString addressLine1 = declaration.Submitter.Address.Description.Value;
				ZString addressLine2 = declaration.Submitter.Address.Line?.Value ?? ZString.Empty;
				strBuilder.Append(addressLine1);
				if (!addressLine2.IsEmpty)
				{
					strBuilder.Append(addressLine2);
				}
				AddressDetailsForDocument = strBuilder.ToStringWithDelimiterBetweenAppends(" ");

				if (DateTime.TryParseExact(declaration.AdditionalInformation?.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					NewReExportDate = dt1;
				}
				if (DateTime.TryParseExact(declaration.PreviousDocument?.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
				{
					CurrentReExportScheduledDate = dt2;
				}

				MessageSendingInvoiceLines.RemoveAndDeleteAll();
				foreach (var goodShipmentLine in declaration.GoodsShipment)
				{
					var lineData = MessageSendingInvoiceLines.AddNew();
					lineData.PopulateFrom(goodShipmentLine);
				}
				HasChanges = false;

				var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, message.EM_MessageNum);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, ElectronicDocumentTypeList.Codes._R43);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, message.EM_LinkedObject.PK);
				query.AddToFilter(EDIMessageSchema.EM_MessageOwner, CustomsEntryStatusTypeList.Codes.ANT);
				query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
				var r43Message = Factory.LoadTop1<EDIMessage>(query);

				if (r43Message != null)
				{
					Response response = null;
					using (var reader = r43Message.GetEM_MessageTextReader())
					{
						response = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Response>(reader);
					}
					if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
					{
						EffectiveDateTime = new ZDate(dt3);
					}
				}
			}
		}
	}
}
