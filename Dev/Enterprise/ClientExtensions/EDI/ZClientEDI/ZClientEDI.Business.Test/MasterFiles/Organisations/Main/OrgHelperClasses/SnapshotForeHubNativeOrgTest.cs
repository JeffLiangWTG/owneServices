using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class SnapshotForEHubNativeOrgTest : TestCase
	{
		public void TestAreEqual()
		{
			var currentSnapshot = new SnapshotForEHubNativeOrg
			{
				Address1 = "Address1",
				Address2 = "Address2",
				City = "City",
				Country = "Country",
				EmailAddress = "Email",
				Fax = "Fax",
				ContactName = "ContactName",
				CustomerName = "CustomerName",
				CustomerNumber = "CustomerNumber",
				FEIN = "FEIN",
				Phone = "Phone",
				State = "State",
				Zip = "Zip",
			};

			Assert(!currentSnapshot.AreEqual(null));

			var databaseSnapshot = new SnapshotForEHubNativeOrg
			{
				Address1 = "Address1",
				Address2 = "Address2",
				City = "City",
				Country = "Country",
				EmailAddress = "Email",
				Fax = "Fax",
				ContactName = "ContactName",
				CustomerName = "CustomerName",
				CustomerNumber = "CustomerNumber",
				FEIN = "FEIN",
				Phone = "Phone",
				State = "State",
				Zip = "Zip",
			};

			var count = 0;

			foreach (var property in typeof(SnapshotForEHubNativeOrg).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				Assert(currentSnapshot.AreEqual(databaseSnapshot));

				if (property.Name == nameof(SnapshotForEHubNativeOrg.CompanyDataChanged))
				{
					property.SetValue(databaseSnapshot, true);
					Assert(!currentSnapshot.AreEqual(databaseSnapshot));

					property.SetValue(databaseSnapshot, false);
					Assert(currentSnapshot.AreEqual(databaseSnapshot));
				}
				else
				{
					var value = property.GetValue(databaseSnapshot);
					property.SetValue(databaseSnapshot, null);
					Assert(!currentSnapshot.AreEqual(databaseSnapshot));

					property.SetValue(databaseSnapshot, value + " New");
					Assert(!currentSnapshot.AreEqual(databaseSnapshot));

					property.SetValue(databaseSnapshot, value);
				}

				count++;
			}

			AssertEquals(14, count);
		}
	}
}
