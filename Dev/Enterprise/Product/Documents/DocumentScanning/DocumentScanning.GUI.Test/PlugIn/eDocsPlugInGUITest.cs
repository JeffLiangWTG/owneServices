using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.DocumentScanning.Business.Constants;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class eDocsPlugInGUITest : TestCaseWithDocumentFactory
	{
		public void TestDragAndDropEDocsNotInEDocsTabWouldNotDeleteNewEDocsWhenResolvingConcurrencyException()
		{
			var orgFactory = new BusinessObjectFactory();
			var org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsConsignee = true;

			var org2 = orgFactory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsConsignor = true;

			org1.OH_Code = "~TEST1~";
			org2.OH_Code = "~TEST2~";
			orgFactory.Save();

			var ship1 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ship1["ConsigneePK"] = org1.PK;
			ship1["ConsignorPK"] = org2.PK;
			ship1["JS_TransportMode"] = "FSA";
			ship1["JS_ShipmentStatus"] = ShipmentStatusList.Codes.Confirmed;

			MasterFactory.RefreshEnabled = false;
			MasterFactory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;
			MasterFactory.Save();
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			factory.RefreshEnabled = false;
			factory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;
			var ship2 = (BusinessObject)factory.Load<Enterprise.Integration.Freight.ICommonShipment>(ship1.PK);
			using (var form1 = new ZFormForPlugInTest(ship1))
			using (var form2 = new ZFormForPlugInTest(ship2))
			{
				form1.Show();
				var plugIn1 = form1.GetTestPlugInInstance();
				var storage1 = (StorageMain)plugIn1.BusinessEntity;
				plugIn1.SynchroniseIfTabPageVisibleForTesting();

				form2.TabControl.TabPages.Add(new TabPage("TestPage"));
				form2.Show();
				var plugIn2 = form2.GetTestPlugInInstance();
				var storage2 = (StorageMain)plugIn2.BusinessEntity;
				plugIn2.SetupForTesting();
				plugIn2.SetupUserControlForTesting();
				plugIn2.SynchroniseIfTabPageVisibleForTesting();

				var doc1 = storage1.Documents.AddNew();
				doc1.SC_Date = ZDateTime.Now;
				doc1.SC_SM = storage1.PK;
				doc1.SC_Desc = "1";
				doc1.SC_DocType = "AGI";
				doc1.SC_FileName = "File name 1";
				AssertEquals("The ship1 should contains 1 eDocs.", 1, (ship1 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);

				var doc2 = storage2.Documents.AddNew();
				doc2.SC_Date = ZDateTime.Now;
				doc2.SC_SM = storage2.PK;
				doc2.SC_Desc = "2";
				doc2.SC_DocType = "COO";
				doc2.SC_FileName = "File name 2";
				AssertEquals("The ship2 should contains 1 eDocs.", 1, (ship2 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);

				form1.FireSaveButton();
				form2.FireSaveButton();

				AssertEquals("While you were working, the eDocs for this record were modified. The system will now need to merge this information. Press OK to have this information loaded and then try saving again.", UnitTestUserNotification.Instance.LastMessage.Text);

				//this is because transaction should have been rolled back, while test transaction prevents this
				DeleteProcessJobHeaderForTransaction(ship2);
				DeleteJobRequiredDocumentForTransaction(factory, ship2, storage1);

				form2.FireSaveButton();

				AssertEquals("The ship2 should contains 2 eDocs.", 2, (ship2 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "AGI", "COO" }, (ship2 as IDocManagerSupport).DocManagerInfo.AllEDocs.OfType<StorageDocsBase>().Select(doc => doc.SC_DocType));
			}
		}

		public void TestMessageShouldBeCorrectWhileSavingOpenFile()
		{
			var orgFactory = new BusinessObjectFactory();
			var org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsConsignee = true;
			org1.OH_Code = "~TEST1~";
			orgFactory.Save();
			var ship = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ship["ConsigneePK"] = org1.PK;
			ship["JS_TransportMode"] = "FSA";
			ship["JS_ShipmentStatus"] = ShipmentStatusList.Codes.Confirmed;

			var remoteFileMock = new Mock<IRemoteFile>();
			remoteFileMock.Setup(mock => mock.RemoteFilesSupported).Returns(true);
			remoteFileMock.Setup(mock => mock.GetIsOpenStatus()).Returns(true);
			remoteFileMock.Setup(mock => mock.Open()).Returns(true).Callback(() => remoteFileMock.Raise(m => m.FileChanged += null, EventArgs.Empty));
			remoteFileMock.Setup(mock => mock.GetDoesExistStatus()).Returns(true);
			ObjectFactory.Substitute(remoteFileMock.Object);
			var remoteChannelMock = new Mock<IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RemoteVersion).Returns(Version.Parse("99.99.0.0"));
			ObjectFactory.Substitute(remoteChannelMock.Object);
			var filePath = Path.Combine(Temp.TempPath, "TestFileForPostChange.txt");
			File.WriteAllText(filePath, "test content");

			using (var form = new ZFormForPlugInTest(ship))
			{
				try
				{
					form.Show();
					var main = form.TopLevelParentMain;
					var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
					var storageBase = plugIn.Add([filePath]).First().Value;
					((IRemoteFile)storageBase.OpenForEdit()).Open();
					Application.DoEvents();
					form.FireSaveButton();
					AssertNotContains("Trying to save an empty storage data.", ErrorReporter.LastMessageReported);
				}
				finally
				{
					File.Delete(filePath);
				}
			}
		}

		public void TestStorageMainShouldRewNewAfterHandlingUniqueIndexFailure()
		{
			var orgFactory = new BusinessObjectFactory();
			var org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsConsignee = true;

			var org2 = orgFactory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsConsignor = true;

			org1.OH_Code = "~TEST1~";
			org2.OH_Code = "~TEST2~";
			orgFactory.Save();

			var ship1 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ship1["ConsigneePK"] = org1.PK;
			ship1["ConsignorPK"] = org2.PK;
			ship1["JS_TransportMode"] = "FSA";
			ship1["JS_ShipmentStatus"] = ShipmentStatusList.Codes.Confirmed;

			MasterFactory.RefreshEnabled = false;
			MasterFactory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;
			MasterFactory.Save();
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			factory.RefreshEnabled = false;
			factory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;
			var ship2 = (BusinessObject)factory.Load<Enterprise.Integration.Freight.ICommonShipment>(ship1.PK);
			using (var form1 = new ZFormForPlugInTest(ship1))
			using (var form2 = new ZFormForPlugInTest(ship2))
			{
				form1.Show();
				var plugIn1 = (eDocsPlugInForTesting)form1.PlugIns.Instances.OfType<eDocsPlugIn>().First();
				var storage1 = (StorageMain)plugIn1.BusinessEntity;
				plugIn1.SynchroniseIfTabPageVisibleForTesting();

				form2.Show();
				var plugIn2 = (eDocsPlugInForTesting)form2.PlugIns.Instances.OfType<eDocsPlugIn>().First();
				var storage2 = (StorageMain)plugIn2.BusinessEntity;
				plugIn2.SynchroniseIfTabPageVisibleForTesting();

				var doc1 = storage1.Documents.AddNew();
				doc1.SC_Date = ZDateTime.Now;
				doc1.SC_SM = storage1.PK;
				doc1.SC_Desc = "1";
				doc1.SC_DocType = "AGI";
				doc1.SC_FileName = "File name 1";
				AssertEquals("The ship1 should contains 1 eDocs.", 1, (ship1 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);

				var doc2 = storage2.Documents.AddNew();
				doc2.SC_Date = ZDateTime.Now;
				doc2.SC_SM = storage2.PK;
				doc2.SC_Desc = "2";
				doc2.SC_DocType = "COO";
				doc2.SC_FileName = "File name 2";
				AssertEquals("The ship2 should contains 1 eDocs.", 1, (ship2 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);

				form1.FireSaveButton();
				form2.FireSaveButton();

				AssertEquals("While you were working, the eDocs for this record were modified. The system will now need to merge this information. Press OK to have this information loaded and then try saving again.", UnitTestUserNotification.Instance.LastMessage.Text);

				//this is because transaction should have been rolled back, while test transaction prevents this
				DeleteProcessJobHeaderForTransaction(ship2);
				DeleteJobRequiredDocumentForTransaction(factory, ship2, storage1);

				doc2.Delete();
				form2.FireSaveButton();

				AssertEquals("The ship2 should only contains 1 eDocs.", 1, (ship2 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);
				AssertEquals("AGI", (ship2 as IDocManagerSupport).DocManagerInfo.AllEDocs[0].DocType);
			}
		}

		public void TestDocumentOwnerDescriptionAfterHandlingUniqueIndexFailure()
		{
			var orgFactory = new BusinessObjectFactory();
			var org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsConsignee = true;

			var org2 = orgFactory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsConsignor = true;

			org1.OH_Code = "~TEST1~";
			org2.OH_Code = "~TEST2~";
			orgFactory.Save();

			var ship = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ship["ConsigneePK"] = org1.PK;
			ship["ConsignorPK"] = org2.PK;
			ship["JS_TransportMode"] = "FSA";
			ship["JS_ShipmentStatus"] = ShipmentStatusList.Codes.Confirmed;

			using (var form = new ZFormForPlugInTest(ship))
			{
				form.Show();
				var doc = form.TopLevelParentMain.Documents.AddNew();
				doc.SC_Date = ZDateTime.Now;
				doc.SC_SM = form.TopLevelParentMain.PK;
				doc.SC_DocType = "ACV";
				doc.SC_FileName = "File name 1";
				doc.SC_Desc = "1";

				var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var main = (StorageMain)documentFactory.NewWithValidTestData(typeof(StorageMain));
				main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				main.SM_ParentFK = ship.PK;
				main.SM_DB = 1;
				documentFactory.Save();

				form.FireSaveButton();

				AssertEquals("TopLevelParentMain DocumentOwnerDescription", "This Shipment", form.TopLevelParentMain.DocumentOwnerDescription);
			}
		}

		public void TestTwoFormsOpenMakeTwoStorageMainRecords()
		{
			var orgFactory = new BusinessObjectFactory();
			var org1 = orgFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsConsignee = true;

			var org2 = orgFactory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsConsignor = true;

			org1.OH_Code = "~TEST1~";
			org2.OH_Code = "~TEST2~";
			orgFactory.Save();

			var ship1 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ship1["ConsigneePK"] = org1.PK;
			ship1["ConsignorPK"] = org2.PK;
			ship1["JS_TransportMode"] = "FSA";
			ship1["JS_ShipmentStatus"] = ShipmentStatusList.Codes.Confirmed;
			var dummy = ((IHaveRequiredDocuments)ship1["DocsAndCartage"]).PK;

			MasterFactory.RefreshEnabled = false;
			MasterFactory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;
			MasterFactory.Save();
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			factory.RefreshEnabled = false;
			factory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;
			var ship2 = (BusinessObject)factory.Load<Enterprise.Integration.Freight.ICommonShipment>(ship1.PK);
			dummy = ((IHaveRequiredDocuments)ship2["DocsAndCartage"]).PK;
			using (var form1 = new ZFormForPlugInTest(ship1))
			using (var form2 = new ZFormForPlugInTest(ship2))
			{
				var page1 = new ZTabPage();
				form1.TabControl.TabPages.Add(page1);
				form1.Show();
				var plugIn1 = form1.GetTestPlugInInstance();
				var storage1 = (StorageMain)plugIn1.BusinessEntity;
				#if !WINZOR
				form1.TabControl.TabPages.Add(plugIn1.TabPage);
				#endif
				form1.TabControl.SelectedIndex = 1;
				plugIn1.SynchroniseIfTabPageVisibleForTesting();

				var page2 = new ZTabPage();

				form2.TabControl.TabPages.Add(page2);
				form2.Show();
				var plugIn2 = form2.GetTestPlugInInstance();
				var storage2 = (StorageMain)plugIn2.BusinessEntity;
				#if !WINZOR
				form2.TabControl.TabPages.Add(plugIn2.TabPage);
				#endif

				form2.TabControl.SelectedIndex = 1;
				plugIn2.SynchroniseIfTabPageVisibleForTesting();

				var doc1 = storage1.Documents.AddNew();
				doc1.SC_Date = ZDateTime.Now;
				doc1.SC_SM = storage1.PK;
				doc1.SC_Desc = "1";
				doc1.SC_DocType = "AGI";
				doc1.SC_FileName = "File name 1";

				AssertEquals("1 eDoc added to storage1", 1, storage1.eDocs.Count);

				var doc2 = storage2.Documents.AddNew();
				doc2.SC_Date = ZDateTime.Now;
				doc2.SC_SM = storage2.PK;
				doc2.SC_Desc = "2";
				doc2.SC_DocType = "COO";
				doc2.SC_FileName = "File name 2";
				AssertEquals("1 eDoc added to storage2", 1, storage2.eDocs.Count);

				form1.FireSaveButton();
				form2.FireSaveButton();

				//this is because transaction should have been rolled back, while test transaction prevents this
				DeleteProcessJobHeaderForTransaction(ship2);
				DeleteJobRequiredDocumentForTransaction(factory, ship2, storage1);

				Assert("StorageMain1 should be saved", storage1.IsInDatabase);
				Assert("StorageMain2 should not yet be saved", !storage2.IsInDatabase);
				Assert("StorageMain2 should be deleted", storage2.IsDeleted);
				AssertEquals("StorageMain1 should still have 1 eDocs", 1, storage1.eDocs.Count);

				form2.FireSaveButton();

				AssertEquals("StorageMain1 should have 2 eDocs", 2, storage1.eDocs.Count);
				AssertEquals("StorageMain2 should have 0 eDocs", 0, storage2.eDocs.Count);

				factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				factory.RefreshEnabled = false;
				Enterprise.Integration.Freight.IJobDocsAndCartage[] jobs = factory.Load<Enterprise.Integration.Freight.IJobDocsAndCartage>(new ZQuery(JobDocsAndCartageSchema.JP_ParentID, storage1.DocumentOwner.PK));
				AssertEquals("Should be 1 JobDocsAndCartage per parent", 1, jobs.Length);

				var documents = factory.Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, ((BusinessObject)jobs[0]).PK));
				AssertEquals("Should be 2 JobRequiredDocuments", 2, documents.Length);
				var types = new string[2] { documents[0].EQ_DocType, documents[1].EQ_DocType };
				AssertCollectionContains("AGI", types);
				AssertCollectionContains("COO", types);
			}
		}

		void DeleteJobRequiredDocumentForTransaction(DocumentFactory factory, BusinessObject bo, StorageMain sm)
		{
			Enterprise.Integration.Freight.IJobDocsAndCartage job = factory.LoadTop1<Enterprise.Integration.Freight.IJobDocsAndCartage>(new ZQuery(JobDocsAndCartageSchema.JP_ParentID, sm.DocumentOwner.PK));
			ZQuery query = new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, (job as BusinessObject).PK);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, "COO");
			JobRequiredDocument docToDelete = bo.Factory.LoadTop1<JobRequiredDocument>(query);
			docToDelete.ParentType = job.GetType();
			docToDelete.Delete();
			docToDelete.Factory.Save();
		}

		void DeleteProcessJobHeaderForTransaction(BusinessObject bo)
		{
			ZQuery query = new ZQuery(ProcessHeaderSchema.FH_ParentId, bo.PK);
			var processJobHeaderToDelete = new BusinessObjectFactory().LoadTop1<IProcessJobHeader>(query);
			if (processJobHeaderToDelete != null)
			{
				processJobHeaderToDelete.Delete();
				processJobHeaderToDelete.Factory.Save();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestOnParentFormDataObjectPasted_SupportedDropType()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				TabPage page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				form.TabControl.SelectedIndex = 1;
				var plugIn = form.GetTestPlugInInstance();
				StorageMain storage = (StorageMain)plugIn.BusinessEntity;

				string testFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestTextFile.txt");
				DataObject file = new DataObject(DataFormats.FileDrop, new string[] { testFile });
				plugIn.OnParentFormDataObjectPastedDone(form, new DataObjectPastedEventArgs(file));
				AssertEquals("Drop should have been sucessful", 1, storage.eDocs.Count);

				using (ZDataObject dataObject = ZDataObject.FromData(file))
				{
					plugIn.OnParentFormDataObjectPastedDone(form, new DataObjectPastedEventArgs(dataObject));
					AssertEquals("Drop should have been sucessful", 2, storage.eDocs.Count);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestPasteJpegFile()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				TabPage page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				form.TabControl.SelectedIndex = 1;
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];
				StorageMain storage = (StorageMain)plugIn.BusinessEntity;

				string testFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.JPG");
				int length = File.ReadAllBytes(testFile).Length;

				DataObject file = new DataObject(DataFormats.FileDrop, new string[] { testFile });
				plugIn.OnParentFormDataObjectPastedDone(form, new DataObjectPastedEventArgs(file));
				AssertEquals("Drop should have been sucessful", 1, storage.eDocs.Count);
				AssertEquals("File should be jpeg still not changing its size", length, storage.eDocs[0].SC_ImageData.Length);
			}
		}

		[RequiresSTA]
		public void TestOnParentFormDataObjectPasted_UnsupportedDropType()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				TabPage page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				form.TabControl.SelectedIndex = 1;
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];
				StorageMain storage = (StorageMain)plugIn.BusinessEntity;

				DataObject file = new DataObject(DataFormats.StringFormat, "blah blah blah");
				plugIn.OnParentFormDataObjectPastedDone(form, new DataObjectPastedEventArgs(file));
				AssertEquals("Drop should have been sucessful", 0, storage.eDocs.Count);

				using (ZDataObjectThatDoesNotSupportEdocs dataObject2 = new ZDataObjectThatDoesNotSupportEdocs())
				{
					DragEventArgs e = new DragEventArgs(dataObject2, 0, 0, 0, DragDropEffects.All, DragDropEffects.All);
					Assert("ZDataObjectThatDoesNotSupportEdocs did not support edocs? What?", !plugIn.IsDropDataSupported(e));
				}
			}
		}

		class ZDataObjectThatDoesNotSupportEdocs : ZDataObject
		{
			public ZDataObjectThatDoesNotSupportEdocs()
				: base()
			{
			}

			public override bool SupportsEDocs
			{
				get { return false; }
			}
		}

		public void TestShowingPlugInFromADatabaseWithLotsOfDocumentRecords()
		{
			// Run code once to ensure all methods have been JITed
			TestUtils.CreateLotsOfDocumentRecords(true);
			OrgHeader org = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery());

			StorageMain main = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;
			main.SM_DB = 1;

			for (int i = 0; i < 10; i++)
			{
				main.Documents.AddNew();
			}

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.FireSaveButton();
			}

			// start actual test
			var timer = new Stopwatch();
			timer.Start();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				Assert("User Control is visible on the tab page", userControl.Visible);
			}
			var showTimeElapsed = timer.Elapsed;

			Assert("Test load and open timing: " + showTimeElapsed, showTimeElapsed <= TimeSpan.FromSeconds(5));
		}

		public void TestShowingPluginOnAFormWithoutExistingStorageMain()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				UserIdleWorker.Flush();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				AssertEquals("Plug in user control should be visible, using existing BizO loaded from factory", true, userControl.Visible);
				AssertNotNull("eDocs Grid on the UserControl is bound", userControl.StorageDocsGrid.ListManager);
				AssertNotNull("Parent Grid on the UserControl is bound", userControl.RelatedParentsGrid.ListManager);
			}
		}

		public void TestShowingPluginOnAFormWithExistingStorageMain()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			Assert("Precondition: Odyssey_SD001 is available", DbHelper.DatabaseExists(1));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_DB = 1;
			main.SM_ParentFK = org.PK;
			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				UserIdleWorker.Flush();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				Assert("User Control should be visible on the tab page", userControl.Visible);
				AssertNotNull("Grid on the UserControl should be bound", userControl.StorageDocsGrid.ListManager);
				AssertNotNull("Parent grid on the user control is bound", userControl.RelatedParentsGrid.ListManager);
			}
		}

		[RequiresSTA]
		public void TestShowingPluginOnAFormWithNewBizO()
		{
			OrgHeader org = MasterFactory.New<OrgHeader>();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				TabPage page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				UserIdleWorker.Flush();
				form.TabControl.SelectedIndex = 1;
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				AssertEquals("User control show now be visible becase BizO is in the database", true, userControl.Visible);
				AssertEquals("Covering label should not be visible", false, ((IPlugInInternals)plugIn).CoveringLabel.Visible);
				AssertNotNull("Grid on the UserControl should be bound", userControl.StorageDocsGrid.ListManager);
				AssertNotNull("related parent grid on the user control should be bound", userControl.RelatedParentsGrid.ListManager);
			}
		}

		[RequiresSTA]
		public void TestShowingPluginOnAFormWithReadOnlyBizO()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			org.ReadOnly = true;

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				TabPage page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				UserIdleWorker.Flush();
				form.TabControl.SelectedIndex = 1;
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				Assert("User Control should be visible on the tab page", userControl.Visible);
				AssertNotNull("Grid on the UserControl should be bound", userControl.StorageDocsGrid.ListManager);
				AssertNotNull("Parent grid on the user control is bound", userControl.RelatedParentsGrid.ListManager);
				Assert("UserControl should be read only because bizO was readonly", userControl.ReadOnly);
				Assert("TopLevelParentMain should be readonly because BizO was Readonly", ((BusinessObject)plugIn.BusinessEntity).ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestRequiredDocumentsReadOnlyStates()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				shipment.ReadOnly = true;

				TabPage page = new TabPage("Dummy");
				form.TabControl.TabPages.Add(page);

				form.Show();
				UserIdleWorker.Flush();
				form.TabControl.SelectedIndex = 1;

				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				JobRequiredDocumentDependentCollection requiredDocuments = ((StorageMain)plugIn.BusinessEntity).GetRequiredDocuments();
				AssertEquals("RequiredDocuments.ReadOnly", true, requiredDocuments.ReadOnly);

				shipment.ReadOnly = false;
				form.TabControl.SelectedIndex = 2;
				form.TabControl.SelectedIndex = 1;
				AssertEquals("RequiredDocuments.ReadOnly", false, requiredDocuments.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestShowingPluginOnAFormWithExistingStorageMainButWrongDB()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			Assert("Precondition: Odyssey_SD002 is not available", !DbHelper.DatabaseExists(2));
			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_DB = 2;
			main.SM_ParentFK = org.PK;
			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				TabPage page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				UserIdleWorker.Flush();
				form.TabControl.SelectedIndex = 1;
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				AssertEquals("Covering label should be visible because the database is not available", true, ((IPlugInInternals)plugIn).CoveringLabel.Visible);
				AssertNull("Grid on the UserControl should not be bound", userControl.StorageDocsGrid.ListManager);
				AssertNull("Related Parent Grid on the UserControl should not be bound", userControl.StorageDocsGrid.ListManager);

				main.SM_DB = 1;
				MasterFactory.Save();

				form.TabControl.SelectedIndex = 0;
				form.TabControl.SelectedIndex = 1;
				AssertEquals("Covering label not should be visible because the database available", false, ((IPlugInInternals)plugIn).CoveringLabel.Visible);
				AssertEquals("User Control should be visible because db 1 is available", true, userControl.Visible);
				AssertNotNull("Grid on the UserControl should be bound", userControl.StorageDocsGrid.ListManager);
				AssertNotNull("Related Parent Grid on the UserControl should be bound", userControl.StorageDocsGrid.ListManager);
			}
		}

		[RequiresSTA]
		public void TestIncorrectlyLoadedeDocs_TestDB()
		{
			ZArchitecture.Core.Testing.LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			Assert("Pre-condition: Should be Test system", !Env.Instance.IsProductionSystem);

			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			Assert("Precondition: Odyssey_SD002 is not available", !DbHelper.DatabaseExists(2));
			var main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_DB = 2;
			main.SM_ParentFK = org.PK;
			MasterFactory.Save();

			using (var form = new ZFormForPlugInTest(org))
			{
				var page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				UserIdleWorker.Flush();
				form.TabControl.SelectedIndex = 1;
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];

				Assert(!plugIn.ShouldPlugInGUIAndBusinessEntityBeCreatedCoreForTest());
				AssertContains("The eDocs tab is not currently available due to an error retrieving data.\r\nPlease inform your System Administrator and have them ensure the eDoc Databases have been correctly restored.",
					plugIn.NotDisplayedMessage);
				AssertContains("Error Message: Invalid object name", plugIn.NotDisplayedMessage);
				AssertContains("Please make sure that the eDoc databases have been restored correctly.", plugIn.NotDisplayedMessage);
			}
		}

		[RequiresSTA]
		public void TestIncorrectlyLoadedeDocs_ProdDB()
		{
			ZArchitecture.Core.Testing.LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			Assert("Pre-condition: Should be Test system", Env.Instance.IsProductionSystem);

			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			Assert("Precondition: Odyssey_SD002 is not available", !DbHelper.DatabaseExists(2));
			var main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_DB = 2;
			main.SM_ParentFK = org.PK;
			MasterFactory.Save();

			using (var form = new ZFormForPlugInTest(org))
			{
				var page = new TabPage("another tab");
				form.TabControl.TabPages.Add(page);
				form.Show();
				UserIdleWorker.Flush();
				form.TabControl.SelectedIndex = 1;
				var plugIn = form.GetTestPlugInInstance();

				Assert(!plugIn.ShouldPlugInGUIAndBusinessEntityBeCreatedCoreForTest());
				AssertContains("The eDocs tab is not currently available due to an error retrieving data.\r\nPlease inform your System Administrator and have them ensure the eDoc Databases have been correctly restored.",
					plugIn.NotDisplayedMessage);
				AssertContains("Error Message: Invalid object name", plugIn.NotDisplayedMessage);
				AssertNotContains("Please make sure that the eDoc databases have been restored correctly.", plugIn.NotDisplayedMessage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPressingRefreshNoChangesWontSaveBeforeReloading()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);

			OrgHeader org = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery());
			StorageMain existingMain = (StorageMain)MasterFactory.LoadTop1(typeof(StorageMain), new ZQuery(StorageMainSchema.SM_ParentFK, org.PK));

			Assert("Precondition: database " + DbHelper.GetDatabaseName(1) + " exists", DbHelper.DatabaseExists(1));
			AssertNull("Precondition: StorageMain for this org does not exist", existingMain);

			StorageMain main = (StorageMain)MasterFactory.New(typeof(StorageMain));
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;
			main.SM_DB = 1;
			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				UserIdleWorker.Flush();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(org.PK, Core.Constants.DocManagerCodes.Organisation, BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif", Core.Constants.RefDocTypes.MiscellaneousDocument, "Test");
				MasterFactory.Save(); // THis is a differnt factory than the one behind the plugin

				StorageMain parentMain = (StorageMain)plugIn.BusinessEntity;
				AssertEquals("ParentMain object does not have changes", false, parentMain.HasChanges);
				AssertEquals("Org does not have changes", false, parentMain.HasChanges);
				userControl.RefreshButton.PerformClick(); // no prompt should come up
				AssertEquals("Refresh happened - parentmain should have one document now ", 1, parentMain.Documents.Count);
			}
		}

		public void TestPressingRefreshWillSaveBeforeReloading()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);

			OrgHeader org = MasterFactory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.Addresses[0].OA_Address1 = "abcd";
			org.Addresses[0].OA_PostCode = "2000";
			MasterFactory.Save();

			StorageMain existingMain = (StorageMain)MasterFactory.LoadTop1(typeof(StorageMain), new ZQuery(StorageMainSchema.SM_ParentFK, org.PK));

			Assert("Precondition: database " + DbHelper.GetDatabaseName(1) + " exists", DbHelper.DatabaseExists(1));
			AssertNull("Precondition: StorageMain for this org does not exist", existingMain);

			StorageMain main = (StorageMain)MasterFactory.New(typeof(StorageMain));
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;
			main.SM_DB = 1;
			MasterFactory.Save();

			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				UserIdleWorker.Flush();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				StorageMain parentMain = (StorageMain)plugIn.BusinessEntity;
				StorageDocs document = parentMain.Documents.AddNew();
				document.SC_IsDeleted = true;

				Assert("ParentMain object has changes", parentMain.HasChanges);
				userControl.RefreshButton.PerformClick();
				Assert("Save happend on click of the refresh button. ParentMain doesn't have changes anymore", !parentMain.HasChanges);
			}
		}

		public void TestPressingRefreshWillNotSaveIfThereIsError()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);

			var org = MasterFactory.NewWithValidTestData<OrgHeader>();
			var existingMain = MasterFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, org.PK));

			Assert("Precondition: database " + DbHelper.GetDatabaseName(1) + " exists", DbHelper.DatabaseExists(1));
			AssertNull("Precondition: StorageMain for this org does not exist", existingMain);

			var main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;
			main.SM_DB = 1;
			MasterFactory.Save();

			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);

			using (var form = new ZFormForPlugInTest(org))
			{
				form.Show();
				UserIdleWorker.Flush();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				StorageMain parentMain = (StorageMain)plugIn.BusinessEntity;
				StorageDocs document = parentMain.Documents.AddNew();
				document.SC_IsDeleted = true;

				var reqDoc1 = org.RequiredDocuments.AddNew();
				reqDoc1.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
				reqDoc1.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
				reqDoc1.EQ_DocUsage = "BRK";
				reqDoc1.EQ_RN_NKRelatedCountry = "AU";

				var reqDoc2 = org.RequiredDocuments.AddNew();
				reqDoc2.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
				reqDoc2.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
				reqDoc2.EQ_DocUsage = "BRK";
				reqDoc2.EQ_RN_NKRelatedCountry = "AU";

				Assert("ParentMain object has changes", parentMain.HasChanges);
				userControl.RefreshButton.PerformClick();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("Should not try to save documents when there is validataion error.", "Saving_Duplicate_Doc_Type", ErrorReporter.LastKeyReported);

				org.FillWithValidTestData();
				org.OH_FullName = "Test Org";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.Addresses[0].OA_Address1 = "abcd";
				org.Addresses[0].OA_PostCode = "2000";
				org.Addresses[0].OA_Language = Core.SharedConstants.Languages.English;
				org.Addresses[0].OA_State = "NSW";
				reqDoc1.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				reqDoc1.EQ_DocDescription = "AAA";
				reqDoc2.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				reqDoc2.EQ_DocDescription = "BBB";
				org.RunPreSaveValidation();
				AssertEquals("Business object should have no validation error", false, org.HasErrors);
				AssertEquals("Business object should have no validation error", false, reqDoc1.HasErrors);
				AssertEquals("Business object should have no validation error", false, reqDoc2.HasErrors);

				form.ForceSaveToFail = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.RefreshButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Document shouldn't be saved because main form fails to save", false, reqDoc1.IsInDatabase);
					AssertEquals("Document shouldn't be saved because main form fails to save", false, reqDoc2.IsInDatabase);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPressingRefresh_NewObject()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Testing 123";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "abcd";
			org.MainAddress.OA_PostCode = "2127";

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				UserIdleWorker.Flush();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				((IDocumentFactory)plugIn.MasterFactory).CreateAndAllocateDocument(org.PK, Core.Constants.DocManagerCodes.Organisation, BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif", Core.Constants.RefDocTypes.MiscellaneousDocument, "Test");

				userControl.RefreshButton.PerformClick();
				AssertEquals("This form needs to be saved before it can be reloaded. Save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				StorageMain parentMain = (StorageMain)plugIn.BusinessEntity;
				AssertEquals(false, parentMain.IsInDatabase);
				AssertEquals(false, org.IsInDatabase);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.RefreshButton.PerformClick();
				AssertEquals("This form needs to be saved before it can be reloaded. Save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				parentMain = (StorageMain)plugIn.BusinessEntity;
				AssertEquals(true, parentMain.IsInDatabase);
				AssertEquals(true, org.IsInDatabase);
				Assert("Save happend on click of the refresh button. ParentMain doesn't have changes anymore", !parentMain.HasChanges);
			}
		}

		public void TestChangingSelectedParentInGridUpdatesCurrentParentMain()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();

			OrgHeader consignee = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			shipment["ConsigneePK"] = consignee.PK;
			OrgHeader consignor = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			shipment["ConsignorPK"] = consignor.PK;

			Assert("Precondition: database " + DbHelper.GetDatabaseName(1) + " exists", DbHelper.DatabaseExists(1));

			StorageMain main = (StorageMain)MasterFactory.New(typeof(StorageMain));
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main.SM_ParentFK = shipment.PK;
			main.SM_DB = 1;
			main.Documents.AddNew();

			StorageMain consigneeMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
			consigneeMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consigneeMain.SM_ParentFK = ((OrgHeader)shipment["Consignee"]).PK;
			consigneeMain.SM_DB = 1;
			consigneeMain.Documents.AddNew();

			StorageMain consignorMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
			consignorMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consignorMain.SM_ParentFK = ((BusinessObject)shipment["Consignor"]).PK;
			consignorMain.SM_DB = 1;
			consignorMain.Documents.AddNew();
			MasterFactory.Save();

			AssertEquals("Precondition: 3 records in StorageMain table", 3, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				UserIdleWorker.Flush();
				
				var plugIn = form.GetTestPlugInInstance();
				AssertNotNull("Precondition: Tab control should exist", form.TabControl);
				Assert("Precondition: Tab page exists", form.TabControl.TabPages.Count > 0);
				Assert("Precondition: eDocs user control exists", form.TabControl.TabPages[0].Controls.Count > 0);
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				AssertNotNull("Related parents grid should be bound", userControl.RelatedParentsGrid.ListManager);
				AssertEquals("Related parents Grid should have 3 elements", 3, userControl.RelatedParentsGrid.ListManager.Count);
				AssertEquals("PlugIn's CurrentStorageMainInGrid should be the top level parent", plugIn.CurrentParentMainOnGrid, userControl.RelatedParentsGrid.ListManager.GetCurrent());

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignee comes first alphabetically)", consigneeMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);

				userControl.RelatedParentsGrid.ListManager.Position = 2;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignor comes last alphabetically)", consignorMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);

				plugIn.TopLevelParentMain.Delete();
				Assert("deleted TopLevelParentMain is regenerated", !plugIn.TopLevelParentMain.IsDeleted);

				AssertEquals("Related parents Grid should have 2 elements", 2, userControl.RelatedParentsGrid.ListManager.Count);

				userControl.RelatedParentsGrid.ListManager.Position = 0;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignee comes first alphabetically)", consigneeMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignor comes last alphabetically)", consignorMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);
			}
		}
		public void TestChangingSelectedParentWillEnableOrDisableMenuOptions()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();

			OrgHeader consignee = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			shipment["ConsigneePK"] = consignee.PK;
			OrgHeader consignor = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			shipment["ConsignorPK"] = consignor.PK;

			Assert("Precondition: database " + DbHelper.GetDatabaseName(1) + " exists", DbHelper.DatabaseExists(1));

			StorageMain main = (StorageMain)MasterFactory.New(typeof(StorageMain));
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main.SM_ParentFK = shipment.PK;
			main.SM_DB = 1;
			main.Documents.AddNew();

			StorageMain consigneeMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
			consigneeMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consigneeMain.SM_ParentFK = ((OrgHeader)shipment["Consignee"]).PK;
			consigneeMain.SM_DB = 1;
			consigneeMain.Documents.AddNew();

			StorageMain consignorMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
			consignorMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consignorMain.SM_ParentFK = ((OrgHeader)shipment["Consignor"]).PK;
			consignorMain.SM_DB = 1;
			consignorMain.Documents.AddNew();
			MasterFactory.Save();

			AssertEquals("Precondition: 3 records in StorageMain table", 3, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = form.GetTestPlugInInstance();
				AssertNotNull("Precondition: Tab control should exist", form.TabControl);
				Assert("Precondition: Tab page exists", form.TabControl.TabPages.Count > 0);
				Assert("Precondition: eDocs user control exists", form.TabControl.TabPages[0].Controls.Count > 0);
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				AssertEquals("Precondition: UserControl's list manager has 3 elements", 3, userControl.RelatedParentsGrid.ListManager.List.Count);

				DocumentsZGrid grid = userControl.StorageDocsGrid;
				grid.LastClickPoint = new System.Drawing.Point(10, 30);
				grid.UpdateContextMenuElements((StorageDocs)grid.ListManager.GetCurrent());

				AssertNotNull("Related parents grid should be bound", userControl.RelatedParentsGrid.ListManager);
				AssertEquals("Related parents Grid should have 3 elements", 3, userControl.RelatedParentsGrid.ListManager.Count);
				AssertEquals("PlugIn's CurrentStorageMainInGrid should be the top level parent", plugIn.CurrentParentMainOnGrid, userControl.RelatedParentsGrid.ListManager.GetCurrent());
				AssertEquals("Copy menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
				AssertEquals("Copy link menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
				AssertEquals("Cut menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
				AssertEquals("Delete menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
				AssertEquals("Deliver menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
				AssertEquals("Modify menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
				AssertEquals("Paste menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
				AssertEquals("restore menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				grid.UpdateContextMenuElements((StorageDocs)grid.ListManager.GetCurrent());
				// none of the grid elements for editing should be enabled
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignee comes first alphabetically)", consigneeMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);
				AssertEquals("Copy menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
				AssertEquals("Copy link menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
				AssertEquals("Cut menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
				AssertEquals("Delete menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
				AssertEquals("Deliver menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
				AssertEquals("Modify menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
				AssertEquals("Paste menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
				AssertEquals("restore menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);

				userControl.RelatedParentsGrid.ListManager.Position = 2;
				grid.UpdateContextMenuElements((StorageDocs)grid.ListManager.GetCurrent());
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignor comes last alphabetically)", consignorMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);
				// none of the grid elements for editing should be enabled
				AssertEquals("Copy menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
				AssertEquals("Copy link menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
				AssertEquals("Cut menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
				AssertEquals("Delete menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
				AssertEquals("Deliver menu item enabled status", true, FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
				AssertEquals("Modify menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
				AssertEquals("Paste menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
				AssertEquals("restore menu item enabled status", false, FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			}
		}

		public void TestNotEditableLabelShowsForRelatedParents()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();

			OrgHeader consignee = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			shipment["ConsigneePK"] = consignee.PK;
			OrgHeader consignor = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			shipment["ConsignorPK"] = consignor.PK;

			Assert("Precondition: database " + DbHelper.GetDatabaseName(1) + " exists", DbHelper.DatabaseExists(1));

			StorageMain main = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main.SM_ParentFK = shipment.PK;
			main.Documents.AddNew();

			StorageMain consigneeMain = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			consigneeMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consigneeMain.SM_ParentFK = ((OrgHeader)shipment["Consignee"]).PK;
			consigneeMain.Documents.AddNew();

			StorageMain consignorMain = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			consignorMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consignorMain.SM_ParentFK = ((OrgHeader)shipment["Consignor"]).PK;
			consignorMain.Documents.AddNew();
			MasterFactory.Save();

			AssertEquals("Precondition: 3 records in StorageMain table", 3, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = form.GetTestPlugInInstance();
				AssertNotNull("Precondition: Tab control should exist", form.TabControl);
				Assert("Precondition: Tab page exists", form.TabControl.TabPages.Count > 0);
				Assert("Precondition: eDocs user control exists", form.TabControl.TabPages[0].Controls.Count > 0);
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				AssertNotNull("Related parents grid should be bound", userControl.RelatedParentsGrid.ListManager);
				AssertEquals("Related parents Grid should have 3 elements", 3, userControl.RelatedParentsGrid.ListManager.Count);
				AssertEquals("PlugIn's CurrentStorageMainInGrid should be the top level parent", plugIn.CurrentParentMainOnGrid, userControl.RelatedParentsGrid.ListManager.GetCurrent());
				AssertEquals("NotEditable label is not visible", false, userControl.NotEditableLabel.Visible);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignee comes first alphabetically)", consigneeMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);
				AssertEquals("NotEditable label is visible", true, userControl.NotEditableLabel.Visible);

				userControl.RelatedParentsGrid.ListManager.Position = 0;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list", main.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);
				AssertEquals("NotEditable label is not visible", false, userControl.NotEditableLabel.Visible);
			}
		}

		public void TestEDocsPlugInIsNotAllowInsertWhenSMDBIsZero()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();

				var plugIn = form.GetTestPlugInInstance();
				plugIn.TopLevelParentMain.SM_DB = 0;
				Assert("IsInsertAllowed should be false when SM_DB of TopLevelParentMain is 0", !plugIn.IsInsertAllowed);
			}
		}

		public void TestStorageMainWasNotDeletedTwice()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			StorageMain storageMain = null;

			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				storageMain = plugIn.CurrentParentMainOnGrid;

				using (var form2 = new ZFormForPlugInTest(bizObj))
				{
					form2.Show();
					var plugIn2 = (eDocsPlugIn)form2.PlugIns.Instances[0];
					_ = plugIn2.CurrentParentMainOnGrid;

					var anotherStorageMain = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
					anotherStorageMain.SM_ParentFK = bizObj.PK;
					MasterFactory.Save();

					AssertNoExceptionThrown(() => storageMain.AttemptToResolveUniqueIndexFailure());
				}
			}
		}

		public void TestEDocsPlugInIsNotAllowInsertWhenEDocsModifyIsNotGranted()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				Env.Security.eDocsModify.IsAllowed = false;
				Assert("IsInsertAllowed should be false when eDocsModify Security items is not granted", !plugIn.IsInsertAllowed);
			}
		}

		[RequiresSTA]
		public void TestEDocsPlugInIsNotAllowInsertWhenIsSecurityNotGranted()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();

			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var checkpoint = new SecurityCheckpoint("TTT", (NoResString)"Jerry Test 1", null, security.ZSecurityInstance);
			checkpoint.IsAllowed = false;
			Env.Security.eDocsModify.IsAllowed = true;

			AssertEDocsPlugInIsNotAllowInsertWhenIsSecurityNotGranted(bizObj, checkpoint, false, true, false);

			checkpoint.IsAllowed = true;
			AssertEDocsPlugInIsNotAllowInsertWhenIsSecurityNotGranted(bizObj, checkpoint, true, true, true);
		}

		void AssertEDocsPlugInIsNotAllowInsertWhenIsSecurityNotGranted(BusinessObject bizObj, SecurityCheckpoint checkpoint, bool expectedIsSecurityGranted, bool expectedIsSecurityGrantedEdit, bool expectedIsInsertAllowed)
		{
			using (var form = new ZFormForPlugInTest(bizObj, checkpoint))
			{
				form.Show();
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];

				AssertEquals("IsSecurityGranted shoule be false", expectedIsSecurityGranted, plugIn.IsSecurityGrantedForTesting);
				AssertEquals("IsSecurityGrantedEdit shoule be true", expectedIsSecurityGrantedEdit, plugIn.IsSecurityGrantedEditForTesting);
				AssertEquals("IsInsertAllowed should be false", expectedIsInsertAllowed, plugIn.IsInsertAllowed);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocManagerDataObjectShouleBeAddedToTopLevelParentMainDuringFileDragDrop()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizObj))
			{
				form.Show();
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];

				var anotherBizObj = MasterFactory.New<eDocsUserControlTest.OrgHeader2>();
				anotherBizObj.OH_Code = "JERRYTEST";
				var anotherStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
				anotherStorageMain.SM_ParentFK = anotherBizObj.PK;
				anotherStorageMain.SM_Type = "ORG";
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(anotherStorageMain, anotherBizObj);

				plugIn.CurrentParentMainOnGrid = anotherStorageMain;

				AssertEquals("TopLevelParentMain should have 0 file", 0, plugIn.TopLevelParentMain.eDocs.Count);
				AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif");
				var documentToDrag = StorageDocs.New_DEBUG(MasterFactory);
				documentToDrag.SC_ImageData = DocumentUtilities.GetFileAsBytes(filePath);
				var dataToDrop = new DocManagerDataObject(null, new BusinessObject[] { documentToDrag });
				var files = (string[])dataToDrop.GetData(DataFormats.FileDrop);

				AssertEquals("dataToDrop should have 1 file", 1, files.Length);

				try
				{
					var args = new DragEventArgs(dataToDrop, 0, 10, 60, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
					plugIn.OnParentFormDragDropDone(form, args);

					AssertEquals("TopLevelParentMain should have 1 file", 1, plugIn.TopLevelParentMain.eDocs.Count);
					AssertEquals("CurrentParentMainOnGrid should have 0 file", 0, plugIn.CurrentParentMainOnGrid.eDocs.Count);
				}
				finally
				{
					File.Delete(files[0]);
				}
			}
		}

		public void TestEDocsPlugInIsAllowInsert()
		{
			var bizObj1 = Factory.NewWithValidTestData<OrgHeader>();
			bizObj1.ReadOnly = true;
			AssertEDocsPlugInIsAllowInsert(bizObj1, false);
			bizObj1.ReadOnly = false;
			AssertEDocsPlugInIsAllowInsert(bizObj1, true);

			var bizObj2 = Factory.NewWithValidTestData<DummyBizObjectAllowReadOnlyAddEDocs>();
			bizObj2.ReadOnly = true;
			AssertEDocsPlugInIsAllowInsert(bizObj2, true);
			bizObj2.ReadOnly = false;
			AssertEDocsPlugInIsAllowInsert(bizObj2, true);
		}

		void AssertEDocsPlugInIsAllowInsert(BusinessObject biz, bool allowDragEdocs)
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(biz))
			{
				form.Show();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				AssertEquals("should " + (allowDragEdocs ? "" : "not") + " allow drag edocs in main screen of job form", allowDragEdocs, plugIn.IsInsertAllowed);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveForm_WhenStorageDocsIsOpened_DocShouldBeReloaded()
		{
			var bizO = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZFormForPlugInTest(bizO))
			{
				form.Show();
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];

				using (var newFile = plugIn.TopLevelParentMainForTesting.Files.AddNew())
				{
					newFile.SC_FileName = "TestTextFile";
					newFile.SC_DataType = "TXT";
					newFile.SC_DocType = "T01";
					newFile.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestTextFile.txt"));
					plugIn.TopLevelParentMainForTesting.MasterFactory.Save();

					newFile.SC_DocType = "T02";

					newFile.SaveToTempFile();
					using (var openedFile = File.Open(newFile.TempFileName, FileMode.Open))
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						plugIn.ShowPreSaveDialogsCore();
						AssertEquals("File should be reloaded after continued with save while file is opened.", "T01", newFile.SC_DocType);
						form.FireSaveButton();
						AssertEquals("Should Not Add DDI Event Log.", 0, (newFile.ParentMain.DocumentOwner as OrgHeader).Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(newFile.PK, "T02", ""))).Length);
					}
				}
			}
		}

		[UseSnapshotProtection(true)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveForm_WhenStorageDocsIsOpenedAndDeleted_ShouldNotThrowException()
		{
			var bizO = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			using (var form = new ZFormForPlugInTest(bizO))
			{
				form.Show();
				var plugIn = (eDocsPlugInForTesting)form.PlugIns.Instances[0];

				using (var newFile = plugIn.TopLevelParentMainForTesting.Files.AddNew())
				{
					newFile.SC_FileName = "TestTextFile";
					newFile.SC_DataType = "TXT";
					newFile.SC_DocType = "T01";
					newFile.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestTextFile.txt"));
					plugIn.TopLevelParentMainForTesting.MasterFactory.Save();

					newFile.SC_DocType = "T02";

					var deleteFileThread = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							var newFactory = new BusinessObjectFactory();
							var orgHeader = newFactory.Load<OrgHeader>(bizO.PK);
							var masterFactory = new DocumentFactoryProvider().GetFactory(newFactory);
							var storageMain = masterFactory.RetrieveExistingOrCreateStorageMain(orgHeader, "ORG");
							storageMain.Files[0].SC_IsDeleted = true;
							masterFactory.Save();
						}
					});
					deleteFileThread.Start();
					deleteFileThread.Join();

					newFile.SaveToTempFile();
					using (var openedFile = File.Open(newFile.TempFileName, FileMode.Open))
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						AssertNoExceptionThrown(() => plugIn.ShowPreSaveDialogsCore());
						AssertEquals("File should be reloaded after continued with save while file is opened.", "T01", newFile.SC_DocType);
					}
				}
			}
		}

		public void TestNoException_AttachOrder_AndAddEDocs()
		{
			var importer = SetupOrgHeader();
			var importerMainAddress = importer.Addresses.MainAddress;

			var order = SetupOrder(importerMainAddress.PK);
			AddNewJobRequiredDoc((IHaveRequiredDocuments)order, Core.Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDateTimeOffset.Empty, ZDate.Empty, ZString.Empty, ZGuid.Empty);
			AddNewJobRequiredDoc((IHaveRequiredDocuments)order, Core.Constants.RefDocTypes.ArrivalNotice, JobRequiredDocument.DocUsage.Both, Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDateTimeOffset.Empty, ZDate.Empty, ZString.Empty, ZGuid.Empty);

			var declaration = SetupDeclaration(importer.PK);
			var order2 = SetupOrder(importerMainAddress.PK);
			order2["JD_JE"] = declaration.PK;

			Factory.Save();
			ReleaseFactory();

			declaration = Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration.PK);
			using (var form = new DeclarationTestForm(declaration) { Width = 1000, Height = 1000 })
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var toolStrip = form.zModuleButtonGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var method = typeof(EmbeddedModulePopup).GetMethod("HandleSelection", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(form.zModuleButtonGrid.LastShownAttachPopupForTesting, new object[] { new BusinessObject[] { order } });
				Application.DoEvents();

				var eDocsTabPage = form.OpenFormTab("eDocsTabPage");
				var documentsGrid = eDocsTabPage.Controls.Find("DocumentsGrid", true)[0] as ZArchitecture.ZGrid;
				AssertNotEquals(1, documentsGrid.VisibleRowCount);

				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals(3, documentsGrid.VisibleRowCount);

				var docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
				docManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "test.pdf", Core.Constants.RefDocTypes.AgentsInvoice);
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();
				AssertNotContains("Should not have DeveloperNotificationException", @"Multiple Row Factories editing the same record.

Different Factories trying to save row in Factory.SaveTogether (JobRequiredDocument", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		JobRequiredDocument AddNewJobRequiredDoc(IHaveRequiredDocuments requiredDocumentParent, ZString docType, ZString docUsage, ZString period, ZDateTimeOffset recvDate, ZDate date, ZString docNumber, ZGuid documentOwner)
		{
			var result = requiredDocumentParent.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate;
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			result.EQ_OH_DocumentOwner = documentOwner;
			return result;
		}

		OrgHeader SetupOrgHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";
			importer.LocalBusinessRegNo = "26008672179";
			importer.MiscServ.OM_IMEFTBankBSB = "123";
			importer.MiscServ.OM_IMEFTBankAccount = "123456789";
			return importer;
		}

		Enterprise.Integration.Customs.IBaseJobDeclaration SetupDeclaration(ZGuid importerPK)
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HYOGO MARU";
			vessel.RV_LloydsNumber = "4567123";

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration["JE_TransportMode"] = Core.Constants.TransportModes.Sea;
			declaration["JE_VesselName"] = vessel.RV_Name;
			declaration["JE_DateAtOrigin"] = ZDateTime.Today;
			declaration["JE_DateAtFinalDestination"] = ZDateTime.Today;
			declaration["JE_ExportDate"] = ZDateTime.Today;
			declaration["JE_PaymentMethod"] = "IMP";

			declaration.JE_OwnerRef = "OWNERREF";
			declaration.JE_MasterBill = "08111111111";
			declaration.JE_HouseBill = "HBL234908-5";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_RL_NKOrigin = "USKCK";

			declaration["JE_RL_NKPortOfArrival"] = "AUSYD";
			declaration["JE_RL_NKPortOfFirstArrival"] = "AUSYD";
			declaration["JE_RL_NKPortOfLoading"] = "USKCK";
			declaration["JE_ShipmentIncoTerm"] = Core.Constants.IncoTerms.FreeOnBoard;

			declaration.JE_TotalNoOfPacks = 32;
			declaration["JE_TotalNoOfPacksPackType"] = "PLT";
			declaration["JE_TotalVolume"] = 4.740m;
			declaration["JE_TotalVolumeUnit"] = "M3";
			declaration["JE_TotalWeight"] = 591.000m;
			declaration["JE_TotalWeightUnit"] = "KG";
			declaration["JE_TransportMode"] = Core.Constants.TransportModes.Air;
			declaration["JE_VoyageFlightNo"] = "HW652";

			declaration.JE_OH_Importer = importerPK;
			declaration.JE_OH_Supplier = supplier.PK;

			return declaration;
		}

		BusinessObject SetupOrder(ZGuid buyerAddressPK)
		{
			var order = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			order["JD_TransportMode"] = Core.Constants.TransportModes.Air;
			order["JD_ContainerMode"] = Core.Constants.ContainerModes.Loose;
			order["JD_OA_BuyerAddress"] = buyerAddressPK;
			return order;
		}

		class DeclarationTestForm : ZTemplateForm
		{
			public readonly Enterprise.Integration.Customs.IBaseJobDeclaration declaration;
			public readonly ZModuleButtonGrid zModuleButtonGrid;
			public DeclarationTestForm(Enterprise.Integration.Customs.IBaseJobDeclaration declaration)
			: base(declaration)
			{
				this.declaration = declaration;

				var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "JD_OrderNumberAndSplit";
				zTextBoxColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(75);

				zModuleButtonGrid = new ZModuleButtonGrid();
				zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				zModuleButtonGrid.BindToFindBoxList = "PossibleOrdersForAttachment_List";
				this.MainTabPage.Controls.Add(zModuleButtonGrid);

				BindingSource.SetBindingMember(zModuleButtonGrid, "AttachedOrders");
				this.PerformLayout();
			}

			public ZTabPage OpenFormTab(string tabPageName)
			{
				var curentTabPage = (ZTabPage)this.Controls.Find(tabPageName, true).FirstOrDefault();
				var tabPage = MainTabControl.GetTabPage(tabPageName);
				if (tabPage != null)
				{
					MainTabControl.SelectedTab = tabPage;
					MainTabControl.PerformLayout();
				}
				return curentTabPage;
			}
		}

		class DummyBizObjectAllowReadOnlyAddEDocs : DummyBusinessObject, IDocManagerSupport
		{
			public DummyBizObjectAllowReadOnlyAddEDocs(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			DocManagerInfo IDocManagerSupport.DocManagerInfo => new DocManagerInfoForTest(this);
		}

		class DocManagerInfoForTest : DocManagerInfo
		{
			public DocManagerInfoForTest(BusinessObject parent)
				: base(parent, Core.Constants.DocManagerCodes.UNLOCO)
			{
			}

			public override bool ReadOnly => false;
		}

		#region Implementation

		MenuItem FindMenuItemByName(ContextMenu menu, string text)
		{
			foreach (MenuItem item in menu.MenuItems)
			{
				if (item.Text == text)
				{
					return item;
				}
			}
			return null;
		}

		DocManagerDBHelper DbHelper
		{
			get
			{
				if (dbHelper == null)
				{
					dbHelper = new DocManagerDBHelper();
				}
				return dbHelper;
			}
		}

		DocManagerDBHelper dbHelper;

		#endregion
	}
}
