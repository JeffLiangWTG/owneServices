// NOT AUTO GENERATED
//     This table is excluded in Build.xml, so if you want to make changes to the schema, just make them manually in this file.

using System;
using System.Data;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Schema
{
	[System.CodeDom.Compiler.GeneratedCode("CargoWise.EntityFramework", "1.0")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class MENTAgedScoreMetricSchema : CargoWise.Schema.Schema, ITableSchema
	{
		static MENTAgedScoreMetricSchema()
		{
			Instance = new MENTAgedScoreMetricSchema();
			PK = new SchemaPKColumn(Instance, Constants.PK);
			MAS_AgedScoreValue = new SchemaDecimalColumn(Instance, Constants.MAS_AgedScoreValue, 0, SqlDbType.Decimal, (decimal)0, !IsNullable, 18, 5);
			MAS_TimeRecordedUtc = new SchemaDateTimeColumn(Instance, Constants.MAS_TimeRecordedUtc, 1, SqlDbType.DateTime, DBNull.Value, !IsNullable);
			MAS_GG_ReleaseGroup = new SchemaGuidColumn(Instance, Constants.MAS_GG_ReleaseGroup, 2, Guid.Empty, IsNullable);
			MAS_FC_Component = new SchemaGuidColumn(Instance, Constants.MAS_FC_Component, 3, Guid.Empty, IsNullable);
			MAS_AttributeValue = new SchemaStringColumn(Instance, Constants.MAS_AttributeValue, 4, SqlDbType.VarChar, "", !IsNullable, 35);
			MAS_GS_NKStaffCode = new SchemaStringColumn(Instance, Constants.MAS_GS_NKStaffCode, 5, SqlDbType.VarChar, "", !IsNullable, 3);
			MAS_BAB_AcceptabilityBand = new SchemaGuidColumn(Instance, Constants.MAS_BAB_AcceptabilityBand, 6, Guid.Empty, IsNullable);
		}

		MENTAgedScoreMetricSchema()
		{
		}

		#region Constants

		public static class Constants
		{
			public const string SqlSchemaName = "dbo";
			public const string TableName = "MENTAgedScoreMetric";
			public const string Prefix = "MAS";
			public const string PK = "MAS_MAQ_NKCode";

			public const string MAS_MAQ_NKCode = "MAS_MAQ_NKCode";
			public const string MAS_AgedScoreValue = "MAS_AgedScoreValue";
			public const string MAS_FC_Component = "MAS_FC_Component";
			public const string MAS_GG_ReleaseGroup = "MAS_GG_ReleaseGroup";
			public const string MAS_TimeRecordedUtc = "MAS_TimeRecordedUtc";
			public const string MAS_GS_NKStaffCode = "MAS_GS_NKStaffCode";
			public const string MAS_BAB_AcceptabilityBand = "MAS_BAB_AcceptabilityBand";
			public const string MAS_AttributeValue = "MAS_AttributeValue";

			#region Indexes

			public const string PkIndex = null;

			public static class Indexes
			{
				public const string NR_UC__MAS_Code_MAS_FC_Component_MAS_GG_ReleaseGroup_MAS_TimeRecordedUtc = "NR_UC__MAS_Code_MAS_FC_Component_MAS_GG_ReleaseGroup_MAS_TimeRecordedUtc";
			}

			#endregion
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
					MAS_AgedScoreValue,
					MAS_FC_Component,
					MAS_GG_ReleaseGroup,
					MAS_TimeRecordedUtc,
					MAS_BAB_AcceptabilityBand,
			});
		}

		#endregion

		public static readonly SchemaPKColumn PK;
		public static readonly SchemaDecimalColumn MAS_AgedScoreValue;
		public static readonly SchemaDateTimeColumn MAS_TimeRecordedUtc;
		public static readonly SchemaGuidColumn MAS_GG_ReleaseGroup;
		public static readonly SchemaGuidColumn MAS_FC_Component;
		public static readonly SchemaStringColumn MAS_AttributeValue;
		public static readonly SchemaStringColumn MAS_GS_NKStaffCode;
		public static readonly SchemaGuidColumn MAS_BAB_AcceptabilityBand;

		#region GetSchemaColumn(ColumnName)

		internal static SchemaColumn GetSchemaColumn(string ColumnName)
		{
			switch (ColumnName)
			{
				case Constants.PK:
					return PK;

				case Constants.MAS_AgedScoreValue:
					return MAS_AgedScoreValue;
				case Constants.MAS_FC_Component:
					return MAS_FC_Component;
				case Constants.MAS_GG_ReleaseGroup:
					return MAS_GG_ReleaseGroup;
				case Constants.MAS_BAB_AcceptabilityBand:
					return MAS_BAB_AcceptabilityBand;
				case Constants.MAS_TimeRecordedUtc:
					return MAS_TimeRecordedUtc;

				default:
					return null;
			}
		}

		#endregion

		#region ITableSchema

		public static readonly MENTAgedScoreMetricSchema Instance;

		string ITableSchema.SqlSchemaName
		{
			get { return Constants.SqlSchemaName; }
		}

		string ITableSchema.TableName
		{
			get { return Constants.TableName; }
		}

		SchemaPKColumn ITableSchema.PK
		{
			get { return PK; }
		}

		string ITableSchema.PkIndexName => Constants.PkIndex;

		SchemaColumn ITableSchema.GetSchemaColumn(string ColumnName)
		{
			return GetSchemaColumn(ColumnName);
		}

		SchemaColumnCollection ITableSchema.All
		{
			get { return All; }
		}

		#endregion
	}
}
