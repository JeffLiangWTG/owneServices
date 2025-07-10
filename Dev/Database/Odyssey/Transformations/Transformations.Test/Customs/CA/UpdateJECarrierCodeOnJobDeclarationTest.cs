using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateJECarrierCodeOnJobDeclaration))]
	public class UpdateJECarrierCodeOnJobDeclarationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals("Should have JE_CarrierCode after update when JE_AddInfo contains CarrierCode and JE_CarrierCode is empty in CA", CarrierCode, Db.Connection.ExecuteScalar($"SELECT JE_CarrierCode FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration1}'"));
			Assert("Should not have CarrierCode in JE_AddInfo", !Db.Connection.Exists($"FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration1}' AND JE_AddInfo LIKE '%CarrierCode=%'"));

			AssertNullOrEmpty("Should not have JE_CarrierCode after update when JE_AddInfo contains CarrierCode and JE_CarrierCode is empty in US", Db.Connection.ExecuteScalar($"SELECT JE_CarrierCode FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration2}'") as string);
			Assert("Should not have CarrierCode in JE_AddInfo", Db.Connection.Exists($"FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration2}' AND JE_AddInfo LIKE '%CarrierCode=%'"));

			AssertEquals("Should have JE_CarrierCode after update when JE_AddInfo contains CarrierCode and JE_CarrierCode is empty in CA", "TEST", Db.Connection.ExecuteScalar($"SELECT JE_CarrierCode FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration3}'"));
			Assert("Should not have CarrierCode in JE_AddInfo", !Db.Connection.Exists($"FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration3}' AND JE_AddInfo LIKE '%CarrierCode=%'"));

			AssertEquals("Should have JE_CarrierCode after update when JE_AddInfo contains CarrierCode and JE_CarrierCode is empty in CA", CarrierCode, Db.Connection.ExecuteScalar($"SELECT JE_CarrierCode FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration4}'"));
			Assert("Should not have CarrierCode in JE_AddInfo", !Db.Connection.Exists($"FROM dbo.JobDeclaration WHERE JE_PK = '{Declaration4}' AND JE_AddInfo LIKE '%CarrierCode=%'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateJECarrierCodeOnJobDeclaration();
		}

		const string CarrierCode = "ABCD";
		Guid Declaration1;
		Guid Declaration2;
		Guid Declaration3;
		Guid Declaration4;

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var caCompanyPK = testDataCreator.CreateGlbCompany("AAA", "CA");
			var usCompanyPK = testDataCreator.CreateGlbCompany("BBB", "US");

			var caBranchPK = testDataCreator.CreateGlbBranch("DDD", caCompanyPK);
			var usBranchPK = testDataCreator.CreateGlbBranch("EEE", usCompanyPK);

			var org = testDataCreator.CreateOrg("TES");

			Declaration1 = testDataCreator.CreateJobDeclaration("CA", caBranchPK, caCompanyPK, org, "", new DateTime(2024, 06, 15), 1, "", "", $"CarrierCode={CarrierCode}");
			Declaration2 = testDataCreator.CreateJobDeclaration("US", usBranchPK, usCompanyPK, org, "", new DateTime(2024, 06, 15), 2, "", "", $"CarrierCode={CarrierCode}");
			Declaration3 = testDataCreator.CreateJobDeclaration("CA", caBranchPK, caCompanyPK, org, "", new DateTime(2024, 06, 15), 3, "", CarrierCode, "CarrierCode=TEST");
			Declaration4 = testDataCreator.CreateJobDeclaration("CA", caBranchPK, caCompanyPK, org, "", new DateTime(2024, 06, 15), 4, "", CarrierCode, "");
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update JE_CarrierCode On CA JobDeclaration_1] ON [dbo].[JobDeclaration] ([JE_DataModel]) INCLUDE ([JE_AddInfo], [JE_AutoVersion], [JE_CarrierCode], [JE_SystemLastEditTimeUtc], [JE_SystemLastEditUser]) WHERE ([JE_DataModel]='CA') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};
	}
}
