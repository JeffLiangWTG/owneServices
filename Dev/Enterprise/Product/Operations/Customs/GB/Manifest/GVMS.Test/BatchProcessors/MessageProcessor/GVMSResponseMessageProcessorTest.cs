using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.CDS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GVMSResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessByGMRID()
		{
			ProcessByGMRIDTest(MessageTextCheckedIn, "GMRO0000F2KW", GVMSCustomsStatus.Codes.CheckedIn);
			ProcessByGMRIDTest(MessageTextEmbarked, "GMRA00002KW2", GVMSCustomsStatus.Codes.Departed);
			ProcessByGMRIDTest(MessageTextNotFinalisable, "GMRA000002FK", GVMSCustomsStatus.Codes.AmendmentRequired);
			ProcessByGMRIDTest(MessageTextOpen, "GMRO0000F2VC", GVMSCustomsStatus.Codes.Open);
			ProcessByGMRIDTest(MessageTextFinalised, "GMRDOQORBMZF", GVMSCustomsStatus.Codes.Finalised);
		}

		void ProcessByGMRIDTest(ZString messageText, ZString gmrID, ZString expectedStatus)
		{
			var manifestHeader = CreateManifestHeader();
			var entryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.UnitedKingdom);
			entryNumber.CE_EntryNum = gmrID;

			var ediMessage = Factory.New<GVMSEDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = messageText;
			Factory.Save();

			var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			AssertEquals(expectedStatus, manifestHeader.RegistrationStatus);
			AssertEquals(true, manifestHeader.InspectionRequired);
			AssertInspectionLocations(manifestHeader.InspectionLocations);
			AssertEquals(new DateTime(2021, 8, 11, 10, 58, 12), TimeZoneInfo.ConvertTimeToUtc(manifestHeader.RegistrationDate.ToDateTime()));
			AssertEquals(manifestHeader, ediMessage.EM_LinkedObject);
		}

		public void TestProcessByMessageID()
		{
			var manifestHeader = CreateManifestHeader();
			var outgoingMessage = Factory.New<GVMSEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "c68a44426336439f8dda5d729fff775c";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = manifestHeader;

			var interchange = Factory.New<CDSInterchange>();
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""GVMS"">
    <NotificationBoxId>4979BB8FB13A44209F6A64452397B657</NotificationBoxId>
    <MessageId>c68a44426336439f8dda5d729fff775c</MessageId>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""JSON"" Encoding=""base64"">
    VEVTVA==
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>").Xml;

			var responseMessage = (GVMSEDIMessage)interchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""gmrState"": ""CHECKED_IN"",
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
			Factory.Save();

			var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals("GMRO0000F2KW", manifestHeader.RegistrationNumber);
			AssertEquals(GVMSCustomsStatus.Codes.CheckedIn, manifestHeader.RegistrationStatus);
			AssertEquals(true, manifestHeader.InspectionRequired);
			AssertInspectionLocations(manifestHeader.InspectionLocations);
			AssertEquals(new DateTime(2021, 9, 11, 10, 58, 12), TimeZoneInfo.ConvertTimeToUtc(manifestHeader.RegistrationDate.ToDateTime()));
			AssertEquals(manifestHeader, responseMessage.EM_LinkedObject);

			var responseMessage2 = (GVMSEDIMessage)interchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW2"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""gmrState"": ""CHECKED_IN"",
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
			Factory.Save();
			var processor2 = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor2.ProcessMessage(responseMessage2);
			AssertEquals("GMRO0000F2KW2", manifestHeader.RegistrationNumber);
			AssertInspectionLocations(manifestHeader.InspectionLocations);

			var responseMessage3 = (GVMSEDIMessage)interchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			responseMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": """",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""gmrState"": ""CHECKED_IN"",
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
			Factory.Save();
			var processor3 = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor3.ProcessMessage(responseMessage3);
			AssertEquals("GMRO0000F2KW2", manifestHeader.RegistrationNumber);
			AssertInspectionLocations(manifestHeader.InspectionLocations);

			var responseMessage4 = (GVMSEDIMessage)interchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			responseMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4.EM_MessageText = @"{
  ""messageId"": ""e0e269cc-d76f-40c1-a6a3-b1c6d4ac5e9c"",
  ""gmrId"": ""GMRDOQORBMZF"",
  ""gmrStatusVersion"": 2,
  ""createdDateTime"": ""2022-07-28T10:07:18.594Z"",
  ""updatedDateTime"": ""2022-07-28T10:09:47.388Z"",
  ""gmrState"": ""FINALISED"",
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
			Factory.Save();
			var processor4 = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor4.ProcessMessage(responseMessage4);
			AssertEquals("GMRDOQORBMZF", manifestHeader.RegistrationNumber);
			AssertEquals(GVMSCustomsStatus.Codes.Finalised, manifestHeader.RegistrationStatus);
			AssertEquals(true, manifestHeader.InspectionRequired);
			AssertInspectionLocations(manifestHeader.InspectionLocations);
			AssertEquals(new DateTime(2022, 7, 28, 10, 07, 18), TimeZoneInfo.ConvertTimeToUtc(manifestHeader.RegistrationDate.ToDateTime()));

			var responseMessage5 = (GVMSEDIMessage)interchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			responseMessage5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage5.EM_MessageText = @"{
  ""messageId"": ""e0e269cc-d76f-40c1-a6a3-b1c6d4ac5e9c"",
  ""gmrId"": ""GMRDOQORBMZF"",
  ""gmrStatusVersion"": 2,
  ""createdDateTime"": ""2022-07-28T10:07:18.594Z"",
  ""updatedDateTime"": ""2022-07-28T10:09:47.388Z"",
  ""gmrState"": ""FINALISED"",
  ""inspectionRequired"": true,
  ""reportToLocations"": null
}
";
			Factory.Save();
			var processor5 = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor5.ProcessMessage(responseMessage5);
			AssertInspectionLocations(manifestHeader.InspectionLocations);
		}

		[TestDate(2021, 6, 30, 17, 30, 0)]
		public void TestProcessWhenHeaderIsNull()
		{
			var outgoingMessage = Factory.New<GVMSEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "c68a44426336439f8dda5d729fff775c";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var interchange = Factory.New<CDSInterchange>();
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""GVMS"">
    <NotificationBoxId>4979BB8FB13A44209F6A64452397B657</NotificationBoxId>
    <MessageId>c68a44426336439f8dda5d729fff775c</MessageId>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""JSON"" Encoding=""base64"">
    VEVTVA==
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>").Xml;

			var responseMessage = (GVMSEDIMessage)interchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageText = @"{
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
			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(responseMessage);
				AssertEquals(null, responseMessage.EM_LinkedObject);
				AssertEquals(1, interchange.EI_RetryCount);
				AssertEquals(new ZDateTime(2021, 6, 30, 17, 31, 0), responseMessage.EM_HeldUntilDate);
				AssertEquals(EDIMessageStatusList.Codes.Queued, responseMessage.EM_Status);

				for (int i = 0; i <= 10; i++)
				{
					processor.ProcessMessage(responseMessage);
				}
				AssertEquals(null, responseMessage.EM_LinkedObject);
				AssertEquals(11, interchange.EI_RetryCount);
				AssertEquals(new ZDateTime(2021, 6, 30, 17, 31, 0), responseMessage.EM_HeldUntilDate);
				AssertEquals(EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			}
		}

		public void TestExecuteGVMSManifestDocument()
		{
			var manifestHeader = CreateManifestHeader();
			manifestHeader.AMA_JobReference = "C1234";
			var entryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.UnitedKingdom);
			entryNumber.CE_EntryNum = "GMRO0000F2KW";

			var ediMessage = Factory.New<GVMSEDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""gmrState"": ""OPEN"",
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
			Factory.Save();

			var storageMain = manifestHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(manifestHeader, Core.Constants.DocManagerCodes.AsycudaManifest);
			AssertEquals(0, storageMain.Files.Count);

			var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			storageMain = manifestHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(manifestHeader, Core.Constants.DocManagerCodes.AsycudaManifest);
			AssertEquals(1, storageMain.Files.Count);
			AssertEquals("GB GVMS Manifest with Barcode - C1234.pdf", storageMain.Files[0].FileName);

			var ediMessage2 = Factory.New<GVMSEDIMessage>();
			ediMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage2.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""gmrState"": ""OPEN"",
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
			processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage2);

			storageMain = manifestHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(manifestHeader, Core.Constants.DocManagerCodes.AsycudaManifest);
			AssertEquals(1, storageMain.Files.Count);
			AssertEquals("GB GVMS Manifest with Barcode - C1234.pdf", storageMain.Files[0].FileName);

			var ediMessage3 = Factory.New<GVMSEDIMessage>();
			ediMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage3.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""gmrState"": ""CHECKED_IN"",
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
			processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage3);

			storageMain = manifestHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(manifestHeader, Core.Constants.DocManagerCodes.AsycudaManifest);
			AssertEquals(2, storageMain.Files.Count);
			AssertEquals("GB GVMS Manifest with Barcode - C1234.pdf", storageMain.Files[0].FileName);
			AssertEquals("GB GVMS Manifest with Barcode - C1234[2].pdf", storageMain.Files[1].FileName);
		}

		public void TestMessageWithNoInterchangeOrLinkedObject()
		{
			var responseMessage = Factory.New<GVMSEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageText = @"{
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
			Factory.Save();

			var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals(null, responseMessage.EM_LinkedObject);
			AssertEquals(EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
		}

		public void TestProcessMessageWithLinkedObject()
		{
			var manifestHeader = CreateManifestHeader();
			var responseMessage = Factory.New<GVMSEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_LinkedObject = manifestHeader;
			responseMessage.EM_MessageText = @"{
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
			Factory.Save();

			var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
		}

		public void TestProcessMessageWithLinkedObject_DiscardDodgyMessages()
		{
			var manifestHeader = CreateManifestHeader();
			var responseMessage = Factory.New<GVMSEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_LinkedObject = manifestHeader;
			responseMessage.EM_MessageText = "Sometimes an error message is all we get and it cannot be deserialised into our expected json";
			Factory.Save();

			var processor = new GVMSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals(EDIMessageStatusList.Codes.Discarded, responseMessage.EM_Status);
		}

		void AssertInspectionLocations(GvmsInspectionAtLocationCusCodeDataCollection inspectionLocations)
		{
			AssertEquals("Should only contains 5 locations", 5, inspectionLocations.Count);
			AssertEquals("1", inspectionLocations[0].CY_Code);
			AssertEquals("L0029A", inspectionLocations[0].CY_Data);
			AssertEquals("1", inspectionLocations[1].CY_Code);
			AssertEquals("L0030A", inspectionLocations[1].CY_Data);
			AssertEquals("1", inspectionLocations[2].CY_Code);
			AssertEquals("L0031A", inspectionLocations[2].CY_Data);
			AssertEquals("2", inspectionLocations[3].CY_Code);
			AssertEquals("L0029A", inspectionLocations[3].CY_Data);
			AssertEquals("2", inspectionLocations[4].CY_Code);
			AssertEquals("L0031A", inspectionLocations[4].CY_Data);
		}

		AsycudaManifestHeader CreateManifestHeader()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			return manifestHeader;
		}

		const string MessageTextCheckedIn = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-08-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-08-24T04:23:50.384Z"",
  ""gmrState"": ""CHECKED_IN"",
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

		const string MessageTextEmbarked = @"{
  ""messageId"": ""714d1c3a-638a-44d2-9830-80736b0936f4"",
  ""gmrId"": ""GMRA00002KW2"",
  ""gmrStatusVersion"": 4,
  ""createdDateTime"": ""2021-08-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-08-12T16:39:03.198Z"",
  ""gmrState"": ""EMBARKED"",
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

		const string MessageTextNotFinalisable = @"{
  ""messageId"": ""714d1c3a-638a-44d2-9830-80736b0936f4"",
  ""gmrId"": ""GMRA000002FK"",
  ""gmrStatusVersion"": 2,
  ""createdDateTime"": ""2021-08-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-08-12T16:39:03.198Z"",
  ""gmrState"": ""NOT_FINALISABLE"",
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

		const string MessageTextOpen = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2VC"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-08-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-08-24T04:23:50.384Z"",
  ""gmrState"": ""OPEN"",
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

		const string MessageTextFinalised = @"{
  ""messageId"": ""e0e269cc-d76f-40c1-a6a3-b1c6d4ac5e9c"",
  ""gmrId"": ""GMRDOQORBMZF"",
  ""gmrStatusVersion"": 2,
  ""createdDateTime"": ""2021-08-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-08-24T04:23:50.384Z"",
  ""gmrState"": ""FINALISED"",
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
}";
	}
}
