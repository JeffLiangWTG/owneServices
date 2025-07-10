using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.Universal;
using NctsDepartureCargoDesc = Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class HarbourFeeDepartureMovementCalculationManager
	{
		readonly BusinessObjectFactory factory;

		public HarbourFeeDepartureMovementCalculationManager(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		public void Calculate(NctsDepartureMovementHeader movementHeader)
		{
			if (movementHeader != null)
			{
				var parent = movementHeader.Header;

				foreach (var desc in parent.DepartureGoodsItems.Cast<NctsDepartureCargoDesc>())
				{
					ClearnctsCargoDescFeeSystemCalculatedStashingUserEnteredValue(desc);
				}

				var firstGoodItem = parent.DepartureGoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1);

				if (firstGoodItem != null)
				{
					var chargePaymentOrDestinationID = movementHeader.ChargePaymentOrDestinationID;
					if (!chargePaymentOrDestinationID.IsEmpty)
					{
						var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
						var bizObj = (Integration.Customs.FR.IHarbourJob)movementHeader;
						var harbourRates = new RefHarbourRate.Loader(factory).Load(bizObj.HarbourType, bizObj.DataGrouping, bizObj.ContainerMode, bizObj.ValuationDate, chargePaymentOrDestinationID, bizObj.IsContainerised);

						foreach (var harbourRate in harbourRates)
						{
							var rateData = new HarbourFeeUniversalRateData(wrapper, harbourRate.ZXF_RateFormula);
							var fee = rateData.ValueForDuty;

							if (fee > 0)
							{
								var harbourFeeCode = UniversalReferenceDataHelper.GetLocalHarbourFeeCode(factory, movementHeader, harbourRate, movementHeader.ValuationDate);
								if (!EntryLineHasFeeForThisCode(firstGoodItem, harbourFeeCode))
								{
									var newFee = firstGoodItem.Fees.AddNew();
									newFee.BFE_ChargeType = harbourFeeCode;
									newFee.BFE_ChargeAmount = fee;
								}
							}
						}
					}
				}
			}
		}

		bool EntryLineHasFeeForThisCode(NctsDepartureCargoDesc desc, string rateCode)
		{
			return desc.Fees.Cast<NctsCargoDescFee>().Any(f => f.BFE_ChargeType == rateCode && f.BFE_RateOverrideReasonCode == EU.Business.RateOverrideReasonList.Codes.Override);
		}

		void ClearnctsCargoDescFeeSystemCalculatedStashingUserEnteredValue(NctsDepartureCargoDesc desc)
		{
			foreach (var fee in desc.Fees.Cast<NctsCargoDescFee>().Where(f => f.BFE_RateOverrideReasonCode.IsEmpty && new HarbourFeeCodes().ContainsCode(f.BFE_ChargeType)).ToList())
			{
				desc.Fees.Delete(fee);
			}
		}
	}
}
