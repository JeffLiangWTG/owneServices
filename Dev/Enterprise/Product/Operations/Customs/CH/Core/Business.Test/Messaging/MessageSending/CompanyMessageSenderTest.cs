using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CompanyMessageSender))]
sealed class CompanyMessageSenderTest : TestCaseWithFactory
{
	public void TestSendRequests() => CombineAssertions(() =>
	{
		var logger = new LoggingInformationForTesting();
		CompanyMessageSender.SendRequests(logger, new CancellationToken());
		AssertEquals(0, logger.Logs.Count());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
		logger = new LoggingInformationForTesting();
		CompanyMessageSender.SendRequests(logger, new CancellationToken());

		var companyCode = GlbCompany.CurrentCompany.GC_Code;
		Assert("Passar Message List Request", logger.ContainsLogEntry($"Information: 1 Passar Message List Request(s) for company {companyCode} has been processed."));
		Assert("Passar Get Message Request", logger.ContainsLogEntry($"Information: 0 Passar Get Message Request(s) for company {companyCode} has been processed."));
		Assert("Chartera Output Message List Request", logger.ContainsLogEntry($"Information: 1 Chartera Output Message List Request(s) for company {companyCode} has been processed."));
		Assert("Chartera Output Get Message Request", logger.ContainsLogEntry($"Information: 0 Chartera Output Get Message Request(s) for company {companyCode} has been processed."));
		Assert("Chartera Output Document Delivery Request", logger.ContainsLogEntry($"Information: 0 Chartera Output Document Delivery Request(s) for company {companyCode} has been processed."));
		Assert("Chartera Output Document Search Request", logger.ContainsLogEntry($"Information: 1 Chartera Output Document Search Request(s) for company {companyCode} has been processed."));
		Assert(logger.AccumulatedLogMessages.ToString() + "\r\nChartera Output Document Search Retry Request", logger.ContainsLogEntry($"Information: 0 Chartera Output Document Search Retry Request(s) for company {companyCode} has been processed."));
	});

	public void TestSearchRequestsEnabled() => CombineAssertions(() =>
	{
		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();

		const string searchRequestLogMessage = $"Chartera Output Document Search Request(s)";
		const string searchRetryLogMessage = $"Chartera Output Document Search Retry Request(s)";

		AssertTask(true, true);
		AssertTask(false, false);

		void AssertTask(bool runExpected, bool taskEnabled)
		{
			var config = new PassarSearchRequestConfig();
			config.IsEnabled = taskEnabled;
			using (CHCustomsDataRegistry.Instance.PassarSearchRequestConfig.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config))
			{
				var logger = new LoggingInformationForTesting();
				TestDateAttribute.AddHours(2);
				CompanyMessageSender.SendRequests(logger, new CancellationToken());
				var logMessages = logger.AccumulatedLogMessages.ToString();
				AssertEquals($"Enabled={taskEnabled} - {searchRequestLogMessage}", runExpected, logMessages.Contains(searchRequestLogMessage));
				AssertEquals($"Enabled={taskEnabled} - {searchRetryLogMessage}", runExpected, logMessages.Contains(searchRetryLogMessage));
			}
		}
	});

	public void TestSendTokensRefresh()
	{
		var logger = new LoggingInformationForTesting();
		CompanyMessageSender.SendTokensRefresh(logger, new CancellationToken());
		AssertEquals(0, logger.Logs.Count());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
		logger = new LoggingInformationForTesting();
		CompanyMessageSender.SendTokensRefresh(logger, new CancellationToken());
		Assert(logger.ContainsLogEntry($"Information: 1 Token Refresh message(s) for company {GlbCompany.CurrentCompany.GC_Code} has been processed."));
	}

	public void TestSendBordereauRequest()
	{
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		AssertSendBordereauRequest(true, true);
		AssertSendBordereauRequest(false, false);

		void AssertSendBordereauRequest(bool logEntryExpected, bool isEnabled)
		{
			using (CHCustomsDataRegistry.Instance.EdecBordereauConfig.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EdecBordereauConfig() { IsEnabled = isEnabled }))
			{
				var logger = new LoggingInformationForTesting();
				CompanyMessageSender.SendBordereauRequest(logger, new CancellationToken());
				AssertEquals($"Enabled={isEnabled} \nActual log:\n{string.Join("\n", logger.Logs)}\n", logEntryExpected, logger.ContainsLogEntry($"1 Bordereau List message(s) for company {GlbCompany.CurrentCompany.GC_Code} has been processed"));
			}
		}
	}
}
