using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Business;

public interface ISadImportExportMessageSubProcessor
{
	void PerformActionsForPositiveIrisp(SadPositiveResponseMessage positiveResponseMessage);

	void PerformActionsForNegativeIrisp(SadNegativeResponseMessage negativeResponseMessage);

	void UpdateEntryStatusAfterChildMessagesProcessing();
}
