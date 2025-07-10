using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class ConsolRevenue : NonPersistentBusinessObject, IApportionedChargesHeader
	{
		#region Schema
		public static class Schema
		{
			public const string CostGovtChargeCode = "CostGovtChargeCode";
			public const string SellGovtChargeCode = "SellGovtChargeCode";
			public const string SellSupplyType = "SellSupplyType";
		}

		#endregion
		public ConsolRevenue(ConsolRevenueMaster master)
			: base(master.Factory)
		{
			this.Consol = master.Consol;
			this.Master = master;

			ChargeCodeInfo.ValueChanged += delegate
			{ ChargeCodeChanged(); };
			CurrencyInfo.ValueChanged += delegate
			{ CurrencyChanged(); };
			CurrencyInfo.ValueChanged += delegate
			{ ApportionRevenue(); };
			SellAmountInfo.ValueChanged += delegate
			{ ApportionRevenue(); };
			ApportionmentMethodInfo.ValueChanged += delegate
			{ ApportionRevenue(); };
			ChargeCodeInfo.ValueChanged += delegate
			{ ApportionRevenue(); };
			DescriptionInfo.ValueChanged += delegate
			{ DescriptionChanged(); };
			CostGovtChargeCodeInfo.ValueChanged += delegate
			{ CostGovtChargeCodeChanged(); };
			SellSupplyTypeInfo.ValueChanged += delegate
			{ SellSupplyTypeChanged(); };
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ApportionmentMethod = AllocationMethod.ChargeableUnits;
			Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#region Properties

		#region Description

		[MaxLength(1024)]
		public ZString Description
		{
			get { return fDescription; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDescription();
				}
				DescriptionInfo.RefreshBinding();
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		protected bool Description_ReadOnly
		{
			get
			{
				var code = Factory.Load<AccChargeCode>(ChargeCode);
				return code != null && !(code.AC_AllowDescriptionOvertype && Env.Security.ApportionRevToShipmentModifyDefaultChargeCodeDescription.IsAllowed);
			}
		}

		#endregion

		#region ChargeCode

		[MaxLength(3)]
		[List("Lookups.ChargeCodes")]
		public ZGuid ChargeCode
		{
			get { return fChargeCode; }
			set
			{
				SetNonPersistentPropertyValue(ChargeCodeInfo, ref fChargeCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateChargeCode();
				}
				ChargeCodeInfo.RefreshBinding();
				ChargeCodeChanged();
			}
		}
		ZGuid fChargeCode;

		public ZPropertyInfo ChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCode)); }
		}

		#endregion

		#region Currency

		[MaxLength(3)]
		[List("Lookups.Currencies")]
		public ZString Currency
		{
			get { return fCurrency; }
			set
			{
				SetNonPersistentPropertyValue(CurrencyInfo, ref fCurrency, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCurrency();
				}
				CurrencyInfo.RefreshBinding();
			}
		}

		ZString fCurrency;

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		#endregion

		#region SellAmount

		public ZDecimal SellAmount
		{
			get { return fSellAmount; }
			set
			{
				SetNonPersistentPropertyValue(SellAmountInfo, ref fSellAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSellAmount();
					Validation.ValidateChargeCode();
					Validation.ValidateApprMethod();
					Validation.ValidateUnapportionedAmount();
				}
			}
		}

		ZDecimal fSellAmount;

		public ZPropertyInfo SellAmountInfo
		{
			get { return GetZPropertyInfo(nameof(SellAmount)); }
		}

		#endregion

		#region ApportionmentMethod

		[MaxLength(5)]
		[List("Lookups.ApportionmentMethodList")]
		public ZString ApportionmentMethod
		{
			get { return fApportionmentMethod; }
			set
			{
				SetNonPersistentPropertyValue(ApportionmentMethodInfo, ref fApportionmentMethod, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateApprMethod();
				}
				ApportionmentMethodInfo.RefreshBinding();
			}
		}

		ZString fApportionmentMethod;

		public ZPropertyInfo ApportionmentMethodInfo
		{
			get { return GetZPropertyInfo(nameof(ApportionmentMethod)); }
		}

		#endregion

		#region UnapportionedAmount

		public ZDecimal UnApportionedAmount
		{
			get
			{
				ZDecimal apportionedChargesTotal = 0;
				foreach (var charge in SplitCharges)
				{
					apportionedChargesTotal += charge.JR_OSSellAmt;
				}

				return SellAmount - apportionedChargesTotal;
			}
		}

		public ZPropertyInfo UnApportionedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(UnApportionedAmount)); }
		}

		#endregion

		#region Charge collection

		public RevenueSplitChargeColllection SplitCharges
		{
			get
			{
				if (fSplitCharges == null)
				{
					fSplitCharges = new RevenueSplitChargeColllection(Factory);

					foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter.ShipmentsList)
					{
						Job.Loader loader = new Job.Loader(Factory, shipment);
						Job shipmentJob = loader.TryLoadOrCreateWithMutex();

						if (shipmentJob == null)
						{
							Master.ReleaseMutexesAndRaiseJobCreationExceptionEvent(loader.GetJobCreationError());
							break;
						}

						Master.OnJobWithMutexCreated(shipmentJob);

						if (shipmentJob != null)
						{
							shipmentJob.Charges.Load();

							Charge charge = Factory.New<Charge>();
							charge.ParentConsolRevenue = this;
							fSplitCharges.Add(charge);

							unchecked
							{
								charge.JR_DisplaySequence = shipmentJob.Charges.GetUniqueSequenceNumberForUnpostedCharges((short)((short)GetBiggestSequenceNumber(shipmentJob) + (short)Master.Revenues.Count));
							}

							charge.JR_JH = shipmentJob.PK;
							charge.JR_GB = shipmentJob.JH_GB;
							charge.JR_GE = shipmentJob.JH_GE;
							charge.IsUsedForApportionment = true;
							charge.ChargeableUnitForRevenueApportionment = Consol.CostSupporter.TotalChargeableUnit;
						}
					}

					RegisterEditableChildObject(fSplitCharges);
				}
				return fSplitCharges;
			}
		}

		public override void Delete()
		{
			base.Delete();
			SplitCharges.DeleteAll();
		}

		RevenueSplitChargeColllection fSplitCharges;

		#endregion

		#region Cost Govt Charge Code

		public ZString CostGovtChargeCode
		{
			get { return costGovtChargeCode; }
			set
			{
				SetNonPersistentPropertyValue(CostGovtChargeCodeInfo, ref costGovtChargeCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCostGovtChargeCode();
				}

				CostGovtChargeCodeInfo.RefreshBinding();
				CostGovtChargeCodeChanged();
			}
		}
		ZString costGovtChargeCode;

		public ZPropertyInfo CostGovtChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CostGovtChargeCode)); }
		}

		protected bool CostGovtChargeCode_ReadOnly
		{
			get { return !Env.Security.ApportionRevToShipmentCostGovtCode.IsAllowed; }
		}

		#endregion

		#region Sell Govt Charge Code

		public ZString SellGovtChargeCode
		{
			get { return sellGovtChargeCode; }
			set
			{
				SetNonPersistentPropertyValue(SellGovtChargeCodeInfo, ref sellGovtChargeCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSellGovtChargeCode();
				}

				SellGovtChargeCodeInfo.RefreshBinding();
				SellGovtChargeCodeChanged();
			}
		}
		ZString sellGovtChargeCode;

		public ZPropertyInfo SellGovtChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(SellGovtChargeCode)); }
		}

		protected bool SellGovtChargeCode_ReadOnly
		{
			get { return !Env.Security.ApportionRevToShipmentSellGovtCode.IsAllowed; }
		}

		#endregion

		#region Sell Supply Type

		[MaxLength(3)]
		[List("Lookups.SupplyTypes")]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Accounting.Business.ConsolRevenue.ConsolRevenue|SellSupplyType", Caption = "Sell Supply Type")]
		public ZString SellSupplyType
		{
			get { return sellSupplyType; }
			set
			{
				SetNonPersistentPropertyValue(SellSupplyTypeInfo, ref sellSupplyType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSellSupplyType();
				}

				SellSupplyTypeInfo.RefreshBinding();
				SellSupplyTypeChanged();
			}
		}
		ZString sellSupplyType;

		public ZPropertyInfo SellSupplyTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SellSupplyType)); }
		}

		#endregion

		#endregion

		#region IApportionedChargesHeader

		IApportionedCharge[] IApportionedChargesHeader.Charges
		{
			get { return SplitCharges.ToArray<Charge>(); }
		}

		RefCurrency IApportionedChargesHeader.Currency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency); }
		}

		AccChargeCode IApportionedChargesHeader.ChargeCode
		{
			get { return Factory.Load<AccChargeCode>(ChargeCode); }
		}

		bool IApportionedChargesHeader.IsChargeReadyToPost(IApportionedCharge charge)
		{
			var revenueCharge = charge as Charge;
			return revenueCharge != null && revenueCharge.JR_OSSellAmt != 0M && !revenueCharge.IsRevenuePosted;
		}

		ZDecimal IApportionedChargesHeader.FreeSpace => Consol.CostSupporter.FreeSpace;

		#endregion

		#region Validation

		public ConsolRevenueValidation Validation
		{
			get { return new ConsolRevenueValidation(this); }
		}

		#endregion

		#region Implementation

		ZShort GetBiggestSequenceNumber(Job job)
		{
			short result = 0;
			foreach (Charge charge in job.Charges)
			{
				if (charge.JR_DisplaySequence > result)
				{
					result = charge.JR_DisplaySequence;
				}
			}
			return result;
		}

		public void ApportionRevenue()
		{
			IApportionedChargesHeader header = this;

			if (!SellAmount.IsEmpty && header.ChargeCode != null && header.Currency != null)
			{
				header.Apportion(JobChargeSchema.JR_OSSellAmt, SellAmount, null, 0);
			}
		}

		void CurrencyChanged()
		{
			foreach (Charge charge in SplitCharges)
			{
				charge.JR_RX_NKSellCurrency = Currency;
			}
		}

		void ChargeCodeChanged()
		{
			foreach (Charge charge in SplitCharges)
			{
				using (charge.SuspendSplittingApportionAmountChangeIsUsedForApportionment())
				{
					charge.JR_AC = ChargeCode;
				}
			}
			Description = SplitCharges.Count == 0 ? ZString.Empty : SplitCharges[0].JR_Desc;
			CostGovtChargeCode = SplitCharges.Count == 0 ? ZString.Empty : SplitCharges[0].JR_CostGovtChargeCode;
			SellGovtChargeCode = SplitCharges.Count == 0 ? ZString.Empty : SplitCharges[0].JR_SellGovtChargeCode;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		void DescriptionChanged()
		{
			AccChargeCode code = Factory.Load<AccChargeCode>(ChargeCode);
			if (code != null && Description != code.AC_Desc && Description != code.AC_LocalLanguageDescription)
			{
				foreach (Charge charge in SplitCharges)
				{
					charge.JR_Desc = Description;
				}
			}
		}

		void CostGovtChargeCodeChanged()
		{
			foreach (Charge charge in SplitCharges)
			{
				charge.JR_CostGovtChargeCode = CostGovtChargeCode;
			}
		}

		void SellGovtChargeCodeChanged()
		{
			foreach (Charge charge in SplitCharges)
			{
				charge.JR_SellGovtChargeCode = SellGovtChargeCode;
			}
		}

		void SellSupplyTypeChanged()
		{
			foreach (Charge charge in SplitCharges)
			{
				charge.JR_SellSupplyType = SellSupplyType;
			}
		}

		internal readonly IJobCostingPlugIn Consol;
		readonly ConsolRevenueMaster Master;

		#endregion

		#region Lookups

		public ConsolRevenueLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ConsolRevenueLookups(this);
				}
				return fLookups;
			}
		}
		ConsolRevenueLookups fLookups;

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			RemoveNonApplicableCharges();
		}

		void RemoveNonApplicableCharges()
		{
			var charges = SplitCharges.ToArray<Charge>();
			foreach (var charge in charges)
			{
				if (charge != null && !charge.IsDeleted)
				{
					if (charge.JR_OSSellAmt == 0m)
					{
						var job = charge.Job;
						if (job != null && !job.IsInDatabase)
						{
							job.Dispose();
							job.Delete();
						}

						SplitCharges.RemoveFromRelationship(charge);
						charge.Delete();
					}
					else
					{
						charge.Calculations.UpdateCostBasedOnRevenueForCleanup();
					}
				}
			}
		}
	}
}

