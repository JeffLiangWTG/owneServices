using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Schema;

namespace Enterprise.ResourceStrings.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Baseline")]
	[ImmutableObject(true)]
	public sealed class HelpDataStringSchema : Schema, ITableSchema, INonPersistentTableSchema
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810")]
		static HelpDataStringSchema()
		{
			Instance = new HelpDataStringSchema();
			int i = 0;
			HD_Caption = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_Caption, i++, SqlDbType.NVarChar, "", !IsNullable, 2147483647);
			HD_Code = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_Code, i++, SqlDbType.VarChar, "", !IsNullable, 256);
			HD_ControlIndexInParent = new SchemaIntColumn(Instance,HelpDataString.Schema.HD_ControlIndexInParent, i++, 0, !IsNullable);
			HD_ControlPath = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_ControlPath, i++, SqlDbType.VarChar, "", !IsNullable, 512);
			HD_FullDescription = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_FullDescription, i++, SqlDbType.NVarChar, "", !IsNullable, 2147483647);
			HD_IsCheckedOut = new SchemaBoolColumn(Instance,HelpDataString.Schema.HD_IsCheckedOut, i++, false, false, false);
			HD_EditReason = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_EditReason, i++, SqlDbType.VarChar, "", !IsNullable, 3);
			HD_Language = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_Language, i++, SqlDbType.VarChar, "", !IsNullable, 7);
			HD_MidCaption = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_MidCaption, i++, SqlDbType.NVarChar, "", !IsNullable, 100);
			HD_ShortCaption = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_ShortCaption, i++, SqlDbType.NVarChar, "", !IsNullable, 100);
			HD_ContextClassName = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_ContextClassName, i++, SqlDbType.VarChar, "", !IsNullable, 256);
			HD_ContextSourceFile = new SchemaStringColumn(Instance,HelpDataString.Schema.HD_ContextSourceFile, i++, SqlDbType.VarChar, "", !IsNullable, 256);
			HD_ContextSourceFileLine = new SchemaIntColumn(Instance,HelpDataString.Schema.HD_ContextSourceFileLine, i++, 0, !IsNullable);
			HD_ContextSourceFileCol = new SchemaIntColumn(Instance,HelpDataString.Schema.HD_ContextSourceFileCol, i++, 0, !IsNullable);
		}

		public readonly static SchemaStringColumn HD_Caption;
		public readonly static SchemaStringColumn HD_Code;
		public readonly static SchemaIntColumn HD_ControlIndexInParent;
		public readonly static SchemaStringColumn HD_ControlPath;
		public readonly static SchemaStringColumn HD_FullDescription;
		public readonly static SchemaBoolColumn HD_IsCheckedOut;
		public readonly static SchemaStringColumn HD_EditReason;
		public readonly static SchemaStringColumn HD_Language;
		public readonly static SchemaStringColumn HD_MidCaption;
		public readonly static SchemaStringColumn HD_ShortCaption;
		public readonly static SchemaStringColumn HD_ContextClassName;
		public readonly static SchemaStringColumn HD_ContextSourceFile;
		public readonly static SchemaIntColumn HD_ContextSourceFileLine;
		public readonly static SchemaIntColumn HD_ContextSourceFileCol;

		public static readonly HelpDataStringSchema Instance;

		string ITableSchema.SqlSchemaName
		{
			get { return "dbo"; }
		}

		string ITableSchema.TableName
		{
			get { return "HelpDataString"; }
		}
		SchemaPKColumn ITableSchema.PK
		{
			get { throw new NotSupportedException(); }
		}

		string ITableSchema.PkIndexName => null;

		SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
		{
			switch (columnName)
			{
				case HelpDataString.Schema.HD_Caption: return HD_Caption;
				case HelpDataString.Schema.HD_Code: return HD_Code;
				case HelpDataString.Schema.HD_ControlIndexInParent: return HD_ControlIndexInParent;
				case HelpDataString.Schema.HD_ControlPath: return HD_ControlPath;
				case HelpDataString.Schema.HD_FullDescription: return HD_FullDescription;
				case HelpDataString.Schema.HD_IsCheckedOut: return HD_IsCheckedOut;
				case HelpDataString.Schema.HD_Language: return HD_Language;
				case HelpDataString.Schema.HD_MidCaption: return HD_MidCaption;
				case HelpDataString.Schema.HD_ShortCaption: return HD_ShortCaption;
				case HelpDataString.Schema.HD_ContextClassName: return HD_ContextClassName;
				case HelpDataString.Schema.HD_ContextSourceFile: return HD_ContextSourceFile;
				case HelpDataString.Schema.HD_ContextSourceFileLine: return HD_ContextSourceFileLine;
				case HelpDataString.Schema.HD_ContextSourceFileCol: return HD_ContextSourceFileCol;

				default: return null;
			}
		}

		SchemaColumnCollection ITableSchema.All
		{
			get { throw new NotSupportedException(); }
		}
	}
}
