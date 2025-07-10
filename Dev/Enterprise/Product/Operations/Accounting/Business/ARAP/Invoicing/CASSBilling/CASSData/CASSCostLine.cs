using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public enum CASSCostLineType
	{
		Default,
		Adjustment,
		Rejected,
		Billing,
		Aggregated
	}
	public abstract class CASSCostLine : CASSData
	{
		protected CASSCostLine(BusinessObjectFactory factory, CASSCostLineType lineType)
			: base(factory)
		{
			LineType = lineType;

			CassCostComponentAmounts = new List<CASSCostComponentAmount>();
			CassVATComponentAmounts = new List<CASSVATComponentAmount>();
		}
		readonly protected List<CASSCostComponentAmount> CassCostComponentAmounts;
		readonly protected List<CASSVATComponentAmount> CassVATComponentAmounts;

		public override void Delete()
		{
			CassCostComponentAmounts.Clear();
			CassVATComponentAmounts.Clear();

			base.Delete();
		}

		#region properties

		[ReadOnly(true)]
		public ZString AirlinePrefix
		{
			get { return airlinePrefix; }
			set { SetNonPersistentPropertyValue(AirlinePrefixInfo, ref airlinePrefix, value); }
		}
		ZString airlinePrefix;

		public ZPropertyInfo AirlinePrefixInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AirlinePrefix));
			}
		}

		[ReadOnly(true)]
		public ZString AWBSerialNumber
		{
			get { return aWBSerialNumber; }
			set { SetNonPersistentPropertyValue(AWBSerialNumberInfo, ref aWBSerialNumber, value); }
		}
		ZString aWBSerialNumber;

		public ZPropertyInfo AWBSerialNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AWBSerialNumber));
			}
		}

		[ReadOnly(true)]
		public ZString AgentCode
		{
			get { return agentCode; }
			set { SetNonPersistentPropertyValue(AgentCodeInfo, ref agentCode, value); }
		}
		ZString agentCode;

		public ZPropertyInfo AgentCodeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AgentCode));
			}
		}

		[ReadOnly(true)]
		public ZDateTime DateAWBExecution
		{
			get { return dateAWBExecution; }
			set { SetNonPersistentPropertyValue(DateAWBExecutionInfo, ref dateAWBExecution, value); }
		}
		ZDateTime dateAWBExecution;

		public ZPropertyInfo DateAWBExecutionInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateAWBExecution));
			}
		}

		[ReadOnly(true)]
		public ZDateTime DateOfArrival
		{
			get { return dateOfArrival; }
			set { SetNonPersistentPropertyValue(DateOfArrivalInfo, ref dateOfArrival, value); }
		}
		ZDateTime dateOfArrival;

		public ZPropertyInfo DateOfArrivalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateOfArrival));
			}
		}

		[ReadOnly(true)]
		public ZDateTime DateOfDelivery
		{
			get { return dateOfDelivery; }
			set { SetNonPersistentPropertyValue(DateOfDeliveryInfo, ref dateOfDelivery, value); }
		}
		ZDateTime dateOfDelivery;

		public ZPropertyInfo DateOfDeliveryInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateOfDelivery));
			}
		}

		[ReadOnly(true)]
		public ZString Origin
		{
			get { return origin; }
			set { SetNonPersistentPropertyValue(OriginInfo, ref origin, value); }
		}
		ZString origin;

		public ZPropertyInfo OriginInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Origin));
			}
		}

		[ReadOnly(true)]
		public ZString Destination
		{
			get { return destination; }
			set { SetNonPersistentPropertyValue(DestinationInfo, ref destination, value); }
		}
		ZString destination;

		public ZPropertyInfo DestinationInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Destination));
			}
		}

		[ReadOnly(true)]
		[DecimalPlaces(1)]
		public ZDecimal Weight
		{
			get { return weight; }
			set { SetNonPersistentPropertyValue(WeightInfo, ref weight, value); }
		}
		ZDecimal weight;

		public ZPropertyInfo WeightInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Weight));
			}
		}

		[ReadOnly(true)]
		public ZString WeightUnit
		{
			get { return weightUnit; }
			set { SetNonPersistentPropertyValue(WeightUnitInfo, ref weightUnit, value); }
		}
		ZString weightUnit;

		public ZPropertyInfo WeightUnitInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(WeightUnit));
			}
		}

		public ZInt WeightSubUnitRatio
		{
			get
			{
				return weightUnit.ToUpper() == "KG" ? 10 : 1;
			}
		}

		[ReadOnly(true)]
		public ZString CurrencyCode
		{
			get { return currencyCode; }
			set { SetNonPersistentPropertyValue(CurrencyCodeInfo, ref currencyCode, value); }
		}
		ZString currencyCode;

		public ZPropertyInfo CurrencyCodeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CurrencyCode));
			}
		}

		public ZInt CurrencyISOSubUnitRatio
		{
			get
			{
				return Currency != null ? Currency.RX_ISOSubUnitRatio : (ZInt)100;
			}
		}

		public int CurrencyISODecimalPlaces
		{
			get
			{
				return Currency != null ? Currency.ISODecimals : 2;
			}
		}

		protected RefCurrency Currency
		{
			get
			{
				return RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCode);
			}
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		public ZDecimal CostAmountRowFileValue
		{
			get { return costAmountRowFileValue; }
			set { SetNonPersistentPropertyValue(CostAmountRowFileValueInfo, ref costAmountRowFileValue, value); }
		}
		ZDecimal costAmountRowFileValue;

		public ZPropertyInfo CostAmountRowFileValueInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CostAmountRowFileValue));
			}
		}

		[ReadOnly(true)]
		public ZString VATIndicator
		{
			get { return vATIndicator; }
			set { SetNonPersistentPropertyValue(VATIndicatorInfo, ref vATIndicator, value); }
		}
		ZString vATIndicator;

		public ZPropertyInfo VATIndicatorInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(VATIndicator));
			}
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		public ZDecimal VATAmountRowFileValue
		{
			get { return vATAmountRowFileValue; }
			set { SetNonPersistentPropertyValue(VATAmountRowFileValueInfo, ref vATAmountRowFileValue, value); }
		}
		ZDecimal vATAmountRowFileValue;

		public ZPropertyInfo VATAmountRowFileValueInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(VATAmountRowFileValue));
			}
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		public ZDecimal VATAmount { get { return VATAmountCore; } }

		public CASSCostLineType LineType { get; }

		public bool IsAdjustmentRecord { get { return LineType == CASSCostLineType.Adjustment; } }

		internal abstract ZDecimal VATAmountCore { get; }

		[DecimalPlaces(nameof(CurrencyISODecimalPlaces))]
		public ZDecimal CASSCost
		{
			get { return GetCASSCost(IsAdjustmentRecord); }
		}

		#endregion

		#region Public and Internal Functions

		public CASSCostLine CreateNewInstance(CASSCostLineType lnType)
		{
			return CreateNewInstanceCore(lnType);
		}

		public void UpdateOriginalAmounts()
		{
			UpdateOriginalAmountsCore();
		}

		public bool HasAnyAmountChanged()
		{
			return HasAnyAmountChangedCore();
		}

		public void Merge(CASSCostLine costLine)
		{
			if (costLine.GetType() != this.GetType())
			{
				throw new InvalidOperationException(Res.GetString("6fd699c1-8f0f-4eb3-8e36-698f503e14d0", "Cannot Merge. Expected Type: {0}. Provided Type: {1}", this.GetType(), costLine.GetType()));
			}

			foreach (CodeDescriptionPair component in CostComponentList)
			{
				var componentCosts = CassCostComponentAmounts.FirstOrDefault(x => x.ComponentName == component.Code);
				if (componentCosts == null)
				{
					componentCosts = new CASSCostComponentAmount(this, component.Code);
					CassCostComponentAmounts.Add(componentCosts);
				}
				componentCosts.AddAmount(costLine);
			}

			foreach (CodeDescriptionPair vatComponent in VATComponentList)
			{
				var vatComponentCosts = CassVATComponentAmounts.FirstOrDefault(x => x.ComponentName == vatComponent.Code);
				if (vatComponentCosts == null)
				{
					vatComponentCosts = new CASSVATComponentAmount(this, vatComponent.Code);
					CassVATComponentAmounts.Add(vatComponentCosts);
				}
				vatComponentCosts.AddAmount(costLine);
			}
		}

		#region Totals

		public ZDecimal GetCASSCost(bool isAdjustmentAmount)
		{
			return AggregateComponentAmount(CassCostComponentAmounts, isAdjustmentAmount);
		}

		public ZDecimal GetTax(bool isAdjustmentAmount)
		{
			return AggregateComponentAmount(CassVATComponentAmounts, isAdjustmentAmount);
		}

		#endregion

		#region Lists

		public CodeDescriptionPairList CostComponentList
		{
			get { return GetCostComponentListCore(); }
		}

		public CodeDescriptionPairList VATComponentList
		{
			get { return GetVATComponentListCore(); }
		}

		public CodeDescriptionPairList GetCostComponentListByVATComponent(string vatComponentName)
		{
			return GetCostComponentListByVATComponentCore(vatComponentName);
		}

		public ZGuid[] GetCASSChargeCodePKsFromRegistry(string cassComponent = "")
		{
			return GetCASSChargeCodePKsFromRegistryCore(cassComponent);
		}

		public bool IsVATComponent(string componentName)
		{
			return VATComponentList.ContainsCode(componentName);
		}

		public bool IsCostComponent(string componentName)
		{
			return CostComponentList.ContainsCode(componentName);
		}

		#endregion

		#region Components

		public ZDecimal GetComponentAmount(bool isAdjustmentAmount, string componentName)
		{
			ZDecimal result = ZDecimal.Zero;
			var components = new List<CASSCostComponentAmount>();

			if (IsCostComponent(componentName))
			{
				components = CassCostComponentAmounts;
			}
			else if (IsVATComponent(componentName))
			{
				components = CassVATComponentAmounts.Cast<CASSCostComponentAmount>().ToList();
			}

			var component = components.FirstOrDefault(x => x.ComponentName == componentName);
			if (component != null)
			{
				result = isAdjustmentAmount ? component.AdjustedAmountWithSign : component.AmountWithSign;
			}
			return result;
		}

		public Dictionary<ZString, decimal> GetVATApportionedToCostComponents(bool isAdjustmentAmount, string vatComponentName)
		{
			if (!IsVATComponent(vatComponentName))
			{
				throw new ArgumentException(Res.GetString("ccc8a646-c011-4673-82db-9752b0584155", "Invalid VAT Component Name", "vatComponentName"));
			}

			Dictionary<ZString, decimal> result = new Dictionary<ZString, decimal>();

			var component = CassVATComponentAmounts.FirstOrDefault(x => x.ComponentName == vatComponentName);
			if (component != null)
			{
				result = isAdjustmentAmount ? component.AdjustedApportionedVATAmountByCostComponets : component.ApportionedVATAmountByCostComponets;
			}

			return result;
		}

		internal int GetComponentAmountMultipler(ZString componentName)
		{
			return GetComponentAmountMultiplierCore(componentName);
		}

		#endregion

		#region VAT Apportioning

		internal Dictionary<ZString, decimal> GetApportionedVATByCostComponent(bool isAdjustment, ZString vatComponentName)
		{
			var result = new Dictionary<ZString, decimal>();

			if (!RecordType.IsEmpty)
			{
				var costComponents = Array.ConvertAll(GetCostComponentListByVATComponent(vatComponentName).GetAllCodes(), x => new ZString(x));
				var total = AggregateComponentAmount(CassCostComponentAmounts, isAdjustment, costComponents);
				var vatAmount = GetComponentAmount(isAdjustment, vatComponentName);

				foreach (ZString costComponent in costComponents)
				{
					var componentAmount = GetComponentAmount(isAdjustment, costComponent);
					var multiplier = Math.Sign(componentAmount) == Math.Sign(vatAmount) ? 1 : -1;
					result[costComponent] = total != 0 ? Utilities.Round(vatAmount * Math.Abs(componentAmount / total), 3) * multiplier : 0;
				}

				AdjustValuesToSumCorrectlyAfterDistribution(result, vatAmount);
			}

			return result;
		}

		#endregion

		#endregion

		#region Abstract and Virtual Functions

		protected abstract int GetComponentAmountMultiplierCore(ZString componentName);

		protected abstract CASSCostLine CreateNewInstanceCore(CASSCostLineType lnType);

		protected abstract bool HasAnyAmountChangedCore();

		protected abstract CodeDescriptionPairList GetCostComponentListCore();

		protected abstract CodeDescriptionPairList GetVATComponentListCore();

		protected abstract CodeDescriptionPairList GetCostComponentListByVATComponentCore(string vatComponentName);

		protected abstract ZGuid[] GetCASSChargeCodePKsFromRegistryCore(string cassComponent = "");

		protected virtual void UpdateOriginalAmountsCore()
		{
			this.HasChanges = false;
		}

		#endregion

		#region Protected Functions

		protected ZDecimal AggregateComponentAmount<T>(List<T> componentAmounts, bool isAdjustmentAmount, params ZString[] components)
			where T : CASSCostComponentAmount
		{
			ZDecimal result = ZDecimal.Zero;
			if (componentAmounts != null)
			{
				if (components == null || components.Length == 0)
				{
					result = componentAmounts.Sum(x => isAdjustmentAmount ? x.AdjustedAmountWithSign : x.AmountWithSign);
				}
				else
				{
					var selectedComponents = componentAmounts.Where(x => components.Contains(x.ComponentName)).ToArray();
					if (selectedComponents.Any())
					{
						result = selectedComponents.Sum(x => isAdjustmentAmount ? x.AdjustedAmountWithSign : x.AmountWithSign);
					}
				}
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void SetComponentAmount(ZPropertyInfo propertyInfo, ZString componentName, ref ZDecimal amount, ZDecimal value)
		{
			if (SetNonPersistentPropertyValue(propertyInfo, ref amount, value))
			{
				if (IsVATComponent(componentName))
				{
					SetAmount(CassVATComponentAmounts, componentName, value, () => new CASSVATComponentAmount(this, componentName));
				}
				else if (IsCostComponent(componentName))
				{
					SetAmount(CassCostComponentAmounts, componentName, value, () => new CASSCostComponentAmount(this, componentName));
				}
			}
		}

		protected bool DenyCASSCostEditing
		{
			get
			{
				return !Env.Security.APCASSCostFileModification.IsAllowed;
			}
		}

		#endregion

		#region Private Function

		void SetAmount<T>(List<T> componentAmounts, ZString componentName, ZDecimal amount, Func<T> getNewComponent)
			where T : CASSCostComponentAmount
		{
			T componentAmount = componentAmounts.FirstOrDefault(x => x.ComponentName == componentName);

			if (componentAmount == default(T))
			{
				componentAmount = getNewComponent();
				componentAmounts.Add(componentAmount);
			}

			if (LineType == CASSCostLineType.Adjustment)
			{
				componentAmount.Amount = 0;
				componentAmount.AdjustedAmount = amount;
			}
			else
			{
				componentAmount.Amount = amount;
				componentAmount.AdjustedAmount = 0;
			}
		}

		void AdjustValuesToSumCorrectlyAfterDistribution(Dictionary<ZString, decimal> distributions, ZDecimal originalTotalAmount)
		{
			ZDecimal totalAmount = 0M;
			ZDecimal maxAbsAmount = 0M;

			totalAmount = maxAbsAmount = 0M;

			if (distributions != null && distributions.Count > 0)
			{
				KeyValuePair<ZString, decimal> itemWithMaxDistributedAmount = distributions.First();

				foreach (KeyValuePair<ZString, decimal> item in distributions)
				{
					totalAmount += item.Value;

					ZDecimal absCostAmount = Math.Abs(item.Value);

					if (absCostAmount > maxAbsAmount)
					{
						maxAbsAmount = absCostAmount;
						itemWithMaxDistributedAmount = item;
					}
				}

				ZDecimal amountDifference = originalTotalAmount - totalAmount;

				if (amountDifference != 0)
				{
					distributions[itemWithMaxDistributedAmount.Key] = itemWithMaxDistributedAmount.Value + amountDifference;
				}
			}
		}

		#endregion
	}
}