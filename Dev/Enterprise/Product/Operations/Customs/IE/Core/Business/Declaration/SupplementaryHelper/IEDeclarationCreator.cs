using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper;

namespace Enterprise.Customs.IE.Business.Declaration.SupplementaryHelper
{
	class IEDeclarationCreator : DeclarationCreator
	{
		public IEDeclarationCreator(EU.Business.Declaration.JobDeclaration declaration, IEntryHeaderFilter filter) : base(declaration, filter)
		{
		}

		protected override ZString GetProcedure(Customs.Business.CusEntryInstruction entryInstruction)
			=> entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault()?.JI_Calc_RequestedProcedure ?? ZString.Empty;

		protected override void UpdateEntryInstruction(Customs.Business.CusEntryInstruction entryInstruction, ZString originalMRN)
		{
			base.UpdateEntryInstruction(entryInstruction, originalMRN);
			var document = (entryInstruction as CusEntryInstruction).PreviousDocuments.AddNew();
			document.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
			document.CSI_ReferenceNumber = originalMRN;
		}
	}
}
