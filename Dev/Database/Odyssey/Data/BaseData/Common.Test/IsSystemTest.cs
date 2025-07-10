using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefExchangeRateSchema))]
[assembly: UsesConstants(typeof(RefTimeZoneSetSchema))]
[assembly: UsesConstants(typeof(RefLocoMapSchema))]
[assembly: UsesConstants(typeof(ProductionRuleSetSchema))]

namespace Enterprise.DbUpgrader.Data.BaseData.Common
{
	sealed class IsSystemTest : TestCase
	{
		public void TestAllIsSystemColumnsContainTrue()
		{
			string sql = @"With recordset as (
				select C.TABLE_NAME, C.COLUMN_NAME , C.DATA_TYPE, c.TABLE_SCHEMA
				from Information_schema.Columns C
				inner join Information_schema.Tables T on T.TABLE_NAME = C.TABLE_NAME and T.TABLE_SCHEMA = C.TABLE_SCHEMA
				where T.TABLE_TYPE <> 'VIEW' and C.COLUMN_NAME like '%IsSystem'
				)

				Select
						'Select TOP 1  ''' + table_name +  ''',''' +column_name + ''' as ColumnName, '''+table_schema+''' as schemaName From ' + table_schema + '.[' + table_name + '] where ' + column_name + ' = ' + Case When DATA_TYPE = 'bit' Then '0' Else '''N''' End  + ' UNION' SelectCmd
				From recordset";

			var selectCmd = new StringBuilder();
			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					selectCmd.AppendLine(reader.GetString(0));
				}
			}

			selectCmd.Length -= 7; // Trim the final " UNION" from the string
			sql = selectCmd.ToString();

			var expectedTableList = ExceptedTableList;
			var listOfTablesFound = new List<Tuple<string, string, string>>();
			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader.GetString(0);
					var columnName = reader.GetString(1);
					var schemaName = reader.GetString(2);
					if (!expectedTableList.Contains(tableName))
					{
						listOfTablesFound.Add(new Tuple<string, string, string>(tableName, columnName, schemaName));
					}
				}
			}

			if (listOfTablesFound.Count == 0)
			{
				Assert(true);
			}

			else
			{
				var html = new StringBuilder();
				foreach (var tableAndColumn in listOfTablesFound)
				{
					var hasWrittenTableHeaders = false;
					html.Append("<h3>" + tableAndColumn + "</h3> \r\n <table border=1> ");
					using (var reader = Db.Connection.Command("select top 10 * from " + tableAndColumn.Item3 + "." + tableAndColumn.Item1 + " where " + tableAndColumn.Item2 + " = '0'").ExecuteReader())
					{
						while (reader.Read())
						{
							if (!hasWrittenTableHeaders)
							{
								html.Append("<tr>");
								for (int i = 0; i < reader.FieldCount; i++)
								{
									var colName = reader.GetName(i);
									string colour = (colName == tableAndColumn.Item2) ? "style='background-color: yellow'" : "";
									html.Append("<th " + colour + ">" + reader.GetName(i) + "</th>");
								}
								html.Append("</tr>");

								hasWrittenTableHeaders = true;
							}

							html.Append("<tr>");
							for (int i = 0; i < reader.FieldCount; i++)
							{
								html.Append("<td>" + reader[i].ToString() + "</td>");
							}
							html.Append("</tr>");
						}
					}
					html.Append("</table>");
				}
				AssertionWithHtml.HtmlAssert("These tables have XX_IsSystem=0, and here are the top 10 rows in each to help you work out what's wrong. " + html.ToString(), false);
			}
		}

		List<string> ExceptedTableList
		{
			get
			{
				return new List<string>
				{
					RefExchangeRateSchema.Constants.TableName,
					RefTimeZoneSetSchema.Constants.TableName,
					RefLocoMapSchema.Constants.TableName,
					ProductionRuleSetSchema.Constants.TableName, // Excluding this table until default rules are added closer to release.
					RefContainerSchema.Constants.TableName,
					RefPacksSchema.Constants.TableName
				};
			}
		}
	}
}
