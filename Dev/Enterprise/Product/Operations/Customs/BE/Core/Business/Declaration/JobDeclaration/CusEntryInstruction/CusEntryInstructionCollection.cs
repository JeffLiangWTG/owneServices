using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryInstructionCollection : CusEntryInstructionCollection<CusEntryInstruction>
{
	public CusEntryInstructionCollection(JobDeclaration parentBO) : base(parentBO)
	{
	}
}
