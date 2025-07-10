using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;

namespace Enterprise.xTMessaging.Business.Test
{
	public class UtilsTest : TestCaseWithFactory
	{
		public void GetHeaderTextDictionary_Normal()
		{
			var testLogger = new TestUtils.TestLogger();
			var interchange = TestUtils.CreateInterchangeForXT(Factory);

			interchange.EI_HeaderText = "{" +
				"'custom.COUNTRY.EMPTY':''," +
				"'custom.COUNTRY.MULTILINE':'Line1\r\nLine2\r\n'," +
				"'custom.COUNTRY.NORMAL':'NORMAL'," +
				"'COUNTRY.PRIVATE':'PRIVATE'," +
				"'custom.COUNTRY.NONSTRING':123," +
				"'custom.ApplicationCode': 'OVD'," +
				"'custom.DUPKEY': 'VAL1'," +
				"'custom.DUPKEY': 'VAL2'," +
				"}";
			AssertContainsExactElementsInAnyOrder("Normal Case - Result", new[]
			{
				"custom.COUNTRY.MULTILINE - Line1\r\nLine2\r\n",
				"custom.COUNTRY.NORMAL - NORMAL",
				"COUNTRY.PRIVATE - PRIVATE",
				"custom.COUNTRY.NONSTRING - 123",
				"custom.DUPKEY - VAL2"
			}, interchange.EI_HeaderText.ToString().GetHeaderTextDictionary(testLogger).Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
			AssertArrayEqualsByElements("Normal Case - Logs", System.Array.Empty<string>(), testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
		}

		public void GetHeaderTextDictionary_Empty()
		{
			var testLogger = new TestUtils.TestLogger();
			var interchange = TestUtils.CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = string.Empty;
			AssertContainsExactElementsInAnyOrder("Empty Case - result", System.Array.Empty<string>(), interchange.EI_HeaderText.ToString().GetHeaderTextDictionary(testLogger).Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
			AssertArrayEqualsByElements("Empty Case - Logs", System.Array.Empty<string>(), testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
		}

		public void GetHeaderTextDictionary_Invalid()
		{
			var testLogger = new TestUtils.TestLogger();
			var interchange = TestUtils.CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "RANDOMSTRING";

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), interchange.EI_HeaderText.ToString().GetHeaderTextDictionary(testLogger).Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
			AssertArrayEqualsByElements("Empty Case - Logs", new string[] {
				"Invalid Header Text for xT Message Attribute"
			}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
		}

		public void TestSetHeaderTextWithAttributeDictionary()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.SetHeaderTextWithAttributeDictionary(null);
			AssertEquals(string.Empty, interchange.EI_HeaderText);

			interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>()
			{
				["custom.test"] = "Test",
				[Constants.xTMsgAttributes.cw1key] = "cw1key",
				[Constants.CustomMsgAttributes.ApplicationCode] = "APP",
				[Constants.CustomMsgAttributes.MessageType] = "MT",
				[Constants.CustomMsgAttributes.SourceParty] = "SP",
				[Constants.CustomMsgAttributes.DestinationParty] = "DP",
				[Constants.CustomMsgAttributes.MessageTrackingID] = "12345678-1234-1234-1234-123456789012"
			});
			AssertEquals("{\"custom.test\":\"Test\"}", interchange.EI_HeaderText);

			interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>()
			{
				["custom.test"] = "Test",
				[Constants.xTMsgAttributes.cw1key] = "cw1key",
				[Constants.CustomMsgAttributes.ApplicationCode] = "APP",
				[Constants.CustomMsgAttributes.MessageType] = "MT",
				[Constants.CustomMsgAttributes.SourceParty] = "SP",
				[Constants.CustomMsgAttributes.DestinationParty] = "DP",
				[Constants.CustomMsgAttributes.MessageTrackingID] = "12345678-1234-1234-1234-123456789012"
			}, includeNonCustomMsgAttribute: true);
			AssertEquals("{\"custom.test\":\"Test\",\"cw1.key\":\"cw1key\"}", interchange.EI_HeaderText);
		}
	}
}
