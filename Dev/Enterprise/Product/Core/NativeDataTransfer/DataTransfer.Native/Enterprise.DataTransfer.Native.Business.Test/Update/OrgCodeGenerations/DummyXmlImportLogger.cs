using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native
{
	class DummyXmlImportLogger : IXmlSessionTracker
	{
		public void Log(string logMsg) => Logs += logMsg;

		public void LogLinkCreated(IEntityID parentEntityID, IEntityID childEntityID)
		{
			throw new NotImplementedException();
		}

		public void LogChildTopLevelObject(DataContextType dataContextType, Func<string> getDataContextKey)
		{
			throw new NotImplementedException();
		}

		public void StartProcessingASubShipment()
		{
			throw new NotImplementedException();
		}

		public void EndProcessingASubShipment()
		{
			throw new NotImplementedException();
		}

		public void RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			throw new NotImplementedException();
		}

		public void RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			throw new NotImplementedException();
		}

		public void LogBoth(LogType type, string message)
		{
			Log(message);
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
			throw new NotImplementedException();
		}

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
			throw new NotImplementedException();
		}

		public void Log(LogType type, string message)
		{
			Log(message);
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
		}

		public IDisposable SetCurrentMessageContext(ZGuid messagePK)
		{
			throw new NotImplementedException();
		}

		public bool IsCurrentMessageContextSet(ZGuid messagePK)
		{
			throw new NotImplementedException();
		}

		public string Logs { get; set; }

		public IEnumerable<IImportResult> ImportResults => throw new NotImplementedException();

		public IDataWritingManager OutboundSessionTracker => throw new NotImplementedException();

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		public INotificationEmailManager NotificationEmailManager => throw new NotImplementedException();

		public bool IsUpdatingConsol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool HasIgnoredModule { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool OrgMatchingDisabled => false;

		public ITopLevelDataObject TopLevelDataObject => throw new NotImplementedException();

		public IDataContextDataObject TopLevelDataContext => throw new NotImplementedException();

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs => throw new NotImplementedException();

		public IEDIMessage SourceMessage { get; set; }

		public bool HasErrors => throw new NotImplementedException();

		public bool HasWarnings => throw new NotImplementedException();
		public IUserContext SessionUserContext { get; set; }
	}
}
