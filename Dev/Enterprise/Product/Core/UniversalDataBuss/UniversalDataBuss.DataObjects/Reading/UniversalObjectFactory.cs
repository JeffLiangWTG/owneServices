using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

#if DEBUG
using Moq;
#endif

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public sealed class UniversalObjectFactory : IUniversalObjectFactory, IDisposable
	{
		public UniversalObjectFactory(BusinessObjectFactory innerFactory, bool delayLogSaveResult = false)
		{
			factory = innerFactory;
			objectsToSave = new List<IBusiness>();
			postSaveActions = new Queue<Action>();
			this.delayLogSaveResult = delayLogSaveResult;
			logSaveResults = new List<string>();
		}

		public UniversalObjectFactory() : this(new RowFactoryExposingBusinessObjectFactory())
		{
		}

		readonly BusinessObjectFactory factory;
		readonly List<IBusiness> objectsToSave;
		readonly Queue<Action> postSaveActions;
		readonly bool delayLogSaveResult;
		readonly List<string> logSaveResults;

		public class RowFactoryExposingBusinessObjectFactory : BusinessObjectFactory, IUniversalBusinessObjectFactory
		{
			public RowFactoryExposingBusinessObjectFactory()
				: base()
			{
				this.RefreshEnabled = false;
				this.SuspendValidation();
				this.NameForDebugging = "Universal Data Buss Import";
			}

			RowFactory IUniversalBusinessObjectFactory.RowFactory
			{
				get { return base._rowFactoryDoNotUseDirectly; }
			}

			public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
			{
				return new RowFactoryExposingBusinessObjectFactory();
			}

			protected override IChangedTableNames SaveInTransactionCore()
			{
				if (!saveAllowed)
				{
					throw new InvalidOperationException("This factory should only ever be saved via the Universal Object Factory, not directly.");
				}
				return base.SaveInTransactionCore();
			}

			IDisposable IUniversalBusinessObjectFactory.EnableSave()
			{
				return new DisposableAction(
					() => { saveAllowed = true; },
					() => { saveAllowed = false; }
					);
			}

			bool saveAllowed;
		}

		public RowFactory RowFactory
		{
			get { return factory is IUniversalBusinessObjectFactory universalBOFactory ? universalBOFactory.RowFactory : ((IBusinessObjectFactoryInternals)factory).RowFactory; }
		}

		public BusinessObjectFactory BOFactory
		{
			get { return factory; }
		}

		#region Exposed Methods from Wrapped BO Factory

		public T New<T>() where T : BusinessObject
		{
			return factory.New<T>();
		}

		public BusinessObject New(Type bizOType)
		{
			return factory.New(bizOType);
		}

		public T[] Load<T>(ZQuery query) where T : BusinessObject
		{
			return factory.Load<T>(query).Select(o => o).ToArray();
		}

		public T Load<T>(ZGuid pK) where T : BusinessObject
		{
			return factory.Load<T>(pK);
		}

		public BusinessObject Load(Type bizOType, ZGuid pK)
		{
			return factory.Load(bizOType, pK);
		}

		public BusinessObject LoadTop1(Type bizOType, ZQuery query)
		{
			return factory.LoadTop1(bizOType, query);
		}

		public T LoadTop1<T>(ZQuery query) where T : BusinessObject
		{
			return factory.LoadTop1<T>(query);
		}

		public T LoadFromUniqueKey<T>(SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue) where T : class
		{
			return factory.LoadFromUniqueKey<T>(uniqueKeyColumn, uniqueKeyValue);
		}

		public T LoadFromNaturalKey<T>(SchemaColumn column, ZString naturalKeyValue) where T : class
		{
			return factory.LoadFromNaturalKey<T>(column, naturalKeyValue);
		}

		public T GetCachedValue<T>(string key, GetValueDelegate<T> getValueDelegate)
		{
			return factory.GetCachedValue(key, getValueDelegate);
		}

		public T GetCachedValue<T>() where T : new()
		{
			return factory.GetCachedValue<T>();
		}

		#region Methods for Unit Testing Only
#if DEBUG

		public Mock<T> NewMoq<T>() where T : BusinessObject
		{
			return factory.NewMoq<T>();
		}

		public T NewWithValidTestData<T>() where T : BusinessObject
		{
			return factory.NewWithValidTestData<T>();
		}

		public BusinessObject NewWithValidTestData(Type type)
		{
			return factory.NewWithValidTestData(type);
		}

		public void SaveForTesting()
		{
			SaveSafe();
		}

#endif
		#endregion

		#endregion

		#region Logging and Saving

		public void RecordEndOfEveryRead(IBusiness objectToSave)
		{
			objectsToSave.Add(objectToSave);
		}

		public void AssertAllowedSaveTypes(IEnumerable<Type> allowableTypes)
		{
			var allowableTypeSet = new HashSet<Type>(allowableTypes);
			var internals = (IBusinessObjectFactoryInternals)factory;
			foreach (var bizo in internals.AllBusinessObjects)
			{
				if (bizo.IsSavedByFactory && bizo.HasChanges && !allowableTypeSet.Contains(bizo.GetType()))
				{
					ErrorReporter.ReportOnce("0b19ab21-d669-4d71-ac21-61283fb7cfd4", string.Format(CultureInfo.InvariantCulture, "Found unexpectedType for save:[{0}].", bizo.GetType().FullName));
				}
			}
		}

		public void SaveAtEndOfImport(IXmlImportLogger logger)
		{
			SaveSafe();

			GetSaveResultMessage();
			if (!delayLogSaveResult)
			{
				LogSaveResults(LogType.Information, Res.GetString("b9e99aea-e1d1-4ac9-b012-a1943ce75ca7", "Successfully saved"), logger);
			}
			objectsToSave.Clear();
		}

		public void AddPostSaveAction(Action action)
		{
			postSaveActions.Enqueue(action);
		}

		public void LogSaveResults(LogType type, string logPrefix, IXmlImportLogger logger)
		{
			foreach (var logSaveResult in logSaveResults)
			{
				logger.LogBoth(type, logPrefix + logSaveResult);
			}
			logSaveResults.Clear();
		}

		void GetSaveResultMessage()
		{
			var splitDistinctObjectsAndEvents = objectsToSave.Distinct().Split(x => !(x is IStmALog));
			var objectsToSaveDistincted = splitDistinctObjectsAndEvents.MatchingSet.ToList();
			var events = splitDistinctObjectsAndEvents.NonMatchingSet;

			if (objectsToSaveDistincted.Count == 0 && events.Any())
			{
				// No logging for events here
				return;
			}

			switch (objectsToSaveDistincted.Count)
			{
				case 0:
					logSaveResults.Add(Res.GetString("8f93ddf2-8226-412d-8615-97bffc7cf48f", ", but nothing was reported as being updated."));
					break;
				case 1:
					logSaveResults.Add(" " + Res.GetString("4d383bb9-3ecd-4a5b-abe0-17fef35512c4", "{0}.", objectsToSaveDistincted[0].HumanReadableName));
					break;
				default:
					var topLevelBO = objectsToSaveDistincted.Last();
					var childObjectSaveSummary = objectsToSaveDistincted
						.Where(o => o != topLevelBO)
						.GroupBy(o => o.GetType().Name)
						.Select(o => Res.GetString("32efae46-1a7c-487c-bb05-38572f60b6f8", "{0} x {1}", o.Count().ToString(), o.Key));

					logSaveResults.Add(" " + Res.GetString("150b5821-58c2-4cd0-9951-95141da323b7", "{0} with {1}.", topLevelBO.HumanReadableName, string.Join(", ", childObjectSaveSummary)));
					break;
			}
		}

		public bool ShouldLogDelayedSaveResults() => delayLogSaveResult;

		void SaveSafe()
		{
			using (factory is IUniversalBusinessObjectFactory universalBOFactory ? universalBOFactory.EnableSave() : null)
			{
				factory.Save();
			}

			ExecutePostSaveActions();
		}

		void ExecutePostSaveActions()
		{
			while (postSaveActions.Any())
			{
				postSaveActions.Dequeue().Invoke();
			}
		}

		#endregion

		public void FireCleanupAfterSaving()
		{
			if (CleanupAfterSaving != null)
			{
				CleanupAfterSaving(EventArgs.Empty, this);
			}
		}

		public void SetDataRefresh(bool dataRefresh) => factory.RefreshEnabled = dataRefresh;

		public void Dispose()
		{
			this.BOFactory.DeactivateActiveCollectionsAndCaches();
		}

		public event CleanupAfterSavingEventHandler CleanupAfterSaving;

		public delegate void CleanupAfterSavingEventHandler(EventArgs args, object source);
	}
}
