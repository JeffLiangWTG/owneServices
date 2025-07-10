using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Customs.CA;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(PopulateSubTypeCLOForMessagesWithStatusCode39))]
	public class PopulateSubTypeCLOForMessagesWithStatusCode39Test : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEM_MessageSubType(emPK1, "CLO");
			AssertEM_MessageSubType(emPK2, "CLO");
			AssertEM_MessageSubType(emPK3, "CLO");
			AssertEM_MessageSubType(emPK4, "   ");
			AssertEM_MessageSubType(emPK5, "   ");
			AssertEM_MessageSubType(emPK6, "   ");
			AssertEM_MessageSubType(emPK7, "   ");
			AssertEM_MessageSubType(emPK8, "ORG");
			AssertEM_MessageSubType(emPK9, "XXX");
		}

		void AssertEM_MessageSubType(Guid pk, string status)
		{
			var sql = $"SELECT EM_MessageSubType FROM dbo.EDIMessage WHERE EM_PK = '{pk}'";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				var count = 0;
				if (reader.Read())
				{
					count++;
					AssertEquals(status, (string)reader[0]);
				}
				AssertEquals(1, count);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateSubTypeCLOForMessagesWithStatusCode39();
		}

		protected override void PrepareTestData()
		{
			var dataCreator = new TransformationTestDataCreator();
			dataCreator.CreateCompany(companyPK, "CCA", "CA", "CAD");
			dataCreator.CreateBranch(branchPK, "BCA", "", companyPK);
			dataCreator.CreateDepartment(departmentPK, "DCA");

			CreateEDIMessage(emPK1, "CAI", "CAD", "", "RCV", "RCV", "39", "2025-01-16");
			CreateEDIMessage(emPK2, "CAI", "CAD", "", "RCV", "RCV", "39", "2024-10-17");
			CreateEDIMessage(emPK3, "CAI", "CAD", "", "RCV", "RCV", "39", "2024-11-08");
			CreateEDIMessage(emPK4, "CAC", "CAD", "", "RCV", "RCV", "39", "2024-12-06");
			CreateEDIMessage(emPK5, "CAI", "DN", "", "RCV", "RCV", "39", "2024-12-04");
			CreateEDIMessage(emPK6, "CAI", "CAD", "", "RCV", "RCV", "40", "2024-11-30");
			CreateEDIMessage(emPK7, "CAI", "CAD", "", "RCV", "QUE", "39", "2024-10-20");
			CreateEDIMessage(emPK8, "CAI", "CAD", "ORG", "TRX", "SNT", "", "2024-10-18");
			CreateEDIMessage(emPK9, "CAI", "CAD", "XXX", "RCV", "FAL", "505", "2024-10-08");
		}

		void CreateEDIMessage(Guid emPK, string applicationCode, string messageType, string messageSubType, string receiveTransmit, string status, string nameCode, string createTime)
		{
			var messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
           <ID>207461995RM0001</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response>
        <Status>
            <NameCode>{0}</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var sql = $@"
INSERT INTO dbo.EDIMessage(EM_PK, EM_ApplicationCode, EM_MessageType, EM_MessageSubType, EM_ReceiveTransmit, EM_Status, EM_MessageText, EM_GB, EM_GE, EM_LinkTable, EM_LinkUniqueId, EM_SystemCreateTimeUTC, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
VALUES ('{emPK}', '{applicationCode}', '{messageType}', '{messageSubType}', '{receiveTransmit}', '{status}', '{string.Format(messageText, nameCode)}', '{branchPK}', '{departmentPK}', 'CusEntryHeader', NEWID(), '{createTime}', '~BP', '2024-12-12', '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid companyPK = Guid.NewGuid();
		Guid branchPK = Guid.NewGuid();
		Guid departmentPK = Guid.NewGuid();
		Guid emPK1 = Guid.NewGuid();
		Guid emPK2 = Guid.NewGuid();
		Guid emPK3 = Guid.NewGuid();
		Guid emPK4 = Guid.NewGuid();
		Guid emPK5 = Guid.NewGuid();
		Guid emPK6 = Guid.NewGuid();
		Guid emPK7 = Guid.NewGuid();
		Guid emPK8 = Guid.NewGuid();
		Guid emPK9 = Guid.NewGuid();

		public void TestLogging()
		{
			var expectedLog = new[] {
				"A total of 3 messages with status code 39 had their subType updated to CLO.",
				"\tCompleted: Populate Subtype CLO for messages with status code 39" };

			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expectedLog, logger);
		}

		public void TestRunCancellationAndExtProperty()
		{
			PrepareTestData();
			var logger = new List<string>();
			var cancellationToken = new CancellationToken(true);
			var transformation = GetNewTestTransformationInstance();

			AssertNull($"ExtProperty '{StartDateFlag}' should be empty", ExtProperty.Database.Select(Db.Connection, StartDateFlag));
			ExtProperty.Database.Update(Db.Connection, StartDateFlag, "2024-10-31 00:00:00.000");
			AssertExceptionThrown<OperationCanceledException>(() => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertContainsExactElementsInExactOrder(new[] { "Processed up to 2024-11-30, total updated: 1." }, logger);
			AssertEquals($"ExtProperty '{StartDateFlag}' startDateFlag should be 2024-11-30", "2024-11-30 00:00:00.000", ExtProperty.Database.Select(Db.Connection, StartDateFlag));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertNull($"ExtProperty '{StartDateFlag}' should be cleared", ExtProperty.Database.Select(Db.Connection, StartDateFlag));
		}

		const string StartDateFlag = "PopulateSubTypeCLOForMessagesWithStatusCode39_StartDate";
	}
}
