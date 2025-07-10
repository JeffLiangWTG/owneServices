using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionBatchDocumentSupporter : DocumentSupporter
	{
		public AccCollectionBatchDocumentSupporter(AccCollectionBatch collectionBatch)
			: base(collectionBatch)
		{
		}

		protected AccCollectionBatch CollectionBatch
		{
			get { return (AccCollectionBatch)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CollectionBatch; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.CollectionBatch };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		#endregion
	}
}

