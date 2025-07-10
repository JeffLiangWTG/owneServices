using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class ManagerDocumentSupporter : DocumentSupporter
	{
		public ManagerDocumentSupporter(OperationalActionManager parent)
			: base(parent) { }

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.OperationalActions; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { DocumentWrappers.DocActionManagerWrapper.New(Manager) };
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.OperationalActions };
		}

		#region Implementation

		OperationalActionManager Manager
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalActionManager)BusinessObject; }
		}

		#endregion
	}
}
