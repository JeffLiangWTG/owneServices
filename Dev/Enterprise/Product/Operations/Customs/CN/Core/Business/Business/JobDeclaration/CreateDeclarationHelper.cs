using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.CN.ICreateDeclarationHelper
	{
		public override Customs.Business.ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment)
		{
			return new ImportJobDeclaration(shipment.Factory);
		}
	}
}
