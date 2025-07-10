using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.DE;

[TestedType(typeof(UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival))]
sealed class UpdateBM_CustomsStatusBM_PhaseNCTSPhase5ArrivalTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival();
	}

	public override string[] expectedIndex => new string[]
	{
		"NONCLUSTERED INDEX [IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BH_ApplicationCode_BH_HeaderType] ON [dbo].[CusInbondHeader] ([BH_ApplicationCode], [BH_HeaderType]) INCLUDE ([BH_GB], [BH_PK]) WHERE ([BH_ApplicationCode]='NC5' AND [BH_HeaderType]='A') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		"NONCLUSTERED INDEX [IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BM_Phase] ON [dbo].[CusInbondMoveHeader] ([BM_Phase]) WHERE ([BM_Phase] IN ('STU', 'FRC', '007', '044')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		"NONCLUSTERED INDEX [IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BM_CustomsStatus] ON [dbo].[CusInbondMoveHeader] ([BM_CustomsStatus]) WHERE ([BM_CustomsStatus] IN ('AUP', 'ART', 'UAP', 'CL1')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
	};

	protected override void PrepareTestData()
	{
		var companyDE = TestDataCreator.CreateCompany("DDE", "DE", "EUR");
		var branchDE = TestDataCreator.CreateBranch(companyDE, "DDE", "DEFIB", "DE");

		var companyLV = TestDataCreator.CreateCompany("DLV", "LV", "EUR");
		var branchLV = TestDataCreator.CreateBranch(companyLV, "DLV", "LVRIG", "LV");

		var headerP5DepDE = TestDataCreator.CreateCusInbondHeader("NC001", branchDE, "NC5", "D");

		var headerP5ArrDE = TestDataCreator.CreateCusInbondHeader("NC002", branchDE, "NC5", "A");

		var headerP4ArrDE = TestDataCreator.CreateCusInbondHeader("NC003", branchDE, "NCT", "A");

		var headerP5ArrLV = TestDataCreator.CreateCusInbondHeader("NC004", branchLV, "NC5", "A");
		var helper = new TestDbHelper(Db.Connection);

		var aupCustomsMoveHeaderP5ArrDE = CreateMoveHeader(helper, headerP5ArrDE, customsStatus: "AUP");
		var artCustomsMoveHeaderP5ArrDE = CreateMoveHeader(helper, headerP5ArrDE, customsStatus: "ART");
		var nonCustomsMoveHeaderP5ArrDE = CreateMoveHeader(helper, headerP5ArrDE, customsStatus: "NON");

		var mdsPhaseMoveHeaderP5ArrDE = CreateMoveHeader(helper, headerP5ArrDE, phaseStatus: "STU");
		var finPhaseMoveHeaderP5ArrDE = CreateMoveHeader(helper, headerP5ArrDE, phaseStatus: "FRC");
		var nonPhaseMoveHeaderP5ArrDE = CreateMoveHeader(helper, headerP5ArrDE, phaseStatus: "NON");

		var moveHeaderP5DepDE = CreateMoveHeader(helper, headerP5DepDE, "AUP", "STU");
		var moveHeaderP4ArrDE = CreateMoveHeader(helper, headerP4ArrDE, "AUP", "STU");
		var moveHeaderP5ArrLV = CreateMoveHeader(helper, headerP5ArrLV, "AUP", "STU");

		ExpectedValues.Add(aupCustomsMoveHeaderP5ArrDE, (nameof(aupCustomsMoveHeaderP5ArrDE), "", "UAP", true));
		ExpectedValues.Add(artCustomsMoveHeaderP5ArrDE, (nameof(artCustomsMoveHeaderP5ArrDE), "", "CL1", true));
		ExpectedValues.Add(nonCustomsMoveHeaderP5ArrDE, (nameof(nonCustomsMoveHeaderP5ArrDE), "", "NON", false));

		ExpectedValues.Add(mdsPhaseMoveHeaderP5ArrDE, (nameof(mdsPhaseMoveHeaderP5ArrDE), "007", "", true));
		ExpectedValues.Add(finPhaseMoveHeaderP5ArrDE, (nameof(finPhaseMoveHeaderP5ArrDE), "044", "", true));
		ExpectedValues.Add(nonPhaseMoveHeaderP5ArrDE, (nameof(nonPhaseMoveHeaderP5ArrDE), "NON", "", false));

		ExpectedValues.Add(moveHeaderP5DepDE, (nameof(moveHeaderP5DepDE), "AUP", "STU", false));
		ExpectedValues.Add(moveHeaderP4ArrDE, (nameof(moveHeaderP4ArrDE), "AUP", "STU", false));
		ExpectedValues.Add(moveHeaderP5ArrLV, (nameof(moveHeaderP5ArrLV), "AUP", "STU", false));
	}

	readonly Dictionary<Guid, (string message, string Phase, string CustomsStatus, bool LastAuditUpdated)> ExpectedValues = new ();

	Guid CreateMoveHeader(TestDbHelper helper, Guid header, string phaseStatus = "", string customsStatus = "")
	{
		var pk = Guid.NewGuid();
		helper.Insert(CusInBondMoveHeaderSchema.Constants.TableName, new
		{
			BM_PK = pk,
			BM_BH = header,
			BM_Phase = phaseStatus,
			BM_CustomsStatus = customsStatus,
			BM_SystemLastEditTimeUtc = LastUpdateDateTime.ToSqlFormat(),
		});
		return pk;
	}

	protected override void AssertTransformationResults()
	{
		var results = new List<(Guid pk, string customsStatus, string phaseStatus, DateTime lastUpdated, string lastUpdatedBy)>();

		TestConnection.ExecuteReader("SELECT * FROM CusInBondMoveHeader",
			reader => results.Add(((Guid)reader["BM_PK"], (string)reader["BM_CustomsStatus"], (string)reader["BM_Phase"], (DateTime)reader["BM_SystemLastEditTimeUtc"], (string)reader["BM_SystemLastEditUser"])));

		foreach (var result in results)
		{
			var expected = ExpectedValues[result.pk];
			AssertEquals("Custom status for " + expected.message, expected.CustomsStatus, result.customsStatus);
			AssertEquals("Phase for " + expected.message, expected.Phase, result.phaseStatus);
			if (expected.LastAuditUpdated)
			{
				AssertEquals("LastUpdatedBy for " + expected.message, "E", result.lastUpdatedBy);
				AssertNotEquals("Last Updated Date changed for " + expected.message, LastUpdateDateTime, result.lastUpdated);
			}
			else
			{
				AssertEquals("LastUpdatedBy for " + expected.message, "A", result.lastUpdatedBy);
				AssertEquals("Last Updated Date not changed for " + expected.message, LastUpdateDateTime, result.lastUpdated);
			}
		}

		AssertContainsExactElementsInAnyOrder("All move headers checked", results.Select(e => e.pk), ExpectedValues.Keys);
	}

	static readonly DateTime LastUpdateDateTime = new (2024, 03, 20, 12, 30, 00);
}
