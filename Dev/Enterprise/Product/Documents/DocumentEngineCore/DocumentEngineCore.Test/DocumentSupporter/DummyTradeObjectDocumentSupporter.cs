using System;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DummyTradeObjectDocumentSupporter : DocumentSupporter
	{
		public DummyTradeObjectDocumentSupporter(DummyTradeObject parent)
			: base(parent)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new[] { new DocumentWrapperForTesting() };
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { DataContext.Dummy };
		}

		#region Overrides Not Implemented
		public override BusinessContext BusinessContext
		{
			get { throw new Exception("The method or operation is not implemented for this test."); }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { throw new Exception("The method or operation is not implemented for this test."); }
		}
		#endregion
	}
}
