using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.DocumentScanning.PlugIn.PlugInTesting;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Exception = System.Exception;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class eDocsPlugInTest : TestCaseWithFactory
	{
		public void TestInsertFromData_WhenGetDataReturnsNothing()
		{
			//Arrange
			var dataObjectMock = new Mock<IDataObject>();
			dataObjectMock.Setup(x => x.GetDataPresent(It.IsAny<string>())).Returns(true);
			dataObjectMock.Setup(x => x.GetData(It.IsAny<Type>())).Returns(null);
			PlugIn.GetZDataObjectFromDataForTest = d => throw new NotSupportedException(@"Invalid Path: c:\doomed\doomed.xxx");

			//Assert
			AssertNoExceptionThrown(() => PlugIn.InsertFromData(dataObjectMock.Object));
			AssertEquals("Error Invalid Path: c:\\doomed\\doomed.xxx", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPluginShouldNotBeShownIfParentSMDBIsZero()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new DocManagerDBHelperTestClassUsingRegistry();
			var dbNumbers = helper.GetStorageDocDbNumbersIncludingMainDb().ToList();
			dbNumbers.ForEach(d =>
			{
				if (d != 0)
				{
					Db.Connection.AlterDbWriteableStateForDocManager(helper.GetDatabaseName(d), false);
				}
			});

			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection1.TryGetLock(MutexIDs.StorageDocsDBBeingCreated.Name, out var dbLock));
				Assert(dbLock.IsHoldingLock());

				using (var plugIn = new eDocsPlugInForTesting(org))
				{
					var parent = plugIn.TopLevelParentMain;
					parent = plugIn.TopLevelParentMain;
					AssertEquals(0, parent.SM_DB);
					Assert(!plugIn.ShouldPlugInGUIAndBusinessEntityBeCreatedCoreForTest());
					AssertContains("The eDocs tab is not currently available, see below message:", plugIn.NotDisplayedMessage);
					AssertContains("Could not acquire lock for creating database", plugIn.NotDisplayedMessage);
				}

				dbLock.Dispose();
			}
		}

		public void TestTopLevelParentMainSM_TypeOverride()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader org = orgFactory.New<OrgHeader>();
			org.OH_FullName = "a123456";
			org.OH_Code = "~1111~";
			org.MainAddress.OA_Code = "123";
			orgFactory.Save();

			DocumentFactory smFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			StorageMain sm = smFactory.NewWithValidTestData<StorageMain>();
			sm.SM_ParentFK = org.PK;
			sm.SM_Type = "XXX";
			smFactory.Save();

			DocumentFactory smFactory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			sm = smFactory2.Load<StorageMain>(sm.PK);
			AssertEquals("XXX", sm.SM_Type);

			org = MasterFactory.Load<OrgHeader>(org.PK);
			using (var plugIn = new eDocsPlugInForTesting(org))
			{
				AssertEquals("XXX", plugIn.TopLevelParentMain.SM_Type);
				AssertEquals("ORG", plugIn.TopLevelParentMain.SM_TypeOverride);
			}
		}

		#region Related Business Objects

		public void TestMasterFactory()
		{
			AssertNotNull("Master factory should never be null", PlugIn.MasterFactory);
			DocumentFactory plugInFactory = PlugIn.MasterFactory;
			AssertEquals("Master factory instance should be cached, should use the same instance", plugInFactory, PlugIn.MasterFactory);

			AssertEquals("When host bizo IS IDocManagerSupport, then the factory for the plugin comes from the DocManagerInfo", ((IDocManagerSupport)Shipment).DocManagerInfo.MasterFactory, PlugIn.MasterFactory);

			using (var plugIn2 = new eDocsPlugInForTesting(Factory.New<OrgContact>()))
			{
				AssertNotNull("When host bizo is not IDocManagerSupport, then a new factory is created", plugIn2.MasterFactory);
			}
		}

		public void TestCurrentParentMainOnGrid()
		{
			AssertEquals("By default, should be TopLevelParent", PlugIn.TopLevelParentMainForTesting, PlugIn.CurrentParentMainOnGrid);

			StorageMain anotherStorageMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
			PlugIn.CurrentParentMainOnGrid = anotherStorageMain;
			AssertEquals("Should return other storage main object", anotherStorageMain, PlugIn.CurrentParentMainOnGrid);

			PlugIn.CurrentParentMainOnGrid = null;
			AssertEquals("If set to null, should return the top level object again", PlugIn.TopLevelParentMainForTesting, PlugIn.CurrentParentMainOnGrid);
		}

		public void TestTopLevelParentMainForNullDocManagerInfo()
		{
			DummyWithNullDocManagerInfo dummy = Factory.New<DummyWithNullDocManagerInfo>();
			using (eDocsPlugInForTesting plugIn = new eDocsPlugInForTesting(dummy))
			{
				AssertNull(plugIn.TopLevelParentMain);
			}
		}

		class DummyWithNullDocManagerInfo : DummyBusinessObject, IDocManagerSupport
		{
			public DummyWithNullDocManagerInfo(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			DocManagerInfo IDocManagerSupport.DocManagerInfo
			{
				get { return null; }
			}
		}

		#endregion

		#region ShowPreSaveDialogs

		public void TestShowPreSaveDialogs_WarnUserAboutOpenDocuments()
		{
			StorageFile storageFile = PlugIn.TopLevelParentMainForTesting.Files.AddNew();
			storageFile.SC_FileName = "test";
			storageFile.SC_DataType = "doc";
			string tempFile = storageFile.SaveToTempFile();
			StorageMain relatedMain = PlugIn.TopLevelParentMainForTesting.RelatedParentMains.AddNew();
			StorageFile relatedStorageFile = relatedMain.Files.AddNew();
			relatedStorageFile.SC_FileName = "testRelated";
			relatedStorageFile.SC_DataType = "doc";
			string relatedTempFile = relatedStorageFile.SaveToTempFile();

			try
			{
				using (File.Open(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					AssertEquals("File should be 'open' now for the test", true, storageFile.IsTempFileOpen);
					ContinueWithSave continueIfFileOpen = PlugIn.ShowPreSaveDialogs();
					AssertEquals("The following files are still open. If they have unsaved changes, they will be lost. Continue with save anyway?\r\n\r\ntest.doc", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
					AssertEquals("Save should be deniable while a file is open", ContinueWithSave.No, continueIfFileOpen);
					((UnitTestUserNotification)Globals.Message).AddOKAnswer();
					continueIfFileOpen = PlugIn.ShowPreSaveDialogs();
					AssertEquals("The following files are still open. If they have unsaved changes, they will be lost. Continue with save anyway?\r\n\r\ntest.doc", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
					AssertEquals("Save can be allowed if user clicks OK", ContinueWithSave.Yes, continueIfFileOpen);
				}
				AssertEquals("File should be 'closed' now for the test", false, storageFile.IsTempFileOpen);

				using (File.Open(relatedTempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					AssertEquals("Related file should be 'open' now for the test", true, relatedStorageFile.IsTempFileOpen);
					ContinueWithSave continueIfFileOpen = PlugIn.ShowPreSaveDialogs();
					AssertEquals("The following files are still open. If they have unsaved changes, they will be lost. Continue with save anyway?\r\n\r\ntestRelated.doc", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
					AssertEquals("Save should not be permitted while a related file is open", ContinueWithSave.No, continueIfFileOpen);
				}
				AssertEquals("Related file should be 'closed' now for the test", false, relatedStorageFile.IsTempFileOpen);

				ContinueWithSave continueIfFileClosed = PlugIn.ShowPreSaveDialogs();
				AssertEquals("Save should be permitted after the user has finished with the file", ContinueWithSave.Yes, continueIfFileClosed);
			}
			finally
			{
				File.Delete(tempFile);
				File.Delete(relatedTempFile);
			}
		}

		public void TestShowPreSaveDialogs_WarnUserAboutUnreadDocments()
		{
			Document1.SC_DocType = DocTypeWithForceUserToRead.RT_DocType;
			Document1.SC_SystemCreateUser = "XXX";
			MasterFactory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ContinueWithSave continueWhenChoseToRead = PlugIn.ShowPreSaveDialogs();
			AssertEquals("The save should not continue if the user has chose to read the eDocs", ContinueWithSave.No, continueWhenChoseToRead);
			AssertRelatedDocumentsNotReadEventAdded("Should not raise 'Unread Related eDocs' when the save is cancelled", false);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ContinueWithSave continueWhenChoseToSaveAnyway = PlugIn.ShowPreSaveDialogs();
			AssertEquals("Allow save to continue if the user has chose to not read the eDocs", ContinueWithSave.Yes, continueWhenChoseToSaveAnyway);
			AssertRelatedDocumentsNotReadEventAdded("'Unread Related eDocs' raised because the user chose not to read", true);
		}

		void AssertRelatedDocumentsNotReadEventAdded(string message, bool expectEvent)
		{
			StmALog log = PlugIn.HostBusinessObject.GetLogs().MostRecentLogByEventTime(Events.RelatedEDocsNotRead);
			if (expectEvent)
			{
				AssertNotNull(message, log);
			}
			else
			{
				AssertNull(message, log);
			}
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestAddRequiredDocument()
		{
			eDocsPlugInForTesting plugIn = null;
			try
			{
				base.SetUp();
				var dbHelper = new DocManagerDBHelperTestClass();
				TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
				TestCaseHelper.ClearTable(dbHelper.GetDatabaseName(1) + ".dbo." + StorageDocsSchema.Constants.TableName);
				var organisation = (IHaveRequiredDocuments)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
				Factory.Save();

				plugIn = new eDocsPlugInForTesting(organisation);
				plugIn.OnGUIShown();
				MasterFactory = plugIn.MasterFactory;
				var parent = plugIn.BusinessEntity as StorageMain;
				var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.PackingDeclaration)).FirstOrDefault();
				Assert(!docType.RT_AllowMultiplePeriodicDocs);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestTextFile.txt");
				var filenames = new string[] { filePath };

				plugIn.UseOverwrite = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.Add(filenames);
				MasterFactory.Save();

				var yesReqDoc = (organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PackingDeclaration));
				AssertNotNull("DocType should be Packing Declaration", organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PackingDeclaration));
				AssertEquals("DocCategory should be SCL", Core.Constants.ReferenceTypes.SupplyChainLogistics, yesReqDoc.EQ_DocCategory);
				AssertEquals("DocPeriod should be Periodic", Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, yesReqDoc.EQ_DocPeriod);
				AssertEquals(1, organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.PackingDeclaration).Count());

				var document2 = parent.Documents.AddNew();
				document2.SC_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
				plugIn.testStorageDoc = document2;
				MasterFactory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				plugIn.Add(filenames);
				MasterFactory.Save();

				var noReqDoc = (organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.ArrivalNotice));
				AssertNotNull("DocType should be Packing Declaration", organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.ArrivalNotice));
				AssertEquals("DocCategory should be CSR", Core.Constants.ReferenceTypes.ClientSupplierRelationship, noReqDoc.EQ_DocCategory);
				AssertEquals("DocPeriod should be Periodic", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, noReqDoc.EQ_DocPeriod);
				AssertEquals(1, organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.PackingDeclaration).Count());

				((IHaveRequiredDocuments)plugIn.TopLevelParentMain.DocumentOwner).RequiredDocuments.RemoveAndDeleteAll();
				plugIn.TopLevelParentMain.DocumentOwner.Reload();
				var document3 = parent.Documents.AddNew();
				document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				var desc = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 ";
				document3.SC_Desc = desc;

				plugIn.testStorageDoc = document3;
				MasterFactory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				plugIn.Add(filenames);
				MasterFactory.Save();

				var doc = (organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.MiscellaneousDocument));
				AssertNotNull("DocType should be Miscellaneous Document", organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.MiscellaneousDocument));
				AssertEquals("DocDescription should be blabla", desc.Substring(0, JobRequiredDocumentSchema.EQ_DocDescription.MaxLength), doc.EQ_DocDescription);
				AssertNotNull("AED event fired because it was received", (organisation as BusinessObject).GetLogs().MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
				AssertNotNull("AID event fired because it was received", (organisation as BusinessObject).GetLogs().MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			}
			finally
			{
				if (plugIn != null)
				{
					plugIn.Dispose();
				}

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestAddRequiredDocument_HR()
		{
			var applicant = Factory.New<Enterprise.Integration.Recruiter.IHRJobApplicant>();
			var bizo = (IHaveRequiredDocuments)applicant;
			applicant.HA_FullName = "full name";

			var newDocType = Factory.New<RefDocType>();
			newDocType.RT_ReferenceType = "HRE";
			newDocType.RT_DocType = "CVS";
			newDocType.RT_Desc = "Get a job!";
			newDocType.RT_AllowMultiplePeriodicDocs = false;

			Factory.Save();

			using (var plugIn = new eDocsPlugInForTesting(bizo))
			{
				plugIn.OnGUIShown();
				MasterFactory = plugIn.MasterFactory;
				var parent = plugIn.BusinessEntity as StorageMain;
				var document = parent.Documents.AddNew();
				document.SC_DocType = newDocType.RT_DocType;

				plugIn.AddRequiredDocument(document);
				MasterFactory.Save();

				var reqDoc = bizo.RequiredDocuments[0];
				AssertNotNull(reqDoc);
				AssertEquals("DocType should be newDocType", newDocType, reqDoc.DocType);
				AssertEquals("DocCategory should be HRE", Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment, reqDoc.EQ_DocCategory);
				AssertEquals("DocPeriod should be OncePerShipment", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, reqDoc.EQ_DocPeriod);
				AssertEquals("DocUsage should be the first one", reqDoc.Lookups.DocUsage_List[0].Code, reqDoc.EQ_DocUsage);
				AssertEquals(1, bizo.RequiredDocuments.GetElementsForTypeSortedByValidDate(newDocType.RT_DocType).Count());
			}
		}

		public void TestAddRequiredDocument_Shipment()
		{
			var bizo = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			Factory.Save();

			using (var plugIn = new eDocsPlugInForTesting(bizo))
			{
				plugIn.OnGUIShown();
				MasterFactory = plugIn.MasterFactory;
				var parent = plugIn.BusinessEntity as StorageMain;
				var document = parent.Documents.AddNew();
				document.SC_DocType = Core.Constants.RefDocTypes.PackingDeclaration;

				plugIn.AddRequiredDocument(document);
				MasterFactory.Save();

				var reqDocProvider = ((Freight.Business.IShipmentWithDocsAndCartage)bizo).RequiredDocumentsProvider;
				reqDocProvider.RequiredDocuments.Reload(true);
				var reqDoc = reqDocProvider.RequiredDocuments[0];
				AssertNotNull(reqDoc);
				AssertEquals("DocType should be PackingDeclaration", Core.Constants.RefDocTypes.PackingDeclaration, reqDoc.DocType.RT_DocType);
				AssertEquals("DocCategory should be SCL", Core.Constants.ReferenceTypes.SupplyChainLogistics, reqDoc.EQ_DocCategory);
				AssertEquals("DocPeriod should be OncePerShipment", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, reqDoc.EQ_DocPeriod);
				AssertEquals("DocUsage should be BTH (well not SHOULD, if a customer complains this can be changed to something else)", "BTH", reqDoc.EQ_DocUsage);
				AssertEquals(1, reqDocProvider.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.PackingDeclaration).Count());
			}
		}

		public void TestAddRequiredDocument_Private()
		{
			var bizo = (IHaveRequiredDocuments)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var newDocType = Factory.New<RefDocType>();
			newDocType.RT_ReferenceType = "ABC";
			newDocType.RT_DocType = "PRV";
			newDocType.RT_Desc = "Arbitrary";
			newDocType.RT_AllowMultiplePeriodicDocs = false;

			Factory.Save();

			using (var plugIn = new eDocsPlugInForTesting(bizo))
			{
				plugIn.OnGUIShown();
				MasterFactory = plugIn.MasterFactory;
				var parent = plugIn.BusinessEntity as StorageMain;
				var document = parent.Documents.AddNew();
				document.SC_DocType = newDocType.RT_DocType;

				plugIn.AddRequiredDocument(document);
				MasterFactory.Save();

				AssertEquals(0, bizo.RequiredDocuments.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 9, 9)]
		public void TestAddRequiredDocumentForMultipleTrackingRecordPeriodic()
		{
			eDocsPlugInForTesting plugIn = null;
			try
			{
				base.SetUp();
				var dbHelper = new DocManagerDBHelperTestClass();
				TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
				TestCaseHelper.ClearTable(dbHelper.GetDatabaseName(1) + ".dbo." + StorageDocsSchema.Constants.TableName);

				IHaveRequiredDocuments organisation = (IHaveRequiredDocuments)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
				Factory.Save();

				plugIn = new eDocsPlugInForTesting(organisation);
				plugIn.OnGUIShown();
				MasterFactory = plugIn.MasterFactory;
				var parent = plugIn.BusinessEntity as StorageMain;

				var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.PackingDeclaration)).FirstOrDefault();
				docType.RT_AllowMultiplePeriodicDocs = true;

				var requiredDocument = organisation.RequiredDocuments.AddNew();
				requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
				requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PackingDeclaration;
				var requiredDocumentPK = requiredDocument.PK;
				Factory.Save();

				var document = parent.Documents.AddNew();
				document.SC_DocType = Core.Constants.RefDocTypes.PackingDeclaration;
				plugIn.UseOverwrite = true;
				plugIn.testStorageDoc = document;
				MasterFactory.Save();

				string filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestTextFile.txt");
				string[] filenames = new string[] { filePath };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				MasterFactory.Save();
				requiredDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				Factory.Save();
				plugIn.Add(filenames);

				AssertEquals(1, organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.PackingDeclaration).Count());
				var yesReqDoc = (organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PackingDeclaration));
				AssertEquals(requiredDocumentPK, yesReqDoc.PK);
				AssertEquals(new ZDateTimeOffset(2014, 9, 9), yesReqDoc.EQ_DateReceived);

				var document2 = parent.Documents.AddNew();
				document2.SC_DocType = Core.Constants.RefDocTypes.PackingDeclaration;
				plugIn.testStorageDoc = document2;
				MasterFactory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.Add(filenames);
				MasterFactory.Save();

				var docs = organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.PackingDeclaration);
				AssertEquals(2, docs.Count());
				foreach (var doc in docs)
				{
					AssertEquals("DocCategory should be SCL", Core.Constants.ReferenceTypes.SupplyChainLogistics, doc.EQ_DocCategory);
					AssertEquals("DocPeriod should be Periodic", Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, doc.EQ_DocPeriod);
					AssertEquals(new ZDateTimeOffset(2014, 9, 9), doc.EQ_DateReceived);
				}

				var document3 = parent.Documents.AddNew();
				document3.SC_DocType = Core.Constants.RefDocTypes.PackingDeclaration;
				plugIn.testStorageDoc = document3;
				MasterFactory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				plugIn.Add(filenames);
				MasterFactory.Save();

				var docs2 = organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.PackingDeclaration);
				AssertEquals(3, docs2.Count());
				var newDocs = docs2.Except(docs).ToList();
				AssertEquals("DocCategory should be CSR", Core.Constants.ReferenceTypes.ClientSupplierRelationship, newDocs[0].EQ_DocCategory);
				AssertEquals("DocPeriod should be SHP", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, newDocs[0].EQ_DocPeriod);
				AssertEquals(new ZDateTimeOffset(2014, 9, 9), newDocs[0].EQ_DateReceived);
			}
			finally
			{
				if (plugIn != null)
				{
					plugIn.Dispose();
				}

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 9, 9)]
		public void TestAddRequiredDocumentForMultipleTrackingRecordOneTimeShipment()
		{
			eDocsPlugInForTesting plugIn = null;
			try
			{
				base.SetUp();
				var dbHelper = new DocManagerDBHelperTestClass();
				TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
				TestCaseHelper.ClearTable(dbHelper.GetDatabaseName(1) + ".dbo." + StorageDocsSchema.Constants.TableName);

				IHaveRequiredDocuments organisation = (IHaveRequiredDocuments)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
				Factory.Save();

				plugIn = new eDocsPlugInForTesting(organisation);
				plugIn.OnGUIShown();
				MasterFactory = plugIn.MasterFactory;
				var parent = plugIn.BusinessEntity as StorageMain;

				var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.Quotation)).FirstOrDefault();
				docType.RT_AllowMultiplePeriodicDocs = true;

				var document = parent.Documents.AddNew();
				document.SC_DocType = Core.Constants.RefDocTypes.Quotation;
				plugIn.UseOverwrite = true;
				plugIn.testStorageDoc = document;
				MasterFactory.Save();

				string filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestTextFile.txt");
				string[] filenames = new string[] { filePath };
				MasterFactory.Save();
				plugIn.Add(filenames);

				AssertEquals(1, organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.Quotation).Count());
				var requiredDocument1 = (organisation.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.Quotation));
				AssertEquals("DocCategory should be CSR", Core.Constants.ReferenceTypes.ClientSupplierRelationship, requiredDocument1.EQ_DocCategory);
				AssertEquals("DocPeriod should be SHP", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, requiredDocument1.EQ_DocPeriod);
				AssertEquals(new ZDateTimeOffset(2014, 9, 9), requiredDocument1.EQ_DateReceived);

				requiredDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				Factory.Save();

				var document2 = parent.Documents.AddNew();
				document2.SC_DocType = Core.Constants.RefDocTypes.Quotation;
				plugIn.testStorageDoc = document2;
				MasterFactory.Save();

				plugIn.Add(filenames);
				MasterFactory.Save();

				var docs = organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.Quotation);
				AssertEquals(2, docs.Count());
				var newDoc = docs.Where(d => d.PK != requiredDocument1.PK).ToList();
				AssertEquals(1, newDoc.Count);
				var requiredDocument2 = newDoc[0];
				AssertEquals("DocCategory should be CSR", Core.Constants.ReferenceTypes.ClientSupplierRelationship, requiredDocument2.EQ_DocCategory);
				AssertEquals("DocPeriod should be SHP", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, requiredDocument2.EQ_DocPeriod);
				AssertEquals(new ZDateTimeOffset(2014, 9, 9), requiredDocument2.EQ_DateReceived);

				TestDateAttribute.Date = new DateTime(2024, 9, 9);

				var document3 = parent.Documents.AddNew();
				document3.SC_DocType = Core.Constants.RefDocTypes.Quotation;
				plugIn.testStorageDoc = document3;
				MasterFactory.Save();
				plugIn.Add(filenames);
				MasterFactory.Save();
				Factory.Save();

				var docs2 = organisation.RequiredDocuments.GetElementsForTypeSortedByValidDate(Core.Constants.RefDocTypes.Quotation);
				AssertEquals("No new document tracking should be added.", 2, docs2.Count());
			}
			finally
			{
				if (plugIn != null)
				{
					plugIn.Dispose();
				}

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestIsTopLevelParentSelected()
		{
			StorageMain otherParent = (StorageMain)MasterFactory.New(typeof(StorageMain));

			PlugIn.CurrentParentMainOnGrid = otherParent;
			AssertEquals("Top Level Parent is not selected", false, PlugIn.IsTopLevelParentSelected);

			PlugIn.CurrentParentMainOnGrid = PlugIn.TopLevelParentMainForTesting;
			AssertEquals("top level parent is selected", true, PlugIn.IsTopLevelParentSelected);
		}

		public void TestOnParentFormDragOver()
		{
			OrgHeader org = MasterFactory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"))[0];

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();

				var plugIn = form.GetTestPlugInInstance();
				DataObject dataToDrop = new DataObject("hello");
				DragEventArgs args = new DragEventArgs(dataToDrop, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				plugIn.OnParentFormDragOverDone(form, args);
				AssertNull("Not Valid Data; Form should NOT be set", plugIn.parentFormOfPlugin);
				AssertEquals("Not Valid Data; Drag effects should NOT be set", DragDropEffects.None, args.Effect);

				dataToDrop = new DataObject(DataFormats.FileDrop, new string[] { TestingConstants.PDF_Sample });
				args = new DragEventArgs(dataToDrop, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				plugIn.OnParentFormDragOverDone(form, args);
				AssertEquals("Form should be set", form, plugIn.parentFormOfPlugin);
				AssertEquals("Drag effects should be set", DragDropEffects.Copy, args.Effect);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnParentFormDragDrop_FileDrop()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();

				var plugIn = form.GetTestPlugInInstance();
				AssertEquals("Precondition: Should not have any docs", 0, plugIn.TopLevelParentMain.eDocs.Count);

				DataObject dataToDrop = new DataObject(DataFormats.FileDrop, new string[] { TestingConstants.PDF_Sample });
				DragEventArgs args = new DragEventArgs(dataToDrop, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				plugIn.OnParentFormDragDropDone(form, args);
				AssertEquals("Form should be set", form, plugIn.parentFormOfPlugin);
				AssertEquals("Should now have one file", 1, plugIn.TopLevelParentMain.eDocs.Count);
			}
		}

		public void TestOnParentFormDragDropNoExceptionThrownWhenBusinessObjectIsNull()
		{
			using (var form = new ZDummyTemplateForm())
			{
				form.Show();

				var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert(!plugIn.IsInsertAllowed);

				var dataToDrop = new DataObject(DataFormats.FileDrop, new[] { TestingConstants.PDF_Sample });
				var args = new DragEventArgs(dataToDrop, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);

				AssertNoExceptionThrown(() => PlugIn.OnParentFormDragDropDone(form, args));
			}
		}

		public void TestIsMonitoringDocumentAttachmentOnParentFormDragDrop_FileDrop()
		{
			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();

				var plugIn = form.GetTestPlugInInstance();
				AssertEquals("Precondition: Should not have any docs", 0, plugIn.TopLevelParentMain.eDocs.Count);

				DataObject dataToDrop = new DataObject(DataFormats.FileDrop, new string[] { TestingConstants.PDF_Sample, TestingConstants.TIF_TwoBarcodes });
				DragEventArgs args = new DragEventArgs(dataToDrop, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				Assert(!plugIn.IsMonitoringDocumentAttachmentForDebug);
				plugIn.OnParentFormDragDropDone(form, args);
				Assert(plugIn.IsMonitoringDocumentAttachmentForDebug);
			}
		}

		public void TestName()
		{
			AssertEquals("Name should be eDocs", "eDocs", PlugIn.Name);
		}

		public void TestCanDelete()
		{
			AssertEquals("Delete should be enabled on plugin", true, PlugIn.CanDelete);
		}

		public void TestBusinessObjectBehindTopLevelForm()
		{
			OrgHeader organisation = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			using (var plugIn = new eDocsPlugInForTesting(organisation))
			{
				AssertEquals("Organisation should be the bizO behind top level form", organisation.PK, plugIn.HostBusinessObject.PK);
			}
		}

		public void TestDeleteDocumentsQuietly()
		{
			AssertEquals(5, Parent.eDocs.Count);
			AssertEquals(5, Parent.Documents.Count);
			((IDocumentManipulationSupport)PlugIn).DeleteDocumentsQuietly(DocumentList);

			var deletedLogs = (PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject).GetLogs().Find(log => log.SL_SE_NKEvent == "DDD");
			AssertEquals(5, deletedLogs.Count());
			var expectReferences = new string[]
			{ 
				$"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document1.tif' Document Deleted|{Document1.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document2.tif' Document Deleted|{Document2.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document3.tif' Document Deleted|{Document3.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document4.tif' Document Deleted|{Document4.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document5.tif' Document Deleted|{Document5.PK}"
			};
			AssertContainsExactElementsInAnyOrder(expectReferences, deletedLogs.Select(log => log.SL_Reference));

			foreach (StorageDocs document in DocumentList)
			{
				Assert("Deleted flag should be set", document.SC_IsDeleted);
				Assert("document should not think its deleted", !document.IsDeleted);
			}

			AssertEquals("No documents should have been removed from the collection", 5, Parent.eDocs.Count);
			AssertEquals("CollectionView should update to exclude deleted documents", 0, Parent.Documents.Count);
		}

		public void TestDeleteDocumentsPermanently()
		{
			((IDocumentManipulationSupport)PlugIn).DeleteDocumentsPermanently(DocumentList);
			AssertEquals(0, Parent.Documents.Count);

			var deletedLogs = (PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject).GetLogs().Find(log => log.SL_SE_NKEvent == "DDP");
			AssertEquals(5, deletedLogs.Count());
			var expectReferences = new string[]
			{ 
				$"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document1.tif' Document Deleted Permanently|{Document1.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document2.tif' Document Deleted Permanently|{Document2.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document3.tif' Document Deleted Permanently|{Document3.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document4.tif' Document Deleted Permanently|{Document4.PK}"
				, $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document5.tif' Document Deleted Permanently|{Document5.PK}"
			};
			AssertContainsExactElementsInAnyOrder(expectReferences, deletedLogs.Select(log => log.SL_Reference));

			foreach (StorageDocs document in DocumentList)
			{
				Assert(document.IsDeleted);
			}
		}

		public void TestDeleteRelatedDocumentsPermanently()
		{
			var relatedParent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			var document = relatedParent.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;

			PlugIn.CurrentParentMainOnGrid = relatedParent;
			var documentsToDelete = new BusinessObject[] { document };
			AssertNoExceptionThrown(() =>
			{
				((IDocumentManipulationSupport)PlugIn).DeleteDocumentsPermanently(documentsToDelete);
			});
		}

		public void TestDeleteDocument()
		{
			AssertEquals(5, Parent.Documents.Count);

			PlugIn.DeleteDocument(Document1.PK);

			var deletedLog = (PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject).GetLogs().MostRecentLogByEventTime(Events.DocumentDeletedPermanently);
			AssertNotNull(deletedLog);

			var expectedReference = $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document1.tif' Document Deleted Permanently|{Document1.PK}";
			AssertEquals(expectedReference, deletedLog.SL_Reference);

			AssertEquals(4, Parent.Documents.Count);
			Assert(Document1.IsDeleted);
			foreach (StorageDocs document in Parent.Documents)
			{
				Assert(!document.IsDeleted);
			}
		}

		public void TestDeleteDocumentWithWrongGuid()
		{
			AssertEquals(5, Parent.Documents.Count);

			PlugIn.DeleteDocument(Guid.Empty);

			AssertEquals(5, Parent.Documents.Count);

			foreach (StorageDocs document in Parent.Documents)
			{
				Assert(!document.IsDeleted);
			}
		}

		public void TestRestoreDocuments()
		{
			Document1.SC_IsDeleted = true;
			Document2.SC_IsDeleted = true;

			BusinessObject[] deletedDocuments = { Document1, Document2 };

			AssertEquals(3, Parent.eDocsView.Count);
			((IDocumentManipulationSupport)PlugIn).RestoreDocuments(deletedDocuments);
			var restoredLogs = (PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject).GetLogs().Find(log => log.SL_SE_NKEvent == "DRE");
			AssertEquals(2, restoredLogs.Count());
			var expectReferences = new string[] { $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document1.tif' Document Restored|{Document1.PK}", $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-document2.tif' Document Restored|{Document2.PK}" };
			AssertContainsExactElementsInAnyOrder(expectReferences, restoredLogs.Select(log => log.SL_Reference));

			foreach (StorageDocs document in deletedDocuments)
			{
				Assert(!document.SC_IsDeleted);
			}
			AssertEquals(5, Parent.eDocsView.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdd()
		{
			byte[] tifFileContents = DocumentUtilities.GetFileAsBytes(TifFile);
			byte[] xlsFileContents = DocumentUtilities.GetFileAsBytes(XlsFile);
			byte[] gifFileContents = DocumentUtilities.GetFileAsBytes(GifFile);
			Document1.SC_ImageData = tifFileContents;
			Document2.SC_ImageData = xlsFileContents;
			Document3.SC_ImageData = gifFileContents;

			BusinessObject[] documents = new BusinessObject[] { Document1, Document2, Document3 };
			SerializableEDocCollection dataFromClipboard = SerializableEDocCollection.New(documents);

			AssertEquals("Precondition: 5 documents on the plugin", 5, Parent.Documents.Count);

			((IDragDropSupport)PlugIn).Add(dataFromClipboard);

			AssertEquals("Should be original 5 + 3 more documents on the plugin after drag and drop", 8, Parent.Documents.Count);

			StorageDocs firstDocumentAdded = Parent.Documents[5];
			StorageDocs secondDocumentAdded = Parent.Documents[6];
			StorageDocs thirdDocumentAdded = Parent.Documents[7];

			AssertNotEquals("Contents should be converted correctly", tifFileContents, firstDocumentAdded.SC_ImageData);
			AssertEquals("Contents should be restored correctly", xlsFileContents, secondDocumentAdded.SC_ImageData);
			AssertEquals("Contents should be restored correctly", gifFileContents, thirdDocumentAdded.SC_ImageData);
		}

		public void TestAddStorageDocsFileWithSameFileName()
		{
			var fileName = "Jerry Test File Name";
			var docType = "PDF";
			var bizObj1 = Factory.NewWithValidTestData<OrgHeader>();
			var main1 = MasterFactory.NewWithValidTestData<StorageMain>();
			main1.SM_ParentFK = bizObj1.PK;
			main1.SM_Type = "ORG";
			var document1 = CreateDocument(main1);

			var bizObj2 = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj2))
			{
				form.Show();
				var plugIn = form.GetTestPlugInInstance();

				var dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { document1 });
				((IDragDropSupport)plugIn).Add(dataFromClipboard);

				dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { document1 });
				((IDragDropSupport)plugIn).Add(dataFromClipboard);

				AssertEquals("Should have 2 files", 2, plugIn.TopLevelParentMain.Files.Count);
				AssertEquals("Jerry Test File Name", plugIn.TopLevelParentMain.Files[0].SC_FileName);
				AssertEquals("Jerry Test File Name[2]", plugIn.TopLevelParentMain.Files[1].SC_FileName);
			}

			StorageDocsBase CreateDocument(StorageMain main)
			{
				var document = main.Files.AddNew();
				document.SC_FileName = fileName;
				document.SC_DocType = docType;
				document.SC_DataType = "PDF";
				document.SC_ImageData = new byte[] { 1, 2, 3 };
				return document;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdd_Will_Create_Logs_Accordingly()
		{
			var fileContent = DocumentUtilities.GetFileAsBytes(XlsFile);
			Document1.SC_ImageData = fileContent;
			var documents = new BusinessObject[] { Document1 };
			var dataFromClipboard = SerializableEDocCollection.New(documents);
			var initialDocumentCount = Parent.Documents.Count;

			((IDragDropSupport)PlugIn).Add(dataFromClipboard);
			AssertEquals("Precondition - There should be 1 more document on the plugin after drag and drop.", initialDocumentCount + 1, Parent.Documents.Count);
			MasterFactory.Save();

			var documentOwner = PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject;
			var logs = documentOwner.GetLogs();
			AssertNotNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Document1.DocType.RT_DocType, Parent.Documents[Parent.Documents.Count - 1].PK.ToString())));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_DeleteDocumentsQuietly() => AssertAddLogForDocument(Events.DocumentDeleted, (newDocument) => ((IDocumentManipulationSupport)PlugIn).DeleteDocumentsQuietly(new BusinessObject[] { newDocument }));

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_DeleteDocument() => AssertAddLogForDocument(Events.DocumentDeletedPermanently, (newDocument) => PlugIn.DeleteDocument(newDocument.PK));

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_DeleteDocumentsPermanently() => AssertAddLogForDocument(Events.DocumentDeletedPermanently, (newDocument) => ((IDocumentManipulationSupport)PlugIn).DeleteDocumentsPermanently(new BusinessObject[] { newDocument }));

		void AssertAddLogForDocument(Event logEvent, Action<StorageDocsBase> documentOperation)
		{
			var fileContent = DocumentUtilities.GetFileAsBytes(XlsFile);
			Document1.SC_ImageData = fileContent;
			var documents = new BusinessObject[] { Document1 };
			var dataFromClipboard = SerializableEDocCollection.New(documents);
			var initialDocumentCount = Parent.Documents.Count;

			((IDragDropSupport)PlugIn).Add(dataFromClipboard);
			AssertEquals("Precondition - There should be 1 more document on the plugin after drag and drop.", initialDocumentCount + 1, Parent.Documents.Count);

			var newDocument = Parent.Documents[Parent.Documents.Count - 1];
			var referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Document1.DocType.RT_DocType, newDocument.PK.ToString());
			var referenceForLogEvent = DocumentLogReferenceHelper.GetReference(newDocument, logEvent);

			documentOperation(newDocument);
			AssertEquals("The new document should be deleted", initialDocumentCount, Parent.Documents.Count);

			MasterFactory.Save();

			var documentOwner = PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject;
			var logs = documentOwner.GetLogs();

			AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
			AssertNull(logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
			AssertNull(Parent.Logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));

			((IDragDropSupport)PlugIn).Add(dataFromClipboard);
			AssertEquals("Precondition - There should be 1 more document on the plugin after drag and drop.", initialDocumentCount + 1, Parent.Documents.Count);

			newDocument = Parent.Documents[Parent.Documents.Count - 1];
			referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Document1.DocType.RT_DocType, newDocument.PK.ToString());
			referenceForLogEvent = DocumentLogReferenceHelper.GetReference(newDocument, logEvent);
			MasterFactory.Save();

			AssertNotNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));

			documentOperation(newDocument);
			AssertEquals("The new document should be deleted", initialDocumentCount, Parent.Documents.Count);

			MasterFactory.Save();
			AssertNotNull(logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
			AssertNotNull(Parent.Logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_RestoreDocuments()
		{
			var fileContent = DocumentUtilities.GetFileAsBytes(XlsFile);
			Document1.SC_ImageData = fileContent;
			var documents = new BusinessObject[] { Document1 };
			var dataFromClipboard = SerializableEDocCollection.New(documents);
			var initialDocumentCount = Parent.Documents.Count;

			((IDragDropSupport)PlugIn).Add(dataFromClipboard);
			AssertEquals("Precondition - There should be 1 more document on the plugin after drag and drop.", initialDocumentCount + 1, Parent.Documents.Count);

			MasterFactory.Save();
			var newDocument = Parent.Documents[Parent.Documents.Count - 1];
			var referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Document1.DocType.RT_DocType, newDocument.PK.ToString());
			var referenceForDeleted = DocumentLogReferenceHelper.GetReference(newDocument, Events.DocumentDeleted);
			var referenceForRestored = DocumentLogReferenceHelper.GetReference(newDocument, Events.DocumentRestored);

			((IDocumentManipulationSupport)PlugIn).DeleteDocumentsQuietly(new BusinessObject[] { newDocument });
			((IDocumentManipulationSupport)PlugIn).RestoreDocuments(new BusinessObject[] { newDocument });

			MasterFactory.Save();
			var documentOwner = PlugIn.TopLevelParentMain.DocumentOwner as EnterpriseBusinessObject;
			var logs = documentOwner.GetLogs();

			AssertNotNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
			AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentDeleted, referenceForDeleted));
			AssertNotNull(Parent.Logs.MostRecentLogByEventTime(Events.DocumentDeleted, referenceForDeleted));
			AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentRestored, referenceForRestored));
			AssertNotNull(Parent.Logs.MostRecentLogByEventTime(Events.DocumentRestored, referenceForRestored));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_RelatedObject_DocumentImported()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;

				var relatedBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
				relatedBizObj.OH_Code = "JERRYTEST";
				var relatedStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
				relatedStorageMain.SM_ParentFK = relatedBizObj.PK;
				relatedStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(relatedStorageMain, relatedBizObj);
				MasterFactory.Save();

				plugIn.CurrentParentMainOnGrid = relatedStorageMain;

				AssertEquals("TopLevelParentMain should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filePath }))
				{
					var args = new DragEventArgs(data, 0, 10, 60, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
					plugIn.OnParentFormDragDropDone(form, args);
					MasterFactory.Save();

					var logs = bizObj.GetLogs();
					var relatedLogs = relatedBizObj.GetLogs();
					var newDocument = relatedStorageMain.Documents[0];
					var referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", newDocument.SC_DocType, newDocument.PK.ToString());

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, relatedStorageMain.Documents.Count);

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_RelatedObject_DeleteDocumentsQuietly() => AssertAddLogForDocument_RelatedObject(Events.DocumentDeleted, (newDocument) => ((IDocumentManipulationSupport)PlugIn).DeleteDocumentsQuietly(new BusinessObject[] { newDocument }));

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_RelatedObject_DeleteDocumentsPermanently() => AssertAddLogForDocument_RelatedObject(Events.DocumentDeletedPermanently, (newDocument) => ((IDocumentManipulationSupport)PlugIn).DeleteDocumentsPermanently(new BusinessObject[] { newDocument }));

		void AssertAddLogForDocument_RelatedObject(Event logEvent, Action<StorageDocsBase> documentOperation)
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;

				var relatedBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
				relatedBizObj.OH_Code = "JERRYTEST";
				var relatedStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
				relatedStorageMain.SM_ParentFK = relatedBizObj.PK;
				relatedStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(relatedStorageMain, relatedBizObj);
				MasterFactory.Save();

				plugIn.CurrentParentMainOnGrid = relatedStorageMain;

				AssertEquals("TopLevelParentMain should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filePath }))
				{
					var args = new DragEventArgs(data, 0, 10, 60, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
					plugIn.OnParentFormDragDropDone(form, args);

					var logs = bizObj.GetLogs();
					var relatedLogs = relatedBizObj.GetLogs();
					var newDocument = relatedStorageMain.Documents[0];
					var referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", newDocument.SC_DocType, newDocument.PK.ToString());
					var referenceForLogEvent = DocumentLogReferenceHelper.GetReference(newDocument, logEvent);

					documentOperation(newDocument);
					MasterFactory.Save();

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 0 document", 0, relatedStorageMain.Documents.Count);

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));

					plugIn.OnParentFormDragDropDone(form, args);
					MasterFactory.Save();

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, relatedStorageMain.Documents.Count);

					newDocument = relatedStorageMain.Documents[0];
					referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", newDocument.SC_DocType, newDocument.PK.ToString());
					referenceForLogEvent = DocumentLogReferenceHelper.GetReference(newDocument, logEvent);

					documentOperation(newDocument);
					MasterFactory.Save();

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 0 document", 0, relatedStorageMain.Documents.Count);

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));
					AssertNotNull(relatedStorageMain.Logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
				}
			}
		}

		/*void AssertAddLogForDocument_RelatedObject1(Event logEvent, Action<StorageDocsBase> documentOperation)
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				plugIn.shouldShowEditFormForTest = true;

				var relatedBizObj = MasterFactory.New<OrgHeader>();
				relatedBizObj.OH_Code = "JERRYTEST";
				var relatedStorageMain = (StorageMain)MasterFactory.NewWithValidTestData<StorageMain>();
				relatedStorageMain.SM_ParentFK = relatedBizObj.PK;
				relatedStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(relatedStorageMain, relatedBizObj);
				MasterFactory.Save();

				plugIn.CurrentParentMainOnGrid = relatedStorageMain;

				AssertEquals("TopLevelParentMain should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filePath }))
				{
					var args = new DragEventArgs(data, 0, 10, 60, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
					plugIn.OnParentFormDragDrop(form, args);

					var logs = bizObj.GetLogs();
					var relatedLogs = relatedBizObj.GetLogs();
					var newDocument = relatedStorageMain.Documents[0];
					var referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", newDocument.SC_DocType, newDocument.PK.ToString());
					var referenceForLogEvent = DocumentLogReferenceHelper.GetReference(newDocument.SC_FileNameWithExtension, newDocument.SC_DocType, logEvent);

					documentOperation(newDocument);
					MasterFactory.Save();

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 0 document", 0, relatedStorageMain.Documents.Count);

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));

					plugIn.OnParentFormDragDrop(form, args);
					MasterFactory.Save();

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, relatedStorageMain.Documents.Count);

					newDocument = relatedStorageMain.Documents[0];
					referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", newDocument.SC_DocType, newDocument.PK.ToString());
					referenceForLogEvent = DocumentLogReferenceHelper.GetReference(newDocument.SC_FileNameWithExtension, newDocument.SC_DocType, logEvent);

					documentOperation(newDocument);
					MasterFactory.Save();

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 0 document", 0, relatedStorageMain.Documents.Count);

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));
					AssertNotNull(relatedStorageMain.Logs.MostRecentLogByEventTime(logEvent, referenceForLogEvent));
				}
			}
		}*/

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddLogForDocument_RelatedObject_RestoreDocuments()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;

				var relatedBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
				relatedBizObj.OH_Code = "JERRYTEST";
				var relatedStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
				relatedStorageMain.SM_ParentFK = relatedBizObj.PK;
				relatedStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(relatedStorageMain, relatedBizObj);
				MasterFactory.Save();

				plugIn.CurrentParentMainOnGrid = relatedStorageMain;

				AssertEquals("TopLevelParentMain should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filePath }))
				{
					var args = new DragEventArgs(data, 0, 10, 60, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
					plugIn.OnParentFormDragDropDone(form, args);
					MasterFactory.Save();

					var logs = bizObj.GetLogs();
					var relatedLogs = relatedBizObj.GetLogs();
					var newDocument = relatedStorageMain.Documents[0];
					var referenceForImported = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", newDocument.SC_DocType, newDocument.PK.ToString());
					var referenceForRestored = DocumentLogReferenceHelper.GetReference(newDocument, Events.DocumentRestored);
					var referenceForDeleted = DocumentLogReferenceHelper.GetReference(newDocument, Events.DocumentDeleted);

					AssertEquals("TopLevelParentMain Should have 0 document", 0, plugIn.TopLevelParentMain.Documents.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, relatedStorageMain.Documents.Count);

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));

					((IDocumentManipulationSupport)PlugIn).DeleteDocumentsQuietly(new BusinessObject[] { newDocument });

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 0 document", 0, relatedStorageMain.Documents.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 deleted document", 1, relatedStorageMain.eDocs.Count);

					((IDocumentManipulationSupport)PlugIn).RestoreDocuments(new BusinessObject[] { newDocument });

					AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, relatedStorageMain.Documents.Count);

					MasterFactory.Save();

					AssertNull(logs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNull(logs.MostRecentLogByEventTime(Events.DocumentDeleted, referenceForDeleted));
					AssertNull(logs.MostRecentLogByEventTime(Events.DocumentRestored, referenceForRestored));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(AutoEvents.DocumentImported, referenceForImported));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.DocumentDeleted, referenceForDeleted));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.DocumentRestored, referenceForRestored));
					AssertNotNull(relatedLogs.MostRecentLogByEventTime(Events.EditedARecord));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRegisterEditableForRelatedEDocs()
		{
			var bizObj = MasterFactory.NewWithValidTestData<eDocsUserControlTest.OrgHeader2>();
			bizObj.OH_Code = "JERRYTEST1";
			var main = MasterFactory.NewWithValidTestData<StorageMain>();
			main.SM_ParentFK = bizObj.PK;
			main.SM_Type = "ORG";

			var relatedBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
			relatedBizObj.OH_Code = "JERRYTEST2";
			var relatedMain = MasterFactory.NewWithValidTestData<StorageMain>();
			relatedMain.SM_ParentFK = relatedBizObj.PK;
			relatedMain.SM_Type = "ORG";

			MasterFactory.Save();

			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				var userControl = (eDocsUserControl)plugIn.UserControl;

				var parentMain = form.TopLevelParentMain;
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(parentMain, bizObj);
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(relatedMain, relatedBizObj);

				parentMain.RelatedParentMains.Add(relatedMain);

				userControl.RelatedParentsGrid.ListManager.Position = 1;

				AssertEquals("TopLevelParentMain DocumentOwner HasChanges is false", false, bizObj.HasChanges);
				AssertEquals("CurrentParentMainOnGrid HasChanges is false", false, relatedMain.HasChanges);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filePath }))
				{
					var args = new DragEventArgs(data, 0, 10, 60, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
					plugIn.OnParentFormDragDropDone(form, args);

					AssertEquals("TopLevelParentMain DocumentOwner HasChanges is true", true, bizObj.HasChanges);
					AssertEquals("CurrentParentMainOnGrid HasChanges is true", true, relatedMain.HasChanges);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractEmbeddedRtfImagesAndAddToEDocs()
		{
			AssertEquals("Precondition: 5 documents on the plugin.", 5, Parent.Documents.Count);

			using (MockEmbeddedRtfImageSource embeddedRtfImageSource = new MockEmbeddedRtfImageSource(
				new EmbeddedRtfImageFileInfo[]
				{
						new EmbeddedRtfImageFileInfo(GifFile, "img.1"),
						new EmbeddedRtfImageFileInfo(TifFile, "img.2")
				}))
			{
				PlugIn.testStorageDoc = Parent.Documents[0];
				PlugIn.testStorageDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				PlugIn.testStorageDoc.SC_Desc = "some test description";

				PlugIn.ExtractEmbeddedRtfImagesAndAddToEDocs(embeddedRtfImageSource);
				AssertEquals("Should be original 5 + 2 more documents on the plugin after drag and drop.", 7, Parent.Documents.Count);

				StorageDocs firstDocumentAdded = Parent.Documents[5];
				AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, firstDocumentAdded.SC_DocType);
				AssertEquals("some test description", firstDocumentAdded.SC_Desc);

				StorageDocs secondDocumentAdded = Parent.Documents[6];
				AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, secondDocumentAdded.SC_DocType);
				AssertEquals("some test description", secondDocumentAdded.SC_Desc);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInsertFromSingleImageDataReturnsOnlyOneEDoc()
		{
			using (MockDataObject imageData = new MockDataObject(new DataObject(DataFormats.FileDrop, new string[] { GifFile })))
			{
				Dictionary<string, StorageDocsBase> insertedEDocs = PlugIn.InsertFromData(imageData);
				AssertEquals("Only one document should be inserted", 1, insertedEDocs.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestApplyConfigToAllAttachingDocuments()
		{
			var filePath1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_200dpi.tif");
			var filePath2 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif");
			var filePath3 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi_400x200.tif");

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();

			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			using (ZDataObject dataObject = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filePath1, filePath2, filePath3 }))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;
				ZFormModaliser.ShowDialogsInTest = true;
				DataObjectPastedEventArgs args = new DataObjectPastedEventArgs(dataObject);

				string[] docTypes = new string[3] { "AIN", "ACV", "HBL" };//Prepares three document types for documents to be pasted
				int currentFileIndex = 0;

				try
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						var editForm = formOrDialog as EditPropertiesFileForm;
						var docs = (StorageDocs)editForm.BusinessEntity;
						docs.SC_DocType = docTypes[currentFileIndex];
						currentFileIndex++;
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					plugIn.OnParentFormDataObjectPastedDone(form, args);

					AssertEquals("All three files are pasted to eDocs", 3, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
					AssertEquals("First document set to first document type", docTypes[0], ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[0].DocType);
					AssertEquals("Second document set to second document type", docTypes[1], ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[1].DocType);
					AssertEquals("Third document set to third document type", docTypes[2], ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[2].DocType);

					currentFileIndex = 0;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						var editForm = formOrDialog as EditPropertiesFileForm;
						ZCheckBox checkBox = editForm.Controls.Find("ApplyToAllCheckBox", true)[0] as ZCheckBox;
						checkBox.Checked = true; //Tick the "Apply to All" checkbox

						var docs = (StorageDocsBase)editForm.BusinessEntity;
						docs.SC_DocType = docTypes[currentFileIndex];
						currentFileIndex++;
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					using (EDocsPluginAttachMonitor.StartMonitorEDocAttachment(plugIn))
					{
						plugIn.OnParentFormDataObjectPastedDone(form, args);
					}

					AssertEquals("All files are pasted to eDocs", 6, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
					AssertEquals("First document set to first document type", docTypes[0], ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[3].DocType);
					AssertEquals("Second document has same document type as first one", docTypes[0], ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[4].DocType);
					AssertEquals("Third document has same document type as first one", docTypes[0], ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[5].DocType);
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogsWhenApplyToAllTicked()
		{
			var filePath1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_200dpi.tif");
			var filePath2 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif");
			var filePath3 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi_400x200.tif");

			var shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();

			var docType = MasterFactory.NewWithValidTestData<RefDocType>();

			docType.RT_Desc = "XXX";
			docType.RT_DocType = "XXX";
			docType.RT_ReferenceType = "ALL";
			docType.RT_SE_NKDocumentReceivedEvent = Events.Arrival.Code;
			MasterFactory.Save();

			using (var form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;
				ZFormModaliser.ShowDialogsInTest = true;

				try
				{
					var currentFileIndex = 0;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						var editForm = formOrDialog as EditPropertiesFileForm;
						var checkBox = editForm.Controls.Find("ApplyToAllCheckBox", true)[0] as ZCheckBox;
						checkBox.Checked = true; //Tick the "Apply to All" checkbox

						var docs = (StorageDocsBase)editForm.BusinessEntity;
						docs.SC_DocType = docType.RT_DocType;
						currentFileIndex++;
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					plugIn.Add(new string[] { filePath1, filePath2, filePath3 });
					plugIn.TopLevelParentMain.Factory.Save();

					AssertEquals("All files are pasted to eDocs", 3, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);

					var logs = (plugIn.TopLevelParentMain.DocumentOwner).GetLogs();
					AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentImported, string.Concat("XXX|", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[0].UniqueKey.ToString())));
					AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentImported, string.Concat("XXX|", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[1].UniqueKey.ToString())));
					AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentImported, string.Concat("XXX|", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[2].UniqueKey.ToString())));
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogsWhenApplyToAllTickedAndOneFileHasNoExtension()
		{
			var file1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			var file2 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls");
			var file1WithoutExtension = Path.Combine(Temp.TempPath, Path.GetFileNameWithoutExtension(file1));
			File.Copy(file1, file1WithoutExtension);

			var shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			shipment.FillWithValidTestData();

			var docType = MasterFactory.NewWithValidTestData<RefDocType>();

			docType.RT_Desc = "XXX";
			docType.RT_DocType = "XXX";
			docType.RT_ReferenceType = "ALL";
			docType.RT_SE_NKDocumentReceivedEvent = Events.Arrival.Code;
			MasterFactory.Save();

			using (var form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;
				ZFormModaliser.ShowDialogsInTest = true;

				try
				{
					var currentFileIndex = 0;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						var editForm = formOrDialog as EditPropertiesFileForm;
						var checkBox = editForm.Controls.Find("ApplyToAllCheckBox", true)[0] as ZCheckBox;
						checkBox.Checked = true; //Tick the "Apply to All" checkbox

						currentFileIndex++;
						const string docDesc = "Doc Description";
						var docs = editForm.BusinessEntity as StorageFile;
						if (currentFileIndex == 2)
						{
							AssertEquals("For the second edit form, eDoc should apply the description of last input", docDesc, docs.SC_Desc);
							AssertEquals("For the second edit form, eDoc should apply the doc type of last selection", docType.RT_DocType, docs.SC_DocType);
						}

						docs.SC_DocType = docType.RT_DocType;
						docs.SC_Desc = docDesc;
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					_ = plugIn.Add([file1, file1WithoutExtension, file2]);
					plugIn.TopLevelParentMain.Factory.Save();

					AssertEquals("All files are pasted to eDocs", 3, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
					AssertEquals("Edit form should show twice because second file doesn't have extension", 2, currentFileIndex);
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
					File.Delete(file1WithoutExtension);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateDocumentImage()
		{
			var gifFile = GifFile;
			byte[] tifFileContents = DocumentUtilities.GetFileAsBytes(TifFile);
			byte[] gifFileContents = DocumentUtilities.GetFileAsBytes(gifFile);
			Document1.SC_ImageData = tifFileContents;
			Document2.SC_ImageData = tifFileContents;
			Document3.SC_ImageData = gifFileContents;

			DocumentChangedEventArgs testArgs = new DocumentChangedEventArgs(Events.EditedARecord, "Edited");

			PlugIn.UpdateDocumentImage(Document1.PK, gifFile, testArgs);

			AssertEquals("Byte data should be equal", Document1.SC_ImageData, Document3.SC_ImageData);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddUsingSystemGeneratedMSCDocument()
		{
			Parent.Documents.RemoveAndDeleteAll(); // start with no documents
			MasterFactory.Save();

			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			newDocument.SC_IsSystemGenerated = true;
			newDocument.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDocument.SC_Desc = "Testing";
			newDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(TifFile);

			SerializableEDocCollection dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { newDocument });
			((IDragDropSupport)PlugIn).Add(dataFromClipboard);
			AssertEquals("Parent has one document", 1, Parent.Documents.Count);

			StorageDocs addedDocument = Parent.Documents[0];
			AssertEquals("Added document's doc type is MSC", Core.Constants.RefDocTypes.MiscellaneousDocument, addedDocument.SC_DocType);
			AssertEquals("Added document's description is Testing", "Testing", addedDocument.SC_Desc);
			Assert("The new document created isn't system generated", !addedDocument.SC_IsSystemGenerated);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddUsingSystemGeneratedDocument()
		{
			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			newDocument.SC_IsSystemGenerated = true;
			newDocument.SC_DocType = "SDT";
			newDocument.SC_Desc = "My Own Custom Description";
			newDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(TifFile);

			SerializableEDocCollection dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { newDocument });

			AssertEquals("Parent starts out with 5 docs", 5, Parent.Documents.Count);
			((IDragDropSupport)PlugIn).Add(dataFromClipboard);
			AssertEquals("Parent ends up with 6 docs", 6, Parent.Documents.Count);

			// removing the previous 5, we should end up with the new one just dropped
			Parent.Documents.Remove(Document1);
			Parent.Documents.Remove(Document2);
			Parent.Documents.Remove(Document3);
			Parent.Documents.Remove(Document4);
			Parent.Documents.Remove(Document5);

			StorageDocs addedDocument = Parent.Documents[0];
			AssertEquals("Added Document's doc type is the same", "SDT", addedDocument.SC_DocType);
			AssertEquals("Added document's description comes from the Doc Type, NOT the description copied, because it's a system generated document", "Shipment Doc Type", addedDocument.SC_Desc);
			Assert("The new document created isn't a system generated document - you can't create a system generated document by drag and drop", !addedDocument.SC_IsSystemGenerated);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReloadLoadsDocsSavedByBatchProcessor()
		{
			AssertEquals("Precondition: 5 documents in collection", 5, Parent.Documents.Count);
			AssertEquals("ParentMain has ParentFk that equals OrgPK", Shipment.PK, Parent.SM_ParentFK);

			StorageDocs newDoc = Parent.Documents.AddNew();
			AssertEquals("Now 6 documents in collection", 6, Parent.Documents.Count);

			DocumentFactory separateFactory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageDocs separateDoc = (StorageDocs)((IDocumentFactory)separateFactory).CreateAndAllocateDocument(Shipment.PK, Core.Constants.DocManagerCodes.Shipment, BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\5pages.tif", Core.Constants.RefDocTypes.MiscellaneousDocument, "Misc Document");
			separateFactory.Save();

			PlugIn.Reload();

			AssertEquals("Now there should be 7 documents in collection", 7, Parent.Documents.Count);
			Assert("the collection contains the SeparateDoc", Parent.Documents.Contains(separateDoc.PK));
			Assert("the collection contains NewDoc", Parent.Documents.Contains(newDoc.PK));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReloadWithoutSave()
		{
			AssertEquals("Precondition: 5 documents in collection", 5, Parent.Documents.Count);
			AssertEquals("Precondition: ParentMain has ParentFk that equals Shipment.PK", Shipment.PK, Parent.SM_ParentFK);
			AssertEquals("Precondition: the edocs collection count", 5, PlugIn.TopLevelParentMain.eDocs.Count);
			Assert("Precondition: the edocs collection contains existing doc 1", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[0].PK));
			Assert("Precondition: the edocs collection contains existing doc 2", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[1].PK));
			Assert("Precondition: the edocs collection contains existing doc 3", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[2].PK));
			Assert("Precondition: the edocs collection contains existing doc 4", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[3].PK));
			Assert("Precondition: the edocs collection contains existing doc 5", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[4].PK));

			var newDoc = ((IDocumentFactory)Parent.MasterFactory).CreateAndAllocateDocument(Shipment.PK, Core.Constants.DocManagerCodes.Shipment, BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\5pages.tif", Core.Constants.RefDocTypes.MiscellaneousDocument, "Misc Document");
			AssertEquals("Precondition: newDoc is not saved in DB", false, newDoc.IsInDatabase);
			AssertEquals("Precondition: the edocs collection count", 1, PlugIn.TopLevelParentMain.eDocs.Count);
			Assert("Precondition: the edocs collection contains newDoc", PlugIn.TopLevelParentMain.eDocs.Contains(newDoc.PK));

			PlugIn.ReloadWithoutSave();

			AssertEquals("newDoc should not be saved on ReloadWithoutSave()", false, newDoc.IsInDatabase);
			AssertEquals("the edocs collection count", 6, PlugIn.TopLevelParentMain.eDocs.Count);
			Assert("the edocs collection contains newDoc", PlugIn.TopLevelParentMain.eDocs.Contains(newDoc.PK));
			Assert("the edocs collection contains existing doc 1", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[0].PK));
			Assert("the edocs collection contains existing doc 2", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[1].PK));
			Assert("the edocs collection contains existing doc 3", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[2].PK));
			Assert("the edocs collection contains existing doc 4", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[3].PK));
			Assert("the edocs collection contains existing doc 5", PlugIn.TopLevelParentMain.eDocs.Contains(Parent.Documents[4].PK));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddWithFilenames()
		{
			string file1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
			string file2 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			string file3 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls");

			string[] filenames = new string[] { file1, file2, file3 };

			AssertEquals("Precondition: count of eDocs on TopLevelParent on PlugIn", 5, PlugIn.TopLevelParentMainForTesting.eDocs.Count);

			PlugIn.Add(filenames);
			AssertEquals("Should now have 3 more eDocs on the TopLevelParent on PlugIn", 8, PlugIn.TopLevelParentMainForTesting.eDocs.Count);
			AssertEquals("Should have 1 more document in the documents collection (gif file only)", 6, PlugIn.TopLevelParentMainForTesting.Documents.Count);
			AssertEquals("Should have 2 files in the files collection (pdf and gif)", 2, PlugIn.TopLevelParentMainForTesting.Files.Count);

			StorageDocsBase eDoc1 = PlugIn.TopLevelParentMainForTesting.eDocs[5];
			AssertEquals("should have data type", "GIF", eDoc1.SC_DataType.ToUpper());
			AssertEquals("should have a filename", "small", eDoc1.SC_FileName);
			AssertEquals("Image data should not be empty", false, eDoc1.SC_ImageData.IsEmpty);

			StorageDocsBase eDoc2 = PlugIn.TopLevelParentMainForTesting.eDocs[6];
			AssertEquals("should have a data type", "PDF", eDoc2.SC_DataType.ToUpper());
			AssertEquals("should have a filename", "Sample", eDoc2.SC_FileName);
			AssertEquals("Image data should not be empty", false, eDoc2.SC_ImageData.IsEmpty);

			StorageDocsBase eDoc3 = PlugIn.TopLevelParentMainForTesting.eDocs[7];
			AssertEquals("should have a data type", "XLS", eDoc3.SC_DataType.ToUpper());
			AssertEquals("should have a filename", "Test", eDoc3.SC_FileName);
			AssertEquals("Image data should not be empty", false, eDoc3.SC_ImageData.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddWithFilenamesWhenSelectRelatedObject()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = form.GetTestPlugInInstance();

				var anotherBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
				anotherBizObj.OH_Code = "JERRYTEST";
				var anotherStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
				anotherStorageMain.SM_ParentFK = anotherBizObj.PK;
				anotherStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(anotherStorageMain, anotherBizObj);

				plugIn.CurrentParentMainOnGrid = anotherStorageMain;

				var file1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				var file2 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
				var file3 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls");

				var filenames = new string[] { file1, file2, file3 };

				CombineAssertions(() =>
				{
					AssertEquals("Precondition: count of eDocs on TopLevelParent on PlugIn", 5, PlugIn.TopLevelParentMainForTesting.eDocs.Count);
					plugIn.Add(filenames);
					AssertEquals("count of eDocs on TopLevelParent on PlugIn not change", 5, PlugIn.TopLevelParentMainForTesting.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should now have 3 eDocs", 3, anotherStorageMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, anotherStorageMain.Documents.Count);
					AssertEquals("CurrentParentMainOnGrid Should have 2 files", 2, anotherStorageMain.Files.Count);

					var eDoc1 = anotherStorageMain.eDocs[0];
					AssertEquals("should have data type", "GIF", eDoc1.SC_DataType.ToUpper());
					AssertEquals("should have a filename", "small", eDoc1.SC_FileName);
					AssertEquals("Image data should not be empty", false, eDoc1.SC_ImageData.IsEmpty);

					var eDoc2 = anotherStorageMain.eDocs[1];
					AssertEquals("should have a data type", "PDF", eDoc2.SC_DataType.ToUpper());
					AssertEquals("should have a filename", "Sample", eDoc2.SC_FileName);
					AssertEquals("Image data should not be empty", false, eDoc2.SC_ImageData.IsEmpty);

					var eDoc3 = anotherStorageMain.eDocs[2];
					AssertEquals("should have a data type", "XLS", eDoc3.SC_DataType.ToUpper());
					AssertEquals("should have a filename", "Test", eDoc3.SC_FileName);
					AssertEquals("Image data should not be empty", false, eDoc3.SC_ImageData.IsEmpty);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddWithDuplicate()
		{
			string filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");

			string[] filenames = new string[] { filePath };

			StorageFile existingFile = PlugIn.TopLevelParentMainForTesting.Files.AddNew();
			existingFile.SC_FileName = "small";
			existingFile.SC_DataType = "GIF";
			existingFile.SC_ImageData = DocumentUtilities.GetFileAsBytes(filePath);
			AssertEquals("Precondition: File count of TopLevelParent on PlugIn", 1, PlugIn.TopLevelParentMainForTesting.Files.Count);

			PlugIn.UseOverwrite = true;
			PlugIn.Add(filenames);

			AssertEquals("Should still have just one file, anything with the same name will prompt for overwrite instead of adding a new file", 1, PlugIn.TopLevelParentMainForTesting.Files.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogs()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();

			RefDocType docType = MasterFactory.NewWithValidTestData<RefDocType>();

			docType.RT_Desc = "XXX";
			docType.RT_DocType = "XXX";
			docType.RT_ReferenceType = "ALL";
			docType.RT_SE_NKDocumentReceivedEvent = Events.Arrival.Code;
			MasterFactory.Save();
			var xlsFile = XlsFile;

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			using (ZDataObject dataObject = ZDataObject.FromData(DataFormats.FileDrop, new string[] { xlsFile }))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;
				DataObjectPastedEventArgs args = new DataObjectPastedEventArgs(dataObject);

				try
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						StorageDocsBase docs = (StorageDocsBase)((ZForm)formOrDialog).BusinessEntity;
						docs.SC_Desc = "meh";
						docs.SC_DocType = "MSC";
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					plugIn.OnParentFormDataObjectPastedDone(form, args);

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						StorageDocsBase docs = (StorageDocsBase)((ZForm)formOrDialog).BusinessEntity;
						docs.SC_Desc = "beh";
						docs.SC_DocType = "XXX";
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					plugIn.OnParentFormDataObjectPastedDone(form, args);
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}

				AssertEquals(2, args.PastedFiles.Length);
				AssertEquals(2, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
				AssertEquals(xlsFile, args.PastedFiles[0].FileName);
				AssertEquals(xlsFile, args.PastedFiles[1].FileName);
				Assert(args.PastedFiles[0].AddedSuccessfully);
				Assert(args.PastedFiles[1].AddedSuccessfully);
				AssertEquals("meh", args.PastedFiles[0].FileCaption);
				AssertEquals("XXX", args.PastedFiles[1].FileCaption);
				AssertEquals("meh", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[0].Description);
				AssertEquals("XXX", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[1].Description);

				var logs = (plugIn.TopLevelParentMain.DocumentOwner).GetLogs();
				plugIn.TopLevelParentMain.Factory.Save();

				AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentImported, string.Concat("MSC|", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[0].UniqueKey.ToString())));
				AssertNotNull(logs.MostRecentLogByEventTime(Events.DocumentImported, string.Concat("XXX|", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[1].UniqueKey.ToString())));
				AssertNotNull(logs.MostRecentLogByEventTime(Events.Arrival, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[1].UniqueKey.ToString()));
			}
		}

		public void TestTopLevelParentMainWithDbError()
		{
			var mock = new Mock<DbBackendDocumentFactory>(new object[] { new BusinessObjectFactory() });
			mock.Setup(m => m.RetrieveExistingOrCreateStorageMainForPK(It.IsAny<ZGuid>(), It.IsAny<BusinessObject>(), It.IsAny<string>()))
				.Throws(new EDocsOffLineException("Exception"));
			var storageMain = PlugIn.ResetTopLevelParentMain(mock.Object);
			AssertEquals(
				@"There is an error with the Database Server. Please inform your System Administrator of this problem.

Error : Exception", PlugIn.NotDisplayedMessage);
			AssertEquals(true, PlugIn.hadExceptionGettingTopLevelParentMain);
			mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDispose()
		{
			using (eDocsPlugInForTesting plugIn = new eDocsPlugInForTesting(Factory.LoadTop1(typeof(OrgHeader), new ZQuery())))
			{
				StorageFile newFile = plugIn.TopLevelParentMainForTesting.Files.AddNew();
				newFile.SC_FileName = "one";
				newFile.SC_DataType = "TXT";
				newFile.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Subfolder\IgnoreThisFile.txt"));
				newFile.SaveToTempFile(); // sets the temp file string

				StorageFile newFile2 = plugIn.TopLevelParentMainForTesting.Files.AddNew();
				newFile2.SC_FileName = "two";
				newFile2.SC_DataType = "TXT";
				newFile2.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Subfolder\IgnoreThisFile.txt"));
				newFile2.SaveToTempFile(); // sets the temp file string

				string tempFilePath1 = newFile.TempFileName;
				string tempFilePath2 = newFile.TempFileName;

				plugIn.Dispose();

				AssertEquals("after dispose called, the files should not exist any more", false, File.Exists(tempFilePath1));
				AssertEquals("after dispose called, the files should not exist any more", false, File.Exists(tempFilePath2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestDisposeWithDeletedBusinessObject()
		{
			using (eDocsPlugInForTesting plugIn = new eDocsPlugInForTesting(Factory.LoadTop1(typeof(OrgHeader), new ZQuery())))
			{
				StorageFile newFile = plugIn.TopLevelParentMainForTesting.Files.AddNew();
				newFile.SC_FileName = "one";
				newFile.SC_DataType = "TXT";
				newFile.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Subfolder\IgnoreThisFile.txt"));
				newFile.SaveToTempFile(); // sets the temp file string

				StorageFile newFile2 = plugIn.TopLevelParentMainForTesting.Files.AddNew();
				newFile2.SC_FileName = "two";
				newFile2.SC_DataType = "TXT";
				newFile2.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Subfolder\IgnoreThisFile.txt"));
				newFile2.SaveToTempFile(); // sets the temp file string

				string tempFilePath1 = newFile.TempFileName;
				string tempFilePath2 = newFile.TempFileName;

				plugIn.TopLevelParentMainForTesting.Delete();
				plugIn.Dispose(); // this method should not access anything on the TopLevelParentMain object

				AssertEquals("after dispose called, the files should not exist any more", false, File.Exists(tempFilePath1));
				AssertEquals("after dispose called, the files should not exist any more", false, File.Exists(tempFilePath2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddWithOpenFile()
		{
			using (eDocsPlugInForTesting plugIn = new eDocsPlugInForTesting(Factory.LoadTop1<OrgHeader>(new ZQuery())))
			using (TempFile filename = TempFile.NewFromFile(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"))
			{
				AssertEquals("Precondition: no eDocs on plugin", 0, plugIn.TopLevelParentMainForTesting.eDocs.Count);
				string[] files = new string[] { filename.Filename };
				using (File.OpenWrite(filename.Filename))
				{
					plugIn.Add(files);
				}

				AssertEquals("no docs got added because file was open", 0, plugIn.TopLevelParentMainForTesting.eDocs.Count);

				plugIn.Add(files);
				AssertEquals("now the doc gets added because there is no filehandle open to it", 1, plugIn.TopLevelParentMainForTesting.eDocs.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileWithReload()
		{
			using (var plugIn = new eDocsPlugInForTesting(Factory.LoadTop1<OrgHeader>(new ZQuery())))
			using (var doc = TempFile.NewFromFile(TestingConstants.TIF_TwoBarcodes))
			using (var file = TempFile.NewFromFile(TestingConstants.PDF_Sample))
			{
				AssertEquals("Precondition: no eDocs on plugin", 0, plugIn.TopLevelParentMainForTesting.eDocs.Count);
				var files = new string[] { doc.Filename, file.Filename };

				plugIn.Add(files);
				AssertEquals(1, plugIn.TopLevelParentMainForTesting.Files.Count);
				AssertEquals(1, plugIn.TopLevelParentMainForTesting.Documents.Count);
				AssertEquals(2, plugIn.TopLevelParentMainForTesting.eDocs.Count);

				plugIn.ReloadWithoutSave();
				plugIn.Add(files);
				AssertEquals(2, plugIn.TopLevelParentMainForTesting.Files.Count);
				AssertEquals(2, plugIn.TopLevelParentMainForTesting.Documents.Count);
				AssertEquals("Total eDocs count should be 4", 4, plugIn.TopLevelParentMainForTesting.eDocs.Count);
			}
		}

		[ExpectNoExceptions()]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestOpeningADeclarationFromADifferentCountryWillNotThrowExceptions()
		{
			string existingCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany existingCompany = GlbCompany.CurrentCompany;
			var initialUserContext = Env.CurrentUserContext;
			try
			{
				GlbCompany demoCompany = MasterFactory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
				demoCompany.GC_RN_NKCountryCode = "NZ";
				MasterFactory.Save();
				Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, demoCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				AssertEquals("Precondition: current company should be the Demo Company", demoCompany.PK, GlbCompany.CurrentCompany.PK);
				AssertEquals("Precondition: now logged into an NZ company", "NZ", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
				shipment[JobShipmentSchema.JS_UniqueConsignRef] = "newship3";
				shipment.FillWithValidTestData();
				StorageMain shipmentParent = MasterFactory.RetrieveExistingOrCreateStorageMainForPK(shipment.PK, Core.Constants.DocManagerCodes.Shipment);
				shipmentParent.Documents.AddNew();

				BusinessObject declaration = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.FillWithValidTestData();
				declaration[JobDeclarationSchema.JE_DeclarationReference] = "B12345678";
				StorageMain declarationParent = MasterFactory.RetrieveExistingOrCreateStorageMainForPK(declaration.PK, "DEC");
				declarationParent.Documents.AddNew();
				declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

				MasterFactory.Save();

				// set a different country by relogging in as a different company- now loading the form it should load an NZ dec not an AU dec
				GlbCompany eDICompanyInAU = MasterFactory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "EDI"));
				Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, eDICompanyInAU.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				AssertEquals("Precondition: now logged into an AU company", "AU", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
				{
					form.Show();
					UserIdleWorker.Flush();
					form.ExposeAllTabPages();
					eDocsPlugIn plugIn = form.GetTestPlugInInstance();
					eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
					StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

					AssertEquals("Related eDocs", 1, userControl.RelatedParentsGrid.ListManager.Count);
					StorageMain relatedDoc1 = (StorageMain)userControl.RelatedParentsGrid.ListManager.List[0];
					AssertEquals("Only Shipment related object expected. NZ Dec should not show here", "SHP", relatedDoc1.SM_Type);
				}
			}
			finally
			{
				existingCompany.SetCountry(existingCountryCode);
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestIsInsertAllowed_BusinessObjectNotInDatabase()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "newship2";
			shipment.FillWithValidTestData();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = form.GetTestPlugInInstance();
				AssertEquals("Drag event should be allowed even if businessobject is not saved", true, plugIn.IsInsertAllowed);
			}

			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = form.GetTestPlugInInstance();
				AssertEquals("Drag event should be allowed if businessobject is saved", true, plugIn.IsInsertAllowed);
			}
		}

		public void TestIsInsertAllowed_BusinessObjectReadOnly()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "newship4";
			shipment.FillWithValidTestData();

			MasterFactory.Save();
			shipment.ReadOnly = true;

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = form.GetTestPlugInInstance();
				AssertEquals("Drag event should not be allowed if businessobject is readonly", false, plugIn.IsInsertAllowed);
			}

			shipment.ReadOnly = false;

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = form.GetTestPlugInInstance();
				AssertEquals("Drag event should be allowed if businessobject is NOT readonly", true, plugIn.IsInsertAllowed);
			}
		}

		[RequiresSTA]
		public void TestIsInsertAllowed_TopLevelBusinessObjectNotSupported()
		{
			BusinessObject notSupportedObject = MasterFactory.New<DummyBusinessObject>();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(notSupportedObject))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = form.GetTestPlugInInstance();
				AssertEquals("Drag event should not be allowed if businessobject does not support eDocs", false, plugIn.IsInsertAllowed);
			}
		}

		public void TestGetDragDropEffect()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "newship";
			shipment.FillWithValidTestData();

			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();

				DragEventArgs args = new DragEventArgs(new DataObject(), 0, 10, 10, DragDropEffects.Move | DragDropEffects.Copy, DragDropEffects.None);
				AssertEquals("DragDropEffect should return Copy over Move if it is part of the available options", DragDropEffects.Copy, plugIn.GetDragDropEffect(args));

				args = new DragEventArgs(new DataObject(), 0, 10, 10, DragDropEffects.Move, DragDropEffects.None);
				AssertEquals("DragDropEffect will return Move if copy is not available", DragDropEffects.Move, plugIn.GetDragDropEffect(args));
			}
		}

		OrgHeader Org;

		public void TestAllowPlugInDisplayWithNoLicenceRuturnTrue()
		{
			using (ZFormForPlugInTest formTest = new ZFormForPlugInTest(Org))
			{
				formTest.DisplayMode = ODisplayMode.Edit;
				formTest.Show();
				UserIdleWorker.Flush();

				AssertEquals("Initial state", 0, formTest.TabControl.SelectedIndex);

				var plugIn = formTest.GetTestPlugInInstance();

				AssertEquals("AllowPlugInDisplayWithNoLicence must be TRUE for eDocsPlugIn", true, PlugIn.AllowPlugInDisplayWithNoLicenceExposed);
			}
		}

		public void TestGetDragDropEffect_PlugInDoesNotAllowDrop()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();

			MasterFactory.Save();
			shipment.ReadOnly = true;

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();

				DragEventArgs args = new DragEventArgs(new DataObject(), 0, 10, 10, DragDropEffects.Move | DragDropEffects.Copy, DragDropEffects.None);
				AssertEquals("DragDropEffect should return None if the plug in does not allow drop (e.g. if the top level bizo is readonly)", DragDropEffects.None, plugIn.GetDragDropEffect(args));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnParentFormDataObjectPasted_EmbeddedRtfImageSource()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();
			MasterFactory.Save();

			var gifFile = GifFile;
			var tifFile = TifFile;
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			using (MockEmbeddedRtfImageSource embeddedRtfImageSource = new MockEmbeddedRtfImageSource(
				new EmbeddedRtfImageFileInfo[]
				{
						new EmbeddedRtfImageFileInfo(gifFile, "img.1"),
						new EmbeddedRtfImageFileInfo(tifFile, "img.2")
				}))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();

				plugIn.testStorageDoc = Parent.Documents[0];
				plugIn.testStorageDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				plugIn.testStorageDoc.SC_Desc = "some test description";

				DataObjectPastedEventArgs args = new DataObjectPastedEventArgs(embeddedRtfImageSource);
				plugIn.OnParentFormDataObjectPastedDone(form, args);

				AssertEquals(2, args.PastedFiles.Length);
				AssertEquals(2, ((IDocManagerSupport)shipment).DocManagerInfo.Documents.Count);
				AssertEquals(gifFile, args.PastedFiles[0].FileName);
				Assert(args.PastedFiles[0].AddedSuccessfully);
				AssertEquals("some test description", args.PastedFiles[0].FileCaption);
				AssertEquals("some test description", ((IDocManagerSupport)shipment).DocManagerInfo.Documents[0].Description);

				AssertEquals(tifFile, args.PastedFiles[1].FileName);
				Assert(args.PastedFiles[1].AddedSuccessfully);
				AssertEquals("some test description", args.PastedFiles[1].FileCaption);
				AssertEquals("some test description", ((IDocManagerSupport)shipment).DocManagerInfo.Documents[1].Description);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnParentFormDataObjectPasted_OtherDataObject_Cancelled()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();
			MasterFactory.Save();

			var xlsFile = XlsFile;
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			using (ZDataObject dataObject = ZDataObject.FromData(DataFormats.FileDrop, new string[] { xlsFile }))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				DataObjectPastedEventArgs args = new DataObjectPastedEventArgs(dataObject);

				try
				{
					plugIn.OnParentFormDataObjectPastedDone(form, args);
					AssertEquals(1, args.PastedFiles.Length);
					AssertEquals("Cancelled document should not be added", 0, ((IDocManagerSupport)shipment).DocManagerInfo.Documents.Count);
					AssertEquals(xlsFile, args.PastedFiles[0].FileName);
					Assert(!args.PastedFiles[0].AddedSuccessfully);
					AssertEquals("", args.PastedFiles[0].FileCaption);
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnParentFormDataObjectPasted_OtherDataObject()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();
			MasterFactory.Save();

			var xlsFile = XlsFile;
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			using (ZDataObject dataObject = ZDataObject.FromData(DataFormats.FileDrop, new string[] { xlsFile }))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;
				DataObjectPastedEventArgs args = new DataObjectPastedEventArgs(dataObject);

				try
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
					{
						StorageDocsBase docs = (StorageDocsBase)((ZForm)formOrDialog).BusinessEntity;
						docs.SC_Desc = "meh";
						docs.SC_DocType = "MSC";
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					plugIn.OnParentFormDataObjectPastedDone(form, args);
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}

				AssertEquals(1, args.PastedFiles.Length);
				AssertEquals(1, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
				AssertEquals(xlsFile, args.PastedFiles[0].FileName);
				Assert(args.PastedFiles[0].AddedSuccessfully);
				AssertEquals("meh", args.PastedFiles[0].FileCaption);
				AssertEquals("meh", ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[0].Description);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestAddSimilarFiles()
		{
			var tifFile = TifFile;
			var gifFile = GifFile;
			var initial = PlugIn.TopLevelParentMainForTesting.eDocs.Count;
			((IDragDropSupport)PlugIn).Add(new string[] { XlsFile, gifFile, tifFile, tifFile, gifFile });
			AssertEquals("Duplicates are discarded", initial + 3, PlugIn.TopLevelParentMainForTesting.eDocs.Count);
			Assert(UnitTestUserNotification.Instance.LastMessage.Contains("You cannot add files with similar filenames. Following files were added only once"));
			var secondline = UnitTestUserNotification.Instance.LastMessage.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[1];
			AssertEquals(tifFile + ", " + gifFile, secondline);
		}

		public void TestReloadPlugInCanRemoveTempFile()
		{
			var eDoc = Parent.eDocs[0];
			var tempFile = eDoc.SaveToTempFile();
			AssertEquals(true, File.Exists(tempFile));

			PlugIn.ReloadWithoutSave();
			AssertNullOrEmpty(eDoc.TempFileName);
			AssertEquals(false, File.Exists(tempFile));
		}

		public void TestReloadPlugInCanReloadShipamaxMessage()
		{
			var oldValue = MasterFactory.RefreshEnabled;
			MasterFactory.RefreshEnabled = false;
			var shipamaxMessage = MasterFactory.NewWithValidTestData<EDIMessage>();
			var eDoc = Parent.eDocs[0];
			shipamaxMessage.EM_LinkedObject = eDoc;
			shipamaxMessage.EM_Status = "QUE";
			MasterFactory.Save();

			AssertNotNull(eDoc.ActiveShipamaxMessage);
			AssertEquals("QUE", eDoc.ActiveShipamaxMessage.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var shipamaxMessage1 = otherFactory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.PK, shipamaxMessage.PK));
			shipamaxMessage1.EM_Status = "CAN";
			otherFactory.Save();

			PlugIn.ReloadWithoutSave();
			AssertEquals("CAN", eDoc.ActiveShipamaxMessage.EM_Status);
			MasterFactory.RefreshEnabled = oldValue;
		}

		public void TestReloadRelatedParentMains()
		{
			AssertEquals(1, Parent.RelatedParentMains.Count);

			Shipment["JS_UniqueConsignRef"] = "TestInvoice";

			var arInvoice = MasterFactory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
			arInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = Shipment["JS_UniqueConsignRef"];

			var storageMain = MasterFactory.New<StorageMain>();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.ReceivableInvoice;
			storageMain.SM_ParentFK = arInvoice.PK;
			storageMain.SM_DB = 1;
			storageMain.Documents.AddNew();

			MasterFactory.Save();

			Parent.RelatedParentMains.RemoveAll();
			AssertEquals(0, Parent.RelatedParentMains.Count);

			PlugIn.ReloadWithoutSave();
			AssertEquals(2, Parent.RelatedParentMains.Count);
		}

		public void TestReloadRelatedParentMainsHandlesMultipleApplications()
		{
			var factory = new BusinessObjectFactory();
			var shipment = (BusinessObject)factory.Load<Enterprise.Integration.Freight.ICommonShipment>(Shipment.PK);
			using (var plugin = new eDocsPlugInForTesting(shipment))
			{
				plugin.TopLevelParentMain.eDocs.Factory.RefreshEnabled = false;

				var arInvoice = MasterFactory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
				arInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = Shipment["JS_UniqueConsignRef"];

				var storageMain = MasterFactory.New<StorageMain>();
				storageMain.SM_Type = Core.Constants.DocManagerCodes.ReceivableInvoice;
				storageMain.SM_ParentFK = arInvoice.PK;
				storageMain.SM_DB = 1;
				storageMain.Documents.AddNew();
				MasterFactory.Save();

				plugin.ReloadWithoutSave();
				AssertEquals(2, plugin.TopLevelParentMain.RelatedParentMains.Count);
				AssertEquals(1, plugin.TopLevelParentMain.RelatedParentMains[1].eDocs.Count);

				storageMain.Documents.AddNew();
				MasterFactory.Save();

				plugin.ReloadWithoutSave();
				AssertEquals(2, plugin.TopLevelParentMain.RelatedParentMains[1].eDocs.Count);
			}
		}

		public void TestReloadRelatedParentMainsHandlesMultipleApplicationsWithEDocsInitiallyEmpty()
		{
			var factory = new BusinessObjectFactory();
			var shipment = (BusinessObject)factory.Load<Enterprise.Integration.Freight.ICommonShipment>(Shipment.PK);
			using (var plugin = new eDocsPlugInForTesting(shipment))
			{
				plugin.TopLevelParentMain.eDocs.Factory.RefreshEnabled = false;

				var arInvoice = MasterFactory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
				arInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = Shipment["JS_UniqueConsignRef"];

				var storageMain = MasterFactory.New<StorageMain>();
				storageMain.SM_Type = Core.Constants.DocManagerCodes.ReceivableInvoice;
				storageMain.SM_ParentFK = arInvoice.PK;
				storageMain.SM_DB = 1;
				// Leave the related parent main object with no eDocs
				MasterFactory.Save();

				plugin.ReloadWithoutSave();
				AssertEquals(1, plugin.TopLevelParentMain.RelatedParentMains.Count);

				storageMain.Documents.AddNew();
				MasterFactory.Save();

				plugin.ReloadWithoutSave();
				AssertEquals(1, plugin.TopLevelParentMain.RelatedParentMains[1].eDocs.Count);
			}
		}

		public void TestReloadRelatedParentMainsHandlesMultipleApplicationsWithDeletion()
		{
			var factory = new BusinessObjectFactory();
			var shipment = (BusinessObject)factory.Load<Enterprise.Integration.Freight.ICommonShipment>(Shipment.PK);
			using (var plugin = new eDocsPlugInForTesting(shipment))
			{
				plugin.TopLevelParentMain.eDocs.Factory.RefreshEnabled = false;

				var arInvoice = MasterFactory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
				arInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = Shipment["JS_UniqueConsignRef"];

				var storageMain = MasterFactory.New<StorageMain>();
				storageMain.SM_Type = Core.Constants.DocManagerCodes.ReceivableInvoice;
				storageMain.SM_ParentFK = arInvoice.PK;
				storageMain.SM_DB = 1;
				storageMain.Documents.AddNew();
				storageMain.Documents.AddNew();
				MasterFactory.Save();

				plugin.ReloadWithoutSave();
				AssertEquals(2, plugin.TopLevelParentMain.RelatedParentMains.Count);
				AssertEquals(2, plugin.TopLevelParentMain.RelatedParentMains[1].eDocs.Count);

				storageMain.Documents.RemoveAndDeleteAll();
				var lastDocument = storageMain.Documents.AddNew();
				MasterFactory.Save();

				plugin.ReloadWithoutSave();
				AssertEquals(1, plugin.TopLevelParentMain.RelatedParentMains[1].eDocs.Count);
				AssertEquals(lastDocument.PK, plugin.TopLevelParentMain.RelatedParentMains[1].eDocs[0].PK);
			}
		}

		public void TestRefreshButtonWorksForMultipleApplications()
		{
			var factoryNewInstance = new BusinessObjectFactory();
			var shipmentNewInstance = (BusinessObject)factoryNewInstance.Load<Enterprise.Integration.Freight.ICommonShipment>(Shipment.PK);
			factoryNewInstance.Save();

			AssertEquals("Precondition: 5 documents in Documents collection of primary instance", 5, Parent.Documents.Count);
			AssertEquals("Precondition: 5 documents in eDocs collection of primary instance", 5, PlugIn.TopLevelParentMain.eDocs.Count);

			using (var pluginNewInstance = new eDocsPlugInForTesting(shipmentNewInstance))
			{
				PlugIn.TopLevelParentMain.eDocs.Factory.RefreshEnabled = false;
				AssertEquals("Precondition: 5 documents in eDocs collection of second instance", 5, pluginNewInstance.TopLevelParentMain.eDocs.Count);
				AssertEquals("Precondition: 5 documents displayed", 5, pluginNewInstance.TopLevelParentMain.eDocsView.Count);
				Assert("Displayed 5 documents don't contain deleted documents", !pluginNewInstance.TopLevelParentMain.eDocsView.IncludeDeletedDocuments);

				var addedDoc = Parent.Documents.AddNew();
				addedDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;

				MasterFactory.Save();

				AssertEquals("Should contain 6 documents in eDocs collection of primary instance", 6, PlugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("Still 5 documents in eDocs collection of second instance", 5, pluginNewInstance.TopLevelParentMain.eDocs.Count);
				AssertEquals("Still 5 documents displayed in second instance", 5, pluginNewInstance.TopLevelParentMain.eDocs.Count);

				pluginNewInstance.Reload();

				AssertEquals("eDocs count should be updated to 6 in second instance", 6, pluginNewInstance.TopLevelParentMain.eDocs.Count);
				AssertEquals("6 documents should be displayed in second instance", 6, pluginNewInstance.TopLevelParentMain.eDocsView.Count);

				Parent.Documents.RemoveAndDelete(addedDoc);
				MasterFactory.Save();

				AssertEquals("Should contain 5 documents in eDocs collection of primary instance", 5, PlugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("Still 6 documents in second instance", 6, pluginNewInstance.TopLevelParentMain.eDocs.Count);
				AssertEquals("Still 6 documents displayed in second instance", 6, pluginNewInstance.TopLevelParentMain.eDocsView.Count);

				pluginNewInstance.Reload();

				AssertEquals("eDocs count should be updated to 5 in second instance", 5, pluginNewInstance.TopLevelParentMain.eDocs.Count);
				AssertEquals("5 documents should be displayed in second instance", 5, pluginNewInstance.TopLevelParentMain.eDocsView.Count);
			}
		}

		[GuiTest]
		public void TestDroppedStorageDocsDB()
		{
			var consignor = MasterFactory.NewWithValidTestData<OrgHeader>();
			MasterFactory.Save();

			var childFactory = MasterFactory.GetFactory(1);
			var dbName = childFactory.DBName;
			var deleteStorageDocTableScript = $@"IF OBJECT_ID('{dbName}..StorageDocs','U') IS NOT NULL
					DROP TABLE [{dbName}].[dbo].[StorageDocs]";
			var createDbObjectScript = new ScriptManager().DocManagerSchemaScript;

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				Db.Connection.ExecuteNonQuery(deleteStorageDocTableScript);
				Db.Connection.ExecuteNonQuery(createDbObjectScript);

				AssertEquals(0, (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM [{dbName}].[dbo].[StorageDocs]"));
			}

			try
			{
				using (var form = new ZFormForPlugInTest(consignor))
				{
					form.Show();

					var plugIn = form.GetTestPlugInInstance();
					var userControl = (GUI.eDocsUserControl)plugIn.UserControl;

					consignor.OH_Code = "CNSGNR";
					consignor.OH_FullName = "Consignor Org";
					consignor.OH_RL_NKClosestPort = "AUSYD";
					consignor.Addresses[0].OA_Address1 = "abcd";
					consignor.Addresses[0].OA_PostCode = "2000";

					var newDoc = consignor.DocManagerInfo().AddFileOrDocument(
						new byte[] { 116, 104, 105, 110, 103 },
						"Thing.txt",
						Core.Constants.RefDocTypes.MiscellaneousDocument,
						description: "Thing");

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					consignor.Validation.ValidateAll();
					Assert("Consignor should have no validation errors", !consignor.HasErrors);
					Assert("New document should have no validation errors", !((BusinessObject)newDoc).HasErrors);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					userControl.RefreshButton.PerformClick();
					Assert("Consignor change should be saved", !consignor.HasChanges);
					Assert("New document should be saved", ((BusinessObject)newDoc).IsInDatabase);
				}

				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					AssertEquals(1, (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM [{dbName}].[dbo].[StorageDocs]"));
				}

				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(deleteStorageDocTableScript);
				}

				var consignee = MasterFactory.NewWithValidTestData<OrgHeader>();
				var shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
				using (var form = new ZFormForPlugInTest(shipment))
				{
					shipment["ConsigneePK"] = consignee.PK;
					shipment["ConsignorPK"] = consignor.PK;

					AssertNoExceptionThrown(form.Show);

					var newDoc = consignor.DocManagerInfo().AddFileOrDocument(
						new byte[] { 116, 104, 105, 110, 103 },
						"Thing.txt",
						Core.Constants.RefDocTypes.MiscellaneousDocument,
						description: "Thing");

					form.FireSaveButton();
					AssertNoExceptionThrown(form.Close);
				}
			}
			finally
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(deleteStorageDocTableScript);
					Db.Connection.ExecuteNonQuery(createDbObjectScript);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileToRelatedStorageMain()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = form.GetTestPlugInInstance();
				plugIn.shouldShowEditFormForTest = true;

				var relatedBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
				relatedBizObj.OH_Code = "JERRYTEST";
				var relatedStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
				relatedStorageMain.SM_ParentFK = relatedBizObj.PK;
				relatedStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(relatedStorageMain, relatedBizObj);
				MasterFactory.Save();

				plugIn.CurrentParentMainOnGrid = relatedStorageMain;

				AssertEquals("TopLevelParentMain should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);

				byte[] xlsFileContents = DocumentUtilities.GetFileAsBytes(XlsFile);
				Document1.SC_ImageData = xlsFileContents;
				SerializableEDocCollection dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { Document1 });

				((IDragDropSupport)plugIn).Add(dataFromClipboard);
				AssertEquals("TopLevelParentMain Should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid Should have 1 document", 1, relatedStorageMain.Documents.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetStorageMainForAddingEDocsWillNotCreateLazyLoadingUserControl()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			var tifFile = TifFile;
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				var plugIn = form.GetTestPlugInInstance();
				AssertEquals(false, plugIn.IsUserControlExist);
				AssertEquals(false, plugIn.IsUserControlVisibleForTest);

				byte[] tifFileContents = DocumentUtilities.GetFileAsBytes(tifFile);
				Document1.SC_ImageData = tifFileContents;
				SerializableEDocCollection dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { Document1 });

				((IDragDropSupport)plugIn).Add(dataFromClipboard);
				AssertEquals(false, plugIn.IsUserControlExist);
				AssertEquals(false, plugIn.IsUserControlVisibleForTest);
			}
		}

		#region Implementation

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (fDocTypeWithForceUserToRead == null)
				{
					fDocTypeWithForceUserToRead = MasterFactory.New<RefDocType>();
					fDocTypeWithForceUserToRead.RT_ForceUserToRead = true;
					fDocTypeWithForceUserToRead.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
					fDocTypeWithForceUserToRead.RT_DocType = "FUR";
				}
				return fDocTypeWithForceUserToRead;
			}
		}

		RefDocType fDocTypeWithForceUserToRead;

		protected override void SetUp()
		{
			base.SetUp();
			DbHelper = new DocManagerDBHelperTestClass();
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DbHelper.GetDatabaseName(1) + ".dbo." + StorageDocsSchema.Constants.TableName);

			Shipment = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			Shipment.FillWithValidTestData();
			Factory.Save();

			PlugIn = new eDocsPlugInForTesting(Shipment);
			PlugIn.OnGUIShown();
			MasterFactory = PlugIn.MasterFactory;

			Parent = PlugIn.BusinessEntity as StorageMain;

			Document1 = Parent.Documents.AddNew();
			Document2 = Parent.Documents.AddNew();
			Document3 = Parent.Documents.AddNew();
			Document4 = Parent.Documents.AddNew();
			Document5 = Parent.Documents.AddNew();

			Document1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document4.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document5.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;

			Document1.SC_FileName = (NoResString)"document1";
			Document2.SC_FileName = (NoResString)"document2";
			Document3.SC_FileName = (NoResString)"document3";
			Document4.SC_FileName = (NoResString)"document4";
			Document5.SC_FileName = (NoResString)"document5";

			DocumentList = new BusinessObject[] { Document1, Document2, Document3, Document4, Document5 };

			ShipDocType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			ShipDocType.RT_Desc = "Shipment Doc Type";
			ShipDocType.RT_DocType = "SDT";
			ShipDocType.RT_ReferenceType = "SCL";

			Org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			MasterFactory.Save();
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DbHelper = new DocManagerDBHelperTestClass();
			if (!DbHelper.DatabaseExists(1))
			{
				DbHelper.CreateDatabase(1);
			}
		}

		protected override void TearDown()
		{
			PlugIn.Dispose();
			base.TearDown();
			foreach (var file in SerializableEDoc.fileNames)
			{
				if (File.Exists(file))
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception) // file in use, etc. don't bother trying to handle.
					{
					}
				}
			}
			SerializableEDoc.fileNames.Clear();
		}

		DocManagerDBHelperTestClass DbHelper;
		DocumentFactory MasterFactory;
		eDocsPlugInForTesting PlugIn;
		BusinessObject Shipment;
		RefDocType ShipDocType;

		string TifFile => BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.tif";
		string XlsFile => BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls";
		string GifFile => BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif";

		StorageMain Parent;
		StorageDocs Document1;
		StorageDocs Document2;
		StorageDocs Document3;
		StorageDocs Document4;
		StorageDocs Document5;
		BusinessObject[] DocumentList;

		#endregion

	}
}
