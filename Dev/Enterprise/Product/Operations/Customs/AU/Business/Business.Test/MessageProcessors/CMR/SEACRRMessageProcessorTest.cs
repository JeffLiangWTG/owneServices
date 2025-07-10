using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEACRRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestReportEmail()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBLNO";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_BGMReference = "HBLNO";

			CMRSEACRRMessage message = Factory.New<CMRSEACRRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			message.EM_LinkedObject = houseBill;
			TestHelperSEACRRMessageProcessor processor = new TestHelperSEACRRMessageProcessor(logger);
			EmailDef reportEmail = processor.GetReportfForTest(message);
			AssertContains("Email Report Text", @"<strong>
OCEAN BILL DETAILS:<br>Ocean Bill: OBLNO<br>Discharge Port: AUBNE<br><br>HOUSE BILL DETAILS:<br>Message Reference: HBLNO<br>Destination: AUBNE<br><br>Status: ORIGINAL ACCEPTED<br>Status Description: THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS<br>
<br />
</strong>A Sea Cargo Report Response(SEACRR) message has been received from the ACS.<br />
<br />
<!--DynamicHtml1-->
<br />
<!--DynamicHtml2-->
<br />
<!--DynamicHtml3-->
<hr />
<br />
<strong><large>
</strong></large>
Regards,<br />", reportEmail.Body);
		}

		public void TestNoResponseWhenAccepted()
		{
			var bill = Factory.New<CusSCAOceanBill>();
			bill.CB_MessageReference = "S00002158";

			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			var message = bill.Messages.AddNew(typeof(CMRSEACRRMessage));
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			var processor = new TestHelperSEACRRMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Accecpted.", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals(1, processor.AcknowledgementEmailSendCount);
				AssertEquals(0, processor.ErrorEmailSendCount);
			});
		}

		public void TestHVLVRegistrySettings()
		{
			var cusSCAHouse = Factory.New<CusSCAHouse>();
			incomingMessage.EM_LinkedObject = cusSCAHouse;

			var processor = new TestHelperSEACRRMessageProcessor(logger);
			processor.ProcessMessage(incomingMessage);

			cusSCAHouse.CA_IsHVLV = false;
			Env.Registry.AUCustoms.SeaCargoSendErrors = Core.Constants.EmailTo.NoEmails;
			AssertEquals("It should return normal settings if the BizO is not HVLV.", Core.Constants.EmailTo.NoEmails, processor.ErrorEmailMode);
			Env.Registry.AUCustoms.HVLVSeaCargoSendErrors = Core.Constants.EmailTo.StaffMember;
			AssertEquals("It should return normal settings if the BizO is not HVLV even if the HVLV registry item is assgined.", Core.Constants.EmailTo.NoEmails, processor.ErrorEmailMode);
			cusSCAHouse.CA_IsHVLV = true;
			AssertEquals("It should return HVLV settings if the BizO is HVLV.", Core.Constants.EmailTo.StaffMember, processor.ErrorEmailMode);

			cusSCAHouse.CA_IsHVLV = false;
			Env.Registry.AUCustoms.SeaCargoSendAcknowledgements = Core.Constants.EmailTo.NoEmails;
			AssertEquals("It should return normal settings if the BizO is not HVLV.", Core.Constants.EmailTo.NoEmails, processor.AcknowledgementEmailMode);
			Env.Registry.AUCustoms.HVLVSeaCargoSendAcknowledgements = Core.Constants.EmailTo.StaffMember;
			AssertEquals("It should return normal settings if the BizO is not HVLV even if the HVLV registry item is assgined.", Core.Constants.EmailTo.NoEmails, processor.AcknowledgementEmailMode);
			cusSCAHouse.CA_IsHVLV = true;
			AssertEquals("It should return HVLV settings if the BizO is HVLV.", Core.Constants.EmailTo.StaffMember, processor.AcknowledgementEmailMode);

			cusSCAHouse.CA_IsHVLV = false;
			Env.Registry.AUCustoms.SeaCargoSendImpediments = Core.Constants.EmailTo.NoEmails;
			AssertEquals("It should return normal settings if the BizO is not HVLV.", Core.Constants.EmailTo.NoEmails, processor.ImpedimentEmailMode);
			Env.Registry.AUCustoms.HVLVSeaCargoSendImpediments = Core.Constants.EmailTo.StaffMember;
			AssertEquals("It should return normal settings if the BizO is not HVLV even if the HVLV registry item is assgined.", Core.Constants.EmailTo.NoEmails, processor.ImpedimentEmailMode);
			cusSCAHouse.CA_IsHVLV = true;
			AssertEquals("It should return HVLV settings if the BizO is HVLV.", Core.Constants.EmailTo.StaffMember, processor.ImpedimentEmailMode);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.SEACR;

		protected override ZString GetExpectedMessageName() => "Sea Cargo Report Response(SEACRR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new SEACRRMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRSEACRRMessage);

		sealed class TestHelperSEACRRMessageProcessor : SEACRRMessageProcessor
		{
			public TestHelperSEACRRMessageProcessor(LoggingInformation logger) : base(logger) { }

			public EmailDef GetReportfForTest(CMRCUSRESMessage message)
			{
				incomingMessage = message;
				return GetReport();
			}

			public new ZString AcknowledgementEmailMode => base.AcknowledgementEmailMode;
			public new ZString ImpedimentEmailMode => base.ImpedimentEmailMode;
			public new ZString ErrorEmailMode => base.ErrorEmailMode;
		}
	}
}
