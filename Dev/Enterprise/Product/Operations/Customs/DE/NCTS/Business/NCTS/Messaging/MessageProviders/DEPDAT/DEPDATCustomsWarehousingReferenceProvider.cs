using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DEPDATCustomsWarehousingReferenceProvider : IDEPDATCustomsWarehousingReference
	{
		public static IDEPDATCustomsWarehousingReference NewOrNull(NctsPreviousDocument previousProcedure) => previousProcedure != null ? new DEPDATCustomsWarehousingReferenceProvider(previousProcedure) : null;

		DEPDATCustomsWarehousingReferenceProvider(NctsPreviousDocument previousProcedure)
		{
			this.previousProcedure = Argument.NotNull(previousProcedure, nameof(previousProcedure));
		}

		public bool AccessViaATLAS => previousProcedure.Status;

		public string MRN => NCTSProviderHelpers.IsDEMRN(previousProcedure.CSI_ReferenceNumber) ? (string)previousProcedure.CSI_ReferenceNumber : null;

		public string RegistrationNumber => !NCTSProviderHelpers.IsDEMRN(previousProcedure.CSI_ReferenceNumber) ? (string)previousProcedure.CSI_ReferenceNumber : null;

		public int GoodsItemNumber => previousProcedure.CSI_LineNo;

		public bool UsualTreatment => previousProcedure.UsualProcessingFlag;

		public string Complement => previousProcedure.CSI_Description;

		public string HarmonizedSystemSubheadingCode => previousProcedure.CSI_Tariff.Left(6);

		public string CombinedNomenclatureCode => previousProcedure.CSI_Tariff.SubstringSafe(6, 2);

		public string TaricCode => previousProcedure.CSI_Tariff.SubstringSafe(8, 2);

		public string NationalAdditionalCode => previousProcedure.CSI_Tariff.SubstringSafe(10, 1);

		public IAmount GoodsReduction => goodsReduction ?? (goodsReduction = new AmountProvider(previousProcedure.CSI_Quantity2.Round(3).Normalize(), previousProcedure.CSI_UnitOfQuantity2));
		IAmount goodsReduction;

		public IAmount GoodsReductionAfterTreatment => CachedValueHelper.GetValue(ref goodsReductionAfterTreatmentCached,
						() => previousProcedure.UsualProcessingFlag ? new AmountProvider(previousProcedure.CSI_Quantity.Round(3).Normalize(), previousProcedure.CSI_UnitOfQuantity) : null);
		CachedValue<IAmount> goodsReductionAfterTreatmentCached;

		readonly NctsPreviousDocument previousProcedure;
	}
}
