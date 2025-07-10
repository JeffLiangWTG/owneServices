using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AESMessageSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public AESMessageSendingActionValidation(AESMessageSendingAction parent) : base(parent) { }

		protected override void CheckAnnotation()
		{
			if (Parent.ShouldSend && (Parent.MessageType == AESOutgoingMessageTypeList.Codes.ExportCancellation || Parent.MessageType == AESOutgoingMessageTypeList.Codes.ExitCancellation))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AnnotationInfo);
			}
		}

		protected override void CheckEntryStatus()
		{
			var targetInfo = Parent.EntryStatusInfo;
			if (Parent.ShouldSend && Parent.EntryHeader.CH_EntryStatus.EqualsIgnoringCase(AESEntryStatusList.Codes.Cancelled))
			{
				targetInfo.AddError(Res.GetString("2969E402-180A-4913-B23F-81542FCA2218", "Entry has been canceled. No further processing allowed."));
			}
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			var info = Parent.MessageTypeInfo;
			var entryHeader = Parent.EntryHeader;

			var jobType = entryHeader.Declaration.JE_MessageType.ToUpperInvariant();
			var logicalStatus = entryHeader.CH_Status.ToUpperInvariant();
			var sendingMessageType = Parent.MessageType.ToUpperInvariant();
			var isMrnEmpty = entryHeader.MovementReferenceNumber.IsEmpty;

			if (jobType == IEJobMessageTypeList.Codes.Export)
			{
				var entryStatus = entryHeader.CH_EntryStatus.ToUpperInvariant();
				if (logicalStatus.IsEmpty && sendingMessageType != AESOutgoingMessageTypeList.Codes.ExportOriginal)
				{
					info.AddMessageError(Res.GetString("E6C5897B-4CA0-409E-9D8C-B76D51EDB23C", "Message Type should be Export Declaration Original(515) when Message Status is empty"));
				}

				if (entryStatus == AESEntryStatusList.Codes.AmendmentRequested && sendingMessageType != AESOutgoingMessageTypeList.Codes.ExportAmendment)
				{
					info.AddMessageError(Res.GetString("29F06D4F-FDC9-4484-B1C0-C88D37348CE0", "Message Type should be Export Declaration Amendment(513) when Entry Status is Amendment Requested"));
				}

				if (entryStatus == AESEntryStatusList.Codes.Requested && sendingMessageType == AESOutgoingMessageTypeList.Codes.ExportAmendment)
				{
					info.AddMessageError(Res.GetString("0A744E0B-4176-4ABC-92F2-0089CFE20AEF", "Message type Export Declaration Amendment (IE513) cannot be used when the entry status is Requested Proof of Exit"));
				}

				if (entryStatus == AESEntryStatusList.Codes.Requested && sendingMessageType == AESOutgoingMessageTypeList.Codes.ExportOriginal)
				{
					info.AddMessageError(Res.GetString("2B69752D-0815-4566-B6B0-049E8B4C6864", "Message type Export Declaration (IE515) cannot be used when the entry status is Requested Proof of Exit"));
				}

				if (entryStatus == AESEntryStatusList.Codes.Prelodged)
				{
					if (sendingMessageType == AESOutgoingMessageTypeList.Codes.ExportOriginal)
					{
						info.AddMessageError(Res.GetString("C117383D-844C-4FF1-BC83-0010A4B58FDE", "Message Type should not be Export Declaration Original(515) when Entry Status is Pre-lodged"));
					}
				}
				else if (entryStatus != AESEntryStatusList.Codes.PendingControl && sendingMessageType == AESOutgoingMessageTypeList.Codes.ExportPresentation)
				{
					info.AddMessageError(Res.GetString("3BCA0E5A-27E7-49D9-BE1B-78609272DBF2", "Export Presentation Notification should only be sent when Customs Status is Pending Control or Pre-Lodged"));
				}

				if(isMrnEmpty && sendingMessageType == AESOutgoingMessageTypeList.Codes.ReleaseAmendment)
				{
					info.AddMessageError(Res.GetString("A4D1E5F2-0B3C-4E7C-8F9A-6D1A0B5E8F2A", "Message type Post Release Amendment (IEX13) cannot be used when the entry status is not Released for Export (IE529)"));
				}

				if (entryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
				{
					var style = entryInstruction.CEI_Style.ToUpperInvariant();
					var subStyle = entryInstruction.CEI_SubStyle.ToUpperInvariant();

					if (subStyle != EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA && subStyle != EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC && sendingMessageType == AESOutgoingMessageTypeList.Codes.ExportPresentation)
					{
						info.AddMessageError(Res.GetString("5B3B164C-265B-480A-9C0F-81B68CAA1B9D", "Export Presentation Notification should only be sent for Preliminary Declarations (Sub-Style D or F)"));
					}

					if (sendingMessageType == AESOutgoingMessageTypeList.Codes.ExportAmendment)
					{
						var hasExportPresentationMessage = entryHeader.HasExportPresentationMessage;
						string[] stylesWithPossibleAmendmentErrors = { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3, ExportDeclarationTypeList.Codes.B4 };

						var shouldShowAmendmentErrorMessage =
							((!hasExportPresentationMessage && stylesWithPossibleAmendmentErrors.Contains(style.ToString()) && (subStyle == EntrySubStyleList.Codes.NormalDeclaration || subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF))
							||
							(hasExportPresentationMessage && ((stylesWithPossibleAmendmentErrors.Contains(style.ToString()) && subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA) || (style == ExportDeclarationTypeList.Codes.B4 && subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC))));

						if (shouldShowAmendmentErrorMessage)
						{
							if (entryStatus == AESEntryStatusList.Codes.ReleasedForExport)
							{
								info.AddMessageError(Res.GetString("0E62F1ED-F7BD-4AB7-B337-5D3071EEE658", "Message type Export Declaration Amendment (IE513) cannot be used when the entry status is Released for Export (IE529)"));
							}
							else if (entryStatus == AESEntryStatusList.Codes.ControlledForExport)
							{
								info.AddMessageError(Res.GetString("954830C7-D566-4D36-A16B-68FC0806A773", "Message type Export Declaration Amendment (IE513) cannot be used when the entry status is Controlled for Export"));
							}
						}
					}
				}
			}
			else if (jobType == IEJobMessageTypeList.Codes.ReExport)
			{
				if (isMrnEmpty)
				{
					if (sendingMessageType != AESOutgoingMessageTypeList.Codes.ReExport)
					{
						info.AddMessageError(Res.GetString("7A4EF37E-2E0A-4399-955C-1BBEB39EAD01", "Message Type should be Re-Export Notification(570) when MRN is empty"));
					}
				}
				else
				{
					if (entryHeader.CH_EntryStatus.EqualsIgnoringCase(AESEntryStatusList.Codes.CancellationRequestedByCustoms) && sendingMessageType != AESOutgoingMessageTypeList.Codes.ExitCancellation)
					{
						info.AddMessageError(MessageTypeShouldBe614);
					}
				}
				if (sendingMessageType == AESOutgoingMessageTypeList.Codes.ReExportAmendment && entryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
				{
					if (Parent.ShouldSend && Parent.EntryHeader.MovementReferenceNumber.IsEmpty)
					{
						info.AddMessageError(Res.GetString("8BD702E1-3618-40ED-B523-8845A9B7BCE0", "It is invalid to send a Re-export Amendment (IE573) when no MRN is available."));
					}

					var shouldShowAmendmentErrorMessage = entryInstruction.CEI_Style.EqualsIgnoringCase(ReExportDeclarationTypeList.Codes.A3) && entryInstruction.CEI_SubStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.NormalDeclaration);
					if (shouldShowAmendmentErrorMessage)
					{
						switch (entryHeader.CH_EntryStatus.ToUpperInvariant())
						{
							case AESEntryStatusList.Codes.ReleasedForExit:
								info.AddMessageError(Res.GetString("4AF8ACE6-C3AF-4A49-878F-73457C918A9B", "Message type Re-Export Declaration Amendment (IE573) cannot be used when the entry status is Released for Exit (IE525)"));
								break;
							case AESEntryStatusList.Codes.ControlledForExport:
								info.AddMessageError(Res.GetString("A3B5E645-EA90-491F-BCB9-AF2233581A3B", "Message type Re-Export Declaration Amendment (IE573) cannot be used when the entry status is Controlled for Export"));
								break;
						}
					}
				}
			}
			else if (jobType == IEJobMessageTypeList.Codes.ExitSummary)
			{
				if (isMrnEmpty)
				{
					if (sendingMessageType != AESOutgoingMessageTypeList.Codes.ExitOriginal)
					{
						info.AddMessageError(Res.GetString("625400B2-9770-4457-B681-2806FA68FBA2", "Message Type should be Exit Summary Declaration Original(615) when MRN is empty"));
					}
				}
				else if (entryHeader.CH_EntryStatus.EqualsIgnoringCase(AESEntryStatusList.Codes.CancellationRequestedByCustoms) && sendingMessageType != AESOutgoingMessageTypeList.Codes.ExitCancellation)
				{
					info.AddMessageError(MessageTypeShouldBe614);
				}

				if (sendingMessageType == AESOutgoingMessageTypeList.Codes.ExitAmendment && entryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
				{
					var style = entryInstruction.CEI_Style.ToUpperInvariant();
					var shouldShowAmendmentErrorMessage = (style == ExitSummaryDeclarationTypeList.Codes.A1 || style == ExitSummaryDeclarationTypeList.Codes.A2) && entryInstruction.CEI_SubStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.NormalDeclaration);
					if (shouldShowAmendmentErrorMessage)
					{
						switch (entryHeader.CH_EntryStatus.ToUpperInvariant())
						{
							case AESEntryStatusList.Codes.ReleasedForExport:
								info.AddMessageError(Res.GetString("67B2FEC0-6373-47A3-BB54-AFEC34F1A468", "Message type Exit Summary Declaration Amendment (IE613) cannot be used when the entry status is Released for Export (IE529)"));
								break;
							case AESEntryStatusList.Codes.ControlledForExport:
								info.AddMessageError(Res.GetString("4417EF76-3FDD-4D71-BE9E-B5F2F54970F0", "Message type Exit Summary Declaration Amendment (IE613) cannot be used when the entry status is Controlled for Export"));
								break;
						}
					}
				}
			}
		}

		static string MessageTypeShouldBe614 => Res.GetString("CF2F1522-E89E-4725-93A2-0BACD275618B", "Message Type SHOULD BE \"Exit Cancellation\"(614) when Entry Status is \"Cancellation Requested by Customs\".");
	}
}
