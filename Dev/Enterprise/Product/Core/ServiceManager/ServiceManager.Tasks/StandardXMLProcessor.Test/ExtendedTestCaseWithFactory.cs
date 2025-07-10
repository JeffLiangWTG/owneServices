using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	abstract class ExtendedTestCaseWithFactory : TestCaseWithFactory
	{
		protected EDIMessage CreateMessage(string messageType, string applicationCode, string direction, string status, ZGuid branchPK)
		{
			return CreateMessage(messageType, applicationCode, direction, status, branchPK, string.Empty, ZDateTime.Now);
		}

		protected EDIMessage CreateMessage(string messageType, string applicationCode, string direction, string status, ZGuid branchPK, string messageSubType)
		{
			return CreateMessage(messageType, applicationCode, direction, status, branchPK, messageSubType, ZDateTime.Now);
		}

		protected EDIMessage CreateMessage(string messageType, string applicationCode, string direction, string status, ZGuid branchPK, string messageSubType, ZDateTime createdTime)
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = branchPK;
			message.EM_MessageSubType = messageSubType;
			message.EM_SystemCreateTimeUtc = createdTime;

			return message;
		}

		protected EDIInterchange CreateInterchange(string interchangeType, string direction)
		{
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = "A";
			interchange.EI_To = "B";
			interchange.EI_ReceiveTransmit = direction;

			return interchange;
		}

		protected void AssertLogs(string expected, string actual)
		{
			expected = PrepareLog(expected.Trim(), "Error to save message: '", "'");
			actual = PrepareLog(actual.Trim(), "Error to save message: '", "'");
			AssertEquals(expected, actual);
		}

		protected string PrepareLog(string text, string startPhrase, string endPhrase)
		{
			string output = text;
			int startPosition = 0;
			while ((startPosition = output.IndexOf(startPhrase, startPosition)) != -1)
			{
				int endPosition = output.IndexOf("'", startPosition + startPhrase.Length);
				output = output.Substring(0, startPosition) + startPhrase + output.Substring(endPosition);
				startPosition++;
			}
			return output;
		}

		protected void AssertLogs(Regex regExp, string actual)
		{
			AssertEquals(
				message: $"Actual text: \r\n '{actual}' \r\n\r\n doesn't match Regular Expression \r\n\r\n '{regExp}' \r\n",
				expected: true,
				actual: regExp.IsMatch(actual));
		}
	}
}
