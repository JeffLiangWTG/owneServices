using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportAdditionalInfoValidation : AdditionalInfoValidation
	{
		public ImportAdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			if (parent.IsAnAdditionalInformation && parent.CSI_Code.EqualsIgnoringCase(Constants.AdditionalInformationCodes.N9001))
			{
				var targetInfo = parent.CSI_CodeInfo;
				if (parent.Parent is JobComInvoiceLine invoiceLine)
				{
					if (!IsValidForBR2041(invoiceLine.RequestedPreviousProcedure))
					{
						targetInfo.AddMessageError(Res.GetString("F8AA825F-A415-49BA-8C31-4907E68CCAE2", "[BR2041] N9001 can only be declared for the following customs procedures: '4051', '4053', '4054', '4071', '5151', '5153', '5154', '5171', '5353', '7151', '7153', '7171'."));
					}

					if (!invoiceLine.PreviousDocuments.Cast<PreviousDocument>().Any(x => x.CSI_Code.EqualsIgnoringCase(Constants.PreviousDocumentTypeCodes.SAD)))
					{
						targetInfo.AddMessageError(Res.GetString("370D0480-1D20-4989-9DD6-D256F6E5C4AE", "[BR2041] N9001 can only be declared when the MRN of the original SAD document is declared at the invoice line level."));
					}
				}
				else
				{
					targetInfo.AddMessageError(Res.GetString("48253BA1-DBBA-4555-84C8-3EBB1DCB7087", "[BR2041] N9001 can only be entered on the invoice line level."));
				}
			}

			if (parent.Declaration is JobDeclaration declaration && declaration.CustomsEntryInstructions is ICusEntryInstructionCollection<CusEntryInstruction> cusEntryInstructions)
			{
				var isH1InstructionPresent = cusEntryInstructions.Any(x => x.CEI_Style == ImportDeclarationTypeList.Codes.H1);

				if (!isH1InstructionPresent && parent.IsAnAdditionalReference && parent.CSI_Code == Constants.AdditionalReferenceCodes._1A06)
				{
					parent.CSI_CodeInfo.AddMessageError(Res.GetString("1B9727A2-52DF-451F-A84F-B3FDFC583C10", "[BR600013] Additional reference 1A06 can only be declared when the declaration dataset is H1."));
				}

				if (declaration.IsSea && parent.IsATransportDocument && !IsEntryInstructionProcedureCodeWith76Or77(parent) && IsInValidDocumentCodesForBR1109(parent.CSI_Code))
				{
					parent.CSI_CodeInfo.AddMessageError(Res.GetString("7F2045AE-3C3A-4058-BF21-596FE836EAF9", "[BR1109] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then 'N740' or 'N741' Transport Document is not allowed."));
				}
			}

			ValidateCSI_Code_BR5153();
		}

		bool IsEntryInstructionProcedureCodeWith76Or77(AdditionalInfo parent) =>
		(parent.EntryInstruction?.IsCustomsWarehousingProcedure76Or77 ?? false) ||
		(parent.InvoiceLine?.IsCustomsWarehousingProcedure76Or77 ?? false) ||
		(parent.InvoiceHeader?.InvoiceLines?.Cast<JobComInvoiceLine>()?.Any(line => line.IsCustomsWarehousingProcedure76Or77) ?? false);

		bool IsValidForBR2041(ZString procedureCode)
		{
			switch (procedureCode)
			{
				case Constants.ProcedureCodesForBR2041._4051:
				case Constants.ProcedureCodesForBR2041._4053:
				case Constants.ProcedureCodesForBR2041._4054:
				case Constants.ProcedureCodesForBR2041._4071:
				case Constants.ProcedureCodesForBR2041._5151:
				case Constants.ProcedureCodesForBR2041._5153:
				case Constants.ProcedureCodesForBR2041._5154:
				case Constants.ProcedureCodesForBR2041._5171:
				case Constants.ProcedureCodesForBR2041._5353:
				case Constants.ProcedureCodesForBR2041._7151:
				case Constants.ProcedureCodesForBR2041._7153:
				case Constants.ProcedureCodesForBR2041._7171:
					return true;
				default:
					return false;
			}
		}

		bool IsInValidDocumentCodesForBR1109(ZString documentCodes)
		{
			switch (documentCodes)
			{
				case Constants.TransportDocumentCodes._N740:
				case Constants.TransportDocumentCodes._N741:
					return true;
				default:
					return false;
			}
		}

		void ValidateCSI_Code_BR5153()
		{
			var parent = Parent;
			if (parent.IsAuthorisationForSpecialProcedure()
				&& (
					parent.EntryInstruction is CusEntryInstruction instruction && instruction.HasAuthorisationInwardProcessingProcedureOnShipmentLevel && instruction.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel
					|| parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader && invoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction
				)
			)
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("4BE506C8-5228-4614-A4E8-805A7AF1329F", "[BR5153] If Requested Procedure is '51', '00100' Additional Information should not be declared when there is a 'C601' Supporting Document entered under the Entry Instructions > Supporting Documents tab or the Invoice Headers > Supporting Documents tab."));
			}
		}
	}
}
