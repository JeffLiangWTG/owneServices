using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class ConsolRevenueValidation : ZValidation
	{
		public ConsolRevenueValidation(ConsolRevenue master)
			: base(master)
		{
			this.Master = master;
		}

		public override Type AutoValidationType
		{
			get { return typeof(ConsolRevenueValidation); }
		}

		public override void ValidateAll()
		{
			ValidateChargeCode();
			ValidateApprMethod();
		}

		public void ValidateUnapportionedAmount()
		{
			ValidateCalculatedProperty(Master.UnApportionedAmountInfo);
		}

		public void ValidateSellAmount()
		{
			ValidateCalculatedProperty(Master.SellAmountInfo);
		}

		public void ValidateChargeCode()
		{
			ValidateCalculatedProperty(Master.ChargeCodeInfo);
		}

		public void ValidateApprMethod()
		{
			ValidateCalculatedProperty(Master.ApportionmentMethodInfo);
		}

		public void ValidateCurrency()
		{
			ValidateCalculatedProperty(Master.CurrencyInfo);
		}

		public void ValidateCostGovtChargeCode()
		{
			ValidateCalculatedProperty(Master.CostGovtChargeCodeInfo);
		}

		public void ValidateSellGovtChargeCode()
		{
			ValidateCalculatedProperty(Master.SellGovtChargeCodeInfo);
		}

		public void ValidateSellSupplyType()
		{
			ValidateCalculatedProperty(Master.SellSupplyTypeInfo);
		}

		public void ValidateDescription()
		{
			ValidateCalculatedProperty(Master.DescriptionInfo);
		}

		protected virtual void CheckChargeCode()
		{
			MandatoryValidation.CheckEntered(Master.ChargeCodeInfo);
			ListValidation.ErrorIfInvalidPK(Master.ChargeCodeInfo, Master.Lookups.ChargeCodes);
		}

		protected virtual void CheckApportionmentMethod()
		{
			MandatoryValidation.CheckEntered(Master.ApportionmentMethodInfo);
		}

		protected virtual void CheckUnApportionedAmount()
		{
			if (!Master.UnApportionedAmount.IsEmpty)
			{
				Master.UnApportionedAmountInfo.AddError(Res.GetString("eee8463e-14d2-4aqb-b347-7592f7755cac", "Please ensure that this Sell Amount is fully apportioned."));
			}
		}

		protected virtual void CheckSellAmount()
		{
			MandatoryValidation.CheckEntered(Master.SellAmountInfo);
		}

		protected virtual void CheckCurrency()
		{
			MandatoryValidation.CheckEntered(Master.CurrencyInfo);
		}

		protected virtual void CheckCostGovtChargeCode()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				MandatoryValidation.CheckEntered(Master.CostGovtChargeCodeInfo);
			}
		}

		protected virtual void CheckSellGovtChargeCode()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				MandatoryValidation.CheckEntered(Master.SellGovtChargeCodeInfo);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected virtual void CheckDescription()
		{
			if (!AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value && !Master.ChargeCode.IsEmpty && Master.ChargeCode.IsValid)
			{
				var chargeCode = new BusinessObjectFactory().Load<AccChargeCode>(Master.ChargeCode);
				if (chargeCode != null && !Master.Description.StartsWith(chargeCode.AC_Desc, StringComparison.OrdinalIgnoreCase))
				{
					Master.DescriptionInfo.AddWarning(Res.GetString("ee873fc1-e5d0-48d8-a851-bb10852e1591", "Charge description was changed from default. This description will appear on AR Invoice without translation."));
				}
			}
		}

		protected virtual void CheckSellSupplyType()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				ListValidation.ErrorIfInvalidCode(Master.SellSupplyTypeInfo);

				if (!Master.SellSupplyTypeInfo.HasErrors() && Master.SellSupplyType.IsEmpty)
				{
					Master.SellSupplyTypeInfo.AddWarning(Res.GetString("A0D61834-FF39-4D00-95FA-966F043BB3E6", "The Sell Supply Type is not specified. Please check if a supply type is needed before posting."));
				}
			}
		}

		readonly ConsolRevenue Master;
	}
}
