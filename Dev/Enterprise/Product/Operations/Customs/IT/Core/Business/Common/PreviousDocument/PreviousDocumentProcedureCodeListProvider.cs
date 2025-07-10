using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public interface IPreviousDocumentProcedureCodeListProvider
{
	CodeDescriptionPairList GetProcedureCodeList();
}

public sealed class PreviousDocumentProcedureCodeListProvider : IPreviousDocumentProcedureCodeListProvider
{
	public PreviousDocumentProcedureCodeListProvider(PreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}

	readonly PreviousDocument previousDocument;

	CodeDescriptionPairList IPreviousDocumentProcedureCodeListProvider.GetProcedureCodeList()
	{
		var factory = previousDocument.Factory;

		return IsImport
			? factory.GetCachedValue("IT.ImportPreviousDocumentProcedureList", () => SortCodeDescriptionPairList(new ImportPreviousDocumentProcedureList()))
			: factory.GetCachedValue("IT.PreviousDocumentProcedureList", () => SortCodeDescriptionPairList(new PreviousDocumentProcedureList()));
	}

	bool IsImport => previousDocument.ImportExportParent?.IsImport ?? false;

	CodeDescriptionPairList SortCodeDescriptionPairList(CodeDescriptionPairList procedureCodeList)
	{
		procedureCodeList.Sort();
		return procedureCodeList;
	}
}
