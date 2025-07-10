using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public abstract class SADDeclarationWrapper : IDeclaration
{
	public SADDeclarationWrapper(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction)
	{
		this.jobDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
	}

	protected readonly JobDeclaration jobDeclaration;
	readonly CusEntryInstruction entryInstruction;

	public ZString TypeDeclarationSubType1 => jobDeclaration.JE_MessageSubType.Left(2);

	public ZString TypeDeclarationSubType2 => entryInstruction.CEI_SubStyle;

	public ZString TypeDeclarationSubType3 => TypeDeclarationSubType3Core;

	protected abstract ZString TypeDeclarationSubType3Core { get; }
}
