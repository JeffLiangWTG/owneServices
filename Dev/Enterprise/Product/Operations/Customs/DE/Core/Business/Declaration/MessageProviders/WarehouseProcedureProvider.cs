using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class WarehouseProcedureProvider : IWarehouseProcedure
	{
		public static WarehouseProcedureProvider NewOrNull(PreviousDocument warehouseProcedure) => warehouseProcedure == null ? null : new WarehouseProcedureProvider(warehouseProcedure);
		public WarehouseProcedureProvider(PreviousDocument warehouseProcedure)
		{
			this.warehouseProcedure = warehouseProcedure;
		}
		readonly PreviousDocument warehouseProcedure;

		public string AccessViaAtlasFlag => warehouseProcedure.Status.MapBoolTo10();

		public string MRN => warehouseProcedure.Status && warehouseProcedure.CSI_ReferenceNumber.Length == 18 ? warehouseProcedure.CSI_ReferenceNumber.ToString() : string.Empty;

		public string RegistrationNumber => warehouseProcedure.CSI_ReferenceNumber;

		public int ReferencedSequenceNumber => warehouseProcedure.CSI_LineNo;

		public string UsualProcessingFlag => warehouseProcedure.UsualProcessingFlag.MapBoolTo10();

		public string Complement => warehouseProcedure.CSI_Description;

		public string HarmonizedSystemSubHeadingCode => warehouseProcedure.CSI_Tariff.SubstringSafe(0, 6);

		public string CombinedNomenclatureCode => warehouseProcedure.CSI_Tariff.SubstringSafe(6, 2);

		public string TaricCode => warehouseProcedure.CSI_Tariff.SubstringSafe(8, 2);

		public string NationalAdditionalCode => warehouseProcedure.CSI_Tariff.SubstringSafe(10, 1);

		public IAmount DebitAmount => CachedValueHelper.GetValue(ref debitAmount, () => new AmountProvider(warehouseProcedure.CSI_Quantity2, warehouseProcedure.CSI_UnitOfQuantity2));
		CachedValue<IAmount> debitAmount;

		public IAmount CommercialAmount => CachedValueHelper.GetValue(ref commercialAmount, () => warehouseProcedure.UsualProcessingFlag ? new AmountProvider(warehouseProcedure.CSI_Quantity, warehouseProcedure.CSI_UnitOfQuantity) : null);
		CachedValue<IAmount> commercialAmount;

		public string CommodityCode => warehouseProcedure.CSI_Tariff;
	}
}
