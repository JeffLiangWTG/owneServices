using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter
{
	public interface IGatewayAdapter
	{
		AdapterResult ReceiveMessage(ITWCustomsRequest request);

		AdapterResult SendMessage(ITWCustomsRequest request);
	}
}
