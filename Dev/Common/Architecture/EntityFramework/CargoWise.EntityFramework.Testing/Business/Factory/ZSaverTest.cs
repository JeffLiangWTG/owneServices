using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZSaverTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestGetTablesThatNeedToBeSavedIsAffectedByShouldRowBeSaved()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow row = table.NewRow();
			row["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(row);
			ZDataUtils.SetShouldRowBeSaved(row, true);
			ZSaver saver = new ZDoISaveSaver(data);
			AssertEquals("Precondition", 1, saver.GetTablesThatNeedToBeSaved().Length);
			ZDataUtils.SetShouldRowBeSaved(row, false);
			AssertEquals(0, saver.GetTablesThatNeedToBeSaved().Length);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestNoChangeDataSetDoesNotSave()
		{
			DataSet data = new DataSet();
			Assert("No data yet", !new ZDoISaveSaver(data).WillSave());

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OH", typeof(Guid));
			table.Columns.Add("OC_ContactName", typeof(string));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			Assert("No data yet", !new ZDoISaveSaver(data).WillSave());

			DataRow row = table.NewRow();
			row["OC_PK"] = Guid.NewGuid();
			row["OC_ContactName"] = "Original";
			table.Rows.Add(row);

			Assert("One insert", new ZDoISaveSaver(data).WillSave());

			row.AcceptChanges();
			Assert("Changes accepted", !new ZDoISaveSaver(data).WillSave());

			row["OC_ContactName"] = "AChangeIsAsGoodAsAHoliday";
			Assert("Valid change", new ZDoISaveSaver(data).WillSave());

			row["OC_ContactName"] = "Original";
			Assert("No need to save, back to original", !new ZDoISaveSaver(data).WillSave());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestWithNoColsRow()
		{
			DataSet data = new DataSet();
			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			data.Tables.Add(table);
			table.Rows.Add(table.NewRow());
			Assert("No cols", !new ZDoISaveSaver(data).WillSave());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestUnchangedRowsInNonSelfReferentialTablesWillNotBeSaved()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OH", typeof(Guid));
			table.Columns.Add("OC_ContactName", typeof(string));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow unchangedRow = table.NewRow();
			unchangedRow["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(unchangedRow);

			DataRow unchangedRow2 = table.NewRow();
			unchangedRow2["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(unchangedRow2);

			DataRow toSaveRow = table.NewRow();
			toSaveRow["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(toSaveRow);

			DataRow unchangedRow3 = table.NewRow();
			unchangedRow3["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(unchangedRow3);

			DataRow unchangedRow4 = table.NewRow();
			unchangedRow4["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(unchangedRow4);

			DataRow toSaveRow2 = table.NewRow();
			toSaveRow2["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(toSaveRow2);

			unchangedRow4["OC_ContactName"] = "Original";
			data.AcceptChanges(); // pretend we've just loaded the table

			toSaveRow["OC_ContactName"] = "something";
			toSaveRow2["OC_ContactName"] = "something";

			// mess up other rows to try and confuse things
			unchangedRow["OC_ContactName"] = "whatever";
			unchangedRow.AcceptChanges();
			unchangedRow2.BeginEdit();
			unchangedRow3.EndEdit();
			unchangedRow4.AcceptChanges();

			unchangedRow4["OC_ContactName"] = "NotOriginal";
			unchangedRow4["OC_ContactName"] = "Original"; // row state modified, but no real change

			DummyZSaver saver = new DummyZSaver(data);
			IList<DataRow> rows = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			AssertEquals("Only Modified rows with real changes", 2, rows.Count);

			Assert("Includes ToSaveRow", rows.Contains(toSaveRow));
			Assert("Includes ToSaveRow2", rows.Contains(toSaveRow2));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestUnchangedRowsInSelfReferentialTablesWillNotBeSaved()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OH", typeof(Guid));
			table.Columns.Add("OC_OC_First", typeof(Guid));
			table.Columns.Add("OC_OC_Second", typeof(Guid));
			table.Columns.Add("OC_ContactName", typeof(string));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow toSaveRow = table.NewRow();
			toSaveRow["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(toSaveRow);

			DataRow unchangedRow = table.NewRow();
			unchangedRow["OC_PK"] = Guid.NewGuid();
			table.Rows.Add(unchangedRow);
			unchangedRow["OC_ContactName"] = "Original";

			table.AcceptChanges(); // pretend we've just loaded the table

			toSaveRow["OC_ContactName"] = "something";

			unchangedRow["OC_ContactName"] = "NotOriginal";
			unchangedRow["OC_ContactName"] = "Original"; // row state modified, but no real change

			DummyZSaver saver = new DummyZSaver(data);
			IList<DataRow> rows = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			AssertEquals("Only one", 1, rows.Count);
			AssertEquals("Should be ToSaveRow", rows[0], toSaveRow);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestModifiedRowsGoBeforeAddedRows()
		{
			var columnName = OrgContactSchema.OC_ContactName.Name;

			var table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add(columnName, typeof(string));

			var dataSet = new DataSet();
			dataSet.Tables.Add(table);

			var r1 = table.NewRow();
			var r2 = table.NewRow();
			var r3 = table.NewRow();
			var r4 = table.NewRow();
			var r5 = table.NewRow();
			var r6 = table.NewRow();

			table.Rows.Add(r1);
			table.Rows.Add(r2);
			table.Rows.Add(r3);
			table.Rows.Add(r4);
			table.Rows.Add(r5);
			table.Rows.Add(r6);

			r2.AcceptChanges();
			r3.AcceptChanges();
			r6.AcceptChanges();

			r1[columnName] = "c1";
			r2[columnName] = "c2";
			r3[columnName] = "c3";
			r4[columnName] = "c4";
			r5[columnName] = "c5";
			r6[columnName] = "c6";

			AssertEquals("Precondition", DataRowState.Added, r1.RowState);
			AssertEquals("Precondition", DataRowState.Modified, r2.RowState);
			AssertEquals("Precondition", DataRowState.Modified, r3.RowState);
			AssertEquals("Precondition", DataRowState.Added, r4.RowState);
			AssertEquals("Precondition", DataRowState.Added, r5.RowState);
			AssertEquals("Precondition", DataRowState.Modified, r6.RowState);

			var saver = new DummyZSaver(dataSet);
			IList<DataRow> ordered = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			var first3 = ordered.Take(3);
			var last3 = ordered.Reverse().Take(3);

			Converter<DataRow, string> converter = x => x[columnName].ToString();

			AssertContainsExactElementsInAnyOrder(converter, new[] { r2, r3, r6 }, first3);
			AssertContainsExactElementsInAnyOrder(converter, new[] { r1, r4, r5 }, last3);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowOrderingWithMultipleSelfReferentialKeysInOneTable()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OH", typeof(Guid));
			table.Columns.Add("OC_OC_First", typeof(Guid));
			table.Columns.Add("OC_OC_Second", typeof(Guid));
			table.Columns.Add("OC_ContactName", typeof(string));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow noRefs = table.NewRow();
			noRefs["OC_PK"] = Guid.NewGuid();

			DataRow deletedNoRefs = table.NewRow();
			deletedNoRefs["OC_PK"] = Guid.NewGuid();

			DataRow deletedTwoRef = table.NewRow();
			deletedTwoRef["OC_PK"] = Guid.NewGuid();
			deletedTwoRef["OC_OC_First"] = deletedNoRefs["OC_PK"];
			deletedTwoRef["OC_OC_Second"] = noRefs["OC_PK"];

			DataRow oneRef = table.NewRow();
			oneRef["OC_PK"] = Guid.NewGuid();
			oneRef["OC_OC_First"] = noRefs["OC_PK"];

			DataRow twoRef = table.NewRow();
			twoRef["OC_PK"] = Guid.NewGuid();
			twoRef["OC_OC_First"] = noRefs["OC_PK"];
			twoRef["OC_OC_Second"] = oneRef["OC_PK"];

			DataRow anotherTwoRef = table.NewRow();
			anotherTwoRef["OC_PK"] = Guid.NewGuid();
			anotherTwoRef["OC_OC_First"] = noRefs["OC_PK"];
			anotherTwoRef["OC_OC_Second"] = twoRef["OC_PK"];

			// in radomish order
			table.Rows.Add(anotherTwoRef);
			table.Rows.Add(noRefs);
			table.Rows.Add(deletedTwoRef);
			table.Rows.Add(oneRef);
			table.Rows.Add(twoRef);
			table.Rows.Add(deletedNoRefs);

			deletedNoRefs.AcceptChanges();
			deletedNoRefs.Delete();

			deletedTwoRef.AcceptChanges();
			deletedTwoRef.Delete();

			DummyZSaver saver = new DummyZSaver(data);
			IList<DataRow> rows = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			AssertEquals("Order", rows[0], deletedTwoRef);
			AssertEquals("Order", rows[1], deletedNoRefs);
			AssertEquals("Order", rows[2], noRefs);
			AssertEquals("Order", rows[3], oneRef);
			AssertEquals("Order", rows[4], twoRef);
			AssertEquals("Order", rows[5], anotherTwoRef);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestRowOrderingWithSelfReferencingRow()
		{
			var dataSet = CreateSelfRelationDataSet();

			var row2PK = Guid.NewGuid();
			var rowReferencingOtherRow = dataSet.Tables[0].NewRow();
			rowReferencingOtherRow["T0_PK"] = Guid.NewGuid();
			rowReferencingOtherRow["T0_T0"] = row2PK;

			var selfReferencingRow = dataSet.Tables[0].NewRow();
			selfReferencingRow["T0_PK"] = row2PK;
			selfReferencingRow["T0_T0"] = row2PK; // Self referencing, ensure this is not identified as the row having a modified parent

			dataSet.Tables[0].Rows.Add(rowReferencingOtherRow);
			dataSet.Tables[0].Rows.Add(selfReferencingRow);

			var saver = new DummyZSaver(dataSet);
			var rowsInSaveOrder = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("First row should be the self referencing row as it has no parents.", selfReferencingRow, rowsInSaveOrder[0]);
			AssertEquals("Second row should be the row referencing the other row as it has a parent.", rowReferencingOtherRow, rowsInSaveOrder[1]);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestRowOrderingWithSelfReferencingRow_HandlesCircularReferenceInvolvingModifiedRows()
		{
			// ASSUMPTION: PKs will not change for a given row. Maybe we should check this, but there is a performance cost.
			// As the modified rows PK exists in the db, it can be referenced without performing the UPDATE first. This allows us to support some circular references.
			var dataSet = CreateSelfRelationDataSet();

			var modifiedRow = dataSet.Tables[0].NewRow();
			modifiedRow["T0_PK"] = Guid.NewGuid();

			var insertedRow = dataSet.Tables[0].NewRow();
			insertedRow["T0_PK"] = Guid.NewGuid();
			insertedRow["T0_T0"] = modifiedRow["T0_PK"]; // Has parent of the modified row. The PK should not change so we do not need to UPDATE the other row prior to this insert.

			dataSet.Tables[0].Rows.Add(modifiedRow);
			dataSet.Tables[0].Rows.Add(insertedRow);
			modifiedRow.AcceptChanges();

			modifiedRow["T0_T0"] = insertedRow["T0_PK"];
			AssertEquals("Precondition.", DataRowState.Modified, modifiedRow.RowState);

			var saver = new DummyZSaver(dataSet);
			var rowsInSaveOrder = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("Should insert the new row first as its parent already exists in the database and can be referenced.", insertedRow, rowsInSaveOrder[0]);
			AssertEquals("Should set the FK on the modified row next as it has the inserted row as a parent.", modifiedRow, rowsInSaveOrder[1]);
		}

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowOrderingWithSelfReferentialSingleRow_ThrowsNoException()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OC", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow row1 = table.NewRow();
			row1["OC_PK"] = Guid.NewGuid();
			row1["OC_OC"] = row1["OC_PK"];

			table.Rows.Add(row1);

			var schemaResolver = GetNewSchemaResolver();
			var saver = new DummyZSaver(data);
			_ = saver.GetModifiedPersistentRowsInSaveOrder(schemaResolver);
			saver.Save();
			row1.AcceptChanges();
			row1.Delete();

			_ = saver.GetModifiedPersistentRowsInSaveOrder(schemaResolver);
			saver.Save();
		}

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowOrderingWithSelfReferentialLoop_ThrowsNoException()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OC", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow row1 = table.NewRow();
			row1["OC_PK"] = Guid.NewGuid();

			DataRow row2 = table.NewRow();
			row2["OC_PK"] = Guid.NewGuid();
			row2["OC_OC"] = row1["OC_PK"];

			DataRow row3 = table.NewRow();
			row3["OC_PK"] = Guid.NewGuid();
			row3["OC_OC"] = row2["OC_PK"];

			row1["OC_OC"] = row3["OC_PK"];

			table.Rows.Add(row1);
			table.Rows.Add(row2);
			table.Rows.Add(row3);

			var saver = new DummyZSaver(data);
			_ = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			saver.Save();
		}

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowOrderingWithSelfReferentialLoop_WhenDeleting_ThrowsException()
		{
			DataSet data = new DataSet();

			DataTable table = new DataTable(OrgContactSchema.Constants.TableName);
			table.Columns.Add("OC_PK", typeof(Guid));
			table.Columns.Add("OC_OC", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			data.Tables.Add(table);

			DataRow row1 = table.NewRow();
			row1["OC_PK"] = Guid.NewGuid();

			DataRow row2 = table.NewRow();
			row2["OC_PK"] = Guid.NewGuid();
			row2["OC_OC"] = row1["OC_PK"];

			DataRow row3 = table.NewRow();
			row3["OC_PK"] = Guid.NewGuid();
			row3["OC_OC"] = row2["OC_PK"];

			row1["OC_OC"] = row3["OC_PK"];

			table.Rows.Add(row1);
			table.Rows.Add(row2);
			table.Rows.Add(row3);

			var saver = new DummyZSaver(data);
			_ = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			saver.Save();

			row1.AcceptChanges();
			row2.AcceptChanges();
			row3.AcceptChanges();

			row1.Delete();
			row2.Delete();
			row3.Delete();

			saver.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		[ExpectNoExceptions]
		public void TestSimpleSelfReferentialLoop_Deleting()
		{
			var dataSet = CreateSelfRelationDataSet();

			var row1 = dataSet.Tables[0].NewRow();
			row1["T0_PK"] = Guid.NewGuid();

			var row2 = dataSet.Tables[0].NewRow();
			row2["T0_PK"] = Guid.NewGuid();
			row2["T0_T0"] = row1["T0_PK"];

			row1["T0_T0"] = row2["T0_PK"];

			dataSet.Tables[0].Rows.Add(row1);
			dataSet.Tables[0].Rows.Add(row2);

			var saver = new DummyZSaver(dataSet);
			_ = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			saver.Save();

			row1.AcceptChanges();
			row2.AcceptChanges();

			row1.Delete();
			row2.Delete();
			saver.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		[ExpectNoExceptions]
		public void TestComplexSelfReferentialLoop_Deleting()
		{
			var dataSet = CreateSelfRelationDataSet();

			var row1 = dataSet.Tables[0].NewRow();
			row1["T0_PK"] = Guid.NewGuid();

			var row2 = dataSet.Tables[0].NewRow();
			row2["T0_PK"] = Guid.NewGuid();

			row2["T0_T0"] = row1["T0_PK"];
			row1["T0_T0"] = row2["T0_PK"];

			var row3 = dataSet.Tables[0].NewRow();
			row3["T0_PK"] = Guid.NewGuid();

			var row4 = dataSet.Tables[0].NewRow();
			row4["T0_PK"] = Guid.NewGuid();

			row3["T0_T0"] = row1["T0_PK"];
			row4["T0_T0"] = row2["T0_PK"];

			dataSet.Tables[0].Rows.Add(row1);
			dataSet.Tables[0].Rows.Add(row2);
			dataSet.Tables[0].Rows.Add(row3);
			dataSet.Tables[0].Rows.Add(row4);

			var saver = new DummyZSaver(dataSet);
			_ = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			saver.Save();

			row1.AcceptChanges();
			row2.AcceptChanges();
			row3.AcceptChanges();
			row4.AcceptChanges();

			row1.Delete();
			row2.Delete();
			row3.Delete();
			row4.Delete();

			saver.Save();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]

		[ExpectNoExceptions]
		public void TestLongChainSelfReferentialLoop_Deleting()
		{
			var dataSet = CreateSelfRelationDataSet();

			var row1 = dataSet.Tables[0].NewRow();
			row1["T0_PK"] = Guid.NewGuid();

			var row2 = dataSet.Tables[0].NewRow();
			row2["T0_PK"] = Guid.NewGuid();
			row1["T0_T0"] = row2["T0_PK"];

			var row3 = dataSet.Tables[0].NewRow();
			row3["T0_PK"] = Guid.NewGuid();
			row2["T0_T0"] = row3["T0_PK"];

			var row4 = dataSet.Tables[0].NewRow();
			row4["T0_PK"] = Guid.NewGuid();
			row3["T0_T0"] = row4["T0_PK"];

			row4["T0_T0"] = row1["T0_PK"];

			dataSet.Tables[0].Rows.Add(row1);
			dataSet.Tables[0].Rows.Add(row2);
			dataSet.Tables[0].Rows.Add(row3);
			dataSet.Tables[0].Rows.Add(row4);

			var saver = new DummyZSaver(dataSet);
			_ = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			saver.Save();

			row1.AcceptChanges();
			row2.AcceptChanges();
			row3.AcceptChanges();
			row4.AcceptChanges();

			row1.Delete();
			row2.Delete();
			row3.Delete();
			row4.Delete();

			saver.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestComplexRowOrdering()
		{
			DataSet data = new DataSet();

			DataTable parentTable = new DataTable(OrgHeaderSchema.Constants.TableName);
			parentTable.Columns.Add("OH_PK", typeof(Guid));
			parentTable.Columns.Add("OH_Splat", typeof(Guid));
			parentTable.PrimaryKey = new DataColumn[] { parentTable.Columns[0] };

			DataTable childTable = new DataTable(OrgContactSchema.Constants.TableName);
			childTable.Columns.Add("OC_PK", typeof(Guid));
			childTable.Columns.Add("OC_OH", typeof(Guid));
			childTable.Columns.Add("OC_OC", typeof(Guid));
			childTable.Columns.Add("OC_ContactName", typeof(string));
			childTable.PrimaryKey = new DataColumn[] { childTable.Columns[0] };

			DataTable babyTable = new DataTable(OrgMiscServSchema.Constants.TableName);
			babyTable.Columns.Add("OM_PK", typeof(Guid));
			babyTable.Columns.Add("OM_OH", typeof(Guid));
			babyTable.Columns.Add("OM_OC", typeof(Guid));
			babyTable.PrimaryKey = new DataColumn[] { babyTable.Columns[0] };

			data.Tables.Add(childTable);
			data.Tables.Add(parentTable);
			data.Tables.Add(babyTable);

			DataRow parentRow = parentTable.NewRow();
			parentRow["OH_PK"] = Guid.NewGuid();

			DataRow parentDeleteRow = parentTable.NewRow();
			parentDeleteRow["OH_PK"] = Guid.NewGuid();

			DataRow parentAfterUpdatesDeleteRow = parentTable.NewRow();
			parentAfterUpdatesDeleteRow["OH_PK"] = Guid.NewGuid();

			DataRow childRow1 = childTable.NewRow();
			childRow1["OC_PK"] = Guid.NewGuid();
			childRow1["OC_OH"] = parentRow["OH_PK"];

			DataRow babyRow = babyTable.NewRow();
			babyRow["OM_PK"] = Guid.NewGuid();
			babyRow["OM_OH"] = parentAfterUpdatesDeleteRow["OH_PK"];
			babyRow["OM_OC"] = childRow1["OC_PK"];

			DataRow babyDeleteRow = babyTable.NewRow();
			babyDeleteRow["OM_PK"] = Guid.NewGuid();
			babyRow["OM_OH"] = parentAfterUpdatesDeleteRow["OH_PK"];
			babyRow["OM_OC"] = childRow1["OC_PK"];

			DataRow childRow2 = childTable.NewRow();
			childRow2["OC_PK"] = Guid.NewGuid();
			childRow2["OC_OH"] = parentRow["OH_PK"];

			DataRow referencedChildDeleteRow = childTable.NewRow();
			referencedChildDeleteRow["OC_PK"] = Guid.NewGuid();
			referencedChildDeleteRow["OC_OH"] = parentDeleteRow["OH_PK"];

			DataRow childDeleteRow = childTable.NewRow();
			childDeleteRow["OC_PK"] = Guid.NewGuid();
			childDeleteRow["OC_OH"] = parentDeleteRow["OH_PK"];
			childDeleteRow["OC_OC"] = referencedChildDeleteRow["OC_PK"];

			DataRow childRow3 = childTable.NewRow();
			childRow3["OC_PK"] = Guid.NewGuid();
			childRow3["OC_OH"] = parentRow["OH_PK"];
			childRow3["OC_OC"] = referencedChildDeleteRow["OC_PK"];

			childRow1["OC_OC"] = childRow3["OC_PK"];
			childRow2["OC_OC"] = childRow1["OC_PK"];

			// add in randomish order
			babyTable.Rows.Add(babyRow);
			parentTable.Rows.Add(parentAfterUpdatesDeleteRow);
			childTable.Rows.Add(childRow3);
			parentTable.Rows.Add(parentRow);
			childTable.Rows.Add(childRow1);
			childTable.Rows.Add(childRow2);
			parentTable.Rows.Add(parentDeleteRow);
			childTable.Rows.Add(childDeleteRow);
			childTable.Rows.Add(referencedChildDeleteRow);
			babyTable.Rows.Add(babyDeleteRow);

			// normal deletes
			parentDeleteRow.AcceptChanges();
			parentDeleteRow.Delete();
			childDeleteRow.AcceptChanges();
			childDeleteRow.Delete();
			babyDeleteRow.AcceptChanges();
			babyDeleteRow.Delete();

			childRow3.AcceptChanges();
			childRow3["OC_ContactName"] = "yay! i have changes";
			childRow3["OC_OC"] = Guid.Empty; // remove FK to deleted ReferencedChildDeleteRow
			referencedChildDeleteRow.AcceptChanges();
			referencedChildDeleteRow.Delete();

			babyRow.AcceptChanges();
			babyRow["OM_OH"] = Guid.NewGuid(); // remove FK to deleted ParentAfterUpdatesDeleteRow
			parentAfterUpdatesDeleteRow.AcceptChanges();
			parentAfterUpdatesDeleteRow.Delete();

			var saver = new DummyZSaver(data);
			var rows = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			AssertEquals("Right number of rows to be saved", 10, rows.Count);

			AssertEquals("Order", rows[0], babyDeleteRow);
			AssertEquals("Order", rows[1], parentRow);
			AssertEquals("Order", rows[2], childRow3);
			AssertEquals("Order", rows[3], childRow1);
			AssertEquals("Order", rows[4], childRow2);
			AssertEquals("Order", rows[5], babyRow);
			AssertEquals("Order", rows[6], childDeleteRow);
			AssertEquals("Order", rows[7], referencedChildDeleteRow);
			AssertEquals("Order", rows[8], parentAfterUpdatesDeleteRow);
			AssertEquals("Order", rows[9], parentDeleteRow);
		}

		#region TestCorrectRowsChosenToBeSaved

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCorrectRowsChosenToBeSaved()
		{
			DataSet data = new DataSet();
			DataTable table = MakeBasicTable1();
			DataTable table2 = MakeBasicTable2();
			data.Tables.Add(table);
			data.Tables.Add(table2);

			DataRow newRow = table.Rows.Add(new object[] { Guid.NewGuid(), "new row" });

			DataRow updateRow = table.Rows.Add(new object[] { Guid.NewGuid(), "update row" });
			updateRow.AcceptChanges();
			updateRow[1] = "updated now";

			DataRow deleteRow = table.Rows.Add(new object[] { Guid.NewGuid(), "delete row" });
			deleteRow.AcceptChanges();
			deleteRow.Delete();

			DataRow doNothingRow = table.Rows.Add(new object[] { Guid.NewGuid(), "do nothing row" });
			doNothingRow.AcceptChanges();

			DataRow nonPersistentRow = table.Rows.Add(new object[] { Guid.NewGuid(), "non-persist row" });
			ZDataUtils.SetShouldRowBeSaved(nonPersistentRow, false);

			DataRow newRow2 = table2.Rows.Add(new object[] { Guid.NewGuid(), "newOT" });

			var saver = new DummyZSaver(data);
			var rows = saver.GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			AssertEquals("Right number of rows to be saved", 4, rows.Count);
			Assert("Has row that should be saved", rows.Contains(newRow));
			Assert("Has row that should be saved", rows.Contains(updateRow));
			Assert("Has row that should be saved", rows.Contains(deleteRow));
			Assert("Has row that should be saved", rows.Contains(newRow2));
		}

		DataTable MakeBasicTable1()
		{
			DataTable table = new DataTable(DummyBizoSchema.Constants.TableName);
			table.Columns.Add(new DataColumn(DummyBizoSchema.PK.Name, typeof(Guid)));
			table.Columns.Add(new DataColumn(DummyBizoSchema.Z0_Description.Name, typeof(string)));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			return table;
		}

		DataTable MakeBasicTable2()
		{
			DataTable table = new DataTable(DummyDependentBizoSchema.Constants.TableName);
			table.Columns.Add(new DataColumn(DummyDependentBizoSchema.PK.Name, typeof(Guid)));
			table.Columns.Add(new DataColumn(DummyDependentBizoSchema.ZD1_Code.Name, typeof(string)));
			table.Columns.Add(new DataColumn(DummyDependentBizoSchema.ZD1_Z0.Name, typeof(Guid)));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			return table;
		}

		#endregion

		#region Test Single Self Referential FK Ordering

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSelfRelationDeleteOrder()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			testDataSet.AcceptChanges();

			ModifyTestDataSetForDelete(testDataSet);

			var orderedRows = new DummyZSaver(testDataSet).GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("Order", testDataSet.Tables[0].Rows[1], orderedRows[0]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[0], orderedRows[1]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[2], orderedRows[2]);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestSelfRelationInsertOrder()
		{
			var testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);

			var orderedRows = new DummyZSaver(testDataSet).GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("Order", testDataSet.Tables[0].Rows[2], orderedRows[0]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[0], orderedRows[1]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[1], orderedRows[2]);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestSelfRelationUpdateOrder()
		{
			var testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			testDataSet.AcceptChanges();

			ModifyTestDataSetForUpdate(testDataSet);
			var orderedRows = new DummyZSaver(testDataSet).GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());

			// When updating, PK should not change so save order is not important
			AssertEquals("Order", testDataSet.Tables[0].Rows[0], orderedRows[0]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[1], orderedRows[1]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[2], orderedRows[2]);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestSelfRelationInsertUpdateOrder()
		{
			var testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			testDataSet.AcceptChanges(); // Now modified/update

			// Insert new rows
			var pk5 = Guid.NewGuid();
			testDataSet.Tables[0].Rows.Add(new object[] { PK4, Guid.Empty }); // Rows[3]
			testDataSet.Tables[0].Rows.Add(new object[] { pk5, PK4 }); // Rows[4]

			// Update old rows
			testDataSet.Tables[0].Rows[0]["T0_T0"] = PK4;
			testDataSet.Tables[0].Rows[1]["T0_T0"] = pk5;
			testDataSet.Tables[0].Rows[2]["T0_T0"] = Guid.Empty;

			var orderedRows = new DummyZSaver(testDataSet).GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("Order", testDataSet.Tables[0].Rows[2], orderedRows[0]); // Maintain initial order when node has no parents
			AssertEquals("Order", testDataSet.Tables[0].Rows[3], orderedRows[1]); // Maintain initial order when node has no parents
			AssertEquals("Order", testDataSet.Tables[0].Rows[0], orderedRows[2]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[4], orderedRows[3]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[1], orderedRows[4]);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSelfRelationUpdateDeleteOrder()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			testDataSet.AcceptChanges();

			ModifyTestDataSetForUpdateDelete(testDataSet);
			IList<DataRow> orderedRows = new DummyZSaver(testDataSet).GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("Order", testDataSet.Tables[0].Rows[1], orderedRows[0]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[0], orderedRows[1]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[2], orderedRows[2]);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSelfRelationInsertDeleteOrder()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			testDataSet.AcceptChanges();

			ModifyTestDataSetForInsertDelete(testDataSet);
			IList<DataRow> orderedRows = new DummyZSaver(testDataSet).GetModifiedPersistentRowsInSaveOrder(GetNewSchemaResolver());
			AssertEquals("Order", testDataSet.Tables[0].Rows[1], orderedRows[0]);
			AssertEquals("Order", testDataSet.Tables[0].Rows[3], orderedRows[1]);
			AssertEquals("Right number of rows", 2, orderedRows.Count);
		}

		#endregion

		#region Implementation

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataSet CreateSelfRelationDataSet()
		{
			DataSet result = new DataSet();
			DataTable selfRelationDataTable = result.Tables.Add("TestSelfRelationTable");
			DataColumn t0_PKColumn = new DataColumn("T0_PK", typeof(Guid));
			DataColumn t0_T0Column = new DataColumn("T0_T0", typeof(Guid));
			selfRelationDataTable.Columns.Add(t0_PKColumn);
			selfRelationDataTable.Columns.Add(t0_T0Column);
			selfRelationDataTable.PrimaryKey = new DataColumn[] { t0_PKColumn };
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var originalResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			resolver = new Mock<IApplicationSchemaResolver>(MockBehavior.Strict);

			Func<string, ITableSchema> getTableSchema = (string tableName) =>
			{
				switch (tableName)
				{
					case "TestTable1":
						return TestTable1Schema.Instance;
					case "TestTable2":
						return TestTable2Schema.Instance;
					case "TestTable3":
						return TestTable3Schema.Instance;
					case "TestTable4":
						return TestTable4Schema.Instance;
					case "TestSelfRelationTable":
						return TestSelfRelationTableSchema.Instance;
					default:
						return originalResolver.GetTableSchema(tableName);
				}
			};

			Func<string, string, SchemaColumn> getSchemaColumn = (string columnName, string tableName) =>
			{
				SchemaColumn result = null;

				ITableSchema tableSchema = getTableSchema(tableName);
				if (tableSchema != null)
				{
					result = tableSchema.GetSchemaColumn(columnName);
				}
				return result;
			};

			Func<string, string, bool> schemaColumnExists = (string columnName, string tableName) =>
			{
				return getSchemaColumn(columnName, tableName) != null;
			};

			Func<string, SchemaColumnCollection> getSchemaColumns = (string tableName) =>
			{
				ITableSchema tableSchema = getTableSchema(tableName);
				if (tableSchema == null)
				{
					throw new InvalidTableNameException(tableName);
				}
				else
				{
					return tableSchema.All;
				}
			};

			resolver.Setup(x => x.GetSchemaColumn(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			resolver.Setup(x => x.GetSchemaColumnSafe(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			resolver.Setup(x => x.GetSchemaColumns(It.IsAny<string>())).Returns(getSchemaColumns);
			resolver.Setup(x => x.SchemaColumnExists(It.IsAny<string>(), It.IsAny<string>())).Returns(schemaColumnExists);
			resolver.Setup(x => x.GetTableSchema(It.IsAny<string>())).Returns(getTableSchema);
			resolverDisposable = ObjectFactory.Substitute(resolver.Object);
		}

		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class TestSelfRelationTableSchema : Schema.Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static TestSelfRelationTableSchema()
			{
				var column = 0;
				Instance = new TestSelfRelationTableSchema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
				T0_T0 = new SchemaGuidColumn(Instance, Constants.T0_T0, column++, DBNull.Value, IsNullable, false, "dbo.TVP_uniqueidentifier", true);
			}

			TestSelfRelationTableSchema()
			{
			}

			#region Constants

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string TableName = "SelfRelationTable";
				public const string Prefix = "T0";
				public const string PK = "T0_PK";

				public const string T0_T0 = "T0_T0";
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
					T0_T0
				});
			}

			#endregion

			#region PK

			public static readonly SchemaPKColumn PK;

			#endregion

			public static readonly SchemaGuidColumn T0_T0;

			#region GetSchemaColumn(columnName)

			internal static SchemaColumn GetSchemaColumn(string columnName)
			{
				switch (columnName)
				{
					case Constants.PK:
						return PK;

					case Constants.T0_T0:
						return T0_T0;

					default:
						return null;
				}
			}

			#endregion

			#region ITableSchema

			public static readonly TestSelfRelationTableSchema Instance;

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

		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class TestTable1Schema : Schema.Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static TestTable1Schema()
			{
				var column = 0;
				Instance = new TestTable1Schema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
				T1_T2 = new SchemaGuidColumn(Instance, Constants.T1_T2, column++, DBNull.Value, IsNullable, false, "dbo.TVP_uniqueidentifier", true);
				T1_string = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "T1_string", 0, CargoWise.Schema.Schema.GenericStringSchemaColumn.SqlDbType, string.Empty, false, 100);
			}

			TestTable1Schema()
			{
			}

			#region Constants

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string TableName = "TestTable1";
				public const string Prefix = "T1";
				public const string PK = "T1_PK";

				public const string T1_T2 = "T1_T2";
				public const string T1_string = "T1_string";
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
					T1_T2,
					T1_string
				});
			}

			#endregion

			#region PK

			public static readonly SchemaPKColumn PK;

			#endregion

			public static readonly SchemaGuidColumn T1_T2;
			public static readonly SchemaStringColumn T1_string;

			#region GetSchemaColumn(columnName)

			internal static SchemaColumn GetSchemaColumn(string columnName)
			{
				switch (columnName)
				{
					case Constants.PK:
						return PK;

					case Constants.T1_T2:
						return T1_T2;

					case Constants.T1_string:
						return T1_string;

					default:
						return null;
				}
			}

			#endregion

			#region ITableSchema

			public static readonly TestTable1Schema Instance;

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

		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class TestTable2Schema : Schema.Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static TestTable2Schema()
			{
				var column = 0;
				Instance = new TestTable2Schema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
				T2_T3 = new SchemaGuidColumn(Instance, Constants.T2_T3, column++, DBNull.Value, IsNullable, false, "dbo.TVP_uniqueidentifier", true);
			}

			TestTable2Schema()
			{
			}

			#region Constants

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string TableName = "TestTable2";
				public const string Prefix = "T2";
				public const string PK = "T2_PK";

				public const string T2_T3 = "T2_T3";
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
					T2_T3
				});
			}

			#endregion

			#region PK

			public static readonly SchemaPKColumn PK;

			#endregion

			public static readonly SchemaGuidColumn T2_T3;

			#region GetSchemaColumn(columnName)

			internal static SchemaColumn GetSchemaColumn(string columnName)
			{
				switch (columnName)
				{
					case Constants.PK:
						return PK;

					case Constants.T2_T3:
						return T2_T3;

					default:
						return null;
				}
			}

			#endregion

			#region ITableSchema

			public static readonly TestTable2Schema Instance;

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

		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class TestTable3Schema : Schema.Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static TestTable3Schema()
			{
				var column = 0;
				Instance = new TestTable3Schema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
				T3_T4 = new SchemaGuidColumn(Instance, Constants.T3_T4, column++, DBNull.Value, IsNullable, false, "dbo.TVP_uniqueidentifier", true);
			}

			TestTable3Schema()
			{
			}

			#region Constants

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string TableName = "TestTable3";
				public const string Prefix = "T3";
				public const string PK = "T3_PK";

				public const string T3_T4 = "T3_T4";
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
					T3_T4
				});
			}

			#endregion

			#region PK

			public static readonly SchemaPKColumn PK;

			#endregion

			public static readonly SchemaGuidColumn T3_T4;

			#region GetSchemaColumn(columnName)

			internal static SchemaColumn GetSchemaColumn(string columnName)
			{
				switch (columnName)
				{
					case Constants.PK:
						return PK;

					case Constants.T3_T4:
						return T3_T4;

					default:
						return null;
				}
			}

			#endregion

			#region ITableSchema

			public static readonly TestTable3Schema Instance;

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

		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class TestTable4Schema : Schema.Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static TestTable4Schema()
			{
				Instance = new TestTable4Schema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
			}

			TestTable4Schema()
			{
			}

			#region Constants

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string TableName = "TestTable4";
				public const string Prefix = "T4";
				public const string PK = "T4_PK";
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

				public static readonly SchemaColumnCollection all = new SchemaColumnCollection(PK, Array.Empty<SchemaColumn>());
			}

			#endregion

			#region PK

			public static readonly SchemaPKColumn PK;

			#endregion

			#region GetSchemaColumn(columnName)

			internal static SchemaColumn GetSchemaColumn(string columnName)
			{
				switch (columnName)
				{
					case Constants.PK:
						return PK;

					default:
						return null;
				}
			}

			#endregion

			#region ITableSchema

			public static readonly TestTable4Schema Instance;

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

		protected override void TearDown()
		{
			resolverDisposable.Dispose();
		}

		Mock<IApplicationSchemaResolver> resolver;
		IDisposable resolverDisposable;

		readonly Guid PK1 = Guid.NewGuid();
		readonly Guid PK2 = Guid.NewGuid();
		readonly Guid PK3 = Guid.NewGuid();
		readonly Guid PK4 = Guid.NewGuid();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void FillDataSetInsert(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK2, PK1 });
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK3, PK2 });
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK1, DBNull.Value });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForUpdate(DataSet selfRelationDataSet)
		{
			// Row0 -> Row1
			// Row1 -> ____
			// Row2 -> Row0
			selfRelationDataSet.Tables[0].Rows[0]["T0_T0"] = PK3;
			selfRelationDataSet.Tables[0].Rows[1]["T0_T0"] = DBNull.Value;
			selfRelationDataSet.Tables[0].Rows[2]["T0_T0"] = PK2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForDelete(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows[0].Delete();
			selfRelationDataSet.Tables[0].Rows[1].Delete();
			selfRelationDataSet.Tables[0].Rows[2].Delete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForUpdateDelete(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows[0]["T0_T0"] = DBNull.Value;
			selfRelationDataSet.Tables[0].Rows[1].Delete();
			selfRelationDataSet.Tables[0].Rows[2]["T0_T0"] = PK2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForInsertDelete(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows[1].Delete();
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK4, PK2 });
		}

		public class DummyZSaver : ZSaver
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public DummyZSaver(DataSet data)
				: base(data)
			{
			}

			protected override void SaveRows(IList<DataRow> rows)
			{
				if (RowsToThrowConcurrencyException.Count > 0 && RowsToThrowConcurrencyException.Any(rows.Contains))
				{
					throw new ZDataConcurrencyException(null, RowsToThrowConcurrencyException.First(rows.Contains), null);
				}
			}

			protected override bool RowExistsInDatabaseCore(DataRow row)
			{
				return RowsToExistInDataBase.Contains(row);
			}

			public List<DataRow> RowsToExistInDataBase { get; } = new List<DataRow>();

			public List<DataRow> RowsToThrowConcurrencyException { get; } = new List<DataRow>();

			public bool HasOnlyLightValidationChangeForTest(DataRow row) => HasOnlyLightValidationChange(row);

			public bool IsLightValidationColumnForTest(DataColumn column) => IsLightValidationColumn(column);

			public bool RowExistsInDatabaseForConcurrencyHandlingForTest(DataRow row) => RowExistsInDatabaseForConcurrencyHandling(row);
		}

		#endregion
	}

	public sealed class ZSaverTestWithFactory : TestCaseWithFactory
	{
		public void TestShouldRemoveAllSavedRowsIfConcurrencyExceptionThrownAndHasOnlyLightValidationIsTrue()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			((ILightValidationInternals)dummy).IsValid = false;

			Factory.Save();

			dummy.Z0_Code = "CodeA";
			((ILightValidationInternals)dummy).IsValid = true;

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var bankAccount1 = newFactory.New<IAccBankAccount>() as BusinessObject;
			bankAccount1.FillWithValidTestData();
			var bankAccount2 = newFactory.New<IAccBankAccount>() as BusinessObject;
			bankAccount2.FillWithValidTestData();

			var taxRate = newFactory.New<IAccTaxRate>() as BusinessObject;
			taxRate.FillWithValidTestData();
			var newDummy = newFactory.Load<DummyBusinessObject>(dummy.PK);
			((ILightValidationInternals)newDummy).IsValid = true;

			Factory.Save();

			AssertNoExceptionThrown("Should handle concurrency for light validation change only", () => newFactory.Save());
			AssertEquals("Should reset light validation change", false, ((ILightValidationInternals)newDummy).IsValid);
			AssertEquals("Should reset row changes", DataRowState.Unchanged, newDummy.Row.RowState);
			var bankAccount = Factory.Load<IAccBankAccount>(bankAccount1.PK);
			AssertNotNull(bankAccount);
			bankAccount = Factory.Load<IAccBankAccount>(bankAccount2.PK);
			AssertNotNull(bankAccount);
			var taxRateReload = Factory.Load<IAccTaxRate>(taxRate.PK);
			AssertNotNull(taxRateReload);
		}

		public void TestIgnoreConcurrencyForLightValidationChangeOnly()
		{
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1Light = (ILightValidationInternals)dummy1;
			dummy1Light.IsValid = false;
			Factory.Save();

			dummy1Light.IsValid = true;

			var otherDummy1 = otherFactory.Load<DummyBusinessObject>(dummy1.PK);
			((ILightValidationInternals)otherDummy1).IsValid = true;
			otherDummy1.Z0_Number += 2;
			otherFactory.Save();

			AssertNoExceptionThrown("Should handle concurrency for light validation change only", () => Factory.Save());
			AssertEquals("Should reset light validation change", false, dummy1Light.IsValid);
			AssertEquals("Should reset row changes", DataRowState.Unchanged, dummy1.Row.RowState);
		}

		public void TestNotIgnoreConcurrencyForLightValidationWithOtherChanges()
		{
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1Light = (ILightValidationInternals)dummy1;
			dummy1Light.IsValid = false;
			Factory.Save();

			dummy1Light.IsValid = true;
			dummy1.Z0_Number += 1;

			var otherDummy1 = otherFactory.Load<DummyBusinessObject>(dummy1.PK);
			((ILightValidationInternals)otherDummy1).IsValid = true;
			otherDummy1.Z0_Number += 2;
			otherFactory.Save();

			AssertExceptionThrown<ZSaveConcurrencyException>("Should not handle concurrency with other changes", () => Factory.Save());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "<Pending>")]
		public void TestHasOnlyLightValidationChange()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1Light = (ILightValidationInternals)dummy1;
			dummy1Light.IsValid = false;
			dummy1.Row.AcceptChanges();
			dummy1Light.IsValid = true;
			var saver = new ZSaverTest.DummyZSaver(new DataSet());

			AssertEquals(true, saver.HasOnlyLightValidationChangeForTest(dummy1.Row));

			dummy1.Z0_Number += 1;

			AssertEquals(false, saver.HasOnlyLightValidationChangeForTest(dummy1.Row));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "<Pending>")]
		public void TestIsLightValidationColumn()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var saver = new ZSaverTest.DummyZSaver(new DataSet());

			AssertEquals(true, saver.IsLightValidationColumnForTest(dummy1.Row.Table.Columns[DummyBizoSchema.Z0_IsValid.Name]));
			AssertEquals(false, saver.IsLightValidationColumnForTest(dummy1.Row.Table.Columns[DummyBizoSchema.Z0_Number.Name]));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowExistsInDatabaseForConcurrencyHandling()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = factory.NewWithValidTestData<DummyBusinessObject>();

			var saver = new ZSaverTest.DummyZSaver(new DataSet());
			saver.RowsToExistInDataBase.Add(dummy1.Row);

			AssertEquals("Should return false for null row", false, saver.RowExistsInDatabaseForConcurrencyHandlingForTest(null));
			AssertEquals("Should return true for dummy1", true, saver.RowExistsInDatabaseForConcurrencyHandlingForTest(dummy1.Row));
			AssertEquals("Should return false for dummy2", false, saver.RowExistsInDatabaseForConcurrencyHandlingForTest(dummy2.Row));
		}
	}

	class DummyBoWithSource : DummyBusinessObject
	{
		public DummyBoWithSource(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetSourceForTest(IStreamSource source, SchemaColumn column)
		{
			SetSource(source, column);
		}
	}

	#region Test Objects

	class ZDoISaveSaver : ZSaver
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public ZDoISaveSaver(DataSet data)
			: base(data)
		{
			IHaveSaved = false;
		}

		public bool WillSave()
		{
			IHaveSaved = false;
			Save();
			return IHaveSaved;
		}

		bool IHaveSaved;
		protected override void SaveRows(IList<DataRow> rows)
		{
			IHaveSaved = true;
		}

		protected override bool RowExistsInDatabaseCore(DataRow row)
		{
			return true;
		}
	}

	#endregion
}
