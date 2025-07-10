using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IAttachedDocumentDataObjectReader
	{
		bool TryAddAttachedDocument(IAttachedDocument attachedDocument, IXmlImportLogger logger, IDocManagerSupportBase parent, out IeDoc eDoc);
	}
}
