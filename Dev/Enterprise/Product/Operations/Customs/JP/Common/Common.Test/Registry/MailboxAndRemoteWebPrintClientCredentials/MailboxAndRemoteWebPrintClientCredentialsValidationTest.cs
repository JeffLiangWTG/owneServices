using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MailboxAndRemoteWebPrintClientCredentialsValidation))]
sealed class MailboxAndRemoteWebPrintClientCredentialsValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckLocalComputerAlias()
	{
		var error = "Local Computer Alias cannot be empty.";
		MailboxAndRemoteWebPrintClientCredentials.Validation.ValidateAll();
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.LocalComputerAliasInfo, error);

		MailboxAndRemoteWebPrintClientCredentials.LocalComputerAlias = "JPJPJP";
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.LocalComputerAliasInfo, error);
	}

	public void TestCheckDomainName()
	{
		var error = "Domain Name cannot be empty.";
		MailboxAndRemoteWebPrintClientCredentials.Validation.ValidateAll();
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.DomainNameInfo, error);

		MailboxAndRemoteWebPrintClientCredentials.DomainName = "JPJPJP";
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.DomainNameInfo, error);
	}

	public void TestCheckReceivingInterval()
	{
		var error = "Enter a digit between 3 to 10.";
		MailboxAndRemoteWebPrintClientCredentials.ReceivingInterval = 648;
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.ReceivingIntervalInfo, error);

		MailboxAndRemoteWebPrintClientCredentials.ReceivingInterval = 6;
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.ReceivingIntervalInfo, error);
	}

	public void TestCheckSendingInterval()
	{
		var error = "Enter a digit between 10 to 180.";
		MailboxAndRemoteWebPrintClientCredentials.SendingInterval = 200;
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.SendingIntervalInfo, error);

		MailboxAndRemoteWebPrintClientCredentials.SendingInterval = 12;
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.SendingIntervalInfo, error);
	}

	public void TestCheckFailureNotificationGroup()
	{
		var glbGroup = Factory.New<GlbGroup>();
		glbGroup.GG_Code = "GP1";
		ValidationTestHelper.AssertErrorIfInvalidCode(MailboxAndRemoteWebPrintClientCredentials.FailureNotificationGroupInfo, "XXX", "GP1");
	}

	public void TestCheckDownTime()
	{
		ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(MailboxAndRemoteWebPrintClientCredentials.DownTimeStartInfo, MailboxAndRemoteWebPrintClientCredentials.DownTimeEndInfo);
		ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(MailboxAndRemoteWebPrintClientCredentials.DownTimeEndInfo, MailboxAndRemoteWebPrintClientCredentials.DownTimeStartInfo);
		MailboxAndRemoteWebPrintClientCredentials.DownTimeEnd = ZDateTime.Invalid;
		MailboxAndRemoteWebPrintClientCredentials.DownTimeStart = ZDateTime.Invalid;

		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.DownTimeStartInfo, "Enter a valid Start.");
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.DownTimeEndInfo, "Enter a valid End.");

		MailboxAndRemoteWebPrintClientCredentials.DownTimeEnd = ZDateTime.Today.AddDays(1);
		MailboxAndRemoteWebPrintClientCredentials.DownTimeStart = ZDateTime.Today.AddDays(2);
		MailboxAndRemoteWebPrintClientCredentials.Validation.ValidateDownTimeEnd();
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.DownTimeStartInfo, "Enter a valid Start.");
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.DownTimeEndInfo, "Enter a valid End.");
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.DownTimeStartInfo, "End must be later than Start.");
		AssertHasError(MailboxAndRemoteWebPrintClientCredentials.DownTimeEndInfo, "End must be later than Start.");

		MailboxAndRemoteWebPrintClientCredentials.DownTimeEnd = ZDateTime.Today.AddDays(3);
		MailboxAndRemoteWebPrintClientCredentials.Validation.ValidateDownTimeStart();
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.DownTimeStartInfo, "End must be later than Start.");
		AssertNoError(MailboxAndRemoteWebPrintClientCredentials.DownTimeEndInfo, "End must be later than Start.");
	}

	MailboxAndRemoteWebPrintClientCredentials MailboxAndRemoteWebPrintClientCredentials => mailboxAndRemoteWebPrintClientCredentials ??= new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
	MailboxAndRemoteWebPrintClientCredentials mailboxAndRemoteWebPrintClientCredentials;
}
