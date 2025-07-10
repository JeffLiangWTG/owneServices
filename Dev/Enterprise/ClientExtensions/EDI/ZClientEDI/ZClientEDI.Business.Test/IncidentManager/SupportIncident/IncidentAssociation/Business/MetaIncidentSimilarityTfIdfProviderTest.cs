using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class MetaIncidentSimilarityTfIdfProviderTest : TestCaseWithFactory
	{
		int currentVersion;
		DateTime now;
		protected override void SetUp()
		{
			base.SetUp();
			now = DateTime.UtcNow;
			currentVersion = 2;
		}

		Guid MakeIncidentMain()
		{
			var incidentMain = Factory.New<IncidentMainBase>();
			incidentMain.FillWithValidTestData();
			Factory.Save();
			return new Guid(incidentMain.PK.ToString());
		}

		Guid MakeIncidentSimilarityTfIdf(Guid incidentGuid)
		{
			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			incidentSimilarityTfIdf.FillWithValidTestData();
			incidentSimilarityTfIdf.ISV_Version = currentVersion;
			incidentSimilarityTfIdf.ISV_IM_Incident = incidentGuid;
			incidentSimilarityTfIdf.ISV_TF = null;
			incidentSimilarityTfIdf.ISV_TFIDF = null;
			incidentSimilarityTfIdf.ISV_Status = "ABC";
			incidentSimilarityTfIdf.ISV_IncidentLastModified = now;
			Factory.Save();
			return new Guid(incidentSimilarityTfIdf.PK.ToString());
		}

		public void TestMetaSupportIncidentProvider_FindByIncidentGuid()
		{
			//Arrange
			var incidentGuid = MakeIncidentMain();
			MakeIncidentSimilarityTfIdf(incidentGuid);

			//Act
			var metaIncidentSimilarityTfidfProvider = new MetaIncidentSimilarityTfIdfProvider();
			var metaObject = metaIncidentSimilarityTfidfProvider.FindByIncidentGuid(currentVersion, incidentGuid);

			//Assert
			AssertEquals(currentVersion, metaObject.Version);
			AssertEquals("ABC", metaObject.Status);
			AssertDateTimeWithinOneSecond("", now, metaObject.IncidentLastModified);
		}

		public void TestMetaSupportIncidentProvider_FindByVectorGuids()
		{
			//Arrange
			var incidentGuidList = new List<Guid>() {
				MakeIncidentMain(),
				MakeIncidentMain(),
				MakeIncidentMain()
			};

			var tfidfGuidList = new List<Guid>() {
				MakeIncidentSimilarityTfIdf(incidentGuidList[0]),
				MakeIncidentSimilarityTfIdf(incidentGuidList[1]),
				MakeIncidentSimilarityTfIdf(incidentGuidList[2])
			};

			//Act
			var metaIncidentSimilarityTfidfProvider = new MetaIncidentSimilarityTfIdfProvider();
			var metaObjects = metaIncidentSimilarityTfidfProvider.FindByVectorGuids(new[] { tfidfGuidList[0], tfidfGuidList[1], tfidfGuidList[2] });
			var firstMetaObject = metaObjects.First();

			//Assert
			AssertEquals(currentVersion, firstMetaObject.Version);
			AssertEquals("ABC", firstMetaObject.Status);
			AssertDateTimeWithinOneSecond("", now, firstMetaObject.IncidentLastModified);
		}

		public void TestMetaSupportIncidentProvider_Load()
		{
			//Arrange
			var incidentGuid = MakeIncidentMain();
			MakeIncidentSimilarityTfIdf(incidentGuid);

			//Act
			var metaIncidentSimilarityTfidfProvider = new MetaIncidentSimilarityTfIdfProvider();
			var metaObjects = metaIncidentSimilarityTfidfProvider.Load(2);
			var firstMetaObject = metaObjects.First();

			//Assert
			AssertEquals(currentVersion, firstMetaObject.Version);
			AssertEquals("ABC", firstMetaObject.Status);
			AssertDateTimeWithinOneSecond("", now, firstMetaObject.IncidentLastModified);
		}

		public void TestMetaSupportIncidentProvider_save()
		{
			//Arrange
			var incidentGuid = Guid.NewGuid();
			var metaIncidentSimilarityTfidf = new MetaIncidentSimilarityTfIdf
			{
				IncidentGuid = incidentGuid,
				Version = currentVersion,
				Status = "ABC",
				IncidentLastModified = now
			};

			//Act
			var metaIncidentSimilarityTfidfProvider = new MetaIncidentSimilarityTfIdfProvider();
			metaIncidentSimilarityTfidfProvider.Save(new List<MetaIncidentSimilarityTfIdf>() { metaIncidentSimilarityTfidf });

			//Assert
			var query = new ZQuery(IncidentSimilarityTfIdfSchema.ISV_Version, currentVersion);

			var newIncidentSimilarityTfIdf = Factory.LoadTop1<IncidentSimilarityTfIdf>(query);
			AssertEquals(incidentGuid, newIncidentSimilarityTfIdf.ISV_IM_Incident);
			AssertEquals(currentVersion, newIncidentSimilarityTfIdf.ISV_Version);
			AssertEquals("ABC", newIncidentSimilarityTfIdf.ISV_Status);
			AssertDateTimeWithinOneSecond("", now, newIncidentSimilarityTfIdf.ISV_IncidentLastModified.ToDateTime());
		}
	}
}
