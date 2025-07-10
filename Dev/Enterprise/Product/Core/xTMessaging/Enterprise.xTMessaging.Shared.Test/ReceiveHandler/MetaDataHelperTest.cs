using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.xTMessaging.Business.Test
{
	public class MetaDataHelperTest : TestCaseWithFactory
	{
		public void TestMetaDataHelperThrowsExceptionOnEmptyDict()
		{
			AssertMetaDataHelperThrowsExceptionWhenMissingKeyInformation(new Dictionary<string, string>());
		}

		public void TestMetaDataHelperThrowsExceptionOnInvalidMessageTrackingID()
		{
			var dict = new Dictionary<string, string>();
			dict[Constants.CustomMsgAttributes.MessageTrackingID] = Guid.Empty.ToString();
			AssertMetaDataHelperThrowsExceptionWhenMissingKeyInformation(dict);
		}

		public void TestMetaDataHelperThrowsExceptionOnInvalidRefExternal()
		{
			var dict = new Dictionary<string, string>();
			dict[Constants.xTMsgAttributes.refexternal] = (0UL).ToString();
			AssertMetaDataHelperThrowsExceptionWhenMissingKeyInformation(dict);
		}

		public void TestMetaDataHelperThrowsExceptionOnInvalidRefExternalUri()
		{
			var dict = new Dictionary<string, string>();
			dict[Constants.xTMsgAttributes.refexternal] = "NOTAVALIDURI";
			AssertMetaDataHelperThrowsExceptionWhenMissingKeyInformation(dict);
		}

		public void AssertMetaDataHelperThrowsExceptionWhenMissingKeyInformation(Dictionary<string, string> dict)
		{
			var ex = AssertExceptionThrown<Exception>(() => new MetaDataHelper(dict));
			AssertEquals("Missing Key Message Information", ex.Message);
		}

		public void TestGenericMetaData()
		{
			CombineAssertions(() =>
			{
				GenericMetaDataAsserts("xt-msg:4ef4ba7d-f95f-4462-8a34-29bd2388772d");
				GenericMetaDataAsserts((1UL).ToString());
			});
		}

		void GenericMetaDataAsserts(string refexternal)
		{
			var dict = new Dictionary<string, string>();
			var guid = Guid.NewGuid();
			dict.Add(Constants.CustomMsgAttributes.MessageTrackingID, guid.ToString());
			dict[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			dict[Constants.CustomMsgAttributes.DestinationParty] = "TESTRECEIVER";
			dict[Constants.CustomMsgAttributes.SourceParty] = "TESTSENDER";
			dict[Constants.CustomMsgAttributes.MessageType] = "TST";
			dict[Constants.xTMsgAttributes.refexternal] = refexternal;
			var metaData = new MetaDataHelper(dict);

			AssertEquals("TST", metaData.ApplicationCode);
			AssertEquals("TESTRECEIVER", metaData.DestinationParty);
			AssertEquals("TESTSENDER", metaData.SourceParty);
			AssertEquals(guid, metaData.MessageTrackingId);
			AssertEquals("TST", metaData.MessageType);
			if (long.TryParse(refexternal, out var actual))
			{
				AssertEquals(actual, metaData.OriginalMessageId);
			}
			else
			{
				AssertEquals(refexternal, metaData.OriginalMessageUri);
			}
		}

		public void TestMergeMessageAttributeFromOriginalMessage()
		{
			var dict1 = new Dictionary<string, string> {
					{ Constants.CustomMsgAttributes.SourceParty, "TESTCUSTOMS" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "F562CE18-AF24-4149-AEA7-0A76B148C7AD" },
					{ Constants.CustomMsgAttributes.MessageType, "RES" },
					{ Constants.xTMsgAttributes.refexternal, "20" }
				};
			var dict2 = new Dictionary<string, string>
				{
					{ Constants.CustomMsgAttributes.SourceParty, "TESTSENDER" },
					{ Constants.CustomMsgAttributes.DestinationParty, "TESTRECEIVER" },
					{ Constants.CustomMsgAttributes.ApplicationCode, "TST" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "4EF4E999-E649-4971-AA19-E8D6BC605B6F" },
					{ Constants.CustomMsgAttributes.MessageType, "TST" }
				};

			var metaData = new MetaDataHelper(dict1);

			metaData.MergeMessageAttributesFromOriginalInfo(dict2);

			AssertArrayEqualsByElements("MetaData for Processing", new string[] {
				$"{Constants.CustomMsgAttributes.ApplicationCode}:TST" ,
				$"{Constants.CustomMsgAttributes.DestinationParty}:TESTSENDER" ,
				$"{Constants.CustomMsgAttributes.MessageTrackingID}:F562CE18-AF24-4149-AEA7-0A76B148C7AD" ,
				$"{Constants.CustomMsgAttributes.MessageType}:RES",
				$"{Constants.CustomMsgAttributes.SourceParty}:TESTCUSTOMS",
				$"{Constants.xTMsgAttributes.refexternal}:20"
			}, dict1.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}").ToArray());
		}
	}
}
