using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Integration;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class ObjectChangeSet : IObjectChangeSet
	{
		readonly BusinessObject sessionInstance;
		readonly BusinessObject databaseInstance;
		readonly IApplicationSchemaResolver schemaResolver;

		readonly List<IPropertyRecord> records = new List<IPropertyRecord>();

		public ObjectChangeSet(BusinessObject sessionInstance, BusinessObject databaseInstance, IApplicationSchemaResolver schemaResolver)
		{
			this.sessionInstance = sessionInstance;
			this.databaseInstance = databaseInstance;
			this.schemaResolver = schemaResolver;
		}

		public bool IsExistsInDatabase
		{
			get { return databaseInstance != null; }
		}

		public bool IsModifiedInDatabase
		{
			get
			{
				CheckDatabaseInstanceExists();
				return records.Count > 0;
			}
		}

		public string DisplayName
		{
			get { return SessionInstance.HumanReadableName; }
		}

		public BusinessObject SessionInstance
		{
			get { return sessionInstance; }
		}

		public BusinessObject DatabaseInstance
		{
			get { return databaseInstance; }
		}

		public List<IPropertyRecord> NonMergeableProperties
		{
			get
			{
				CheckDatabaseInstanceExists();
				return records.FindAll(property => !property.IsMergeAllowed);
			}
		}

		public List<IPropertyRecord> MergeableProperties
		{
			get
			{
				CheckDatabaseInstanceExists();
				return records.FindAll(property => property.IsMergeAllowed);
			}
		}

		internal bool IsAutoLogged => sessionInstance is IAutoLog logTarget && logTarget.IsAutoAdminBusinessObjectLoggerEnabled;

		public string LastModified
		{
			get { return IsExistsInDatabase ? ZDataUtils.GetUsernameAndTimeOfLastModification(databaseInstance.Row, IsAutoLogged) : string.Empty; }
		}

		public virtual bool CanMerge()
		{
			CheckDatabaseInstanceExists();
			return NonMergeableProperties.Count == 0;
		}

		public virtual void Merge()
		{
			CheckDatabaseInstanceExists();

			if (!CanMerge())
			{
				throw new Exception("Can not proceed with merge. Object contains changes in critical fields");
			}

			AcceptDatabaseValues();

			foreach (IPropertyRecord record in records)
			{
				record.Merge();
			}
		}

		public void Delete()
		{
			((IBusiness)sessionInstance).DeleteForDataRefresh();
			if (sessionInstance.Row.RowState != DataRowState.Detached && sessionInstance.IsDeleted)
			{
				sessionInstance.Row.AcceptChanges();
			}
		}

		internal void Populate()
		{
			CheckDatabaseInstanceExists();

			foreach (DataColumn column in SessionInstance.Row.Table.Columns)
			{
				ZPropertyInfo info = sessionInstance.FindPropertyInfo(column.ColumnName);

				if (info == null && sessionInstance is ILightValidationInternals lightValidationInternals && lightValidationInternals.IsValidSchemaColumn.Name == column.ColumnName)
				{
					var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ILightValidationInternals))[nameof(ILightValidationInternals.IsValid)];
					info = new ZPropertyInfo(sessionInstance, column.ColumnName, propertyDescriptor);
				}

				if (info != null)
				{
					var rowLastModified = new Lazy<string>(() => LastModified);
					var property = new PropertyRecord(info, column.ColumnName, sessionInstance.Row, databaseInstance.Row, rowLastModified);

					var schemaColumn = schemaResolver.GetSchemaColumnSafe(column.ColumnName, SessionInstance.TableName);
					if (schemaColumn != null && schemaColumn.IsLargeBinaryOrText
						&& !LazyLoading.LoadRequired(property.CurrentValue)
						&& LazyLoading.LoadRequired(property.DatabaseValue))
					{
						((IBusinessObjectInternals)databaseInstance).EnsureBlobField(schemaColumn);
						property = new PropertyRecord(info, column.ColumnName, sessionInstance.Row, databaseInstance.Row, rowLastModified);
					}

					if (property.HasChangedInDatabase)
					{
						records.Add(property);
					}
				}
			}
		}

		void AcceptDatabaseValues()
		{
			Hashtable rebuildChanges = null;
			if (sessionInstance.Row.HasVersion(DataRowVersion.Original))
			{
				rebuildChanges = ZSqlLoader.StoreCurrentChanges(sessionInstance.Row);
				sessionInstance.Row.RejectChanges();
			}

			foreach (IPropertyRecord record in records)
			{
				sessionInstance.Row[record.ColumnName] = record.DatabaseValue;
				if (rebuildChanges != null && rebuildChanges.ContainsKey(record.ColumnName))
				{
					rebuildChanges.Remove(record.ColumnName);
				}
			}

			sessionInstance.Row.AcceptChanges();
			if (rebuildChanges != null)
			{
				ZSqlLoader.RestoreRowChanges(sessionInstance.Row, rebuildChanges);
			}
			if (sessionInstance.Row.RowState == DataRowState.Deleted || sessionInstance.Row.RowState == DataRowState.Detached)
			{
				sessionInstance.WasDeleted = true;
			}
		}

		void CheckDatabaseInstanceExists()
		{
			if (!IsExistsInDatabase)
			{
				throw new ObjectNotFoundInDatabaseException();
			}
		}
	}
}
