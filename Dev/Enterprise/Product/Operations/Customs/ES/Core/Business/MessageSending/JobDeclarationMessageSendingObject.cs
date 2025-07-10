using System;
using System.Collections.Immutable;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class JobDeclarationMessageSendingObject : EU.Business.JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessageSendingObject(Declaration.CusEntryHeader header, bool isAutoSend = false)
			: base(header)
		{
			DefaultValuesForAutomation(isAutoSend);
		}

		public new Declaration.CusEntryHeader Header => (Declaration.CusEntryHeader)base.Header;

		public new sealed class Schema : AutoJobDeclarationMessageSendingObject.Schema
		{
			public const string MessageSubType = "MessageSubType";
			public const string MessageStatus = "MessageStatus";
			public const string MRN = "MRN";
			public const string CH_BGMReference = "CH_BGMReference";
			public const string SecurityFlag = "SecurityFlag";
			public const string RequestDispatch = "RequestDispatch";
			public const string ActivateByOperatorFlag = "ActivateByOperatorFlag";

			public const int MRNMaxLength = 18;
		}

		ZBool IsEditableProperty => !ShouldSend;

		public ZBool IsUcc6 => Header.IsUCC6;

		public ZBool IsPOUS => Header.ZG_POUSVersion > POUSVersionCodes.NoPOUS;

		ZBool IsPOUS2 => Header.ZG_POUSVersion > POUSVersionCodes.POUS;

		ZBool IsImportH1 => IsImport && IsUcc6;

		#region New Export UCC6 Properties

		public ReasonForCancellation ReasonForCancellation => reasonForCancellation ?? (reasonForCancellation = new ReasonForCancellation(Factory));
		ReasonForCancellation reasonForCancellation;

		[List(nameof(ActivateByOperatorFlagList))]
		public ZString ActivateByOperatorFlag
		{
			get => activateByOperatorFlag;
			set => SetNonPersistentPropertyValue(ActivateByOperatorFlagInfo, ref activateByOperatorFlag, value);
		}
		ZString activateByOperatorFlag;

		void SetDefaultActivateByOperator()
		{
			var entryInstruction = Header.EntryInstruction;

			ActivateByOperatorFlag = IsImportH1
										? entryInstruction.ZG_ActivateByOperator
											? ActivateByOperatorCodeList.Codes.ActivationWillBeManuallyDoneByDeclarant
											: ActivateByOperatorCodeList.Codes.ActivationWillBeDoneAutomaticallyByCustoms
										: ZString.Empty;
		}

		public ZPropertyInfo ActivateByOperatorFlagInfo => GetZPropertyInfo(Schema.ActivateByOperatorFlag);

		public ZBool ActivateByOperatorFlagVisible => IsImportH1 && (MessageType == "DCP" || MessageType == "DSP");

		[List(nameof(SecurityFlagList))]
		public ZString SecurityFlag
		{
			get => securityFlag;
			set => SetNonPersistentPropertyValue(SecurityFlagInfo, ref securityFlag, value);
		}
		ZString securityFlag;

		void SetDefaultSecurityFlag()
		{
			var result = ZString.Empty;
			var declaration = Header.Declaration;

			if (declaration.IsExport && IsUcc6 && declaration.JE_MessageSubType != EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory)
			{
				result = declaration.ZG_IsSecurityDeclaration ? SecurityFlagCodeList.Codes.Exs : SecurityFlagCodeList.Codes.NotUsedForSafetyAndSecurityPurposes;
			}

			SecurityFlag = result;
		}

		public ZPropertyInfo SecurityFlagInfo => GetZPropertyInfo(Schema.SecurityFlag);

		public ZBool SecurityFlagVisible => Header.Declaration.IsExport && IsUcc6 && (MessageType == DeclarationMessageTypeList.Codes.ExportUcc6 || MessageType == DeclarationMessageTypeList.Codes.ExportAmendmentUcc6 || MessageType == DeclarationMessageTypeList.Codes.ExportPreDeclaration);

		[List(nameof(RequestDispatchList))]
		public ZString RequestDispatch
		{
			get => requestDispatch;
			set => SetNonPersistentPropertyValue(RequestDispatchInfo, ref requestDispatch, value);
		}
		ZString requestDispatch;

		public ZPropertyInfo RequestDispatchInfo => GetZPropertyInfo(Schema.RequestDispatch);

		public ZBool RequestDispatchVisible => (Header.HasSendableAnnexesForAES && MessageType == DeclarationMessageTypeList.Codes.ExportAnnexes)
												|| (Header.HasSendableAnnexesForT2LPOUS && MessageType == DeclarationMessageTypeList.Codes.T2lDocumentationPous);

		#endregion

		#region New Properties

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|CH_BGMReference", Caption = "Ref No.")]
		public ZString CH_BGMReference => Header.CH_BGMReference;

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|MRN", ShortCaption = "MRN", Caption = "MRN")]
		[MaxLength(Schema.MRNMaxLength)]
		public ZString MRN => Header.MovementReferenceNumber;

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(Schema.MRN);

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|MessageSubType", ShortCaption = "Msg. Sub Type", Caption = "Message Sub Type")]
		[List(nameof(MessageSubTypesList))]
		[ReadOnlyMember(nameof(MessageSubType_ReadOnly))]
		public ZString MessageSubType
		{
			get => messageSubType;
			set
			{
				SetNonPersistentPropertyValue(MessageSubTypeInfo, ref messageSubType, value);
				MessageType = GetDefaultMessageType();
			}
		}
		ZString messageSubType;

		public ZPropertyInfo MessageSubTypeInfo => GetZPropertyInfo(Schema.MessageSubType);

		ZBool MessageSubType_ReadOnly
		{
			get
			{
				var result = IsEditableProperty;
				if (!result)
				{
					var entrySubStyle = Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
					result = !((IsImport && !IsUcc6 && (IsEntryStatusPDI || IsEntryStatusPDA) && entryInstructionsABCYZ.Contains(entrySubStyle))
							|| (IsImport && (IsEntryStatusCLP || (IsEntryStatusCLR && !IsPOUS)) && IsEntryInstructionT2L)
							|| (IsExport && IsEntryStatusCLP && !IsUcc6 && IsEntryInstructionBC)
							|| (IsExport && IsEntryStatusCLR && IsEntryInstructionEXS)
							|| (IsExport && IsUcc6 && IsEntryStatusPDAorCLPorCLR)
							);
				}
				return result;
			}
		}

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|MessageStatus", ShortCaption = "Msg. Status", Caption = "Message Status")]
		public ZString MessageStatus => Header.CH_Status;

		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(Schema.MessageStatus);

		public ZBool IsImport => (Header.Declaration?.JE_MessageType ?? ZString.Empty) == JobMessageTypeList.Codes.Import;
		ZBool IsImportNoUcc6NoH2AndOriginalAndNotPDI => !IsUcc6 && IsImport && MessageSubType == DeclarationMessageSubTypeList.Codes.OriginalDeclaration && !IsDeclarationH2 && !IsEntryStatusPDI;
		ZBool IsImportNoH2AndAmendmentAndPDA => IsImport && !IsDeclarationH2 && MessageSubType == DeclarationMessageSubTypeList.Codes.Amendment && IsEntryStatusPDA && IsEntryInstructionABYZ;
		ZBool IsComplementaryT2LNoPOUS => !IsPOUS && MessageSubType == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration && IsEntryInstructionT2L;
		ZBool IsT2lPousAndEntryStatusCLR => IsEntryStatusCLR && IsPOUS;
		ZBool IsImportAndOriginalAndT2LOrT2C => IsImport && MessageSubType == DeclarationMessageSubTypeList.Codes.OriginalDeclaration && IsEntryInstructionT2LOrT2C;
		public ZBool IsExport => (Header.Declaration?.JE_MessageType ?? ZString.Empty) == JobMessageTypeList.Codes.Export;
		ZBool IsExportUCC6EntryStatusNotCDAorPDAorCLPorCLRorEntryInstructionABC => IsExport && IsUcc6 && !IsEntryStatusCDA && !IsEntryStatusPDAorCLPorCLR && IsEntryInstructionABC;
		ZBool IsEntryStatusEmpty => Header.CH_EntryStatus.IsEmpty;
		ZBool IsEntryStatusERR => Header.CH_EntryStatus == EntryStatusCodes.Error;
		ZBool IsEntryStatusFFT => Header.CH_EntryStatus == Common.EU.MessageStatusList.Codes.FailedFromTransmission;
		ZBool IsEntryStatusCLP => Header.CH_EntryStatus == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		ZBool IsEntryStatusCDA => Header.CH_EntryStatus == EntryStatusCodes.CustomsDeclarationAccepted;
		ZBool IsEntryStatusCLR => Header.CH_EntryStatus == EntryStatusCodes.Cleared;
		ZBool IsEntryStatusPDA => Header.CH_EntryStatus == EntryStatusCodes.PreDeclarationAccepted;
		ZBool IsEntryStatusCDP => Header.CH_EntryStatus == EntryStatusCodes.ClearedWithPendingDocuments;
		ZBool IsEntryStatusPDI => Header.CH_EntryStatus == EntryStatusCodes.IncompletePreDeclaration;
		ZBool IsEntryStatusPDAorCLPorCLR => IsEntryStatusPDA || IsEntryStatusCLP || IsEntryStatusCLR;
		ZBool IsEntryInstructionC => (Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == EntrySubStyleList.Codes.C;
		ZBool IsEntryInstructionB => (Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == EntrySubStyleList.Codes.B;
		ZBool IsEntryInstructionT2L => (Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == EntrySubStyleList.Codes.T2L;
		ZBool IsEntryInstructionT2C => (Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == EntrySubStyleList.Codes.T2C;
		ZBool IsEntryInstructionEXS => (Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == ExsEntrySubStyleList.Codes.EXS;
		public ZBool IsEntryInstructionT2LOrT2C => entryInstructionsT2LT2C.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);
		ZBool IsEntryInstructionABC => entryInstructionsABC.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);
		ZBool IsEntryInstructionBC => entryInstructionsBC.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);

		public ZBool IsDeclarationH2 => (Header.EntryInstruction?.CEI_Style ?? ZString.Empty) == IMPDeclarationTypeList.Codes.H2;

		public ZBool IsEntryInstructionABXYZ => entryInstructionsABXYZ.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);

		public ZBool IsEntryInstructionABCXYZ => entryInstructionsABCXYZ.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);

		public ZBool IsEntryInstructionABZ => entryInstructionsABZ.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);

		public ZBool IsEntryInstructionABXZ => entryInstructionsABXZ.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);

		public ZBool IsEntryInstructionABYZ => entryInstructionsABYZ.Contains(Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);

		readonly ImmutableHashSet<ZString> entryInstructionsABXZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.X, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsABCYZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C, EntrySubStyleList.Codes.Y, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsABC = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C);
		readonly ImmutableHashSet<ZString> entryInstructionsABYZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.Y, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsABXYZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.X, EntrySubStyleList.Codes.Y, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsABCXYZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C, EntrySubStyleList.Codes.X, EntrySubStyleList.Codes.Y, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsABZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsBC = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C);
		readonly ImmutableHashSet<ZString> entryInstructionsBCZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsABCZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsBZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsYZ = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.Y, EntrySubStyleList.Codes.Z);
		readonly ImmutableHashSet<ZString> entryInstructionsT2LT2C = ImmutableHashSet.Create<ZString>(EntrySubStyleList.Codes.T2L, EntrySubStyleList.Codes.T2C);

		EDIMessage PreviousRejectedMessage
		{
			get
			{
				var lastResponse = Header.Messages.LastIncomingMessage;
				if (lastResponse != null && lastResponse.EM_MessageSubType == DeclarationMessageSubTypeList.Codes.RejectedResponse)
				{
					return Header.Messages.LastOutgoingMessage;
				}
				return null;
			}
		}

		void SetMessageSubType()
		{
			var entrySubStyle = Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

			var previousRejectedMsg = PreviousRejectedMessage;
			if (previousRejectedMsg != null)
			{
				MessageSubType = previousRejectedMsg.EM_MessageSubType;
			}
			else if (IsEntryStatusEmpty || IsEntryStatusERR || IsEntryStatusFFT)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			}
			else if (IsImport)
			{
				if (IsEntryStatusCDP
					|| (IsEntryStatusCLP && entryInstructionsBCZ.Contains(entrySubStyle))
					|| (IsEntryStatusCDA && (entryInstructionsABCYZ.Contains(entrySubStyle) || (IsEntryInstructionT2LOrT2C && IsPOUS))))
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				}
				else if (IsUcc6 && IsEntryStatusPDI && entryInstructionsABC.Contains(entrySubStyle))
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment;
				}
				else
				{
					MessageSubType = ZString.Empty;
				}
			}
			else if (IsExport)
			{
				if (IsUcc6 && IsEntryStatusPDAorCLPorCLR)
				{
					MessageSubType = ZString.Empty;
				}
				else if (((IsUcc6 || IsPOUS) && IsEntryStatusCDA)
					|| (IsEntryInstructionT2L && IsEntryStatusCLP))
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				}
				else if ((IsEntryInstructionT2L && IsEntryStatusCLR && !IsT2lPousAndEntryStatusCLR)
					|| (!IsUcc6 && IsEntryStatusPDAorCLPorCLR && entryInstructionsABCZ.Contains(entrySubStyle)))
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment;
				}
				else
				{
					MessageSubType = ZString.Empty;
				}
			}
			else
			{
				MessageSubType = ZString.Empty;
			}
		}
		#endregion

		#region Override Properties

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|MessageType", ShortCaption = "Msg. Type", Caption = "Message Type")]
		[List(nameof(MessageTypesList))]
		[ReadOnlyMember(nameof(MessageType_ReadOnly))]
		public override ZString MessageType
		{
			get => base.MessageType;
			set
			{
				var oldValue = MessageType;
				base.MessageType = value;

				if (oldValue != MessageType)
				{
					if (MessageType == DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration)
					{
						Header.ValidationMode = ValidationModes.PDI;
					}
					else if (MessageType == DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration)
					{
						Header.ValidationMode = ValidationModes.PDS;
					}
					else
					{
						Header.ValidationMode = ValidationModes.None;
					}

					ShouldSendInfo.RefreshBinding();
				}
			}
		}

		protected override bool MessageType_ReadOnly => IsEditableProperty ||
														!(IsImportNoUcc6NoH2AndOriginalAndNotPDI
															|| IsImportNoH2AndAmendmentAndPDA
															|| IsComplementaryT2LNoPOUS
															|| IsExportUCC6EntryStatusNotCDAorPDAorCLPorCLRorEntryInstructionABC)
														|| IsImportAndOriginalAndT2LOrT2C;

		#region ImportMessageType

		ZString SetImportMessageType()
		{
			var entrySubStyle = Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			var messageSubType = MessageSubType;

			if (messageSubType == DeclarationMessageSubTypeList.Codes.OriginalDeclaration)
			{
				return GetImportMessageTypeForOriginalDeclaration();
			}

			if (messageSubType == DeclarationMessageSubTypeList.Codes.Amendment)
			{
				return GetImportMessageTypeForAmendment();
			}

			if (messageSubType == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration)
			{
				return GetImportMessageTypeForComplementaryDeclaration(entrySubStyle);
			}

			if (messageSubType == DeclarationMessageSubTypeList.Codes.Cancellation)
			{
				return GetImportMessageTypeForCancellation();
			}

			return ZString.Empty;
		}

		ZString GetImportMessageTypeForOriginalDeclaration()
		{
			if (IsEntryInstructionT2L)
			{
				return IsPOUS2 ? DeclarationMessageTypeList.Codes.T2lReceptionPous : DeclarationMessageTypeList.Codes.T2lReception;
			}

			if (IsEntryInstructionT2C)
			{
				return IsPOUS ? DeclarationMessageTypeList.Codes.T2lPresentationPous : DeclarationMessageTypeList.Codes.T2lClearance;
			}

			if (IsDeclarationH2 && IsEntryInstructionABZ)
			{
				return DeclarationMessageTypeList.Codes.DvdH2;
			}

			if (IsEntryStatusPDI)
			{
				if (IsEntryInstructionABYZ)
				{
					return DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration;
				}

				if (IsEntryInstructionC)
				{
					return DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
				}
			}

			if (IsUcc6 && IsEntryInstructionABC)
			{
				return DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1;
			}

			return ZString.Empty;
		}

		ZString GetImportMessageTypeForAmendment()
		{
			if (IsEntryInstructionT2L)
			{
				return DeclarationMessageTypeList.Codes.T2lReceptionAmendment;
			}

			if (IsEntryStatusPDI)
			{
				if (IsUcc6 && IsEntryInstructionABC)
				{
					return DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1;
				}

				return DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration;
			}

			if (IsEntryStatusPDA)
			{
				if (IsDeclarationH2)
				{
					return DeclarationMessageTypeList.Codes.DvdH2;
				}

				if (IsEntryInstructionC)
				{
					return DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
				}
			}

			return ZString.Empty;
		}

		ZString GetImportMessageTypeForComplementaryDeclaration(ZString entrySubStyle)
		{
			if (IsEntryStatusCLP)
			{
				if (IsEntryInstructionC)
				{
					return DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration;
				}

				if (IsEntryInstructionB && IsDeclarationH2)
				{
					return DeclarationMessageTypeList.Codes.TypeXDvdH2;
				}

				if (entryInstructionsBZ.Contains(entrySubStyle))
				{
					return DeclarationMessageTypeList.Codes.PendingSupportingDocuments;
				}

				if (IsEntryInstructionT2L)
				{
					return DeclarationMessageTypeList.Codes.T2lAnnex;
				}
			}

			if (IsEntryStatusCDP)
			{
				return DeclarationMessageTypeList.Codes.Box44Documents;
			}

			if (IsEntryStatusCDA && IsPOUS && IsEntryInstructionT2LOrT2C)
			{
				return DeclarationMessageTypeList.Codes.T2lDocumentationPous;
			}

			return ZString.Empty;
		}

		ZString GetImportMessageTypeForCancellation()
		{
			if (IsDeclarationH2)
			{
				return DeclarationMessageTypeList.Codes.DvdH2Cancellation;
			}

			return DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation;
		}

		#endregion

		ZString SetExportMessageType()
		{
			var messageTypeRet = ZString.Empty;

			var entrySubStyle = Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			var messageSubTypeLocal = MessageSubType;

			if (IsEntryInstructionT2L && messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.OriginalDeclaration)
			{
				messageTypeRet = IsPOUS ? DeclarationMessageTypeList.Codes.T2lRequestPous : DeclarationMessageTypeList.Codes.T2lExpedition;
			}
			else if (IsEntryInstructionT2L && messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.Amendment && !IsT2lPousAndEntryStatusCLR)
			{
				messageTypeRet = DeclarationMessageTypeList.Codes.T2lExpeditionAmendment;
			}
			else if (messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.OriginalDeclaration && IsEntryInstructionT2C)
			{
				messageTypeRet = DeclarationMessageTypeList.Codes.T2lClearance;
			}
			else if ((messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.OriginalDeclaration || messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.Amendment || messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.Cancellation)
					&& IsEntryInstructionEXS)
			{
				messageTypeRet = DeclarationMessageTypeList.Codes.ExitSummaryDeclaration;
			}
			else if (messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.OriginalDeclaration)
			{
				if (IsUcc6)
				{
					if (IsEntryStatusPDA && entryInstructionsABC.Contains(entrySubStyle))
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.ExportNotification;
					}
					else if (entryInstructionsYZ.Contains(entrySubStyle))
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.ExportUcc6;
					}
					else if (entryInstructionsABC.Contains(entrySubStyle))
					{
						messageTypeRet = ZString.Empty;
					}
				}
				else
				{
					messageTypeRet = DeclarationMessageTypeList.Codes.Export;
				}
			}
			else if (IsEntryStatusCLP && messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration)
			{
				if (IsEntryInstructionB)
				{
					if (IsUcc6)
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.TypeXExportUcc6;
					}
					else
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.TypeXExport;
					}
				}
				else if (IsEntryInstructionC)
				{
					if (IsUcc6)
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.ExportUcc6;
					}
					else
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.Export;
					}
				}
			}
			else if (IsEntryStatusPDAorCLPorCLR && !IsEntryInstructionT2L)
			{
				if (messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.Amendment)
				{
					if (IsUcc6)
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.ExportAmendmentUcc6;
					}
					else if (entryInstructionsABCZ.Contains(entrySubStyle))
					{
						messageTypeRet = DeclarationMessageTypeList.Codes.ExportAmendment;
					}
				}
				else if (IsUcc6 && messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.Cancellation)
				{
					messageTypeRet = DeclarationMessageTypeList.Codes.ExportCancellation;
				}
			}
			else if (IsEntryStatusCDA && messageSubTypeLocal == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration)
			{
				if (IsPOUS && IsEntryInstructionT2LOrT2C)
				{
					messageTypeRet = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
				}
				else if (IsUcc6)
				{
					messageTypeRet = DeclarationMessageTypeList.Codes.ExportAnnexes;
				}
			}
			return messageTypeRet;
		}

		protected override ZString GetDefaultMessageType()
		{
			var messageTypeRet = ZString.Empty;
			if (IsImport)
			{
				messageTypeRet = SetImportMessageType();
			}
			else if (IsExport)
			{
				messageTypeRet = SetExportMessageType();
			}

			if (messageTypeRet.IsEmpty)
			{
				var previousRejectedMsg = PreviousRejectedMessage;
				if (previousRejectedMsg != null)
				{
					messageTypeRet = previousRejectedMsg.EM_MessageType;
				}
			}
			return messageTypeRet;
		}

		protected override void SetMessageSendingObjectDefaultValues()
		{
			SetMessageSubType();
			SetDefaultSecurityFlag();
			SetDefaultActivateByOperator();
			base.SetMessageSendingObjectDefaultValues();
		}

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|EntryType", ShortCaption = "En. Type", Caption = "Entry type")]
		public override ZString DeclarationType => Header.EntryTypeFriendlyName;

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.MessageSendingObject|EntryStatus", ShortCaption = "En. Status", Caption = "Entry Status")]
		public override ZString EntryStatus => base.EntryStatus;

		protected override bool ShouldSend_ReadOnly => Header.IsWaitingForResponse || Header.HasAnnexesSentWithoutResponse() || NoEntryInstruction;

		bool NoEntryInstruction => (Header?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty).IsEmpty;

		#endregion

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
			=> new JobDeclarationMessageSendingObjectValidation(this);

		public new JobDeclarationMessageSendingObjectValidation Validation => (JobDeclarationMessageSendingObjectValidation)base.Validation;

		#region Lookup Lists

		public CodeDescriptionPairList MessageSubTypesList
		{
			get
			{
				var entrySubStyle = Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

				return Factory.GetCachedValue("JobDeclarationMessageSendingObject.MessageSubTypeList_" + IsImport + "_" + Header.CH_EntryStatus + "_" + entrySubStyle + "_" + Header.ZG_POUSVersion + "_" + Header.ZG_UCC6Version, () =>
				{
					var list = new CodeDescriptionPairList();
					var isImportNoUcc6_ABCYZInstruction = IsImport && !IsUcc6 && (IsEntryStatusPDI || IsEntryStatusPDA) && entryInstructionsABCYZ.Contains(entrySubStyle);
					var isImportNoUcc6_ABCYZInstruction_PDIorPDA = isImportNoUcc6_ABCYZInstruction && (IsEntryStatusPDI || IsEntryStatusPDA);
					var isImportNoUcc6_ABCYZInstruction_PDI = isImportNoUcc6_ABCYZInstruction && IsEntryStatusPDI;
					var isT2LImport_CLPorCLR = IsImport && (IsEntryStatusCLP || IsEntryStatusCLR) && IsEntryInstructionT2L;

					var isExport_CLR_EXS = IsExport && IsEntryStatusCLR && IsEntryInstructionEXS;
					var isExportNoUcc6_CLR_CLP_BCInstruction = IsExport && !IsUcc6 && IsEntryStatusCLP && IsEntryInstructionBC;

					var isExportUcc6_NoT2LPous = IsExport && IsUcc6 && !IsT2lPousAndEntryStatusCLR;
					var isExportUcc6_NoT2LPous_PDAorCLPorCLR = isExportUcc6_NoT2LPous && IsEntryStatusPDAorCLPorCLR;
					var isExportUcc6_NoT2LPous_CLP_BCInstruction = isExportUcc6_NoT2LPous && IsEntryStatusCLP && IsEntryInstructionBC;
					var isExportUcc6_NoT2LPous_PDA_ABCInstruction = isExportUcc6_NoT2LPous && IsEntryStatusPDA && IsEntryInstructionABC;

					AddPairToListIf(list, isImportNoUcc6_ABCYZInstruction_PDIorPDA || isExport_CLR_EXS || isExportUcc6_NoT2LPous_PDAorCLPorCLR, AddAmendmentAndCancellation);
					AddPairToListIf(list, isImportNoUcc6_ABCYZInstruction_PDI || isExportUcc6_NoT2LPous_PDA_ABCInstruction, AddOriginalDeclaration);
					AddPairToListIf(list, !IsT2lPousAndEntryStatusCLR && isT2LImport_CLPorCLR || isExportNoUcc6_CLR_CLP_BCInstruction, AddAmendmentAndComplementary);
					AddPairToListIf(list, isExportUcc6_NoT2LPous_CLP_BCInstruction, AddComplementaryDeclaration);

					return list;
				});

				void AddPairToListIf(CodeDescriptionPairList list, bool condition, Action<CodeDescriptionPairList> addList)
				{
					if (condition)
					{
						addList(list);
					}
				}

				void AddAmendmentAndCancellation(CodeDescriptionPairList list)
				{
					AddAmendmentDeclaration(list);
					list.AddPair(DeclarationMessageSubTypeList.Codes.Cancellation, DeclarationMessageSubTypeList.Descriptions.Cancellation);
				}

				void AddAmendmentAndComplementary(CodeDescriptionPairList list)
				{
					AddAmendmentDeclaration(list);
					AddComplementaryDeclaration(list);
				}

				void AddOriginalDeclaration(CodeDescriptionPairList list)
				{
					list.AddPair(DeclarationMessageSubTypeList.Codes.OriginalDeclaration, DeclarationMessageSubTypeList.Descriptions.OriginalDeclaration);
				}

				void AddComplementaryDeclaration(CodeDescriptionPairList list)
				{
					list.AddPair(DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, DeclarationMessageSubTypeList.Descriptions.ComplementaryDeclaration);
				}

				void AddAmendmentDeclaration(CodeDescriptionPairList list)
				{
					list.AddPair(DeclarationMessageSubTypeList.Codes.Amendment, DeclarationMessageSubTypeList.Descriptions.Amendment);
				}
			}
		}

		public CodeDescriptionPairList MessageTypesList
		{
			get
			{
				var entrySubStyle = Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
				var messageSubType = MessageSubType;

				return Factory.GetCachedValue("JobDeclarationMessageSendingObject.MessageTypesList_" + entrySubStyle + "_" + messageSubType + "_" + Header.CH_EntryStatus + "_IsH2" + IsDeclarationH2, () =>
				{
					var list = new CodeDescriptionPairList();
					if (!MessageType_ReadOnly && IsImport && !IsUcc6 && !IsDeclarationH2 && entryInstructionsABCYZ.Contains(entrySubStyle))
					{
						if (messageSubType == DeclarationMessageSubTypeList.Codes.Amendment && IsEntryStatusPDA)
						{
							list.AddPair(DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageTypeList.Descriptions.ImportCompletePreDeclaration);
							list.AddPair(DeclarationMessageTypeList.Codes.ImportAmendmentBox40, DeclarationMessageTypeList.Descriptions.ImportAmendmentBox40);
						}
						else if (messageSubType != DeclarationMessageSubTypeList.Codes.Cancellation)
						{
							list.AddPair(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, DeclarationMessageTypeList.Descriptions.ImportIncompletePreDeclaration);

							if (IsEntryInstructionABYZ)
							{
								list.AddPair(DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageTypeList.Descriptions.ImportCompletePreDeclaration);
							}
							else if (IsEntryInstructionC)
							{
								list.AddPair(DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageTypeList.Descriptions.ImportSimplifiedPreDeclaration);
							}
						}
					}
					else if (!MessageType_ReadOnly && IsEntryInstructionT2L && messageSubType == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration)
					{
						list.AddPair(DeclarationMessageTypeList.Codes.T2lAnnex, DeclarationMessageTypeList.Descriptions.T2lAnnex);
						list.AddPair(DeclarationMessageTypeList.Codes.T2lClearance, DeclarationMessageTypeList.Descriptions.T2lClearance);
					}
					else if (!MessageType_ReadOnly && IsExportUCC6EntryStatusNotCDAorPDAorCLPorCLRorEntryInstructionABC && messageSubType == DeclarationMessageSubTypeList.Codes.OriginalDeclaration)
					{
						list.AddPair(DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageTypeList.Descriptions.ExportUcc6);
						list.AddPair(DeclarationMessageTypeList.Codes.ExportPreDeclaration, DeclarationMessageTypeList.Descriptions.ExportPreDeclaration);
					}
					return list;
				});
			}
		}

		public CodeDescriptionPairList ActivateByOperatorFlagList => Factory.GetCachedValue<ActivateByOperatorCodeList>();

		public CodeDescriptionPairList SecurityFlagList => Factory.GetCachedValue<SecurityFlagCodeList>();

		public CodeDescriptionPairList RequestDispatchList => Factory.GetCachedValue<YesNoList>();

		#endregion

		#region Automation

		void DefaultValuesForAutomation(bool isAutoSend)
		{
			if (isAutoSend)
			{
				using (GetValidationSuspender())
				using (SuspendSettingHasChanges())
				{
					SetShouldSendForAutomation();
					SetMessageSubTypeForAutomation();
					SetMessageTypeForAutomation();
				}
			}
		}

		void SetShouldSendForAutomation()
		{
			if (EntryStatus.IsEmpty)
			{
				ShouldSend = true;
			}
		}

		void SetMessageSubTypeForAutomation()
		{
			if (ShouldSend)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			}
		}

		void SetMessageTypeForAutomation()
		{
			if (ShouldSend)
			{
				var messsageType = ZString.Empty;

				if (IsExport)
				{
					if (IsEntryInstructionABCXYZ && IsUcc6)
					{
						messsageType = DeclarationMessageTypeList.Codes.ExportUcc6;
					}
					else if (IsEntryInstructionT2L && IsPOUS)
					{
						messsageType = DeclarationMessageTypeList.Codes.T2lRequestPous;
					}
					else if (IsEntryInstructionEXS)
					{
						messsageType = DeclarationMessageTypeList.Codes.ExitSummaryDeclaration;
					}
				}
				else if (IsImport)
				{
					if (IsDeclarationH2 && IsEntryInstructionABXZ)
					{
						messsageType = DeclarationMessageTypeList.Codes.DvdH2;
					}
					else if (IsEntryInstructionABYZ)
					{
						messsageType = DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration;
					}
					else if (IsEntryInstructionC)
					{
						messsageType = DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
					}
					else if (IsPOUS)
					{
						if (IsEntryInstructionT2C)
						{
							messsageType = DeclarationMessageTypeList.Codes.T2lPresentationPous;
						}
						else if (IsEntryInstructionT2L)
						{
							messsageType = IsPOUS2 ? DeclarationMessageTypeList.Codes.T2lReceptionPous : DeclarationMessageTypeList.Codes.T2lReception;
						}
					}
				}

				MessageType = messsageType;
			}
		}

		#endregion
	}
}
