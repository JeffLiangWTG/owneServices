using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Moq;
using static Enterprise.DbUpgrader.Schema.Testing.IndexScriptRunnerWithPartitionedTableMockDbsTest;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class IndexScriptRunnerMockDbsTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestDropOldIndexes()
		{
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeRemoved", false, false, 40);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FkRefToBeRemoved", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, true, 50);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 60);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 70);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 0);
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreated");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "_WTG_Ignored", false, false, 0);

			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", false, true, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 0);
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeRemoved");
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "IX_FkRefToBeRemoved");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, false, 55);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 65);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 35);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreated", true, false, 85, filter: null, compression: "PAGE");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude", false, false, 0);
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "_WTG_Ignored");

			IndexScriptRunner testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testScriptRunner.DropOldIndexes);

			// In Main DB and not in the Template - should have been removed
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeRemoved");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_FkRefToBeRemoved");
			// Not in Main DB or in both Main and Template DBs - should NOT have been added, removed or changed
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, true, 50);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 60);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 70);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 0);
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreated");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "_WTG_Ignored", false, false, 0);

			// Spatial Indexes
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				var expected = new string[]
					{
						"SPATIAL INDEX [IX_Spatial] ON [dbo].[ClientSpatial] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
					};
				AssertContainsExactElementsInAnyOrder(expected
					, SpatialIndexLoader.Load(TestConnection, "dbo", "ClientSpatial", null).Select(index => index.Definition));

				expected = new string[]
					{
						"SPATIAL INDEX [IX_Spatial_Modified] ON [dbo].[Spatial_Index] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
					};
				AssertContainsExactElementsInAnyOrder(expected
					, SpatialIndexLoader.Load(TestConnection, "dbo", "Spatial_Index", null).Select(index => index.Definition));
			}
		}

		[UseSnapshotProtection]
		public void TestDropOldIndexes_Xml()
		{
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old", "IX_XmlTypeIssues_Col2", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path", "IX_XmlTypeIssues_Col3_Old", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old", null, null, "u11", "TransportMode11");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New");

			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "PROPERTY");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value", "IX_XmlTypeIssues_Col6_New", "VALUE");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary", null, null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New", null, null, "u11", "TransportMode11");

			IndexScriptRunner testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testScriptRunner.DropOldIndexes);

			//Avoid deadlock, because 'Drop Index' will lock sys.selective_xml_index_paths table. Need to rerun the transaction.
			TestConnection.CommitTransaction();
			TestConnection.BeginTransaction();

			// In Main DB and not in the Template - should have been removed
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old");
			// Not in Main DB or in both Main and Template DBs - should NOT have been added, removed or changed
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New");
		}

		public void TestCreateNewIndexes()
		{
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeRemoved", false, false, 40);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FkRefToBeRemoved", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, true, 50);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 60);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 70);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 0);
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreated");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithFilter");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter1", false, false, 0, "([Col4]<(0))");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter2", false, false, 0, "null");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithoutFilter", false, false, 0, "([Col5]=(0))");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCompressed", false, false, 0, filter: null, compression: null);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Page_2_Row_Compression", false, false, 0, filter: null, compression: "PAGE");

			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", false, true, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 0);
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeRemoved");
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "IX_FkRefToBeRemoved");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, false, 55);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 65);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 35);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreated", true, false, 85, filter: null, compression: "PAGE");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude", false, false, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreatedWithFilter", false, false, 0, filter: "([Col4]>(0))");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter1", false, false, 0, filter: "([Col4]>(0))");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter2", false, false, 0, filter: "([Col4]>(0))");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeUpdatedWithoutFilter", false, false, 0, filter: "null");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCompressed", false, false, 0, filter: null, compression: "PAGE");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_Page_2_Row_Compression", false, false, 0, filter: null, compression: "ROW");

			var logger = new DummyUpgradeManager();
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);

			var testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testScriptRunner.CreateNewIndexes);

			// Not in Main DB and in the Template - should have been added
			// IX_ToBeCreated
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreated", true, false, 85, filter: null, compression: "PAGE");
			string[] keyColumns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ToBeCreated");
			AssertEquals("Number of KEY columns on index Match_IndexDiff.IX_ToBeCreated", 1, keyColumns.Length);
			AssertEquals("1st column of Match_IndexDiff.IX_ToBeCreated", "Col3", keyColumns[0]);
			string[] includeColumns = IndexTestHelper.GetIndexIncludeColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ToBeCreated");
			AssertEquals("Number of INCLUDE columns on index Match_IndexDiff.IX_ToBeCreated", 0, includeColumns.Length);
			// IX_ToBeCreatedWithInclude
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude", false, false, 0);
			keyColumns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertEquals("Number of columns on index Match_IndexDiff.IX_ToBeCreatedWithInclude", 2, keyColumns.Length);
			AssertEquals("1st column of Match_IndexDiff.IX_ToBeCreatedWithInclude", "Col5", keyColumns[0]);
			AssertEquals("2nd column of Match_IndexDiff.IX_ToBeCreatedWithInclude", "Col6", keyColumns[1]);
			includeColumns = IndexTestHelper.GetIndexIncludeColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertEquals("Number of INCLUDE columns on index Match_IndexDiff.IX_ToBeCreatedWithInclude", 1, includeColumns.Length);
			AssertEquals("1st INCLUDE column of Match_IndexDiff.IX_ToBeCreatedWithInclude", "Col1", includeColumns[0]);
			// IX_ToBeCreatedWithFilter
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithFilter", false, false, 0, "([Col4]>(0))");

			// Already in Main DB
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeRemoved", false, false, 40);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, true, 50);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 60);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 70);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter1", false, false, 0, "([Col4]<(0))");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter2", false, false, 0, "null");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithoutFilter", false, false, 0, "([Col5]=(0))");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCompressed", false, false, 0, filter: null, compression: null);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Page_2_Row_Compression", false, false, 0, filter: null, compression: "PAGE");

			// In Main DB - removed by Column sunchroniser
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_FkRefToBeRemoved");

			// Spatial Indexes
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				var expected = new string[]
					{
						"SPATIAL INDEX [IX_Spatial] ON [dbo].[ClientSpatial] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
					};
				AssertContainsExactElementsInAnyOrder(expected
					, SpatialIndexLoader.Load(TestConnection, "dbo", "ClientSpatial", null).Select(index => index.Definition));

				expected = new string[]
					{
						"SPATIAL INDEX [IX_Spatial_Old] ON [dbo].[Spatial_Index] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
						"SPATIAL INDEX [IX_Spatial_Modified] ON [dbo].[Spatial_Index] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
					};
				AssertContainsExactElementsInAnyOrder(expected
					, SpatialIndexLoader.Load(TestConnection, "dbo", "Spatial_Index", null).Select(index => index.Definition));
			}
		}

		[UseSnapshotProtection]
		public void TestCreateNewIndexes_Xml()
		{
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old", "IX_XmlTypeIssues_Col2", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path", "IX_XmlTypeIssues_Col3_Old", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old", null, null, "u11", "TransportMode11");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New");

			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "PROPERTY");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value", "IX_XmlTypeIssues_Col6_New", "VALUE");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary", null, null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New", null, null, "u11", "TransportMode11");

			var logger = new DummyUpgradeManager();
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);

			var testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testScriptRunner.CreateNewIndexes);

			//Avoid deadlock, because 'Drop Index' will lock sys.selective_xml_index_paths table. Need to rerun the transaction.
			TestConnection.CommitTransaction();
			TestConnection.BeginTransaction();

			// Not in Main DB and in the Template - should have been added
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New", "IX_XmlTypeIssues_Col2", "VALUE");
			string[] columns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New");
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Col2_New column count", 1, columns.Length);
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Col2_New column", "Col2", columns[0]);

			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value", "IX_XmlTypeIssues_Col6_New", "VALUE");
			columns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New");
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Col6_New column count", 1, columns.Length);
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Col6_New column", "Col6", columns[0]);

			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary", null, null);
			columns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary");
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_NewPrimary column count", 1, columns.Length);
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_NewPrimary column", "Col7", columns[0]);

			// In Main DB - should NOT have been added, removed or changed
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old", "IX_XmlTypeIssues_Col2", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path", "IX_XmlTypeIssues_Col3_Old", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old", null, null, "u11", "TransportMode11");

			// In Main DB - should have been added
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New", null, null, "u11", "TransportMode11");
		}

		public void TestRecreateModifiedIndexes()
		{
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeRemoved", false, false, 40);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FkRefToBeRemoved", true, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, true, 50);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 60);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 70);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 0);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_IncludeColListChanged", false, false, 0);
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreated");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithFilter");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter1", false, false, 0, "([Col4]<(0))");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter2", false, false, 0, "null");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithoutFilter", false, false, 0, "([Col5]=(0))");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCompressed", false, false, 0, filter: null, compression: null);
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Page_2_Row_Compression", false, false, 0, filter: null, compression: "PAGE");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Page_2_None_Compression", false, false, 0, compression: "PAGE");

			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", false, true, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 0);
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeRemoved");
			AssertIndexNotExistInTestTemplateDb("Match_IndexDiff", "IX_FkRefToBeRemoved");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, false, 55);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 65);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 35);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreated", true, false, 85, filter: null, compression: "PAGE");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude", false, false, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_IncludeColListChanged", false, false, 0);
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCreatedWithFilter", false, false, 0, "([Col4]>(0))");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter1", false, false, 0, "([Col4]>(0))");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter2", false, false, 0, "([Col4]>(0))");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeUpdatedWithoutFilter", false, false, 0, "null");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_ToBeCompressed", false, false, 0, filter: null, compression: "PAGE");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_Page_2_Row_Compression", false, false, 0, filter: null, compression: "ROW");
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_Page_2_None_Compression", false, false, 0, compression: "");

			var logger = new DummyUpgradeManager();
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);

			var testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testScriptRunner.RecreateModifiedIndexes);

			// Both in Main DB and the Template with different KEY column lists - should have been re-created using new column-lists
			// IX_ColListChangeOnly - fillfactor not defined in the template, so it remains unchanged
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			string[] indexColListChangeOnlyKeys = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ColListChangeOnly");
			AssertEquals("Number of KEY columns on index Match_IndexDiff.IX_ColListChangeOnly", 2, indexColListChangeOnlyKeys.Length);
			AssertEquals("1st key column of Match_IndexDiff.IX_ColListChangeOnly", "Col3", indexColListChangeOnlyKeys[0]);
			AssertEquals("2nd key column of Match_IndexDiff.IX_ColListChangeOnly", "Col2", indexColListChangeOnlyKeys[1]);
			string[] indexColListChangeOnlyIncludeColumns = IndexTestHelper.GetIndexIncludeColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ColListChangeOnly");
			AssertEquals("Number of INCLUDE columns on index Match_IndexDiff.IX_ColListChangeOnly", 1, indexColListChangeOnlyIncludeColumns.Length);
			AssertEquals("1st include column of Match_IndexDiff.IX_ColListChangeOnly", "Col4", indexColListChangeOnlyIncludeColumns[0]);

			// Both in Main DB and the Template with different INCLUDE column lists - should have been re-created using new column-lists
			// IX_IncludeColListChanged
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_IncludeColListChanged", false, false, 0);
			string[] indexIncludeListChangeKeys = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_IncludeColListChanged");
			AssertEquals("Number of KEY columns on index Match_IndexDiff.IX_IncludeColListChanged", 1, indexIncludeListChangeKeys.Length);
			AssertEquals("1st key column of Match_IndexDiff.IX_IncludeColListChanged", "Col6", indexIncludeListChangeKeys[0]);
			string[] indexIncludeListChangeIncludeColumns = IndexTestHelper.GetIndexIncludeColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_IncludeColListChanged");
			AssertEquals("Number of INCLUDE columns on index Match_IndexDiff.IX_IncludeColListChanged", 3, indexIncludeListChangeIncludeColumns.Length);
			AssertEquals("1st include column of Match_IndexDiff.IX_IncludeColListChanged", "Col3", indexIncludeListChangeIncludeColumns[0]);
			AssertEquals("2nd include column of Match_IndexDiff.IX_IncludeColListChanged", "Col4", indexIncludeListChangeIncludeColumns[1]);
			AssertEquals("3rd include column of Match_IndexDiff.IX_IncludeColListChanged", "Col5", indexIncludeListChangeIncludeColumns[2]);

			// Both in Main DB and the Template with different FILTER definition - should have been re-created using new filter
			// IX_ToBeUpdatedWithFilter1
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter1", false, false, 0, "([Col4]>(0))");
			// IX_ToBeUpdatedWithFilter2
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithFilter2", false, false, 0, "([Col4]>(0))");
			// IX_ToBeUpdatedWithoutFilter
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeUpdatedWithoutFilter", false, false, 0, "null");

			// Both in Main DB and the Template with different attributes - should have been re-created using new attributes
			// IX_UniqNonclst_2_NonuniqClst
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst", false, true, 0);
			string[] indexUniqNonclst_2_NonuniqClstColumns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_UniqNonclst_2_NonuniqClst");
			AssertEquals("Number of columns on index Match_IndexDiff.IX_UniqNonclst_2_NonuniqClst", 1, indexUniqNonclst_2_NonuniqClstColumns.Length);
			AssertEquals("1st column of Match_IndexDiff.IX_UniqNonclst_2_NonuniqClst", "Col4", indexUniqNonclst_2_NonuniqClstColumns[0]);
			// IX_Clst_2_Nonclst
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Clst_2_Nonclst", false, false, 55);
			string[] indexClst_2_NonclstColumns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_Clst_2_Nonclst");
			AssertEquals("Number of columns on index Match_IndexDiff.IX_Clst_2_Nonclst", 1, indexClst_2_NonclstColumns.Length);
			AssertEquals("1st column of Match_IndexDiff.IX_Clst_2_Nonclst", "Col5", indexClst_2_NonclstColumns[0]);
			// IX_FillFactorAndOtherChanges
			AssertIndexExistInTestTemplateDb("Match_IndexDiff", "IX_FillFactorAndOtherChanges", false, false, 35);
			string[] indexFillFactorAndOtherChangesColumns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_FillFactorAndOtherChanges");
			AssertEquals("Number of columns on index Match_IndexDiff.IX_FillFactorAndOtherChanges", 2, indexFillFactorAndOtherChangesColumns.Length);
			AssertEquals("1st column of Match_IndexDiff.IX_FillFactorAndOtherChanges", "Col3", indexFillFactorAndOtherChangesColumns[0]);
			AssertEquals("2nd column of Match_IndexDiff.IX_FillFactorAndOtherChanges", "Col6", indexFillFactorAndOtherChangesColumns[1]);

			// Both in Main DB and the Template with fill factor only changes - template fillfactor applied
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_FillFactorChangeOnly", false, false, 65);

			// In Main DB with a fillfactor and in the Template with no fill factor specified - index remains unchanged (SHOULD NOT BE RECREATED)
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_NoTemplateFillFactor", false, false, 70);
			var sqlText = String.Format("SELECT is_padded FROM [{0}].sys.indexes WHERE name = '{1}'", mockMainDb, "IX_NoTemplateFillFactor");
			AssertEquals("IsPadded (used to check if index was recreated)", 1, Convert.ToInt32(TestConnection.ExecuteScalar(sqlText)));

			// Both in Main DB and Template with compression changes
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeCompressed", false, false, 0, filter: null, compression: "PAGE");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Page_2_Row_Compression", false, false, 0, filter: null, compression: "ROW");
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_Page_2_None_Compression", false, false, 0, compression: "");

			// Either not in Main DB or in the Template - should NOT have been added, removed or changed
			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ToBeRemoved", false, false, 40);
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreated");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithInclude");
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_ToBeCreatedWithFilter");

			// In Main DB - removed by Column synchroniser
			AssertIndexNotExistInTestMainDb("Match_IndexDiff", "IX_FkRefToBeRemoved");

			// Spatial Indexes
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				var expected = new string[]
					{
						"SPATIAL INDEX [IX_Spatial] ON [dbo].[ClientSpatial] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
					};
				AssertContainsExactElementsInAnyOrder(expected
					, SpatialIndexLoader.Load(TestConnection, "dbo", "ClientSpatial", null).Select(index => index.Definition));

				expected = new string[]
					{
						"SPATIAL INDEX [IX_Spatial_Old] ON [dbo].[Spatial_Index] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
						"SPATIAL INDEX [IX_Spatial_Modified] ON [dbo].[Spatial_Index] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
					};
				AssertContainsExactElementsInAnyOrder(expected
					, SpatialIndexLoader.Load(TestConnection, "dbo", "Spatial_Index", null).Select(index => index.Definition));
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateModifiedIndexes_Xml()
		{
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old", "IX_XmlTypeIssues_Col2", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path", "IX_XmlTypeIssues_Col3_Old", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old", null, null, "u11", "TransportMode11");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col4_Namespaces", null, null, "u10", "TransportMode10");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col5_Paths", null, null, "u11", "TransportMode10");

			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "PROPERTY");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value", "IX_XmlTypeIssues_Col6_New", "VALUE");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary", null, null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New", null, null, "u11", "TransportMode11");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col4_Namespaces", null, null, "u11", "TransportMode10");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockTemplateDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col5_Paths", null, null, "u11", "TransportMode11");

			var logger = new DummyUpgradeManager();
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);

			IndexScriptRunner testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			testScriptRunner.SetSelectiveXMLIndexesChangedFilter();
			RunActionOnMockMainDb(testScriptRunner.RecreateModifiedIndexes);

			//Avoid deadlock, because 'Drop Index' will lock sys.selective_xml_index_paths table. Need to rerun the transaction.
			TestConnection.CommitTransaction();
			TestConnection.BeginTransaction();

			// Both in Main DB and the Template with different key column lists - should have been re-created using new column-lists
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			string[] columns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing");
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Changing column count", 1, columns.Length);
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Changing column", "Col5", columns[0]);

			AssertIndexExistInTestMainDb("Match_IndexDiff", "IX_ColListChangeOnly", false, false, 30);
			string[] indexColListChangeOnlyColumns = IndexTestHelper.GetIndexKeyColumnsInOrder(TestConnection, mockMainDb, "Match_IndexDiff", "IX_ColListChangeOnly");
			AssertEquals("Number of columns on index Match_IndexDiff.IX_ColListChangeOnly", 2, indexColListChangeOnlyColumns.Length);
			AssertEquals("1st column of Match_IndexDiff.IX_ColListChangeOnly", "Col3", indexColListChangeOnlyColumns[0]);
			AssertEquals("2nd column of Match_IndexDiff.IX_ColListChangeOnly", "Col2", indexColListChangeOnlyColumns[1]);

			// Both in Main DB and the Template with different attributes - should have been re-created using new attributes
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "PROPERTY");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col4_Namespaces", null, null, "u11", "TransportMode10");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col5_Paths", null, null, "u11", "TransportMode11");

			// Either not in Main DB or in the Template - should NOT have been added, removed or changed
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old", "IX_XmlTypeIssues_Col2", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old", null, null);
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path", "IX_XmlTypeIssues_Col3_Old", "PATH");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col6_New_Value");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary");
			IndexTestHelper.AssertXmlIndexExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col2_Old", null, null, "u11", "TransportMode11");
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "XmlTypeIssues", "UX_XmlTypeIssues_Col3_New");
		}

		#region On-line indexes

		public void TestOnLineIndexes_Clustered()
		{
			var fullTableName = "[dbo].[Match_IndexDiff]";

			var expected_main = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX PK_Match_IndexDiff ON dbo.Match_IndexDiff (Col1);",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_UniqNonclst_2_NonuniqClst ON dbo.Match_IndexDiff (Col4);",
					"CREATE        NONCLUSTERED INDEX IX_ColListChangeOnly ON dbo.Match_IndexDiff (Col2) INCLUDE (Col4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeRemoved ON dbo.Match_IndexDiff (Col3) WITH (FILLFACTOR = 40);",
					"CREATE           CLUSTERED INDEX IX_Clst_2_Nonclst ON dbo.Match_IndexDiff (Col5) WITH (FILLFACTOR = 50);",
					"CREATE        NONCLUSTERED INDEX IX_FillFactorChangeOnly ON dbo.Match_IndexDiff (Col3, Col4) WITH (FILLFACTOR = 60);",
					"CREATE        NONCLUSTERED INDEX IX_NoTemplateFillFactor ON dbo.Match_IndexDiff (Col3, Col4, Col5) WITH (FILLFACTOR = 70);",
					"CREATE        NONCLUSTERED INDEX IX_FillFactorAndOtherChanges ON dbo.Match_IndexDiff (Col3, Col5);",
					"CREATE        NONCLUSTERED INDEX IX_IncludeColListChanged ON dbo.Match_IndexDiff (Col6) INCLUDE (Col2, Col4);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter1 ON dbo.Match_IndexDiff (Col1, Col3) INCLUDE (Col2) WHERE ([Col4]<(0));",
					"CREATE        NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter2 ON dbo.Match_IndexDiff (Col1, Col5) INCLUDE (Col2);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeUpdatedWithoutFilter ON dbo.Match_IndexDiff (Col1, Col4) INCLUDE (Col6) WHERE ([Col5]=(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_FkRefToBeRemoved ON dbo.Match_IndexDiff (Col7);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeCompressed ON dbo.Match_IndexDiff (Col6);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_Row_Compression ON dbo.Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = PAGE);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_None_Compression ON dbo.Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = PAGE);",
					"CREATE        NONCLUSTERED INDEX _WTG_Ignored ON dbo.Match_IndexDiff (Col1) INCLUDE (Col2);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_Option_1 ON dbo.Match_IndexDiff (Col6);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_Option_2 ON dbo.Match_IndexDiff (Col6);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Main:", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			var expected_template = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX PK_Match_IndexDiff ON dbo.Match_IndexDiff (Col1);",
					"CREATE           CLUSTERED INDEX IX_UniqNonclst_2_NonuniqClst ON dbo.Match_IndexDiff (Col4);",
					"CREATE        NONCLUSTERED INDEX IX_ColListChangeOnly ON dbo.Match_IndexDiff (Col3, Col2) INCLUDE (Col4);",
					"CREATE        NONCLUSTERED INDEX IX_Clst_2_Nonclst ON dbo.Match_IndexDiff (Col5) WITH (FILLFACTOR = 55);",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_ToBeCreated ON dbo.Match_IndexDiff (Col3) WITH (FILLFACTOR = 85, DATA_COMPRESSION = PAGE);",
					"CREATE        NONCLUSTERED INDEX IX_FillFactorChangeOnly ON dbo.Match_IndexDiff (Col3, Col4) WITH (FILLFACTOR = 65);",
					"CREATE        NONCLUSTERED INDEX IX_NoTemplateFillFactor ON dbo.Match_IndexDiff (Col3, Col4, Col5);",
					"CREATE        NONCLUSTERED INDEX IX_FillFactorAndOtherChanges ON dbo.Match_IndexDiff (Col3, Col6) WITH (FILLFACTOR = 35);",
					"CREATE        NONCLUSTERED INDEX IX_IncludeColListChanged ON dbo.Match_IndexDiff (Col6) INCLUDE (Col3, Col4, Col5);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeCreatedWithInclude ON dbo.Match_IndexDiff (Col5, Col6) INCLUDE (Col1);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeCreatedWithFilter ON dbo.Match_IndexDiff (Col1, Col2) INCLUDE (Col3) WHERE ([Col4]>(0));",
					"CREATE        NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter1 ON dbo.Match_IndexDiff (Col1, Col3) INCLUDE (Col2) WHERE ([Col4]>(0));",
					"CREATE        NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter2 ON dbo.Match_IndexDiff (Col1, Col5) INCLUDE (Col2) WHERE ([Col4]>(0));",
					"CREATE        NONCLUSTERED INDEX IX_ToBeUpdatedWithoutFilter ON dbo.Match_IndexDiff (Col1, Col4) INCLUDE (Col6);",
					"CREATE        NONCLUSTERED INDEX IX_ToBeCompressed ON dbo.Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = PAGE);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_Row_Compression ON dbo.Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = ROW);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_None_Compression ON dbo.Match_IndexDiff (Col6);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_Option_1 ON dbo.Match_IndexDiff (Col6);",
					"CREATE        NONCLUSTERED INDEX IX_Page_2_Option_2 ON dbo.Match_IndexDiff (Col6);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Template:", expected_template, IndexTestHelper.GetIndexDefinitions(TestConnection, mockTemplateDb, fullTableName));

			// No ON-line indexes added because there is CLUSTERED <-> NONCLUSTERED index changes
			var logger = new DummyUpgradeManager();
			var indexSynchroniser = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, logger);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewOnLineIndexes);
			AssertContainsExactElementsInAnyOrder("CreateNewOnLineIndexes", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
			RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);
			AssertContainsExactElementsInAnyOrder("CreateNewOnLineIndexes", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
			RunActionOnMockMainDb(indexSynchroniser.SynchroniseOnlineIndexes);
			AssertContainsExactElementsInAnyOrder("SynchroniseOnlineIndexes", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
		}

		public void TestOnLineIndexes_NonClustered()
		{
			var fullTableName = "[dbo].[Match_OnlineIndex]";

			// Initial setup

			var expected_main = new string[]
				{
					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",
					"CREATE        NONCLUSTERED INDEX MOI_ToBeRemoved ON dbo.Match_OnlineIndex (Col_3) INCLUDE (Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_All ON dbo.Match_OnlineIndex (Col_5, Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 60);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3, Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2) WHERE ([Col_3]='N');",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Main:", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			var expected_template = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX PK_Match_OnlineIndex ON dbo.Match_OnlineIndex (Col_1);",

					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",
					"CREATE        NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE UNIQUE NONCLUSTERED INDEX MOI_New_Unique ON dbo.Match_OnlineIndex (Col_2);",
					"CREATE        NONCLUSTERED INDEX MOI_New ON dbo.Match_OnlineIndex (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Key ON dbo.Match_OnlineIndex (Col_4, Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Filter ON dbo.Match_OnlineIndex (Col_4) WHERE ([Col_3]=(0));",
					"CREATE        NONCLUSTERED INDEX MOI_New_Missed_Key ON dbo.Match_OnlineIndex (Col_6);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Missed_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_6);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Missed_Filter ON dbo.Match_OnlineIndex (Col_4) WHERE ([Col_5]=(0) AND [Col_6]=(0));",

					"CREATE        NONCLUSTERED INDEX MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_All ON dbo.Match_OnlineIndex (Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, Col_3) INCLUDE (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4) WHERE ([Col_3]=(0));",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Template:", expected_template, IndexTestHelper.GetIndexDefinitions(TestConnection, mockTemplateDb, fullTableName));

			// ON-line. Index synchroniser
			var logger = new DummyUpgradeManager();
			var indexSynchroniser = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, logger);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewOnLineIndexes);

			expected_main = new string[]
				{
					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",
					"CREATE        NONCLUSTERED INDEX MOI_ToBeRemoved ON dbo.Match_OnlineIndex (Col_3) INCLUDE (Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_All ON dbo.Match_OnlineIndex (Col_5, Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 60);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3, Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2) WHERE ([Col_3]='N');",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_New ON dbo.Match_OnlineIndex (Col_4);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_New_Key ON dbo.Match_OnlineIndex (Col_4, _2_Col_3);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_New_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (_2_Col_3);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Recreated ON dbo.Match_OnlineIndex (_2_Col_3);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_All ON dbo.Match_OnlineIndex (_2_Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, _2_Col_3) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (_2_Col_3, Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (_2_Col_3);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Compressed ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("ON-line. Index synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);
			expected_main = new string[]
				{
					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",
					"CREATE        NONCLUSTERED INDEX MOI_ToBeRemoved ON dbo.Match_OnlineIndex (Col_3) INCLUDE (Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_All ON dbo.Match_OnlineIndex (Col_5, Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 60);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3, Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2) WHERE ([Col_3]='N');",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MOI_New_Unique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_New ON dbo.Match_OnlineIndex (Col_4);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_New_Key ON dbo.Match_OnlineIndex (Col_4, _2_Col_3);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_New_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (_2_Col_3);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Recreated ON dbo.Match_OnlineIndex (_2_Col_3);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_All ON dbo.Match_OnlineIndex (_2_Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, _2_Col_3) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (_2_Col_3, Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (_2_Col_3);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Compressed ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);",
				};
			AssertContainsExactElementsInAnyOrder("ON-line. Unique nonclustered index synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Column synchroniser
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);

			expected_main = new string[]
				{
					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4);",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_New ON dbo.Match_OnlineIndex (Col_4);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_New_Key ON dbo.Match_OnlineIndex (Col_4, Col_3);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_New_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_3);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MOI_New_Unique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_All ON dbo.Match_OnlineIndex (Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, Col_3) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3);",
					"CREATE        NONCLUSTERED INDEX _on_MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);",

					"CREATE        NONCLUSTERED INDEX _on_MOI_Compressed ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Column synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Synchronising on-line indexes
			RunActionOnMockMainDb(indexSynchroniser.SynchroniseOnlineIndexes);

			expected_main = new string[]
				{
					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4);",

					"CREATE        NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_New ON dbo.Match_OnlineIndex (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Key ON dbo.Match_OnlineIndex (Col_4, Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_3);",

					"CREATE        NONCLUSTERED INDEX MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX MOI_New_Unique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_All ON dbo.Match_OnlineIndex (Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, Col_3) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Synchronising on-line indexes", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Index synchroniser
			RunActionOnMockMainDb(indexSynchroniser.DropOldIndexes);
			RunActionOnMockMainDb(indexSynchroniser.RecreateModifiedIndexes);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewIndexes);

			// rename variable
			var expected_main_after_offline_index_sync = new string[]
				{
					// -- TEMPLATE {CREATE UNIQUE NONCLUSTERED INDEX PK_Match_OnlineIndex ON dbo.Match_OnlineIndex (Col_1);}
					// -- MAIN DB AFTER OFFLINE INDEX SYNCHRONISATION => Nothing. Will be added by Constraint synchroniser.

					"CREATE        NONCLUSTERED INDEX MOI_Unchanged ON dbo.Match_OnlineIndex (Col_5);",
					"CREATE        NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);",

					"CREATE UNIQUE NONCLUSTERED INDEX MOI_New_Unique ON dbo.Match_OnlineIndex (Col_2);",
					"CREATE        NONCLUSTERED INDEX MOI_New ON dbo.Match_OnlineIndex (Col_4);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Key ON dbo.Match_OnlineIndex (Col_4, Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Filter ON dbo.Match_OnlineIndex (Col_4) WHERE ([Col_3]=(0));",
					"CREATE        NONCLUSTERED INDEX MOI_New_Missed_Key ON dbo.Match_OnlineIndex (Col_6);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Missed_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_6);",
					"CREATE        NONCLUSTERED INDEX MOI_New_Missed_Filter ON dbo.Match_OnlineIndex (Col_4) WHERE ([Col_5]=(0) AND [Col_6]=(0));",

					"CREATE        NONCLUSTERED INDEX MOI_Recreated ON dbo.Match_OnlineIndex (Col_3);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_All ON dbo.Match_OnlineIndex (Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);",

					// -- TEMPLATE {CREATE NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, Col_3) INCLUDE (Col_4);}
					// -- MAIN DB AFTER OFFLINE INDEX SYNCHRONISATION => Uses fill factor of db being upgraded.
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Key ON dbo.Match_OnlineIndex (Col_2, Col_3) INCLUDE (Col_4) WITH (FILLFACTOR = 30);",

					"CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 30);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Include ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3);",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4) WHERE ([Col_3]=(0));",
					"CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2 ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);",

					"CREATE        NONCLUSTERED INDEX MOI_Compressed ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Index synchroniser", expected_main_after_offline_index_sync, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
		}

		public void TestCreateUniqueIndexesOnline_NonClustered()
		{
			var fullTableName = "[dbo].[Match_UniqueIndex]";

			// Initial setup

			var expected_main = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_Unique ON dbo.Match_UniqueIndex (Col_1, Col_2 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_UniqueWithFilter ON dbo.Match_UniqueIndex (Col_4, Col_5) INCLUDE (Col_2) WHERE ([Col_1]>(0));",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Main:", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			var expected_template = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX PK_Match_UniqueIndex ON dbo.Match_UniqueIndex (Col_1);",

					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated1 ON dbo.Match_UniqueIndex (Col_1, Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated2 ON dbo.Match_UniqueIndex (Col_2 DESC, Col_1 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated4 ON dbo.Match_UniqueIndex (Col_4 DESC, Col_2, Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_5]=(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_4, Col_5) WHERE ([Col_1]>(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated9 ON dbo.Match_UniqueIndex (Col_1, Col_2) WITH (DATA_COMPRESSION = ROW);",

					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated1 ON dbo.Match_UniqueIndex (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated2 ON dbo.Match_UniqueIndex (Col_5, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_7);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated4 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_6);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_7]>(10));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_3]=(42));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Template:", expected_template, IndexTestHelper.GetIndexDefinitions(TestConnection, mockTemplateDb, fullTableName));

			// ON-line. Index synchroniser
			var logger = new DummyUpgradeManager();
			var indexSynchroniser = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, logger);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewOnLineIndexes);

			AssertContainsExactElementsInAnyOrder("ON-line. Index synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);

			expected_main = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_Unique ON dbo.Match_UniqueIndex (Col_1, Col_2 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_UniqueWithFilter ON dbo.Match_UniqueIndex (Col_4, Col_5) INCLUDE (Col_2) WHERE ([Col_1]>(0));",

					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated1 ON dbo.Match_UniqueIndex (Col_1, Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated2 ON dbo.Match_UniqueIndex (Col_2 DESC, Col_1 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated4 ON dbo.Match_UniqueIndex (Col_4 DESC, Col_2, Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_5]=(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_4, Col_5) WHERE ([Col_1]>(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (_2_Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated9 ON dbo.Match_UniqueIndex (Col_1, Col_2) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("ON-line. Unique nonclustered index synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Column synchroniser
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);

			expected_main = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_Unique ON dbo.Match_UniqueIndex (Col_1, Col_2 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_UniqueWithFilter ON dbo.Match_UniqueIndex (Col_4, Col_5) INCLUDE (Col_2) WHERE ([Col_1]>(0));",

					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated1 ON dbo.Match_UniqueIndex (Col_1, Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated2 ON dbo.Match_UniqueIndex (Col_2 DESC, Col_1 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated4 ON dbo.Match_UniqueIndex (Col_4 DESC, Col_2, Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_5]=(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_4, Col_5) WHERE ([Col_1]>(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX _on_MUI_ToBeCreated9 ON dbo.Match_UniqueIndex (Col_1, Col_2) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Column synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Synchronising on-line indexes
			RunActionOnMockMainDb(indexSynchroniser.SynchroniseOnlineIndexes);

			expected_main = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_Unique ON dbo.Match_UniqueIndex (Col_1, Col_2 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_UniqueWithFilter ON dbo.Match_UniqueIndex (Col_4, Col_5) INCLUDE (Col_2) WHERE ([Col_1]>(0));",

					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated1 ON dbo.Match_UniqueIndex (Col_1, Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated2 ON dbo.Match_UniqueIndex (Col_2 DESC, Col_1 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated4 ON dbo.Match_UniqueIndex (Col_4 DESC, Col_2, Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_5]=(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_4, Col_5) WHERE ([Col_1]>(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated9 ON dbo.Match_UniqueIndex (Col_1, Col_2) WITH (DATA_COMPRESSION = ROW);",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Synchronising on-line indexes", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Index synchroniser
			RunActionOnMockMainDb(indexSynchroniser.DropOldIndexes);
			RunActionOnMockMainDb(indexSynchroniser.RecreateModifiedIndexes);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewIndexes);

			// rename variable
			var expected_main_after_offline_index_sync = new string[]
				{
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated1 ON dbo.Match_UniqueIndex (Col_1, Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated2 ON dbo.Match_UniqueIndex (Col_2 DESC, Col_1 DESC);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated4 ON dbo.Match_UniqueIndex (Col_4 DESC, Col_2, Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_5]=(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_4, Col_5) WHERE ([Col_1]>(0));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated9 ON dbo.Match_UniqueIndex (Col_1, Col_2) WITH (DATA_COMPRESSION = ROW);",

					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated1 ON dbo.Match_UniqueIndex (Col_5);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated2 ON dbo.Match_UniqueIndex (Col_5, Col_4);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_7);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated4 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_6);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_7]>(10));",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_3);",
					"CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_3]=(42));",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Index synchroniser", expected_main_after_offline_index_sync, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
		}

		public void TestCreateUniqueIndexesOnline_Clustered()
		{
			var fullTableName = "[dbo].[Match_UniqueClusteredIndex]";

			// Initial setup
			
			var expected_main = new string[]
				{
					"CREATE UNIQUE    CLUSTERED INDEX MUI_UniqueClustered ON dbo.Match_UniqueClusteredIndex (Col_1);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Main:", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			var expected_template = new string[]
				{
					"CREATE UNIQUE    CLUSTERED INDEX MUI_UniqueClustered ON dbo.Match_UniqueClusteredIndex (Col_1, Col_2);",
				};

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Template:", expected_template, IndexTestHelper.GetIndexDefinitions(TestConnection, mockTemplateDb, fullTableName));

			// ON-line. Index synchroniser
			var logger = new DummyUpgradeManager();
			var indexSynchroniser = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, logger);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewOnLineIndexes);
			AssertContainsExactElementsInAnyOrder("ON-line. Index synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);
			AssertContainsExactElementsInAnyOrder("ON-line. Unique nonclustered index synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Column synchroniser
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);
			AssertContainsExactElementsInAnyOrder("OFF-line. Column synchroniser", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Synchronising on-line indexes
			RunActionOnMockMainDb(indexSynchroniser.SynchroniseOnlineIndexes);
			AssertContainsExactElementsInAnyOrder("OFF-line. Synchronising on-line indexes", expected_main, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Index synchroniser
			RunActionOnMockMainDb(indexSynchroniser.DropOldIndexes);
			RunActionOnMockMainDb(indexSynchroniser.RecreateModifiedIndexes);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewIndexes);

			// rename variable
			var expected_main_after_offline_index_sync = new string[]
				{
					"CREATE UNIQUE    CLUSTERED INDEX MUI_UniqueClustered ON dbo.Match_UniqueClusteredIndex (Col_1, Col_2);",
				};

			AssertContainsExactElementsInAnyOrder("OFF-line. Index synchroniser", expected_main_after_offline_index_sync, IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
		}

		public void TestCreateUniqueIndexesOnline_ClusteredToNonclustered()
		{
			AssertCreateUniqueIndexesOnlineNotRunning(
				"[dbo].[Match_ClusteredToNonclusteredIndex]",
				new[]
				{
					"CREATE           CLUSTERED INDEX MainIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX UniqueIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1);",
				},
				new[]
				{
					"CREATE        NONCLUSTERED INDEX MainIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX NewUniqueIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1, Col_2);",
				}
			);
		}

		public void TestCreateUniqueIndexesOnline_NonclusteredToClustered()
		{
			AssertCreateUniqueIndexesOnlineNotRunning(
				"[dbo].[Match_NonclusteredToClusteredIndex]",
				new[]
				{
					"CREATE        NONCLUSTERED INDEX MainIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX UniqueIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1);",
				},
				new[]
				{
					"CREATE           CLUSTERED INDEX MainIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1);",
					"CREATE UNIQUE NONCLUSTERED INDEX NewUniqueIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1, Col_2);",
				}
			);
		}

		public void TestCreateUniqueIndexesOnline_Clustered_KeyChange()
		{
			AssertCreateUniqueIndexesOnlineNotRunning(
				"[dbo].[Match_Clustered_KeyChange]",
				new[]
				{
					"CREATE UNIQUE    CLUSTERED INDEX MainIndex ON dbo.Match_Clustered_KeyChange (Col_1);",
				},
				new[]
				{
					"CREATE           CLUSTERED INDEX MainIndex ON dbo.Match_Clustered_KeyChange (Col_2);",
					"CREATE UNIQUE NONCLUSTERED INDEX NewUniqueIndex ON dbo.Match_Clustered_KeyChange (Col_1, Col_2);",
				}
			);
		}

		void AssertCreateUniqueIndexesOnlineNotRunning(string fullTableName, string[] expected_main, string[] expected_template) {
			// Initial setup
			AssertContainsExactElementsInAnyOrder("PRECONDITION, Main:", expected_main,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			AssertContainsExactElementsInAnyOrder("PRECONDITION, Template:", expected_template,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockTemplateDb, fullTableName));

			// ON-line. Index synchroniser
			var logger = new DummyUpgradeManager();
			var indexSynchroniser = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, logger);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewOnLineIndexes);
			AssertContainsExactElementsInAnyOrder("ON-line. Index synchroniser", expected_main,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);
			AssertContainsExactElementsInAnyOrder("ON-line. Unique nonclustered index synchroniser", expected_main,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Column synchroniser
			var columnSynchroniser = new ColumnSynchroniser(TestConnection, logger, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(columnSynchroniser.DropAlterAndAddColumns);
			AssertContainsExactElementsInAnyOrder("OFF-line. Column synchroniser", expected_main,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Synchronising on-line indexes
			RunActionOnMockMainDb(indexSynchroniser.SynchroniseOnlineIndexes);
			AssertContainsExactElementsInAnyOrder("OFF-line. Synchronising on-line indexes", expected_main,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));

			// OFF-line. Index synchroniser
			RunActionOnMockMainDb(indexSynchroniser.DropOldIndexes);
			RunActionOnMockMainDb(indexSynchroniser.RecreateModifiedIndexes);
			RunActionOnMockMainDb(indexSynchroniser.CreateNewIndexes);

			AssertContainsExactElementsInAnyOrder("OFF-line. Index synchroniser", expected_template,
				IndexTestHelper.GetIndexDefinitions(TestConnection, mockMainDb, fullTableName));
		}

		sealed class IndexScriptRunnerForTests : IndexScriptRunner
		{
			readonly string indexName;
			readonly System.Data.Common.DbException exception;
			int throwCount;

			// IndexScriptRunner that has special behaviour for ExecuteNonQuery and ReportOnce methods:
			// first <throwCount> times when sql-to-be-executed string contains <indexName> it throws <exception>
			public IndexScriptRunnerForTests(DbConnection upgConnection, string dbBeingUpgraded, string templateDb,
				IUpgradeTaskWorkflowLogger taskLogger, string indexName, System.Data.Common.DbException exception, int throwCount)
				: base(upgConnection, dbBeingUpgraded, templateDb, taskLogger)
			{
				this.indexName = indexName;
				this.exception = exception;
				this.throwCount = throwCount;
			}

			protected override void ExecuteNonQuery(string sqlText)
			{
				if (throwCount > 0 && sqlText.Contains(indexName))
				{
					throwCount--;
					throw exception;
				}

				base.ExecuteNonQuery(sqlText);
			}
		}

		public void TestCreateUniqueIndexesOnline_ExceptionHandling()
		{
			var retryMessage = "Retrying to create index due to ";
			var skipMessage = "Skipped due to ";
			var indexToFail = "MUI_ToBeCreated1";

			// Scenario 1: error not related to index creation.
			{
				var logger = new DummyLogger();
				var syntaxError = SqlExceptionBuilder.CreateSqlException(102, "Incorrect syntax near '%.*ls'.");
				var indexSynchroniser = new IndexScriptRunnerForTests(TestConnection, mockMainDb, mockTemplateDb, logger,
					indexToFail, syntaxError, throwCount: 2);

				RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);

				var ind = logger.Logs.FindIndex(s => s.Contains(indexToFail));
				AssertEquals("Index is present in log",  true, ind >= 0);
				AssertEquals("Index should be skipped",  true, logger.Logs[ind + 1].Contains(skipMessage));
				AssertEquals("No Retry", false, logger.Logs.Exists(s => s.Contains(retryMessage)));
				AssertEquals("Exception should be reported", syntaxError.Message, ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}

			var indexCreationFailed = SqlExceptionBuilder.CreateSqlException(1505,
				"The CREATE UNIQUE INDEX statement terminated because a duplicate key was found");

			// Scenario 2: repetitive error related to index creation.
			{
				var logger = new DummyLogger();
				var indexSynchroniser = new IndexScriptRunnerForTests(TestConnection, mockMainDb, mockTemplateDb, logger,
					indexToFail, indexCreationFailed, throwCount: 2);

				RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);

				var ind = logger.Logs.FindIndex(s => s.Contains(indexToFail));
				AssertEquals("Index is present in log",  true, ind >= 0);
				AssertEquals("Retry", true, logger.Logs[ind + 1].Contains(retryMessage));
				AssertEquals("Index should be skipped",  true, logger.Logs[ind + 2].Contains(skipMessage));
				AssertEquals("No Exception reported",  null, ErrorReporter.LastExceptionReported);
			}

			// Scenario 3: index creation error that disappears on retry
			{
				var logger = new DummyLogger();
				var indexSynchroniser = new IndexScriptRunnerForTests(TestConnection, mockMainDb, mockTemplateDb, logger,
					indexToFail, indexCreationFailed, throwCount: 1);

				RunActionOnMockMainDb(indexSynchroniser.CreateUniqueIndexesOnline);

				var ind = logger.Logs.FindIndex(s => s.Contains(indexToFail));
				AssertEquals("Index is present in log",  true, ind >= 0);
				AssertEquals("Retry", true, logger.Logs[ind + 1].Contains(retryMessage));
				AssertEquals("Index should not be skipped",  false, logger.Logs.Exists(s => s.Contains(skipMessage)));
				AssertEquals("No Exception reported",  null, ErrorReporter.LastExceptionReported);
			}
		}

		#endregion // On-line indexes

		#region Alter Options

		public void TestAlterIndexOptions()
		{
			// Arrange
			AssertEquals("[PRE] Index 1 ALLOW_PAGE_LOCKS in Template DB", false, GetIndexOption(mockTemplateDb, "IX_Page_2_Option_1", "ALLOW_PAGE_LOCKS"));
			AssertEquals("[PRE] Index 1 ALLOW_PAGE_LOCKS in Main DB", true, GetIndexOption(mockMainDb, "IX_Page_2_Option_1", "ALLOW_PAGE_LOCKS"));
			AssertEquals("[PRE] Index 2 ALLOW_ROW_LOCKS in Template DB", false, GetIndexOption(mockTemplateDb, "IX_Page_2_Option_2", "ALLOW_ROW_LOCKS"));
			AssertEquals("[PRE] Index 2 ALLOW_ROW_LOCKS in Main DB", true, GetIndexOption(mockMainDb, "IX_Page_2_Option_2", "ALLOW_ROW_LOCKS"));

			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			var scriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, loggerMock.Object);

			// Act
			RunActionOnMockMainDb(scriptRunner.SynchroniseIndexOptions);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("Index 1 ALLOW_PAGE_LOCKS should be altered", false, GetIndexOption(mockMainDb, "IX_Page_2_Option_1", "ALLOW_PAGE_LOCKS"));
				AssertEquals("Index 2 ALLOW_ROW_LOCKS should be altered", false, GetIndexOption(mockMainDb, "IX_Page_2_Option_2", "ALLOW_ROW_LOCKS"));
				loggerMock.Verify(l => l.ShowInfoMessage("    (~) ALTER INDEX [IX_Page_2_Option_1] ON [dbo].[Match_IndexDiff] SET (ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = ON)"));
				loggerMock.Verify(l => l.ShowInfoMessage("    (~) ALTER INDEX [IX_Page_2_Option_2] ON [dbo].[Match_IndexDiff] SET (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = OFF)"));
			});

			return;

			bool GetIndexOption(string dbName, string indexName, string option)
			{
				return TestConnection.ExecuteScalar<bool>($"SELECT {option} FROM {dbName.QuoteName()}.sys.indexes WHERE name = N{indexName.QuoteName('\'')} and object_id = OBJECT_ID(N'{dbName}.dbo.Match_IndexDiff')");
			}
		}

		#endregion

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		void AssertIndexExistInTestMainDb(string tableName, string indexName, bool isUnique, bool isClustered, int fillFactor, string filter = null, string compression = null)
		{
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockMainDb, tableName, indexName, isUnique, isClustered, fillFactor, filter, compression);
		}

		void AssertIndexNotExistInTestMainDb(string tableName, string indexName)
		{
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, tableName, indexName);
		}

		void AssertIndexExistInTestTemplateDb(string tableName, string indexName, bool isUnique, bool isClustered, int fillFactor, string filter = null, string compression = null)
		{
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockTemplateDb, tableName, indexName, isUnique, isClustered, fillFactor, filter, compression);
		}

		void AssertIndexNotExistInTestTemplateDb(string tableName, string indexName)
		{
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, tableName, indexName);
		}

		#region Scripts

		#region dbBeingUpgraded

		const string createTestMainDbObjectsScript = @"
			--   IX_UniqNonclst_2_NonuniqClst - will change to NOT UNIQUE, CLUSTERED
			--   IX_ColListChangeOnly         - col3 added to the index
			--   IX_ToBeRemoved               - will be REMOVED
			--   IX_Clst_2_Nonclst            - will change to NONCLUSTERED
			--   IX_ToBeCreated               - will be CREATED
			--   IX_FillFactorChangeOnly      - will have its fillfactor changed
			--   IX_NoTemplateFillFactor      - will NOT be recreated (FILLFACTOR to remain as is, PAD_INDEX used to ensure no drop+recreate occurs)
			--   IX_FillFactorAndOtherChanges - will change, including FILLFACTOR
			--   IX_IncludeColListChanged     - include column list: Col4, Col2 => Col3, Col4, Col5 
			--   IX_ToBeCreatedWithInclude    - will be CREATED (with include columns)
			--   IX_FkRefToBeRemoved          - will be REMOVED
			--   IX_ToBeCompressed            - will change to add PAGE compression
			--   IX_Page_2_Row_Compression    - will change from PAGE compression to ROW compression
			--   IX_Page_2_None_Compression   - will change from PAGE compression to NONE compression
			CREATE TABLE Match_IndexDiff
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL,
				Col5 INT         NULL,
				Col6 INT         NULL,
				Col7 INT         NULL,
				Col8 INT         NULL,
			)
			;
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT PK_Match_IndexDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			CREATE UNIQUE NONCLUSTERED INDEX IX_UniqNonclst_2_NonuniqClst ON Match_IndexDiff (Col4)
			;
			CREATE NONCLUSTERED INDEX IX_ColListChangeOnly ON Match_IndexDiff (Col2) INCLUDE (Col4) WITH (FILLFACTOR = 30)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeRemoved ON Match_IndexDiff (Col3) WITH (FILLFACTOR = 40)
			;
			CREATE CLUSTERED INDEX IX_Clst_2_Nonclst ON Match_IndexDiff (Col5) WITH (FILLFACTOR = 50)
			;
			CREATE NONCLUSTERED INDEX IX_FillFactorChangeOnly ON Match_IndexDiff (Col3, Col4) WITH (FILLFACTOR = 60)
			;
			CREATE NONCLUSTERED INDEX IX_NoTemplateFillFactor ON Match_IndexDiff (Col3, Col4, Col5) WITH (FILLFACTOR = 70, PAD_INDEX = ON)
			;
			CREATE NONCLUSTERED INDEX IX_FillFactorAndOtherChanges ON Match_IndexDiff (Col3, Col5)
			;
			CREATE NONCLUSTERED INDEX IX_IncludeColListChanged ON Match_IndexDiff (Col6) INCLUDE (Col4, Col2)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter1 ON Match_IndexDiff (Col1, Col3) INCLUDE (Col2) WHERE ([Col4]<(0))
			;
			CREATE NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter2 ON Match_IndexDiff (Col1, Col5) INCLUDE (Col2)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeUpdatedWithoutFilter ON Match_IndexDiff (Col1, Col4) INCLUDE (Col6) WHERE ([Col5]=(0))
			;
			CREATE UNIQUE NONCLUSTERED INDEX IX_FkRefToBeRemoved ON Match_IndexDiff (Col7)
			;
			CREATE NONCLUSTERED INDEX _WTG_Ignored ON Match_IndexDiff (Col1) INCLUDE (Col2)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeCompressed ON Match_IndexDiff (Col6)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_Row_Compression ON Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = PAGE)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_None_Compression ON Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = PAGE)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_Option_1 ON Match_IndexDiff (Col6)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_Option_2 ON Match_IndexDiff (Col6)
			;

			-- Foreign Keys
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT FK_Match_IndexDiff_TO_Match_ColDiff_01 FOREIGN KEY (Col4)
						REFERENCES Match_IndexDiff (Col1)
			;
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT FK_Match_IndexDiff_Col6_TO_Col7 FOREIGN KEY (Col6)
						REFERENCES Match_IndexDiff (Col7)
			;
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT FK_Match_IndexDiff_Col8_TO_Col4 FOREIGN KEY (Col8)
						REFERENCES Match_IndexDiff (Col4)
			;

			-- Match_IndexDiff
			INSERT INTO Match_IndexDiff VALUES (1, 'A', 'Row One'                     , null, 1   , 11  , 11  , null)
			INSERT INTO Match_IndexDiff VALUES (2, 'B', 'Row Two'                     , 1   , 2   , null, 3   , null)
			INSERT INTO Match_IndexDiff VALUES (3, 'C', 'Row Three'                   , 2   , 3   , 3   , null, 1   )
			INSERT INTO Match_IndexDiff VALUES (4, 'D', 'DupCol6ToFixWrongUniqueIndex', 3   , null, null, 4   , 1   )
			;

			-- This table will have
			--   A number of secondary XML index changes
			CREATE TABLE XmlTypeIssues
			( 
				Col1 INT         NOT NULL,
				Col2 XML	     NULL,
				Col3 XML	     NULL,
				Col4 XML	     NULL,
				Col5 XML	     NULL,
				Col6 XML	     NULL,
				Col7 XML	     NULL,
			)
			;
			ALTER TABLE XmlTypeIssues
				ADD CONSTRAINT PK_XmlTypeIssues PRIMARY KEY CLUSTERED (Col1)
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col2 ON XmlTypeIssues (Col2) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_Old ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR PATH
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_Value2Property ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR VALUE
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col3_Old ON XmlTypeIssues (Col3) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col3_Old_Path ON XmlTypeIssues (Col3) 
				USING XML INDEX IX_XmlTypeIssues_Col3_Old FOR PATH
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Changing ON XmlTypeIssues (Col4) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Changing_Col ON XmlTypeIssues (Col4) 
				USING XML INDEX IX_XmlTypeIssues_Changing FOR PATH
			;
			CREATE SELECTIVE XML INDEX UX_XmlTypeIssues_Col2_Old ON XmlTypeIssues (Col2) 
			WITH XMLNAMESPACES
			(
				'http://www.cargowise.com/Schemas/Universal/2011/11' as u11,
				'http://www.cargowise.com/Schemas/Universal/2012/11' as u12
			)
			FOR
			(
				TransportMode11 = '/u11:UniversalShipment/u11:Shipment/u11:TransportMode/u11:Code' AS SQL VARCHAR(3) SINGLETON,
				OrganizationAddress12 = '/u12:UniversalShipment/u12:Shipment/u12:OrganizationAddressCollection/u12:OrganizationAddress' AS XQUERY 'node()',
				AddressType11 = '/u11:UniversalShipment/u11:Shipment/u11:OrganizationAddressCollection/u11:OrganizationAddress/u11:AddressType'
			);
			CREATE SELECTIVE XML INDEX UX_XmlTypeIssues_Col4_Namespaces ON XmlTypeIssues (Col4) 
			WITH XMLNAMESPACES
			(
				'http://www.cargowise.com/Schemas/Universal/2011/10' as u10,
				'http://www.cargowise.com/Schemas/Universal/2012/11' as u12
			)
			FOR
			(
				TransportMode10 = '/u10:UniversalShipment/u10:Shipment/u10:TransportMode/u10:Code' AS SQL VARCHAR(3) SINGLETON
			);
			CREATE SELECTIVE XML INDEX UX_XmlTypeIssues_Col5_Paths ON XmlTypeIssues (Col5) 
			WITH XMLNAMESPACES
			(
				'http://www.cargowise.com/Schemas/Universal/2011/11' as u11,
				'http://www.cargowise.com/Schemas/Universal/2012/11' as u12
			)
			FOR
			(
				TransportMode10 = '/u11:UniversalShipment/u11:Shipment/u11:TransportMode/u11:Code' AS SQL VARCHAR(3) SINGLETON,
				OrganizationAddress12 = '/u12:UniversalShipment/u12:Shipment/u12:OrganizationAddressCollection/u12:OrganizationAddress1' AS XQUERY 'node()'
			);
			-- Insert Data
			INSERT INTO XmlTypeIssues VALUES (1, 'SingleElement', '<tag>whatever</tag>', 'AnotherSingle', '<anothertag>blah blah</anothertag>', null, null)
			;

			if (OBJECT_ID(N'dbo.Match_OnlineIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_OnlineIndex;
			CREATE TABLE dbo.Match_OnlineIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
				Col_3 char(1) NOT NULL,
				Col_4 int     NOT NULL,
				Col_5 int     NOT NULL,

				_2_Col_3 bit  NOT NULL, -- converted to bit by ON-line transform
			);

			CREATE        NONCLUSTERED INDEX MOI_Unchanged         ON dbo.Match_OnlineIndex (Col_5);
			CREATE        NONCLUSTERED INDEX MOI_ToBeRemoved       ON dbo.Match_OnlineIndex (Col_3) INCLUDE (Col_2);
			CREATE UNIQUE NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);

			CREATE        NONCLUSTERED INDEX MOI_Recreated        ON dbo.Match_OnlineIndex (Col_3);

			CREATE        NONCLUSTERED INDEX MOI_Alter_All        ON dbo.Match_OnlineIndex (Col_5, Col_3);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Key        ON dbo.Match_OnlineIndex (Col_2) INCLUDE (Col_4) WITH (FILLFACTOR = 30);
			CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 60);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Include    ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3, Col_4);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1   ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2   ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2) WHERE ([Col_3]='N');

			CREATE        NONCLUSTERED INDEX MOI_Compressed       ON dbo.Match_OnlineIndex (Col_5);

			if (OBJECT_ID(N'dbo.Match_UniqueIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_UniqueIndex;
			CREATE TABLE dbo.Match_UniqueIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
				Col_3 char(1) NOT NULL,
				Col_4 int     NOT NULL,
				Col_5 int     NOT NULL,

				_2_Col_3 bit  NOT NULL, -- converted to bit by ON-line transform
			);

			CREATE UNIQUE NONCLUSTERED INDEX MUI_Unique ON dbo.Match_UniqueIndex (Col_1 ASC, Col_2 DESC);
			CREATE UNIQUE NONCLUSTERED INDEX MUI_UniqueWithFilter ON dbo.Match_UniqueIndex (Col_4, Col_5) INCLUDE (Col_2) WHERE ([Col_1] > 0);

			if (OBJECT_ID(N'dbo.Match_UniqueClusteredIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_UniqueClusteredIndex;
			CREATE TABLE dbo.Match_UniqueClusteredIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE UNIQUE CLUSTERED INDEX MUI_UniqueClustered ON dbo.Match_UniqueClusteredIndex (Col_1);

			if (OBJECT_ID(N'dbo.Match_ClusteredToNonclusteredIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_ClusteredToNonclusteredIndex;
			CREATE TABLE Match_ClusteredToNonclusteredIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE CLUSTERED INDEX MainIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1);
			CREATE UNIQUE NONCLUSTERED INDEX UniqueIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1);

			if (OBJECT_ID(N'dbo.Match_NonclusteredToClusteredIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_NonclusteredToClusteredIndex;
			CREATE TABLE Match_NonclusteredToClusteredIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE NONCLUSTERED INDEX MainIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1);
			CREATE UNIQUE NONCLUSTERED INDEX UniqueIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1);

			-- clustered index -> clustered index with different key
			if (OBJECT_ID(N'dbo.Match_Clustered_KeyChange', N'U') is NOT NULL) DROP TABLE dbo.Match_Clustered_KeyChange;
			CREATE TABLE Match_Clustered_KeyChange
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE UNIQUE CLUSTERED INDEX MainIndex ON dbo.Match_Clustered_KeyChange (Col_1);

			-- Spatial indexes
			CREATE TABLE ClientSpatial
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_ClientSpatial PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial ON ClientSpatial (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			CREATE TABLE Spatial_Index
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_Spatial_Index PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial_Old ON Spatial_Index (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_Modified ON Spatial_Index (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			";

		#endregion // dbBeingUpgraded

		#region templateDb

		const string createTestTemplateDbObjectsScript = @"
			--   IX_UniqNonclst_2_NonuniqClst - changed to NOT UNIQUE, CLUSTERED
			--   IX_ColListChangeOnly         - col3 added to the index
			--   IX_ToBeRemoved               - removed
			--   IX_Clst_2_Nonclst            - changed to NONCLUSTERED
			--   IX_ToBeCreated               - created
			--   IX_FillFactorChangeOnly      - FILLFACTOR changed
			--   IX_NoTemplateFillFactor      - FILLFACTOR remained as in the database being upgraded
			--   IX_FillFactorAndOtherChanges - column list and FILLFACTOR changed
			--   IX_IncludeColListChanged     - include column list: Col4, Col2 => Col3, Col4, Col5 
			--   IX_ToBeCreatedWithInclude    - created (with include columns)
			--   IX_FkRefToBeRemoved          - removed
			--   IX_ToBeCompressed            - changed to add PAGE compression
			--   IX_Page_2_Row_Compression    - changed from PAGE compression to ROW compression
			--   IX_Page_2_None_Compression   - changed from PAGE compression to NONE compression
			CREATE TABLE Match_IndexDiff
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL,
				Col5 INT         NULL,
				Col6 INT         NULL,
			)
			;
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT PK_Match_IndexDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			CREATE CLUSTERED INDEX IX_UniqNonclst_2_NonuniqClst ON Match_IndexDiff (Col4)
			;
			CREATE NONCLUSTERED INDEX IX_ColListChangeOnly ON Match_IndexDiff (Col3,Col2) INCLUDE (Col4)
			;
			CREATE NONCLUSTERED INDEX IX_Clst_2_Nonclst ON Match_IndexDiff (Col5) WITH (FILLFACTOR = 55)
			;
			CREATE UNIQUE NONCLUSTERED INDEX IX_ToBeCreated ON Match_IndexDiff (Col3) WITH (FILLFACTOR = 85, DATA_COMPRESSION = PAGE)
			;
			CREATE NONCLUSTERED INDEX IX_FillFactorChangeOnly ON Match_IndexDiff (Col3, Col4) WITH (FILLFACTOR = 65)
			;
			CREATE NONCLUSTERED INDEX IX_NoTemplateFillFactor ON Match_IndexDiff (Col3, Col4, Col5)
			;
			CREATE NONCLUSTERED INDEX IX_FillFactorAndOtherChanges ON Match_IndexDiff (Col3, Col6) WITH (FILLFACTOR = 35)
			;
			CREATE NONCLUSTERED INDEX IX_IncludeColListChanged ON Match_IndexDiff (Col6) INCLUDE (Col3, Col4, Col5)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeCreatedWithInclude ON Match_IndexDiff (Col5, Col6) INCLUDE (Col1)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeCreatedWithFilter ON Match_IndexDiff (Col1, Col2) INCLUDE (Col3) WHERE ([Col4]>(0))
			;
			CREATE NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter1 ON Match_IndexDiff (Col1, Col3) INCLUDE (Col2) WHERE ([Col4]>(0))
			;
			CREATE NONCLUSTERED INDEX IX_ToBeUpdatedWithFilter2 ON Match_IndexDiff (Col1, Col5) INCLUDE (Col2) WHERE ([Col4]>(0))
			;
			CREATE NONCLUSTERED INDEX IX_ToBeUpdatedWithoutFilter ON Match_IndexDiff (Col1, Col4) INCLUDE (Col6)
			;
			CREATE NONCLUSTERED INDEX IX_ToBeCompressed ON Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = PAGE)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_Row_Compression ON Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = ROW)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_None_Compression ON Match_IndexDiff (Col6) WITH (DATA_COMPRESSION = NONE)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_Option_1 ON Match_IndexDiff (Col6) WITH (ALLOW_PAGE_LOCKS = OFF)
			;
			CREATE NONCLUSTERED INDEX IX_Page_2_Option_2 ON Match_IndexDiff (Col6) WITH (ALLOW_ROW_LOCKS = OFF)
			;

			-- Foreign Keys
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT FK_Match_IndexDiff_TO_Match_ColDiff_01 FOREIGN KEY (Col4)
						REFERENCES Match_IndexDiff (Col1)
			;

			-- This table had
			--   A number of secondary XML index changes
			CREATE TABLE XmlTypeIssues
			( 
				Col1 INT         NOT NULL,
				Col2 XML	     NULL,
				Col3 XML	     NULL,
				Col4 XML	     NULL,
				Col5 XML	     NULL,
				Col6 XML	     NULL,
				Col7 XML	     NULL,
			)
			;
			ALTER TABLE XmlTypeIssues
				ADD CONSTRAINT PK_XmlTypeIssues PRIMARY KEY CLUSTERED (Col1)
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col2 ON XmlTypeIssues (Col2) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_Value2Property ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR PROPERTY
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_New ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR VALUE
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col6_New ON XmlTypeIssues (Col6) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col6_New_Value ON XmlTypeIssues (Col6) 
				USING XML INDEX IX_XmlTypeIssues_Col6_New FOR VALUE
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Changing ON XmlTypeIssues (Col5) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Changing_Col ON XmlTypeIssues (Col5) 
				USING XML INDEX IX_XmlTypeIssues_Changing FOR PATH
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_NewPrimary ON XmlTypeIssues (Col7) 
			;
			CREATE SELECTIVE XML INDEX UX_XmlTypeIssues_Col3_New ON XmlTypeIssues (Col3) 
			WITH XMLNAMESPACES
			(
			'http://www.cargowise.com/Schemas/Universal/2011/11' as u11,
			'http://www.cargowise.com/Schemas/Universal/2012/11' as u12
			)
			FOR
			(
			TransportMode11 = '/u11:UniversalShipment/u11:Shipment/u11:TransportMode/u11:Code' AS SQL VARCHAR(3) SINGLETON,
			OrganizationAddress12 = '/u12:UniversalShipment/u12:Shipment/u12:OrganizationAddressCollection/u12:OrganizationAddress' AS XQUERY 'node()',
			AddressType11 = '/u11:UniversalShipment/u11:Shipment/u11:OrganizationAddressCollection/u11:OrganizationAddress/u11:AddressType'
			)
			;
			CREATE SELECTIVE XML INDEX UX_XmlTypeIssues_Col4_Namespaces ON XmlTypeIssues (Col4) 
			WITH XMLNAMESPACES
			(
				'http://www.cargowise.com/Schemas/Universal/2011/11' as u11,
				'http://www.cargowise.com/Schemas/Universal/2012/11' as u12
			)
			FOR
			(
				TransportMode10 = '/u11:UniversalShipment/u11:Shipment/u11:TransportMode/u11:Code' AS SQL VARCHAR(3) SINGLETON
			)
			;
			CREATE SELECTIVE XML INDEX UX_XmlTypeIssues_Col5_Paths ON XmlTypeIssues (Col5) 
			WITH XMLNAMESPACES
			(
				'http://www.cargowise.com/Schemas/Universal/2011/11' as u11,
				'http://www.cargowise.com/Schemas/Universal/2012/11' as u12
			)
			FOR
			(
				TransportMode11 = '/u11:UniversalShipment/u11:Shipment/u11:TransportMode/u11:Code' AS SQL VARCHAR(3) SINGLETON,
				OrganizationAddress12 = '/u12:UniversalShipment/u12:Shipment/u12:OrganizationAddressCollection/u12:OrganizationAddress' AS XQUERY 'node()'
			)
			;

			if (OBJECT_ID(N'dbo.Match_OnlineIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_OnlineIndex
			CREATE TABLE dbo.Match_OnlineIndex
			(
				Col_1 int NOT NULL,
				Col_2 int NOT NULL,
				Col_3 bit NOT NULL, -- converted from char(1) to bit
				Col_4 int NOT NULL,
				Col_5 int NOT NULL,
				Col_6 int NOT NULL, -- New column, used in new indexes
				Col_7 int NOT NULL, -- New column, not used
			);

			ALTER TABLE dbo.Match_OnlineIndex ADD
				CONSTRAINT PK_Match_OnlineIndex PRIMARY KEY NONCLUSTERED (Col_1);

			CREATE        NONCLUSTERED INDEX MOI_Unchanged         ON dbo.Match_OnlineIndex (Col_5);
			CREATE        NONCLUSTERED INDEX MOI_UniqueToNonUnique ON dbo.Match_OnlineIndex (Col_2);

			CREATE UNIQUE NONCLUSTERED INDEX MOI_New_Unique         ON dbo.Match_OnlineIndex (Col_2);
			CREATE        NONCLUSTERED INDEX MOI_New                ON dbo.Match_OnlineIndex (Col_4);
			CREATE        NONCLUSTERED INDEX MOI_New_Key            ON dbo.Match_OnlineIndex (Col_4, Col_3);
			CREATE        NONCLUSTERED INDEX MOI_New_Include        ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_3);
			CREATE        NONCLUSTERED INDEX MOI_New_Filter         ON dbo.Match_OnlineIndex (Col_4) WHERE ([Col_3]=(0));
			CREATE        NONCLUSTERED INDEX MOI_New_Missed_Key     ON dbo.Match_OnlineIndex (Col_6);
			CREATE        NONCLUSTERED INDEX MOI_New_Missed_Include ON dbo.Match_OnlineIndex (Col_4) INCLUDE (Col_6);
			CREATE        NONCLUSTERED INDEX MOI_New_Missed_Filter  ON dbo.Match_OnlineIndex (Col_4) WHERE ([Col_5]=(0) AND [Col_6]=(0));

			CREATE        NONCLUSTERED INDEX MOI_Recreated        ON dbo.Match_OnlineIndex (Col_3);

			CREATE        NONCLUSTERED INDEX MOI_Alter_All        ON dbo.Match_OnlineIndex (Col_3, Col_5) INCLUDE (Col_4) WHERE ([Col_4]=(0)) WITH (FILLFACTOR = 30);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Key        ON dbo.Match_OnlineIndex (Col_2, Col_3) INCLUDE (Col_4);
			CREATE        NONCLUSTERED INDEX MOI_Alter_FillFactor ON dbo.Match_OnlineIndex (Col_3, Col_4) WITH (FILLFACTOR = 30);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Include    ON dbo.Match_OnlineIndex (Col_5) INCLUDE (Col_3);
			CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_1   ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_4) WHERE ([Col_3]=(0));
			CREATE        NONCLUSTERED INDEX MOI_Alter_Filter_2   ON dbo.Match_OnlineIndex (Col_1, Col_5) INCLUDE (Col_2);

			CREATE        NONCLUSTERED INDEX MOI_Compressed       ON dbo.Match_OnlineIndex (Col_5) WITH (DATA_COMPRESSION = ROW);

			if (OBJECT_ID(N'dbo.Match_UniqueIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_UniqueIndex
			CREATE TABLE dbo.Match_UniqueIndex
			(
				Col_1 int NOT NULL,
				Col_2 int NOT NULL,
				Col_3 bit NOT NULL, -- converted from char(1) to bit
				Col_4 int NOT NULL,
				Col_5 int NOT NULL,
				Col_6 int NOT NULL, -- New column, used in new indexes
				Col_7 int NOT NULL, -- New column, not used
			);

			-- Primary key constraint should not be created
			ALTER TABLE dbo.Match_UniqueIndex ADD
				CONSTRAINT PK_Match_UniqueIndex PRIMARY KEY NONCLUSTERED (Col_1);

			-- Same keys set that already exist
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated1 ON dbo.Match_UniqueIndex (Col_1, Col_2);
			-- Same keys set that already exist but with different order
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated2 ON dbo.Match_UniqueIndex (Col_2 DESC, Col_1 DESC);
			-- Existing unique set is subset of new unique set.
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_4);
			-- Existing unique set is subset of new unique set but with different order
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated4 ON dbo.Match_UniqueIndex (Col_4 DESC, Col_2 ASC, Col_1);
			-- More strict filter (no filter -> some filter)
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_5]=(0));
			-- Same filter and superset of existing unique index keys
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_4, Col_5) WHERE ([Col_1]>0);
			-- With include of non changed field
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_5);
			-- With include of populated field
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated8 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_3);
			-- With some options
			CREATE UNIQUE NONCLUSTERED INDEX MUI_ToBeCreated9 ON dbo.Match_UniqueIndex (Col_1, Col_2) WITH (DATA_COMPRESSION = ROW);

			-- Can't prove uniqueness
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated1 ON dbo.Match_UniqueIndex (Col_5);
			-- Same unique key set exist but with filter -> can't prove uniqueness
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated2 ON dbo.Match_UniqueIndex (Col_5, Col_4);
			-- Not existing field in key
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated3 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_7);
			-- Not existing field in include
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated4 ON dbo.Match_UniqueIndex (Col_1, Col_2) INCLUDE (Col_6);
			-- Not existing field in filter
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated5 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_7]>(10));
			-- Populated field in key
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated6 ON dbo.Match_UniqueIndex (Col_1, Col_2, Col_3);
			-- Populated field in filter
			CREATE UNIQUE NONCLUSTERED INDEX MUI_NotToBeCreated7 ON dbo.Match_UniqueIndex (Col_1, Col_2) WHERE ([Col_3]=(42));
			-- Unique constraint should not be created
			ALTER TABLE dbo.Match_UniqueIndex ADD CONSTRAINT MUI_NotToBeCreated8 UNIQUE (Col_1, Col_2);

			if (OBJECT_ID(N'dbo.Match_UniqueClusteredIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_UniqueClusteredIndex;
			CREATE TABLE dbo.Match_UniqueClusteredIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE UNIQUE CLUSTERED INDEX MUI_UniqueClustered ON dbo.Match_UniqueClusteredIndex (Col_1, Col_2);

			if (OBJECT_ID(N'dbo.Match_ClusteredToNonclusteredIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_ClusteredToNonclusteredIndex;
			CREATE TABLE Match_ClusteredToNonclusteredIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE NONCLUSTERED INDEX MainIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1);
			CREATE UNIQUE NONCLUSTERED INDEX NewUniqueIndex ON dbo.Match_ClusteredToNonclusteredIndex (Col_1, Col_2);

			if (OBJECT_ID(N'dbo.Match_NonclusteredToClusteredIndex', N'U') is NOT NULL) DROP TABLE dbo.Match_NonclusteredToClusteredIndex;
			CREATE TABLE Match_NonclusteredToClusteredIndex
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE CLUSTERED INDEX MainIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1);
			CREATE UNIQUE NONCLUSTERED INDEX NewUniqueIndex ON dbo.Match_NonclusteredToClusteredIndex (Col_1, Col_2);

			if (OBJECT_ID(N'dbo.Match_Clustered_KeyChange', N'U') is NOT NULL) DROP TABLE dbo.Match_Clustered_KeyChange;
			CREATE TABLE Match_Clustered_KeyChange
			(
				Col_1 int     NOT NULL,
				Col_2 int     NOT NULL,
			);

			CREATE CLUSTERED INDEX MainIndex ON dbo.Match_Clustered_KeyChange (Col_2);
			CREATE UNIQUE NONCLUSTERED INDEX NewUniqueIndex ON dbo.Match_Clustered_KeyChange (Col_1, Col_2);

			-- Spatial Indexes
			CREATE TABLE Spatial_Index
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_Spatial_Index PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial_Modified ON Spatial_Index (S_Geography) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 2)
			;
			CREATE SPATIAL INDEX IX_Spatial_New ON Spatial_Index (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			";

		#endregion // templateDb

		#endregion // Scripts

		#endregion // Implementation
	}
}
