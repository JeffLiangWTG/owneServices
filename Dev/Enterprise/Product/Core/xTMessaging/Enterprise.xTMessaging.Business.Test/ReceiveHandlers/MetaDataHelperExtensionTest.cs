using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.xTMessaging.Business.Test
{
	public class MetaDataHelperExtensionTest : TestCaseWithFactory
	{
		public void TestIsValidForSavingToEDIInterchange()
		{
			var dict = new Dictionary<string, string>();
			dict.Add(Constants.CustomMsgAttributes.MessageTrackingID, Guid.NewGuid().ToString());
			var metaData = new MetaDataHelper(dict);
			AssertEquals(false, metaData.IsValidForSavingToEDIInterchange());

			dict[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			dict[Constants.CustomMsgAttributes.DestinationParty] = "TESTRECEIVER";
			dict[Constants.CustomMsgAttributes.MessageTrackingID] = "4EF4E999-E649-4971-AA19-E8D6BC605B6F";
			dict[Constants.CustomMsgAttributes.MessageType] = "TST";
			metaData = new MetaDataHelper(dict);
			AssertEquals(false, metaData.IsValidForSavingToEDIInterchange());

			dict[Constants.CustomMsgAttributes.SourceParty] = "";
			metaData = new MetaDataHelper(dict);
			AssertEquals(false, metaData.IsValidForSavingToEDIInterchange());

			dict[Constants.CustomMsgAttributes.SourceParty] = "TESTSENDER";
			metaData = new MetaDataHelper(dict);
			AssertEquals(true, metaData.IsValidForSavingToEDIInterchange());
		}

		public void TestMergeMessageAttributeFromOriginalInterchange()
		{
			var dict1 = new Dictionary<string, string> {
					{ Constants.CustomMsgAttributes.SourceParty, "TESTCUSTOMS" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "F562CE18-AF24-4149-AEA7-0A76B148C7AD" },
					{ Constants.CustomMsgAttributes.MessageType, "RES" },
					{ Constants.xTMsgAttributes.refexternal, "20" }
				};

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			Factory.Save();

			var oppositeInterchange = Factory.New<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = ZGuid.ParseSafe("F562CE18-AF24-4149-AEA7-0A76B148C7AD");
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_GB = branch.PK;
			oppositeInterchange.EI_From = "TSTRECIPIENT";
			oppositeInterchange.EI_To = "TSTSENDER";
			oppositeInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			oppositeInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			oppositeInterchange.EI_XTInternalMsgID = 12345L;

			var metaData = new MetaDataHelper(dict1);

			metaData.MergeMessageAttributesFromOriginalInterchange(oppositeInterchange);

			AssertArrayEqualsByElements("MetaData for Processing", new string[] {
				$"{Constants.CustomMsgAttributes.ApplicationCode}:TST" ,
				$"{Constants.CustomMsgAttributes.DestinationParty}:TSTRECIPIENT" ,
				$"{Constants.CustomMsgAttributes.MessageTrackingID}:F562CE18-AF24-4149-AEA7-0A76B148C7AD" ,
				$"{Constants.CustomMsgAttributes.MessageType}:RES",
				$"{Constants.CustomMsgAttributes.SourceParty}:TESTCUSTOMS",
				$"{Constants.xTMsgAttributes.refexternal}:20"
			}, dict1.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}").ToArray());
		}
	}
}
