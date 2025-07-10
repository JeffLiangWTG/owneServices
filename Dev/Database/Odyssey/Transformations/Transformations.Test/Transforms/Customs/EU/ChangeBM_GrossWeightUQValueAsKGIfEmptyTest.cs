using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;

[TestedType(typeof(ChangeBM_GrossWeightUQValueAsKGIfEmpty))]
class ChangeBM_GrossWeightUQValueAsKGIfEmptyTest : DataTransformationTestCase
{
	public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update BM_GrossWeightUQ as default value KG when BM_GrossWeight is not null._1] ON [dbo].[CusInBondMoveHeader] ([BM_GrossWeightUQ], [BM_GrossWeight]) INCLUDE ([BM_SystemLastEditTimeUtc], [BM_SystemLastEditUser]) WHERE ([BM_GrossWeightUQ]='' AND [BM_GrossWeight]<>(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

	protected override DataTransformation GetNewTestTransformationInstance() => new ChangeBM_GrossWeightUQValueAsKGIfEmpty();

	protected override void PrepareTestData()
	{
		var sql = $@"
				Alter table dbo.CusInBondMoveHeader Nocheck Constraint All

				DECLARE @CompanyPK			UNIQUEIDENTIFIER = NEWID(),
						@BranchPK			UNIQUEIDENTIFIER = NEWID(),
						@bhPK1				UNIQUEIDENTIFIER = '{bhPK1}',
						@bmPK1				UNIQUEIDENTIFIER = '{bmPK1}',
						@bmPK2				UNIQUEIDENTIFIER = '{bmPK2}',
						@bmPK3				UNIQUEIDENTIFIER = '{bmPK3}'

				DELETE FROM GlbCompany WHERE GC_PK = @CompanyPK
				DELETE FROM GlbBranch WHERE GB_PK = @BranchPK
				DELETE FROM CusInbondHeader WHERE BH_PK = @bhPK1

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)	VALUES
					(@companyPK, 'DE', 'EUR', 'DDE', 'DE company', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@branchPK, @companyPK, 'BRN', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO dbo.CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@bhPK1, @BranchPK, 'BH01', 'NCT', '2022-07-30', 1, GetUtcDate(), 'STD', 'STD');

				INSERT INTO dbo.CusInBondMoveHeader (BM_PK, BM_BH,BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_GrossWeight, BM_GrossWeightUQ) VALUES
					(@bmPK1, @bhPK1, '2022-07-30', 'STD', GetUtcDate(), 'STD', 0, ''),
					(@bmPK2, @bhPK1, '2022-12-15', 'STD', GetUtcDate(), 'STD', 15, ''),
					(@bmPK3, @bhPK1, '2023-03-02', 'STD', GetUtcDate(), 'STD', 36, '')";

		TestConnection.ExecuteNonQuery(sql);
	}

	protected override void AssertTransformationResults()
	{
		var resultList = new List<Tuple<Guid, Guid, decimal, string, string, DateTime>>();
		var utcDate = DateTime.UtcNow.ToShortDateString();
		TestConnection.ExecuteReader("SELECT * FROM dbo.CusInBondMoveHeader",
			reader => resultList.Add(Tuple.Create((Guid)reader["BM_PK"], (Guid)reader["BM_BH"], (decimal)reader["BM_GrossWeight"], (string)reader["BM_GrossWeightUQ"], (string)reader["BM_SystemLastEditUser"], (DateTime)reader["BM_SystemLastEditTimeUtc"])));
		AssertContainsExactElementsInAnyOrder(new[]
		{
				$"{bmPK1}, {bhPK1}, 0.000000, , STD, {utcDate}",
				$"{bmPK2}, {bhPK1}, 15.000000, KG, E, {utcDate}",
				$"{bmPK3}, {bhPK1}, 36.000000, KG, E, {utcDate}",
			}, resultList.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item4}, {x.Item5}, {x.Item6.ToShortDateString()}").ToArray());
	}
	Guid bhPK1 = Guid.NewGuid();
	Guid bmPK1 = Guid.NewGuid();
	Guid bmPK2 = Guid.NewGuid();
	Guid bmPK3 = Guid.NewGuid();
}
