using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ArchiveManager.Test.Actions
{
	class DocumentSupporterForTest : DocumentSupporter
	{
		public DocumentSupporterForTest(BusinessObject businessObject)
			: base(businessObject)
		{ }

		public override BusinessContext BusinessContext
			=> BusinessContext.Test;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			=> throw new NotImplementedException();

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			=> dataContext == Core.Constants.DataContext.UnitTest
			? (new DocumentWrapper[] { new DummyWrapper() })
			: null;

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			=> new Core.Constants.DataContext[] { Core.Constants.DataContext.UnitTest };
	}
}
