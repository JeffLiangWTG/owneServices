using System.Linq;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Business
{
	public class UploadDocumentsSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public UploadDocumentsSendingActionValidation(UploadDocumentsSendingAction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMovementReferenceNumber();
			ValidateLocalReferenceNumber();
			CheckFallbackProcedure();
			CheckHasOpenDocumentsOrIsControl();
		}

		public void ValidateMovementReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.MovementReferenceNumberInfo);
		}

		protected virtual void CheckMovementReferenceNumber()
		{
			if (Parent.ShouldSend && !Parent.IsValidationSuspended)
			{
				if (Parent.MovementReferenceNumber.IsEmpty)
				{
					Parent.MovementReferenceNumberInfo.AddError(Res.GetString("80BB33BB-1CE0-41C8-A97C-17BC05B9D3BC", "MRN cannot be empty. MRN is generated when an acceptance message is received from the customs."));
				}
			}
		}

		public void ValidateLocalReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.LocalReferenceNumberInfo);
		}

		protected virtual void CheckLocalReferenceNumber()
		{
			if (Parent.ShouldSend && !Parent.IsValidationSuspended)
			{
				if (Parent.LocalReferenceNumber.IsEmpty)
				{
					Parent.LocalReferenceNumberInfo.AddError(Res.GetString("7E98CF3A-9ADC-40DE-8954-D1E8609B1496", "LRN cannot be empty. LRN is generated when a customs declaration (IM415) is sent to the customs."));
				}
			}
		}

		public void CheckFallbackProcedure()
		{
			var message = Res.GetString("2CA240CE-175D-4736-B97F-0DA1F403C71C", "{0} is required for Fallback Procedure");
			Parent.RemoveRowError(string.Format(message, Parent.AlternativeDateOfAcceptanceInfo.HumanReadableName));
			Parent.RemoveRowError(string.Format(message, Parent.CustomsReferenceNumberInfo.HumanReadableName));
			Parent.RemoveRowError(string.Format(message, Parent.CustomsJustificationInfo.HumanReadableName));
			if (Parent.ShouldSend && !(Parent.AlternativeDateOfAcceptance.IsEmpty &&
				Parent.CustomsReferenceNumber.IsEmpty &&
				Parent.CustomsJustification.IsEmpty))
			{
				if (Parent.AlternativeDateOfAcceptance.IsEmpty)
				{
					Parent.AddRowError(string.Format(message, Parent.AlternativeDateOfAcceptanceInfo.HumanReadableName));
				}
				if (Parent.CustomsReferenceNumber.IsEmpty)
				{
					Parent.AddRowError(string.Format(message, Parent.CustomsReferenceNumberInfo.HumanReadableName));
				}
				if (Parent.CustomsJustification.IsEmpty)
				{
					Parent.AddRowError(string.Format(message, Parent.CustomsJustificationInfo.HumanReadableName));
				}
			}
		}

		public virtual void CheckHasOpenDocumentsOrIsControl()
		{
			var message = Res.GetString("12D60AA4-53EB-4E9E-B511-FE3049F06162", "The customs authorities have not requested any documents.");
			if (Parent.ShouldSend && !HasOpenRequestedDocuments() && !Parent.EntryHeader.CH_EntryStatus.EqualsIgnoringCase(AISEntryStatusList.Codes.Control))
			{
				Parent.AddRowWarning(message);
			}
			else
			{
				Parent.RemoveRowWarning(message);
			}
		}

		bool HasOpenRequestedDocuments() => Parent.EntryHeader.EntryInstruction?.RequestedDocuments?.Cast<EU.Business.RequestedDocument>().Any(p => p.CSI_Status == EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened) ?? false;

		new UploadDocumentsSendingAction Parent => (UploadDocumentsSendingAction)base.Parent;
	}
}
