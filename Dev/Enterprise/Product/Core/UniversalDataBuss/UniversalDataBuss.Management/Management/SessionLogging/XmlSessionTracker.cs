using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Management.SessionLogging;

namespace Enterprise.UniversalDataBuss.Management
{
	public class XmlSessionTracker : SimpleLogger, IXmlSessionTracker
	{
		public XmlSessionTracker(ISimpleLogger taskLogger)
		{
			Argument.NotNull(taskLogger, nameof(taskLogger));
			this.taskLogger = new TaskLogger(taskLogger);
			this.importResults = new List<IImportResult>();
			this.individualChildLoggers = new List<IndividualImportLogger>();
			this.unknownAddressTypeWarningInfoKeeperStack = new Stack<UnknownAddressTypeWarningInfoKeeper>();
			this.currentMessagePK = null;
		}

		ZGuid? currentMessagePK;
		readonly ISimpleLogger taskLogger;
		readonly List<IImportResult> importResults;
		readonly List<IndividualImportLogger> individualChildLoggers;
		readonly Stack<UnknownAddressTypeWarningInfoKeeper> unknownAddressTypeWarningInfoKeeperStack;
		UnknownAddressTypeWarningInfoKeeper currentUnknownAddressTypeWarningInfoKeeper;

		public IEDIMessage SourceMessage { get; internal set; }
		public IUserContext SessionUserContext { get; set; }

		#region Import Attempt Processing

		IEnumerable<IImportResult> IXmlSessionTracker.ImportResults
		{
			get { return importResults; }
		}

		void IXmlSessionTracker.LogLinkCreated(IEntityID parentEntityID, IEntityID childEntityID)
		{
			if (!this.IsInternalImport() || individualImportLogger == null)
			{
				throw new InvalidOperationException("You cannot call LogLinkCreated without calling IndividualImportBegin first within an internal send.");
			}

			if (parentEntityID != null && childEntityID != null)
			{
				individualImportLogger.AddLinkedJobs(parentEntityID, childEntityID);
			}
		}

		public void IndividualImportBegin(DataContextType? dataContextType)
		{
			individualImportLogger = new IndividualImportLogger(dataContextType);
			individualChildLoggers.Clear();
		}

		IndividualImportLogger individualImportLogger;

		public void IndividualImportEnd(bool reportLogAsAResult = true)
		{
			if (individualImportLogger == null)
			{
				throw new InvalidOperationException("You cannot call IndividualImportEnd without calling IndividualImportBegin first.");
			}

			importResults.AddRange(individualChildLoggers);
			individualChildLoggers.Clear();

			if (reportLogAsAResult)
			{
				importResults.Add(individualImportLogger);

				var originalLogger = individualImportLogger;
				if (((IImportResult)originalLogger).LinkedJobs.Any())
				{
					IndividualImportBegin(null); // this new logger will be nulled out just below
					originalLogger.TransferLinkedJobs(individualImportLogger);
					importResults.Add(individualImportLogger); // Add the Linked Jobs as a separate Import Result
				}
			}

			individualImportLogger = null;
		}

		internal void IndividualSetDataContextKeyIfPossible(IEnumerable<IEntityID> linkedJobs)
		{
			if (individualImportLogger == null)
			{
				throw new InvalidOperationException("You cannot call IndividualSetDataContextKeyIfPossible without calling IndividualImportBegin first.");
			}
			var matchLinkedJobs = linkedJobs.Where(x => x.DataContextType == individualImportLogger.DataContextType).Take(2).ToArray();
			if (matchLinkedJobs.Length == 1)
			{
				individualImportLogger.SetDataContextKey(() => matchLinkedJobs[0].DataContextKey);
			}
		}

		internal void LogWasNotUsedByModule(LogType type, string message)
		{
			IndividualImportBegin(null);
			LogBoth(type, message);
			individualImportLogger.LogWasNotUsedByAModule();
			IndividualImportEnd();
		}

		void IXmlSessionTracker.LogChildTopLevelObject(DataContextType dataContextType, Func<string> getDataContextKey)
		{
			var individualChildLogger = new IndividualImportLogger(dataContextType);
			individualChildLogger.SetDataContextKey(getDataContextKey);
			individualChildLoggers.Add(individualChildLogger);
		}

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
			if (individualImportLogger == null)
			{
				throw new InvalidOperationException("You cannot call LogTopLevelDataContextKey without calling IndividualImportBegin first.");
			}

			individualImportLogger.SetDataContextKey(() => getDataContextKey.Invoke());
		}

		protected override void LogCore(LogType type, string message)
		{
			if (individualImportLogger != null)
			{
				individualImportLogger.Log(type, message);
			}

			base.LogCore(type, message);
		}

		#endregion

		public void LogBoth(LogType type, string message)
		{
			Log(type, message);
			taskLogger.Log(type, message);
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
			taskLogger.Log(LogType.Error, message);
		}

		public bool IsUpdatingConsol { get; set; }

		public bool HasIgnoredModule { get; set; }

		public bool OrgMatchingDisabled => false;

		public ITopLevelDataObject TopLevelDataObject { get; internal set; }

		public IDataContextDataObject TopLevelDataContext
		{
			get { return TopLevelDataObject == null ? null : TopLevelDataObject.DataContext; }
		}

		void IXmlImportLogger.FireDataImportedToBusinessObject(BusinessObject boImportedTo)
		{
			if (DataImportedToBusinessObject != null)
			{
				DataImportedToBusinessObject(boImportedTo);
			}
		}

		#region DataImportedToBusinessObject Event and Hooking

		event DataImportedToBusinessObjectAction DataImportedToBusinessObject;

		internal IDisposable RegisterForBusinessObjectImported(DataImportedToBusinessObjectAction dataImportedToBusinessObject)
		{
			return new DataImportedToBusinessObjectEventHookingManager(this, dataImportedToBusinessObject);
		}

		class DataImportedToBusinessObjectEventHookingManager : IDisposable
		{
			internal DataImportedToBusinessObjectEventHookingManager(XmlSessionTracker logger, DataImportedToBusinessObjectAction dataImportedToBusinessObject)
			{
				this.logger = logger;
				this.dataImportedToBusinessObject = dataImportedToBusinessObject;

				logger.DataImportedToBusinessObject += dataImportedToBusinessObject;
			}
			readonly XmlSessionTracker logger;
			readonly DataImportedToBusinessObjectAction dataImportedToBusinessObject;

			void IDisposable.Dispose()
			{
				logger.DataImportedToBusinessObject -= dataImportedToBusinessObject;
			}
		}

		#endregion

		#region INotificationEmailManager

		public INotificationEmailManager NotificationEmailManager => notificationEmailManager ?? (notificationEmailManager = new NotificationEmailManager());
		INotificationEmailManager notificationEmailManager;

		#endregion

		public void WriteTo(Stream stream)
		{
			var logLines = LogLines;
			var firstLine = logLines.FirstOrDefault();
			if (firstLine != null)
			{
				var writer = new StreamWriter(stream);
				writer.Write(firstLine);
				foreach (var logLine in LogLines.Skip(1))
				{
					writer.Write("\r\n" + logLine);
				}

				writer.Flush();
				stream.Position = 0;
			}
		}

		public IDataWritingManager OutboundSessionTracker
		{
			get;
			set;
		}

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		public void StartProcessingASubShipment()
		{
			var keeper = new UnknownAddressTypeWarningInfoKeeper();
			this.unknownAddressTypeWarningInfoKeeperStack.Push(keeper);
			this.currentUnknownAddressTypeWarningInfoKeeper = keeper;
		}

		public void EndProcessingASubShipment()
		{
			this.currentUnknownAddressTypeWarningInfoKeeper = null;
		}

		public void RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			if (this.currentUnknownAddressTypeWarningInfoKeeper != null)
			{
				this.currentUnknownAddressTypeWarningInfoKeeper.RecordUsedAddressType(addressType);
			}
		}

		public void RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			if (this.currentUnknownAddressTypeWarningInfoKeeper != null)
			{
				this.currentUnknownAddressTypeWarningInfoKeeper.RecordUnknownAddressTypeWarning(addressType, this.logs.Count - 1);
			}
		}

		public void CheckAndRemoveUnnecessaryUnknownAddressTypeWarnings()
		{
			this.unknownAddressTypeWarningInfoKeeperStack.ForEach(x => x.CheckAndRemoveUnnecessaryUnknownAddressTypeWarnings(logs));
			ResetWarningIndicator();
		}

		public IDisposable SetCurrentMessageContext(ZGuid messagePK)
		{
			currentMessagePK = messagePK;
			return new DisposableAction(() => {
				currentMessagePK = null;
			});
		}

		public bool IsCurrentMessageContextSet(ZGuid messagePK)
		{
			return currentMessagePK != null && currentMessagePK == messagePK;
		}
	}

	class UnknownAddressTypeWarningInfoKeeper
	{
		readonly HashSet<string> usedAddressType = new HashSet<string>();
		readonly Stack<Tuple<string, int>> warningInfoStack = new Stack<Tuple<string, int>>();

		public void RecordUsedAddressType(string addressType)
		{
			usedAddressType.Add(addressType);
		}

		public void RecordUnknownAddressTypeWarning(string addressType, int logIndex)
		{
			warningInfoStack.Push(new Tuple<string, int>(addressType, logIndex));
		}

		public bool IsAddressTypeUsed(string addressType)
		{
			return usedAddressType.Contains(addressType);
		}

		public void CheckAndRemoveUnnecessaryUnknownAddressTypeWarnings(List<ISimpleLog> logs)
		{
			warningInfoStack.ForEach(x => CheckAndRemoveUnknownAddressTypeWarning(logs, x.Item1, x.Item2));
		}

		void CheckAndRemoveUnknownAddressTypeWarning(List<ISimpleLog> logs, string addressType, int logIndex)
		{
			if (usedAddressType.Contains(addressType))
			{
				logs.RemoveAt(logIndex);
			}
		}
	}

	internal delegate void DataImportedToBusinessObjectAction(BusinessObject boImportedTo);
}
