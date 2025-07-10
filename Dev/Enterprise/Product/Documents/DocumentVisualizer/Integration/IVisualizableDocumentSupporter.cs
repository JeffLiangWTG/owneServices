using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IVisualizableDocumentSupporter
	{
		object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj);

		object GetEventParent(IXmlEventValueObject universalEvent);

		ISecurityCheckpoint CustomizeFormCheckpoint { get; }

		IMessageEventsProcessor GetMessageEventsProcessor(IDocument document);
		IMessageLogCreator GetMessageLogCreator(IDocument document);
		IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions);
		IEnumerable<IMacroLibrary> GetLibraries(string dataContext);

		IEnumerable<ICommand> GetCustomCommands(string dataContext);

		Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters);
		Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem);
		Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified);

		string GetMessageBroker();

		IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions);

		bool ShouldUseDraftWatermark(IDocument document);
	}
}
