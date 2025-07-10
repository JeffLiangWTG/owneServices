using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceLineImportSupportingDocumentValidation : ImportSupportingDocumentValidation
	{
		public InvoiceLineImportSupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
		}

		JobComInvoiceLine invoiceLine => (JobComInvoiceLine)Parent.Parent;

		protected override void CheckCSI_UnitOfQuantity()
		{
			InvoiceLineSupportingDocumentValidation.CheckCSI_UnitOfQuantity(Parent);
		}

		protected override void CheckCSI_RX_NKCurrency()
		{
			InvoiceLineSupportingDocumentValidation.CheckCSI_RX_NKCurrency(Parent);
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (Parent.CSI_Code == Constants.SupportingDocumentCodes._1A05 && IsInstructionH1_H2_H3_H4_H5() && invoiceLine.Declaration is JobDeclaration declaration && declaration.IsUCC5)
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("1EDC37BF-8B29-4EB6-8661-8DCFDEC7A177", "[BR2030] Please enter Supporting Document '1A05' at Entry Instruction or Invoice Header level."));
			}

			bool IsInstructionH1_H2_H3_H4_H5() => invoiceLine.EntryInstruction is CusEntryInstruction instruction && (instruction.IsH1 || instruction.IsH2 || instruction.IsH3 || instruction.IsH4 || instruction.IsH5);
		}

		protected override string BR20314ErrorMessage => Res.GetString("9611116B-A0E4-470D-89B5-67699B2124AD", "[BR20314] Supporting document type C100 (REX Registered Exporter Number) must be unique across all invoice lines under the same entry instruction.");

		protected override bool BR20314_C100Duplicating(BusinessObject parentOfSupportingDocument, ZString c100ReferenceNumber)
			=> parentOfSupportingDocument is JobComInvoiceLine invoiceLine
			&& invoiceLine.EntryInstruction is CusEntryInstruction instruction
			&& instruction.DuplicatedInvoiceLineC100SupportingDocReferences.Contains(c100ReferenceNumber);
	}
}
