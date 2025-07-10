using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class UCC5ImportCusEntryInstructionValidation : CommonImportCusEntryInstructionValidation
	{
		public UCC5ImportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidationSupportingDocuments();
		}

		void ValidationSupportingDocuments()
		{
			ValidateBR20319();
			var parent = Parent;
			if (parent.IsI1 && (parent.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration || parent.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC) && !IsC512ExistedOnSupportingDocuments(parent))
			{
				parent.AddRowMessageError(Res.GetString("4296277E-437E-40E2-B631-81633DE41410", "[BR1026] If Declaration Type is 'I1' and Sub Style is either 'C' or 'F' supporting documents must be submitted 'C512' at either Entry Instruction, Invoice Header or Invoice Line level."));
			}
		}

		bool IsC512ExistedOnSupportingDocuments(CusEntryInstruction instruction) => instruction.Factory.GetValue(ref isC512ExistedOnSupportingDocuments, () =>
			instruction.SupportingDocuments.Cast<SupportingDocument>()
			.Union(instruction.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(line => line.SupportingDocuments.Cast<SupportingDocument>()))
			.Union(instruction.Invoices.Cast<JobComInvoiceHeader>().SelectMany(header => header.SupportingDocuments.Cast<SupportingDocument>()))
			.Any(doc => doc.CSI_Code.EqualsIgnoringCase(Constants.SupportingDocumentCodes._C512)));
		CachedProperty<bool> isC512ExistedOnSupportingDocuments;

		void ValidateBR20319()
		{
			var parent = Parent;
			if (!parent.SupportingDocuments.Cast<SupportingDocument>().Any(Constants.SupportingDocumentCodes.IsEstimatedDestination))
			{
				parent.AddRowMessageError(Res.GetString("2EB547EF-BF85-4B12-97A8-8EEBF6562650", "[BR20319] You have not entered the mandatory {0} Scheduled Time of Arrival under Supporting Document.", Constants.SupportingDocumentCodes._1D24));
			}
		}

		protected override bool ShouldTriggerRuleBR3005(CusEntryInstruction parent) => !parent.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H2) &&
				parent.InvoiceLines.Select(line => line.JI_OA_ExporterAddress).Distinct().Take(2).Count() > 1 &&
				!parent.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsAnAdditionalInformation && info.CSI_Code == Constants.AdditionalInformationCodes._00200);

		protected override bool IsValidForBR2037(ZString supportingDocumentCode)
		{
			switch (supportingDocumentCode)
			{
				case Constants.SupportingDocumentCodes._D005:
				case Constants.SupportingDocumentCodes._D008:
				case Constants.SupportingDocumentCodes._N325:
				case Constants.SupportingDocumentCodes._N380:
				case Constants.SupportingDocumentCodes._N864:
				case Constants.SupportingDocumentCodes._N935:
				case Constants.SupportingDocumentCodes._1N09:
				case Constants.SupportingDocumentCodes._1N21:
				case Constants.SupportingDocumentCodes._1N22:
				case Constants.SupportingDocumentCodes._1N99:
					return true;
				default:
					return false;
			}
		}

		protected override string RuleBR2037ErrorMessage => Res.GetString("BDF738EF-A936-4C22-A5F3-5A6207BBFC31", "[BR2037] At least one of the following supporting documents is required under the Entry Instructions > Supporting Documents tab: 'D005', 'D008', 'N325', 'N380', 'N864', 'N935', '1N09', '1N21', '1N22', '1N99' provided that at least one invoice does not contain the additional procedure 'C08'.");
	}
}
