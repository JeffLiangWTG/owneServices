using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(UpdateDeclarationConsolidatedEntryNumber))]
	class UpdateDeclarationConsolidatedEntryNumberTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertConsolidatedJobNumber("declarationN10 Invalid country", declarationN10, "SV9");
			AssertConsolidatedJobNumber("declarationN20 Nothing change", declarationN20, "SV92");
			AssertConsolidatedJobNumber("declarationN30 Entry change", declarationN30, "SV93");
			AssertConsolidatedJobNumber("declarationN40 Invalid entry type", declarationN40, "SV9");
			AssertConsolidatedJobNumber("declarationN50 No entry type", declarationN50, "SV9");
			AssertConsolidatedJobNumber("declarationN60 Filer and entry both change", declarationN60, "XJ56");
			AssertConsolidatedJobNumber("declarationN70 Invalid message type", declarationN70, "SV9");
			AssertConsolidatedJobNumber("declarationN80 Consol declaration and normal declaration in different branch", declarationN80, "SV9");
			AssertConsolidatedJobNumber("declarationN90 Filer change", declarationN90, "XJ59");
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateDeclarationConsolidatedEntryNumber(1);

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var companyUS = testDataCreator.CreateGlbCompany("AAA", "US");
			var companyCA = testDataCreator.CreateGlbCompany("BBB", "CA");
			var branchUS = testDataCreator.CreateGlbBranch("CCC", companyUS);
			var branchUS2 = testDataCreator.CreateGlbBranch("EEE", companyUS);
			var branchCA = testDataCreator.CreateGlbBranch("DDD", companyCA);
			testDataCreator.CreateDeclaration(declarationN10, "10", 10, branchCA, companyCA, messageType: "IMP", dataModel: "CA");
			testDataCreator.CreateDeclaration(declarationN20, "20", 20, branchUS, companyUS, messageType: "IMP", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN30, "30", 30, branchUS, companyUS, messageType: "IMP", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN40, "40", 40, branchUS, companyUS, messageType: "IMP", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN50, "50", 50, branchUS, companyUS, messageType: "IMP", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN60, "60", 60, branchUS, companyUS, messageType: "IMP", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN70, "70", 70, branchUS, companyUS, messageType: "FTZ", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN80, "80", 80, branchUS2, companyUS, messageType: "IMP", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationN90, "90", 90, branchUS, companyUS, messageType: "IMP", dataModel: "US");

			testDataCreator.CreateDeclaration(declarationC1, "1", 1, branchCA, companyCA, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "CA");
			testDataCreator.CreateDeclaration(declarationC2, "2", 2, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC3, "3", 3, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC4, "4", 4, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC5, "5", 5, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC6, "6", 6, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=XJ5", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC7, "7", 7, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC8, "8", 8, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=SV9", dataModel: "US");
			testDataCreator.CreateDeclaration(declarationC9, "9", 9, branchUS, companyUS, addInfo: "ConsolACE=Y*EntryFilerCode=XJ5", dataModel: "US");

			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC1, "JobDeclaration", "1", "ENS", countryCode: "CA");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC2, "JobDeclaration", "2", "ENS", countryCode: "US");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC3, "JobDeclaration", "3", "ENS", countryCode: "US");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC4, "JobDeclaration", "4", "FTZ", countryCode: "US");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC6, "JobDeclaration", "6", "ENS", countryCode: "US");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC7, "JobDeclaration", "7", "ENS", countryCode: "US");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC8, "JobDeclaration", "8", "ENS", countryCode: "US");
			testDataCreator.CreateCusEntryNum(Guid.NewGuid(), declarationC9, "JobDeclaration", "9", "ENS", countryCode: "US");

			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN10);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV92", "JE", declarationN20);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN30);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN40);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN50);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN60);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN70);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV9", "JE", declarationN80);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedEntryNumber", "SV99", "JE", declarationN90);

			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "1", "JE", declarationN10);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "2", "JE", declarationN20);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "3", "JE", declarationN30);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "4", "JE", declarationN40);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "5", "JE", declarationN50);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "6", "JE", declarationN60);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "7", "JE", declarationN70);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "8", "JE", declarationN80);
			testDataCreator.CreateGenAddOnColumn("US_ConsolidatedJobNumber", "9", "JE", declarationN90);
		}

		void AssertConsolidatedJobNumber(string message, Guid decPK, object expectedNumber)
		{
			object actualNumber = null;
			var sql = $"SELECT TOP 1 XA_Data FROM dbo.GenAddOnColumn WHERE XA_Name = 'US_ConsolidatedEntryNumber' AND XA_ParentTableCode = 'JE' AND XA_ParentID = @decPK";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@decPK", System.Data.SqlDbType.UniqueIdentifier, decPK);
				actualNumber = cmd.ExecuteScalar();
			}
			AssertEquals(message, expectedNumber, actualNumber);
		}

		public void TestLogging()
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();

			transformation.Run(s => logger.Add(s), CancellationToken.None);

			AssertContainsExactElementsInExactOrder(
				new[]
				{
					"Processed release declaration count [1].",
					"Processed release declaration count [1].",
					"Processed release declaration count [1].",
					"Processed release declaration count [0].",
					"\tCompleted: Update Declaration Consolidated Entry Number",
				},
				logger);
		}

		Guid declarationN10 = Guid.NewGuid();
		Guid declarationN20 = Guid.NewGuid();
		Guid declarationN30 = Guid.NewGuid();
		Guid declarationN40 = Guid.NewGuid();
		Guid declarationN50 = Guid.NewGuid();
		Guid declarationN60 = Guid.NewGuid();
		Guid declarationN70 = Guid.NewGuid();
		Guid declarationN80 = Guid.NewGuid();
		Guid declarationN90 = Guid.NewGuid();

		Guid declarationC1 = Guid.NewGuid();
		Guid declarationC2 = Guid.NewGuid();
		Guid declarationC3 = Guid.NewGuid();
		Guid declarationC4 = Guid.NewGuid();
		Guid declarationC5 = Guid.NewGuid();
		Guid declarationC6 = Guid.NewGuid();
		Guid declarationC7 = Guid.NewGuid();
		Guid declarationC8 = Guid.NewGuid();
		Guid declarationC9 = Guid.NewGuid();
	}
}
