using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlSessionTracker : IXmlImportLogger
	{
		IEnumerable<IImportResult> ImportResults { get; }
		void LogLinkCreated(IEntityID parentEntityID, IEntityID childEntityID);
		void LogChildTopLevelObject(DataContextType dataContextType, Func<string> getDataContextKey);
		IDataWritingManager OutboundSessionTracker { get; }
		INotificationEmailManager NotificationEmailManager { get; }
		IEDIMessage SourceMessage { get; }
		bool HasErrors { get; }
		bool HasWarnings { get; }

		void StartProcessingASubShipment();
		void EndProcessingASubShipment();
		void RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType);
		void RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType);

		IDisposable SetCurrentMessageContext(ZGuid messagePK);

		bool IsCurrentMessageContextSet(ZGuid messagePK);

		IUserContext SessionUserContext { get; set; }
	}
}
