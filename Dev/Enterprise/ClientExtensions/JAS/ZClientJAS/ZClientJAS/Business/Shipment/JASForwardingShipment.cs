using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business
{
	[Enterprise.Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.NotApplied, "Enterprise.Client.JAS.Metadata.JASForwardingShipment, ZClientJAS, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")]
	public class JASForwardingShipment : ForwardingShipment
	{
		public enum SuitableForJXC
		{
			NotAPreShipment,
			InvalidShipmentTransportMode,
			Suitable
		}

		public JASForwardingShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal GrossWeightInKilograms
		{
			get
			{
				return (JS_UnitOfWeight != Core.Constants.Weight.Kilograms)
					? (ZDecimal)Core.Constants.Weight.Convert(JS_ActualWeight, JS_UnitOfWeight, Core.Constants.Weight.Kilograms)
					: JS_ActualWeight;
			}
		}

		public ZDecimal MeasurementInCubicMetres
		{
			get
			{
				return (JS_UnitOfVolume != Core.Constants.Volume.CubicMetres)
					? (ZDecimal)Core.Constants.Volume.Convert(JS_ActualVolume, JS_UnitOfVolume, Core.Constants.Volume.CubicMetres)
					: JS_ActualVolume;
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (JASDataRegistry.Instance.EnableAutoJXCMessaging && !JASDataRegistry.Instance.DisableGsumMessaging)
			{
				GsumMessageExporter.CheckAndCreateGsumLinesToBeSent();
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (SaveSucceededOverridesForTesting.HasValue)
			{
				saveSucceeded = SaveSucceededOverridesForTesting.Value;
			}

			base.OnFactorySaved(saveSucceeded);
			if (JASDataRegistry.Instance.EnableAutoJXCMessaging && !JASDataRegistry.Instance.DisableGsumMessaging)
			{
				if (saveSucceeded)
				{
					GsumMessageExporter.WriteToFile();
				}
				else
				{
					GsumMessageExporter.ClearCurrentGsumEvents();
				}
			}
		}

		GsumMessageExporter GsumMessageExporter
		{
			get
			{
				if (fGsumMessageExporter == null)
				{
					fGsumMessageExporter = new GsumMessageExporter(this);
				}
				return fGsumMessageExporter;
			}
		}

		public bool? SaveSucceededOverridesForTesting;

		GsumMessageExporter fGsumMessageExporter;

		#region Shipment Job & Charges

		public new JASJob Job
		{
			get { return (JASJob)base.ShipmentJobHeader; }
		}

		public ZDecimal TotalFreightRevenue
		{
			get
			{
				ZDecimal result = 0;

				foreach (JobCharge charge in FreightCharges)
				{
					result += GetLocalSellAmount(charge);
				}

				return result;
			}
		}

		public ZDecimal TotalFreightCost
		{
			get
			{
				ZDecimal result = 0;

				foreach (JobCharge charge in FreightCharges)
				{
					result += GetLocalCostAmount(charge);
				}

				return result;
			}
		}

		public ZDecimal TotalCost
		{
			get
			{
				ZDecimal result = 0;

				if (Job != null)
				{
					foreach (JobCharge charge in Job.Charges)
					{
						result += GetLocalCostAmount(charge);
					}
				}

				return result;
			}
		}

		public ZDecimal TotalOtherCosts
		{
			get { return TotalCost - TotalFreightCost; }
		}

		public ZDecimal AgentDeclaredGrossProfit
		{
			get { return Math.Abs(TotalFreightRevenue - TotalCost); }
		}

		public ZDecimal GetProfitShareDueDestination(JASOrgHeader sellAccount)
		{
			ZDecimal result = 0;

			ZQuery additionalFilter = new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			foreach (JobCharge charge in GetCollectCharges(sellAccount, additionalFilter))
			{
				result += GetLocalSellAmount(charge);
			}

			return Math.Abs(result);
		}

		public ZDecimal GetTotalCollectChargesWithoutProfitShare(JASOrgHeader sellAccount)
		{
			ZDecimal result = 0;

			ZQuery additionalFilter = new ZQuery(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			foreach (JobCharge charge in GetCollectCharges(sellAccount, additionalFilter))
			{
				result += GetLocalSellAmount(charge);
			}

			return result;
		}

		public JobCharge[] GetCollectCharges(JASOrgHeader sellAccount)
		{
			ZQuery additionalFilter = null;
			if (!JASDataRegistry.Instance.IncludeProfitShareCharges)
			{
				additionalFilter = new ZQuery(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			}
			return GetCollectCharges(sellAccount, additionalFilter);
		}

		JobCharge[] GetCollectCharges(JASOrgHeader sellAccount, ZQuery additionalFilter)
		{
			JobCharge[] result = Array.Empty<JobCharge>();

			if (Job != null && sellAccount != null)
			{
				ZQuery filter = new ZQuery(JobChargeSchema.JR_OH_SellAccount, sellAccount.PK);
				if (additionalFilter != null)
				{
					filter.AddToFilter(additionalFilter);
				}
				result = (JobCharge[])Job.Charges.Find(filter);
			}

			return result;
		}

		JobCharge[] FreightCharges
		{
			get
			{
				JobCharge[] result = Array.Empty<JobCharge>();

				if (Job != null)
				{
					ZQuery filter = new ZQuery(JobChargeSchema.JR_AC, Env.Registry.FreightChargeCode);
					result = (JobCharge[])Job.Charges.Find(filter);
				}

				return result;
			}
		}

		ZDecimal GetLocalCostAmount(JobCharge jobCharge)
		{
			return (!jobCharge.JR_AgentDeclaredCostAmtLocal.IsEmpty) ? jobCharge.JR_AgentDeclaredCostAmtLocal : jobCharge.JR_LocalCostAmt;
		}

		ZDecimal GetLocalSellAmount(JobCharge jobCharge)
		{
			return (!jobCharge.JR_AgentDeclaredSellAmtLocal.IsEmpty) ? jobCharge.JR_AgentDeclaredSellAmtLocal : jobCharge.JR_LocalSellAmt;
		}

		#endregion

		#region IsSuitableForJXC

		public SuitableForJXC IsSuitableForJXC()
		{
			SuitableForJXC result;

			if (Consols.Count > 0)
			{
				result = SuitableForJXC.NotAPreShipment;
			}
			else if (!IsAir && !IsSea)
			{
				result = SuitableForJXC.InvalidShipmentTransportMode;
			}
			else
			{
				result = SuitableForJXC.Suitable;
			}

			return result;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (IsAir)
			{
				object lazyLoadAWBPlease = AWBHeader;
			}
		}

		protected override ShipmentDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			ShipmentDocAddressValidation result = base.PiggyBackedDocAddressValidation(addressToValidate);

			JobDocAddressValidation extraValidation = JXCValidation.JobDocAddressValidation(addressToValidate);
			if (extraValidation != null)
			{
				result.Add(extraValidation);
			}

			return result;
		}

		public virtual
 JXCForwardingShipmentValidation JXCValidation
		{
			get
			{
				if (Factory.HasDomainValidation)
				{
					foreach (DomainValidationGroup validationGroup in Factory.Validation.AllValidationGroups)
					{
						ZValidation[] validations = validationGroup.CreateDomainValidation(this);
						foreach (ZValidation validation in validations)
						{
							JXCForwardingShipmentValidation result = validation as JXCForwardingShipmentValidation;
							if (result != null)
							{
								return result;
							}
						}
					}
				}

				return new JXCForwardingShipmentValidation(this);
			}
		}

		#endregion
	}
}
