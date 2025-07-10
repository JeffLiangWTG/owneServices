using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSAdjustmentReasons
	{
		CASSAdjustmentReasons() { }

		public static CodeDescriptionPairList GetReasonCodes(ZString reasonType)
		{
			CASSAdjustmentReasons adjustmentReasons = new CASSAdjustmentReasons();

			var result = new CodeDescriptionPairList();

			if (reasonType == CASSAdjustmentReasonTypes.ChargeBasis.Code)
			{
				result = adjustmentReasons.ChargeBasisAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.Commission.Code)
			{
				result = adjustmentReasons.CommissionAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.DisputedRebill.Code)
			{
				result = adjustmentReasons.DisputedRebillAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.DueAgent.Code)
			{
				result = adjustmentReasons.DueAgentAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.DueCarrier.Code)
			{
				result = adjustmentReasons.DueCarrierAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.General.Code)
			{
				result = adjustmentReasons.GeneralAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.Incentive.Code)
			{
				result = adjustmentReasons.IncentiveAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.Rate.Code)
			{
				result = adjustmentReasons.RateAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.Valuation.Code)
			{
				result = adjustmentReasons.ValuationAdjustmentReasons;
			}
			else if (reasonType == CASSAdjustmentReasonTypes.Weight.Code)
			{
				result = adjustmentReasons.WeightAdjustmentReasons;
			}
			return result;
		}

		CodeDescriptionPairList GeneralAdjustmentReasons
		{
			get
			{
				if (generalAdjustmentReasons == null)
				{
					generalAdjustmentReasons = new CodeDescriptionPairList();
					generalAdjustmentReasons.Add(new CodeDescriptionPair("1", (NoResString)"Billed to Wrong Office"));
					generalAdjustmentReasons.Add(new CodeDescriptionPair("2", (NoResString)"Billed to Wrong Agent"));
					generalAdjustmentReasons.Add(new CodeDescriptionPair("3", (NoResString)"Paid Previously"));
					generalAdjustmentReasons.Add(new CodeDescriptionPair("4", (NoResString)"Calculation Error"));
					generalAdjustmentReasons.Add(new CodeDescriptionPair("5", (NoResString)"Other"));
					generalAdjustmentReasons.Add(new CodeDescriptionPair("6", (NoResString)"VOID; Shipment Never Moved"));
					generalAdjustmentReasons.Add(new CodeDescriptionPair("7", (NoResString)"Domestic Shipment; Billed as International"));
				}
				return generalAdjustmentReasons;
			}
		}
		CodeDescriptionPairList generalAdjustmentReasons;

		CodeDescriptionPairList WeightAdjustmentReasons
		{
			get
			{
				if (weightAdjustmentReasons == null)
				{
					weightAdjustmentReasons = new CodeDescriptionPairList();
					weightAdjustmentReasons.Add(new CodeDescriptionPair("10", (NoResString)"Incorrect Gross Weight"));
					weightAdjustmentReasons.Add(new CodeDescriptionPair("11", (NoResString)"Incorrect Chargeable Weight"));
					weightAdjustmentReasons.Add(new CodeDescriptionPair("12", (NoResString)"Incorrect Volume Weight"));
				}
				return weightAdjustmentReasons;
			}
		}
		CodeDescriptionPairList weightAdjustmentReasons;

		CodeDescriptionPairList RateAdjustmentReasons
		{
			get
			{
				if (rateAdjustmentReasons == null)
				{
					rateAdjustmentReasons = new CodeDescriptionPairList();
					rateAdjustmentReasons.Add(new CodeDescriptionPair("20", (NoResString)"Incorrect Contract Rate"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("21", (NoResString)"Incorrect Spot or Ad-hoc Rate"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("22", (NoResString)"Incorrect Service Level Rate"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("23", (NoResString)"Incorrect Published Rate"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("24", (NoResString)"Contract Rate not Applied"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("25", (NoResString)"Spot/Ad-hoc Rate not Applied"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("26", (NoResString)"Service Level Rate not Applied"));
					rateAdjustmentReasons.Add(new CodeDescriptionPair("27", (NoResString)"Incorrect Pallet/Container Rate"));
				}
				return rateAdjustmentReasons;
			}
		}
		CodeDescriptionPairList rateAdjustmentReasons;

		CodeDescriptionPairList DueCarrierAdjustmentReasons
		{
			get
			{
				if (dueCarrierAdjustmentReasons == null)
				{
					dueCarrierAdjustmentReasons = new CodeDescriptionPairList();
					dueCarrierAdjustmentReasons.Add(new CodeDescriptionPair("30", (NoResString)"Incorrect Insurance Fee"));
					dueCarrierAdjustmentReasons.Add(new CodeDescriptionPair("31", (NoResString)"Incorrect Security Fee"));
					dueCarrierAdjustmentReasons.Add(new CodeDescriptionPair("32", (NoResString)"Incorrect Fuel Surcharge"));
					dueCarrierAdjustmentReasons.Add(new CodeDescriptionPair("33", (NoResString)"Incorrect DG/RA Fee"));
					dueCarrierAdjustmentReasons.Add(new CodeDescriptionPair("34", (NoResString)"Incorrect Other Due Carrier Fee"));
				}
				return dueCarrierAdjustmentReasons;
			}
		}
		CodeDescriptionPairList dueCarrierAdjustmentReasons;

		CodeDescriptionPairList DueAgentAdjustmentReasons
		{
			get
			{
				if (dueAgentAdjustmentReasons == null)
				{
					dueAgentAdjustmentReasons = new CodeDescriptionPairList();
					dueAgentAdjustmentReasons.Add(new CodeDescriptionPair("40", (NoResString)"Incorrect Due Agent Disbursement"));
					dueAgentAdjustmentReasons.Add(new CodeDescriptionPair("41", (NoResString)"Incorrect Due Agent Disbursement Applied"));
				}
				return dueAgentAdjustmentReasons;
			}
		}
		CodeDescriptionPairList dueAgentAdjustmentReasons;

		CodeDescriptionPairList ValuationAdjustmentReasons
		{
			get
			{
				if (valuationAdjustmentReasons == null)
				{
					valuationAdjustmentReasons = new CodeDescriptionPairList();
					valuationAdjustmentReasons.Add(new CodeDescriptionPair("50", (NoResString)"Incorrect Valuation Charge"));
					valuationAdjustmentReasons.Add(new CodeDescriptionPair("51", (NoResString)"Valuation Chg not Applied"));
				}
				return valuationAdjustmentReasons;
			}
		}
		CodeDescriptionPairList valuationAdjustmentReasons;

		CodeDescriptionPairList CommissionAdjustmentReasons
		{
			get
			{
				if (commissionAdjustmentReasons == null)
				{
					commissionAdjustmentReasons = new CodeDescriptionPairList();
					commissionAdjustmentReasons.Add(new CodeDescriptionPair("60", (NoResString)"Incorrect Commission"));
					commissionAdjustmentReasons.Add(new CodeDescriptionPair("61", (NoResString)"Commission not Applied"));
				}
				return commissionAdjustmentReasons;
			}
		}
		CodeDescriptionPairList commissionAdjustmentReasons;

		CodeDescriptionPairList IncentiveAdjustmentReasons
		{
			get
			{
				if (incentiveAdjustmentReasons == null)
				{
					incentiveAdjustmentReasons = new CodeDescriptionPairList();
					incentiveAdjustmentReasons.Add(new CodeDescriptionPair("70", (NoResString)"Incorrect Incentive"));
					incentiveAdjustmentReasons.Add(new CodeDescriptionPair("71", (NoResString)"Incentive not Applied"));
				}
				return incentiveAdjustmentReasons;
			}
		}
		CodeDescriptionPairList incentiveAdjustmentReasons;

		CodeDescriptionPairList ChargeBasisAdjustmentReasons
		{
			get
			{
				if (chargeBasisAdjustmentReasons == null)
				{
					chargeBasisAdjustmentReasons = new CodeDescriptionPairList();
					chargeBasisAdjustmentReasons.Add(new CodeDescriptionPair("80", (NoResString)"Charges Changed to Prepaid; Billed as Collect"));
					chargeBasisAdjustmentReasons.Add(new CodeDescriptionPair("81", (NoResString)"Charges Changed to Collect; Billed as Prepaid"));
				}
				return chargeBasisAdjustmentReasons;
			}
		}
		CodeDescriptionPairList chargeBasisAdjustmentReasons;

		CodeDescriptionPairList DisputedRebillAdjustmentReasons
		{
			get
			{
				if (disputedRebillAdjustmentReasons == null)
				{
					disputedRebillAdjustmentReasons = new CodeDescriptionPairList();
					disputedRebillAdjustmentReasons.Add(new CodeDescriptionPair("90", (NoResString)"Re-bill Removed Per Carrier"));
					disputedRebillAdjustmentReasons.Add(new CodeDescriptionPair("91", (NoResString)"Re-bill in Dispute Per Agent"));
					disputedRebillAdjustmentReasons.Add(new CodeDescriptionPair("93", (NoResString)"Stale Dated Invoice"));
				}
				return disputedRebillAdjustmentReasons;
			}
		}
		CodeDescriptionPairList disputedRebillAdjustmentReasons;
	}

	public sealed class CASSAdjustmentReasonTypes : CodeDescriptionPairList
	{
		public CASSAdjustmentReasonTypes()
		{
			Add(General);
			Add(Weight);
			Add(Rate);
			Add(DueCarrier);
			Add(DueAgent);
			Add(Valuation);
			Add(Commission);
			Add(Incentive);
			Add(ChargeBasis);
			Add(DisputedRebill);
		}

		public static CodeDescriptionPair General { get { return new CodeDescriptionPair("GEN", (NoResString)"General"); } }
		public static CodeDescriptionPair Weight { get { return new CodeDescriptionPair("WGT", (NoResString)"Weight"); } }
		public static CodeDescriptionPair Rate { get { return new CodeDescriptionPair("RAT", (NoResString)"Rate"); } }
		public static CodeDescriptionPair DueCarrier { get { return new CodeDescriptionPair("CAR", (NoResString)"Due Carrier"); } }
		public static CodeDescriptionPair DueAgent { get { return new CodeDescriptionPair("AGN", (NoResString)"Due Agent"); } }
		public static CodeDescriptionPair Valuation { get { return new CodeDescriptionPair("VAL", (NoResString)"Valuation"); } }
		public static CodeDescriptionPair Commission { get { return new CodeDescriptionPair("COM", (NoResString)"Commission"); } }
		public static CodeDescriptionPair Incentive { get { return new CodeDescriptionPair("INC", (NoResString)"Incentive"); } }
		public static CodeDescriptionPair ChargeBasis { get { return new CodeDescriptionPair("CRG", (NoResString)"Charge Basis"); } }
		public static CodeDescriptionPair DisputedRebill { get { return new CodeDescriptionPair("DRB", (NoResString)"Disputed Re-bill"); } }
	}

	public static class VATComponents
	{
		public const string VATDueAirline = "VAIR";
		public const string VATDueAgent = "VAGT";
	}
}
