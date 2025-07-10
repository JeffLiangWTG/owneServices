using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGovernmentProcedureWrapper : IDeclarationGovernmentProcedure
	{
		DeclarationGovernmentProcedureWrapper(CusEntryInstruction cusEntryInstruction)
		{
			this.cusEntryInstruction = cusEntryInstruction;
		}
		readonly CusEntryInstruction cusEntryInstruction;

		public static DeclarationGovernmentProcedureWrapper NewOrNull(CusEntryInstruction cusEntryInstruction) => cusEntryInstruction == null ? null : new DeclarationGovernmentProcedureWrapper(cusEntryInstruction);

		public ICodeType CurrentCode => CodeTypeWrapper.NewOrNull(cusEntryInstruction.CEI_FormattedProcedure);
	}
}
