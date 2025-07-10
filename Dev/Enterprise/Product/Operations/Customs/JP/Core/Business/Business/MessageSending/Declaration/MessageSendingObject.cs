using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class MessageSendingObject : JobDeclarationMessageSendingObject
	{
		public MessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public static class JPSchema
		{
			public const string EntryNumber = nameof(MessageSendingObject.EntryNumber);
			public const string InputReference = nameof(MessageSendingObject.InputReference);
			public const string Status = nameof(MessageSendingObject.Status);
			public const string ProcedureCode = nameof(MessageSendingObject.ProcedureCode);
			public const string Action = nameof(MessageSendingObject.Action);
			public const string DeclarationCorrectionCopyRequest = nameof(MessageSendingObject.DeclarationCorrectionCopyRequest);
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		#region Status

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|MessageStatus", Caption = "Message Status")]
		public ZString Status => Header.CH_Status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(JPSchema.Status);

		#endregion

		#region EntryStatus

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|EntryStatus", Caption = "Customs Status")]
		public override ZString EntryStatus => base.EntryStatus;

		#endregion

		#region EntryNumber

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|EntryNumber", Caption = "Declaration Number")]
		public ZString EntryNumber => Header.EntryNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(JPSchema.EntryNumber);

		#endregion

		#region EntryNumber

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|InputReference", Caption = "Input Reference")]
		public ZString InputReference => Header.CH_BGMReference;

		public ZPropertyInfo InputReferenceInfo => GetZPropertyInfo(JPSchema.InputReference);

		#endregion

		#region ProcedureCode

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|ProcedureCode", Caption = "Procedure Code")]
		[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.ProcedureCodeList))]
		[ReadOnly(true)]
		public ZString ProcedureCode
		{
			get => procedureCode;
			set
			{
				SetNonPersistentPropertyValue(ProcedureCodeInfo, ref procedureCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeclarationCorrectionCopyRequest();
				}
			}
		}

		ZString procedureCode;

		public ZPropertyInfo ProcedureCodeInfo => GetZPropertyInfo(JPSchema.ProcedureCode);

		#endregion

		#region Action

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|Action", Caption = "Action")]
		[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.MessageActionList))]
		public ZString Action
		{
			get => action;
			set
			{
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
				ShouldSendInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateAction();
				}
			}
		}

		ZString action;

		public ZPropertyInfo ActionInfo => GetZPropertyInfo(JPSchema.Action);

		public bool Action_ReadOnly { get; set; }

		#endregion

		#region DeclarationCorrectionCopyRequest

		[ResourceStringData("Enterprise.Customs.JP.Business.MessageSendingObject|DeclarationCorrectionCopyRequest", Caption = "Declaration Correction Copy Request", MediumCaption = "Copy Request", ShortCaption = "Copy?", FullDescription = "Indicates if declaration correction copies are requested.")]
		public ZBool DeclarationCorrectionCopyRequest
		{
			get => declarationCorrectionCopyRequest;
			set
			{
				SetNonPersistentPropertyValue(DeclarationCorrectionCopyRequestInfo, ref declarationCorrectionCopyRequest, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeclarationCorrectionCopyRequest();
				}
			}
		}

		ZBool declarationCorrectionCopyRequest;

		public ZPropertyInfo DeclarationCorrectionCopyRequestInfo => GetZPropertyInfo(JPSchema.DeclarationCorrectionCopyRequest);

		#endregion

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();

			var messageType = Header.Declaration?.JE_MessageType;

			if (!string.IsNullOrEmpty(messageType))
			{
				ProcedureCode = NACCSStateMachineBuilder.Build(Header).PickNextPhase();
			}

			DefaultECRActionIfNeeded();
		}

		void DefaultECRActionIfNeeded()
		{
			if (Header.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				if (entryInstruction.ExportControlNumber.IsEmpty)
				{
					Action = ActionList.Codes.Nine;
				}
				else if (entryInstruction.IsExportControlEntryNumSystemGenerated)
				{
					Action = ActionList.Codes.Five;
				}
			}
		}

		public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new MessageSendingObjectValidation(this);

		public MessageSendingObjectLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new MessageSendingObjectLookups(this);
				}
				return lookups;
			}
		}
		MessageSendingObjectLookups lookups;
	}
}
