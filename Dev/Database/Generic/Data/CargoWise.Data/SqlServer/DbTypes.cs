using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	// <summary>
	// Returns the SqlDbType base on the data type name on DB.
	// Notes:
	//   NUMERIC and DECIMAL are synonyms. Both return DECIMAL
	//   CHAR and NCHAR parameters append spaces to strings to complete the length. VARCHAR and NVARCHAR are used instead.
	// </summary>
	[Immutable]
	public class DbTypes : IEnumerable
	{
		#region Singleton Pattern

		// Explicit static constructor to tell C# compiler NOT to mark type as beforefieldinit
		// This is to guarantee the lazy instantiation of the singleton object
		static DbTypes() { }
		DbTypes()
		{
		}

		public static readonly DbTypes Instance = new DbTypes();

		#endregion

		public SqlDbType this[string dataType]
		{
			get
			{
				Argument.NotNullOrEmpty(dataType, nameof(dataType)); // Suggested By ReviewBot 

				SqlDbType result;
				if (!list.TryGetValue(dataType.ToLower(), out result))
				{
					throw new NotSupportedException("Not supported DataType: " + dataType + ".");
				}
				return result;
			}
		}

		public int Count => list.Count;

		#region SuppressResourceStringsCheckRegion

		readonly ImmutableDictionary<string, SqlDbType> list = new Dictionary<string, SqlDbType>()
		{
			{ "bigint", SqlDbType.BigInt },
			{ "binary", SqlDbType.Binary },
			{ "bit", SqlDbType.Bit },
			{ "boolean", SqlDbType.Bit },
			{ "datetime", SqlDbType.DateTime },
			{ "datetimeoffset", SqlDbType.DateTimeOffset },
			{ "datetime2", SqlDbType.DateTime2 },
			{ "date", SqlDbType.Date },
			{ "decimal", SqlDbType.Decimal },
			{ "numeric", SqlDbType.Decimal },
			{ "float", SqlDbType.Float },
			{ "int", SqlDbType.Int },
			{ "money", SqlDbType.Money },

			// ADO DataType as used in BizO's generated from .XSD / .XML
			{ "string", SqlDbType.NVarChar },
			{ "nchar", SqlDbType.NVarChar },
			{ "nvarchar", SqlDbType.NVarChar },
			{ "guid", SqlDbType.UniqueIdentifier },
			{ "int64", SqlDbType.BigInt },
			{ "int32", SqlDbType.Int },
			{ "int16", SqlDbType.SmallInt },
			{ "byte[]", SqlDbType.VarBinary },
			{ "real", SqlDbType.Real },
			{ "smalldatetime", SqlDbType.SmallDateTime },
			{ "smallint", SqlDbType.SmallInt },
			{ "smallmoney", SqlDbType.SmallMoney },
			{ "time", SqlDbType.Time },
			{ "timestamp", SqlDbType.Timestamp },
			{ "tinyint", SqlDbType.TinyInt },
			{ "uniqueidentifier", SqlDbType.UniqueIdentifier },
			{ "varbinary", SqlDbType.VarBinary },
			{ "char", SqlDbType.VarChar },
			{ "varchar", SqlDbType.VarChar },
			{ "sql_variant", SqlDbType.Variant },
			{ "xml", SqlDbType.Xml },

			//there is no SqlDbType.Geography - suggested workaround is to use NVarChar https://stackoverflow.com/questions/3736666/sqldbtype-and-geography . Udt could also work with a refactor
			{ "geography", SqlDbType.NVarChar },
		}.ToImmutableDictionary();

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return list.Values.GetEnumerator();
		}

		#endregion
	}
}
