using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework.Business.Public.Interfaces;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

#if DEBUG
using Moq;
using NUnit.Framework;
#endif

namespace CargoWise.EntityFramework
{
	#region BusinessObjectFactory Internals

	public interface IBusinessObjectFactoryInternals
	{
		bool ReadOnly { get; set; }
		bool CanSave { get; set; }
		void Rollback();

		RowFactory RowFactory { get; }

		string ContentsAsXMLForDebugging
		{
			get;
		}

		int NumberOfBusinessObjects { get; }
		IReadOnlyList<BusinessObject> AllBusinessObjects { get; }
#if DEBUG
		bool DisableQueryCacheReset { get; set; }
#endif
		bool IsProcessingOnAllTransactionsCommitted { get; }

		bool LastSavingRollbackHadException { get; set; }

		bool IsProcessingOnFactorySavingBeforeTransaction { get; }

		bool IncludeWithOtherFactoriesForIssueReport { get; set; }
	}

	#endregion

	#region RowsLoadedEventArgs & RowsLoadedEventHandler

	public class RowsLoadedEventArgs : EventArgs
	{
		public RowsLoadedEventArgs(string tableOrViewName, DataRow[] rows, ZQuery query)
		{
			this.tableOrViewName = tableOrViewName;
			this.rows = rows;
			this.query = query;
		}

		public string TableOrViewName
		{
			get { return tableOrViewName; }
		}

		readonly string tableOrViewName;

		public DataRow[] Rows
		{
			get { return rows; }
		}

		readonly DataRow[] rows;

		public ZQuery Query
		{
			get { return query; }
		}

		readonly ZQuery query;
	}

	public delegate void RowsLoadedEventHandler(object sender, RowsLoadedEventArgs e);

	#endregion

#if DEBUG
	public class LoadedEventArgs : EventArgs
	{
		public LoadedEventArgs(IEnumerable<BusinessObject> newObjects)
		{
			var o = new List<BusinessObject>();
			o.AddRange(newObjects);
			NewObjects = o.ToArray();
		}

		public BusinessObject[] NewObjects
		{
			get;
			private set;
		}
	}
#endif

	#region NoConcreteTypeException

	[Serializable]
	public class NoConcreteTypeException : Exception
	{
		public NoConcreteTypeException(string message)
			: base(message)
		{
		}

		public NoConcreteTypeException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected NoConcreteTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion

	[DebuggerDisplay("Instance = {_Instance}, Name: {NameForDebugging}")]
	public partial class BusinessObjectFactory : ITransactionParticipant, INeedDataSet, IFactory, IFactoryProvider, IDbConnected, ICacheVersionProvider, IExternalFetchHintSupporter, ICacheProvider
	{
		public void EnqueueRefresh(Type businessObjectType, ZGuid pk, object[] clonedRow)
		{
			ThreadSentry.Post(RefreshObject, Tuple.Create(businessObjectType, pk, clonedRow), FormattableString.Invariant($"{nameof(BusinessObjectFactory)}.{nameof(EnqueueRefresh)}"));
		}

		void TryActionForRefresh(Action action)
		{
			try
			{
				action.Invoke();
			}
			catch (NullReferenceException ex) when (!ex.IsCriticalException())
			{
				// do nothing, sometimes the form could be closed in the middle of the execution of the refresh
			}
		}

		void RefreshObject(object sender)
		{
			var (type, pk, clonedRow) = (Tuple<Type, ZGuid, object[]>)sender;
			var bizO = Load(type, pk);

			if (bizO != null)
			{
				TryActionForRefresh(() => bizO.PerformRefresh(pk, clonedRow));
			}
		}

		public void EnqueueDelete(Type businessObjectType, ZGuid pk)
		{
			ThreadSentry.Post(DeleteObject, Tuple.Create(businessObjectType, pk), FormattableString.Invariant($"{nameof(BusinessObjectFactory)}.{nameof(EnqueueDelete)}"));
		}

		void DeleteObject(object sender)
		{
			var tuple = (Tuple<Type, ZGuid>)sender;
			var bizO = Load(tuple.Item1, tuple.Item2) as IBusiness;

			if (bizO != null)
			{
				TryActionForRefresh(() => bizO.DeleteForDataRefresh());
			}
		}

		/// <summary>
		/// Creates a new BusinessObjectFactory for creating, loading and saving BusinessObjects.
		/// Uses default Db.Connection and default Odyssey database.
		/// </summary>
		public BusinessObjectFactory()
			: this(enableCrossThreadErrorChecking: true, allowChangingThreadOwnership: true)
		{
		}

		/// <summary>
		/// Creates a new BusinessObjectFactory for creating, loading and saving BusinessObjects.		
		/// Uses default Db.Connection but all loads/saves are to another database (specified by DatabaseName) on 
		/// the same server as the default Odyssey database.
		/// </summary>
		public BusinessObjectFactory(string databaseName)
		{
			SetupSentry(true);
			_rowFactoryDoNotUseDirectly = new RowFactory(databaseName, this);
			Initialise();
		}

		public BusinessObjectFactory(string databaseName, ThreadSentry threadSentry)
		{
			ThreadSentry = threadSentry;
			SetupSentry(true);
			_rowFactoryDoNotUseDirectly = new RowFactory(databaseName, this);
			Initialise();
		}

#if DEBUG
		public BusinessObjectFactory(IFactoryProcessingExtension processingExtension)
		{
			SetupSentry(true);
			_rowFactoryDoNotUseDirectly = new RowFactory();
			Initialise(processingExtension);
		}

		readonly bool _generateRandomUniqueStringForProperties;

		public BusinessObjectFactory(bool generateRandomUniqueStringForProperties) : this()
		{
			_generateRandomUniqueStringForProperties = generateRandomUniqueStringForProperties;
		}

		internal void NotifyMarkedAsNeedingValidation(BusinessObject obj)
		{
			OnMarkedAsNeedingValidation(obj);
		}

		public delegate void MarkedAsNeedingValidationEventHandler(BusinessObject obj);
		public event MarkedAsNeedingValidationEventHandler MarkedAsNeedingValidation;

		void OnMarkedAsNeedingValidation(BusinessObject obj)
		{
			MarkedAsNeedingValidation?.Invoke(obj);
		}
#endif
		/// <summary>
		/// Creates a new BusinessObjectFactory for creating, loading and saving BusinessObjects.		
		/// Uses specified Connection, allowing you to load/save to a completely different
		/// database server on another machine.
		/// </summary>
		public BusinessObjectFactory(DbConnection connection)
		{
			SetupSentry(true);
			_rowFactoryDoNotUseDirectly = new RowFactory(connection, "", this);
			Initialise();
		}

		public BusinessObjectFactory(DbConnection connection, ThreadSentry threadSentry)
		{
			ThreadSentry = threadSentry;
			SetupSentry(true);
			_rowFactoryDoNotUseDirectly = new RowFactory(connection, "", this);
			Initialise();
		}

		internal protected BusinessObjectFactory(bool enableCrossThreadErrorChecking, bool allowChangingThreadOwnership)
		{
			try
			{
				SetupSentry(enableCrossThreadErrorChecking, allowChangingThreadOwnership);
				_rowFactoryDoNotUseDirectly = new RowFactory(this);
				Initialise();
			}
			catch (NullReferenceException ex)
			{
				var settings = ObjectFactory.Get<IEntityFrameworkSettings>();
				ErrorReporter.ReportOnce("BusinessObjectFactory_Constructor_NullReferenceException", $"IEntityFrameworkSettings is null? {settings is null}.", ex);
			}
		}

		public string NameForDebugging
		{
			get { return _rowFactoryDoNotUseDirectly.NameForDebugging; }
			set { _rowFactoryDoNotUseDirectly.NameForDebugging = value; }
		}

		public bool IndexingEnabled
		{
			get { return RowFactory.IndexingEnabled; }
			set { RowFactory.IndexingEnabled = value; }
		}

		public bool IsUberFactory => GetRowFactory(true).IsUberFactory;

		public ZDateTime TransactionStartedTime
		{
			get { return RowFactory.TransactionStartedTime; }
		}

		public ZDateTime TransactionStartedTimeUtc
		{
			get { return RowFactory.TransactionStartedTimeUtc; }
		}

		public BusinessObjectFactory Parent { get; internal set; }

		/// <summary>
		/// Returns a cached value based on the Type T.
		/// The same object will be returned if T has been previously presented.
		/// The GetValueDelegate will only be called if T has not been presented.
		/// The delegate will only be called once.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetCachedValue<T>()
			where T : new()
		{
			return GetCachedValue<T>(CacheStalenessPolicy.NeverStale);
		}

		public T GetCachedValue<T>(CacheStalenessPolicy stalePolicy)
			where T : new()
		{
			return GetCachedValue(GetDefaultCacheKey(), () => new T(), stalePolicy);
		}

		/// <summary>
		/// Returns a cached value based on the Type T and the key.
		/// The same object will be returned if T and a matching key has been previously presented.
		/// The GetValueDelegate will be called once for each T and key combination.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="getValueDelegate"></param>
		/// <returns></returns>
		public T GetCachedValue<T>(object key, GetValueDelegate<T> getValueDelegate)
		{
			return GetCachedValue(key, getValueDelegate, CacheStalenessPolicy.NeverStale);
		}

		public T GetCachedValue<T>(object key, GetValueDelegate<T> getValueDelegate, CacheStalenessPolicy stalePolicy)
		{
			return ValueCacheDomainService.Get(this).GetCachedValue(key, getValueDelegate, stalePolicy);
		}

		public bool TryGetValueFromCacheOnly<T>(object key, out T value)
		{
			return ValueCacheDomainService.Get(this).TryGetValueFromCacheOnly(key, out value);
		}

		public void ClearCachedValue<T>()
		{
			ClearCachedValue<T>(GetDefaultCacheKey());
		}

		public void ClearCachedValue<T>(object key)
		{
			ValueCacheDomainService.GetIfCreated(this)?.ClearCachedValue<T>(key);
		}

		static string GetDefaultCacheKey()
		{
			return string.Empty;
		}

		[SuppressThreadStaticFieldMessage]
		public static long _NextInstance = 1;
		public long _Instance;

		IThreadSentry SetupSentry(bool shouldEnableCrossThreadErrorReporting, bool allowChangingThreadOwnership = true)
		{
			AllowChangingThreadOwnership = allowChangingThreadOwnership;
			CrossThreadErrorReportingEnabled = shouldEnableCrossThreadErrorReporting && ObjectFactory.Get<IEntityFrameworkSettings>().ReportCrossThreadFactoryAccess;
			if (ThreadSentry != null)
			{
				return ThreadSentry;
			}
			return ThreadSentry = ThreadSentryProvider.GetThreadSentry(CrossThreadErrorReportingEnabled, new FactoryThreadSentryHelper(this), allowChangingThreadOwnership);
		}

		void Initialise(IFactoryProcessingExtension processingExtension = null)
		{
#if DEBUG
			SetupTraceData();
#endif
			_Instance = _NextInstance++;
			refreshManager = new DataRefreshManager();
			BusinessObjectCache = new BusinessObjectCache();
			WrapperManager = new BusinessObjectWrapperManager();
			businessObjectFetchHintManager = new BusinessObjectFetchHintManager(this);
			InstantiationTime = DateTime.Now;
			bool recordAllocationPath = true;
#if DEBUG
			recordAllocationPath = !TestingState.IsRunningTests;
#endif
			if (recordAllocationPath)
			{
				allocationPath = new StackTrace(1);
			}

			extension = new Lazy<IFactoryProcessingExtension>(() => processingExtension ?? ObjectFactory.Get<IFactoryProcessingExtension>());
			Saved += BusinessObjectsSavedHandler;
			PersistentFactoryCacheManager.Instance.Add(this); // Subscribe to multi-threaded manager last.
		}

		Lazy<IFactoryProcessingExtension> extension;

		public IThreadSentry ThreadSentry { get; internal set; }

		public void RelinquishThreadOwnership()
		{
			ThreadSentry.RelinquishThreadOwnership();
		}

		public void TakeThreadOwnership()
		{
			ThreadSentry.TakeThreadOwnership();
		}

		internal void NotifyDeleted(BusinessObject bizObj)
		{
			BusinessObjectCache.Remove(bizObj);
			if (businessObjectNKCache != null)
			{
				businessObjectNKCache.Remove(bizObj);
			}
			WrapperManager.DeleteFor(bizObj);
		}

		public void UpdateNaturalKeyCache(BusinessObject bizo, SchemaColumn column, IZType oldValue, IZType newValue)
		{
			if (businessObjectNKCache != null)
			{
				businessObjectNKCache.UpdateNaturalKeyCache(bizo, column, oldValue, newValue);
			}
		}

		public virtual BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			ThreadSentry.EnsureCurrentThreadIsOwner();
			var type = GetType();

			if (!ThreadSentry.IsOwner)
			{
				if (usingMyThreadSentry && type == typeof(BusinessObjectFactory))
				{
					return (BusinessObjectFactory)Activator.CreateInstance(type, Db.Connection, ThreadSentry);
				}
				else
				{
					return (BusinessObjectFactory)Activator.CreateInstance(type, Db.Connection);
				}
			}
			else
			{
				var rowFactory = GetRowFactory();
				if (usingMyThreadSentry && type == typeof(BusinessObjectFactory))
				{
					return
							rowFactory.DbConnection == null
									? (BusinessObjectFactory)Activator.CreateInstance(type, rowFactory.DatabaseName, ThreadSentry)
									: (BusinessObjectFactory)Activator.CreateInstance(type, rowFactory.DbConnection, ThreadSentry);
				}
				else
				{
					return
							rowFactory.DbConnection == null
									? (BusinessObjectFactory)Activator.CreateInstance(type, rowFactory.DatabaseName)
									: (BusinessObjectFactory)Activator.CreateInstance(type, rowFactory.DbConnection);
				}
			}
		}

		#region ChangeNumber

		uint fLastChangeNumber;
		internal uint GetNextChangeNumber()
		{
			fLastChangeNumber++;
			return fLastChangeNumber;
		}

		public uint LastChangeNumber
		{
			get { return fLastChangeNumber; }
		}

		#endregion

		#region EnsureConnectionIsOpen

		public void EnsureConnectionIsOpen()
		{
			RowFactory.EnsureConnectionIsOpen();
		}

		#endregion

		#region PreFetching

		public void SeedQueryCache(string tableName, ZQuery query)
		{
			RowFactory.SeedQueryCache(tableName, query);
		}

		public bool LoadFetchingEnabled = true;

		void FetchForType(Type typeOfPersistentBusinessObject)
		{
			if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(typeOfPersistentBusinessObject))
			{
				businessObjectFetchHintManager.FetchTable(GetViewNameFromType(typeOfPersistentBusinessObject));
			}
		}

		public void ExecuteAllFetchHints()
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(this))
			{
				RowFactory.ExecuteAllFetchHints();
			}
		}

		public void AddFetchHint(ITableSchema tableSchema, ZQuery query)
		{
			AddFetchHint(new ZQueryFetchHint(tableSchema, query));
		}

		public void AddFetchHint(Type businessObjectType, ZQuery query)
		{
			if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(businessObjectType))
			{
				AddFetchHint(new ImmediateZQueryFetchHint(businessObjectType, query));
			}
#if DEBUG
			else
			{
				throw new InvalidOperationException("Attempted to add a fetch hint for non-persistent business object.");
			}
#endif
		}

		/// <summary>
		/// Use this fetch hint for concatenating multiple queries such as
		/// OM_GC = GlbCompany.CurrentCompany.PK and blah
		/// The company part goes in MainQuery and blah goes in the second query
		/// The system will generate optimised sql passing MainQuery only once to the db
		/// </summary>
		/// <param name="tableSchema"></param>
		/// <param name="mainQuery"></param>
		/// <param name="secondQuery"></param>
		public void AddFetchHint(ITableSchema tableSchema, ZQuery mainQuery, ZQuery secondQuery)
		{
			AddFetchHint(new ZMultiQueryFetchHint(tableSchema, mainQuery, secondQuery));
		}

		/// <summary>
		/// Use this fetch hint for concatenating multiple queries such as
		/// CusAddInfoSchema.B7_ParentID = JobComInvoiceLine.PK and CusAddInfoSchema.B7_Type = 'AII'
		/// The company part goes in MainQuery and blah goes in the second query
		/// The system will generate optimised sql passing MainQuery only once to the db
		/// </summary>
		/// <param name="businessObjectType"></param>
		/// <param name="mainQuery"></param>
		/// <param name="secondQuery"></param>
		public void AddFetchHint(Type businessObjectType, ZQuery mainQuery, ZQuery secondQuery)
		{
			if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(businessObjectType))
			{
				AddFetchHint(new ImmediateZMultiQueryFetchHint(businessObjectType, mainQuery, secondQuery));
			}
#if DEBUG
			else
			{
				throw new InvalidOperationException("Attempted to add a fetch hint for non-persistent business object.");
			}
#endif
		}

		public
#if DEBUG
 virtual
#endif
 void AddFetchHint(Type businessObjectType, SchemaColumn column, IZType value)
		{
			if (value.IsValid && !value.IsEmpty
				&& !RowFactory.QueryCache.IsCached(column.TableName, new ZQuery(column, value)))
			{
				if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(businessObjectType))
				{
					AddFetchHint(new ImmediateFetchHint(businessObjectType, column, value));
				}
#if DEBUG
				else
				{
					throw new InvalidOperationException("Attempted to add a fetch hint for non-persistent business object.");
				}
#endif
			}
		}

		const string StmALog_PK = "SL_PK";

		public
#if DEBUG
 virtual
#endif
		void AddFetchHint(SchemaColumn column, IZType value)
		{
			if (value.IsValid && !value.IsEmpty && column.Name != StmALog_PK)
			{
				if (!RowFactory.QueryCache.IsCached(column.TableName, new ZQuery(column, value)))
				{
					AddFetchHint(new FetchHint(column, value));
				}
			}
		}

		public void AddFetchHint(string tableName, ZGuid pK)
		{
			AddFetchHint(ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName), pK);
		}

		public void AddFetchHint(Type businessObjectType, ZGuid pK)
		{
			if (pK.IsValid)
			{
				if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(businessObjectType))
				{
					SchemaColumn pkColumn = BusinessObjectFactory.GetPKColumnFromType(businessObjectType);

					if (pkColumn.Name != StmALog_PK)
					{
						AddFetchHint(new ImmediateFetchHint(businessObjectType, pkColumn, pK));
					}
				}
#if DEBUG
				else
				{
					throw new InvalidOperationException("Attempted to add a fetch hint for non-persistent business object.");
				}
#endif
			}
		}

		public void AddFetchHint(IFetchHint fetchHint)
		{
			if (!fetchHint.GetQuery().IsNoResultQuery)
			{
				if (fetchHint is IImmediateHint)
				{
					businessObjectFetchHintManager.AddFetchHint((IImmediateHint)fetchHint);
				}
				RowFactory.AddFetchHint(fetchHint);
			}
		}

		internal BusinessObjectFetchHintManager businessObjectFetchHintManager;

		#endregion

		#region AllowMultipleBusinessObjectsAroundOneRow

		/// <summary>
		/// Allow two or more BusinessObjects in the same inheritance hierarchy (eg, Shipment, CFSShipment)
		/// to be loaded, wrapping the same row.
		/// When AllowMultipleBusinessObjectsAroundOneRow is false, and code tries to load the same object as 
		/// a different type, a developer warning will be reported.
		/// </summary>
		public bool AllowMultipleBusinessObjectsAroundOneRow
		{
			get { return allowMultipleBusinessObjectsAroundOneRow; }
			set
			{
				if (hasLoadedMultipleBusinessObjectsAroundOneRow)
				{
					ErrorReporter.ReportOnce("AllowMultipleBusinessObjectsAroundOneRowSet", "AllowMultipleBusinessObjectsAroundOneRow cannot be set to False as there are already multiple business objects around one row.");
				}
				else
				{
					allowMultipleBusinessObjectsAroundOneRow = value;
				}
			}
		}

		bool allowMultipleBusinessObjectsAroundOneRow = true;
		bool hasLoadedMultipleBusinessObjectsAroundOneRow;

		#endregion

		#region DataRefresh Manager

		/// <summary>
		/// Set to false to disallow in-memory data synchronisation of BusinessObjects across Factories.
		/// This property is true by default.
		/// </summary>
		public bool RefreshEnabled
		{
			get { return refreshManager.Enabled; }
			set { refreshManager.Enabled = value; }
		}

		/// <summary>
		/// Force a business object to be published out on the data refresh bus.
		/// </summary>
		/// <param name="bizObjectToPublishOut"></param>
		public void ForcePublishForDataRefresh(BusinessObject bizObjectToPublishOut)
		{
			if (bizObjectToPublishOut != null)
			{
				RefreshManager.ForcePublish(new[] { bizObjectToPublishOut });
			}
		}

		/// <summary>
		/// Force a business object to be published out on the data refresh bus as a new object that will
		/// be added to any subscribing collections.
		/// </summary>
		/// <param name="bizObjectToPublishOut"></param>
		public void ForcePublishForDataRefreshByTableName(BusinessObject bizObjectToPublishOut)
		{
			if (bizObjectToPublishOut != null)
			{
				DataRefreshManager.Bus.PublishByTableName(new[] { bizObjectToPublishOut });
			}
		}

		/// <summary>
		/// When a BusinessObjectCollection is managed, if a new BusinessObject is saved to the same table
		/// in the database as the Collection, then an exact copy of this BusinessObject will be added to this collection
		/// </summary>
		internal void StartManagingCollectionForAddingBusinessObjects(BusinessObjectCollection collectionToManage)
		{
			refreshManager.StartManaging(collectionToManage);
		}

		/// <summary>
		/// Once a collection is no longer managed, when a new BusinessObject is saved to the same table
		/// in the database as the collection, this collection will not be updated.
		/// </summary>
		internal void StopManagingCollectionForAddingBusinessObjects(BusinessObjectCollection managedCollection)
		{
			refreshManager.StopManaging(managedCollection);
		}

		internal DataRefreshManager RefreshManager
		{
			get { return refreshManager; }
		}

		#endregion

		#region Creating New BizOs

#if DEBUG
		public Mock<T> NewMoq<T>() where T : BusinessObject
		{
			var bizOType = TypeDecider.GetTypeForBinding(typeof(T));
			Mock<T> mock = null;
			New(bizOType, Guid.Empty, objects =>
			{
				mock = new Mock<T>(MockBehavior.Default, objects) { CallBase = true };
				return mock.Object;
			}, null);
			return mock;
		}

#endif

		public T New<T>() where T : class
		{
			Assembly callingAssembly =
#if DEBUG
 Assembly.GetCallingAssembly();
#else
 null;
#endif
			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T>(New(bizoType));
		}

		public T New<T>(ITypeDeciderContext typeDeciderContext) where T : class
		{
			Assembly callingAssembly =
#if DEBUG
 Assembly.GetCallingAssembly();
#else
 null;
#endif
			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T>(New(bizoType, Guid.Empty, null, typeDeciderContext));
		}

		T Cast<T>(object value) where T : class
		{
			T result = value as T;
			if (result == null && value != null)
			{
				throw new InvalidCastException("Could not cast " + value.GetType().FullName + " to " + typeof(T).FullName + ".");
			}
			return result;
		}

		public T NewWithPrimaryKey<T>(Guid initialisingPk) where T : BusinessObject
		{
			return Cast<T>(New(typeof(T), initialisingPk));
		}

		/// <summary>
		/// Creates a new BusinessObject of the type specified.
		/// </summary>
		/// 
		public virtual BusinessObject New(Type bizOType)
		{
			return New(bizOType, Guid.Empty);
		}

		public virtual BusinessObject New(Type bizOType, Guid initialisingPk)
		{
			return New(bizOType, initialisingPk, null, null);
		}

		BusinessObject New(Type bizOType, Guid initialisingPk, Func<object[], BusinessObject> createInstanceFunc, ITypeDeciderContext typeDeciderContext)
		{
#if DEBUG
			LogAction("Creating new BizO of type: " + bizOType.FullName, this);
#endif

			DataRow row = RowFactory.New(GetViewNameFromType(bizOType));

			if (initialisingPk != Guid.Empty)
			{
				row[GetPKColumnFromType(bizOType).Name] = initialisingPk;
			}

			var result = CreateBusinessObject(row, bizOType, true, createInstanceFunc, typeDeciderContext).BusinessObject;
			row.Table.Rows.Add(row);
			result.IsDataRowInDataTable = true;

			return result;
		}

#if DEBUG

		public T NewWithValidTestData<T>() where T : BusinessObject
		{
			return Cast<T>(NewWithValidTestData(typeof(T)));
		}

		public T NewWithValidTestData<T>(TestBusinessObjectKind kind) where T : BusinessObject
		{
			return Cast<T>(NewWithValidTestData(typeof(T), kind));
		}

		public BusinessObject NewWithValidTestData(Type bizOType)
		{
			return NewWithValidTestData(bizOType, TestBusinessObjectKind.MinimumRequiredToSave);
		}

		public BusinessObject NewWithValidTestData(Type bizOType, TestBusinessObjectKind kind)
		{
			return new BusinessObjectTestDataHelper(_generateRandomUniqueStringForProperties).NewWithValidTestData(this, bizOType, kind);
		}
#endif

		#endregion

		#region Loading BizOs

		#region Load By PK

		public T Load<T>(ZGuid pK) where T : class
		{
			Assembly callingAssembly =
#if DEBUG
 Assembly.GetCallingAssembly();
#else
 null;
#endif
			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T>(Load(bizoType, pK));
		}

		/// <summary>
		/// Loads a BusinessObject with a given TablePrefix and PK. Returns null if the PK was not found.
		/// *** YOUR TABLEPREFIX MUST EXIST IN BusinessObjectPrefixTypesConfiguration.xml. ***
		/// </summary>
		public BusinessObject Load(string tablePrefix, ZGuid pk)
		{
			BusinessObject result = null;
			Type bizOType = GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix);
			if (bizOType != null)
			{
				result = Load(bizOType, pk);
			}
			return result;
		}

		/// <summary>
		/// Loads a BusinessObject with a given TablePrefix and PK. Returns null if the PK was not found.
		/// *** YOUR TABLEPREFIX MUST EXIST IN BusinessObjectPrefixTypesConfiguration.xml. ***
		/// </summary>
		public T Load<T>(string tablePrefix, ZGuid pk) where T : class
		{
			var result = Load(tablePrefix, pk);
			return Cast<T>(result);
		}

		/// <summary>
		/// Loads a BusinessObject with a given PK.
		/// Returns null if PK was not found.
		/// Returns null and sends an error report if PK is valid and bizOType is null.
		/// </summary>
		public virtual BusinessObject Load(Type bizOType, ZGuid pK)
		{
			return Load(bizOType, pK, null);
		}

		BusinessObject Load(Type bizOType, ZGuid pK, Func<object[], BusinessObject> createInstanceFunc)
		{
			BusinessObject result = null;
			if (pK.IsValid)
			{
				if (bizOType == null)
				{
					ErrorReporter.ReportOnce("Factory.Load.TypeIsNull", "Factory.Load was called with null bizOType");
					return null;
				}

				using (NotifyIsLoading())
				using (PerformanceStatisticsCollector.StartMonitoring((NoResString)"BusinessObjectFactory.Load(Type, ZGuid)", bizOType))
				{
#if DEBUG
					if (BusinessObjectFactory.ExcludedGuidForLogging != pK)
					{
						LogAction("LOAD " + bizOType.FullName + " PK = " + pK, this);
					}
#endif
					result = LoadBusinessObjectFromMRU(bizOType, pK);
					if (result == null)
					{
						result = LoadWithoutMRUCache(bizOType, pK, createInstanceFunc);
						UpdateMRUCache(result);
					}
				}
			}
			return result;
		}

		internal BusinessObject LoadFromDatabase(Type bizOType, ZGuid pk)
		{
			BusinessObject result = null;
			DataRow row = RowFactory.LoadFromPK(GetViewNameFromType(bizOType), pk, true);

			if (row != null)
			{
				using (NotifyIsLoading())
				{
					result = CreateBusinessObject(row, bizOType);
				}
			}
			UpdateMRUCache(result);

			return result;
		}

		BusinessObject LoadBusinessObjectFromMRU(Type bizOType, ZGuid pk)
		{
			// Use explicit loop for to prevent changing collection during iteration.
			// Additionally check MaximumFetchHintsToIssuePerTableSelect to do not go for (MaximumFetchHintsToIssuePerTableSelect+1)'th element not yet removed by other thread.
			for (int i = 0; i < last10BizObjsLoaded?.Count && i < MaximumFetchHintsToIssuePerTableSelect; i++)
			{
				BusinessObject businessObject = last10BizObjsLoaded[i];

				if (businessObject != null && businessObject.PK == pk &&
					((businessObject.GetType() == bizOType) || (IDontMindLoadingASubclassInsteadAttribute.HasAttribute(bizOType) && businessObject.GetType().IsSubclassOf(bizOType)))
					&& !businessObject.IsDeleted)
				{
					return businessObject;
				}
			}
			return null;
		}

		BusinessObject LoadWithoutMRUCache(Type bizOType, ZGuid pk, Func<object[], BusinessObject> createInstanceFunc)
		{
			var result = BusinessObjectCache.Fetch(bizOType, pk);

			if (object.ReferenceEquals(result, null))
			{
				if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(bizOType))
				{
					var tableOrViewName = GetViewNameFromType(bizOType);
					var row = LoadFromPKCore(tableOrViewName, pk);

					if (row != null)
					{
						GenerateRowFetchHints(bizOType, new[] { row });
						result = CreateBusinessObject(row, bizOType, true, createInstanceFunc).BusinessObject;
					}
					if (result == null)
					{
						result = BusinessObjectCache.Fetch(TypeDecider.GetTypeForBinding(bizOType), pk);
					}
				}
				else
				{
					var strategies = ((Hashtable)ObjectFactory.Get("BusinessObjectLoadStrategyList"))
						.Cast<DictionaryEntry>()
						.ToDictionary((kvp) => (string)kvp.Key, (kvp) => (ObjectHandle)kvp.Value);

					IBusinessObjectLoadStrategy strategy = null;

					if (strategies.TryGetValue(bizOType.FullName, out ObjectHandle handle))
					{
						strategy = (IBusinessObjectLoadStrategy)handle.GetObject();
					}

					if (strategy != null)
					{
						result = strategy.Load(this, pk);
					}
				}
			}

			RowFactory.shouldAddDiagnosisForFactoryQueryCache = false;
			FetchForType(bizOType);

			return GetNullObjectIfNull(result, bizOType);
		}

		protected virtual DataRow LoadFromPKCore(string tableOrViewName, ZGuid pk)
		{
			return RowFactory.LoadFromPK(tableOrViewName, pk);
		}

		public IDisposable AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany()
		{
			return RowFactory.AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany();
		}

		#endregion

		#region Load By Natural Key

		public virtual BusinessObject LoadFromUniqueKey(Type bizOType, SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
		{
			using (NotifyIsLoading())
			using (PerformanceStatisticsCollector.StartMonitoring((NoResString)"BusinessObjectFactory.LoadFromUniqueKey(" + bizOType.Name + (NoResString)", " + uniqueKeyColumn.Name + (NoResString)", IZType)", ""))
			{
				BusinessObject result = null;
				if (uniqueKeyValue.IsValid && !uniqueKeyValue.IsEmpty)
				{
#if DEBUG
					LogAction("LOAD FromUniqueKey " + bizOType.FullName + " KeyField = " + uniqueKeyColumn.Name + " Value = " + uniqueKeyValue, this);
#endif
					result = LoadBusinessObjectFromMRU(bizOType, uniqueKeyColumn, uniqueKeyValue);
					if (result == null)
					{
						result = LoadWithoutMRUCache(bizOType, uniqueKeyColumn, uniqueKeyValue);
						UpdateMRUCache(result);
					}
				}

				return result;
			}
		}

		BusinessObject LoadBusinessObjectFromMRU(Type bizOType, SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
		{
			// Use explicit loop for to prevent changing collection during iteration.
			// Additionally check MaximumFetchHintsToIssuePerTableSelect to do not go for (MaximumFetchHintsToIssuePerTableSelect+1)'th element not yet removed by other thread.
			for (int i = 0; i < last10BizObjsLoaded.Count && i < MaximumFetchHintsToIssuePerTableSelect; i++)
			{
				BusinessObject businessObject = last10BizObjsLoaded[i];

				if (businessObject != null && businessObject.GetType() == bizOType && !businessObject.IsDeleted && uniqueKeyValue.Equals(businessObject[uniqueKeyColumn.Name]))
				{
					return businessObject;
				}
			}
			return null;
		}

		BusinessObject LoadWithoutMRUCache(Type bizOType, SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
		{
			BusinessObject result = null;
			result = BusinessObjectNKCache.Fetch(bizOType, uniqueKeyColumn, uniqueKeyValue);

			if (object.ReferenceEquals(result, null))
			{
				DataRow row = RowFactory.LoadFromNaturalKey(GetViewNameFromType(bizOType), uniqueKeyColumn, uniqueKeyValue, false);

				if (row != null)
				{
					result = CreateBusinessObject(row, bizOType);
					BusinessObjectNKCache.Add(result, uniqueKeyColumn, uniqueKeyValue);
				}
			}

			FetchForType(bizOType);
			return GetNullObjectIfNull(result, bizOType);
		}

		public T LoadFromNaturalKey<T>(SchemaColumn column, ZString naturalKeyValue) where T : class
		{
			Assembly callingAssembly =
#if DEBUG
 Assembly.GetCallingAssembly();
#else
 null;
#endif
			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T>(LoadFromNaturalKey(bizoType, column, naturalKeyValue));
		}

		public BusinessObject LoadFromNaturalKey(Type bizOType, SchemaColumn column, ZString naturalKeyValue)
		{
			return LoadFromUniqueKey(bizOType, column, naturalKeyValue);
		}

		public T LoadFromUniqueKey<T>(SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue) where T : class
		{
			Assembly callingAssembly =
#if DEBUG
 Assembly.GetCallingAssembly();
#else
 null;
#endif
			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T>(LoadFromUniqueKey(bizoType, uniqueKeyColumn, uniqueKeyValue));
		}

		#endregion

		#region Load By Filter

		public T[] Load<T>(ZQuery query) where T : class
		{
			Assembly callingAssembly = null;
#if DEBUG
			callingAssembly = Assembly.GetCallingAssembly();
#endif

			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T[]>(Load(bizoType, query));
		}

		/// <summary>
		/// Create an array of BusinessObjects that match the ZQuery provided.
		/// May return an array of 0 items.
		/// </summary>
		public virtual BusinessObject[] Load(Type bizOType, ZQuery pK)
		{
			using (NotifyIsLoading())
			using (PerformanceStatisticsCollector.StartMonitoring((NoResString)"BusinessObjectFactory.Load(" + bizOType.Name + (NoResString)", ZQuery)"))
			{
#if DEBUG
				LogAction("LOAD " + bizOType.FullName + " Filter = " + pK.LiteralTextADO, this);
#endif

				BusinessObject[] result;
				if (pK.IsNoResultQuery)
				{
					result = (BusinessObject[])Array.CreateInstance(bizOType, 0);
				}
				else
				{
					ZQuery effectiveFilter = GetFilterIncludingActiveFilter(bizOType, pK);
					var tableOrViewName = GetViewNameFromType(bizOType);

					result = LoadCore(tableOrViewName, bizOType, effectiveFilter);
				}

				if (pK.ReLoadExistingRows)
				{
					ActiveBusinessObjectCollection.RefreshAll(bizOType, this);
				}

				return result;
			}
		}

		protected virtual BusinessObject[] LoadCore(string tableOrViewName, Type bizOType, ZQuery effectiveFilter)
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(this))
			{
				try
				{
					var rows = RowFactory.Load(tableOrViewName, effectiveFilter);
					OnRowsLoaded(tableOrViewName, rows, effectiveFilter);
					GenerateRowFetchHints(bizOType, rows);
					return CreateBusinessObjectsFromRows(bizOType, rows);
				}
				catch (ApplicationException ex)
				{
					var message = string.Format((NoResString)"Error has occur while loading information. Business object name: {1}{0}Effective Filter:{2}"
						, Environment.NewLine     // 0
						, bizOType.FullName              // 1
						, effectiveFilter.LiteralTextADO // 2
						);

					throw new ApplicationException(message, ex);
				}
			}
		}

		public T LoadTop1<T>(ZQuery query) where T : class
		{
			Assembly callingAssembly =
#if DEBUG
 Assembly.GetCallingAssembly();
#else
 null;
#endif
			Type bizoType = GetConcreteBusinessObjectType(callingAssembly, typeof(T));
			return Cast<T>(LoadTop1(bizoType, query));
		}

		/// <summary>
		/// Loads one BusinessObject that meets the ZQuery provided.
		/// Provide an OrderBy clause to control which is returned.
		/// Returns null if none found
		/// </summary>
		public BusinessObject LoadTop1(Type bizOType, ZQuery sQLFilter)
		{
			using (NotifyIsLoading())
			using (PerformanceStatisticsCollector.StartMonitoring((NoResString)"BusinessObjectFactory.LoadTop1(" + bizOType.Name + (NoResString)", ZQuery)", ""))
			{
#if DEBUG
				LogAction("LOAD Top1 " + bizOType.FullName + " Filter = " + sQLFilter.LiteralTextADO, this);
#endif
				BusinessObject result = null;
				if (!sQLFilter.IsNoResultQuery)
				{                         // GetFilterIncludingActiveFilter is called later, causing duplicate parameters to be added.
					ZQuery effectiveFilter = new ZQuery(sQLFilter)
					{
						MaximumRows = 1
					}; // Done to set ModificationsEnabled to true
					BusinessObject[] bizObjects = Load(bizOType, effectiveFilter);
					if (bizObjects.Length > 0)
					{
						result = bizObjects[0];
					}
				}

				return GetNullObjectIfNull(result, bizOType);
			}
		}

		#endregion

		#region Load Blob Fields

		public void LoadBlobField(BusinessObject businessObject, SchemaColumn schemaColumn)
		{
			using (NotifyIsLoading())
			{
				if (businessObject.IsInDatabase)
				{
					if (businessObject.Row == null)
					{
						throw new NotSupportedException("You cannot load a blob field on BusinessObject" + businessObject.GetType().FullName + " as it has a null row property.");
					}

					RowFactory.LoadBlobField(businessObject.Row, schemaColumn);
				}
			}
		}

		public Stream GetBinaryFieldStream(BusinessObject businessObject, SchemaColumn schemaColumn)
		{
			return RowFactory.GetBinaryFieldStream(businessObject.Row, schemaColumn.TableName, schemaColumn.Name);
		}

		public Stream GetBinaryFieldStreamRawFromDb(BusinessObject businessObject, SchemaColumn schemaColumn)
		{
			return RowFactory.GetBinaryFieldStreamRawFromDb(businessObject.Row, schemaColumn.TableName, schemaColumn.Name);
		}

		public TextReader GetTextFieldReader(BusinessObject businessObject, SchemaColumn schemaColumn, bool closeReaderBetweenReads)
		{
			return RowFactory.GetTextFieldReader(businessObject.Row, schemaColumn.TableName, schemaColumn.Name, closeReaderBetweenReads);
		}

		public Stream GetBinaryFieldStream(BusinessObject businessObject, string tableName, string columnName)
		{
			return RowFactory.GetBinaryFieldStream(businessObject.Row, tableName, columnName);
		}

		public TextReader GetTextFieldReader(BusinessObject businessObject, string tableName, string columnName, bool closeReaderBetweenReads)
		{
			return RowFactory.GetTextFieldReader(businessObject.Row, tableName, columnName, closeReaderBetweenReads);
		}

		#endregion

		#region Load Mock
#if DEBUG
		public Mock<T> LoadMoq<T>(ZGuid pk) where T : BusinessObject
		{
			var bizOType = TypeDecider.GetTypeForBinding(typeof(T));

			if (LoadBusinessObjectFromMRU(bizOType, pk) != null || BusinessObjectCache.Fetch(bizOType, pk) != null)
			{
				throw new InvalidOperationException($"The requested BusinessObject ({bizOType.FullName}) already exists in the factory and cannot be reloaded as a Mock.");
			}

			Mock<T> mock = null;
			Load(bizOType, pk, objects =>
			{
				mock = new Mock<T>(MockBehavior.Default, objects) { CallBase = true };
				return mock.Object;
			});
			return mock;
		}
#endif
		#endregion

		#region Can Load

		public virtual bool CanLoadFromTable(string tableOrViewName)
		{
			return true;
		}

		#endregion

		#region Null Object Support

		public bool IsConstructingNullBusinessObject
		{
			get
			{
				return isConstructingNullBusinessObjectCounter > 0;
			}
			private set
			{
				if (value)
				{
					isConstructingNullBusinessObjectCounter++;
				}
				else
				{
					if (isConstructingNullBusinessObjectCounter > 0)
					{
						isConstructingNullBusinessObjectCounter--;
					}
				}
			}
		}

		int isConstructingNullBusinessObjectCounter;

		public BusinessObject GetNull(Type bizOType)
		{
			if (!NullObjects.TryGetValue(bizOType, out BusinessObject nullObject))
			{
				NullObjectFactory.NameForDebugging = "Null factory for " + bizOType.Name;
				NullObjectFactory.IsConstructingNullBusinessObject = true;
				try
				{
					nullObject = NullObjectFactory.New(bizOType);
					nullObject.IsNull = true;
				}
				finally
				{
					NullObjectFactory.IsConstructingNullBusinessObject = false;
				}
				NullObjects[bizOType] = nullObject;
			}
			return nullObject;
		}

		BusinessObjectFactory NullObjectFactory
		{
			get
			{
				if (nullObjectFactory == null)
				{
					nullObjectFactory = CreateNewFactory(usingMyThreadSentry: true);

					// Validation can cause stack overflow when new NullObject tries again to get NullObject of same type from a validation called indirectly from its constructor
					// (and previous instance of this NullObject is not yet stored to NullObjects cache)
					nullObjectFactory.SuspendValidation();
					nullObjectFactory.RefreshEnabled = false;
				}
				return nullObjectFactory;
			}
		}
		BusinessObjectFactory nullObjectFactory;

		public T GetNull<T>() where T : BusinessObject
		{
			return Cast<T>(GetNull(typeof(T)));
		}

		protected Dictionary<Type, BusinessObject> NullObjects
		{
			get
			{
				if (nullObjects == null)
				{
					nullObjects = new Dictionary<Type, BusinessObject>();
				}
				return nullObjects;
			}
		}

		Dictionary<Type, BusinessObject> nullObjects;

		protected BusinessObject GetNullObjectIfNull(BusinessObject bizO, Type businessObjectType)
		{
			return bizO;//(object)BizO == null ? GetNull(BizOType) : BizO;
		}

		#endregion

#if DEBUG
		protected
#endif
		void UpdateMRUCache(BusinessObject result)
		{
			if (result != null)
			{
				last10BizObjsLoaded.Insert(0, result);

				if (last10BizObjsLoaded.Count > MRUCacheMaximumCount)
				{
					try
					{
						last10BizObjsLoaded.RemoveAt(MRUCacheMaximumCount);
					}
					catch (ArgumentOutOfRangeException)
					{
						// if this exception is caught here, it means that due to concurrency issue, the list doesn't contain any element more than the max count to be removed.
						// so simply ignore this exception.
					}
				}
			}
		}

		const int MRUCacheMaximumCount = 10;

		const int MaximumFetchHintsToIssuePerTableSelect = 100;

		public bool IsLoading
		{
			get { return isLoading > 0; }
		}

		IDisposable NotifyIsLoading()
		{
			isLoading++;
			return new DisposableAction(delegate
			{
				isLoading--;
			});
		}
		int isLoading;

		public static Type GetBusinessObjectBaseTypeFromTablePrefix(string tablePrefix, bool reportUnknownPrefix = true)
		{
			Type bizOType = null;
			if (!string.IsNullOrEmpty(tablePrefix))
			{
				Hashtable types = (Hashtable)ObjectFactory.Get("EnterpriseBusinessObjectPrefixTypes");
				ObjectHandle objectHandle = (ObjectHandle)types[tablePrefix];
				if (objectHandle != null)
				{
					bizOType = objectHandle.GetObjectType();
				}
				else
				{
					var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(tablePrefix);
					if (tableSchema != null)
					{
						bizOType = DataBoundResourceStrings.GetTypeForTable(tableSchema.TableName);
						if (bizOType != null && bizOType.IsAbstract)
						{
							var abstractBizOType = bizOType;
							var concreteTypeName = bizOType.Namespace + "." + bizOType.Name.Substring(4);
							bizOType = abstractBizOType.Assembly.GetType(concreteTypeName);

							if (bizOType == null && concreteTypeName[concreteTypeName.Length - 1] == 's')
							{
								bizOType = abstractBizOType.Assembly.GetType(concreteTypeName.Substring(0, concreteTypeName.Length - 1));
							}

							if (bizOType != null && bizOType.IsAbstract)
							{
								bizOType = null;
							}
						}
					}
					if (bizOType == null && reportUnknownPrefix)
					{
						ErrorReporter.ReportOnce("Business object for TablePrefix '" + tablePrefix + "' is unknown", "Cannot determine the Busines object for TablePrefix '" + tablePrefix + "'");
					}
				}
			}
			return bizOType;
		}

		#endregion

		#region ImportFromAnotherFactory

		public BusinessObject ImportFromAnotherFactory(BusinessObject bizOToImport)
		{
			return ImportFromAnotherFactory(bizOToImport, bizOToImport.GetType());
		}

		public BusinessObject ImportFromAnotherFactory(BusinessObject bizOToImport, Type businessObjectType)
		{
			var tableName = bizOToImport.Row?.Table?.TableName;
			if (tableName != null)
			{
				DataTable targetTable = RowFactory.GetTable(tableName);
				targetTable.ImportRow(bizOToImport.Row);
			}
			var result = Load(businessObjectType, bizOToImport.PK);
			if (!bizOToImport.IsInDatabase)
			{
				ImportFactoryGroups.Track(bizOToImport);
			}
			return result;
		}

		ImportFactoryGroupCollection importFactoryGroups;
		internal ImportFactoryGroupCollection ImportFactoryGroups => importFactoryGroups ?? (importFactoryGroups = new ImportFactoryGroupCollection(this));

		public BusinessObject ImportFromItemArray(Type businessObjectType, object[] itemArray)
		{
			ThreadSentry.EnsureCurrentThreadIsOwner();

			var tableName = GetTableNameFromType(businessObjectType);
			var table = RowFactory.GetTable(tableName);
			var row = table.NewRow();
			var primaryKeyIndex = table.PrimaryKey[0].Ordinal;
			row.ItemArray = itemArray;
			table.Rows.Add(row);
			row.AcceptChanges();

			return Load(businessObjectType, (Guid)row[primaryKeyIndex]);
		}

		#endregion

		#region Reloading BizOsB

		/// <summary>
		/// Reloads the data in an existing BusinessObject, if it is in the database.
		/// </summary>
		/// <param name="bizO"></param>
		/// <param name="throwExceptionIfNotInDatabase"></param>
		///	Used by BusinessObject.ReloadSafe() and BusinessObjectFactory.ReloadAll&lt;T&gt;() to suppress throwing
		///	an exception if the bizo is not in the database
		/// 
		internal void Reload(BusinessObject bizO, bool throwExceptionIfNotInDatabase)
		{
#if DEBUG
			LogAction("RELOAD " + bizO.GetType().FullName + " PK = " + bizO.PK, this);
#endif
			if (bizO.IsInDatabase)
			{
				RowFactory.Reload(GetViewNameFromType(bizO.GetType()), bizO.PK);
			}
			else if (throwExceptionIfNotInDatabase)
			{
				ErrorReporter.ReportOnce("Argh", "Attempted to reload a business object that is not in the database");
			}
		}

		public void ReloadAll<T>() where T : BusinessObject
		{
			var bizosMatchingType = BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess.Where(b => b.GetType() == typeof(T)).Cast<T>();
			if (bizosMatchingType.Any(b => !b.IsInDatabase))
			{
				ErrorReporter.ReportOnce("ReloadAll", "Attempted to reload a business object that is not in the database");
			}

			ReloadAllSafe(bizosMatchingType);
		}

		public void ReloadAllSafe<T>() where T : BusinessObject
		{
			ReloadAllSafe(BusinessObjectCache.AllBusinessObjects.Where(b => b.GetType() == typeof(T)).Cast<T>());
		}

		public void ReloadAllSafe<T>(IEnumerable<T> bizos) where T : BusinessObject
		{
			const int maxCount = 1000;
			ClearQueryCache(GetTableNameFromType(typeof(T)));

			var batchPks = new List<ZGuid>(maxCount);
			foreach (var pk in bizos.Select(b => b.PK))
			{
				batchPks.Add(pk);
				if (batchPks.Count >= maxCount)
				{
					Reload<T>(batchPks);
					batchPks.Clear();
				}
			}
			if (batchPks.Count > 0)
			{
				Reload<T>(batchPks);
			}
		}

		internal void Reload<T>(List<ZGuid> pks) where T : BusinessObject
		{
			var schema = GetTableSchemaFromType(typeof(T));
			var pksQuery = new ZQuery(schema.PK, pks)
			{
				ReLoadExistingRows = true
			};
			Load<T>(pksQuery);
		}

		public void ClearQueryCache()
		{
			RowFactory.ClearQueryCache();
		}

		public void ClearQueryCache(string tableName)
		{
			RowFactory.ClearQueryCache(tableName);
		}

		public bool IsCached(string tableName,ZQuery query)
		{
			return RowFactory.QueryCache.IsCached(tableName, query);
		}

		public bool ShouldPerformFullQueryCacheCleanOnSave
		{
			get { return RowFactory.ShouldPerformFullQueryCacheCleanOnSave; }
			set { RowFactory.ShouldPerformFullQueryCacheCleanOnSave = value; }
		}

		#endregion

		#region Saving BizOs

		/// <summary>
		/// Registers a listener for all factories
		/// You must keep a reference to your object as internally held only using a weak reference
		/// </summary>
		/// <param name="listener"></param>
		public static void RegisterListener(ITransactionParticipantListener listener)
		{
			listenerManager.Register(listener);
		}
		static readonly ListenerManager listenerManager = new ListenerManager();

		public static void UnRegisterListener(ITransactionParticipantListener listener)
		{
			listenerManager.UnRegister(listener);
		}

		public static int GlobalSaveCount
		{
			get { return globalSaveCount; }
			private set { globalSaveCount = value; }
		}
		[ThreadStatic]
		static int globalSaveCount;

		[ThreadStatic]
		static int savingCount;
		public static int SavingCount => savingCount;

		public int SaveCount { get; private set; }

		public bool IsEqualToCurrentSaveCount(int valueToCheck)
		{
			return valueToCheck == SaveCount;
		}

		[ThreadStatic]
		static bool isSavingTogether;
		public static bool IsSavingTogether => isSavingTogether;

#if DEBUG
		public
#endif
		static IDisposable NotifyIsSavingTogether()
		{
			if (isSavingTogether)
			{
				return null;
			}
			else
			{
				isSavingTogether = true;
				return new DisposableAction(() => isSavingTogether = false);
			}
		}

		/// <summary>
		/// Save all BusinessObjects created/loaded by the Factory to the database.
		/// </summary>
		public void Save()
		{
			try
			{
				savingCount++;
				HookSaveForUnitTests();
				SaveCore();
			}
			finally
			{
				savingCount--;
			}
		}

		protected virtual void SaveCore()
		{
			GlobalSaveCount++;
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObjectFactory.Save()", ""))
			{
				SaveTogether(this);
			}
		}

		/// <summary>
		/// Save multiple factories/post-managers to the database in a single transaction.
		/// </summary>
		public static void SaveTogether(params ITransactionParticipant[] factories)
		{
			var success = false;
			listenerManager.NotifyFactorySaveBeginning(factories);
			using (NotifyIsSavingTogether())
			{
				try
				{
					using (PerformanceStatisticsCollector.StartMonitoring("BusinessObjectFactory.SaveTogether()", ""))
					{
						RowFactory.SaveTogether(factories);
					}
					success = true;
				}
				finally
				{
					listenerManager.NotifyFactorySaveCompleted(factories, success);

					if (success)
					{
						foreach (var factory in factories.OfType<BusinessObjectFactory>())
						{
							ValueCacheDomainService.GetIfCreated(factory)?.ClearAfterFactorySave();
						}
					}
				}
			}
		}

		readonly List<SqlApplicationLock> SqlApplicationLocks = new List<SqlApplicationLock>();
		public void AddSqlLockToTransaction(SqlApplicationLock sqlLock)
		{
			SqlApplicationLocks.Add(sqlLock);
			if (RowFactory.DbConnection.TryGetTransactionLockManager(out var transactionLockManager))
			{
				transactionLockManager.AddSqlLock(sqlLock);
			}

			if (!this.IsInTransaction)
			{
				ErrorReporter.ReportOnce("Don't take SqlApplicationLocks on a BusinessObjectFactory outside of a transaction, please.");
			}
		}

		public void ReleaseSqlLocks()
		{
			if (!RowFactory.DbConnection.TryGetTransactionLockManager(out _))
			{
				foreach (var sqlLock in SqlApplicationLocks)
				{
					sqlLock?.Dispose();
				}
			}

			SqlApplicationLocks.Clear();
		}

#if DEBUG
		public
#endif
		int SqlLockCount => SqlApplicationLocks.Count;

		protected virtual bool NotifyOfSaveBeforeAnyFactorySaveBegins()
		{
			return false;
		}

		#endregion

		#region ReadOnlyFactory

		public ReadOnlyBusinessObjectFactory GetCachedReadOnlyFactory()
		{
			var service = ServiceContainer.GetService<ReadOnlyFactoryService>();

			if (service == null)
			{
				service = new ReadOnlyFactoryService();
				ServiceContainer.AddService(service);
				IsReadOnlyFactoryCreated = true;
			}

			return service.ReadOnlyFactory;
		}

		internal bool IsReadOnlyFactoryCreated { get; private set; }

		#endregion

		#region Getting Database Count

#if DEBUG
		/// <summary>
		/// Gets the count of records from the DATABASE (this may not be the same as the count in the FACTORY!).
		/// </summary>
		public int GetDatabaseCount(Type bizOType)
		{
			LogAction("DB COUNT " + bizOType.FullName, this);

			return RowFactory.GetDatabaseCount(GetViewNameFromType(bizOType));
		}
#endif

		/// <summary>
		/// Gets the count of records matching the filter from the DATABASE (this may not be the same as the count in the FACTORY!).
		/// </summary>
		public int GetDatabaseCount(Type bizOType, ZQuery filter)
		{
			ZQuery filterIncludingActive = GetFilterIncludingActiveFilter(bizOType, filter);
#if DEBUG
			LogAction("DB COUNT " + bizOType.FullName + " Filter = " + filterIncludingActive.LiteralTextADO, this);
#endif
			int result = 0;

			if (!filterIncludingActive.IsNoResultQuery)
			{
				result = RowFactory.GetDatabaseCount(GetViewNameFromType(bizOType), filterIncludingActive);
			}
			return result;
		}

		#endregion

		#region Suspend/ResumeValidation

		int ValidationSuspendedCount;

		public void SuspendValidation()
		{
#if DEBUG
			ValidationHasBeenSuspended = true;
#endif
			ValidationSuspendedCount++;
		}

#if DEBUG
		public bool ValidationHasBeenSuspended;
#endif

		public void ResumeValidation()
		{
			if (ValidationSuspendedCount == 0)
			{
				ErrorReporter.ReportOnce("ShouldNotResumeValidationTooManyTimes", "ResumeValidation called 1 too many times");
			}
			ValidationSuspendedCount--;
		}

		public bool IsValidationSuspended
		{
			get { return ValidationSuspendedCount > 0; }
		}

		#endregion

		#region Validation Caching

		public IValidationCache ValidationCache
		{
			get { return validationCache; }
		}

		ValidationCache validationCache;

		int validationDepth;

		internal void BeginValidation(BusinessObject bizO)
		{
			if (validationCache == null)
			{
				validationCache = new ValidationCache();
			}
			validationDepth++;
			OnBeginValidation?.Invoke(bizO);
			validationCache.BeginValidation(bizO);
		}
		public delegate void OnBeginValidationEventHandler(BusinessObject bizo);
		public event OnBeginValidationEventHandler OnBeginValidation;

		internal void BeginValidation(ZPropertyInfo info)
		{
			if (validationCache == null)
			{
				validationCache = new ValidationCache();
			}
			validationDepth++;
			validationCache.BeginValidation(info);
		}

		internal void EndValidation(ZPropertyInfo info)
		{
			if (validationCache == null)
			{
				throw new InvalidOperationException(
					"validationCache should not be null in EndValidation, after passing BeginValidation.");
			}
			validationDepth--;
			//We don't null validationCache here - the purpose of this is that if property validation calls RunPreSaveValidation, then RunPreSaveValidation won't think it's at the shallowest validationDepth and null validationCache.
			//Instead, we get to keep the validationCache around, which is what we want.
			validationCache.EndValidation(info);
		}

		internal void EndValidation(BusinessObject bizO)
		{
			if (validationCache == null)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture,
					"validationCache should not be null in EndValidation, after passing BeginValidation. validationDepth = {0}", validationDepth));
				if (validationDepth <= 0)
				{
					validationDepth = 1; //just so it doesn't remain desynchronized
				}
			}
			validationCache?.EndValidation(bizO);
			validationDepth--;
			if (validationDepth == 0)
			{
				validationCache = null;
			}
		}

		internal bool HasValidationBeenRun(ZPropertyInfo info)
		{
			return validationCache != null && validationCache.HasValidationBeenRun(info);
		}

		internal void SetHasValidationBeenRun(ZPropertyInfo info)
		{
			if (validationCache != null)
			{
				validationCache.SetHasValidationBeenRun(info);
			}
		}

		#endregion

		#region String Interning

		bool isStringInterningActive;

		public void ActivateStringInterning()
		{
			isStringInterningActive = true;
			stringInterner = new StringInterner();
		}

		public void DeactivateStringInterning()
		{
			isStringInterningActive = false;
			stringInterner = null;
		}

		StringInterner stringInterner;

		public object InternValue(ZPropertyInfo info, object value)
		{
			if (isStringInterningActive && info.PropertyType == typeof(ZString))
			{
				return stringInterner.InternValue(value);
			}

			return value;
		}

		#endregion

		#region Calculated Property Cache

		public void InvalidateCachedProperties()
		{
			if (cacheVersion == int.MaxValue)
			{
				cacheVersion = int.MinValue;
			}
			else
			{
				cacheVersion++;
			}

			if (validationCache != null)
			{
				validationCache.ResetCachedData();
			}

			InvalidateCachedPropertiesCore();
		}

		protected virtual void InvalidateCachedPropertiesCore()
		{
		}

		public int CacheVersion
		{
			get { return cacheVersion; }
		}
		int cacheVersion;

		#endregion

		#region Statistics

		StackTrace allocationPath;
		public string AllocationPath
		{
			get { return allocationPath != null ? allocationPath.ToString() : (NoResString)"Suppressed for performance"; }
		}

		public string BusinessObjectsInformation
		{
			get
			{
				return string.Join(
					Environment.NewLine,
					(this as IBusinessObjectFactoryInternals).AllBusinessObjects
						.GroupBy(b => b.GetType())
						.Select(b => new Tuple<Type, int>(b.Key, b.Count()))
						.OrderByDescending(x => x.Item2)
						.Select(formatInfoTuple)
				);

				string formatInfoTuple(Tuple<Type, int> info)
				{
					var tableName = GetTableNameFromType(info.Item1, false);

					if (tableName == null)
					{
						return $"{info.Item1}, {info.Item2}, No table";
					}

					var loadedFetchHints = IsOwnedByCurrentThread ? GetLoadedFetchHintCountForTable(tableName) : -1;

					return $"{info.Item1}, {info.Item2}, Table: {tableName} (Fetch Hints: {loadedFetchHints})";
				}
			}
		}

		public ZDateTime InstantiationTime { get; private set; }

#if DEBUG
		public void ResetDatabaseLoadCount()
		{
			RowFactory.ResetDatabaseLoadCount();
		}

		public void DropHints()
		{
			RowFactory.DropHints();
		}

#endif

		public int DatabaseLoadCount
		{
			get { return _rowFactoryDoNotUseDirectly.DatabaseLoadCount; }
		}

		public TableHitCount[] TableSelects
		{
			get { return RowFactory.TableSelects; }
		}

		public int ActiveTableFetchHints
		{
			get { return RowFactory.ActiveTableFetchHints; }
		}

		public bool ShouldLogNonPersistentTableHitCount => logNonPersistentTableHitCountIndex > 0;
		byte logNonPersistentTableHitCountIndex;

		public IDisposable EnableLogNonPersistentTableHit()
		{
			return new DisposableAction(() => logNonPersistentTableHitCountIndex++, () => logNonPersistentTableHitCountIndex--);
		}

		public int GetLoadedFetchHintCountForTable(string tableName)
		{
			return RowFactory.GetLoadedFetchHintCountForTable(tableName);
		}

		public void ClearLoadedFetchHintCountForTable(string tableName)
		{
			RowFactory.ClearLoadedFetchHintCountForTable(tableName);
		}

#if DEBUG

		public IEnumerable<string> GetAllFetchHintedTableNames()
		{
			return RowFactory.GetAllFetchHintedTableNames();
		}

		public IDisposable EnableFetchHintsProcessingWithoutTableHitCounter()
		{
			return RowFactory.EnableFetchHintsProcessingWithoutTableHitCounter();
		}

		public IDisposable EnableTableHitQueryCollection(string[] tableNames)
		{
			return RowFactory.EnableTableHitQueryCollection(tableNames);
		}

#endif

		public int ActiveFetchHintsForTable(string tableName)
		{
			return RowFactory.ActiveFetchHintsForTable(tableName);
		}

		#endregion

		#region Factory Services

		public bool HasDomainValidation
		{
			get { return validationContainer != null && validationContainer.HasDomainValidation; }
		}

		public ValidationDomainService Validation
		{
			get { return validationContainer ?? (validationContainer = ValidationDomainService.Get(this)); }
		}
		ValidationDomainService validationContainer;

		public ZServiceContainer ServiceContainer
		{
			get { return serviceContainer ?? (serviceContainer = new ZServiceContainer()); }
		}
		ZServiceContainer serviceContainer;

		#endregion

		#region Implementation

		public override int GetHashCode()
		{
			return this._Instance.GetHashCode();
		}

#if DEBUG
		protected
#endif
		List<BusinessObject> last10BizObjsLoaded = new List<BusinessObject>();

		DataRefreshManager refreshManager;
		internal BusinessObjectWrapperManager WrapperManager;

		protected internal BusinessObject[] BusinessObjectsInLastSaveOrder
		{
			get
			{
				if (businessObjectsInOnSavingOrder == null)
				{
					throw new NotSupportedException("No save has not occurred. No save order available.");
				}
				return businessObjectsInOnSavingOrder.ToArray();
			}
			set
			{
				List<BusinessObject> list = new List<BusinessObject>();
				if (value != null)
				{
					list.AddRange(value);
				}
				businessObjectsInOnSavingOrder = list;
			}
		}

		#region INeedDataSet Members

		DataSet INeedDataSet.Data
		{
			get { return ((INeedDataSet)RowFactory).Data; }
		}

		#endregion

		#region RowsLoaded Events

		public event RowsLoadedEventHandler RowsLoaded;

		void OnRowsLoaded(string tableOrViewName, DataRow[] rows, ZQuery query)
		{
			RowsLoaded?.Invoke(null, new RowsLoadedEventArgs(tableOrViewName, rows, query));
		}

		#endregion

		#region Save Events

		public delegate void SavingEventHandler(BusinessObjectFactory factory);
		public event SavingEventHandler Saving;

		void OnSaving()
		{
			Saving?.Invoke(this);
		}

		public delegate void SavedEventHandler(BusinessObjectFactory factory, bool savedSuccessfully);

#if DEBUG
		public int NumberOfSubscribedHandlersToSaved;
		SavedEventHandler saved;
		public event SavedEventHandler Saved
		{
			add
			{
				saved += value;
				NumberOfSubscribedHandlersToSaved++;
			}
			remove
			{
				saved -= value;
				NumberOfSubscribedHandlersToSaved--;
			}
		}
#else
		public event SavedEventHandler Saved;
#endif

		void OnSaved(bool savedSuccessfully)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObjectFactory.OnSaved", ""))
			{
#if DEBUG
				saved?.Invoke(this, savedSuccessfully);
#else
				Saved?.Invoke(this, savedSuccessfully);
#endif
			}
		}

		protected virtual void BusinessObjectsSavedHandler(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				extension.Value.OnFactoryBusinessObjectsSaved(this, businessObjectsInOnSavingOrder);
			}
		}

		#endregion

		RowFactory IBusinessObjectFactoryInternals.RowFactory
		{
			get { return RowFactory; }
		}

		public bool CrossThreadErrorReportingEnabled { get; internal set; }

		public bool AllowChangingThreadOwnership { get; internal set; }

		bool currentlyCheckingCrossThread;

		public bool IsOwnedByCurrentThread
		{
			get { return ThreadSentry.IsOwner; }
		}

		internal RowFactory RowFactory
		{
			get { return GetRowFactory(); }
		}

		/// <summary>
		/// Gets the RowFactory for this BusinessObjectFactory
		/// </summary>
		/// <param name="suppressThreadSentryCheck_neverSetThisToTrueUnlessYouKnowWhatYouAreDoing">Suppresses ThreadSentry.EnsureCurrentThreadIsOwner. This should rarely if ever be set to true. Probably just when DataRefreshBus or QueryCache needs access to all active factories.</param>
		/// <returns></returns>
		internal RowFactory GetRowFactory(bool suppressThreadSentryCheck_neverSetThisToTrueUnlessYouKnowWhatYouAreDoing = false)
		{
			if (CrossThreadErrorReportingEnabled && !currentlyCheckingCrossThread && !suppressThreadSentryCheck_neverSetThisToTrueUnlessYouKnowWhatYouAreDoing)
			{
				currentlyCheckingCrossThread = true;
				try
				{
					ThreadSentry.EnsureCurrentThreadIsOwner();
				}
				finally
				{
					currentlyCheckingCrossThread = false;
				}
			}
			return _rowFactoryDoNotUseDirectly;
		}

		protected readonly RowFactory _rowFactoryDoNotUseDirectly;

		internal void UpdateHasChangesOnOtherBusinessObjectsAroundThisRow(DataRow row, bool hasChangesValue)
		{
			BusinessObjectsForARow bizOs = BusinessObjectCache[row];
			if (bizOs != null)
			{
				bizOs.SetHasChanges(hasChangesValue);
			}
		}

		internal void UpdateHasChangesOnOtherBusinessObjects(BusinessObject bizO)
		{
			var bizObjCached = BusinessObjectCache[bizO.Row];
			if (bizObjCached != null)
			{
				foreach (BusinessObject otherBusinessObject in bizObjCached.Values)
				{
					if (otherBusinessObject != bizO)
					{
						otherBusinessObject.HasChanges = bizO.HasChanges;
					}
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public BusinessObject[] GetBizOsForPK(Guid pK)
		{
			return BusinessObjectCache.GetBusinessObjectsForPK(pK);
		}

		public BusinessObject[] GetBizOsForDataRow(DataRow row)
		{
			BusinessObjectsForARow bizOsForRow = BusinessObjectCache[row];
			return (bizOsForRow?.Values);
		}

		internal void AddNewBusinessObjectToCache(BusinessObject newBizO)
		{
			BusinessObjectCache.Add(newBizO);
		}

		internal T[] LoadDynamicNonPersistent<T>(ZNonPersistentDataQuery dataQuery, out ZDataTable dataTable)
			where T : BusinessObject
		{
			return LoadDynamicNonPersistent<T>(typeof(T), dataQuery, out dataTable);
		}

		T[] LoadDynamicNonPersistent<T>(Type dynamicBizObjType, ZNonPersistentDataQuery dataQuery, out ZDataTable dataTable)
			where T : BusinessObject
		{
			var result = RowFactory.LoadDynamicNonPersistent(dataQuery, ShouldLogNonPersistentTableHitCount);
			dataTable = result.DataTable;
			DataRow[] rows = result.Rows;

			T[] bOs = new T[rows.Length];
			for (int i = 0; i < rows.Length; i++)
			{
				bOs[i] = (T)Activator.CreateInstance(dynamicBizObjType, new object[] { this, rows[i] });
			}

			return bOs;
		}

		#region Get TableName, PK from Type

		public static bool HasTableCode(Type bizOType)
		{
			return GetSchemaPropertyFromType("TableName", bizOType, false) != null && GetSchemaPropertyFromType("PK", bizOType) != null;
		}

		public static bool HasTableName(Type bizOType)
		{
			return GetSchemaPropertyFromType("TableName", bizOType, false) != null;
		}

		public static string GetTableNameFromType(Type bizOType)
		{
			return GetTableNameFromType(bizOType, true);
		}

		public static string GetTableNameFromType(Type bizOType, bool throwOnError)
		{
			string result = null;
#if DEBUG
			if (TestingState.IsRunningTests)
			{
				result = GetTestRunDatabaseName(bizOType, throwOnError);
			}
			else
#endif
			{
				result = GetTableNameFromTypeCore(bizOType, throwOnError);
			}
			return result;
		}

		static string GetTableNameFromTypeCore(Type bizOType, bool throwOnError)
		{
			string result = GetSchemaPropertyFromType("TableName", bizOType, throwOnError);

			if (result == null && throwOnError)
			{
				throw new ZException("The business object <" + bizOType.FullName + "> does not have a Schema.TableName.");
			}

			return result;
		}

#if DEBUG
		static string GetTestRunDatabaseName(Type bizOType, bool throwOnError)
		{
			if (!TestRunDataBaseNames.TryGetValue(bizOType, out string result))
			{
				var tableName = GetTableNameFromTypeCore(bizOType, throwOnError);
				var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(tableName);

				if (tableName != null)
				{
					var schemaTableName = (tableSchema == null) ? tableName : $"{tableSchema.SqlSchemaName}.{tableName}";
					FullyQualifiedSqlTableName fullyQualifiedSqlTableName = new FullyQualifiedSqlTableName() { FullyQualifiedName = schemaTableName };
					if (fullyQualifiedSqlTableName.Database.Length > 0)
					{
						string testRunDatabaseName = fullyQualifiedSqlTableName.Database + NUnit.Framework.TestingState.DatabaseNameSuffix;
						if (Db.Connection.DatabaseExists(testRunDatabaseName))
						{
							fullyQualifiedSqlTableName.Database = testRunDatabaseName;
						}
					}
					result = fullyQualifiedSqlTableName.FullyQualifiedName;
					TestRunDataBaseNames[bizOType] = result;
				}
			}
			return result;
		}

		static Dictionary<Type, string> TestRunDataBaseNames
		{
			get { return testRunDataBaseNames ?? (testRunDataBaseNames = new Dictionary<Type, string>()); }
		}
		[ThreadStatic]
		static Dictionary<Type, string> testRunDataBaseNames;
#endif

		public static string GetTableCodeFromType(Type bizOType)
		{
			string result = GetPKNameFromType(bizOType);
			if (result != null)
			{
				int indexOf_ = result.IndexOf("_");
				if (indexOf_ > 0)
				{
					return result.Substring(0, indexOf_);
				}
			}
			return "";
		}

		public static string GetViewNameFromType(Type bizOType)
		{
			return GetSchemaPropertyFromType("ViewName", bizOType) ?? GetTableNameFromType(bizOType);
		}

		public static ITableSchema GetTableSchemaFromType(Type bizOType, bool throwOnError = true)
		{
			string tableName = GetTableNameFromType(bizOType, throwOnError);
			ITableSchema tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(tableName);
			return tableSchema;
		}

		internal static SchemaPKColumn GetPKColumnFromType(Type bizOType)
		{
			return BusinessObjectFactory.GetTableSchemaFromType(bizOType).PK;
		}

		internal static string GetPKNameFromType(Type bizOType)
		{
			return GetSchemaPropertyFromType("PK", bizOType) ?? throw new ZException("The business object <" + bizOType.FullName + "> does not have a Schema.PK.");
		}

		static string GetSchemaPropertyFromType(string schemaPropertyName, Type bizOType)
		{
			return GetSchemaPropertyFromType(schemaPropertyName, bizOType, true);
		}

		static string GetSchemaPropertyFromType(string schemaPropertyName, Type bizOType, bool throwOnErrorInDebug)
		{
#if DEBUG
			if (throwOnErrorInDebug && !bizOType.IsSubclassOf(typeof(BusinessObject)))
			{
				throw new ZException(bizOType.FullName + " is not a valid BusinessObject type.\n This is most likely a namespace issue. Or it could be that you are using 'return Factory.New<MyBizOCollection>()' instead of 'return new MyBizOCollection(Factory)' in GetCollectionToTest(). \n");
			}
#endif
			return BizOTypeCache[bizOType][schemaPropertyName];
		}

		static BizOTypeHash BizOTypeCache
		{
			get { return bizOTypeCache ?? (bizOTypeCache = new BizOTypeHash()); }
		}
		[ThreadStatic]
		static BizOTypeHash bizOTypeCache;

		#endregion

		#region Creating BizOs

		BusinessObjectCache BusinessObjectCache;

		BusinessObjectNKCache businessObjectNKCache;
		BusinessObjectNKCache BusinessObjectNKCache
		{
			get { return businessObjectNKCache ?? (businessObjectNKCache = new BusinessObjectNKCache()); }
		}

		/// <summary>
		/// For creating BusinessObject in another Factory
		/// </summary>
		/// <param name="row"></param>
		/// <param name="initialType"></param>
		/// <param name="factory"></param>
		/// <returns></returns>
		public BusinessObject CreateBusinessObject(DataRow row, Type initialType, BusinessObjectFactory factory, ITypeDeciderContext typeDeciderContext = null)
		{
			return factory.CreateBusinessObject(row, initialType, typeDeciderContext);
		}

		protected internal virtual BusinessObject CreateBusinessObject(DataRow row, Type initialType, ITypeDeciderContext typeDeciderContext = null)
		{
			return CreateBusinessObject(row, initialType, true, typeDeciderContext: typeDeciderContext).BusinessObject;
		}

		struct BusinessObjectCreationResult
		{
			public BusinessObjectCreationResult(BusinessObject businessObject, bool alreadyExisted)
			{
				BusinessObject = businessObject;
				AlreadyExisted = alreadyExisted;
			}

			public readonly BusinessObject BusinessObject;
			public readonly bool AlreadyExisted;
		}

		BusinessObjectCreationResult CreateBusinessObject(DataRow row, Type initialType, bool callOnLoad, Func<object[], BusinessObject> createInstanceFunc = null, ITypeDeciderContext typeDeciderContext = null)
		{
			Type bizOType = TypeDecider.GetTypeToCreateFromRow(row, initialType, this, typeDeciderContext);

			BusinessObject result = null;
			BusinessObjectsForARow fromCache = BusinessObjectCache[row];

			if (fromCache != null)
			{
				BusinessObject potentialResult = fromCache[bizOType];
				if (!ReferenceEquals(potentialResult, null))
				{
					result = potentialResult;
				}
				else if (!AllowMultipleBusinessObjectsAroundOneRow || SingleObjectAroundARow.HasAttribute(initialType))
				{
					result = fromCache.FirstBizO;
					if (!initialType.IsAssignableFrom(result.GetType()))
					{
						throw new ApplicationException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
							"Attempted to return a {0} when a {1} was requested.(AllowMultipleBusinessObjectsAroundOneRow={2}, SingleObjectAroundARow={3})",
							result.GetType().FullName, initialType.FullName, AllowMultipleBusinessObjectsAroundOneRow, SingleObjectAroundARow.HasAttribute(initialType)));
					}
				}
				else
				{
					hasLoadedMultipleBusinessObjectsAroundOneRow = true;
				}
			}
			bool alreadyExistedInCache = result != null;
			if (result == null)
			{
				result = InstantiateNewBusinessObject(row, bizOType, callOnLoad, createInstanceFunc);
				if (activeSaveMethodCaller != null)
				{
					activeSaveMethodCaller.BusinessObjectsAddedDuringCallMethod.Add(result);
				}
				UpdateHasChangesForOtherObjectsAroundRow(result);
			}
			return new BusinessObjectCreationResult(result, alreadyExistedInCache);
		}

		void UpdateHasChangesForOtherObjectsAroundRow(BusinessObject newBizO)
		{
			BusinessObjectCache[newBizO.Row].UpdateHasChangesForOtherObjectsAroundRow(newBizO);
		}

		readonly Dictionary<Type, Type> rowFetchStrategyCache = new Dictionary<Type, Type>();

		void GenerateRowFetchHints(Type bizOType, DataRow[] rows)
		{
			if (!rowFetchStrategyCache.TryGetValue(bizOType, out Type fetchStrategyType))
			{
				var attributes = bizOType.GetCustomAttributes(typeof(RowFetchStrategyAttribute), true);
				fetchStrategyType = attributes.Length > 0 ? ((RowFetchStrategyAttribute)attributes[0]).FetchStrategyType : null;
				rowFetchStrategyCache[bizOType] = fetchStrategyType;
			}
			if (fetchStrategyType != null)
			{
				var strategy = (IRowFetchStrategy)Activator.CreateInstance(fetchStrategyType);
				strategy.FetchForLoad(this, rows);
			}
		}

		BusinessObject[] CreateBusinessObjectsFromRows(Type bizOType, DataRow[] rows)
		{
			var result = new List<BusinessObject>(rows.Length);
			var newObjects = new List<BusinessObject>();

			if (rows.Length > 0)
			{
				foreach (var row in rows)
				{
					var cachedResult = BusinessObjectCache.Fetch(bizOType, ZDataUtils.GetPK(row));
					if (ReferenceEquals(cachedResult, null))
					{
						var creationResult = new BusinessObjectCreationResult(null, false);
						try
						{
#if DEBUG
							ChangeRowStatus?.Invoke(row);
#endif
							creationResult = CreateBusinessObject(row, bizOType, false);
						}
						catch (DeletedRowInaccessibleException)
						{
							continue;
						}

						if (!bizOType.IsInstanceOfType(creationResult.BusinessObject))
						{
							ReportIncompatibleTypeErrorMessage(bizOType, creationResult.BusinessObject, "BusinessObjectFactory.CreateBusinessObject()", row);
						}
						else
						{
							result.Add(creationResult.BusinessObject);
							if (creationResult.BusinessObject.IsInDatabase && !creationResult.AlreadyExisted)
							{
								newObjects.Add(creationResult.BusinessObject);
							}
						}
					}
					else
					{
						if (!bizOType.IsInstanceOfType(cachedResult))
						{
							ReportIncompatibleTypeErrorMessage(bizOType, cachedResult, "BusinessObjectCache.Fetch()", row);
						}
						else
						{
							result.Add(cachedResult);
						}
					}
				}

				foreach (var bizObj in newObjects)
				{
					using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnLoaded", bizObj.GetType()))
					{
						bizObj.OnLoaded();
					}
				}
			}

			FetchForType(bizOType);

#if DEBUG

			if (newObjects.Count > 0)
			{
				Loaded?.Invoke(this, new LoadedEventArgs(newObjects));

				if (TestingState.IsRunningTests)
				{
					if (newObjects.Count > MaximumObjectsToCreateBeforeRaisingLargeLoadError)
					{
						if (TestingState.CurrentTestMethod == null || TestingState.CurrentTestMethod.GetCustomAttributes(typeof(StressTestAttribute), false).Length == 0)
						{
							ErrorReporter.ReportOnce("ExceededAllowableNewObjectCount", Environment.NewLine +
									"You have just created " + newObjects.Count + " " + bizOType.FullName + " objects, " + Environment.NewLine +
									"which is a very large number for a test. Change the test so it uses fewer objects, " + Environment.NewLine +
									"while still testing the business logic correctly." + Environment.NewLine +
									"If this is a stress test, and is meant to be slow and use lots of memory, add the StressTestAttribute.");
						}
					}
				}
			}

#endif

			BusinessObject[] resultArray = (BusinessObject[])Array.CreateInstance(bizOType, result.Count);
			for (int i = 0; i < result.Count; i++)
			{
				resultArray[i] = result[i];
			}

			return resultArray;
		}

#if DEBUG
		public delegate void DelegateForChangeRowStatus(DataRow row);
		public DelegateForChangeRowStatus ChangeRowStatus { get; set; }
#endif

		void ReportIncompatibleTypeErrorMessage(Type expectedType, BusinessObject actualBizObj, string source, DataRow row)
		{
			var actualType = actualBizObj.GetType();
			var additionalDetails = actualBizObj is IAdditionalDebuggingDetails additionalDebuggingDetails ? string.Join("", additionalDebuggingDetails.AdditionalDetails.WhereNotNull().Select(x => Environment.NewLine + x.Trim())) : string.Empty;
			ErrorReporter.ReportOnce($"Cannot cast {actualType.FullName} to {expectedType.FullName}", FormattableString.Invariant(
$@"{nameof(CreateBusinessObjectsFromRows)} attempted to return a business object of incompatible type. 
Expected type: {expectedType.FullName}; 
Actual object type: {actualType.FullName}; 
Actual object returned by: {source}; 
Data row PK: {ZDataUtils.GetPK(row)}{additionalDetails}"));
		}

#if DEBUG

		public event EventHandler<LoadedEventArgs> Loaded;

		public const int MaximumObjectsToCreateBeforeRaisingLargeLoadError = 1000;

#endif

		BusinessObject InstantiateNewBusinessObject(DataRow row, Type bizOType, bool callOnLoaded, Func<object[], BusinessObject> createInstanceFunc)
		{
			BusinessObject @new = null;

			EnsureTypeIsBusinessObject(bizOType);

			try
			{
				object[] constructorArgs = new object[] { this, row };

				if (createInstanceFunc != null)
				{
					@new = createInstanceFunc(constructorArgs);
				}
				else
				{
					@new = (BusinessObject)Activator.CreateInstance(bizOType, constructorArgs);
				}

				if (@new != null)
				{
					if (((IBusinessObjectInternals)@new).SubscribeToDataRefreshOnInstantiation)
					{
						refreshManager.StartManaging(@new);
					}

					InvalidateCachedProperties();

					if (@new.IsInDatabase)
					{
						if (LoadFetchingEnabled)
						{
							@new.FetchStrategy.FetchForLoad();

							foreach (IBusinessObjectStrategy boStrategy in @new.Strategies)
							{
								boStrategy.FetchForLoad(@new);
							}
						}
						if (callOnLoaded)
						{
							using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnLoaded", bizOType))
							{
								@new.OnLoadedInternal();
							}
						}
					}

					@new.OnInitialized();
				}
			}
			catch (MissingMethodException e)
			{
				throw new ApplicationException("Can't create a " + bizOType.Name +
						". Please check that you have implemented a constructor in your " + bizOType.Name + ":" +
						"\r\n\r\n" +
						"public " + bizOType.Name + "(BusinessObjectFactory Factory, DataRow Row) : base(Factory, Row) {} \r\n\r\n", e);
			}
			catch (TargetInvocationException e)
			{
				throw new ApplicationException("Can't create a " + bizOType.Name +
						" as its constructor threw an exception.\r\n\r\n", e);
			}

			return @new;
		}

		void EnsureTypeIsBusinessObject(Type type)
		{
			if (!typeof(BusinessObject).IsAssignableFrom(type))
			{
				throw new ZException("The BusinessObjectFactory only works with BusinessObject types. " +
						type.FullName + " is not a BusinessObject.");
			}
		}

		#endregion

		void SetRowsShouldBeSaved()
		{
			DataSet data = ((INeedDataSet)this).Data;

			int tableIndex = 0, rowIndex = 0;

			while (tableIndex < data.Tables.Count)
			{
				DataTable table = data.Tables[tableIndex];

				var skipCheckingRowsShouldBePersistent =
					table.ExtendedProperties[typeof(BulkCopySetting)] is BulkCopySetting
					{
						CheckRowsShouldBePersistent: false
					};

				if (!skipCheckingRowsShouldBePersistent)
				{
					rowIndex = 0;
					while (rowIndex < table.Rows.Count)
					{
						DataRow row = table.Rows[rowIndex];
						BusinessObjectsForARow fromCache = BusinessObjectCache[row];
						bool isPersistent = fromCache == null || fromCache.IsSavedByFactory;
						ZDataUtils.SetShouldRowBeSaved(row, isPersistent);

						rowIndex++;
					}
				}

				tableIndex++;
			}
		}

		protected internal virtual ZQuery GetFilterIncludingActiveFilter(Type bizObjType, ZQuery query)
		{
			ZQuery result = query.ShallowClone();
			if (!query.IgnoreActiveFilter)
			{
				result.AddToFilter(BusinessObject.GetActiveFilter(bizObjType), JoinCondition.And);
			}
			return result;
		}

		#region Debug Info

		//TODO Remove on resolution of WI00704670. Added to aid diagnosis. 
		public string GetDebugInformation(string tableName, ZGuid primaryKey)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Is this factory an Uber Factory itself: {IsUberFactory}");
			sb.AppendLine($"Does key exist if loaded with this factory: {GetRowFactory().LoadFromPK(tableName, primaryKey) != null}");
			sb.AppendLine($"Does key exist with a reload: {GetRowFactory(true).Reload(tableName, primaryKey) != null}");
			sb.AppendLine($"Row Factory Information:");
			sb.AppendLine(GetRowFactory().GetDebugInformation(tableName, primaryKey));
			return sb.ToString();
		}

#if DEBUG

		public long _BusinessObjectInstance;
		public string _TestName;

		void SetupTraceData()
		{
			_TestName = NUnit.Framework.TestCase.CurrentTestName;
		}

		[ThreadStatic]
		static bool LoggingEnabled;

		[ThreadStatic]
		internal static ZGuid ExcludedGuidForLogging;

		static StringCollectionX Log
		{
			get { return log ?? (log = new StringCollectionX()); }
		}
		[ThreadStatic]
		static StringCollectionX log;

		protected static void LogAction(string message, BusinessObjectFactory factory)
		{
			if (LoggingEnabled && factory.NameForDebugging != "UserContext")
			{
				Log.Add("#" + factory._Instance + " - " + message);
			}
		}

		public static void StartLogging()
		{
			LoggingEnabled = true;
			Log.Clear();
		}

		public static void StopLogging()
		{
			LoggingEnabled = false;
			Log.Clear();
			ExcludedGuidForLogging = ZGuid.Empty;
		}

		public static int DebugLogCount
		{
			get
			{
				if (!LoggingEnabled)
				{
					throw new NotSupportedException("Logging is not started, can't access log");
				}

				return Log.Count;
			}
		}

		public static string DebugLog
		{
			get
			{
				if (!LoggingEnabled)
				{
					throw new NotSupportedException("Logging is not started, can't access log");
				}

				return Log.ToString();
			}
		}

		public static string DebugLogWithoutFirstEntry
		{
			get
			{
				StringCollectionX result = new StringCollectionX();
				if (Log.Count > 0)
				{
					result.AddRange(Log);
					result.RemoveAt(0);
				}
				return result.ToString();
			}
		}

#endif
		#endregion

		#endregion

		#region IBusinessObjectFactoryInternals Members

		void IBusinessObjectFactoryInternals.Rollback()
		{
			var allBizos = BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess;

			for (int i = 0; i < allBizos.Count; i++)
			{
				BusinessObject bizo = allBizos[i];
				bizo.OnSaveRollbackInternal();
			}

			RowFactory.Rollback();

			allBizos = BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess;
			int initialCount = allBizos.Count;

			for (int i = 0; i < initialCount; i++)
			{
				if (i < allBizos.Count)
				{
					BusinessObject bizo = allBizos[i];
					bizo.OnSaveRollback();
				}
			}
		}

		/// <summary>
		/// No changes will be possible to data in the factory.
		/// </summary>
		bool IBusinessObjectFactoryInternals.ReadOnly
		{
			get { return fReadOnly; }
			set { fReadOnly = value; }
		}
		bool fReadOnly;

		/// <summary>
		/// No changes will be possible to data in the factory.
		/// </summary>
		bool IBusinessObjectFactoryInternals.CanSave
		{
			get { return fCanSave; }
			set { fCanSave = value; }
		}
		bool fCanSave = true;

		/// <summary>
		/// Dumps contents from XML.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		string IBusinessObjectFactoryInternals.ContentsAsXMLForDebugging
		{
			get
			{
				StringWriter writer = new StringWriter();
				var dataSet = ((INeedDataSet)this).Data;
				foreach (DataTable table in dataSet.Tables)
				{
					var columns = table.Columns.Cast<DataColumn>().Where(c => c.DataType.Name == "SqlGeography").ToList();
					foreach (var column in columns)
					{
						table.Columns.Remove(column);
					}
				}

				dataSet.WriteXml(writer, XmlWriteMode.WriteSchema);
				return writer.ToString();
			}
		}

		int IBusinessObjectFactoryInternals.NumberOfBusinessObjects
		{
			get { return BusinessObjectCache.NumberOfBusinessObjects; }
		}

		IReadOnlyList<BusinessObject> IBusinessObjectFactoryInternals.AllBusinessObjects
		{
			get { return BusinessObjectCache.AllBusinessObjects; }
		}

#if DEBUG
		bool IBusinessObjectFactoryInternals.DisableQueryCacheReset
		{
			get { return RowFactory.DisableQueryCacheReset; }
			set { RowFactory.DisableQueryCacheReset = value; }
		}
#endif

		bool IBusinessObjectFactoryInternals.IsProcessingOnAllTransactionsCommitted
		{
			get { return isProcessingOnAllTransactionsCommitted; }
		}
		bool isProcessingOnAllTransactionsCommitted;

		bool IBusinessObjectFactoryInternals.LastSavingRollbackHadException { get; set; }

		bool IBusinessObjectFactoryInternals.IsProcessingOnFactorySavingBeforeTransaction => isProcessingOnFactorySavingBeforeTransaction;
		bool isProcessingOnFactorySavingBeforeTransaction;

		bool IBusinessObjectFactoryInternals.IncludeWithOtherFactoriesForIssueReport
		{
			get
			{
				return includeWithOtherFactoriesForIssueReport;
			}
			set
			{
				includeWithOtherFactoriesForIssueReport = value;
			}
		}

		bool includeWithOtherFactoriesForIssueReport = true;

		#endregion

		#region IFactoryProvider Members

		BusinessObjectFactory IFactoryProvider.Factory
		{
			get { return this; }
		}

		#endregion

		internal ZPropertyInfoStorage PropertyInfoStorage = new ZPropertyInfoStorage();

		#region ITransactionParticipant Members

		public long TransactionId { get; private set; }
		long previousSaveTransactionId = ZDateTime.UtcNow.Ticks;

		ITransactionManager ITransactionStarter.BeginTransactionWithManager()
		{
			TransactionId = Interlocked.Increment(ref previousSaveTransactionId);
			var transactionManager = ((ITransactionParticipant)RowFactory).BeginTransactionWithManager();
			try
			{
				if (NotifyOfSaveBeforeAnyFactorySaveBegins())
				{
					new OnFactorySavingCaller().CallMethodOnAllBusinessObjects(this);
					CallBusinessObjectsOnSaving();
				}

				return new TransactionManager(this, transactionManager);
			}
			catch
			{
				transactionManager.Dispose();
				throw;
			}
		}

		void ITransactionParticipant.OnAllTransactionsBeginning()
		{
			ValueCacheDomainService.GetIfCreated(this)?.ClearBeforeFactorySaving();

			new OnFactorySavingFetchBeforeTransactionCaller().CallMethodOnAllBusinessObjects(this);
			new OnFactorySavingBeforeTransactionCaller().CallMethodOnAllBusinessObjects(this);
			((ITransactionParticipant)RowFactory).OnAllTransactionsBeginning();
		}

		void ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			((ITransactionParticipant)RowFactory).OnAllTransactionsCommitted(changedTableNames);

			ActiveBusinessObjectCollection.CollectionsAreRefreshedOnSave = true;
			isProcessingOnAllTransactionsCommitted = true;
			servicesAddedDuringOnSaving = null;
			try
			{
				using (RefreshEnabled ? DelayListChangedEventsOnAllFactories() : ActiveBusinessObjectCollection.DelayListChangedEvents(this))
				{
					ActiveBusinessObjectCollection.RefreshAll(this);

					var bizToPublish = new List<BusinessObject>();
					bizToPublish.AddRange(BusinessObjectsInLastSaveOrder);
					if (nonPersistentBusinessObjectsInOnPublishOrder != null)
					{
						bizToPublish.AddRange(nonPersistentBusinessObjectsInOnPublishOrder);
					}
					RefreshManager.Publish(bizToPublish.ToArray());
					importFactoryGroups?.EnsureSynchronisation(this);
					if (businessObjectsInOnSavingOrder != null)
					{
						List<BusinessObject> bizosForCurrentIteration = new List<BusinessObject>(businessObjectsInOnSavingOrder);

						CallBusinessObjectsOnFactorySaved(true);
						using (PerformanceStatisticsCollector.StartMonitoring("BusinessObjectFactory.CallBusinessObjectsOnSaved", ""))
						{
							foreach (BusinessObject bizObj in bizosForCurrentIteration)
							{
								bizObj.OnSavedInternal(true);
							}
						}
					}

					OnSaved(true);

					BusinessObjectCache.ClearDeletedElements();
				}
			}
			finally
			{
				isProcessingOnAllTransactionsCommitted = false;
				ActiveBusinessObjectCollection.CollectionsAreRefreshedOnSave = false;

				var afterCommittedServiceProvider = ServiceContainer.GetService<AfterCommittedServiceProvider>();
				afterCommittedServiceProvider?.DoAfterCommitted(businessObjectsInOnSavingOrder);
			}
		}

		void ITransactionParticipant.OnAllTransactionsRolledBack()
		{
			((ITransactionParticipant)RowFactory).OnAllTransactionsRolledBack();

			TransactionId = 0;
			CallBusinessObjectsOnFactorySaved(false);
			if (businessObjectsInOnSavingOrder != null) // if an exception was thrown earlier, this could be null
			{
				var exceptions = new List<Exception>();

				foreach (BusinessObject bizObj in businessObjectsInOnSavingOrder)
				{
					try
					{
						bizObj.OnSavedInternal(false);
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}

				if (exceptions.Count != 0)
				{
					throw new AggregateException("Aggregated Exception during OnSave", exceptions);
				}
			}
			if (servicesAddedDuringOnSaving != null)
			{
				foreach (var service in servicesAddedDuringOnSaving)
				{
					service.OnSaveFailed();
				}
				servicesAddedDuringOnSaving = null;
			}
			OnSaved(false);
		}

		IDisposable DelayListChangedEventsOnAllFactories()
		{
			lock (allFactoriesOperationMutex)
			{
				return new DisposableList(DataRefreshManager.Factories.Where(f => f.ThreadSentry.IsOwner).Select(ActiveBusinessObjectCollection.DelayListChangedEvents));
			}
		}

		static readonly object allFactoriesOperationMutex = new object();

		IChangedTableNames ITransactionParticipant.SaveInTransaction()
		{
			HookSaveInTransactionForUnitTests();
			ThrowSpecificExceptionWhenCountReachesLimit();
			return SaveInTransactionCore();
		}

		[Conditional("DEBUG")]
		void ThrowSpecificExceptionWhenCountReachesLimit()
		{
#if DEBUG
			saveCount.Value = saveCount.Value + 1;
			if (saveLimit.Value == saveCount.Value)
			{
				throw exceptionToThrow.Value;
			}
#endif
		}

#if DEBUG
		static readonly Overridable<int> saveLimit = new Overridable<int>();
		static readonly Overridable<int> saveCount = new Overridable<int>();
		static readonly Overridable<Exception> exceptionToThrow = new Overridable<Exception>();

		public static void ThrowExceptionWhenSaveCountReachesLimit(int limit, Exception exception)
		{
			Argument.GreaterThan(limit, 0, nameof(limit));
			Argument.NotNull(exception, nameof(exception));
			saveCount.Value = 0;
			saveLimit.Value = limit;
			exceptionToThrow.Value = exception;
		}
#endif

		public bool IsAfterOnSavingButBeforeCommit => logAfterOnSavingButBeforeCommit > 0;
		byte logAfterOnSavingButBeforeCommit;

		public IDisposable TrackingAfterOnSavingButBeforeCommit()
		{
			return new DisposableAction(() => logAfterOnSavingButBeforeCommit++, () => logAfterOnSavingButBeforeCommit--);
		}

		public bool IsInSaveTransaction;

		protected virtual IChangedTableNames SaveInTransactionCore()
		{
			try
			{
				IsInSaveTransaction = true;
				SaveCount++;
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(this))
				{
					if (!((IBusinessObjectFactoryInternals)this).CanSave)
					{
						throw new NotSupportedException("This factory cannot be saved.");
					}

#if DEBUG
					LogAction("SAVE", this);
#endif

					new OnFactorySavingFetchCaller().CallMethodOnAllBusinessObjects(this);
					using (FactorySavingCaller caller = new FactorySavingCaller(this))
					{
						caller.FactorySaving();
						if (!NotifyOfSaveBeforeAnyFactorySaveBegins())
						{
							new OnFactorySavingCaller().CallMethodOnAllBusinessObjects(this);
							CallBusinessObjectsOnSaving();
						}
					}

					var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
					var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(this, BusinessObjectsInLastSaveOrder);

					using (TrackingAfterOnSavingButBeforeCommit())
					{
						var afterOnSavingService = ServiceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>();
						if (afterOnSavingService != null)
						{
							afterOnSavingService.ProcessBusinesObjects(BusinessObjectsInLastSaveOrder);
						}
						SetRowsShouldBeSaved();

						var criticalValidationService = ServiceContainer.GetService<CriticalValidationServiceProvider>();
						if (criticalValidationService != null)
						{
							criticalValidationService.ProcessBusinessObjects(BusinessObjectsInLastSaveOrder);
						}
					}

					refreshManager.AcceptChangesOnRowsWithDelayedAcceptChanges(BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess);

					IChangedTableNames changedTables = null;

					try
					{
						var rows = RowFactory.GetModifiedPersistentRowsInSaveOrder().ToList();
						var bizosNeedCallBusinessObjectStrategys = rows.SelectMany(r => BusinessObjectCache[r]?.Values ?? Enumerable.Empty<BusinessObject>()).Where(bizo => !bizo.OnSavingCalledInternal || bizo.HasSkippedOnSavingDueToLightValidation);

						foreach (var bizo in bizosNeedCallBusinessObjectStrategys)
						{
							using (bizo.HasSkippedOnSavingDueToLightValidation ? bizo.SuspendListChanged() : null)
							{
								bizo.OnSavingInObjectsWithLateChanges();
							}
							if (bizo.HasSkippedOnSavingDueToLightValidation)
							{
								bizo.HasSkippedOnSavingDueToLightValidation = false;
							}
						}

						changedTables = new ChangedTableNames(RowFactory.GetTablesThatNeedToBeSaved().Select(table => table.TableName).ToList());
						((ITransactionParticipant)RowFactory).SaveInTransaction();
						ServiceContainer.GetService<AfterSaveInTransactionService>()?.DoFinalCheckBeforeCommit(BusinessObjectsInLastSaveOrder);
						deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, this);
					}
					catch (ZDataConcurrencyException ex)
					{
						throw new ZSaveConcurrencyException(ex, this);
					}
					catch (ZDataException ex)
					{
						CheckDataSavedButNotUpdated(ex);

						throw new ZSaveException(ex, this);
					}
					return changedTables;
				}
			}
			finally
			{
				IsInSaveTransaction = false;
			}
		}

		public bool IsInTransaction
		{
			get { return ((ITransactionParticipant)RowFactory).IsInTransaction; }
		}

		public long TopLevelTransactionsBegunCount => RowFactory.DbConnection?.TopLevelTransactionsBegunCount ?? 0;

		public bool AllowTransactionWithOtherParticipant => Parent != null || (ChildFactories.Count > 0);

		#region CheckDataSavedButNotUpdated

		void CheckDataSavedButNotUpdated(ZDataException ex)
		{
			if (ex.CoreErrorHandler != null)
			{
				var errorType = ex.CoreErrorHandler.ExceptionType;
				if (errorType == DbErrorType.TimeoutExpired || errorType == DbErrorType.LockTimeoutExpired || errorType == DbErrorType.GeneralNetworkError)
				{
					var connection = ((IDbConnected)this).Connection;
					if (connection == null || connection.CurrentDatabase != Db.DatabaseName)
					{
						return;
					}

#if DEBUG
					if (!CheckSaveButNotUpdatedInSameConnectionForTest)
					{
						using (var otherConnection = Db.NewExtraConnectionToMainDbWithReaderCredentials())
						{
							CheckForUnsyncedRecords(ex, connection);
						}
					}
					else
#endif
					{
						CheckForUnsyncedRecords(ex, connection);
					}
				}
			}
		}

		void CheckForUnsyncedRecords(ZDataException ex, DbConnection connection)
		{
			var extraFactory = new BusinessObjectFactory(connection) { RefreshEnabled = false, fReadOnly = true, NameForDebugging = "CheckDataSavedButNotUpdated" };
			if (HasSavedUnsyncRecords(extraFactory))
			{
				throw new ZSaveErrorAfterCommitInDbException(ex, this);
			}
		}

		#region Test stuff
#if DEBUG
		public bool CheckSaveButNotUpdatedInSameConnectionForTest { get; set; }
#endif
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		bool HasSavedUnsyncRecords(BusinessObjectFactory extraFactory)
		{
			var allBizos = businessObjectsInOnSavingOrder ?? BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess;
			var bizo = allBizos.FirstOrDefault(b => b.IsSavedByFactory && !b.IsInDatabase && !b.IsRowDeletedOrDetachedOrNull);
			if (bizo != null)
			{
				try
				{
					if (bizo is IBusinessObjectReload reloadableBizo)
					{
						return reloadableBizo.Reload(extraFactory) != null;
					}

					return extraFactory.Load(bizo.GetType(), bizo.PK) != null;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Selected business object does not support load - ignore and go with standard exception processing.
					return false;
				}
			}

			return false;
		}

		#endregion

		#region ITableFetchHintCreatorSupporter

		void IExternalFetchHintSupporter.AddTableFetchHintCreator(ITableSchema tableSchema, Func<IColumnIndexer, IEnumerable<IFetchHint>> getFetchHints, bool applyToExistingRows)
		{
			RowFactory.AddTableFetchHintCreator(tableSchema, getFetchHints, applyToExistingRows);
		}

		IDisposable IExternalFetchHintSupporter.SetupCreator()
		{
			return RowFactory.SetupTableFetchHintCreators();
		}

		void IExternalFetchHintSupporter.AddRelatedTableHints(string tableName, IColumnIndexer[] dataRows)
		{
			RowFactory.AddRelatedTableHints(tableName, dataRows);
		}

		#endregion

		#region Child Participiants

		ITransactionParticipant[] ITransactionParticipant.ChildParticipants
		{
			get
			{
				List<ITransactionParticipant> result = new List<ITransactionParticipant>(ChildFactories.Count + SaveInTransactionActions.Count);
				result.AddRange(ChildFactories.ToArray());
				result.AddRange(SaveInTransactionActions);
				return result.ToArray();
			}
		}

		IEnumerable<ISqlApplicationLock> ITransactionParticipant.TransactionLocks => SqlApplicationLocks;

		List<SaveInTransactionAction> saveInTransactionActions;
		public List<SaveInTransactionAction> SaveInTransactionActions
		{
			get
			{
				if (saveInTransactionActions == null)
				{
					saveInTransactionActions = new List<SaveInTransactionAction>();
				}
				return saveInTransactionActions;
			}
		}

		public BusinessObjectFactoryChildCollection ChildFactories
		{
			get
			{
				if (childFactories == null)
				{
					childFactories = new BusinessObjectFactoryChildCollection(this);
				}
				return childFactories;
			}
		}

		BusinessObjectFactoryChildCollection childFactories;

		#endregion

		#region IPersistentFactory Implementation

		abstract class BOMethodCaller
		{
			public void CallMethodOnAllBusinessObjects(BusinessObjectFactory factory)
			{
				bool wasCriticalExceptionThrown = false;
				using (PerformanceStatisticsCollector.StartMonitoring("BusinessObjectFactory." + GetType().Name, ""))
				{
					factory.activeSaveMethodCaller = this;
					try
					{
						var allBizos = factory.BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess;
						int initialCount = allBizos.Count;
						factory.isProcessingOnFactorySavingBeforeTransaction = this is OnFactorySavingBeforeTransactionCaller;
						for (int i = 0; i < initialCount; i++)
						{
							if (i < allBizos.Count)
							{
								BusinessObject bizo = allBizos[i];
								if (!bizo.IsDeleted)
								{
									CallMethod(bizo);
								}
							}
						}
					}
					catch (OutOfMemoryException)
					{
						wasCriticalExceptionThrown = true;
						throw;
					}
					finally
					{
						if (!wasCriticalExceptionThrown)
						{
							try
							{
								if (businessObjectsAddedDuringCallMethod != null)
								{
									for (int i = 0; i < BusinessObjectsAddedDuringCallMethod.Count; i++)
									{
										var biz = BusinessObjectsAddedDuringCallMethod[i];

										if (!biz.IsDeleted)
										{
											CallMethod(biz);
										}
									}
								}
							}
							finally
							{
								factory.activeSaveMethodCaller = null;
							}
						}
						factory.isProcessingOnFactorySavingBeforeTransaction = false;
					}
				}
			}

			public List<BusinessObject> BusinessObjectsAddedDuringCallMethod
			{
				get
				{
					if (businessObjectsAddedDuringCallMethod == null)
					{
						businessObjectsAddedDuringCallMethod = new List<BusinessObject>();
					}
					return businessObjectsAddedDuringCallMethod;
				}
			}
			List<BusinessObject> businessObjectsAddedDuringCallMethod;

			protected abstract void CallMethod(BusinessObject bO);
		}

		sealed class OnFactorySavingCaller : BOMethodCaller
		{
			protected override void CallMethod(BusinessObject bO)
			{
				bO.OnFactorySavingInternal();
			}
		}

		sealed class FactorySavingCaller : BOMethodCaller, IDisposable
		{
			readonly BusinessObjectFactory factory;
			public FactorySavingCaller(BusinessObjectFactory factory)
			{
				this.factory = factory;
				factory.activeSaveMethodCaller = this;
			}

			protected override void CallMethod(BusinessObject bO)
			{ }

			#region IDisposable Members

			public void Dispose()
			{
				if (factory.NotifyOfSaveBeforeAnyFactorySaveBegins())
				{
					for (int i = 0; i < BusinessObjectsAddedDuringCallMethod.Count; i++)
					{
						BusinessObjectsAddedDuringCallMethod[i].OnSavingInternal();
					}
				}
				factory.activeSaveMethodCaller = null;
			}

			#endregion

			internal void FactorySaving()
			{
				if (factory != null)
				{
					factory.OnSaving();
				}
			}
		}

		class OnFactorySavingFetchCaller : BOMethodCaller
		{
			protected override void CallMethod(BusinessObject bO)
			{
				bO.FetchStrategy.FetchForFactorySave();
			}
		}

		class OnFactorySavingFetchBeforeTransactionCaller : BOMethodCaller
		{
			protected override void CallMethod(BusinessObject bO)
			{
				bO.FetchStrategy.FetchForFactorySaveBeforeTransaction();
			}
		}

		class OnFactorySavingBeforeTransactionCaller : BOMethodCaller
		{
			protected override void CallMethod(BusinessObject bO)
			{
				bO.OnFactorySavingBeforeTransaction();
			}
		}

		List<BusinessObject> businessObjectsInOnSavingOrder;
		List<IOnSavingService> servicesAddedDuringOnSaving;
		List<BusinessObject> nonPersistentBusinessObjectsInOnPublishOrder;
		BOMethodCaller activeSaveMethodCaller;

		void CallBusinessObjectsOnSaving()
		{
			try 
			{
				OnSaveDelayer.SetDelayStrategy();
				nonPersistentBusinessObjectsInOnPublishOrder = new List<BusinessObject>();
				foreach (var bizObj in BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess)
				{
					bizObj.OnSavingCalledInternal = false;
					if (bizObj.IsForcePublishForNonPersistentBusinessObject && bizObj.HasChanges)
					{
						nonPersistentBusinessObjectsInOnPublishOrder.Add(bizObj);
					}
				}

				businessObjectsInOnSavingOrder = new List<BusinessObject>();

				bool needAnotherLoop = true;

				while (needAnotherLoop)
				{
					SetRowsShouldBeSaved();
					needAnotherLoop = false;

					var rows = RowFactory.GetModifiedPersistentRowsInSaveOrder().ToList();
					var firstBizos = rows.Select(r => BusinessObjectCache[r]).WhereNotNull().Select(r => r.FirstBizO).ToList();
					var onSavingServices = ServiceContainer.GetOnSavingContainer().GetOnSavingServices(firstBizos);

					if (onSavingServices.Count > 0)
					{
						if (servicesAddedDuringOnSaving == null)
						{
							servicesAddedDuringOnSaving = new List<IOnSavingService>();
						}
						servicesAddedDuringOnSaving.AddRange(onSavingServices);
					}

					var onSavingBizos = rows.SelectMany(r => BusinessObjectCache[r]?.Values ?? Enumerable.Empty<BusinessObject>())
						.Where(b => !b.OnSavingCalledInternal);

					foreach (var service in onSavingServices)
					{
						service.Apply(onSavingBizos);
					}

					foreach (var bizObj in onSavingBizos)
					{
						businessObjectsInOnSavingOrder.Add(bizObj);
						try
						{
							bizObj.OnSavingInternal();
						}
						catch (NotSupportedException ex)
						{
							if (ex.Message.Contains((NoResString)"This is a non-persisted version of a ProcessTaskNotification and should never be saved."))
							{
								ErrorReporter.ReportOnce("JobCompletionTriggerAction_OnSavingHasBeenCalled", FormattableString.Invariant(
$@"JobCompletionTriggerAction is Non-Persistent but OnSaving() hsa been called.
Thread ID:{Thread.CurrentThread.ManagedThreadId}
IsSavedByFactory: {bizObj.IsSavedByFactory}
rows.Contains: {rows.Contains(bizObj.Row)}
BusinessObjectCache[row] == null: {BusinessObjectCache[bizObj.Row] == null}
RowFactory.GetRow() == null: {RowFactory.GetRow(bizObj.Row.Table.TableName, bizObj.PK) == null}
DataUtils.ShouldRowBeSaved():{DataUtils.ShouldRowBeSaved(bizObj.Row)}"));
							}
							throw;
						}
						needAnotherLoop = true;
					}

					if (!needAnotherLoop)
					{
						needAnotherLoop = OnSaveDelayer.RunAllDelayed();
					}
				}
			}
			catch (Exception)
			{
				OnSaveDelayer.MarkDelayActionsAsUnsafe();
				throw;
			}
			finally
			{
				OnSaveDelayer.Dispose();
			}
		}

		public ActionDelayer OnSaveDelayer { get; } = new ActionDelayer();

		protected virtual void CallBusinessObjectsOnFactorySaved(bool saveSucceded)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObjectFactory.CallBusinessObjectsOnFactorySaved", ""))
			{
				var allBizos = BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess;
				int initialCount = allBizos.Count;
				for (int i = 0; i < initialCount; i++)
				{
					if (i < allBizos.Count)
					{
						BusinessObject bizo = allBizos[i];
						if (!bizo.IsDeleted)
						{
							bizo.OnFactorySavedInternal(saveSucceded);
						}
						else if (saveSucceded)
						{
							bizo.ClearHasChanges();
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region IDbConnected Members

		DbConnection IDbConnected.Connection
		{
			get { return RowFactory.DbConnection; }
		}

		#endregion

		#region AccessedPersistentProperties

#if DEBUG
		readonly Dictionary<BusinessObject, bool> IgnoredBusinessObjectsForPersistentValuesDictionary = new Dictionary<BusinessObject, bool>();
		internal void AddIgnoredBusinessObjectForTestingLightValidation(BusinessObject bo)
		{
			IgnoredBusinessObjectsForPersistentValuesDictionary[bo] = true;
		}
		internal void RemoveIgnoredBusinessObjectForTestingLightValidation(BusinessObject bo)
		{
			if (IgnoredBusinessObjectsForPersistentValuesDictionary.ContainsKey(bo))
			{
				IgnoredBusinessObjectsForPersistentValuesDictionary.Remove(bo);
			}
		}

		internal void OnAccessingPersistentValueForTesting(BusinessObject bo, DataColumn column)
		{
			if (AccessingPersistentValueForTesting != null)
			{
				if (!IgnoredBusinessObjectsForPersistentValuesDictionary.ContainsKey(bo))
				{
					AccessingPersistentValueForTesting(bo, column);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event AccessingPersistentValueForTestingDelegate AccessingPersistentValueForTesting;
		public delegate void AccessingPersistentValueForTestingDelegate(BusinessObject bo, DataColumn column);
#endif

		#endregion

		public IFactoryChangeSet GetChanges()
		{
			return new FactoryChangeSet(GetDirtyObjects());
		}

		/// <summary>
		/// Gets the all dirty objects in this factory.
		/// </summary>
		/// <returns>Array of <see cref="BusinessObject"/> instances</returns>
		/// <remarks>
		/// Will return all persistent business objects for which database's state
		/// need to be synchronized with. Includes new, modified and deleted objects.
		/// </remarks>
		ICollection<BusinessObject> GetDirtyObjects()
		{
			List<BusinessObject> result = new List<BusinessObject>();

			foreach (BusinessObject each in BusinessObjectCache.AllBusinessObjectsUnsafeForQuickAccess)
			{
				if (each is NonPersistentBusinessObject)
				{
					continue;
				}

				if (each.Row.RowState == DataRowState.Detached)
				{
					continue;
				}

				if (((IBusiness)each).HasChangesNotIncludingChildren || each.IsDeleted || each.Row.HasVersion(DataRowVersion.Original)) //Mykola suggests IsSavedByFactory() method
				{
					result.Add(each);
				}
				else if (each is ILightValidationInternals && ((ILightValidationInternals)each).IsValidHasChanges)
				{
					result.Add(each);
				}
			}

			return result;
		}

		internal static Type GetConcreteBusinessObjectType(Assembly callingAssembly, Type type)
		{
			if (type == null)
			{
				throw new ArgumentException("type could not be null.");
			}

			var result = type.IsInterface ? GetTypeFromInterface(type, callingAssembly) : type;
			return result ?? throw new ArgumentException("Concrete type for interface " + type.FullName + " could not be found.");
		}

		static Type GetTypeFromInterface(Type type, Assembly callingAssembly)
		{
#pragma warning disable 0612 // the ONLY place this method should be used
			return ObjectFactory.GetType(type, callingAssembly);
#pragma warning restore 0612
		}

		#region BusinessObject Has Change cache

		class HasChangesCached : IDisposable
		{
			public HasChangesCached(BusinessObjectFactory factory)
			{
				this.factory = factory;
				businessObjectHasChangesCache = new Dictionary<ZGuid, bool>();
				factory.HasChangesCachedList.Add(this);
			}
			Dictionary<ZGuid, bool> businessObjectHasChangesCache;
			readonly BusinessObjectFactory factory;

			public void InvalidateHasChangesCache()
			{
				businessObjectHasChangesCache.Clear();
			}

			public bool GetHasChangesFromCached(BusinessObject bizObj)
			{
				if (bizObj == null)
				{
					return false;
				}

				try
				{
					if (!businessObjectHasChangesCache.TryGetValue(bizObj.PK, out bool result))
					{
						result = bizObj.GetHasChangesCore();
						if (!businessObjectHasChangesCache.ContainsKey(bizObj.PK))
						{
							businessObjectHasChangesCache.Add(bizObj.PK, result);
						}
					}
					return result;
				}
				catch (NullReferenceException ex)
				{
					//HasChangesCached got used after Dispose() has been called on it; this can happen due to cross thread access
					ErrorReporter.ReportOnce("HasChangesCached got used after Dispose() has been called on it. Possibly cross thread related: e.g., one thread fires delayed changes using ActiveBusinessObjectCollection.DelayListChangedEvents() at the moment when another thread accesses the HasChanges property of a business object on the same factory. (This should not happen.)", ex);
					return bizObj.GetHasChangesCore();
				}
				catch (IndexOutOfRangeException ex)
				{
					//This can happen when two threads manipulate (Add, TryGetValue) businessObjectHasChangesCache at the same time
					ErrorReporter.ReportOnce("HasChangesCached got used by two threads at the same time. Possibly cross thread related: e.g., two threads try to access the HasChanges property of the same business object. (This should not happen.)", ex);
					return bizObj.GetHasChangesCore();
				}
			}

			public void Dispose()
			{
				factory.HasChangesCachedList.Remove(this);
				businessObjectHasChangesCache = null;
			}
		}

		internal bool IsBusinessObjectHasChangesCached
		{
			get { return HasChangesCachedList.Count > 0; }
		}

		internal void InvalidateHasChangesCache()
		{
			HasChangesCachedList[HasChangesCachedList.Count - 1].InvalidateHasChangesCache();
		}

		public bool GetHasChangesFromCached(BusinessObject bizObj)
		{
			if (!IsBusinessObjectHasChangesCached)
			{
				return false;
			}
			HasChangesCached lastCache;
			try
			{
				lastCache = HasChangesCachedList[HasChangesCachedList.Count - 1];
			}
			catch (ArgumentOutOfRangeException ex)
			{
				ErrorReporter.ReportOnce("GetHasChangesFromCached() got invoked at the same time when another thread tried to remove HasChangesCached from HasChangesCachedList. This can happen when one thread fires delayed changes using ActiveBusinessObjectCollection.DelayListChangedEvents() at the moment when another thread accesses the HasChanges property of a business object on the same factory. (This should not happen.)", ex);
				return false;
			}
			return (lastCache != null && lastCache.GetHasChangesFromCached(bizObj));
		}

		internal IDisposable CacheBusinessObjectHasChanges()
		{
			return new HasChangesCached(this);
		}

		readonly List<HasChangesCached> HasChangesCachedList = new List<HasChangesCached>();

		#endregion

		public bool IsDeactivated
		{
			get;
			private set;
		}

		class TransactionManager : AggregateTransactionManager<BusinessObjectFactory>
		{
			public TransactionManager(BusinessObjectFactory owner, ITransactionManager innerTransactionManager) : base(owner, innerTransactionManager)
			{
			}

			protected override void Commit()
			{
				try
				{
					base.Commit();
					owner.TransactionId = 0;
				}
				catch (ZDataException ex)
				{
					owner.CheckDataSavedButNotUpdated(ex);

					throw new ZSaveException(ex, owner);
				}
			}
		}

		public void DeactivateActiveCollectionsAndCaches()
		{
			DeactivateActiveCollectionsAndCachesCore();
			IsDeactivated = true;
		}

		void DeactivateActiveCollectionsAndCachesCore()
		{
			var indexes = ActiveBusinessObjectCollectionIndexCache.GetInstance(this).All.ToList();
			foreach (var index in indexes)
			{
				index.DeactivateAllOwners();
			}
		}

		#region Helper methods

		/// <summary>
		/// Determines whether the specified object exists in the database only.
		/// </summary>
		public bool ExistsInDatabase(string tableName, ZQuery query)
		{
			RowFactory.IncreaseDatabaseLoadCount(tableName);

			var sql = FormattableString.Invariant($"SELECT TOP(1) RowExists = CONVERT(bit, NULL) FROM {tableName} WHERE {query.ParameterisedText.ParameterisedQueryText}");
			var collection = new DynamicBusinessObjectCollection(this);
			collection.Load(sql, query.Params);

			return collection.Count > 0;
		}

		/// <summary>
		/// Determines whether the specified object exists in the factory / database.
		/// </summary>
		public bool Exists(Type bizOType, ZQuery query, bool mergeDbAndCacheResult = true)
		{
			if (mergeDbAndCacheResult)
			{
				return LoadTop1(bizOType, query) != null;
			}
			else
			{
				var previousFetchOnlyFromLocalCache = query.FetchOnlyFromLocalCache;

				query.FetchOnlyFromLocalCache = true;
				var cachedBizO = LoadTop1(bizOType, query);

				query.FetchOnlyFromLocalCache = previousFetchOnlyFromLocalCache;

				if (cachedBizO == null)
				{
					return ExistsInDatabase(GetTableNameFromType(bizOType), query);
				}

				return true;
			}
		}

		public bool AreCollectionListChangedEventsDelayed => CollectionListChangedSuspender.GetInstance(this).IsDelayerEnabled;

		#endregion

		#region Clean Up

		public bool IsCleanedUp { get; private set; }

		public void CleanUp()
		{
			if (!IsCleanedUp)
			{
				ReleaseSqlLocks();
				DeactivateActiveCollectionsAndCachesCore();
				RefreshEnabled = false;
				IsCleanedUp = true;
			}
		}

		#endregion

		#region For Test Purposes

		partial void HookSaveForUnitTests();
		partial void HookSaveInTransactionForUnitTests();

#if DEBUG

		public int GetTableHitCount(string tableName)
		{
			return RowFactory.GetTableHitCount(tableName).Value;
		}

		public bool IsInNaturalKeyCache_ForTest(Type bizoType, SchemaColumn column, IZType value)
		{
			return BusinessObjectNKCache.Fetch(bizoType, column, value) != null;
		}

#endif
		#endregion
	}

	#region Test
#if DEBUG
	// Tested in CargoWise.EntityFramework.Testing solution
	// BusinessObjectFactoryTest

	public partial class BusinessObjectFactory : ITransactionParticipant, INeedDataSet, IFactory, IFactoryProvider, IDbConnected, ICacheVersionProvider, IExternalFetchHintSupporter
	{
		static readonly Overridable<Action<BusinessObjectFactory>> onFactorySave = new Overridable<Action<BusinessObjectFactory>>(null);
		static readonly Overridable<Action<BusinessObjectFactory>> onFactorySaveInTransaction = new Overridable<Action<BusinessObjectFactory>>(null);

		public static void SetOnFactorySaveHookForTest(Action<BusinessObjectFactory> onSaveHook)
		{
			onFactorySave.Value = onSaveHook;
		}

		public static void SetOnFactorySaveInTransactionForTest(Action<BusinessObjectFactory> onSaveHook)
		{
			onFactorySaveInTransaction.Value = onSaveHook;
		}

		public int PublishCountForLastSave
		{
			get { return refreshManager.PublishCount; }
		}

		public bool SubscriptionFiredForLastSave
		{
			get { return refreshManager.SubscriptionFired; }
		}

		partial void HookSaveForUnitTests()
		{
			onFactorySave.Value?.Invoke(this);
		}

		partial void HookSaveInTransactionForUnitTests()
		{
			onFactorySaveInTransaction.Value?.Invoke(this);
		}
	}
#endif
	#endregion
}
