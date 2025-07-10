using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSDISQueryMessage))]
	class CDSDISQueryMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaults()
		{
			AssertEquals(ApplicationCodeList.Codes.GbCDSDISQuery, queryMessage.EM_ApplicationCode);
			AssertEquals(Direction.Transmit, queryMessage.EM_ReceiveTransmit);
			AssertType(typeof(GbMessageNumberStrategy), queryMessage.MessageNumberStrategy);

			CombineAssertions("Default Query String Context", () =>
			{
				AssertEquals("ALL", queryMessage.DeclarationCategory);
				AssertEquals(ZDate.Today.AddDays(-7), queryMessage.DateFrom);
				AssertEquals(ZDate.Today, queryMessage.DateTo);
				AssertEquals("Uncleared", queryMessage.DeclarationStatus);
				AssertEquals(1, queryMessage.PageNumber);
				AssertEquals(new ZString($"c={queryMessage.DeclarationCategory}&s={queryMessage.DeclarationStatus}&f={CDSDISQueryHelper.FormatDate(queryMessage.DateFrom)}&t={CDSDISQueryHelper.FormatDate(queryMessage.DateTo)}&p={queryMessage.PageNumber}"), queryMessage.EM_ApplicationReference);
			});

			Factory.Save();

			AssertEquals("Expecting 1...", "1", queryMessage.EM_MessageNum);
		}

		public void TestUniversalCopy()
		{
			var copyManager = new ZArchitecture.Business.UniversalCopy.BusinessObjectCopyManager(Factory);

			queryMessage.EM_MessageText = "Sample message to be copied";
			queryMessage.EM_MessageOwner = "ABC";
			queryMessage.EM_MessageType = "QWE";
			queryMessage.EM_MessageSubType = "RTY";

			Factory.Save();

			CombineAssertions("Msg1 Pre-check", () =>
			{
				AssertEquals(ApplicationCodeList.Codes.GbCDSDISQuery, queryMessage.EM_ApplicationCode);
				AssertEquals("Expecting 1...", "1", queryMessage.EM_MessageNum);
			});

			var copyResult = copyManager.Copy(queryMessage, new CargoWise.UniversalCopy.CopyTemplateTree(typeof(CDSDISQueryMessage)));
			var msg2 = copyResult.Object as CDSDISQueryMessage;

			AssertNotNull(msg2);
			CombineAssertions("Same / Copied", () =>
			{
				AssertEquals("ApplicationCode", queryMessage.EM_ApplicationCode, msg2.EM_ApplicationCode);
				AssertEquals("GB", queryMessage.EM_GB, msg2.EM_GB);
				AssertEquals("MessageOwner", queryMessage.EM_MessageOwner, msg2.EM_MessageOwner);
				AssertEquals("ReceiveTransmit", queryMessage.EM_ReceiveTransmit, msg2.EM_ReceiveTransmit);
				AssertEquals("MessageNumberStrategy", queryMessage.MessageNumberStrategy.GetType(), msg2.MessageNumberStrategy.GetType());
			});

			CombineAssertions("Different / Skipped", () =>
			{
				AssertEquals("MessageText", ZString.Empty, msg2.EM_MessageText);
				AssertEquals("MessageType", ZString.Empty, msg2.EM_MessageType);
				AssertEquals("MessageSubType", ZString.Empty, msg2.EM_MessageSubType);
				AssertNotEquals("MessageNum", queryMessage.EM_MessageNum, msg2.EM_MessageNum);
				AssertNotEquals("SystemCreateTimeUtc", queryMessage.EM_SystemCreateTimeUtc, msg2.EM_SystemCreateTimeUtc);
			});
		}

		public void TestReadOnly()
		{
			AssertEquals("Should not be readonly", expected: false, queryMessage.ReadOnly);
			Factory.Save();
			AssertEquals("Should now be readonly", expected: true, queryMessage.ReadOnly);
		}

		public void TestValidationType()
		{
			AssertType(typeof(CDSDISQueryMessageValidation), queryMessage.Validation);
		}

		public void TestLookupType()
		{
			AssertType(typeof(CDSDISQueryMessageLookups), queryMessage.Lookups);
		}

		public void TestGlbExternalPasswordReturnsSingleRecord()
		{
			AssertEquals("Expect Empty", ZString.Empty, queryMessage.EM_MessageOwner);
			AssertNull("Expect NULL", queryMessage.GlbExternalPassword);

			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "ABC", "12345678901234", PasswordTypesList.Codes.CDS);
			queryMessage = Factory.New<CDSDISQueryMessage>();
			AssertEquals("Expecting data", "12345678901234.ABC", queryMessage.EM_MessageOwner);
			AssertNotNull(queryMessage.GlbExternalPassword);
			AssertEquals("12345678901234.ABC", queryMessage.GlbExternalPassword.GP_UserID);
		}

		public void TestDefaultProfile()
		{
			AssertEquals("Expect Empty", ZString.Empty, queryMessage.EM_MessageOwner);

			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "ABC", "12345678901234", PasswordTypesList.Codes.CDS);
			queryMessage = Factory.New<CDSDISQueryMessage>();
			AssertEquals("Expecting data", "12345678901234.ABC", queryMessage.EM_MessageOwner);

			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "DEF", "12345678901234", PasswordTypesList.Codes.CDS);
			queryMessage = Factory.New<CDSDISQueryMessage>();
			AssertEquals("Expect Empty", ZString.Empty, queryMessage.EM_MessageOwner);
		}

		public void TestResponseInterpretation()
		{
			AssertEquals("Not saved = no message", string.Empty, queryMessage.ResponseInterpretation);

			Factory.Save();
			AssertEquals("Saved, not sent", "<html><body style='font-family: arial;'><p>The request has not been sent.</p></body></html>", queryMessage.ResponseInterpretation);

			queryMessage.EM_Status = EDIMessage.Status.Failed;
			AssertEquals("Sent, no response", "<html><body style='font-family: arial;'><p>The request was not delivered successfully, please refer to the Logs tab for more information, in particular the detail of any event with code MRJ.</p></body></html>", queryMessage.ResponseInterpretation);

			queryMessage.EM_Status = EDIMessage.Status.Sent;
			AssertEquals("Sent, no response", "<html><body style='font-family: arial;'><p>No response message was found for this request.</p></body></html>", queryMessage.ResponseInterpretation);

			queryMessage.EM_Status = EDIMessage.Status.Acknowledged;
			AssertEquals("Sent, no response", "<html><body style='font-family: arial;'><p>Query has been acknowledged, but no response has been received.</p></body></html>", queryMessage.ResponseInterpretation);

			var response = Factory.New<CDSDeclarationInfoResponseEDIMessage>();
			response.EM_MessageText = CDSDeclarationInfoResponseEDIMessage.Serialize(CDSDeclarationInfoResponseTests.CDSDeclarationInfoResponseXMLForTest);

			response.EM_LinkTable = EDIMessage.Schema.TableName;
			response.EM_LinkUniqueID = queryMessage.PK;

			AssertEquals("Got a response", CDSDeclarationInfoResponseTests.ExpectedHTMLInterpretation, queryMessage.ResponseInterpretation);
		}

		public void TestMessageTextCreatedOnSave()
		{
			var msg = Factory.New<CDSDISQueryMessage>();
			msg.EM_MessageOwner = "12345678901234.XYZ";

			AssertEquals("No msg txt yet", ZString.Empty, msg.EM_MessageText);
			Factory.Save();

			var txt = msg.EM_MessageText;
			Assert("Shoud Not be empty", !txt.IsEmpty);
			AssertContains("Should be universal event", "<UniversalEvent", txt);
			AssertContains("Should contain profile", $"<Value>{GBExtensions.GetEnterpriseCode()}.12345678901234.XYZ</Value>", txt);
		}

		public void TestDisplayReference()
		{
			queryMessage.EM_MessageOwner = "12345678901234.XYZ";
			queryMessage.EM_MessageNum = "911";
			AssertEquals("DisplayRef", "DQ:12345678901234.XYZ #911", queryMessage.DisplayReference);
		}

		public void TestEM_ApplicationReference()
		{
			queryMessage.EM_MessageOwner = "AR1";
			queryMessage.DeclarationCategory = "EX";
			queryMessage.DateFrom = ZDate.BrettsBirthday;
			queryMessage.DateTo = ZDate.Today;
			queryMessage.DeclarationStatus = "Rejected";
			queryMessage.PageNumber = 5;

			CombineAssertions("set query string context values", () =>
			{
				AssertEquals("EX", queryMessage.DeclarationCategory);
				AssertEquals(ZDate.BrettsBirthday, queryMessage.DateFrom);
				AssertEquals(ZDate.Today, queryMessage.DateTo);
				AssertEquals("Rejected", queryMessage.DeclarationStatus);
				AssertEquals(5, queryMessage.PageNumber);
			});

			Factory.Save();
			AssertEquals(new ZString($"c=EX&s=Rejected&f={CDSDISQueryHelper.FormatDate(ZDate.BrettsBirthday)}&t={CDSDISQueryHelper.FormatDate(ZDate.Today)}&p=5"), queryMessage.EM_ApplicationReference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			queryMessage = Factory.New<CDSDISQueryMessage>();
		}

		CDSDISQueryMessage queryMessage;
	}
}
