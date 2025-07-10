using System;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(UpdateAccChargeTaxOverrideTransportModeEmptyValue))]
	class UpdateAccChargeTaxOverrideTransportModeEmptyValueTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update AccChargeTaxOverride AO_TransportMode empty value with 'ALL'. Cleanup data for new constraint_1] ON [dbo].[AccChargeTaxOverride] ([AO_TransportMode]) INCLUDE ([AO_SystemLastEditTimeUtc], [AO_SystemLastEditUser]) WHERE ([AO_TransportMode]='') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update AccChargeTaxOverride AO_TransportMode empty value with 'ALL'. Cleanup data for new constraint_2] ON [dbo].[AccChargeTaxOverride] ([AO_TransportMode]) INCLUDE ([AO_A9_DefaultVATClass], [AO_GB], [AO_SystemCreateTimeUtc], [AO_SystemLastEditTimeUtc]) WHERE ([AO_TransportMode]<>'ALL' AND [AO_TransportMode]<>'AIR' AND [AO_TransportMode]<>'COU' AND [AO_TransportMode]<>'FAS' AND [AO_TransportMode]<>'FIX' AND [AO_TransportMode]<>'FSA' AND [AO_TransportMode]<>'IWT' AND [AO_TransportMode]<>'MAI' AND [AO_TransportMode]<>'OWN' AND [AO_TransportMode]<>'RAI' AND [AO_TransportMode]<>'ROA' AND [AO_TransportMode]<>'SEA') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateAccChargeTaxOverrideTransportModeEmptyValue();

		protected override void AssertTransformationResults()
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccChargeTaxOverride ORDER BY AO_TransportMode");
			AssertEquals("row with AO_transportMode ZZZ is deleted", 13, dataTable.Rows.Count);

			AssertEquals("AIR", (string)dataTable.Rows[0]["AO_TransportMode"]);
			AssertEquals("row with empty value is changed to ALL", "ALL", (string)dataTable.Rows[1]["AO_TransportMode"]);
			AssertEquals("ALL", (string)dataTable.Rows[2]["AO_TransportMode"]);
			AssertEquals("COU", (string)dataTable.Rows[3]["AO_TransportMode"]);
			AssertEquals("FAS", (string)dataTable.Rows[4]["AO_TransportMode"]);
			AssertEquals("FIX", (string)dataTable.Rows[5]["AO_TransportMode"]);
			AssertEquals("FSA", (string)dataTable.Rows[6]["AO_TransportMode"]);
			AssertEquals("IWT", (string)dataTable.Rows[7]["AO_TransportMode"]);
			AssertEquals("MAI", (string)dataTable.Rows[8]["AO_TransportMode"]);
			AssertEquals("OWN", (string)dataTable.Rows[9]["AO_TransportMode"]);
			AssertEquals("RAI", (string)dataTable.Rows[10]["AO_TransportMode"]);
			AssertEquals("ROA", (string)dataTable.Rows[11]["AO_TransportMode"]);
			AssertEquals("SEA", (string)dataTable.Rows[12]["AO_TransportMode"]);

			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.StmALog WHERE SL_Table = '{AccChargeTaxOverrideSchema.Constants.TableName}'");
			AssertEquals(2, dataTable.Rows.Count);
			AssertEquals(rowWithIncorrectTransportModePK, (Guid)dataTable.Rows[0]["SL_Parent"]);
			var expectedInfo = $"UpdateAccChargeTaxOverrideTransportModeEmptyValue|AO_ParentID:{rowWithIncorrectTransportModeParentID}|AO_ParentTableCode:AX|AO_TransportMode:ZZZ|AO_AT:{rowWithIncorrectTransportModeTaxID.ToString().ToUpper()}";
			AssertEquals(expectedInfo, (string)dataTable.Rows[0]["SL_Reference"]);
			AssertEquals("DEL", (string)dataTable.Rows[0]["SL_SE_NKEvent"]);
			expectedInfo = "UpdateAccChargeTaxOverrideTransportModeEmptyValue|AO_ParentID:00000000-0000-0000-0000-000000000000|AO_ParentTableCode:AX|AO_TransportMode:XXX|AO_AT:NULL";
			AssertEquals(expectedInfo, (string)dataTable.Rows[1]["SL_Reference"]);
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			DBTransformationTestHelper.DropConstraintIfExists(AccChargeTaxOverrideSchema.Constants.TableName, "Constraint_AO_TransportMode");

			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT TOP 1 AT_PK FROM {AccTaxRateSchema.Constants.TableName}");
			AssertEquals(1, dataTable.Rows.Count);
			rowWithIncorrectTransportModeTaxID = (Guid)dataTable.Rows[0]["AT_PK"];

			//Good data
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "AIR");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, string.Empty);
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "ALL");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "COU");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "FAS");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "FIX");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "FSA");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "IWT");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "MAI");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "OWN");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "RAI");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "ROA");
			helper.InsertTaxOverrideRule("REV", true, "ALL", "ALL", "ALL", "ALL", "ALL", Guid.Empty, "SEA");

			//Bad data
			rowWithIncorrectTransportModeParentID = new Guid();
			rowWithIncorrectTransportModePK = helper.InsertTaxOverrideRule("REV", true, "ALL", "EXP", "ALL", "ALL", "ALL", rowWithIncorrectTransportModeParentID, "ZZZ", rowWithIncorrectTransportModeTaxID);
			helper.InsertTaxOverrideRule("REV", true, "ALL", "EXP", "ALL", "ALL", "ALL", Guid.Empty, "XXX");

			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT 1 FROM dbo.AccChargeTaxOverride");
			AssertEquals(15, dataTable.Rows.Count);

			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT 1 FROM dbo.StmALog WHERE SL_Table = '{AccChargeTaxOverrideSchema.Constants.TableName}'");
			AssertEquals(0, dataTable.Rows.Count);
		}

		Guid rowWithIncorrectTransportModePK;
		Guid rowWithIncorrectTransportModeParentID;
		Guid rowWithIncorrectTransportModeTaxID;
	}
}
