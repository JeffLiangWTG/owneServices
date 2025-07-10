using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	[TestedType(typeof(StandAloneFsrEnquiry))]
	class StandAloneFsrEnquiryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOnSavingStoresPlaceholder()
		{
			var message = Factory.New<StandAloneFsrEnquiry>();
			message.EM_MessageText = "HELLO " + StandAloneFsrEnquiry.MessageNumberPlaceHolder + " " + GbTransmissionMessageGenerator.SysCarPlaceHolder;
			Factory.Save();
			AssertContains("PK is put into body upon saving", message.PK.ToString().Substring(0, 8).ToUpper(), message.EM_MessageText);
			AssertNotContains("PK replaces placeholder", GbTransmissionMessageGenerator.SysCarPlaceHolder, message.EM_MessageText);
		}

		public void TestSetDefaultValues()
		{
			var message = Factory.New<StandAloneFsrEnquiry>();
			AssertEquals("FSR", message.EM_MessageType);
			AssertEquals("ENQ", message.EM_MessageSubType);
		}

		public void TestLinkedMessage()
		{
			var parentMessage = Factory.New<StandAloneFsrEnquiry>();
			AssertEquals("No linked message", false, parentMessage.HasLinkedMessage);
			AssertEquals("No linked message", null, parentMessage.LinkedMessage);
			var childMessage = Factory.New<StandAloneFsrEnquiry>();
			childMessage.EM_LinkedObject = parentMessage;
			AssertEquals("No linked message, the link is the wrong way round", false, parentMessage.HasLinkedMessage);
			AssertEquals("No linked message, the link is the wrong way round", null, parentMessage.LinkedMessage);
			parentMessage.EM_LinkedObject = childMessage;
			AssertEquals(true, parentMessage.HasLinkedMessage);
			AssertEquals(childMessage, parentMessage.LinkedMessage);
		}

		public void TestResponseText()
		{
			var message = Factory.New<StandAloneFsrEnquiry>();
			message.ResponseText = "Hello";
			message.EM_MessageText = StandAloneFsrEnquiry.MessageNumberPlaceHolder;
			AssertEquals("HELLO", message.ResponseText);
			Factory.Save();
			message = new BusinessObjectFactory().Load<StandAloneFsrEnquiry>(message.PK);
			AssertEquals("HELLO", message.ResponseText);
			message.ResponseText = "Basic consignment record retrieved which has babies and is really long in fact much longer than Daniel's recommended 100 characters that could be store in GenAddOn even after removing the key words";
			AssertEquals("BASIC CONSIGNMENT RECORD RETRIEVED WHICH HAS BABIES AND IS REALLY LONG IN FACT MUCH LONGER THAN DANI", message.ResponseText);
		}

		public void TestMakeNewOutboundFromPayload()
		{
			var npbo = NonPersistentStandAloneFsrEnquiryForNewTest.GetNonPersistentStandAloneFsrEnquiryForNew(Factory);
			npbo.DatabaseToQuery = FsrRequestType.Codes.ImportEnquiry;
			var result = StandAloneFsrEnquiry.MakeNewOutboundFromPayload(npbo);
			AssertEquals("CUKFFW98000DAN", result.EM_MessageOwner);
			AssertEquals("QUE", result.EM_Status);
			AssertEquals("TRX", result.EM_ReceiveTransmit);
			AssertEquals("LHRBAC 125-12345678-87654321/99", result.EM_ApplicationReference);
			AssertEquals("CUK", result.EM_ApplicationCode);
			AssertContains("<p>AWB: 125-12345678-87654321/99</p>", result.EM_MessageInterpretation);
			AssertContains("<p>Database: Import Enquiry</p>", result.EM_MessageInterpretation);
			AssertContains("CUKFSR:1:912:BT", result.EM_MessageText);
			AssertContains("BGM++12512345678+++HWB:87654321:99++IMP'", result.EM_MessageText);
			AssertContains("LOC+11:LHR:145:3::BAC:129:ZZZ'", result.EM_MessageText);
		}

		public void TestGetRequeryMessage()
		{
			RunGetRequeryMessageTest("", "125-1234 5678", "", "", "FSA", "CUKFFW98000WIS");
			RunGetRequeryMessageTest("", "125-1234 5678", "HOUSE001", "", "BTH", "CUKFFW98000CAR");
			RunGetRequeryMessageTest("", "125-1234 5678", "", "02", "FSA", "CUKFFW98000WIS");
			RunGetRequeryMessageTest("", "125-1234 5678", "HOUSE001", "03", "BTH", "CUKFFW98000CAR");
			RunGetRequeryMessageTest("", "125-1234 5678", "", "", "IMP", "CUKFFW98000WIS");
			RunGetRequeryMessageTest("", "125-1234 5678", "HOUSE001", "", "EXP", "CUKFFW98000CAR");

			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "", "", "FSA", "CUKFFW98000WIS");
			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "HOUSE001", "", "BTH", "CUKFFW98000CAR");
			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "", "02", "FSA", "CUKFFW98000WIS");
			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "HOUSE001", "03", "BTH", "CUKFFW98000CAR");
			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "", "", "IMP", "CUKFFW98000WIS");
			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "HOUSE001", "", "EXP", "CUKFFW98000CAR");

			RunGetRequeryMessageTest("LHRBAC", "125-1234 5678", "HOUSE001", "", "FSN", "CUKFFW98000CAR");
		}

		void RunGetRequeryMessageTest(ZString location, ZString mawb, string hawb, string split, string type, string pima)
		{
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(Factory);
			npbo.Airport = location.Left(3);
			npbo.Shed = location.Right(3);
			npbo.MAWB = mawb;
			npbo.HAWB = hawb;
			npbo.SRF = split;
			npbo.DatabaseToQuery = type;
			npbo.PIMA = pima;
			var message = StandAloneFsrEnquiry.MakeNewOutboundFromPayload(npbo);
			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message);
			interchange.EI_From = message.EM_MessageOwner;
			var clone = message.GetRequeryMessage();
			AssertEquals("Location of goods", location, clone.Airport + clone.Shed);
			AssertEquals("MAWB#", mawb.KeepAlphanumericCharacters(), clone.MAWB);
			AssertEquals("HAWB#", hawb, clone.HAWB);
			AssertEquals("Split", split, clone.SRF);
			AssertEquals("Profile", pima, clone.PIMA);
			AssertEquals("Database", type, clone.DatabaseToQuery);
		}
	}

	[TestedType(typeof(StandAloneFsrEnquiryCollection))]
	class StandAloneFsrEnquiryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(StandAloneFsrEnquiryCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StandAloneFsrEnquiryCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<StandAloneFsrEnquiry>();
		}
	}

	[TestedType(typeof(NonPersistentStandAloneFsrEnquiryForNew))]
	internal class NonPersistentStandAloneFsrEnquiryForNewTest : NonPersistentBusinessObjectTestCase
	{
		public static NonPersistentStandAloneFsrEnquiryForNew GetNonPersistentStandAloneFsrEnquiryForNew(BusinessObjectFactory factory)
		{
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(factory);
			npbo.MAWB = "125-1234 5678"; // formatted as per ZMasterBillControl
			npbo.HAWB = "87654321";
			npbo.SRF = "99";
			npbo.Airport = "LHR";
			npbo.Shed = "BAC";
			npbo.PIMA = "CUKFFW98000DAN";
			return npbo;
		}

		public void TestAwbNumberFormatted()
		{
			var npbo = GetNonPersistentStandAloneFsrEnquiryForNew(Factory);
			AssertEquals("125-12345678-87654321/99", npbo.AwbNumberFormatted);
		}

		public void TestFsnRetransmissionDoesNotExplode()
		{
			var npbo = GetNonPersistentStandAloneFsrEnquiryForNew(Factory);
			npbo.DatabaseToQuery = FsrRequestType.Codes.FsnRetransmission;
			var ediMessage = StandAloneFsrEnquiry.MakeNewOutboundFromPayload(npbo);
			AssertContains("The recipient shed PIMA is in the messagem, the construction of which dcoes not explode", "CUKAIR98LHRBAC:EI", ediMessage.EM_MessageText);
		}

		public void TestDefaultingOfPima()
		{
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(new BusinessObjectFactory());
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);  // makes one PIMA
			npbo = new NonPersistentStandAloneFsrEnquiryForNew(new BusinessObjectFactory());
			AssertEquals("Several PIMAs exist so do not select one", "", npbo.PIMA);
		}
	}

	class NonPersistentStandAloneFsrEnquiryForNewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAllButPima()
		{
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(Factory);
			AssertValidation(npbo.MAWBInfo, 11, true);
			AssertValidation(npbo.HAWBInfo, 8, false);
			AssertValidation(npbo.SRFInfo, 2, false);
			AssertValidation(npbo.AirportInfo, 3, false);
			AssertValidation(npbo.ShedInfo, 3, false);
			AssertPairedValidation(npbo.AirportInfo, npbo.ShedInfo);
		}

		public void TestPima()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(Factory);
			npbo.PIMA = "X";
			AssertHasErrorContaining(npbo.PIMAInfo, "Enter a valid");
			npbo.PIMA = "";
			AssertHasErrorContaining(npbo.PIMAInfo, "Please enter");
			npbo.PIMA = "CUKFFW98000LXA";
			AssertNoErrorContaining(npbo.PIMAInfo, "Enter a valid");
			AssertNoErrorContaining(npbo.PIMAInfo, "Please enter");

			AssertNoErrorContaining(npbo.PIMAInfo, "security right");
			Environment.Env.Security.AirCcsukShed.IsAllowed = false;
			npbo.PIMA = "CUKAIR98LHRBAC";
			AssertHasErrorContaining(npbo.PIMAInfo, "security right");
			npbo.PIMA = "CUKFFW98000LXA";
			AssertNoErrorContaining(npbo.PIMAInfo, "security right");
			Environment.Env.Security.AirCcsukShed.IsAllowed = true;
			npbo.PIMA = "CUKFFW98000LXA";
			AssertNoErrorContaining(npbo.PIMAInfo, "security right");
			npbo.PIMA = "CUKAIR98LHRBAC";
			AssertNoErrorContaining(npbo.PIMAInfo, "security right");
		}

		public void TestDatabaseToQuery()
		{
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(Factory);
			npbo.DatabaseToQuery = "XXX";
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Please enter");
			AssertHasErrorContaining(npbo.DatabaseToQueryInfo, "Enter a valid");
			npbo.DatabaseToQuery = "";
			AssertHasErrorContaining(npbo.DatabaseToQueryInfo, "Please enter");
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Enter a valid");
			npbo.DatabaseToQuery = FsrRequestType.Codes.FsaEnquiry;
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Please enter");
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Enter a valid");

			npbo.DatabaseToQuery = FsrRequestType.Codes.ExportEnquiry;
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Please enter");
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Enter a valid");

			npbo.DatabaseToQuery = FsrRequestType.Codes.ImportEnquiry;
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Please enter");
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Enter a valid");

			npbo.DatabaseToQuery = FsrRequestType.Codes.FirstImportsThenExports;
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Please enter");
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "Enter a valid");

			npbo.DatabaseToQuery = FsrRequestType.Codes.FsnRetransmission;
			AssertHasErrorContaining(npbo.DatabaseToQueryInfo, "FSN retransmission requires airport and shed");
			npbo.DatabaseToQuery = FsrRequestType.Codes.FsaEnquiry;
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "FSN retransmission requires airport and shed");
			npbo.Airport = "XXX";
			npbo.Shed = "YYY";
			npbo.DatabaseToQuery = FsrRequestType.Codes.FsnRetransmission;
			AssertNoErrorContaining(npbo.DatabaseToQueryInfo, "FSN retransmission requires airport and shed");
		}

		void AssertPairedValidation(ZPropertyInfo zPropertyInfo, ZPropertyInfo zPropertyInfo_2)
		{
			zPropertyInfo.Value = new ZString("AAA");
			AssertHasErrorContaining(zPropertyInfo, "both present or both missing");
			AssertHasErrorContaining(zPropertyInfo_2, "both present or both missing");
			zPropertyInfo.Value = new ZString("");
			AssertNoErrorContaining(zPropertyInfo, "both present or both missing");
			AssertNoErrorContaining(zPropertyInfo_2, "both present or both missing");
			zPropertyInfo_2.Value = new ZString("AAA");
			AssertHasErrorContaining(zPropertyInfo, "both present or both missing");
			AssertHasErrorContaining(zPropertyInfo_2, "both present or both missing");
			zPropertyInfo.Value = new ZString("BBB");
			AssertNoErrorContaining(zPropertyInfo, "both present or both missing");
			AssertNoErrorContaining(zPropertyInfo_2, "both present or both missing");
		}

		void AssertValidation(ZPropertyInfo zPropertyInfo, int minLength, bool isMandatory)
		{
			zPropertyInfo.Value = ZString.Replicate('x', minLength);
			AssertNoErrorContaining(zPropertyInfo, "Please enter");
			AssertNoErrorContaining(zPropertyInfo, "must be exactly");
			zPropertyInfo.Value = ZString.Replicate('x', minLength - 1);
			AssertNoErrorContaining(zPropertyInfo, "Please enter");
			AssertHasErrorContaining(zPropertyInfo, "must be exactly");
			if (isMandatory)
			{
				zPropertyInfo.Value = ZString.Empty;
				AssertHasErrorContaining(zPropertyInfo, "Please enter");
			}
			else
			{
				zPropertyInfo.Value = ZString.Empty;
				AssertNoErrorContaining(zPropertyInfo, "Please enter");
			}
		}
	}
}
