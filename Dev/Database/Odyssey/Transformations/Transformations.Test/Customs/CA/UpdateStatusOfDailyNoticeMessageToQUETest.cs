using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateStatusOfDailyNoticeMessageToQUE))]
	public class UpdateStatusOfDailyNoticeMessageToQUETest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEM_Status(emPK1, "QUE");
			AssertEM_Status(emPK2, "QUE");
			AssertEM_Status(emPK3, "QUE");
			AssertEM_Status(emPK4, "RCV");
			AssertEM_Status(emPK5, "RCV");
			AssertEM_Status(emPK6, "PRS");
			AssertEM_Status(emPK7, "RCV");
			AssertEM_Status(emPK8, "RCV");
			AssertEM_Status(emPK9, "QUE");
		}

		void AssertEM_Status(Guid pk, string status)
		{
			var sql = $"SELECT EM_Status FROM dbo.EDIMessage WHERE EM_PK = '{pk}'";
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
			return new UpdateStatusOfDailyNoticeMessageToQUE(2);
		}

		protected override void PrepareTestData()
		{
			var dataCreator = new TransformationTestDataCreator();
			dataCreator.CreateCompany(companyPK, "CCA", "CA", "CAD");
			dataCreator.CreateBranch(branchPK, "BCA", "", companyPK);
			dataCreator.CreateDepartment(departmentPK, "DCA");

			CreateEDIMessage(emPK1, "CAC", "RCV", "RCV", "DN", "2024-10-21");
			CreateEDIMessage(emPK2, "CAC", "RCV", "RCV", "DN", "2024-10-22");
			CreateEDIMessage(emPK3, "CAC", "RCV", "RCV", "DN", "2024-10-23");
			CreateEDIMessage(emPK4, "ABC", "RCV", "RCV", "DN", "2024-10-20");
			CreateEDIMessage(emPK5, "CAC", "TRX", "RCV", "DN", "2024-10-21");
			CreateEDIMessage(emPK6, "CAC", "RCV", "PRS", "DN", "2024-10-21");
			CreateEDIMessage(emPK7, "CAC", "RCV", "RCV", "AB", "2024-10-21");
			CreateEDIMessage(emPK8, "CAC", "RCV", "RCV", "AB", "2024-10-20");
			CreateEDIMessage(emPK9, "CAC", "RCV", "RCV", "DN", "2024-10-22");
		}

		void CreateEDIMessage(Guid emPK, string applicationCode, string receiveTransmit, string status, string messageType, string createTime)
		{
			var sql = $@"
INSERT INTO dbo.EDIMessage(EM_PK, EM_ApplicationCode, EM_ReceiveTransmit, EM_Status, EM_MessageType, EM_MessageSubType, EM_GB, EM_GE, EM_LinkTable, EM_LinkUniqueId, EM_SystemCreateTimeUTC, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
VALUES ('{emPK}', '{applicationCode}', '{receiveTransmit}', '{status}', '{messageType}', 'XXX', '{branchPK}', '{departmentPK}', 'CusEntryHeader', NEWID(), '{createTime}', '~BP', '2024-11-11', '~BP')";
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
				"A total of 4 daily notice messages were updated to QUE status.",
				"\tCompleted: Update The Status Of Daily Notice Message To QUE." };

			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expectedLog, logger);
		}
	}
}
