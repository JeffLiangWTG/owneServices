using System;
using CargoWise.Schema;

namespace Enterprise.AuditDataServices.ArchiveManager.Testing
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class InvalidMatchTestSchema : Schema, ITableSchema
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
		static InvalidMatchTestSchema()
		{
			var column = 0;
			Instance = new InvalidMatchTestSchema();
			PK = new SchemaPKColumn(Instance, Constants.PK, true);
			ParentTableCode = new SchemaStringColumn(Instance, Constants.ParentTableCode, 0, GenericStringSchemaColumn.SqlDbType, string.Empty, true, 3);
			ParentID = new SchemaGuidColumn(Instance, Constants.ParentID, column++, DBNull.Value, true);
		}

		InvalidMatchTestSchema()
		{
		}

		#region Constants

		public static class Constants
		{
			public const string SqlSchemaName = "dbo";
			public const string TableName = "InvalidMatchTest";
			public const string Prefix = "IMT";
			public const string PK = "IMT_PK";
			public const string ParentTableCode = "IMT_ParentTableCode";
			public const string ParentID = "IMT_NonMatchingParentID";
		}

		#endregion

		#region All

		public static SchemaColumnCollection All
		{
			get { return AllHolder.all; }
		}

		class AllHolder
		{
			static AllHolder()
			{
				// Empty constructor to prevent initialisation until first member access.
			}

			public static readonly SchemaColumnCollection all = new SchemaColumnCollection(PK, new SchemaColumn[]
			{
					ParentTableCode,
					ParentID
			});
		}

		#endregion

		#region PK

		public static readonly SchemaPKColumn PK;

		#endregion

		public static readonly SchemaStringColumn ParentTableCode;
		public static readonly SchemaGuidColumn ParentID;

		#region GetSchemaColumn(columnName)

		internal static SchemaColumn GetSchemaColumn(string columnName)
		{
			switch (columnName)
			{
				case Constants.PK:
					return PK;

				case Constants.ParentTableCode:
					return ParentTableCode;

				case Constants.ParentID:
					return ParentID;

				default:
					return null;
			}
		}

		#endregion

		#region ITableSchema

		public static readonly InvalidMatchTestSchema Instance;

		string ITableSchema.SqlSchemaName => Constants.SqlSchemaName;

		string ITableSchema.TableName => Constants.TableName;

		SchemaPKColumn ITableSchema.PK => PK;

		string ITableSchema.PkIndexName => null;

		SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
		{
			return GetSchemaColumn(columnName);
		}

		SchemaColumnCollection ITableSchema.All => All;

		#endregion
	}
}
