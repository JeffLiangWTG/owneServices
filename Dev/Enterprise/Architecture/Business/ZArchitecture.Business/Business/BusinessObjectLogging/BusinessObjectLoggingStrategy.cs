using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	class BusinessObjectLoggingStrategy : IBusinessObjectStrategy, ISavingFetchStrategy
	{
		public BusinessObjectLoggingStrategy(BusinessObject businessObject) : this(businessObject, BusinessObjectLoggerFactory.GetLoggers)
		{
			SchemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
		}

		IApplicationSchemaResolver SchemaResolver { get; }

		internal BusinessObjectLoggingStrategy(BusinessObject businessObject, Func<BusinessObject, IEnumerable<IBusinessObjectLogger>> getLoggers)
		{
			if (getLoggers is null)
			{
				throw new ArgumentNullException(nameof(getLoggers), "delegate providing loggers must not be null");
			}

			this.getLoggers = getLoggers;
			businessObjectLoggers = new Lazy<List<IBusinessObjectLogger>>(() => getLoggers(businessObject).ToList(), false);
		}

		public void OnSaving(BusinessObject businessObject)
		{
			if (businessObject is IAutoAdminLogTarget)
			{
				if (businessObjectLoggers.Value.Any(l => l.RunAfterOnSavingForAllBizos))
				{
					var factory = businessObject.Factory;

					var businessObjectLoggingService = factory.ServiceContainer.GetAfterOnSavingService<BusinessObjectLoggingService>();

					if (businessObjectLoggingService == null)
					{
						factory.ServiceContainer.AddAfterOnSavingService(new BusinessObjectLoggingService(getLoggers));
					}
				}

				foreach (var logger in businessObjectLoggers.Value.Where(l => !l.RunAfterOnSavingForAllBizos))
				{
					logger.CreateSaveLog(businessObject);
				}
			}

			if (businessObject is IUpdateAuditFields)
			{
				UpdateEditAndCreateLogFields(businessObject);
			}
		}

		public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
		{
			if (!businessObject.IsDeleted)
			{
				UpdateEditAndCreateLogFields(businessObject);
			}
		}

		public void OnSaved(BusinessObject businessObject, bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				OnSaveRollback(businessObject);
			}

			foreach (var logger in businessObjectLoggers.Value)
			{
				logger.OnSaved(businessObject, saveSucceeded);
			}
		}

		public void OnFactorySaving(BusinessObject businessObject)
		{
			if (!ShouldLogIfOnlyChildrenChanged(businessObject))
			{
				return;
			}

			if (ShouldCreateAutoLogIfOnlyChildrenChanged(businessObject))
			{
				foreach (var logger in businessObjectLoggers.Value)
				{
					logger.CreateModifiedSaveLog(businessObject);
				}
			}

			if (ShouldUpdateAuditFieldsIfOnlyChildrenChanged(businessObject))
			{
				UpdateEditAndCreateLogFields(businessObject);
			}
		}

		public bool ShouldCreateAutoLogIfOnlyChildrenChanged(BusinessObject loggingBizo)
		{
			if (!(loggingBizo is IAutoAdminLogTarget logTarget))
			{
				return false;
			}

			if (logTarget.ShouldCreateAutoLogIfOnlyChildrenHaveChanges)
			{
				return true;
			}

			return ((IBusiness)loggingBizo).Children.Any(child => child is IPartOfParentChangesForAdminLogging and BusinessObject bizo && bizo.IsSavedByFactory && (bizo.IsRowChanged || !bizo.IsInDatabase));
		}

		public bool ShouldUpdateAuditFieldsIfOnlyChildrenChanged(BusinessObject businessObject) => businessObject is IUpdateAuditFields auditTarget && auditTarget.ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges;

		public void OnSaveRollback(BusinessObject businessObject)
		{
			foreach (var logger in businessObjectLoggers.Value)
			{
				logger.RemoveLog(businessObject);
			}
		}

		public void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
			// Data row can be already deleted if there are multiple business objects around one row.
			if (businessObject != null && !businessObject.IsRefreshingByDataRefreshBus && !businessObject.IsDeleted)
			{
				foreach (var logger in businessObjectLoggers.Value)
				{
					logger.CreateDeleteLog(businessObject);
				}
			}
		}

		bool ShouldLogIfOnlyChildrenChanged(BusinessObject loggingBizo)
		{
			if (!ShouldCreateAutoLogIfOnlyChildrenChanged(loggingBizo) && !ShouldUpdateAuditFieldsIfOnlyChildrenChanged(loggingBizo))
			{
				return false;
			}

			if (!DoChildrenHaveChangesForAdminLogging(loggingBizo))
			{
				return false;
			}

			if (!((IBusinessObjectState)loggingBizo).HasChangesNotIncludingChildren)
			{
				return true;
			}

			return loggingBizo.IsInDatabase
				&& !ZDataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(SchemaResolver, ((INeedRow)loggingBizo).Row);
		}

		bool DoChildrenHaveChangesForAdminLogging(BusinessObject loggingBizo)
		{
			if (((IBusinessObjectInternals)loggingBizo).DoChildrenHaveChanges())
			{
				return true;
			}

			if (HasChildrenNotInDatabaseAndSavedByFactory(loggingBizo))
			{
				return true;
			}

			return false;
		}

		bool HasChildrenNotInDatabaseAndSavedByFactory(IBusiness parent)
		{
			foreach (var child in parent.Children)
			{
				if (child is IForceLogsOnParentForNewWithNoHasChanges and BusinessObject { IsInDatabase: false, IsDeleted: false, IsSavedByFactory: true })
				{
					return true;
				}

				if (HasChildrenNotInDatabaseAndSavedByFactory(child))
				{
					return true;
				}
			}

			return false;
		}

		void ISavingFetchStrategy.FetchForSaving(BusinessObject businessObject)
		{
			if (businessObject.IsDeleted)
			{
				return;
			}

			foreach (var logger in businessObjectLoggers.Value)
			{
				logger.FetchForSaving(businessObject);
			}
		}

		internal static void UpdateEditAndCreateLogFields(BusinessObject businessObject)
		{
			if (((INeedRow)businessObject)?.Row == null || businessObject.ShouldSkipUpdateAuditColumns)
			{
				return;
			}

			var tablePrefix = businessObject.TablePrefix;

			if (string.IsNullOrEmpty(tablePrefix))
			{
				return;
			}

			using (businessObject.SuspendSettingHasChanges())
			{
				tablePrefix += "_";

				var createOrEditTime = businessObject.Factory.TransactionStartedTimeUtc;
				var userCode = StaticCurrentFetcher.Instance.CurrentUserCode;
				using (((ISingleElementListInternal)businessObject).SuspendListChanged())
				{
					if (!businessObject.IsInDatabase)
					{
						var createTimePropertyName = tablePrefix + AuditDetailsColumns.SystemCreateTimeUtc;
						businessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(createTimePropertyName, createOrEditTime, true);

						var createUserPropertyName = tablePrefix + AuditDetailsColumns.SystemCreateUser;
						businessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(createUserPropertyName, userCode, true);

						var createBranchPropertyName = tablePrefix + AuditDetailsColumns.SystemCreateBranch;
						using (EnvProxy.Instance.SuspendBranchAccessError())
						{
							businessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(createBranchPropertyName, StaticCurrentFetcher.Instance.CurrentBranchCode, true);
						}

						var createDepartmentPropertyName = tablePrefix + AuditDetailsColumns.SystemCreateDepartment;
						businessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(createDepartmentPropertyName, StaticCurrentFetcher.Instance.CurrentDepartmentCode, true);
					}

					if (!businessObject.IsDeleted)
					{
						var lastEditTimePropertyName = tablePrefix + AuditDetailsColumns.SystemLastEditTimeUtc;
						businessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(lastEditTimePropertyName, createOrEditTime, false);

						var lastEditUserPropertyName = tablePrefix + AuditDetailsColumns.SystemLastEditUser;
						businessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(lastEditUserPropertyName, userCode, false);
					}
				}

				businessObject.RefreshBinding();
			}
		}

		readonly Lazy<List<IBusinessObjectLogger>> businessObjectLoggers;
		readonly Func<BusinessObject, IEnumerable<IBusinessObjectLogger>> getLoggers;

		#region Unused

		void IBusinessObjectStrategy.FetchForLoad(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnDelete(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}
		DeleteDetails IBusinessObjectStrategy.DeleteDetails(BusinessObject businessObject)
		{
			return null;
		}

		#endregion
	}
}
