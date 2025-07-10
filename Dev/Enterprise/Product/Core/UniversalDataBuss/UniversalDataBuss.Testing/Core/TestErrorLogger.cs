using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	public class TestErrorLogger : IXmlSessionTracker
	{
		public TestErrorLogger()
		{
			this.logs = new List<SimpleLog>();
		}

		readonly List<SimpleLog> logs;

		public string Logs
		{
			get { return string.Join("\r\n", logs.Select(log => log.Type.ToString() + " - " + log.Message)); }
		}

		public void Log(LogType type, string message)
		{
			logs.Add(new SimpleLog(type, message));

			if (type == LogType.Error)
			{
				Errors.Add(message);
			}

			if (type == LogType.Warning)
			{
				Warnings.Add(message);
			}
		}

		public void LogBoth(LogType type, string message)
		{
			Log(type, message);
		}

		public bool HasErrors
		{
			get { return Errors.Any(); }
		}

		public bool HasWarnings
		{
			get { return Warnings.Any(); }
		}

		public IUserContext SessionUserContext { get; set; }

		public string GetWarnings()
		{
			return string.Join("\r\n", Warnings);
		}

		public string GetErrors()
		{
			return string.Join("\r\n", Errors);
		}

		internal readonly List<string> Errors = new List<string>();
		internal readonly List<string> Warnings = new List<string>();

		public void ClearLogs()
		{
			Errors.Clear();
			Warnings.Clear();
			logs.Clear();
			linkedEntitiesInternal = null;
		}

		public bool IsUpdatingConsol { get; set; }

		public bool HasIgnoredModule { get; set; }

		public bool OrgMatchingDisabled => false;

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
		}

		public ITopLevelDataObject TopLevelDataObject { get; set; }

		public IDataContextDataObject TopLevelDataContext
		{
			get { return TopLevelDataObject == null ? null : TopLevelDataObject.DataContext; }
		}

		public void StartProcessingASubShipment()
		{
		}

		public void EndProcessingASubShipment()
		{
		}

		public void RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
		}

		public void RecordAUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
		}

		public void RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
		}

		#region ISimpleLogResult Members

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs
		{
			get { return logs; }
		}

		#endregion

		#region IXmlSessionTracker

		public IEnumerable<IImportResult> ImportResults
		{
			get { return Enumerable.Empty<IImportResult>(); }
		}

		public void LogChildTopLevelObject(DataContextType dataContextType, Func<string> getDataContextKey)
		{
			ChildTopLevelObjectsInternal.Add(new EntityID(dataContextType, getDataContextKey));
		}

		public IEnumerable<IEntityID> ChildTopLevelObjects
		{
			get { return ChildTopLevelObjectsInternal; }
		}

		List<IEntityID> ChildTopLevelObjectsInternal
		{
			get { return childTopLevelObjectsInternal ?? (childTopLevelObjectsInternal = new List<IEntityID>()); }
		}

		List<IEntityID> childTopLevelObjectsInternal;

		class EntityID : IEntityID
		{
			public EntityID(DataContextType dataContextType, Func<string> getDataContextKey)
			{
				DataContextType = dataContextType;
				GetDataContextKey = getDataContextKey;
			}

			readonly DataContextType DataContextType;
			readonly Func<string> GetDataContextKey;

			string IEntityID.DataContextKey
			{
				get { return GetDataContextKey(); }
			}

			DataContextType IEntityID.DataContextType
			{
				get { return DataContextType; }
			}
		}

		public void LogLinkCreated(IEntityID parentEntityID, IEntityID childEntityID)
		{
			LinkedEntitiesInternal.Add(new EntityLink(parentEntityID, childEntityID));
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
			throw new NotImplementedException();
		}

		public IDisposable SetCurrentMessageContext(ZGuid messagePK)
		{
			throw new NotImplementedException();
		}

		public bool IsCurrentMessageContextSet(ZGuid messagePK)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<EntityLink> LinkedEntities
		{
			get { return LinkedEntitiesInternal; }
		}

		List<EntityLink> LinkedEntitiesInternal
		{
			get { return linkedEntitiesInternal ?? (linkedEntitiesInternal = new List<EntityLink>()); }
		}

		List<EntityLink> linkedEntitiesInternal;

		public class EntityLink
		{
			public EntityLink(IEntityID parentEntityID, IEntityID childEntityID)
			{
				ParentEntityID = parentEntityID;
				ChildEntityID = childEntityID;
			}

			public IEntityID ParentEntityID { get; private set; }
			public IEntityID ChildEntityID { get; private set; }

			public override string ToString()
			{
				return "Parent:" + ParentEntityID.DataContextType + "-" + ParentEntityID.DataContextKey + "|Child:" + ChildEntityID.DataContextType + "-" + ChildEntityID.DataContextKey;
			}
		}

		public IDataWritingManager OutboundSessionTracker
		{
			get;
			set;
		}

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		public INotificationEmailManager NotificationEmailManager => notificationEmailManager ?? (notificationEmailManager = new NotificationEmailManager());

		public IEDIMessage SourceMessage { get; set; }

		INotificationEmailManager notificationEmailManager;

		#endregion
	}
}

