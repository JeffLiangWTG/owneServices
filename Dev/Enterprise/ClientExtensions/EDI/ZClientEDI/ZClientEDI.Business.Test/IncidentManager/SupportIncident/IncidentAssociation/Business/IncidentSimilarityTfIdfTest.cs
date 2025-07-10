using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class IncidentSimilarityTfIdfTest : TestCaseWithFactory
	{
		public void TestGetRawTermFrequency_GetRawTermFrequencyNull()
		{
			var supportIncident = Factory.New<SupportIncident>();
			supportIncident.FillWithValidTestData();
			Factory.Save();

			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();

			AssertNull(incidentSimilarityTfIdf.RawTermFrequency);
		}

		public void TestRawTermFrequency_GetRawTermFrequencyNonNull()
		{
			var supportIncident = Factory.New<SupportIncident>();
			supportIncident.FillWithValidTestData();
			Factory.Save();

			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			incidentSimilarityTfIdf.RawTermFrequency = new List<double>() { 0.1, 0.2, 0.3 };
			AssertEquals(0.1, actual: incidentSimilarityTfIdf.RawTermFrequency.ToList<double>()[0]);
			AssertEquals(0.2, actual: incidentSimilarityTfIdf.RawTermFrequency.ToList<double>()[1]);
			AssertEquals(0.3, actual: incidentSimilarityTfIdf.RawTermFrequency.ToList<double>()[2]);
		}

		public void TestAugmentedTermFrequency_GetRawTermFrequencyNull()
		{
			var supportIncident = Factory.New<SupportIncident>();
			supportIncident.FillWithValidTestData();
			Factory.Save();

			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			AssertNull(incidentSimilarityTfIdf.AugmentedTermFrequency);
		}

		public void TestAugmentedTermFrequency_GetRawTermFrequencyNonNull()
		{
			var supportIncident = Factory.New<SupportIncident>();
			supportIncident.FillWithValidTestData();
			Factory.Save();

			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			incidentSimilarityTfIdf.RawTermFrequency = new List<double>() { 1, 2, 3 };
			AssertEquals(1d / 3d, actual: incidentSimilarityTfIdf.AugmentedTermFrequency.ToList<double>()[0], 1E-3);
			AssertEquals(2d / 3d, actual: incidentSimilarityTfIdf.AugmentedTermFrequency.ToList<double>()[1], 1E-3);
			AssertEquals(1d, actual: incidentSimilarityTfIdf.AugmentedTermFrequency.ToList<double>()[2], 1E-3);
		}

		public void TestTFIDF_GetTFIDFNull()
		{
			var supportIncident = Factory.New<SupportIncident>();
			supportIncident.FillWithValidTestData();
			Factory.Save();

			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			AssertNull(incidentSimilarityTfIdf.TFIDF);
		}
		public void TestTFIDF_GetTFIDFNonNull()
		{
			var supportIncident = Factory.New<SupportIncident>();
			supportIncident.FillWithValidTestData();
			Factory.Save();

			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityTfIdf>();
			AssertNull(incidentSimilarityTfIdf.TFIDF);
			incidentSimilarityTfIdf.TFIDF = new List<double>() { 1.1, 2.2, 3.3 };
			AssertEquals(1.1, actual: incidentSimilarityTfIdf.TFIDF.ToList<double>()[0]);
			AssertEquals(2.2, actual: incidentSimilarityTfIdf.TFIDF.ToList<double>()[1]);
			AssertEquals(3.3, actual: incidentSimilarityTfIdf.TFIDF.ToList<double>()[2]);
		}
	}
}
