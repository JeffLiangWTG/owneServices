using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Messaging.Integration
{
	public interface IInterchangeAcknowledgement
	{
		void Send(BusinessObjectFactory factory, INotifications notifications, IEDIInterchange interchange, string dataImportLog, IEnumerable<IValidationRule> validationRules, InterchangeAcknowledgementType acknowledgementType);
		void Send(BusinessObjectFactory factory, INotifications notifications, IEDIMessage message, string dataImportLog, IEnumerable<IValidationRule> validationRules, InterchangeAcknowledgementType acknowledgementType);
	}
}
