using System.Linq;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DE.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class WarehouseCustomsDetailsChangeOfRegime : WarehouseCustomsDetailsChangeOfRegimeBase
	{
		public WarehouseCustomsDetailsChangeOfRegime(Shipment shipment) : base(shipment)
		{
		}

		public override CustomsRegime IntoRegimeType =>
			Shipment.EntryInstructionCollection?.FirstOrDefault() is EntryInstruction entryInstruction && entryInstruction.Procedure.GetValueOrDefault() == ImportMainProcedureCodeList.Codes._51
				? CustomsRegime.InwardProcessing
				: CustomsRegime.BondedWarehouse;
	}
}
