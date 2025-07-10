using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Env = System.Environment;

namespace CargoWise.EntityFramework
{
	public class ConcurrencyResolver
	{
		#region Notification Texts

		INotificationHandlerWithMessageOverride NotificationHandlerWithMessageOverride => notifier as INotificationHandlerWithMessageOverride;

		string GetMergeWarningMessage()
		{
			return NotificationHandlerWithMessageOverride?.MergeWarningMessage ?? MergeWarningMessage;
		}

		internal static string MergeWarningMessage
		{
			get { return Res.GetString("95a4b7f7-55cb-41d3-8fae-f1b3565c14be", "While you have been working with this form, another user has made changes.\r\n\r\nThe system will now try to combine your changes with those of the other user.\r\nAfter you click 'OK', the form will merge your changes with changes made by other user.\r\n\r\nHowever, the fields will have warning messages explaining the other user's changes.\r\nPlease review the form carefully before clicking the 'Save' button again."); }
		}

		internal static string CannotDeleteMessage
		{
			get { return Res.GetString("00A55B8A-FDB6-45C0-86D5-1B8A6E122F90", "While you have been working with this form, another user has made changes.\r\n\r\nThe system cannot automatically merge your changes.\r\nPlease cancel your changes and reload the form."); }
		}

		string GetCannotDeleteMessage()
		{
			return NotificationHandlerWithMessageOverride?.CannotDeleteMessage ?? CannotDeleteMessage;
		}

		string GetCriticalWarningMessage()
		{
			return NotificationHandlerWithMessageOverride?.CriticalWarningMessage ?? CriticalWarningMessage;
		}

		internal static string CriticalWarningMessage
		{
			get { return Res.GetString("58f9e013-26fe-4df4-bc95-2bb50a095fc9", "While you have been working with this form, another user has made changes.\r\n\r\nThe system cannot automatically merge your changes because there are conflicts with critical fields.\r\nPlease cancel your changes and reload the form."); }
		}

		string GetCannotDeleteObjectsHeader()
		{
			return NotificationHandlerWithMessageOverride?.CannotDeleteObjectsHeader ?? CannotDeleteObjectsHeader;
		}

		string GetDeletedObjectsHeader()
		{
			return NotificationHandlerWithMessageOverride?.DeletedObjectsHeader ?? DeletedObjectsHeader;
		}

		internal static string CannotDeleteObjectsHeader
		{
			get { return Res.GetString("84257978-B648-4FB6-94E9-80A438C81F4A", "The following objects have been deleted by another user and cannot be amended here."); }
		}

		internal static string DeletedObjectsHeader
		{
			get { return Res.GetString("dee6cb60-433a-4217-87ab-04e73921067a", "The following objects have been deleted:"); }
		}

		string GetMergedObjectsHeader()
		{
			return NotificationHandlerWithMessageOverride?.MergedObjectsHeader ?? MergedObjectsHeader;
		}

		internal static string MergedObjectsHeader
		{
			get { return Res.GetString("3aa1c88e-ddb9-4665-a383-66b4295f11b5", "The following objects have changes and will be merged:"); }
		}

		string GetCriticalObjectsHeader()
		{
			return NotificationHandlerWithMessageOverride?.CriticalObjectsHeader ?? CriticalObjectsHeader;
		}

		internal static string CriticalObjectsHeader
		{
			get { return Res.GetString("d42a3450-3417-492d-a265-b3cdf387d576", "The following objects have critical changes and cannot be merged:"); }
		}

		#endregion

		readonly ZSaveConcurrencyException exception;
		readonly IFactoryChangeSet changes;
		readonly INotificationHandler notifier;

		readonly List<IObjectChangeSet> deleted = new List<IObjectChangeSet>();
		readonly List<IObjectChangeSet> CannotDelete = new List<IObjectChangeSet>();
		readonly List<IObjectChangeSet> mergeable = new List<IObjectChangeSet>();
		readonly List<IObjectChangeSet> critical = new List<IObjectChangeSet>();

		internal ConcurrencyResolver(ZSaveConcurrencyException exception, IFactoryChangeSet changes, INotificationHandler notifier)
		{
			this.exception = exception;
			this.changes = changes;
			this.notifier = notifier;
			OnMergeFailure = OnMergeFailureAction.SendReport;
		}

		internal OnMergeFailureAction OnMergeFailure { get; set; }

		internal enum OnMergeFailureAction
		{
			None,
			SendReport,
			ThrowException,
		}

		internal void Resolve()
		{
			Collect();
			if (OnMergeFailure != OnMergeFailureAction.None)
			{
				Report();
			}
			if (!HasCriticalObjects())
			{
				HandleCustomResolve();
				Merge();
				HandleCustomResolveAfterMerge();
				Delete();
			}
		}

		void HandleCustomResolve()
		{
			foreach (var obj in mergeable)
			{
				obj.SessionInstance?.OnConcurrencyException(obj.MergeableProperties);
			}
		}

		void HandleCustomResolveAfterMerge()
		{
			foreach (var obj in mergeable)
			{
				obj.SessionInstance?.OnConcurrencyExceptionAfterMerge(obj.MergeableProperties);
			}
		}

		bool databaseChangeWasRollback;

		void Collect()
		{
			foreach (IObjectChangeSet obj in changes.GetChangedObjects())
			{
				if (!obj.IsExistsInDatabase)
				{
					if (obj.SessionInstance != null && !obj.SessionInstance.IsDeleted && !obj.SessionInstance.CanDelete)
					{
						CannotDelete.Add(obj);
						continue;
					}

					deleted.Add(obj);
					continue;
				}

				if (!obj.IsModifiedInDatabase)
				{
					if (exception?.InnerException?.Row != null && obj.DatabaseInstance?.PK == DataUtils.GetPk(exception.InnerException.Row))
					{
						if (obj.SessionInstance.Row != null && obj.SessionInstance.Row.RowState == DataRowState.Deleted)
						{
							databaseChangeWasRollback = true;
						}
						else
						{
							foreach (DataColumn column in obj.DatabaseInstance.Row.Table.Columns)
							{
								if (exception.InnerException.ColumnsDBChanged.TryGetValue(column.ColumnName, out var valueFromException))
								{
									var valueFromDatabase = obj.DatabaseInstance.Row[column.ColumnName, DataRowVersion.Original];

									if (HasDBChanged(valueFromException, valueFromDatabase))
									{
										throw new ZSaveConcurrencyException(exception);
									}
								}
							}
						}
					}

					continue;
				}

				if (obj.CanMerge())
				{
					mergeable.Add(obj);
					continue;
				}

				critical.Add(obj);
				(obj.SessionInstance as IConflictWithCriticalFields)?.SetConflictWithCriticalFieldsBusinessContext();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localising diagnostic information is counterproductive.")]
		bool HasDBChanged(object valueFromException, object valueFromDatabase)
		{
			var hasChanged = false;

			if (valueFromDatabase is bool valueBool && valueFromException is string valueString)
			{
				hasChanged = valueBool != (valueString == "True");
			}
			else if (!valueFromDatabase.Equals(valueFromException))
			{
				hasChanged = true;
			}

			return hasChanged;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		void Report()
		{
			if (!ShouldNotify())
			{
				if (exception != null)
				{
					if (exception.InnerException?.InnerException is InvalidOperationException invalidOpException)
					{
						notifier.ReportInformation(invalidOpException.Message, "WARNING");
					}
					else if (exception.InnerException?.InnerException is DBConcurrencyException concurrencyException
						&& concurrencyException.Message.StartsWith((NoResString)"Unreconcilable concurrency error due to", StringComparison.OrdinalIgnoreCase))
					{
						throw exception;
					}
					else
					{
						switch (OnMergeFailure)
						{
							case OnMergeFailureAction.SendReport:
								if (!databaseChangeWasRollback)
								{
									ErrorReporter.ReportOnce("ConcurrencyResolver_ResolverException",
										"ConcurrencyResolver could not find any differences between session and database versions of a row.\r\n\r\n" +
										CreateDataRowsDump(changes, exception) + "\r\n\r\nOriginal Error Message: " + exception);
								}
								throw new ZSaveConcurrencyException(exception, databaseChangeWasRollback);
							case OnMergeFailureAction.ThrowException:
								throw new ZSaveConcurrencyException(exception);

							default:
								throw new InvalidOperationException(FormattableString.Invariant($"Unknown value for {OnMergeFailure}"));
						}
					}
				}

				return;
			}

			var message = new StringBuilder();

			if (HasCannotDeleteObjects())
			{
				message.AppendLine(GetCannotDeleteMessage());
				ReportCannotDelete(message);
				notifier.ReportError(message.ToString(), "Error");
				return;
			}

			message.AppendLine(HasCriticalObjects()
							? GetCriticalWarningMessage()
							: GetMergeWarningMessage());

			if (!HasCriticalObjects())
			{
				ReportMergeable(message);
			}
			else
			{
				ReportCritical(message);
			}

			ReportDeleted(message);

			notifier.ReportInformation(message.ToString(), "WARNING");
		}

		static string CreateDataRowsDump(IFactoryChangeSet changes, ZSaveConcurrencyException exception)
		{
			var sb = new StringBuilder();
			var exceptionBizos = exception?.BusinessObjects?.ToList() ?? new List<BusinessObject>(0);
			var changedObjects = changes.GetChangedObjects();
			foreach (var obj in changedObjects)
			{
				CreateDataRowDump(obj, sb);
			}
			var hashedObjectPks = new HashSet<ZGuid>(changedObjects.Select(obj => obj.SessionInstance.PK));
			exceptionBizos.RemoveAll(match => hashedObjectPks.Contains(match.PK));

			if (exceptionBizos.Count > 0)
			{
				sb.AppendLine().AppendFormat((NoResString)"Following {0} row(s) from concurrency exception were skipped by ConcurrencyResolver:", exceptionBizos.Count).AppendLine().AppendLine();
				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				foreach (var exceptionBizo in exceptionBizos)
				{
					CreateDataRowDump(new ObjectChangeSet(exceptionBizo, FactoryChangeSet.LoadDatabaseEdition(exceptionBizo), schemaResolver), sb);
				}
			}

			return sb.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		static void CreateDataRowDump(IObjectChangeSet obj, StringBuilder sb)
		{
			if (obj == null)
			{
				sb.AppendLine().AppendLine((NoResString)"ChangeSet object is null");
			}
			else if (obj.SessionInstance == null)
			{
				sb.AppendLine().AppendLine((NoResString)"Session instance is null");
			}
			else if (obj.SessionInstance.Row == null)
			{
				sb.AppendLine().AppendLine((NoResString)"Session instance row is null");
			}
			else if (obj.SessionInstance.Row.RowState == DataRowState.Modified || obj.SessionInstance.Row.RowState == DataRowState.Deleted)
			{
				sb.AppendLine().AppendLine((NoResString)"Session instance Current and Original comparison:");
				CompareDataRows(obj.SessionInstance.Row, DataRowVersion.Current, obj.SessionInstance.Row, DataRowVersion.Original, sb, "Session CURRENT", "Session ORIGINAL");

				if (obj.DatabaseInstance != null)
				{
					if (ReferenceEquals(obj.DatabaseInstance, obj.SessionInstance))
					{
						sb.AppendLine().AppendLine((NoResString)"Database instance is same object as session instance");
					}
					else if (ReferenceEquals(obj.DatabaseInstance.Factory, obj.SessionInstance.Factory))
					{
						sb.AppendLine().AppendLine((NoResString)"Database instance is in same factory as session instance");
					}

					sb.AppendLine().AppendLine((NoResString)"Database instance Current and Original comparison:");
					CompareDataRows(obj.DatabaseInstance.Row, DataRowVersion.Current, obj.DatabaseInstance.Row, DataRowVersion.Original, sb, "DataBase CURRENT", "DataBase ORIGINAL");

					sb.AppendLine().AppendLine((NoResString)"Database UP TO DATE CURRENT row:");
					var row = GetUpToDateRowFromDatabase(obj.DatabaseInstance.TableName, obj.DatabaseInstance.PKSchemaColumn, obj.DatabaseInstance.PK.ToGuid(), out var upToDateDbRowMessage);
					if (!string.IsNullOrEmpty(upToDateDbRowMessage))
					{
						sb.AppendLine(upToDateDbRowMessage);
					}

					sb.AppendLine().AppendLine((NoResString)"Database instance CURRENT and Session instance ORIGINAL comparison:");
					CompareDataRows(obj.SessionInstance.Row, DataRowVersion.Original, obj.DatabaseInstance.Row, DataRowVersion.Current, sb, "Session ORIGINAL", "DataBase CURRENT");

					sb.AppendLine().AppendLine((NoResString)"Database UP TO DATE ORIGINAL and Session instance ORIGINAL comparison:");
					CompareDataRows(obj.SessionInstance.Row, DataRowVersion.Original, row, DataRowVersion.Original, sb, "Session ORIGINAL", " DataBase Up TO DATE ORIGINAL");
				}
				else
				{
					sb.AppendLine().AppendLine((NoResString)"Database instance row not found (maybe already deleted)");
				}
			}
		}

		static DataRow GetUpToDateRowFromDatabase(string table, SchemaColumn pkColumn, Guid pk, out string outputMessage)
		{
			outputMessage = string.Empty;

			if (pkColumn == null)
			{
				outputMessage = (NoResString)"PKSchemaColumn is not defined";
				return null;
			}

			if (Db.Connection.IsInTransaction)
			{
				using (var connection = Db.NewExtraConnectionToMainDbWithReaderCredentials())
				{
					outputMessage = (NoResString)"Using extra connection";
					return GetUpToDateRowFromDatabase(table, pkColumn, pk, connection);
				}
			}

			return GetUpToDateRowFromDatabase(table, pkColumn, pk, Db.Connection);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		static DataRow GetUpToDateRowFromDatabase(string table, SchemaColumn pkColumn, Guid pk, DbConnection connection)
		{
			using (var cmd = connection.Command($"select * from {table} where {pkColumn.Name} = @pk"))
			{
				cmd.AddParameterBasedOnDbColumn((NoResString)"@pk", pk, pkColumn);

				using (var adapter = cmd.NewDataAdapter())
				{
					var ds = new DataSet { Locale = DefaultCulture.Instance };
					adapter.Fill(ds);
					if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
					{
						return ds.Tables[0].Rows[0];
					}
				}
			}

			return null;
		}

		static StringBuilder CompareDataRows(DataRow row1, DataRowVersion rowVersion1, DataRow row2, DataRowVersion rowVersion2, StringBuilder sb, String nameOfRow1, String nameOfRow2)
		{
			var result = new StringBuilder();

			if (row1 == null || row2 == null)
			{
				result.AppendLine((NoResString)"ROW NOT FOUND");
			}
			else if (!row1.HasVersion(rowVersion1) || !row2.HasVersion(rowVersion2))
			{
				result.AppendLine((NoResString)"ROW VERSION NOT AVAILABLE");
			}
			else
			{
				var value = row1[row1.Table.Columns[0], rowVersion1];
				var pKRowName = row1.Table.Columns[0].ColumnName;
				result.AppendLine(String.Format((NoResString)"Row {0}: Version: {1} State: {2} {3}: {4}", nameOfRow1, rowVersion1, row1.RowState, pKRowName, value.ToString()));
				value = row2[row2.Table.Columns[0], rowVersion2];
				result.AppendLine(String.Format((NoResString)"Row {0}: Version: {1} State: {2} {3}: {4}", nameOfRow2, rowVersion2, row2.RowState, pKRowName, value.ToString()));

				foreach (var column in row1.Table.Columns.Cast<DataColumn>().Skip(1).OrderBy(x => x.ColumnName))
				{
					try
					{
						var valueDataRow1 = row1[column, rowVersion1];
						var typeDataRow = valueDataRow1.GetType().ToString();
						var isGeographyDataRow = typeDataRow.Contains((NoResString)"Geography");
						var stringValueDataRow1 = (valueDataRow1 == null || valueDataRow1 == DBNull.Value) ? (NoResString)"<null>" : valueDataRow1.ToString()
							+ " (" + typeDataRow
							+ (isGeographyDataRow ? "" : (", " + valueDataRow1.GetHashCode().ToString())) + ")";

						var valueDataRow2 = row2[column.ColumnName, rowVersion2];
						typeDataRow = valueDataRow2.GetType().ToString();
						isGeographyDataRow = typeDataRow.Contains((NoResString)"Geography");
						var stringValueDataRow2 = (valueDataRow2 == null || valueDataRow2 == DBNull.Value) ? (NoResString)"<null>" : valueDataRow2.ToString()
							+ " (" + typeDataRow
							+ (isGeographyDataRow ? "" : (", " + valueDataRow2.GetHashCode().ToString())) + ")";

						if (!string.Equals(stringValueDataRow1, stringValueDataRow2))
						{
							result.AppendLine(column.ColumnName);
							result.Append(nameOfRow1).Append(": ").AppendLine(stringValueDataRow1);
							result.Append(nameOfRow2).Append(": ").AppendLine(stringValueDataRow2);
						}
					}
					catch (ArgumentException ex)
					{
						result.AppendLine(ex.Message);
					}
				}
			}
			sb.Append(result);
			return result;
		}

		bool HasCriticalObjects()
		{
			return critical.Count != 0;
		}

		bool HasCannotDeleteObjects()
		{
			return CannotDelete.Count != 0;
		}

		bool ShouldNotify()
		{
			return (deleted.Count > 0 || mergeable.Count > 0 || critical.Count > 0 || CannotDelete.Count > 0);
		}

		void ReportCannotDelete(StringBuilder message)
		{
			if (!HasCannotDeleteObjects())
			{
				return;
			}

			message.AppendLine(Env.NewLine + GetCannotDeleteObjectsHeader());

			foreach (IObjectChangeSet obj in CannotDelete)
			{
				ReportObjectChange(message, obj);
			}
		}

		void ReportDeleted(StringBuilder message)
		{
			if (deleted.Count == 0)
			{
				return;
			}

			message.AppendLine(Env.NewLine + GetDeletedObjectsHeader());

			foreach (IObjectChangeSet obj in deleted)
			{
				ReportObjectChange(message, obj);
			}
		}

		void ReportMergeable(StringBuilder message)
		{
			ReportChanges(mergeable, message, GetMergedObjectsHeader(), false);
		}

		void ReportCritical(StringBuilder message)
		{
			ReportChanges(critical, message, GetCriticalObjectsHeader(), true);
		}

		static void ReportChanges(ICollection<IObjectChangeSet> objects, StringBuilder message, string header, bool critical)
		{
			if (objects.Count == 0)
			{
				return;
			}

			message.AppendLine().AppendLine(header);

			foreach (var obj in objects)
			{
				ReportObjectChange(message, obj);

				foreach (var record in obj.MergeableProperties)
				{
					if (record.HasChangedInDatabase)
					{
						if (obj.SessionInstance is IConcurrencyExceptionDecorator decorator)
						{
							decorator.AppendDecoratedDisplayName(message, record);
						}
						else
						{
							message.Append(Tab).Append(record.DisplayName);
							message.AppendLine();
						}
					}
				}

				if (critical)
				{
					foreach (var record in obj.NonMergeableProperties)
					{
						if (record.HasChangedInDatabase)
						{
							message.Append(Tab).Append(record.DisplayName).Append((NoResString)" (Critical change)");
							message.AppendLine();
						}
					}
				}
			}
		}

		public const string Tab = "\t";

		static void ReportObjectChange(StringBuilder message, IObjectChangeSet obj)
		{
			message.Append(obj.DisplayName);
			string lastModified = obj.LastModified;
			if (!string.IsNullOrEmpty(lastModified))
			{
				message.Append(" (").Append(lastModified).Append(")");
			}
			if (obj.SessionInstance != null && obj.SessionInstance.IsDeleted)
			{
				message.Append((NoResString)" (pending delete)");
			}
			message.AppendLine();
		}

		void Merge()
		{
			foreach (IObjectChangeSet obj in mergeable)
			{
				obj.Merge();
			}
		}

		void Delete()
		{
			foreach (IObjectChangeSet obj in deleted)
			{
				obj.Delete();
			}
		}

#if DEBUG
		internal List<IObjectChangeSet> Mergeable_ForTest => mergeable;
#endif
	}
}
