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

[TestedType(typeof(UpdateB0_WeightUQToKilograms))]
sealed class UpdateB0_WeightUQToKilogramsTest : DataTransformationTestCase
{
	const string UpdateCusInBondBillQueryText =
		"""
		UPDATE dbo.CusInBondBill
		SET B0_WeightUQ = @WeightUQ,
			B0_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
			B0_SystemLastEditUser = @SystemLastEditUser
		WHERE B0_PK = @CusInBondBillPk;
		""";

	const string SelectCusInBondBillQueryText =
		"""
		SELECT B0_PK, B0_WeightUQ, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser
		FROM dbo.CusInBondBill
		""";

	const string SystemLastEditUser = "OMR";

	static readonly DateTime SystemLastEditTimeUtc = new(year: 2025, month: 02, day: 18);

	readonly ICollection<Guid> CusInBondBillsInScope = new List<Guid>();

	readonly ICollection<Guid> CusInBondBillsOutOfScope = new List<Guid>();

	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateB0_WeightUQToKilograms();

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
				.SelectMany(_ => headerTypes, (branchPk, headerType) => new { BranchPk = branchPk, HeaderType = headerType })
				.SelectMany(_ => applicationCodes, (tuple, applicationCode) => new { tuple.BranchPk, tuple.HeaderType, ApplicationCode = applicationCode })
			.ToArray();

		var jobNumber = 1;

		foreach (var testCase in testCases)
		{
			CusInBondBillsInScope.Add(CreateCusInBondBill(testCase.BranchPk, $"NCT{jobNumber++:D8}", testCase.ApplicationCode, testCase.HeaderType, string.Empty));
		}

		foreach (var testCase in testCases)
		{
			CusInBondBillsOutOfScope.Add(CreateCusInBondBill(testCase.BranchPk, $"NCT{jobNumber++:D8}", testCase.ApplicationCode, testCase.HeaderType, "KG"));
		}

		var twBranchPk = CreateBranch("TTW", "TW", "TWD", "TPE");
		CusInBondBillsOutOfScope.Add(CreateCusInBondBill(twBranchPk, $"TWJ{jobNumber:D8}", "TW", headerType: string.Empty, weightUq: string.Empty));
	}

	protected override void AssertTransformationResults() => CombineAssertions(() =>
	{
		var data = ReadData();

		foreach (var guid in CusInBondBillsInScope)
		{
			AssertCusInBondBillUpdated(data, guid);
		}

		foreach (var guid in CusInBondBillsOutOfScope)
		{
			AssertCusInBondBillNotUpdated(data, guid);
		}
	});

	static Guid CreateBranch(string companyCode, string countryCode, string currencyCode, string branchCode)
	{
		var companyPk = TestDataCreator.CreateCompany(companyCode, countryCode, currencyCode);
		var branchPk = TestDataCreator.CreateBranch(companyPk, branchCode, "Port1", countryCode);
		return branchPk;
	}

	Guid CreateCusInBondBill(Guid branchPk, string jobNumber, string applicationCode, string headerType, string weightUq)
	{
		var headerPk = TestDataCreator.CreateCusInbondHeader(jobNumber, branchPk, applicationCode, headerType);
		var cusInBondBillPk = TestDataCreator.CreateCusInbondBill(headerPk);
		UpdateCusInBondBill(cusInBondBillPk, weightUq);

		return cusInBondBillPk;
	}

	void UpdateCusInBondBill(Guid cusInBondBillPk, string weightUq)
	{
		using var command = TestConnection.Command(UpdateCusInBondBillQueryText);
		command.AddParameter("@CusInBondBillPk", SqlDbType.UniqueIdentifier, cusInBondBillPk);
		command.AddParameter("@WeightUQ", SqlDbType.VarChar, weightUq);
		command.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, SystemLastEditTimeUtc);
		command.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, SystemLastEditUser);
		command.ExecuteNonQuery();
	}

	IReadOnlyDictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)> ReadData()
	{
		var results = new Dictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)>();
		TestConnection.ExecuteReader(SelectCusInBondBillQueryText, record =>
		{
			results[record.GetGuid(0)] = (record.GetString(1), record.GetDateTime(2), record.GetString(3));
		});

		return results;
	}

	static void AssertCusInBondBillUpdated(IReadOnlyDictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)> data, Guid cusInBondBillPk)
	{
		var (weightUq, systemLastEditTimeUtc, systemLastEditUser) = data[cusInBondBillPk];

		AssertEquals($"B0_PK: {cusInBondBillPk}, B0_WeightUQ: Updated", "KG", weightUq);
		AssertLessThan($"B0_PK: {cusInBondBillPk}, B0_SystemLastEditTimeUtc: Updated", SystemLastEditTimeUtc, systemLastEditTimeUtc);
		AssertEquals($"B0_PK: {cusInBondBillPk}, B0_SystemLastEditUser: Updated", "~BP", systemLastEditUser);
	}

	static void AssertCusInBondBillNotUpdated(IReadOnlyDictionary<Guid, (string WeightUq, DateTime SystemLastEditTimeUtc, string SystemLastEditUser)> data, Guid cusInBondBillPk)
	{
		var (_, systemLastEditTimeUtc, systemLastEditUser) = data[cusInBondBillPk];

		AssertEquals($"B0_PK: {cusInBondBillPk}, B0_SystemLastEditTimeUtc: Not Updated", SystemLastEditTimeUtc, systemLastEditTimeUtc);
		AssertEquals($"B0_PK: {cusInBondBillPk}, B0_SystemLastEditUser: Not Updated", "OMR", systemLastEditUser);
	}
}
