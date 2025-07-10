using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.Customs.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business.Documents.DocDataObjects
{
	public class NctsHeaderVisualizableDocumentSupporter : CustomsVisualizableDocumentSupporter<NctsHeader>
	{
		public NctsHeaderVisualizableDocumentSupporter(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.EuNcts;

		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem) => (object)null;

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(DocumentVisualizer.Core.IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(DocumentVisualizer.Core.IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(DocumentVisualizer.Core.IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName) => string.Empty;

		protected override string GetDocProviderKey() => "NCTS";
	}
}
