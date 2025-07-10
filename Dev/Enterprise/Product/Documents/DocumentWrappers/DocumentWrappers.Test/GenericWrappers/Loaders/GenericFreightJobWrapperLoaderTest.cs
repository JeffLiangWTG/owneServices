using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GenericFreightJobWrapperLoader))]
	sealed class GenericFreightJobWrapperLoaderTest : GenericWrapperLoaderBaseTest<GenericFreightJobWrapperLoader, FreightWrapper>
	{
		public override BusinessObject GetBusinessObjectToWrap()
		{
			return Factory.New<ForwardingShipment>();
		}

		public override BusinessObject GetParentBusinessObjectToWrap()
		{
			return Factory.New<ForwardingConsol>();
		}

		public override BusinessObject GetChildBusinessObjectToWrap()
		{
			return Factory.New<ForwardingShipment>();
		}

		public override Core.Constants.DataContext[] SupportedDataContexts
		{
			get
			{
				return new Core.Constants.DataContext[]
				{
					Core.Constants.DataContext.GenericFreightJob,
					Core.Constants.DataContext.GenericFreightJobRouting,
					Core.Constants.DataContext.GenericFreightJobByContainerIfFCL,
					Core.Constants.DataContext.GenericPickupDeliveryConfirm,
					Core.Constants.DataContext.GenericLocalTransportLeg,
					Core.Constants.DataContext.GenericChargeSheet,
					Core.Constants.DataContext.GenericFreightJobByContainer,
					Core.Constants.DataContext.GenericFreightJobInvoice,
				};
			}
		}
	}
}
