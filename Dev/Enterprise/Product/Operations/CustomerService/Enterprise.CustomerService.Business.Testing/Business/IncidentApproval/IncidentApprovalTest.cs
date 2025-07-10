using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(IncidentApproval))]
	sealed class IncidentApprovalTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<IncidentApproval>();
		}

		public void TestSupportRequestEmailAddress()
		{
			AssertEquals("SupportRequestEmailAddress", "EnterpriseSupportRequest@edi.net.au", Constants.EmailAddresses.SupportRequestEmailAddress);
		}

		public void TestSetReadOnlyIfCR1()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			Factory.Save();

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals(false, loadedIncident.IA_ClientSpecifiedStatusInfo.ReadOnly);

			AssertEquals(true, loadedIncident.IA_CriticalityInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_GS_NKReportingStaffInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_IncidentDetailsInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_IncidentSummaryInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_GS_NKApprovingStaffInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_LicenceCodeInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_ModuleDescriptionInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_StatusInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_IncidentNumberInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_GS_NKApprovingStaffInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_ClientReferenceInfo.ReadOnly);
			AssertEquals(true, loadedIncident.IA_StatusInfo.ReadOnly);

			IncidentApproval incident2 = Factory.New<IncidentApproval>();
			incident2.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident2.IA_Module = "COR";
			incident2.IA_IncidentSummary = "Test Summary";
			incident2.IA_IncidentDetails = "Test Details";
			Factory.Save();

			IncidentApproval loadedIncident2 = new BusinessObjectFactory().Load<IncidentApproval>(incident2.PK);
			AssertEquals(false, loadedIncident2.IA_ClientSpecifiedStatusInfo.ReadOnly);

			AssertEquals(false, loadedIncident2.IA_CriticalityInfo.ReadOnly);
			AssertEquals(false, loadedIncident2.IA_GS_NKReportingStaffInfo.ReadOnly);
			AssertEquals(false, loadedIncident2.IA_IncidentDetailsInfo.ReadOnly);
			AssertEquals(false, loadedIncident2.IA_IncidentSummaryInfo.ReadOnly);
			AssertEquals(true, loadedIncident2.IA_GS_NKApprovingStaffInfo.ReadOnly);
			AssertEquals(false, loadedIncident2.IA_LicenceCodeInfo.ReadOnly);
			AssertEquals(true, loadedIncident2.IA_ModuleDescriptionInfo.ReadOnly);
			AssertEquals(true, loadedIncident2.IA_StatusInfo.ReadOnly);
			AssertEquals(true, loadedIncident2.IA_IncidentNumberInfo.ReadOnly);
			AssertEquals(true, loadedIncident2.IA_GS_NKApprovingStaffInfo.ReadOnly);
			AssertEquals(true, loadedIncident2.IA_ClientReferenceInfo.ReadOnly);
		}

		public void TestSetReadOnlyIfRequestSentButNoIncidentNumber()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			incident.IA_ClientReference = "SR00001002";
			Factory.Save();

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals(true, loadedIncident.ReadOnly);
		}

		#region Actions

		public void TestApprove_ExceptionHandling()
		{
			IncidentApprovalForTest request = Factory.New<IncidentApprovalForTest>();
			request.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			request.IA_Module = "COR";
			request.IA_IncidentSummary = "Test Summary";
			request.IA_IncidentDetails = "Test Details";

			request.SaveException = new InvalidOperationException("save failed");

			Exception caught = null;
			try
			{
				request.Approve();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				caught = ex;
			}

			AssertEquals("exception thrown", request.SaveException, caught);
			AssertEquals("status is restored to original value", ZString.Empty, request.IA_Status);
			AssertEquals("no memory dump at all", 0, request.DocManagerInfo.AllEDocs.Count);
		}

		public void TestSaveSetsClientReference()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			AssertEquals("Precondition: Client Reference is empty", true, incident.IA_ClientReference.IsEmpty);
			Factory.Save();
			AssertEquals("Client Reference is filled in", false, incident.IA_ClientReference.IsEmpty);
		}

		public void TestDefaultValues()
		{
			SystemDataRegistry.Instance.CustomerStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList());

			IncidentApproval incident = Factory.New<IncidentApproval>();
			AssertEquals(Env.CurrentCompany.GetLicenceCode(), incident.IA_LicenceCode);
			AssertEquals(ZString.Empty, incident.IA_Status);
			AssertEquals(ZString.Empty, incident.IA_Criticality);
			AssertEquals(ZString.Empty, incident.IA_ClientSpecifiedStatus);
			AssertEquals(IncidentApproval.NotAvailableActiveModuleID, incident.IA_ActiveModuleId);
		}

		public void TestReadOnlyFields()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			AssertEquals("Status should always be readonly", true, incident.IA_StatusInfo.ReadOnly);
			AssertEquals("Incident Num should always be readonly", true, incident.IA_IncidentNumberInfo.ReadOnly);
			AssertEquals("Client Ref should always be readonly", true, incident.IA_ClientReferenceInfo.ReadOnly);
		}

		public void TestPopulateXsd()
		{
			int initialStaffCount = Factory.GetDatabaseCount(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_IsSystemAccount, ZBool.False));

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FriendlyName = "";
			staff2.GS_FullName = "My Full Name";
			staff2.GS_NameSuffix = "Suffix";

			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_FriendlyName = "Hello";
			staff3.GS_FullName = "Hello Full Name";

			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_FriendlyName = "Hello";
			staff4.GS_HomePhone = "** View DENIED";
			staff4.GS_MobilePhone = "** view denied";
			staff4.GS_WorkPhone = "** VIEW DENIED";
			staff4.GS_EmailAddress = "** VIEW DEnIED";

			GlbStaff staffInactive = Factory.NewWithValidTestData<GlbStaff>();
			staffInactive.GS_FriendlyName = "";
			staffInactive.GS_FullName = "Seayou Later";
			staffInactive.GS_IsActive = false;

			Factory.Save();

			IncidentApproval request = Factory.New<IncidentApproval>();
			request.IA_IncidentSummary = "Hello";
			request.IA_IncidentDetails = "Some details here";
			request.IA_GS_NKApprovingStaff = staff2.GS_Code;
			request.IA_Criticality = "CR3";
			request.IA_Module = "COR";
			request.IA_ActiveModuleId = "ACC";
			request.IA_ClientReference = "TEST";
			request.IA_Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			request.IA_IncidentNumber = "CS00005783";

			string file1Name = "";
			string file2Name = "";
			using (TempFile file = TempFile.New())
			using (TempFile file2 = TempFile.New())
			{
				File.WriteAllBytes(file.Filename, new byte[] { 1, 2, 3 });
				File.WriteAllBytes(file2.Filename, new byte[] { 1, 2, 3 });

				file1Name = file.Filename;
				file2Name = file2.Filename;

				request.DocManagerInfo.AddFileOrDocument(file.Filename, "XXX");
				request.DocManagerInfo.AddFileOrDocument(file2.Filename, "XXX");
			}

			Xsd.CustomerServiceRequest xsdSrv = request.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);

			AssertEquals("Hello", xsdSrv.IncidentSummary);
			AssertEquals("Some details here", xsdSrv.IncidentDetails);
			AssertEquals(Env.CurrentUser.FullName, xsdSrv.ReportingStaffMemberName);
			AssertEquals("My Full Name Suffix", xsdSrv.ApprovingUser);
			AssertEquals("ENT", xsdSrv.Product);
			AssertEquals("COR", xsdSrv.Module);
			AssertEquals("ACC", xsdSrv.ActiveModuleId);
			AssertEquals("CR3", xsdSrv.Criticality);
			AssertEquals("TEST", xsdSrv.ClientReferenceNumber);
			AssertEquals("APP", xsdSrv.Status);
			AssertEquals("CS00005783", xsdSrv.IncidentNumber);

			AssertEquals(2, xsdSrv.Attachments.Count);
			AssertEquals(Path.GetFileName(file1Name), xsdSrv.Attachments[0].FileName);
			AssertEquals(Path.GetFileName(file2Name), xsdSrv.Attachments[1].FileName);

			AssertEquals(1, xsdSrv.Staff.Count);
			AssertEquals("My Full Name Suffix", xsdSrv.Staff[0].Name);
		}

		public void TestPopulateXsd_Language()
		{
			var reportedStaff = Factory.New<GlbStaff>();
			reportedStaff.GS_LoginName = "Juergen.Klinsmann";
			reportedStaff.GS_Code = "J.K";
			reportedStaff.GS_FriendlyName = "Klinsy";
			reportedStaff.GS_FullName = "Juergen Klinsmann";
			reportedStaff.GS_NameSuffix = "M.E.H";
			reportedStaff.GS_EmailAddress = "juergen.klinsmann@cargowise.com";
			reportedStaff.GS_WorkingLanguage = SharedConstants.Languages.German;

			var approvingStaff = Factory.NewWithValidTestData<GlbStaff>();
			approvingStaff.GS_LoginName = "mei.zhou";
			approvingStaff.GS_Code = "MLZ";
			approvingStaff.GS_FriendlyName = "Mei";
			approvingStaff.GS_FullName = "Mei-Ling Zhou";
			approvingStaff.GS_NameSuffix = "Hero";
			approvingStaff.GS_EmailAddress = "mei@overwatch.com";
			approvingStaff.GS_WorkingLanguage = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(reportedStaff.GS_LoginName))
			{
				var request = Factory.New<IncidentApproval>();
				request.IA_GS_NKApprovingStaff = approvingStaff.GS_Code;
				var requestXsd = request.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
				AssertEquals(1, requestXsd.Staff.Count);
				AssertEquals("Juergen Klinsmann M.E.H", requestXsd.ReportingStaffMemberName);
				AssertEquals("juergen.klinsmann@cargowise.com", requestXsd.ReportingStaffEmail);
				AssertEquals("Mei-Ling Zhou Hero", requestXsd.ApprovingUser);
				AssertEquals("mei@overwatch.com", requestXsd.ApprovingUserEmail);
				AssertEquals("Approving User's Working Language", SharedConstants.Languages.ChineseSimplified, requestXsd.Language);
			}
		}

		public void TestPopulateXsd_ApprovingStaff()
		{
			GlbStaff approvingStaff = Factory.New<GlbStaff>();
			approvingStaff.GS_Code = "J.K";
			approvingStaff.GS_FriendlyName = "Klinsy";
			approvingStaff.GS_FullName = "Juergen Klinsmann";
			approvingStaff.GS_NameSuffix = "M.E.H";
			approvingStaff.GS_EmailAddress = "juergen.klinsmann@cargowise.com";

			IncidentApproval request = Factory.New<IncidentApproval>();
			request.IA_GS_NKApprovingStaff = approvingStaff.GS_Code;
			Xsd.CustomerServiceRequest requestXsd = request.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
			AssertEquals("Should use full name", "Juergen Klinsmann M.E.H", requestXsd.ApprovingUser);
			AssertEquals("Should include email address", "juergen.klinsmann@cargowise.com", requestXsd.ApprovingUserEmail);
		}

		public void TestPopulateXsd_Company()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "AAA";
			newCompany.GC_Name = "Test Company";
			newCompany.GC_RN_NKCountryCode = "US";

			Factory.Save();

			var incident = Factory.New<IncidentApproval>();
			incident.IA_LicenceCode = newCompany.LicenceKeyIdentifier;

			var requestXsd = incident.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
			AssertEquals("AAA", requestXsd.CompanyCode);
			AssertEquals("Test Company", requestXsd.CompanyName);
			AssertEquals("US", requestXsd.CompanyCountry);
		}

		[TestDate(2016, 1, 29)]
		public void TestEConversationUpdate_UpdateLastEditDetails()
		{
			IncidentApproval incident = Factory.NewWithValidTestData<IncidentApproval>();
			incident.IA_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			incident.IA_SystemLastEditUser = "EEE";
			string currentUserInitials = Env.CurrentUser.Initials;

			Assert(incident.IA_SystemLastEditTimeUtc == new ZDateTime(2016, 1, 27));
			Assert(incident.IA_SystemLastEditUser == "EEE");
			Assert(incident.IA_SystemLastEditUser != currentUserInitials);

			incident.AddUserMessageToEConversation("Hello");

			Assert("Incident LastEditTimeUtc should change", incident.IA_SystemLastEditTimeUtc == ZDateTime.UtcNow);
			Assert("Incident LastEditUser should change", incident.IA_SystemLastEditUser == Env.CurrentUser.Initials);
		}

		public void TestPopulateXsd_Attachments()
		{
			IncidentApproval request = Factory.New<IncidentApproval>();

			var image = new Bitmap(1, 1);
			image.SetPixel(0, 0, Color.White);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				image.Save(stream, ImageFormat.Tiff);

				var imageDoc1 = request.DocManagerInfo.AddFileOrDocument(stream, "image1.tif", "XXX");
				imageDoc1.Description = "Some/Stuff ? Here";
				SetFileNameLikeEDocsPlugIn(imageDoc1, "image1");

				stream.Position = 0;
				var imageDoc2 = request.DocManagerInfo.AddFileOrDocument(stream, "image2.tif", "XXX");
				imageDoc2.Description = "";
				SetFileNameLikeEDocsPlugIn(imageDoc2, "image2");

				var txtDoc1 =
					request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("foo"), "text1.txt", "XXX");
				txtDoc1.Description = @"\/:?...";

				var txtDoc2 =
					request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("foo2"), "text2.txt", "XXX");
				txtDoc2.Description = "";

				stream.Position = 0;
				var imageDoc3 =
					request.DocManagerInfo.AddFileOrDocument(stream, "image1AAAAAAAAAAAAAAAAAAAAAAAAAAAAA.tif", "XXX");
				imageDoc3.Description = ZString.Replicate('-', StorageDocsSchema.SC_Desc.MaxLength);
				SetFileNameLikeEDocsPlugIn(imageDoc3, "image1AAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

				var requestXsd = request.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);

				AssertEquals(5, requestXsd.Attachments.Count);

				// Files added first
				AssertEquals(@"text1.txt", requestXsd.Attachments[0].FileName);
				AssertEquals("text2.txt", requestXsd.Attachments[1].FileName);

				AssertEquals("image1.tif", requestXsd.Attachments[2].FileName);
				AssertEquals("image2.tif", requestXsd.Attachments[3].FileName);
				ZString expectedFileName = imageDoc3.FileName;

				if (expectedFileName.Length > StorageDocsSchema.SC_FileName.MaxLength)
				{
					string extension = Path.GetExtension(expectedFileName);
					expectedFileName =
						expectedFileName.Substring(0, StorageDocsSchema.SC_FileName.MaxLength - extension.Length)
							.TrimEnd() + extension;
				}

				AssertEquals(expectedFileName, requestXsd.Attachments[4].FileName);
			}
		}

		public void TestPopulateXsdAttachments_NewNotSentIncident()
		{
			IncidentApproval request = Factory.New<IncidentApproval>();

			var image = new Bitmap(1, 1);
			image.SetPixel(0, 0, Color.White);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				image.Save(stream, ImageFormat.Tiff);
				var imageDoc = request.DocManagerInfo.AddFileOrDocument(stream, "image1.tif", "XXX");

				imageDoc.Description = "Screen Shot";
				SetFileNameLikeEDocsPlugIn(imageDoc, "image1");
			}

			var requestXsd = request.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
			AssertEquals("Attachment is published in xsd", 1, requestXsd.Attachments.Count);

			request.DocManagerInfo.Save();
			Factory.Save();

			IncidentApproval loadedRequest = new BusinessObjectFactory().Load<IncidentApproval>(request.PK);
			AssertEquals(IncidentApprovalLookups.StatusCodes.New, loadedRequest.IA_Status);
			requestXsd = loadedRequest.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
			AssertEquals("Attachment should still be published in xsd because incident not yet sent", 1, requestXsd.Attachments.Count);

			loadedRequest = new BusinessObjectFactory().Load<IncidentApproval>(request.PK);
			loadedRequest.Approve();
		}

		void SetFileNameLikeEDocsPlugIn(IeDoc eDoc, string fileName)
		{
			// DocManager strips the file name from images and eDocPlugin adds it back. Strange, but true.
			if (((ZString)((BusinessObject)eDoc)["SC_FileName"]).IsEmpty)
			{
				((BusinessObject)eDoc)["SC_FileName"] = fileName;
			}
		}

		public void TestSend()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_FullName = "Zubin Appoo";
			staff.GS_EmailAddress = "zubin@test.com";
			Factory.Save();

			IncidentApproval request = Factory.New<IncidentApproval>();
			request.IA_GS_NKReportingStaff = staff.GS_Code;
			Xsd.CustomerServiceRequest xsdSrv = request.GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
			request.Send(xsdSrv);

			AssertEquals("email count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchange count", 1, interchanges.Length);
			AssertEquals(SystemMessage.ApplicationCode, interchanges[0].EI_ApplicationCode);
			AssertContains("EI_BodyText", "<CustomerServiceRequest", interchanges[0].EI_BodyText);
			string expectedRef = "Customer Service Request from " + Env.CurrentCompany.Name + " - " + Env.CurrentCompany.Code;
			AssertEquals("send event ref", expectedRef, request.Logs.MostRecentLog.SL_Reference);
			AssertEquals("send event code", AutoEvents.TransferredCode, request.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		#endregion

		#region EConversation

		public void TestEConversation()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.AddUserMessageToEConversation("Testing");

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Testing", messageList[0].Body);

			Factory.Save();
			AssertEquals("Should be no changes after save", false, incident.HasChanges);

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			IncidentApproval loadedRequest = loadFactory.Load<IncidentApproval>(incident.PK);
			messageList = loadedRequest.EConversation.GetTimeOrderedMessages();
			AssertEquals("Testing", messageList[0].Body);
		}

		public void TestReloadEConversation()
		{
			var incidentApproval = Factory.NewWithValidTestData<IncidentApproval>();
			Factory.Save();

			// run twice to test caching
			for (var i = 1; i <= 2; i++)
			{
				// Simulate updating IncidentApproval from a different process - Make sure all factories are 'isolated' so that Factory.Save() does not clear their cache
				var otherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(Factory))
				using (GetFactoryIsolater(otherFactory))
				{
					var incidentApprovalInOtherFactory = otherFactory.Load<IncidentApproval>(incidentApproval.PK);
					incidentApprovalInOtherFactory.EConversation.AddMessageFromLocalUser("My Message " + i);
					otherFactory.Save();
				}

				incidentApproval.ReloadEConversation();
				AssertEquals(true, incidentApproval.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "My Message " + i));
			}
		}

		public void TestEConversation_Concurrency()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var incidentInFactory2 = factory2.Load<IncidentApproval>(incident.PK);

			incident.AddUserMessageToEConversation("Line1");
			incidentInFactory2.AddUserMessageToEConversation("Line2");
			Factory.Save();
			factory2.Save();

			incident.AddUserMessageToEConversation("Line3");
			Factory.Save();

			incidentInFactory2.AddUserMessageToEConversation("Line4");
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var incidentInFactory3 = factory3.Load<IncidentApproval>(incident.PK);

			var messageList = incidentInFactory3.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Line1"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Line2"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Line3"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Line4"));
		}

		public void TestEConversation_LocalFirstConcurrency()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.AddUserMessageToEConversation("LineLocal1");
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal1"));
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var incidentInFactory2 = factory2.Load<IncidentApproval>(incident.PK);
			incidentInFactory2.AddUserMessageToEConversation("LineRemote");
			factory2.Save();
			messageList = incidentInFactory2.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote"));

			incident.AddUserMessageToEConversation("LineLocal2");
			Factory.Save();

			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal1"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal2"));

			incidentInFactory2.AddUserMessageToEConversation("LineRemote2");
			factory2.Save();
			messageList = incidentInFactory2.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote2"));

			var factory3 = new BusinessObjectFactory();
			var incidentInFactory3 = factory3.Load<IncidentApproval>(incident.PK);
			messageList = incidentInFactory3.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal1"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal2"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote2"));
		}

		public void TestEConversation_RemoteFirstConcurrency()
		{
			Factory.RefreshEnabled = false;
			IncidentApproval incident = Factory.New<IncidentApproval>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var incidentInFactory2 = factory2.Load<IncidentApproval>(incident.PK);
			incidentInFactory2.AddUserMessageToEConversation("LineRemote");
			factory2.Save();
			var messageList = incidentInFactory2.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote"));

			incident.AddUserMessageToEConversation("LineLocal1");
			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal1"));

			incidentInFactory2.AddUserMessageToEConversation("LineRemote2");
			factory2.Save();
			messageList = incidentInFactory2.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote"));

			incident.AddUserMessageToEConversation("LineLocal2");
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var incidentInFactory3 = factory3.Load<IncidentApproval>(incident.PK);
			messageList = incidentInFactory3.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal1"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineLocal2"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "LineRemote2"));
		}

		#endregion

		#region Properties

		#region IA_ModuleDescription

		public void TestIA_ModuleDescription()
		{
			var incident = Factory.New<IncidentApproval>();

			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Customs;
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Descriptions.Customs, incident.IA_ModuleDescription);

			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IA_Module = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			AssertEquals(Cr8ModuleList.Descriptions.CarbonEnvironmentalCompliance, incident.IA_ModuleDescription);

			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incident.IA_Module = Cr9ModuleList.Codes.AccountingDataTakeOn;
			AssertEquals(Cr9ModuleList.Descriptions.AccountingDataTakeOn, incident.IA_ModuleDescription);
		}

		#endregion

		#region Reported By Staff

		public void TestReportedByStaff()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			AssertEquals(GlbStaff.CurrentUser.GS_Code, incident.ReportedByStaffCode);

			Factory.Save();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			incident.IA_GS_NKReportingStaff = staff.GS_Code;
			AssertEquals(GlbStaff.CurrentUser.GS_Code, incident.ReportedByStaffCode);

			GlbStaff batchProcessor = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);
			using (CurrentUserChanger.SwitchToNewUserTemporarily(batchProcessor.GS_LoginName))
			{
				incident = Factory.New<IncidentApproval>();
				Factory.Save();
				incident.IA_GS_NKApprovingStaff = staff.GS_Code;
				AssertEquals(staff.GS_Code, incident.ReportedByStaffCode);
			}
		}

		#endregion

		#region Need Confirm Criticality

		public void TestNeedConfirmCriticality()
		{
			IncidentApproval incident = Factory.NewWithValidTestData<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			AssertEquals(false, incident.NeedConfirmCriticality);
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			AssertEquals(true, incident.NeedConfirmCriticality);

			Factory.Save();
			AssertEquals(true, incident.NeedConfirmCriticality);

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			AssertEquals(false, incident.NeedConfirmCriticality);

			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			AssertEquals(false, incident.NeedConfirmCriticality);
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(false, incident.NeedConfirmCriticality);
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals(false, incident.NeedConfirmCriticality);
		}

		#endregion

		#region Incident Key

		public void TestIncidentKey()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			AssertEquals(incident.PK.ToString(), incident.IncidentKey);

			string key = ZGuid.NewZGuid().ToString();
			incident.AddIncidentKey(key);
			AssertEquals(key, incident.IncidentKey);
		}

		#endregion

		#region Last Sync Time

		public void TestLastSyncTime()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			AssertEquals(ZDateTime.Empty, incident.LastSyncTime);

			ZDateTime datetime = new ZDateTime(2012, 6, 15, 2, 16, 40, DateTimeKind.Utc).AddMilliseconds(324);
			incident.LastSyncTime = datetime;
			AssertEquals(new ZDateTime(2012, 6, 15, 2, 16, 40).AddMilliseconds(324), incident.LastSyncTime);

			datetime = new ZDateTime(2012, 6, 15, 2, 16, 41, DateTimeKind.Utc).AddMilliseconds(692);
			incident.LastSyncTime = datetime;
			AssertEquals(new ZDateTime(2012, 6, 15, 2, 16, 41).AddMilliseconds(692), incident.LastSyncTime);

			Factory.Save();

			incident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "LastSyncTime");
			var logs = incident.Logs.Find(query);
			AssertEquals(1, logs.Length);
			AssertEquals("LastSyncTime 2012-06-15T02:16:41.6920000Z", logs[0].SL_Reference);
			AssertEquals(new ZDateTime(2012, 6, 15, 2, 16, 41).AddMilliseconds(692), incident.LastSyncTime);
		}

		#endregion

		#region Is Closed Within 7 Days

		[TestDate(2013, 2, 26, 10, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsClosedWithin7Days()
		{
			DateTime now = new DateTime(2013, 2, 26, 10, 0, 0);

			var incident = Factory.New<IncidentApproval>();
			AssertEquals("Incident is not yet closed", false, incident.IsClosedWithin7Days);
			Factory.Save();

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.Closed;
			Factory.Save();
			AssertEquals("Incident is closed", true, incident.IsClosedWithin7Days);

			now = now.AddDays(7).AddHours(1);
			TestDateAttribute.Date = now;
			AssertEquals("Incident is closed more than 7 days", false, incident.IsClosedWithin7Days);

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.Closed;
			Factory.Save();
			AssertEquals("Status doesn't change so last close time remains the same", false, incident.IsClosedWithin7Days);

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			Factory.Save();
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.Closed;
			Factory.Save();
			AssertEquals("Status changes so last close time updates", true, incident.IsClosedWithin7Days);

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			Factory.Save();
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.ClosedAwaitingResponse;
			Factory.Save();
			now = now.AddDays(7).AddHours(1);
			TestDateAttribute.Date = now;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.Closed;
			Factory.Save();
			AssertEquals("Incident has been awaiting response for more than 7 days", false, incident.IsClosedWithin7Days);
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			var eRequest = Factory.New<IncidentApproval>();
			eRequest.IA_ClientReference = "SR00002491";

			AssertEquals("eRequest SR00002491", eRequest.HumanReadableName);

			eRequest.IA_IncidentNumber = "CS00149583";
			AssertEquals("eRequest CS00149583", eRequest.HumanReadableName);

			eRequest.IA_IncidentSummary = "Test Incident";
			AssertEquals("eRequest CS00149583 - Test Incident", eRequest.HumanReadableShortcutName);
		}

		#endregion

		#endregion

		#region Implementation

		class IncidentApprovalForTest : IncidentApproval
		{
			public IncidentApprovalForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SaveInternal()
			{
				if (SaveException != null)
				{
					throw SaveException;
				}

				base.SaveInternal();
			}

			public Exception SaveException;
		}

		#endregion
	}
}
