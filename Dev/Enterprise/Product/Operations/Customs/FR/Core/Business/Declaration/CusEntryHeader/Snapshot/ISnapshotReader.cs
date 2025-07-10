using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.Business.Declaration;

public interface ISnapshotReader
{
	IDisposable TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(CusEntryHeader entryHeader, Customs.Business.SnapshotRevertingStrategy strategy, IXmlImportLogger snapshotReaderLogger);
}
