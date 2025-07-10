using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.CA.ICreateDeclarationHelper
	{
		public override Customs.Business.ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment)
		{
			return new ImportJobDeclaration(shipment.Factory);
		}
	}
}
