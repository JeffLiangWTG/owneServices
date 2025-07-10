using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.Customs.Business.Documents.DocDataObjects;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Business.Documents.DocDataObjects
{
	sealed class JobDeclarationDEVisualizableDocumentSupporter : CustomsVisualizableDocumentSupporter<JobDeclaration>
	{
		public JobDeclarationDEVisualizableDocumentSupporter(JobDeclaration parent) : base(parent)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainJobDeclarationCustomiseForms;

		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem) => (object)null;

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName) => null;

		protected override string GetDocProviderKey() => Core.Constants.CountryCodes.Germany;
	}
}
