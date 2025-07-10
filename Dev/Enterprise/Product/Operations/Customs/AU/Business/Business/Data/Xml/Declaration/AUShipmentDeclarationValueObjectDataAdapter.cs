using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUShipmentDeclarationValueObjectDataAdapter : ShipmentDeclarationValueObjectDataAdapter, Integration.Customs.AU.IAUShipmentDeclarationValueObjectDataAdapter
	{
		public AUShipmentDeclarationValueObjectDataAdapter()
		{
		}

		public AUShipmentDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		protected override BaseJobDeclaration GetJobDeclaration(IValueObjectImportContext context)
		{
			return context.Factory.New<JobDeclaration>();
		}
	}
}
