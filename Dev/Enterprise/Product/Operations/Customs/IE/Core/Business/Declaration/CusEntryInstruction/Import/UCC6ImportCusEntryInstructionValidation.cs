using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class UCC6ImportCusEntryInstructionValidation : CommonImportCusEntryInstructionValidation
	{
		public UCC6ImportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}
		protected override bool ShouldTriggerRuleBR3005(CusEntryInstruction parent) => !parent.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H2) &&
				parent.Invoices.All(invoice => invoice.ExporterAddress == null) &&
				!parent.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsAnAdditionalInformation && info.CSI_Code == Constants.AdditionalInformationCodes._00200);

		protected override string RuleBR2037ErrorMessage => Res.GetString("78B767AD-9A76-4A35-ADB2-C60835F85FB7", "[BR2037] At least one of the following supporting documents is required under the Entry Instructions > Supporting Documents tab: 'D005', 'D008', 'N325', 'N380', 'N864', 'N935', provided that at least one invoice does not contain the additional procedure 'C08'.");

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
					return true;
				default:
					return false;
			}
		}
	}
}
