using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.AU.ICreateDeclarationHelper
	{
		public override Customs.Business.ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment)
		{
			return new ImportJobDeclaration(shipment.Factory);
		}
	}
}
