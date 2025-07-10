using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	#region SourceTable Class

	public class SourceTable : ISourceTableTestHelper
	{
		public SourceTable(string originalDb, string originalName, IList<SourceColumn> columns, string whereClause)
			: this(originalDb, Db.SqlDbOwnerSchema, originalName, columns, whereClause)
		{
		}

		public SourceTable(string originalDb, string originalSchema, string originalName, IList<SourceColumn> columns, string whereClause)
		{
			if (columns == null || columns.Count == 0)
			{
				throw new ArgumentException("Column list cannot be empty", nameof(columns));
			}

			OriginalDb = originalDb;
			OriginalName = originalName;
			OriginalSchema = originalSchema;
			Columns = columns;
			WhereClause = whereClause;
		}

		public SourceTable(string originalDb, string originalName, IList<SourceColumn> columns)
			: this(originalDb, originalName, columns, null)
		{
		}

		public SourceTable(string originalName, IList<SourceColumn> columns, string whereClause)
			: this(Db.DatabaseName, originalName, columns, whereClause)
		{
		}

		public SourceTable(string originalName, IList<SourceColumn> columns)
			: this(Db.DatabaseName, originalName, columns, null)
		{
		}

		readonly string OriginalDb;
		readonly string OriginalSchema;
		public readonly string OriginalName;
		internal readonly IList<SourceColumn> Columns;
		readonly string WhereClause;

		#region Full Table Names

		protected static string DataCopyStorageDb => TransformationHelper.DataCopyDbName;

		protected string FullName
		{
			get { return TablePrefix + OriginalName; }
		}

		public string FullyQualifiedName
		{
			get { return "[" + DataCopyStorageDb + "]..[" + FullName + "]"; }
		}

		internal string OriginalFullyQualifiedName
		{
			get { return "[" + OriginalDb + "].[" + OriginalSchema + "].[" + OriginalName + "]"; }
		}

		#region TablePrefix

		protected string TablePrefix
		{
			get { return UpgUtils.UpgraderPrefix + "T" + TableUid + "_"; }
		}

		protected string TableUid
		{
			get
			{
				if (fTableUid == null)
				{
					fTableUid = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
				}

				return fTableUid;
			}
		}

		string fTableUid;

		#endregion

		#endregion

		#region Create / Drop Table

		/// <summary>
		/// Creates a copy of the origin table
		/// Note: Tries to create table by copying rows from origin table.
		///       If it fails and table is not mandatory, an empty table is created.
		/// </summary>
		public void Create()
		{
			DoDropIfExists();

			CreateAndPopulate();
			SetStatusCopied();
		}

		public void Drop()
		{
			if (IsTableCopied)
			{
				DoDropIfExists();
			}
		}

		#region Create and Populate Table

		/// <summary>
		/// Runs data copy on Original DB, so as tables reference on the WHERE clause are visible.
		/// </summary>
		protected void CreateAndPopulate()
		{
			string sqlText = GetInsertRowsScript();
			UpgCommandRunner.RunCommandOnGivenDb(Db.Connection, OriginalDb, sqlText);
		}

		protected string GetInsertRowsScript()
		{
			StringBuilder sqlText = new StringBuilder();

			sqlText.Append("SELECT ");

			foreach (SourceColumn column in Columns)
			{
				if (
					column.ReplacementValueIfNotExist != null &&
					!DbObjectCreator.ColumnExists(Db.Connection, OriginalDb, Db.SqlDbOwnerSchema, OriginalName, column.Name))
				{
					sqlText.Append(" ");
					sqlText.Append("convert(");
					sqlText.Append(column.ExpectedType);
					sqlText.Append(",");
					sqlText.Append(column.ReplacementValueIfNotExist);
					sqlText.Append(")");
				}

				sqlText.Append(" ");
				sqlText.Append(column.Name);
				sqlText.Append(",");
			}

			sqlText.Remove(sqlText.Length - 1, 1);

			sqlText.Append(" INTO ");
			sqlText.Append(FullyQualifiedName);

			sqlText.Append(" FROM ");
			sqlText.Append(OriginalFullyQualifiedName);

			if (WhereClause != null && !string.IsNullOrEmpty(WhereClause.Trim()))
			{
				sqlText.Append(" WHERE ");
				sqlText.Append(WhereClause);
			}

			return sqlText.ToString();
		}

		#endregion

		#region Drop Table

		protected void DoDropIfExists()
		{
			string sqlText = GetDropTableIfExistsScript();
			UpgCommandRunner.RunCommandOnGivenDb(Db.Connection, DataCopyStorageDb, sqlText);

			SetStatusNotCopied();
		}

		protected string GetDropTableIfExistsScript()
		{
			return String.Format("IF EXISTS (SELECT null FROM sys.objects WHERE name = '{0}' AND type = 'U') DROP TABLE {0}", FullName);
		}

		#endregion

		#region Table Copy Status

		#region CopyStatus

		public CopyStatusEnum CopyStatus
		{
			get { return fCopyStatus; }
		}

		protected CopyStatusEnum fCopyStatus = CopyStatusEnum.NotCopied;

		public bool IsTableCopied
		{
			get { return (fCopyStatus == CopyStatusEnum.CopiedAndPopulated); }
		}

		#endregion

		#region WarningMessageOnCopy

		public string WarningMessageOnCopy
		{
			get { return fWarningMessageOnCopy; }
		}

		protected string fWarningMessageOnCopy;

		public bool HasWarningsOnCopy
		{
			get { return (fWarningMessageOnCopy != null); }
		}

		#endregion

		#region Set Methods

		protected void SetStatusCopied()
		{
			fCopyStatus = CopyStatusEnum.CopiedAndPopulated;
		}

		protected void SetStatusNotCopied()
		{
			fCopyStatus = CopyStatusEnum.NotCopied;
			fWarningMessageOnCopy = null;
		}

		#endregion

		#endregion

		#endregion

		#region CopyStatusEnum

		public enum CopyStatusEnum
		{
			NotCopied
			, CopiedAndPopulated
		}

		#endregion

		#region ISourceTableTestHelper Members
		#if DEBUG

		void ISourceTableTestHelper.CreateOriginalTableAndOrColumnsIfNotExist()
		{
			using (((ICurrentDbControl)Db.Connection).UseDatabase(OriginalDb))
			{
				CreateOriginalTableAndOrColumnsIfNotExist_OnOriginalDb();
			}
		}

		void ISourceTableTestHelper.DropCreatedOriginalTableAndOrColumnsAfterCopyingData()
		{
			using (((ICurrentDbControl)Db.Connection).UseDatabase(OriginalDb))
			{
				DropCreatedOriginalTableAndOrColumnsAfterCopyingData_OnOriginalDb();
			}
		}

		protected void CreateOriginalTableAndOrColumnsIfNotExist_OnOriginalDb()
		{
			string dummyColumnName = "CreateOriginalIfNotExistForTest_Dummy01";
			string tableCreationScript = String.Format("CREATE TABLE {0} ({1} int null)", OriginalName, dummyColumnName);

			if (!DbObjectCreator.TableExists(Db.Connection, OriginalName))
			{
				DbObjectCreator.CreateTableIfNotExists(Db.Connection, OriginalName, tableCreationScript);
				WasTableCreatedForTest = true;
			}

			foreach (SourceColumn column in Columns)
			{
				if (!DbObjectCreator.ColumnExists(Db.Connection, OriginalName, column.Name))
				{
					DbObjectCreator.CreateColumnIfNotExists(Db.Connection, OriginalName, column.Name, column.ExpectedType, column.ReplacementValueIfNotExist);
					SourceColumnCreatedForTestList.Add(column);
				}
			}

			string dropColumnScript = String.Format(@"
					IF EXISTS (SELECT null FROM sys.objects tab INNER JOIN sys.columns col ON tab.object_id = col.object_id
					           WHERE tab.name = '{0}' AND col.name = '{1}')
					  ALTER TABLE {0} DROP COLUMN {1}",
				OriginalName, dummyColumnName);
			Db.Connection.ExecuteNonQuery(dropColumnScript);
		}

		protected void DropCreatedOriginalTableAndOrColumnsAfterCopyingData_OnOriginalDb()
		{
			if (WasTableCreatedForTest)
			{
				string dropTableScript = String.Format(@"
					IF EXISTS (SELECT null FROM sys.objects tab WHERE tab.name = '{0}')
					  DROP TABLE {0}",
					OriginalName);
				Db.Connection.ExecuteNonQuery(dropTableScript);
			}
			else
			{
				foreach (SourceColumn column in SourceColumnCreatedForTestList)
				{
					if (DbObjectCreator.ColumnExists(Db.Connection, OriginalName, column.Name))
					{
						DBTransformationTestHelper.DropDefaultConstraintFor(OriginalName, column.Name, Db.Connection);
						Db.Connection.ExecuteNonQuery($"ALTER TABLE {OriginalName.QuoteName()} DROP COLUMN {column.Name.QuoteName()}");
					}
				}
			}
		}

		protected bool WasTableCreatedForTest;
		protected ArrayList SourceColumnCreatedForTestList = new ArrayList();

		#endif
		#endregion
	}

	#endregion

	#region SourceColumn Class

	public class SourceColumn
	{
		/// <summary>
		/// Instantiate a new SourceColumn.
		/// </summary>
		/// <param name="name">Column name</param>
		/// <param name="expectedType">
		///		Column DataType
		///		Note: The data type should be as in a database column declaration
		///	        ie. char, varchar, nchar, nvarchar, varbinary should include the size in parentheses.
		///					eg. char(5)
		/// </param>
		public SourceColumn(string name, string expectedType)
			: this(name, expectedType, null)
		{
		}

		/// <summary>
		/// Instantiate a new SourceColumn, specifying a Default Value to be used in case the original column does not exist.
		/// </summary>
		/// <param name="name">Column name</param>
		/// <param name="expectedType">
		///		Column DataType
		///		Note: The data type should be as in a database column declaration
		///	        ie. char, varchar, nchar, nvarchar, varbinary should include the size in parentheses.
		///					eg. char(5)
		/// </param>
		/// <param name="dbStyleDefaultValueIfNotExist">
		///		Replacement Value
		///		Note:
		///		  The replacement value should follow SQL literal value syntax (including single quotes if needed)
		///     eg. 0 , 1.5 , getdate() , newid() , '' , 'Y' , '2005-05-11' , '73EC5F51-270F-4B1C-B8BF-2B8544E9E40B'
		/// </param>
		public SourceColumn(string name, string expectedType, string dbReplacementValueIfNotExist)
		{
			if (expectedType == null || string.IsNullOrEmpty(expectedType.Trim()))
			{
				throw new ArgumentException("A valid data-type must be specified", nameof(expectedType));
			}

			if (dbReplacementValueIfNotExist != null && string.IsNullOrEmpty(dbReplacementValueIfNotExist.Trim()))
			{
				throw new ArgumentException("A valid replacement value (e.g.: '', 'Y', getdate(), 1.5) must be supplied, or pass a null string if no replacement.", nameof(dbReplacementValueIfNotExist));
			}

			this.Name = name;
			this.ExpectedType = expectedType;
			this.ReplacementValueIfNotExist = dbReplacementValueIfNotExist;
		}

		public readonly string Name;
		public readonly string ExpectedType;
		public readonly string ReplacementValueIfNotExist;
	}

	#endregion
}
