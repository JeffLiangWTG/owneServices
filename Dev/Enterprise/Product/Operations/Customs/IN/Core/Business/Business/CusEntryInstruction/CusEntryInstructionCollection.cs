using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class CusEntryInstructionCollection : CusEntryInstructionCollection<CusEntryInstruction>
{
	public CusEntryInstructionCollection(JobDeclaration parentBO)
			: base(parentBO)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		if (child is CusEntryInstruction entryInstruction && entryInstruction.IsExport)
		{
			entryInstruction.CEI_Style = DeclarationTypeList.Codes.ForeignExchangeInvolved;
			entryInstruction.CEI_WeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}
