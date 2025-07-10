using System;
using System.Reflection;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS316Processor))]
	sealed class TS316ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS316Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS316Provider>
	{
		[TestDate(2024, 5, 13)]
		public void TestUpdateLRN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew((ZString)OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIE001", Core.Constants.CountryCodes.Ireland);
			Company.GC_OH_OrgProxy = org.PK;
			var testItems = CreateSetupData();
			var temporaryStorageHeader = testItems.declaration;

			typeof(TemporaryStorageHeader).GetMethod("OnFactorySaving", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(temporaryStorageHeader, null);
			AssertEquals("To make sure LRN is updated after message processing.", "TSDATBIE24000000001V01", temporaryStorageHeader.LRN);

			using (temporaryStorageHeader.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(testItems.incomingMessage);
				processor.ProcessMessage(testItems.incomingMessage);
				Factory.Save();
				AssertEquals("Should have updated LRN.", "TSDATBIE24000000001V02", temporaryStorageHeader.LRN);
			}
		}

		protected override void AssertProcessResultCore(TemporaryStorageHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("AMA_MessageStatus", LogicalStatusList.Codes.Invalid, messageAttachee.AMA_MessageStatus);
			AssertEquals("CustomsStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"A TSD Rejection (TS316) message has been received for Job MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""2"">TS316 – Temporary Storage Declaration Rejection message</td></tr><tr><td>LRN</td><td>LRN123</td></tr><tr><td>Declaration Rejection Date</td><td>05-Sep-23</td></tr><tr><td>Declaration Rejection Reason</td><td>reason</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail("TS316: Temporary Storage Declaration Rejection",
				new[] { "A TSD Rejection (TS316) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS316;

		protected override ZString MessageFriendlyName => "TS316 - Temporary Storage Declaration Invalidation Decision";

		protected override TS316Processor Processor => new TS316Processor(logger, typeof(Ts316));

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS316Text("LRN123", new DateTime(2023, 9, 5, 12, 30, 30));
	}
}
