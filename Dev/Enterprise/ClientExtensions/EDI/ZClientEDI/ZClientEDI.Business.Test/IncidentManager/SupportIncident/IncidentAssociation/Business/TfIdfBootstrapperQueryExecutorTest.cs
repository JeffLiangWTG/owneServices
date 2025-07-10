using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class TfIdfBootstrapperQueryExecutorTest : TestCaseWithFactory
	{
		int currentVersion;

		protected override void SetUp()
		{
			base.SetUp();
			currentVersion = 2;
		}

		Guid MakeIncidentMain()
		{
			var incidentMain = Factory.New<IncidentMainBase>();
			incidentMain.FillWithValidTestData();
			Factory.Save();
			return new Guid(incidentMain.PK.ToString());
		}

		Guid MakeIncidentSimilarityTfIdf(Guid incidentMainGuid)
		{
			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			incidentSimilarityTfIdf.FillWithValidTestData();
			incidentSimilarityTfIdf.ISV_Version = currentVersion;
			incidentSimilarityTfIdf.ISV_Status = "ABC";
			incidentSimilarityTfIdf.ISV_IM_Incident = incidentMainGuid;
			Factory.Save();
			return new Guid(incidentSimilarityTfIdf.PK.ToString());
		}

		public void TestUpdateTFIDF()
		{
			//Arrange
			var incidentMainPkList = new List<Guid>() { MakeIncidentMain(), MakeIncidentMain() };
			incidentMainPkList.Sort();

			var tfidfFPkList = new List<Guid>() {
				MakeIncidentSimilarityTfIdf(incidentMainPkList[0]),
				MakeIncidentSimilarityTfIdf(incidentMainPkList[1])
			};

			var tfidfBootstrapperQueryExecutor = new TfIdfBootstrapperQueryExecutor();
			var updatePairs = new List<TfidfPairDataTransferObject>() {
				new TfidfPairDataTransferObject(tfidfFPkList[0], new List<double>() { 1, 1, 1, 1 }),
				new TfidfPairDataTransferObject(tfidfFPkList[1], new List<double>() { 2, 2, 2, 2 }),
			};
			//Act 
			tfidfBootstrapperQueryExecutor.UpdateTfidfPairs(updatePairs, 1000);

			//Assert
			var query = new ZQuery();
			query.OrderBy = AutoIncidentSimilarityTfIdf.Schema.PK;

			Factory.ReloadAll<IncidentSimilarityTfIdf>();
			var newIncidentSimilarityTfIdfs = Factory.Load<IncidentSimilarityTfIdf>(query);
			AssertEquals(2, newIncidentSimilarityTfIdfs.Length);
			AssertEquals(IncidentSimilarityTfIdf.Status.TfIdfComputed, newIncidentSimilarityTfIdfs[0].ISV_Status);
			AssertEquals(IncidentSimilarityTfIdf.Status.TfIdfComputed, newIncidentSimilarityTfIdfs[1].ISV_Status);
		}
	}
}
