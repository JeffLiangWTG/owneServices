using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public abstract class DeclarationMessageSendingObject : JobDeclarationMessageSendingObject, IMessageSendingObject
	{
		protected DeclarationMessageSendingObject(CusEntryHeader header)
			: base(header)
		{
		}

		public new class Schema : Customs.Business.JobDeclarationMessageSendingObject.Schema
		{
			public const string SubmittedDate = "SubmittedDate";
			public const string CustomsStatus = "CustomsStatus";
			public const string MessageStatusDescription = "MessageStatusDescription";
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public BusinessObject MessageAttachee => Header;

		public abstract ZString GetMessageOwner();

		public abstract ZString GetMessageTypeForEDIMessage();

		public abstract ZString GetMessageText();

		public virtual ZGuid GetGlbExternalPasswordPK() => ZGuid.Empty;

		public virtual ZString GetApplicationReference() => Header.MovementReferenceNumber;

		#region MessageType

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.JobDeclarationMessageSendingObject|MessageType", ShortCaption = "Msg. Type")]
		[List(nameof(MessageTypesList))]
		public override ZString MessageType
		{
			get => base.MessageType;
			set => base.MessageType = value;
		}

		public virtual CodeDescriptionPairList MessageTypesList { get; }

		#endregion

		#region SubmittedDate

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.JobDeclarationMessageSendingObject|SubmittedDate", ShortCaption = "Sub. Date", Caption = "Submitted Date")]
		public ZDateTime SubmittedDate => Header.CH_EntrySubmittedDate;

		public ZPropertyInfo SubmittedDateInfo => GetZPropertyInfo(Schema.SubmittedDate);

		#endregion

		#region CustomsStatus

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.JobDeclarationMessageSendingObject|CustomsStatus", ShortCaption = "Status", Caption = "Customs Status")]
		public ZString CustomsStatus => Header.CH_Status;

		public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(Schema.CustomsStatus);

		#endregion

		#region MessageStatusDescription

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.JobDeclarationMessageSendingObject|MessageStatusDescription", Caption = "Message Status Description")]
		public ZString MessageStatusDescription => Header.MessageStatusDescription;

		#endregion

		#region EntryInstructionDescription

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.MessageSending.JobDeclarationMessageSendingObject|EntryInstructionDescription", Caption = "Entry Instruction Description")]
		public ZString EntryInstructionDescription => Header.EntryInstruction?.CEI_Description ?? ZString.Empty;

		#endregion
	}
}
