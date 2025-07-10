using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Database.Shared;
using Microsoft.SqlServer.Types;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoBusinessObjectSchema : AutoSourceFile
	{
		public AutoBusinessObjectSchema(BusinessObjectInfo info)
		{
			this.Info = info;
		}

		#region Code for Body

		protected override string Body
		{
			get
			{
				return LinesOfCode(
					CodeForUsingClause,
					"",
					"namespace " + Info.NamespaceOfSchema,
					"{",
					"	[System.CodeDom.Compiler.GeneratedCode(\"CargoWise.EntityFramework\", \"1.0\")]",
					"	[WTG.StaticAnalysis.Annotation.Immutable]",
					"	public sealed class " + ClassName + " : " + InheritsFrom,
					"	{",
								CodeForStaticConstructor,
					"",
								CodeForPrivateConstructor,
					"",
								CodeForSchemaConstants,
					"",
								CodeForAllProperty,
					"",
								CodeForPK,
					"",
								CodeForFieldDefinitions,
					"",
								CodeForGetSchemaColumn,
					"",
								CodeForITableSchema,
					"	}",
					"}"
				);
			}
		}

		#endregion

		#region Code for Using Clause

		string CodeForUsingClause
		{
			get
			{
				return LinesOfCode(
					"using System;",
					"using System.Data;",
					"using CargoWise.Database.Shared;",
					"using CargoWise.Schema;"
					);
			}
		}

		#endregion

		#region Code for Static Constructor

		string CodeForStaticConstructor
		{
			get
			{
				var linesOfText = new List<string>();
				linesOfText.Add("		static " + ClassName + "()");
				linesOfText.Add("		{");
				if (!string.IsNullOrWhiteSpace(CodeForFieldInitialisations))
				{
					linesOfText.Add("			var column = 0;");
				}
				linesOfText.Add("			Instance = new " + ClassName + "();");
				string pkLine = "			PK = new SchemaPKColumn(Instance, Constants.PK";
				string index = AutoProperty.IndexForColumn(Info.Table.TableName, Info.PKColumnName, Info.RefDbType);
				pkLine += ", " + (index != null ? "true" : "false");
				linesOfText.Add(pkLine + ");");
				if (!string.IsNullOrWhiteSpace(CodeForFieldInitialisations))
				{
					linesOfText.Add(CodeForFieldInitialisations);
				}
				linesOfText.Add("		}");

				return LinesOfCode(linesOfText.ToArray());
			}
		}

		#endregion

		#region Code for Private Constructor

		string CodeForPrivateConstructor
		{
			get
			{
				return LinesOfCode(
					"		" + ClassName + "()",
					"		{",
					"		}");
			}
		}

		#endregion

		#region Code for Schema Constants

		string CodeForSchemaConstants
		{
			get
			{
				return LinesOfCode(
					"		#region Constants",
					"",
					"		public static class Constants",
					"		{",
								CodeForSqlSchemaName,
								CodeForTableName,
					"			public const string Prefix = \"" + Info.PKColumnName.Substring(0, 3).TrimEnd('_') + "\";",
								CodeForColumnNames,
					"",
								CodeForIndexSchemaConstants,
					"		}",
					"",
					"		#endregion"
					);
			}
		}

		string CodeForIndexSchemaConstants
		{
			get
			{
				return LinesOfCode(
					"			#region Indexes",
					"",
								CodeForPkIndex,
								CodeForUniqueIndexSchema,
					"			#endregion // Indexes"
					);
			}
		}

		string CodeForPkIndex
		{
			get
			{
				return (string.IsNullOrWhiteSpace(Info.PkIndex))
					? "			public const string PkIndex = null;"
					: string.Format(CultureInfo.InvariantCulture, "			public const string PkIndex = \"{0}\";", Info.PkIndex)
					;
			}
		}

		string CodeForUniqueIndexSchema
		{
			get
			{
				if (Info.Indexes.Length > 0)
				{
					List<string> result = new List<string>();
					foreach (string indexName in Info.Indexes)
					{
						result.Add("				public const string " + indexName + " = \"" + indexName + "\";");
					}

					return LinesOfCode(
						"",
						"			public static class Indexes",
						"			{",
										string.Join(System.Environment.NewLine, result.ToArray()),
						"			}",
						""
						);
				}

				return string.Empty;
			}
		}

		string CodeForSqlSchemaName
		{
			get { return "			public const string SqlSchemaName = \"" + Info.SqlSchemaName + "\";"; }
		}

		string CodeForTableName
		{
			get
			{
				return "			public const string TableName = \"" + Info.TableName + "\";";
			}
		}

		string CodeForColumnNames
		{
			get
			{
				if (fCodeForColumnNames == null)
				{
					fCodeForColumnNames = "			public const string PK = \"" + Info.PKColumnName + "\";" + System.Environment.NewLine + System.Environment.NewLine;

					foreach (AutoProperty property in Properties)
					{
						fCodeForColumnNames += "			public const string " + property.ColumnName + " = \"" + property.ColumnName + "\";" + System.Environment.NewLine;
					}

					fCodeForColumnNames = fCodeForColumnNames.TrimEnd(Whitespace);
				}

				return fCodeForColumnNames;
			}
		}

		string fCodeForColumnNames;

		#endregion

		#region Code for All property

		string CodeForAllProperty
		{
			get
			{
				return LinesOfCode(
					"		#region All",
					"",
					"		public static SchemaColumnCollection All",
					"		{",
					"			get { return AllHolder.all; }",
					"		}",
					"",
					"		class AllHolder",
					"		{",
					"			static AllHolder()",
					"			{",
					"				// Empty constructor to prevent initialisation until first member access.",
					"			}",
					"",
					"			public static readonly SchemaColumnCollection all = new SchemaColumnCollection(PK, new SchemaColumn[]",
					"			{",
					CodeForAddingColumnsToAllCollection,
					"			});",
					"		}",
					"",
					"		#endregion"
					);
			}
		}

		string CodeForAddingColumnsToAllCollection
		{
			get
			{
				if (fCodeForAddingColumnsToAllCollection == null)
				{
					fCodeForAddingColumnsToAllCollection = "";

					foreach (AutoProperty property in Properties)
					{
						if (property.IsComputed)
						{
							continue;
						}

						fCodeForAddingColumnsToAllCollection += "					" + property.ColumnName + "," + System.Environment.NewLine;
					}

					fCodeForAddingColumnsToAllCollection = fCodeForAddingColumnsToAllCollection.TrimEnd(Whitespace);
				}

				return fCodeForAddingColumnsToAllCollection;
			}
		}

		string fCodeForAddingColumnsToAllCollection;

		#endregion

		#region Code for PK

		string CodeForPK
		{
			get
			{
				return LinesOfCode(
					"		#region PK",
					"",
					"		public static readonly SchemaPKColumn PK;",
					"",
					"		#endregion"
					);
			}
		}

		#endregion

		#region Code for Field Definitions

		string CodeForFieldDefinitions
		{
			get
			{
				if (codeForFieldDefinitions == null)
				{
					codeForFieldDefinitions = "";

					foreach (AutoProperty property in Properties)
					{
						codeForFieldDefinitions += CodeForFieldDefinition(property) + System.Environment.NewLine;
					}

					codeForFieldDefinitions = codeForFieldDefinitions.TrimEnd(Whitespace);
				}

				return codeForFieldDefinitions;
			}
		}
		string CodeForFieldDefinition(AutoProperty property)
		{
			string codeForType = property.SchemaColumnType;
			return LinesOfCode(
				"		public static readonly " + codeForType + " " + property.ColumnName + ";");
		}
		string codeForFieldDefinitions;

		#endregion

		#region Code for Field Initialisations

		string CodeForFieldInitialisations
		{
			get
			{
				if (codeForFieldInitialisations == null)
				{
					codeForFieldInitialisations = "";
					foreach (AutoProperty property in Properties)
					{
						codeForFieldInitialisations += CodeForFieldInitialisation(property) + System.Environment.NewLine;
					}

					codeForFieldInitialisations = codeForFieldInitialisations.TrimEnd(Whitespace);
				}

				return codeForFieldInitialisations;
			}
		}

		string CodeForFieldInitialisation(AutoProperty property)
		{
			// Constructors from CargoWise.Schema.sln
			// protected SchemaColumn           (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaPKColumn            (ITableSchema tableSchema, string name,                                                                                                                                       , string index)
			// public SchemaGuidColumn          (ITableSchema tableSchema, string name, int ordinal,                      object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaLongColumn          (ITableSchema tableSchema, string name, int ordinal,                      long   defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaIntColumn           (ITableSchema tableSchema, string name, int ordinal,                      int    defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName, string index)
			// public SchemaShortColumn         (ITableSchema tableSchema, string name, int ordinal,                      short  defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaByteColumn          (ITableSchema tableSchema, string name, int ordinal,                      object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaBoolColumn          (ITableSchema tableSchema, string name, int ordinal,                      bool   defaultValue, bool isNullable, bool isBitField,            bool isLiteralOnly, string tvpName)
			// public SchemaStringColumn        (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable, int maxLength,              bool isLiteralOnly, string tvpName)
			// public SchemaBinaryColumn        (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable, int length,                                     string tvpName)
			// public SchemaDecimalColumn       (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable, byte precision, byte scale, bool isLiteralOnly, string tvpName)
			// public SchemaDateTimeColumn      (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaDateTimeOffsetColumn(ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaXmlColumn           (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable, int maxLength,                                  string tvpName)
			// public SchemaGeographyColumn     (ITableSchema tableSchema, string name, int ordinal,                      object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)
			// public SchemaTimeColumn          (ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable,                             bool isLiteralOnly, string tvpName)

			Func<bool, string> getTrueOrFalseString = isTrue => isTrue ? "true" : "false";

			string columnType = property.SchemaColumnType;
			string defaultValueParam = property.CodeForPropertyDefault;
			string isNullableParam = (property.IsRequiredField) ? "!IsNullable" : "IsNullable";
			string isLiteralOnlyParam = getTrueOrFalseString(property.IsLiteralOnly);
			string isNonBlankFilteredIndexParticipant = getTrueOrFalseString(property.IsNonBlankFilteredIndexParticipant);
			string constructorParameters = "(Instance, Constants." + property.ColumnName + ", column++, ";

			if (property.IsBoolProperty)
			{
				constructorParameters += property.IsSparse || property.IsComputed
					? string.Join(", ", defaultValueParam, isNullableParam, getTrueOrFalseString(property.SqlDbType == SqlDbType.Bit), isLiteralOnlyParam)
					: string.Join(", ", defaultValueParam, getTrueOrFalseString(property.SqlDbType == SqlDbType.Bit), isLiteralOnlyParam);
			}
			else if (property.DataType == typeof(Guid) || property.DataType == typeof(int) || property.DataType == typeof(short) || property.DataType == typeof(byte) || property.DataType == typeof(SqlGeography))
			{
				constructorParameters += string.Join(", ", defaultValueParam, isNullableParam, isLiteralOnlyParam);
			}
			else if (property.DataType == typeof(string))
			{
				if (property.SqlDbType == SqlDbType.Xml)
				{
					constructorParameters += "SqlDbType." + string.Join(", ", property.SqlDbType, defaultValueParam, isNullableParam, property.MaxLength);
				}
				else
				{
					constructorParameters += "SqlDbType." + string.Join(", ", property.SqlDbType, defaultValueParam, isNullableParam, property.MaxLength, isLiteralOnlyParam, isNonBlankFilteredIndexParticipant);
				}
			}
			else if (property.DataType == typeof(byte[]) && (property.SqlDbType == System.Data.SqlDbType.VarBinary || property.SqlDbType == System.Data.SqlDbType.Binary))
			{
				constructorParameters += "SqlDbType." + string.Join(", ", property.SqlDbType, defaultValueParam, isNullableParam, property.MaxLength);
			}
			else if (property.DataType == typeof(decimal))
			{
				constructorParameters += "SqlDbType." + string.Join(", ", property.SqlDbType, defaultValueParam, isNullableParam, property.DecimalPrecision, property.DecimalScale, isLiteralOnlyParam);
			}
			else if (property.DataType == typeof(long))
			{
				constructorParameters += string.Join(", ", defaultValueParam, isNullableParam, isLiteralOnlyParam);
			}
			else if (property.DataType == typeof(DateTimeOffset))
			{
				constructorParameters += "SqlDbType." + string.Join(", ", property.SqlDbType, defaultValueParam, isNullableParam, property.DateTimeOffsetScale, isLiteralOnlyParam);
			}
			else
			{
				constructorParameters += "SqlDbType." + string.Join(", ", property.SqlDbType, defaultValueParam, isNullableParam, isLiteralOnlyParam);
			}

			constructorParameters += ", ";
			var tvpName = TVPHelper.GetTVPName(property.SqlDbType, property.MaxLength, property.DecimalPrecision, property.DecimalScale);
			if (tvpName == TVPHelper.TVP_varchar)
			{
				constructorParameters += FormattableString.Invariant($"{nameof(TVPHelper)}.{nameof(TVPHelper.TVP_varchar)}");
			}
			else if (tvpName == TVPHelper.TVP_nvarchar)
			{
				constructorParameters += FormattableString.Invariant($"{nameof(TVPHelper)}.{nameof(TVPHelper.TVP_nvarchar)}");
			}
			else
			{
				constructorParameters += "\"" + tvpName + "\"";
			}

			if (property.DataType == typeof(Guid))
			{
				constructorParameters += ", " + (property.Index != null ? "true" : "false");
			}

			if (property.IsSparse)
			{
				constructorParameters += ", isSparse: true";
			}

			if (property.IsComputed)
			{
				constructorParameters += ", isComputed: true";
			}

			var smartParameterizationTableOverride = property.SmartParameterizationTableOverride;
			if (!string.IsNullOrEmpty(smartParameterizationTableOverride))
			{
				constructorParameters += $", smartParameterizationTableOverride: \"{smartParameterizationTableOverride}\"";
			}

			var smartParameterizationColumnOverride = property.SmartParameterizationColumnOverride;
			if (!string.IsNullOrEmpty(smartParameterizationColumnOverride))
			{
				constructorParameters += $", smartParameterizationColumnOverride: \"{smartParameterizationColumnOverride}\"";
			}

			constructorParameters += ")";

			return LinesOfCode("			" + property.ColumnName + " = new Schema" + columnType.Substring(6) + constructorParameters + ";");
		}

		string codeForFieldInitialisations;

		#endregion

		#region Code for GetSchemaColumn( ColumnName )

		string CodeForGetSchemaColumn
		{
			get
			{
				if (fCodeForGetSchemaColumn == null)
				{
					fCodeForGetSchemaColumn = LinesOfCode(
						"		#region GetSchemaColumn(ColumnName)",
						"",
						"		internal static SchemaColumn GetSchemaColumn(string ColumnName)",
						"		{",
						"			switch (ColumnName)",
						"			{",
						CodeForGetSchemaColumnCaseStatement,
						"",
						"				default : return null;",
						"			}",
						"		}",
						"",
						"		#endregion");
				}

				return fCodeForGetSchemaColumn;
			}
		}

		string CodeForGetSchemaColumnCaseStatement
		{
			get
			{
				if (fCodeForCaseStatement == null)
				{
					fCodeForCaseStatement = "				case Constants.PK : return PK;" + System.Environment.NewLine + System.Environment.NewLine;

					foreach (AutoProperty property in Properties)
					{
						fCodeForCaseStatement +=
							"				case Constants." + property.ColumnName + " " + property.ColumnNameWhiteSpace +
							": return " + property.ColumnName + ";"
							+ System.Environment.NewLine;
					}

					fCodeForCaseStatement = fCodeForCaseStatement.TrimEnd(Whitespace);
				}

				return fCodeForCaseStatement;
			}
		}

		string fCodeForGetSchemaColumn;
		string fCodeForCaseStatement;

		#endregion

		#region Code for ITableSchema Members

		string CodeForITableSchema
		{
			get
			{
				return
					@"		#region ITableSchema

		public static readonly " + ClassName + @" Instance;

		string ITableSchema.SqlSchemaName => Constants.SqlSchemaName;

		string ITableSchema.TableName => Constants.TableName;

		SchemaPKColumn ITableSchema.PK => PK;

		string ITableSchema.PkIndexName => Constants.PkIndex;

		SchemaColumn ITableSchema.GetSchemaColumn(string ColumnName)
		{
			return GetSchemaColumn(ColumnName);
		}

		SchemaColumnCollection ITableSchema.All => All;

		#endregion";
			}
		}

		#endregion

		string ClassName
		{
			get { return Info.ClassNames.Schema; }
		}

		protected virtual string InheritsFrom => "CargoWise.Schema.Schema, ITableSchema";

		AutoProperty[] Properties
		{
			get { return fProperties ?? (fProperties = new AutoPropertyList(Info).Properties.ToArray()); }
		}

		AutoProperty[] fProperties;
		readonly BusinessObjectInfo Info;
	}
}
