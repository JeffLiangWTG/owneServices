using System;
using System.Collections.Generic;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU;

[TestedType(typeof(UpdateBM_GONumberForNC5ArrivalMovementHeaderIfSimplified))]
sealed class UpdateBM_GONumberForNC5ArrivalMovementHeaderIfSimplifiedTest : DataTransformationTestCase
{
	public override string[] expectedIndex => new[]
	{
		"NONCLUSTERED INDEX [_WTG__Update BM_GONumber to A3 for NCTS5 arrival movement header if who is previously Y_1] ON [dbo].[CusInBondMoveHeader] ([BM_SubApplicationCode], [BM_GONumber]) INCLUDE ([BM_BH], [BM_SystemLastEditTimeUtc], [BM_SystemLastEditUser]) WHERE ([BM_SubApplicationCode]='A' AND [BM_GONumber]='Y') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
	};

	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateBM_GONumberForNC5ArrivalMovementHeaderIfSimplified();

	protected override void PrepareTestData()
	{
		var frCompanyPK = TestDataCreator.CreateCompany("FRC", "FR", "EUR");
		frBranchPK = TestDataCreator.CreateBranch(frCompanyPK, "PAR", "FR00001");
		var esCompanyPK = TestDataCreator.CreateCompany("ESC", "ES", "EUR");
		esBranchPK = TestDataCreator.CreateBranch(esCompanyPK, "AVL", "ES00001");
		orgHeader = TestDataCreator.CreateOrganisation("ORG", "Demo Organization");

		var moveHeader1 = CreateMovementHeader("NC001", frBranchPK, true, "A", "A3", "ACE");
		var moveHeader2 = CreateMovementHeader("NC002", frBranchPK, true, "A", "Y", "ACE");
		var moveHeader3 = CreateMovementHeader("NC003", frBranchPK, true, "A", "Y", "ACT");
		var moveHeader4 = CreateMovementHeader("NC004", frBranchPK, true, "A", "Y", string.Empty);

		var moveHeaderGONumberN = CreateMovementHeader("NC005", frBranchPK, true, "A", "N", "ACE");
		var moveHeaderDeparture = CreateMovementHeader("NC006", frBranchPK, true, "D", "Y", "ACE");
		var moveHeaderInAnotherCountry = CreateMovementHeader("NC007", esBranchPK, true, "A", "Y", "ACE");
		var moveHeaderNC4 = CreateMovementHeader("NC008", frBranchPK, false, "A", "Y", "ACE");
	}

	protected override void AssertTransformationResults()
	{
		var sql = $@"SELECT BH_JobReference, BM_GONumber FROM CusInBondMoveHeader JOIN CusInBondHeader ON BM_BH = BH_PK";
		var result = new List<(string, string)>();

		TestConnection.ExecuteReader(sql, reader => result.Add(((string)reader["BH_JobReference"], (string)reader["BM_GONumber"])));
		AssertContainsExactElementsInAnyOrder(new List<(string, string)>()
		{
			("NC001", "A3"),
			("NC002", "A3"),
			("NC003", "A3"),
			("NC004", "A3"),
			("NC005", "N"),
			("NC006", "Y"),
			("NC007", "A3"),
			("NC008", "Y"),
		}, result);
	}

	Guid CreateMovementHeader(string jobNumber, Guid branchPK, bool isNC5, string moveType, string gONumber, string authorisationCode)
	{
		var header = TestDataCreator.CreateCusInbondHeader(jobNumber, branchPK, isNC5 ? "NC5" : "NCT", moveType);
		var moveHeader = TestDataCreator.CreateCusInBondMoveHeader(header, moveType, gONumber: gONumber);
		if (!string.IsNullOrEmpty(authorisationCode))
		{
			var isArrival = moveType == "A";
			TestDataCreator.CreateCusAuthorizationUsage(isArrival ? header : moveHeader, isArrival ? "BH" : "BM", orgHeader, authorisationCode, "number", 0);
		}
		return moveHeader;
	}

	Guid frBranchPK;
	Guid esBranchPK;
	Guid orgHeader;
}
