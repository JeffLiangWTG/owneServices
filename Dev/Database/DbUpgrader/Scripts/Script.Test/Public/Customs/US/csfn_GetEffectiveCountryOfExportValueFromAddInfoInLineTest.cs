using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetEffectiveCountryOfExportValueFromAddInfoInLine))]
	class csfn_GetEffectiveCountryOfExportValueFromAddInfoInLineTest : DbCreateScriptTest
	{
		public void TestGetCountryOfExportValueForInvoices()
		{
			var reportSql = @"select Value from csfn_GetEffectiveCountryOfExportValueFromAddInfoInLine(
			@DeclarationAddInfoValue,
			@InvoiceAddInfoValue,
			@InvoiceLineAddInfoValue,
			@BillAddInfoValue,
			@MessageMode,
			@SetIndicatorValue,
			@SecondarySPIValue,
			@ParentInvoiceLineAddInfoValue);";

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice header should be FR, default from declaration", "FR", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice header should be AU, default from bill", "AU", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=CN");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice header should be CN, shouldn't default from declaration", "CN", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=CN");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice header should be CN, shouldn't default from bill", "CN", reader["Value"].ToString());
					}
				}
			}
		}

		public void TestGetCountryOfExportValueForInvoiceLines()
		{
			var reportSql = @"select Value from csfn_GetEffectiveCountryOfExportValueFromAddInfoInLine(
			@DeclarationAddInfoValue,
			@InvoiceAddInfoValue,
			@InvoiceLineAddInfoValue,
			@BillAddInfoValue,
			@MessageMode,
			@SetIndicatorValue,
			@SecondarySPIValue,
			@ParentInvoiceLineAddInfoValue);";

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be FR, default from declaration", "FR", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=KR");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be KR, default from bill", "KR", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=KR");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be AU, default from invoice header", "AU", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be AU, default from invoice header", "AU", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=CN");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be CN, shouldn't default from invoice header or declaration", "CN", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=NZ");
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be NZ, default from parent invoice line", "NZ", reader["Value"].ToString());
					}
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<Guid, string, string>>();
				command.AddParameter("@DeclarationAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=FR");
				command.AddParameter("@InvoiceAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=AU");
				command.AddParameter("@InvoiceLineAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@BillAddInfoValue", SqlDbType.VarChar, "");
				command.AddParameter("@MessageMode", SqlDbType.VarChar, "ACE");
				command.AddParameter("@SetIndicatorValue", SqlDbType.VarChar, "V");
				command.AddParameter("@SecondarySPIValue", SqlDbType.VarChar, "");
				command.AddParameter("@ParentInvoiceLineAddInfoValue", SqlDbType.VarChar, "UC_NKCountryOfExport=NZ");
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("C/E on invoice line should be AU, default from invoice header becasue invoice line is V child line", "AU", reader["Value"].ToString());
					}
				}
			}
		}
	}
}
