using CargoWise.Definitions;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyDocManagerTestBizODocumentSupporter : DocumentSupporter
	{
		public DummyDocManagerTestBizODocumentSupporter(DummyDocManagerTestBizO dummyDocManagerTestBizO)
			: base(dummyDocManagerTestBizO)
		{
		}

		protected override Enterprise.Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return null;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContextOverride ?? new BusinessContext(); }
		}

		internal BusinessContext? BusinessContextOverride { get; set; }

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return null; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { new DocumentCommandTest.DocDummyBusinessObject.DummyWrapper() };
		}
	}
}
