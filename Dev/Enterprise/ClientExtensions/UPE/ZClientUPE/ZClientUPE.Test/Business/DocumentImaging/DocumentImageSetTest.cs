using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.DocumentImaging.Testing
{
	sealed class DocumentImageSetTest : DocumentImageImportingTestCase
	{
		#region ImportAndDelete

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_Success()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			JobDeclaration.Factory.Save();
			AssertNotNull("Import should be successful", DocumentImageSet.Import());
			AssertEquals("Files deleted after a successful import", 0, Directory.GetFiles(Env.TempPath).Length);
			AssertEquals("1 multi-page document should be imported", 1, JobDeclaration.DocManagerInfo.Documents.Count);
			StorageDocs document = (StorageDocs)JobDeclaration.DocManagerInfo.Documents[0];
			AssertEquals(StorageDocs.Schema.SC_Desc, "UPS Test Document Type", document.SC_Desc);
			document.SaveToTempFile();
			try
			{
				using (ZImage image = ZImage.FromFile(document.TempFileName))
				{
					AssertEquals("2 pages should be in the imported document image", 2, image.PageCount);
				}
			}
			finally
			{
				File.Delete(document.TempFileName);
				document.Delete();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_CouldNotFindAttachee()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			AssertNull("Import should not be successful due to no attachment found", DocumentImageSet.Import());
			AssertEquals("Files should not be deleted after a failed import", 3, Directory.GetFiles(Env.TempPath).Length);
			AssertMultilineASCIIEquals("Notifications", @"
Could not find job with house bill 'M1302370459' for index file 'W41G0RYZ.000'
".Trim(), EmailedNotifications.AsString.Trim());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_WithIndexFileDeleteError()
		{
			ErrorReporter.Clear();
			CopyTestFileToTempRepository("W41G0RYZ.*");
			var indexFile = Path.Combine(Env.TempPath, "W41G0RYZ.000");
			var imageFile = Path.Combine(Env.TempPath, "W41G0RYZ.002");
			File.SetAttributes(indexFile, FileAttributes.ReadOnly);
			File.SetAttributes(imageFile, FileAttributes.ReadOnly);
			JobDeclaration.Factory.Save();
			AssertNull("Import should not be successful", DocumentImageSet.Import());
			AssertEquals("No documents should be imported", true, JobDeclaration.DocManagerInfo.Documents.Count == 0 || ((BusinessObject)JobDeclaration.DocManagerInfo.Documents[0]).IsInDatabase);
			Assert("error message", EmailedNotifications.AsString.Trim().StartsWith("Error: Error found with file '" + indexFile + "':"));
			File.SetAttributes(indexFile, FileAttributes.Normal);
			File.SetAttributes(imageFile, FileAttributes.Normal);
			ErrorReporter.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_WithImageFileDeleteError()
		{
			ErrorReporter.Clear();
			CopyTestFileToTempRepository("W41G0RYZ.*");
			var imageFile = Path.Combine(Env.TempPath, "W41G0RYZ.002");
			File.SetAttributes(imageFile, FileAttributes.ReadOnly);
			JobDeclaration.Factory.Save();
			AssertNotNull("Import should be successful even though an image file could not be deleted, because the index file is now gone", DocumentImageSet.Import());
			AssertEquals("1 multi-page document should be imported", 1, JobDeclaration.DocManagerInfo.Documents.Count);
			Assert("error message", EmailedNotifications.AsString.Trim().StartsWith("Error: Error found with file '" + imageFile + "':"));
			File.SetAttributes(imageFile, FileAttributes.Normal);
			ErrorReporter.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate]
		public void TestImportAndDelete_DeleteUnattachableFilesOlderThan1Months()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			AssertNull("Images could not be attached", DocumentImageSet.Import());
			AssertEquals("Files not deleted because image could not be attached", 3, Directory.GetFiles(Env.TempPath).Length);
			TestDateAttribute.Date = DateTime.Now.AddMonths(1).AddDays(1);
			AssertNull("Images could still not be attached", DocumentImageSet.Import());
			AssertEquals("Files deleted due because they are over 1 month old", 0, Directory.GetFiles(Env.TempPath).Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_NotifyUserInEmailNotificationsIfRequiredByDocumentType()
		{
			DocumentType.Description = "Document resulting in user notification";
			PrepareMaterialsForImportAndDelete(true);
			JobDeclaration.Factory.Save();
			DocumentImageSet.Import();
			AssertMultilineASCIIEquals("EmailedNotifications already notifies the batch processor INotifications", string.Empty, Notifications.AsString.Trim());
			AssertMultilineASCIIEquals("User should be notified by email of the import", @"Document of type 'Document resulting in user notification' processed for house bill 'M1302370459'.".Trim(), EmailedNotifications.AsString.Trim());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_DontNotifyUserInEmailNotificationsIfNotRequiredByDocumentType()
		{
			PrepareMaterialsForImportAndDelete(false);
			JobDeclaration.Factory.Save();
			DocumentImageSet.Import();
			AssertMultilineASCIIEquals("Batch processor INotifications should be notified", @"Document of type 'UPS Test Document Type' processed for house bill 'M1302370459'.".Trim(), Notifications.AsString);
			AssertMultilineASCIIEquals("User should NOT be notified by email", string.Empty, EmailedNotifications.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_NotifyUserIfUnknownImageType()
		{
			DocumentImageType catchAllImageType = DocumentTypes.FindByUPSCode(string.Empty);
			DocumentTypes.Remove(catchAllImageType);
			DocumentType.UPSCode = "XXX";
			PrepareMaterialsForImportAndDelete();
			CusHAWB.Factory.Save();
			DocumentImageSet.Import();
			AssertMultilineASCIIEquals("Notifications", @"Unknown image type aka DocType '03' for index file 'W41G0RYZ.000'", EmailedNotifications.AsString.Trim());
		}

		//This method is used to be observed for http://crikey.corporate.cargowise.com/TestFailureHistory.aspx?pk=9aae7f85-e464-46ff-b6bf-42db3e54032a 
		void PrepareMaterialsForImportAndDelete(bool? notifyOnImport = null)
		{
			EmailedNotifications.Clear();
			if (notifyOnImport != null)
			{
				DocumentType.NotifyOnImport = notifyOnImport.Value;
			}

			UpdateDocumentTypesRegistryValue();
			CopyTestFileToTempRepository("W41G0RYZ.*");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAndDelete_MoveJobIfRequiredByImageType()
		{
			const bool DocumentTypeRequiresMoveToClass = true;
			const bool ExpectMoveJobToClassification = true;
			// queues that dont require movement
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DocumentTypeRequiresMoveToClass, !ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.Submitted, string.Empty, DocumentTypeRequiresMoveToClass, !ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCA, ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker, DocumentTypeRequiresMoveToClass, !ExpectMoveJobToClassification);
			// queues that do require movement
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCO, ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DocumentTypeRequiresMoveToClass, ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DocumentTypeRequiresMoveToClass, ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCO, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, DocumentTypeRequiresMoveToClass, ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCO, ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DocumentTypeRequiresMoveToClass, ExpectMoveJobToClassification);
			// when DocumentImageType.MoveJobToClassOnImport == false
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCO, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, !DocumentTypeRequiresMoveToClass, !ExpectMoveJobToClassification);
			// without document type Commercial Invoice document (CI)
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCA, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, DocumentTypeRequiresMoveToClass, !ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, DocumentTypeRequiresMoveToClass, !ExpectMoveJobToClassification);
			DocumentType.UPSCode = DocumentImageType.CommercialInvoiceUPSCode;
			UpdateDocumentTypesRegistryValue();
			// with document type Commercial Invoice document (CI)
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.BCO, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, DocumentTypeRequiresMoveToClass, ExpectMoveJobToClassification);
			TestImportAndDelete_MoveJobIfRequiredByImageType(DeclarationQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, DocumentTypeRequiresMoveToClass, ExpectMoveJobToClassification);
		}

		void TestImportAndDelete_MoveJobIfRequiredByImageType(ZString initialQueue, ZString initialReason, bool documentTypeRequiresMoveToClass, bool expectJobMoveToClassification)
		{
			DocumentType.MoveJobToClassOnImport = documentTypeRequiresMoveToClass;
			UpdateDocumentTypesRegistryValue();
			CopyTestFileToTempRepository("W41G0RYZ.*");
			ReplaceInFile(Path.Combine(Env.TempPath, "W41G0RYZ.000"), "DocType=03", "DocType=" + DocumentType.UPSCode);
			documentImageSet = null;
			JobDeclaration.CurrentQueue.P4_CustomsQueue = initialQueue;
			JobDeclaration.CurrentQueue.P4_CustomsStatus = initialReason;
			Factory.Save();
			AssertNotNull("Import should be successful", DocumentImageSet.Import());
			AssertEquals("1 multi-page document should be imported", 1, JobDeclaration.DocManagerInfo.Documents.Count);
			if (expectJobMoveToClassification)
			{
				AssertEquals("Job should be moved to Classification", DeclarationQueueCodeDescriptionPairList.Codes.Classification, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			}
			else
			{
				AssertEquals("Job should remain in it's original queue", initialQueue, JobDeclaration.CurrentQueue.P4_CustomsQueue);
			}

			UPEDataRegistry.Instance.DocumentImagingImageTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, UPEDataRegistry.Instance.DocumentImagingImageTypes.DefaultValue);
			RecreateJobDeclaration();
		}

		#endregion
		#region GetBusinessObjectToAttachTo

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBusinessObjectToAttachTo_ForDeclaration()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			CusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			Factory.Save();
			AssertEquals("When a formal declaration exists, attach to it", JobDeclaration.PK, ((UPEJobDeclaration)DocumentImageSet.GetBusinessObjectToAttachTo()).PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBusinessObjectToAttachTo_WithDuplicateJobDeclaration()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			UPEJobDeclaration duplicateCusHAWB = Factory.New<UPEJobDeclaration>();
			duplicateCusHAWB.JE_AgentsReference = JobDeclaration.JE_AgentsReference;
			Factory.Save();
			AssertNull("When a duplicate JobDeclaration exists, don't return any job", DocumentImageSet.GetBusinessObjectToAttachTo());
			AssertEquals("Notifications for a duplicate JobDeclaration", "Duplicate matches found for house bill 'M1302370459' for index file 'W41G0RYZ.000'. This document must be imported manually.", EmailedNotifications.AsString.Trim());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBusinessObjectToAttachTo_ForCusHAWB()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			CusHAWB.Factory.Save();
			AssertEquals("When a CusHAWB exists without a formal declaration, attach to it", CusHAWB.PK, ((UPECusHAWB)DocumentImageSet.GetBusinessObjectToAttachTo()).PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBusinessObjectToAttachTo_ForSplitCusHAWB()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			UPECusHAWB subsequentSplitCusHAWB = (UPECusHAWB)CusHAWB.MAWB.ChildBills.AddNew();
			subsequentSplitCusHAWB.WayBillShort = CusHAWB.WayBillShort;
			subsequentSplitCusHAWB.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			subsequentSplitCusHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			Factory.Save();
			AssertEquals("When a split CusHAWB exists, return the one not marked as 'C1_SubsequentSplitShipment'", CusHAWB.PK, ((UPECusHAWB)DocumentImageSet.GetBusinessObjectToAttachTo()).PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBusinessObjectToAttachTo_WithDuplicateCusHAWB()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			UPECusHAWB duplicateCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			duplicateCusHAWB.WayBillShort = CusHAWB.WayBillShort;
			Factory.Save();
			AssertNull("When a duplicate CusHAWB exists, don't return any job", DocumentImageSet.GetBusinessObjectToAttachTo());
			AssertEquals("Notifications for a duplicate CusHAWB", "Duplicate matches found for house bill 'M1302370459' for index file 'W41G0RYZ.000'. This document must be imported manually.", EmailedNotifications.AsString.Trim());
		}

		#endregion
		readonly NotificationBuffer Notifications = new NotificationBuffer();
		readonly NotificationBuffer EmailedNotifications = new NotificationBuffer();
		DocumentImageSetForTesting DocumentImageSet
		{
			get
			{
				if (documentImageSet == null)
				{
					documentImageSet = new DocumentImageSetForTesting(Path.Combine(Env.TempPath, "W41G0RYZ.000"), Notifications, EmailedNotifications);
				}

				return documentImageSet;
			}
		}

		DocumentImageSetForTesting documentImageSet;
		class DocumentImageSetForTesting : DocumentImageSet
		{
			public DocumentImageSetForTesting(ZString indexFileName, INotifications notifications, INotifications emailedNotifications) : base(new DocumentIndexFileObject(new FileInfo(indexFileName), notifications, emailedNotifications), notifications, emailedNotifications)
			{
			}

			public new IDocManagerSupport GetBusinessObjectToAttachTo()
			{
				return base.GetBusinessObjectToAttachTo();
			}

			public new DocumentIndexFileObject Import()
			{
				return base.Import();
			}
		}
	}
}
