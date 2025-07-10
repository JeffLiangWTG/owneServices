using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
	{
		public AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		protected new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

		protected new AddInfoCusEntryInstructionLookups Lookups
		{
			get
			{
				return ((JobDeclaration)Parent.Parent.JobDeclaration)?.ApplicationExtender.GetAddInfoCusEntryInstructionLookups(Parent) ?? new DeltaGAddInfoCusEntryInstructionLookups(Parent);
			}
		}

		protected override void CheckZG_TransNature()
		{
			base.CheckZG_TransNature();

			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_TransNatureInfo, Lookups.TransNatureList);
			CheckRuleR0012AndC0002();
			CheckRuleC0627();
		}

		void CheckRuleC0627()
		{
			var parent = Parent;
			var entryInstruction = parent.Parent;

			if (entryInstruction.Validation.ValidationDecider is IEntryInstructionValidationDecider { IsRuleC0627Active: true })
			{
				if (entryInstruction.IsSimplifiedOrPreliminaryUnderCodeC)
				{
					if (!parent.ZG_TransNature.IsEmpty)
					{
						parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("98C39AE9-42A1-4C1C-9845-5BD978AFB891", "[C0627] Transaction Nature must be empty in case of Declaration Sub Type C or F."));
					}
				}
				else
				{
					if (entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ZG_TransNature.IsEmpty) && Parent.ZG_TransNature.IsEmpty)
					{
						parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("E2FA7C90-06D7-44F4-B38A-76A90CD17F65", "[C0627] Transaction Nature is mandatory for this Declaration Sub Type."));
					}
				}
			}
		}

		protected override void CheckZG_BypassCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_BypassCodeInfo, Lookups.ValuationBypassCodeList, ListValidation.InvalidCodeMessageError);

			ValidateZG_BypassReason();
		}

		protected override void CheckZG_BypassReason()
		{
			var parent = Parent;
			if (parent.ZG_BypassCode == FRConstants.ValuationBypassCodes.ReasonRequiredCode && parent.ZG_BypassReason.IsEmpty)
			{
				parent.ZG_BypassReasonInfo.AddMessageError(Res.GetString("8225F695-382E-4312-A63E-DB576773E623", "Valuation bypass reason is required when valuation bypass code is 'I'"));
			}
		}

		void CheckRuleR0012AndC0002()
		{
			var parent = Parent;
			var entryInstruction = parent.Parent;
			var entryInstructionValidationDecider = entryInstruction.Validation.ValidationDecider as IEntryInstructionValidationDecider;

			if ((entryInstructionValidationDecider?.IsRuleR0012Active ?? false) &&
				(entryInstructionValidationDecider?.IsRuleC0002Active ?? false) &&
				entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => !l.ZG_TransNature.IsEmpty) &&
				entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.ZG_TransNature.IsEmpty))
			{
				if (parent.ZG_TransNature.IsEmpty)
				{
					parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("8c0a43fe-2726-4c2e-9571-e07a331745e2", "[R0012 & C0002] If Nature Of Transaction is not entered on all Invoice Lines, it must be entered in Entry Instruction."));
				}
				else
				{
					parent.ZG_TransNatureInfo.AddWarning(Res.GetString("45c134f9-c33f-4bc9-abb4-995e2ec22498", "Invoice Lines Nature of Transaction will be mapped from the Entry Instruction value."));
				}
			}
		}
	}
}
