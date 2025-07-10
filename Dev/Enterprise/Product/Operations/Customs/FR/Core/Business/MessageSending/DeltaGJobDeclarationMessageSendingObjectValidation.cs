using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaGJobDeclarationMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
	{
		public DeltaGJobDeclarationMessageSendingObjectValidation(DeltaGJobDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		public new DeltaGJobDeclarationMessageSendingObject Parent => (DeltaGJobDeclarationMessageSendingObject)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDateMessage();
			ValidateReplacementDeclarationType();
		}

		#region New Validation

		public void CheckDateMessage()
		{
			if (Parent.MessageType == EntryActionCodeList.Codes.ANT && Parent.DateMessage.IsEmpty)
			{
				Parent.VOCReasonInfo.AddMessageError(GetMessageError(Parent.Header, DateMessageWarning));
			}
		}

		protected void CheckReplacementDeclarationType()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.IsAmend || Parent.IsCancel)
				{
					MandatoryValidation.CheckEntered(Parent.ReplacementDeclarationTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.ReplacementDeclarationTypeInfo);
			}
		}

		protected void CheckTriggeringPointForValidation(ZString messageType, ZPropertyInfo info)
		{
			if (messageType == EntryActionCodeList.Codes.ANT)
			{
				if (Parent.TriggeringPointForValidation == ZString.Empty && FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.Value.EnableAutomatedValidation)
				{
					info.AddError(VaaTriggeringPointMissing);
				}
				else if (Parent.TriggeringPointForValidation != TriggerPointsCodeList.Codes.NUL && Parent.TriggeringPointForValidation != ZString.Empty && !FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.Value.EnableAutomatedValidation)
				{
					info.AddWarning(AutomatedValidationTurnedOff);
				}
				else if (Parent.TriggeringPointForValidation == TriggerPointsCodeList.Codes.PAB
				&& Parent.Header.IsImport
				&& !Parent.Header.MergedLines.SelectMany(x => x.PreviousDocuments).Any(x => x.CSI_Code == PreviousDocumentCodeList.Codes._355 || x.CSI_Code == PreviousDocumentCodeList.Codes._337))
				{
					info.AddMessageError(Doc355Or337Missing);
				}
			}
		}

		public void ValidateReplacementDeclarationType()
		{
			ValidateCalculatedProperty(Parent.ReplacementDeclarationTypeInfo);
		}

		public void ValidateDateMessage()
		{
			ValidateCalculatedProperty(Parent.DateMessageInfo);
		}

		#endregion

		#region Override Validation

		#region MessageType

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			if (Parent.ShouldSend)
			{
				var info = Parent.MessageTypeInfo;
				var messageType = Parent.MessageType;
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);

				if (Parent.Header.IsCancelledWithCustoms)
				{
					info.AddMessageError(NoMessageShouldbeSentWhenCancelled);
				}
				CheckAvailableBalanceOnA12Account(messageType, info);
				CheckRecalculateEntrySubStyle(messageType, info);
				CheckTriggeringPointForValidation(messageType, info);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		void CheckRecalculateEntrySubStyle(ZString messageType, ZPropertyInfo info)
		{
			var parent = Parent;
			var instruction = parent.Header.EntryInstruction;
			if (instruction != null)
			{
				var substyle = instruction.CEI_SubStyle;
				var declaration = (JobDeclaration)instruction.JobDeclaration;

				if (!parent.DoNotRecalculateEntrySubstyle)
				{
					if ((instruction.IsPrelodgedSubstyle && messageType == EntryActionCodeList.Codes.VAL) ||
						(instruction.IsLodgedSubstyle && messageType == EntryActionCodeList.Codes.ANT))
					{
						var message = Res.GetString("8A6F40DB-DF22-4663-A4DE-D180A25583F3", @"The entry Sub Style will be recalculated to [{0}] " +
					"and this value will be both saved on the job and sent in the message. To disable this and send [{1}] verbatim, tick 'Keep Entered Sub Style'", CusEntryInstruction.GetComplementarySubstyle(substyle), substyle);
						info.AddWarning(message);
					}
				}

				if (EntryActionHelper.IsGreaterThanOrEqualToBAE(parent.EntryStatus) && !instruction.IsLodgedSubstyle)
				{
					var lodgedSubStyle = declaration.IsDeltaC ? EntrySubstyleCodePairList.Codes.A : EntrySubstyleCodePairList.Codes.C;
					var message = Res.GetString("10C0787B-8F8E-4007-8CDD-D15185897738", "The sub type [{0}] is not valid for message {1}. Use [{2}] instead please.", substyle, messageType, lodgedSubStyle);
					info.AddError(message);
				}
			}
		}

		void CheckAvailableBalanceOnA12Account(ZString messageType, ZPropertyInfo messageTypeInfo)
		{
			if (Parent.ShouldSend && messageType == EntryActionCodeList.Codes.VAL)
			{
				var declaration = Parent.Header?.Declaration;
				var ai2Permit = declaration?.Ai2Permit;

				if (ai2Permit != null)
				{
					var ai2PermitRemainingBalanceAmount = declaration.RemainingAi2PermitBalanceAmountInLocalCurrency;

					var ai2Amount = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Sum(x => x.Ai2Amount);
					if (ai2PermitRemainingBalanceAmount < ai2Amount)
					{
						messageTypeInfo.AddMessageError(TheAvailableBalanceLessThanAi2Amount(ai2PermitRemainingBalanceAmount, ai2Amount));
					}
				}
			}
		}

		#endregion

		protected override void CheckVOCReason()
		{
			base.CheckVOCReason();

			if (Parent.ShouldSend && Parent.VOCReason.IsEmpty && (Parent.IsAmend || Parent.IsCancel))
			{
				Parent.VOCReasonInfo.AddError(GetVOCReason());
			}
		}

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();

			var parent = Parent;
			if (parent.ShouldSend && FRCustomsDataRegistry.DeltaGFallbackIsActive && !parent.Header.DeltaGFallbackStatus.IsEmpty)
			{
				parent.ShouldSendInfo.AddError(DeltaGFallbackActiveAndMessageHasBeenCreated);
			}
		}

		protected override void CheckChangeAcknowledgementIndicator()
		{
			base.CheckChangeAcknowledgementIndicator();

			var parent = Parent;
			if (parent.ShouldSend && (parent.IsAmend || parent.IsCancel))
			{
				MandatoryValidation.CheckEntered(Parent.ChangeAcknowledgementIndicatorInfo);
			}
		}

		#endregion

		#region Message

		public static string DateMessageWarning => Res.GetString("064C0ED7-CA14-4F70-B79D-E09250812F40", "Date must be entered when message is anticipated");
		public static string NoMessageShouldbeSentWhenCancelled => Res.GetString("1BA1208C-B2D7-414E-8D49-A8EC98983EFB", "This entry is canceled; No further message should be sent.");
		public static string TheAvailableBalanceLessThanAi2Amount(ZDecimal availiabelBalance, ZDecimal ai2Amount) => Res.GetString("2408D431-C60B-4B08-AAB3-FE5855F8D21A", "The available balance on the AI2 account is {0} but {1} is required.", Utilities.FormatNumberNationalWithGroupSeparators((decimal)availiabelBalance, 2), Utilities.FormatNumberNationalWithGroupSeparators((decimal)ai2Amount, 2));
		public static string DeltaGFallbackActiveAndMessageHasBeenCreated => Res.GetString("F186418B-B281-4A4E-B69F-CB4E4612DD6F", "Delta G fallback is active and a message for this entry has already been created during fallback. Do not create a second one. After the timer elapses (default 30 minutes after message creation), this entry will automatically be given fallback clearance status and goods may be released; at the end of fallback the automatic regularization will deliver the message that is already queued, and only when Delta replies to that message should you consider sending a second.");
		public static string Doc355Or337Missing => Res.GetString("BA2FEB0D-9C8B-491A-A5BB-0507EF97F8D7", "If PAB is selected, a previous document with code 355 or 337 should be captured.");
		public static string AutomatedValidationTurnedOff => Res.GetString("B6034540-8EE0-4012-891D-E67CD632DA90", "The automated validation is turned off so the triggering point won’t take effect, consider turning it on in registry Customs > France > Auto Send Messages > Triggering points for automated validation.");
		public static string VaaTriggeringPointMissing => Res.GetString("5BA28BED-9260-4BFC-8E7E-AE5340B01CF4", "Registry settings requires the entry to have a triggering point for validation when sending an ANT message.");

		#endregion
	}
}
