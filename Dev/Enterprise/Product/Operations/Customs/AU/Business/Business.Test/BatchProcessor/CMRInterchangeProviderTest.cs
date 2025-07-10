using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRInterchangeProviderTest : InterchangeProviderTestCase
	{
		public void TestCollateWithDoNotCollateTypeMessages()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);

			EDIMessage controlMessage0 = messages.AddNew();
			controlMessage0.EM_MessageType = "CTL";

			EDIMessage messageOwnerMessage1 = messages.AddNew();
			messageOwnerMessage1.EM_MessageOwner = "OWNER1";
			messageOwnerMessage1.EM_MessageType = "HA";

			EDIMessage messageOwnerMessage2 = messages.AddNew();
			messageOwnerMessage2.EM_MessageOwner = "OWNER1";
			messageOwnerMessage2.EM_MessageType = "HA";

			EDIMessage messageOwnerMessage3 = messages.AddNew();
			messageOwnerMessage3.EM_MessageOwner = "OWNER1";
			messageOwnerMessage3.EM_MessageType = "HA";

			EDIMessage controlMessage1 = messages.AddNew();
			controlMessage1.EM_MessageType = "CTL";

			EDIMessage messageOwnerMessage4 = messages.AddNew();
			messageOwnerMessage4.EM_MessageOwner = "OWNER1";
			messageOwnerMessage4.EM_MessageType = "HA";

			EDIMessage messageOwnerMessage5 = messages.AddNew();
			messageOwnerMessage5.EM_MessageOwner = "OWNER1";
			messageOwnerMessage5.EM_MessageType = "HA";

			EDIMessage messageOwnerMessage6 = messages.AddNew();
			messageOwnerMessage6.EM_MessageOwner = "OWNER2";
			messageOwnerMessage6.EM_MessageType = "HB";

			EDIMessage messageOwnerMessage7 = messages.AddNew();
			messageOwnerMessage7.EM_MessageOwner = "OWNER2";
			messageOwnerMessage7.EM_MessageType = "HB";

			CMRInterchangeProvider provider = new CMRInterchangeProvider(messages);
			EDIInterchange[] interchanges = provider.Interchanges;
			AssertEquals(3, interchanges.Length);

			string[] expected = new string[]
			{
				"CTL", "HA", "HB"
			};

			foreach (EDIInterchange interchange in interchanges)
			{
				AssertCollectionContains(((string)interchange.EI_InterchangeType), expected);

				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					AssertEquals(interchange.EI_InterchangeType, message.EM_MessageType);
				}
			}
		}

		public void TestGetCollationKey()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);

			string key = "hello_world";
			EDIMessage message = messages.AddNew();
			message.EM_MessageOwner = key;

			var provider = new CMRInterchangeProviderForTest(messages);
			AssertEquals(key, provider.GetCollationKey_Exposed(message));
		}

		public void TestGetCustomsRegNumber()
		{
			GlbCompany otherCompany = OtherCompany;
			otherCompany.GC_CustomsRegistrationNo = "24680";
			Message.EM_GB = otherCompany.Branches.AddNew().PK;

			AssertEquals("SenderID", "24680", CMRInterchangeProvider.GetCustomsRegNumber(Message));
		}

		public void TestGetCMRTestMode()
		{
			GlbCompany otherCompany = OtherCompany;
			Message.EM_GB = otherCompany.Branches.AddNew().PK;
			var emptyProvider = new CMRInterchangeProvider(new NonDependentEDIMessageCollection(Factory));

			Env.Registry.SetCMRTestModeForCompany(otherCompany.PK.ToGuid(), false);
			AssertEquals("TestMode", false, emptyProvider.GetCMRTestMode(Message));
			Env.Registry.SetCMRTestModeForCompany(otherCompany.PK.ToGuid(), true);
			AssertEquals("TestMode", true, emptyProvider.GetCMRTestMode(Message));
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			Env.Registry.CMRTestMode = false;
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_ApplicationCode = "CMR";
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Outgoing1.txt"));
			message.EM_MessageType = "ACR";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_IsActive = true;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageOwner = "";

			EDIMessage message2 = messages.AddNew();
			message2.EM_ApplicationCode = "CMR";
			message2.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Outgoing2.txt"));
			message2.EM_MessageType = "ACR";
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message2.EM_IsActive = true;
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_MessageOwner = "";

			CMRInterchangeProvider provider = new CMRInterchangeProvider(messages);
			AssertEquals("Interchanges.Count", 1, provider.Interchanges.Length);

			EDIInterchange interchange = provider.Interchanges[0];
			AssertEquals("1", interchange.ContainedMessages[0].EM_MessageNum);
			AssertEquals("2", interchange.ContainedMessages[1].EM_MessageNum);
			Assert(interchange.ContainedMessages[0].EM_MessageText.StartsWith("UNH+1"));
			Assert(interchange.ContainedMessages[1].EM_MessageText.StartsWith("UNH+2"));

			ZString expectedFirstPart = "UNA:+.? 'UNB+UNOC:3+" + CMRInterchangeProvider.GetCustomsRegNumber(message) + "::" + CMRInterchangeProvider.GetCustomsRegNumber(message) + "+" + CMRInterchangeProvider.CMRRecipientID + "+";
			ZString expectedSecondPart = "+<<INTERCHANGENUMBERPLACEHOLDER>>++++1'";
			ZString actualHeader = provider.Interchanges[0].EI_HeaderText;

			AssertEquals("Header First Part", expectedFirstPart, actualHeader.Left(expectedFirstPart.Length));
			AssertEquals("Header Second Part", expectedSecondPart, actualHeader.Right(expectedSecondPart.Length));
			AssertEquals("Footer", "UNZ+2+<<INTERCHANGENUMBERPLACEHOLDER>>'", provider.Interchanges[0].EI_FooterText);
			AssertEquals("Body", (message.EM_MessageText + message2.EM_MessageText), provider.Interchanges[0].EI_BodyText);
			AssertEquals("InterchangeNum", ZString.Empty, provider.Interchanges[0].EI_InterchangeNum);
			AssertEquals("Status", EDIInterchange.Status.SendPending, provider.Interchanges[0].EI_Status);
			AssertEquals("To", CMRInterchangeProvider.CMRRecipientID, provider.Interchanges[0].EI_To);
			AssertEquals("From", CMRInterchangeProvider.GetCustomsRegNumber(message), provider.Interchanges[0].EI_From);
			AssertEquals("Branch", message.EM_GB, provider.Interchanges[0].EI_GB);
			AssertEquals("ApplicationCode", "CMR", provider.Interchanges[0].EI_ApplicationCode);
			AssertEquals("MessageStatus", "SNT", message.EM_Status);
		}

		public void TestInterchangeOwnerSet()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_ApplicationCode = "CMR";
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Outgoing1.txt"));
			message.EM_MessageType = "ACR";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_IsActive = true;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageOwner = MessageOwner;
			message.EM_LinkedObject = LinkedObject;

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			collection.Add(message);

			InterchangeProviderBase provider = GetInterchangeProvider(collection);
			AssertNotNull(provider.Interchanges);
			EDIInterchange interchange = provider.Interchanges[0];
			AssertContains("Interchange Header Text", MessageOwner, interchange.EI_HeaderText);
		}

		public void TestInterchangeOwnerNotSet()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_ApplicationCode = "CMR";
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Outgoing1.txt"));
			message.EM_MessageType = "ACR";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_IsActive = true;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageOwner = ZString.Empty;
			message.EM_LinkedObject = LinkedObject;

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			collection.Add(message);

			CMRInterchangeProvider provider = new CMRInterchangeProvider(collection);
			EDIInterchange interchange = provider.Interchanges[0];
			AssertEquals("Interchange Header Text", false, interchange.EI_HeaderText.Contains(MessageOwner));
		}

		public void TestMaximumInterchangeSize()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message0 = messages.AddNew();
			message0.FillWithValidTestData();
			message0.EM_MessageText = "AAAAAAAAAA";
			var message1 = messages.AddNew();
			message1.FillWithValidTestData();
			message1.EM_MessageText = "BBBBBBBBBB";
			var message2 = messages.AddNew();
			message2.FillWithValidTestData();
			message2.EM_MessageText = "CCCCCCCCCC";
			var message3 = messages.AddNew();
			message3.FillWithValidTestData();
			message3.EM_MessageText = "DDDDDDDDDD";

			var provider = new CMRInterchangeProvider(messages);
			var result = provider.Interchanges;
			AssertNotNull(result);
			AssertEquals(1, result.Length);
			AssertEquals("AAAAAAAAAABBBBBBBBBBCCCCCCCCCCDDDDDDDDDD", result[0].EI_BodyText);

			result[0].ContainedMessages.RemoveAll();
			AUCustomsDataRegistry.Instance.MaximumInterchangeSize.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 130);
			message0.EM_Status = "QUE";
			message1.EM_Status = "QUE";
			message2.EM_Status = "QUE";
			message3.EM_Status = "QUE";

			provider = new CMRInterchangeProvider(messages);
			result = provider.Interchanges;
			AssertNotNull(result);
			AssertEquals(1, result.Length);
			AssertEquals("AAAAAAAAAABBBBBBBBBBCCCCCCCCCC", result[0].EI_BodyText);
			AssertEquals("SNT", message0.EM_Status);
			AssertEquals("SNT", message1.EM_Status);
			AssertEquals("SNT", message2.EM_Status);
			AssertEquals("QUE", message3.EM_Status);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new CMRInterchangeProvider(collection);

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.BatchProcessor.TestFiles." + fileName;

		const string MessageOwner = "A33849";

		sealed class CMRInterchangeProviderForTest : CMRInterchangeProvider
		{
			public CMRInterchangeProviderForTest(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			public string GetCollationKey_Exposed(EDIMessage message) => GetCollationKey(message);
		}
	}
}
