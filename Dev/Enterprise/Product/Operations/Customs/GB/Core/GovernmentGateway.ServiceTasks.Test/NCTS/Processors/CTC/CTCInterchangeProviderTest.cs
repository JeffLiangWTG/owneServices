using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Moq.Protected;
using WTG.TestHelpers.Xml;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class CTCInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var enterpriseCode = GBExtensions.GetEnterpriseCode(); // e.g. HYECMT or EDIDAT
			var credentialsNode = $@"  <Credentials Key=""{enterpriseCode}.GB123456789000.CTC"" />";

			var headerText = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>CTCGB</Provider>
  <Service>Depart</Service>
{credentialsNode}
  <JobNumber>NCT00000001</JobNumber>
  <ServiceReference>Ref1</ServiceReference>
  <Version>1.0</Version>
  <ContentType>XML</ContentType>
</GBCustomsRequest>";

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "Ncts001";

			var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_TEST>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("TEST");
			mockMessage.Setup(m => m.EM_ApplicationReference).Returns("Ref1");
			mockMessage.Setup(m => m.EM_MessageSubType).Returns("015");
			mockMessage.Setup(m => m.EM_ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCommonTransitConvention);
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
			var message1 = CreateAndPopulateMessage(nctsHeader, "015");
			var message2 = CreateAndPopulateMessage(nctsHeader, "007");
			messages.AddRange(new EDIMessage[] { message1, message2 });

			var provider = ((CTCInterchangeProvider)GetInterchangeProvider(messages));
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

		public void TestPopulateInterchange_ExpectNoInterchangeIfNoOwner()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();

			CombineAssertions(() =>
			{
				var ownedMessages = new NonDependentEDIMessageCollection(Factory);
				var ownedMessage = CreateAndPopulateMessage(nctsHeader, "015");
				ownedMessages.AddRange(new EDIMessage[] { ownedMessage });

				var ownedProvider = ((CTCInterchangeProvider)GetInterchangeProvider(ownedMessages));
				ownedProvider.PackCollatedMessagesIntoInterchanges();

				var interchanges = ownedProvider.Interchanges;
				AssertEquals("There should be one interchange", 1, interchanges.Length);
				AssertEquals(ownedMessage.EM_MessageOwner, interchanges[0].EI_From);

				var unownedMessages = new NonDependentEDIMessageCollection(Factory);
				var unownedMessage = CreateAndPopulateMessage(nctsHeader, "015");
				unownedMessage.EM_MessageOwner = null;
				unownedMessages.AddRange(new EDIMessage[] { unownedMessage });

				var unownedProvider = ((CTCInterchangeProvider)GetInterchangeProvider(unownedMessages));
				unownedProvider.PackCollatedMessagesIntoInterchanges();

				AssertEquals("There should be no interchanges", 0, unownedProvider.Interchanges.Length);
			});
		}

		public static void SetupNctsHeaderForCredentialsTest(BusinessObjectFactory factory, NctsHeader nctsHeader, ZString eori)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "ImporterA";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori, Core.Constants.CountryCodes.UnitedKingdom);
			var aaaBranch = factory.New<GlbBranch>();
			aaaBranch.GB_Code = "ABC";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = orgHeader.PK;
			factory.Save();

			nctsHeader.BH_GB = aaaBranch.PK;
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new CTCInterchangeProvider(new LoggingInformation(), collection);
		}

		EDIMessage CreateAndPopulateMessage(NctsHeader header, ZString subType)
		{
			var message = Factory.New<EDIMessage>();
			message.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "<CC015/>";
			message.EM_MessageSubType = subType;
			message.EM_MessageOwner = "ABC";
			message.EM_LinkedObject = header;

			return message;
		}
	}
	public class EDIMessageDummyForTest_TEST : EDIMessage
	{
		public EDIMessageDummyForTest_TEST(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "TEST";
		}
	}
}
