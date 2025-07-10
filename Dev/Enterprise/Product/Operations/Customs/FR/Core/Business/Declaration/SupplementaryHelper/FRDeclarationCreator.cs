using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper;

namespace Enterprise.Customs.FR.Business.Declaration.SupplementaryHelper
{
	public class FRDeclarationCreator : DeclarationCreator
	{
		public FRDeclarationCreator(JobDeclaration declaration, IEntryHeaderFilter filter) : base(declaration, filter)
		{
		}

		protected override void UpdateEntryInstruction(Customs.Business.CusEntryInstruction entryInstruction, ZString originalMRN)
		{
			base.UpdateEntryInstruction(entryInstruction, originalMRN);

			if (entryInstruction is CusEntryInstruction frEntryInstruction)
			{
				var document = frEntryInstruction.PreviousDocuments.AddNew();
				document.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				document.CSI_ReferenceNumber = originalMRN;
			}
		}

		protected override ZString GetDefaultEntryStyleFromProcedure(Customs.Business.CusEntryInstruction entryInstruction)
		{
			var result = base.GetDefaultEntryStyleFromProcedure(entryInstruction);

			if (((CusEntryInstruction)entryInstruction).ValueSetStrategy is DeltaIECusEntryInstructionValueSetStrategy valueSetStrategy)
			{
				result = valueSetStrategy.GetDefaultEntryStyleFromProcedure();
			}

			return result;
		}

		protected override void UpdateRelatedDeclaration(Customs.Business.BaseJobDeclaration newDeclaration, Customs.Business.BaseJobDeclaration declaration)
		{
			base.UpdateRelatedDeclaration(newDeclaration, declaration);
			newDeclaration.JE_UCR = declaration.JE_UCR;
		}
	}
}
