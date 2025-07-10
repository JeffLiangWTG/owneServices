using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IAcknowledgementUniversalEventCreator
	{
		IXmlEventValueObject CreateUniversalEvent(Dictionary<string, string> acknowledgementContextCollection, string processingResultStatus, string dataImportLog, IEnumerable<IValidationRule> validationRules);
	}
}
