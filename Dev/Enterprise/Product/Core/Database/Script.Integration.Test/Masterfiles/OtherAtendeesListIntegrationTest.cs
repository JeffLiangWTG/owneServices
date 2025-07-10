using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	class OtherAtendeesListIntegrationTest : TransactionedTestCase
	{
		public void TestOutputSupportsUnicodeCharacters()
		{
			var factory = new BusinessObjectFactory();

			var contactName = "希瑟";

			var org = factory.NewWithValidTestData<OrgHeader>();
			var orgSalesCall = factory.NewWithValidTestData<OrgSalesCall>();
			var contact = factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = contactName;
			var additionalAttendee = factory.NewWithValidTestData<OrgSalesCallAdditionalAttendee>();
			additionalAttendee.O6_AttendeeID = contact.PK;
			additionalAttendee.O6_AttendeeTableCode = "OC";
			additionalAttendee.O6_OQ = orgSalesCall.PK;

			factory.Save();

			var result = new List<string>();
			using (var command = TestConnection.Command(@"select dbo.OtherAtendeesList(@Key,'Contact')"))
			{
				command.AddParameter("@Key", SqlDbType.UniqueIdentifier, orgSalesCall.PK.ToGuid());
				command.ExecuteNonQuery();

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var value = reader.GetString(0);
						result.Add(value);
					}
				}
			}
			AssertEquals(1, result.Count);
			AssertEquals(contactName, result[0]);
		}
	}
}

