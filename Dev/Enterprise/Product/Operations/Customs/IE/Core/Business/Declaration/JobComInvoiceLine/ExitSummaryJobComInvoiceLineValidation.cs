using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryJobComInvoiceLineValidation : CommonExportJobComInvoiceLineValidation
	{
		public ExitSummaryJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckJI_Tariff_NoPackage()
		{
			if (Declaration is JobDeclaration declaration
				&& !declaration.IsExpressConsignmentsOfExitSummary
				&& Parent.EntryInstruction is CusEntryInstruction instruction
				&& instruction.CEI_Style.EqualsIgnoringCase(ExitSummaryDeclarationTypeList.Codes.A1))
			{
				CheckJI_Tariff_NoPackage_Export(declaration);
			}
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();

			MandatoryValidation.MessageErrorIfIsZero(Parent.JI_WeightInfo);
		}
	}
}
