using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[DebuggerDisplay("RecordType:{RecordType}, PWC:{WeightChargePP}, PVC:{ValuationChargePP}, PCC:{ChargesDueCarrierPP}, COA:{ChargesDueAgentCC}, COM:{commission}, DOI:{Discount}, VATAirline:{VATDueAirline}, VATAgent:{VATDueAgent}")]
	public class CASSCostExportLine : CASSCostLine
	{
		public CASSCostExportLine(BusinessObjectFactory factory, CASSCostLineType lineType)
			: base(factory, lineType)
		{
		}

		#region Properties

		[ReadOnly(true)]
		public ZString CCADCMNumber
		{
			get { return ccadcmNumber; }
			set { SetNonPersistentPropertyValue(CCADCMNumberInfo, ref ccadcmNumber, value); }
		}
		ZString ccadcmNumber;

		public ZPropertyInfo CCADCMNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CCADCMNumber));
			}
		}

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal WeightChargePP
		{
			get { return weightChargePP; }
			set { SetComponentAmount(WeightChargePPInfo, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ref weightChargePP, value); }
		}
		ZDecimal weightChargePP;

		public ZPropertyInfo WeightChargePPInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(WeightChargePP));
			}
		}

		public bool WeightChargePP_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal ValuationChargePP
		{
			get { return valuationChargePP; }
			set { SetComponentAmount(ValuationChargePPInfo, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ref valuationChargePP, value); }
		}
		ZDecimal valuationChargePP;

		public ZPropertyInfo ValuationChargePPInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ValuationChargePP));
			}
		}

		public bool ValuationChargePP_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal ChargesDueCarrierPP
		{
			get { return chargesDueCarrierPP; }
			set { SetComponentAmount(ChargesDueCarrierPPInfo, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ref chargesDueCarrierPP, value); }
		}
		ZDecimal chargesDueCarrierPP;

		public ZPropertyInfo ChargesDueCarrierPPInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChargesDueCarrierPP));
			}
		}

		public bool ChargesDueCarrierPP_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal ChargesDueAgentCC
		{
			get { return chargesDueAgentCC; }
			set { SetComponentAmount(ChargesDueAgentCCInfo, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ref chargesDueAgentCC, value); }
		}
		ZDecimal chargesDueAgentCC;

		public ZPropertyInfo ChargesDueAgentCCInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChargesDueAgentCC));
			}
		}

		public bool ChargesDueAgentCC_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal Commission
		{
			get { return commission; }
			set { SetComponentAmount(CommissionInfo, CASSChargeCodeLookups.CASSComponents.Commission.Code, ref commission, value); }
		}
		ZDecimal commission;

		public ZPropertyInfo CommissionInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Commission));
			}
		}

		public bool Commission_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal Discount
		{
			get { return discount; }
			set { SetComponentAmount(DiscountInfo, CASSChargeCodeLookups.CASSComponents.Discount.Code, ref discount, value); }
		}
		ZDecimal discount;

		public ZPropertyInfo DiscountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Discount));
			}
		}

		public bool Discount_ReadOnly { get { return DenyCASSCostEditing; } }

		[List("AdjustmentReasonTypeList")]
		public ZString AdjustmentReasonType
		{
			get { return adjustmentReasonType; }
			set { SetNonPersistentPropertyValue(AdjustmentReasonTypeInfo, ref adjustmentReasonType, value); }
		}
		ZString adjustmentReasonType;

		public ZPropertyInfo AdjustmentReasonTypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AdjustmentReasonType));
			}
		}

		public bool AdjustmentReasonType_ReadOnly { get { return DenyCASSCostEditing; } }

		[List("AdjustmentReasonList")]
		[MaxLength(2)]
		public ZString AdjustmentReason
		{
			get { return adjustmentReason; }
			set
			{
				SetNonPersistentPropertyValue(AdjustmentReasonInfo, ref adjustmentReason, value);
				ValidateReasonCode();
			}
		}
		ZString adjustmentReason;

		public ZPropertyInfo AdjustmentReasonInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AdjustmentReason));
			}
		}

		public bool AdjustmentReason_ReadOnly { get { return DenyCASSCostEditing; } }

		[ReadOnly(false)]
		[MaxLength(215)]
		public ZString AdjustmentReasonComment
		{
			get { return adjustmentReasonComment; }
			set
			{
				SetNonPersistentPropertyValue(AdjustmentReasonCommentInfo, ref adjustmentReasonComment, value);
				ValidateAdjustmentComment();
			}
		}
		ZString adjustmentReasonComment;

		public ZPropertyInfo AdjustmentReasonCommentInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AdjustmentReasonComment));
			}
		}

		public bool AdjustmentReasonComment_ReadOnly { get { return DenyCASSCostEditing; } }

		[ReadOnly(true)]
		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		public ZDecimal VATDueAirline
		{
			get { return vATDueAirline; }
			set { SetComponentAmount(VATDueAirlineInfo, VATAirline, ref vATDueAirline, value); }
		}
		ZDecimal vATDueAirline;

		public ZPropertyInfo VATDueAirlineInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(VATDueAirline));
			}
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		public ZDecimal VATDueAgent
		{
			get { return vATDueAgent; }
			set { SetComponentAmount(VATDueAgentInfo, VATAgent, ref vATDueAgent, value); }
		}
		ZDecimal vATDueAgent;

		public ZPropertyInfo VATDueAgentInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(VATDueAgent));
			}
		}

		public ZDecimal WeightChargePP_Original { get; private set; }
		public ZDecimal ValuationChargePP_Original { get; private set; }
		public ZDecimal ChargesDueCarrierPP_Original { get; private set; }
		public ZDecimal ChargesDueAgentCC_Original { get; private set; }
		public ZDecimal Commission_Original { get; private set; }
		public ZDecimal Discount_Original { get; private set; }
		public ZString AdjustmentReasonType_Original { get; private set; }
		public ZString AdjustmentReason_Original { get; private set; }
		public ZString AdjustmentReasonComment_Original { get; private set; }

		internal override ZDecimal VATAmountCore
		{
			get
			{
				return AggregateComponentAmount(CassVATComponentAmounts, IsAdjustmentRecord);
			}
		}

		public CodeDescriptionPairList AdjustmentReasonList
		{
			get { return CASSAdjustmentReasons.GetReasonCodes(AdjustmentReasonType); }
		}

		public CASSAdjustmentReasonTypes AdjustmentReasonTypeList
		{
			get { return new CASSAdjustmentReasonTypes(); }
		}

		bool DoAllVATComponentsHaveNonZeroValue
		{
			get
			{
				Func<string, bool> hasValue = (x) =>
				{
					var component = CassVATComponentAmounts.FirstOrDefault(y => y.ComponentName == x);
					return component != null && (IsAdjustmentRecord ? component.AdjustedAmount : component.Amount) != 0;
				};

				return VATComponentList.GetAllCodes().All(x => hasValue(x));
			}
		}

		#endregion

		#region Constants

		public const string VATAirline = "VATDueAirline";
		public const string VATAgent = "VATDueAgent";

		#endregion

		#region Overridden Functions

		protected override void UpdateOriginalAmountsCore()
		{
			WeightChargePP_Original = WeightChargePP;
			ValuationChargePP_Original = ValuationChargePP;
			ChargesDueCarrierPP_Original = ChargesDueCarrierPP;
			ChargesDueAgentCC_Original = ChargesDueAgentCC;
			Commission_Original = Commission;
			Discount_Original = Discount;

			AdjustmentReasonType_Original = AdjustmentReasonType;
			AdjustmentReason_Original = AdjustmentReason;
			AdjustmentReasonComment_Original = AdjustmentReasonComment;

			base.UpdateOriginalAmountsCore();
		}

		protected override bool HasAnyAmountChangedCore()
		{
			return (WeightChargePP != WeightChargePP_Original
					|| ValuationChargePP != ValuationChargePP_Original
					|| ChargesDueCarrierPP != ChargesDueCarrierPP_Original
					|| ChargesDueAgentCC != ChargesDueAgentCC_Original
					|| Commission != Commission_Original
					|| Discount != Discount_Original
					|| AdjustmentReasonType != AdjustmentReasonType_Original
					|| AdjustmentReason != AdjustmentReason_Original
					|| AdjustmentReasonComment != AdjustmentReasonComment_Original);
		}

		protected override CASSCostLine CreateNewInstanceCore(CASSCostLineType lnType)
		{
			return new CASSCostExportLine(Factory, lnType);
		}

		protected override CodeDescriptionPairList GetCostComponentListCore()
		{
			return CASSChargeCodeRegistryExtractor.CASSExportCostComponents;
		}

		protected override CodeDescriptionPairList GetVATComponentListCore()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(VATAirline);
			result.AddPair(VATAgent);
			return result;
		}

		protected override CodeDescriptionPairList GetCostComponentListByVATComponentCore(string vatComponentName)
		{
			var result = new CodeDescriptionPairList();

			if ((DoAllVATComponentsHaveNonZeroValue && !RecordType.IsEmpty))
			{
				switch (vatComponentName)
				{
					case VATAirline:
						result.AddRange(CostCompoentListForVATAirline);
						break;

					case VATAgent:
						result.AddRange(CostCompoentListForVATAgent);
						break;

					default:
						break;
				}
			}
			else
			{
				result.AddRange(CASSChargeCodeRegistryExtractor.CASSExportCostComponents);
			}

			return result;
		}

		protected override ZGuid[] GetCASSChargeCodePKsFromRegistryCore(string cassComponent = "")
		{
			if (!string.IsNullOrEmpty(cassComponent))
			{
				return CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Export.Code, cassComponent);
			}
			else
			{
				return CASSChargeCodeRegistryExtractor.GetAllChargeCodePksByCASSType(CASSChargeCodeLookups.CASSTypes.Export.Code);
			}
		}

		protected override int GetComponentAmountMultiplierCore(ZString componentName)
		{
			int multiplier = 1;

			if (!RecordType.IsEmpty)
			{
				if (componentName == CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code
					|| componentName == CASSChargeCodeLookups.CASSComponents.Commission.Code
					|| componentName == CASSChargeCodeLookups.CASSComponents.Discount.Code)
				{
					multiplier = -1;
				}
				else if (componentName == VATAirline)
				{
					multiplier = CASSCost < 0 ? -1 : 1;
				}
				else if (componentName == VATAgent)
				{
					multiplier = CASSCost < 0 ? 1 : -1;
				}
			}

			return multiplier;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReasonCode();
			ValidateAdjustmentComment();
		}

		#endregion

		#region Private Functions and Properties

		CodeDescriptionPairList CostCompoentListForVATAirline
		{
			get { return new CodeDescriptionPairList() { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier, CASSChargeCodeLookups.CASSComponents.Discount }; }
		}

		CodeDescriptionPairList CostCompoentListForVATAgent
		{
			get { return new CodeDescriptionPairList() { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport, CASSChargeCodeLookups.CASSComponents.Commission }; }
		}

		void ValidateReasonCode()
		{
			AdjustmentReasonInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (HasAnyAmountChanged())
				{
					MandatoryValidation.CheckEntered(AdjustmentReasonInfo);
					ListValidation.ErrorIfInvalidCode(AdjustmentReasonInfo);
				}
			}
		}

		void ValidateAdjustmentComment()
		{
			AdjustmentReasonCommentInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (HasAnyAmountChanged())
				{
					MandatoryValidation.CheckEntered(AdjustmentReasonCommentInfo);
				}
			}
		}

		#endregion
	}
}