using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CusEntryHeaderMessageSendingAction : Customs.Business.BaseMessageSendingObject, IMessageSendingAction
	{
		protected CusEntryHeaderMessageSendingAction(CusEntryHeader entryHeader) : base(entryHeader.Factory)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			EntryInstruction = EntryHeader.EntryInstruction;
		}

		public readonly CusEntryHeader EntryHeader;
		public readonly CusEntryInstruction EntryInstruction;

		#region Fields

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				UpdateValidationModes(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryStatus();
				}
			}
		}

		protected void UpdateValidationModes(bool isSpecificModeEnabled)
		{
			if (EntryInstruction != null)
			{
				EntryInstruction.ValidationModesCalculator.UpdateValidationModes(GetApplicableValidationModes(), isSpecificModeEnabled);
			}
		}
		protected virtual EU.Business.Declaration.ValidationModes GetApplicableValidationModes() => EU.Business.Declaration.ValidationModes.None;

		[ResourceStringData("AF1175DC-3EA6-4A1F-B909-D10193FFBA98", Caption = "Sub Style")]
		public ZString SubStyle => EntryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

		public ZPropertyInfo SubStyleInfo => GetZPropertyInfo(nameof(SubStyle));

		[ResourceStringData("19A23E8C-5E32-43BC-96B1-1CE45C744FF7", Caption = "Description")]
		public ZString Description => EntryHeader.EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		[ResourceStringData("292B9C3A-FC81-46D3-8D1F-676A1A091CDD", Caption = "LRN", FullDescription = "Local Reference Number")]
		public ZString LocalReferenceNumber => EntryHeader.CH_BGMReference;

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(nameof(LocalReferenceNumber));

		[ResourceStringData("713A54FA-0A0F-4F3C-93ED-FBF6F24A149B", Caption = "Message Status")]
		public ZString MessageStatus => EntryHeader.CH_Status;

		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(nameof(MessageStatus));

		[ResourceStringData("2FCB894A-C983-4C8D-AC3D-7952656CBCEC", Caption = "Entry Status")]
		public ZString EntryStatus => EntryHeader.EntryHeaderStatusDescription;

		public ZPropertyInfo EntryStatusInfo => GetZPropertyInfo(nameof(EntryStatus));

		[ResourceStringData("331342E0-CAB6-4EA3-9003-448C76821D95", Caption = "Declaration Type")]
		public ZString DeclarationType => EntryHeader.EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZPropertyInfo DeclarationTypeInfo => GetZPropertyInfo(nameof(DeclarationType));

		[ResourceStringData("AD3F5CA0-4DC2-4E69-973D-D2B4B7A18C0B", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderMessageSendingActionLookups.SendingActionTypeList))]
		[MaxLength(EDIMessage.Schema.EM_MessageTypeMaxLength)]
		public virtual ZString MessageType
		{
			get => fMessageType;
			set
			{
				if (SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value))
				{
					if (!settingMessageTypeInProgress)
					{
						var newValue = MessageType;
						var mapping = Lookups?.SendingActionTypeListForDisplay.Cast<ICodeDescription>()
							.FirstOrDefault(x => newValue.EqualsIgnoringCase(x.PK?.ToString()));
						fMessageTypeForDisplay = mapping == null ? newValue : (ZString)mapping.Code;
					}
					ClearIrrelevantData();
					UpdateValidationModes(ShouldSend);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageType();
				}
			}
		}
		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		[ResourceStringData("34F63BA2-DA9E-4EC9-A695-0FDE6B710C47", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderMessageSendingActionLookups.SendingActionTypeListForDisplay))]
		[MaxLength(messageTypeForDisplayMaxLength)]
		public ZString MessageTypeForDisplay
		{
			get => fMessageTypeForDisplay;
			set
			{
				var oldValue = MessageTypeForDisplay;
				if (oldValue != value)
				{
					fMessageTypeForDisplay = value;
					if (!settingMessageTypeInProgress)
					{
						try
						{
							settingMessageTypeInProgress = true;
							if (value.IsEmpty)
							{
								MessageType = ZString.Empty;
							}
							else
							{
								var mapping = Lookups?.SendingActionTypeListForDisplay[value];

								MessageType = mapping == null ? invalidMessageType : (mapping.PK == null ? mapping.Code : mapping.PK.ToString());
							}
						}
						finally
						{
							settingMessageTypeInProgress = false;
						}
					}
				}
			}
		}
		ZString fMessageTypeForDisplay;
		bool settingMessageTypeInProgress;

		public ZPropertyInfo MessageTypeForDisplayInfo => GetWrappedZPropertyInfo(nameof(MessageTypeForDisplay), (object x) => MessageTypeInfo);

		[ResourceStringData("6B2A7F96-ACEA-4FA1-BD78-332C90F94941", Caption = "Message Type Description", MediumCaption = "Description", ShortCaption = "Desc.")]
		public ZString MessageTypeDescription
		{
			get => Lookups.SendingActionTypeListForDisplay.GetDescriptionFromCode(MessageTypeForDisplay);
		}

		public ZPropertyInfo MessageTypeDescriptionInfo => GetZPropertyInfo(nameof(MessageTypeDescription));

		[ResourceStringData("6B59DC88-7EC3-413B-A5CA-52E4FAC232FD", Caption = "Annotation")]
		[ReadOnlyMember(nameof(Annotation_ReadOnly))]
		[MaxLength(512)]
		public ZString Annotation
		{
			get => annotation;
			set
			{
				SetNonPersistentPropertyValue(AnnotationInfo, ref annotation, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAnnotation();
				}
			}
		}
		ZString annotation;

		protected virtual bool Annotation_ReadOnly => false;

		public ZPropertyInfo AnnotationInfo => GetZPropertyInfo(nameof(Annotation));

		#endregion

		#region Lookups & Validations

		protected abstract CusEntryHeaderMessageSendingActionLookups GetNewLookups();

		public CusEntryHeaderMessageSendingActionLookups Lookups => fLookups ?? (fLookups = GetNewLookups());
		CusEntryHeaderMessageSendingActionLookups fLookups;

		public CusEntryHeaderMessageSendingActionValidation Validation => GetNewValidation();

		protected virtual CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new CusEntryHeaderMessageSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		void ClearIrrelevantData()
		{
			if (Annotation_ReadOnly && !Annotation.IsEmpty)
			{
				Annotation = ZString.Empty;
			}
		}

		#endregion

		#region Message Sender

		public MessageSender CreateSender() => (MessageSender)Activator.CreateInstance(SenderType, this);

		protected abstract Type SenderType { get; }

		#endregion

		#region IMessageSendingAction Members
		IMessageAttachee IMessageSendingAction.MessageAttachee => EntryHeader;
		void IMessageSendingAction.AddMessage(OutboundEDIMessage message)
		{
			EntryHeader.Messages.Add(message);
		}
		#endregion

		const int messageTypeForDisplayMaxLength = 6;
		const string invalidMessageType = "!X!";
	}
}
