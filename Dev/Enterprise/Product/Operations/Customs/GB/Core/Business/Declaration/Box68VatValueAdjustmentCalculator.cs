using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	class Box68VatValueAdjustmentCalculator
	{
		public void CalculateVATAdjustmentBox68(JobDeclaration declaration)
		{
			SetCalculatedAmount(0m, declaration);
			if (declaration.IsAir)
			{
				var poundsSterlingPerKilo = GBCustomsDataRegistry.Instance.VAT_AdjustmentRatePerKiloAir.Value;
				var minAdjAmt = GBCustomsDataRegistry.Instance.VAT_AdjustmentDeminimusAir.Value;
				var chargeableWeight = GetChargeableWeightInKilos(declaration);
				var vatAdjAmt = chargeableWeight * poundsSterlingPerKilo;
				if (vatAdjAmt <= minAdjAmt)
				{
					SetCalculatedAmount(minAdjAmt, declaration);
				}
				else
				{
					SetCalculatedAmount(vatAdjAmt, declaration);
				}
			}
			else if (declaration.IsSea || declaration.IsRoad)
			{
				var fCLFee = 0m;
				var lCLFee = 0m;
				foreach (Eu.CusContainer container in declaration.CusContainers)
				{
					if (container.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.FCL ||
						container.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.FCLMixedShipper)
					{
						fCLFee += GBCustomsDataRegistry.Instance.VAT_AdjustmentFCLFeeSeaAndRoad.Value;
					}
					else if (container.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.LCL)
					{
						var weight = new ZWeight(container.CO_Weight, container.CO_WeightUQ);

						if (!weight.IsValid)
						{
							throw new InvalidOperationException("A container's weight UQ is not correct. Please update it.");
						}

						var weightInKilos = weight.InKilograms;
						lCLFee += (GBCustomsDataRegistry.Instance.VAT_AdjustmentLCLRateSeaAndRoad.Value * weightInKilos / 1000m);
					}
				}
				if (lCLFee > 0)
				{
					lCLFee += GBCustomsDataRegistry.Instance.VAT_AdjustmentLCLFlatFeeSeaAndRoad.Value;
					if (lCLFee < GBCustomsDataRegistry.Instance.VAT_AdjustmentLCLDeminimusSeaAndRoad.Value)
					{
						lCLFee = GBCustomsDataRegistry.Instance.VAT_AdjustmentLCLDeminimusSeaAndRoad.Value;
					}
				}
				SetCalculatedAmount(fCLFee + lCLFee, declaration);
			}
		}

		void SetCalculatedAmount(decimal fee, JobDeclaration declaration)
		{
			if (declaration.ZG_ManualCalc)
			{
				declaration.ZG_VATAdjAmt = fee;
				declaration.ZG_RX_NKVATAdj = Core.Constants.CurrencyCodes.UnitedKingdom;
			}
			else
			{
				var vat = declaration.TopGroupInvoice.Charges.OfType<JobComInvCharge>().FirstOrDefault(c => c.J7_ChargeType == ChargesProvider.VATAdjustmentCode);
				if (vat == null)
				{
					vat = declaration.TopGroupInvoice.Charges.AddNew();
					vat.J7_ChargeType = ChargesProvider.VATAdjustmentCode;
				}
				vat.J7_Amount = fee;
				vat.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			}
		}

		ZDecimal GetChargeableWeightInKilos(Eu.JobDeclaration jobDeclaration)
		{
			if (jobDeclaration.Shipment != null)
			{
				return jobDeclaration.Shipment.JS_ActualChargeable; // already in kilos for air
			}
			else
			{
				string weightUnit = FreightUtilities.IsValidWeightUnit(jobDeclaration.JE_TotalWeightUnit) ? jobDeclaration.JE_TotalWeightUnit.ToString() : Env.Registry.FreightWeightUnit;
				return ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
				{
					Weight = new ZWeight(jobDeclaration.JE_TotalWeight, weightUnit),
					Volume = jobDeclaration.Volume,
					TargetUnit = Core.Constants.Weight.Kilograms,
					ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, jobDeclaration.JE_TransportMode, Core.Constants.Weight.Kilograms)
				}).Chargeable.Amount;
			}
		}
	}
}
