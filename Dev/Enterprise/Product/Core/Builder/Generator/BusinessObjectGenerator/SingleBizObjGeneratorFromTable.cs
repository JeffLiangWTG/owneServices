using System;
using System.Collections.Generic;
using System.Data;

namespace Enterprise.Builder.Generator
{
	public class SingleBizObjGeneratorFromTable : SingleBizObjGenerator
	{
		public SingleBizObjGeneratorFromTable(string fileNameOfBusinessObject, DataTable table, GeneratorOutputDirectory outputDirectory, bool convertZStringToWesternEuropeanCharacters = false)
			: base(fileNameOfBusinessObject, outputDirectory)
		{
			this.fTable = table;
			this.ConvertZStringToWesternEuropeanCharacters = convertZStringToWesternEuropeanCharacters;
		}

		public override DataTable Table
		{
			get { return fTable; }
		}

		protected override bool IsPersistent
		{
			get { return false; }
		}

		protected override Dictionary<string, string> DbTypesFromDatabaseSchema
		{
			get
			{
				if (fDbTypesFromDatabaseSchema == null)
				{
					fDbTypesFromDatabaseSchema = new Dictionary<string, string>();

					foreach (DataColumn column in Table.Columns)
					{
						var dataType = column.DataType;
						var dataTypeName = "";
						if (dataType == typeof(DateTime) && column.ExtendedProperties.ContainsKey("IsDateOnly") && column.ExtendedProperties["IsDateOnly"].ToString().ToUpper() == "Y")
						{
							dataTypeName = "Date";
						}
						else if (column.MaxLength == 1 && dataType == typeof(string) && (column.DefaultValue.ToString() == "N" || column.DefaultValue.ToString() == "Y"))
						{
							dataTypeName = nameof(Boolean);
						}
						else
						{
							dataTypeName = column.DataType.Name;
						}
						fDbTypesFromDatabaseSchema.Add(column.ColumnName, dataTypeName);
					}
				}

				return fDbTypesFromDatabaseSchema;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		protected override DataTable DateTimeOffsetScaleFromDatabaseSchema
		{
			get
			{
				if (dateTimeOffsetScaleFromDatabaseSchema == null)
				{
					dateTimeOffsetScaleFromDatabaseSchema = new DataTable();
					dateTimeOffsetScaleFromDatabaseSchema.Columns.Add("column_name", typeof(string));
					dateTimeOffsetScaleFromDatabaseSchema.Columns.Add("datetime_precision", typeof(byte));

					foreach (DataColumn column in Table.Columns)
					{
						if (column.ExtendedProperties.Contains("Scale"))
						{
							byte scale = (byte)column.ExtendedProperties["Scale"];

							dateTimeOffsetScaleFromDatabaseSchema.Rows.Add(new object[] { column.ColumnName, scale });
						}
					}
				}

				return dateTimeOffsetScaleFromDatabaseSchema;
			}
		}
		DataTable dateTimeOffsetScaleFromDatabaseSchema;

		protected override DataTable DecimalScaleFromDatabaseSchema
		{
			get
			{
				if (fDecimalScaleFromDatabaseSchema == null)
				{
					fDecimalScaleFromDatabaseSchema = new DataTable();
					fDecimalScaleFromDatabaseSchema.Columns.Add("column_name", typeof(string));
					fDecimalScaleFromDatabaseSchema.Columns.Add("numeric_precision", typeof(byte));
					fDecimalScaleFromDatabaseSchema.Columns.Add("numeric_scale", typeof(byte));

					foreach (DataColumn column in Table.Columns)
					{
						if (column.ExtendedProperties.Contains("Precision"))
						{
							byte precision = (byte)column.ExtendedProperties["Precision"];
							byte scale = (byte)column.ExtendedProperties["Scale"];

							fDecimalScaleFromDatabaseSchema.Rows.Add(new object[] { column.ColumnName, precision, scale });
						}
					}
				}

				return fDecimalScaleFromDatabaseSchema;
			}
		}

		readonly DataTable fTable;
		Dictionary<string, string> fDbTypesFromDatabaseSchema;
		DataTable fDecimalScaleFromDatabaseSchema;
	}
}
