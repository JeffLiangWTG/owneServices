using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Business.Testing
{
	public static class ModelViewTestHelper
	{
		public static void AssertAllAddInfoColumnsAreInModelView(BusinessObject bizO,
			string viewName,
			Func<string, bool> predicate = null,
			string schemaTypeName = "Schema")
		{
			var autoBizOType = GetAutoBizOType(bizO.GetType());
			var schemaType = autoBizOType.GetNestedType(schemaTypeName, BindingFlags.Static | BindingFlags.Public);
			var oldTablePrefix = (string)autoBizOType.GetProperty("OldTablePrefix", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
			var fieldNames = schemaType
				.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
				.Where(field => oldTablePrefix == null || !field.Name.StartsWith(oldTablePrefix))
				.Where(field => !field.Name.EndsWith("MaxLength"))
				.Select(field => field.Name);
			if (predicate != null)
			{
				fieldNames = fieldNames.Where(predicate);
			}
			fieldNames = fieldNames.ToHashSet();

			if (!fieldNames.Any())
			{
				Assertion.Assert("No column to check against", condition: true);
				return;
			}

			var tableSchema = bizO switch
			{
				IAddInfoSchemaProvider addInfoSchemaProvider => addInfoSchemaProvider.AddInfoTableSchema,
				IAddInfoManagerWithSchema addInfoManagerWithSchema => addInfoManagerWithSchema.AddInfoSchema,
				_ => null,
			};

			Assert.That(tableSchema, Is.Not.EqualTo(default(ITableSchema)), "Could not find table schema - should not be [null]");

			var schemaColumns = GetSchemaColumns(tableSchema, fieldNames);
			var viewColumns = TestDbViewHelper.GetViewColumns(Db.Connection, viewName, bizO.TableName, isSynonyms: false);

			Assert.That(schemaColumns.Except(viewColumns, new DbColumnEqualityComparer()), Is.EquivalentTo(Array.Empty<string>()).Using(CustomComparers.TypeComparison));
		}

		static Type GetAutoBizOType(Type sourceBizOType)
		{
			var headerType = sourceBizOType;
			while (headerType != null && !headerType.Name.StartsWith("Auto"))
			{
				headerType = headerType.BaseType;
			}

			return headerType;
		}

		static List<TestDbViewHelper.DbColumn> GetSchemaColumns(ITableSchema tableSchema, IEnumerable<string> fields)
		{
			var columns = new List<TestDbViewHelper.DbColumn>();

			foreach (var field in fields)
			{
				if (IsInExclusion(tableSchema.TableName, field))
				{
					continue;
				}

				var schemaColumn = tableSchema.GetSchemaColumn(field);
				Assert.That(schemaColumn, Is.Not.EqualTo(default(SchemaColumn)), $"Field {field} not found in schema - should not be [null]");

				var dbColumn = new TestDbViewHelper.DbColumn(field,
					schemaColumn.SqlDbType,
					schemaColumn.MaxLength,
					GetPrecision(schemaColumn),
					GetScale(schemaColumn));

				columns.Add(dbColumn);
			}
			return columns;
		}

		static int GetScale(SchemaColumn schemaColumn)
		{
			return schemaColumn switch
			{
				SchemaDecimalColumn schemaDecimalColumn => schemaDecimalColumn.Scale,
				SchemaIntColumn _ => 0,
				SchemaShortColumn _ => 0,
				SchemaLongColumn _ => 0,
				_ => -1
			};
		}

		static int GetPrecision(SchemaColumn schemaColumn)
		{
			return schemaColumn switch
			{
				SchemaDecimalColumn schemaDecimalColumn => schemaDecimalColumn.Precision,
				SchemaIntColumn _ => 10,
				SchemaShortColumn _ => 5,
				SchemaLongColumn _ => 19,
				_ => -1
			};
		}

		static bool IsInExclusion(string tableName, string fieldName)
		{
			// This is a temp workaround until "AddInfoChild" logic decommissioned.
			// Then it should be safe to remove the exclusions.
			return ExclusionList.Contains(string.Join(".", tableName, fieldName));
		}

		static readonly ImmutableArray<string> ExclusionList = ImmutableArray.Create("USAddInfo.USI_DRW99ClaimedDuty",
			"USAddInfo.USI_DRW99ClaimedHMF",
			"USAddInfo.USI_DRW99ClaimedMPF",
			"USAddInfo.USI_DRW99ClaimedTax",
			"USAddInfo.USI_DRWImportInvoiceLineNo");

		sealed class DbColumnEqualityComparer : IEqualityComparer<TestDbViewHelper.DbColumn>
		{
			public bool Equals(TestDbViewHelper.DbColumn x, TestDbViewHelper.DbColumn y)
			{
				return GetNormalizedColumnName(x.ColumnName) == GetNormalizedColumnName(y.ColumnName) &&
					   GetNormalizedDataType(x.DataType) == GetNormalizedDataType(y.DataType) &&
					   x.CharacterMaximumLength == y.CharacterMaximumLength &&
					   x.Precision == y.Precision &&
					   x.Scale == y.Scale;
			}

			public int GetHashCode(TestDbViewHelper.DbColumn obj)
			{
				unchecked
				{
					var hashCode = (obj.ColumnName != null ? GetNormalizedColumnName(obj.ColumnName).GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.DataType != null ? GetNormalizedDataType(obj.DataType).GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ obj.CharacterMaximumLength;
					hashCode = (hashCode * 397) ^ obj.Precision;
					hashCode = (hashCode * 397) ^ obj.Scale;
					return hashCode;
				}
			}

			static string GetNormalizedColumnName(string columnName)
			{
				return columnName.Substring(columnName.IndexOf("_", StringComparison.Ordinal) + 1);
			}

			static string GetNormalizedDataType(string dataType)
			{
				if (dataType.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase))
				{
					return "varchar";
				}

				return dataType;
			}
		}
	}
}
