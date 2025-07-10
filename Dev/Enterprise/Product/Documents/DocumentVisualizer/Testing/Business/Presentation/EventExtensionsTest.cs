using System;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EventExtensionsTest : TestCase
	{
		public void TestSubscribeFiltersAction()
		{
			var broker = new EventBroker();

			var document1 = new DummyDocument();
			var document2 = new DummyDocument();

			var invocationsCounter = new Dictionary<IDocument, int>
			{
				[document1] = 0,
				[document2] = 0,
			};

			void HandleEvent(DummyDocumentAwareEvent args) => invocationsCounter[args.Document]++;

			var handlerForDocument1 = broker.GetEvent<DummyDocumentAwareEvent>().Subscribe(HandleEvent, document1);
			var handlerForDocument2 = broker.GetEvent<DummyDocumentAwareEvent>().Subscribe(HandleEvent, document2);

			broker.Publish(new DummyDocumentAwareEvent
			{
				Document = document1
			});

			AssertEquals("event handler for document 1 was invoked", 1, invocationsCounter[document1]);
			AssertEquals("event handler for document 2 was not invoked", 0, invocationsCounter[document2]);

			broker.Publish(new DummyDocumentAwareEvent
			{
				Document = document2
			});

			AssertEquals("event handler for document 1 was not invoked", 1, invocationsCounter[document1]);
			AssertEquals("event handler for document 2 was invoked", 1, invocationsCounter[document2]);

			broker.Publish(new DummyDocumentAwareEvent
			{
				Document = document1
			});

			AssertEquals("event handler for document 1 was invoked", 2, invocationsCounter[document1]);
			AssertEquals("event handler for document 2 was not invoked", 1, invocationsCounter[document2]);
		}

		sealed class DummyDocumentAwareEvent : IDocumentAwareEvent
		{
			public IDocument Document { get; set; }
		}
	}
}
