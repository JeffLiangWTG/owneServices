using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture
{
	[MetadataContext(Enterprise.Metadata.Integration.MetadataContext.EnterpriseBusinessObject)]
	public abstract class EnterpriseBusinessObject : BusinessObject, IStmNoteParent, IStmALogParent, IAutoAdminLogTarget, IUpdateAuditFields
	{
		protected EnterpriseBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (factory.IsAfterOnSavingButBeforeCommit)
			{
				BusinessObjectLoggingStrategy.UpdateEditAndCreateLogFields(this);
			}
		}

		static EnterpriseBusinessObject()
		{
			ZMetaDataTypes.RegisterTypes();
		}

		protected sealed override void RegisterCustomBusinessObjectAsChild()
		{
			if (RegisterCustomBizoAsChild && this is ICustomFieldProvider customFieldProvider)
			{
				var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
				if (customBusinessObject != null)
				{
					RegisterEditableChildObject(customBusinessObject);
				}
			}
		}

		protected virtual bool RegisterCustomBizoAsChild => false;

		#region PopulateNumberPropertyIfRequired

		/// <summary>
		/// Set a job number property from a fountain.
		/// Should be called from every call to OnSaving if this is not in the database unless specified otherwise.
		/// Will set a new number if this is called again from a different transaction, to prevent using a stale number that another record will also use.
		/// Will not set a new number if this is called additional times from the same transaction, to prevent using up more than one number.
		/// Supports being called for multiple properties if there are multiple kinds of job number on the record.
		/// Does nothing if the value has already been populated by some other code.
		/// </summary>
		protected void PopulateNumberPropertyIfRequired<T>(ZPropertyInfo prop, Func<BusinessObjectFactory, T> calculateValue, bool ignoreInDatabaseCheck = false, bool forceRegenerate = false)
			where T : IZType
		{
			PopulateNumberPropertyIfRequired(new[] { prop }, (factory, props) => new[] { calculateValue(Factory) }, ignoreInDatabaseCheck, forceRegenerate);
		}

		protected static void PopulateNumberPropertyIfRequired<T>(ZPropertyInfo[] props, Func<BusinessObjectFactory, ZPropertyInfo[], IList<T>> calculateValues, bool ignoreInDatabaseCheck = false, bool forceRegenerate = false)
			where T : IZType
		{
			if (props.Any())
			{
				var factory = props.First().BizObj.Factory;
				var propertiesToPopulate = GetPropertiesToPopulate(props, factory, ignoreInDatabaseCheck, forceRegenerate);
				if (propertiesToPopulate.Length > 0)
				{
					var values = calculateValues(factory, propertiesToPopulate);

					if (values.Count != propertiesToPopulate.Length)
					{
						ErrorReporter.ReportOnce($"Number of values returned by 'calculateValues' method is unexpected({values.Count} returned but {propertiesToPopulate.Length} required).");
					}
					else
					{
						for (var i = 0; i < values.Count; i++)
						{
							var value = values[i];
							((EnterpriseBusinessObject)propertiesToPopulate[i].BizObj).propertyNameToTransactionData
								.Value
									[propertiesToPopulate[i].Name] = (value, factory.TopLevelTransactionsBegunCount);
							propertiesToPopulate[i].Value = value;
						}
					}
				}
			}
		}

		protected void PopulateFormattedNumberPropertyIfRequired(ZPropertyInfo prop, INumberFountainProxy fountain, bool ignoreInDatabaseCheck = false, bool forceRegenerate = false)
		{
			PopulateNumberPropertyIfRequired(prop, objectFactory => new ZString(fountain.GetNextFormatted(objectFactory)), ignoreInDatabaseCheck, forceRegenerate);
		}

		protected static void PopulateFormattedNumberPropertyIfRequired(ZPropertyInfo[] props, INumberFountainProxy fountain, Func<IList<string>, BusinessObjectFactory, IList<string>> getDuplicatedNumbers, bool ignoreInDatabaseCheck = false, bool forceRegenerate = false, int numberPerBatch = 500)
		{
			PopulateNumberPropertyIfRequired(props,
				(factory, properties) => GetUniqueNumbersFromNumberFountain(fountain, factory, properties.Length, numberPerBatch, getDuplicatedNumbers),
				ignoreInDatabaseCheck, forceRegenerate);
		}

		static ZPropertyInfo[] GetPropertiesToPopulate(ZPropertyInfo[] props, BusinessObjectFactory factory, bool ignoreInDatabaseCheck = false, bool forceRegenerate = false)
		{
			var result = new List<ZPropertyInfo>();
			foreach (var prop in props)
			{
				if ((ignoreInDatabaseCheck && prop.OriginalValue.IsEmpty) || !prop.BizObj.IsInDatabase || forceRegenerate)
				{
					var currentValue = prop.Value;
					if (forceRegenerate || currentValue.IsEmpty ||
						(
							((EnterpriseBusinessObject)prop.BizObj).propertyNameToTransactionData.Value.TryGetValue(prop.Name, out var transactionData) &&
							transactionData.number.Equals(currentValue) &&
							transactionData.count != factory.TopLevelTransactionsBegunCount
						))
					{
						result.Add(prop);
					}
				}
			}
			return result.ToArray();
		}

		static IList<ZString> GetUniqueNumbersFromNumberFountain(INumberFountainProxy fountain, BusinessObjectFactory factory, int requiredCount, int numbersPerBatch, Func<IList<string>, BusinessObjectFactory, IList<string>> getDuplicatedNumbers)
		{
			var matchingKeys = new List<ZString>();
			if (requiredCount > 0)
			{
				var newKeyNeededCount = Math.Min(numbersPerBatch, requiredCount);
				do
				{
					var newKeys = fountain.GetNextsFormatted(factory, newKeyNeededCount).ToList();
					var duplicatedKeys = getDuplicatedNumbers(newKeys, factory);
					duplicatedKeys.ForEach(key => newKeys.Remove(key));
					newKeys.ForEach((key) => matchingKeys.Add(key));
					newKeyNeededCount = Math.Min(numbersPerBatch, requiredCount - matchingKeys.Count);
				} while (newKeyNeededCount > 0);
			}

			return matchingKeys;
		}

		readonly Lazy<IDictionary<string, (IZType number, long count)>> propertyNameToTransactionData =
			new Lazy<IDictionary<string, (IZType number, long count)>>(() => new Dictionary<string, (IZType number, long count)>());

		#endregion

		#region HasChangesInAuditDetails

		DataColumn[] AuditDetails
		{
			get
			{
				if (auditDetails == null)
				{
					var row = ((INeedRow)this).Row;
					var table = row != null ? row.Table : null;

					if (table != null)
					{
						auditDetails = new DataColumn[]
						{
							table.Columns[TablePrefix + "_" + AuditDetailsColumns.SystemLastEditUser],
							table.Columns[TablePrefix + "_" + AuditDetailsColumns.SystemLastEditTimeUtc],
							table.Columns[TablePrefix + "_" + AuditDetailsColumns.SystemCreateUser],
							table.Columns[TablePrefix + "_" + AuditDetailsColumns.SystemCreateTimeUtc]
						};
					}
				}
				return auditDetails;
			}
		}
		DataColumn[] auditDetails;

		public override bool HasChangesInAuditDetails
		{
			get
			{
				return !IsDeleted && ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges && AuditDetails.Where(column => column != null).Any(column =>
				{
					object currentValue = ((IBusinessObjectInternals)this).GetValueFromRowSafely(column, DataRowVersion.Current);
					return currentValue != null && !currentValue.Equals(((IBusinessObjectInternals)this).GetColumnOriginalValue(column.ColumnName));
				});
			}
		}

		#endregion

		#region Fetch Strategy

		protected sealed override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return GetFetchStrategyCore();
		}

		protected virtual EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new EnterpriseBusinessObjectFetchStrategy(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				if (SupportsNotes)
				{
					if (IsInDatabase || fNotes != null)
					{
						Notes.RemoveAndDeleteAll();
					}

					var additionalNotes = AdditionalNotesProvider?.AdditionalNotes;
					if (additionalNotes != null)
					{
						foreach (var note in additionalNotes)
						{
							var noteWillBeAutoDeletedAnyway = note.ST_NoteDataAsText.IsEmpty;
							if (!noteWillBeAutoDeletedAnyway)
							{
								note.Delete();
							}
						}
					}
				}

				if (!IsInDatabase && !(this is StmALog))
				{
					var query = new ZQuery(StmALogSchema.SL_Parent, PK);
					query.FetchOnlyFromLocalCache = true;
					var logsInMemory = Factory.Load<StmALog>(query).Where(l => !l.IsInDatabase);
					logsInMemory.DeleteAll();
				}
			}
			base.Delete();
		}

		protected internal IAdditionalNoteProvider AdditionalNotesProvider => additionalNoteProvider ?? (additionalNoteProvider = GetAdditionalNoteProvider());
		IAdditionalNoteProvider additionalNoteProvider;

		protected virtual IAdditionalNoteProvider GetAdditionalNoteProvider()
		{
			if (!SupportsNotes)
			{
				throw new InvalidOperationException(GetType().Name + " does not support notes");
			}

			return ObjectFactory.New<IAdditionalNoteProvider>(this);
		}

		#endregion

		#region Metadata

		public IMetadata Metadata
		{
			get
			{
				if (!this.IsDeleted && (metadata == null || this.HasChangesNotIncludingChildren))
				{
					metadata = NewMetadata();
				}

				return metadata;
			}
		}

		IMetadata metadata;

		IMetadata NewMetadata()
		{
			return ObjectFactory.Get<IMetadataProvider>().GetMetadataFromBO(this);
		}

		Type EnterpriseBusinessObjectMetadataType
		{
			get { return ObjectFactory.Get<IEnterpriseBusinessObjectMetadata>().GetType(); }
		}

		bool GetIsMetadataNullOrEnterpriseBusinessObjectMetadataType(IMetadata tempMetadata)
		{
			return tempMetadata == null || tempMetadata.GetType() == EnterpriseBusinessObjectMetadataType;
		}

		#endregion

		#region IStmNoteParent Members

		[BusinessObjectTestExclude]
		public virtual Notes Notes
		{
			get { return fNotes ?? (fNotes = new Notes(this)); }
		}

		ZGuid IStmNoteParent.NotesParentPK
		{
			get { return PK; }
		}

		string IStmNoteParent.NotesParentTableName
		{
			get { return TableName; }
		}

		protected internal virtual BusinessObjectFactory NotesFactory
		{
			get { return Factory; }
		}

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return NotesFactory; }
		}

		GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate
		{
			get { return CustomNoteTypesDelegate; }
			set { CustomNoteTypesDelegate = value; }
		}
		GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate;

		/// <summary>
		/// Indicates whether or not notes are accessible for the business object (by default, all business objects support notes).
		/// Objects that do not support notes, such as those objects that do not reside
		/// in a database containing StmNote should override and return false.
		/// </summary>
		public virtual bool SupportsNotes
		{
			get { return true; }
		}

		/// <summary>
		/// Override this method and return each related Business Object for which you want to see Notes (on the Notes tab).
		/// For example, return new BusinessObject[] { Consol, PackLine }
		/// </summary>
		[BusinessObjectTestExclude]
		public virtual BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get { return Array.Empty<EnterpriseBusinessObject>(); }
		}

		/// <summary>
		/// Override this method and return the StmNoteContexts for which you want to see Related Notes (on the Notes tab).
		/// You can combine these enums using a Bitwise OR, for example, return StmNoteContexts.Module.S | StmNoteContexts.Module.F | StmNoteContexts.Module.D.
		/// Note that you do not need to include StmNoteContexts.Module.A, StmNoteContexts.Direction.A and StmNoteContexts.FreightMode.A as it is included by default.
		/// </summary>
		protected internal virtual StmNoteContexts NoteContextsForRelatedNotes
		{
			get { return StmNoteContexts.Default; }
		}

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get { return NoteContextsForRelatedNotes; }
		}

		protected virtual NoteTypeCollection NoteTypesCore
		{
			get { return new NoteTypeCollection(); }
		}

		/// <summary>
		/// Determines which predefinted notes are available to this business object. When bound to the Notes tab the grid drop list for "Description" will contain these.
		/// </summary>
		public NoteTypeCollection NoteTypes
		{
			get
			{
				NoteTypeCollection result = new NoteTypeCollection();

				var tempMetaData = Metadata;
				if (GetIsMetadataNullOrEnterpriseBusinessObjectMetadataType(tempMetaData))
				{
					result.Add(GetNoteTypesWithoutUsingMetadata());
				}
				else
				{
					result.Add((NoteTypeCollection)tempMetaData.NoteTypes);
				}

				if (CustomNoteTypes != null)
				{
					result.Add(CustomNoteTypes);
				}

				return result;
			}
		}

		public NoteTypeCollection GetNoteTypesWithoutUsingMetadata()
		{
			return NoteTypesCore;
		}

		NoteTypeCollection CustomNoteTypes
		{
			get
			{
				if (customNoteTypes == null && CustomNoteTypesDelegate != null)
				{
					customNoteTypes = CustomNoteTypesDelegate();
				}

				return customNoteTypes;
			}
		}

		#endregion

		#region IStmALogParent Members

		/// <summary>
		/// Override this method and return each related Business Object for which you want provide events that can trigger workflow.
		/// For example, return new BusinessObject[] { Shipment, Container, PackLine[0], PackLine[1] }
		/// </summary>
		/// <remarks>
		///	Please be aware that this can scale very poorly when there are many child objects, particularly for deeper nesting.
		///	Only use if it is absolutely necessary to have the event propogate from child to parent.
		///	See CS00843817 for an example of how bad things can get.
		/// </remarks>
		public BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get
			{
				if (useBusinessObjectsWithRelatedEventsCache)
				{
					return businessObjectsWithRelatedEventsCache ?? (businessObjectsWithRelatedEventsCache = GetNonCachedBusinessObjectsWithRelatedEvents());
				}
				else
				{
					return GetNonCachedBusinessObjectsWithRelatedEvents();
				}
			}
		}
		BusinessObject[] businessObjectsWithRelatedEventsCache;
		bool useBusinessObjectsWithRelatedEventsCache;

		public IDisposable CacheBusinessObjectsWithRelatedEvents()
		{
			return new DisposableAction(
				() => { useBusinessObjectsWithRelatedEventsCache = true; },
				() => { useBusinessObjectsWithRelatedEventsCache = false; businessObjectsWithRelatedEventsCache = null; });
		}

		BusinessObject[] GetNonCachedBusinessObjectsWithRelatedEvents()
		{
			var businessObjects = new List<BusinessObject>();
			var relatedBizoProviders = ObjectFactory.Get<IEnumerable>("BusinessObjectsWithRelatedEventsProviders");

			foreach (IRelatedBusinessObjectProvider provider in relatedBizoProviders)
			{
				businessObjects.AddRange(provider.GetRelatedBusinessObjects(this));
			}

			businessObjects.AddRange(BusinessObjectsWithRelatedEventsCore);

			return businessObjects.ToArray();
		}

		protected virtual BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		public Logs Logs
		{
			get { return logs ?? (logs = GetNewLogs()); }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return GetLogsParentTableName(); }
		}

		protected virtual string GetLogsParentTableName()
		{
			return TableName;
		}

		protected internal virtual string GetDataVersionLogsTablePrefix() => TablePrefix;

		protected virtual Logs GetNewLogs()
		{
			return new Logs(this);
		}

		protected virtual BusinessObjectFactory LogsFactory
		{
			get { return Factory; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return LogsFactory; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
			ProcessLogCore(log);
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return DeferFiringWorkflowCore; }
		}

		#endregion

		#region IAutoAdminLogTarget Members

		protected virtual AutologState AutoLoggingState => AutologState.NotLogged;

		public bool IsAutoLogged => AutoLoggingState == AutologState.AutoLogged;

		/// <summary>
		/// NotLogged -  AutoAdminBusinessObjectLogger will NOT create ADD/EDT/DEL events on Audit Changes<br/>
		/// AutoLogged - AutoAdminBusinessObjectLogger will create ADD/EDT/DEL events on Audit Changes, saved to the DB<br/>
		/// AutoLoggedToQueueOnly - AutoAdminBusinessObjectLogger will create ADD/EDT/DEL events on Audit Changes, NOT saved to the DB. Logs are inserted into StmALogQueue table for processing by LogWalker.<br/>
		/// </summary>
		public enum AutologState
		{
			NotLogged,
			AutoLoggedToQueueOnly,
			AutoLogged
		}

		bool IAutoLog.IsAutoLogOnlyEnabledForACT => IsAutoLogOnlyEnabledForACT;

		protected virtual bool IsAutoLogOnlyEnabledForACT => false;

		public bool IsAutoAdminBusinessObjectLoggerEnabled
			=> ObjectFactory.Get<IAuditStmALogDecider>().IsAutoAdminBusinessObjectLoggerEnabled(this, AutoLoggingState);

		void IAutoAdminLogTarget.OnCreateAutoAdminLog()
		{
			OnCreateAutoAdminLog();
		}

		protected virtual void OnCreateAutoAdminLog()
		{
		}

		ZString IAutoAdminLogTarget.CustomLogReferenceSuffix
		{
			get { return CustomLogReferenceSuffix; }
		}

		protected virtual ZString CustomLogReferenceSuffix
		{
			get { return ""; }
		}

		bool IAutoAdminLogTarget.ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return ShouldCreateAutoLogIfOnlyChildrenHaveChanges; }
		}

		protected virtual bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => (IsTopLevel || CreateAutoLogIfOnlyChildrenHaveChanges) && IsAutoAdminBusinessObjectLoggerEnabled;

		#endregion

		#region IUpdateAuditFields Members

		bool IUpdateAuditFields.ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges
		{
			get { return ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges; }
		}

		protected virtual bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => IsTopLevel || UpdateAuditFieldsIfOnlyChildrenHaveChanges;

		#endregion

		#region Implementation

		[NonSerialized]
		protected Notes fNotes;
		NoteTypeCollection customNoteTypes;
		Logs logs;

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			return new IBusinessObjectStrategy[] { new BusinessObjectLoggingStrategy(this) };
		}

		protected virtual void ProcessLogCore(IStmALog log)
		{
		}

		protected virtual bool DeferFiringWorkflowCore
		{
			get { return false; }
		}

		#endregion

		public enum LoggingAction
		{
			Attach,
			Detach
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log text prefix")]
		public virtual void AddRelationshipLog(LoggingAction action, ZString code, ZString desc, bool shouldAddLog = true)
		{
			const string logTextFormat = "{0} - ({1}) {2}";
			var newLogSuffix = (action == LoggingAction.Attach ? "Attached" : "Detached");
			var removeLogSuffix = (action == LoggingAction.Attach ? "Detached" : "Attached");

			var logTextToDelete = ZString.Format(logTextFormat, removeLogSuffix, code, desc);
			var logTextToAdd = ZString.Format(logTextFormat, newLogSuffix, code, desc);
			this.AddRelationshipLog(logTextToDelete, logTextToAdd, shouldAddLog);
		}

		public virtual void AddRelationshipLog(ZString logTextToDelete, ZString logTextToAdd, bool shouldAddLog = true)
		{
			var logToDelete = this.Logs.LogsNotInDB.Where(log => log.SL_Reference == logTextToDelete).OrderByDescending(log => log.SL_EventTime).FirstOrDefault();
			if (logToDelete != null)
			{
				logToDelete.Delete();
			}
			else if (shouldAddLog)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				this.Logs.AddNew(Events.EditedARecord, logTextToAdd);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				//also delete empty EDT log so we don't have double logs
				var emptyEDTLog = this.Logs.LogsNotInDB.Where(log => log.SL_SE_NKEvent == AutoEvents.EditedARecordCode && string.IsNullOrEmpty(log.SL_Reference)).OrderByDescending(log => log.SL_EventTime).FirstOrDefault();
				if (emptyEDTLog != null)
				{
					emptyEDTLog.Delete();
				}
			}
		}

		protected virtual bool CreateAutoLogIfOnlyChildrenHaveChanges => false;
		protected virtual bool UpdateAuditFieldsIfOnlyChildrenHaveChanges => false;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (this is IClusterKeyWorker clusterKeyWorkerObj)
			{
				ClusterKeyManager.LoadAndFlagDescendantsForSavingIfKeyCascadingNeeded(clusterKeyWorkerObj);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (this is IClusterKeyEntity clusterKeyObj && !(this is IOptionalClusterKeyEntity optionalClusterKeyObj && !optionalClusterKeyObj.UseClusterKey))
			{
				ClusterKeyManager.SetClusterKeyIfNeeded(clusterKeyObj);
			}
		}

		internal long? LastClusterKeySetTransactionId { get; set; }
	}
}
