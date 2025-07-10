using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(UpdateJobChargeProFormaCostAndRevenue))]
	class UpdateJobChargeProFormaCostAndRevenueTest : DataTransformationTestCase
	{
		public void TestLastProcessedGuid_IsDeleted_AfterTransformation()
		{
			PrepareTestData();
			var transformationInstance = GetNewTestTransformationInstance();

			AssertExceptionThrown<OperationCanceledException>(() => transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));
			var lastProcessedGuid = ExtProperty.Table.Select(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, "LastProcessedGuidForUpdateJRProformaCostOrRevenue");
			AssertNotNull("Precondition - LastProcessedGuid", lastProcessedGuid);

			transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(false));
			lastProcessedGuid = ExtProperty.Table.Select(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, "LastProcessedGuidForUpdateJRProformaCostOrRevenue");
			AssertNull("LastProcessedGuid", lastProcessedGuid);
		}

		public void TestJobChargeTableSize_IsDeleted_AfterTransformation()
		{
			PrepareTestData();
			var transformationInstance = GetNewTestTransformationInstance();

			AssertExceptionThrown<OperationCanceledException>(() => transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));
			var jobChargeTableSize = ExtProperty.Table.Select(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, "JobChargeTableSize");
			AssertNotNull("Precondition - JobChargeTableSize", jobChargeTableSize);

			transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(false));
			jobChargeTableSize = ExtProperty.Table.Select(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, "JobChargeTableSize");
			AssertNull("JobChargeTableSize", jobChargeTableSize);
		}

		public void TestProgressIsLogged_WhenCancellationRequestedOnToken()
		{
			PrepareTestData();
			string log = null;
			var transformationInstance = GetNewTestTransformationInstance();
			AssertExceptionThrown<OperationCanceledException>(() => ((IOnlineTransformation)transformationInstance).Run(s => log = s, new CancellationToken(true)));
			AssertContains("PK", "PK of Last processed record: ", log);
			AssertContains("Table Size", "total no of records: ", log);
		}

		public void TestOnlinePostUpgradeTransformation_Batching()
		{
			var generatedGuids = PrepareTestDataForBatching();
			var transformationInstance = new UpdateJobChargeProFormaCostAndRevenue(2);
			transformationInstance.Initialise(manager: new DummyUpgradeManager());

			var expectedRatingJobChargeGuids = new List<Guid>();
			var expectedNonRatingJobChargeGuids = new List<Guid>();

			for (var i = 0; i < generatedGuids.Count; i++)
			{
				expectedRatingJobChargeGuids.Add(generatedGuids[i++]);
				expectedNonRatingJobChargeGuids.Add(generatedGuids[i]);
				AssertExceptionThrown<OperationCanceledException>(() => transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));
				AssertResult(true, RatingJobCharges, expectedRatingJobChargeGuids);
				AssertResult(false, NonRatingJobCharges, expectedNonRatingJobChargeGuids);
			}
			void AssertResult(bool isRatingJobChargeData, List<Guid> allGuids, List<Guid> expectedGuids)
			{
				foreach (var guid in allGuids)
				{
					if (expectedGuids.Contains(guid))
					{
						JobCharge.AssertFromDB(Db.Connection, guid).ExpectEquals("JR_ProformaCost", p => p.JR_ProFormaCost, isRatingJobChargeData)
						.ExpectEquals("JR_ProformaRevenue", p => p.JR_ProFormaRevenue, isRatingJobChargeData).VerifyAll("Valid Rating JobCharges");
					}
					else
					{
						JobCharge.AssertFromDB(Db.Connection, guid).ExpectEquals("JR_ProformaCost", p => p.JR_ProFormaCost, !isRatingJobChargeData)
						.ExpectEquals("JR_ProformaRevenue", p => p.JR_ProFormaRevenue, !isRatingJobChargeData).VerifyAll("Invalid Rating JobCharges");
					}
				}
			}
		}

		public void TestTransformation_ThrowsException_WhenCancellationRequestedOnToken()
		{
			PrepareTestData();
			var transformationInstance = GetNewTestTransformationInstance();
			AssertExceptionThrown<OperationCanceledException>("Exception when cancellation requested on token", () => transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformation = new UpdateJobChargeProFormaCostAndRevenue(5);
			transformation.Initialise(manager: new DummyUpgradeManager());
			return transformation;
		}

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var company = new GlbCompany("CMP", "AU").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BRN", company.PK).AppendInsertAndReturnObject(sql);
			var department = new GlbDepartment("DPT").AppendInsertAndReturnObject(sql);
			var accChargeCode = new AccChargeCode("ORG").AppendInsertAndReturnObject(sql);
			var jobShipment1 = new JobShipment("TESTJOBSHIPMENT1");
			var jobShipment2 = new JobShipment("TESTJOBSHIPMENT2");
			var jobShipment3 = new JobShipment("TESTJOBSHIPMENT3");
			var jobShipment4 = new JobShipment("TESTJOBSHIPMENT4");

			var ratingJobHeader1 = new JobHeader("WRK", branch.PK, department.PK, company.PK, jobShipment1.PK) { JH_ParentTableCode = "TH", JH_JobNum = "JOBHEADER1" }.AppendInsertAndReturnObject(sql);
			var ratingJobHeader2 = new JobHeader("WRK", branch.PK, department.PK, company.PK, jobShipment2.PK) { JH_ParentTableCode = "TH", JH_JobNum = "JOBHEADER2" }.AppendInsertAndReturnObject(sql);
			var nonratingJobHeader1 = new JobHeader("WRK", branch.PK, department.PK, company.PK, jobShipment3.PK) { JH_ParentTableCode = "JS", JH_JobNum = "JOBHEADER3" }.AppendInsertAndReturnObject(sql);
			var nonratingJobHeader2 = new JobHeader("WRK", branch.PK, department.PK, company.PK, jobShipment4.PK) { JH_ParentTableCode = "JS", JH_JobNum = "JOBHEADER4" }.AppendInsertAndReturnObject(sql);

			//Invalid Rating JobCharges
			RatingJobCharges.Add(new JobCharge(ratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = true }.AppendInsertAndReturnObject(sql).PK);
			RatingJobCharges.Add(new JobCharge(ratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			RatingJobCharges.Add(new JobCharge(ratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = true, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);

			//Invalid NonRating JobCharges
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader2.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = true }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = true, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = true, JR_ProFormaRevenue = true }.AppendInsertAndReturnObject(sql).PK);

			//Valid Rating JobCharge
			RatingJobCharges.Add(new JobCharge(ratingJobHeader2.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = true, JR_ProFormaRevenue = true }.AppendInsertAndReturnObject(sql).PK);

			//Valid duplicate NonRating JobCharge 
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader2.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);

			//Duplicate Invalid Rating JobCharge
			RatingJobCharges.Add(new JobCharge(ratingJobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = false, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);

			//Duplicate Invalid NonRating JobCharge
			NonRatingJobCharges.Add(new JobCharge(nonratingJobHeader2.PK, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = true, JR_ProFormaRevenue = false }.AppendInsertAndReturnObject(sql).PK);

			Db.Connection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			foreach (var jobChargeGuid in RatingJobCharges)
			{
				JobCharge.AssertFromDB(Db.Connection, jobChargeGuid).ExpectEquals("JR_ProformaCost", p => p.JR_ProFormaCost, true)
							.ExpectEquals("JR_ProformaRevenue", p => p.JR_ProFormaRevenue, true).VerifyAll();
			}
			foreach (var jobChargeGuid in NonRatingJobCharges)
			{
				JobCharge.AssertFromDB(Db.Connection, jobChargeGuid).ExpectEquals("JR_ProformaCost", p => p.JR_ProFormaCost, false)
							.ExpectEquals("JR_ProformaRevenue", p => p.JR_ProFormaRevenue, false).VerifyAll();
			}
		}

		List<Guid> PrepareTestDataForBatching()
		{
			var sql = new StringBuilder();
			var company = new GlbCompany("CMP", "AU").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BRN", company.PK).AppendInsertAndReturnObject(sql);
			var department = new GlbDepartment("DPT").AppendInsertAndReturnObject(sql);
			var accChargeCode = new AccChargeCode("ORG").AppendInsertAndReturnObject(sql);
			var jobShipment1 = new JobShipment("TESTJOBSHIPMENT1");
			var jobShipment2 = new JobShipment("TESTJOBSHIPMENT2");

			var ratingJobHeader = new JobHeader("WRK", branch.PK, department.PK, company.PK, jobShipment1.PK) { JH_ParentTableCode = "TH", JH_JobNum = "JOBHEADER1" }.AppendInsertAndReturnObject(sql);
			var nonratingJobHeader = new JobHeader("WRK", branch.PK, department.PK, company.PK, jobShipment2.PK) { JH_ParentTableCode = "JS", JH_JobNum = "JOBHEADER2" }.AppendInsertAndReturnObject(sql);
			var generatedGuids = new List<Guid>();
			foreach (var chunk in GuidChunker.GenerateChunks(2, 8, Guid.Empty))
			{
				RatingJobCharges.Add(CreateJobCharge(chunk.LowerBound, ratingJobHeader.PK, false, false));
				NonRatingJobCharges.Add(CreateJobCharge(chunk.UpperBound, nonratingJobHeader.PK, true, true));
				generatedGuids.Add(chunk.LowerBound);
				generatedGuids.Add(chunk.UpperBound);
			}

			Db.Connection.ExecuteNonQuery(sql.ToString());
			return generatedGuids;

			Guid CreateJobCharge(Guid jobChargeGuid, Guid jobHeaderGuid, bool proformaCost, bool proformaRevenue)
			{
				var jobCharge = new JobCharge(jobHeaderGuid, accChargeCode.PK, branch.PK, company.PK, department.PK) { JR_ProFormaCost = proformaCost, JR_ProFormaRevenue = proformaRevenue };
				jobCharge.SetPK(jobChargeGuid);
				jobCharge = jobCharge.AppendInsertAndReturnObject(sql);
				return jobCharge.PK;
			}
		}

		readonly List<Guid> RatingJobCharges = new List<Guid>();
		readonly List<Guid> NonRatingJobCharges = new List<Guid>();
	}
}
