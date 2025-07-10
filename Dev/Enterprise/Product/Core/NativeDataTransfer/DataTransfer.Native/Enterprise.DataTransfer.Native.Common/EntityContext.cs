using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.DataTransfer.Native.Common.AuditLogs;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Common.Operations;
using Enterprise.DataTransfer.Native.Common.PostUpdateProcesses;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.DataTransfer.Native.Common
{
	public class EntityContext : IEntityContext
	{
		public EntityContext(AncillaryImportServices sessionServices, INativeFactoryProvider factoryProvider)
		{
			RowFactory = new RowFactory(Connection, null);
			ObjectFactory = factoryProvider.GetNewFactory(Connection);

			InterceptorSettings = new List<IInterceptorSetting>();

			Statistics = new StatisticsImpl();

			updateOperation =
				new InterceptedUpdateOperation(this)
				{
					UpdateOperation =
						new UpdateOperation
						{
							EntityRepository = new EntityRepository(this, sessionServices)
						}
				};
		}

		readonly IUpdateOperation updateOperation;

		#region Database Connection

		public DbConnection Connection
		{
			get { return connection ?? (connection = Db.Connection); }
			set { connection = value; }
		}
		DbConnection connection;

		#endregion

		public IList<IInterceptorSetting> InterceptorSettings { get; }

		// just for watch whether InterceptorSettings has been operated, need be removed when the Work Item WI00151691 is fixed
		#region For InterceptorSettings Testing

		public bool IsInterceptorSettingsAdded { get; set; }

		public bool IsInterceptorSettingsCleared { get; set; }

		public StackTrace StackTraceInterceptorSettingsCleared { get; set; }

		#endregion

		public bool AlwaysUseInternalPK
		{ get; set; }

		#region Operation

		/// <summary>
		/// Unit of Work. 
		/// If nothing goes wrong, it would save all changes made with in Update process
		/// Else it would roll back and no changes would be submit
		/// </summary>
		/// <param name="entitySet"></param>
		/// <returns></returns>
		public void Update(IEntitySet entitySet)
		{
			Process(entitySet, false);
		}

		public void Save()
		{
			RowFactory.Save();
			RowFactory = new RowFactory(Connection, null);
		}

		public void Import(IEntitySet entitySet)
		{
			Process(entitySet, true);
		}

		void Process(IEntitySet entitySet, bool addLog)
		{
			using (var manager = Connection.BeginTransactionWithManager())
			{
				try
				{
					updateOperation.Update(entitySet);

					new PostUpdateProcessManager(this).Process(entitySet.Root);

					if (addLog)
					{
						var rowRepository = new RowRepository(Connection, RowFactory);
						new AuditLogger(rowRepository).LogAction(entitySet.Root, Events.DataImportCode);
						rowRepository.Save();
					}

					RefreshBusinessObjects();
					ObjectFactory.Save();
					manager.CommitTransaction();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ClearImportStatistics();
					try
					{
						manager.RollbackTransaction();
					}
					catch (Exception ex2) when (!ex2.IsCriticalException())
					{
						// Could not rollback transaction but we're crashing so just leave it be.
					}

					if (ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex) != null)
					{
						throw;
					}
					else
					{
						var innerSqlException = ex.InnerException as SqlException;
						var dbErrorMatch = innerSqlException != null ? new DbErrorMatch(innerSqlException) : null;

						if (dbErrorMatch?.ExceptionType == DbErrorType.InsertConflictedWithCheckConstraint
							|| dbErrorMatch?.ExceptionType == DbErrorType.UpdateConflictedWithCheckConstraint
							|| dbErrorMatch?.ExceptionType == DbErrorType.InsertConflictedWithForeignKey
							|| dbErrorMatch?.ExceptionType == DbErrorType.UpdateConflictedWithForeignKey
							|| dbErrorMatch?.ExceptionType == DbErrorType.DeleteConflictedWithForeignKey)
						{
							throw new NativeXMLUserVisibleException(dbErrorMatch.GetUserFriendlyMessage(connection));
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		void RefreshBusinessObjects()
		{
			var entity = Statistics.EntityAffected.FirstOrDefault();
			if (entity == null || entity.Action != DBEntity.DbAction.Insert || !WorkflowSupportableTableNames.Instance.GetTableNames().Contains(entity.Name))
			{
				return;
			}

			var businessObject = ObjectFactory.Load(CargoWise.Application.ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(entity.Name), entity.PK);
			if (businessObject is IWorkflowProvider)
			{
				businessObject.HasChanges = true;
			}
		}

		#endregion

		#region DataFactoryProvider

		public RowFactory RowFactory { get; private set; }

		public BusinessObjectFactory ObjectFactory { get; }

		public RowFactory GetOrCreateBehaviourRowFactorySavedFirst()
		{
			if (BehaviourRowFactorySavedFirst == null)
			{
				BehaviourRowFactorySavedFirst = new RowFactory(Connection, null);
#if DEBUG
				BehaviourRowFactorySavedFirst.EnableTableHitQueryCollection(RowFactory.TableNamesNeedingHitQueryCollection.ToArray());
#endif
			}
			return BehaviourRowFactorySavedFirst;
		}

		/// <summary>
		/// The BehaviourRowFactory if any.
		/// Created when GetOrCreateBehaviourRowFactorySavedFirst() is first called
		/// </summary>
		public RowFactory BehaviourRowFactorySavedFirst { get; private set; }

		#endregion

		public StatisticsImpl Statistics { get; }

		public void Dispose()
		{
			ClearImportStatistics();
		}

		void ClearImportStatistics()
		{
			InterceptorSettings.Clear();
			IsInterceptorSettingsCleared = true;
			StackTraceInterceptorSettingsCleared = new StackTrace();
			Statistics.Clear();
		}
	}
}
