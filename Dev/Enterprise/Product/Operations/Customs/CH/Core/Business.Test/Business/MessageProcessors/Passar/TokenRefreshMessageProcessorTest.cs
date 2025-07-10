using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TokenRefreshMessageProcessor))]
sealed class TokenRefreshMessageProcessorTest : TestCaseWithFactory
{
	ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new TokenRefreshMessageProcessor(logger);

	LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestMessageLinkedObject()
	{
		CombineAssertions(() =>
		{
			var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.TRE, MessageSubTypeCodeList.Codes.Accepted, TokenJsonObject);

			var processor = GetMessageProcessor(Logger);
			processor.ProcessMessage(ediMessage);

			AssertEquals("Status", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("Linked Table", GlbCompany.Schema.TableName, ediMessage.EM_LinkTable);
			AssertEquals("Linked Entry Header", company.PK, ediMessage.EM_LinkUniqueID);
		});
	}

	public void TestEventSucceed() => TestEvent(TokenJsonObject, MessageSubTypeCodeList.Codes.Accepted, "Succeded");
	public void TestEventFailure() => TestEvent(ErrorJsonObject, MessageSubTypeCodeList.Codes.CustomsRejected, "Failed|Client Authentication failed.|InterchangeNum=1");

	public void TestEvent(string bodyTest, string messageSubType, string expectedEventReference)
	{
		CombineAssertions(() =>
		{
			var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.TRE, messageSubType, bodyTest);

			CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
			var processor = GetMessageProcessor(Logger);
			processor.ProcessMessage(ediMessage);

			var logEvent = company.Logs.MostRecentLogByEventTime(Events.CommunicationTokensRefresh);

			AssertNotNull("Event written", logEvent);
			AssertEquals("Event Reference", expectedEventReference, logEvent?.SL_Reference);
		});
	}

	public void TestProcessTokenResponse()
	{
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.TRE, MessageSubTypeCodeList.Codes.Accepted, TokenJsonObject);

		var processor = GetMessageProcessor(Logger);
		processor.ProcessMessage(ediMessage);

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
		var tokenCredentials = companyWrapper.TokenCredentials;

		CombineAssertions(() =>
		{
			AssertEquals("GP_Certificate", "access token value", Encoding.UTF8.GetString(tokenCredentials.GP_Certificate));
			AssertEquals("Token Refresh", "refresh token value", tokenCredentials.RefreshTokenText);
			AssertEquals("GP_ExpireDate", true, (ZDateTime.UtcNow.AddSeconds(86400) - tokenCredentials.GP_ExpiryDate).Seconds < 1);
			AssertEquals("GP_PasswordStatus", "RCV", tokenCredentials.GP_PasswordStatus);
		});
	}

	public void TestProcessTokenResponseError()
	{
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.TRE, MessageSubTypeCodeList.Codes.CustomsRejected, ErrorJsonObject);

		var processor = GetMessageProcessor(Logger);
		processor.ProcessMessage(ediMessage);

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
		var tokenCredentials = companyWrapper.TokenCredentials;

		AssertEquals("GP_PasswordStatus", "RCV", tokenCredentials.GP_PasswordStatus);
	}

	public void TestErrorNotificationResponse()
	{
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.TRE, MessageSubTypeCodeList.Codes.Rejected, ErrorNotificationMessage);

		var processor = GetMessageProcessor(Logger);
		processor.ProcessMessage(ediMessage);

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
		var tokenCredentials = companyWrapper.TokenCredentials;

		AssertEquals("GP_PasswordStatus", "RCV", tokenCredentials.GP_PasswordStatus);

		var logEvent = company.Logs.MostRecentLogByEventTime(Events.CommunicationTokensRefresh);
		AssertNull("Event not written", logEvent);
	}

	const string TokenJsonObject = @"{""access_token"":""access token value"",""refresh_token"":""refresh token value"",""scope"":""default"",""token_type"":""Bearer"",""expires_in"":86400}";
	const string ErrorJsonObject = @"{""error_description"":""Client Authentication failed."",""error"":""invalid_client""}";

	const string ErrorNotificationMessage = $@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>CHCustomsPassar</SenderID>
		<RecipientID>HYESASCM2</RecipientID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<EventTime>2024-01-08 06:24:58.545</EventTime>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Reason>Error Message
Contract: xt-contract:/Customs/CH Passar/CH Passar Processing Contract
Reference Object: xt-procstep:{{635713fb-1a37-4054-ba9a-cd2ecaff1f11}}

Error description: Processing step execution error: Exception from LoadConfig: Could not load file or assembly 'Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed' or one of its dependencies. Access is denied.
</Reason>
					<MessageType>XER</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>OriginalMessage</Type>
						<Value><![CDATA[<messages />]]></Value>
					</Context>
					<Context>
						<Type>OriginalAttributes</Type>
						<Value><![CDATA[sequuidin : 00000000-0000-0000-0000-000000000000
custom.CH.accessToken : eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik9UZ3hNek5tWWpRM01qY3pNV0ZrWldFMllqWTNPVGhsWm1ZMVpUYzJNalV5TmpobU1EUmhNQSIsImtpZCI6Ik9UZ3hNek5tWWpRM01qY3pNV0ZrWldFMllqWTNPVGhsWm1ZMVpUYzJNalV5TmpobU1EUmhNQV9SUzI1NiJ9.eyJzdWIiOiJhYm5fcGFtc18xMDE3NzcwMDc3QGNhcmJvbi5zdXBlciIsImJhY2tlbmRKd3QiOiJleUowZVhBaU9pSktWMVFpTENKaGJHY2lPaUpTVXpJMU5pSXNJbmcxZENJNklrOVVaM2hOZWs1dFdXcFJNMDFxWTNwTlYwWnJXbGRGTWxscVdUTlBWR2hzV20xWk1WcFVZekpOYWxWNVRtcG9iVTFFVW1oTlFTSXNJbXRwWkNJNklrOVVaM2hOZWs1dFdXcFJNMDFxWTNwTlYwWnJXbGRGTWxscVdUTlBWR2hzV20xWk1WcFVZekpOYWxWNVRtcG9iVTFFVW1oTlFWOVNVekkxTmlKOS5leUpoZFdRaU9pSm9kSFJ3T2x3dlhDOXZjbWN1ZDNOdk1pNWhjR2x0WjNSY0wyZGhkR1YzWVhraUxDSnpkV0lpT2lKaFltNWZjR0Z0YzE4eE1ERTNOemN3TURjM0lpd2lZbkJ5YjJ4bGN5STZleUl4TURFM056Y3dNRGMzSWpwYklpSmRmU3dpYm1KbUlqb3dMQ0pqZEhnaU9pSkNNa0lpTENKcGMzTWlPaUpvZEhSd2N6cGNMMXd2YTJWNWJXRnVZV2RsY2k1aGNHa3VZV1J0YVc0dVkyaGNMM1J2YTJWdUlpd2laWGh3SWpveE56QTBOek0xTlRrM0xDSnBZWFFpT2pFM01EUTJORGt4T1Rjc0ltcDBhU0k2SWprMk5EaGpNelV3TFdaa1lqQXRORFZpT0MxaE9XWmxMVGsxTjJWak1HUmhaRGd4T0NKOS5OU3R3VFhQRC1NdlNBdGZsV2g1VFlITzJ5QmxsNUNMVnU5bmZYQ0xhV0Y3blhlcXM5N0RJUVd0bTZPV2dBNkRmLTAzSXJiRVZaZ3hISlQ5cUJZT013RU92ZnN2T3RrUC1nVXNka0x3Q1N6TEpVV1lSbTRMVW5RTnhHMm0tNG9nb2o2VVR0RlVVc2lqSE9UaWttSzFMU3VvU0xEV0YzSEtLbGIwRUkyZjd0MURnQ3hyeEdoT2ZLelVvRmFtU3hWbG5SVEYzNHc0N3ZkZzUybHVqeUZDZWNIVGJtbmpWM0VmaEE5UDRkNHhmaE9QNzAwcTVndkV1TTB5NEpla0RsY05mNDJOd2ktTDdCbDlsOE9RcXZDV0RIVmNvb1hGZVdtbHJyMjgwUWNBTG9Bck9ETHdPNF9TY0pnRkR3dFJxYl83d1RnS3VTUktJQzJDM2dkNTlXYnkzYmciLCJpc3MiOiJodHRwczpcL1wva2V5bWFuYWdlci5hcGkuYWRtaW4uY2hcL3Rva2VuIiwidGllckluZm8iOnsiVW5saW1pdGVkIjp7InN0b3BPblF1b3RhUmVhY2giOnRydWUsInNwaWtlQXJyZXN0TGltaXQiOjAsInNwaWtlQXJyZXN0VW5pdCI6bnVsbH19LCJrZXl0eXBlIjoiUFJPRFVDVElPTiIsInN1YnNjcmliZWRBUElzIjpbeyJzdWJzY3JpYmVyVGVuYW50RG9tYWluIjoiY2FyYm9uLnN1cGVyIiwibmFtZSI6InBhc3Nhci1iMmJodWJfYWJuX3Bhc3Nhcl9kZWNsYXJhdGlvbiIsImNvbnRleHQiOiJcL3Bhc3Nhci1iMmJodWItYWJuXC9hcGlcL3YyIiwicHVibGlzaGVyIjoicGFzc2FyLWIyYmh1YiIsInZlcnNpb24iOiJ2MiIsInN1YnNjcmlwdGlvblRpZXIiOiJVbmxpbWl0ZWQifSx7InN1YnNjcmliZXJUZW5hbnREb21haW4iOiJjYXJib24uc3VwZXIiLCJuYW1lIjoiY2hvdXQtYjJiaHViX2Fibl9jaGFydGVyYW91dHB1dF9kb2N1bWVudHMiLCJjb250ZXh0IjoiXC9jaG91dC1iMmJodWItYWJuXC9hcGlcL3YyIiwicHVibGlzaGVyIjoiY2hvdXQtYjJiaHViIiwidmVyc2lvbiI6InYyIiwic3Vic2NyaXB0aW9uVGllciI6IlVubGltaXRlZCJ9XSwiYXVkIjoiaHR0cDpcL1wvb3JnLndzbzIuYXBpbWd0XC9nYXRld2F5IiwiYXBwbGljYXRpb24iOnsib3duZXIiOiJlenZiMmJndyIsInRpZXIiOiJVbmxpbWl0ZWQiLCJuYW1lIjoiQ1cxX0NNMl9TQVMtMTAxNzc3MDA3NyIsImlkIjoyODcwLCJ1dWlkIjpudWxsfSwic2NvcGUiOiJkZWZhdWx0IiwiY29uc3VtZXJLZXkiOiJVSDlRNkVtOE82RHZ5YlJtYUdfbXJndHhnVXdhIiwiZXhwIjoxNzA0NzM1NTk3LCJpYXQiOjE3MDQ2NDkxOTcsImp0aSI6IjlhZjgyZjg2LTllZjgtNGU0Mi1hNDg5LTI4OTUyYWIxYjhjNCJ9.YzS1_vo0cSR7Kx0fmuOJ_iqwtqJDUeYDTqbYHdHwncVj3DJQCIoTS8uxpnbvEl8r2ThXDg74pw2KLPyTsad-p3a_T_pvL0oXSXajBhZ0lXBXHe9S_VhV-xzSLfv4gZcO1Yv-Rf0ASDW2498gNseHfXVJikPODhVs60cOqAyFKUXkrFbT3FurbFHvEr0kh1nQInvFk9Q91onk25oOn0tUSYK8BldBa_sjwIAarraGP2a0_LuoFmfa2mWXVp5k3LTVITy6AuH3rX_B-pD66ZKQkcudu2zuJTPLblX8RKDzllYsaoOkmQXmHsiWX0STMHiUTvKG0-r6zoTDOlrZtIOkBg
msgid : 5969831
datasizein : 0
http.status-code : 200
contractobj : xt-contract:/Customs/CH Passar/CH Passar MsgList Contract
syncreply : 0
custom.MessageTrackingID : c4a08c11-fe10-4f2c-9792-94f534b5f858
custom.SourceParty : HYESASCM2
customprivate.X-Vcap-Request-Id : 34f332df-f1ef-4c56-59f6-dca0b92852a1
statename : ST_WAIT_REPLY
seqidin : 0
flags : 16
internalid : 1714
cfgversion : 833790
owner : 18446744073709551615
wfinst : 00000000-0000-0000-0000-000000000000
custom.CH.bpId : 1017770077
filenameout : 5969831
datahashin : d41d8cd98f00b204e9800998ecf8427e
filenamein : 
custom.CH.lastMessageId : 134a0b83-1eb0-45e1-b606-fcd8bc03b8a7
seqnoout : 0
custom.MessageType : MSL
toprot-name : xt-none
seqtypeout : 0
version : 5
customprivate.Access-Control-Allow-Methods : GET
msginfo : 
archiveflags : 0
toobj : xt-contract:/Customs/CH Passar/CH Passar Processing Contract
folderin : 
state : 1035
ackprot : 0
fromprot-name : xt-application
seqidout : 0
customprivate.Access-Control-Allow-Headers : 
syncflags : 256
customprivate.X-B3-Traceid : d78b0fe8aca14cf4ba14077c706965d3
fromprot : 903
seqtypein : 0
datasizeout : 11
knownversion : 0
toparty : 
toprot : 0
acktimeout : 0
priority : 1
msgtype : 1
fromparty : 
customprivate.Date : Mon, 08 Jan 2024 06:24:37 GMT
ackprot-name : xt-none
customprivate.Set-Cookie : cookiesession1=678B76CD1CC7866B20C55121C11CE75F;Expires=Tue, 07 Jan 2025 06:24:37 GMT;Path=/;HttpOnly
custom.ApplicationCode : CHP
customprivate.Connection : keep-alive
sequuidout : 00000000-0000-0000-0000-000000000000
custom.Url : https://abn-passar-b2bhub.ezv.admin.ch/api/v2/messages?lastMessageId=134a0b83-1eb0-45e1-b606-fcd8bc03b8a7
fromobj : xt-application:/Framework/Direct Endpoints/HYECM2
customprivate.Strict-Transport-Security : max-age=15552000
seqnoin : 0
curracklevel : 0
flagstext : MF_PROC
creationtime : 1704695089
acklevel : 0
msguuid : 887c15ab-0f04-4e11-820c-5a956aee4f15
reftocontract : 5969845
laststate : 1028
parseinfoin.std.sender : HYECM2
datahashout : 921e1365411677fea973eac958d877c6
custom.DestinationParty : CHCustomsPassar]]></Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>
";
}
