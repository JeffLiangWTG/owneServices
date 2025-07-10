using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromGateTransport))]
	sealed class FreightWrapperFromGateTransportTest : FreightWrapperTest
	{
		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var gateTransport = Factory.New<GateTransport>();
			var gatetransportCYDetail = gateTransport.GateTransportCYDetails.AddNew();
			return gatetransportCYDetail;
		}
	}
}
