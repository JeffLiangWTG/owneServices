using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUShipmentDeclarationGenerator : ShipmentDeclarationGenerator, Integration.Customs.AU.IAUShipmentDeclarationGenerator
	{
		protected override InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration)
		{
			return new AUInvoicesGeneratorFromXSD(declaration);
		}
	}
}
