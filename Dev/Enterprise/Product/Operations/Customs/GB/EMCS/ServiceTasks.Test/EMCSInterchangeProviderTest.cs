using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks.Testing
{
	public class EMCSInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());

			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			var message2 = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.Splitting);
			messages.AddRange(new EDIMessage[] { message1, message2 });

			var provider = (EMCSInterchangeProvider)GetInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();
			message1.Reload();
			message2.Reload();

			var interchanges = provider.Interchanges;
#if NET8_0_OR_GREATER
			var expectedHeaderText = $@"<GBCustomsRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Provider>EMCS</Provider>
  <Service>Consignor</Service>
  <Credentials Key=""HYECMT.123456789.EMC"" />
  <JobNumber>{declaration.JE_DeclarationReference}</JobNumber>
  <Version>1.0</Version>
  <ContentType>XML</ContentType>
</GBCustomsRequest>";
#else
var expectedHeaderText = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>EMCS</Provider>
  <Service>Consignor</Service>
  <Credentials Key=""HYECMT.123456789.EMC"" />
  <JobNumber>{declaration.JE_DeclarationReference}</JobNumber>
  <Version>1.0</Version>
  <ContentType>XML</ContentType>
</GBCustomsRequest>";
#endif

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 2, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertNotNull("Interchange 2 is linked to message 2", interchanges.FirstOrDefault(x => x.PK == message2.EM_EI));
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);

				AssertStartsWith("Header text", expectedHeaderText, interchange1.EI_HeaderText);
				AssertEquals("Body", "<Test 815/>", interchange1.EI_BodyText);
				AssertEquals("Footer", ZString.Empty, interchange1.EI_FooterText);
				AssertEquals("Status", EDIInterchange.Status.eHubQueued, interchange1.EI_Status);
				AssertEquals("EI_To", "GBCustoms", interchange1.EI_To);
				AssertEquals("EI_From", "ABC", interchange1.EI_From);
			});

			message1.EM_ApplicationReference = "APPLICATIONREFERENCEVALUE";
			messages = new NonDependentEDIMessageCollection(Factory);
			var message3 = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD);
			messages.Add(message3);

			provider = (EMCSInterchangeProvider)GetInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();
			message3.Reload();

			var interchange3 = provider.Interchanges.FirstOrDefault(x => x.PK == message3.EM_EI);
			CombineAssertions(() =>
			{
				AssertNotNull("Interchange 3 is linked to message 3", interchange3);
				AssertEquals("Message 3 is sent", EDIMessage.Status.Sent, message3.EM_Status);
				AssertContains("Header text", "<ServiceReference>APPLICATIONREFERENCEVALUE</ServiceReference>", interchange3.EI_HeaderText);
			});
		}

		public void TestWithNoCredentials()
		{
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid(), true);
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			messages.Add(message);
			var provider = (EMCSInterchangeProvider)GetInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();
			message.Reload();
			AssertEquals("Message should have failed", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestInvalidMessageType()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, "xxx");
			messages.Add(message);
			var provider = (EMCSInterchangeProvider)GetInterchangeProvider(messages);
			AssertExceptionThrown(typeof(NotSupportedException), "Message type xxx is not supported.", provider.PackCollatedMessagesIntoInterchanges);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages)
		{
			return new EMCSInterchangeProvider(new LoggingInformation(), messages);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
		}
	}
}
