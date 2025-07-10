using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class IMDeclarationWrapper : SADDeclarationWrapper
{
	public IMDeclarationWrapper(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction) : base(jobDeclaration, entryInstruction)
	{
	}

	protected override ZString TypeDeclarationSubType3Core => ZString.Empty;
}
