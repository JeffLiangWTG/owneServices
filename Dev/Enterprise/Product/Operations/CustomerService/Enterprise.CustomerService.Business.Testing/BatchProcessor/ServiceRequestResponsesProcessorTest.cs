using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	public class ServiceRequestResponsesProcessorTest : TestCaseWithFactory
	{
		public void TestCollectionProcess()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";

			Factory.Save();

			Xsd.CustomerServiceResponse re = new Xsd.CustomerServiceResponse();
			re.ClientReferenceNumber = incident.IA_ClientReference;
			re.IncidentNumber = "Number";

			MailItem correctItem = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re);

			correctItem.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			correctItem.MI_Body = "gdfgdgdgdf gdf dfg dfg dgkfTEXT";

			Factory.Save();

			var helper = new MessageFilterTestHelper<ServiceRequestResponsesProcessor, MailItem>();
			helper.Process(correctItem);

			AssertContains("Must contain record in a log file", "Processed Incident Approval", helper.Log.ToString());
		}

		public void TestUpdateIncidentStatus()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";

			Factory.Save();

			Xsd.CustomerServiceResponse re = new Xsd.CustomerServiceResponse();
			re.ClientReferenceNumber = incident.IA_ClientReference;
			re.IncidentNumber = "Number 1";
			re.Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;

			MailItem correctItem = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re);

			Factory.Save();

			var helper = new MessageFilterTestHelper<ServiceRequestResponsesProcessor, MailItem>();
			helper.Process(correctItem);

			AssertEquals("Must be eaquil", IncidentApprovalLookups.StatusCodes.ApprovedAndSent, incident.IA_Status);
		}

		public void TestProcessor()
		{
			IncidentApproval incident1 = Factory.New<IncidentApproval>();
			incident1.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident1.IA_Module = "COR";
			incident1.IA_IncidentSummary = "Test Summary";
			incident1.IA_IncidentDetails = "Test Details";

			IncidentApproval incident2 = Factory.New<IncidentApproval>();
			incident2.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident2.IA_Module = "COR";
			incident2.IA_IncidentSummary = "Test Summary";
			incident2.IA_IncidentDetails = "Test Details";

			IncidentApproval incident3 = Factory.New<IncidentApproval>();
			incident3.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident3.IA_Module = "COR";
			incident3.IA_IncidentSummary = "Test Summary";
			incident3.IA_IncidentDetails = "Test Details";

			IncidentApproval incident4 = Factory.New<IncidentApproval>();
			incident4.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident4.IA_Module = "COR";
			incident4.IA_IncidentSummary = "Test Summary";
			incident4.IA_IncidentDetails = "Test Details";

			Factory.Save();

			incident4.IA_ClientReference = incident3.IA_ClientReference;
			Factory.Save();

			Xsd.CustomerServiceResponse re1 = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponse re2 = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponse re3 = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponse re4 = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponse re5 = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponse re6 = new Xsd.CustomerServiceResponse();

			re1.ClientReferenceNumber = incident1.IA_ClientReference;
			re1.IncidentNumber = "Number 1";

			re2.ClientReferenceNumber = incident2.IA_ClientReference;
			re2.IncidentNumber = "Number 2";

			re3.ClientReferenceNumber = incident3.IA_ClientReference;
			re3.IncidentNumber = "Number 3";

			re4.ClientReferenceNumber = incident1.IA_ClientReference;
			re4.IncidentNumber = "Number 4";

			re5.ClientReferenceNumber = incident1.IA_ClientReference;
			re5.IncidentNumber = "Number 5";

			re6.ClientReferenceNumber = incident1.IA_ClientReference;
			re6.IncidentNumber = "Number 6";

			MailItem correctItem1 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re1);
			MailItem correctItem2 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re2);
			CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re3);
			CreateMailItem(MailDirection.Receive, MailStatus.Failed, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re4);
			CreateMailItem(MailDirection.Receive, MailStatus.Queued, "Wrong Subject", re5);
			CreateMailItem(MailDirection.Transmit, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re6);
			CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, null);

			correctItem1.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			correctItem1.MI_Body = "gdfgdgdgdf gdf dfg dfg dgkfTEXT";
			correctItem2.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			correctItem2.MI_Body = "gdfgdgdgdf gdf dfg dfg dgkfTEXT";

			Factory.Save();

			string mailSubj1 = correctItem1.MI_Subject;
			string mailSubj2 = correctItem2.MI_Subject;

			var helper = new MessageFilterTestHelper<ServiceRequestResponsesProcessor, MailItem>();
			helper.Process(correctItem1);
			helper.Process(correctItem2);

			AssertEquals("Must contain record in a log file", true, helper.Log.ToString().Contains("Number 1"));
			AssertEquals("Must contain record in a log file", true, helper.Log.ToString().Contains("Number 2"));

			AssertEquals("Must not contain record in a log file", false, helper.Log.ToString().Contains("Number 4"));
			AssertEquals("Must not contain record in a log file - incorrect status", false, helper.Log.ToString().Contains("Number 4"));
			AssertEquals("Must not contain record in a log file - incorrect subject", false, helper.Log.ToString().Contains("Number 5"));
			AssertEquals("Must not contain record in a log file - incorrect direction", false, helper.Log.ToString().Contains("Number 6"));
		}

		public void TestAddAndSaveEDocAttachments()
		{
			byte[] pDFbody = new byte[] { 1, 1, 1, 1, 1 };
			byte[] iMGbody = new byte[] { 0x42, 0x4D, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x00, 0x28, 0x00,
										  0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00,
										  0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
										  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x80, 0x00,
										  0x00, 0x00 };

			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";

			Factory.Save();

			Xsd.CustomerServiceResponse re1 = new Xsd.CustomerServiceResponse();

			re1.ClientReferenceNumber = incident.IA_ClientReference;
			re1.IncidentNumber = "Number 1";

			Xsd.CustomerServiceResponseAttachment tempDoc = new Xsd.CustomerServiceResponseAttachment();
			tempDoc.Data = iMGbody;
			tempDoc.FileName = "NewSCR.jpg";
			tempDoc.Desc = "Test SCR";
			tempDoc.DocType = "XXX";

			Xsd.CustomerServiceResponseAttachment tempFile = new Xsd.CustomerServiceResponseAttachment();
			tempFile.Data = pDFbody;
			tempFile.FileName = "TestName.txt";
			tempFile.Desc = "Test Desc";
			tempFile.DocType = Constants.RefDocTypes.RequestDocument;
			RefDocType refDocType1 = Factory.New<RefDocType>();
			refDocType1.RT_DocType = Constants.RefDocTypes.RequestDocument;
			refDocType1.RT_ReferenceType = Constants.ReferenceTypes.All;

			Xsd.CustomerServiceResponseAttachment tempDoc2 = new Xsd.CustomerServiceResponseAttachment();
			tempDoc2.Data = iMGbody;
			tempDoc2.FileName = "yada yada.jpg";
			tempDoc2.Desc = "Test COR";
			tempDoc2.DocType = "COR";
			RefDocType refDocType2 = Factory.New<RefDocType>();
			refDocType2.RT_DocType = "COR";
			refDocType2.RT_ReferenceType = Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

			re1.Attachments.Add(tempDoc);
			re1.Attachments.Add(tempFile);
			re1.Attachments.Add(tempDoc2);

			MailItem correctItem1 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re1);
			Factory.Save();

			ServiceRequestResponsesProcessor processor = new ServiceRequestResponsesProcessor();
			TestServiceLogger logger = new TestServiceLogger();
			processor.ProcessServiceRequestResponse(correctItem1, logger);

			AssertEquals("Must contain record in a log file", true, logger.ToString().Contains("Number 1"));
			incident.Reload();

			AssertEquals("Must contain 2 added documents", 2, incident.DocManagerInfo.Documents.Count);
			AssertEquals("Must contain 1 added files", 1, incident.DocManagerInfo.Files.Count);
			AssertEquals("DoctType", "XXX", incident.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("DoctType must not be MSC", Constants.RefDocTypes.RequestDocument, incident.DocManagerInfo.AllEDocs[1].DocType);
			AssertEquals("DoctType must COR", "COR", incident.DocManagerInfo.AllEDocs[2].DocType);

			Xsd.CustomerServiceResponse re2 = new Xsd.CustomerServiceResponse();

			re2.ClientReferenceNumber = incident.IA_ClientReference;
			re2.IncidentNumber = "Number 2";

			MailItem correctItem2 = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re2);
			Factory.Save();

			processor.ProcessServiceRequestResponse(correctItem2, logger);

			incident.Reload();
			AssertEquals("Must contain record in a log file", true, logger.ToString().Contains("Number 2"));
			AssertEquals("Must contain 0 added documents", 3, incident.DocManagerInfo.AllEDocs.Count);
		}

		public void TestProcessServiceRequestResponse()
		{
			IncidentApproval incident1 = Factory.New<IncidentApproval>();
			incident1.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident1.IA_Module = "COR";
			incident1.IA_IncidentSummary = "Test Summary";
			incident1.IA_IncidentDetails = "Test Details";

			IncidentApproval incident2 = Factory.New<IncidentApproval>();
			incident2.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident2.IA_Module = "COR";
			incident2.IA_IncidentSummary = "Test Summary";
			incident2.IA_IncidentDetails = "Test Details";

			Factory.Save();

			Xsd.CustomerServiceResponse re1 = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponse re2 = new Xsd.CustomerServiceResponse();

			re1.ClientReferenceNumber = incident1.IA_ClientReference;
			re1.IncidentNumber = "Number 1";

			re2.ClientReferenceNumber = incident2.IA_ClientReference;
			re2.IncidentNumber = "Number 2";

			MailItem item1 = CreateMailItem(MailDirection.Receive, MailStatus.Unprocessed, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, re1);
			MailItem item2 = CreateMailItem(MailDirection.Receive, MailStatus.Unprocessed, "Wrong Subject", re2);
			MailItem item3 = CreateMailItem(MailDirection.Receive, MailStatus.Unprocessed, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, null);

			var helper = new MessageFilterTestHelper<ServiceRequestResponsesProcessor, MailItem>();
			Assert(helper.Process(item1));
			AssertEquals("Number 1", incident1.IA_IncidentNumber);
			AssertEquals(1, helper.Log.Count);
			AssertEquals("Information|Processed Incident Approval SR00001000 - Incident Number 1", helper.Log[0]);
			helper.Log.ClearLog();

			Assert(!helper.Process(item2));
			AssertEquals(0, helper.Log.Count);

			Assert(!helper.Process(item3));
			AssertEquals(0, helper.Log.Count);
		}

		public void TestInccorectXmlAttachments()
		{
			ServiceRequestResponsesProcessor processor = new ServiceRequestResponsesProcessor();
			TestServiceLogger logger = new TestServiceLogger();
			MailItem incorrectItem = CreateMailItem(MailDirection.Receive, MailStatus.Queued, ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject, null);
			MailAttachment attachment1 = incorrectItem.MailAttachments.AddNew();
			attachment1.MA_Data = ZBlob.FromAscii("<html></html>");
			AssertExceptionThrown("Deseralize xml data failed.", typeof(InvalidOperationException), () => processor.ProcessServiceRequestResponse(incorrectItem, logger));
		}

		#region Implementation

		MailItem CreateMailItem(string direction, string status, string subject, Xsd.CustomerServiceResponse response)
		{
			ZXmlSerializer ser = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));

			MailItem item = Factory.New<MailItem>();
			using (MemoryStream xmlStream = new MemoryStream())
			{
				item.MI_Direction = direction;
				item.MI_Status = status;
				item.MI_Subject = subject;
				item.MI_LastAttemptDateTime = ZDateTime.Now;
				item.MI_ReceivedDateTime = ZDateTime.Now;
				item.MI_SendDateTime = ZDateTime.Now;

				if (response != null)
				{
					ser.Serialize(xmlStream, response);
					MailAttachment attachment1 = item.MailAttachments.AddNew();
					attachment1.MA_Data = new ZBlob(xmlStream.ToArray());
					attachment1.MA_FileName = "Incident Details.xml";
				}
			}

			return item;
		}

		#endregion
	}
}
