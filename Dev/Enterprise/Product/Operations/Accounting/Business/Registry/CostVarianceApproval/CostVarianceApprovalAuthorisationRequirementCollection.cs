using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;
using VarianceSigns = Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement.VarianceSigns;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CostVarianceApprovalAuthorisationRequirementCollection : AmountBasedAuthorisationRequirementCollection
	{
		public CostVarianceApprovalAuthorisationRequirementCollection()
		{
		}

		public CostVarianceApprovalAuthorisationRequirementCollection(CostVarianceApproval parent, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			this.Parent = parent;
		}

		public new CostVarianceApprovalAuthorisationRequirement this[int x]
		{
			get { return (CostVarianceApprovalAuthorisationRequirement)base[x]; }
		}

		public new CostVarianceApprovalAuthorisationRequirement AddNew()
		{
			return (CostVarianceApprovalAuthorisationRequirement)base.AddNew();
		}

		public CostVarianceApprovalAuthorisationRequirementCollection Clone(CostVarianceApproval parent, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CostVarianceApprovalAuthorisationRequirementCollection result = (CostVarianceApprovalAuthorisationRequirementCollection)Clone(fallbackLevel, factory);
			result.Parent = parent;

			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CostVarianceApprovalAuthorisationRequirementCollection(null, fallbackLevel, null);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CostVarianceApprovalAuthorisationRequirement();
		}

		public CostVarianceApproval Parent { get; set; }

		#region Helper Methods

		public CostVarianceApprovalAuthorisationRequirement GetMatchingAuthorisationRequirement(ZDecimal amountVariance, ZDecimal accrualAmount)
		{
			if (amountVariance == 0)
			{
				return null;
			}

			ZDecimal variance = amountVariance;
			if (Parent.VarianceCalculationStyle != Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount)
			{
				if (accrualAmount != ZDecimal.Zero)
				{
					variance = (ZDecimal)(amountVariance / accrualAmount * 100M);
				}
				else
				{
					variance = amountVariance > 0 ? Decimal.MaxValue : Decimal.MinValue;
				}
			}

			if (Parent.VarianceCalculationStyle == Core.Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance)
			{
				return AuthorisationRequiredWithMaximum(variance, accrualAmount);
			}
			else if (Parent.VarianceCalculationStyle == Core.Constants.CostVarianceCalculationStyle.PercentageVarianceAndLocalCostAmountExTax)
			{
				return AuthorisationRequiredWithPAA(variance, amountVariance);
			}
			else
			{
				return GetElementsBySign(variance).GetAuthorisationRequired(Math.Abs(variance));
			}
		}

		public CostVarianceApprovalAuthorisationRequirement GetAuthorisationRequiredForTotal(ZDecimal totalAmount)
		{
			CostVarianceApprovalAuthorisationRequirement result = null;
			var absAmount = Math.Abs(totalAmount);

			if (totalAmount != 0)
			{
				foreach (var item in GetElementsBySign(totalAmount))
				{
					if (item.Range == RangeCodes.Above && item.MonitorTotalInvoiceVariance && absAmount > item.TotalInvoiceVarianceAmount)
					{
						return item;
					}
				}

				var isMonitorTotalInvoiceVarianceTicked = false;
				foreach (var item in GetElementsBySign(totalAmount))
				{
					if (item.Range == RangeCodes.UpTo)
					{
						if (item.MonitorTotalInvoiceVariance)
						{
							isMonitorTotalInvoiceVarianceTicked = true;

							if ((result == null || !result.MonitorTotalInvoiceVariance || item.TotalInvoiceVarianceAmount < result.TotalInvoiceVarianceAmount)
								&& (item.TotalInvoiceVarianceAmount >= absAmount))
							{
								result = item;
							}
						}
						else if (result == null || (!result.MonitorTotalInvoiceVariance
							&& result.GetAuthorisationRequirementWeight(result.AuthorisationRequirement) > item.GetAuthorisationRequirementWeight(item.AuthorisationRequirement)))
						{
							result = item;
						}
					}
				}

				if (!isMonitorTotalInvoiceVarianceTicked)
				{
					result = null;
				}
			}

			return result;
		}

		public IEnumerable<CostVarianceApprovalAuthorisationRequirement> GetElementsBySign(ZDecimal signAmount)
		{
			var sign = Math.Sign(signAmount);
			switch (sign)
			{
				case -1: return this.Cast<CostVarianceApprovalAuthorisationRequirement>().Where(x => x.VarianceSign == VarianceSigns.Minus);
				case 1: return this.Cast<CostVarianceApprovalAuthorisationRequirement>().Where(x => x.VarianceSign == VarianceSigns.Plus);
			}
			return Enumerable.Empty<CostVarianceApprovalAuthorisationRequirement>();
		}

		CostVarianceApprovalAuthorisationRequirement AuthorisationRequiredWithMaximum(ZDecimal percentage, ZDecimal accrualAmount)
		{
			CostVarianceApprovalAuthorisationRequirement result = null;

			if (percentage != 0)
			{
				var absPercentage = Math.Abs(percentage);
				foreach (CostVarianceApprovalAuthorisationRequirement item in GetElementsBySign(percentage))
				{
					if (item.Range == RangeCodes.Above && absPercentage >
						GetVariancePercentageIncludingMaximum(item.Percentage, item.LocalCostAmount, accrualAmount))
					{
						return item;
					}
				}

				ZDecimal resultPercentage = decimal.MaxValue;
				foreach (CostVarianceApprovalAuthorisationRequirement item in GetElementsBySign(percentage))
				{
					if (item.Range == RangeCodes.UpTo)
					{
						ZDecimal currentPercentage =
							GetVariancePercentageIncludingMaximum(item.Percentage, item.LocalCostAmount, accrualAmount);
						if (result == null || currentPercentage < resultPercentage)
						{
							if (currentPercentage >= absPercentage)
							{
								result = item;
								resultPercentage = currentPercentage;
							}
						}
					}
				}
			}

			return result;
		}

		CostVarianceApprovalAuthorisationRequirement AuthorisationRequiredWithPAA(ZDecimal percentage, ZDecimal accrualAmount)
		{
			if (percentage == 0 || accrualAmount == 0)
			{
				return null;
			}

			CostVarianceApprovalAuthorisationRequirement result = null;

			foreach (var item in GetElementsBySign(percentage).Where(item => item.Range == RangeCodes.UpTo))
			{
				var isMoreLowLevel = result == null
					|| item.GetAuthorisationRequirementWeight(item.AuthorisationRequirement) < result.GetAuthorisationRequirementWeight(result.AuthorisationRequirement);
				if (isMoreLowLevel && (item.Percentage >= Math.Abs(percentage) || item.LocalCostAmount >= Math.Abs(accrualAmount)))
				{
					result = item;
				}
			}

			if (result == null)
			{
				var rangeAbove = GetElementsBySign(percentage).FirstOrDefault(item => item.Range == RangeCodes.Above);
				if (rangeAbove != null
					&& Math.Abs(percentage) > rangeAbove.Percentage
					&& Math.Abs(accrualAmount) > rangeAbove.LocalCostAmount)
				{
					result = rangeAbove;
				}
			}

			return result;
		}

		ZDecimal GetVariancePercentageIncludingMaximum(ZDecimal percentage, ZDecimal maximum, ZDecimal accrualAmount)
		{
			return Math.Min((decimal)percentage, accrualAmount == 0 ? decimal.MaxValue : (maximum / accrualAmount * 100M));
		}

		#endregion

	}
}
