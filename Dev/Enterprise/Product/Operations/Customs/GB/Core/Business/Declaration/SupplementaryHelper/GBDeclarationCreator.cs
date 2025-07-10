using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration.SupplementaryHelper
{
	class GBDeclarationCreator : DeclarationCreator
	{
		public GBDeclarationCreator(EU.Business.Declaration.JobDeclaration declaration, IEntryHeaderFilter filter) : base(declaration, filter)
		{
		}

		protected override ZString GetProcedure(Customs.Business.CusEntryInstruction entryInstruction)
		{
			var invoiceLines = entryInstruction.InvoiceLines;
			return invoiceLines.Length > 0 ? invoiceLines[0].JI_Procedure.Left(2) : ZString.Empty;
		}

		protected override void UpdateInvoiceLine(BaseJobComInvoiceLine invoiceLine, ZString originalMRN)
		{
			base.UpdateInvoiceLine(invoiceLine, originalMRN);
			var document = ((JobComInvoiceLine)invoiceLine).PreviousDocuments.AddNew();
			document.CSI_SubType = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
			document.CSI_Code = PreviousDocumentCodeListCDS.Codes.SimplifiedDeclarationForSimplifiedDeclarationProcedureSdp;
			document.CSI_ReferenceNumber = originalMRN;
		}

		protected override void UpdateRelatedDeclaration(BaseJobDeclaration newDeclaration, BaseJobDeclaration declaration)
		{
			base.UpdateRelatedDeclaration(newDeclaration, declaration);
			newDeclaration.JE_UCR = declaration.JE_UCR;
		}
	}
}
