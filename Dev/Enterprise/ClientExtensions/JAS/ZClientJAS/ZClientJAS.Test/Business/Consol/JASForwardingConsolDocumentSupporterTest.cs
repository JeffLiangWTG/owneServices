using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASForwardingConsolDocumentSupporter))]
	internal class JASForwardingConsolDocumentSupporterTest : ForwardingConsolDocumentSupporterTest
	{
		public void TestCorrectBaseClass()
		{
			ForwardingConsolForTest baseConsol = Factory.New<ForwardingConsolForTest>();
			AssertEquals("Should be sub-classing from ForwardingConsolDocumentSupporter", baseConsol.DocumentSupporter.GetType(), typeof(JASForwardingConsolDocumentSupporter).BaseType);
		}

		#region JXC Message Export On DocumentPrinted
		public void TestDocumentPrinted_AutoJXCMessagingDisabled()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = false;
			DocumentSupporter.Initialise(DocumentEventSource);
			ConsolMock.Verify(m => m.ExportJXCAirOrOceanMessage(), Times.Never);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoExceptionThrown(() => DocumentEventSource.FireDocumentPrinted("Print Final Master", "AWB"));
			ConsolMock.VerifyAll();
		}

		public void TestDocumentPrinted_OtherMenuNameShouldNotTriggerJXCAirOrOceanExport()
		{
			DocumentSupporter.Initialise(DocumentEventSource);
			ConsolMock.Verify(m => m.ExportJXCAirOrOceanMessage(), Times.Never);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoExceptionThrown(() => DocumentEventSource.FireDocumentPrinted("Print Neutral Master", "AWB"));
			ConsolMock.VerifyAll();
		}

		public void TestDocumentPrinted_WrongTransportMode()
		{
			DocumentSupporter.Initialise(DocumentEventSource);
			ConsolMock.Verify(m => m.ExportJXCAirOrOceanMessage(), Times.Never);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Sea;
			DocumentEventSource.FireDocumentPrinted("Print Final Master", "AWB");
			ConsolMock.VerifyAll();
			ConsolMock.Verify(m => m.ExportJXCAirOrOceanMessage(), Times.Never);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoExceptionThrown(() => DocumentEventSource.FireDocumentPrinted("Container Manifest", "Departure/Manifest"));
			ConsolMock.VerifyAll();
		}

		public void TestDocumentPrinted()
		{
			DocumentSupporter.Initialise(DocumentEventSource);
			ConsolMock.Setup(m => m.ExportJXCAirOrOceanMessage());
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			DocumentEventSource.FireDocumentPrinted("Print Final Master", "AWB");
			ConsolMock.VerifyAll();
			ConsolMock.Setup(m => m.ExportJXCAirOrOceanMessage());
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Sea;
			DocumentEventSource.FireDocumentPrinted("Container Manifest", "Departure/Manifest");
			ConsolMock.VerifyAll();
			ConsolMock.Setup(m => m.ExportJXCAirOrOceanMessage());
			AssertNoExceptionThrown(() => DocumentEventSource.FireDocumentPrinted("", "Departure/Manifest"));
			ConsolMock.VerifyAll();
		}

		DocumentEventSourceForTest DocumentEventSource
		{
			get
			{
				if (fDocumentEventSource == null)
				{
					fDocumentEventSource = new DocumentEventSourceForTest(Factory);
				}

				return fDocumentEventSource;
			}
		}

		DocumentEventSourceForTest fDocumentEventSource;
		#region class DocumentEventSourceForTest
		class DocumentEventSourceForTest : IDocumentEvents
		{
			public DocumentEventSourceForTest(BusinessObjectFactory factory)
			{
				this.Factory = factory;
				// to make the compiler happy
				DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSourceForTest_DocumentPrintRequested);
			}

			void DocumentEventSourceForTest_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public void FireDocumentPrePreviewed(ZString menuName, ZString menuPath)
			{
				if (DocumentPrePreviewed != null)
				{
				}
			}

			public void FireDocumentPrePrinted(ZString menuName, ZString menuPath)
			{
				if (DocumentPrePrinted != null)
				{
				}
			}

			public void FireDocumentPrinted(ZString menuName, ZString menuPath)
			{
				if (DocumentPrinted != null)
				{
					StmMenuItem menuItem = Factory.New<StmMenuItem>();
					menuItem.SU_MenuName = menuName;
					menuItem.SU_MenuPath = menuPath;
					DocumentPrinted(this, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Auto, menuItem));
				}
			}

			readonly BusinessObjectFactory Factory;
			#region IDocumentEvents Members
			public event DocumentCancelEventHandler DocumentPrintRequested
			{
				add
				{
				}

				remove
				{
				}
			}

			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;
			#endregion
		}

		#endregion
		#endregion
		#region GetDataStateBeforeRun
		public void TestGetDataStateBeforeRun_AutoJXCMessagingDisabled()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = false;
			HookGettingDataStateBeforeRunEvent();
			ShouldBeCancelled = true;
			DocumentSupporterDataState dataState = DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Assert("Should be valid, auto messaging is not enabled", dataState.IsValid);
		}

		public void TestGetDataStateBeforeRun_UnrelatedMenuNameIsNotCheckedForAutoJXCMessaging()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			DocumentCommand decoyDocument = Factory.New<DocumentCommand>();
			decoyDocument.SU_MenuName = "Some Documents";
			HookGettingDataStateBeforeRunEvent();
			ShouldBeCancelled = true;
			DocumentSupporterDataState dataState = DocumentSupporter.GetDataStateBeforeRun(decoyDocument);
			Assert("Should be valid, command does not trigger auto messaging", dataState.IsValid);
		}

		public void TestGetDataStateBeforeRun_WrongTransportMode()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			HookGettingDataStateBeforeRunEvent();
			ShouldBeCancelled = true;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Sea;
			DocumentSupporterDataState dataState = DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Assert("Should be valid, transport mode is not related to the command", dataState.IsValid);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			dataState = DocumentSupporter.GetDataStateBeforeRun(ManifestDocument);
			Assert("Should be valid, transport mode is not related to the command", dataState.IsValid);
		}

		public void TestGetDataStateBeforeRun_GettingDataStateBeforeRunEventIsUnhooked()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			DocumentSupporterDataState dataState = DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Assert("Should be valid, GettingDataStateBeforeRunEvent is unhooked", dataState.IsValid);
		}

		public void TestGetDataStateBeforeRun()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			HookGettingDataStateBeforeRunEvent();
			ShouldBeCancelled = true;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			DocumentSupporterDataState dataState = DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument);
			Assert("Should not be valid, event is cancelled for 'print final master' document and air transport mode", !dataState.IsValid);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Sea;
			dataState = DocumentSupporter.GetDataStateBeforeRun(ManifestDocument);
			Assert("Should not be valid, event is cancelled for 'Export/Manifest' document and sea transport mode", !dataState.IsValid);
			ShouldBeCancelled = false;
			dataState = DocumentSupporter.GetDataStateBeforeRun(ManifestDocument);
			Assert("Should be valid, event not cancelled", dataState.IsValid);
		}

		public void TestGetDataStateBeforeRun_StmMenuItem_NoException()
		{
			var documentPackDocument = Factory.NewWithValidTestData<DocumentCommand>();
			documentPackDocument.SU_MenuPath = "Departure/Document Pack";
			documentPackDocument.SU_MenuName = "Consol Agent Pack J (AIR)";
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			DocumentSupporter.GettingDataStateBeforeRun += new CancelEventHandler(DocumentSupporter_GettingDataStateBeforeRun);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			documentPackDocument.Parent = Consol;
			documentPackDocument.SU_IsDocPack = true;
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals("Should not have pop-up", false, isOnGettingDataStateBeforeRunCalled);
			StmMenuMenuPivotBase childPivot = documentPackDocument.ChildMenus.AddNew();
			childPivot.SF_SU_Inward = documentPackDocument.PK;
			childPivot.SF_SU_Outward = PrintFinalMasterDocument.PK;
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = true;
			AssertEquals("Should Not Export Air Or Ocean JXC Message", false, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals("Should have pop-up", true, isOnGettingDataStateBeforeRunCalled);
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(1, DocumentSupporter.ChildList.Count);
			AssertEquals("Should have pop-up", true, isOnGettingDataStateBeforeRunCalled);
			isOnGettingDataStateBeforeRunCalled = false;
			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery("Consol", "Print Final Master"));
			AssertEquals("Should not throw exception", true, DocumentSupporter.GetDataStateBeforeRun(stmMenuItem).IsValid);
			AssertEquals(1, DocumentSupporter.ChildList.Count);
			AssertEquals("Should have pop-up", true, isOnGettingDataStateBeforeRunCalled);
		}

		public void TestGetDataStateBeforeRun_DocumentPack()
		{
			var documentPackDocument = Factory.NewWithValidTestData<DocumentCommand>();
			documentPackDocument.SU_MenuPath = "Departure/Document Pack";
			documentPackDocument.SU_MenuName = "Consol Agent Pack J (AIR)";
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			DocumentSupporter.GettingDataStateBeforeRun += new CancelEventHandler(DocumentSupporter_GettingDataStateBeforeRun);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			documentPackDocument.Parent = Consol;
			documentPackDocument.SU_IsDocPack = true;
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals("Should not have pop-up", false, isOnGettingDataStateBeforeRunCalled);
			StmMenuMenuPivotBase childPivot = documentPackDocument.ChildMenus.AddNew();
			childPivot.SF_SU_Inward = documentPackDocument.PK;
			childPivot.SF_SU_Outward = PrintFinalMasterDocument.PK;
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = true;
			AssertEquals("Should Not Export Air Or Ocean JXC Message", false, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals("Should have pop-up", true, isOnGettingDataStateBeforeRunCalled);
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(1, DocumentSupporter.ChildList.Count);
			AssertEquals("Should have pop-up", true, isOnGettingDataStateBeforeRunCalled);
			isOnGettingDataStateBeforeRunCalled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(PrintFinalMasterDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals("Should not have pop-up", false, isOnGettingDataStateBeforeRunCalled);
		}

		public void TestGetDataStateBeforeRun_DocumentPack_ExportBadDocPackAfterGoodDocPack()
		{
			var documentPackDocument = Factory.NewWithValidTestData<DocumentCommand>();
			documentPackDocument.SU_MenuPath = "Departure/Document Pack";
			documentPackDocument.SU_MenuName = "Consol Agent Pack J (AIR)";
			JASDataRegistry.Instance.EnableAutoJXCMessaging = true;
			DocumentSupporter.GettingDataStateBeforeRun += new CancelEventHandler(DocumentSupporter_GettingDataStateBeforeRun);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.Shipments[0].JS_TransportMode = Core.Constants.TransportModes.Air;
			documentPackDocument.Parent = Consol;
			documentPackDocument.SU_IsDocPack = true;
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals(false, isOnGettingDataStateBeforeRunCalled);
			StmMenuMenuPivotBase childPivot = documentPackDocument.ChildMenus.AddNew();
			childPivot.SF_SU_Inward = documentPackDocument.PK;
			childPivot.SF_SU_Outward = PrintFinalMasterDocument.PK;
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = true;
			AssertEquals("Should Not Export Air Or Ocean JXC Message", false, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals(true, isOnGettingDataStateBeforeRunCalled);
			documentPackDocument.ChildMenus.Remove(childPivot);
			isOnGettingDataStateBeforeRunCalled = false;
			ShouldBeCancelled = false;
			AssertEquals("Should Export Air Or Ocean JXC Message", true, DocumentSupporter.GetDataStateBeforeRun(documentPackDocument).IsValid);
			AssertEquals(0, DocumentSupporter.ChildList.Count);
			AssertEquals(false, isOnGettingDataStateBeforeRunCalled);
		}

		#endregion
		public void TestEnsureAutoJXCExportTriggerMenuItemsExist()
		{
			AssertMenuItemsExist("AWB", "Print Final Master");
			AssertMenuItemsExist("Departure/Manifest", "");
		}

		#region Implementation
		JASForwardingConsolDocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new JASForwardingConsolDocumentSupporter(Consol);
				}

				return fDocumentSupporter;
			}
		}

		Mock<JASForwardingConsol> ConsolMock
		{
			get
			{
				if (fConsolMock == null)
				{
					fConsolMock = Factory.NewMoq<JASForwardingConsol>();
					Consol.Shipments.AddNew();
				}

				return (Mock<JASForwardingConsol>)fConsolMock;
			}
		}

		JASForwardingConsol Consol
		{
			get
			{
				return ConsolMock.Object;
			}
		}

		void HookGettingDataStateBeforeRunEvent()
		{
			DocumentSupporter.GettingDataStateBeforeRun += new CancelEventHandler(DocumentSupporter_GettingDataStateBeforeRun);
		}

		void DocumentSupporter_GettingDataStateBeforeRun(object sender, CancelEventArgs args)
		{
			isOnGettingDataStateBeforeRunCalled = true;
			args.Cancel = ShouldBeCancelled;
		}

		DocumentCommand PrintFinalMasterDocument
		{
			get
			{
				return Factory.LoadTop1<DocumentCommand>(new DocumentZQuery("Consol", "Print Final Master"));
			}
		}

		DocumentCommand ManifestDocument
		{
			get
			{
				return Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Departure/Manifest"));
			}
		}

		void AssertMenuItemsExist(ZString menuPath, ZString menuName)
		{
			StmMenuItem[] menuItems = LoadMenuItems(menuPath, menuName);
			string failureMessage = "Number of menu items with MenuPath = '" + menuPath + "'" + ((!menuName.IsEmpty) ? (" and MenuName = '" + menuName + "'") : "");
			Assert(failureMessage, menuItems.Length > 0);
		}

		StmMenuItem[] LoadMenuItems(ZString menuPath, ZString menuName)
		{
			DocumentZQuery query = new DocumentZQuery(StmMenuItemSchema.SU_MenuPath, menuPath);
			if (!menuName.IsEmpty)
			{
				query.AddToFilter(StmMenuItemSchema.SU_MenuName, menuName);
			}

			return (StmMenuItem[])Factory.Load(typeof(StmMenuItem), query);
		}

		JASForwardingConsolDocumentSupporter fDocumentSupporter;
		Mock fConsolMock;
		bool ShouldBeCancelled;
		bool isOnGettingDataStateBeforeRunCalled;
		#region ForwardingConsolForTest
		class ForwardingConsolForTest : ForwardingConsol
		{
			public ForwardingConsolForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
		#endregion
		#endregion
	}
}
