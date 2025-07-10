using System;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MailManager.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	sealed class WowDataImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportCsv_ErrOnUnmatchOrgNotSet()
		{
			using (StreamReader message = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Parts\\Testing\\TestProducts.csv"))
			{
				WowDataRegistry.Instance.UnmatchedDataItemsAccount = ZGuid.Empty;
				NotificationBuffer buffer = new NotificationBuffer(null);
				fImporter.ImportData(message, buffer);
				AssertEquals("UnmatchOrgNotSet error raised", true, buffer.ContainsNotificationType(WowErrorType.UnmatchOrgNotSet));
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZGuid orgPK = new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(factory).PK;
				factory.Save();
				WowDataRegistry.Instance.UnmatchedDataItemsAccount = orgPK;
				buffer = new NotificationBuffer(null);
				fImporter.ImportData(message, buffer);
				AssertEquals("UnmatchOrgNotSet not raised", false, buffer.ContainsNotificationType(WowErrorType.UnmatchOrgNotSet));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailSentOnProcessMessagesError()
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			MailItem newMailItem1 = Factory.New<MailItem>();
			newMailItem1.MI_ReceivedDateTime = ZDateTime.Now;
			newMailItem1.MI_SendDateTime = ZDateTime.Now;
			newMailItem1.MI_Subject = "WOOLWORTH SHIPMENT PRE-ALERT FOR VSL SLD ON 09/29/05 EX MUMBAI (APL MUMBAI/041) (SYDNEY)";
			newMailItem1.MI_ReceivedDateTime = ZDateTime.Now;
			newMailItem1.MI_Direction = MailDirection.Receive;
			MailAttachment mail1_Attach1 = newMailItem1.MailAttachments.AddNew();
			mail1_Attach1.MA_Data = ZBlob.FromAscii(new WowTestUtil().ReadTestCsvFile("DataImportExport\\Testing\\TestMissingTrailer1.csv"));
			MailFilterLocatorTestHelper.SetApplication(newMailItem1, MailFilterCodes.WooliesDataImporter);
			Factory.Save();
			OutgoingMailCreatorForTest testOutgoingMailCreator = new OutgoingMailCreatorForTest();
			fImporter.OutgoingMailManager = testOutgoingMailCreator;
			fImporter.ImportFromEmails(ZDateTime.Empty, fNotifyBuffer, SourceInfo.EmptySourceInfo);
			AssertEquals("Should have errors", true, fNotifyBuffer.HasErrors);
			// check that an email is sent when there are errors
			AssertNotNull("Should be an email sent as the data has errors", ((OutgoingMailCreatorForTest)fImporter.OutgoingMailManager).LastMailItemSentInDB);
			// expect no exception to bubble
			fImporter.OutgoingMailManager = new ExceptionThrowingOutgoingMailCreator();
			fImporter.ImportFromEmails(ZDateTime.Empty, fNotifyBuffer, SourceInfo.EmptySourceInfo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDontDeleteEmailIfCouldntSave()
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			WowDataRegistry.Instance.ManagingImportsOutputDirectory = BaseSourcePath;
			string nonExistantDir = Env.GetTempFileName();
			File.Delete(nonExistantDir);
			WowDataRegistry.Instance.ManagingImportsOutputDirectory = nonExistantDir;
			MailItem newMailItem1 = Factory.New<MailItem>();
			newMailItem1.MI_ReceivedDateTime = ZDateTime.Now;
			newMailItem1.MI_SendDateTime = ZDateTime.Now;
			newMailItem1.MI_Subject = "WOOLWORTH SHIPMENT PRE-ALERT FOR VSL SLD ON 09/29/05 EX MUMBAI (APL MUMBAI/041) (SYDNEY)";
			newMailItem1.MI_ReceivedDateTime = ZDateTime.Now;
			newMailItem1.MI_Direction = MailDirection.Receive;
			MailAttachment mail1_Attach1 = newMailItem1.MailAttachments.AddNew();
			mail1_Attach1.MA_FileName = "attachment_filename";
			mail1_Attach1.MA_Data = ZBlob.FromAscii(new WowTestUtil().ReadTestCsvFile(@"DataImportExport\Testing\TestMissingTrailer1.csv"));
			MailFilterLocatorTestHelper.SetApplication(newMailItem1, MailFilterCodes.WooliesDataImporter);
			Factory.Save();
			OutgoingMailCreatorForTest testOutgoingMailCreator = new OutgoingMailCreatorForTest();
			fImporter.OutgoingMailManager = testOutgoingMailCreator;
			fImporter.ImportFromEmails(ZDateTime.Empty, fNotifyBuffer, SourceInfo.EmptySourceInfo);
			AssertEquals("Should have errors", true, fNotifyBuffer.ContainsNotificationType(WowErrorType.PostToDatabaseError));
			MailItem item = Factory.Load<MailItem>(newMailItem1.PK);
			Assert("Item should NOT be mark as processed as it didn't save", MailStatus.Processed != item.MI_Status);
		}

		#region Testing Failure Modes

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingHeader1()
		{
			TestInvalidCsvFile("TestMissingHeader1.csv", ErrorType.MissingHeader);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingHeader2()
		{
			TestInvalidCsvFile("TestMissingHeader2.csv", ErrorType.MissingHeader);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingTrailer1()
		{
			TestInvalidCsvFile("TestMissingTrailer1.csv", ErrorType.MissingTrailer);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingTrailer2()
		{
			TestInvalidCsvFile("TestMissingTrailer2.csv", ErrorType.MissingTrailer);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUnknownRecordType()
		{
			TestInvalidCsvFile("TestUnknownRecordType.csv", ErrorType.UnknownRecordType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingMasterRecord()
		{
			TestInvalidCsvFile("TestMissingMasterRecord.csv", ErrorType.MissingMasterRecord);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingPKFromNK()
		{
			TestInvalidCsvFile("TestMissingPKFromNK.csv", ErrorType.MissingPKFromNK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMoreThan1NKMatch()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			org1.OH_FullName = "duporg";
			org2.OH_FullName = "duporg";
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Testing\\TestMoreThan1NKMatch.csv"))
			{
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", fNotifyBuffer, SourceInfo.EmptySourceInfo, out transactionActions);
				Assert(WowErrorType.MoreThan1NKMatch.Message, fNotifyBuffer.ContainsNotificationType(WowErrorType.MoreThan1NKMatch));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNotEnoughColumns()
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			AUOrgSupplierPartCollection parts = new AUOrgSupplierPartCollection(Factory);
			parts.Load();
			parts.RemoveAndDeleteAll();
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Testing\\TestNotEnoughColumns.csv"))
			{
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", fNotifyBuffer, SourceInfo.EmptySourceInfo, out transactionActions);
				parts.Load();
				AssertEquals("10 records in the file, even though 3 records have not enough columns", 10, parts.Count);
				Assert(WowErrorType.NotEnoughColumns.Message, fNotifyBuffer.ContainsNotificationType(WowErrorType.NotEnoughColumns));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataTypeConversionError()
		{
			TestInvalidCsvFile("TestDataTypeConversionError.csv", ErrorType.MissingPKFromNK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalRecordCountMismatch()
		{
			TestInvalidCsvFile("TestTotalRecordCountMismatch.csv", ErrorType.TotalRecordCountMismatch);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestJunkFile1()
		{
			TestInvalidCsvFile("TestJunkFile1.csv", ErrorType.MissingHeader);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestJunkFile2()
		{
			TestInvalidCsvFile("TestJunkFile2.csv", ErrorType.MissingHeader);
		}

		void TestInvalidCsvFile(string testFileName, ErrorType errorType)
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Testing\\" + testFileName))
			{
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", fNotifyBuffer, SourceInfo.EmptySourceInfo, out transactionActions);
				Assert("Expecting " + errorType.Message, fNotifyBuffer.ContainsNotificationType(errorType));
			}
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMIOutputFileCreated()
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			Factory.Save();
			WowDataRegistry.Instance.ManagingImportsOutputDirectory = Env.TempPath;
			string fullFilePath = Path.Combine(WowDataRegistry.Instance.ManagingImportsOutputDirectory, "test.csv");
			using (StreamReader message = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Parts\\Testing\\TestProducts.csv"))
			{
				try
				{
					fImporter.ImportData(message, "test.csv", fNotifyBuffer, SourceInfo.EmptySourceInfo);
					AssertEquals("Output file created", true, File.Exists(fullFilePath));
				}
				finally
				{
					File.Delete(fullFilePath);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessagesAfterDate()
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			string attachmentData = new WowTestUtil().ReadTestCsvFile("DataImportExport\\UnprocessedOrdersAndContainersForEdiTrack\\Testing\\TestContainers.csv");
			MailItem mailItem = CreateNewMailItemWithAttachment("blah@blah", "WOOLWORTH SHIPMENT PRE-ALERT FOR VSL SLD ON 09/29/05 EX MUMBAI (APL MUMBAI/041) (SYDNEY)", attachmentData);
			Factory.Save();
			fImporter.ImportFromEmails(ZDateTime.Now, fNotifyBuffer, SourceInfo.EmptySourceInfo);
			mailItem.Reload();
			AssertEquals("Mail should not be processed as added date has passed", MailStatus.Queued, mailItem.MI_Status);
			fImporter.ImportFromEmails(ZDateTime.Now.AddDays(-1), fNotifyBuffer, SourceInfo.EmptySourceInfo);
			mailItem.Reload();
			AssertEquals("Mail should be processed as the added date has not yet passed", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestGetMailItemFilter_WhenFilteringTheSubjectSpecifically()
		{
			WowDataRegistry.Instance.FilterEmailImportFilesBySpecificSubject.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestGetMailItemFilter("Should not process mail item with IFTSTA junk subject", "test_valid@email.address.com", "IFTSTA", false);
			TestGetMailItemFilter("Should not process mail item with subject starting with 'Undeliverable:'", "test_valid@email.address.com", "Undeliverable: splaty", false);
			TestGetMailItemFilter("Should not process mail item with subject starting with 'Out of Office Reply:'", "test_valid@email.address.com", "out of office reply", false);
			TestGetMailItemFilter("Should not process mail item from edi", "test@acsedi.edi.net.au", "edi stuff", false);
			var ccfEmailAddress = Customs.AU.Declaration.Business.SysConfigHelper.Instance.AUCCustomsCCFCurrentEmailAddress;
			TestGetMailItemFilter("Should not process mail item from customs", ccfEmailAddress, "customs stuff", false);
			TestGetMailItemFilter("Should not process an email item with unknown subject", "test_valid@email.address.com", "valid subject", false);
			TestGetMailItemFilter("Should process container manifest", "\"Sriwanarat, Malai\" <Malai_Sriwanarat@apllogistics.com>", "WW shpmt prealert - Melbourne (Ex. BKK 09/19/05)", true);
			TestGetMailItemFilter("Should not process 'consolidation handover'", "\"Rabe Nicolene\" <nrabe@woolworths.com.au>", "Consolidation handover", false);
			TestGetMailItemFilter("Should process container manifest", "\"BOM, APLLBKG\" <APLLBKG_BOM@apl.com>", "WOOLWORTH SHIPMENT PRE-ALERT FOR VSL SLD ON 09/29/05 EX MUMBAI (APL MUMBAI/041) (SYDNEY)", true);
			TestGetMailItemFilter("Should process container manifest", "\"Lai, Calvin\" <Calvin_K_L_Lai@apllogistics.com>", "Woolworths - Pre-alert - Pac Banda 1539S (Etd-PD1: 21 Aug 2005) - BL#:39232580 (SYD)", true);
			TestGetMailItemFilter("Should process container manifest", "\"Lai, Calvin\" <Calvin_K_L_Lai@apllogistics.com>", "Woolworths - CSF 2301 0161J (Etd-PD1: 20 Aug 2005) - BL#:39232590 (MEL)", true);
		}

		public void TestGetMailItemFilter_WhenFilteringOnAnyEmailSentNotFromEdiOrCustoms()
		{
			WowDataRegistry.Instance.FilterEmailImportFilesBySpecificSubject.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestGetMailItemFilter("Should not process mail item with IFTSTA junk subject", "test_valid@email.address.com", "IFTSTA", false);
			TestGetMailItemFilter("Should not process mail item with subject starting with 'Undeliverable:'", "test_valid@email.address.com", "Undeliverable: splaty", false);
			TestGetMailItemFilter("Should not process mail item with subject starting with 'Out of Office Reply:'", "test_valid@email.address.com", "out of office reply", false);
			TestGetMailItemFilter("Should not process mail item from edi", "test@acsedi.edi.net.au", "edi stuff", false);
			var ccfEmailAddress = Customs.AU.Declaration.Business.SysConfigHelper.Instance.AUCCustomsCCFCurrentEmailAddress;
			TestGetMailItemFilter("Should not process mail item from customs", ccfEmailAddress, "customs stuff", false);
			TestGetMailItemFilter("Should process any other valid email item", "test_valid@email.address.com", "valid subject", true);
		}

		void TestGetMailItemFilter(string message, string from, string subject, bool expectToBeProcessed)
		{
			MailItem mailItem = CreateNewMailItemWithAttachment(from, subject, "Attachment Data", expectToBeProcessed);
			Factory.Save();
			TestWowDataImporter importer = new TestWowDataImporter(Factory);
			var foundMailItem = importer.GetMailItemFilter().Load(Factory, 1).SingleOrDefault();
			if (expectToBeProcessed)
			{
				AssertNotNull(message, foundMailItem);
				((BusinessObject)foundMailItem).Delete();
				Factory.Save();
			}
			else
			{
				AssertNull(message, foundMailItem);
			}
		}

		#region Implementation
		MailItem CreateNewMailItemWithAttachment(string from, string subject, string attachmentData, bool expectToBeProcessed = true)
		{
			MailItem mailItem = Factory.New<MailItem>();
			mailItem.MI_From = from;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.MI_SendDateTime = ZDateTime.Now;
			mailItem.MI_Subject = subject;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.MI_Direction = MailDirection.Receive;
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.WooliesDataImporter, expectToBeProcessed);
			MailAttachment attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii(attachmentData);
			return mailItem;
		}

		NotificationBuffer fNotifyBuffer;
		OutgoingMailCreatorForTest fOutgoingMailCreator;
		TestWowDataImporter fImporter;
		protected override void SetUp()
		{
			base.SetUp();
			fNotifyBuffer = new NotificationBuffer(null);
			fOutgoingMailCreator = new OutgoingMailCreatorForTest();
			fImporter = new TestWowDataImporter(Factory);
			fImporter.OutgoingMailManager = fOutgoingMailCreator;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType(testCurrentCCFAddress, "Current AU Customs CCF Email Address", "Current Email Address to which CMR Messages are/will be sent.");
			helper.CreateRefSysConfig(testCurrentCCFAddress, testCurrentCCFAddressValue, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddYears(2));
			Factory.Save();
		}

		const string testCurrentCCFAddress = "AUCCFCurr";
		const string testCurrentCCFAddressValue = "cargo@ccf.abf.gov.au";
		class TestWowDataImporter : WowDataImporter
		{
			public TestWowDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
			{
			}

			public new bool ImportDataToFactory(TextReader data, string attachmentFileName, INotifications notify, ISourceInfo sourceInfo, out ITransactionParticipant[] transactionActions)
			{
				return base.ImportDataToFactory(data, attachmentFileName, notify, sourceInfo, out transactionActions);
			}

			public new IMailFilter GetMailItemFilter()
			{
				return base.GetMailItemFilter();
			}
		}
		#endregion
	}
}
