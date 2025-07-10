using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class WarehouseCustomsDetailsChangeOfRegime : WarehouseCustomsDetailsChangeOfRegimeBase
	{
		public WarehouseCustomsDetailsChangeOfRegime(Shipment shipment) : base(shipment)
		{
		}

		public override CustomsRegime IntoRegimeType =>
			Shipment.EntryInstructionCollection?.FirstOrDefault() is EntryInstruction entryInstruction && entryInstruction.Style.GetValueOrDefault() == DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing
				? CustomsRegime.InwardProcessing
				: CustomsRegime.BondedWarehouse;

		protected override ZString GetEntryInstructionProcedure(EntryInstruction entryInstruction) => entryInstruction.Style.GetValueOrDefault();
	}
}
