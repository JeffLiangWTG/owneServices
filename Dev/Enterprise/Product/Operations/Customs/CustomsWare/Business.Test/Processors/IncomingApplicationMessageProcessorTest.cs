using System;
using System.IO;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	sealed class IncomingApplicationMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_WrongFormatImageXMLFile_InvalidStream()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "InvalidStream";
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			message.EM_MessageNum = "IANTEST";
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = File.ReadAllText(testPath + "WrongFormatImageXMLFile_InvalidStream.xml");
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			var logger = new LoggingInformation();
			var processor = new IncomingApplicationMessageProcessor(logger);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("No Exception", () =>
				{
					processor.ProcessMessage(message);
				}

				);
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Received).Using(CustomComparers.TypeComparison), "EM_Status");
			}

			);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_WrongFormatImageXMLFile_EmptyStream()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "EmptyStream";
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			message.EM_MessageNum = "IANTEST";
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = File.ReadAllText(testPath + "WrongFormatImageXMLFile_EmptyStream.xml");
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			var logger = new LoggingInformation();
			var processor = new IncomingApplicationMessageProcessor(logger);
			CombineAssertions("No Exception", () =>
			{
				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(message);
				}

				);
				NUnit.Framework.Assert.That(logger.UserLogStrings[0], Does.Not.Contain("contains invalid image. The image data stream supplied does not contain a valid image format."), "No logged, Handled earlier");
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Received).Using(CustomComparers.TypeComparison), "EM_Status");
			}

			);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestExecute_Declaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "CHResponseSampleXMLFile1";
			declaration.JE_GB = branchInOtherCountry.PK;
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "CHResponseSampleXMLFile1";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "noreply@cargowise.com";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			var response1 = File.ReadAllText(testPath + "CHResponseSampleXMLFile1.xml");
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			message.EM_MessageNum = "SampleXMLFile1.xml";
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = response1;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			var log = new LoggingInformation();
			var processor = new IncomingApplicationMessageProcessor(log);
			processor.ProcessMessage(message);
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Received).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_LinkUniqueID, Is.EqualTo(declaration.PK));
			NUnit.Framework.Assert.That(log.UserLogStrings[1], Does.Contain("Linked message SampleXMLFile1.xml"));
			var defaultStatusDescription = declaration?.ActiveEntryHeaders[0]?.DefaultStatusDescription ?? "Not Sent";
			defaultStatusDescription = defaultStatusDescription == ZString.Empty ? (ZString)"&nbsp;" : defaultStatusDescription;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
			NUnit.Framework.Assert.That(email.Subject, Is.EqualTo("CustomsWare Response for Job: CHResponseSampleXMLFile1"));
			var expectedPart1 = $@"<br /><strong><a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=";
			var expectedPart2 = $@">CHResponseSampleXMLFile1</a></strong><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Reference Number</th><th>Status</th><th>Status Description</th><th>Release Date</th></tr></thead><tr><td>CHResponseSampleXMLFile1\001</td><td>&nbsp;</td><td>{defaultStatusDescription}</td><td>&nbsp;</td></tr></table><br />Regards,<br /><br />{BrandingFactory.Instance.ProductName} Automated Messages Sender<br />      </td>";
			NUnit.Framework.Assert.That(email.Body, Does.Contain(expectedPart1));
			NUnit.Framework.Assert.That(email.Body, Does.Contain(expectedPart2));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestExecute_Event1()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "EventSample1";
			declaration.JE_GB = branchInOtherCountry.PK;
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "EventSample1";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = "EventSample1\\002";
			entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = "EventSample1\\001";
			var message = AddToEDIMessageAndProcess("EventSample1.xml");
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Received).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_LinkUniqueID, Is.EqualTo(declaration.PK));
			var log = entry.Logs.MostRecentLogByEventTime(AutoEvents.CustomsEntryStatus);
			NUnit.Framework.Assert.That(log.SL_Reference, Is.EqualTo("RELEASED").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CH_EntryStatus, Is.EqualTo(CustomsWareEntryStatusList.Codes.Released).Using(CustomComparers.TypeComparison));
			message = AddToEDIMessageAndProcess("EventSample2.xml");
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Received).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_LinkUniqueID, Is.EqualTo(declaration.PK));
			log = entry.Logs.MostRecentLogByEventTime(AutoEvents.CustomsEntryStatus);
			NUnit.Framework.Assert.That(log.SL_Reference, Is.EqualTo("CLEARED").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CH_EntryStatus, Is.EqualTo(CustomsWareEntryStatusList.Codes.Cleared).Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestAttachDocumentToEmail()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@cargowise.com";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "AttachmentSample1";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			message.EM_MessageNum = "IANTEST";
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = File.ReadAllText(testPath + "AttachmentSample1.xml");
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			var processor = new IncomingApplicationMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Received).Using(CustomComparers.TypeComparison));
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
			var document = email.Attachments.OfType<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "recv_RD-312880813_20120314-162605_28939.pdf");
			NUnit.Framework.Assert.That(document, Is.Not.EqualTo(default(AttachmentDef)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestCreateNotificationEmail()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_LoginName = "ST1";
			staff1.GS_EmailAddress = "staff1@cargowise.com";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.GS_LoginName = "ST2";
			staff2.GS_EmailAddress = "staff2@cargowise.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "ST3";
			staff3.GS_LoginName = "ST3";
			staff3.GS_EmailAddress = "staff3@cargowise.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(staff3);
			Factory.Save();
			CustomsDataRegistry.Instance.ThirdPartyCustomsResponseEmailNotificationGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B0000100X";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			outgoingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "12345";
			outgoingMessage.EM_SystemCreateUser = staff1.GS_Code;
			AddToEDIMessageAndProcess("DecSample.xml");
			NUnit.Framework.Assert.That(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault().Recipients.Contains(staff1.GS_EmailAddress), Is.True);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			staff1.GS_EmailAddress = "";
			declaration.JE_GS_NKCusAgent = staff2.GS_Code;
			AddToEDIMessageAndProcess("DecSample.xml");
			NUnit.Framework.Assert.That(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault().Recipients.Contains(staff2.GS_EmailAddress), Is.True);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			staff2.GS_EmailAddress = "";
			AddToEDIMessageAndProcess("DecSample.xml");
			NUnit.Framework.Assert.That(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault().Recipients.Contains(staff3.GS_EmailAddress), Is.True);
		}

		EDIMessage AddToEDIMessageAndProcess(ZString fileName)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			message.EM_MessageNum = fileName;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = File.ReadAllText(testPath + fileName);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			var processor = new IncomingApplicationMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Switzerland);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			branchInOtherCountry = Factory.NewWithValidTestData<GlbBranch>();
			branchInOtherCountry.GB_GC = company.PK;
		}

		GlbBranch branchInOtherCountry;

		readonly string testPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\CustomsWare\Business.Test\Processors\TestData\";
	}
}
