using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Document = Enterprise.DocumentVisualizer.Business.Document;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentWrapperTest : TestCaseWithFactory
	{
		public void TestDocumentName()
		{
			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var logParent = new Moq.Mock<IStmALogParent>();

			descriptor.SetupGet(d => d.Name).Returns(documentName);

			var info = new Document(descriptor.Object, logParent.Object);
			AssertEquals(nameof(info.Name), documentName, info.Name);
		}

		public void TestLanguage()
		{
			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var logParent = new Moq.Mock<IStmALogParent>();

			descriptor.SetupGet(d => d.EnableTranslation).Returns(true);

			var info = new Document(descriptor.Object, logParent.Object);
			AssertEquals(nameof(info.EnableTranslation), true, info.EnableTranslation);
		}

		public void TestDocumentMenuName()
		{
			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var logParent = new Moq.Mock<IStmALogParent>();

			descriptor.SetupGet(d => d.MenuName).Returns(menuName);

			var info = new Document(descriptor.Object, logParent.Object);
			AssertEquals(nameof(info.MenuName), menuName, info.MenuName);
		}

		public void TestDocumentType()
		{
			var documentType = "TestDocumentType";

			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var logParent = new Moq.Mock<IStmALogParent>();

			descriptor.SetupGet(d => d.DocumentType).Returns(documentType);

			var info = new Document(descriptor.Object, logParent.Object);
			AssertEquals(nameof(info.Type), documentType, info.Type);
		}

		public void TestDocumentLogs()
		{
			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var logParent = (IStmALogParent)Factory.New<DummyWithLogs>();

			descriptor.SetupGet(d => d.Name).Returns(documentName);

			logParent.Logs.AddNew(Events.MessageSent,
				new ZDateTimeOffset(2015, 1, 1),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			logParent.Logs.AddNew(Events.MessageAccepted,
				new ZDateTimeOffset(2015, 1, 2),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			logParent.Logs.AddNew(Events.Authorised,
				new ZDateTimeOffset(2015, 1, 3));

			logParent.Logs.AddNew(Events.MessageSent,
				new ZDateTimeOffset(2015, 1, 4),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, string.Concat("other", documentName)));

			logParent.Logs.AddNew(Events.Attached, new ZDateTimeOffset(2015, 1, 4)).Cancel();

			var info = new Document(descriptor.Object, logParent);

			var formattedEvents = new List<string>();

			foreach (dynamic log in info.Logs)
			{
				var formattedEvent = string.Format("{0}|{1}|{2}", log.Event.Code, log.EventTime, log.MST);

				formattedEvents.Add(formattedEvent);
			}

			AssertContainsExactElementsInAnyOrder("Events",
				new[]
				{
					"ATH|03-Jan-15 00:00:00|",
					"MSN|04-Jan-15 00:00:00|otherTestDocumentName",
					"MSN|01-Jan-15 00:00:00|TestDocumentName",
					"MAA|02-Jan-15 00:00:00|TestDocumentName"
				},
				formattedEvents);
		}

		public void TestDocumentLogs_OrderByEventTime()
		{
			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var logParent = (IStmALogParent)Factory.New<DummyWithLogs>();

			descriptor.SetupGet(d => d.Name).Returns(documentName);

			logParent.Logs.AddNew(Events.MessageSent,
				new ZDateTimeOffset(2017, 6, 3),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			logParent.Logs.AddNew(Events.MessageAccepted,
				new ZDateTimeOffset(2017, 6, 1),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			logParent.Logs.AddNew(Events.Authorised,
				new ZDateTimeOffset(2017, 6, 2));

			logParent.Logs.AddNew(Events.MessageSent,
				new ZDateTimeOffset(2017, 6, 5),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));
			var info = new Document(descriptor.Object, logParent);

			var formattedEvents = info.Logs
				.Select(log => string.Format("{0}|{1}|{2}", log.Event.Code, log.EventTime, ((dynamic)log).MST));

			Assert(formattedEvents.SequenceEqual(new[]
				{
					"MSN|05-Jun-17 00:00:00|TestDocumentName",
					"MSN|03-Jun-17 00:00:00|TestDocumentName",
					"ATH|02-Jun-17 00:00:00|",
					"MAA|01-Jun-17 00:00:00|TestDocumentName"
				}));
		}

		const string documentName = "TestDocumentName";
		const string menuName = "TestMenuName";
	}
}
