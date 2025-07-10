using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.ZArchitecture.QuantityExtensions;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChargeCodeAndAmountInfo
	{
		public AccChargeCode ChargeCode;
		public ZDecimal CostAmount;
		public ZDecimal SellAmount;

		public ChargeCodeAndAmountInfo(AccChargeCode chargeCode, ZDecimal costAmount, ZDecimal sellAmount)
		{
			this.ChargeCode = chargeCode;
			this.CostAmount = costAmount;
			this.SellAmount = sellAmount;
		}
	}

	public class JobChargeQuickCalculateBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public JobChargeQuickCalculateBusinessObject(IQuickCalculateRating host, IQuickCalculatorCharge charge, IEnumerable<ChargeCodeAndAmountInfo> allChargeCodeAndAmountInfos = null)
			: base(charge.Factory)
		{
			this.Host = host;
			HostMeasures = (RateableMeasureSet)host.QuickMeasures;

			this.Charge = charge;
			this.Job = charge.InvoicingJob;
			if (charge.InvoicingJob != null && charge.InvoicingJob.Charges != null)
			{
				this.AllChargesInfo = charge.InvoicingJob.Charges.Cast<Charge>().Select(x => new ChargeCodeAndAmountInfo(x.ChargeCode, x.JR_OSCostAmt, x.JR_OSSellAmt));
			}
			else if (allChargeCodeAndAmountInfos != null)
			{
				this.AllChargesInfo = allChargeCodeAndAmountInfos;
			}

			RestorePreservedInformation();
		}

		public JobChargeQuickCalculateBusinessObject(IAutoRating host, IQuickCalculatorCharge charge)
			: this(new QuickCalculateRating(host), charge)
		{
		}

		void RestorePreservedInformation()
		{
			if (IsSpotConsolCost)
			{
				var paymentBasis = (JobPaymentBasis)Charge.CostPaymentBasesView.FirstOrDefault();
				if (paymentBasis != null)
				{
					if (IsVolume(paymentBasis.RateUnit) || IsWeight(paymentBasis.RateUnit))
					{
						CostRate = paymentBasis.RateValue;
					}

					if (RatingBehaviours.IsAutoRatingOverriderSpotBehaviour(Charge.RatingBehaviour))
					{
						if (!paymentBasis.PBS_PerUnitRate.IsDefault)
						{
							CostRate = paymentBasis.PBS_PerUnitRate;
						}

						if (!paymentBasis.PBS_ChargeableBasis.IsEmpty)
						{
							QuantityDescription = paymentBasis.PBS_ChargeableBasis.SubstringSafe(0, QuantityDescriptionInfo.MaxLength);
						}

						if (!paymentBasis.PBS_MinRate.IsDefault)
						{
							IsMinimum = true;
							CostMinimum = paymentBasis.PBS_MinRate;
						}
					}
				}
			}
		}

		readonly RateableMeasureSet HostMeasures;

		const string TEU = "TEU";

		#region Related Business Objects

		readonly Job Job;
		readonly IQuickCalculateRating Host;

		IQuickCalculatorCharge Charge
		{
			get { return quickCharge; }
			set
			{
				quickCharge = value;
				using (SuspendSettingHasChanges())
				{
					UpdateSell = Charge.CanUpdateSell;
					UpdateCost = Charge.CanUpdateCost;
				}
			}
		}

		IQuickCalculatorCharge quickCharge;

		public RefCurrency SellCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Charge.SellCurrencyCode); }
		}

		public RefCurrency CostCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Charge.CostCurrencyCode); }
		}

		readonly IEnumerable<ChargeCodeAndAmountInfo> AllChargesInfo;

		ChargeCodeAndAmountInfo ContainsChargeCode(AccChargeCode code)
		{
			return code != null && AllChargesInfo != null
				? AllChargesInfo.FirstOrDefault(charge => charge.ChargeCode?.PK == code.PK)
				: null;
		}

		#endregion

		#region Properties

		#region Is Spot Consol Cost

		public ZBool IsSpotConsolCost => RatingBehaviours.IsSpotBehaviour(Charge?.RatingBehaviour);

		#endregion

		#region Update Sell

		public ZBool UpdateSell
		{
			get { return fUpdateSell; }
			set
			{
				SetNonPersistentPropertyValue(UpdateSellInfo, ref fUpdateSell, value);
				ValidateUpdateSell();
				if (!UsesContainerCountMeasure)
				{
					return;
				}

				foreach (ContainerCalculationData containerData in Containers)
				{
					containerData.SellInfo.RefreshBinding();
				}
			}
		}

		ZBool fUpdateSell;

		public ZPropertyInfo UpdateSellInfo
		{
			get { return GetZPropertyInfo(nameof(UpdateSell)); }
		}

		protected bool UpdateSell_ReadOnly => !Charge.CanUpdateSell;

		#endregion

		#region Update Cost

		public ZBool UpdateCost
		{
			get { return fUpdateCost; }
			set
			{
				SetNonPersistentPropertyValue(UpdateCostInfo, ref fUpdateCost, value);
				ValidateUpdateCost();

				if (!UsesContainerCountMeasure)
				{
					return;
				}

				foreach (ContainerCalculationData containerData in Containers)
				{
					containerData.CostInfo.RefreshBinding();
				}
			}
		}

		ZBool fUpdateCost;

		public ZPropertyInfo UpdateCostInfo
		{
			get { return GetZPropertyInfo(nameof(UpdateCost)); }
		}

		protected bool UpdateCost_ReadOnly => !Charge.CanUpdateCost;

		#endregion

		#region Sell Rate

		public ZDecimal SellRate
		{
			get { return fSellRate; }
			set
			{
				SetNonPersistentPropertyValue(SellRateInfo, ref fSellRate, value);
				UpdateSellTotal();
			}
		}

		public ZPropertyInfo SellRateInfo
		{
			get { return GetZPropertyInfo(nameof(SellRate)); }
		}

		ZDecimal fSellRate;

		protected bool SellRate_ReadOnly => !Charge.CanUpdateSell;

		#endregion

		#region Cost Rate

		public ZDecimal CostRate
		{
			get { return fCostRate; }
			set
			{
				SetNonPersistentPropertyValue(CostRateInfo, ref fCostRate, value);
				UpdateCostTotal();
			}
		}

		public ZPropertyInfo CostRateInfo
		{
			get { return GetZPropertyInfo(nameof(CostRate)); }
		}

		ZDecimal fCostRate;

		protected bool CostRate_ReadOnly
		{
			get { return UpdateCost_ReadOnly; }
		}

		#endregion

		#region Quantity Description

		[MaxLength(25)]
		[List("QuantityDescriptionList")]
		public ZString QuantityDescription
		{
			get { return quantityDescription; }
			set
			{
				if (QuantityDescription == value)
				{
					return;
				}
				CheckMaximumLength(QuantityDescriptionInfo, value);
				SetNonPersistentPropertyValue(QuantityDescriptionInfo, ref quantityDescription, value);

				UpdateQuantityAndChargeCode();

				if (!IsValidationSuspended)
				{
					ValidateQuantityDescription();
				}
			}
		}

		void UpdateQuantityAndChargeCode()
		{
			// Since QuantityDescription is editable by the user, check for a valid value
			var quantityType = QuantityDescriptionList.GetDescriptionFromCode(quantityDescription);
			if (!string.IsNullOrEmpty(quantityType))
			{
				UpdateQuantityAndChargeCodeForValidType(quantityType);
			}
			else
			{
				SetDefaultQuantityAndChargeCode();
			}

			UpdateSellTotal();
			UpdateCostTotal();
		}

		void UpdateQuantityAndChargeCodeForValidType(string quantityType)
		{
			// Type is either
			// - a MeasureType present in the HostMeasures
			// - one of the special quantities added in QuantityDescriptionList
			if (quantityType == TEU)
			{
				// Note, TEU is only added to the QuantityDescriptionList if there is a container list (MeasureType.ContainerCount).
				var containerCount = HostMeasures.GetAllContainers().Sum(c => c.TEU);
				if (containerCount != 0)
				{
					Quantity = containerCount;
				}
				else
				{
					Quantity = HostMeasures.GetActual(MeasureType.ContainerCount);
				}
				QuantityUnit = TEU;
				ChargeCodePK = ZGuid.Empty;
			}
			else if (quantityType == ShipmentChargeable)
			{
				var forwardingShipment = RelatedShipment;
				if (forwardingShipment != null)
				{
					Quantity = forwardingShipment.JS_ActualChargeable;
					QuantityUnit = forwardingShipment.JS_ChargeableUnit;
					ChargeCodePK = ZGuid.Empty;
				}
				else
				{
					SetDefaultQuantityAndChargeCode();
				}
			}
			else if (IsPercentageCharge)
			{
				// quantityType == CarrierCommission || quantityType == ChargePercentage)
				Quantity = 0m;
				QuantityUnit = "%";
				ChargeCodePK = Env.Registry.FreightChargeCode;
			}
			else if (quantityType == Custom)
			{
				SetDefaultQuantityAndChargeCode();
			}
			else
			{
				var measureType = (MeasureType)Enum.Parse(typeof(MeasureType), quantityType, true);
				Quantity = HostMeasures.GetActual(measureType);
				QuantityUnit = HostMeasures.GetUnit(measureType);
				ChargeCodePK = ZGuid.Empty;
			}
		}

		void SetDefaultQuantityAndChargeCode()
		{
			Quantity = 0m;
			QuantityUnit = "";
			ChargeCodePK = ZGuid.Empty;
		}

		public ZPropertyInfo QuantityDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(QuantityDescription)); }
		}

		ZString quantityDescription;

		#endregion

		#region Quantity

		public ZDecimal Quantity
		{
			get { return quantity; }
			set
			{
				SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value);
				UpdateSellTotal();
				UpdateCostTotal();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code for Custom Calculation")]
		protected bool Quantity_ReadOnly
		{
			get { return QuantityDescription != "Custom" && !IsPercentageCharge; }
		}

		public ZPropertyInfo QuantityInfo
		{
			get { return GetZPropertyInfo(nameof(Quantity)); }
		}

		ZDecimal quantity;

		#endregion

		#region Quantity Unit

		[MaxLength(5)]
		public ZString QuantityUnit
		{
			get { return quantityUnit; }
			private set { SetNonPersistentPropertyValue(QuantityUnitInfo, ref quantityUnit, value); }
		}

		ZString quantityUnit;

		public ZPropertyInfo QuantityUnitInfo
		{
			get { return GetZPropertyInfo(nameof(QuantityUnit)); }
		}

		#endregion

		#region Sell Total

		public ZDecimal SellTotal
		{
			get { return this.sellTotal; }
			set
			{
				SetNonPersistentPropertyValue(SellTotalInfo, ref sellTotal, value);
			}
		}

		ZDecimal sellTotal;

		void UpdateSellTotal()
		{
			ZDecimal result = 0m;
			if (UsesContainerCountMeasure)
			{
				result = GetTotalFromContainers(CostSell.Revenue);
			}
			else
			{
				result = SellRate * (IsPercentageCharge ? (ZDecimal)(Quantity / 100m) : Quantity);
			}

			if (SellCurrency != null)
			{
				result = Utilities.Round(result, SellCurrency.Decimals);
			}

			SellTotal = (IsMinimum && SellMinimum > result) ? SellMinimum : result;
		}

		protected bool SellTotal_ReadOnly => !IsMinimum || !Charge.CanUpdateSell;

		public ZPropertyInfo SellTotalInfo
		{
			get { return GetZPropertyInfo(nameof(SellTotal)); }
		}

		#endregion

		#region Cost Total

		public ZDecimal CostTotal
		{
			get { return this.costTotal; }
			set
			{
				SetNonPersistentPropertyValue(CostTotalInfo, ref costTotal, value);
			}
		}

		ZDecimal costTotal;

		void UpdateCostTotal()
		{
			ZDecimal result = 0m;

			if (UsesContainerCountMeasure)
			{
				result = GetTotalFromContainers(CostSell.Cost);
			}
			else
			{
				result = CostRate * (IsPercentageCharge ? (ZDecimal)(Quantity / 100m) : Quantity);
			}

			if (CostCurrency != null)
			{
				result = Utilities.Round(result, CostCurrency.Decimals);
			}

			CostTotal = (IsMinimum && CostMinimum > result) ? CostMinimum : result;
		}

		protected bool CostTotal_ReadOnly => !IsMinimum || !Charge.CanUpdateCost;

		public ZPropertyInfo CostTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CostTotal)); }
		}

		#endregion

		#region Sell Minimum

		public ZDecimal SellMinimum
		{
			get { return this.sellMinimum; }
			set
			{
				SetNonPersistentPropertyValue(SellMinimumInfo, ref sellMinimum, value);
				UpdateSellTotal();
			}
		}

		void UpdateSellMinimum()
		{
			if (!IsMinimum)
			{
				SetNonPersistentPropertyValue(SellMinimumInfo, ref this.sellMinimum, 0m);
			}
		}

		ZDecimal sellMinimum;

		protected bool SellMinimum_ReadOnly
		{
			get { return !IsMinimum; }
		}

		public ZPropertyInfo SellMinimumInfo
		{
			get { return GetZPropertyInfo(nameof(SellMinimum)); }
		}

		#endregion

		#region Cost Minimum

		public ZDecimal CostMinimum
		{
			get { return this.costMinimum; }
			set
			{
				SetNonPersistentPropertyValue(CostMinimumInfo, ref costMinimum, value);
				UpdateCostTotal();
			}
		}

		void UpdateCostMinimum()
		{
			if (!IsMinimum)
			{
				SetNonPersistentPropertyValue(CostMinimumInfo, ref this.costMinimum, 0m);
			}
		}

		ZDecimal costMinimum;

		protected bool CostMinimum_ReadOnly
		{
			get { return !IsMinimum; }
		}

		public ZPropertyInfo CostMinimumInfo
		{
			get { return GetZPropertyInfo(nameof(CostMinimum)); }
		}

		#endregion

		#region Charge Code

		[List("ChargeCodes")]
		public ZGuid ChargeCodePK
		{
			get { return fChargeCodePK; }
			set
			{
				if (ChargeCodePK != value)
				{
					SetNonPersistentPropertyValue(ChargeCodePKInfo, ref fChargeCodePK, value);

					if (ChargeCode != null)
					{
						var existingCharge = ContainsChargeCode(ChargeCode);
						if (existingCharge != null)
						{
							SellRate = existingCharge.SellAmount;
							CostRate = existingCharge.CostAmount;
							UpdateTotals();
						}
					}

					if (!IsValidationSuspended)
					{
						ValidateChargeCodePK();
					}
				}
			}
		}

		ZGuid fChargeCodePK;

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCodePK)); }
		}

		public AccChargeCode ChargeCode
		{
			get { return Factory.Load<AccChargeCode>(ChargeCodePK); }
		}

		#endregion

		#region Percentage Charge

		public ZBool IsPercentageCharge
		{
			get { return QuantityDescription == ChargePercentage.GetUnresolvedString() || IsCarrierCommission; }
		}

		public ZBool IsCarrierCommission
		{
			get { return QuantityDescription == CarrierCommission.GetUnresolvedString(); }
		}

		public ZPropertyInfo IsPercentageChargeInfo
		{
			get { return GetZPropertyInfo(nameof(IsPercentageCharge)); }
		}

		#endregion

		#region Rate Label Text

		public ZString RateLabelText
		{
			get { return IsPercentageCharge ? Res.GetString("Accounting|JobChargeQuickCalculateBO|Amount", "Amount") : Res.GetString("Accounting|JobChargeQuickCalculateBO|Rate", "Rate"); }
		}

		public ZPropertyInfo RateLabelTextInfo
		{
			get { return GetZPropertyInfo(nameof(RateLabelText)); }
		}

		#endregion

		#region Is Minimum

		public ZBool IsMinimum
		{
			get { return this.isMinimum; }
			set
			{
				SetNonPersistentPropertyValue(IsMinimumInfo, ref this.isMinimum, value);
				UpdateSellTotal();
				UpdateCostTotal();
				UpdateSellMinimum();
				UpdateCostMinimum();
			}
		}

		ZBool isMinimum;

		public ZPropertyInfo IsMinimumInfo
		{
			get { return GetZPropertyInfo(nameof(IsMinimum)); }
		}

		public ZBool IsMinimumVisible
		{
			get { return !UsesContainerCountMeasure; }
		}

		public ZPropertyInfo IsMinimumVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(IsMinimumVisible)); }
		}

		#endregion

		#region Container Types

		public ContainerCalculationDataCollection Containers
		{
			get
			{
				if (containers != null)
				{
					return containers;
				}

				containers = new ContainerCalculationDataCollection(this);
				containers.UpdateTotals += Containers_UpdateTotals;
				var measures = HostMeasures;
				if (measures == null)
				{
					return containers;
				}

				var containersGroupedByTypePk = measures.GetContainerListWithCalculatedWeightVolume()
					.Where(c => c.ContainerTypePk != MeasureInfo.ContainerInfo.LCL)
					.GroupBy(c => c.ContainerTypePk);

				var containerTypePKs = containersGroupedByTypePk.Select(x => x.Key);
				if (!containerTypePKs.Any())
				{
					return containers;
				}

				var containerFilter = new ZQuery();
				containerFilter.AddToFilter(RefContainerSchema.PK, containerTypePKs);
				var refContainers = Factory.Load<RefContainer>(containerFilter);

				foreach (var containerTypeGroup in containersGroupedByTypePk)
				{
					var containerTypePK = containerTypeGroup.Key;
					var containerType = refContainers.FirstOrDefault(rc => rc.PK == containerTypePK)?.RC_Code ?? ZString.Empty;
					if (containerType.IsEmpty)
					{
						continue;
					}

					var containerData = containers.AddNew();
					containerData.ContainerType = containerType;

					if (IsSpotConsolCost)
					{
						var rate = Charge.CostPaymentBasesView.Where(
							x => x.PBS_ChargeableUnit == containerType &&
							x.RateUnit == RatingConstants.Units.CN
						).SingleOrDefault()?.RateValue;

						containerData.Cost = rate.GetValueOrDefault();
					}

					foreach (var container in containerTypeGroup)
					{
						var containerNumber = container.ContainerNumber;
						var containerCalculatedWeight = container.Weight;
						var containerCalculatedVolume = container.Volume;
						var commodity = container.CommodityCode;

						containerData.ContainerSelections.AddNewSelection(
							true,
							string.IsNullOrWhiteSpace(containerNumber) ? (string)(ZString)Res.GetString("4e37ccab-c356-4332-a83e-81389770f017", "<blank>") : containerNumber,
							container.ContainerCount,
							containerCalculatedWeight,
							containerCalculatedVolume,
							commodity);
					}

					containerData.ContainerSelections.Sort("Number");
					containerData.ContainerSelections.SelectionsChanged += ContainersOnSelectionsChanged;
				}

				return containers;
			}
		}

		void Containers_UpdateTotals(object sender, EventArgs e)
		{
			UpdateTotals();
		}

		void ContainersOnSelectionsChanged(object sender, EventArgs e)
		{
			Quantity = Containers.SelectedQuantity;
			UpdateTotals();
		}

		void UpdateTotals()
		{
			UpdateSellTotal();
			UpdateCostTotal();
		}

		ContainerCalculationDataCollection containers;

		public ZBool UsesContainerCountMeasure
		{
			get { return QuantityDescriptionList.GetDescriptionFromCode(QuantityDescription) == nameof(MeasureType.ContainerCount); }
		}

		public ZPropertyInfo UsesContainerCountMeasureInfo
		{
			get { return GetZPropertyInfo(nameof(UsesContainerCountMeasure)); }
		}

		#endregion

		#region Related Shipment

		ForwardingShipment RelatedShipment
		{
			get
			{
				if (relatedShipment == null && !string.IsNullOrEmpty(RelatedJobNumber))
				{
					relatedShipment = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, RelatedJobNumber);
				}
				return relatedShipment;
			}
		}
		ForwardingShipment relatedShipment;

		string RelatedJobNumber
		{
			get
			{
				if (relatedJobNumber == null && Charge is Charge charge && charge.InvoicingJob != null && charge.InvoicingJob.IsGatewayBillingJob())
				{
					relatedJobNumber = charge.JR_Calc_RelatedJobNumber;
				}
				return relatedJobNumber;
			}
		}
		string relatedJobNumber;

		#endregion

		#endregion

		#region Lookups

		#region Quantity Descriptions

		public CodeDescriptionPairList QuantityDescriptionList
		{
			get
			{
				if (quantityDescriptionList == null)
				{
					quantityDescriptionList = new CodeDescriptionPairList();

					var measures = HostMeasures;
					if (measures != null)
					{
						foreach (MeasureType measureType in measures.GetMeasureTypes())
						{
							if (measureType != MeasureType.Unidentified)
							{
								if (measureType == MeasureType.Chargeable && !string.IsNullOrEmpty(RelatedJobNumber))
								{
									quantityDescriptionList.AddPair(ShipmentChargeable, ShipmentChargeable);
								}
								else
								{
									quantityDescriptionList.AddPair(RateableMeasureSet.GetDescription(measureType), measureType.ToString());

									if (measureType == MeasureType.ContainerCount)
									{
										string teuCode = Res.GetString("7fc64722-71d5-4601-9963-d8adce59bcf6", "TEU");
										quantityDescriptionList.AddPair(teuCode, teuCode);
									}
								}
							}
						}
						quantityDescriptionList.Sort();
					}

					quantityDescriptionList.AddPair(CarrierCommission, CarrierCommission);
					if (Charge != null)
					{
						quantityDescriptionList.AddPair(ChargePercentage, ChargePercentage);
					}

					quantityDescriptionList.AddPair(Custom, Custom);
				}

				return quantityDescriptionList;
			}
		}

		CodeDescriptionPairList quantityDescriptionList;

		static MultilingualString ShipmentChargeable { get { return ResString.GetMultilingualString("Accounting|JobChargeQuickCalculateBO|ShipmentChargeable", "Shipment Chargeable"); } }
		static MultilingualString ChargePercentage { get { return ResString.GetMultilingualString("Accounting|JobChargeQuickCalculateBO|ChargePercentage", "Charge Percentage"); } }
		static MultilingualString CarrierCommission { get { return ResString.GetMultilingualString("Accounting|JobChargeQuickCalculateBO|CarrierCommission", "Carrier Commission"); } }
		static MultilingualString Custom { get { return ResString.GetMultilingualString("Accounting|JobChargeQuickCalculateBO|Custom", "Custom"); } }

		#endregion

		#region Charge Codes

		public AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion

		#endregion

		#region Validation

		public bool ValidateChargeIsDeleted()
		{
			ChargeCodePKInfo.ClearAllNotifications();

			var charge = Charge as Charge;
			if (charge != null && charge.IsDeleted)
			{
				ChargeCodePKInfo.AddError(Res.GetString("00e59d3e-2810-48de-9b0c-9b546f8f0e2e", "Charge {0} is deleted. You can only enter a un-deleted charge code.", charge.PK));
				return false;
			}
			return true;
		}

		public void ValidateQuantityDescription()
		{
			QuantityDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(QuantityDescriptionInfo);
			ListValidation.ErrorIfInvalidCode(QuantityDescriptionInfo);
		}

		public void ValidateUpdateCost()
		{
			UpdateCostInfo.ClearAllNotifications();
			if (UpdateCost_ReadOnly)
			{
				var warning = Charge is Charge charge && charge.JR_IsApportioned
					? Res.GetString("6c10a8e1-3a50-46e1-9837-01938b34d4a1", "Apportioned Cost charges may only be updated from Consol Costing")
					: Res.GetString("C3FAB744-C516-46EA-8458-4C9E5C0A3350", "Cost Charges may not be updated for {0} Charge Types.", Charge.ChargeCode?.AC_ChargeType);

				UpdateCostInfo.AddWarning(warning);
			}
		}

		public void ValidateUpdateSell()
		{
			UpdateSellInfo.ClearAllNotifications();
			if (UpdateSell_ReadOnly)
			{
				UpdateSellInfo.AddWarning(Res.GetString("b1cfed7a-5427-4656-b062-78fdb4a206e3", "Sell charges may not be updated from Consol Costing since it may already be posted and Disbursement Sell charges may only be updated from Cost."));
			}
		}

		public void ValidateChargeCodePK()
		{
			ChargeCodePKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo);

			if (ChargeCode != null && ContainsChargeCode(ChargeCode) == null)
			{
				ChargeCodePKInfo.AddError(Res.GetString("b3fe1c3e-555d-4e41-97e1-64424f669a3c", "Charge Code {0} does not exist on the job. You can only enter a charge code that already has a charge on the job.", ChargeCode.AC_Code));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (!ValidateChargeIsDeleted())
			{
				return;
			}
			ValidateQuantityDescription();
			ValidateUpdateCost();
			ValidateUpdateSell();
			ValidateChargeCodePK();
		}

		#endregion

		#region Calculation Results

		public void SetCalculationResults()
		{
			IDisposable ratingOverrideSuppressor = null;
			var job = Job;
			if (job != null)
			{
				ratingOverrideSuppressor = job.SuppressAutoRatingOverride();
			}

			using (ratingOverrideSuppressor)
			using ((Charge as ChargeWithCost)?.InvoiceTypeUpdateSuspender.GetSuspender())
			{
				var rateInfo = new AutoRateInfo(Factory);
				rateInfo.ChargeCode = Charge.ChargeCode;
				rateInfo.HasExplicitZeroAmount = true;

				SetIsMinimumForRateInfo(rateInfo);

				if (UpdateCost)
				{
					SetCostResult(rateInfo);
				}

				if (UpdateSell)
				{
					SetRevenueResult(rateInfo);
				}

				DisableCalculationLogs();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		void SetRevenueResult(AutoRateInfo rateInfo)
		{
			rateInfo.Bases.Clear();
			rateInfo.Bases.AddRange(CreateQuickCalculatorPaymentBasis(CostSell.Revenue, SellRate, SellMinimum, Charge.SellCurrencyCode));

			rateInfo.ChargeCode = Charge.ChargeCode;
			if (Charge.IsCalculationDescriptionRelevant)
			{
				rateInfo.InvoiceLineDescription = (Charge.ChargeCode != null ? Charge.ChargeCode.AC_Desc + " - " : "") + rateInfo.Bases.GetDescription();
			}

			SetRateDescription(rateInfo, false);

			var debtor = IsCarrierCommission && Host.Carrier != null ? Host.Carrier : null;
			Charge.SetAmount(CostSell.Revenue, rateInfo, debtor, true);
		}

		void SetCostResult(AutoRateInfo rateInfo)
		{
			rateInfo.Bases.Clear();
			rateInfo.Bases.AddRange(CreateQuickCalculatorPaymentBasis(CostSell.Cost, CostRate, CostMinimum, Charge.CostCurrencyCode));

			SetRateDescription(rateInfo, true);
			Charge.SetAmount(CostSell.Cost, rateInfo, null, true);

			SetSpotRateReference(rateInfo);
		}

		void SetRateDescription(AutoRateInfo rateInfo, bool isCost)
		{
			var description = new ZString();
			if (Charge.ChargeCode != null)
			{
				description += $"{Charge.ChargeCode.AC_Code}: ";
			}

			var rateDescription = rateInfo.Bases.GetDescription();
			if (IsCarrierCommission)
			{
				var commissionDescription = Res.GetString("0156688d-301d-452e-9af6-80bac3a68c2a", "Carrier Commission for") + " " + (Host.Carrier != null ? Host.Carrier.OH_FullNameTruncated : (ZString)Res.GetString("45b39067-5a92-47f6-81bd-e3ac0723dcab", "Unknown Carrier"));
				rateDescription = Invariant($"{rateDescription} - {commissionDescription}");
			}

			description += rateDescription;
			description += System.Environment.NewLine + System.Environment.NewLine;
			description += isCost ? Res.GetString("c38cde98-4d84-492f-9f03-ecaf397279e4", "Cost Amount Entered using Quick Calculator") : Res.GetString("7e8b96f9-1fce-4a59-b07c-43ccb2f1b644", "Sell Amount Entered using Quick Calculator");
			description += System.Environment.NewLine;

			rateInfo.Description = description;
		}

		#region SuppressResourceStringsCheckRegion
		void SetSpotRateReference(AutoRateInfo rateInfo)
		{
			if (Charge == null || Charge.ChargeCode == null)
			{
				return;
			}

			if (Host.AutoRatedFor is IAdditionalReferenceNumberSupporter hostNumbersProvider && RatingBehaviours.IsAutoRatingOverriderSpotBehaviour(Charge.RatingBehaviour))
			{
				var description = $"{Charge.RatingBehaviour} {Charge.ChargeCode.AC_Code} {Charge.CostCurrencyCode} {GetSpotRerefenceDescription(rateInfo.Bases)}";

				var relatedSpotRateUpdated = false;
				foreach (var item in hostNumbersProvider.AdditionalReferenceNumbers)
				{
					if (item is CusEntryNumber entryNumber && entryNumber.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.SpotReference)
					{
						var numbers = entryNumber.CE_EntryNum.Split(' ');
						if (numbers.Length >= 2)
						{
							if (numbers[0] == Charge.RatingBehaviour && numbers[1] == Charge.ChargeCode.AC_Code)
							{
								entryNumber.CE_EntryNum = description;
								relatedSpotRateUpdated = true;
							}
						}
					}
				}

				if (!relatedSpotRateUpdated)
				{
					var newReference = hostNumbersProvider.AdditionalReferenceNumbers.AddNew();
					newReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.SpotReference;
					newReference.CE_EntryNum = description;
				}
			}
		}

		string GetSpotRerefenceDescription(IEnumerable<PaymentBasis> paymentBases)
		{
			var result = string.Empty;
			var perUnitBasis = paymentBases.ExcludingMinOrMax().FirstOrDefault(p => p.RateInfo.PerUnitRate > 0);

			if (perUnitBasis != default)
			{
				result = PaymentBasisExtensions.Str("{0}@{1}", perUnitBasis.RateInfo.PerUnitRate, perUnitBasis.RateInfo.Unit);
			}

			if (paymentBases.Any(p => p.RateInfo.MinRate > 0))
			{
				var minBasis = paymentBases.First(p => p.RateInfo.MinRate > 0);
				result += PaymentBasisExtensions.Str(" Min: {0}", minBasis.RateInfo.MinRate);
			}
			else
			{
				result += $" Min: 0";
			}

			return result;
		}
		#endregion

		IEnumerable<PaymentBasis> CreateQuickCalculatorPaymentBasis(CostSell costOrSell, ZDecimal rate, ZDecimal minimum, string currency)
		{
			void SetMeasurementBasis(ref RateInfo info)
			{
				if (RatingBehaviours.IsAutoRatingOverriderSpotBehaviour(Charge.RatingBehaviour))
				{
					info.MeasurementBasis = QuantityDescription;
				}
			}

			var result = new List<PaymentBasis>();

			if (UsesContainerCountMeasure)
			{
				var containersWithNonZeroAmountCount = Containers.Cast<ContainerCalculationData>()
					.Where(x => costOrSell == CostSell.Cost && x.Cost != 0
						|| costOrSell == CostSell.Revenue && x.Sell != 0)
					.Sum(x => x.QuantitySelectedContainers);

				if (containersWithNonZeroAmountCount > 0)
				{
					foreach (ContainerCalculationData containerData in Containers)
					{
						var amountForContainerType = costOrSell == CostSell.Cost ? containerData.Cost : containerData.Sell;
						if (amountForContainerType != 0)
						{
							var rateInfo = RateInfo.CreateUNT(amountForContainerType, RatingConstants.Units.CN, currency);
							SetMeasurementBasis(ref rateInfo);
							var containerQuantity = new Quantity(containerData.QuantitySelectedContainers, containerData.ContainerType, reference: containerData.ContainerNumbers);

							var paymentBasis = new PaymentBasis(containerQuantity, rateInfo, Host.AdapterType, Host.OperationalJobCode, chargeableUnitDescription: containerData.ContainerType);
							result.Add(paymentBasis);
						}
					}
				}
			}
			else
			{
				if (IsPercentageCharge)
				{
					var chargeableQuantity = new Quantity(rate, currency);
					var chargeCode = ChargeCode != null ? ChargeCode.AC_Code : null;
					var rateInfo = RateInfo.CreatePER(Quantity, currency);
					SetMeasurementBasis(ref rateInfo);
					var percentageBasis = new PaymentBasis(chargeableQuantity, rateInfo, Host.AdapterType, Host.OperationalJobCode, chargeableDescription: chargeCode);
					result.Add(percentageBasis);
				}
				else
				{
					var unit = string.IsNullOrWhiteSpace(QuantityUnit) ? QuantityDescription : QuantityUnit;
					var chargeableQuantity = new Quantity(Quantity, unit);
					var rateInfo = RateInfo.CreateUNT(rate, unit, currency);
					SetMeasurementBasis(ref rateInfo);
					var paymentBasis = new PaymentBasis(chargeableQuantity, rateInfo, Host.AdapterType, Host.OperationalJobCode);
					result.Add(paymentBasis);
				}
			}

			if (IsMinimum)
			{
				var rateInfo = RateInfo.CreateMIN(minimum, currency);
				SetMeasurementBasis(ref rateInfo);
				result.Add(new PaymentBasis(default, rateInfo, Host.AdapterType, Host.OperationalJobCode));
			}

			return result;
		}

		ZDecimal GetTotalFromContainers(CostSell costOrSell) => (costOrSell == CostSell.Cost)
			? Containers.Cast<ContainerCalculationData>().Sum(x => x.Cost * x.QuantitySelectedContainers)
			: Containers.Cast<ContainerCalculationData>().Sum(x => x.Sell * x.QuantitySelectedContainers);

		// TODO: Remove this method in 2 years time with WI00617632
		void SetIsMinimumForRateInfo(AutoRateInfo rateInfo)
		{
			if (IsMinimum)
			{
				rateInfo.Attributes.Add(JobChargeAttribTypeList.Codes.MinimumRateUsed, ZBool.True.ToString());
			}
		}

		void DisableCalculationLogs()
		{
			BusinessObject bizo = Charge as BusinessObject;
			if (bizo != null)
			{
				CalculationLogsLoader.Disable(bizo);
			}

			bizo = Host.HostBusinessObject;
			if (bizo != null)
			{
				CalculationLogsLoader.Disable(bizo);
			}
		}

		#endregion
	}

	public class ContainerCalculationData : NonPersistentBusinessObject
	{
		public ContainerCalculationData(ContainerCalculationDataCollection parentCollection)
		{
			this.parentCollection = parentCollection;
		}

		readonly ContainerCalculationDataCollection parentCollection;

		public ZString ContainerType
		{
			get => containerType;
			set => SetNonPersistentPropertyValue(ContainerTypeInfo, ref containerType, value);
		}
		ZString containerType;

		[List("ContainerSelections")]
		public ZString ContainerNumbers => ContainerSelections?.SelectedNumbers ?? ZString.Empty;

		public ZDecimal Quantity
		{
			get => quantity;
			set => SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value);
		}
		ZDecimal quantity;

		public ZDecimal QuantitySelectedContainers => ContainerSelections.SelectedQuantity;

		public ZPropertyInfo QuantitySelectedContainersInfo => GetZPropertyInfo(nameof(QuantitySelectedContainers));

		public ZDecimal Cost
		{
			get => cost;
			set
			{
				SetNonPersistentPropertyValue(CostInfo, ref cost, value);
				parentCollection.OnUpdateTotals();
			}
		}
		ZDecimal cost;

		public ZDecimal Sell
		{
			get => sell;
			set
			{
				SetNonPersistentPropertyValue(SellInfo, ref sell, value);
				parentCollection.OnUpdateTotals();
			}
		}
		ZDecimal sell;

		public ContainerSelectionBusinessObjectCollection ContainerSelections => containerSelections ?? (containerSelections = new ContainerSelectionBusinessObjectCollection());
		ContainerSelectionBusinessObjectCollection containerSelections;

		public ZPropertyInfo ContainerTypeInfo => GetZPropertyInfo(nameof(ContainerType));

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		public ZPropertyInfo CostInfo => GetZPropertyInfo(nameof(Cost));

		protected bool Cost_ReadOnly => !parentCollection.Parent?.UpdateCost ?? true;

		public ZPropertyInfo SellInfo => GetZPropertyInfo(nameof(Sell));

		protected bool Sell_ReadOnly => !parentCollection.Parent?.UpdateSell ?? true;

		public ZPropertyInfo ContainerNumbersInfo => GetZPropertyInfo(nameof(ContainerNumbers));
	}

	public class ContainerCalculationDataCollection : NonPersistentBusinessObjectCollection<ContainerCalculationData>
	{
		public ContainerCalculationDataCollection(JobChargeQuickCalculateBusinessObject parent)
		{
			Parent = parent;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContainerCalculationData(this);
		}

		protected override bool AllowNewCore => false;

		public event EventHandler UpdateTotals;

		public void OnUpdateTotals()
		{
			UpdateTotals?.Invoke(this, new EventArgs());
		}

		public ZDecimal SelectedQuantity => this.Cast<ContainerCalculationData>().Sum(c => c.ContainerSelections.SelectedQuantity);

		public readonly JobChargeQuickCalculateBusinessObject Parent;
	}
}

