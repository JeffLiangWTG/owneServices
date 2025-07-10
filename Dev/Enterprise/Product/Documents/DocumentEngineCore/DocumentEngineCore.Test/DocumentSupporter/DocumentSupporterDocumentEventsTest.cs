using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterDocumentEventsTest : TestCaseWithFactory
	{
		public void TestDocumentEvents()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var documentEventsHandler = new MockDocumentEventsHandler();
			var documentSupporter = new MockDocumentSupporter(dummy, documentEventsHandler);

			AssertEquals("Precondition: documentSupporter.DocumentEventsHandlers.Length", 1, documentSupporter.DocumentEventsHandlers.Length);
			AssertEquals("documentSupporter.DocumentEventsHandlers[0]", documentEventsHandler, documentSupporter.DocumentEventsHandlers[0]);

			var documentEvents = new MockDocumentEvents();
			documentSupporter.Initialise(documentEvents);
			AssertEquals("Pre-condition: documentEventsHandler.HandleDocumentPrintedCallCount", 0, documentEventsHandler.HandleDocumentPrintedCallCount);
			AssertEquals("Pre-condition: documentEventsHandler.HandleDocumentPrePrintedCallCount", 0, documentEventsHandler.HandleDocumentPrePrintedCallCount);
			AssertEquals("Pre-condition: documentEventsHandler.HandleDocumentPrintRequestedCallCount", 0, documentEventsHandler.HandleDocumentPrintRequestedCallCount);

			var documentCancelEventArgs = new DocumentCancelEventArgs(null);
			var documentPrintedEventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Auto, null);
			documentEventsHandler.SetCanHandleMenuItem(false);
			documentEvents.RaiseDocumentPrintRequested(this, documentCancelEventArgs);
			documentEvents.RaiseDocumentPrePreviewed(this, documentPrintedEventArgs);
			documentEvents.RaiseDocumentPrePrinted(this, documentPrintedEventArgs);
			documentEvents.RaiseDocumentPrinted(this, documentPrintedEventArgs);
			AssertEquals("documentEventsHandler.HandleDocumentPrintedCallCount", 0, documentEventsHandler.HandleDocumentPrintedCallCount);
			AssertEquals("documentEventsHandler.HandleDocumentPrePreviewedCallCount", 0, documentEventsHandler.HandleDocumentPrePreviewedCallCount);
			AssertEquals("documentEventsHandler.HandleDocumentPrePrintedCallCount", 0, documentEventsHandler.HandleDocumentPrePrintedCallCount);
			AssertEquals("documentEventsHandler.HandleDocumentPrintRequestedCallCount", 0, documentEventsHandler.HandleDocumentPrintRequestedCallCount);

			documentEventsHandler.SetCanHandleMenuItem(true);
			documentEvents.RaiseDocumentPrintRequested(this, documentCancelEventArgs);
			documentEvents.RaiseDocumentPrePreviewed(this, documentPrintedEventArgs);
			documentEvents.RaiseDocumentPrePrinted(this, documentPrintedEventArgs);
			documentEvents.RaiseDocumentPrinted(this, documentPrintedEventArgs);
			AssertEquals("documentEventsHandler.HandleDocumentPrintedCallCount", 1, documentEventsHandler.HandleDocumentPrintedCallCount);
			AssertEquals("documentEventsHandler.HandleDocumentPrePreviewedCallCount", 1, documentEventsHandler.HandleDocumentPrePreviewedCallCount);
			AssertEquals("documentEventsHandler.HandleDocumentPrePrintedCallCount", 1, documentEventsHandler.HandleDocumentPrePrintedCallCount);
			AssertEquals("documentEventsHandler.HandleDocumentPrintRequestedCallCount", 1, documentEventsHandler.HandleDocumentPrintRequestedCallCount);
		}

		class MockDocumentEvents : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;

			public void RaiseDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(sender, e);
				}
			}

			public void RaiseDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(sender, e);
			}

			public void RaiseDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(sender, e);
				}
			}

			public void RaiseDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(sender, e);
				}
			}
		}

		class MockDocumentEventsHandler : IDocumentEventsHandler
		{
			bool canHandleMenuItem;

			public bool CanHandleMenuItem(IStmMenuItem menuItem)
			{
				return canHandleMenuItem;
			}

			public void SetCanHandleMenuItem(bool canHandleMenuItem)
			{
				this.canHandleMenuItem = canHandleMenuItem;
			}

			public int HandleDocumentPrintRequestedCallCount;
			public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				HandleDocumentPrintRequestedCallCount++;
			}

			public int HandleDocumentPrePreviewedCallCount;
			public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
				HandleDocumentPrePreviewedCallCount++;
			}

			public int HandleDocumentPrePrintedCallCount;
			public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
				HandleDocumentPrePrintedCallCount++;
			}

			public int HandleDocumentPrintedCallCount;
			public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				HandleDocumentPrintedCallCount++;
			}

			public DocumentSupporter DocumentSupporter { get; set; }
		}

		class MockDocumentSupporter : DocumentSupporter
		{
			readonly IDocumentEventsHandler documentEventsHandler;

			internal MockDocumentSupporter(BusinessObject parentBusinessObject, IDocumentEventsHandler documentEventsHandler)
				: base(parentBusinessObject)
			{
				this.documentEventsHandler = documentEventsHandler;
			}

			protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
			{
				var result = new List<IDocumentEventsHandler>() { documentEventsHandler };
				return result.ToArray();
			}

			public override BusinessContext BusinessContext
			{
				get { throw new NotImplementedException(); }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				throw new NotImplementedException();
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				throw new NotImplementedException();
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}
