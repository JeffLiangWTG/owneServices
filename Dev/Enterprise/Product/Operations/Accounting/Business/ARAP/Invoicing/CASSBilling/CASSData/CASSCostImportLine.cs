using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSCostImportLine : CASSCostLine
	{
		public CASSCostImportLine(BusinessObjectFactory factory, CASSCostLineType lineType)
			: base(factory, lineType)
		{
		}

		#region Properties

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal WeightCharges
		{
			get { return weightCharges; }
			set { SetComponentAmount(WeightChargesInfo, CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge.Code, ref weightCharges, value); }
		}
		ZDecimal weightCharges;

		public ZPropertyInfo WeightChargesInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(WeightCharges));
			}
		}

		public bool WeightCharges_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal ChargesDueAgentCC
		{
			get { return chargesDueAgentCC; }
			set { SetComponentAmount(ChargesDueAgentCCInfo, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentImport.Code, ref chargesDueAgentCC, value); }
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
		public ZDecimal ChargesDueCarrierCC
		{
			get { return chargesDueCarrierCC; }
			set { SetComponentAmount(ChargesDueCarrierCCInfo, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheCarrier.Code, ref chargesDueCarrierCC, value); }
		}
		ZDecimal chargesDueCarrierCC;

		public ZPropertyInfo ChargesDueCarrierCCInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChargesDueCarrierCC));
			}
		}

		public bool ChargesDueCarrierCC_ReadOnly { get { return DenyCASSCostEditing; } }

		public ZBool FeeCharged
		{
			get { return feeCharged; }
			set { SetNonPersistentPropertyValue(FeeChargedInfo, ref feeCharged, value); }
		}
		ZBool feeCharged;

		public ZPropertyInfo FeeChargedInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(FeeCharged));
			}
		}

		public bool FeeCharged_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal FeeAmount
		{
			get { return feeAmount; }
			set { SetComponentAmount(FeeAmountInfo, CASSChargeCodeLookups.CASSComponents.CollectFee.Code, ref feeAmount, value); }
		}
		ZDecimal feeAmount;

		public ZPropertyInfo FeeAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(FeeAmount));
			}
		}

		public bool FeeAmount_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal HandlingCharges
		{
			get { return handlingCharges; }
			set { SetComponentAmount(HandlingChargesInfo, CASSChargeCodeLookups.CASSComponents.HandlingCharges.Code, ref handlingCharges, value); }
		}
		ZDecimal handlingCharges;

		public ZPropertyInfo HandlingChargesInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(HandlingCharges));
			}
		}

		public bool HandlingCharges_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal StorageCharges
		{
			get { return storageCharges; }
			set { SetComponentAmount(StorageChargesInfo, CASSChargeCodeLookups.CASSComponents.StorageCharges.Code, ref storageCharges, value); }
		}
		ZDecimal storageCharges;

		public ZPropertyInfo StorageChargesInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(StorageCharges));
			}
		}

		public bool StorageCharges_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal OtherCharge1Amount
		{
			get { return otherCharge1Amount; }
			set { SetComponentAmount(OtherCharge1AmountInfo, CASSChargeCodeLookups.CASSComponents.OtherCharge1.Code, ref otherCharge1Amount, value); }
		}
		ZDecimal otherCharge1Amount;

		public ZPropertyInfo OtherCharge1AmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(OtherCharge1Amount));
			}
		}

		public bool OtherCharge1Amount_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal OtherCharge2Amount
		{
			get { return otherCharge2Amount; }
			set { SetComponentAmount(OtherCharge2AmountInfo, CASSChargeCodeLookups.CASSComponents.OtherCharge2.Code, ref otherCharge2Amount, value); }
		}
		ZDecimal otherCharge2Amount;

		public ZPropertyInfo OtherCharge2AmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(OtherCharge1Amount));
			}
		}

		public bool OtherCharge2Amount_ReadOnly { get { return DenyCASSCostEditing; } }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		[MaxLength(12)]
		public ZDecimal MiscellaneousChargesAmount
		{
			get { return miscellaneousChargesAmount; }
			set { SetComponentAmount(MiscellaneousChargesAmountInfo, CASSChargeCodeLookups.CASSComponents.MiscellaneousChargesAmount.Code, ref miscellaneousChargesAmount, value); }
		}
		ZDecimal miscellaneousChargesAmount;

		public ZPropertyInfo MiscellaneousChargesAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(MiscellaneousChargesAmount));
			}
		}

		public bool MiscellaneousChargesAmount_ReadOnly { get { return DenyCASSCostEditing; } }

		public ZDecimal WeightCharges_Original { get; private set; }
		public ZDecimal ChargesDueAgentCC_Original { get; private set; }
		public ZDecimal ChargesDueCarrierCC_Original { get; private set; }
		public ZBool FeeCharged_Original { get; private set; }
		public ZDecimal FeeAmount_Original { get; private set; }
		public ZDecimal HandlingCharges_Original { get; private set; }
		public ZDecimal StorageCharges_Original { get; private set; }
		public ZDecimal OtherCharge1Amount_Original { get; private set; }
		public ZDecimal OtherCharge2Amount_Original { get; private set; }
		public ZDecimal MiscellaneousChargesAmount_Original { get; private set; }

		internal override ZDecimal VATAmountCore
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		#endregion

		#region Overridden Functions
		protected override void UpdateOriginalAmountsCore()
		{
			WeightCharges_Original = WeightCharges;
			ChargesDueAgentCC_Original = ChargesDueAgentCC;
			ChargesDueCarrierCC_Original = ChargesDueCarrierCC;
			FeeCharged_Original = FeeCharged;
			FeeAmount_Original = FeeAmount;
			HandlingCharges_Original = HandlingCharges;
			StorageCharges_Original = StorageCharges;
			OtherCharge1Amount_Original = OtherCharge1Amount;
			OtherCharge2Amount_Original = OtherCharge2Amount;
			MiscellaneousChargesAmount_Original = MiscellaneousChargesAmount;

			base.UpdateOriginalAmountsCore();
		}

		protected override bool HasAnyAmountChangedCore()
		{
			return WeightCharges != WeightCharges_Original
					|| ChargesDueAgentCC != ChargesDueAgentCC_Original
					|| ChargesDueCarrierCC != ChargesDueCarrierCC_Original
					|| FeeCharged != FeeCharged_Original
					|| FeeAmount != FeeAmount_Original
					|| HandlingCharges != HandlingCharges_Original
					|| StorageCharges != StorageCharges_Original
					|| OtherCharge1Amount != OtherCharge1Amount_Original
					|| OtherCharge2Amount != OtherCharge2Amount_Original
					|| MiscellaneousChargesAmount != MiscellaneousChargesAmount_Original;
		}

		protected override CASSCostLine CreateNewInstanceCore(CASSCostLineType lnType)
		{
			return new CASSCostImportLine(Factory, lnType);
		}

		protected override CodeDescriptionPairList GetCostComponentListCore()
		{
			return CASSChargeCodeRegistryExtractor.CASSImportCostComponents;
		}

		protected override CodeDescriptionPairList GetVATComponentListCore()
		{
			return new CodeDescriptionPairList();
		}

		protected override CodeDescriptionPairList GetCostComponentListByVATComponentCore(string vatComponentName)
		{
			return new CodeDescriptionPairList();
		}

		protected override ZGuid[] GetCASSChargeCodePKsFromRegistryCore(string cassComponent = "")
		{
			if (!string.IsNullOrEmpty(cassComponent))
			{
				return CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Import.Code, cassComponent);
			}
			else
			{
				return CASSChargeCodeRegistryExtractor.GetAllChargeCodePksByCASSType(CASSChargeCodeLookups.CASSTypes.Import.Code);
			}
		}

		protected override int GetComponentAmountMultiplierCore(ZString componentName)
		{
			int multiplier = 1;
			if (!RecordType.IsEmpty && componentName == CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentImport.Code)
			{
				multiplier = -1;
			}
			return multiplier;
		}

		#endregion
	}
}
