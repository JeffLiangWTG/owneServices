using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(PopulateSourceForCusEntryLineFee))]
	internal class PopulateSourceForCusEntryLineFeeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateSourceForCusEntryLineFee();

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			TestCase(nameof(cf1111), cf1111, null);
			TestCase(nameof(cf1211), cf1211, null);
			TestCase(nameof(cf2111), cf2111, null);
			TestCase(nameof(cf2211), cf2211, "CW1");
			TestCase(nameof(cf2212), cf2212, "CW1");
			TestCase(nameof(cf2213), cf2213, "CW2");
			TestCase(nameof(cf2311), cf2311, "CW1");
			TestCase(nameof(cf2312), cf2312, "CW1");
			TestCase(nameof(cf2313), cf2313, "CW2");
			TestCase(nameof(cf0011), cf0011, "CW1");
			TestCase(nameof(cf0012), cf0012, "CW1");
			TestCase(nameof(cf0013), cf0013, "CW2");

			void TestCase(string key, Guid clPK, string expectedSource)
			{
				var actualSource = TestConnection.ExecuteScalar($"SELECT CF_Source FROM CusEntryLineFee WHERE CF_PK = @clPK", cmd => cmd.AddParameter("@clPK", SqlDbType.UniqueIdentifier, clPK));
				AssertEquals(key, expectedSource == null ? DBNull.Value : expectedSource, actualSource);
			}
		}

		public void TestLogging()
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)(new PopulateSourceForCusEntryLineFee());
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(new[]
				{
					"Processing completed, 3 records updated.",
					"\tCompleted: Populate Source For Entry Line Fee",
				}, logger);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var company = TestDataCreator.CreateCompany("DCA", "CA", "CAD");
			var branch = TestDataCreator.CreateBranch(company, "BRN", "CAXXX");

			// CH_DataModel != CA -> Ignore
			var je1 = TestDataCreator.CreateJobDeclaration(branch, company, "1", "IMP", 1);
			var ch11 = TestDataCreator.CreateCusEntryHeader(je1, 1, "B3C", dataModel: "US");
			var cl111 = TestDataCreator.CreateCusEntryLine(ch11, 1);
			cf1111 = CreateCusEntryLineFee(cl111, 1, null);
			var ch12 = TestDataCreator.CreateCusEntryHeader(je1, 1, "CAD", dataModel: "US");
			var cl121 = TestDataCreator.CreateCusEntryLine(ch11, 1);
			cf1211 = CreateCusEntryLineFee(cl111, 1, null);

			// CH_MessageType NOT IN ('B3C', 'CAD') -> Ignore
			var je2 = TestDataCreator.CreateJobDeclaration(branch, company, "2", "IMP", 2);
			var ch21 = TestDataCreator.CreateCusEntryHeader(je2, 2, "IMP", dataModel: "CA");
			var cl211 = TestDataCreator.CreateCusEntryLine(ch21, 2);
			cf2111 = CreateCusEntryLineFee(cl211, 2, null);

			// CH_MessageType = B3C -> Update if CF_Source is null
			var ch22 = TestDataCreator.CreateCusEntryHeader(je2, 2, "B3C", dataModel: "CA");
			var cl221 = TestDataCreator.CreateCusEntryLine(ch22, 2);
			cf2211 = CreateCusEntryLineFee(cl221, 2, null);
			cf2212 = CreateCusEntryLineFee(cl221, 2, "CW1");
			cf2213 = CreateCusEntryLineFee(cl221, 2, "CW2");

			// CH_MessageType = CAD -> Update if CF_Source is null
			var ch23 = TestDataCreator.CreateCusEntryHeader(je2, 2, "CAD", dataModel: "CA");
			var cl231 = TestDataCreator.CreateCusEntryLine(ch23, 2);
			cf2311 = CreateCusEntryLineFee(cl231, 2, null);
			cf2312 = CreateCusEntryLineFee(cl231, 2, "CW1");
			cf2313 = CreateCusEntryLineFee(cl231, 2, "CW2");

			// ClusterKey = 0 will be processed
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, JobDeclarationSchema.Constants.TableName, "Constraint_JE_ClusterKey"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, CusEntryHeaderSchema.Constants.TableName, "Constraint_CH_ClusterKey"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, CusEntryLineSchema.Constants.TableName, "Constraint_CL_ClusterKey"))
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, CusEntryLineFeeSchema.Constants.TableName, "Constraint_CF_ClusterKey"))
			{
				var je0 = TestDataCreator.CreateJobDeclaration(branch, company, "0", "IMP", 0);
				var ch01 = TestDataCreator.CreateCusEntryHeader(je0, 0, "B3C", dataModel: "CA");
				var cl001 = TestDataCreator.CreateCusEntryLine(ch01, 0);
				cf0011 = CreateCusEntryLineFee(cl001, 0, null);
				cf0012 = CreateCusEntryLineFee(cl001, 0, "CW1");
				cf0013 = CreateCusEntryLineFee(cl001, 0, "CW2");
			}
		}

		Guid CreateCusEntryLineFee(Guid clPK, int clusterKey, string source)
		{
			var cfPK = Guid.NewGuid();
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, CusEntryLineFeeSchema.Constants.TableName, "Constraint_CF_Source"))
			{
				TestConnection.ExecuteNonQuery("INSERT INTO dbo.CusEntryLineFee([CF_PK],[CF_IsValid],[CF_ChargeType],[CF_ChargeAmount],[CF_CL],[CF_Source],[CF_IsLandedCostOnly],[CF_ClusterKey],[CF_SystemCreateTimeUtc],[CF_SystemCreateUser],[CF_SystemLastEditTimeUtc],[CF_SystemLastEditUser]) VALUES (@cfPk,1,'EXS',1,@cfCl,@cfSource,0,@clusterKey,GetUtcDate(),'~BP',GetUtcDate(),'~BP');", cmd =>
				{
					cmd.AddParameter("@cfPk", SqlDbType.UniqueIdentifier, cfPK);
					cmd.AddParameter("@cfCl", SqlDbType.UniqueIdentifier, clPK);
					cmd.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
					cmd.AddParameter("@cfSource", SqlDbType.VarChar, source == null ? DBNull.Value : source);
				});
			}
			return cfPK;
		}

		Guid cf0011; Guid cf0012; Guid cf0013; Guid cf1111; Guid cf1211; Guid cf2111; Guid cf2211; Guid cf2212; Guid cf2213; Guid cf2311; Guid cf2312; Guid cf2313;
	}
}
