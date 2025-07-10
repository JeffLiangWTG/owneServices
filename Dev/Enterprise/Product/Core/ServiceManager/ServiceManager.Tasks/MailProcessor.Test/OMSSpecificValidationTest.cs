using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing;

class OMSSpecificValidationTest : TransactionedTestCase
{
	public void TestValidateReturnsEmptyValidationResultIfNotUseGraphApiForOutgoingAndNotUseOffice365()
	{
		// Arrange
		var validation = new OMSSpecificValidation(4);
		using var useGraphApi =
			Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		using var smtp =
			Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "abc");

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals("no error is expected", 0, result.Errors.Count());
		AssertEquals("no warning is expected", 0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsValidationResultWithWarningsIfGivenValueIsGreaterThan3()
	{
		// Arrange
		var validation = new OMSSpecificValidation(4);
		using var useGraphApi =
			Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		using var smtp =
			Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				"smtp.office365.com");

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals(0, result.Errors.Count());
		AssertEquals(3, result.PropertySpecificWarnings.Count());

		var first = result.PropertySpecificWarnings.First();
		AssertEquals("SecondaryProcessesMaxCountInfo", first.Key);
		AssertEquals(
			"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages for details.",
			first.Value);

		var second = result.PropertySpecificWarnings.ElementAt(1);
		AssertEquals("ExtendedConfigProcessesMaxCountWarningMessage", second.Key);
		AssertEquals(
			"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to the following for details:",
			second.Value);

		var third = result.PropertySpecificWarnings.ElementAt(2);
		AssertEquals("ExtendedConfigProcessesMaxCountWarningLink", third.Key);
		AssertEquals("https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages", third.Value);
	}

	public void TestValidateReturnsEmptyValidationResultIfGivenValueEquals3()
	{
		// Arrange
		var validation = new OMSSpecificValidation(3);
		using var useGraphApi =
			Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		using var smtp =
			Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "smtp.office365.com");

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals("no error is expected", 0, result.Errors.Count());
		AssertEquals("no warning is expected", 0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsEmptyValidationResultIfGivenValueIsLessThan3()
	{
		// Arrange
		var validation = new OMSSpecificValidation(2);
		using var useGraphApi =
			Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		using var smtp =
			Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "smtp.office365.com");

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals("no error is expected", 0, result.Errors.Count());
		AssertEquals("no warning is expected", 0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsValidationResultWithWarningsIfGivenValueIsLessThan4AndUsingOutlookServer()
	{
		// Arrange
		var validation = new OMSSpecificValidation(2);
		using var graphApi = Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		using var smtp = Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "smtp-mail.outlook.com");

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals("no error is expected", 0, result.Errors.Count());
		AssertEquals("no warning is expected", 0, result.PropertySpecificWarnings.Count());
	}

	public void TestValidateReturnsEmptyValidationResultIfGivenValueIs4OrGreaterAndUsingOutlookServer()
	{
		// Arrange
		var validation = new OMSSpecificValidation(4);
		using var graphApi = Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		using var smtp = Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "smtp-mail.outlook.com");

		// Act
		var result = validation.Validate();

		// Assert
		AssertEquals(0, result.Errors.Count());
		AssertEquals(3, result.PropertySpecificWarnings.Count());

		var first = result.PropertySpecificWarnings.First();
		AssertEquals("SecondaryProcessesMaxCountInfo", first.Key);
		AssertEquals(
			"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages for details.",
			first.Value);

		var second = result.PropertySpecificWarnings.ElementAt(1);
		AssertEquals("ExtendedConfigProcessesMaxCountWarningMessage", second.Key);
		AssertEquals(
			"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to the following for details:",
			second.Value);

		var third = result.PropertySpecificWarnings.ElementAt(2);
		AssertEquals("ExtendedConfigProcessesMaxCountWarningLink", third.Key);
		AssertEquals("https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages", third.Value);
	}
}

