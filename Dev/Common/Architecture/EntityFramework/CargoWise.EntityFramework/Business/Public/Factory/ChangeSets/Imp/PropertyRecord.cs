using System;
using System.Collections;
using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class PropertyRecord : IPropertyRecord
	{
		readonly ZPropertyInfo propertyInfo;
		readonly string columnName;

		readonly DataRow sessionRow;
		readonly Lazy<string> lastModified;

		readonly object originalValue;
		readonly object currentValue;
		readonly object databaseValue;

		public PropertyRecord(ZPropertyInfo propertyInfo, string columnName, DataRow sessionRow, DataRow databaseRow, Lazy<string> lastModified)
		{
			this.propertyInfo = propertyInfo;
			this.columnName = columnName;
			this.sessionRow = sessionRow;
			this.lastModified = lastModified;

			originalValue = sessionRow[columnName, DataRowVersion.Original];
			currentValue = sessionRow.RowState == DataRowState.Deleted ? DBNull.Value : sessionRow[columnName, DataRowVersion.Current];

			if (PropertyInfo.IsDbColumnSmallDateTime)
			{
				var zDateTimeCurrentValue = new ZDateTime(currentValue);
				if (zDateTimeCurrentValue.IsValid)
				{
					currentValue = zDateTimeCurrentValue.ToSmallDateTimeFloor().ToDateTime();
				}

				var zDateTimeOriginalValue = new ZDateTime(originalValue);
				if (zDateTimeOriginalValue.IsValid)
				{
					originalValue = zDateTimeOriginalValue.ToSmallDateTimeFloor().ToDateTime();
				}
			}

			databaseValue = databaseRow[columnName, databaseRow.HasVersion(DataRowVersion.Original) ? DataRowVersion.Original : DataRowVersion.Current]; // Current row version could be modified during BusinessObject initialization
		}

		public ZPropertyInfo PropertyInfo
		{
			get { return propertyInfo; }
		}

		public string ColumnName
		{
			get { return columnName; }
		}

		public string DisplayName
		{
			get { return propertyInfo.HumanReadableName; }
		}

		public object OriginalValue
		{
			get { return originalValue; }
		}

		public object CurrentValue
		{
			get { return currentValue; }
		}

		public object DatabaseValue
		{
			get { return databaseValue; }
		}

		public string LastModified => lastModified.Value;

		public bool HasChangedInDatabase
		{
			get { return !EqualValues(OriginalValue, DatabaseValue); }
		}

		public bool HasChangedInSession
		{
			get { return !EqualValues(OriginalValue, CurrentValue); }
		}

		public bool IsConsistentWithDatabase
		{
			get { return EqualValues(CurrentValue, DatabaseValue); }
		}

		public static bool EqualValues(object a, object b)
		{
			if (ZGeography.IsGeographyValue(a) && ZGeography.IsGeographyValue(b))
			{
				return ZGeography.Equals(a, b);
			}
			else
			{
				var equatable = a as IStructuralEquatable;
				return equatable != null
					? equatable.Equals(b, StructuralComparisons.StructuralEqualityComparer)
					: a.Equals(b);
			}
		}

		public bool IsMergeAllowed
		{
			get { return !propertyInfo.IsPersistent || propertyInfo.ConcurrencyPolicy.AllowMerge; }
		}

		public void Merge()
		{
			if (sessionRow.RowState == DataRowState.Deleted)
			{
				return;
			}

			var oldValue = propertyInfo.Value;
			var eventArgs = new ConcurrencyValueChangedEventArgs(oldValue, CurrentValue, propertyInfo, originalValue, lastModified.Value);
			propertyInfo.OnConcurrencyMerged(eventArgs);

			if (eventArgs.FinalValue != null)
			{
				sessionRow[columnName] = eventArgs.FinalValue;
				return;
			}

			if (IsConsistentWithDatabase)
			{
				return;
			}

			if (!IsMergeAllowed)
			{
				throw new Exception("Merge is not allowed for this property " + propertyInfo.Name);
			}

			if (HasChangedInSession && !HasChangedInDatabase)
			{
				sessionRow[columnName] = CurrentValue;
				return;
			}

			sessionRow[columnName] = DatabaseValue;
			propertyInfo.OnValueChanged(eventArgs);

			if (propertyInfo.IsPersistent && propertyInfo.ConcurrencyPolicy.IsSilent)
			{
				return;
			}

			propertyInfo.AddWarningWithoutValidationCheck(GetWarning(LastModified, CurrentValue ?? "", DatabaseValue ?? ""));
		}

		public static string GetWarning(string lastModified, object value, object databaseValue)
		{
			return Res.GetString("CFCB13BD-F61C-4BAF-BC5B-540E8016FA51", "Another user ({0}) has changed this field.\r\nYours: '{1}', Theirs: '{2}'",
								lastModified,
								value,
								databaseValue);
		}
	}
}
