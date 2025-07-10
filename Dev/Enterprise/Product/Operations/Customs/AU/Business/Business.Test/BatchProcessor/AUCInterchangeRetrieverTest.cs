using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.BatchProcessor.Customs.Testing;
using Enterprise.Core;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using NUnit.Framework;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCInterchangeRetrieverTest : NewBaseInterchangeRetrieverTest
	{
		[TestDate(2012, 9, 1)]
		public void TestICSTextFormatResponse()
		{
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "Content-Disposition: attachment; filename=\"smime.p7m\"\r\n;Content-Type: application/pkcs7-mime; smime-type=enveloped-data\r\nname=\"smime.p7m\"\r\nContent-Transfer-Encoding: base64\r\n";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ICSTextFormatResponse.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "AAA336C_2JDI 6F1H B6B2001_AAA374M";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupNewCertificates();
			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				string result = batchProcessor.GetInterchangeText(item, false);
				AssertEquals("No interchange text", ZString.Empty, result);
			}
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "A Text message response has been received from Australian Customs"));
			AssertNotNull("Email sent to postmaster", email);
		}

		[TestDate(2005, 10, 1)]
		public void TestFIDPDFIsRecordedAgainstCustomsEntry()
		{
			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: FID for AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAAA7GW6R";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);
			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should be processed", "PRS", item.MI_Status);
			IDocManagerSupport docManagerSupport = entryHeader.Declaration;
			AssertEquals("eDocs document added", 1, docManagerSupport.DocManagerInfo.Files.Count);

			var entryPrintPDF = (BusinessObject)docManagerSupport.DocManagerInfo.Files[0];
			Assert("File should be in database", entryPrintPDF.IsInDatabase);
			var docType = (RefDocType)entryPrintPDF["DocType"];
			AssertEquals("eDocs document type exists and correct", "EPR", docType.RT_DocType);
		}

		[TestDate(2005, 10, 1)]
		public void TestPYRPDFIsRecordedAgainstCustomsEntry()
		{
			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: PYR for FID AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAAA7GW6R";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should be processed", "PRS", item.MI_Status);
			IDocManagerSupport docManagerSupport = entryHeader.Declaration;
			AssertEquals("eDocs document added", 1, docManagerSupport.DocManagerInfo.Files.Count);

			var entryPrintPDF = (BusinessObject)docManagerSupport.DocManagerInfo.Files[0];
			Assert("File should be in database", entryPrintPDF.IsInDatabase);
			var docType = (RefDocType)entryPrintPDF["DocType"];
			AssertEquals("eDocs document type exists and correct", "EPR", docType.RT_DocType);
		}

		[TestDate(2005, 10, 1, 10, 0, 0)]
		[TestUtcOffset(-12, 0, 0)]
		public void TestPYRPDFBeforeEntryNumberKnown()
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: PYR for FID AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();
			AUCustomsDataRegistry.Instance.SendUnmatchedICSReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NoEmails);

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should not have been processed", "QUE", item.MI_Status);
			IDocManagerSupport docManagerSupport = entryHeader.Declaration;
			AssertEquals("No eDocs document added", 0, docManagerSupport.DocManagerInfo.Files.Count);

			item.MI_ReceivedDateTime = item.MI_ReceivedDateTime.AddDays(-1);
			item.MI_ReceivedDateTime = item.MI_ReceivedDateTime.AddSeconds(-1);
			item.ResetReceivedUTCTimeForTesting();

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should have been processed, with PDF attachment forwarded to postmasters", "PRS", item.MI_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "[cmr] COMMERCIAL-IN-CONFIDENCE: PYR for FID AAAA7GW6R 001"));
			AssertNotNull("PDF forwarding to postmasters email", email);
			docManagerSupport = entryHeader.Declaration;
			AssertEquals("No eDocs document added", 0, docManagerSupport.DocManagerInfo.Files.Count);

			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_Status = "QUE";
			entryHeader.EntryNumber = "AAAA7GW6R";

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should now have been processed", "PRS", item.MI_Status);
			AssertEquals("eDocs document now added", 1, docManagerSupport.DocManagerInfo.Files.Count);

			var entryPrintPDF = (BusinessObject)docManagerSupport.DocManagerInfo.Files[0];
			Assert("File should be in database", entryPrintPDF.IsInDatabase);
			var docType = (RefDocType)entryPrintPDF["DocType"];
			AssertEquals("eDocs document type exists and correct", "EPR", docType.RT_DocType);
		}

		[TestDate(2005, 10, 1, 10, 0, 0)]
		[TestUtcOffset(-12, 0, 0)]
		public void TestOtherPDFWithoutEntryProcessedImmediately()
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: FID for AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should have been processed, with PDF attachment forwarded to postmasters", "PRS", item.MI_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "[cmr] COMMERCIAL-IN-CONFIDENCE: FID for AAAA7GW6R 001"));
			AssertNotNull("PDF forwarding to postmasters email", email);
		}

		[TestDate(2005, 10, 1, 10, 0, 0)]
		[TestUtcOffset(-12, 0, 0)]
		public void TestUnmatchedICSReports_HavePDFFileName()
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			//attached pdf in PDFEntryPrint.txt is "2005-10-31.pdf". 
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: Tariff Advice Report";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			AUCustomsDataRegistry.Instance.SendUnmatchedICSReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			AUCustomsDataRegistry.Instance.SendUnmatchedICSReportsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postMasters.PK.ToGuid());
			Factory.Save();

			SetupCertificates();

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals("Email should have been processed, with PDF attachment forwarded to postmasters", "PRS", item.MI_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "[cmr] COMMERCIAL-IN-CONFIDENCE: Tariff Advice Report"));
			AssertNotNull("PDF forwarding to postmasters email", email);
			AssertEquals("Get the original pdf file name for unmatch report", "2005-10-31.pdf", email.Attachments[0].DisplayName);
		}

		public void TestEmailWithNewSubjectLineIsProcessed()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CO1";
			company1.GC_Name = "Company 1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var com1Branch1 = company1.Branches.AddNew();
			com1Branch1.GB_Code = "BR1";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";
			company2.GC_Name = "Company 2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var com2Branch1 = company2.Branches.AddNew();
			com2Branch1.GB_Code = "BR2";

			Env.Registry.SetAUCustomsSenderIDForBranch(com1Branch1.PK.ToGuid(), "AUCustomsSenderID1");
			Env.Registry.SetAUCustomsSenderIDForBranch(com2Branch1.PK.ToGuid(), "AUCustomsSenderID2");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(com1Branch1.PK.ToGuid(), "AUCustomsSeaCargoDepotMailbox1");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(com2Branch1.PK.ToGuid(), "AUCustomsSeaCargoDepotMailbox2");
			Env.Registry.SetAUCustomsEdificeSenderIDForBranch(com1Branch1.PK.ToGuid(), "SetAUCustomsEdificeSenderID1");
			Env.Registry.SetAUCustomsEdificeSenderIDForBranch(com2Branch1.PK.ToGuid(), "SetAUCustomsEdificeSenderID2");

			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}
			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var rawRegistry = ZArchitecture.Environment.DataRegistry.Instance.RawRegistry;
			rawRegistry.AUCCompanyCertificateData.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("old_type3.pfx")));
			rawRegistry.AUCCompanyCertificatePassword.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCertificatePasswordForTest);
			CertificatesHelper.RemoveCertificates();
			CertificatesHelper.CreateCustomsCertificates2005();

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "OFFICIAL: Sensitive: FID for AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_Application = "AUI";
			Factory.Save();

			using (var processor = new AUCInterchangeRetriever())
			{
				processor.ExecuteBatch();
			}

			item.Reload();
			AssertEquals("Mail interchange with new subject line is processed", MailStatus.Processed, item.MI_Status);
		}

		[TestDate(2005, 10, 1)]
		public void TestATDPDFIsRecordedAgainstCustomsEntry()
		{
			var item = Factory.New<MailItem>();
			item.MI_Direction = DirectionList.Codes.Transmit;
			item.MI_Status = "QUE";
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: ATD for FID AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAAA7GW6R";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);

			using (var batchProcessor = new AUCInterchangeRetrieverTestProxy())
			{
				batchProcessor.GetInterchangeText(item, false);
			}

			AssertEquals(1, ((IDocManagerSupport)entryHeader.Declaration).DocManagerInfo.Files.Count);
			AssertEquals("PRS", item.MI_Status);
		}

		[TestDate(2005, 10, 1)]
		public void TestATDPDFIsRecordedAgainstCustomsEntryUsingWindowsCertificateStoreAndDecryptInNewThread()
		{
			SystemDataRegistry.Instance.UseWindowsCertificateStore.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("old_type3.pfx")), companyCertificatePasswordForTest);
			var store = new System.Security.Cryptography.X509Certificates.X509Store(System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser);
			store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadWrite);
			store.Remove(cert);
			store.Close();

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: ATD for FID AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAAA7GW6R";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);
			using (var batchProcessor2 = new AUCInterchangeRetrieverTestProxy())
			{
				try
				{
					batchProcessor2.GetInterchangeText(item, true);
				}
				catch (System.Security.Cryptography.CryptographicException)
				{
				}
				batchProcessor2.GetInterchangeText(item, true);
			}

			AssertEquals(1, ((IDocManagerSupport)entryHeader.Declaration).DocManagerInfo.Files.Count);
			AssertEquals("PRS", item.MI_Status);
		}

		[TestDate(2005, 10, 1)]
		public void TestNPDPdfIsStoredAgainstCustomsEntry()
		{
			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "COMMERCIAL-IN-CONFIDENCE: NPD for AAFAMG74L 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAFAMG74L";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);
			using (var interchangeRetriever = new AUCInterchangeRetrieverTestProxy())
			{
				interchangeRetriever.GetInterchangeText(item, false);
			}

			AssertEquals(1, ((IDocManagerSupport)entryHeader.Declaration).DocManagerInfo.Files.Count);
			var eDocItem = entryHeader.Declaration.DocManagerInfo.AllEDocs[0];
			AssertEquals("DocType", "CAU", eDocItem.DocType);
			AssertEquals("FileName", "Need to Produce Documents for AAFAMG74L.pdf", eDocItem.FileName);
			AssertEquals("PRS", item.MI_Status);
		}

		[TestDate(2005, 10, 1)]
		public void TestNPDPdfIsEmailedWhenThirdPartyUnsolicitedDocument()
		{
			var broker1 = CreateStaffMember("broker1", "B1", "Broker 1", "broker1@test1.com");
			var broker2 = CreateStaffMember("broker2", "B2", "Broker 2", "");
			var broker3 = CreateStaffMember("broker3", "B3", "Broker 3", "");
			var postMaster = CreateStaffMember("postmaster", "PM$", "Post Master", "pm@edi.com");

			var notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = "REFREJ";
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = notificationGroup.PK;
			link1.GK_GS = broker1.PK;
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = notificationGroup.PK;
			link2.GK_GS = postMaster.PK;

			AUCustomsDataRegistry.Instance.SendUnmatchedICSReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			AUCustomsDataRegistry.Instance.SendUnmatchedICSReportsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "COMMERCIAL-IN-CONFIDENCE: NPD for AAFAMG74L 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			using (var interchangeRetriever = new AUCInterchangeRetrieverTestProxy())
			{
				interchangeRetriever.GetInterchangeText(item, false);
			}

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatch) => emailToMatch.Subject == "COMMERCIAL-IN-CONFIDENCE: NPD for AAFAMG74L 001"));
			AssertNotNull("Email sent to UnmatchedICSReportsToGroup", email);
			AssertEquals("Should have been 2 recipients added to this notification", 2, email.Recipients.Count);
			Assert("Group user to be notified", email.Recipients.Contains("broker1@test1.com"));
			Assert("Group user to be notified", email.Recipients.Contains("pm@edi.com"));
			AssertEquals("Body", "The attached NPD document could not be matched to any local declaration.", email.Body);
			AssertEquals("Subject", "COMMERCIAL-IN-CONFIDENCE: NPD for AAFAMG74L 001", email.Subject);
			AssertEquals("Should have been 1 attachment added to this notification", 1, email.Attachments.Count);
			AssertEquals("Should have been 1 attachment added to this notification", "Need to Produce Documents for AAFAMG74L.pdf", email.Attachments[0].DisplayName);
			AssertEquals("PRS", item.MI_Status);
		}

		[TestDate(2005, 10, 1)]
		public void TestPdfIsEmailedWhenUnattachedUnsolicitedDocument()
		{
			var broker1 = CreateStaffMember("broker1", "B1", "Broker 1", "broker1@test1.com");
			var broker2 = CreateStaffMember("broker2", "B2", "Broker 2", "");
			var broker3 = CreateStaffMember("broker3", "B3", "Broker 3", "");
			var postMaster = CreateStaffMember("postmaster", "PM$", "Post Master", "pm@edi.com");

			var notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = "REFREJ";
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = notificationGroup.PK;
			link1.GK_GS = broker1.PK;
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = notificationGroup.PK;
			link2.GK_GS = postMaster.PK;

			AUCustomsDataRegistry.Instance.SendUnmatchedICSReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			AUCustomsDataRegistry.Instance.SendUnmatchedICSReportsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "COMMERCIAL-IN-CONFIDENCE: Another type of unsolicted Customs email report";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			using (var interchangeRetriever = new AUCInterchangeRetrieverTestProxy())
			{
				interchangeRetriever.GetInterchangeText(item, false);
			}

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatch) => emailToMatch.Subject == "COMMERCIAL-IN-CONFIDENCE: Another type of unsolicted Customs email report"));
			AssertNotNull("Email sent to UnmatchedICSReportsToGroup", email);
			AssertEquals("Should have been 2 recipients added to this notification", 2, email.Recipients.Count);
			Assert("Group user to be notified", email.Recipients.Contains("broker1@test1.com"));
			Assert("Group user to be notified", email.Recipients.Contains("pm@edi.com"));
			AssertEquals("Body", "The attached ICS document has been received unsolicted from Customs.", email.Body);
			AssertEquals("Subject", "COMMERCIAL-IN-CONFIDENCE: Another type of unsolicted Customs email report", email.Subject);
			AssertEquals("Should have been 1 attachment added to this notification", 1, email.Attachments.Count);
			AssertEquals("PRS", item.MI_Status);
		}

		[TestDate(2007, 09, 09, 09, 09, 09)]
		[ExpectNoExceptions]
		public void TestBlankBody()
		{
			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = ZString.Empty;
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: ATD for FID AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAAA7GW6R";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);
			using (var batchProcessor2 = new AUCInterchangeRetriever())
			{
				var found = false;
				batchProcessor2.Logger.OnLogInfoAdded += (string log, LogType logType) =>
				{
					if (log.Contains("Mail message has no content, ignoring. (from: " + aUCCustomsCCFEmailAddress + ", date: 09-Sep-07 09:09:09, subject: [cmr] COMMERCIAL-IN-CONFIDENCE: ATD for FID AAAA7GW6R 001)"))
					{
						found = true;
					}
				};

				batchProcessor2.ExecuteBatch();
				AssertEquals(true, found);
			}

			item.Reload();
			AssertEquals(0, ((IDocManagerSupport)entryHeader.Declaration).DocManagerInfo.Files.Count);
			AssertEquals("FAL", item.MI_Status);
		}

		[ExpectNoExceptions]
		[TestDate(2007, 09, 09, 09, 09, 09)]
		public void TestBlankAttachment()
		{
			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: ATD for FID AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MailAttachments.AddNew();
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAAA7GW6R";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			SetupCertificates();

			AssertEquals("Precondition", 0, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);
			using (var batchProcessor2 = new AUCInterchangeRetriever())
			{
				var found = false;
				batchProcessor2.Logger.OnLogInfoAdded += (string log, LogType logType) =>
				{
					if (log.Contains("Mail message has no content, ignoring. (from: " + aUCCustomsCCFEmailAddress + ", date: 09-Sep-07 09:09:09, subject: [cmr] COMMERCIAL-IN-CONFIDENCE: ATD for FID AAAA7GW6R 001)"))
					{
						found = true;
					}
				};

				batchProcessor2.ExecuteBatch();
				AssertEquals(true, found);
			}

			item.Reload();
			AssertEquals(0, ((IDocManagerSupport)entryHeader.Declaration).DocManagerInfo.Files.Count);
			AssertEquals("FAL", item.MI_Status);
		}

		[ExpectNoExceptions()]
		public void TestGenerateAcknowledgementWhenWeAreInUNOB()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "SENDER";
			interchange.EI_To = "RECIPIENT";
			interchange.EI_InterchangeNum = "REFNUM";
			batchProcessor.CreateAcknowledgementMessage(interchange);
		}

		public void TestRetrieveMultiPartOneStopInterchange()
		{
			var incomingEmail = Factory.New<MailItem>();
			incomingEmail.RawMIMEString = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingMultiPartOneStopInterchange.txt"));
			incomingEmail.ExtractAttachments();
			incomingEmail.MI_Direction = MailDirection.Receive;
			AssertEquals("precondition", 1, incomingEmail.MailAttachments.Count);
			AssertEquals("precondition", "eRoutEdi.pra", incomingEmail.MailAttachments[0].MA_FileName);
			AssertEquals("precondition", MailDirection.Receive, incomingEmail.MI_Direction);
			AssertEquals("precondition", MailStatus.Queued, incomingEmail.MI_Status);
			batchProcessor.GetMailFilter();
			MailFilterLocatorTestHelper.SetApplication(incomingEmail, MailFilterCodes.AUCInterchange, incomingEmail.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			batchProcessor.Execute();
			AssertEquals("NumberRetrieved", 1, batchProcessor.RetrievedInterchanges);
			var filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.OneStop);
			filter.AddToFilter(EDIInterchangeSchema.EI_To, "EDIAL");
			filter.AddToFilter(EDIInterchangeSchema.EI_From, "1STOP");
			filter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, "000290842");
			var oneStopInterchanges = (EDIInterchange[])Factory.Load(typeof(EDIInterchange), filter);
			AssertEquals("Number of one stop interchanges retrieved", 1, oneStopInterchanges.Length);
			var interchange = oneStopInterchanges[0];
			AssertEquals("Nessages not yet spwaned", 0, interchange.ContainedMessages.Count);
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchange.ContainedMessages.Load();
			AssertEquals("Number of messages in interchange now set", 1, interchange.ContainedMessages.Count);
		}

		public void TestRetrieveOneStopInterchange()
		{
			var incomingEmail = CreateIncomingMailItem();
			incomingEmail.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingOneStopInterchange.txt")).Replace("\r\n", "");
			incomingEmail.MI_From = MessagingConstants.eRouterPRAEmailAddress;
			incomingEmail.MI_Subject = AUCInterchangeRetriever.PRAMailBox;
			batchProcessor.GetMailFilter();
			MailFilterLocatorTestHelper.SetApplication(incomingEmail, MailFilterCodes.AUCInterchange, incomingEmail.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			batchProcessor.Execute();
			AssertEquals("NumberRetrieved", 1, batchProcessor.RetrievedInterchanges);
			var filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.OneStop);
			filter.AddToFilter(EDIInterchangeSchema.EI_To, "EDIAL");
			filter.AddToFilter(EDIInterchangeSchema.EI_From, "1STOP");
			filter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, "0026");
			var oneStopInterchanges = (EDIInterchange[])Factory.Load(typeof(EDIInterchange), filter);
			AssertEquals("Number of one stop interchanges retrieved", 1, oneStopInterchanges.Length);
			AssertEquals("Nessages not yet spwaned", 0, oneStopInterchanges[0].ContainedMessages.Count);
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			oneStopInterchanges[0].ContainedMessages.Load();
			AssertEquals("Number of messages in interchange now set", 1, oneStopInterchanges[0].ContainedMessages.Count);
		}

		public void TestValidRetrieveEXDOCTestInterchange()
		{
			TestValidRetrieveEXDOCInterchange(AUCustomsDataRegistry.GetEXDOCTestEmailAddress());
		}

		public void TestValidRetrieveEXDOCLiveInterchange()
		{
			TestValidRetrieveEXDOCInterchange(AUCustomsDataRegistry.GetEXDOCProdEmailAddress());
		}

		[TestDate(2010, 4, 8, 1, 1, 1)]
		public void TestDuplicatedEXDOCInterchangeAllowed()
		{
			var filter = GetEXDOCInterchangeFilter("406120100408010101");
			AssertNull("Pre-Condition, Interchange musn't already exist", Factory.LoadTop1<EDIInterchange>(filter));

			var testInterchange = Factory.New<EDIInterchange>();
			testInterchange.EI_To = EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox;
			testInterchange.EI_From = EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox;
			testInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.EXDOC;
			testInterchange.EI_InterchangeNum = "4061";
			var incomingTestEXDOCEmail = GetIncomingTestEXDOCEmail(AUCustomsDataRegistry.GetEXDOCTestEmailAddress());
			Factory.Save();

			batchProcessor.Execute();
			incomingTestEXDOCEmail.Reload();
			AssertEquals("EXDOC message has been retrieved", 1, batchProcessor.RetrievedInterchanges);
			AssertEquals("MailDBItem is set to processed", "PRS", incomingTestEXDOCEmail.MI_Status);
			AssertNotNull("Duplicated EXDOC interchange is retrieved with new number", Factory.LoadTop1<EDIInterchange>(filter));
		}

		public void TestProcessRemotePrintFileByDeclarationReference()
		{
			TestProcessRemotePrintFile_PCL(declaration => declaration.JE_DeclarationReference = "BTEST0001");
		}

		public void TestProcessRemotePrintFileByDeclarationReference_PDF()
		{
			TestProcessRemotePrintFile_PDF(declaration => declaration.JE_DeclarationReference = "BTEST0001");
		}

		public void TestProcessRemotePrintFileByOwnerReference()
		{
			TestProcessRemotePrintFile_PCL(declaration => declaration.JE_OwnerRef = "BTEST0001");
		}

		public void TestProcessRemotePrintFileByOwnerReference_PDF()
		{
			TestProcessRemotePrintFile_PDF(declaration => declaration.JE_OwnerRef = "BTEST0001");
		}

		public void TestProcessRemotePrintFileByEntryNumber()
		{
			TestProcessRemotePrintFile_PCL(declaration => declaration.QuarantineInvoice.QuarantineExDocHeader.QH_RequestForPermitNumber = "2100227");
		}

		public void TestProcessRemotePrintFileByEntryNumber_PDF()
		{
			TestProcessRemotePrintFile_PDF(declaration => declaration.QuarantineInvoice.QuarantineExDocHeader.QH_RequestForPermitNumber = "2100227");
		}

		public void TestUnmatchedRemotePrintFile_NotificationGroupNotSet()
		{
			using (AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var pmgGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				var staff = pmgGroup.Staff.AddNew();
				staff.GS_EmailAddress = "blah@blah.com";
				var item = CreateIncomingMailItem();
				item.MI_Subject = AUCInterchangeRetriever.EXDOCRemotePrintFileSubject;
				item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
				item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
				item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
				var attachment = item.MailAttachments.AddNew();
				attachment.MA_FileName = "4087.edi";
				attachment.MA_Data = ZBlob.FromAscii(@"%PR2     13674ExporterRef:BTEST0001,Commodity:H,RFPs(2100227,2100230),HCNbr:2088454,CertReqId:188989" + System.Environment.NewLine);
				MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
				Factory.Save();
				AssertEquals("Pre-Condition: Mail item queued", "QUE", item.MI_Status);

				AssertNoExceptionThrown(() => batchProcessor.Execute());

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "RFP Quarantine Remote Print receipt notification for Entry Number 2100227 / Exporter Reference BTEST0001");
				AssertEquals("Email sent to staff in Post Masters Group", "blah@blah.com", email.Recipients[0].Email);
			}
		}

		public void TestUnmatchedRemotePrintFile_PDF()
		{
			var aQISAcknowledgementsGroup = Factory.New<GlbGroup>();
			aQISAcknowledgementsGroup.GG_Code = "G1";
			aQISAcknowledgementsGroup.Staff.AddNew();
			aQISAcknowledgementsGroup.Staff[0].GS_EmailAddress = "blah@blah.com";
			AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aQISAcknowledgementsGroup.PK.ToGuid());

			var item = CreateIncomingMailItem();
			item.MI_Subject = AUCInterchangeRetriever.EXDOCRemotePrintFileSubject;
			item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
			item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			var attachment = item.MailAttachments.AddNew();
			attachment.MA_FileName = "4087.edi";
			attachment.MA_Data = ZBlob.FromAscii(@"%PR2     13674ExporterRef:BTEST0001,Commodity:H,RFPs(2100227,2100230),HCNbr:2088454,CertReqId:188989" + System.Environment.NewLine);
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();
			AssertEquals("Pre-Condition: Mail item queued", "QUE", item.MI_Status);

			batchProcessor.Execute();
			item.Reload();
			AssertEquals("Mail item processed at interchange - even though unmatched - processing will generate the required declaration", "PRS", item.MI_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "RFP Quarantine Remote Print receipt notification for Entry Number 2100227 / Exporter Reference BTEST0001");
			AssertNotNull("Unrecognized RFP email", email);
		}

		public void TestUnmatchedRemotePrintFile_PCL()
		{
			var aQISAcknowledgementsGroup = Factory.New<GlbGroup>();
			aQISAcknowledgementsGroup.GG_Code = "G1";
			aQISAcknowledgementsGroup.Staff.AddNew();
			aQISAcknowledgementsGroup.Staff[0].GS_EmailAddress = "blah@blah.com";
			AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aQISAcknowledgementsGroup.PK.ToGuid());

			var item = CreateIncomingMailItem();
			item.MI_Subject = AUCInterchangeRetriever.EXDOCRemotePrintFileSubject;
			item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
			item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			var attachment = item.MailAttachments.AddNew();
			attachment.MA_FileName = "4087.edi";
			attachment.MA_Data = ZBlob.FromAscii(@"PR2     13674ExporterRef:BTEST0001,Commodity:H,RFPs(2100227,2100230),HCNbr:2088454,CertReqId:188989" + System.Environment.NewLine + "TestData");
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();
			AssertEquals("Pre-Condition: Mail item queued", "QUE", item.MI_Status);

			batchProcessor.Execute();
			item.Reload();
			AssertEquals("Mail item processed at interchange - even though unmatched - processing will generate the required declaration", "PRS", item.MI_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "RFP Quarantine Remote Print receipt notification for Entry Number 2100227 / Exporter Reference BTEST0001");
			AssertNotNull("Unrecognized RFP email", email);
		}

		public void TestUnmatchedRemotePrintFileIncludesReference()
		{
			var aQISAcknowledgementsGroup = Factory.New<GlbGroup>();
			aQISAcknowledgementsGroup.GG_Code = "G1";
			aQISAcknowledgementsGroup.Staff.AddNew();
			aQISAcknowledgementsGroup.Staff[0].GS_EmailAddress = "john.citizen@everyman.com";
			AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aQISAcknowledgementsGroup.PK.ToGuid());

			var item = CreateIncomingMailItem();
			item.MI_Subject = AUCInterchangeRetriever.EXDOCRemotePrintFileSubject;
			item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
			item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			var attachment = item.MailAttachments.AddNew();
			attachment.MA_FileName = "4087.edi";
			attachment.MA_Data = ZBlob.FromAscii(@"%PR2     13674ExporterRef:BTEST0001,Commodity:H,RFPs(2100227,2100230),HCNbr:2088454,CertReqId:188989" + System.Environment.NewLine);
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();
			AssertEquals("Pre-Condition: Mail item queued", "QUE", item.MI_Status);

			batchProcessor.Execute();
			item.Reload();
			AssertEquals("Mail item processed at interchange - even though unmatched - processing will generate the required declaration", "PRS", item.MI_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "RFP Quarantine Remote Print receipt notification for Entry Number 2100227 / Exporter Reference BTEST0001");
			AssertNotNull("Unrecognized RFP email", email);
		}

		[TestDate(2004, 4, 1)]
		public void TestProcessNewEmail()
		{
			var filename = "NewEmail.txt";
			var expectedInterchange = "UNB+UNOC:3+AAA336C::AAA336C+AAA374M+040731:1236+00000000191694++++1++1'UNH+000001+CUSRES:D:99B:UN'BGM+961:::IDL+2CG4 16GB HHG4:001+14'NAD+MR+41065894724::95'DOC+1'RFF+TN:AAAAF6NP9'RFF+ACW:EDN'RFF+ABO:B00130952/1'RFF+ACE:20040731'FTX+AAH+++THE CAN MENTIONED IN THE LINE TO WHICH THIS ADVICE RELATES HAS BEEN DEEMED AS IDLE BY CUSTOMS. IF THE CAN HAS BEEN EXPORTED, PROVIDE CUSTOMS WITH A PROOF OF EXPORT. IF THE EXPORTATION HAS BEEN DELAYED, AMEND THE DATE OF EXPORT ON THE CAN TO THE NEW DATE. IF THE GOODS ARE NOT BE EXPORTED, WITHDRAW THE CAN. IF NO ACTION IS TAKEN BY 16-AUG-2004 THEN THE CAN AND THE AUTHORITY TO DEAL FOR THE ASSOCIATED GOODS WILL BE REVOKED BY CUSTOMS.'UNT+10+000001'UNZ+1+00000000191694'";
			TestProcessEmail(filename, expectedInterchange, true);
			TestProcessEmail(filename, expectedInterchange, false);
		}

		[TestDate(2004, 4, 1)]
		public void TestProcessOldEmail()
		{
			var filename = "OldEmail.txt";
			var expectedInterchange = "UNB+UNOC:3+AAA336C::AAA336C+AAA374M+040730:1733+00000000191693++++1++1'UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+1CFE 27AE GA64:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:K00001000/1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAAGNHKG'DOC+1'RFF+TN:AAAAGKERY'CST+0001'FTX+AHN+++CLEAR'CNT+5:0001'UNT+14+000001'UNZ+1+00000000191693'";
			TestProcessEmail(filename, expectedInterchange, true);
			TestProcessEmail(filename, expectedInterchange, false);
		}

		public void TestGetMailItemPRAEmail()
		{
			InsertEmail("EDI from: CSXWTADL to: " + AUCInterchangeRetriever.PRAMailBox + " IntRef: 20119", MessagingConstants.eRouterPRAEmailAddress);
			AssertEquals("MailLength", 1, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);
		}

		public void TestGetMailItemSeaCargoDepotBranchEmail()
		{
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(GlbCompany.CurrentCompany.Branches[0].PK.ToGuid(), "12345678901234");
			InsertEmail("EDI from: 06050000010028 to: 12345678901234 IntRef: 20119", MessagingConstants.eRouterPRAEmailAddress);
			AssertEquals("MailLength", 1, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);
		}

		public void TestGetMailItemSeaCargoDepotCompanyEmail()
		{
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(GlbCompany.CurrentCompany.PK.ToGuid(), "12345678901234");
			InsertEmail("EDI from: 06050000010028 to: 12345678901234 IntRef: 20119", MessagingConstants.eRouterPRAEmailAddress);
			AssertEquals("MailLength", 1, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);
		}

		public void TestGetMailItemCMREmail()
		{
			InsertEmail("[cmr] AAA336C_00000000221824_AAA374M", aUCCustomsCCFEmailAddress);
			AssertEquals("MailLength", 1, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);
		}

		public void TestGetMailItemCMREmailWithAngledBracketsAroundAddress()
		{
			InsertEmail("[cmr] AAA336C_00000000221824_AAA374M", " <" + aUCCustomsCCFEmailAddress + "> ");
			AssertEquals("MailLength", 1, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);
		}

		public void TestGetMailItemCMREmailNotForUs()
		{
			InsertEmail("[cmr] AAA336C_00000000221824_AAA999X", aUCCustomsCCFEmailAddress, false);
			AssertEquals("MailLength", 0, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);
		}

		public void TestGetMailItemMultipleCompanies()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K^@";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$^";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(GlbCompany.CurrentCompany.PK.ToGuid(), "12345678901234");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(company.PK.ToGuid(), "12345678901235");
			InsertEmail("EDI from: 06050000010028 to: 12345678901234 IntRef: 20119", MessagingConstants.eRouterPRAEmailAddress);
			InsertEmail("EDI from: 06050000010028 to: 12345678901234 IntRef: 20120", MessagingConstants.eRouterPRAEmailAddress);
			InsertEmail("EDI from: 06050000010028 to: 12345678901235 IntRef: 20121", MessagingConstants.eRouterPRAEmailAddress);

			//testing IncomingInterchangeMailFilter
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				AssertEquals("MailLength", 3, new AUCInterchangeRetriever().IncomingInterchangeMailFilter.Load(Factory, 100).Length);
			}

			AssertEquals("MailLength", 3, batchProcessor.IncomingInterchangeMailFilter.Load(Factory, 100).Length);

			//testing SetApplication - no exception should be thrown
			var item = CreateIncomingMailItem();
			item.MI_Subject = "EDI from: 06050000010028 to: 12345678901234 IntRef: 20119";
			item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);

			var item2 = CreateIncomingMailItem();
			item2.MI_Subject = "EDI from: 06050000010028 to: 12345678901235 IntRef: 20121";
			item2.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			MailFilterLocatorTestHelper.SetApplication(item2, MailFilterCodes.AUCInterchange, item2.MI_Direction == DirectionList.Codes.Receive);
		}

		public void TestProcessEmailWhichScannerHasViolated()
		{
			ZString bodyText = @"This is a multi-part message in MIME format.

--=_Boundary_gHiH5jUwCfWRGjkDe3T5
Content-Transfer-Encoding: Base64
content-disposition: attachment; filename=""eRoutEdi.erf""
content-type: application/octet-stream; charset=""us-ascii"";	name=""eroutedi.erf""

VU5BHx0uICAcVU5CHVVOT0IfMR1BQUEzMzZDHTA2MDUwMTA1MDAwNTU1HTA0MTExNx8xMDE2HTId
HUVESUZJQ0UdHR0dMRxVTkgdMjAwHUNVU1JFUx9EHzk3QR9VTh0dMRxCR00dOTYxHx8fQ1JFQVRF
HVMwMDAwMTAyMS8xLzEdMDExHE5BRB1DQh0wMDkzN0MfHzA5NRxUQVgdMx1DVUQcTU9BHTU1HzAu
MDAcVEFYHTMdR1NUHE1PQR0zNjkfNTAwLjAwHFRBWB0zHU9USBxNT0EdMjMfNDQuMDAcVEFYHTMd
T1RIHE1PQR01OB80LjkxHFRBWB0zHUdTVBxNT0EdNTgfMC40ORxUQVgdMx1PVEgcTU9BHTM1Hzku
NTAcVEFYHTMdT1RIHE1PQR0zMDQfMi41MBxUQVgdNB1UT1QcTU9BHTEyOB81NjEuNDAcRE9DHR8f
H0NSRUFURR05QjQzMjIwMDAxSi8wMS8wMB8fMhxNT0EdMzkfNTAwMC4wMBxNT0EdNDMfNTAwMC4w
MBxDU1QdMRxUQVgdMR1GUkUcTU9BHTQ1HzUwMDAuMDAcVEFYHTEdR1NUHE1PQR0zNjkfNTAwLjAw
HFVOVB0yOB0yMDAcVU5aHTEdMhw=

--=_Boundary_gHiH5jUwCfWRGjkDe3T5
Content-Type: text/plain;	charset=utf-8
Content-Transfer-Encoding: quoted-printable

-------------------------------Safe Stamp-------------------------------=
----
Your Anti-virus Service scanned this email. It is safe from known viruse=
s.
For more information regarding this service, please contact your service=
 provider.
--=_Boundary_gHiH5jUwCfWRGjkDe3T5--
";

			Env.Registry.SetAUCustomsSenderIDForBranch(GlbCompany.CurrentCompany.Branches[0].PK.ToGuid(), "12345678901234");
			InsertEmailWithMailBody("EDI from: 06050000010028 to: 12345678901234 IntRef: 20119", MessagingConstants.eRouterPRAEmailAddress, bodyText);
			batchProcessor.Execute();
			AssertEquals("NumberRetrieved", 1, batchProcessor.RetrievedInterchanges);
		}

		public void TestHandleEmailDecryptionException_SuccessButNotInFirstCompany()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CO1";
			company1.GC_Name = "Company 1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var com1Branch1 = company1.Branches.AddNew();
			com1Branch1.GB_Code = "BR1";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";
			company2.GC_Name = "Company 2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var com2Branch1 = company2.Branches.AddNew();
			com2Branch1.GB_Code = "BR2";

			Env.Registry.SetAUCustomsSenderIDForBranch(com1Branch1.PK.ToGuid(), "AUCustomsSenderID1");
			Env.Registry.SetAUCustomsSenderIDForBranch(com2Branch1.PK.ToGuid(), "AUCustomsSenderID2");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(com1Branch1.PK.ToGuid(), "AUCustomsSeaCargoDepotMailbox1");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(com2Branch1.PK.ToGuid(), "AUCustomsSeaCargoDepotMailbox2");
			Env.Registry.SetAUCustomsEdificeSenderIDForBranch(com1Branch1.PK.ToGuid(), "SetAUCustomsEdificeSenderID1");
			Env.Registry.SetAUCustomsEdificeSenderIDForBranch(com2Branch1.PK.ToGuid(), "SetAUCustomsEdificeSenderID2");

			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}
			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var rawRegistry = ZArchitecture.Environment.DataRegistry.Instance.RawRegistry;
			rawRegistry.AUCCompanyCertificateData.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("old_type3.pfx")));
			rawRegistry.AUCCompanyCertificatePassword.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCertificatePasswordForTest);
			CertificatesHelper.RemoveCertificates();
			CertificatesHelper.CreateCustomsCertificates2005();

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: FID for AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_Application = "AUI";

			Factory.Save();

			using (var processor = new AUCInterchangeRetriever())
			{
				processor.ExecuteBatch();
			}
			item.Reload();
			AssertEquals(
				"Processed under a company but not the first company, should succeed.", MailStatus.Processed, item.MI_Status
			);
		}

		public void TestHandleEmailDecryptionException_NoCompanyHasCredential()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CO1";
			company1.GC_Name = "Company 1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var com1Branch1 = company1.Branches.AddNew();
			com1Branch1.GB_Code = "BR1";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";
			company2.GC_Name = "Company 2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var com2Branch1 = company2.Branches.AddNew();
			com2Branch1.GB_Code = "BR2";

			Env.Registry.SetAUCustomsSenderIDForBranch(com1Branch1.PK.ToGuid(), "AUCustomsSenderID1");
			Env.Registry.SetAUCustomsSenderIDForBranch(com2Branch1.PK.ToGuid(), "AUCustomsSenderID2");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(com1Branch1.PK.ToGuid(), "AUCustomsSeaCargoDepotMailbox1");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(com2Branch1.PK.ToGuid(), "AUCustomsSeaCargoDepotMailbox2");
			Env.Registry.SetAUCustomsEdificeSenderIDForBranch(com1Branch1.PK.ToGuid(), "SetAUCustomsEdificeSenderID1");
			Env.Registry.SetAUCustomsEdificeSenderIDForBranch(com2Branch1.PK.ToGuid(), "SetAUCustomsEdificeSenderID2");

			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}
			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var item = Factory.New<MailItem>();
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Header = "content-type: application/pkcs7-mime;   name=\"smime.p7m\" ";
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("PDFEntryPrint.txt"));
			item.MI_From = aUCCustomsCCFEmailAddress;
			item.MI_Subject = "[cmr] COMMERCIAL-IN-CONFIDENCE: FID for AAAA7GW6R 001";
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_Application = "AUI";

			Factory.Save();

			AssertNoExceptionThrown(() => { batchProcessor.Execute(); });
			item.Reload();
			AssertEquals("Email's status should be set to FAL", MailStatus.Failed, item.MI_Status);
			AssertEquals("Logger should has warning message for missing Company Certificate file", true, batchProcessor.Logger.Logs.Any((l) => l.Type == LogType.Warning && l.Message.Contains("The 'Company Key File' is missing for company EDI")));
			AssertEquals("Logger should has warning message for missing Company Certificate file", true, batchProcessor.Logger.Logs.Any((l) => l.Type == LogType.Warning && l.Message.Contains("The 'Company Key File' is missing for company CO1")));
			AssertEquals("Logger should has warning message for missing Company Certificate file", true, batchProcessor.Logger.Logs.Any((l) => l.Type == LogType.Warning && l.Message.Contains("The 'Company Key File' is missing for company CO2")));
		}

		public void TestGetCompanyCodesFromMailItem()
		{
			var mail1 = Factory.New<MailItem>();
			mail1.MI_From = "test1@test1.gov.au";
			mail1.MI_Status = "QUE";
			mail1.MI_Direction = "RCV";
			mail1.MI_Application = "AUI";
			mail1.MI_Subject = "COMMERCIAL-IN-CONFIDENCE: B5F0FFA9";
			mail1.MI_ReceivedDateTime = ZDateTime.Now;

			var mail2 = Factory.New<MailItem>();
			mail2.MI_From = "test2@test2.gov.au";
			mail2.MI_Status = "QUE";
			mail2.MI_Direction = "RCV";
			mail2.MI_Application = "AUI";
			mail2.MI_Subject = "COMMERCIAL-IN-CONFIDENCE: 7DCC8E96";
			mail2.MI_ReceivedDateTime = ZDateTime.Now;

			var mail3 = Factory.New<MailItem>();
			mail3.MI_From = "xxx@xxx.gov.au";
			mail3.MI_Status = "QUE";
			mail3.MI_Direction = "RCV";
			mail3.MI_Application = "AUI";
			mail3.MI_Subject = "COMMERCIAL-IN-CONFIDENCE: B7281583";
			mail3.MI_ReceivedDateTime = ZDateTime.Now;

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CO1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company1.Branches.AddNew().GB_Code = "BR1";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company2.Branches.AddNew().GB_Code = "BR2";
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.AdditionalEDIMessageSender.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "test1@test1.gov.au"))
			using (AUCustomsDataRegistry.Instance.AdditionalEDIMessageSender.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "test2@test2.gov.au"))
			{
				var testItem = new AUCInterchangeRetrieverTestProxy();
				var result1 = testItem.GetCompanyCodesFromMailItem(mail1);
				AssertEquals("GetCompanyCodesFromMailItem for mail1 should return only company1", 1, result1.Count());
				AssertEquals("GetCompanyCodesFromMailItem for mail1 should return only company1", "CO1", result1.First());
				var result2 = testItem.GetCompanyCodesFromMailItem(mail2);
				AssertEquals("GetCompanyCodesFromMailItem for mail2 should return only company2", 1, result2.Count());
				AssertEquals("GetCompanyCodesFromMailItem for mail2 should return only company2", "CO2", result2.First());
				var result3 = testItem.GetCompanyCodesFromMailItem(mail3);
				AssertEquals("GetCompanyCodesFromMailItem for mail3 should return empty", false, result3.Any());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Assert("Default UseWindowsCertificateStore should be true", SystemDataRegistry.Instance.UseWindowsCertificateStore.Value);
			SystemDataRegistry.Instance.UseWindowsCertificateStore.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CertificatesHelper.CreateCustomsCertificates2021();

			batchProcessor = new AUCInterchangeRetrieverTestProxy();
			interchangeProcessor = new AUCInboundInterchangeProcessor(new LoggingInformation());

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType(testCurrentCCFAddress, "Current AU Customs CCF Email Address", "Current Email Address to which CMR Messages are/will be sent.");
			helper.CreateRefSysConfig(testCurrentCCFAddress, testCurrentCCFAddressValue, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddYears(2));
			helper.CreateRefSysConfigType(testPreviousCCFAddress, "Previous AU Customs CCF Email Address", "Previous Email Address to which CMR Messages are/were sent.");
			helper.CreateRefSysConfig(testPreviousCCFAddress, testPreviousCCFAddressValue, ZDateTime.Today.AddYears(-4), ZDateTime.Today.AddDays(-2));
			Factory.Save();

			aUCCustomsCCFEmailAddress = SysConfigHelper.Instance.AUCCustomsCCFCurrentEmailAddress;
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			batchProcessor.Dispose();
			SystemDataRegistry.Instance.UseWindowsCertificateStore.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			embeddedResourceRetriever.Dispose();
		}

		ICertificateManagerHelper CertificatesHelper => certificatesHelper ?? (certificatesHelper = ObjectFactory.New<ICertificateManagerHelper>(Factory));
		ICertificateManagerHelper certificatesHelper;

		ZString aUCCustomsCCFEmailAddress;
		AUCInterchangeRetrieverTestProxy batchProcessor;
		AUCInboundInterchangeProcessor interchangeProcessor;
		const string testCurrentCCFAddress = "AUCCFCurr";
		const string testCurrentCCFAddressValue = "cargo@ccf.abf.gov.au";
		const string testPreviousCCFAddress = "AUCCFPrev";
		const string testPreviousCCFAddressValue = "cargo@ccf.border.gov.au";
		const string companyCertificatePasswordForTest = "dvcmload";
		EmbeddedResourceRetriever embeddedResourceRetriever;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.BatchProcessor.TestFiles." + fileName;

		void AssertEDocs(JobDeclaration declaration)
		{
			AssertEquals("eDocs count", 1, ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs.Count);
			AssertEquals("DocType", "QRP", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("FileName", "454.pcl", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Description", "Quarantine Remote Print", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].Description);
		}

		void AssertEDocs_Pdf(JobDeclaration declaration)
		{
			AssertEquals("eDocs count", 1, ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs.Count);
			AssertEquals("DocType", "QRP", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("FileName", "454.pdf", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Description", "Quarantine Remote Print", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].Description);
		}

		GlbStaff CreateStaffMember(ZString loginName, ZString code, ZString fullName, ZString eMail)
		{
			var result = Factory.New<GlbStaff>();
			result.GS_LoginName = loginName;
			result.GS_Code = code;
			result.GS_FullName = fullName;
			result.GS_EmailAddress = eMail;
			return result;
		}

		void InsertEmail(ZString subject, ZString senderEmailAddress, bool shouldSucceed = true)
		{
			InsertEmail(subject, senderEmailAddress, ZString.Empty, shouldSucceed);
		}

		void InsertEmail(ZString subject, ZString senderEmailAddress, ZString interchangeText, bool shouldSucceed = true)
		{
			var mailBody = ZString.Empty;
			if (!interchangeText.IsEmpty)
			{
				mailBody = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(interchangeText));
			}

			InsertEmailWithMailBody(subject, senderEmailAddress, mailBody, shouldSucceed);
		}

		void InsertEmailWithMailBody(ZString subject, ZString senderEmailAddress, ZString mailBody, bool shouldSucceed = true)
		{
			var factory = new BusinessObjectFactory();
			var item = factory.New<MailItem>();

			item.MI_Header = @"Return-Path: <06050000010028@acsedi.edi.net.au>
Received: from syd.commercialcustoms.com.au (syd.commercialcustoms.com.au [127.0.0.1])	by syd.commercialcustoms.com.au (8.12.8/8.12.8) with ESMTP id iAGNKu96005243	for <ediccpsyd>; Wed, 17 Nov 2004 10:20:56 +1100
Received: from outrelay2.firstwave.com.au (outrelay2.firstwave.com.au [202.12.141.237])	by syd.commercialcustoms.com.au (8.12.8/8.12.8) with ESMTP id iAGNKuGp005239	for <ediccpsyd@commercialcustoms.com.au>; Wed, 17 Nov 2004 10:20:56 +1100
Received: from vwall2.firstwave.com.au (vwall2.firstwave.com.au [172.16.180.21])	by outrelay2.firstwave.com.au (8.12.10/8.12.6) with ESMTP id iAGNKtpk029494	for <ediccpsyd@commercialcustoms.com.au>; Wed, 17 Nov 2004 10:20:55 +1100
Received: from spam2.firstwave.com.au (localhost [127.0.0.1])	by vwall2.firstwave.com.au (8.12.10/8.12.10) with ESMTP id iAGNKtrT008643	for <ediccpsyd@commercialcustoms.com.au>; Wed, 17 Nov 2004 10:20:55 +1100
Received: from sswitch1.firstwave.com.au (sswitch1.firstwave.com.au 	[172.16.180.10])by spam2.firstwave.com.au (8.12.10/8.12.9) with ESMTP id 	iAGNKs0d024345for <ediccpsyd@commercialcustoms.com.au>; Wed, 17 Nov 2004 	10:20:54 +1100
Received: from acsedi.edi.net.au (TRA715.tradeway.com.au [210.10.90.225] 	(may be forged))by sswitch1.firstwave.com.au (8.12.10/8.12.10) with ESMTP 	id iAGNKpYg006057for <ediccpsyd@commercialcustoms.com.au>; Wed, 17 Nov 2004	 10:20:54 +1100
Date: Wed, 17 Nov 2004 10:20:51 +1100
Message-Id: <200411162320.iAGNKpYg006057@sswitch1.firstwave.com.au>
Received: from FWB ([192.168.210.218])by acsedi.edi.net.au 	(acsedi.edi.net.au [192.168.210.6])(MDaemon.PRO.v7.1.2.R)with ESMTP id 	md50007451977.msgfor <ediccpsyd@commercialcustoms.com.au>; Wed, 17 Nov 2004	 10:17:20 +1100
From: 06050000010028 <06050000010028@acsedi.edi.net.au>
To: ""06050105000555 via ediccpsyd@commercialcustoms.com.au"" <ediccpsyd@commercialcustoms.com.au>
Reply-To: <eRouterDeliveryFailures@acsedi.edi.net.au>
Subject: EDI from: 06050000010028 to: 06050105000555 IntRef: 2
Mime-Version: 1.0
X-Spam-Processed: acsedi.edi.net.au, Wed, 17 Nov 2004 10:17:20 +1100(not 	processed: message from valid local sender)
X-MDRemoteIP: 192.168.210.218
X-Return-Path: 06050000010028@acsedi.edi.net.au
X-MDaemon-Deliver-To: ediccpsyd@commercialcustoms.com.au
X-Archived: SAVENG4Lz2@sswitch1.firstwave.com.au
X-Brightmail-Tracker: AAAAAA==
X-Brightmail: No SPAM Detected
Content-Type: multipart/mixed;	boundary=""=_Boundary_gHiH5jUwCfWRGjkDe3T5""
Status:  O
X-UIDL: 419a8b7200000001
";

			item.MI_Subject = subject;
			item.MI_From = senderEmailAddress;
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_Status = MailStatus.Queued;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_Body = mailBody;
			if (!mailBody.IsEmpty)
			{
				item.ExtractAttachments();
			}
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, shouldSucceed && item.MI_Direction == DirectionList.Codes.Receive);
			factory.Save();
		}

		void TestProcessEmail(string emailBodyFilename, string expectedInterchange, bool extractAttachments)
		{
			Env.Registry.AUCCompanyCertificateData = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("type3.pfx"));
			Env.Registry.AUCCompanyCertificatePassword = "chess";
			CertificatesHelper.RemoveCertificates();
			CertificatesHelper.CreateCustomsCertificates2005();

			var item = Factory.New<MailItem>();
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath(emailBodyFilename));
			item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EmailHeader.txt"));
			item.MI_Subject = Env.Registry.AUCustomsSenderID;
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);

			if (extractAttachments)
			{
				item.ExtractAttachments();
				AssertEquals("NumberOfAttachemnts", 1, item.MailAttachments.Count);
			}

			using (var batchProcessor2 = new AUCInterchangeRetrieverTestProxy())
			{
				string result = batchProcessor2.GetInterchangeText(item, false);
				AssertEquals("Text", expectedInterchange, result);
			}
		}

		void TestValidRetrieveEXDOCInterchange(string from)
		{
			var filter = GetEXDOCInterchangeFilter("4061");
			var testInterchange = Factory.LoadTop1<EDIInterchange>(filter);
			AssertNull("Pre-Condition, Interchange musn't already exist", testInterchange);

			var incomingTestEXDOCEmail = GetIncomingTestEXDOCEmail(from);
			Factory.Save();

			batchProcessor.Execute();
			AssertEquals("EXDOC message has been retrieved", 1, batchProcessor.RetrievedInterchanges);
			incomingTestEXDOCEmail.Reload();
			AssertEquals("MailDBItem is set to processed", "PRS", incomingTestEXDOCEmail.MI_Status);
			testInterchange = Factory.LoadTop1<EDIInterchange>(filter);
			AssertNotNull("EXDOC interchange is now retrieved", testInterchange);
			AssertEquals("Nessages not yet spwaned", 0, testInterchange.ContainedMessages.Count);
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			testInterchange.ContainedMessages.Load();
			AssertEquals("Number of EXDOC messages in the interchange", 1, testInterchange.ContainedMessages.Count);
			testInterchange.Reload();
			AssertEquals("interchange type set", "EXD", testInterchange.EI_InterchangeType);
		}

		ZQuery GetEXDOCInterchangeFilter(string interchangeNum)
		{
			var filter = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.EXDOC);
			filter.AddToFilter(EDIInterchangeSchema.EI_To, EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox);
			filter.AddToFilter(EDIInterchangeSchema.EI_From, EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox);
			filter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, interchangeNum);
			return filter;
		}

		MailItem GetIncomingTestEXDOCEmail(string from)
		{
			var incomingTestEXDOCEmail = CreateIncomingMailItem();
			incomingTestEXDOCEmail.MI_Subject = "EXDOC EDI 21 4061";
			incomingTestEXDOCEmail.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
			incomingTestEXDOCEmail.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
			incomingTestEXDOCEmail.MI_From = "<" + from + ">";
			var incomingEXDOCAttachment = incomingTestEXDOCEmail.MailAttachments.AddNew();
			incomingEXDOCAttachment.MA_FileName = "4087.edi";
			incomingEXDOCAttachment.MA_Data = ZBlob.FromAscii("UNBUNOB28706080717134061EXDOC1UNH5080SANCRTD97BUNRF0801BGMAQ911RESTSTRNAQNAQRFFABEB00001002FTXAAOL00  E041  Segment  48  (DTM)  cannot  be  processed  because  prior  mandatory  segment  PNA  is  missingUNT65080UNZ14061");
			MailFilterLocatorTestHelper.SetApplication(incomingTestEXDOCEmail, MailFilterCodes.AUCInterchange, incomingTestEXDOCEmail.MI_Direction == DirectionList.Codes.Receive);
			return incomingTestEXDOCEmail;
		}

		void SetupCertificates()
		{
			Env.Registry.AUCCompanyCertificateData = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("old_type3.pfx"));
			Env.Registry.AUCCompanyCertificatePassword = companyCertificatePasswordForTest;
			CertificatesHelper.RemoveCertificates();
			CertificatesHelper.CreateCustomsCertificates2005();
		}

		void SetupNewCertificates()
		{
			Env.Registry.AUCCompanyCertificateData = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("type3_1.pfx"));
			Env.Registry.AUCCompanyCertificatePassword = companyCertificatePasswordForTest;
			CertificatesHelper.RemoveCertificates();
			CertificatesHelper.CreateCustomsCertificates2012();
		}

		void TestProcessRemotePrintFile_PCL(SetupDeclarationDelegate setupDeclarationDelegate)
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var item = CreateIncomingMailItem();
			item.MI_Subject = AUCInterchangeRetriever.EXDOCRemotePrintFileSubject;
			item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
			item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			var attachment = item.MailAttachments.AddNew();
			attachment.MA_FileName = "4087.edi";
			attachment.MA_Data = ZBlob.FromAscii("PR2     13674ExporterRef:BTEST0001,Commodity:H,RFPs(2100227,2100230),HCNbr:2088454,CertReqId:188989" + System.Environment.NewLine + "TestData");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_IsCancelled = false;
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			declaration.Invoices.AddNew();
			setupDeclarationDelegate(declaration);
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange);
			Factory.Save();

			AssertEquals("Pre-Condition: eDocs count", 0, ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs.Count);
			AssertEquals("Pre-Condition: Mail item queued", "QUE", item.MI_Status);

			batchProcessor.Execute();
			AssertEDocs(new BusinessObjectFactory().LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK)));

			item.Reload();
			AssertEquals("Mail item processed", "PRS", item.MI_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "RFP Quarantine Remote Print receipt notification for 2100227");
			AssertNotNull("email on successful receipt", email);
		}

		void TestProcessRemotePrintFile_PDF(SetupDeclarationDelegate setupDeclarationDelegate)
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			var item = CreateIncomingMailItem();
			item.MI_Subject = AUCInterchangeRetriever.EXDOCRemotePrintFileSubject;
			item.MI_Header = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailHeader.txt"));
			item.MI_Body = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingEXODCTestEmailBody.txt"));
			item.MI_From = "<" + AUCustomsDataRegistry.GetEXDOCProdEmailAddress() + ">";
			var attachment = item.MailAttachments.AddNew();
			attachment.MA_FileName = "4087.edi";
			attachment.MA_Data = ZBlob.FromAscii("%PR2     13674ExporterRef:BTEST0001,Commodity:H,RFPs(2100227,2100230),HCNbr:2088454,CertReqId:188989" + System.Environment.NewLine);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_IsCancelled = false;
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			declaration.Invoices.AddNew();
			setupDeclarationDelegate(declaration);
			MailFilterLocatorTestHelper.SetApplication(item, MailFilterCodes.AUCInterchange, item.MI_Direction == DirectionList.Codes.Receive);
			Factory.Save();

			AssertEquals("Pre-Condition: eDocs count", 0, ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs.Count);
			AssertEquals("Pre-Condition: Mail item queued", "QUE", item.MI_Status);

			batchProcessor.Execute();
			AssertEDocs_Pdf(new BusinessObjectFactory().LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK)));

			item.Reload();
			AssertEquals("Mail item processed", "PRS", item.MI_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "RFP Quarantine Remote Print receipt notification for 2100227");
			AssertNotNull("email on successful receipt", email);
		}

		delegate void SetupDeclarationDelegate(JobDeclaration declaration);

		sealed class AUCInterchangeRetrieverTestProxy : AUCInterchangeRetriever
		{
			public new ZString GetInterchangeText(MailItem item, bool decryptInNewThread) => base.GetInterchangeText(item, decryptInNewThread);

			public new void CreateAcknowledgementMessage(EDIInterchange interchangeToAcknowledge) => base.CreateAcknowledgementMessage(interchangeToAcknowledge);

			public new IMailFilter GetMailFilter() => base.GetMailFilter();

			public new void Execute() => base.Execute();

			public new IEnumerable<ZString> GetCompanyCodesFromMailItem(MailItem item) => base.GetCompanyCodesFromMailItem(item);
		}
	}
}
