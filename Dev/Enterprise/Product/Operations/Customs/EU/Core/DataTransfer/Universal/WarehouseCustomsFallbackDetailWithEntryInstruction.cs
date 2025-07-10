using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using BaseWarehouseCustomsFallbackDetailWithEntryInstruction = Enterprise.Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseCustomsFallbackDetailWithEntryInstruction : BaseWarehouseCustomsFallbackDetailWithEntryInstruction
	{
		public ZString LinePriceCurrency;
		public ZString CountryCode;

		public static WarehouseCustomsFallbackDetailWithEntryInstruction CloneFrom(BaseWarehouseCustomsFallbackDetailWithEntryInstruction source)
		{
			return new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				IsExport = source.IsExport,
				IsExWarehouse = source.IsExWarehouse,
				SupplierAddress = source.SupplierAddress,
				InvoiceLineAddInfosApplicableForInwardWarehousing = source.InvoiceLineAddInfosApplicableForInwardWarehousing,
				EntryInstructionProcedureMap = source.EntryInstructionProcedureMap,
				EntryInstructionEntryHeaderMap = source.EntryInstructionEntryHeaderMap,
				FallbackAddInfos = source.FallbackAddInfos
			};
		}

		protected override WarehouseCustomsFallbackDetail CloneCore()
		{
			var fallbackDetail = CloneFrom(this);
			fallbackDetail.LinePriceCurrency = this.LinePriceCurrency;
			fallbackDetail.CountryCode = this.CountryCode;
			return fallbackDetail;
		}
	}
}
