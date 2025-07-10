using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol
{
	[TestedType(typeof(TG_CheckUniqueJobMawbForAirline_Update))]
	class TG_CheckUniqueJobMawbForAirline_UpdateTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			try
			{
				TestConnection.ExecuteNonQuery(string.Format(@"				
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{0}', '081', '55555555', '2009-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{1}', '091', '55555555', '2010-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{2}', '081', '88888888', '2011-05-01')

				UPDATE dbo.JobMawb
				SET
					JM_Airline3DigitPrefix = '081',
					JM_SystemLastEditTimeUtc = GETUTCDATE(),
					JM_SystemLastEditUser = '~BP'
				WHERE
					JM_PK = '{1}'

				UPDATE dbo.JobMawb
				SET
					JM_MAWB = '55555555',
					JM_SystemLastEditTimeUtc = GETUTCDATE(),
					JM_SystemLastEditUser = '~BP'
				WHERE
					JM_PK = '{2}'
				",
					pks[0], pks[1], pks[2]));
			}
			catch (SqlException ex)
			{
				Fail("Unexpected SqlException " + ex.Message);
			}
		}

		public void TestSampleCall_JM_MAWB()
		{
			try
			{
				TestConnection.ExecuteNonQuery(string.Format(@"				
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{0}', '081', '66666666', '2009-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{1}', '081', '77777777', '2010-04-30')

				UPDATE dbo.JobMawb SET JM_MAWB='66666666',JM_SystemLastEditTimeUtc=GETUTCDATE(),JM_SystemLastEditUser='~BP' WHERE JM_PK = '{1}'
				",
					pks[0], pks[1]));

				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(50000, ex.Number);
				AssertEquals(@"The Air Waybill Number Can Be Used Once Per Year For Each Airline.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestSampleCall_JM_Airline3DigitPrefix()
		{
			try
			{
				TestConnection.ExecuteNonQuery(string.Format(@"				
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{0}', '081', '66666666', '2009-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES ('{1}', '091', '66666666', '2010-04-30')

				UPDATE dbo.JobMawb SET JM_Airline3DigitPrefix='081',JM_SystemLastEditTimeUtc=GETUTCDATE(),JM_SystemLastEditUser='~BP' WHERE JM_PK = '{1}'
				",
					pks[0], pks[1]));

				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(50000, ex.Number);
				AssertEquals(@"The Air Waybill Number Can Be Used Once Per Year For Each Airline.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		readonly Guid[] pks = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
	}
}

