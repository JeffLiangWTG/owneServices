using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMiscMessageSendingObject : JobDeclarationMiscMessageSendingObjectCore
	{
		public JobDeclarationMiscMessageSendingObject(CusEntryHeader entry, string messageType, MessageFunctionCode functionCode, Func<CusEntryLine, bool> entryLineFilter)
			: base(entry, messageType)
		{
			this.functionCode = functionCode;

			var result = GetAmendmentTypeDescription(messageType, functionCode);
			amendmentType = result.Item1;
			amendmentDescription = result.Item2;
			this.entryLineFilter = entryLineFilter;
		}

		readonly MessageFunctionCode functionCode;
		readonly string amendmentType;
		readonly string amendmentDescription;
		readonly Func<CusEntryLine, bool> entryLineFilter;

		public new class Schema : JobDeclarationMessageSendingObject.Schema
		{
			public const string CurrentDate = "CurrentDate";
			public const string NewDate = "NewDate";
			public const string PayerBusinessNumber = "PayerBusinessNumber";
			public const string DeclarationDate = "DeclarationDate";
		}

		public void PopulateCancellationObject(EDIMessage message)
		{
			CargoWise.Customs.KR.MessageDefinitions.GOVCBRDKJ.Declaration declaration = null;

			using (var reader = message.GetEM_MessageTextReader())
			{
				declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBRDKJ.Declaration>(reader);
			}
			using (GetValidationSuspender())
			{
				if (declaration.DeclarationOfficeId.Value.Length == 5)
				{
					CustomsOffice = declaration.DeclarationOfficeId.Value.Substring(0, 3);
					CustomsDivision = declaration.DeclarationOfficeId.Value.Substring(3, 2);
				}
				FaultParty = declaration.Reason?.Value ?? ZString.Empty;
				ReasonCode = declaration.AdditionalInformation?.StatementCode.Value ?? ZString.Empty;
				AmendmentReason = declaration.AdditionalInformation?.StatementDescription.Value ?? ZString.Empty;
				if (DateTime.TryParseExact(declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					DateOfApplication = new ZDateTime(dt);
				}

				HasChanges = false;
			}
		}

		static (string, string) GetAmendmentTypeDescription(string messageType, MessageFunctionCode functionCode)
		{
			var amendmentType = string.Empty;
			var amendmentDescription = string.Empty;

			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5AS:
					amendmentType = _5ASAmendmentType.Codes.Extension;
					amendmentDescription = _5ASAmendmentType.Descriptions.Extension;
					break;
				case ElectronicDocumentTypeList.Codes._DKJ:
					amendmentType = _5ASAmendmentType.Codes.Cancellation;
					amendmentDescription = _5ASAmendmentType.Descriptions.Cancellation;
					break;
				case ElectronicDocumentTypeList.Codes._5DS:
				case ElectronicDocumentTypeList.Codes._5DR:
					if (functionCode == MessageFunctionCode.Cancellation)
					{
						amendmentType = LocalExportAmendmentTypeList.Codes.Cancellation;
						amendmentDescription = LocalExportAmendmentTypeList.Descriptions.Cancellation;
					}
					break;
				default:
					break;
			}
			return (amendmentType, amendmentDescription);
		}
		public override ZString AmendmentType => amendmentType;

		public const string CaptionKeyDKJCancellation = "DKJ|Cancellation";
		public const string CaptionKey5DRCancellation = "5DR|Cancellation";
		public const string CaptionKey5DSCancellation = "5DS|Cancellation";

		public override ZString AmendmentTypeDescription => amendmentDescription;
		public ZDateTime DateOfApplication { get; set; }

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|DKJCancellationReason", Caption = "Cancellation Reason", MultipleKey = CaptionKeyDKJCancellation)]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|5DRCancellationReason", Caption = "Cancellation Reason", MultipleKey = CaptionKey5DRCancellation)]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|5DSCancellationReason", Caption = "Cancellation Reason", MultipleKey = CaptionKey5DSCancellation)]
		public override ZString AmendmentReason
		{
			get => base.AmendmentReason;
			set => base.AmendmentReason = value;
		}

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|CancellationCode", Caption = "Cancellation Code", MultipleKey = CaptionKeyDKJCancellation)]
		public override ZString ReasonCode
		{
			get => base.ReasonCode;
			set => base.ReasonCode = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|CurrentDate", Caption = "Current Date")]
		public ZDate CurrentDate
		{
			get
			{
				var result = ZDate.Empty;
				if (functionCode == MessageFunctionCode.Extend)
				{
					var entryNum = Header.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == SharedJobMessageTypeList.Codes.Export);
					if (entryNum != null)
					{
						result = (ZDate)entryNum.CE_ExpiryDate;
					}
				}
				return result;
			}
		}
		public ZPropertyInfo CurrentDateInfo => GetZPropertyInfo(Schema.CurrentDate);

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|NewDate", Caption = "New Date")]
		public virtual ZDate NewDate
		{
			get { return newDate; }
			set
			{
				SetNonPersistentPropertyValue(NewDateInfo, ref newDate, value);
			}
		}
		ZDate newDate;

		public ZPropertyInfo NewDateInfo => GetZPropertyInfo(Schema.NewDate);

		[ResourceStringData("B12DE526-6CD2-457D-A475-7E3629710F9D", Caption = "Payer Business Number")]
		public ZString PayerBusinessNumber => Header.Declaration.PayerBusinessNumber?.Number ?? ZString.Empty;

		public ZString CustomsOffice { get; set; }
		public ZString CustomsDivision { get; set; }
		public ZString CustomsOfficeName => Messaging.MessageFunctions.GetCustomsOffice(Factory, CustomsOffice);

		public MessageSendingEntryLineObjectCollection MessageSendingEntryLines
		{
			get
			{
				if (messageSendingEntryLines == null)
				{
					messageSendingEntryLines = new MessageSendingEntryLineObjectCollection(Header);
					messageSendingEntryLines.PopulateElementsFromMergedLines(entryLineFilter, MessageType);
					RegisterEditableChildObject(messageSendingEntryLines);
					RegisterListChangedCalledRefreshBinding(messageSendingEntryLines);

					if (MessageType == ElectronicDocumentTypeList.Codes._5TM)
					{
						foreach (MessageSendingEntryLineObject entryLineObject in messageSendingEntryLines)
						{
							entryLineObject.IsGoldOrItsProductInfo.ValueChanged += IsGoldOrItsProductInfo_ValueChanged;
						}
					}
				}
				return messageSendingEntryLines;
			}
		}

		void IsGoldOrItsProductInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateShouldSend();
			}
		}
		MessageSendingEntryLineObjectCollection messageSendingEntryLines;

		public new JobDeclarationMiscMessageSendingObjectValidation Validation => (JobDeclarationMiscMessageSendingObjectValidation)base.Validation;

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationMiscMessageSendingObjectValidation(this);
	}
}
