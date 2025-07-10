using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class ETDeclarationWrapper : SADDeclarationWrapper
{
	public ETDeclarationWrapper(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction) : base(jobDeclaration, entryInstruction)
	{
	}

	protected override ZString TypeDeclarationSubType3Core => jobDeclaration.ZG_CTStatusID;
}
