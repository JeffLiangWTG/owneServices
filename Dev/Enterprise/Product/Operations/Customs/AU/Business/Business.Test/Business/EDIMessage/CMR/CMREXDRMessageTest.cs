using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMREXDRMessage))]
	sealed class CMREXDRMessageTest : CMRCUSRESMessageTest
	{
		public void TestLoadingRightDeclarationForCountry()
		{
			var nZCompany = Factory.New<GlbCompany>();
			var nZBranch = nZCompany.Branches.AddNew();

			var nZDeclaration = JobDeclaration.New(Factory);
			nZDeclaration.JE_DeclarationReference = "B00111733";
			nZDeclaration.JE_GB = nZBranch.PK;

			var aUDeclaration = JobDeclaration.New(Factory);
			aUDeclaration.JE_DeclarationReference = "B00111733";
			aUDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;

			var testMessage = Factory.New<CMREXDRMessageTestClass>();
			testMessage.EM_MessageText = sampleEXDR;
			var @object = testMessage.GetWrappedBizoExposed();

			AssertEquals("AU Declaration", true, aUDeclaration.PK == @object.PK);
		}

		public void TestGetWrappedObject()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B00111733";
			var message = Factory.New<CMREXDRMessageTestClass>();
			message.EM_MessageText = sampleEXDR;
			AssertEquals("Link with JobDeclaration", declaration, message.GetWrappedBizoExposed());

			declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("Link with CusEntryHeader", declaration.ActiveEntryHeaders[0], message.GetWrappedBizoExposed());
		}

		public void TestSendersReference()
		{
			AssertEquals("B00111733", Message.SendersReference);
		}

		public void TestGetErrorsArrayList()
		{
			AssertEquals(1, RejectionMessage.GetErrorsArrayList().Count);
		}

		public void TestGetStatus()
		{
			AssertEquals("CLEAR", Message.GetStatus());
		}

		public void TestGetStatusDescription()
		{
			AssertEquals("THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.", Message.GetStatusDescription());
		}

		CMREXDRMessage fMessage;
		CMREXDRMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<CMREXDRMessage>();
					fMessage.EM_MessageText = sampleEXDR;
				}
				return fMessage;
			}
		}

		CMREXDRMessage fRejectionMessage;
		CMREXDRMessage RejectionMessage
		{
			get
			{
				if (fRejectionMessage == null)
				{
					fRejectionMessage = Factory.New<CMREXDRMessage>();
					fRejectionMessage.EM_MessageText = rejectionEXDR;
				}
				return fRejectionMessage;
			}
		}

		readonly string sampleEXDR = "UNH+000000001+CUSRES:D:99B:UN'BGM+961:::EXDR+271I 2AHC 8DA4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:B00111733::001'RFF+ACW:EXD'RFF+AFM:9'RFF+ED:AAAAAJJN7'CNT+5:0001'UNT+10+000000001'";
		readonly string rejectionEXDR = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::EXDR+17IE 3JJ7 H51D:001+11'FTX+AHN+++REJECTED:THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.'NAD+MR+41065894724::95'RFF+ABO:B00111733::001'RFF+ACW:EXD'RFF+AFM:9'ERP+1'ERC+XD0085::95'FTX+AAO+++ATTEMPTED TO LODGE A DOCUMENT WHERE ONE ALREADY EXISTS WITH THE SAME MESSAGE OWNER SITE ID, MESSAGE TYPE AND SENDERS REFERENCE.'CNT+55:01'UNT+11+000001'";
	}
}
