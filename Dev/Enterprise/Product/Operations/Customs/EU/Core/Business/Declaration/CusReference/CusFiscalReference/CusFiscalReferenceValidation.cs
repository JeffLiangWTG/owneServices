using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusFiscalReferenceValidation : CommonCusReferenceValidation
	{
		public CusFiscalReferenceValidation(CusFiscalReference parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();
			ValidateRuleR0010();
		}

		protected override void CheckCFR_Code()
		{
			base.CheckCFR_Code();
			var parent = Parent;

			if (parent.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative
				&& parent.CFR_ParentTableCode.EqualsIgnoringCase(CusEntryInstructionSchema.Constants.Prefix)
				&& parent.Instruction?.EntryHeader is CusEntryHeader entry
				&& entry.IsAllEntryLinesVatSuspended)
			{
				parent.CFR_CodeInfo.AddMessageError(Res.GetString("85CB933C-2D04-4BC4-8352-BD2AD91E324B", "VAT is suspended on all entry lines; FR3 is likely not necessary."));
			}
			ValidateFiscalReferenceAvailability();
		}

		void ValidateRuleR0010()
		{
			var parent = Parent;
			if (parent.Parent is ICusFiscalReferenceProviderWithValidationDecider provider
				&& provider.ValidationDecider is ICusFiscalReferenceValidationDecider validationDecider
				&& validationDecider.IsRuleR0010Active
				&& parent.Declaration is JobDeclaration declaration)
			{
				var ruleR0010MessageError = Res.GetString("682E1792-76F0-4489-8FAA-FFDF676294E1", "[R0010] Value can’t be entered in both Entry Instruction and Invoice lines.");
				var code = parent.CFR_Code;
				var number = parent.CFR_Reference;
				switch (parent.CFR_ParentTableCode.ToUpperInvariant())
				{
					case CusEntryInstructionSchema.Constants.Prefix:
						if (parent.Instruction is CusEntryInstruction cusEntryInstruction
							&& cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.FiscalReferences.Any(y => y.CFR_Code == code && y.CFR_Reference == number)))
						{
							Parent.AddRowMessageError(ruleR0010MessageError);
						}
						break;
					case JobComInvoiceLineSchema.Constants.Prefix:
						if (parent.InvoiceLine?.EntryInstruction is CusEntryInstruction instruction
							&& instruction.FiscalReferences.Any(x => x.CFR_Code == code && x.CFR_Reference == number))
						{
							Parent.AddRowMessageError(ruleR0010MessageError);
						}
						break;
				}
			}
		}

		static bool IsValidStyleForInstruction(ZString style)
		{
			switch (style.ToUpperInvariant())
			{
				case EUCommonConstants.ImportDeclarationTypeList.H1:
				case EUCommonConstants.ImportDeclarationTypeList.H6:
				case EUCommonConstants.ImportDeclarationTypeList.H7:
				case EUCommonConstants.ImportDeclarationTypeList.I1:
					return true;
				default:
					return false;
			}
		}

		void ValidateFiscalReferenceAvailability()
		{
			var parent = Parent;
			if (parent.Parent is ICusFiscalReferenceProviderWithValidationDecider provider
				&& provider.ValidationDecider is ICusFiscalReferenceValidationDecider validationDecider
				&& validationDecider.IsFiscalReferenceAvailabilityValidationActive
				&& parent.Declaration is JobDeclaration declaration)
			{
				switch (parent.CFR_ParentTableCode.ToUpperInvariant())
				{
					case CusEntryInstructionSchema.Constants.Prefix:
						if (parent.Instruction is CusEntryInstruction instruction
							&& !IsValidStyleForInstruction(instruction.CEI_Style))
						{
							parent.CFR_CodeInfo.AddWarning(Res.GetString("E3423CF1-7B4E-442E-A846-91D64AA9BF8D", "Entry Instruction Additional Fiscal References are only required when Dataset (Declaration Type) is H1, H6, H7 or I1."));
						}
						break;
					case JobComInvoiceLineSchema.Constants.Prefix:
						if (parent.InvoiceLine.EntryInstruction is CusEntryInstruction entryInstruction
							&& !entryInstruction.CEI_Style.EqualsIgnoringCase(EUCommonConstants.ImportDeclarationTypeList.I1)
							&& !entryInstruction.CEI_Style.EqualsIgnoringCase(EUCommonConstants.ImportDeclarationTypeList.H1))
						{
							parent.CFR_CodeInfo.AddWarning(Res.GetString("EA67F27E-08BE-49FD-83EA-F41EEB361218", "Invoice Line Additional Fiscal References are only required when Dataset (Declaration Type) is H1 or I1."));
						}
						break;
				}
			}
		}

		protected new readonly CusFiscalReference Parent;
	}
}
