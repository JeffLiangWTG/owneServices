using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaGJobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public DeltaGJobDeclarationMessageSendingObject(Declaration.CusEntryHeader header) : base(header)
		{
			sequenceNumber = header.CH_SequenceNumber;
			doNotRecalculateEntrySubstyle = false;
			if (!IsValidationSuspended)
			{
				Validation.ValidateAll();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new sealed class Schema : Customs.Business.JobDeclarationMessageSendingObject.Schema
		{
			Schema()
			{
			}

			public const string SequenceNumber = "SequenceNumber";
			public const int SequenceNumberMaxLength = 10;
			public const string ReplacementDeclarationType = "ReplacementDeclarationType";
			public const int ReplacementDeclarationTypeMaxLength = 35;
			public const string DateMessage = "DateMessage";
			public const int AmendmentReasonMaxLength = 520;
			public const string DoNotRecalculateEntrySubstyle = "DoNotRecalculateEntrySubstyle";
			public const string EntryInstructionDescription = "EntryInstructionDescription";
			public const string TriggeringPointForValidation = "TriggeringPointForValidation";
			public const int TriggeringPointForValidationMaxLength = 3;
		}

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new DeltaGJobDeclarationMessageSendingObjectValidation(this);
		}

		public new DeltaGJobDeclarationMessageSendingObjectValidation Validation => (DeltaGJobDeclarationMessageSendingObjectValidation)base.Validation;

		#region ShouldSend

		public override ZBool ShouldSend
		{
			get
			{
				return base.ShouldSend;
			}
			set
			{
				base.ShouldSend = value;
				if (!ShouldSend)
				{
					VOCReason = ZString.Empty;
				}
			}
		}

		internal ZDateTime LastMessageSentTime
		{
			get
			{
				if (!lastMessageSentTime.HasValue)
				{
					lastMessageSentTime = Header.Messages.LastOutgoingMessage?.EM_SystemCreateTimeUtc ?? ZDateTime.Empty;
				}
				return lastMessageSentTime.Value;
			}
		}
		ZDateTime? lastMessageSentTime;

		#endregion

		#region MessageType
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|MessageType", ShortCaption = "Msg. Type", Caption = "Message Type")]
		[List(nameof(MessageTypesList))]
		public new ZString MessageType
		{
			get => base.MessageType;
			set
			{
				base.MessageType = value;
				ChangeAcknowledgementIndicator = GetRegularJustification();
			}
		}

		protected override bool MessageType_ReadOnly => false;

		ZString GetRegularJustification()
		{
			var result = ZString.Empty;

			if (MessageType == EntryActionCodeList.Codes.REC)
			{
				result = Customs.FR.Business.ReasonCodeList.Codes.C173;
			}
			else if (MessageType == EntryActionCodeList.Codes.INV)
			{
				int.TryParse(Header.CH_EntryStatus, out int entryStatusWeight);
				var justificationCode = entryStatusWeight < 100 ? Customs.FR.Business.ReasonCodeList.Codes.C174 : Customs.FR.Business.ReasonCodeList.Codes.C148;
				result = justificationCode;
			}

			return result;
		}

		#endregion

		#region MovementReferenceNumber

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|MovementReferenceNumber", ShortCaption = "MRN", Caption = "Movement Reference Number")]
		public override ZString MovementReferenceNumber
		{
			get => base.MovementReferenceNumber;
			set => base.MovementReferenceNumber = value;
		}

		#endregion

		#region VocReason

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|AmendmentReason", ShortCaption = "Reason", Caption = "Amendment Reason")]
		[MaxLength(Schema.AmendmentReasonMaxLength)]
		public override ZString VOCReason
		{
			get => base.VOCReason;
			set => base.VOCReason = value;
		}

		#endregion

		#region Entry type
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|Entry type", ShortCaption = "En. Code", Caption = "Entry type")]
		public override ZString DeclarationType
		{
			get { return base.EntryType; }
		}

		#endregion

		#region ChangeAcknowledgementIndicator

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|ChangeAcknowledgementIndicator", ShortCaption = "Regular Justification", Caption = "Regular Justification Code")]
		[MaxLength(Schema.ChangeAcknowledgementIndicatorMaxLength)]
		[List(nameof(ReasonCodeList))]
		public override ZString ChangeAcknowledgementIndicator
		{
			get { return base.ChangeAcknowledgementIndicator; }
			set { base.ChangeAcknowledgementIndicator = value; }
		}

		protected override bool ChangeAcknowledgementIndicator_ReadOnly => !IsAmendOrDelete;

		#endregion

		#region Sequencenumber
		ZInt sequenceNumber;

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|SequenceNumber", ShortCaption = "Seq. No.", Caption = "Sequence Number")]
		public ZInt SequenceNumber
		{
			get { return sequenceNumber; }
			set
			{
				sequenceNumber = value;
				SequenceNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SequenceNumberInfo => GetZPropertyInfo(Schema.SequenceNumber);

		#endregion

		#region doNotRecalculateEntrySubstyle
		ZBool doNotRecalculateEntrySubstyle;

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|DoNotRecalculateEntrySubstyle", ShortCaption = "Keep Sub Style", Caption = "Keep Entered Sub Style")]
		public ZBool DoNotRecalculateEntrySubstyle
		{
			get { return doNotRecalculateEntrySubstyle; }
			set
			{
				doNotRecalculateEntrySubstyle = value;
				DoNotRecalculateEntrySubstyleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DoNotRecalculateEntrySubstyleInfo => GetZPropertyInfo(Schema.DoNotRecalculateEntrySubstyle);
		#endregion

		#region ReplacementDeclarationType

		ZString replacementDeclarationType;
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|ReplacementDeclarationType", ShortCaption = "Replacement Declaration", Caption = "Replacement Declaration Type")]
		[MaxLength(Schema.ReplacementDeclarationTypeMaxLength)]
		[List(nameof(ReplacementDeclarationList))]
		public ZString ReplacementDeclarationType
		{
			get { return replacementDeclarationType; }
			set
			{
				if (replacementDeclarationType != value)
				{
					CheckMaximumLength(ReplacementDeclarationTypeInfo, value);

					replacementDeclarationType = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateReplacementDeclarationType();
					}
					ReplacementDeclarationTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReplacementDeclarationTypeInfo => GetZPropertyInfo(Schema.ReplacementDeclarationType);

		#endregion

		#region Date message

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|DateMessage", ShortCaption = "Date", Caption = "Date")]
		public ZDateTime DateMessage => LastMessageSentTime;

		public ZPropertyInfo DateMessageInfo => GetZPropertyInfo(Schema.DateMessage);

		#endregion

		#region Trigger Point For Validation

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|TriggerPointForValidation", Caption = "Triggering Point for Validation", ShortCaption = "VAA Trig. Point")]
		[MaxLength(Schema.TriggeringPointForValidationMaxLength)]
		[ReadOnlyMember(nameof(TriggeringPointForValidation_ReadOnly))]
		public ZString TriggeringPointForValidation
		{
			get
			{
				return Header.CH_TriggeringPointForValidation;
			}
		}

		public ZPropertyInfo TriggeringPointForValidationInfo => GetZPropertyInfo(Schema.TriggeringPointForValidation);

		protected bool TriggeringPointForValidation_ReadOnly => true;

		#endregion

		#region SetDefaultValuesFromEntry

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = IsOneAndOnlyEntry
							|| MovementReferenceNumber.IsEmpty
							|| !IsCleared;

			if (ShouldSend)
			{
				ShouldSend = !Header.IsCancelledWithCustoms;
			}

			VOCReason = Header.CH_CustomsMessageRemarks;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		protected override ZString GetDefaultMessageType()
		{
			string result = "";
			if (FRCustomsDataRegistry.DeltaGFallbackIsActive || Header.IsDeltaGFallbackInactiveAndNotRegularised)
			{
				result = EntryActionCodeList.Codes.RPS;
			}
			else if (Header.Declaration.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1)
			{
				switch (Header.CH_EntryStatus)
				{
					case "":
					case EntryStatusDescriptionCodeList.Codes.ES010:
					case EntryStatusDescriptionCodeList.Codes.ES040:
						result = GetInitialMessageType();
						break;
					case EntryStatusDescriptionCodeList.Codes.ES050:
						result = EntryActionCodeList.Codes.MAP;
						break;
					case EntryStatusDescriptionCodeList.Codes.ES055:
						result = EntryActionCodeList.Codes.EAV;
						break;
					case EntryStatusDescriptionCodeList.Codes.ES060:
					case EntryStatusDescriptionCodeList.Codes.ES061:
					case EntryStatusDescriptionCodeList.Codes.ES070:
					case EntryStatusDescriptionCodeList.Codes.ES075:
					case EntryStatusDescriptionCodeList.Codes.ES080:
					case EntryStatusDescriptionCodeList.Codes.ES083:
					case EntryStatusDescriptionCodeList.Codes.ES085:
					case EntryStatusDescriptionCodeList.Codes.ES100:
					case EntryStatusDescriptionCodeList.Codes.ES101:
					case EntryStatusDescriptionCodeList.Codes.ES120:
						result = EntryActionCodeList.Codes.REC;
						break;
					case EntryStatusDescriptionCodeList.Codes.ES114:
						result = EntryActionCodeList.Codes.VAR;
						break;
					default:
						break;
				}
			}
			else if (Header.Declaration.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G2)
			{
				switch (Header.CH_EntryStatus)
				{
					default:
					case "":
					case EntryStatusDescriptionCodeList.Codes.ES010:
					case EntryStatusDescriptionCodeList.Codes.ES040:
						result = GetInitialMessageType();
						break;
					case EntryStatusDescriptionCodeList.Codes.ES050:
						result = EntryActionCodeList.Codes.MDA;
						break;
					case EntryStatusDescriptionCodeList.Codes.ES060:
					case EntryStatusDescriptionCodeList.Codes.ES061:
					case EntryStatusDescriptionCodeList.Codes.ES070:
					case EntryStatusDescriptionCodeList.Codes.ES075:
					case EntryStatusDescriptionCodeList.Codes.ES080:
					case EntryStatusDescriptionCodeList.Codes.ES082:
					case EntryStatusDescriptionCodeList.Codes.ES083:
					case EntryStatusDescriptionCodeList.Codes.ES085:
						result = EntryActionCodeList.Codes.REC;
						break;
					case EntryStatusDescriptionCodeList.Codes.ES100:
					case EntryStatusDescriptionCodeList.Codes.ES101:
					case EntryStatusDescriptionCodeList.Codes.ES130:
						result = EntryActionCodeList.Codes.D2M;
						break;
					case EntryStatusDescriptionCodeList.Codes.ES140:
						result = EntryActionCodeList.Codes.INV;
						break;
				}
			}

			return result;
		}

		public ZString GetInitialMessageType()
		{
			var result = ZString.Empty;

			if (Header.EntryInstruction?.IsNeitherPrelodgedNorLodged == false)
			{
				result = Header.EntryInstruction.IsPrelodgedSubstyle ? EntryActionCodeList.Codes.ANT : EntryActionCodeList.Codes.VAL;
			}

			return result;
		}

		public bool IsAmendOrDelete => IsAmend || IsCancel;

		protected override bool IsAmendCore => MessageType == EntryActionCodeList.Codes.REC;

		protected override bool IsCancelCore => MessageType == EntryActionCodeList.Codes.INV;

		bool IsCleared => EntryStatus == EntryStatusList.Codes.Clear;

		#endregion

		#region Lookup Lists

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public CodeDescriptionPairList MessageTypesList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (FRCustomsDataRegistry.DeltaGFallbackIsActive || Header.IsDeltaGFallbackInactiveAndNotRegularised)
				{
					result.AddPair(EntryActionCodeList.Codes.RPS, EntryActionCodeList.Descriptions.RPS);
				}
				else if (Header.Declaration.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1)
				{
					switch (Header.CH_EntryStatus)
					{
						case "":
						case EntryStatusDescriptionCodeList.Codes.ES010:
						case EntryStatusDescriptionCodeList.Codes.ES040:
							result.AddPair(EntryActionCodeList.Codes.ANT, EntryActionCodeList.Descriptions.ANT);
							result.AddPair(EntryActionCodeList.Codes.VAL, EntryActionCodeList.Descriptions.VAL);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES050:
							result.AddPair(EntryActionCodeList.Codes.ANA, EntryActionCodeList.Descriptions.ANA);
							result.AddPair(EntryActionCodeList.Codes.MAP, EntryActionCodeList.Descriptions.MAP);
							result.AddPair(EntryActionCodeList.Codes.VAA, EntryActionCodeList.Descriptions.VAA);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES055:
							result.AddPair(EntryActionCodeList.Codes.EAV, EntryActionCodeList.Descriptions.EAV);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES060:
						case EntryStatusDescriptionCodeList.Codes.ES061:
						case EntryStatusDescriptionCodeList.Codes.ES070:
						case EntryStatusDescriptionCodeList.Codes.ES075:
						case EntryStatusDescriptionCodeList.Codes.ES080:
						case EntryStatusDescriptionCodeList.Codes.ES083:
						case EntryStatusDescriptionCodeList.Codes.ES085:
						case EntryStatusDescriptionCodeList.Codes.ES100:
						case EntryStatusDescriptionCodeList.Codes.ES101:
						case EntryStatusDescriptionCodeList.Codes.ES120:
							result.AddPair(EntryActionCodeList.Codes.CMP, EntryActionCodeList.Descriptions.CMP);
							result.AddPair(EntryActionCodeList.Codes.INV, EntryActionCodeList.Descriptions.INV);
							result.AddPair(EntryActionCodeList.Codes.REC, EntryActionCodeList.Descriptions.REC);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES114:
							result.AddPair(EntryActionCodeList.Codes.ANR, EntryActionCodeList.Descriptions.ANR);
							result.AddPair(EntryActionCodeList.Codes.VAR, EntryActionCodeList.Descriptions.VAR);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES090:
						case EntryStatusDescriptionCodeList.Codes.ES115:
						case EntryStatusDescriptionCodeList.Codes.ES116:
						case EntryStatusDescriptionCodeList.Codes.ES117:
						case EntryStatusDescriptionCodeList.Codes.ES118:
						case EntryStatusDescriptionCodeList.Codes.ES119:
						case EntryStatusDescriptionCodeList.Codes.ES150:
						case EntryStatusDescriptionCodeList.Codes.ES151:
							break;
						default:
							result.AddPair(EntryActionCodeList.Codes.ANA, EntryActionCodeList.Descriptions.ANA);
							result.AddPair(EntryActionCodeList.Codes.ANR, EntryActionCodeList.Descriptions.ANR);
							result.AddPair(EntryActionCodeList.Codes.ANT, EntryActionCodeList.Descriptions.ANT);
							result.AddPair(EntryActionCodeList.Codes.CMP, EntryActionCodeList.Descriptions.CMP);
							result.AddPair(EntryActionCodeList.Codes.EAV, EntryActionCodeList.Descriptions.EAV);
							result.AddPair(EntryActionCodeList.Codes.INV, EntryActionCodeList.Descriptions.INV);
							result.AddPair(EntryActionCodeList.Codes.MAP, EntryActionCodeList.Descriptions.MAP);
							result.AddPair(EntryActionCodeList.Codes.REC, EntryActionCodeList.Descriptions.REC);
							result.AddPair(EntryActionCodeList.Codes.RPS, EntryActionCodeList.Descriptions.RPS);
							result.AddPair(EntryActionCodeList.Codes.VAA, EntryActionCodeList.Descriptions.VAA);
							result.AddPair(EntryActionCodeList.Codes.VAL, EntryActionCodeList.Descriptions.VAL);
							result.AddPair(EntryActionCodeList.Codes.VAR, EntryActionCodeList.Descriptions.VAR);
							break;
					}
				}
				else if (Header.Declaration.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G2)
				{
					switch (Header.CH_EntryStatus)
					{
						case "":
						case EntryStatusDescriptionCodeList.Codes.ES010:
						case EntryStatusDescriptionCodeList.Codes.ES040:
							result.AddPair(EntryActionCodeList.Codes.ANT, EntryActionCodeList.Descriptions.ANT);
							result.AddPair(EntryActionCodeList.Codes.VAL, EntryActionCodeList.Descriptions.VAL);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES050:
							result.AddPair(EntryActionCodeList.Codes.ANN, EntryActionCodeList.Descriptions.ANN);
							result.AddPair(EntryActionCodeList.Codes.MDA, EntryActionCodeList.Descriptions.MDA);
							result.AddPair(EntryActionCodeList.Codes.VAA, EntryActionCodeList.Descriptions.VAA);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES060:
						case EntryStatusDescriptionCodeList.Codes.ES061:
						case EntryStatusDescriptionCodeList.Codes.ES070:
						case EntryStatusDescriptionCodeList.Codes.ES075:
						case EntryStatusDescriptionCodeList.Codes.ES080:
						case EntryStatusDescriptionCodeList.Codes.ES082:
						case EntryStatusDescriptionCodeList.Codes.ES083:
						case EntryStatusDescriptionCodeList.Codes.ES085:
						case EntryStatusDescriptionCodeList.Codes.ES100:
						case EntryStatusDescriptionCodeList.Codes.ES101:
							result.AddPair(EntryActionCodeList.Codes.D2M, EntryActionCodeList.Descriptions.D2M);
							result.AddPair(EntryActionCodeList.Codes.INV, EntryActionCodeList.Descriptions.INV);
							result.AddPair(EntryActionCodeList.Codes.REC, EntryActionCodeList.Descriptions.REC);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES062:
						case EntryStatusDescriptionCodeList.Codes.ES063:
						case EntryStatusDescriptionCodeList.Codes.ES081:
						case EntryStatusDescriptionCodeList.Codes.ES090:
						case EntryStatusDescriptionCodeList.Codes.ES111:
						case EntryStatusDescriptionCodeList.Codes.ES112:
						case EntryStatusDescriptionCodeList.Codes.ES113:
						case EntryStatusDescriptionCodeList.Codes.ES131:
						case EntryStatusDescriptionCodeList.Codes.ES132:
						case EntryStatusDescriptionCodeList.Codes.ES150:
						case EntryStatusDescriptionCodeList.Codes.ES151:
							break;
						case EntryStatusDescriptionCodeList.Codes.ES130:
							result.AddPair(EntryActionCodeList.Codes.D2M, EntryActionCodeList.Descriptions.D2M);
							result.AddPair(EntryActionCodeList.Codes.INV, EntryActionCodeList.Descriptions.INV);
							result.AddPair(EntryActionCodeList.Codes.REC, EntryActionCodeList.Descriptions.REC);
							break;
						case EntryStatusDescriptionCodeList.Codes.ES140:
							result.AddPair(EntryActionCodeList.Codes.INV, EntryActionCodeList.Descriptions.INV);
							break;
						default:
							result.AddPair(EntryActionCodeList.Codes.ANT, EntryActionCodeList.Descriptions.ANT);
							result.AddPair(EntryActionCodeList.Codes.ANN, EntryActionCodeList.Descriptions.ANN);
							result.AddPair(EntryActionCodeList.Codes.D2M, EntryActionCodeList.Descriptions.D2M);
							result.AddPair(EntryActionCodeList.Codes.INV, EntryActionCodeList.Descriptions.INV);
							result.AddPair(EntryActionCodeList.Codes.MDA, EntryActionCodeList.Descriptions.MDA);
							result.AddPair(EntryActionCodeList.Codes.MDV, EntryActionCodeList.Descriptions.MDV);
							result.AddPair(EntryActionCodeList.Codes.REC, EntryActionCodeList.Descriptions.REC);
							result.AddPair(EntryActionCodeList.Codes.RPS, EntryActionCodeList.Descriptions.RPS);
							result.AddPair(EntryActionCodeList.Codes.VAA, EntryActionCodeList.Descriptions.VAA);
							result.AddPair(EntryActionCodeList.Codes.VAL, EntryActionCodeList.Descriptions.VAL);

							break;
					}
				}
				return result;
			}
		}
		public CodeDescriptionPairList ReasonCodeList => GetReasonCodeList();

		CodeDescriptionPairList GetReasonCodeList()
		{
			var result = new CodeDescriptionPairList();

			if (MessageType == EntryActionCodeList.Codes.REC)
			{
				result.AddPair(Customs.FR.Business.ReasonCodeList.Codes.C173, Customs.FR.Business.ReasonCodeList.Descriptions.C173);
			}
			else if (MessageType == EntryActionCodeList.Codes.INV)
			{
				result.AddPair(Customs.FR.Business.ReasonCodeList.Codes.C174, Customs.FR.Business.ReasonCodeList.Descriptions.C174);
				result.AddPair(Customs.FR.Business.ReasonCodeList.Codes.C148, Customs.FR.Business.ReasonCodeList.Descriptions.C148);
			}

			return result;
		}

		public CodeDescriptionPairList ReplacementDeclarationList => Factory.GetCachedValue<ReplacementDeclarationTypeList>();

		public CodeDescriptionPairList ReasonCodeShortDescriptionList => GetReasonCodeShortList();

		CodeDescriptionPairList GetReasonCodeShortList()
		{
			var result = new CodeDescriptionPairList();

			if (MessageType == EntryActionCodeList.Codes.REC)
			{
				result.AddPair(Customs.FR.Business.ReasonCodeShortList.Codes.C173, Customs.FR.Business.ReasonCodeShortList.Descriptions.C173);
			}
			else if (MessageType == EntryActionCodeList.Codes.INV)
			{
				result.AddPair(Customs.FR.Business.ReasonCodeShortList.Codes.C174, Customs.FR.Business.ReasonCodeShortList.Descriptions.C174);
				result.AddPair(Customs.FR.Business.ReasonCodeShortList.Codes.C148, Customs.FR.Business.ReasonCodeShortList.Descriptions.C148);
			}

			return result;
		}

		#endregion

		#region EntryInstruction
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.FR.Business.MessageSending.MessageSendingObject|EntryInstructionDescription", ShortCaption = "Entry Type Desc.", Caption = "Entry Type Description")]
		public ZString EntryInstructionDescription => Header.EntryInstruction?.CEI_Description ?? ZString.Empty;
		public ZPropertyInfo EntryInstructionDescriptionInfo => GetZPropertyInfo(Schema.EntryInstructionDescription);
		#endregion
	}
}
