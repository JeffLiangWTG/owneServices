using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAgedScoreQuery))]
	class MENTAgedScoreQueryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsAbleToSaveInactiveQuery()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = "CRINGLBRY";
			query.MAQ_IsActive = false;

			Factory.Save();

			AssertEquals(true, query.IsInDatabase);
		}

		public void TestCodeAndDescriptionProperty()
		{
			AssertEquals("CodeProperty", AutoMENTAgedScoreQuery.Schema.MAQ_Code, CodePropertyAttribute.CodePropertyNameFromType(typeof(MENTAgedScoreQuery)));
			AssertEquals("DescriptionProperty", AutoMENTAgedScoreQuery.Schema.MAQ_AttributeDescription, DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(MENTAgedScoreQuery)));
		}

		public void TestIsSystemReadonly()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "HOnk");
			query.MAQ_IsSystem = false;

			AssertEquals(true, query.MAQ_IsSystemInfo.ReadOnly);

			query.MAQ_IsSystem = true;

			AssertEquals(true, query.MAQ_IsSystemInfo.ReadOnly);
		}

		public void TestReadonlyOfFieldsWhenIsSystem()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "HOnk");
			query.MAQ_IsSystem = true;

			AssertEquals(true, query.MAQ_AttributeDescriptionInfo.ReadOnly);
			AssertEquals(false, query.MAQ_IsActiveInfo.ReadOnly);
			AssertEquals(true, query.MAQ_SqlTextInfo.ReadOnly);
			AssertEquals(true, query.MAQ_IsFaultyInfo.ReadOnly);
			AssertEquals(true, query.MAQ_AutoArchiveInfo.ReadOnly);
			AssertEquals(true, query.MAQ_AutoPurgeInfo.ReadOnly);

			query.MAQ_IsSystem = false;

			AssertEquals(false, query.MAQ_AttributeDescriptionInfo.ReadOnly);
			AssertEquals(false, query.MAQ_IsActiveInfo.ReadOnly);
			AssertEquals(false, query.MAQ_SqlTextInfo.ReadOnly);
			AssertEquals(true, query.MAQ_IsFaultyInfo.ReadOnly);
			AssertEquals(false, query.MAQ_AutoArchiveInfo.ReadOnly);
			AssertEquals(false, query.MAQ_AutoPurgeInfo.ReadOnly);
		}

		public void TestMAQ_CodeReadonly_WhenInDatabase()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = "CRINGLBRY";

			AssertEquals(false, query.MAQ_CodeInfo.ReadOnly);

			Factory.Save();

			AssertEquals(true, query.MAQ_CodeInfo.ReadOnly);
		}

		public void TestIsFaultyReadOnly()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			AssertEquals(true, query.MAQ_IsFaultyInfo.ReadOnly);
		}

		public void TestExpectScheduleForQuery()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = "IAMQUERY";

			AssertNotNull(query.QuerySchedule);
			AssertEquals(query.PK, query.QuerySchedule.S5_ParentID);
			AssertEquals(MENTAgedScoreQuerySchema.Constants.Prefix, query.QuerySchedule.S5_ParentTableCode);

			query.QuerySchedule.S5_ScheduleDescription = "And if your friends can't query they aint no friends of mine";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedQuery = anotherFactory.Load<MENTAgedScoreQuery>(query.PK);
			AssertNotNull(loadedQuery.QuerySchedule);
			AssertEquals(query.PK, query.QuerySchedule.S5_ParentID);
			AssertEquals(MENTAgedScoreQuerySchema.Constants.Prefix, query.QuerySchedule.S5_ParentTableCode);
			AssertEquals("And if your friends can't query they aint no friends of mine", loadedQuery.QuerySchedule.S5_ScheduleDescription);
		}

		public void TestRelatedExtractions()
		{
			var query1 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query1.MAQ_Code = "PINACOLADA";

			var query2 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query2.MAQ_Code = "PENCIL";

			Factory.Save();

			var extraction1 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction1.MEX_MAQ = query1.PK;
			var extraction2 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction2.MEX_MAQ = query1.PK;
			var extraction3 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction3.MEX_MAQ = query2.PK;

			AssertEquals(2, query1.Extractions.Count);
			AssertCollectionContains(extraction1, query1.Extractions);
			AssertCollectionContains(extraction2, query1.Extractions);
			AssertEquals(1, query2.Extractions.Count);
			AssertCollectionContains(extraction3, query2.Extractions);
		}

		public void TestDelete()
		{
			var query1 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query1.MAQ_Code = "PANDADA";

			var query2 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query2.MAQ_Code = "GROUPIE";

			Factory.Save();

			var extraction1 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction1.MEX_MAQ = query1.PK;
			var extraction2 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction2.MEX_MAQ = query1.PK;
			var extraction3 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction3.MEX_MAQ = query2.PK;

			Factory.Save();

			query1.Delete();

			AssertEquals(true, query1.IsDeleted);
			AssertEquals(true, query1.QuerySchedule.IsDeleted);
			AssertEquals(true, extraction1.IsDeleted);
			AssertEquals(true, extraction2.IsDeleted);
		}

		public void TestGetQueryable()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			var query = MENTTestHelper.CreateQuery(Factory, "Waterbttl");

			AssertEquals(query, query.GetQueryable());

			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			AssertEquals(band, query.GetQueryable());
		}

		public void TestReadonlySQLQueryWhenLinked()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var query = MENTTestHelper.CreateQuery(Factory, "ORG");

			Assert("Query should not be linked", !query.Linked);
			Assert("SQL Text field should not be readonly", !query.MAQ_SqlTextInfo.ReadOnly);

			var sql = @"select 4 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			var t = MENTTestHelper.CreateRelatedBand(query, AcceptabilityBandTypes.Codes.Count, buffer.PK, sql);

			Assert("Query should be linked", query.Linked);
			Assert("SQL Text field should be readonly, since the query is linked", query.MAQ_SqlTextInfo.ReadOnly);
		}

		public void TestDefaultValues()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "Query");
			AssertEquals(365, query.MAQ_PurgeDays);
		}
	}
}
