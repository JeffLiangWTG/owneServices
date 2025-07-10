using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.GVMS.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GVMS.ServiceTasks.Testing
{
	[TestedType(typeof(GVMSMessageRetrieverServiceTask))]
	public class GVMSMessageRetrieverServiceTaskTest : ServiceTaskTestCase<GVMSMessageRetrieverServiceTask>
	{
		public void TestParseOneRealExample()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			var entryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.UnitedKingdom);
			entryNumber.CE_EntryNum = "GMRO0000F2KW";

			var ediMessage = Factory.New<GVMSEDIMessage>();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsGVMSManifest;
			ediMessage.EM_MessageText = ResponseJson;

			Factory.Save();

			InitialiseAndRunTaskSchedule(new GVMSMessageRetrieverServiceTask());

			var message = new BusinessObjectFactory().Load<EDIMessage>(ediMessage.PK);

			AssertEquals(manifestHeader.PK, message.EM_LinkUniqueID);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestParseRealExampleForResponseBodyJson()
		{
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
			var interchange = $@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""GVMS"">
    <NotificationBoxId>4979BB8FB13A44209F6A64452397B657</NotificationBoxId>
    <MessageId>c68a44426336439f8dda5d729fff775c</MessageId>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""JSON"" Encoding=""base64"">{Convert.ToBase64String(Encoding.UTF8.GetBytes(ResponseJson))}</s0:ResponseBody>
  </s0:GBCustomsBusinessResponse>";
			ZDateTime localCreatedDate = Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 09, 11, 10, 58, 12, DateTimeKind.Utc));
			ZDateTime localUpdatedDate = Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 09, 24, 04, 23, 50, DateTimeKind.Utc));

			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
				"<H3>GVMS Response</H3><p>" +
				"<strong>GMR ID:</strong>GMRO0000F2KW<br/>" +
				"<strong>Status:</strong> - <br/>" +
				"<strong>Created Date/Time:</strong>" + localCreatedDate + "<br/>" +
				"<strong>Updated Date/Time:</strong>" + localUpdatedDate + "<br/>" +
				"<strong>Version:</strong>3<br/>" +
				"<strong>Inspection(s) Details:</strong>" +
				"<ul style=\"margin-top: 3px;\">" +
				"<li>By CUSTOMS at Sevington (1, L0029A)</li>" +
				"<li>By CUSTOMS at Stop 24 (1, L0030A)</li>" +
				"<li>By CUSTOMS at Dover Western Docks (1, L0031A)</li>" +
				"<li>By DEFRA at Sevington (2, L0029A)</li>" +
				"<li>By DEFRA at Dover Western Docks (2, L0031A)</li>" +
				"</ul>";
			TestRealWorldExample(interchange, expectedInterpretation);
			AssertContains("A newly-spawned received message already has a pretty HTML message interpretation", "<H3>GVMS Response</H3><p>", expectedInterpretation);
		}

		const string ResponseJson = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""state"": ""CHECKED IN"",
  ""inspectionRequired"": true,
    ""reportToLocations"": [{
      ""inspectionTypeId"": ""1"",
      ""locationIds"": [
        ""L0029A"", ""L0030A"", ""L0031A""
      ]
    }, {
      ""inspectionTypeId"": ""2"",
      ""locationIds"": [
        ""L0029A"", ""L0031A""
      ]
    }
  ]
}
";

		void TestRealWorldExample(ZString interchangeText, ZString expectedMessageInterpretation)
		{
			var asycudaManifest = Factory.New<AsycudaManifestHeader>();

			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";

			var outgoingMessage = (GVMSEDIMessage)asycudaManifest.Messages.AddNew(typeof(GVMSEDIMessage));
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ApplicationReference = "c68a44426336439f8dda5d729fff775c";
			outgoingMessage.EM_LinkedObject = asycudaManifest;
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			Factory.Save();

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbCustomsGVMSManifest;
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = interchangeText;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new GVMSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsGVMSManifest);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange.PK });
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = Factory.Load<EDIMessage>(query);

			var originalMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);

			CombineAssertions(() =>
			{
				AssertEquals(1, messages.Length);
				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, "", asycudaManifest.PK, "c68a44426336439f8dda5d729fff775c");
				AssertEquals(expectedMessageInterpretation, messages[0].EM_MessageInterpretation);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs GVMS messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsGVMSManifest),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"UK Customs GVMS interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsGVMSManifest),
				};
			}
		}

		static void AssertMessage(EDIMessage message, ZString messageStatus, ZString expectedMessageNum, ZGuid expectedLinkedID, ZString expectedApplicationReference, string expectedLinkTable = AsycudaManifestHeaderSchema.Constants.TableName)
		{
			AssertEquals("Message.EM_Status", messageStatus, message.EM_Status);
			AssertEquals("Message.EM_MessageNum", expectedMessageNum, message.EM_MessageNum);
			AssertEquals("Message.EM_LinkTable", expectedLinkTable, message.EM_LinkTable);
			AssertEquals("Message.EM_LinkUniqueID", expectedLinkedID, message.EM_LinkUniqueID);
			AssertEquals("Message ID Stored", expectedApplicationReference, message.EM_ApplicationReference);
		}
	}
}
