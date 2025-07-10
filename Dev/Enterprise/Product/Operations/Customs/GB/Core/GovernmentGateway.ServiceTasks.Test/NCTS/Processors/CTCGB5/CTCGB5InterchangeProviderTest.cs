using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Moq.Protected;
using WTG.TestHelpers.Xml;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class CTCGB5InterchangeProviderTest : CTCInterchangeProviderTest
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var enterpriseCode = GBExtensions.GetEnterpriseCode(); // e.g. HYECMT or EDIDAT
			var credentialsNode = $@"  <Credentials Key=""{enterpriseCode}.GB123456789000.CTC"" />";

			var headerText = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>CTCGB</Provider>
  <Service>UpdateDepart</Service>
{credentialsNode}
  <JobNumber>NCT00000001</JobNumber>
  <ServiceReference>Ref1</ServiceReference>
  <Version>2.0</Version>
  <ContentType>XML</ContentType>
  <Accept>application/vnd.hmrc.2.0+json</Accept>
</GBCustomsRequest>";

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "Ncts001";

			var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_TEST>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("TEST");
			mockMessage.Setup(m => m.EM_ApplicationReference).Returns("Ref1");
			mockMessage.Setup(m => m.EM_MessageType).Returns("170");
			mockMessage.Setup(m => m.EM_ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			nctsHeader.Messages.Add(mockMessage.Object);

			SetupNctsHeaderForCredentialsTest(Factory, nctsHeader, "GB123456789000");

			var company = nctsHeader.Company;
			var wrapper = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company);
			var gbbPasword1 = wrapper.GBBPasswordCollection.AddNew();
			gbbPasword1.Badge = "CTC";
			gbbPasword1.EORI = "GB123456789000";
			gbbPasword1.Status = PasswordStatusList.Codes.Valid;
			gbbPasword1.IsTokenForNCTS = true;

			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = CreateAndPopulateMessage(nctsHeader, "170");
			var message2 = CreateAndPopulateMessage(nctsHeader, "007");
			messages.AddRange(new EDIMessage[] { message1, message2 });

			var provider = (CTCGB5InterchangeProvider)GetInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;
			Factory.Save();
			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 2, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertNotNull("Interchange 2 is linked to message 2", interchanges.FirstOrDefault(x => x.PK == message2.EM_EI));
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);

				XmlComparison.CompareAndAssertXml(headerText, interchange1.EI_HeaderText);
				AssertEquals("Body", "<CC015/>", interchange1.EI_BodyText);
				AssertEquals("Footer", ZString.Empty, interchange1.EI_FooterText);
				AssertEquals("Status", EDIInterchange.Status.eHubQueued, interchange1.EI_Status);
				AssertEquals("EI_To", "GBCustoms", interchange1.EI_To);
				AssertEquals("EI_From", "ABC", interchange1.EI_From);
			});
			mockMessage.VerifyAll();
		}

		public void TestLargeMessage()
		{
			var sysConfigType = Factory.New<RefSysConfigType>();
			sysConfigType.ZRT_ConfigCode = "GBCTC5LFS";
			sysConfigType.ZRT_Description = "GB NCTS phase 5 large file threshold";
			sysConfigType.ZRT_LongDescription = "Threshold size in bytes above which CW tells eHub to use the \"large file\" upload option for CTC/NCTS phase 5";

			const int largeFileSize = 1024;
			var sysConfig = Factory.New<RefSysConfig>();
			sysConfig.ZRC_ZRT_NKConfigCode = "GBCTC5LFS";
			sysConfig.ZRC_DecimalValue = largeFileSize;
			sysConfig.ZRC_StartDate = ZDateTime.MinSmallDateTimeValue;
			sysConfig.ZRC_EndDate = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "Ncts001";

			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = CreateAndPopulateMessage(nctsHeader, "170");
			var message2 = CreateAndPopulateMessage(nctsHeader, "007");
			message2.EM_MessageText = ZString.Replicate('X', largeFileSize);
			messages.AddRange(new[] { message1, message2 });

			var provider = (CTCGB5InterchangeProvider)GetInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			CombineAssertions(() =>
			{
				var interchange1 = provider.Interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals("Status", EDIInterchange.Status.eHubQueued, interchange1.EI_Status);
				AssertEquals("Body size", "<CC015/>".Length, interchange1.EI_BodyText.Length);
				AssertContains("Accept node in EI_HeaderText", "<Accept>application/vnd.hmrc.2.0+json</Accept>", interchange1.EI_HeaderText);
				AssertNotContains("LargeFile node in EI_HeaderText", "<LargeFile>true</LargeFile>", interchange1.EI_HeaderText);

				var interchange2 = provider.Interchanges.FirstOrDefault(x => x.PK == message2.EM_EI);
				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);
				AssertEquals("Status", EDIInterchange.Status.eHubQueued, interchange2.EI_Status);
				AssertEquals("Body size", largeFileSize, interchange2.EI_BodyText.Length);
				AssertContains("Accept node in EI_HeaderText", "<Accept>application/vnd.hmrc.2.0+json</Accept>", interchange2.EI_HeaderText);
				AssertContains("LargeFile node in EI_HeaderText", "<LargeFile>true</LargeFile>", interchange2.EI_HeaderText);
			});
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new CTCGB5InterchangeProvider(new LoggingInformation(), collection);
		}

		EDIMessage CreateAndPopulateMessage(NctsHeader header, ZString subType)
		{
			var message = Factory.New<EDIMessage>();
			message.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsNCTS);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsNCTS;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "<CC015/>";
			message.EM_MessageType = subType;
			message.EM_MessageOwner = "ABC";
			message.EM_LinkedObject = header;

			return message;
		}
	}
}
