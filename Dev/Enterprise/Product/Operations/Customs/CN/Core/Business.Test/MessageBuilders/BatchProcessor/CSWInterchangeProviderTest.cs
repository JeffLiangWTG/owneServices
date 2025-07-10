using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CSWInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			RunTestWithCNSWClientSettingOnBranch(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();

				var message1 = CreateNewACDMessage();
				var message2 = CreateNewACDMessage();
				var messageCollection = new NonDependentEDIMessageCollection(Factory) { message1, message2 };
				var interchangeProvider = new CSWInterchangeProvider(messageCollection);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();

				var interchanges = interchangeProvider.Interchanges;

				CombineAssertions(() =>
				{
					AssertEquals("Should have 2 interchanges(DoNotCollateType)", 2, interchanges.Length);

					var interchange = interchangeProvider.Interchanges[0];
					AssertType<CNEDIInterchange>(interchange);
					AssertEquals("EI_InterchangeNum", "", interchange.EI_InterchangeNum);
					AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.GenericMessageDelivery, interchange.EI_ApplicationCode);
					AssertEquals("EI_InterchangeType", "CSW", interchange.EI_InterchangeType);
					AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
					AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("EI_To", "ENTCNCSVRCN1_CSW", interchange.EI_To);
					AssertEquals("EI_Status", "HQU", interchange.EI_Status);
					AssertEquals("EI_BodyText", "ACD MESSAGE TEXT", interchange.EI_BodyText);
					AssertEquals("EI_FooterText", ZString.Empty, interchange.EI_FooterText);

					AssertEquals("EM_EI", interchange.PK, message1.EM_EI);
					AssertEquals("EM_Status", "SNT", message1.EM_Status);
				});

				EDIMessage CreateNewACDMessage()
				{
					var message = Factory.New<CNEDIMessage>();
					entryHeader.Messages.Add(message);
					message.EM_MessageType = EDIMessageTypeList.Codes.ACD;
					message.EM_MessageText = "ACD MESSAGE TEXT";
					return message;
				}
			});
		}

		[TestDate(2022, 12, 29, 09, 50, 30, 123)]
		public void TestPopulateInterchangeForDecMessage()
		{
			var expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Header>
  <SenderID>sender</SenderID>
  <RecipientID>receiver</RecipientID>
  <Body>
    <test1>body1</test1>
  </Body>
</Header>";

			RunTestWithCNSWClientSettingOnBranch(() =>
			{
				var message1 = CreateNewDECMessage();
				Factory.Save();

				var interchange = PackMessagesIntoInterchanges(message1);

				using (var memoryStream = new MemoryStream(Convert.FromBase64String(interchange.EI_BodyText)))
				using (var zipArchive = new ZipArchive(memoryStream))
				{
					CombineAssertions(() =>
					{
						AssertEquals("ZipArchive should have 1 files", 1, zipArchive.Entries.Count);
						AssertZipContent(zipArchive, "CUS202209030000001_20221229095030123.xml", expectedXml);
					});
				}

				var message2 = CreateNewDECMessage();
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var docManagerSupport = (IDocManagerSupport)declaration;
				var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "DEC");
				var eDoc1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "PDF", false);
				var eDoc2 = storageMain.AddFileOrDocument(new byte[] { 4, 5, 6 }, "DEF.pdf", "PDF", false);
				var attach1 = message2.MessageAttachments.AddNew();
				attach1.EG_StorageDocsGuid = eDoc1.UniqueKey;
				attach1.EG_FileName = eDoc1.FileName;
				var attach2 = message2.MessageAttachments.AddNew();
				attach2.EG_StorageDocsGuid = eDoc2.UniqueKey;
				attach2.EG_FileName = eDoc2.FileName;
				var attach3 = message2.MessageAttachments.AddNew();
				var attach4 = message2.MessageAttachments.AddNew();
				attach4.EG_StorageDocsGuid = eDoc2.UniqueKey;
				Factory.Save();
				docManagerSupport.DocManagerInfo.MasterFactory.Save();

				interchange = PackMessagesIntoInterchanges(message2);
				using (var memoryStream = new MemoryStream(Convert.FromBase64String(interchange.EI_BodyText)))
				using (var zipArchive = new ZipArchive(memoryStream))
				{
					CombineAssertions(() =>
					{
						AssertEquals("ZipArchive should have 3 files", 3, zipArchive.Entries.Count);
						AssertZipContent(zipArchive, "CUS202209030000001_20221229095030123.xml", expectedXml);
						AssertZipContent(zipArchive, "ABC.pdf", eDoc1.ImageData);
						AssertZipContent(zipArchive, "DEF.pdf", eDoc2.ImageData);
					});
				}

				var message3 = CreateNewDECMessage();
				message3.EM_MessageText = ZString.Empty;

				interchange = PackMessagesIntoInterchanges(message3);

				using (var memoryStream = new MemoryStream(Convert.FromBase64String(interchange.EI_BodyText)))
				using (var zipArchive = new ZipArchive(memoryStream))
				{
					AssertEquals("ZipArchive should have 0 files", 0, zipArchive.Entries.Count);
				}
			});
		}

		void AssertZipContent(ZipArchive zipArchive, string filename, string expectedText)
		{
			var zipEntry = zipArchive.GetEntry(filename);
			AssertNotNull($"Zip file contains {filename}", zipEntry);

			using (var stream = zipEntry.Open())
			{
				AssertEquals("Zip File Content", expectedText, Encoding.ASCII.GetString(stream.ToByteArray()));
			}
		}

		void AssertZipContent(ZipArchive zipArchive, string filename, byte[] exceptBytes)
		{
			var zipEntry = zipArchive.GetEntry(filename);
			AssertNotNull($"Zip file contains {filename}", zipEntry);

			using (var stream = zipEntry.Open())
			{
				AssertEquals("Zip File Content", exceptBytes, stream.ToByteArray());
			}
		}

		EDIInterchange PackMessagesIntoInterchanges(CNEDIMessage message)
		{
			var messageCollection = new NonDependentEDIMessageCollection(Factory) { message };
			var interchangeProvider = new CSWInterchangeProvider(messageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();

			return interchangeProvider.Interchanges[0];
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new CSWInterchangeProvider(collection);

		CNEDIMessage CreateNewDECMessage()
		{
			var message = Factory.New<CNEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageType = EDIMessageTypeList.Codes.DEC;
			var decMessage = new XDocument();
			decMessage.Add(new XElement("Header", new XElement("SenderID", "sender"), new XElement("RecipientID", "receiver")));
			decMessage.Root.Add(new XElement("Body", new XElement("test1", "body1")));

			message.EM_MessageText = decMessage.ToString();
			message.EM_ApplicationReference = "CUS202209030000001";
			return message;
		}

		void RunTestWithCNSWClientSettingOnBranch(Action unitTest)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CNC";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "CN1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "CN2";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";
			Factory.Save();

			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory, Guid.Empty, branch1.PK.ToGuid()))
			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				unitTest();
			}
		}
	}
}
