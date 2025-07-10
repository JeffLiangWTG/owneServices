using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.NCTS.DataTransfer;

public class DepartureGoodsItemFeeDataObjectWriter : DataObjectWriter<NctsCargoDescFee, TaxOrFee>
{
	public DepartureGoodsItemFeeDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
	{
	}

	protected override TaxOrFee PopulateDataObject(NctsCargoDescFee cargoDescFee)
	{
		return new TaxOrFee()
		{
			Amount = cargoDescFee.BFE_ChargeAmount,
			BaseValue = cargoDescFee.BFE_BaseValue,
			MethodOfCalculation = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(cargoDescFee.BFE_MethodOfCalculation, cargoDescFee.ITLookups.MethodOfCalculationList),
			MethodOfPayment = ListHelper.GetWithDescription<CodeDescriptionPair>(cargoDescFee.BFE_MethodOfPayment, cargoDescFee.ITLookups.MethodOfPaymentList),
			RateReasonOverride = ListHelper.GetWithDescription<CodeDescriptionPair>(cargoDescFee.BFE_RateOverrideReasonCode, cargoDescFee.ITLookups.RateOverrideReasonList),
			Type = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(cargoDescFee.BFE_ChargeType, cargoDescFee.Lookups.ChargeTypeList),
		};
	}
}
