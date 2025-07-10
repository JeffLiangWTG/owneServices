using System;
using System.Collections.Generic;
using System.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZRowRelationshipManagerTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestGetSelfReferentialColumnNamesForTable()
		{
			DataSet data = new DataSet();
			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK");
			table.Columns.Add("OC_OH_Splat");
			table.Columns.Add("OC_OHSplat");
			table.Columns.Add("OC_OH");
			table.Columns.Add("OC_OC_RecursiveRelationshipFK1");
			table.Columns.Add("OC_OC_RecursiveRelationshipFK2");
			table.Columns.Add("OC_OC_RecursiveRelationshipFK3");
			table.Columns.Add("OC_Z0_Something");
			table.Columns.Add("OC_Splat");
			table.Columns.Add("OCZ_OHP_NoodleOrComrade(courtesydrjimtcrisp)");
			table.Columns.Add("OH_x");
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			IList<string> fKs = ZRowRelationshipManager.GetSelfReferentialFKsForTable(table);
			AssertEquals("Right number", 3, fKs.Count);
			Assert("Has right FK", fKs.Contains("OC_OC_RecursiveRelationshipFK1"));
			Assert("Has right FK", fKs.Contains("OC_OC_RecursiveRelationshipFK2"));
			Assert("Has right FK", fKs.Contains("OC_OC_RecursiveRelationshipFK3"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestGetSelfReferentialColumnNamesForTableWithNoSelfReferentialKeys()
		{
			DataSet data = new DataSet();
			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK");
			table.Columns.Add("OC_OH_Splat");
			table.Columns.Add("OC_OHSplat");
			table.Columns.Add("OC_OH");
			table.Columns.Add("OC_Z0_Something");
			table.Columns.Add("OC_Splat");
			table.Columns.Add("OCZ_OHP_NoodleOrComrade(courtesydrjimtcrisp)");
			table.Columns.Add("OH_x");
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			string[] fKs = ZRowRelationshipManager.GetSelfReferentialFKsForTable(table);
			AssertEquals("No self relationships", 0, fKs.Length);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRemoveNotNullConstraintsOnFKsAndDates()
		{
			DataTable table = new DataTable("MyFunTable");
			table.Columns.Add("ZZ_Guid", typeof(Guid)).AllowDBNull = false;
			table.Columns.Add("ZZ_Date", typeof(DateTime)).AllowDBNull = false;
			table.Columns.Add("ZZ_String", typeof(string)).AllowDBNull = false;

			DataSet data = new DataSet();
			data.Tables.Add(table);

			ZRowRelationshipManager.RemoveNotNullConstraintsOnFKsAndDates(table);
			Assert("Removed constraint", table.Columns["ZZ_Guid"].AllowDBNull);
			Assert("Removed constraint", table.Columns["ZZ_Guid"].AllowDBNull);
			Assert("Not removed constraint", !table.Columns["ZZ_String"].AllowDBNull);
		}
	}
}
