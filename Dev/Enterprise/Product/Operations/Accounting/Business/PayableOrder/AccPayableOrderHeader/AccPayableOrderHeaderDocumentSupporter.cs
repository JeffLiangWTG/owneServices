using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderHeaderDocumentSupporter : DocumentSupporter
	{
		public AccPayableOrderHeaderDocumentSupporter(AccPayableOrderHeader order)
			: base(order)
		{
		}

		protected AccPayableOrderHeader Order
		{
			get { return (AccPayableOrderHeader)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.PayableOrder; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Order);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper[] result = System.Array.Empty<DocumentWrapper>();

			switch (dataContext)
			{
				case Core.Constants.DataContext.PayableOrder:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.PayableOrder, Order) };
					break;
				default:
					break;
			}

			return result;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.PayableOrder,
				Core.Constants.DataContext.GenericFreightJob,
			};
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.OrderTrackingCustomiseDocuments; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(Order.Supplier, null);
		}
	}
}
