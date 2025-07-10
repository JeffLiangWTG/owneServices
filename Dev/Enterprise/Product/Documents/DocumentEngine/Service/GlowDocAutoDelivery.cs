using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine.Service
{
	public class GlowDocAutoDelivery : DocAutoDelivery
	{
		public GlowDocAutoDelivery(IStmMenuItem menuItem, DocumentSupporter documentSupporter)
		{
			InitialiseDeliveryDetails(menuItem, documentSupporter, null, null);
		}
	}
}
