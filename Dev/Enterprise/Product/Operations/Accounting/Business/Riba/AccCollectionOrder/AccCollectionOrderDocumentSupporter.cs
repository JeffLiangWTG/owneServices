using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderDocumentSupporter : DocumentSupporter
	{
		public AccCollectionOrderDocumentSupporter(AccCollectionOrder collectionOrder)
			: base(collectionOrder)
		{
		}

		protected AccCollectionOrder CollectionOrder
		{
			get { return (AccCollectionOrder)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CollectionOrder; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { Enterprise.Core.Constants.DataContext.CollectionOrder };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.CollectionOrder)
			{
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateAccountingWrapper(DataContext.CollectionOrder, CollectionOrder) };
			}
			else
			{
				return null;
			}
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(CollectionOrder.Debtor, null);
		}

		#endregion
	}
}