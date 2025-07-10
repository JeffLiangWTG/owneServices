using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.BusinessObjectGenerator;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	sealed class SingleBizObjGeneratorTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTableAndIndexes()
		{
			SingleBizObjGenerator generator = new SingleBizObjGenerator(BaseSourcePath + @"\AutoTestBusinessObject.cs", outputDirectory);
			string[] indexes = generator.GetUniqueIndexesForTable_ForTest("StmEvent");
			AssertEquals(1, indexes.Length);
			AssertEquals("NR_UC__SE_Code", indexes[0]);

			indexes = generator.GetUniqueIndexesForTable_ForTest("RefUNLoco");
			AssertEquals(1, indexes.Length);
			AssertEquals("NR_UC__RL_Code", indexes[0]);

			Assert(SingleBizObjGenerator.tables.Count > 20);
		}

		#region Filtered index: IsLiteralOnly, IsNonEmpty

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilteredIndexColumns()
		{
			var tableName = "DummyDependentBizo";
			var generator = new SingleBizObjGenerator(BaseSourcePath + @"\AutoTestBusinessObject.cs", outputDirectory);

			SingleBizObjGenerator.tables = null;
			var expected_LiteralOnly = Array.Empty<string>();
			AssertContainsExactElementsInAnyOrder("PRECONDITION: LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));

			// Simple filters, exclude simple "NonEmpty" and "is [NOT] NULL"
			SingleBizObjGenerator.tables = null;
			var sql = @"-- LiteralOnly indexes
CREATE NONCLUSTERED INDEX _zd1_01 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Number = 1);
CREATE NONCLUSTERED INDEX _zd1_02 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Code <> '' AND ZD1_Number = 2);
CREATE NONCLUSTERED INDEX _zd1_03 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Z0 is NOT NULL);
CREATE NONCLUSTERED INDEX _zd1_04 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Z0 is NULL);
";
			TestConnection.ExecuteNonQuery(sql);
			expected_LiteralOnly = new string[] { "ZD1_Number", };
			AssertContainsExactElementsInAnyOrder("Simple filters: LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));

			// Complex filters, include "NonEmpty" and exclude "is [NOT] NULL"
			DropIndexes(tableName, "_zd1_01", "_zd1_02", "_zd1_03", "_zd1_04");
			SingleBizObjGenerator.tables = null;
			sql = @"CREATE NONCLUSTERED INDEX _zd1_00 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Number = 1 AND ZD1_Code <> '' AND ZD1_Z0 is NOT NULL);";
			TestConnection.ExecuteNonQuery(sql);
			expected_LiteralOnly = new string[] { "ZD1_Number" };
			AssertContainsExactElementsInAnyOrder("Exclude 'is NOT NULL': LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));

			SingleBizObjGenerator.tables = null;
			var expected_NonBlank = new string[] { "ZD1_Code", };
			AssertContainsExactElementsInAnyOrder("NonBlank Column", expected_NonBlank, generator.GetNonBlankFilteredIndexColumns_ForTest(tableName));

			// Complex filters, include "NonEmpty" and exclude "is [NOT] NULL"
			DropIndexes(tableName, "_zd1_00");
			SingleBizObjGenerator.tables = null;
			sql = @"CREATE NONCLUSTERED INDEX _zd1_00 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Number = 1 AND ZD1_Code <> '3' AND ZD1_Z0 is NULL);";
			TestConnection.ExecuteNonQuery(sql);
			expected_LiteralOnly = new string[] { "ZD1_Number", "ZD1_Code", };
			AssertContainsExactElementsInAnyOrder("Exclude 'is NULL': LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));

			// Complex filters, include "NonEmpty" and exclude "is [NOT] NULL"
			DropIndexes(tableName, "_zd1_00");
			SingleBizObjGenerator.tables = null;
			sql = @"CREATE NONCLUSTERED INDEX _zd1_00 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Number = 1 AND ZD1_Code <> '4' AND ZD1_Z0 <> '00000000-0000-0000-0000-000000000000');";
			TestConnection.ExecuteNonQuery(sql);
			expected_LiteralOnly = new string[] { "ZD1_Number", "ZD1_Code", "ZD1_Z0", };
			AssertContainsExactElementsInAnyOrder("All literals: LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));

			// Mix indexes, exclude simple "NonEmpty", include "NonEmpty" and exclude "is [NOT] NULL", fixed list of columns to literalize
			DropIndexes(tableName, "_zd1_00");
			SingleBizObjGenerator.tables = null;
			generator.FieldsToLiteralize_ForTest = new string[] { "ZD1_NumberUnitCode", };
			sql = @"-- LiteralOnly indexes
CREATE NONCLUSTERED INDEX _zd1_01 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Number = 1);
CREATE NONCLUSTERED INDEX _zd1_02 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Code <> '2');
CREATE NONCLUSTERED INDEX _zd1_03 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Z0 is NOT NULL);
CREATE NONCLUSTERED INDEX _zd1_04 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Z0 is NULL);
CREATE NONCLUSTERED INDEX _zd1_05 ON dbo.DummyDependentBizo (ZD1_PK) WHERE (ZD1_Number = 1 AND ZD1_Code <> '5' AND ZD1_Z0 is NOT NULL);
";
			TestConnection.ExecuteNonQuery(sql);
			expected_LiteralOnly = new string[] { "ZD1_Number", "ZD1_Code", "ZD1_NumberUnitCode", };
			AssertContainsExactElementsInAnyOrder("Mix indexes: LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilteredIndexColumns_IgnoreTempIndex()
		{
			var tableName = "DummyDependentBizo";
			var generator = new SingleBizObjGenerator(BaseSourcePath + @"\AutoTestBusinessObject.cs", outputDirectory);

			SingleBizObjGenerator.tables = null;
			AssertEquals("PRECONDITION: LiteralOnly columns", 0, generator.GetLiteralOnlyColumns_ForTest(tableName).Length);

			// Test temp index
			SingleBizObjGenerator.tables = null;
			var index = IndexInfo.Builder.New("dbo", "DummyDependentBizo", $"{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_zd1")
					   .Key("ZD1_Number")
					   .Where("ZD1_Number = 1")
					   .GetInfo();
			index.Create(TestConnection);
			var expected_LiteralOnly = Array.Empty<string>();
			AssertContainsExactElementsInAnyOrder("Simple filters: LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));

			index.Drop(TestConnection);
			// Test normal index
			SingleBizObjGenerator.tables = null;
			index = IndexInfo.Builder.New("dbo", "DummyDependentBizo", "_zd1_01")
					   .Key("ZD1_Number")
					   .Where("ZD1_Number = 1")
					   .GetInfo();
			index.Create(TestConnection);
			expected_LiteralOnly = new string[] { "ZD1_Number", };
			AssertContainsExactElementsInAnyOrder("Simple filters: LiteralOnly columns", expected_LiteralOnly, generator.GetLiteralOnlyColumns_ForTest(tableName));
		}

		void DropIndexes(string tableName, params string[] indexNames)
		{
			foreach (var indName in indexNames)
			{
				var sql = string.Format(@"DROP INDEX [{1}] ON dbo.[{0}];"
					, tableName
					, indName
					);

				using (var cmd = TestConnection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion // Filtered index: IsLiteralOnly, IsNonEmpty

		#region CanForceUpdateNaturalKeyCacheColumns

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCanForceUpdateNaturalKeyCache()
		{
			var table = "TestCanForceUpdateNaturalKeyCache";
			var createTestTableSql = $@"
				CREATE TABLE {table} (
					Bit1 BIT
					, String1 NVARCHAR(3)
					, String2 NVARCHAR(3)
					, String3 NVARCHAR(3)
					, String4 NVARCHAR(3)
					, String5 NVARCHAR(3)
					, String6 NVARCHAR(3)
					, String7 NVARCHAR(3)
					, String8 NVARCHAR(3)
				);
				CREATE UNIQUE NONCLUSTERED INDEX IndexBitWithComplexFilter                     ON {table} (Bit1)     WHERE Bit1 = 1;
				CREATE UNIQUE NONCLUSTERED INDEX IndexWithComplexFilter                        ON {table} (String1)  WHERE String1 <> 'foo';
				CREATE UNIQUE NONCLUSTERED INDEX IndexWithoutFilter                            ON {table} (String2);
				CREATE UNIQUE NONCLUSTERED INDEX IndexWithInclude                              ON {table} (String3)  INCLUDE (String4) WHERE String3 <> 'foo';
				CREATE UNIQUE NONCLUSTERED INDEX IndexComposite                                ON {table} (String4, String5) WHERE String4 <> 'foo';
				CREATE UNIQUE NONCLUSTERED INDEX IndexFilteringBlanksOnly                      ON {table} (String5)  WHERE String5 <> '';
				CREATE UNIQUE NONCLUSTERED INDEX IndexFilterMultiClause                        ON {table} (String6)  WHERE String6 <> 'foo' AND String6 <> '';
				CREATE UNIQUE NONCLUSTERED INDEX IndexFilteringBlanksOnlyWithAlternateNotation ON {table} (String7)  WHERE ((String7 != ''));
				CREATE        NONCLUSTERED INDEX IndexNonUnique                                ON {table} (String8)  WHERE String8 <> 'foo';
				";

			TestConnection.ExecuteNonQuery(createTestTableSql);

			var generator = new SingleBizObjGenerator(BaseSourcePath + @"\AutoTestBusinessObject.cs", outputDirectory);
			generator.ClearCanForceUpdateNaturalKeyCacheColumns_ForTest();
			var columns = generator.CanForceUpdateNaturalKeyCacheColumns_ForTest;

			CombineAssertions(() =>
			{
				AssertCollectionContains("[Bit1] Bit index with complex filter IS in this set (non-strings may be ignored later)",
					"TESTCANFORCEUPDATENATURALKEYCACHE.BIT1", columns);
				AssertCollectionContains("[String1] Index with complex filter IS in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING1", columns);
				AssertCollectionNotContains("[String2] Index without filter is NOT in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING2", columns);
				AssertCollectionContains("[String3] Index with complex filter and INCLUDE IS in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING3", columns);
				AssertCollectionNotContains("[String4] Composite index is NOT in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING4", columns);
				AssertCollectionNotContains("[String5] Index filtering out blanks only is NOT in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING5", columns);
				AssertCollectionContains("[String6] Index with multiple clauses one of which is filtering out blanks only IS in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING6", columns);
				AssertCollectionNotContains("[String7] Index filtering out blanks only with alternate notation is NOT in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING7", columns);
				AssertCollectionNotContains("[String8] Non-unique index is NOT in this set",
					"TESTCANFORCEUPDATENATURALKEYCACHE.STRING8", columns);
			});
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileNameOfBusinessObjectSchema_Standard()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\Product\Operations\MasterFilters\Business\AutoOrgHeader.cs", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\OrgHeaderSchema.cs"), generator.FileNameOfBusinessObjectSchema);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileNameOfBusinessObjectSchema_DifferentDatabase()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\CMR\Business\AutoCMRAHECCCode.cs", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\RefDb_AU_Customs\CMRAHECCCodeSchema.cs"), generator.FileNameOfBusinessObjectSchema);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileNameOfBusinessObjectSchema_ClientSpecific()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\AutoClientTestBusinessObject.cs", outputDirectory);
			AssertEquals(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\ClientTestBusinessObjectSchema.cs", generator.FileNameOfBusinessObjectSchema);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileNameOfBusinessObjectSchema_Common()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\Architecture\Business\ZArchitecture.Business.Test\Business\Internal\Testing\AutoGeneratedDummy\AutoDummyBizo.cs", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\DummySchema\DummyBizoSchema.cs"), generator.FileNameOfBusinessObjectSchema);

			generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Common\Architecture\EntityFramework\CargoWise.EntityFramework\Business\Testing\AutoGeneratedDummy\AutoDummyPivot.cs", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\DummySchema\DummyPivotSchema.cs"), generator.FileNameOfBusinessObjectSchema);

			generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\Architecture\Business\ZArchitecture.Business.Test\Business\Internal\Testing\AutoGeneratedDummy\AutoDummyLogged.cs", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\DummySchema\DummyLoggedSchema.cs"), generator.FileNameOfBusinessObjectSchema);

			generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Common\Architecture\EntityFramework\CargoWise.EntityFramework\Business\Testing\AutoGeneratedDummy\AutoDummyDependentBizo.cs", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\DummySchema\DummyDependentBizoSchema.cs"), generator.FileNameOfBusinessObjectSchema);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClientTableIndexesIncludedWhenMixedWithNonClientSpecificTables()
		{
			try
			{
				SingleBizObjGenerator.tables = null;
				SingleBizObjGenerator.clientAssemblyNameUsedWhileRetrievingIndexes = "";

				TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"\AutoTestBusinessObject.cs", outputDirectory);
				generator.Generate();
				object loadStaticHastable = generator.GetUniqueIndexesForTable_ForTest("StmALog");

				TestSingleBizObjGenerator clientSpecificGenerator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\AutoClientTestBusinessObject.cs", outputDirectory);
				clientSpecificGenerator.tableNameToGetIndexesFor = "ClientBISIShipmentHeader";
				clientSpecificGenerator.Generate();
				AssertEquals(1, clientSpecificGenerator.IndexesInDbDuringGeneration.Count);
				AssertEquals("NR_UX__T8_CS", clientSpecificGenerator.IndexesInDbDuringGeneration[0]);
			}
			finally
			{
				SingleBizObjGenerator.tables = null;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClientTableIndexes_MultipleClients()
		{
			try
			{
				SingleBizObjGenerator.tables = null;
				SingleBizObjGenerator.clientAssemblyNameUsedWhileRetrievingIndexes = "";

				TestSingleBizObjGenerator client1Generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\AutoClientTestBusinessObject.cs", outputDirectory);
				client1Generator.tableNameToGetIndexesFor = "ClientBISIShipmentHeader";
				client1Generator.Generate();
				AssertEquals(1, client1Generator.IndexesInDbDuringGeneration.Count);
				AssertEquals("NR_UX__T8_CS", client1Generator.IndexesInDbDuringGeneration[0]);

				TestSingleBizObjGenerator client2Generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\AUS\ZClientAUS\AutoClientTestBusinessObject.cs", outputDirectory);
				client2Generator.tableNameToGetIndexesFor = "ClientAUSOriginPreferenceMapping";
				client2Generator.Generate();
				AssertEquals(1, client2Generator.IndexesInDbDuringGeneration.Count);
				AssertEquals("NR_UX__T7_OH_Importer__T7_OH_Supplier__T7_RN_NKOrigin", client2Generator.IndexesInDbDuringGeneration[0]);
			}
			finally
			{
				SingleBizObjGenerator.tables = null;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnIsNullable()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\Architecture\Business\Public\StmEvent\AutoStmEvent.cs", outputDirectory);
			DataTable testTable = generator.TableFromDatabaseSchema_Exposed();

			AssertEquals("SE_OH nullable?", true, testTable.Columns["SE_OH"].AllowDBNull);
			AssertEquals("SE_Code nullable?", false, testTable.Columns["SE_Code"].AllowDBNull);
			AssertEquals("SE_DisplayOrder nullable?", false, testTable.Columns["SE_DisplayOrder"].AllowDBNull);
			AssertEquals("SE_IsAirMilestone nullable?", false, testTable.Columns["SE_IsAirMilestone"].AllowDBNull);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestComputedColumnsAreIgnored()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"C:\Dev\Enterprise\Product\Operations\eManifest\Business\SupplierBookingLine\Autos\AutoSupplierBookingLine.cs", outputDirectory);
			DataTable testTable = generator.TableFromDatabaseSchema_Exposed();

			AssertEquals(testTable.Columns.Contains("DL_GrossWeight"), true);
			AssertEquals(testTable.Columns.Contains("DL_GrossWeightInKg"), false);
			AssertEquals(testTable.Columns.Contains("DL_Cubic"), true);
			AssertEquals(testTable.Columns.Contains("DL_CubicInM3"), false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAll()
		{
			SingleBizObjGenerator generator = new SingleBizObjGenerator(@"AutoOrgHeader.cs", outputDirectory, false);
			generator.GenerateAll(outputDirectory);
			AssertEquals(true, File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\OrgHeaderSchema.cs")));
			AssertEquals(false, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\MasterFilters\Business\AutoOrgHeader.cs")));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreventDelete()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(@"AutoOrgHeader.cs", outputDirectory, false);
			Assert(!generator.Info_Exposed.PreventDelete);

			generator = new TestSingleBizObjGenerator(@"AutoJobShipment.cs", outputDirectory, false);
			Assert(generator.Info_Exposed.PreventDelete);
		}

		#region TestGenerateFilesForSolutionWithExistingConcreteClasses

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateFilesForSolutionWithExistingConcreteClasses()
		{
			var testInstance = new BuildXml(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\Builder\Generator\Testing\TestBuild.xml"));
			BuildXml.SetInstanceForTesting(testInstance);

			var businessClassFile = "ZArchitecture\\StmLog\\StmALog.cs";
			var validationFile = "ZArchitecture\\Validation\\StmALogValidation.cs";
			var lookupsFile = "ZArchitecture\\Lookups\\StmALogLookups.cs";

			SetupFilesInProjectFileAndSourceControl(businessClassFile, validationFile, lookupsFile);

			string[] args = { Db.ServerName, Db.DatabaseName, CommandLineOptions.GenerateBizObjectsForSolution, "ZArchitecture" };
			var entryPoint = new GeneratorEntryPoint(outputDirectory);
			int result = entryPoint.Execute(GeneratorArguments.Parse(args));
			AssertEquals("No error.", 0, result);

			Assert(File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\AutoStmALog.cs")));
			Assert(File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\AutoStmALogValidation.cs")));
			Assert(File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\AutoStmALogLookups.cs")));

			AssertSkipExistingConcreteFiles(businessClassFile, "ZArchitecture\\StmALog.cs");
			AssertSkipExistingConcreteFiles(validationFile, "ZArchitecture\\StmALogValidation.cs");
			AssertSkipExistingConcreteFiles(lookupsFile, "ZArchitecture\\StmALogLookups.cs");
		}

		void AssertSkipExistingConcreteFiles(string businessClassFile, string defaultLocation)
		{
			AssertEquals("File should not be generated in default location.", false, File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, defaultLocation)));
			AssertEquals("File should be in existing location", true, File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, businessClassFile)));
		}

		void SetupFilesInProjectFileAndSourceControl(string businessClassFile, string validationFile, string lookupsFile)
		{
			AddExistingFilesInProjectFile(businessClassFile);
			AddExistingFilesInProjectFile(validationFile);
			AddExistingFilesInProjectFile(lookupsFile);

			AddFilesToTempLocationAndMockSourceControl(businessClassFile);
			AddFilesToTempLocationAndMockSourceControl(validationFile);
			AddFilesToTempLocationAndMockSourceControl(lookupsFile);
		}

		void AddFilesToTempLocationAndMockSourceControl(string fileName)
		{
			const string testContents = "BLAH";

			string pathSourceControl = Path.Combine(MockSourceControl.MockSourceControlPath, fileName);
			var sourceControlFile = new FileInfo(pathSourceControl);
			sourceControlFile.Directory.Create();

			string pathWorkSpace = Path.Combine(MockSourceControl.MockWorkspacePath, fileName);
			var newFile = new FileInfo(pathWorkSpace);
			newFile.Directory.Create();

			File.WriteAllText(pathWorkSpace, testContents);
			File.SetAttributes(pathWorkSpace, FileAttributes.ReadOnly);
			SourceControl.EnterpriseDatabase.AddFile(pathWorkSpace);
		}

		void AddExistingFilesInProjectFile(string fileName)
		{
			var doc = new XmlDocument();
			var docLocation = Path.Combine(MockSourceControl.MockWorkspacePath, @"ZArchitecture\ZArchitecture.csproj");
			doc.Load(docLocation);

			var buildNamespaceManager = new XmlNamespaceManager(doc.NameTable);
			buildNamespaceManager.AddNamespace("csproj", "http://schemas.microsoft.com/developer/msbuild/2003");
			var itemGroup = doc.SelectNodes("//csproj:ItemGroup", buildNamespaceManager);

			foreach (XmlNode node in itemGroup)
			{
				if (node.FirstChild.Name == "Compile")
				{
					var newCompileElement = doc.CreateNode(XmlNodeType.Element, "Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
					var attr = doc.CreateAttribute("Include");
					attr.Value = fileName.Remove(fileName.IndexOf("ZArchitecture\\"), "ZArchitecture\\".Length);
					newCompileElement.Attributes.SetNamedItem(attr);

					node.AppendChild(newCompileElement);
				}
			}

			File.SetAttributes(docLocation, FileAttributes.Normal);
			doc.Save(docLocation);
			File.SetAttributes(docLocation, FileAttributes.ReadOnly);
		}

		#endregion

		#region TestFileIsNew

		public void TestFileIsNew()
		{
			SingleBizObjGenerator generator = new SingleBizObjGenerator(@"AutoOrgHeader.cs", outputDirectory, false);

			string filePath = Path.Combine(MockSourceControl.MockWorkspacePath, Guid.NewGuid().ToString() + ".cs");
			try
			{
				Assert(!File.Exists(filePath));
				AssertEquals("New file", true, generator.FileIsNew(filePath));

				File.WriteAllText(filePath, "Abracadabra" + System.Environment.NewLine);
				SourceControl.EnterpriseDatabase.AddFile(filePath);
				Assert(File.Exists(filePath));
				AssertEquals("Same and in source control", false, generator.FileIsNew(filePath, new TestSourceFile { _Body = "Abracadabra" }));

				SourceControl.EnterpriseDatabase.DeleteFile(filePath);
				AssertEquals("Not in source control", true, generator.FileIsNew(filePath));

				File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
				AssertEquals("Same and readonly", false, generator.FileIsNew(filePath, new TestSourceFile { _Body = "Abracadabra" }));
				AssertEquals("Different contents", true, generator.FileIsNew(filePath, new TestSourceFile { _Body = "Qwertyuiop" }));
			}
			finally
			{
				TempFile.Delete(filePath, false);
			}
		}

		class TestSourceFile : AutoSourceFile
		{
			protected override string Body
			{
				get { return _Body; }
			}

			public string _Body { get; set; }
		}

		#endregion

		public void TestActualDatabaseNameDuringRegen_NoBrackets()
		{
			var generator = new TestSingleBizObjGenerator(@"AutoOrgHeader.cs", outputDirectory, false);
			AssertEquals(false, generator.ActualDatabaseNameDuringRegen_Exposed.StartsWith("["));
			AssertEquals(Db.SqlDbOwnerSchema, generator.SqlSchemaName);
		}

		public void TestActualDatabaseNameDuringRegen_WithBrackets()
		{
			var generator = new TestSingleBizObjGenerator(@"AutoOrgHeader.cs", outputDirectory, false);
			AssertEquals(false, generator.ActualDatabaseNameDuringRegen_Exposed.StartsWith("["));

			generator.ActualDatabaseNameDuringRegen_ForTest = "[" + generator.ActualDatabaseNameDuringRegen_Exposed + "]";
			AssertEquals(true, generator.ActualDatabaseNameDuringRegen_Exposed.StartsWith("["));
			AssertEquals(Db.SqlDbOwnerSchema, generator.SqlSchemaName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestComputedAndSparseColumns()
		{
			var generator = new SingleBizObjGenerator(@"AutoJobDeclaration.cs", outputDirectory, false);
			generator.GenerateAll(outputDirectory);

			var outputContent = File.ReadAllText(Path.Combine(outputDirectory.CWSharedOutputDirectory,
				@"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\JobDeclarationSchema.cs"));

			AssertContains("JE_RV_NKVessel = new SchemaStringColumn(Instance, Constants.JE_RV_NKVessel, column++, SqlDbType.VarChar, DBNull.Value, IsNullable, 35, false, false, TVPHelper.TVP_varchar, isComputed: true);", outputContent);

			AssertContains("JE_AircraftRegistration = new SchemaStringColumn(Instance, Constants.JE_AircraftRegistration, column++, SqlDbType.VarChar, DBNull.Value, IsNullable, 8, false, false, TVPHelper.TVP_varchar, isSparse: true);", outputContent);
		}

		#region Implementation

		internal static IList GetTableNamesFromDb()
		{
			ArrayList tableNames = new ArrayList();
			DbCommand command = Db.Connection.Command("sp_tables");
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					tableNames.Add(reader["TABLE_NAME"]);
				}
			}
			return tableNames;
		}

		protected override void SetUp()
		{
			base.SetUp();
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut);
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			outputDirectory.Dispose();
			MockSourceControl.TearDown();
		}

		GeneratorOutputDirectory outputDirectory;

		#endregion
	}
}
