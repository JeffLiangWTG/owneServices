using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Test
{
	class TokenBootstrapperTest : TestCaseWithFactory
	{
		int currentVersion;
		int newVersion;

		protected override void SetUp()
		{
			base.SetUp();
			currentVersion = 2;
			newVersion = 3;

			MakeStmData(currentVersion);
		}

		void MakeStmData(int version, string name = "IncidentSimilarity_LatestVersion")
		{
			var stmData = Factory.New<StmData>();
			stmData.FillWithValidTestData();
			stmData.SD_Name = name;
			stmData.SD_BinaryValue = BitConverter.GetBytes(version);
			Factory.Save();
		}
		Guid MakeIncidentSimilarityToken(int version)
		{
			var incidentSimilarityTfIdf = Factory.New<IncidentSimilarityToken>();
			incidentSimilarityTfIdf.FillWithValidTestData();
			incidentSimilarityTfIdf.IST_Version = version;

			Factory.Save();
			return new Guid(incidentSimilarityTfIdf.PK.ToString());
		}

		public void TestTokenBootstrapper_NewVersionNotGreaterThanOld()
		{
			//Arrange
			//Act 
			//Assert
			AssertExceptionThrown<ArgumentException>("Invalid version should throw ArgumentException.", () => new TokenBootstrapper(-1));
			AssertExceptionThrown<ArgumentException>("Invalid version should throw ArgumentException.", () => new TokenBootstrapper(0));
			AssertExceptionThrown<ArgumentException>("Invalid version should throw ArgumentException.", () => new TokenBootstrapper(1));
			AssertNoExceptionThrown("Valid version should not throw an exception.", () => new TokenBootstrapper(3));
			AssertNoExceptionThrown("Valid version should not throw an exception.", () => new TokenBootstrapper(4));
		}

		public void TestTokenBootstrapper_CallsExpectedDbCommands()
		{
			//Arrange
			MakeIncidentSimilarityToken(currentVersion);
			var tokenBootstrapper = new TokenBootstrapper(newVersion);
			var tokens = new Dictionary<string, int>() { { "b", 2 } };
			//Act 
			tokenBootstrapper.BootstrapTokens(tokens);
			//Assert
			var allIncidentSimilarityToken = Factory.Load<IncidentSimilarityToken>(new ZQuery(IncidentSimilarityTokenSchema.IST_Version, newVersion));
			AssertEquals(allIncidentSimilarityToken.Length, 1);
			AssertEquals(allIncidentSimilarityToken[0].IST_Version, 3);
			AssertEquals(allIncidentSimilarityToken[0].IST_InputToken, "b");
			AssertEquals(allIncidentSimilarityToken[0].IST_OutputToken, 2);
		}
	}
}
