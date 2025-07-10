using System.Configuration;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.FaxRouter.Processor
{
	sealed class BlacklistCheckerTest : TestCase
	{
		BlacklistChecker checker;
		BlacklistResult result;
		MailDBItemDataLine mailDBItem;

		protected override void SetUp()
		{
			base.SetUp();

			ConfigurationSettings.AppSettings["ENTERPRISE_DATABASE_NAME"] = Db.DatabaseName;
			ConfigurationSettings.AppSettings["AUTO_START"] = "1";

			ConfigurationSettings.AppSettings["FAX_GATEWAY_ADMINISTRATOR_EMAIL"] = "test1@edi.com.au;test2@cargowise.com";
			ConfigurationSettings.AppSettings["FAX_ACK_SENDER"] = "test_sender@edi.com.au";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_POLLING_INTERVAL"] = "30000";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_HIEGHT"] = "950";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_WIDTH"] = "750";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_TEMP_FILE_DIRECTORY"] = @"C:\Temp\";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_LOG"] = @"EventLog.txt";

			ConfigurationSettings.AppSettings["ACK_FORMAT"] = "TNZ";
			ConfigurationSettings.AppSettings["BLACKLIST"] = @"djtest@edi.com.au:\+61290251194|0421944394|294|299";
			ConfigurationSettings.AppSettings["DAILY_REPORT_DISCREPANCY_THRESHOLD"] = "0";
			ConfigurationSettings.AppSettings["NOREPLY_EMAIL"] = "noreply@cargowise.com";

			checker = new BlacklistChecker(@"banned@example.com:NONE/somenumbers@EXAMPLE.net:((\+612|02)?902511)|((\+61|0)4219443)");
			mailDBItem = new MailDBItemDataLine();
		}

		void Check()
		{
			result = checker.CheckInternal(mailDBItem);
		}

		public void TestCheckSenderDoesNotMatch()
		{
			mailDBItem.From = "ok@example.com";
			Check();
			Assert(!result.IsBlacklisted);
		}

		public void TestCheckSenderMatchesNumberIsOK()
		{
			mailDBItem.From = "Some Sender <somenumbers@example.net>";

			mailDBItem.FaxRecipientNumber = "+61290251199";
			Check();
			Assert(!result.IsBlacklisted);

			mailDBItem.FaxRecipientNumber = "0290251199";
			Check();
			Assert(!result.IsBlacklisted);

			mailDBItem.FaxRecipientNumber = "90251199";
			Check();
			Assert(!result.IsBlacklisted);

			mailDBItem.FaxRecipientNumber = "+61 (2) 9025-1199";
			Check();
			Assert(!result.IsBlacklisted);

			mailDBItem.FaxRecipientNumber = "0421 944 394";
			Check();
			Assert(!result.IsBlacklisted);
		}

		public void TestCheckSenderMatchesNumberIsBlacklisted()
		{
			mailDBItem.From = "Someone <somenumbers@example.NET>";
			mailDBItem.FaxRecipientNumber = "+61 (2) 9481-1111";
			Check();
			Assert(result.IsBlacklisted);
			AssertEquals("+61294811111", result.Destination);
			AssertEquals("somenumbers@EXAMPLE.net", result.Sender);
		}

		public void TestPatternIsAnchoredToStartOfNumber()
		{
			mailDBItem.From = "Some Sender <somenumbers@example.net>";
			mailDBItem.FaxRecipientNumber = "90251199";
			Check();
			Assert(!result.IsBlacklisted);

			mailDBItem.FaxRecipientNumber = "0390251199";
			Check();
			Assert(result.IsBlacklisted);
		}
	}
}
