using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Environment;
using AuthorisationRequirementCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using CostVarianceKey = Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalHelper.CostVarianceKey;
using PrepaidCollectCodes = Enterprise.Accounting.Integration.PrepaidCollectFreightForwardingList.Codes;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ApportionSplitCharge : BaseCharge, IApportionedCharge, IApportionmentChargeToImport, IComplianceInfoToImport
	{
		#region Schema

		public new abstract class Schema : BaseCharge.Schema
		{
			public const string JR_JR = "JR_JR";
			public const string JR_HouseBill = "JR_HouseBill";
			public const string JR_PrepaidCollect = "JR_PrepaidCollect";
			public const string JR_IsUsedForApportionment = "JR_IsUsedForApportionment";
			public const string JR_ShipmentNumberOfColoadMaster = "JR_ShipmentNumberOfColoadMaster";
		}

		#endregion

		public ApportionSplitCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Lookups = new ApportionSplitChargeLookups(this);
		}

		public override ZBool JR_SellRated
		{
			get { return base.JR_SellRated; }
			set
			{
				if (value && !Globals.IsTest)
				{
					ErrorReporter.ReportOnce("Apportionment charge SellRated is set to true");
				}

				base.JR_SellRated = value;
			}
		}

		[ReadOnly(true)]
		public override ZBool JR_IsCostTaxAmountOverridden
		{
			get { return base.JR_IsCostTaxAmountOverridden; }
			set { base.JR_IsCostTaxAmountOverridden = value; }
		}

		#region Related Business Objects

		public override IJobInvoicingPlugIn ShipmentInfo => RelatedJob ?? shipmentInfo;
		IJobInvoicingPlugIn shipmentInfo;

		internal override IJobInvoicingPlugIn RelatedJob => (ParentConsolCost?.Consol?.IsGatewayBillingEnabled() ?? false) ? base.RelatedJob : null;

		internal void SetShipmentInfo(IJobInvoicingPlugIn value)
		{
			shipmentInfo = value;
		}

		internal bool IsGatewaySellApportionmentCharge => JR_E6_GatewaySellHeader.IsValid;

		bool IsConsolCostApprovingPosting
		{
			get { return ParentConsolCost != null && ParentConsolCost.IsApprovingPosting; }
		}

		bool IsConsolCostImported
		{
			get { return ParentConsolCost != null && ParentConsolCost.IsImportedConsolCost; }
		}

		public bool IsParentConsolCostImported
		{
			get { return RelatedApportionChargeFromDB != null; }
		}

		public JobCharge RelatedApportionChargeFromDB { get; set; }

		bool IsBranchDepartmentReadOnly
		{
			get { return IsConsolCostApprovingPosting || IsParentConsolCostImported || IsConsolCostImported; }
		}

		public bool ValidateIfRelatedApportionChargeCanBeMarkedAsImported(JobCharge relatedApportionCharge)
		{
			return relatedApportionCharge != null && relatedApportionCharge.JR_JH == JR_JH && relatedApportionCharge.JR_AC == JR_AC && relatedApportionCharge.JR_GB == JR_GB && relatedApportionCharge.JR_GE == JR_GE;
		}

		#endregion

		#region Properties

		public override bool JR_Calc_RelatedJobNumber_ReadOnly => true;

		[List(nameof(Lookups) + "." + nameof(ApportionSplitChargeLookups.ApportionTargetJobNumbers))]
		public override ZString JR_JobNumber
		{
			get
			{
				return base.JR_JobNumber;
			}
			set
			{
				var newOperationalJob = this.ApportionTargets().SingleOrDefault(x => x.JobNumber == value);
				Job newJob = TryLoadOrCreateJob(newOperationalJob);

				if (newJob != null && JR_JH != newJob.PK)
				{
					var newOpJob = newJob.Parent;
					var previousOpJob = InvoicingJob.Parent;

					var newOpJobIsShipment = (!(newOpJob is IJobCostingPlugIn)) && newOpJob is IJobInvoicingPlugIn;
					var previousOpJobIsShipment = (!(previousOpJob is IJobCostingPlugIn)) && previousOpJob is IJobInvoicingPlugIn;

					if (newOpJobIsShipment)
					{
						JR_Calc_RelatedJobNumber = ZString.Empty;
					}
					else if (previousOpJobIsShipment)
					{
						JR_Calc_RelatedJobNumber = previousOpJob.JobNumber;
					}

					JR_JH = newJob.PK;
					JR_GB = newJob.JH_GB;
					JR_GE = newJob.JH_GE;

					SetNonPersistentPropertyValue(JR_JobInvoiceNumberInfo, ref jr_jobNumber, value);
					ResetChargeDebtor();
				}
				else
				{
					JR_JobNumberInfo.RefreshBinding();
				}
			}
		}
		ZString jr_jobNumber;

		Job TryLoadOrCreateJob(IJobInvoicingPlugIn jobParent)
		{
			return jobParent != null
				? ParentConsolCost?.Consol?.GetApportionments(true)?.TryLoadOrCreateJobWithMutexAndTracking(jobParent)
				: null;
		}

		public bool JR_JobNumber_ReadOnly => IsInDatabase || !ParentConsolCost.IsGatewayConsolCost;

		protected override void SetJR_RX_NKSellCurrencyCore(ZString value)
		{
			var charge = LoadChargeInstanceToHandleSavingAndDeleting();
			if (charge != null)
			{
				charge.JR_RX_NKSellCurrency = value;
			}
			else
			{
				base.SetJR_RX_NKSellCurrencyCore(value);
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (ParentConsolCost != null && (ParentConsolCost.IsPosted && !IsConsolCostApprovingPosting)); }
			set { base.ReadOnly = value; }
		}

		public override bool IsSavedByFactory
		{
			get
			{
				return (IsDeleted || (InvoicingJob != null && (InvoicingJob.PlugInData == null || !InvoicingJob.IsPluginDeleted))) &&
					 base.IsSavedByFactory;
			}
		}

		protected override ZStringBuilder GetIsSavedByFactoryEvaluationInfoCore()
		{
			var msgBuilder = base.GetIsSavedByFactoryEvaluationInfoCore();
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => InvoicingJob != null, "IsInvoicingJobNotNull"));
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => InvoicingJob != null && InvoicingJob.PlugInData == null, "IsInvoicingPlugInDataNull"));
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => InvoicingJob != null && InvoicingJob.IsPluginDeleted, nameof(InvoicingJob.IsPluginDeleted)));
			return msgBuilder;
		}

		protected override void OnFactorySavingBeforeTransactionCoreInCompanyContext()
		{
			LoadChargeInstanceToHandleSavingAndDeleting();
			base.OnFactorySavingBeforeTransactionCoreInCompanyContext();
		}

		public override void Delete()
		{
			LoadChargeInstanceToHandleSavingAndDeleting();
			base.Delete();
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (IsValidChargeForCalculatingCostVarianceApproval)
			{
				AutoTickFinalFlag(new CostVarianceKey(this));
			}
		}

		ZGuid consolCostPKForDelete;

		/// <summary>
		/// Load as Charge only if this ApportionSplitCharge exists in database and it is not deleted. 
		/// As 'In-Memory' ApportionSplitCharge is going to be deleted before saving in the transformer, there is no point of loading it as charge
		/// </summary>
		/// <returns></returns>
		Charge LoadChargeInstanceToHandleSavingAndDeleting()
		{
			Charge result = null;
			// Get Charge to handle WIP/ACR and other extended functionality
			if (!IsDeleted && IsInDatabase)
			{
				result = Factory.Load<Charge>(PK);
			}
			return result;
		}

		protected override bool CalculateAPInvoiceOnJobCharges
		{
			get { return false; }
		}

		public void ClearCostData()
		{
			JR_E6 = ZGuid.Empty;
			JR_OSCostAmt = 0;
			JR_OH_CostAccount = ZGuid.Empty;
			JR_APInvoiceNum = ZString.Empty;
			JR_APInvoiceDate = ZDateTime.Empty;
			JR_PaymentDate = ZDateTime.Empty;
			JR_APDocumentReceivedDate = ZDateTime.Empty;
			JR_CostReference = ZString.Empty;
			JR_AB = ZGuid.Empty;
			UpdateChequeBook(ZGuid.Empty);
			UpdateChequeNumberWithoutUpdatingChequeBook(ZString.Empty);
		}

		internal void ClearCostSide()
		{
			ReverseAccrual(ZDateTime.Now);
			ClearCostAmount();
			ClearCostData();
		}

		public void RestoreSellData()
		{
			RestoreIfRequired(JR_ACInfo);
			RestoreIfRequired(JR_GBInfo);
			RestoreIfRequired(JR_GEInfo);

			RestoreIfRequired(JR_AL_ARLineInfo);
			RestoreIfRequired(JR_OH_SellAccountInfo);

			RestoreIfRequired(JR_OA_SellInvoiceAddressInfo);
			RestoreIfRequired(JR_OC_SellInvoiceContactInfo);

			RestoreIfRequired(JR_AT_SellGSTRateInfo);
			RestoreIfRequired(JR_A9_SellVATClassInfo);

			RestoreIfRequired(JR_AgentDeclaredSellAmtInfo);
			RestoreIfRequired(JR_AW_SellWHTRateInfo);

			RestoreIfRequired(JR_OSSellExRateInfo);
			RestoreIfRequired(JR_RX_NKSellCurrencyInfo);

			RestoreIfRequired(JR_LocalSellAmtInfo);
			RestoreIfRequired(JR_OSSellAmtInfo);

			RestoreIfRequired(JR_OSSellWHTAmtInfo);

			RestoreIfRequired(JR_SellRatedInfo);
			RestoreIfRequired(JR_SellRatingOverrideInfo);
			RestoreIfRequired(JR_SellRatingOverrideCommentInfo);

			RestoreIfRequired(JR_SellReferenceInfo);
		}

		void RestoreIfRequired(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.HasChanges)
			{
				propertyInfo.Value = propertyInfo.OriginalValue;
			}
		}

		public void UpdateCostData()
		{
			using (SuspendAmountsCalculations())
			{
				JR_E6 = ZGuid.Empty;
				JR_RX_NKCostCurrency = JR_LocalCurrencyCode;
				JR_OSCostExRate = 1M;

				if (IsDisbursementCharge)
				{
					JR_OSCostAmt = JR_LocalSellAmt;
				}
				else if (IsMarginCharge)
				{
					JR_OSCostAmt = JR_LocalSellAmt * MarginPercentage / 100;
				}
				else if (IsManualJobAccrualCharge)
				{
					JR_OSCostAmt = 0m;
				}

				SetLocalCostAmt();
			}
		}

		public ZBool IsChargeRevenueEdited
		{
			get
			{
				var result = false;
				Charge charge = LoadChargeInstanceToHandleSavingAndDeleting();
				if (charge != null)
				{
					ZDecimal defaultSellAmt = charge.GetRevenueAmountBasedOnCost();
					ZDecimal defaultLocalSellAmt = ExchangeRate.ForeignToLocal(charge.JR_OSSellAmt, charge.JR_OSSellExRate);
					result = JR_OSSellAmt != defaultSellAmt || JR_LocalSellAmt != defaultLocalSellAmt;
				}
				return result;
			}
		}

		[ReadOnlyMember(nameof(IsBranchDepartmentReadOnly))]
		[List("Departments")]
		public override ZGuid JR_GE
		{
			get { return base.JR_GE; }
			set
			{
				var oldValue = JR_GE;
				var costVarianceKey = new CostVarianceKey(this);
				base.JR_GE = value;

				if (oldValue != JR_GE)
				{
					AutoTickFinalFlag(costVarianceKey);
				}
			}
		}

		[ReadOnlyMember(nameof(IsBranchDepartmentReadOnly))]
		[List("Branches")]
		public override ZGuid JR_GB
		{
			get { return base.JR_GB; }
			set
			{
				var oldValue = JR_GB;
				var costVarianceKey = new CostVarianceKey(this);
				base.JR_GB = value;

				if (oldValue != JR_GB)
				{
					AutoTickFinalFlag(costVarianceKey);
				}
			}
		}

		public override ZGuid JR_E6
		{
			get
			{
				return base.JR_E6;
			}

			set
			{
				if (!IsDeleted && JR_E6.IsValid && value.IsEmpty)
				{
					consolCostPKForDelete = JR_E6;
				}
				base.JR_E6 = value;
			}
		}

		public override ZDecimal JR_OSCostAmt
		{
			get { return base.JR_OSCostAmt; }
			set
			{
				bool hasChanges = JR_OSCostAmt != value;

				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, hasChanges))
				{
					var oldValue = JR_OSCostAmt;
					base.JR_OSCostAmt = value;

					if (!IsAmountsCalculationsSuspended)
					{
						SetLocalCostAmt();
					}

					if (ParentConsolCost != null && !ParentConsolCost.IsDeleted)
					{
						if (!IsSplittingApportionAmountChangeIsUsedForApportionmentSuspended)
						{
							SetChargeIsUsedForApportionmentWithoutCalulations(ShouldIncludeInApportionment);
						}
						ParentConsolCost.PushLocalAmountExchangeRateDifferencesToGreatestCharge();
						ParentConsolCost.ApportionGSTCharges();

						if (!ParentConsolCost.IsValidationSuspended)
						{
							if (ParentConsolCost.Validation is CommonConsolCostValidation)
							{
								((CommonConsolCostValidation)ParentConsolCost.Validation).ValidateUnApportionedAmount();
							}

							foreach (ApportionSplitCharge charge in ParentConsolCost.ApportionmentCharges)
							{
								charge.Validation.ValidateJR_OSCostAmt();
							}

							CollectCriticalValidationInfo();
						}

						void CollectCriticalValidationInfo()
						{
							if (JR_OSCostAmt != 0 && JR_LocalCostAmt == 0 && ParentConsolCost != null && ParentConsolCost.E6_LocalCostAmount != 0)
							{
								CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountIsZeroWithNonZeroOSAmount, () =>
								{
									return $"Apportion Charge: OS Cost Amount is changed from {oldValue} to {JR_OSCostAmt} with exchange rate {JR_OSCostExRate}.\r\n {System.Environment.StackTrace}";
								}, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
							}

							if (oldValue == 0 || JR_OSCostAmt == 0)
							{
								CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(ParentConsolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero,
								() => $@"OS Cost Amount is changed from {oldValue} to {JR_OSCostAmt}. CostAccount is Empty : {JR_OH_CostAccount.IsEmpty}.
Call stack:
{System.Environment.StackTrace}"
							);
							}
						}
					}
				}
			}
		}

		internal bool ShouldIncludeInApportionment
		{
			get
			{
				var result = false;

				if (ParentConsolCost != null)
				{
					var homeCountry = GlbCompany.CurrentCompany?.Country;
					result = (
								(ParentConsolCost.E6_PPDCLT == PrepaidCollectList.Codes.All || JR_PrepaidCollect == ParentConsolCost.E6_PPDCLT || JR_PrepaidCollect == PrepaidCollectList.Codes.Both)
								&& ((ParentConsolCost.E6_OSCostAmount == 0M && JR_ShipmentNumberOfColoadMaster.IsEmpty) || JR_OSCostAmt != 0M)
							 )
							 || (ParentConsolCost.E6_PPDCLT == PrepaidCollectCodes.CTS && HasContainerCostShare())
							 || (ParentConsolCost.E6_PPDCLT == PrepaidCollectCodes.FOG && IsForeignOrigin(homeCountry))
							 || (ParentConsolCost.E6_PPDCLT == PrepaidCollectCodes.LOG && IsLocalOrigin(homeCountry))
							 || (ParentConsolCost.E6_PPDCLT == PrepaidCollectCodes.FDT && IsForeignDestination(homeCountry))
							 || (ParentConsolCost.E6_PPDCLT == PrepaidCollectCodes.LDT && IsLocalDestination(homeCountry));
				}

				return result;
			}
		}

		internal bool IsForeignOrigin(RefCountry homeCountry) => !(homeCountry == null || ShipmentInfo?.InvoicingSupporter?.Origin == null)
				&& ShipmentInfo.InvoicingSupporter.Origin.RL_RN_NKCountryCode != homeCountry.Code;

		internal bool IsLocalOrigin(RefCountry homeCountry) => !(homeCountry == null || ShipmentInfo?.InvoicingSupporter?.Origin == null)
				&& ShipmentInfo.InvoicingSupporter.Origin.RL_RN_NKCountryCode == homeCountry.Code;

		internal bool IsForeignDestination(RefCountry homeCountry) => !(homeCountry == null || ShipmentInfo?.InvoicingSupporter?.Destination == null)
				&& ShipmentInfo.InvoicingSupporter.Destination.RL_RN_NKCountryCode != homeCountry.Code;

		internal bool IsLocalDestination(RefCountry homeCountry) => !(homeCountry == null || ShipmentInfo?.InvoicingSupporter?.Destination == null)
				&& ShipmentInfo.InvoicingSupporter.Destination.RL_RN_NKCountryCode == homeCountry.Code;

		protected override void SetJR_OSCostExRateChangedCore(ZDecimal value)
		{
			base.SetJR_OSCostExRateChangedCore(value);

			if (!IsAmountsCalculationsSuspended)
			{
				SetLocalCostAmt();
			}
		}

		protected void SetLocalCostAmt()
		{
			JR_LocalCostAmt = ExchangeRate.ForeignToLocal(JR_OSCostAmt, JR_OSCostExRate);
		}

		ZArchitecture.Environment.ExchangeRate ExchangeRate => (Company ?? Env.CurrentCompany).ExchangeRate;

		public override ZDecimal JR_LocalCostAmt
		{
			get { return base.JR_LocalCostAmt; }
			set
			{
				var hasChange = JR_LocalCostAmt != value;

				base.JR_LocalCostAmt = value;
				if (JR_OSCostAmt != 0m && !JR_RX_NKCostCurrency.IsEmpty && JR_OSCostExRate != 0m &&
						Math.Abs(Math.Abs(ExchangeRate.ForeignToLocal(JR_OSCostAmt, JR_OSCostExRate)) - Math.Abs(value)) > 5)
				{
					base.JR_LocalCostAmt = ExchangeRate.ForeignToLocal(JR_OSCostAmt, JR_OSCostExRate);
				}

				if (hasChange)
				{
					AutoTickFinalFlag();
				}
			}
		}

		internal ZBool IsFinalDefault { get; set; }

		void AutoTickFinalFlag(CostVarianceKey costVarianceKey = null)
		{
			var consolCost = IsDeleting ? Factory.Load<JobConsolCost>(consolCostPKForDelete) : ParentConsolCost;

#if DEBUG
			if (consolCost != null && consolCost.ParentAPInvoice != null && consolCost.ParentAPInvoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().IsSuspended)
			{
				consolCost.ParentAPInvoice.SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly++;
			}
#endif

			if ((!IsValidChargeForCalculatingCostVarianceApproval && costVarianceKey == null)
				|| !JR_IsUsedForApportionment
				|| consolCost == null
				|| !(consolCost.ParentAPInvoice is APInvoice)
				|| !consolCost.ParentAPInvoice.ConsolCosting.CostVarianceApprovalHelper.AutoTickFinalFlag
				|| consolCost.ParentAPInvoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().IsSuspended)
			{
				return;
			}

			var consolCosting = consolCost.ParentAPInvoice.ConsolCosting;
			consolCosting.CostVarianceApprovalHelper.CalculateAuthorisationByLines();

			using (consolCost.ParentAPInvoice.SetDefaultFinalWhenAutoTickingFinalFlagSuspender.GetSuspender())
			{
				if (consolCosting.CostVarianceApprovalHelper.MonitorTotalInvoiceVariance)
				{
					var totalRequirement = consolCosting.CostVarianceApprovalHelper.GetTotalAuthorisationRequirement();
					var totalAppropvalLevel = totalRequirement != null ? totalRequirement.AuthorisationRequirement.ToString() : AuthorisationRequirementCodes.NoApprovalRequired;
					var untickAll = (totalAppropvalLevel != AuthorisationRequirementCodes.NoApprovalRequired);

					foreach (ApportionSplitCharge charge in consolCosting.CostVarianceApprovalHelper.ApportionSplitCharges)
					{
						if (charge.IsValidChargeForCalculatingCostVarianceApproval)
						{
							if (untickAll)
							{
								charge.IsFinal = charge.IsFinalDefault = false;
								continue;
							}

							charge.AutoTickFinalFlagForEachCharge(consolCost);
						}
					}
				}
				else
				{
					if (IsValidChargeForCalculatingCostVarianceApproval && !IsDeleting)
					{
						AutoTickFinalFlagForEachCharge(consolCost);
					}

					if (costVarianceKey != null && costVarianceKey.IsValid)
					{
						ChangeFinalFlagForOriginalGroup(consolCost, costVarianceKey);
					}
				}
			}
		}

		void AutoTickFinalFlagForEachCharge(JobConsolCost consolCost)
		{
			var requirement = consolCost.ParentAPInvoice.ConsolCosting.CostVarianceApprovalHelper.GetLineAuthorisationRequirement(this);
			var appropvalLevel = requirement != null ? requirement.AuthorisationRequirement.ToString() : AuthorisationRequirementCodes.NoApprovalRequired;
			IsFinal = IsFinalDefault = (appropvalLevel == AuthorisationRequirementCodes.NoApprovalRequired);
		}

		void ChangeFinalFlagForOriginalGroup(JobConsolCost consolCost, CostVarianceKey costVarianceKey)
		{
			var charges = consolCost.ParentAPInvoice.ConsolCosting.CostVarianceApprovalHelper.ApportionSplitCharges.Where(charge => costVarianceKey == new CostVarianceKey(charge)).ToList();

			foreach (var charge in charges)
			{
				try
				{
					charge.IsSetAL_IsFinalByGroup = true;
					charge.AutoTickFinalFlagForEachCharge(consolCost);
				}
				finally
				{
					charge.IsSetAL_IsFinalByGroup = false;
				}
			}
		}

		[ReadOnlyMember(nameof(IsConsolCostApprovingPosting))]
		public ZBool IsFinal
		{
			get { return fIsFinal; }
			set
			{
				fIsFinal = value;

				if (!IsValidationSuspended && ApportionSplitChargeValidation != null)
				{
					ApportionSplitChargeValidation.ValidateIsFinal();
				}

				IsFinalInfo.RefreshBinding();
				if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					MarkAsNeedingValidation();
				}

				if (!IsSetAL_IsFinalByGroup && ParentConsolCost != null && ParentConsolCost.ParentAPInvoice is APInvoice && ParentConsolCost.ParentAPInvoice.ConsolCosting.CostVarianceApprovalHelper.AutoTickFinalFlag)
				{
					var key = new CostVarianceKey(this);

					if (key.IsValid)
					{
						var charges = ParentConsolCost.ParentAPInvoice.ConsolCosting.CostVarianceApprovalHelper.ApportionSplitCharges
							.Where(charge => charge.PK != PK && key == new CostVarianceKey(charge) && (charge.IsFinal != value || charge.IsFinalDefault != value));

						SetAL_IsFinalForRelatedCharges(charges, value, IsFinalDefault);
					}
				}
			}
		}

		internal bool IsValidChargeForCalculatingCostVarianceApproval
		{
			get
			{
				return JR_AC.IsValid && JR_JH.IsValid && JR_GB.IsValid && JR_GE.IsValid;
			}
		}

		void SetAL_IsFinalForRelatedCharges(IEnumerable<ApportionSplitCharge> charges, bool isFinal, bool isFinalDefault)
		{
			foreach (var charge in charges)
			{
				try
				{
					charge.IsSetAL_IsFinalByGroup = true;
					charge.IsFinalDefault = isFinalDefault;
					charge.IsFinal = isFinal;
				}
				finally
				{
					charge.IsSetAL_IsFinalByGroup = false;
				}
			}
		}

		internal bool IsSetAL_IsFinalByGroup { get; set; }

		ApportionSplitChargeValidation ApportionSplitChargeValidation
		{
			get { return Validation as ApportionSplitChargeValidation; }
		}

		public ZPropertyInfo IsFinalInfo
		{
			get { return GetZPropertyInfo(nameof(IsFinal)); }
		}

		ZBool fIsFinal;

		#region JR_JR
		protected ZGuid fJR_JR;
		public ZGuid JR_JR
		{
			get { return fJR_JR; }
			set { SetNonPersistentPropertyValue(JR_JRInfo, ref fJR_JR, value); }
		}

		public ZPropertyInfo JR_JRInfo
		{
			get { return GetZPropertyInfo(Schema.JR_JR); }
		}
		#endregion

		#region JR_HouseBill

		public ZString JR_HouseBill
		{
			get
			{
				var invoicingPlugin = ShipmentInfo ?? Job?.Parent as IJobInvoicingPlugIn;
				return invoicingPlugin?.InvoicingSupporter?.HouseBillNumber ?? ZString.Empty;
			}
		}

		public ZPropertyInfo JR_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.JR_HouseBill); }
		}

		#endregion

		#region JR_PrepaidCollect

		public ZString JR_PrepaidCollect
		{
			get
			{
				var result = ZString.Empty;

				if (ParentConsolCost != null)
				{
					var consol = ParentConsolCost.Consol;
					if (consol != null)
					{
						result = consol.GetPrepaidCollect(ShipmentInfo);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JR_PrepaidCollectInfo
		{
			get { return GetZPropertyInfo(Schema.JR_PrepaidCollect); }
		}

		#endregion

		#region JR_IsUsedForApportionment

		[ReadOnlyMember(nameof(IsBranchDepartmentReadOnly))]
		public ZBool JR_IsUsedForApportionment
		{
			get { return isUsedForApportionment; }
			set
			{
				if (SetNonPersistentPropertyValue(JR_IsUsedForApportionmentInfo, ref isUsedForApportionment, value))
				{
					if (!isUsedForApportionment)
					{
						JR_OSCostAmt = 0M;
						IsFinal = IsFinalDefault = false;
					}
					else
					{
						AutoTickFinalFlag();
					}

					if (ParentConsolCost != null)
					{
						ParentConsolCost.SplitApportionAmount();
						ParentConsolCost.ApportionGSTCharges();
					}

					JR_OSCostAmtInfo.RefreshBinding();
				}

				if (!IsValidationSuspended && ApportionSplitChargeValidation != null)
				{
					ApportionSplitChargeValidation.ValidateJR_IsUsedForApportionment();
				}
			}
		}
		ZBool isUsedForApportionment;

		internal void SetChargeIsUsedForApportionmentWithoutCalulations(ZBool value)
		{
			isUsedForApportionment = value;
		}

		public override bool ShouldValidateBranchAndDepartment
		{
			get { return JR_IsUsedForApportionment; }
		}

		public ZPropertyInfo JR_IsUsedForApportionmentInfo
		{
			get { return GetZPropertyInfo(Schema.JR_IsUsedForApportionment); }
		}

		#endregion

		#region JR_OSCostAmt

		protected bool JR_OSCostAmt_ReadOnly
		{
			get { return !JR_IsUsedForApportionment; }
		}

		#endregion

		#region JR_ShipmentNumberOfColoadMaster

		public ZString JR_ShipmentNumberOfColoadMaster
		{
			get { return ShipmentInfo != null ? ShipmentInfo.InvoicingSupporter.ShipmentNumberOfColoadMaster : ZString.Empty; }
		}

		public ZPropertyInfo JR_ShipmentNumberOfColoadMasterInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ShipmentNumberOfColoadMaster); }
		}

		#endregion

		public override ZGuid JR_AC
		{
			get { return base.JR_AC; }
			set
			{
				var oldValue = JR_AC;
				var costVarianceKey = new CostVarianceKey(this);
				base.JR_AC = value;

				SetEstimatedCost(JR_OSCostAmt);

				if (oldValue != JR_AC)
				{
					AutoTickFinalFlag(costVarianceKey);
				}
			}
		}

		[ResourceStringData("Charge|ChargeableRate", Caption = "Chargeable Rate", FullDescription = "Jobs’ Chargeable Rate = Apportioned Cost / Chargeable Units")]
		public ZString ChargeableRate => GetChargeableRate(ParentConsolCost?.E6_ApportionmentMethod, JR_LocalCostAmt);

		public ZPropertyInfo ChargeableRateInfo => GetZPropertyInfo(nameof(ChargeableRate));

		protected override bool JR_SellGovtChargeCode_ReadOnly
		{
			get { return true; }
		}

		protected override bool JR_CostGovtChargeCode_ReadOnly
		{
			get { return true; }
		}

		public bool JR_SellPlaceOfSupply_ReadOnly => true;

		public bool JR_CostPlaceOfSupply_ReadOnly => true;

		protected override bool JR_SellSupplyType_ReadOnly => true;

		protected override bool JR_CostSupplyType_ReadOnly => true;

		protected override bool JR_GB_CostTaxBranch_ReadOnly => true;

		protected override bool JR_GB_SellTaxBranch_ReadOnly => true;

		public IDisposable SuspendSplittingApportionAmountChangeIsUsedForApportionment() => SplittingApportionAmountChangeIsUsedForApportionmentSuspender.GetSuspender();

		bool IsSplittingApportionAmountChangeIsUsedForApportionmentSuspended => SplittingApportionAmountChangeIsUsedForApportionmentSuspender.IsSuspended;

		FunctionalitySuspender SplittingApportionAmountChangeIsUsedForApportionmentSuspender => splittingApportionAmountChangeIsUsedForApportionmentSuspender ?? (splittingApportionAmountChangeIsUsedForApportionmentSuspender = new FunctionalitySuspender());
		FunctionalitySuspender splittingApportionAmountChangeIsUsedForApportionmentSuspender;

		#endregion

		#region Validation

		protected override JobChargeValidation GetNewValidation()
		{
			return new ApportionSplitChargeValidation(this);
		}

		#endregion

		#region Lookups

		public new ApportionSplitChargeLookups Lookups { get; }

		#endregion

		#region Calculation Suspender

		public IDisposable SuspendAmountsCalculations()
		{
			return new AmountCalculationSuspender(this);
		}
		ZBool IsAmountsCalculationsSuspended;

		class AmountCalculationSuspender : IDisposable
		{
			public AmountCalculationSuspender(ApportionSplitCharge parent)
			{
				this.Parent = parent;
				parent.IsAmountsCalculationsSuspended = true;
			}

			readonly ApportionSplitCharge Parent;

			void IDisposable.Dispose()
			{
				Parent.IsAmountsCalculationsSuspended = false;
			}
		}

		#endregion

		#region IApportionedCharge Members

		[DecimalPlaces(nameof(WeightVolumeDecimals))]
		public ZDecimal ChargeableUnits
		{
			get { return JR_Chargeable; }
		}

		[DecimalPlaces(nameof(WeightVolumeDecimals))]
		public ZDecimal GrossWeight => ShipmentInfo.GrossWeightInKilos();

		public ZDecimal GrossVolume => ShipmentInfo.GrossVolumeInCubicMeters();

		ZBool IApportionedCharge.IsUsedForApportionment
		{
			get { return JR_IsUsedForApportionment; }
		}

		ZInt IApportionedCharge.ContainerCount
		{
			get { return ShipmentInfo != null ? ShipmentInfo.InvoicingSupporter.ContainerCount : 0; }
		}

		ZDecimal IApportionedCharge.TEUCount
		{
			get { return ShipmentInfo != null ? ShipmentInfo.InvoicingSupporter.TEUCount : (ZDecimal)0m; }
		}

		ZInt IApportionedCharge.OuterPackTotal
		{
			get { return ShipmentInfo != null ? ShipmentInfo.InvoicingSupporter.OuterPackTotal : 0; }
		}

		ZString IApportionedCharge.CurrencyCode
		{
			get { return JR_RX_NKCostCurrency; }
			set { JR_RX_NKCostCurrency = value; }
		}

		ZDecimal IApportionedCharge.ExcessActualVolumeWeight => ShipmentInfo != null ? ShipmentInfo.InvoicingSupporter.ExcessActualVolumeWeight : 0;

		ZDecimal IApportionedCharge.ExcessChargeableVolumeWeight => ShipmentInfo != null ? ShipmentInfo.InvoicingSupporter.ExcessChargeableVolumeWeight : 0;

		#endregion

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
			{
				MarkAsNeedingValidation();
			}
		}

		protected override bool JR_JH_InternalJob_ReadOnly => this.IsGatewayApportionedCostCharge() || base.JR_JH_InternalJob_ReadOnly;
		protected override bool JR_GB_InternalBranch_ReadOnly => this.IsGatewayApportionedCostCharge() || base.JR_GB_InternalBranch_ReadOnly;
		protected override bool JR_GE_InternalDept_ReadOnly => this.IsGatewayApportionedCostCharge() || base.JR_GE_InternalDept_ReadOnly;

		public void CopyInternalFieldsOver(Charge charge)
		{
			if (charge != null)
			{
				JR_GB_InternalBranch = charge.JR_GB_InternalBranch;
				JR_GE_InternalDept = charge.JR_GE_InternalDept;
				JR_JH_InternalJob = charge.JR_JH_InternalJob;
			}
		}

		protected internal override ZDecimal CalculateCFXAmt()
		{
			var charge = LoadChargeInstanceToHandleSavingAndDeleting();
			return charge != null ? charge.CalculateCFXAmt() : ZDecimal.Zero;
		}

		#region Compliance

		[BusinessObjectTestExclude]
		public ZString ComplianceDocumentNumber { get; set; }

		[BusinessObjectTestExclude]
		public ZString ComplianceSubType { get; set; }

		[BusinessObjectTestExclude]
		public ZGuid ComplianceDocumentOrganization { get; set; }

		[BusinessObjectTestExclude]
		public ZString ComplianceDocumentVATRegistrationNum { get; set; }

		[BusinessObjectTestExclude]
		public ZDateTime ComplianceDocumentDate { get; set; }

		[BusinessObjectTestExclude]
		public ZInt ComplianceDocumentReportingPeriod { get; set; }

		[BusinessObjectTestExclude]
		public ZString ComplianceDocumentSupportingReason { get; set; }

		[BusinessObjectTestExclude]
		public ZString ComplianceSupportingDocumentType { get; set; }

		[BusinessObjectTestExclude]
		public ZString ComplianceSupportingDocumentNumber { get; set; }

		[BusinessObjectTestExclude]
		public ZBool CreateComplianceDocumentRecordOnPosting { get; set; }

		#endregion
	}
}
