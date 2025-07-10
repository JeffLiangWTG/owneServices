using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(PopulateCRD_DataModel))]
	sealed class PopulateCRD_DataModelTest : DataTransformationTestCase
	{
		public void TestOffLinePostUpgrade()
		{
			PrepareTestData();
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateCRD_DataModel();
			transform.Initialise(null, manager);
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
		}

		protected override void AssertTransformationResults()
		{
			var values = new List<(string, string)>();
			var sqlText = @"SELECT GC_RN_NKCountryCode, CRD_DataModel
FROM dbo.CusReconDeclaration 
INNER JOIN GlbBranch on GB_PK= CRD_GB_Branch
INNER JOIN GlbCompany on GC_PK = GB_GC
ORDER BY GC_RN_NKCountryCode";

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add((reader.GetString(0), reader.GetString(1)));
				}
			}

			AssertContainsExactElementsInExactOrder(new[] {
				("AI", "AI"),
				("BW", "BW"),
				("CH", "CH"),
				("CN", "CN"),
				("KR", "KR"),
				("LS", "LS"),
				("NA", "NA"),
				("PL", "PL"),
				("SZ", "SZ"),
				("TR", "TR"),
				("US", "US"),
			}, values);
		}

		protected override void PrepareTestData()
		{
			var referenceNumber = 0;
			foreach (var countryCode in new[] { "AI", "BW", "CH", "CN", "PL", "TR", "US", "NA", "LS", "SZ", "KR" })
			{
				referenceNumber++;

				var sqlText = $@"
DECLARE @CountryCode VARCHAR(2) = '{countryCode}';
DECLARE @ReferenceNumber INT = {referenceNumber};
DECLARE @CompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CompanyPK, 'D' + @CountryCode, 'AU company', @CountryCode, 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @BranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BranchPK, @CompanyPK, 'B' + @CountryCode, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @DecPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@DecPK, @BranchPK, @CompanyPK, @ReferenceNumber, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusReconDeclaration(CRD_PK, CRD_ApplicationCode, CRD_JobReferenceNumber, CRD_JE_LeadDeclaration, CRD_GB_Branch, CRD_SystemCreateTimeUtc, CRD_SystemCreateUser, CRD_SystemLastEditTimeUtc, CRD_SystemLastEditUser) VALUES (NEWID(), 'BLT', @ReferenceNumber, @DecPK, @BranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
				Db.Connection.ExecuteNonQuery(sqlText);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateCRD_DataModel();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AddColumnIfNotExists();
			DBTransformationTestHelper.DropConstraintIfExists(CusReconDeclarationSchema.Constants.TableName, "Constraint_CRD_DataModel");
		}

		static void AddColumnIfNotExists()
		{
			if (!DbObjectCreator.ColumnExists(Db.Connection, CusReconDeclarationSchema.Constants.TableName, CusReconDeclarationSchema.Constants.CRD_DataModel))
			{
				Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.CusReconDeclaration
		ADD CRD_DataModel VarChar(3) NOT NULL
		DEFAULT ''");
			}
		}
	}
}
