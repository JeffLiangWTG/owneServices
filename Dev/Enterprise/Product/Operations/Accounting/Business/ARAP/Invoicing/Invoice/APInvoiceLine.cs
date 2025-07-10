using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using CostVarianceKey = Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalHelper.CostVarianceKey;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class APInvoiceLine : InvoiceLine, IApportionedCharge
	{
		public APInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return true; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return InvoiceBase != null && InvoiceBase.HasApprovalRequest ? Env.Security.APInvoiceApproval_AllowInvVATRecoverableOverride : Env.Security.AllowAPInvoiceLineVATRecoverableOverride; }
		}

		public override void ImportFromApportionSplitCharge(ApportionSplitCharge chargeToImportFrom)
		{
			base.ImportFromApportionSplitCharge(chargeToImportFrom);
			if (APInvoiceLineValidation != null)
			{
				APInvoiceLineValidation.ValidateAL_IsFinalCharge();
			}
		}

		protected override void SetDefaultAL_JHCore(ZGuid jobHeaderGuid)
		{
			SuspendChargePopup();
			this.AL_JH = jobHeaderGuid;
			SetExchangeRate();
			ResumeChargePopup();
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZQuery JobListFilter
		{
			get
			{
				return new ZQuery();
			}
		}

		public override ZBool AL_IsFinalCharge
		{
			get { return base.AL_IsFinalCharge; }
			set
			{
				base.AL_IsFinalCharge = value;
				if (!IsValidationSuspended && APInvoiceLineValidation != null)
				{
					APInvoiceLineValidation.ValidateAL_IsFinalCharge();
				}

				if (!IsSetAL_IsFinalChargeByGroup && !IsPopulatedFromImportedApportionment && APInvoice != null && APInvoice.CostVarianceApprovalHelper.AutoTickFinalFlag)
				{
					var key = new CostVarianceKey(this);

					if (key.IsValid)
					{
						var lines = APInvoice.Lines.Cast<APInvoiceLine>().Where(line => line.PK != PK && key == new CostVarianceKey(line) && (line.AL_IsFinalCharge != value || line.AL_IsFinalChargeDefault != value));

						SetAL_IsFinalChargeForRelatedLines(lines, value, AL_IsFinalChargeDefault);
					}
				}
			}
		}

		internal bool IsValidLineForCalculatingCostVarianceApproval
		{
			get
			{
				return AL_AC.IsValid && AL_JH.IsValid && AL_GB.IsValid && AL_GE.IsValid;
			}
		}

		void SetAL_IsFinalChargeForRelatedLines(IEnumerable<APInvoiceLine> lines, bool finalFlag, bool finalFlagDefault)
		{
			foreach (var line in lines)
			{
				using (line.StopChangingConsolRelatedLinesSuspender.GetSuspender())
				{
					try
					{
						line.IsSetAL_IsFinalChargeByGroup = true;
						line.AL_IsFinalChargeDefault = finalFlagDefault;
						line.AL_IsFinalCharge = finalFlag;
					}
					finally
					{
						line.IsSetAL_IsFinalChargeByGroup = false;
					}

					if (line.IsPopulatedFromImportedApportionment)
					{
						line.ApportionmentChargeImportedFrom.IsFinalDefault = finalFlagDefault;
						line.ApportionmentChargeImportedFrom.IsFinal = finalFlag;
					}
				}
			}
		}

		internal bool IsSetAL_IsFinalChargeByGroup { get; set; }

		APInvoiceLineValidation APInvoiceLineValidation
		{
			get { return Validation as APInvoiceLineValidation; }
		}

		protected override AccTaxRate GetFallbackTaxRate(out ZGuid overrideInvTaxMsg)
		{
			AccTaxRate fallbackRate = base.GetFallbackTaxRate(out overrideInvTaxMsg);
			if (OriginalJobCharge != null && OriginalAccrualCharge != null && OriginalAccrualCharge.CostGSTRate != null)
			{
				fallbackRate = OriginalAccrualCharge.CostGSTRate;
			}
			return fallbackRate;
		}

		Charge OriginalAccrualCharge => OriginalJobCharge as Charge;

		protected override bool ShouldDefaultAL_JH
		{
			get { return base.ShouldDefaultAL_JH && (APInvoice != null && !APInvoice.IsImportingJobCharges); }
		}

		ZString fConsolNumber;
		public ZString ConsolNumber
		{
			get
			{
				return fConsolNumber;
			}
			set
			{
				fConsolNumber = value;
			}
		}

		protected override GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return new APGenericChargeCollectionBuilder(this);
		}

		protected override ZString LineType
		{
			get { return TransactionLineTypes.Cost; }
		}

		public override AccTransactionHeader TransactionHeader => APInvoice ?? base.TransactionHeader;

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new APInvoiceLineLookups(this);
		}

		public APInvoice APInvoice => MasterTransactionHeader as APInvoice;

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (Factory.HasContext(BusinessContext.SavingIncompleteTransaction))
			{
				return new IncompleteInvoicingLineBaseValidation(this);
			}

			return new APInvoiceLineValidation(this);
		}

		public APInvoice.CostVarianceAuthorisationRequiredType CostVarianceAuthorisationRequired
		{
			get
			{
				var result = APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired;
				if (APInvoice != null)
				{
					result = APInvoice.GetLineCostVarianceAuthorisationRequired(this);
				}
				return result;
			}
		}

		protected override bool AL_ExchangeRate_ReadOnly
		{
			get { return base.AL_ExchangeRate_ReadOnly || !Env.Security.AllowAPInvoiceLineExchangeRateOverride.IsAllowed; }
		}

		protected override void OnFactorySavingBeforeTransactionCore2()
		{
			base.OnFactorySavingBeforeTransactionCore2();

			APLineRelatedJobOpener.ReopenClosedJobwithSuspendedValidation(InvoiceBase);
		}

		public override ZGuid AL_AC
		{
			get { return base.AL_AC; }
			set
			{
				var oldValue = AL_AC;
				var costVarianceKey = new CostVarianceKey(this);
				base.AL_AC = value;
				if (oldValue != AL_AC)
				{
					ValidateAL_OSExTaxAmount();

					if (!IsPopulatedFromImportedApportionment)
					{
						AutoTickFinalFlag(costVarianceKey);
					}
				}
			}
		}

		public override ZGuid AL_GB
		{
			get { return base.AL_GB; }
			set
			{
				var oldValue = AL_GB;
				var costVarianceKey = new CostVarianceKey(this);
				base.AL_GB = value;
				if (oldValue != AL_GB)
				{
					ValidateAL_OSExTaxAmount();

					if (!IsPopulatedFromImportedApportionment)
					{
						AutoTickFinalFlag(costVarianceKey);
					}
				}
			}
		}

		public override ZGuid AL_GE
		{
			get { return base.AL_GE; }
			set
			{
				var oldValue = AL_GE;
				var costVarianceKey = new CostVarianceKey(this);
				base.AL_GE = value;
				if (oldValue != AL_GE)
				{
					ValidateAL_OSExTaxAmount();

					if (!IsPopulatedFromImportedApportionment)
					{
						AutoTickFinalFlag(costVarianceKey);
					}
				}
			}
		}

		public override ZGuid AL_JH
		{
			get { return base.AL_JH; }
			set
			{
				var oldValue = AL_JH;
				var costVarianceKey = new CostVarianceKey(this);
				base.AL_JH = value;
				if (oldValue != AL_JH)
				{
					ValidateAL_OSExTaxAmount();

					if (!IsPopulatedFromImportedApportionment)
					{
						AutoTickFinalFlag(costVarianceKey);
					}
				}
			}
		}

		public override ZDecimal AL_LocalExTaxAmount
		{
			get
			{
				return base.AL_LocalExTaxAmount;
			}
			set
			{
				var hasChange = AL_LocalExTaxAmount != value;

				base.AL_LocalExTaxAmount = value;

				if (hasChange && !IsPopulatedFromImportedApportionment)
				{
					AutoTickFinalFlag();
				}
			}
		}

		protected override bool CanChangeLineValues
		{
			get
			{
				if (StopChangingConsolRelatedLinesSuspender.IsSuspended)
				{
					return true;
				}
				else
				{
					return base.CanChangeLineValues;
				}
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (IsValidLineForCalculatingCostVarianceApproval)
			{
				AutoTickFinalFlag(new CostVarianceKey(this));
			}
		}

		internal ZBool AL_IsFinalChargeDefault { get; set; }

		void AutoTickFinalFlag(CostVarianceKey costVarianceKey = null)
		{
#if DEBUG
			if (APInvoice != null && APInvoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().IsSuspended)
			{
				APInvoice.SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly++;
			}
#endif

			if ((!IsValidLineForCalculatingCostVarianceApproval && costVarianceKey == null)
				|| APInvoice == null
				|| !APInvoice.CostVarianceApprovalHelper.AutoTickFinalFlag
				|| APInvoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().IsSuspended
				|| APInvoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(true).IsSuspended)
			{
				return;
			}

			APInvoice.CostVarianceApprovalHelper.CalculateAuthorisationByLines();

			using (APInvoice.SetDefaultFinalWhenAutoTickingFinalFlagSuspender.GetSuspender())
			{
				if (APInvoice.CostVarianceApprovalHelper.MonitorTotalInvoiceVariance)
				{
					APInvoice.AutoTickFinalFlagForLinesIfMonitorTotalInvoiceVariance();
				}
				else
				{
					if (IsValidLineForCalculatingCostVarianceApproval && !IsDeleting)
					{
						AutoTickFinalFlagForEachLine();
					}

					if (costVarianceKey != null && costVarianceKey.IsValid)
					{
						ChangeFinalFlagForOriginalGroup(costVarianceKey);
					}
				}
			}
		}

		internal void AutoTickFinalFlagForEachLine()
		{
			var requirement = APInvoice.CostVarianceApprovalHelper.GetLineAuthorisationRequirement(this);
			var appropvalLevel = requirement != null ? requirement.AuthorisationRequirement.ToString() : AuthorisationCodes.NoApprovalRequired;
			if (appropvalLevel == AuthorisationCodes.NoApprovalRequired)
			{
				AL_IsFinalCharge = AL_IsFinalChargeDefault = true;

				if (IsPopulatedFromImportedApportionment)
				{
					ApportionmentChargeImportedFrom.IsFinal = ApportionmentChargeImportedFrom.IsFinalDefault = true;
				}
			}
			else
			{
				AL_IsFinalCharge = AL_IsFinalChargeDefault = false;
				if (IsPopulatedFromImportedApportionment)
				{
					ApportionmentChargeImportedFrom.IsFinal = ApportionmentChargeImportedFrom.IsFinalDefault = false;
				}
			}
		}

		void ChangeFinalFlagForOriginalGroup(CostVarianceKey costVarianceKey)
		{
			var lines = APInvoice.Lines.Cast<APInvoiceLine>().Where(line => costVarianceKey == new CostVarianceKey(line)).ToList();

			foreach (var line in lines)
			{
				using (line.StopChangingConsolRelatedLinesSuspender.GetSuspender())
				{
					try
					{
						line.IsSetAL_IsFinalChargeByGroup = true;
						line.AutoTickFinalFlagForEachLine();
					}
					finally
					{
						line.IsSetAL_IsFinalChargeByGroup = false;
					}
				}
			}
		}

		public FunctionalitySuspender StopChangingConsolRelatedLinesSuspender
		{
			get { return stopChangingConsolRelatedLinesSuspender ?? (stopChangingConsolRelatedLinesSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender stopChangingConsolRelatedLinesSuspender;

		void ValidateAL_OSExTaxAmount()
		{
			if (this.Factory.HasContext(BusinessContext.APInvoiceForm) && !IsValidationSuspended && APInvoiceLineValidation != null)
			{
				APInvoiceLineValidation.ValidateAL_OSExTaxAmount();

#if DEBUG
				ValidateAL_OSExTaxAmountCount_ForTestOnly++;
#endif
			}
		}

#if DEBUG
		internal int ValidateAL_OSExTaxAmountCount_ForTestOnly;
#endif

		#region IApportionedCharge Members

		Job IApportionedCharge.InvoicingJob
		{
			get { return InvoicingJob; }
		}

		ZDecimal IApportionedCharge.ChargeableUnits
		{
			get { return InvoicingJob != null ? InvoicingJob.ChargeableWgtVol : (ZDecimal)0m; }
		}

		ZDecimal IApportionedCharge.GrossWeight
		{
			get { return InvoicingJob != null && InvoicingJob.PlugInData != null ? InvoicingJob.PlugInData.InvoicingSupporter.ActualWeight : (ZDecimal)0m; }
		}

		ZDecimal IApportionedCharge.GrossVolume
		{
			get { return InvoicingJob != null && InvoicingJob.PlugInData != null ? InvoicingJob.PlugInData.InvoicingSupporter.ActualVolume : (ZDecimal)0m; }
		}

		ZBool IApportionedCharge.IsUsedForApportionment
		{
			get { return ZBool.True; }
		}

		ZInt IApportionedCharge.ContainerCount
		{
			get { return InvoicingJob != null ? InvoicingJob.ContainerCount : ZInt.Zero; }
		}

		ZDecimal IApportionedCharge.TEUCount
		{
			get { return InvoicingJob != null ? InvoicingJob.TEUCount : (ZDecimal)0m; }
		}

		ZInt IApportionedCharge.OuterPackTotal
		{
			get { return InvoicingJob != null ? InvoicingJob.OuterPackTotal : ZInt.Zero; }
		}

		ZString IApportionedCharge.CurrencyCode
		{
			get { return AL_RX_NKTransactionCurrency; }
			set { AL_RX_NKTransactionCurrency = value; }
		}

		ZDecimal IApportionedCharge.GetContainersCostShare()
		{
			return 0;
		}

		ZDecimal IApportionedCharge.ExcessActualVolumeWeight => InvoicingJob != null ? InvoicingJob.ExcessActualVolumeWeight : 0;

		ZDecimal IApportionedCharge.ExcessChargeableVolumeWeight => InvoicingJob != null ? InvoicingJob.ExcessChargeableVolumeWeight : 0;

		#endregion
	}
}
