using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserComputedColumnsTest : TestCaseWithMockMainDbAndTemplateDb
	{
		public void TestDropAlterAndAddColumns()
		{
			// Arrange

			// Verify that changing a calculated column in combination with changing its underlying column is correct and effective
			// Consider the following scenarios:
			// 1. creating computed only
			// 2. creating both, base and computed
			// 3. altering computed only
			// 4. altering base only
			// 5. altering both
			// 6. dropping computed only
			// 7. dropping base only
			// 8. dropping both
			// 9. Case diff - base
			// 10. Case diff - computed
			// 11. Case diff - both

			// Act
			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			// Assert
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				var fullTableName = "dbo.ComputedColumnASC";
				AssertColumnExists(true, fullTableName, "C_01_Other");
				// 1. creating computed only
				AssertColumnExists(true, fullTableName, "C_02_Base1");
				AssertColumnExists(true, fullTableName, "C_03_Comp1");
				// 2. creating both, base and computed
				AssertColumnExists(true, fullTableName, "C_04_Base2");
				AssertColumnExists(true, fullTableName, "C_05_Comp2");
				// 3. altering computed only
				AssertColumnExists(true, fullTableName, "C_06_Base3");
				AssertColumnExists(true, fullTableName, "C_07_Comp3");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_07_Comp3",
					"C_07_Comp3 int NULL NOT PERSISTED AS ([C_06_Base3]+(100))");
				// 4. altering base only
				AssertColumnExists(true, fullTableName, "C_08_Base4");
				AssertColumnExists(true, fullTableName, "C_09_Comp4");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_09_Comp4",
					"C_09_Comp4 bigint NULL NOT PERSISTED AS ([C_08_Base4]+(1))");
				// 5. altering both
				AssertColumnExists(true, fullTableName, "C_10_Base5");
				AssertColumnExists(true, fullTableName, "C_11_Comp5");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_11_Comp5",
					"C_11_Comp5 bigint NULL NOT PERSISTED AS ([C_10_Base5]+(100))");
				// 6. dropping computed only
				AssertColumnExists(true, fullTableName, "C_12_Base6");
				AssertColumnExists(false, fullTableName, "C_13_Comp6");
				// 7. dropping base only
				AssertColumnExists(false, fullTableName, "C_14_Base7");
				AssertColumnExists(true, fullTableName, "C_15_Comp7");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_15_Comp7",
					"C_15_Comp7 int NULL NOT PERSISTED AS ([C_01_Other]+(1))");
				// 8. dropping both
				AssertColumnExists(false, fullTableName, "C_16_Comp8");
				AssertColumnExists(false, fullTableName, "C_17_Comp8");
				// 9. Case diff - base
				AssertColumnExists(true, fullTableName, "C_20_CaseDiff_Base9");
				AssertColumnExists(true, fullTableName, "C_21_CaseDiff_Comp9");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_21_CaseDiff_Comp9",
					"C_21_CaseDiff_Comp9 int NULL NOT PERSISTED AS ([C_20_CaseDiff_Base9]+(1))");
				// 10. Case diff - computed
				AssertColumnExists(true, fullTableName, "C_22_CaseDiff_Base10");
				AssertColumnExists(true, fullTableName, "C_23_CaseDiff_Comp10");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_23_CaseDiff_Comp10",
					"C_23_CaseDiff_Comp10 int NULL NOT PERSISTED AS ([C_22_CaseDiff_Base10]+(1))");
				// 11. Case diff - both
				AssertColumnExists(true, fullTableName, "C_24_CaseDiff_Base11");
				AssertColumnExists(true, fullTableName, "C_25_CaseDiff_Comp11");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_25_CaseDiff_Comp11",
					"C_25_CaseDiff_Comp11 int NULL NOT PERSISTED AS ([C_24_CaseDiff_Base11]+(1))");

				fullTableName = "dbo.ComputedColumnDESC";
				AssertColumnExists(true, fullTableName, "C_99_Other");
				// 1. creating computed only
				AssertColumnExists(true, fullTableName, "C_98_Base1");
				AssertColumnExists(true, fullTableName, "C_97_Comp1");
				// 2. creating both, base and computed
				AssertColumnExists(true, fullTableName, "C_96_Base2");
				AssertColumnExists(true, fullTableName, "C_95_Comp2");
				// 3. altering computed only
				AssertColumnExists(true, fullTableName, "C_94_Base3");
				AssertColumnExists(true, fullTableName, "C_93_Comp3");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_93_Comp3",
					"C_93_Comp3 int NULL NOT PERSISTED AS ([C_94_Base3]+(100))");
				// 4. altering base only
				AssertColumnExists(true, fullTableName, "C_92_Base4");
				AssertColumnExists(true, fullTableName, "C_91_Comp4");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_91_Comp4",
					"C_91_Comp4 bigint NULL NOT PERSISTED AS ([C_92_Base4]+(1))");
				// 5. altering both
				AssertColumnExists(true, fullTableName, "C_90_Base5");
				AssertColumnExists(true, fullTableName, "C_89_Comp5");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_89_Comp5",
					"C_89_Comp5 bigint NULL NOT PERSISTED AS ([C_90_Base5]+(100))");
				// 6. dropping computed only
				AssertColumnExists(true, fullTableName, "C_88_Base6");
				AssertColumnExists(false, fullTableName, "C_87_Comp6");
				// 7. dropping base only
				AssertColumnExists(false, fullTableName, "C_86_Base7");
				AssertColumnExists(true, fullTableName, "C_85_Comp7");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_85_Comp7",
					"C_85_Comp7 int NULL NOT PERSISTED AS ([C_99_Other]+(1))");
				// 8. dropping both
				AssertColumnExists(false, fullTableName, "C_84_Comp8");
				AssertColumnExists(false, fullTableName, "C_83_Comp8");
				// 9. Case diff - base
				AssertColumnExists(true, fullTableName, "C_70_CaseDiff_Base9");
				AssertColumnExists(true, fullTableName, "C_69_CaseDiff_Comp9");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_69_CaseDiff_Comp9",
					"C_69_CaseDiff_Comp9 int NULL NOT PERSISTED AS ([C_70_CaseDiff_Base9]+(1))");
				// 10. Case diff - computed
				AssertColumnExists(true, fullTableName, "C_68_CaseDiff_Base10");
				AssertColumnExists(true, fullTableName, "C_67_CaseDiff_Comp10");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_67_CaseDiff_Comp10",
					"C_67_CaseDiff_Comp10 int NULL NOT PERSISTED AS ([C_68_CaseDiff_Base10]+(1))");
				// 11. Case diff - both
				AssertColumnExists(true, fullTableName, "C_66_CaseDiff_Base11");
				AssertColumnExists(true, fullTableName, "C_65_CaseDiff_Comp11");
				AssertComputedColumnDefinition(mockMainDb, fullTableName, "C_65_CaseDiff_Comp11",
					"C_65_CaseDiff_Comp11 int NULL NOT PERSISTED AS ([C_66_CaseDiff_Base11]+(1))");
			}
		}

		#region Implementation

		void AssertColumnExists(bool expected, string fullTableName, string columnName)
		{
			var actual = TestConnection.Exists(@$"
FROM
	sys.columns AS col
WHERE 1=1
	AND col.object_id = OBJECT_ID(@fullTableName, 'U')
	AND col.name = @columnName

"
			, cmd =>
			{
				cmd.AddParameter("@fullTableName", SqlDbType.NVarChar, 257, fullTableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
			});

			AssertEquals($"{fullTableName}.[{columnName}] column exists?", expected, actual);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[]
			{
				@" -- TemplateDB for test
CREATE TABLE dbo.ComputedColumnASC -- column names in ASC order - TemplateDB
(
	C_01_Other int,

	-- 1. creating computed
	C_02_Base1 int,
	C_03_Comp1 AS C_02_Base1 + 1,
	-- 2. creating both, base and computed
	C_04_Base2 int,
	C_05_Comp2 AS C_04_Base2 + 1,
	-- 3. altering computed
	C_06_Base3 int,
	C_07_Comp3 AS C_06_Base3 + 100,
	-- 4. altering base
	C_08_Base4 bigint,
	C_09_Comp4 AS C_08_Base4 + 1,
	-- 5. altering both
	C_10_Base5 bigint,
	C_11_Comp5 AS C_10_Base5 + 100,
	-- 6. dropping computed
	C_12_Base6 int,
	-- 7. dropping base
	C_15_Comp7 AS C_01_Other + 1,
	-- 8. dropping both

	-- 9. Case diff - base
	C_20_CaseDiff_Base9 int,
	C_21_CaseDiff_Comp9 AS C_20_CaseDiff_Base9 + 1,
	-- 10. Case diff - computed
	C_22_CaseDiff_Base10 int,
	C_23_CaseDiff_Comp10 AS C_22_CaseDiff_Base10 + 1,
	-- 11. Case diff - both
	C_24_CaseDiff_Base11 int,
	C_25_CaseDiff_Comp11 AS C_24_CaseDiff_Base11 + 1,
);

CREATE TABLE dbo.ComputedColumnDESC -- column names in DESC order - TemplateDB
(
	C_99_Other int,

	-- 1. creating computed
	C_98_Base1 int,
	C_97_Comp1 AS C_98_Base1 + 1,
	-- 2. creating both, base and computed
	C_96_Base2 int,
	C_95_Comp2 AS C_96_Base2 + 1,
	-- 3. altering computed
	C_94_Base3 int,
	C_93_Comp3 AS C_94_Base3 + 100,
	-- 4. altering base
	C_92_Base4 bigint,
	C_91_Comp4 AS C_92_Base4 + 1,
	-- 5. altering both
	C_90_Base5 bigint,
	C_89_Comp5 AS C_90_Base5 + 100,
	-- 6. dropping computed
	C_88_Base6 int,
	-- 7. dropping base
	C_85_Comp7 AS C_99_Other + 1,
	-- 8. dropping both

	-- 9. Case diff - base
	C_70_CaseDiff_Base9 int,
	C_69_CaseDiff_Comp9 AS C_70_CaseDiff_Base9 + 1,
	-- 10. Case diff - computed
	C_68_CaseDiff_Base10 int,
	C_67_CaseDiff_Comp10 AS C_68_CaseDiff_Base10 + 1,
	-- 11. Case diff - both
	C_66_CaseDiff_Base11 int,
	C_65_CaseDiff_Comp11 AS C_66_CaseDiff_Base11 + 1,
);

"
			});
		}

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[]
			{
				@"-- MainDB for test
CREATE TABLE dbo.ComputedColumnASC -- column names in ASC order - MainDB
(
	C_01_Other int,

	-- 1. creating computed
	C_02_Base1 int,
	-- 2. creating both, base and computed
	-- 3. altering computed
	C_06_Base3 int,
	C_07_Comp3 AS C_06_Base3 + 1,
	-- 4. altering base
	C_08_Base4 int,
	C_09_Comp4 AS C_08_Base4 + 1,
	-- 5. altering both
	C_10_Base5 int,
	C_11_Comp5 AS C_10_Base5 + 1,
	-- 6. dropping computed
	C_12_Base6 int,
	C_13_Comp6 AS C_12_Base6 + 1,
	-- 7. dropping base
	C_14_Base7 int,
	C_15_Comp7 AS C_14_Base7 + 1,
	-- 8. dropping both
	C_16_Base8 int,
	C_17_Comp8 AS C_16_Base8 + 1,

	-- 9. Case diff - base
	C_20_CASEDIFF_BASE9 int,
	C_21_CaseDiff_Comp9 AS C_20_CaseDiff_Base9 + 1,
	-- 10. Case diff - computed
	C_22_CaseDiff_Base10 int,
	C_23_CASEDIFF_COMP10 AS C_22_CaseDiff_Base10 + 1,
	-- 11. Case diff - both
	C_24_CASEDIFF_BASE11 int,
	C_25_CASEDIFF_COMP11 AS C_24_CaseDiff_Base11 + 1,
);

CREATE TABLE dbo.ComputedColumnDESC -- column names in DESC order - MainDB
(
	C_99_Other int,

	-- 1. creating computed
	C_98_Base1 int,
	-- 2. creating both, base and computed
	-- 3. altering computed
	C_94_Base3 int,
	C_93_Comp3 AS C_94_Base3 + 1,
	-- 4. altering base
	C_92_Base4 int,
	C_91_Comp4 AS C_92_Base4 + 1,
	-- 5. altering both
	C_90_Base5 int,
	C_89_Comp5 AS C_90_Base5 + 1,
	-- 6. dropping computed
	C_88_Base6 int,
	C_87_Comp6 AS C_88_Base6 + 1,
	-- 7. dropping base
	C_86_Base7 int,
	C_85_Comp7 AS C_86_Base7 + 1,
	-- 8. dropping both
	C_84_Base8 int,
	C_83_Comp8 AS C_84_Base8 + 1,

	-- 9. Case diff - base
	C_70_CASEDIFF_BASE9 int,
	C_69_CaseDiff_Comp9 AS C_70_CaseDiff_Base9 + 1,
	-- 10. Case diff - computed
	C_68_CaseDiff_Base10 int,
	C_67_CASEDIFF_COMP10 AS C_68_CaseDiff_Base10 + 1,
	-- 11. Case diff - both
	C_66_CASEDIFF_BASE11 int,
	C_65_CASEDIFF_COMP11 AS C_66_CaseDiff_Base11 + 1,
);

"
			});
		}

		#endregion // Implementation
	}
}
