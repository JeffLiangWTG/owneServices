using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	class vw_Report_ContactIntegrationTest : TransactionedTestCase
	{
		public void TestEmailDeliveryReportStatus()
		{
			var factory = new BusinessObjectFactory();
			var contact = factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "TestContact@qq.com";
			factory.Save();

			AssertFilterCorrectWithDifferentDeliveryStatus("All", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("NDR", 0);
			AssertFilterCorrectWithDifferentDeliveryStatus("UNV", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("VLD", 0);

			var glbEmailAddress = factory.NewWithValidTestData<GlbEmailAddress>();
			glbEmailAddress.GI_EmailAddress = "TestContact@qq.com";
			glbEmailAddress.GI_DeliveryStatus = "NDR";
			factory.Save();

			AssertFilterCorrectWithDifferentDeliveryStatus("All", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("NDR", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("UNV", 0);
			AssertFilterCorrectWithDifferentDeliveryStatus("VLD", 0);

			glbEmailAddress.GI_DeliveryStatus = "UNV";
			factory.Save();

			AssertFilterCorrectWithDifferentDeliveryStatus("All", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("NDR", 0);
			AssertFilterCorrectWithDifferentDeliveryStatus("UNV", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("VLD", 0);

			glbEmailAddress.GI_DeliveryStatus = "VLD";
			factory.Save();

			AssertFilterCorrectWithDifferentDeliveryStatus("All", 1);
			AssertFilterCorrectWithDifferentDeliveryStatus("NDR", 0);
			AssertFilterCorrectWithDifferentDeliveryStatus("UNV", 0);
			AssertFilterCorrectWithDifferentDeliveryStatus("VLD", 1);

			void AssertFilterCorrectWithDifferentDeliveryStatus(string deliveryStatusFilter, int expectedCount)
			{
				using (var command = TestConnection.Command("SELECT count(1) FROM dbo.vw_Report_Contact where Email = 'TestContact@qq.com' AND (@DeliveryStatus = 'All' OR @DeliveryStatus = DeliveryStatus)"))
				{
					command.AddParameter("@DeliveryStatus", SqlDbType.VarChar, deliveryStatusFilter);

					AssertEquals(expectedCount, command.ExecuteScalar());
				}
			}
		}
	}
}

