using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class CustomsAndExciseReportOutboundMessage : CustomsAndExciseReportMessage, ICustomsAndExciseReportOutboundMessage, IRelatedJob
	{
		public CustomsAndExciseReportOutboundMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : Enterprise.Messaging.Business.EDIMessage.Schema
		{
			public const string MessageDate = nameof(CustomsAndExciseReportOutboundMessage.MessageDate);
		}

		protected override bool CanContinueWithSaveCore => GlbCompany.CurrentCompany.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired() && base.CanContinueWithSaveCore;

		[List(nameof(Lookups) + "." + nameof(CustomsAndExciseReportOutboundMessageLookups.ReportTypeList))]
		[ReadOnlyMember(nameof(MessageType_ReadOnly))]
		[ResourceStringData("33230581-426D-40AE-9D0F-06AA9FAA63BF", Caption = "Report Type")]
		public override ZString EM_MessageType
		{
			get => base.EM_MessageType;
			set
			{
				if (base.EM_MessageType != value)
				{
					base.EM_MessageType = value;
					if (!IsCopying)
					{
						MessageDate = GetMessageDate(MessageDate);
					}
					UpdateMessageInterpretation();
				}
			}
		}

		[ResourceStringData("2C9FE803-442C-4318-8B95-B7BA533435B3", Caption = "Message Number")]
		public override ZString EM_MessageNum { get => base.EM_MessageNum; set => base.EM_MessageNum = value; }

		[ResourceStringData("4DE0CD37-11D8-495C-81AC-6FE512387210", Caption = "Status")]
		public override ZString EM_Status { get => base.EM_Status; set => base.EM_Status = value; }

		public override ZDateTime EM_SystemCreateTimeUtc
		{
			get => base.EM_SystemCreateTimeUtc;
			set
			{
				base.EM_SystemCreateTimeUtc = value;
				UpdateMessageInterpretation();
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(MessageDate_ReadOnly))]
		[ResourceStringData("F5ED79FD-055F-462D-9658-126FB6D9FB68", Caption = "Date")]
		public ZDateTime MessageDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.MessageDate);
			set
			{
				this.SetSystemDefinedValue(Schema.MessageDate, GetMessageDate(value));
				UpdateMessageInterpretation();
				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageDate();
				}
			}
		}

		ZDateTime GetMessageDate(ZDateTime dateTime)
		{
			if (dateTime.IsValid)
			{
				if (EM_MessageType == CustomsAndExciseReportTypeList.Codes.PSR || EM_MessageType == CustomsAndExciseReportTypeList.Codes.PCT || EM_MessageType == CustomsAndExciseReportTypeList.Codes.PTT || EM_MessageType == CustomsAndExciseReportTypeList.Codes.PCI)
				{
					return new ZDateTime(dateTime.Year, dateTime.Month, 1);
				}
				else
				{
					return dateTime;
				}
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		public ZPropertyInfo MessageDateInfo => GetZPropertyInfo(nameof(MessageDate));

		[ResourceStringData("00F8735E-8DEE-4681-A230-64E1EFF8DE95", Caption = "Interchange Number")]
		public ZString InterchangeNum => Interchange?.EI_InterchangeNum ?? ZString.Empty;

		[ResourceStringData("2AD2076B-9E53-495E-BD94-3184EF07AF63", Caption = "Interchange Type")]
		public ZString InterchangeType => Interchange?.EI_InterchangeType ?? ZString.Empty;

		[ResourceStringData("C20ED32B-A488-4393-AD86-0C8A1FAAA1C0", Caption = "Body Text")]
		public ZString BodyText => Interchange?.EI_BodyText ?? ZString.Empty;

		public ZPropertyInfo DateInfo => GetZPropertyInfo(nameof(MessageDate));

		void UpdateMessageInterpretation() => EM_MessageInterpretation = MessageInterpretation();

		ZString MessageInterpretation()
		{
			var messageType = EM_MessageType;
			if (messageType.IsEmpty)
			{
				return string.Empty;
			}
			else
			{
				var reportPeriodRow = string.Empty;
				switch (messageType)
				{
					case CustomsAndExciseReportTypeList.Codes.PSR:
					case CustomsAndExciseReportTypeList.Codes.PTT:
					case CustomsAndExciseReportTypeList.Codes.PCT:
					case CustomsAndExciseReportTypeList.Codes.PCI:
						reportPeriodRow = $"<tr><td>Report Period</td><td>{MessageDate.ToString("MMM-yy")}</td></tr>";
						break;
					case CustomsAndExciseReportTypeList.Codes.DSR:
					case CustomsAndExciseReportTypeList.Codes.DTT:
					case CustomsAndExciseReportTypeList.Codes.DCT:
						reportPeriodRow = $"<tr><td>Report Period</td><td>{MessageDate.ToShortDateString()}</td></tr>";
						break;
				}

				return $@"<html><body style='font-family: arial;'>
					<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
						<tr><td>Message Type</td><td>{messageType}</td></tr>
						{reportPeriodRow}
						<tr><td>Created Date</td><td>{EM_SystemCreateTimeUtc}</td></tr>
					</table></body></html>";
			}
		}

		public bool MessageType_ReadOnly => HasResponseMessage;

		public bool MessageDate_ReadOnly => EM_MessageType == CustomsAndExciseReportTypeList.Codes.UDR || EM_MessageType == CustomsAndExciseReportTypeList.Codes.BAL || HasResponseMessage;

		bool HasResponseMessage => Messages.Count > 1;

		public override string CollationKey => $"{EM_MessageType}{MessageDate.ToString(Constants.DateTimeFormat.ShortDateTime)}";

		#region Messages

		public EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = new EDIMessageCollection(this);
					ediMessages.Load();
					ediMessages.Add(this);
					ediMessages.Sort(EDIMessageSchema.EM_SystemCreateTimeUtc.Name);
				}
				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		#endregion

		#region IRelatedJob
		public ZString JobNumber => EM_MessageNum;

		public ZString JobDescription => EM_MessageType;

		public ZString JobStatus => EM_Status;

		public ControllerID ControllerID => ControllerIDs.Customs.IE.CustomsAndExciseReports;

		public Guid BusinessObjectPK => PK.ToGuid();
		#endregion

		#region Overrides

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			var readyToBeSent = IsMessageReadyToBeSent;
			if (readyToBeSent && EM_Status.IsEmpty)
			{
				EM_Status = EDIMessage.Status.Queued;
			}
			else if (!readyToBeSent && EM_Status == EDIMessage.Status.Queued)
			{
				EM_Status = ZString.Empty;
			}
		}

		bool IsMessageReadyToBeSent => EM_MessageType.ToString() switch
		{
			CustomsAndExciseReportTypeList.Codes.PSR or
			CustomsAndExciseReportTypeList.Codes.PCT or
			CustomsAndExciseReportTypeList.Codes.PTT or
			CustomsAndExciseReportTypeList.Codes.PCI or
			CustomsAndExciseReportTypeList.Codes.DSR or
			CustomsAndExciseReportTypeList.Codes.DCT or
			CustomsAndExciseReportTypeList.Codes.DTT => MessageDate.IsValid,
			CustomsAndExciseReportTypeList.Codes.UDR or CustomsAndExciseReportTypeList.Codes.BAL => true,
			_ => false,
		};

		public new CustomsAndExciseReportOutboundMessageLookups Lookups => (CustomsAndExciseReportOutboundMessageLookups)base.Lookups;
		protected override EDIMessageLookups GetNewLookups() => new CustomsAndExciseReportOutboundMessageLookups(this);
		public new CustomsAndExciseReportOutboundMessageValidation Validation => (CustomsAndExciseReportOutboundMessageValidation)base.Validation;
		protected override EDIMessageValidation GetNewValidation() => new CustomsAndExciseReportOutboundMessageValidation(this);

		protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.IEMessageControlNumber(EDIMessage.ApplicationCodes.IECustomsAndExcise).GetNextFormatted(Factory) : (string)EM_MessageNum;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_Status = ZString.Empty;
		}
		#endregion
	}
}
