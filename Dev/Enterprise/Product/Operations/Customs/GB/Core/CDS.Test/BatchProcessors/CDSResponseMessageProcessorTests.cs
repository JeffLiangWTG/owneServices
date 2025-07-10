using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PdfiumWrapper;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Test;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Customs.GB.CDS.CDSResponse.Testing;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;
using CusEntryPayInfo = Enterprise.Customs.GB.Business.CusEntryPayInfo;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSResponseMessageProcessorTests : TestCaseWithFactory
	{
		public void TestEntryStatusLogging()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B1234568";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";
			entry.CH_EntryStatus = "AAA";
			Factory.Save();

			var outgoingMessage = Factory.New<CDSEDIMessage>();
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_MessageNum = "21";
			outgoingMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			outgoingMessage.EM_Status = "XXX";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateUser = "ABC";
			outgoingMessage.EM_ApplicationReference = "CONVERSATIONID";

			var incomingMessage = Factory.New<CDSResponseEDIMessage>();
			incomingMessage.EM_MessageNum = "22";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationReference = "CONVERSATIONID";
			incomingMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212+01</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			entry.Messages.Add(incomingMessage);
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new CDSResponseMessageProcessor(logger);
			processor.ProcessMessage(incomingMessage);
			ZString year = ZDate.Today.Year.ToString();
			AssertContains(string.Format(CultureInfo.CurrentCulture, "Entry {0}-B1234568, message #22 (REJ), original message #1 (NEW), original status = AAA, status update needed = Y, new status = CAN, final status = CAN", year.Right(1)), logger.Logs.First().ToString());
		}

		public void TestLogoChangesWhenBranchChanges()
		{
			var logo1 = new Bitmap(32, 32);
			Graphics.FromImage(logo1).Clear(Color.Red);
			var logo2 = new Bitmap(32, 32);
			Graphics.FromImage(logo2).Clear(Color.Blue);
			var logo3 = new Bitmap(32, 32);
			Graphics.FromImage(logo3).Clear(Color.Green);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch1);
			company.Branches.Add(branch2);

			using (SystemDataRegistry.Instance.CompanyLogo.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, logo1))
			using (SystemDataRegistry.Instance.CompanyLogo.SetTemporaryValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, logo2))
			using (SystemDataRegistry.Instance.CompanyLogo.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, logo3))
			{
				ResponseFunctionTests.SetUpZZRefData(Factory);

				var (dec1, entry1) = CreateJobDeclarationWithBranch(branch1, "LRN123456789000-S0001000");
				var (dec2, entry2) = CreateJobDeclarationWithBranch(branch2, "LRN123456789000-S0001001");
				var ediMessage1 = AddResponseMessageToEntry(entry1);
				var ediMessage2 = AddResponseMessageToEntry(entry2);

				var processor = new CDSResponseMessageProcessor(Logger);
				processor.ProcessMessage(ediMessage1);
				processor.ProcessMessage(ediMessage2);

				AssertImageUsedCorrectLogo(dec1, Color.FromArgb(Color.Red.ToArgb()));
				AssertImageUsedCorrectLogo(dec2, Color.FromArgb(Color.Blue.ToArgb()));
			}
		}

		CDSResponseEDIMessage AddResponseMessageToEntry(CusEntryHeader entry)
		{
			var ediMessage = Factory.New<CDSResponseEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = $@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">{DateTime.UtcNow:yyyyMMddhhmmss}Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>{entry.LRN}</FunctionalReferenceID>
    <ID>{Guid.NewGuid()}</ID>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);
			return ediMessage;
		}

		(JobDeclaration dec, CusEntryHeader entry) CreateJobDeclarationWithBranch(GlbBranch branch, string lrn)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_GB = branch.PK;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = dec.CustomsEntryInstructions.FirstOrDefault().PK;
			entry.LRN = lrn;
			Factory.Save();
			return (dec, entry);
		}

		void AssertImageUsedCorrectLogo(JobDeclaration dec, Color expectedLogoColour)
		{
			var storageMain = dec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(dec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using (var memoryStream = new MemoryStream(storageMain.Files[storageMain.Files.Count - 1].ImageData))
			using (var doc = new PdfDocument(memoryStream))
			{
				var documentText = doc.GetAllText();
				var bitmap = new Bitmap(64, 100, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
				doc.RenderPage(Graphics.FromImage(bitmap), 0, bitmap.Size);
				var colour = bitmap.GetPixel(10, 12);
				AssertEquals("This pixel should either be Red, Green, or Blue, depending on the logo used.", expectedLogoColour, colour);
			}
		}

		public void TestUpdateMovementReferenceNumber()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";
			entry.MovementReferenceNumberSetter("123", ZDateTime.Today);
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageNum = "00000001";
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>456</ID>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);

			var logger = new LoggingInformation();
			var processor = new CDSResponseMessageProcessor(logger);
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				var mrn = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "456", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 27, 12, 12, 12), mrn.CE_IssueDate);
				Assert(logger.UserLogStrings.Contains("\tWe're about to change the MRN from 123 to 456 while processing message number 00000001."));
			});

			logger.ClearLogs();
			var ediMessage2 = Factory.New<CDSResponseEDIMessage>();
			ediMessage2.EM_MessageNum = "00000002";
			ediMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage2.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180728121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>789</ID>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage2);

			processor.ProcessMessage(ediMessage2);
			CombineAssertions(() =>
			{
				var mrn = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "789", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 28, 12, 12, 12), mrn.CE_IssueDate);
				Assert(logger.UserLogStrings.Contains("\tWe're about to change the MRN from 456 to 789 while processing message number 00000002."));
			});
		}

		public void TestProcess()
		{
			GlbGroup postmastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			currentUserInCurrentFactory.GS_IsSystemAccount = false;
			postmastersGroup.Staff.Add(currentUserInCurrentFactory);
			Factory.Save();

			var systemAccountUser = Factory.New<GlbStaff>();
			systemAccountUser.GS_Code = "AAA";
			systemAccountUser.GS_LoginName = "AAA";
			systemAccountUser.GS_IsSystemAccount = true;
			systemAccountUser.GS_EmailAddress = "AAA@evenmorepointless.com";
			Factory.Save();

			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";
			Factory.Save();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_SystemCreateUser = currentUserInCurrentFactory.GS_Code;
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "CONVERSATIONID";
			entry.Messages.Add(newMessage);
			Thread.Sleep(100);

			var extraMessageSentBySystemAccount = Factory.New<CDSNewDeclarationEDIMessage>();
			extraMessageSentBySystemAccount.EM_SystemCreateUser = systemAccountUser.GS_Code;
			extraMessageSentBySystemAccount.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			extraMessageSentBySystemAccount.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			extraMessageSentBySystemAccount.EM_Status = EDIMessage.Status.Acknowledged;
			extraMessageSentBySystemAccount.EM_MessageText = @"<MetaData><Message>Message sent from systems account to prove they are ignored when sending email to notification group</Message></MetaData>";
			extraMessageSentBySystemAccount.EM_ApplicationReference = "CONVERSATIONID";
			entry.Messages.Add(extraMessageSentBySystemAccount);
			Thread.Sleep(100);

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_ApplicationReference = "CONVERSATIONID";
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been registered</H3><p><strong>Function Code: </strong>02-RCV<br><strong>Old CHIEF Report Code: </strong>H2/P2<br><strong>MRN: </strong>15GB000060100C85A5<br><strong>LRN: </strong>8GB123456789000-S0001000<br><strong>Issued Date: </strong>2018-07-27 12:12</p>", ediMessage.EM_MessageInterpretation);

				var recentLog = entry.Logs.MostRecentLog;
				AssertEquals("Event.SE_Code", AutoEvents.CustomsEntryStatusCode, recentLog.Event.SE_Code);
				AssertEquals("Log.SL_Reference", Constants.ThreeCharFunctionCodes.MessageRegistered, recentLog.SL_Reference);
				AssertEquals("Log.SL_EventTimeUtc", new ZDateTime(2018, 7, 27, 12, 12, 12), recentLog.SL_EventTimeUtc);

				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, entry.CH_EntryStatus);
				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, ediMessage.EM_MessageSubType);

				var mrn = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "15GB000060100C85A5", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 27, 12, 12, 12), mrn.CE_IssueDate);

				AssertEquals("Message should be attached to entry header", entry, ediMessage.EM_LinkedObject);
				AssertContains(currentUserInCurrentFactory.GS_EmailAddress, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
			});
		}

		public void TestProcessForBondedWarehousing()
		{
			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			ResponseFunctionTests.SetUpZZRefData(Factory);

			var helper = new GBWhsDataTestHelper(Factory, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var entry = helper.GetNewEntryHeader("IMP", "B123", helper.InwardCusProcedure.ZZ6_ProcedureCode, "EN00001", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m, GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
				entry.Declaration.JE_MessageType = "IMP";
				entry.CH_EntryStatus = "ABC";
				entry.CH_BGMReference = "9GB123456789000-B00001000";
				entry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreationHeld;
				entry.LRN = "8GB123456789000-S0001000";

				Factory.Save();

				var declarationEdiMessage = Factory.New<CDSNewDeclarationEDIMessage>();
				declarationEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRequestMessageXmlContent();
				entry.Messages.Add(declarationEdiMessage);

				var ediMessage = Factory.New<CDSResponseEDIMessage>();

				ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>09</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
				var logger = new LoggingInformation();
				var processor = new CDSResponseMessageProcessor(logger);
				processor.ProcessMessage(ediMessage);

				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
					AssertEquals("Message.EM_MessageInterpretation", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Declaration is now cleared</H3><p><strong>Function Code: </strong>09-CLE<br><strong>MRN: </strong>15GB000060100C85A5<br><strong>LRN: </strong>8GB123456789000-S0001000<br><strong>Issued Date: </strong>2018-07-27 12:12</p>", ediMessage.EM_MessageInterpretation);

					AssertEquals("CLR", entry.CH_EntryStatus);
					AssertEquals("CLE", ediMessage.EM_MessageSubType);

					var mrn = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					AssertNotNull("MovementReferenceNumber", mrn);
					AssertEquals("MovementReferenceNumber.CE_EntryNum", "EN00001", mrn.CE_EntryNum);

					AssertEquals("Message should be attached to entry header", entry, ediMessage.EM_LinkedObject);

					var allMessages = string.Join(" ", logger.Logs.Select(x => x.Message));
					AssertNotContains("Error Processing Incoming EDI Message", allMessages);
					AssertNotContains("Data Context for the WarehouseCustomsEntry data object was empty", allMessages);
					AssertContains("SUBJECT: Stock Levels Update for Entry: H2 - HELLO - EN00001", allMessages);
				});
			}
		}

		public void TestProcessWithDaylightSavingTime()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212+01</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been registered</H3><p><strong>Function Code: </strong>02-RCV<br><strong>Old CHIEF Report Code: </strong>H2/P2<br><strong>MRN: </strong>15GB000060100C85A5<br><strong>LRN: </strong>8GB123456789000-S0001000<br><strong>Issued Date: </strong>2018-07-27 12:12</p>", ediMessage.EM_MessageInterpretation);

				var recentLog = entry.Logs.MostRecentLog;
				Assert("Event", recentLog.Event.SE_Code == Events.CustomsEntryStatusCode);
				Assert("Log.SL_Reference", recentLog.SL_Reference == Constants.ThreeCharFunctionCodes.MessageRegistered);

				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, entry.CH_EntryStatus);
				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, ediMessage.EM_MessageSubType);

				var mrn = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "15GB000060100C85A5", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 27, 12, 12, 12), mrn.CE_IssueDate);

				AssertEquals("Message should be attached to entry header", entry, ediMessage.EM_LinkedObject);
			});
		}

		public void TestRemoveDeclarationSemaphoreOnReceive()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationReference = "B1230";
			dec1.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.LRN = "8GB123456789000-S0001000";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "B1231";
			dec2.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			entry2.LRN = "8GB123456789001-S0001001";

			Factory.Save();

			using var helper = new MessageResponseSemaphoreHelper();
			helper.CreateSemaphoreForDeclaration(dec1);
			helper.CreateSemaphoreForDeclaration(dec2);

			var ediMessage = Factory.New<CDSResponseEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212+01</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			AssertEquals("Pre-requisite: message should have been linked to the correct entry", entry1, ediMessage.EM_LinkedObject);

			CombineAssertions(() =>
			{
				AssertEquals("Semaphore for first declaration should have been removed", expected: false, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(dec1));
				AssertEquals("Semaphore for second declaration should still exist", expected: true, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(dec2));
			});
		}

		public void TestSaveDutiesFees()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "18GB123456789";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			entryLine.Fees.AddOrUpdate("A00", 97.05m).CF_BaseValue = 10000;
			entryLine.Fees.AddOrUpdate("B00", 219.50m).CF_BaseValue = 9999;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
		<FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
		<IssueDateTime>
			<DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180119155400Z</DateTimeString>
		</IssueDateTime>
		<AppealOffice>
			<ID>GBLBA001</ID>
		</AppealOffice>
		<Bank>
			<ReferenceID>CITIGB2LLON</ReferenceID>
			<ID>GB25CITI08320011963155</ID>
		</Bank>
		<ContactOffice>
			<ID>GBLBA001</ID>
			<Communication>
				<ID>See Developer Hub</ID>
				<TypeCode>EM</TypeCode>
			</Communication>
			<Communication>
				<ID>+441234567891</ID>
				<TypeCode>FX</TypeCode>
			</Communication>
		</ContactOffice>
		<Status>
			<NameCode>4</NameCode>
		</Status>
		<Declaration>
			<FunctionalReferenceID>18GB123456789</FunctionalReferenceID>
			<ID>18GBJCM3USAFD2WD51</ID>
			<VersionID>1</VersionID>
			<GoodsShipment>
				<GovernmentAgencyGoodsItem>
					<SequenceNumeric>1</SequenceNumeric>
					<Commodity>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>9.70</TaxRateNumeric>
							<TypeCode>A00</TypeCode>
							<Payment>
								<TaxAssessedAmount>97.00</TaxAssessedAmount>
								<PaymentAmount>97.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>20.00</TaxRateNumeric>
							<TypeCode>B00</TypeCode>
							<Payment>
								<TaxAssessedAmount>219.40</TaxAssessedAmount>
								<PaymentAmount>219.40</PaymentAmount>
							</Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>30.00</TaxRateNumeric>
							<TypeCode>STA</TypeCode>
							<Payment>
								<TaxAssessedAmount>98.00</TaxAssessedAmount>
								<PaymentAmount>98.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>40.00</TaxRateNumeric>
							<TypeCode>C00</TypeCode>
							<Payment>
								<TaxAssessedAmount>99</TaxAssessedAmount>
							</Payment>
						</DutyTaxFee>
					</Commodity>
				</GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
	</Response>";
			entry.Messages.Add(ediMessage);
			dec.MarkApportionmentDirty();

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Duties and taxes have been calculated and are due</H3><p><strong>Function Code: </strong>13-TAX<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>Bank: </strong>GB25CITI08320011963155<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>18GBJCM3USAFD2WD51<br><strong>LRN: </strong>18GB123456789<br><strong>Issued Date: </strong>2018-01-19 15:54</p><p>Status Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>4</td><td>Final customs debt</td><td>&nbsp;</td></tr></table></p><p>Payment Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Item</td><td>Tax Type or Reference</td><td>Payment Amount</td><td>Assessed Amount</td><td>Tax Rate</td><td>Tax Base</td></tr><tr><td>Item 1</td><td>A00</td><td>97.00</td><td>97.00</td><td>9.70%</td><td>&nbsp;</td></tr><tr><td>Item 1</td><td>B00</td><td>219.40</td><td>219.40</td><td>20.00%</td><td>&nbsp;</td></tr><tr><td>Item 1</td><td>STA</td><td>98.00</td><td>98.00</td><td>30.00%</td><td>&nbsp;</td></tr><tr><td>Item 1</td><td>C00</td><td>0</td><td>99</td><td>40.00%</td><td>&nbsp;</td></tr></table></p>", ediMessage.EM_MessageInterpretation);

				AssertEquals("ConfirmedFees.Count", 3, entryLine.ConfirmedFees.Count);

				var a00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals(9.70m, a00.CF_Rate);
					AssertEquals(97.00m, a00.CF_ChargeAmount);
					AssertEquals("E", a00.CF_MethodOfPayment);
					AssertEquals(10000.00m, a00.CF_BaseValue);
				}

				var b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals(20.0m, b00.CF_Rate);
					AssertEquals(219.40m, b00.CF_ChargeAmount);
					AssertEquals("E", b00.CF_MethodOfPayment);
					AssertEquals(9999.00m, b00.CF_BaseValue);
				}

				AssertNull("ConfirmedFee STA", entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "STA"));

				var c00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "C00");
				AssertNotNull("ConfirmedFee C00", c00);
				if (c00 != null)
				{
					AssertEquals(true, c00.CF_IsLandedCostOnly);
					AssertEquals(99.0m, c00.CF_ChargeAmount);
				}

				AssertEquals("Have run apportionment for TAX/4 response", false, dec.ApportionmentDirty);
			});

			CombineAssertions("Fees should not be changed by message.", () =>
			{
				AssertEquals(2, entryLine.Fees.Count);
				var a00 = entryLine.Fees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull(a00);
				AssertEquals(0.0m, a00.CF_Rate);
				AssertEquals(97.05m, a00.CF_ChargeAmount);
				AssertEquals("E", a00.CF_MethodOfPayment);
				AssertEquals(10000.00m, a00.CF_BaseValue);

				var b00 = entryLine.Fees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
				AssertNotNull(b00);
				AssertEquals(0.0m, b00.CF_Rate);
				AssertEquals(219.50m, b00.CF_ChargeAmount);
				AssertEquals("E", b00.CF_MethodOfPayment);
				AssertEquals(9999.00m, b00.CF_BaseValue);

				AssertNull(entryLine.Fees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "STA"));
				AssertNull(entryLine.Fees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "C00"));
			});
		}

		public void TestSaveDutiesFees_MoP_ZeroPaymentWithFiscalReference()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "18GB123456789";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var lineMerger = new CDSLineMerger(dec);
			lineMerger.DoMerge();

			var reference = entry.EntryInstruction.FiscalReferences.AddNew();
			reference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			reference.CFR_Reference = "1234";

			AssertEquals("IsPostponedVatViaFiscalReference should be true when FR1 FiscalReference is present", true, entry.EntryInstruction.IsPostponedVatViaFiscalReference);

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
		<FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
		<IssueDateTime>
			<DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180119155400Z</DateTimeString>
		</IssueDateTime>
		<AppealOffice>
			<ID>GBLBA001</ID>
		</AppealOffice>
		<Bank>
			<ReferenceID>CITIGB2LLON</ReferenceID>
			<ID>GB25CITI08320011963155</ID>
		</Bank>
		<ContactOffice>
			<ID>GBLBA001</ID>
			<Communication>
				<ID>See Developer Hub</ID>
				<TypeCode>EM</TypeCode>
			</Communication>
			<Communication>
				<ID>+441234567891</ID>
				<TypeCode>FX</TypeCode>
			</Communication>
		</ContactOffice>
		<Status>
			<NameCode>4</NameCode>
		</Status>
		<Declaration>
			<FunctionalReferenceID>18GB123456789</FunctionalReferenceID>
			<ID>18GBJCM3USAFD2WD51</ID>
			<VersionID>1</VersionID>
			<GoodsShipment>
				<GovernmentAgencyGoodsItem>
					<SequenceNumeric>1</SequenceNumeric>
					<Commodity>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>9.70</TaxRateNumeric>
							<TypeCode>A00</TypeCode>
							<Payment>
								<TaxAssessedAmount>97.00</TaxAssessedAmount>
								<PaymentAmount>0.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>20.00</TaxRateNumeric>
							<TypeCode>B00</TypeCode>
							<Payment>
								<TaxAssessedAmount>219.40</TaxAssessedAmount>
								<PaymentAmount>0.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
					</Commodity>
				</GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
	</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);

				AssertEquals(2, entryLine.ConfirmedFees.Count);
				var a00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals(ZString.Empty, a00.CF_MethodOfPayment);
					AssertEquals(9.70m, a00.CF_Rate);
					AssertEquals(97.00m, a00.CF_ChargeAmount);
				}

				var b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals(ZString.Empty, b00.CF_MethodOfPayment);
					AssertEquals(20.0m, b00.CF_Rate);
					AssertEquals(219.40m, b00.CF_ChargeAmount);
				}
			});
		}

		public void TestSettingOfMethodOfPaymentForPVAWhenVATFeeIsZeroPayableButAlsoZeroAssessed()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "18GB123456789";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var lineMerger = new CDSLineMerger(dec);
			lineMerger.DoMerge();

			var reference = entry.EntryInstruction.FiscalReferences.AddNew();
			reference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			reference.CFR_Reference = "1234";

			AssertEquals("IsPostponedVatViaFiscalReference should be true when FR1 FiscalReference is present", true, entry.EntryInstruction.IsPostponedVatViaFiscalReference);

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
		<FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
		<IssueDateTime>
			<DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180119155400Z</DateTimeString>
		</IssueDateTime>
		<AppealOffice>
			<ID>GBLBA001</ID>
		</AppealOffice>
		<Bank>
			<ReferenceID>CITIGB2LLON</ReferenceID>
			<ID>GB25CITI08320011963155</ID>
		</Bank>
		<ContactOffice>
			<ID>GBLBA001</ID>
			<Communication>
				<ID>See Developer Hub</ID>
				<TypeCode>EM</TypeCode>
			</Communication>
			<Communication>
				<ID>+441234567891</ID>
				<TypeCode>FX</TypeCode>
			</Communication>
		</ContactOffice>
		<Status>
			<NameCode>4</NameCode>
		</Status>
		<Declaration>
			<FunctionalReferenceID>18GB123456789</FunctionalReferenceID>
			<ID>18GBJCM3USAFD2WD51</ID>
			<VersionID>1</VersionID>
			<GoodsShipment>
			  <GovernmentAgencyGoodsItem>
				<SequenceNumeric>1</SequenceNumeric>
				<Commodity>
					<DutyTaxFee>
						<AdValoremTaxBaseAmount currencyID=""GBP"">7610.33</AdValoremTaxBaseAmount>
						<DeductAmount currencyID=""GBP"">0.00</DeductAmount>
						<DutyRegimeCode>100</DutyRegimeCode>
						<TaxRateNumeric>12.00</TaxRateNumeric>
						<TypeCode>A00</TypeCode>
						<Payment>
							<TaxAssessedAmount currencyID=""GBP"">913.23</TaxAssessedAmount>
							<PaymentAmount currencyID=""GBP"">913.23</PaymentAmount>
						</Payment>
					</DutyTaxFee>
					<DutyTaxFee>
						<DeductAmount>0.00</DeductAmount>
						<DutyRegimeCode>100</DutyRegimeCode>
						<TaxRateNumeric>20.00</TaxRateNumeric>
						<TypeCode>B00</TypeCode>
						<Payment>
							<TaxAssessedAmount>219.40</TaxAssessedAmount>
							<PaymentAmount>0.00</PaymentAmount>
						</Payment>
					</DutyTaxFee>
					<DutyTaxFee>
						<AdValoremTaxBaseAmount currencyID=""GBP"">8557.24</AdValoremTaxBaseAmount>
						<DeductAmount currencyID=""GBP"">0.00</DeductAmount>
						<DutyRegimeCode>120</DutyRegimeCode>
						<TaxRateNumeric>0.00</TaxRateNumeric>
						<TypeCode>B00</TypeCode>
						<Payment>
							<TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
							<PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
						</Payment>
					</DutyTaxFee>
				</Commodity>
			  </GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
	</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals(3, entryLine.ConfirmedFees.Count);

				var a00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals("E", a00.CF_MethodOfPayment);
				}

				var b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00" && x.CF_BaseValue == 0);
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals("When PaymentAmount = 0 and PaymentAssessedAmount <> 0, CF_MethodOfPayment should be empty", ZString.Empty, b00.CF_MethodOfPayment);
				}

				b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00" && x.CF_BaseValue == 8557.24);
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals("When PaymentAmount = 0 and PaymentAssessedAmount = 0, CF_MethodOfPayment should be empty ", ZString.Empty, b00.CF_MethodOfPayment);
				}
			});
		}

		public void TestSaveDutiesFees_MoP_ZeroPaymentButNoFiscalReference()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "18GB123456789";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
		<FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
		<IssueDateTime>
			<DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180119155400Z</DateTimeString>
		</IssueDateTime>
		<AppealOffice>
			<ID>GBLBA001</ID>
		</AppealOffice>
		<Bank>
			<ReferenceID>CITIGB2LLON</ReferenceID>
			<ID>GB25CITI08320011963155</ID>
		</Bank>
		<ContactOffice>
			<ID>GBLBA001</ID>
			<Communication>
				<ID>See Developer Hub</ID>
				<TypeCode>EM</TypeCode>
			</Communication>
			<Communication>
				<ID>+441234567891</ID>
				<TypeCode>FX</TypeCode>
			</Communication>
		</ContactOffice>
		<Status>
			<NameCode>4</NameCode>
		</Status>
		<Declaration>
			<FunctionalReferenceID>18GB123456789</FunctionalReferenceID>
			<ID>18GBJCM3USAFD2WD51</ID>
			<VersionID>1</VersionID>
			<GoodsShipment>
				<GovernmentAgencyGoodsItem>
					<SequenceNumeric>1</SequenceNumeric>
					<Commodity>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>9.70</TaxRateNumeric>
							<TypeCode>A00</TypeCode>
							<Payment>
								<TaxAssessedAmount>97.00</TaxAssessedAmount>
								<PaymentAmount>0.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>20.00</TaxRateNumeric>
							<TypeCode>B00</TypeCode>
							<Payment>
								<TaxAssessedAmount>219.40</TaxAssessedAmount>
								<PaymentAmount>0.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
					</Commodity>
				</GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
	</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);

				AssertEquals(2, entryLine.ConfirmedFees.Count);
				var a00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals(ZString.Empty, a00.CF_MethodOfPayment);
					AssertEquals(9.70m, a00.CF_Rate);
					AssertEquals(97.00m, a00.CF_ChargeAmount);
				}

				var b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals(ZString.Empty, b00.CF_MethodOfPayment);
					AssertEquals(20.0m, b00.CF_Rate);
					AssertEquals(219.40m, b00.CF_ChargeAmount);
				}
			});
		}

		public void TestSaveDutiesFees_MoC()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "18GB123456789";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			var fee1 = entryLine.Fees.AddOrUpdate("A00", 138.50m);
			fee1.CF_BaseValue = 1000.00m;
			fee1.CF_MethodOfCalculation = "%";
			fee1.CF_MethodOfPayment = "M";
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "A00";
			fee2.CF_BaseValue = 0.50m;
			fee2.CF_MethodOfCalculation = "DTN";
			fee2.CF_Rate = 77.00m;
			fee2.CF_ChargeAmount = 38.50m;
			var fee3 = entryLine.Fees.AddOrUpdate("B00", 0.00m);
			fee3.CF_BaseValue = 1377.00m;
			fee3.CF_MethodOfCalculation = "%";
			fee3.CF_MethodOfPayment = "M";
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
		<FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
		<IssueDateTime>
			<DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180119155400Z</DateTimeString>
		</IssueDateTime>
		<AppealOffice>
			<ID>GBLBA001</ID>
		</AppealOffice>
		<Bank>
			<ReferenceID>CITIGB2LLON</ReferenceID>
			<ID>GB25CITI08320011963155</ID>
		</Bank>
		<ContactOffice>
			<ID>GBLBA001</ID>
			<Communication>
				<ID>See Developer Hub</ID>
				<TypeCode>EM</TypeCode>
			</Communication>
			<Communication>
				<ID>+441234567891</ID>
				<TypeCode>FX</TypeCode>
			</Communication>
		</ContactOffice>
		<Status>
			<NameCode>4</NameCode>
		</Status>
		<Declaration>
			<FunctionalReferenceID>18GB123456789</FunctionalReferenceID>
			<ID>18GBJCM3USAFD2WD51</ID>
			<VersionID>1</VersionID>
			<GoodsShipment>
				<GovernmentAgencyGoodsItem>
					<SequenceNumeric>1</SequenceNumeric>
					<Commodity>
						<DutyTaxFee>
							<DeductAmount currencyID=""GBP"">0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TypeCode>A00</TypeCode>
						   <Payment>
								<TaxAssessedAmount currencyID=""GBP"">138.50</TaxAssessedAmount>
								<PaymentAmount currencyID=""GBP"">138.50</PaymentAmount>
						   </Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<AdValoremTaxBaseAmount currencyID=""GBP"">1338.50</AdValoremTaxBaseAmount>
							<DeductAmount currencyID=""GBP"">0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>0.00</TaxRateNumeric>
							<TypeCode>B00</TypeCode>
							<Payment>
							  <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
							  <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
							</Payment>
						</DutyTaxFee>
					</Commodity>
				</GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
	</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);

				AssertEquals(2, entryLine.ConfirmedFees.Count);
				var a00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals(138.50m, a00.CF_ChargeAmount);
					AssertEquals("M", a00.CF_MethodOfPayment);
				}

				var b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals(0.00m, b00.CF_ChargeAmount);
					AssertEquals("M", b00.CF_MethodOfPayment);
					AssertEquals(1338.50m, b00.CF_BaseValue);
				}
			});
		}

		public void TestSaveDutiesFees_ChargeAmount()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "W35BFSPRD0000000052567";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			var fee1 = entryLine.Fees.AddOrUpdate("A00", 138.50m);
			fee1.CF_BaseValue = 1000.00m;
			fee1.CF_MethodOfCalculation = "%";
			fee1.CF_MethodOfPayment = "M";
			var fee2 = entryLine.Fees.AddOrUpdate("B00", 0.00m);
			fee2.CF_BaseValue = 1377.00m;
			fee2.CF_MethodOfCalculation = "%";
			fee2.CF_MethodOfPayment = "M";
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
  <FunctionalReferenceID>64d031ff80a94c91b272ddb03ddd7fc7</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20221024122228Z</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>BARCGB22</ReferenceID>
    <ID>GB16BARC20051723372545</ID>
  </Bank>
  <Status>
    <NameCode>67</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>W35BFSPRD0000000052567</FunctionalReferenceID>
    <ID>22GBBROUZ30ET3KAR5</ID>
    <VersionID>1</VersionID>
    <GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">69.69</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>6.9</TaxRateNumeric>
            <TypeCode>A00</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">560.00</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>2.70</TaxRateNumeric>
            <TypeCode>A50</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">610.00</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>0.00</TaxRateNumeric>
            <TypeCode>B00</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</Response>";

			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			CombineAssertions(() =>
			{
				var a00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals(69.69m, a00.CF_BaseValue);
					AssertEquals(6.90m, a00.CF_Rate);
					AssertEquals(0.00m, a00.CF_ChargeAmount);
				}

				var a50 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A50");
				AssertNotNull("ConfirmedFee A50", a50);
				if (a50 != null)
				{
					AssertEquals(560.00m, a50.CF_BaseValue);
					AssertEquals(2.70m, a50.CF_Rate);
					AssertEquals(0.00m, a50.CF_ChargeAmount);
				}
			});
		}

		public void TestSaveDutiesFees_NullPayment()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "B75SHEPRD0000000044258";

			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;

			var entryLineDutyDetails = entryLine.DutyDetails;
			var entryLineDutyDetailsForVAT = entryLine.DutyDetailsForVAT;
			var entryLineVATDetails = entryLine.VATDetails;
			var entryDuty = entry.Duty;

			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
  <FunctionalReferenceID>91c5be833e67468c8a5d39f694a526ba</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20221214121745Z</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>BARCGB22</ReferenceID>
    <ID>GB16BARC20051723372545</ID>
  </Bank>
  <Status>
    <NameCode>4</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>B75SHEPRD0000000044258</FunctionalReferenceID>
    <ID>22GBDSIY5VL0PWIAR0</ID>
    <VersionID>1</VersionID>
    <DutyTaxFee>
      <Payment>
        <ReferenceID>CDSI22L0PWIAR000</ReferenceID>
        <TaxAssessedAmount currencyID=""GBP"">43948.68</TaxAssessedAmount>
        <PaymentAmount currencyID=""GBP"">43948.68</PaymentAmount>
      </Payment>
    </DutyTaxFee>
    <GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">219743.41</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>0.00</TaxRateNumeric>
            <TypeCode>A00</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">219743.41</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>20.00</TaxRateNumeric>
            <TypeCode>B00</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">43948.68</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>T24</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">43948.68</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Duties and taxes have been calculated and are due</H3><p><strong>Function Code: </strong>13-TAX<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>Bank: </strong>GB16BARC20051723372545<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>22GBDSIY5VL0PWIAR0<br><strong>LRN: </strong>B75SHEPRD0000000044258<br><strong>Issued Date: </strong>2022-12-14 12:17</p><p>Status Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>4</td><td>Final customs debt</td><td>&nbsp;</td></tr></table></p><p>Payment Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Item</td><td>Tax Type or Reference</td><td>Payment Amount</td><td>Assessed Amount</td><td>Tax Rate</td><td>Tax Base</td></tr><tr><td>Item 1</td><td>A00</td><td>GBP0.00</td><td>GBP0.00</td><td>0.00%</td><td>219743.41</td></tr><tr><td>Item 1</td><td>B00</td><td>GBP0.00</td><td>GBP43948.68</td><td>20.00%</td><td>219743.41</td></tr><tr><td>Item 1</td><td>T24</td><td>0</td><td>GBP43948.68</td><td>0%</td><td>&nbsp;</td></tr><tr><td>Total</td><td>CDSI22L0PWIAR000</td><td>43948.68</td><td>43948.68</td><td>&nbsp;</td><td>&nbsp;</td></tr></table></p>", ediMessage.EM_MessageInterpretation);

				AssertEquals("ConfirmedFees.Count", 3, entryLine.ConfirmedFees.Count);

				var a00 = entryLine.ConfirmedFees.Cast<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "A00");
				AssertNotNull("ConfirmedFee A00", a00);
				if (a00 != null)
				{
					AssertEquals(0m, a00.CF_Rate);
					AssertEquals(0m, a00.CF_ChargeAmount);
					AssertEquals("E", a00.CF_MethodOfPayment);
					AssertEquals(219743.41m, a00.CF_BaseValue);
				}

				var b00 = entryLine.ConfirmedFees.Cast<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
				AssertNotNull("ConfirmedFee B00", b00);
				if (b00 != null)
				{
					AssertEquals(20.0m, b00.CF_Rate);
					AssertEquals(43948.68m, b00.CF_ChargeAmount);
					AssertEquals("E", b00.CF_MethodOfPayment);
					AssertEquals(219743.41m, b00.CF_BaseValue);
				}

				var t24 = entryLine.ConfirmedFees.Cast<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "T24");
				AssertNotNull("ConfirmedFee T24", t24);
				if (t24 != null)
				{
					AssertEquals(43948.68m, t24.CF_ChargeAmount);
					AssertEquals(true, t24.CF_IsLandedCostOnly);
				}

				AssertEquals(entryLineDutyDetails, entryLine.DutyDetails);
				AssertEquals(entryLineDutyDetailsForVAT, entryLine.DutyDetailsForVAT);
				AssertEquals(entryLineVATDetails, entryLine.VATDetails);
				AssertEquals(entryDuty, entry.Duty);
				AssertEquals(43948.68m, entry.VAT);
			});
		}

		public void TestConfirmedFeesAreReplaced()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "18GB123456789";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			entryLine.Fees.AddOrUpdate("A00", 97.05m).CF_BaseValue = 10000;
			entryLine.Fees.AddOrUpdate("B00", 219.50m).CF_BaseValue = 9999;
			entryLine.ConfirmedFees.AddOrUpdate("A00", 97.05m).CF_BaseValue = 10000;
			entryLine.ConfirmedFees.AddOrUpdate("B00", 219.50m).CF_BaseValue = 9998;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
		<FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
		<IssueDateTime>
			<DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180119155400Z</DateTimeString>
		</IssueDateTime>
		<AppealOffice>
			<ID>GBLBA001</ID>
		</AppealOffice>
		<Bank>
			<ReferenceID>CITIGB2LLON</ReferenceID>
			<ID>GB25CITI08320011963155</ID>
		</Bank>
		<ContactOffice>
			<ID>GBLBA001</ID>
			<Communication>
				<ID>See Developer Hub</ID>
				<TypeCode>EM</TypeCode>
			</Communication>
			<Communication>
				<ID>+441234567891</ID>
				<TypeCode>FX</TypeCode>
			</Communication>
		</ContactOffice>
		<Status>
			<NameCode>4</NameCode>
		</Status>
		<Declaration>
			<FunctionalReferenceID>18GB123456789</FunctionalReferenceID>
			<ID>18GBJCM3USAFD2WD51</ID>
			<VersionID>1</VersionID>
			<GoodsShipment>
				<GovernmentAgencyGoodsItem>
					<SequenceNumeric>1</SequenceNumeric>
					<Commodity>
						<DutyTaxFee>
							<AdValoremTaxBaseAmount currencyID=""GBP"">1338.50</AdValoremTaxBaseAmount>
							<DeductAmount>0.00</DeductAmount>
							<DutyRegimeCode>100</DutyRegimeCode>
							<TaxRateNumeric>20.00</TaxRateNumeric>
							<TypeCode>B00</TypeCode>
							<Payment>
								<TaxAssessedAmount>267.70</TaxAssessedAmount>
								<PaymentAmount>267.70</PaymentAmount>
							</Payment>
						</DutyTaxFee>
					</Commodity>
				</GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
	</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			AssertEquals(1, entryLine.ConfirmedFees.Count);
			var b00 = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
			AssertNotNull("ConfirmedFee B00", b00);
			CombineAssertions(() =>
			{
				if (b00 != null)
				{
					AssertEquals(20.0m, b00.CF_Rate);
					AssertEquals(267.70m, b00.CF_ChargeAmount);
					AssertEquals("E", b00.CF_MethodOfPayment);
					AssertEquals(1338.50m, b00.CF_BaseValue);
				}
			});
		}

		public void TestSaveDutiesFeesAtHeaderLevelCPR()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "HYEDUKMIK0000000001046";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>15</FunctionCode>
  <FunctionalReferenceID>501535a71c434aae86cd44abd0c441aa</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190620145200+01</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>CITIGB2LLON</ReferenceID>
    <ID>GB25CITI08320011963155</ID>
  </Bank>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>19GB6QAQC9LG6FGVR7</ID>
    <VersionID>1</VersionID>
    <DutyTaxFee>
      <Payment>
        <ReferenceID>7e9bb91</ReferenceID>
        <PaymentAmount>21539.76</PaymentAmount>
      </Payment>
    </DutyTaxFee>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Payment is due (reminder)</H3><p><strong>Function Code: </strong>15-CPR<br><strong>Bank: </strong>GB25CITI08320011963155<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>19GB6QAQC9LG6FGVR7<br><strong>LRN: </strong>HYEDUKMIK0000000001046<br><strong>Issued Date: </strong>2019-06-20 14:52</p><p>Payment Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Item</td><td>Tax Type or Reference</td><td>Payment Amount</td><td>Assessed Amount</td><td>Tax Rate</td><td>Tax Base</td></tr><tr><td>Total</td><td>7e9bb91</td><td>21539.76</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table></p>", ediMessage.EM_MessageInterpretation);
		}

		public void TestSaveDutiesFeesAtHeaderLevelTax()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "HYEDUKMIK0000000001046";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
  <FunctionalReferenceID>ea9319785ba44b5c8dbd1abdaeb50b9d</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190619145101+01</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>CITIGB2LLON</ReferenceID>
    <ID>GB25CITI08320011963155</ID>
  </Bank>
  <Status>
    <NameCode>4</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>19GB6QAQC9LG6FGVR7</ID>
    <VersionID>1</VersionID>
    <DutyTaxFee>
      <Payment>
        <ReferenceID>7e9bb91</ReferenceID>
        <TaxAssessedAmount currencyID=""GBP"">22639.76</TaxAssessedAmount>
        <PaymentAmount currencyID=""GBP"">22639.76</PaymentAmount>
      </Payment>
    </DutyTaxFee>
    <GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <DeductAmount>0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>2.20</TaxRateNumeric>
            <TypeCode>A00</TypeCode>
            <Payment>
              <TaxAssessedAmount>2199.97</TaxAssessedAmount>
              <PaymentAmount>2199.97</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <DeductAmount>0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>20.00</TaxRateNumeric>
            <TypeCode>B00</TypeCode>
            <Payment>
              <TaxAssessedAmount>20439.79</TaxAssessedAmount>
              <PaymentAmount>20439.79</PaymentAmount>
            </Payment>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Duties and taxes have been calculated and are due</H3><p><strong>Function Code: </strong>13-TAX<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>Bank: </strong>GB25CITI08320011963155<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>19GB6QAQC9LG6FGVR7<br><strong>LRN: </strong>HYEDUKMIK0000000001046<br><strong>Issued Date: </strong>2019-06-19 14:51</p><p>Status Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>4</td><td>Final customs debt</td><td>&nbsp;</td></tr></table></p><p>Payment Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Item</td><td>Tax Type or Reference</td><td>Payment Amount</td><td>Tax Rate</td><td>Tax Base</td></tr><tr><td>Item 1</td><td>A00</td><td>2199.97</td><td>2.20%</td><td>&nbsp;</td></tr><tr><td>Item 1</td><td>B00</td><td>20439.79</td><td>20.00%</td><td>&nbsp;</td></tr><tr><td>Total</td><td>7e9bb91</td><td>22639.76</td><td>&nbsp;</td><td>&nbsp;</td></tr></table></p>", ediMessage.EM_MessageInterpretation);
		}

		public void TestPostponedVatAccountingB00()
		{
			CommonSaveDutiesFeesResponseTesting(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, Constants.MethodOfPayment.DeferredPayment, 0, Constants.MethodOfPayment.DeferredPayment);
		}

		public void TestPostponedVatAccountingWithPayment()
		{
			CommonSaveDutiesFeesResponseTesting(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, Constants.MethodOfPayment.DeferredPayment, 9.70m, Constants.MethodOfPayment.DeferredPayment);
		}

		public void TestPostponedVatAccountingB05()
		{
			CommonSaveDutiesFeesResponseTesting(EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, Constants.MethodOfPayment.DeferredPayment, 0, Constants.MethodOfPayment.DeferredPayment);
		}

		public void TestPostponedVatAccountingA00() // specific test to verify the changes for PostponedVatAccounting do not apply when DutyCodeType not in ['B00','B05']
		{
			CommonSaveDutiesFeesResponseTesting(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, Constants.MethodOfPayment.DeferredPayment, 0, Constants.MethodOfPayment.DeferredPayment);
		}

		void CommonSaveDutiesFeesResponseTesting(string responseDutyTypeCode, string initialPaymentType, decimal paymentAmount, string expectedPaymentType)
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = initialPaymentType;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "HYEDUKMIK0000000001046";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = $@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
  <FunctionalReferenceID>ea9319785ba44b5c8dbd1abdaeb50b9d</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190619145101+01</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>CITIGB2LLON</ReferenceID>
    <ID>GB25CITI08320011963155</ID>
  </Bank>
  <Status>
    <NameCode>4</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>19GB6QAQC9LG6FGVR7</ID>
    <VersionID>1</VersionID>
	<GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <DeductAmount>0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>9.70</TaxRateNumeric>
            <TypeCode>{responseDutyTypeCode}</TypeCode>
            <Payment>
              <TaxAssessedAmount>97.00</TaxAssessedAmount>
              <PaymentAmount>{paymentAmount:0.00}</PaymentAmount>
            </Payment>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</Response>";

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);

			var fee = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault();
			AssertNotNull(fee);
			AssertEquals(expectedPaymentType, fee.CF_MethodOfPayment);

			fee.CF_MethodOfPayment = initialPaymentType;
			processor.ProcessMessage(ediMessage);

			fee = entryLine.ConfirmedFees.OfType<Customs.Business.CusEntryLineFee>().FirstOrDefault();
			AssertNotNull(fee);
			AssertEquals(initialPaymentType, fee.CF_MethodOfPayment);

			if (paymentAmount == 0)
			{
				AssertEquals(97.00m, fee.CF_ChargeAmount);
				AssertEquals(true, fee.CF_IsLandedCostOnly);
			}
			else
			{
				AssertEquals(paymentAmount, fee.CF_ChargeAmount);
				AssertEquals(false, fee.CF_IsLandedCostOnly);
			}
		}

		public void TestCashPayments_A()
		{
			TestCashPayments(MethodOfPaymentCodes.A, true);
		}

		public void TestCashPayments_E()
		{
			TestCashPayments(MethodOfPaymentCodes.E, false);
		}

		public void TestCashPayments_P()
		{
			TestCashPayments(MethodOfPaymentCodes.P, true);
		}

		void TestCashPayments(string methodOfPayment, bool shouldCreateCusEntryPayInfo)
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = methodOfPayment;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "HYEDUKMIK0000000001046";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
  <FunctionalReferenceID>2e346fa6308a48efba63804c9997a135</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20210317044238Z</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>CITIGB2LLON</ReferenceID>
    <ID>GB25CITI08320011963155</ID>
  </Bank>
  <Status>
    <NameCode>4</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>21GB2YOEGYNH56EQR2</ID>
    <VersionID>2</VersionID>
    <DutyTaxFee>
      <Payment>
        <ReferenceID>CDSI21NH56EQR200</ReferenceID>
        <TaxAssessedAmount currencyID=""GBP"">79.32</TaxAssessedAmount>
        <PaymentAmount currencyID=""GBP"">79.32</PaymentAmount>
      </Payment>
    </DutyTaxFee>
    <GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">27.95</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>6.50</TaxRateNumeric>
            <TypeCode>A50</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">1.81</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">1.81</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">1.81</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>20.00</TaxRateNumeric>
            <TypeCode>B05</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">27.95</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>0.00</TaxRateNumeric>
            <TypeCode>B00</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
         </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</Response>";
			ediMessage.EM_MessageNum = "TEST1";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			var payments = entry.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();

			if (shouldCreateCusEntryPayInfo)
			{
				AssertEquals($"Processed TAX message for entry with MoP=\"{methodOfPayment}\" should create one CusEntryPayInfo record", 1, payments.Length);
				CombineAssertions($"Processed TAX message for entry with MoP=\"{methodOfPayment}\":", () =>
				{
					AssertEquals("C9_IncomingPayResponseNo", "TEST1", payments[0].C9_IncomingPayResponseNo);
					AssertEquals("C9_PaymentAmount", 79.32m, payments[0].C9_PaymentAmount);
					AssertEquals("C9_TransactionType", PaymentTransactionTypeList.Codes.Cash, payments[0].C9_TransactionType);
					Assert("C9_PaymentDate should be empty", payments[0].C9_PaymentDate.IsEmpty);
					AssertEquals("C9_PaymentStatus", PaymentStatusList.Codes.FinalUnpaid, payments[0].C9_PaymentStatus);
					AssertEquals("C9_PaymentReference", "CDSI21NH56EQR200", payments[0].C9_PaymentReference);
					AssertEquals("C9_ReceiptDate", new ZDate(2021, 3, 17), payments[0].C9_ReceiptDate);
				});
			}
			else
			{
				AssertEquals($"Processed TAX message for entry with MoP=\"{methodOfPayment}\" should not create any CusEntryPayInfo records", 0, payments.Length);
			}

			var ediMessage2 = Factory.New<CDSResponseEDIMessage>();
			ediMessage2.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>09</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20210318121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			ediMessage2.EM_MessageNum = "TEST2";
			entry.Messages.Add(ediMessage2);

			processor.ProcessMessage(ediMessage2);

			if (shouldCreateCusEntryPayInfo)
			{
				AssertEquals($"Processed CLR message for entry with MoP=\"{methodOfPayment}\" should update existing CusEntryPayInfo record", 1, payments.Length);
				CombineAssertions($"Processed CLR message for entry with MoP=\"{methodOfPayment}\":", () =>
				{
					AssertEquals("C9_IncomingPayResponseNo", "TEST2", payments[0].C9_IncomingPayResponseNo);
					AssertEquals("C9_PaymentAmount", 79.32m, payments[0].C9_PaymentAmount);
					AssertEquals("C9_TransactionType", "CAS", payments[0].C9_TransactionType);
					AssertEquals("C9_PaymentDate", new ZDateTime(2021, 3, 18, 12, 12, 12), payments[0].C9_PaymentDate);
					AssertEquals("C9_PaymentStatus", PaymentStatusList.Codes.FinalPaid, payments[0].C9_PaymentStatus);
					AssertEquals("C9_PaymentReference", "CDSI21NH56EQR200", payments[0].C9_PaymentReference);
					AssertEquals("C9_ReceiptDate", new ZDate(2021, 3, 17), payments[0].C9_ReceiptDate);
				});
			}
			else
			{
				AssertEquals($"Processed CLR message for entry with MoP=\"{methodOfPayment}\" should not create any CusEntryPayInfo records", 0, payments.Length);
			}
		}

		public void TestPostponedVatPaymentsWithFR1FiscalReference()
		{
			TestPVAPayments(MethodOfPaymentCodes.P, true, true);
		}

		public void TestPostponedVatPaymentsWithoutFR1FiscalReference()
		{
			TestPVAPayments(MethodOfPaymentCodes.P, true, false);
		}

		void TestPVAPayments(string methodOfPayment, bool shouldCreateCusEntryPayInfo, bool shouldCreatePVAPayInfo)
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction = dec.CustomsEntryInstructions.FirstOrDefault();
			if (shouldCreatePVAPayInfo)
			{
				var reference = entryInstruction.FiscalReferences.AddNew();
				reference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR1_Importer;
				reference.CFR_Reference = "AB123";
			}
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = methodOfPayment;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "HYEDUKMIK0000000001046";
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
  <FunctionalReferenceID>2e346fa6308a48efba63804c9997a135</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20210317044238Z</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>CITIGB2LLON</ReferenceID>
    <ID>GB25CITI08320011963155</ID>
  </Bank>
  <Status>
    <NameCode>4</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>21GB2YOEGYNH56EQR2</ID>
    <VersionID>2</VersionID>
    <DutyTaxFee>
      <Payment>
        <ReferenceID>CDSI21NH56EQR200</ReferenceID>
        <TaxAssessedAmount currencyID=""GBP"">79.32</TaxAssessedAmount>
        <PaymentAmount currencyID=""GBP"">79.32</PaymentAmount>
      </Payment>
    </DutyTaxFee>
    <GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">27.95</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>6.50</TaxRateNumeric>
            <TypeCode>A50</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">1.81</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">1.81</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">1.81</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>20.00</TaxRateNumeric>
            <TypeCode>B05</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">5.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">6.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <AdValoremTaxBaseAmount currencyID=""GBP"">27.95</AdValoremTaxBaseAmount>
            <DeductAmount currencyID=""GBP"">0.00</DeductAmount>
            <DutyRegimeCode>100</DutyRegimeCode>
            <TaxRateNumeric>0.00</TaxRateNumeric>
            <TypeCode>B00</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""GBP"">7.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">8.00</PaymentAmount>
            </Payment>
         </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</Response>";
			ediMessage.EM_MessageNum = "TEST1";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			var cashPayment = entry.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(x => x.C9_TransactionType == PaymentTransactionTypeList.Codes.Cash);

			if (shouldCreateCusEntryPayInfo)
			{
				AssertNotNull($"Processed TAX message for entry with MoP=\"{methodOfPayment}\" should create one CusEntryPayInfo record", cashPayment);
				CombineAssertions($"Processed TAX message for entry with MoP=\"{methodOfPayment}\":", () =>
				{
					AssertEquals("C9_IncomingPayResponseNo", "TEST1", cashPayment.C9_IncomingPayResponseNo);
					AssertEquals("C9_PaymentAmount", 79.32m, cashPayment.C9_PaymentAmount);
					AssertEquals("C9_TransactionType", PaymentTransactionTypeList.Codes.Cash, cashPayment.C9_TransactionType);
					Assert("C9_PaymentDate should be empty", cashPayment.C9_PaymentDate.IsEmpty);
					AssertEquals("C9_PaymentStatus", PaymentStatusList.Codes.FinalUnpaid, cashPayment.C9_PaymentStatus);
					AssertEquals("C9_PaymentReference", "CDSI21NH56EQR200", cashPayment.C9_PaymentReference);
					AssertEquals("C9_ReceiptDate", new ZDate(2021, 3, 17), cashPayment.C9_ReceiptDate);
				});
			}
			else
			{
				AssertNull($"Processed TAX message for entry with MoP=\"{methodOfPayment}\" should not create  CusEntryPayInfo record for cash", cashPayment);
			}

			var pvaPayment = entry.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(x => x.C9_TransactionType == PaymentTransactionTypeList.Codes.PostponedVATAccounting);

			if (shouldCreatePVAPayInfo)
			{
				AssertNotNull("Should create pay info for PVA", pvaPayment);
				CombineAssertions($"Processed TAX message for entry with MoP=\"{methodOfPayment}\":", () =>
				{
					AssertEquals("C9_IncomingPayResponseNo", "TEST1", pvaPayment.C9_IncomingPayResponseNo);
					AssertEquals("C9_PaymentAmount", 12m, pvaPayment.C9_PaymentAmount);
					AssertEquals("C9_TransactionType", PaymentTransactionTypeList.Codes.PostponedVATAccounting, pvaPayment.C9_TransactionType);
					Assert("C9_PaymentDate should be empty", pvaPayment.C9_PaymentDate.IsEmpty);
					AssertEquals("C9_PaymentStatus", PaymentStatusList.Codes.FinalUnpaid, cashPayment.C9_PaymentStatus);
					AssertEquals("C9_PaymentReference", "AB123", pvaPayment.C9_PaymentReference);
					AssertEquals("C9_ReceiptDate", new ZDate(2021, 3, 17), pvaPayment.C9_ReceiptDate);
				});
			}
			else
			{
				AssertNull($"Processed TAX message for entry with MoP=\"{methodOfPayment}\" should not create  CusEntryPayInfo record for PVA", pvaPayment);
			}

			var ediMessage2 = Factory.New<CDSResponseEDIMessage>();
			ediMessage2.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>09</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20210318121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			ediMessage2.EM_MessageNum = "TEST2";
			entry.Messages.Add(ediMessage2);

			processor.ProcessMessage(ediMessage2);

			if (shouldCreateCusEntryPayInfo)
			{
				AssertNotNull($"Processed CLR message for entry with MoP=\"{methodOfPayment}\" should update existing CusEntryPayInfo record", cashPayment);
				CombineAssertions($"Processed CLR message for entry with MoP=\"{methodOfPayment}\":", () =>
				{
					AssertEquals("C9_IncomingPayResponseNo", "TEST2", cashPayment.C9_IncomingPayResponseNo);
					AssertEquals("C9_PaymentAmount", 79.32m, cashPayment.C9_PaymentAmount);
					AssertEquals("C9_TransactionType", PaymentTransactionTypeList.Codes.Cash, cashPayment.C9_TransactionType);
					AssertEquals("C9_PaymentDate", new ZDateTime(2021, 3, 18, 12, 12, 12), cashPayment.C9_PaymentDate);
					AssertEquals("C9_PaymentStatus", PaymentStatusList.Codes.FinalPaid, cashPayment.C9_PaymentStatus);
					AssertEquals("C9_PaymentReference", "CDSI21NH56EQR200", cashPayment.C9_PaymentReference);
					AssertEquals("C9_ReceiptDate", new ZDate(2021, 3, 17), cashPayment.C9_ReceiptDate);
				});
			}
			else
			{
				AssertNull($"Processed CLR message for entry with MoP=\"{methodOfPayment}\" should not create any CusEntryPayInfo records", cashPayment);
			}

			if (shouldCreatePVAPayInfo)
			{
				AssertNotNull("Should create pay info for PVA", pvaPayment);
				CombineAssertions($"Processed TAX message for entry with MoP=\"{methodOfPayment}\":", () =>
				{
					AssertEquals("C9_IncomingPayResponseNo", "TEST2", pvaPayment.C9_IncomingPayResponseNo);
					AssertEquals("C9_PaymentAmount", 12m, pvaPayment.C9_PaymentAmount);
					AssertEquals("C9_TransactionType", PaymentTransactionTypeList.Codes.PostponedVATAccounting, pvaPayment.C9_TransactionType);
					AssertEquals("C9_PaymentDate", new ZDateTime(2021, 3, 18, 12, 12, 12), pvaPayment.C9_PaymentDate);
					AssertEquals("C9_PaymentStatus", PaymentStatusList.Codes.FinalPaid, pvaPayment.C9_PaymentStatus);
					AssertEquals("C9_PaymentReference", "AB123", pvaPayment.C9_PaymentReference);
					AssertEquals("C9_ReceiptDate", new ZDate(2021, 3, 17), pvaPayment.C9_ReceiptDate);
				});
			}
			else
			{
				AssertNull($"Processed TAX message for entry with MoP=\"{methodOfPayment}\" should not create  CusEntryPayInfo record for PVA", pvaPayment);
			}
		}

		public void TestPVAPaymentsForPaymentDue()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction = dec.CustomsEntryInstructions.FirstOrDefault();
			var reference = entryInstruction.FiscalReferences.AddNew();
			reference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR1_Importer;
			reference.CFR_Reference = "AB123";
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = MethodOfPaymentCodes.P;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "HYEDUKMIK0000000001046";
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invLine.JI_CL = entryLine.PK;
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>15</FunctionCode>
  <FunctionalReferenceID>5fe555a1096240c5ac201854dd25b53b</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20240104164221Z</DateTimeString>
  </IssueDateTime>
  <Bank>
    <ReferenceID>BARCGB22</ReferenceID>
    <ID>GB16BARC20051723372545</ID>
  </Bank>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000001046</FunctionalReferenceID>
    <ID>23GBDEG33VSGULTAR0</ID>
    <VersionID>1</VersionID>
    <DutyTaxFee>
      <Payment>
        <ReferenceID>CDSI23SGULTAR000</ReferenceID>
        <PaymentAmount>300.00</PaymentAmount>
      </Payment>
    </DutyTaxFee>
  </Declaration>
</Response>";
			ediMessage.EM_MessageNum = "TEST1";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			AssertNoExceptionThrown("No exception should be thrown when processing a Payment due (15) message", () => processor.ProcessMessage(ediMessage));
		}

		public void TestFindingJob()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B0001000";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB1223456789000-B00XXXX";
			Factory.Save();

			var outgoingInterchange = Factory.New<EDIInterchange>();
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
			outgoingInterchange.EI_HeaderText = "";
			var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(CDSNewDeclarationEDIMessage));
			entry.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.EM_ApplicationCode = "";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingSentMessage.EM_MessageNum = "999";
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "AAW";
			outgoingInterchange.EI_BodyText = "";

			var testEventXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B0001000</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, testEventXml, Events.WarehouseJobCanNowBeFinalised.Code, eHubTrackingIdGuid));
			var subscriber = GetNewEventParentFinder();
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			var declaration = (JobDeclaration)logParents.First();
			entry.Reload();
			AssertEquals(2, entry.Messages.Count);
			var messageWithCID = entry.Messages.OfType<EDIMessage>().FirstOrDefault(x => x.EM_ApplicationReference == "d03f84e3b5894aaaa1f8cafd744add8e");
			AssertNotNull(messageWithCID);

			var ediMessage = Factory.New<CDSResponseEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_ApplicationReference = "d03f84e3b5894aaaa1f8cafd744add8e"; // This would be set by CDSInboundInterchangeProcessor when processing an incoming Interchange from eHub
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("15GB000060100C85A5", entry.MovementReferenceNumber);

			ediMessage.Delete();
			ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

			processor.ProcessMessage(ediMessage);

			AssertEquals("Finding Job from MRN", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
		}

		public void TestSendEntryDocs()
		{
			var (dec, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs();
			processor.ProcessMessage(ediMessage);

			CombineAssertions(() =>
			{
				var storageMain = dec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(dec, Core.Constants.DocManagerCodes.JobDeclaration);
				AssertEquals(1, storageMain.Files.Count);
				AssertEquals("CDS Entry Document - BGM123456789000-S0001000.pdf", storageMain.Files[0].FileName);
			});

			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				var storageMain = dec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(dec, Core.Constants.DocManagerCodes.JobDeclaration);
				AssertEquals(1, storageMain.Files.Count);
				AssertEquals("CDS Entry Document - BGM123456789000-S0001000.pdf", storageMain.Files[0].FileName);
			});

			var docType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.EntryPrint));
			docType.RT_OverrideVersions = false;
			Factory.Save();
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				var storageMain = dec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(dec, Core.Constants.DocManagerCodes.JobDeclaration);
				AssertEquals(2, storageMain.Files.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "CDS Entry Document - BGM123456789000-S0001000", "CDS Entry Document - BGM123456789000-S0001000[2]" }, storageMain.Files.Cast<StorageFile>().Select(x => x.SC_FileName));
			});

			CombineAssertions(() =>
			{
				Assert("Log for DDA not added", dec.CustomsEntryHeaders[0].Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code).Any());
				Assert("Event for DDA not added", dec.CustomsEntryHeaders[0].Logs.Find(x => x.Event.SE_Code == Events.DocumentAllocated.Code).Any());
			});
		}

		(JobDeclaration dec, CDSResponseEDIMessage ediMessage, CDSResponseMessageProcessor processor) CreateDeclarationAndResponseMessageForTestingSendEntryDocs(bool export = true)
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = export ? SharedJobMessageTypeList.Codes.Export : SharedJobMessageTypeList.Codes.Import;
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			if (dec.CustomsEntryInstructions.Count == 0)
			{
				var cei = dec.CustomsEntryInstructions.AddNew();
				cei.CEI_Style = export ? EntryStyleListExport.Codes.ExportNormal : EntryStyleListImport.Codes.ImportNormal;
				cei.CEI_SubStyle = export ? EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD : EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived;
			}
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = dec.CustomsEntryInstructions.FirstOrDefault().PK;
			entry.CH_BGMReference = "BGM123456789000-S0001000";
			entry.LRN = "LRN123456789000-S0001000";
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727131212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <AcceptanceDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727123456+01</DateTimeString>
    </AcceptanceDateTime>
    <FunctionalReferenceID>LRN123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(Logger);
			return (dec, ediMessage, processor);
		}

		[TestDate(2023, 5, 21)]
		public void TestEntryPrintDates()
		{
			var (dec, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs();

			var log = dec.CustomsEntryHeaders[0].Logs.AddNew(AutoEvents.CustomsEntryStatus, ThreeCharFunctionCode.Codes.CLE);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2023, 5, 20, 14, 0, 0);
			}

			processor.ProcessMessage(ediMessage);

			var storageMain = dec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(dec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using (var memoryStream = new MemoryStream(storageMain.Files[storageMain.Files.Count - 1].ImageData))
			using (var doc = new PdfDocument(memoryStream))
			{
				var documentText = doc.GetAllText();

				AssertContains("Issued time: 2018-07-27 13:12", documentText);
				AssertContains("Accepted time: 2018-07-27 12:34", documentText);
				AssertContains("Clearance time: 2023-05-20 14:00", documentText);
			}
		}

		[TestDate(2023, 5, 26)]
		public void TestEntryPrintClearanceDate()
		{
			var testHelper = new CDSResponseStatusTestDataHelper(Factory);
			testHelper.CreateCDSCustomsStatuses();
			var cusCodeList = testHelper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
				Constants.NumbericFunctionCodes.DeclarationCleared, "Declaration is now cleared",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeList.Attributes.AddNew("ISendEntryDocs", ZString.Empty);
			Factory.Save();

			var (dec, _, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs(export: false);

			var clearMessage = Factory.New<CDSResponseEDIMessage>();
			clearMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>09</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20230526121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>LRN123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			dec.CustomsEntryHeaders[0].Messages.Add(clearMessage);
			processor.ProcessMessage(clearMessage);

			var storageMain = dec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(dec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using (var memoryStream = new MemoryStream(storageMain.Files[storageMain.Files.Count - 1].ImageData))
			using (var doc = new PdfDocument(memoryStream))
			{
				var documentText = doc.GetAllText();

				AssertContains("Clearance time: 2023-05-26", documentText);
			}
		}

		void FailIfEmptyDocumentStorage(IStorageMain storageMain)
		{
			if (storageMain.Files.Count == 0)
			{
				Assert("Document was not created. Logger says: " + string.Join(System.Environment.NewLine, Logger.Logs.Select(x => x.Message)), condition: false);
			}
		}

		public void TestEntryPrintPaymentSummary_Export()
		{
			var (exportDec, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs();
			processor.ProcessMessage(ediMessage);

			var storageMain = exportDec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(exportDec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using (var memoryStream = new MemoryStream(storageMain.Files[0].ImageData))
			using (var doc = new PdfDocument(memoryStream))
			{
				var documentText = doc.GetAllText();
				AssertNotContains("Payment summary", documentText);
			}
		}

		public void TestEntryPrintPaymentSummary_Import()
		{
			var (importDec, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs(export: false);
			processor.ProcessMessage(ediMessage);

			var storageMain = importDec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(importDec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using (var memoryStream = new MemoryStream(storageMain.Files[0].ImageData))
			using (var doc = new PdfDocument(memoryStream))
			{
				var documentText = doc.GetAllText();
				AssertContains("Payment summary", documentText);
			}
		}

		public void TestEntryPrintExitResults_Import()
		{
			var (exportDec, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs(export: false);
			processor.ProcessMessage(ediMessage);

			var storageMain = exportDec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(exportDec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using var memoryStream = new MemoryStream(storageMain.Files[0].ImageData);
			using var doc = new PdfDocument(memoryStream);
			var documentText = doc.GetAllText();
			AssertNotContains("Exit Results", documentText);
		}

		public void TestEntryPrintExitResults_Export()
		{
			var (importDec, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs(export: true);

			var entry = (CusEntryHeader)importDec.ActiveEntryHeaders[0];
			entry.CH_ExitDate = new ZDateTime(2024, 3, 21);
			entry.CH_ExitActualOffice = "GB000084";
			entry.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;

			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>16</FunctionCode>
  <FunctionalReferenceID>9d286890f130400188c39039608969b5</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20240321095924Z</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementCode />
    <StatementDescription>GB000084</StatementDescription>
    <StatementTypeCode>CEX</StatementTypeCode>
  </AdditionalInformation>
  <Status>
    <EffectiveDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20240321000000Z</DateTimeString>
    </EffectiveDateTime>
    <NameCode>A1</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>LRN123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
    <VersionID>2</VersionID>
  </Declaration>
</Response>";
			processor.ProcessMessage(ediMessage);

			var storageMain = importDec.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(importDec, Core.Constants.DocManagerCodes.JobDeclaration);
			FailIfEmptyDocumentStorage(storageMain);
			using var memoryStream = new MemoryStream(storageMain.Files[0].ImageData);
			using var doc = new PdfDocument(memoryStream);
			var documentText = doc.GetAllText();
			AssertContains("Exit Results", documentText);
			AssertContains($"Status {ExportExitStatus.Codes.ExitedSatisfactorily} {ExportExitStatus.Descriptions.ExitedSatisfactorily} Date 2024-03-21 Office GB000084", documentText);
		}

		public void TestChallenges()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = dec.CustomsEntryInstructions[0].PK;
			entry.LRN = "LRN123456789000-S0001000";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;
			invLine1.JI_CustomsQuantity = 1000;
			invLine1.JI_LinePrice = 2000;
			Factory.Save();

			AssertChallenges(entry, entryLine1, "Value per kilo appears too high for this commodity", new string[] { FECChallengeFields.Codes.JI_NettMass, FECChallengeFields.Codes.JI_Price }, new ZString[] { invLine1.JI_CustomsQuantity.ToString(), invLine1.JI_LinePrice.ToString() });
			AssertChallenges(entry, entryLine1, "Value does not appear credible for commodity weight", new string[] { FECChallengeFields.Codes.JI_NettMass, FECChallengeFields.Codes.JI_Price }, new ZString[] { invLine1.JI_CustomsQuantity.ToString(), invLine1.JI_LinePrice.ToString() });
			AssertChallenges(entry, entryLine1, "Supplementary unit does not appear credible for commodity value", new string[] { FECChallengeFields.Codes.JI_Price, FECChallengeFields.Codes.JI_Supp, FECChallengeFields.Codes.JI_SuppUQ }, new ZString[] { invLine1.JI_LinePrice.ToString(), invLine1.JI_CustomsSecondQuantity.ToString(), invLine1.JI_CustomsSecondUnitQty });
			AssertChallenges(entry, entryLine1, "Supplementary unit does not appear credible for commodity weight", new string[] { FECChallengeFields.Codes.JI_NettMass, FECChallengeFields.Codes.JI_Supp, FECChallengeFields.Codes.JI_SuppUQ }, new ZString[] { invLine1.JI_CustomsQuantity.ToString(), invLine1.JI_CustomsSecondQuantity.ToString(), invLine1.JI_CustomsSecondUnitQty });

			entryLine1.CL_LineNumber = 1001;
			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 1002;
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;
			invLine2.JI_CustomsQuantity = 1234;
			invLine2.JI_LinePrice = 2345;
			Factory.Save();

			AssertChallenges(entry, entryLine1, "Value per kilo appears too low for this commodity", new string[] { FECChallengeFields.Codes.JI_NettMass, FECChallengeFields.Codes.JI_Price }, new ZString[] { invLine1.JI_CustomsQuantity.ToString(), invLine1.JI_LinePrice.ToString() }, 1001);
			AssertChallenges(entry, entryLine2, "Weight appears too low per item", new string[] { FECChallengeFields.Codes.JI_NettMass }, new ZString[] { invLine2.JI_CustomsQuantity.ToString() }, 1002);
			AssertChallenges(entry, entryLine2, "Weight appears too high per item", new string[] { FECChallengeFields.Codes.JI_NettMass }, new ZString[] { invLine2.JI_CustomsQuantity.ToString() }, 1002);
		}

		void AssertChallenges(CusEntryHeader entry, CusEntryLine entryLine, ZString errorMessage, string[] fECChallengeFieldsCodes, ZString[] propertiesValue, int entryLineNumber = 1)
		{
			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = $@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <FunctionalReferenceID>{entry.PK}</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190619120406+01</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementCode>smartErrorMsg</StatementCode>
    <StatementDescription>{errorMessage}</StatementDescription>
    <StatementTypeCode>1</StatementTypeCode>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>07B</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>53A</DocumentSectionCode>
    </Pointer>
  </AdditionalInformation>
  <Error>
    <ValidationCode>CDS13000</ValidationCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>67A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>{entryLineNumber}</SequenceNumeric>
      <DocumentSectionCode>68A</DocumentSectionCode>
    </Pointer>
  </Error>
  <Declaration>
    <AcceptanceDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190619120402+01</DateTimeString>
    </AcceptanceDateTime>
    <FunctionalReferenceID>LRN123456789000-S0001000</FunctionalReferenceID>
    <ID>19GB6Q54XXWF0FGVR0</ID>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";
			entry.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(Logger);
			processor.ProcessMessage(ediMessage);

			var challenges = entry.FECChallenges.OfType<FECChallenge>();

			try
			{
				for (int i = 0; i < fECChallengeFieldsCodes.Length; i++)
				{
					Assert(challenges.Any(
					challenge =>
						challenge.CY_Code == fECChallengeFieldsCodes[i]
						&& challenge.CY_ParentTableCode == CusEntryLineSchema.Constants.Prefix
						&& challenge.CY_ParentID == entryLine.PK
						&& challenge.NewValue == propertiesValue[i]
					));
				}
			}
			finally
			{
				entry.FECChallenges.DeleteAll();
			}
		}

		DataTransfer.Universal.JobDeclarationEventParentFinder GetNewEventParentFinder()
		{
			return new DataTransfer.Universal.JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), new DummyLogger());
		}

		public void TestProcessRejectedMessage()
		{
			var helper = new DeclarationTestHelper();
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "Import_Obligation_REJ";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 3;
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			var inv3 = dec.Invoices.AddNew();
			var invLine3 = inv3.InvoiceLines.AddNew();
			invLine3.JI_CL = entryLine3.PK;

			var declarationEdiMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			declarationEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRequestMessageXmlContent();
			entry.Messages.Add(declarationEdiMessage);
			Factory.Save();

			var responseEdiMessage = Factory.New<CDSResponseEDIMessage>();
			responseEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRejectionMessageXml().GetResponses().First().Serialize();
			entry.Messages.Add(responseEdiMessage);

			Factory.Save();

			var permitHeader = helper.SetupPermits(entry.DeclarantOrganisation, PermitQtyValIndicatorList.Codes.BTH);
			helper.SetupTransaction(permitHeader, entry.CH_BGMReference, declarationEdiMessage.EM_MessageNum);

			var processor = new CDSResponseMessageProcessor(Logger);

			processor.ProcessMessage(responseEdiMessage);

			var exepctedInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been rejected</H3><p><strong>Function Code: </strong>03-REJ<br><strong>Old CHIEF Report Code: </strong>27 (or N3/S3)<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>18GBJCUDI9ADRHWD54<br><strong>LRN: </strong>Import_Obligation_REJ</p><p>Error Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Error Code</td><td>Error Description</td><td>Field Name</td><td>Original Value</td><td>Data Element</td><td>Path (for technical support only)</td></tr><tr><td>Item 4</td><td>DMS10001</td><td>Obligation error: Obligation rule is not met</td><td>Net net weight</td><td>100</td><td>6/1</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[4]/Commodity/GoodsMeasure/NetNetWeightMeasure<br>42A/67A[1]/68A[4]/23A/65A/128[1]</td></tr><tr><td>Item 1</td><td>DMS12056</td><td>&nbsp;</td><td>Document category, coded</td><td>absent</td><td>2/3</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument/CategoryCode<br>42A/67A[1]/68A[1]/02A/D031[1]</td></tr><tr><td>Item 1</td><td>CDS10001</td><td>Obligation error: Obligation rule is not met</td><td>Statistical value</td><td>10</td><td>8/6</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/StatisticalValueAmount<br>42A/67A/68A[1]/114[1]</td></tr><tr><td>Header</td><td>CDS12056</td><td>&nbsp;</td><td>Additional document type, coded</td><td>absent</td><td>2/3</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument/TypeCode<br>42A/67A/68A/02A/D006[1]</td></tr><tr><td>Header</td><td>CDS12070</td><td>&nbsp;</td><td>Document category, coded</td><td>absent</td><td>2/3 & 2/6</td><td>Declaration/AdditionalDocument[4]/CategoryCode<br>42A/02A[4]/D031[1]</td></tr><tr><td>Item 1</td><td>CDS10020</td><td>&nbsp;</td><td>Additional document reference number</td><td>absent</td><td>2/3</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument[4]/ID<br>42A/67A[1]/68A[1]/02A[4]/D005[1]</td></tr></table></p>";

			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessage.Status.ProcessedOK, responseEdiMessage.EM_Status);
				AssertXMLEquals("Message.EM_MessageInterpretation", exepctedInterpretation, responseEdiMessage.EM_MessageInterpretation);
				Assert(invLine1.ZG_HadErrorInLastResponse);
				Assert(!invLine2.ZG_HadErrorInLastResponse);
				Assert(invLine3.ZG_HadErrorInLastResponse);
				AssertEquals("", entry.CH_EntryStatus);
			});

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Deleted, transactions[0].CPL_TransactionStatus);
		}

		#region Test UpdateStatus

		public void TestProcess_UpdateStatus_Acknowledged()
		{
			foreach (var numericFunctionCode in new[]
			{
				Constants.NumbericFunctionCodes.DeclarationAccepted,
				Constants.NumbericFunctionCodes.MessageRegistered,
				Constants.NumbericFunctionCodes.DeclarationIncomplete,
				Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl,
				Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl2,
				Constants.NumbericFunctionCodes.DeclarationUpdatedByCustoms,
				Constants.NumbericFunctionCodes.GoodsMayBeReleased,
				Constants.NumbericFunctionCodes.DeclarationCancelled,
				Constants.NumbericFunctionCodes.AdditionalMessageProcessed,
				Constants.NumbericFunctionCodes.DutiesTaxesCalculatedAndDue,
				Constants.NumbericFunctionCodes.InsufficientDefermentBalance,
				Constants.NumbericFunctionCodes.PaymentDue,
				Constants.NumbericFunctionCodes.GoodsExitedCustomsUnion,
				Constants.NumbericFunctionCodes.ExceptionalIrregularityNeedsToBeHandled,
				Constants.NumbericFunctionCodes.ExitOfGoodsFromEUNotConfirmed,
			})
			{
				TestProcess_UpdateStatus<CDSNewDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.AcknowledgedOriginal);

				TestProcess_UpdateStatus<CDSAmendDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.AcknowledgedChange);

				TestProcess_UpdateStatus<CDSCancelDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.AcknowledgedDelete);
			}
		}

		public void TestProcess_UpdateStatus_Rejected()
		{
			foreach (var numericFunctionCode in new[]
			{
				Constants.NumbericFunctionCodes.MessageRejected,
			})
			{
				TestProcess_UpdateStatus<CDSNewDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.ErrorOriginal);

				TestProcess_UpdateStatus_V2<CDSAmendDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.ErrorChange);

				TestProcess_UpdateStatus<CDSCancelDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.ErrorDelete);
			}
		}

		public void TestProcess_UpdateStatus_Cleared()
		{
			foreach (var numericFunctionCode in new[]
			{
				Constants.NumbericFunctionCodes.DeclarationCleared,
			})
			{
				TestProcess_UpdateStatus<CDSNewDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.ClearOriginal);

				TestProcess_UpdateStatus<CDSAmendDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.ClearChange);

				TestProcess_UpdateStatus<CDSCancelDeclarationEDIMessage>(
					ResponseFunction.New(numericFunctionCode),
					MessageStatusList.Codes.ClearDelete);
			}
		}

		void TestProcess_UpdateStatus<T>(ResponseFunction responseFunction, ZString expectedStatus) where T : CDSEDIMessage
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 3;
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			var inv3 = dec.Invoices.AddNew();
			var invLine3 = inv3.InvoiceLines.AddNew();
			invLine3.JI_CL = entryLine3.PK;

			CDSAmendDeclarationEDIMessage namMessage = null;

			if (typeof(T) == typeof(CDSAmendDeclarationEDIMessage) && responseFunction.NumericFunctionCode == Constants.NumbericFunctionCodes.MessageRejected)
			{
				var mockMessage1 = Factory.NewMoq<CDSAmendDeclarationEDIMessageDummyForTest_1>();
				mockMessage1.Protected().Setup<string>("GetMessageReferenceNumber").Returns("1");
				namMessage = mockMessage1.Object;
				namMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewAmendment;
				entry.Messages.Add(namMessage);
				mockMessage1.VerifyAll();
			}

			var declarationEdiMessage = Factory.New<T>();
			declarationEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRequestMessageXmlContent();

			if (typeof(T) == typeof(CDSAmendDeclarationEDIMessage) && responseFunction.NumericFunctionCode == Constants.NumbericFunctionCodes.MessageRejected)
			{
				namMessage.EM_ApplicationReference = GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(declarationEdiMessage);
			}

			entry.Messages.Add(declarationEdiMessage);
			Factory.Save();

			var responseEdiMessage = Factory.New<CDSResponseEDIMessage>();
			responseEdiMessage.EM_MessageText = new Response
			{
				FunctionCode = new ResponseFunctionCodeType
				{
					Value = responseFunction.NumericFunctionCode
				},
				FunctionalReferenceID = new ResponseFunctionalReferenceIDType
				{
					Value = "c41cb7554783489c94e24939cb1ccf51"
				},
				Declaration = new ResponseDeclaration
				{
					FunctionalReferenceID = new DeclarationFunctionalReferenceIDType1
					{
						Value = entry.LRN
					},
					ID = new DeclarationIdentificationIDType1
					{
						Value = "18GBJCUDI9ADRHWD54"
					},
				}
			}.Serialize();

			var logger = new LoggingInformation();
			var processor = new CDSResponseMessageProcessor(logger);
			processor.ProcessMessage(responseEdiMessage);

			AssertEquals(responseFunction.NumericFunctionCode + "|" + declarationEdiMessage.GetType(), expectedStatus, entry.CH_Status);

			if (typeof(T) == typeof(CDSAmendDeclarationEDIMessage) && responseFunction.NumericFunctionCode == Constants.NumbericFunctionCodes.MessageRejected)
			{
				var foundNamMessage = entry.Messages.OfType<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == CDSEDIMessageTypeList.Codes.NewAmendment);
				AssertNotNull(foundNamMessage);
				AssertEquals(EDIMessageStatusList.Codes.Discarded, foundNamMessage.EM_Status);
			}
		}

		void TestProcess_UpdateStatus_V2<T>(ResponseFunction responseFunction, ZString expectedStatus) where T : CDSEDIMessage
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 3;
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			var inv3 = dec.Invoices.AddNew();
			var invLine3 = inv3.InvoiceLines.AddNew();
			invLine3.JI_CL = entryLine3.PK;

			CDSAmendDeclarationEDIMessage namMessage = null;

			if (typeof(T) == typeof(CDSAmendDeclarationEDIMessage) && responseFunction.NumericFunctionCode == Constants.NumbericFunctionCodes.MessageRejected)
			{
				var mockMessage1 = Factory.NewMoq<CDSAmendDeclarationEDIMessageDummyForTest_1>();
				namMessage = mockMessage1.Object;
				namMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewAmendment;
				entry.Messages.Add(namMessage);
			}

			var declarationEdiMessage = Factory.New<T>();
			declarationEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRequestMessageXmlContent();

			if (typeof(T) == typeof(CDSAmendDeclarationEDIMessage) && responseFunction.NumericFunctionCode == Constants.NumbericFunctionCodes.MessageRejected)
			{
				namMessage.EM_ApplicationReference = GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(declarationEdiMessage);
			}

			entry.Messages.Add(declarationEdiMessage);
			Factory.Save();

			var responseEdiMessage = Factory.New<CDSResponseEDIMessage>();
			responseEdiMessage.EM_MessageText = new Response
			{
				FunctionCode = new ResponseFunctionCodeType
				{
					Value = responseFunction.NumericFunctionCode
				},
				FunctionalReferenceID = new ResponseFunctionalReferenceIDType
				{
					Value = "c41cb7554783489c94e24939cb1ccf51"
				},
				Declaration = new ResponseDeclaration
				{
					FunctionalReferenceID = new DeclarationFunctionalReferenceIDType1
					{
						Value = entry.LRN
					},
					ID = new DeclarationIdentificationIDType1
					{
						Value = "18GBJCUDI9ADRHWD54"
					},
				}
			}.Serialize();

			var logger = new LoggingInformation();
			var processor = new CDSResponseMessageProcessor(logger);
			processor.ProcessMessage(responseEdiMessage);

			AssertEquals(responseFunction.NumericFunctionCode + "|" + declarationEdiMessage.GetType(), expectedStatus, entry.CH_Status);

			if (typeof(T) == typeof(CDSAmendDeclarationEDIMessage) && responseFunction.NumericFunctionCode == Constants.NumbericFunctionCodes.MessageRejected)
			{
				var foundNamMessage = entry.Messages.OfType<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == CDSEDIMessageTypeList.Codes.NewAmendment);
				AssertNotNull(foundNamMessage);
				AssertEquals(EDIMessageStatusList.Codes.Discarded, foundNamMessage.EM_Status);
			}
		}

		#endregion

		public void TestRejectMessageOnNEW()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);
			var entry1 = CreateTestDeclarationAndEntryHeader();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			entry1.Messages.Add(newMessage);

			var canMessage = Factory.New<CDSCancelDeclarationEDIMessage>();
			canMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			canMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			canMessage.EM_Status = EDIMessage.Status.Acknowledged;
			canMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			canMessage.EM_ApplicationReference = "CAN-Message";
			entry1.Messages.Add(canMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var reqMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REQ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>11</FunctionCode>    <FunctionalReferenceID>REQMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Status>      <NameCode>39</NameCode>    </Status>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"CAN-Message");

			entry1.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REQ Msg", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Received, entry1.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedDelete, entry1.CH_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Acknowledged, canMessage.EM_Status);
				AssertEquals("REQ Message", EDIMessage.Status.ProcessedOK, reqMessage.EM_Status);
			});

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"NEW-Message");

			entry1.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REJ Msg", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Cancelled, entry1.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedDelete, entry1.CH_Status);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Acknowledged, canMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
				AssertContains("Response from CDS: Pre-lodged declaration canceled OK", rejMessage.EM_MessageInterpretation);
			});
		}

		public void TestRejectMessageOnNEWWithErrors()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);
			var entry1 = CreateTestDeclarationAndEntryHeader();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			entry1.Messages.Add(newMessage);

			var canMessage = Factory.New<CDSCancelDeclarationEDIMessage>();
			canMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			canMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			canMessage.EM_Status = EDIMessage.Status.Acknowledged;
			canMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			canMessage.EM_ApplicationReference = "CAN-Message";
			entry1.Messages.Add(canMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var reqMessage = Factory.New<CDSResponseEDIMessage>();
			reqMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			reqMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
			reqMessage.EM_MessageSubType = ThreeCharFunctionCode.Codes.REQ;
			reqMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			reqMessage.EM_ApplicationReference = "CAN-Message";
			reqMessage.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>11</FunctionCode>    <FunctionalReferenceID>REQMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Status>      <NameCode>39</NameCode>    </Status>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <VersionID>1</VersionID>    </Declaration>  </Response>";

			Factory.Save();
			processor.ProcessMessage(reqMessage);
			Factory.Save();

			entry1.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REQ Msg", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Received, entry1.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedDelete, entry1.CH_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Acknowledged, canMessage.EM_Status);
				AssertEquals("REQ Message", EDIMessage.Status.ProcessedOK, reqMessage.EM_Status);
			});

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>  <Error>    <Description>How do you expect us to process your dodgy messages?</Description>    <ValidationCode>DODGY101</ValidationCode>  </Error>  <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"NEW-Message");

			Factory.Save();
			processor.ProcessMessage(rejMessage);
			Factory.Save();

			entry1.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REJ msg", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Cancelled, entry1.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedDelete, entry1.CH_Status);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Acknowledged, canMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
				AssertContains("Response from CDS: Pre-lodged declaration canceled OK", rejMessage.EM_MessageInterpretation);
			});
		}

		public void TestRejectMessageOnNEW_WithoutCAN()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);
			var entry1 = CreateTestDeclarationAndEntryHeader();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			entry1.Messages.Add(newMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"NEW-Message");

			entry1.Reload();
			newMessage.Reload();

			CombineAssertions("After REJ msg", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Cancelled, entry1.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedOriginal, entry1.CH_Status);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
				AssertContains("Response from CDS: Pre-lodged declaration canceled OK", rejMessage.EM_MessageInterpretation);
			});
		}

		public void TestRejectMessageOnCAN()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);
			var entry1 = CreateTestDeclarationAndEntryHeader();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			entry1.Messages.Add(newMessage);

			var canMessage = Factory.New<CDSCancelDeclarationEDIMessage>();
			canMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			canMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			canMessage.EM_Status = EDIMessage.Status.Acknowledged;
			canMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			canMessage.EM_ApplicationReference = "CAN-Message";
			entry1.Messages.Add(canMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"CAN-Message");

			entry1.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REJ msg", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Received, entry1.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.ErrorDelete, entry1.CH_Status);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Rejected, canMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
				AssertContains("Response from CDS: Message has been rejected", rejMessage.EM_MessageInterpretation);
			});
		}

		public void TestAdditionalMessageProcessedResponse_WithStatus39()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);
			var entry1 = CreateTestDeclarationAndEntryHeader();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "ConversationID";
			entry1.Messages.Add(newMessage);

			var amendMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			amendMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			amendMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			amendMessage.EM_Status = EDIMessage.Status.Acknowledged;
			amendMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			amendMessage.EM_ApplicationReference = "ConversationID";
			entry1.Messages.Add(amendMessage);

			string testResponse = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>11</FunctionCode>
  <FunctionalReferenceID>REQMessage001</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementTypeCode>AFB</StatementTypeCode>
  </AdditionalInformation>
  <Status>
    <NameCode>39</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>
    <ID>MRN1234567890</ID>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";

			var reqMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REQ, testResponse);

			entry1.Reload();
			newMessage.Reload();
			amendMessage.Reload();
			reqMessage.Reload();

			CombineAssertions("After this REQ Msg the status should not be changed to CAN", () =>
			{
				AssertEquals("Entry Status", EDIMessageStatusList.Codes.Received, entry1.CH_EntryStatus);
				AssertEquals("REQ Message", EDIMessage.Status.ProcessedOK, reqMessage.EM_Status);
			});
		}

		public void TestExitOfGoods()
		{
			var entry = CreateTestDeclarationAndEntryHeader();

			foreach (var test in new[] {
				("A1", "20240321000000Z", "GB000084", ExportExitStatus.Codes.ExitedSatisfactorily, new ZDateTime(2024, 3, 21), "EOG|LOC=GB000084|TYP=A1"),
				("A2", "20240323000000Z", "GB000085", ExportExitStatus.Codes.ExitedSatisfactorily, new ZDateTime(2024, 3, 23), "EOG|LOC=GB000085|TYP=A2"),
				("B1", "20240326000000Z", "", ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory, new ZDateTime(2024, 3, 26), "EOG|TYP=B1"),
				("X1", "", "", ExportExitStatus.Codes.UnknownOrNotReported, ZDateTime.Empty, "EOG|TYP=X1"),
				("", "", "", ExportExitStatus.Codes.UnknownOrNotReported, ZDateTime.Empty, "EOG"),
			})
			{
				var message = ProcessTestMessage(ThreeCharFunctionCode.Codes.EOG,
					$@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>16</FunctionCode>
  <FunctionalReferenceID>9d286890f130400188c39039608969b5</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20240321095924Z</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementCode />
    <StatementDescription>{test.Item3}</StatementDescription>
    <StatementTypeCode>CEX</StatementTypeCode>
  </AdditionalInformation>
  <Status>
    <EffectiveDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">{test.Item2}</DateTimeString>
    </EffectiveDateTime>
    <NameCode>{test.Item1}</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>
    <ID>24GB2X20FOKNQOLAA0</ID>
    <VersionID>2</VersionID>
  </Declaration>
</Response>
");
				AssertEquals($"Pre-requisite {test.Item1}: EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				CombineAssertions(() =>
				{
					AssertEquals($"{test.Item1}: CH_ExitedStatus", test.Item4, entry.CH_ExitedStatus);
					AssertEquals($"{test.Item1}: CH_ExitActualOffice", test.Item3, entry.CH_ExitActualOffice);
					AssertEquals($"{test.Item1}: CH_ExitDate", test.Item5, entry.CH_ExitDate);

					var recentLog = (StmALog)entry.Logs.GetAllLogs().Last();
					AssertEquals($"{test.Item1}: Event.SE_Code", AutoEvents.CustomsEntryStatusCode, recentLog.Event.SE_Code);
					AssertEquals($"{test.Item1}: Log.SL_Reference", test.Item6, recentLog.SL_Reference);
					AssertEquals($"{test.Item1}: Log.SL_EventTimeUtc", new ZDateTime(2024, 3, 21, 9, 59, 24), recentLog.SL_EventTimeUtc);
				});
			}
		}

		public void TestExitOfGoodsReminder()
		{
			var entry = CreateTestDeclarationAndEntryHeader();
			var message = ProcessTestMessage(ThreeCharFunctionCode.Codes.GER,
				$@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>18</FunctionCode>
  <FunctionalReferenceID>9d286890f130400188c39039608969b5</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20240321095924Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>
    <ID>24GB2X20FOKNQOLAA0</ID>
    <VersionID>2</VersionID>
  </Declaration>
</Response>
");
			AssertEquals("Pre-requisite: EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("CH_ExitedStatus", ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived, entry.CH_ExitedStatus);
		}

		public void TestUpdateAcceptanceDateAfterProcessingAcceptedResponse()
		{
			var (declaration, ediMessage, processor) = CreateDeclarationAndResponseMessageForTestingSendEntryDocs(export: false);
			declaration.MarkApportionmentDirty();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			processor.ProcessMessage(ediMessage);

			CombineAssertions("Process ACC response", () =>
			{
				var entryStatusLog = entryHeader.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code).FirstOrDefault();
				AssertNotNull(entryStatusLog);
				AssertEquals(Constants.ThreeCharFunctionCodes.DeclarationAccepted, entryStatusLog.SL_Reference);

				var expectedAcceptTime = new ZDateTime(2018, 7, 27, 12, 34, 56);
				AssertEquals("Log event time from Declaration.AcceptanceDateTime", expectedAcceptTime, entryStatusLog.SL_EventTime);
				AssertEquals("Set CEI_DateForDuty from Declaration.AcceptanceDateTime", expectedAcceptTime, entryHeader.EntryInstruction.CEI_DateForDuty);

				AssertEquals("Have run apportionment", false, declaration.ApportionmentDirty);
			});
		}

		LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;

		CusEntryHeader CreateTestDeclarationAndEntryHeader()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationReference = "B001";
			dec1.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.LRN = "ABCDEF0000000001";
			entry1.CH_EntryStatus = EDIMessageStatusList.Codes.Received;
			entry1.CH_Status = MessageStatusList.Codes.AcknowledgedOriginal;
			return entry1;
		}

		CDSResponseEDIMessage ProcessTestMessage(string messageSubType, string messageText, string applicationReference = "ConversationID")
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			message.EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_ApplicationReference = applicationReference;
			message.EM_MessageText = messageText;

			var processor = new CDSResponseMessageProcessor(Logger);

			Factory.Save();
			processor.ProcessMessage(message);
			Factory.Save();

			return message;
		}

		public void TestEmptyMessageLogging()
		{
			CreateTestDeclarationAndEntryHeader();
			ProcessTestMessage(ThreeCharFunctionCode.Codes.GER, "");
			ZString year = ZDate.Today.Year.ToString();
			AssertContains(string.Format(CultureInfo.CurrentCulture, "Could not process message # as its content was blank. Most likely this is due to a user marking a system-generated eHub acknowledgement message as status queued for reprocessing. Message is skipped.", year.Right(1)), Logger.Logs.First().ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			friendlyCodeWithPointerTest = new FriendlyCodeWithPointerTest();
		}

		FriendlyCodeWithPointerTest friendlyCodeWithPointerTest;
	}

	public class CDSAmendDeclarationEDIMessageDummyForTest_1 : CDSAmendDeclarationEDIMessage
	{
		public CDSAmendDeclarationEDIMessageDummyForTest_1(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "1";
		}
	}
}
