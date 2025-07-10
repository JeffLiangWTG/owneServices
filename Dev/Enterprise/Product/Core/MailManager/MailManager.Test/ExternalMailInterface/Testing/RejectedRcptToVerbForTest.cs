using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rnwood.SmtpServer;
using Rnwood.SmtpServer.Verbs;

namespace Enterprise.MailManager.ExternalMailInterface
{
	sealed class RejectedRcptToVerbForTest : IVerb
	{
		public void Process(IConnection connection, SmtpCommand command)
		{
			if (connection.CurrentMessage == null)
			{
				connection.WriteResponse(new SmtpResponse(StandardSmtpResponseCode.BadSequenceOfCommands, "No current message"));
				return;
			}

			if (command.ArgumentsText == "<>" || !command.ArgumentsText.StartsWith("<") ||
				!command.ArgumentsText.EndsWith(">") || command.ArgumentsText.Count(c => c == '<') != command.ArgumentsText.Count(c => c == '>'))
			{
				connection.WriteResponse(
					new SmtpResponse(StandardSmtpResponseCode.SyntaxErrorInCommandArguments, "Must specify to address <address>"));
				return;
			}

			var address = command.ArgumentsText.Remove(0, 1).Remove(command.ArgumentsText.Length - 2);
			connection.Server.Behaviour.OnMessageRecipientAdding(connection, connection.CurrentMessage, address);
			var toListField = typeof(Message).GetProperty("ToList", BindingFlags.NonPublic | BindingFlags.Instance);
			(toListField.GetValue(connection.CurrentMessage) as List<String>).Add(address);

			if (address == "validTo@test.cargowise.com")
			{
				connection.WriteResponse(new SmtpResponse(StandardSmtpResponseCode.OK, "Recipient accepted"));
			}

			if (address == "invalidTo@test.cargowise.com")
			{
				connection.WriteResponse(new SmtpResponse(StandardSmtpResponseCode.CommandParameterNotImplemented, "Recipient rejected"));
			}
		}
	}
}
