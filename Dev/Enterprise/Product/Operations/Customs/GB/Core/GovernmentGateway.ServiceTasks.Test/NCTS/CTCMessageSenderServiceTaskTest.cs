using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	[TestedType(typeof(CTCMessageSenderServiceTask))]
	public class CTCMessageSenderServiceTaskTest : ServiceTaskTestCase<CTCMessageSenderServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs CTC NCTS messages outbound (GCT)",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCommonTransitConvention,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs CTC NCTS messages outbound (GBN)",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsNCTS,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		public void TestParseOneRealExample()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";

			CTCInterchangeProviderTest.SetupNctsHeaderForCredentialsTest(Factory, nctsHeader, "GB123456789000");

			var company = nctsHeader.Company;
			var wrapper = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company);
			var gbbPasword1 = wrapper.GBBPasswordCollection.AddNew();
			gbbPasword1.Badge = "CTC";
			gbbPasword1.EORI = "GB123456789000";
			gbbPasword1.Status = PasswordStatusList.Codes.Valid;
			gbbPasword1.IsTokenForNCTS = true;

			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			ediMessage.EM_LinkedObject = nctsHeader;
			ediMessage.EM_MessageSubType = "015";
			ediMessage.EM_MessageOwner = "ABC";
			ediMessage.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""state"": ""CHECKED IN"",
  ""inspectionRequired"": false
}
";
			Factory.Save();

			_ = InitialiseAndRunTaskSchedule(new CTCMessageSenderServiceTask());

			var message = new BusinessObjectFactory().Load<EDIMessage>(ediMessage.PK);
			AssertEquals(EDIMessageStatusList.Codes.Sent, message.EM_Status);
		}

		public void TestParseOneRealExample_CTCGB5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";

			CTCGB5InterchangeProviderTest.SetupNctsHeaderForCredentialsTest(Factory, nctsHeader, "GB123456789000");

			var company = nctsHeader.Company;
			var wrapper = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company);
			var gbbPasword1 = wrapper.GBBPasswordCollection.AddNew();
			gbbPasword1.Badge = "CTC";
			gbbPasword1.EORI = "GB123456789000";
			gbbPasword1.Status = PasswordStatusList.Codes.Valid;
			gbbPasword1.IsTokenForNCTS = true;

			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsNCTS);
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsNCTS;
			ediMessage.EM_LinkedObject = nctsHeader;
			ediMessage.EM_MessageType = "015";
			ediMessage.EM_MessageOwner = "ABC";
			ediMessage.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""state"": ""CHECKED IN"",
  ""inspectionRequired"": false
}
";
			Factory.Save();

			_ = InitialiseAndRunTaskSchedule(new CTCMessageSenderServiceTask());

			var message = new BusinessObjectFactory().Load<EDIMessage>(ediMessage.PK);
			AssertEquals(EDIMessageStatusList.Codes.Sent, message.EM_Status);
		}
	}
}
