using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;

[TestedType(typeof(UpdateBM_GrossWeightUQToKilograms))]
sealed class UpdateBM_GrossWeightUQToKilogramsTest : DataTransformationTestCase
{
	const string UpdateCusInBondMoveHeaderQueryText =
		"""
		UPDATE dbo.CusInBondMoveHeader
		SET BM_GrossWeightUQ = @GrossWeightUQ,
			BM_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
			BM_SystemLastEditUser = @SystemLastEditUser
		WHERE BM_PK = @CusInBondMoveHeaderPk;
		""";

	const string SelectCusInBondMoveHeaderQueryText =
		"""
		SELECT BM_PK, BM_GrossWeightUQ, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser
		FROM dbo.CusInBondMoveHeader
		""";

	const string SystemLastEditUser = "OMR";

	static readonly DateTime SystemLastEditTimeUtc = new(year: 2025, month: 02, day: 18);

	readonly ICollection<Guid> CusInBondMoveHeaderInScope = new List<Guid>();

	readonly ICollection<Guid> CusInBondMoveHeaderOutOfScope = new List<Guid>();

	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateBM_GrossWeightUQToKilograms();

	protected override void PrepareTestData()
	{
		var beBranchPk = CreateBranch("BBE", "BE", "EUR", "BRS");
		var chBranchPk = CreateBranch("CCH", "CH", "CHF", "ZRH");
		var deBranchPk = CreateBranch("DDE", "DE", "EUR", "BER");
		var esBranchPk = CreateBranch("EES", "ES", "EUR", "BAR");
		var frBranchPk = CreateBranch("FFR", "FR", "EUR", "PRS");
		var gbBranchPk = CreateBranch("GGB", "GB", "GBP", "LND");
		var ieBranchPk = CreateBranch("IIE", "IE", "EUR", "DBL");
		var itBranchPk = CreateBranch("IIT", "IT", "EUR", "MLN");
		var nlBranchPk = CreateBranch("NNL", "NL", "EUR", "AMD");
		var noBranchPk = CreateBranch("NNO", "NO", "NOK", "OSL");
		var plBranchPk = CreateBranch("PPL", "PL", "PLN", "WRS");
		var trBranchPk = CreateBranch("TTR", "TR", "TRL", "STB");

		Guid[] euBranchPks = [beBranchPk, chBranchPk, deBranchPk, esBranchPk, frBranchPk, gbBranchPk, ieBranchPk, itBranchPk, nlBranchPk, noBranchPk, plBranchPk, trBranchPk];
		string[] headerTypes = ["A", "D", "DA"];
		string[] applicationCodes = ["NC5", "NCT"];

		var testCases = euBranchPks
			.SelectMany(branchPk => headerTypes, (branchPk, headerType) => new { BranchPk = branchPk, HeaderType = headerType })
			.SelectMany(tuple => applicationCodes, (tuple, applicationCode) => new { tuple.BranchPk, tuple.HeaderType, ApplicationCode = applicationCode })
			.ToArray();

		var jobNumber = 1;

		foreach (var testCase in testCases)
		{
			CusInBondMoveHeaderInScope.Add(CreateCusInBondMoveHeader(testCase.BranchPk, $"NCT{jobNumber++:D8}", testCase.ApplicationCode, testCase.HeaderType, string.Empty));
		}

		foreach (var testCase in testCases)
		{
			CusInBondMoveHeaderOutOfScope.Add(CreateCusInBondMoveHeader(testCase.BranchPk, $"NCT{jobNumber++:D8}", testCase.ApplicationCode, testCase.HeaderType, "KG"));
		}

		var twBranchPk = CreateBranch("TTW", "TW", "TWD", "TPE");
		CusInBondMoveHeaderOutOfScope.Add(CreateCusInBondMoveHeader(twBranchPk, $"TWJ{jobNumber++:D8}", "TW", string.Empty, string.Empty));
	}

	protected override void AssertTransformationResults() => CombineAssertions(() =>
	{
		var data = ReadData();

		foreach (var guid in CusInBondMoveHeaderInScope)
		{
			AssertCusInBondMoveHeaderUpdated(data, guid);
		}

		foreach (var guid in CusInBondMoveHeaderOutOfScope)
		{
			AssertCusInBondMoveHeaderNotUpdated(data, guid);
		}
	});

	static Guid CreateBranch(string companyCode, string countryCode, string currencyCode, string branchCode)
	{
		var companyPk = TestDataCreator.CreateCompany(companyCode, countryCode, currencyCode);
		var branchPk = TestDataCreator.CreateBranch(companyPk, branchCode, "Port1", countryCode);
		return branchPk;
	}

	Guid CreateCusInBondMoveHeader(Guid branchPk, string jobNumber, string applicationCode, string headerType, string weightUq)
	{
		var headerPk = TestDataCreator.CreateCusInbondHeader(jobNumber, branchPk, applicationCode, headerType);
		var cusInBondMoveHeaderPk = TestDataCreator.CreateCusInBondMoveHeader(headerPk);
		UpdateCusInBondMoveHeader(cusInBondMoveHeaderPk, weightUq);

		return cusInBondMoveHeaderPk;
	}

	void UpdateCusInBondMoveHeader(Guid cusInBondMoveHeaderPk, string weightUq)
	{
		using var command = TestConnection.Command(UpdateCusInBondMoveHeaderQueryText);
		command.AddParameter("@CusInBondMoveHeaderPk", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderPk);
		command.AddParameter("@GrossWeightUQ", SqlDbType.VarChar, weightUq);
		command.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, SystemLastEditTimeUtc);
		command.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, SystemLastEditUser);
		command.ExecuteNonQuery();
	}

	IReadOnlyDictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)> ReadData()
	{
		var results = new Dictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)>();
		TestConnection.ExecuteReader(SelectCusInBondMoveHeaderQueryText, record =>
		{
			results[record.GetGuid(0)] = (record.GetString(1), record.GetDateTime(2), record.GetString(3));
		});

		return results;
	}

	static void AssertCusInBondMoveHeaderUpdated(IReadOnlyDictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)> data, Guid cusInBondMoveHeaderPk)
	{
		var (weightUq, systemLastEditTimeUtc, systemLastEditUser) = data[cusInBondMoveHeaderPk];

		AssertEquals($"BM_PK: {cusInBondMoveHeaderPk}, BM_WeightUQ: Updated", "KG", weightUq);
		AssertLessThan($"BM_PK: {cusInBondMoveHeaderPk}, BM_SystemLastEditTimeUtc: Updated", SystemLastEditTimeUtc, systemLastEditTimeUtc);
		AssertEquals($"BM_PK: {cusInBondMoveHeaderPk}, BM_SystemLastEditUser: Updated", "~BP", systemLastEditUser);
	}

	static void AssertCusInBondMoveHeaderNotUpdated(IReadOnlyDictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)> data, Guid cusInBondMoveHeaderPk)
	{
		var (_, systemLastEditTimeUtc, systemLastEditUser) = data[cusInBondMoveHeaderPk];

		AssertEquals($"BM_PK: {cusInBondMoveHeaderPk}, BM_SystemLastEditTimeUtc: Not Updated", SystemLastEditTimeUtc, systemLastEditTimeUtc);
		AssertEquals($"BM_PK: {cusInBondMoveHeaderPk}, BM_SystemLastEditUser: Not Updated", "OMR", systemLastEditUser);
	}
}
