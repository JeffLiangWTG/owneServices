using System;
using System.Data;
using System.Threading;
using CargoWise.Database.Shared;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.MasterFiles.Testing
{
	[TestedType(typeof(ConvertJobServiceBookedDateTimeToDateTimeOffset))]
	class ConvertJobServiceBookedDateTimeToDateTimeOffsetTest : ConvertDateTimeToDateTimeOffsetTransformTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "Address1") { OA_RL_NKRelatedPortCode = "AUSYD" }.AppendInsertAndReturnObject(sql);
			var docketJobParent = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			new JobServiceOld_V01("WD", docketJobParent.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0), ES_OA_Location = address.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestUserDescription()
		{
			AssertEquals("Convert JobService ES_Booked DateTime to DateTimeOffset and store the new value as ES_BookedDateTimeOffset.", GetNewTestTransformationInstance().UserDescription);
		}

		public void TestNoBookedDate()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "Address1") { OA_RL_NKRelatedPortCode = "AUSYD" }.AppendInsertAndReturnObject(sql);
			var docketJobParent = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WD", docketJobParent.PK, "FUM") { ES_OA_Location = address.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			JobService.AssertFromDB(TestConnection, jobService.PK)
				.ExpectEquals("ES_BookedDateTimeOffset should be equal.", d => d.ES_BookedDateTimeOffset.HasValue, false)
				.VerifyAll();
		}

		public void TestNoServiceLocation_DocketParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var docketJobParent = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WD", docketJobParent.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_VASOrderParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var vasOrder = new WhsVASOrder(client, area, "WVO1").AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WVO", vasOrder.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_AdHocServiceJobParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var adHocServiceJob = new WhsAdHocServiceJob(whs.PK, client.PK, "JOB1", "JOB1", DateTime.Now).AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WSJ", adHocServiceJob.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_DtbConsignmentParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var dtbConsignment = new DtbConsignment("ABC1", "LTL") { LTC_GB_Branch = branch.PK }.AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("LTC", dtbConsignment.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_DtbConsignmentRunSheetParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var dtbConsignmentRunSheet = new DtbConsignmentRunSheet("S1", branch.PK).AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("KG", dtbConsignmentRunSheet.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_DtbBookingParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var bookingConsolidation = new DtbBookingConsolidation().AppendInsertAndReturnObject(sql);
			var booking = new DtbBooking(bookingConsolidation.PK) { KM_JobID = "JOB1", KM_GB_Branch = branch.PK }.AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("KM", booking.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_WhsItemDispatchConsignmentParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var whsItemDispatchConsignment = new WhsItemDispatchConsignment(whs, "CONS1", "JOB1", "STD").AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WDC", whsItemDispatchConsignment.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_WhsItemReceiveConsignmentParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var whsItemReceiveConsignment = new WhsItemReceiveConsignment(whs, "CONS1", "JOB1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WRC", whsItemReceiveConsignment.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_JobBookedCtgMoveParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var cartage = new JobCartage { JJ_ConsignmentID = "CONS1", JJ_GB = branch.PK }.AppendInsertAndReturnObject(sql);
			var jobBookedCtgMove = new JobBookedCtgMove { EW_JJ = cartage.PK }.AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("EW", jobBookedCtgMove.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_CusInBondHeaderParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var cusInBondHeader = new CusInBondHeader(branch).AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("BH", cusInBondHeader.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_JobContainerParent()
		{
			var sql = new SqlQueryBuilder();
			var jobContainer = new JobContainer().AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("JC", jobContainer.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(0)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_CusContainerParent()
		{
			var sql = new SqlQueryBuilder();
			var company = new GlbCompany("ABC", "AU").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BR1", company.PK) { GB_RL_NKHomePort = "AUSYD" }.AppendInsertAndReturnObject(sql);
			var jobDeclaration = new JobDeclaration(100, company, branch).AppendInsertAndReturnObject(sql);
			var container = new CusContainer { CO_ClusterKey = 100, CO_JE = jobDeclaration.PK, CO_DataModel = "AU" }.AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("CO", container.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_WorkItemParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var workItem = new WorkItem { WKI_WorkItemNumber = "W0001", WKI_GB_AssignedBranch = branch.PK }.AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("WKI", workItem.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		public void TestNoServiceLocation_JobDocsAndCartageParent()
		{
			var sql = new SqlQueryBuilder();
			var jobShipment = new JobShipment("IAMUNIQUE").AppendInsertAndReturnObject(sql);
			var jobDocsAndCartage = new JobDocsAndCartage { JP_ParentID = jobShipment.PK, JP_ParentTableCode = "JS" }.AppendInsertAndReturnObject(sql);
			var jobService = new JobServiceOld_V01("JP", jobDocsAndCartage.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(0)), TestConnection.ExecuteScalar("SELECT ES_BookedDateTimeOffset FROM JobService"));
		}

		#region Implementation

		protected override bool TableHasAutoVersion => true;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.SmallDateTime;

		protected override SchemaDateTimeOffsetColumn GetDateTimeOffsetColumnToConvert() => JobServiceSchema.ES_BookedDateTimeOffset;

		protected override SchemaDateTimeColumn GetSourceDateTimeColumn() => JobServiceSchema.ES_Booked;

		protected override DataTransformation GetNewTestTransformationInstance() => new ConvertJobServiceBookedDateTimeToDateTimeOffset();

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider provider)
		{
			return provider.New(JobServiceSchema.Instance)
				.Key(JobServiceSchema.Constants.ES_Booked)
				.Include(JobServiceSchema.Constants.ES_OA_Location, JobServiceSchema.Constants.ES_ParentID, JobServiceSchema.Constants.ES_ParentTableCode, "ES_AutoVersion")
				.Where("[ES_Booked] IS NOT NULL")
				.GetInfo();
		}

		protected override string GetTimeZoneSubQuery(string timeZoneColumnName) => $@"
SELECT 
	CASE
		WHEN ES_OA_Location IS NOT NULL
		THEN
		(
			SELECT
				OA_RL_NKRelatedPortCode
			FROM
				dbo.OrgAddress
			WHERE
				OA_PK = ES_OA_Location
		)
		WHEN ES_ParentTableCode = 'WD'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsDocket
				JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WD_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WVO'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsVASOrder
				JOIN dbo.WhsArea ON WA_PK = WVO_WA_ServiceArea
				JOIN dbo.WhsWarehouse ON WA_WW_Whs = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WVO_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WSJ'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsAdHocServiceJob
				JOIN dbo.WhsWarehouse ON WSJ_WW_Whs = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WSJ_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'CO'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.CusContainer
				JOIN dbo.JobDeclaration ON CO_ClusterKey = JE_ClusterKey
				JOIN dbo.GlbBranch ON JE_GB = GB_PK
			WHERE
				CO_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'BH'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.CusInBondHeader
				JOIN dbo.GlbBranch ON BH_GB = GB_PK
			WHERE
				BH_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'KM'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.DtbBooking
				JOIN dbo.GlbBranch ON KM_GB_Branch = GB_PK
			WHERE
				KM_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'LTC'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.DtbConsignment
				JOIN dbo.GlbBranch ON LTC_GB_Branch = GB_PK
			WHERE
				LTC_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'KG'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.DtbConsignmentRunSheet
				JOIN dbo.GlbBranch ON KG_GB_Branch = GB_PK
			WHERE
				KG_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'EW'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.JobBookedCtgMove
				JOIN dbo.JobCartage ON EW_JJ = JJ_PK
				JOIN dbo.GlbBranch ON JJ_GB = GB_PK
			WHERE
				EW_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WDC'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsItemDispatchConsignment
				JOIN dbo.WhsWarehouse ON WDC_WW_Warehouse = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WDC_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WRC'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsItemReceiveConsignment
				JOIN dbo.WhsWarehouse ON WRC_WW_IntendedWarehouse = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WRC_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WKI'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WorkItem
				JOIN dbo.GlbBranch ON WKI_GB_AssignedBranch = GB_PK
			WHERE
				WKI_PK = ES_ParentID
		)
	END AS {timeZoneColumnName}";

		protected override void InsertDataWithThisTimeZone(string timeZoneUnloco)
		{
			var sql = new SqlQueryBuilder();

			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "SGSIN" }.InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "Address1") { OA_RL_NKRelatedPortCode = timeZoneUnloco }.AppendInsertAndReturnObject(sql);
			var docketJobParent = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			new JobServiceOld_V01("WD", docketJobParent.PK, "FUM") { ES_Booked = new DateTime(2024, 6, 6, 0, 0, 0), ES_OA_Location = address.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void RevertSchemaToOriginalStatePriorToUpgrade()
		{
			new DbColumnDependencyRemover(JobServiceSchema.Constants.TableName, JobServiceSchema.Constants.ES_CompletedDateTimeOffset).DropRelateObjects(TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(JobServiceSchema.Constants.TableName, JobServiceSchema.Constants.ES_CompletedDateTimeOffset);
			TestConnection.ExecuteNonQuery("ALTER TABLE JobService ADD ES_Completed datetime NULL");
			new DbColumnDependencyRemover(JobServiceSchema.Constants.TableName, JobServiceSchema.Constants.ES_BookedDateTimeOffset).DropRelateObjects(TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(JobServiceSchema.Constants.TableName, JobServiceSchema.Constants.ES_BookedDateTimeOffset);
			TestConnection.ExecuteNonQuery("ALTER TABLE JobService ADD ES_Booked datetime NULL");
		}

		#endregion
	}
}
