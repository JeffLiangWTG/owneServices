using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportCusEntryInstructionValidation : CommonExportCusEntryInstructionValidation
	{
		public ExportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckMultipleMergingKeyFields();
			CheckAdditionalInfo1D23Mandatory();
		}

		void CheckMultipleMergingKeyFields()
		{
			var instruction = Parent;
			if (instruction.JobDeclaration != null)
			{
				if (instruction.HasMultipleDeliveryTerms)
				{
					var invoice = instruction.Invoices.First();
					instruction.AddRowMessageError(Res.GetString("529AB4FF-8687-4F10-98C0-399E2A4B0930", "Entry Instruction '{0}' is linked to invoices with different delivery terms ({1}, {2}, {3}).", instruction.HumanReadableName, invoice.JZ_IncoTermInfo.HumanReadableName, invoice.JZ_IncoTermPlaceInfo.HumanReadableName, invoice.ZG_AgreedPlaceCodeInfo.HumanReadableName));
				}
				if (instruction.HasMultipleCurrencies)
				{
					instruction.AddRowMessageError(Res.GetString("529AB4FF-8687-4F10-98C0-399E2A4B0934", "Entry Instruction '{0}' is linked to invoices with different currencies ({1}).", instruction.HumanReadableName, instruction.Invoices.First().JZ_RX_NKInvoice_CurrencyInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckCEI_Style()
		{
			var ceiStyle = Parent.CEI_Style;
			if (ceiStyle.IsEmpty)
			{
				var targetInfo = Parent.CEI_StyleInfo;
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.Description));
			}
			else if (!((ExportCusEntryInstructionLookups)Parent.Lookups).DeclarationTypeListG2018.ContainsCode(ceiStyle))
			{
				Parent.CEI_StyleInfo.AddMessageError(Res.GetString("792116E2-7DCB-4AA7-B578-8AE3DEC62A0C", "As per the G0128 rule, if the Shipment Declaration type is 'EX' then, Entry Instruction's Declaration type can be 'B1', 'B2' or 'C1'; and for 'CO' Shipment type, the declaration type can be 'B3' or 'B4'."));
			}
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();

			var targetInfo = Parent.CEI_SubStyleInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			if (Parent.IsAmendmentValidationMode && !Parent.OriginalAdditionalDeclarationType.IsEmpty && Parent.OriginalAdditionalDeclarationType != Parent.CEI_SubStyle)
			{
				targetInfo.AddMessageError(CommonResStrings.ShouldNotAmendThisValue);
			}
			if (Parent.IsSupplementaryDeclarationForCode && Parent.JobDeclaration is JobDeclaration declaration && declaration.IsTransitionPeriodAES30 && !Parent.HasHeaderLevelPreviousDocuments())
			{
				targetInfo.AddMessageError(Res.GetString("10E5190C-AF0C-4EA4-A423-15DBE9E922F9", "For Sub Style X or Y, please enter at least one Previous Document at Entry Instruction or Invoice Header level."));
			}
		}

		void CheckAdditionalInfo1D23Mandatory()
		{
			if (!(Parent.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF || Parent.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic))
			{
				if (!(Parent.HasEstimatedTimeOfDepartureAdditionalInfo
					|| Parent.HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo))
				{
					Parent.AddRowMessageError(Constants.ValidationMessage.AdditionalInfo1D23FullTypeIsRequired);
				}
			}
		}
	}
}
