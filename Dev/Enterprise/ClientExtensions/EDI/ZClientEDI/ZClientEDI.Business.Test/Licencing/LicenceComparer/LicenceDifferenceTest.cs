using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public abstract class LicenceDifferenceTest : TestCaseWithFactory
	{
		public abstract void TestGetTextualDifference();
	}

	public class LicenceCheckpointDifferenceTest : LicenceDifferenceTest
	{
		public override void TestGetTextualDifference()
		{
			AssertTextualDifferenceForSameCheckpoints();
			AssertTextualDifferenceForDifferentLicenceTypes();
			AssertTextualDifferenceForDifferentExpiryDates();
			AssertTextualDifferenceForDifferentUserCount();
			AssertTextualDifferenceForAllFieldsDifferent();
			AssertTextualDifferenceForPastExpiryDates();
		}

		void AssertTextualDifferenceForSameCheckpoints()
		{
			var cp1 = GetNewCheckPoint("NAM", "dispname", LicenceTypes.Codes.PUR, ZDateTime.BrettsBirthday, 10);
			var cp2 = GetNewCheckPoint("NAM", "dispname", LicenceTypes.Codes.PUR, ZDateTime.BrettsBirthday, 10);
			AssertEquals("There should be no differences reported", String.Empty, new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());
		}

		void AssertTextualDifferenceForDifferentLicenceTypes()
		{
			var cp1 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.BrettsBirthday, 10);
			var cp2 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.TRI, ZDateTime.BrettsBirthday, 10);
			AssertEquals("Differences should have been reported in the name", "MODULE: ediCore\r\n\tTYPE:\r\n\t\tCLIENT: PUR\r\n\t\tPROD: TRI\r\n", new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());
		}

		void AssertTextualDifferenceForDifferentExpiryDates()
		{
			var cp1 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.Empty, 10);
			var cp2 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, new ZDateTime(2006, 06, 15), 10);
			AssertEquals("Differences should have been reported in the name", "MODULE: ediCore\r\n\tEXPIRY:\r\n\t\tCLIENT: (NONE)\r\n\t\tPROD: 15/06/2006\r\n", new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());

			cp1 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.BrettsBirthday, 10);
			cp2 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.Empty, 10);
			AssertEquals("Differences should have been reported in the name", "MODULE: ediCore\r\n\tEXPIRY:\r\n\t\tCLIENT: 18/09/1971\r\n\t\tPROD: (NONE)\r\n", new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());
		}

		void AssertTextualDifferenceForDifferentUserCount()
		{
			var cp1 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.BrettsBirthday, 10);
			var cp2 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.BrettsBirthday, 5);
			AssertEquals("Differences should have been reported in the name", "MODULE: ediCore\r\n\tUSER COUNT:\r\n\t\tCLIENT: 10\r\n\t\tPROD: 5\r\n", new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());
		}

		void AssertTextualDifferenceForAllFieldsDifferent()
		{
			var cp1 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.PUR, ZDateTime.Empty, 10);
			var cp2 = GetNewCheckPoint("COR", "ediCore", LicenceTypes.Codes.TRI, new ZDateTime(2006, 06, 15), 5);
			AssertEquals("Differences should have been reported in the name", "MODULE: ediCore\r\n\tTYPE:\r\n\t\tCLIENT: PUR\r\n\t\tPROD: TRI\r\n\tEXPIRY:\r\n\t\tCLIENT: (NONE)\r\n\t\tPROD: 15/06/2006\r\n\tUSER COUNT:\r\n\t\tCLIENT: 10\r\n\t\tPROD: 5\r\n", new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());
		}

		void AssertTextualDifferenceForPastExpiryDates()
		{
			var cp1 = GetNewCheckPoint("NAM", "dispname", LicenceTypes.Codes.TRI, ZDateTime.Today.AddDays(-2), 10);
			var cp2 = GetNewCheckPoint("NAM", "dispname", LicenceTypes.Codes.TRI, ZDateTime.Today.AddDays(-3), 10);
			AssertEquals("There should be no differences reported", String.Empty, new LicenceCheckpointDifference(cp1, cp2).GetTextualDifference());
		}

		LegacyLicenceCheckpoint GetNewCheckPoint(string name, string displayName, string licenceType, ZDateTime expiryDate, int userLimit)
		{
			var result = new LegacyLicenceCheckpoint(name, displayName, null, "");
			result.LicenceType = licenceType;
			result.ExpiryDate = expiryDate == ZDateTime.Empty ? DateTime.MinValue : expiryDate.ToDateTime();
			result.UserLimit = userLimit;

			return result;
		}
	}
}