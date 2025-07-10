using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonImportOperationWrapper : IH1CommonImportOperation
{
	public ImportH1CommonImportOperationWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
	}
	protected readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;

	public ZString LRN => entryHeader.CH_BGMReference;

	public ZString DeclarationType => declaration.JE_MessageSubType;
}
