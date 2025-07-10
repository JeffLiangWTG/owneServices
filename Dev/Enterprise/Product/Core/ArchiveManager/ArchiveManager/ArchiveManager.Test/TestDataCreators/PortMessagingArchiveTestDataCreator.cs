using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	sealed class PortMessagingArchiveTestDataCreator : IPortMessagingTestDataCreator
	{
		public void CreatePackLineData(ZGuid pk)
		{
			var factory = new BusinessObjectFactory();
			var packLine = factory.New<ForwardingPackLine>();
			packLine.JL_JS = pk;
			var packLinePortMessaging = factory.New<PackLinePortMessaging>();
			packLinePortMessaging.JLM_JL_PackLine = packLine.PK;
			packLinePortMessaging.JLM_MovementReferenceNumber = "TOENABLESAVE";
			factory.Save();
		}
	}
}
