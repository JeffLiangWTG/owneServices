using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyWithUXmlSupportDocumentSupporter : DocumentSupporter
	{
		public DummyWithUXmlSupportDocumentSupporter(DummyWithUXmlSupport dummy)
			: base(dummy)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Test; }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		protected override Enterprise.Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Enterprise.Core.Constants.DataContext.UnitTest
			};
		}
	}
}