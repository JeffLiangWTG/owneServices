using System;
using System.IO;
using CargoWise.Application;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class HKCustomsMessageHandlerTests : TestCaseWithFactory
	{
		public void TestHKCustomsMessageHandler()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.HongKong);
			var branch = GlbCompany.CurrentCompany.FirstActiveBranch;
			var messageText = @"UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:PIMA+RHKAGT021330984/HKG85:PIMA+210720:0939+5162+0'UNH+HMF1137367X157+CIMFNA:0+516200'FNA
ACK/NOT AUTHORIZED TO UPDATE HWB OF OTHER AGENT   HWB SMOA00171069
AWB157-11373670 TASKID20330771
'UNT+3+HMF1137367X157'UNZ+1+5162'
";

			var trackingId = Guid.NewGuid();
			var message = CreateMessage(messageText, trackingId);

			var handler = new HKCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
			}
			var expectedHeader = "UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:PIMA+RHKAGT021330984/HKG85:PIMA+210720:0939+5162+0'";
			var expectedBody = @"UNH+HMF1137367X157+CIMFNA:0+516200'FNA
ACK/NOT AUTHORIZED TO UPDATE HWB OF OTHER AGENT   HWB SMOA00171069
AWB157-11373670 TASKID20330771
'UNT+3+HMF1137367X157'";
			var expectedFooter = "UNZ+1+5162'";

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.Traxon, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.HKCustoms, interchange.EI_InterchangeType);
			AssertEquals(expectedHeader, interchange.EI_HeaderText);
			AssertEquals(expectedBody, interchange.EI_BodyText);
			AssertEquals(expectedFooter, interchange.EI_FooterText);
			AssertEquals(trackingId, interchange.EI_SessionGUID);
			AssertEquals("RHKAPT01HKGSTCR", interchange.EI_From);
			AssertEquals("RHKAGT021330984/HKG85", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertEquals(branch.PK, interchange.EI_GB);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(1, interchange.ContainedMessages.Count);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(EDIMessage.ApplicationCodes.Traxon, ediMessage.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.Traxon, ediMessage.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.Traxon, ediMessage.EM_MessageType);
			AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
			AssertEquals(expectedBody, ediMessage.EM_MessageText);
			AssertEquals(branch.PK, ediMessage.EM_GB);
		}

		public void TestHKCustomsMessageHandler_NoActiveBranch()
		{
			var messageText = @"UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:PIMA+RHKAGT021330984/HKG85:PIMA+210720:0939+5162+0'UNH+HMF1137367X157+CIMFNA:0+516200'FNA
ACK/NOT AUTHORIZED TO UPDATE HWB OF OTHER AGENT   HWB SMOA00171069
AWB157-11373670 TASKID20330771
'UNT+3+HMF1137367X157'UNZ+1+5162'
";

			var trackingId = Guid.NewGuid();
			var message = CreateMessage(messageText, trackingId);

			var handler = new HKCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
			}

			var expectedHeader = "UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:PIMA+RHKAGT021330984/HKG85:PIMA+210720:0939+5162+0'";
			var expectedBody = @"UNH+HMF1137367X157+CIMFNA:0+516200'FNA
ACK/NOT AUTHORIZED TO UPDATE HWB OF OTHER AGENT   HWB SMOA00171069
AWB157-11373670 TASKID20330771
'UNT+3+HMF1137367X157'";
			var expectedFooter = "UNZ+1+5162'";

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.Traxon, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.HKCustoms, interchange.EI_InterchangeType);
			AssertEquals(expectedHeader, interchange.EI_HeaderText);
			AssertEquals(expectedBody, interchange.EI_BodyText);
			AssertEquals(expectedFooter, interchange.EI_FooterText);
			AssertEquals(trackingId, interchange.EI_SessionGUID);
			AssertEquals("RHKAPT01HKGSTCR", interchange.EI_From);
			AssertEquals("RHKAGT021330984/HKG85", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			var interchangeNotes = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, interchangeNotes.Length);
			AssertEquals("File Name is stored in note", "FileName", interchangeNotes[0].ST_NoteDataAsText);
			interchangeNotes = interchange.Notes.FindByDescription("Parse message error");
			AssertStartsWith("failNote.ST_NoteDataAsText", "There is no active branch under HK company.", interchangeNotes[0].ST_NoteDataAsText);
			AssertEquals(1, interchange.ContainedMessages.Count);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(EDIMessage.ApplicationCodes.Traxon, ediMessage.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.Traxon, ediMessage.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.Traxon, ediMessage.EM_MessageType);
			AssertEquals(EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
			var failedNotes = ediMessage.Notes.FindByDescription("Parse message error");
			AssertStartsWith("failNote.ST_NoteDataAsText", "There is no active branch under HK company.", failedNotes[0].ST_NoteDataAsText);
			AssertEquals(expectedBody, ediMessage.EM_MessageText);
		}

		public void TestHKCustomsMessageHandler_MultipleCompany()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.HongKong);
			var (company1, branch1) = PrepareCompanyBranch("TC1", "TB1");
			var (company2, branch2) = PrepareCompanyBranch("TC2", "TB2");

			Factory.Save();

			var registryItem = ObjectFactory.Get<Integration.Customs.HK.IHKCustomsDataRegistry>().HKTraxonSenderID;
			using (registryItem.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021330984/HKG85"))
			using (registryItem.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021330984/HKG86"))
			{
				var messageText = @"UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:PIMA+RHKAGT021330984/HKG85:PIMA+210720:0939+5162+0'UNH+HMF1137367X157+CIMFNA:0+516200'FNA
ACK/NOT AUTHORIZED TO UPDATE HWB OF OTHER AGENT   HWB SMOA00171069
AWB157-11373670 TASKID20330771
'UNT+3+HMF1137367X157'UNZ+1+5162'
";

				var trackingId1 = Guid.NewGuid();
				var message1 = CreateMessage(messageText, trackingId1);

				var trackingId2 = Guid.NewGuid();
				var message2 = CreateMessage(messageText.Replace("RHKAGT021330984/HKG85", "RHKAGT021330984/HKG86"), trackingId2);

				var handler = new HKCustomsMessageHandler();
				var companyForTest = TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current);
				handler.SaveMessage(message1, companyForTest, new NotificationBuffer());
				handler.SaveMessage(message2, companyForTest, new NotificationBuffer());

				AssertEquals(2, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange1 = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId1));
				var interchangeNum1 = interchange1.EI_InterchangeNum;
				AssertEquals(branch1.PK, interchange1.EI_GB);
				AssertEquals(false, string.IsNullOrEmpty(interchangeNum1));

				var interchange2 = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId2));
				var interchangeNum2 = interchange2.EI_InterchangeNum;
				AssertEquals(branch2.PK, interchange2.EI_GB);
				AssertEquals(false, string.IsNullOrEmpty(interchangeNum2));
				AssertNotEquals(interchangeNum1, interchangeNum2);
			}
		}

		IeHubMessage CreateMessage(string messageText, Guid trackingId)
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(messageText);
			writer.Flush();

			var mockRepository = new MockRepository(MockBehavior.Default);
			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.Traxon);
			message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			message.Setup(m => m.SenderID).Returns("GLSHK");
			message.Setup(m => m.TrackingID).Returns(trackingId);
			message.Setup(m => m.Filename).Returns("FileName");

			return message.Object;
		}

		(GlbCompany, GlbBranch) PrepareCompanyBranch(string companyCode, string branchCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			company.GC_Code = companyCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;

			return (company, branch);
		}
	}
}
