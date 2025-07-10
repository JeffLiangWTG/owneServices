using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestIsAFRTransmitMessage()
		{
			XmlEDIMessage message = null;
			AssertEquals(false, message.IsAFRTransmitMessage());

			message = Factory.New<XmlEDIMessage>();
			AssertEquals(false, message.IsAFRTransmitMessage());
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_To = Constants.JapanCustomsReceipientID;
			interchange.ContainedMessages.Add(message);
			AssertEquals(true, message.IsAFRTransmitMessage());

			interchange.EI_To = "asd";
			AssertEquals(false, message.IsAFRTransmitMessage());
			interchange.EI_To = Constants.JapanCustomsReceipientID;
			AssertEquals(true, message.IsAFRTransmitMessage());

			message.EM_EI = ZGuid.Empty;
			AssertEquals(false, message.IsAFRTransmitMessage());
			message.EM_EI = interchange.PK;
			AssertEquals(true, message.IsAFRTransmitMessage());

			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals(false, message.IsAFRTransmitMessage());
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsAFRTransmitMessage());

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalSchedule;
			AssertEquals(false, message.IsAFRTransmitMessage());
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			AssertEquals(true, message.IsAFRTransmitMessage());
		}

		public void TestGetTargetBillNumber()
		{
			Guid currentPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testShipment.JS_HouseBill = "Bill";
			AssertEquals("BILL", testShipment.GetTargetBillNumber(Guid.Empty));
			AssertEquals("BILL", testShipment.GetTargetBillNumber(currentPK));

			JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(currentPK, Guid.Empty, Guid.Empty, "TEST");
			AssertEquals("BILL", testShipment.GetTargetBillNumber(Guid.Empty));
			AssertEquals("TESTBILL", testShipment.GetTargetBillNumber(currentPK));

			JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(currentPK, Guid.Empty, Guid.Empty, "Tes");
			AssertEquals("BILL", testShipment.GetTargetBillNumber(Guid.Empty));
			AssertEquals("Tes-BILL", testShipment.GetTargetBillNumber(currentPK));

			JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(currentPK, Guid.Empty, Guid.Empty, "");
			AssertEquals("BILL", testShipment.GetTargetBillNumber(Guid.Empty));
			AssertEquals("BILL", testShipment.GetTargetBillNumber(currentPK));

			testShipment = null;
			AssertEquals(string.Empty, testShipment.GetTargetBillNumber(Guid.Empty));
		}

		public void TestGetJapanNotificationParty()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			var testConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var result1 = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			var result2 = testConsigneeOrg.GetJapanNotificationParty(testCompany);
			AssertNull(result1);
			AssertNull(result2);

			var relatedOrg_NON_COM = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_NON_COM.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();
			result1 = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			result2 = testConsigneeOrg.GetJapanNotificationParty(testCompany);
			AssertNull(result1);
			AssertNull(result2);

			var relatedOrg_PIC_COM = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_PIC_COM.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Pickup, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();
			result1 = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			result2 = testConsigneeOrg.GetJapanNotificationParty(testCompany);
			AssertNull(result1);
			AssertNull(result2);

			var relatedOrg_DLV_COM = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_DLV_COM.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();
			result1 = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			result2 = testConsigneeOrg.GetJapanNotificationParty(testCompany);
			AssertNotNull(result1);
			AssertNull(result2);
			AssertEquals(relatedOrg_DLV_COM, result1.RelatedParty);

			var relatedOrg_NON_ENT = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_NON_ENT.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, ZString.Empty, ZString.Empty, ZString.Empty, null);
			Factory.Save();
			result1 = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			result2 = testConsigneeOrg.GetJapanNotificationParty(testCompany);
			AssertNotNull(result1);
			AssertNull(result2);
			AssertEquals(relatedOrg_DLV_COM, result1.RelatedParty);

			var relatedOrg_DLV_ENT = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_DLV_ENT.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty, null);
			Factory.Save();
			result1 = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			result2 = testConsigneeOrg.GetJapanNotificationParty(testCompany);
			AssertNotNull(result1);
			AssertNotNull(result2);
			AssertEquals(relatedOrg_DLV_COM, result1.RelatedParty);
			AssertEquals(relatedOrg_DLV_ENT, result2.RelatedParty);
		}

		public void TestGetJapanNotificationParty_Order()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			var testConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var result = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			AssertNull(result);

			var relatedOrg_PAD_ENT = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_PAD_ENT.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.PickupAndDelivery, ZString.Empty, ZString.Empty, null);
			Factory.Save();
			result = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			AssertEquals(relatedOrg_PAD_ENT, result.RelatedParty);

			var relatedOrg_DLV_ENT = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_DLV_ENT.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty, null);
			Factory.Save();
			result = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			AssertEquals(relatedOrg_DLV_ENT, result.RelatedParty);

			var relatedOrg_PAD_COM = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_PAD_COM.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.PickupAndDelivery, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();
			result = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			AssertEquals(relatedOrg_PAD_COM, result.RelatedParty);

			var relatedOrg_DLV_COM = Factory.NewWithValidTestData<OrgHeader>();
			testConsigneeOrg.AddRelatedParty(relatedOrg_DLV_COM.PK, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();
			result = testConsigneeOrg.GetJapanNotificationParty(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			AssertEquals(relatedOrg_DLV_COM, result.RelatedParty);
		}

		public void TestGetTargetOceanBillNumber()
		{
			Guid currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(currentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("TESTBILL", (new ZString("TESTBILL")).GetTargetOceanBillNumber(currentCompanyPK, "1234"));
			AssertEquals("TESTBILL", (new ZString("TESTBILL")).GetTargetOceanBillNumber(currentCompanyPK, "123"));
			AssertEquals("TESTBILL", (new ZString("TESTBILL")).GetTargetOceanBillNumber(currentCompanyPK, ""));

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(currentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals("1234TESTBILL", (new ZString("TESTBILL")).GetTargetOceanBillNumber(currentCompanyPK, "1234"));
			AssertEquals("123-TESTBILL", (new ZString("TESTBILL")).GetTargetOceanBillNumber(currentCompanyPK, "123"));
			AssertEquals("TESTBILL", (new ZString("TESTBILL")).GetTargetOceanBillNumber(currentCompanyPK, ""));
		}
	}
}
