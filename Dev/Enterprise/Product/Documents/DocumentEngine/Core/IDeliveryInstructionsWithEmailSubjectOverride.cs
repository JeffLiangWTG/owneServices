using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	public interface IDeliveryInstructionsWithEmailSubjectOverride
	{
		// This Interface was introduced to provide the ability of overriding the EmailSubject when creating StmPrintJob to be inserted into eDocs.
		// It will be removed when the setting of filename in eDocs is fixed to get from correct field in DbBackendDocumentFactory
		ZString EmailSubjectOverride { get; set; }
	}
}
