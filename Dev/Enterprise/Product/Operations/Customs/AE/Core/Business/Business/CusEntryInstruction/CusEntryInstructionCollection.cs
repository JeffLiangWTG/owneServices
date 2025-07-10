using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusEntryInstructionCollection : CusEntryInstructionCollection<CusEntryInstruction>
{
	public CusEntryInstructionCollection(JobDeclaration parentBO)
			: base(parentBO)
	{
	}
}
