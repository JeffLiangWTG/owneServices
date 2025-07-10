using System;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// This class was formerly RowWrapper - now all partial classes to reduce memory footprint of reflection
	/// </summary>
	public abstract partial class BusinessObject : ZCustomTypeDescriptor, IKnowPropertyInformation, INeedRow, INeedDataSet, IIdentified
	{
		protected BusinessObject(DataRow row)
		{
			InitialiseRowWrapper(row);
		}

		internal virtual void InitialiseRowWrapper(DataRow row)
		{
			if (row == null)
			{
				throw new ArgumentNullException(nameof(row), "RowWrapper requires a non-null Row to be passed");
			}

			this.Row = row;
			this.Table = row.Table;
			this.Data = Table.DataSet;
			if (Row.RowState == DataRowState.Detached)
			{
				this.IsDataRowInDataTable = false;
			}
		}
		public void CancelChanges()
		{
			if (Row != null && (Row.RowState == DataRowState.Added || Row.RowState == DataRowState.Modified))
			{
				Row.RejectChanges();
				IsDataRowInDataTable = true;
			}
			IsLightValidationValid = null;
		}

		/// <summary>
		/// Delete the object and the row it wraps.
		/// </summary>
#if DEBUG
		protected
#endif
		internal void DeleteRow()
		{
			if (Row != null && Row.RowState != DataRowState.Deleted)
			{
				try
				{
					Row.Delete();
				}
				catch (InvalidOperationException ex)
				{
					ErrorReporter.ReportOnce(
						FormattableString.Invariant($"Error on DeleteRow Row.Delete(), Table: {Row.Table},  BusinessObject: {GetType().Name}, ThreadName: {Thread.CurrentThread.Name}"), ex);
					throw;
				}
				IsDataRowInDataTable = true;
			}
		}

		public bool IsRowDeletedOrNull
		{
			get { return (Row == null || Row.RowState == DataRowState.Deleted); }
		}

		public bool IsRowDeletedOrDetachedOrNull
		{
			get { return IsRowDeletedOrNull || Row.RowState == DataRowState.Detached; }
		}

		#region PK & Identiers

		public ZGuid PK
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetPKInternal(); }
		}

		ZGuid IIdentified.Identifier
		{
			get { return GetPKInternal(); }
		}

		#endregion

		#region TableName

		public virtual string TableName
		{
			get
			{
				return Table.TableName;
			}
		}

		#endregion

		#region INeedRow

		DataRow INeedRow.Row
		{
			get { return this.Row; }
		}

		#endregion

		#region INeedDataSet

		DataSet INeedDataSet.Data
		{
			get { return Data; }
		}

		#endregion

		#region IKnowPropertyInformation

		bool IKnowPropertyInformation.IsNullable(string propertyName)
		{
			bool result = true;

			if (Table != null && Table.Columns[propertyName] != null)
			{
				result = Table.Columns[propertyName].AllowDBNull;
			}

			return result;
		}

		object IKnowPropertyInformation.GetDefaultValue(string propertyName)
		{
			object result = DBNull.Value;

			if (Table != null && Table.Columns[propertyName] != null)
			{
				result = Table.Columns[propertyName].DefaultValue;
			}

			return result;
		}

		int IKnowPropertyInformation.GetMaxLength(string propertyName)
		{
			int result = -1;

			if (Table != null && Table.Columns[propertyName] != null)
			{
				result = Table.Columns[propertyName].MaxLength;
			}

			return result;
		}

		#endregion

		#region Implementation

		internal DataSet Data;
		internal DataTable Table;
		[NonSerialized]
		internal DataRow Row;

		#endregion
	}
}
