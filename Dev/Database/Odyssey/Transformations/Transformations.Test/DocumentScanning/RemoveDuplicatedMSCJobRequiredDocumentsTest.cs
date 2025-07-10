using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(RemoveDuplicatedMSCJobRequiredDocuments))]
	class RemoveDuplicatedMSCJobRequiredDocumentsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicatedMSCJobRequiredDocuments();
		}

		protected override void PrepareTestData()
		{
			CreateJobRequiredDocuments(orgPK1, MSC, Description1, 200);
			CreateJobRequiredDocuments(orgPK1, MSC, Description2, 101);
			CreateJobRequiredDocuments(orgPK1, ACV, Description1, 1);
			CreateJobRequiredDocuments(orgPK1, ACV, Description2, 1, docUsage: "CRR");
			CreateJobRequiredDocuments(orgPK1, ACV, Description2, 1, docCategory: "SCL");

			CreateJobRequiredDocuments(orgPK2, MSC, Description1, 2);
			CreateJobRequiredDocuments(orgPK2, MSC, Description2, 200);

			CreateJobRequiredDocuments(orgPK1, MSC, Description3, 10);
			jobReqPk1 = CreateJobRequiredDocument(orgPK1, MSC, Description3, withDocumentAddInfo: true);

			CreateJobRequiredDocuments(orgPK1, MSC, Description4, 10);
			jobReqPk2 = CreateJobRequiredDocument(orgPK1, MSC, Description4, withDocAttrib: true);
		}

		protected override void AssertTransformationResults()
		{
			var query = @"
SELECT [EQ_ParentID], [EQ_DocType], [EQ_DocDescription], count(*) AS [Count]
FROM dbo.JobRequiredDocument
GROUP BY [EQ_ParentID], [EQ_DocType], [EQ_DocDescription]";
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, query);

			AssertCount(dataTable, 1, orgPK1, MSC, Description1);
			AssertCount(dataTable, 1, orgPK1, MSC, Description2);
			AssertCount(dataTable, 1, orgPK1, ACV, Description1);
			AssertCount(dataTable, 2, orgPK1, ACV, Description2);

			AssertCount(dataTable, 1, orgPK2, MSC, Description1);
			AssertCount(dataTable, 1, orgPK2, MSC, Description2);

			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT [EQ_PK] FROM dbo.JobRequiredDocument");

			AssertExists(dataTable, jobReqPk1);
			AssertExists(dataTable, jobReqPk2);
		}

		void AssertCount(DataTable dataTable, int count, Guid pk, string docType, string docDescription)
		{
			AssertEquals(count, (int)dataTable.Select($"EQ_ParentID='{pk}' AND EQ_DocType = '{docType}' AND EQ_DocDescription = '{docDescription}'")[0]["Count"]);
		}

		void AssertExists(DataTable dataTable, Guid pk)
		{
			AssertEquals(1, dataTable.Select($"EQ_PK='{pk}'").Length);
		}

		void CreateJobRequiredDocuments(Guid parentPk, string docType, string description, int times,
			string docUsage = "BRK", string docPeriod = "SHP", string parentCode = "OH", string docCategory = "CSR",
			bool withDocumentAddInfo = false, bool withDocAttrib = false)
		{
			for (var i = 0; i < times; i++)
			{
				CreateJobRequiredDocument(parentPk, docType, description, docUsage, docPeriod, parentCode, docCategory, withDocumentAddInfo, withDocAttrib);
			}
		}

		Guid CreateJobRequiredDocument(Guid parentPk, string docType, string description,
			string docUsage = "BRK", string docPeriod = "SHP", string parentCode = "OH", string docCategory = "CSR",
			bool withDocumentAddInfo = false, bool withDocAttrib = false)
		{
			var creator = new TransformationTestDataCreator();
			var docPK = creator.CreateJobRequiredDocument(docType, docUsage, docPeriod, description, parentCode, docCategory, parentPk);

			if (withDocumentAddInfo)
			{
				if (companyPK == Guid.Empty)
				{
					companyPK = creator.CreateGlbCompany("AAA", "AU");
				}
				creator.CreateJobRequiredDocumentAddInfo("DIS", "", docPK, companyPK);
			}

			if (withDocAttrib)
			{
				creator.CreateJobRequiredDocAttrib("Test name", "Test value", docPK);
			}
			return docPK;
		}

		public void TestTransformation_NoData_NoExceptionThrown()
		{
			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None));
		}

		[UseSnapshotProtection]
		public void TestTransformation_NoExceptionThrown()
		{
			using (var manager = TestConnection.BeginTransactionWithManager())
			{
				PrepareTestData();
				manager.CommitTransaction();
			}

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None));
		}

		Guid orgPK1 = Guid.NewGuid();
		Guid orgPK2 = Guid.NewGuid();
		Guid companyPK = Guid.Empty;

		Guid jobReqPk1 = Guid.Empty;
		Guid jobReqPk2 = Guid.Empty;

		const string MSC = "MSC";
		const string ACV = "ACV";
		const string Description1 = "Description 1";
		const string Description2 = "Description 2";
		const string Description3 = "Description 3";
		const string Description4 = "Description 4";
	}
}
