using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.APL.Testing
{
	[TestedType(typeof(APLDocAWB))]
	public class APLDocAWBTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var doc = (APLDocAWB)APLDocAWB.New(shipment.AWBHeader, Factory);
			return new DocumentWrapper[] { doc };
		}
	}
}
