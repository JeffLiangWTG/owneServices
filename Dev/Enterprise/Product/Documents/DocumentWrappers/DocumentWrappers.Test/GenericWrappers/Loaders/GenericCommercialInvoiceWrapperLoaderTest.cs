using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GenericCommercialInvoiceWrapperLoader))]
	sealed class GenericCommercialInvoiceWrapperLoaderTest : GenericWrapperLoaderBaseTest<GenericCommercialInvoiceWrapperLoader, CommercialInvoiceWrapper>
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
			return Factory.New<BaseJobDeclaration>();
		}

		public override Core.Constants.DataContext[] SupportedDataContexts
		{
			get { return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericCommercialInvoice }; }
		}
	}
}
