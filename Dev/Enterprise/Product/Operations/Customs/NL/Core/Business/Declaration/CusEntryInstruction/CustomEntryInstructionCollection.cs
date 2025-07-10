using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryInstructionCollection : EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>
{
	readonly JobDeclaration _parentDeclaration;

	public CusEntryInstructionCollection(JobDeclaration parentDeclaration) : base(parentDeclaration)
	{
		_parentDeclaration = parentDeclaration;
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		if (child is CusEntryInstruction entryInstruction && _parentDeclaration.JE_MessageType.Equals(MessageTypeList.Codes.Export))
		{
			entryInstruction.CEI_Style = DeclarationTypeList.Codes.B1;
			entryInstruction.CEI_SubStyle = DeclarationSubTypeList.Codes.A;
			entryInstruction.CEI_Procedure = ProcedureCodes._10;
			entryInstruction.ZG_TransNature = NatureOfTransactionList.Codes._11;
		}
	}
}
