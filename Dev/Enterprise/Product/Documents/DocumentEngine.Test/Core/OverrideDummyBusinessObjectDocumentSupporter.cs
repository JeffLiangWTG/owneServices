using CargoWise.Definitions;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class OverrideDummyBusinessObjectDocumentSupporter : DocumentCommandTest.DummyBusinessObjectDocumentSupporter
	{
		public OverrideDummyBusinessObjectDocumentSupporter(DocDummyBusinessObjectWithOverrideSupporter docDummyBusinessObject)
			: base(docDummyBusinessObject)
		{
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			return System.Array.Empty<IDocumentSupportable>();
		}
	}
}
