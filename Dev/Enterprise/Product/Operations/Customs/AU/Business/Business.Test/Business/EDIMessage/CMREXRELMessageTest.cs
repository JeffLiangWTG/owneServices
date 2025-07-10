using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMREXRELMessage))]
	sealed class CMREXRELMessageTest : CMRCUSRESMessageTest
	{
		public void TestFindShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "88326446A4";

			var entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = CusEntryNumberTypes.Australia.CRN;
			entryNum.CE_EntryNum = "AAACYXYJP";
			entryNum.CE_ParentID = shipment.PK;
			entryNum.CE_ParentTable = shipment.TableName;
			Factory.Save();

			string validExrelMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::EXREL+15FC CIEH 04FE:1+14'
NAD+MR+41065894724::95'
RFF+CRN:AAACYXYJP:2:3'
RFF+EEC:EXLV'
RFF+BH:88326446A4'
DOC+1'
FTX+RAT+++THE GOODS ASSOCIATED WITH THE PARTICULAR LINE REFERRED TO IN THIS ADVICE HAVE BEEN ASSESSED BY CUSTOMS, ARE AUTHORISED FOR EXPORT AND MAY NOW BE DEALT WITH.'
UNT+9+000001'".Replace("\r\n", "");
			var message = Factory.New<CMREXRELMessage>();
			message.EM_MessageText = validExrelMessage;

			AssertNotNull(message.FindShipment());
			var foundShipment = message.FindShipment();
			AssertEquals(shipment, foundShipment);
		}

		public void TestFindExemptionShipment()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_HouseBill = "SAME_HB";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_HouseBill = "SAME_HB";
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_HouseBill = "SAME_HB";

			var entryNum = shipment2.CusEntryNumbers.AddNew();
			entryNum.CE_EntryType = CMRExportExemptionCodes.Get3CharCode(CMRExportExemptionCodes.EXLV.Code);
			entryNum.CE_ParentID = shipment2.PK;
			entryNum.CE_ParentTable = shipment2.TableName;
			entryNum.CE_EntryIsSystemGenerated = false;
			Factory.Save();

			string exrelResponseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::EXREL+15FC CIEH 04FE:1+14'
NAD+MR+41065894724::95'
RFF+CRN:AAACYXYJP:2:3'
RFF+EEC:EXLV'
RFF+BH:SAME_HB'
DOC+1'
FTX+RAT+++THE GOODS ASSOCIATED WITH THE PARTICULAR LINE REFERRED TO IN THIS ADVICE HAVE BEEN ASSESSED BY CUSTOMS, ARE AUTHORISED FOR EXPORT AND MAY NOW BE DEALT WITH.'
UNT+9+000001'".Replace("\r\n", "");
			var message = Factory.New<CMREXRELMessage>();
			message.EM_MessageText = exrelResponseMessage;

			AssertNotNull(message.FindShipment());
			var foundShipment = message.FindShipment();
			AssertEquals("Should find shipment 2 based on HB & exemption code search", shipment2, foundShipment);
		}

		public void TestFindCANShipment()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_HouseBill = "88326446A4";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_HouseBill = "88326446A4";
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_HouseBill = "88326446A4";

			var entryNum = shipment1.CusEntryNumbers.AddNew();
			entryNum.CE_EntryType = "CAN";
			entryNum.CE_EntryNum = "AAACYXYGR";
			entryNum.CE_ParentID = shipment1.PK;
			entryNum.CE_ParentTable = shipment1.TableName;
			entryNum.CE_EntryIsSystemGenerated = false;
			Factory.Save();

			string exrelResponseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::EXREL+15FC CIEH 04FE:1+14'
NAD+MR+41065894724::95'
RFF+CRN:AAACYXYJP:2:3'
RFF+CDI:AAACYXYGR'
RFF+BH:88326446A4'
DOC+1'
FTX+RAT+++THE GOODS ASSOCIATED WITH THE PARTICULAR LINE REFERRED TO IN THIS ADVICE HAVE BEEN ASSESSED BY CUSTOMS, ARE AUTHORISED FOR EXPORT AND MAY NOW BE DEALT WITH.'
UNT+9+000001'".Replace("\r\n", "");
			var message = Factory.New<CMREXRELMessage>();
			message.EM_MessageText = exrelResponseMessage;

			AssertNotNull(message.FindShipment());
			var foundShipment = message.FindShipment();
			AssertEquals("Should find shipment1 based on HB & CAN code search", shipment1, foundShipment);
		}

		public void TestFindContingencyShipment()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_HouseBill = "88326446A4";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_HouseBill = "88326446A4";
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_HouseBill = "88326446A4";

			var entryNum = shipment3.CusEntryNumbers.AddNew();
			entryNum.CE_EntryType = "CCN";
			entryNum.CE_EntryNum = "80362015D";
			entryNum.CE_ParentID = shipment3.PK;
			entryNum.CE_ParentTable = shipment3.TableName;
			entryNum.CE_EntryIsSystemGenerated = false;
			Factory.Save();

			string exrelResponseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::EXREL+15FC CIEH 04FE:1+14'
NAD+MR+41065894724::95'
RFF+CRN:AAACYXYJP:2:3'
RFF+CC:80362015D'
RFF+BH:88326446A4'
DOC+1'
FTX+RAT+++THE GOODS ASSOCIATED WITH THE PARTICULAR LINE REFERRED TO IN THIS ADVICE HAVE BEEN ASSESSED BY CUSTOMS, ARE AUTHORISED FOR EXPORT AND MAY NOW BE DEALT WITH.'
UNT+9+000001'".Replace("\r\n", "");
			var message = Factory.New<CMREXRELMessage>();
			message.EM_MessageText = exrelResponseMessage;

			AssertNotNull(message.FindShipment());
			var foundShipment = message.FindShipment();
			AssertEquals("Should find shipment3 based on HB & (CC) contingency code search", shipment3, foundShipment);
		}
	}
}
