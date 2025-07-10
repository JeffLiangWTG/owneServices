using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyConsolVisualizableDocumentSupporter : IVisualizableDocumentSupporter
	{
		public IDisposable Activate(IMessagingExtensions messagingExtensions)
		{
			messagingExtensionsOverride = messagingExtensions;
			return ObjectFactory.Substitute("ForwardingConsolVisualizableDocumentSupporter", this);
		}

		[ThreadStatic]
		static IMessagingExtensions messagingExtensionsOverride;

		public ISecurityCheckpoint CustomizeFormCheckpoint => null;
		public Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => (object)null;

		public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj) => bizObj;

		public IEnumerable<ICommand> GetCustomCommands(string dataContext) => CustomCommands;

		public static ICommand[] CustomCommands
		{
			get => customCommands;
			set => customCommands = value;
		}

		[ThreadStatic]
		static ICommand[] customCommands;

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters) => new object();
		public object GetEventParent(IXmlEventValueObject universalEvent) => null;
		public IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;
		public IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;
		public IMessageLogCreator GetMessageLogCreator(IDocument document) => null;
		public IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => messagingExtensionsOverride;

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified) => new Either<string, ITopLevelDataObject>((ITopLevelDataObject)null);

		public string GetMessageBroker() => string.Empty;

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions) => null;

		public bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
