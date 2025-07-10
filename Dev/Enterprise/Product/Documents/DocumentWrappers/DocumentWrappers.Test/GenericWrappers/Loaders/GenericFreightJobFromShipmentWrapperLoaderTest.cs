using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GenericFreightJobFromShipmentWrapperLoader))]
	sealed class GenericFreightJobFromShipmentWrapperLoaderTest : GenericWrapperLoaderBaseTest<GenericFreightJobFromShipmentWrapperLoader, FreightWrapperFromShipment>
	{
		public override BusinessObject GetBusinessObjectToWrap()
		{
			return Factory.New<ForwardingShipment>();
		}

		public override BusinessObject GetParentBusinessObjectToWrap()
		{
			return null;
		}

		public override BusinessObject GetChildBusinessObjectToWrap()
		{
			return Factory.New<ForwardingShipment>();
		}

		public override Core.Constants.DataContext[] SupportedDataContexts
		{
			get { return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJobFromShipment }; }
		}
	}
}
