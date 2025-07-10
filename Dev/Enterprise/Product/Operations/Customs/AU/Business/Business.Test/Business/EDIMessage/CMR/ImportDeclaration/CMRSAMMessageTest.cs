using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSAMMessage))]
	sealed class CMRSAMMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestSetDefaultValues()
		{
			CMRSAMMessage message = Factory.New<CMRSAMMessage>();
			AssertEquals("Message type is set", CMRMessage.CMRMessageTypes.SAM, message.EM_MessageType);
		}

		public void TestGetReport()
		{
			ZString response = CMRImportDeclarationTestData.SAM;

			ZString expectedEmailBody =
				@"Status: CLEAR
Status Description: CLEAR
";
			CMRSAMMessage message = Factory.New<CMRSAMMessage>();
			message.EM_MessageText = response;
			AssertEquals("Body Of Email", expectedEmailBody, message.GetReport());
		}

		public void TestGetReportWithMessageAdvices()
		{
			ZString response =
				"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SAM+3987 G103 0JF5:1+11'FTX+AHN+++CLEAR:CLEAR'NAD+MR+AAA374M::95'" +
				"NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'NAD+IM++MR CHRISTOPHER JOHN REED'" +
				"RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAATW6E9::1'RFF+ABQ:1'RFF+ADU:B00122382'DOC+1'ERP+::2'ERC+4::95'" +
				"FTX+ABS+++IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED'DOC+1'ERP+::3'ERC+4::95'" +
				"FTX+ABS+++IMPORTED FOOD CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'" +
				"UNT+20+000001'";

			ZString expectedEmailBody =
				@"Status: CLEAR
Status Description: CLEAR

Message Advice:
	IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED (LINE 4)
	IMPORTED FOOD CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED (LINE 4)
";
			CMRSAMMessage message = Factory.New<CMRSAMMessage>();
			message.EM_MessageText = response;
			AssertEquals("Body Of Email", expectedEmailBody, message.GetReport());
		}

		#region Implementation

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRSAMMessageTestClass message = Factory.New<CMRSAMMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.SAM.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRSAMMessageTestClass message = (CMRSAMMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}

		#endregion
	}
}
