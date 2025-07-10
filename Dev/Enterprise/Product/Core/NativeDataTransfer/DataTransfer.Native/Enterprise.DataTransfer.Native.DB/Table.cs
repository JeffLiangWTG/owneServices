using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.DB
{
	public class Table
	{
		#region Constructor

		internal Table()
		{
		}

		public static Table Get(string tableName)
		{
			return new TableBuilder().Construct(tableName);
		}

		#endregion

		public string Name { get; internal set; }

		#region Table Name and Prefix

		public string Prefix
		{
			get
			{
				if (prefix.IsEmpty())
				{
					prefix = GetPrefix();
				}
				return prefix;
			}
		}
		string prefix;

		string GetPrefix()
		{
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Name);
		}

		#endregion

		#region ColumnDef Definitions

		public ColumnCollection Columns
		{
			get;
			internal set;
		}

		#endregion

		public IEnumerable<Table> ReferenceTables
		{
			get
			{
				var result = new List<Table>();
				foreach (ForeignKey foreignKey in Columns.ForeignKeys)
				{
					if (foreignKey.ReferenceTable != null)
					{
						result.Add(foreignKey.ReferenceTable);
					}
				}
				return result;
			}
		}

		#region Constraint and CandidateKey
		/// <summary>
		/// Constraints for a table
		/// </summary>

		/// <summary>
		/// Unique Constraints for a table except for PK constraint
		/// </summary>
		public IEnumerable<Constraint> CandidateKeyConstraints
		{
			get; internal set;
		}

		/// <summary>
		/// Unique Constraint that only consist one column
		/// </summary>
		public IEnumerable<ColumnDef> SingleColumnUniqueConstraints
		{
			get
			{
				var indexes = new List<ColumnDef>();
				indexes.Add(Columns.PrimaryKey);
				indexes.AddRange(SingleColumnCandidateConstraints);
				return indexes.Where(item => item != null).Distinct();
			}
		}

		public IEnumerable<ColumnDef> SingleColumnCandidateConstraints
		{
			get
			{
				var columns = from candidateKey in CandidateKeyConstraints
							  where candidateKey.ColumnSpan == 1
							  select candidateKey.Columns.Single();
				return columns;
			}
		}

		#endregion

		#region IndexStrings

		public IEnumerable<string> IndexStrings
		{
			get { return indexStrings ?? (indexStrings = TableBuilder.BuildIndexStrings(this)); }
		}
		IEnumerable<string> indexStrings;

		#endregion

		#region ToString

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		public override string ToString()
		{
			return string.Format("Name: {0}", Name);
		}

		public bool Equals(Table other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.Name, Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(Table))
			{
				return false;
			}

			return Equals((Table)obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}

		#endregion
	}

	public static class TableExtension
	{
		const string sql = "SELECT * FROM {0}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static DataTable ToDataTable(this Table table)
		{
			var dataTable = new DataTable(table.Name);
			using (var adapter = DataSetContext.Connection.Command(String.Format(sql, table.Name)).NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return RemoveColumnsNotInApplicationSchema(dataTable);
		}

		public static IEnumerable<DataRow> ToDataSet(this Table table, params Criteria[] criterias)
		{
			IEnumerable<DataRow> result = table.ToDataTable().AsEnumerable();
			foreach (var criteria in criterias.Where(c => c.TableName == table.Name))
			{
				result = result.Where(row => row.Match(criteria));
			}
			return result;
		}

		public static string ToSql(this Table table, Dictionary<string, Sql.SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParams, params MultipleValueCriteria[] criterias)
		{
			return new FindByCriteriaStatement(table, null, criterias, keyToParameters, tvpParams).Generate();
		}

		static DataTable RemoveColumnsNotInApplicationSchema(DataTable table)
		{
			var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(table.TableName);
			var columns = table.Columns;
			for (var i = 0; i < columns.Count; i++)
			{
				var schemaColumn = tableSchema.GetSchemaColumn(columns[i].ColumnName);
				if (schemaColumn == null || schemaColumn.IsComputed)
				{
					columns.RemoveAt(i);
					i--;
				}
			}
			return table;
		}
	}
}
