using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol
{
	[TestedType(typeof(TG_CheckUniqueJobMawbForAirline_Insert))]
	class TG_CheckUniqueJobMawbForAirline_InsertTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			try
			{
				TestConnection.ExecuteNonQuery(@"
				
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES (NEWID(), '081', '55555555', '2009-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES (NEWID(), '081', '55555555', '2010-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES (NEWID(), '091', '55555555', '2009-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES (NEWID(), '081', '55555566', '2009-05-01')
				");
			}
			catch (SqlException)
			{
				Fail("Unexpected SqlException");
			}

			try
			{
				TestConnection.ExecuteNonQuery(@"
				
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES (NEWID(), '081', '66666666', '2009-05-01')

				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
				VALUES (NEWID(), '081', '66666666', '2010-04-30')
				");

				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(50000, ex.Number);
				AssertEquals(@"The Air Waybill Number Can Be Used Once Per Year For Each Airline.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		[ExpectNoExceptions()]
		public void TestWhenExistingOneIsMoreThanOneYearInTheFuture()
		{
			TestConnection.ExecuteNonQuery(@"
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
					VALUES (NEWID(), '081', '77777777', '2010-05-01');
				INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc)
					VALUES (NEWID(), '081', '77777777', '2009-05-01');
			");
		}
	}
}

