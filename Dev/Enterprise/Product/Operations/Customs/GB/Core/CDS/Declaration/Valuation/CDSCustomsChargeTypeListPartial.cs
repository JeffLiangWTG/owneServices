using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSCustomsChargeTypeList : EU.Business.UCCCustomsChargeTypeList
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string MapToCDSAdditionDeductionChargeType(ZString chargeType, bool isDutiable, ZString distributeBy, bool isIncluded)
		{
			var result = string.Empty;
			switch (chargeType)
			{
				case Codes.CommissionAndBrokerageCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AB;
					break;
				case Codes.ContainersAndPackingCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AD;
					break;
				case Codes.MaterialsComponentsPartsCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AE;
					break;
				case Codes.ToolsMiesMouldsCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AF;
					break;
				case Codes.MaterialsConsumedCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AG;
					break;
				case Codes.EngineeringDevelopmentArtworkCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AH;
					break;
				case Codes.RoyaltiesLicenseFeeCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AI;
					break;
				case Codes.ProceedsOfAnySubsequentResaleCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AJ;
					break;
				case Codes.InsuranceCostsCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AK;
					break;
				case Codes.IndirectAndOtherPaymentsCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AL;
					break;
				case Codes.Additions71Charge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AN;
					break;
				case Codes.TransportCostsCharge:
					result = MapTransportCostsCharge(distributeBy, isIncluded);
					break;
				case Codes.OtherNotElsewhereDeclaredCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.AT;
					break;
				case Codes.AdjustmentCharge:
					result = distributeBy == ChargeDistributeByList.Codes.Value ? CDSAdditionDeductionChargeTypeList.Codes.AV : CDSAdditionDeductionChargeTypeList.Codes.AW;
					break;
				case Codes.AirTransportCostsCharge:
					result = MapAirTransportCostsCharge(distributeBy, isIncluded);
					break;
				case Codes.ConstructionErectionAssemblyCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BB;
					break;
				case Codes.InterestCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BD;
					break;
				case Codes.RightToReproduceCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BE;
					break;
				case Codes.BuyingCommissionsCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BM;
					break;
				case Codes.Deductions71Charge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BG;
					break;
				case Codes.DiscountNotElsewhereDeclaredCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BH;
					break;
				case Codes.DeductionsNotElsewhereDeclaredCharge:
					result = CDSAdditionDeductionChargeTypeList.Codes.BT;
					break;
			}

			return result;
		}

		static string MapAirTransportCostsCharge(ZString distributeBy, bool isIncluded)
		{
			var result = string.Empty;

			if (!isIncluded)
			{
				result = distributeBy == ChargeDistributeByList.Codes.Value ? CDSAdditionDeductionChargeTypeList.Codes.AR
					   : distributeBy == ChargeDistributeByList.Codes.Weight ? CDSAdditionDeductionChargeTypeList.Codes.AS
					   : string.Empty;
			}
			else
			{
				result = distributeBy == ChargeDistributeByList.Codes.Value ? CDSAdditionDeductionChargeTypeList.Codes.BR
					   : distributeBy == ChargeDistributeByList.Codes.Weight ? CDSAdditionDeductionChargeTypeList.Codes.BS
					   : string.Empty;
			}

			return result;
		}

		static string MapTransportCostsCharge(ZString distributeBy, bool isIncluded)
		{
			var result = string.Empty;

			if (!isIncluded)
			{
				result = distributeBy == ChargeDistributeByList.Codes.Value ? CDSAdditionDeductionChargeTypeList.Codes.AP
					   : distributeBy == ChargeDistributeByList.Codes.Weight ? CDSAdditionDeductionChargeTypeList.Codes.AQ
					   : string.Empty;
			}
			else
			{
				result = distributeBy == ChargeDistributeByList.Codes.Value ? CDSAdditionDeductionChargeTypeList.Codes.BA
					   : distributeBy == ChargeDistributeByList.Codes.Weight ? CDSAdditionDeductionChargeTypeList.Codes.BU
					   : string.Empty;
			}

			return result;
		}
	}
}
