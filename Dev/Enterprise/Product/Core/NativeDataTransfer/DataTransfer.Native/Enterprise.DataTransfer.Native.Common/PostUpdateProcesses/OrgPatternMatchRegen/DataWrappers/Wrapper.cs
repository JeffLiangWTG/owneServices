using System.Data;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	abstract class Wrapper
	{
		internal virtual void Init(DataRow row, WrapperFactory factory)
		{
			this.row = Argument.NotNull(row, "DataRow row");
			this.Factory = Argument.NotNull(factory, "WrapperFactory factory");
			if (PK.IsEmpty)
			{
				PK = ZGuid.NewZGuid();
			}
		}

		DataRow row;
		protected WrapperFactory Factory { get; private set; }

		protected ZString GetValue(SchemaStringColumn column)
		{
			return new ZString(row[column.Name]);
		}

		protected ZGuid GetValue(SchemaGuidColumn column)
		{
			return new ZGuid(row[column.Name]);
		}

		protected ZBool GetValue(SchemaBoolColumn column)
		{
			return new ZBool(row[column.Name]);
		}

		protected void SetValue(SchemaBoolColumn column, ZBool value)
		{
			row[column.Name] = (bool)value;
		}

		protected void SetValue(SchemaGuidColumn column, ZGuid value)
		{
			row[column.Name] = value.ToGuid();
		}

		protected void SetValue(SchemaStringColumn column, ZString value)
		{
			row[column.Name] = value.ToString();
		}

		protected ZString GetOriginalValue(SchemaStringColumn column)
		{
			return row.HasVersion(DataRowVersion.Original) ? new ZString(row[column.Name, DataRowVersion.Original]) : ZString.Empty;
		}

		public ZGuid PK
		{
			get { return GetValue(PKSchemaColumn); }
			set { SetValue(PKSchemaColumn, value); }
		}

		protected abstract SchemaPKColumn PKSchemaColumn { get; }

		public bool HasChanges
		{
			get { return row.RowState != DataRowState.Unchanged; }
		}

		public bool IsInDatabase
		{
			get { return row.RowState != DataRowState.Added; }
		}

		public bool IsDeleted
		{
			get { return row.RowState == DataRowState.Deleted; }
		}

		public void Delete()
		{
			row.Delete();
		}

		public override bool Equals(object obj)
		{
			var other = obj as Wrapper;
			return other != null
				&& other.GetType() == this.GetType()
				&& other.PK == this.PK;
		}

		public override int GetHashCode()
		{
			return PK.GetHashCode();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override string ToString()
		{
			return row.Table.TableName + " PK: " + PK.ToString() + " RowState: " + row.RowState.ToString();
		}
	}
}
