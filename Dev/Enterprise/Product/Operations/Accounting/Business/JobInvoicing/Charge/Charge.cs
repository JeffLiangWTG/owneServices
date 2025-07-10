using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region IQuickCalculatorCharge Interface

	public interface IQuickCalculatorCharge : IPaymentBasisViewCharge
	{
		BusinessObjectFactory Factory { get; }
		Job InvoicingJob { get; }
		bool CanUpdateSell { get; }
		bool CanUpdateCost { get; }
		AccChargeCode ChargeCode { get; }
		void SetAmount(CostSell costSell, AutoRateInfo result, OrgHeader organisation, bool recalculateOverriden = false);
		bool IsCalculationDescriptionRelevant { get; }
		string SellCurrencyCode { get; }
		string CostCurrencyCode { get; }
		string RatingBehaviour { get; }
	}

	#endregion

	#region IPaymentBasisViewCharge Interface

	public interface IPaymentBasisViewCharge
	{
		BusinessObjectCollection<JobPaymentBasis> SellPaymentBasesView { get; }
		BusinessObjectCollection<JobPaymentBasis> CostPaymentBasesView { get; }
	}

	#endregion

#if DEBUG
	public partial class
#else
	public sealed class
#endif
		Charge : ChargeWithCost,
		ICharge,
		ICustomPropertyContainer,
		IDefaultLandedCostInput,
		IReceivablesPostingCharge,
		IPayablesPostingCharge,
		IQuickCalculatorCharge,
		IPaymentBasisViewCharge,
		IApportionedCharge,
		IShouldSkipDataRefreshUpdateForDeletedSubscriber
	{
		public Charge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : ChargeWithCost.Schema
		{
			public const string JR_OSSellInvoiceAmt = "JR_OSSellInvoiceAmt";
			public const string JR_OSSellInvoiceGSTAmt = "JR_OSSellInvoiceGSTAmt";
			public const string JR_OSSellInvoiceExRate = "JR_OSSellInvoiceExRate";
			public const string JR_OSSellInvoiceAmt_ForDisplay = "JR_OSSellInvoiceAmt_ForDisplay";
			public const string JR_OSSellInvoiceGSTAmt_ForDisplay = "JR_OSSellInvoiceGSTAmt_ForDisplay";
			public const string JR_OSSellInvoiceExRate_ForDisplay = "JR_OSSellInvoiceExRate_ForDisplay";
			public const string SellComplianceDescription = nameof(SellComplianceDescription);
		}

		#endregion

		internal static string[] PersistentFieldsToCopyAndInValidOrder
		{
			get
			{
				return new[]
				{
					JobChargeSchema.JR_AC.Name,
					JobChargeSchema.JR_Desc.Name,
					JobChargeSchema.JR_GB.Name,
					JobChargeSchema.JR_GC.Name,
					JobChargeSchema.JR_GE.Name,

					JobChargeSchema.JR_OH_CostAccount.Name,
					JobChargeSchema.JR_OH_SellAccount.Name,
					JobChargeSchema.JR_OA_SellInvoiceAddress.Name,
					JobChargeSchema.JR_OC_SellInvoiceContact.Name,
					JobChargeSchema.JR_RX_NKCostCurrency.Name,
					JobChargeSchema.JR_RX_NKSellCurrency.Name,
					JobChargeSchema.JR_InvoiceType.Name,
					JobChargeSchema.JR_RX_NKSellInvoiceCurrency.Name,

					JobChargeSchema.JR_OSCostExRate.Name,
					JobChargeSchema.JR_AT_CostGSTRate.Name,
					JobChargeSchema.JR_CostTaxDate.Name,
					JobChargeSchema.JR_A9_CostVATClass.Name,
					JobChargeSchema.JR_AW_CostWHTRate.Name,

					JobChargeSchema.JR_OSCostAmt.Name,
					JobChargeSchema.JR_LocalCostAmt.Name,
					JobChargeSchema.JR_OSCostWHTAmt.Name,
					JobChargeSchema.JR_EstimatedCost.Name,
					JobChargeSchema.JR_DeclaredOSCostAmt.Name,

					JobChargeSchema.JR_APInvoiceNum.Name,
					JobChargeSchema.JR_APInvoiceDate.Name,
					JobChargeSchema.JR_APDocumentReceivedDate.Name,
					JobChargeSchema.JR_PaymentDate.Name,
					JobChargeSchema.JR_PaymentType.Name,
					JobChargeSchema.JR_AB.Name,
					JobChargeSchema.JR_AK.Name,
					JobChargeSchema.JR_ChequeNo.Name,
					JobChargeSchema.JR_CostReference.Name,

					JobChargeSchema.JR_OSSellExRate.Name,
					JobChargeSchema.JR_AT_SellGSTRate.Name,
					JobChargeSchema.JR_SellTaxDate.Name,
					JobChargeSchema.JR_A9_SellVATClass.Name,
					JobChargeSchema.JR_AW_SellWHTRate.Name,
					JobChargeSchema.JR_LineCFX.Name,

					JobChargeSchema.JR_OSSellAmt.Name,
					JobChargeSchema.JR_LocalSellAmt.Name,
					JobChargeSchema.JR_OSSellWHTAmt.Name,
					JobChargeSchema.JR_EstimatedRevenue.Name,
					JobChargeSchema.JR_SellReference.Name,

					JobChargeSchema.JR_CostRated.Name,
					JobChargeSchema.JR_CostRatingOverride.Name,
					JobChargeSchema.JR_CostRatingOverrideComment.Name,
					JobChargeSchema.JR_SellRated.Name,
					JobChargeSchema.JR_SellRatingOverride.Name,
					JobChargeSchema.JR_SellRatingOverrideComment.Name,

					JobChargeSchema.JR_IsIncludedInProfitShare.Name,
					JobChargeSchema.JR_AgentDeclaredCostAmt.Name,
					JobChargeSchema.JR_AgentDeclaredSellAmt.Name,

					JobChargeSchema.JR_APLinePostingStatus.Name,
					JobChargeSchema.JR_ARLinePostingStatus.Name,

					JobChargeSchema.JR_APNumberOfSupportingDocuments.Name,
					JobChargeSchema.JR_ARNumberOfSupportingDocuments.Name,

					JobChargeSchema.JR_OrderReference.Name,
					JobChargeSchema.JR_PreventInvoicePrintGrouping.Name,
					JobChargeSchema.JR_ProductQuantity.Name,

					JobChargeSchema.JR_GB_InternalBranch.Name,
					JobChargeSchema.JR_GE_InternalDept.Name,
					JobChargeSchema.JR_JH_InternalJob.Name,

					JobChargeSchema.JR_JR_RevenueLine.Name,
					JobChargeSchema.JR_LineType.Name,

					JobChargeSchema.JR_CostGovtChargeCode.Name,
					JobChargeSchema.JR_SellGovtChargeCode.Name,

					JobChargeSchema.JR_SellPlaceOfSupplyType.Name,
					JobChargeSchema.JR_CostPlaceOfSupplyType.Name,

					JobChargeSchema.JR_SellPlaceOfSupply.Name,
					JobChargeSchema.JR_CostPlaceOfSupply.Name,

					JobChargeSchema.JR_CostSupplyType.Name,
					JobChargeSchema.JR_SellSupplyType.Name,

					JobChargeSchema.JR_GB_CostTaxBranch.Name,
					JobChargeSchema.JR_GB_SellTaxBranch.Name,

					JobChargeSchema.JR_IsAPCashAdvance.Name,
					JobChargeSchema.JR_IsARCashAdvance.Name,
				};
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public void CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(Charge sourceObject)
		{
			Argument.NotNull(sourceObject, "sourceObject");

			var destinationObjectJob = this.InvoicingJob;
			var sourceObjectJob = sourceObject.InvoicingJob;
			if (destinationObjectJob == null)
			{
				ErrorReporter.ReportOnce("Destination charges must belong to a job." +
						System.Environment.NewLine + this.GetJobChargeInfo());
			}
			else if (sourceObjectJob == null)
			{
				ErrorReporter.ReportOnce("Source charge must belong to a job." +
						System.Environment.NewLine + sourceObject.GetJobChargeInfo());
			}
			else if (destinationObjectJob.Company == null)
			{
				ErrorReporter.ReportOnce("Destination charge job must have company set." +
						System.Environment.NewLine + this.GetJobChargeInfo() +
						System.Environment.NewLine + destinationObjectJob.GetJobInfo());
			}
			else if (destinationObjectJob.JH_GC != sourceObjectJob.JH_GC)
			{
				ErrorReporter.ReportOnce(string.Format((NoResString)"Charges must belong to the same company job: destination company '{0}', source company '{1}'.", destinationObjectJob.Company.GC_Code, sourceObjectJob.Company.GC_Code) +
						System.Environment.NewLine + "Destination " + this.GetJobChargeInfo() +
						System.Environment.NewLine + "Destination " + destinationObjectJob.GetJobInfo() +
						System.Environment.NewLine + "Source " + sourceObject.GetJobChargeInfo() +
						System.Environment.NewLine + "Source " + sourceObjectJob.GetJobInfo());
			}
			else
			{
				using (Calculations.SuspendCalculations())
				{
					this.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceObject, PersistentFieldsToCopyAndInValidOrder);
				}
			}
		}

		protected override sealed void OnSellInvoiceExchangeRateChangedCore(object sender, EventArgs e)
		{
			base.OnSellInvoiceExchangeRateChangedCore(sender, e);

			UpdateLineCFX();

			JR_OSSellInvoiceExRateInfo.RefreshBinding();
			JR_OSSellInvoiceAmtInfo.RefreshBinding();
			JR_OSSellInvoiceGSTAmtInfo.RefreshBinding();

			JR_OSSellInvoiceAmt_ForDisplayInfo.RefreshBinding();
			JR_OSSellInvoiceGSTAmt_ForDisplayInfo.RefreshBinding();
			JR_OSSellInvoiceExRate_ForDisplayInfo.RefreshBinding();
		}

		#region Saving

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

		protected override void OnSavedInCompanyContext(bool saveSucceeded)
		{
			base.OnSavedInCompanyContext(saveSucceeded);
			if (saveSucceeded && !IsValidationSuspended)
			{
				Validation.ValidateRow();
			}
		}

		protected override void OnSavingInCompanyContext()
		{
			base.OnSavingInCompanyContext();
			DisableAutoRatingCalculationLogsIfRatingOverride();
		}

		void DisableAutoRatingCalculationLogsIfRatingOverride()
		{
			if (shouldDisableCostLog)
			{
				CalculationLogsLoader.Disable(this, CalculationLogsLoader.DisableOption.Costing);
			}

			if (shouldDisableSellLog)
			{
				CalculationLogsLoader.Disable(this, CalculationLogsLoader.DisableOption.Both);
			}
		}

		protected override void DeleteForDataRefresh()
		{
			if (!this.IsDeleted)
			{
				var unlinkedACR = this.Accrual;
				var unlinkedWIP = this.WIP;

				if (unlinkedACR != null && !unlinkedACR.IsInDatabase)
				{
					unlinkedACR.Delete();
				}
				if (unlinkedWIP != null && !unlinkedWIP.IsInDatabase)
				{
					unlinkedWIP.Delete();
				}
			}

			base.DeleteForDataRefresh();
		}

		#endregion

		#region Properties

		public override ZGuid JR_JH_InternalJob
		{
			get { return base.JR_JH_InternalJob; }
			set
			{
				base.JR_JH_InternalJob = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public static ResourceStringData GetJR_JH_InternalJobCaption(Job job) => job.IsGatewayBillingJob() ? Res.GetData("b03bc8a3-e91d-40c9-ab50-4fc3e6669253", "Internal Job", @"Please enter or select an internal job related to this charge. To record gateway revenue as cost on a specific job, enter or select this job number.
To apportion gateway revenue as cost on multiple shipments attached to this consol, set Internal Job to the current consol number and review the Gateway Sell Apportionment tab.") : null;

		public override ZGuid JR_GB_InternalBranch
		{
			get { return base.JR_GB_InternalBranch; }
			set
			{
				base.JR_GB_InternalBranch = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_GB_CostTaxBranch
		{
			get { return base.JR_GB_CostTaxBranch; }
			set
			{
				base.JR_GB_CostTaxBranch = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_GB_SellTaxBranch
		{
			get { return base.JR_GB_SellTaxBranch; }
			set
			{
				base.JR_GB_SellTaxBranch = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_GE_InternalDept
		{
			get { return base.JR_GE_InternalDept; }
			set
			{
				base.JR_GE_InternalDept = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDecimal JR_OSCostAmt
		{
			get
			{
				return base.JR_OSCostAmt;
			}
			set
			{
				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, JR_OSCostAmt != value))
				{
					base.JR_OSCostAmt = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
					MarkJobAsNeedingValidation();

					Validation.ValidateJR_EstimatedCost();
				}
			}
		}

		void MarkJobAsNeedingValidation()
		{
			if (InvoicingJob != null)
			{
				InvoicingJob.MarkAsNeedingValidation();
			}
		}

		public override ZDecimal JR_OSSellAmt
		{
			get
			{
				return base.JR_OSSellAmt;
			}
			set
			{
				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, JR_OSSellAmt != value))
				{
					var oldValue = JR_OSSellAmt;

					base.JR_OSSellAmt = value;

					if (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.Value &&
						JR_OSSellAmt > 0 &&
						oldValue <= 0 &&
						SellAccount != null &&
						!JR_IsARCashAdvance)
					{
						JR_IsARCashAdvance = IsSuitableForARCashAdvanceDefaulting();
					}

					if (JR_EstimatedRevenue.IsEmpty || !IsInDatabase)
					{
						JR_EstimatedRevenue = base.JR_OSSellAmt;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
					MarkJobAsNeedingValidation();
					Validation.ValidateJR_EstimatedRevenue();

					if (ParentConsolRevenue != null && !ParentConsolRevenue.IsDeleted)
					{
						if (!IsSplittingApportionAmountChangeIsUsedForApportionmentSuspended)
						{
							isUsedForApportionment = ParentConsolRevenue.SellAmount == 0M || JR_OSSellAmt != 0M;
						}

						if (!ParentConsolRevenue.IsValidationSuspended)
						{
							ParentConsolRevenue.Validation.ValidateUnapportionedAmount();
						}

						foreach (var charge in ParentConsolRevenue.SplitCharges)
						{
							charge.Validation.ValidateJR_OSSellAmt();
						}
					}

					AddChangeInfoToCiriticalValidationCollectorIfRequires();
				}
			}
		}

		public override ZGuid JR_AB
		{
			get
			{
				return base.JR_AB;
			}
			set
			{
				base.JR_AB = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_AC
		{
			get
			{
				return base.JR_AC;
			}
			set
			{
				if (JR_AC != value)
				{
					base.JR_AC = value;

					if (IsCommentChargeCode)
					{
						JR_IsIncludedInProfitShare = false;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		internal void SetChargeCodeAndCreditorForNewAutoRated(
			AccChargeCode rateChargeCode,
			CostSell costOrSell,
			bool hasAmount,
			ZGuid providerOrgPK,
			ZGuid creditorOverrideOrgPK,
			bool isIntercompanyTariff)
		{
			if (rateChargeCode != null)
			{
				using (ServiceContainerSuspenderHelper.FunctionalitySuspender<UpdateCreditorFunctionalitySuspender>.GetSuspender(Factory))
				{
					if (JR_AC != rateChargeCode.PK)
					{
						JR_AC = rateChargeCode.PK;
						if (costOrSell == CostSell.Cost && JR_OH_CostAccount.IsEmpty)
						{
							var proposedCreditor = (Job?.Parent as IChargeCreditorDefaulting)?.DefaultCreditorPK ?? ZGuid.Empty;
							JR_OH_CostAccount =
								IsValidCreditorForDefaulting(proposedCreditor)
									? proposedCreditor
									: ZGuid.Empty;
						}
					}
				}
			}

			ZGuid creditorPK;
			if (!creditorOverrideOrgPK.IsEmpty)
			{
				creditorPK = creditorOverrideOrgPK;
			}
			else
			{
				var chargeType = ChargeType;

				// Decide the charge creditor.
				// It is either the job creditor or the provider or nothing.
				// It must be an organization flagged as a creditor.
				//
				// For costs or disbursement or intercompany tariffs the provider is preferred (they are providing the service and should get the payment).
				// However, for costs or disbursement, if the creditor is an agent of the provider then the creditor is preferred (agent should get the payment).
				//
				// For revenue (other than intercompany tariffs), the job creditor is preferred.

				bool isCostOrDisbursement = costOrSell == CostSell.Cost || chargeType == Constants.ChargeType.Disbursement;
				bool isProviderPreferred = isCostOrDisbursement || isIntercompanyTariff;
				bool isProviderAllowed = !((costOrSell == CostSell.Cost && chargeType == Constants.ChargeType.Revenue)
											|| (costOrSell == CostSell.Revenue && !hasAmount));

				var lazyJobCreditorPK = new Lazy<ZGuid>(() =>
				{
					// For costs, there is duplication of creditor logic between IJobInvoicingSupporter and IGenericJobCostSupporter.
					// Use IGenericJobCostSupporter if available. It's more specific to costs (it considers provider).
					// We only get here via AutoRateInvoicingStrategy (as of Nov 2021), which in turn requires the job implements IJobInvoicingPlugIn.
					// So this only applies for jobs that implement both IJobInvoicingPlugIn and IJobCostingPlugIn.
					ZGuid pk = ZGuid.Empty;
					if (costOrSell == CostSell.Cost && !providerOrgPK.IsEmpty && rateChargeCode != null)
					{
						var costSupporter = (InvoicingJob?.Parent as IJobCostingPlugIn)?.CostSupporter;
						if (costSupporter != null)
						{
							pk = costSupporter.GetCreditorPK(rateChargeCode.AC_ChargeGroup, providerOrgPK);
						}
					}
					if (pk.IsEmpty)
					{
						pk = InvoicingJob?.GetCreditorPK(ChargeCode, chargeType, JR_InvoiceType, providerOrgPK) ?? ZGuid.Empty;
					}
					return pk;
				});

				var lazyValidProviderOrg = new Lazy<OrgHeader>(() =>
				{
					OrgHeader result = null;
					if (isProviderAllowed && !providerOrgPK.IsEmpty)
					{
						var providerOrg = Factory.Load<OrgHeader>(providerOrgPK);

						if (providerOrg != null && providerOrg.OH_IsCreditor)
						{
							if (isCostOrDisbursement)
							{
								var jobCreditorPK = lazyJobCreditorPK.Value;

								if (jobCreditorPK.IsValid
								 && providerOrg.AllRelatedParties.Cast<OrgRelatedParty>()
								 .Any(o => o.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor && o.PR_OH_RelatedParty == jobCreditorPK))
								{
									// Don't use provider. creditor will be assigned instead, later as a fallback.
									providerOrg = null;
								}
							}

							result = providerOrg;
						}
					}

					return result;
				});

				creditorPK = GetJobCreditorOrProvider(isProviderPreferred, lazyJobCreditorPK, lazyValidProviderOrg);
				if (creditorPK.IsEmpty)
				{
					creditorPK = GetJobCreditorOrProvider(!isProviderPreferred, lazyJobCreditorPK, lazyValidProviderOrg);
				}
			}

			if (creditorPK.IsValid)
			{
				ClearDebtorIfBothCostAndSellAccountsAreOrgProxies(creditorPK);
				JR_OH_CostAccount = IsValidCreditorForDefaulting(creditorPK) ? creditorPK : ZGuid.Empty;
			}
		}

		ZGuid GetJobCreditorOrProvider(bool isProviderPreferred, Lazy<ZGuid> lazyJobCreditorPK, Lazy<OrgHeader> lazyValidProviderOrg)
		{
			bool providerPreferredFromSupporter = InvoicingJob?.GetInvoicingSupporter()?.ProviderPreferred ?? false;
			if (!isProviderPreferred
				|| (InvoicingJob?.GetInvoicingSupporter() != null
				&& !providerPreferredFromSupporter))
			{
				return lazyJobCreditorPK.Value;
			}
			return lazyValidProviderOrg.Value?.PK ?? ZGuid.Empty;
		}

		/// <summary>
		/// Prevent validation error "You cannot set both the Cost Account and Sell Account to be the organization proxies."
		/// </summary>
		internal void ClearDebtorIfBothCostAndSellAccountsAreOrgProxies(ZGuid costAccountPK)
		{
			GlbCompany calculatedCompany;
			var shouldClearDebtor = !JR_OH_SellAccount.IsEmpty
				&& !IsRevenuePosted
				&& !JR_SellRatingOverride
				&& null != (calculatedCompany = CalculatedCompany)
				&& SellAccountIsOrgProxy
				&& AutoJRJRegistryStatusHelper.IsAutoJRJEnabled()
				&& (Factory.Load<OrgHeader>(costAccountPK)?.IsProxyOrg(calculatedCompany) ?? false);

			if (shouldClearDebtor)
			{
				JR_OH_SellAccount = ZGuid.Empty;
			}
		}

		[ReadOnlyMember(nameof(JR_AgentDeclaredCostAmt_ReadOnly))]
		public override ZDecimal JR_AgentDeclaredCostAmt
		{
			get
			{
				return base.JR_AgentDeclaredCostAmt;
			}
			set
			{
				if (JR_AgentDeclaredCostAmt != value)
				{
					base.JR_AgentDeclaredCostAmt = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(JR_AgentDeclaredSellAmt_ReadOnly))]
		public override ZDecimal JR_AgentDeclaredSellAmt
		{
			get
			{
				return base.JR_AgentDeclaredSellAmt;
			}
			set
			{
				if (base.JR_AgentDeclaredSellAmt != value)
				{
					base.JR_AgentDeclaredSellAmt = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZGuid JR_AK
		{
			get
			{
				return base.JR_AK;
			}
			set
			{
				base.JR_AK = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZBool JR_IsARCashAdvance
		{
			get => base.JR_IsARCashAdvance;
			set
			{
				base.JR_IsARCashAdvance = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZBool JR_IsAPCashAdvance
		{
			get => base.JR_IsAPCashAdvance;
			set
			{
				base.JR_IsAPCashAdvance = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_CAL_ARLine
		{
			get => base.JR_CAL_ARLine;
			set
			{
				base.JR_CAL_ARLine = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_CAL_APLine
		{
			get => base.JR_CAL_APLine;
			set
			{
				base.JR_CAL_APLine = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_AL_APLine
		{
			get
			{
				return base.JR_AL_APLine;
			}
			set
			{
				base.JR_AL_APLine = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_AL_ARLine
		{
			get
			{
				return base.JR_AL_ARLine;
			}
			set
			{
				base.JR_AL_ARLine = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		#region Cash Advance

		public bool JR_IsARCashAdvance_ReadOnly => IsRevenuePosted ||
								(!ARCashAdvanceRequestStatus.IsEmpty
								&& ARCashAdvanceRequestStatus != CashAdvanceStatusCodes.RequestHeader.Pending
								&& ARCashAdvanceRequestStatus != CashAdvanceStatusCodes.RequestHeader.Cancelled);

		public AccCashAdvanceRequestHeader ARCashAdvanceRequestHeader => ARCashAdvanceRequestLine?.RequestHeader;
		public AccCashAdvanceRequestHeader APCashAdvanceRequestHeader => APCashAdvanceRequestLine?.RequestHeader;
		public ZString ARCashAdvanceRequestStatus
		{
			get
			{
				var result = ZString.Empty;
				if (JR_IsARCashAdvance)
				{
					result = ARCashAdvanceRequestHeader?.CAH_Status ?? CashAdvanceStatusCodes.RequestHeader.Pending;
				}
				return result;
			}
		}

		public ZString ARCashAdvanceRequestStatusDescription => CashAdvanceStatusCodes.RequestHeader.CodesList.GetDescriptionFromCode(ARCashAdvanceRequestStatus);

		public ZString ARCashAdvanceRequestID => ARCashAdvanceRequestHeader?.CAH_RequestReferenceNumber ?? ZString.Empty;

		bool IsSuitableForARCashAdvanceDefaulting() => IsSuitableForCashAdvanceDefaulting(LedgerTypes.AccountsReceivable);

		bool IsSuitableForCashAdvanceDefaulting(ZString ledgerType)
		{
			var isSuitable = false;
			var jobType = InvoicingJob?.JobType?.Code ?? ZString.Empty;
			var direction = InvoicingJob?.Direction ?? ZString.Empty;
			var transportMode = InvoicingJob?.TransportMode ?? ZString.Empty;
			var matchingConfig = SellAccount.CompanyData.AccARCashAdvanceConfigurations.FindBestMatchingConfiguration(ledgerType, jobType, direction, transportMode);
			if (matchingConfig != null)
			{
				switch (matchingConfig.CAC_DefaultingOption)
				{
					case CashAdvanceDefaultingOption.All:
						isSuitable = true;
						break;
					case CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups:
						if (ChargeCode != null)
						{
							isSuitable = matchingConfig.ChargeCodes.MatchingChargeCodeExist(JR_AC);
							if (!isSuitable)
							{
								isSuitable = matchingConfig.ChargeGroups.MatchingChargeGroupExist(ChargeCode.AC_ChargeGroup);
							}
						}
						break;
					case CashAdvanceDefaultingOption.None:
					default:
						break;
				}
			}
			return isSuitable;
		}

		#endregion

		public override ZGuid JR_AL_CFXLine
		{
			get
			{
				return base.JR_AL_CFXLine;
			}
			set
			{
				base.JR_AL_CFXLine = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDateTime JR_APInvoiceDate
		{
			get
			{
				return base.JR_APInvoiceDate;
			}
			set
			{
				base.JR_APInvoiceDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_APInvoiceNum
		{
			get
			{
				return base.JR_APInvoiceNum;
			}
			set
			{
				base.JR_APInvoiceNum = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_CostReference
		{
			get { return base.JR_CostReference; }
			set
			{
				base.JR_CostReference = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_SellReference
		{
			get { return base.JR_SellReference; }
			set
			{
				base.JR_SellReference = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_APLinePostingStatus
		{
			get
			{
				return base.JR_APLinePostingStatus;
			}
			set
			{
				base.JR_APLinePostingStatus = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_ARLinePostingStatus
		{
			get
			{
				return base.JR_ARLinePostingStatus;
			}
			set
			{
				base.JR_ARLinePostingStatus = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_AT_CostGSTRate
		{
			get
			{
				return base.JR_AT_CostGSTRate;
			}
			set
			{
				var hasChanged = base.JR_AT_CostGSTRate != value;

				base.JR_AT_CostGSTRate = value;
				if (hasChanged && !IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDate JR_CostTaxDate
		{
			get
			{
				return base.JR_CostTaxDate;
			}
			set
			{
				var hasChanged = base.JR_CostTaxDate != value;

				base.JR_CostTaxDate = value;

				if (hasChanged && !IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_AT_SellGSTRate
		{
			get
			{
				return base.JR_AT_SellGSTRate;
			}
			set
			{
				var hasChanged = base.JR_AT_SellGSTRate != value;

				base.JR_AT_SellGSTRate = value;

				if (hasChanged && !IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDate JR_SellTaxDate
		{
			get
			{
				return base.JR_SellTaxDate;
			}
			set
			{
				var hasChanged = base.JR_SellTaxDate != value;

				base.JR_SellTaxDate = value;

				if (hasChanged && !IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_AW_CostWHTRate
		{
			get
			{
				return base.JR_AW_CostWHTRate;
			}
			set
			{
				if (JR_AW_CostWHTRate != value)
				{
					base.JR_AW_CostWHTRate = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZGuid JR_AW_SellWHTRate
		{
			get
			{
				return base.JR_AW_SellWHTRate;
			}
			set
			{
				if (JR_AW_SellWHTRate != value)
				{
					base.JR_AW_SellWHTRate = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZString JR_ChargeType
		{
			get
			{
				return base.JR_ChargeType;
			}
			set
			{
				base.JR_ChargeType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_ChequeNo
		{
			get
			{
				return base.JR_ChequeNo;
			}
			set
			{
				base.JR_ChequeNo = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_CostGovtChargeCode
		{
			get
			{
				return base.JR_CostGovtChargeCode;
			}
			set
			{
				base.JR_CostGovtChargeCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_CostSupplyType
		{
			get
			{
				return base.JR_CostSupplyType;
			}
			set
			{
				base.JR_CostSupplyType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_CostPlaceOfSupplyType
		{
			get { return base.JR_CostPlaceOfSupplyType; }
			set
			{
				base.JR_CostPlaceOfSupplyType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_SellPlaceOfSupplyType
		{
			get { return base.JR_SellPlaceOfSupplyType; }
			set
			{
				base.JR_SellPlaceOfSupplyType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_CostPlaceOfSupply
		{
			get { return base.JR_CostPlaceOfSupply; }
			set
			{
				base.JR_CostPlaceOfSupply = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public bool JR_CostPlaceOfSupply_ReadOnly => IsBaseCostFieldsReadonly || IsAllCostFieldsReadonly;

		public override ZString JR_SellPlaceOfSupply
		{
			get { return base.JR_SellPlaceOfSupply; }
			set
			{
				base.JR_SellPlaceOfSupply = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public bool JR_SellPlaceOfSupply_ReadOnly => IsBaseSellFieldsReadonly || IsMainAllFieldsReadonly;

		public override ZBool JR_CostRated
		{
			get
			{
				return base.JR_CostRated;
			}
			set
			{
				if (JR_CostRated != value)
				{
					base.JR_CostRated = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZDecimal JR_DeclaredOSCostAmt
		{
			get
			{
				return base.JR_DeclaredOSCostAmt;
			}
			set
			{
				base.JR_DeclaredOSCostAmt = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_Desc
		{
			get
			{
				return base.JR_Desc;
			}
			set
			{
				if (JR_Desc != value)
				{
					base.JR_Desc = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(JR_DisplaySequence_ReadOnly))]
		public override ZShort JR_DisplaySequence
		{
			get
			{
				return base.JR_DisplaySequence;
			}
			set
			{
				base.JR_DisplaySequence = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
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
				base.JR_E6 = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_E6_GatewaySellHeader
		{
			get { return base.JR_E6_GatewaySellHeader; }
			set
			{
				base.JR_E6_GatewaySellHeader = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDecimal JR_EstimatedCost
		{
			get
			{
				return base.JR_EstimatedCost;
			}
			set
			{
				base.JR_EstimatedCost = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDecimal JR_EstimatedRevenue
		{
			get
			{
				return base.JR_EstimatedRevenue;
			}
			set
			{
				if (JR_EstimatedRevenue != value)
				{
					base.JR_EstimatedRevenue = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZGuid JR_GB
		{
			get
			{
				return base.JR_GB;
			}
			set
			{
				bool hasChanged = JR_GB != value;

				base.JR_GB = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();

					if (hasChanged)
					{
						Validation.ValidateAll();
					}
				}

				RefreshBinding();
			}
		}

		public override ZGuid JR_GE
		{
			get
			{
				return base.JR_GE;
			}
			set
			{
				bool hasChanged = JR_GE != value;

				base.JR_GE = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();

					if (hasChanged)
					{
						Validation.ValidateAll();
					}
				}

				RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(JR_IsIncludedInProfitShare_ReadOnly))]
		public override ZBool JR_IsIncludedInProfitShare
		{
			get
			{
				return base.JR_IsIncludedInProfitShare;
			}
			set
			{
				if (IsCommentChargeCode)
				{
					base.JR_IsIncludedInProfitShare = false;
					JR_AgentDeclaredCostAmt = 0m;
					JR_AgentDeclaredSellAmt = 0m;
				}
				else
				{
					base.JR_IsIncludedInProfitShare = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_JH
		{
			get
			{
				return base.JR_JH;
			}
			set
			{
				base.JR_JH = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDecimal JR_LineCFX
		{
			get
			{
				return base.JR_LineCFX;
			}
			set
			{
				if (JR_LineCFX != value)
				{
					base.JR_LineCFX = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}

					AddChangeInfoToCiriticalValidationCollectorIfRequires();
				}
			}
		}

		protected override bool JR_LineCFX_ReadOnly => true;

		public override ZDecimal JR_LocalCostAmt
		{
			get
			{
				return base.JR_LocalCostAmt;
			}
			set
			{
				if (JR_LocalCostAmt != value)
				{
					base.JR_LocalCostAmt = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
					MarkJobAsNeedingValidation();
				}
			}
		}

		public override ZDecimal JR_MarginPercentage
		{
			get
			{
				return base.JR_MarginPercentage;
			}
			set
			{
				base.JR_MarginPercentage = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZGuid JR_OH_CostAccount
		{
			get
			{
				return base.JR_OH_CostAccount;
			}
			set
			{
				if (base.JR_OH_CostAccount == value)
				{
					return;
				}

				base.JR_OH_CostAccount = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}

				if (CostAccount != null && CostAccount.CompanyData.OB_APCostsSelfBilled)
				{
					JR_APInvoiceNum = ZString.Empty;
					JR_APInvoiceDate = ZDateTime.Empty;
					JR_PaymentDate = ZDateTime.Empty;
					JR_APDocumentReceivedDate = ZDateTime.Empty;
				}
			}
		}

		public override ZGuid JR_OH_SellAccount
		{
			get
			{
				return base.JR_OH_SellAccount;
			}
			set
			{
				if (JR_OH_SellAccount != value)
				{
					base.JR_OH_SellAccount = value;

					if (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.Value &&
						SellAccount != null && JR_OSSellAmt > 0)
					{
						JR_IsARCashAdvance = IsSuitableForARCashAdvanceDefaulting();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZGuid JR_OP_Product
		{
			get
			{
				return base.JR_OP_Product;
			}
			set
			{
				base.JR_OP_Product = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_OrderReference
		{
			get
			{
				return base.JR_OrderReference;
			}
			set
			{
				if (JR_OrderReference != value)
				{
					base.JR_OrderReference = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		protected override void SetJR_OSCostExRateChangedCore(ZDecimal value)
		{
			if (JR_OSCostExRate != value)
			{
				base.SetJR_OSCostExRateChangedCore(value);
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateRow();
			}
		}

		public override ZDecimal JR_OSCostGSTAmt_Calc
		{
			get
			{
				return base.JR_OSCostGSTAmt_Calc;
			}
			set
			{
				base.JR_OSCostGSTAmt_Calc = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZBool JR_IsCostTaxAmountOverridden
		{
			get
			{
				return base.JR_IsCostTaxAmountOverridden;
			}
			set
			{
				base.JR_IsCostTaxAmountOverridden = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDecimal JR_OSCostWHTAmt
		{
			get
			{
				return base.JR_OSCostWHTAmt;
			}
			set
			{
				base.JR_OSCostWHTAmt = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		[ReadOnly(true)]
		public override ZDecimal JR_OSSellExRate
		{
			get
			{
				return base.JR_OSSellExRate;
			}
			set
			{
				var currentValue = JR_OSSellExRate;
				if (currentValue != value)
				{
					base.JR_OSSellExRate = value;

					if (!IsValidationSuspended && base.JR_OSSellExRate != currentValue)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZDecimal JR_OSSellWHTAmt
		{
			get
			{
				return base.JR_OSSellWHTAmt;
			}
			set
			{
				base.JR_OSSellWHTAmt = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZBool JR_CostRatingOverride
		{
			get { return base.JR_CostRatingOverride; }
			set
			{
				if (!AutoRatingOverrideSuppressed)
				{
					base.JR_CostRatingOverride = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}

					JR_CostRatingOverrideCommentInfo.RefreshBinding();
				}
			}
		}

		public override ZString JR_CostRatingOverrideComment
		{
			get { return base.JR_CostRatingOverrideComment; }
			set
			{
				base.JR_CostRatingOverrideComment = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZBool JR_SellRatingOverride
		{
			get { return base.JR_SellRatingOverride; }
			set
			{
				if (!AutoRatingOverrideSuppressed)
				{
					base.JR_SellRatingOverride = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}

					JR_SellRatingOverrideCommentInfo.RefreshBinding();
				}
			}
		}

		public override ZString JR_SellRatingOverrideComment
		{
			get { return base.JR_SellRatingOverrideComment; }
			set
			{
				base.JR_SellRatingOverrideComment = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		[List("Lookups.RatingBehaviors_Cost")]
		[BusinessObjectTestExclude]
		public override ZString JR_Calc_CostRatingBehavior
		{
			get
			{
				return JR_IsSpotCost
					? JobChargeLookups.SpotCost
					: JR_CostRatingOverride
						? JobChargeLookups.CreateNewCharge
						: JR_IsCostPosted || JR_IsApportioned || JR_Calc_IsAcceptedByDebtor
							? JobChargeLookups.StopFromAutorating
							: JobChargeLookups.ReAutorateCharge;
			}
			set
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateCostRatingBehavior();
				}

				if (!JR_Calc_CostRatingBehaviorInfo.HasErrors())
				{
					var isNonOverrideBehaviour = value == JobChargeLookups.StopFromAutorating
						|| value == JobChargeLookups.ReAutorateCharge;

					JR_CostRatingOverride = !isNonOverrideBehaviour;
				}
			}
		}

		public ZPropertyInfo JR_Calc_CostRatingBehaviorInfo
		{
			get { return GetZPropertyInfo(nameof(JR_Calc_CostRatingBehavior)); }
		}

		public bool JR_Calc_CostRatingBehavior_ReadOnly
		{
			get { return JR_CostRatingOverride_ReadOnly || JR_Calc_IsAcceptedByDebtor || JR_IsSpotCost; }
		}

		/// <summary>
		/// Indicates if this charge's cost values match a group company charge's sell values,
		/// i.e., this company is the debtor for the group company charge
		/// and this charge is the accepted cost of that other charge.
		/// </summary>
		bool JR_Calc_IsAcceptedByDebtor
		{
			get
			{
				if (InvoicingJob == null)
				{
					return false;
				}

				if ((groupChargeMatch & GroupChargeMatch.CostCalculated) == 0)
				{
					bool isAccepted = InvoicingJob.GroupCompanyChargesForDebtor.IsAcceptedCost(this);
					groupChargeMatch |= GroupChargeMatch.CostCalculated;
					if (isAccepted)
					{
						groupChargeMatch |= GroupChargeMatch.CostAccepted;
					}
					else
					{
						groupChargeMatch &= ~GroupChargeMatch.CostAccepted;
					}
				}
				return (groupChargeMatch & GroupChargeMatch.CostAccepted) != 0;
			}
		}

		/// <summary>
		/// Store all the group charge matching states in a single enum to save memory
		/// </summary>
		[Flags]
		enum GroupChargeMatch
		{
			NotCalculated = 0,
			CostCalculated = 1, // For calculating on demand
			CostAccepted = 2, // The flag indicating if another charge's sell values match this charge's cost values
			SellCalculated = 4, // For calculating on demand
			SellAccepted = 8, // The flag indicating if another charge's cost values match this charge's sell values
		}
		GroupChargeMatch groupChargeMatch = GroupChargeMatch.NotCalculated;

#if DEBUG
		public bool IsGroupChargeCostCalculated_ForTestOnly => (groupChargeMatch & GroupChargeMatch.CostCalculated) != 0;
		public bool IsGroupChargeCostAccepted_ForTestOnly => (groupChargeMatch & GroupChargeMatch.CostAccepted) != 0;
		public bool IsGroupChargeSellCalculated_ForTestOnly => (groupChargeMatch & GroupChargeMatch.SellCalculated) != 0;
		public bool IsGroupChargeSellAccepted_ForTestOnly => (groupChargeMatch & GroupChargeMatch.SellAccepted) != 0;
#endif

		internal void InvalidateGroupChargeMatch(bool isForCreditor)
		{
			if (isForCreditor)
			{
				groupChargeMatch &= ~(GroupChargeMatch.SellCalculated | GroupChargeMatch.SellAccepted);
			}
			else
			{
				groupChargeMatch &= ~(GroupChargeMatch.CostCalculated | GroupChargeMatch.CostAccepted);
			}
		}

		[List("Lookups.RatingBehaviors_Sell")]
		[BusinessObjectTestExclude]
		public override ZString JR_Calc_SellRatingBehavior
		{
			get
			{
				return JR_SellRatingOverride
					? JobChargeLookups.CreateNewCharge
					: JR_IsRevenuePosted
						? JobChargeLookups.StopFromAutorating
						: JobChargeLookups.ReAutorateCharge;
			}
			set
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateSellRatingBehavior();
				}

				if (!JR_Calc_SellRatingBehaviorInfo.HasErrors())
				{
					var isNonOverrideBehaviour = value == JobChargeLookups.StopFromAutorating
						|| value == JobChargeLookups.ReAutorateCharge;

					JR_SellRatingOverride = !isNonOverrideBehaviour;
				}
			}
		}

		public ZPropertyInfo JR_Calc_SellRatingBehaviorInfo
		{
			get { return GetZPropertyInfo(nameof(JR_Calc_SellRatingBehavior)); }
		}

		public bool JR_Calc_SellRatingBehavior_ReadOnly
		{
			get { return JR_SellRatingOverride_ReadOnly; }
		}

		/// <summary>
		/// Indicates if this charge's sell values match a group company charge's cost values,
		/// i.e., the other company is the debtor for this sell charge
		/// and there is a matching group charge that is the accepted cost of this charge.
		/// Used for "Accepted By Debtor" checkbox on Revenue tab.
		/// </summary>
		public ZBool JR_Calc_HasDebtorAcceptedThisSellCharge
		{
			get
			{
				if (InvoicingJob == null)
				{
					return false;
				}

				if ((groupChargeMatch & GroupChargeMatch.SellCalculated) == 0)
				{
					bool isAccepted = InvoicingJob.GroupCompanyChargesForCreditor.IsAcceptedSell(this);
					groupChargeMatch |= GroupChargeMatch.SellCalculated;
					if (isAccepted)
					{
						groupChargeMatch |= GroupChargeMatch.SellAccepted;
					}
					else
					{
						groupChargeMatch &= ~GroupChargeMatch.SellAccepted;
					}
				}
				return (groupChargeMatch & GroupChargeMatch.SellAccepted) != 0;
			}
		}

		public ZPropertyInfo JR_Calc_HasDebtorAcceptedThisSellChargeInfo
		{
			get { return GetZPropertyInfo(nameof(JR_Calc_HasDebtorAcceptedThisSellCharge)); }
		}

		public override ZDateTime JR_PaymentDate
		{
			get
			{
				return base.JR_PaymentDate;
			}
			set
			{
				base.JR_PaymentDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDateTime JR_APDocumentReceivedDate
		{
			get
			{
				return base.JR_APDocumentReceivedDate;
			}
			set
			{
				base.JR_APDocumentReceivedDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		[List("PaymentTypes")]
		public override ZString JR_PaymentType
		{
			get
			{
				return base.JR_PaymentType;
			}
			set
			{
				base.JR_PaymentType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
					Validation.ValidateJR_PaymentType();
					Validation.ValidateJR_OH_CostAccount();
				}
			}
		}

		public override ZBool JR_PreventInvoicePrintGrouping
		{
			get
			{
				return base.JR_PreventInvoicePrintGrouping;
			}
			set
			{
				base.JR_PreventInvoicePrintGrouping = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZDecimal JR_ProductQuantity
		{
			get
			{
				return base.JR_ProductQuantity;
			}
			set
			{
				base.JR_ProductQuantity = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		protected override void SetJR_RX_NKCostCurrencyCore(ZString value)
		{
			base.SetJR_RX_NKCostCurrencyCore(value);

			if (!IsValidationSuspended)
			{
				Validation.ValidateRow();
			}
		}

		protected override void SetJR_RX_NKSellCurrencyCore(ZString value)
		{
			if (JR_RX_NKSellCurrency != value)
			{
				base.SetJR_RX_NKSellCurrencyCore(value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		#region JR_RX_NKSellInvoiceCurrency 

		public override ZString JR_RX_NKSellInvoiceCurrency
		{
			get { return base.JR_RX_NKSellInvoiceCurrency; }
			set
			{
				if (JR_RX_NKSellInvoiceCurrency != value)
				{
					InitializeSellInvoiceExchangeRate();
					base.JR_RX_NKSellInvoiceCurrency = value;

					if (!JR_RX_NKSellInvoiceCurrency.IsEmpty &&
						InvoicingJob != null && SellInvoiceCurrency != null && !IsRevenuePosted)
					{
						InvoicingJob.AddCurrency(SellInvoiceCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyTypeForAR);
					}

					UpdateSellInvoiceExchangeRate();

					if (value != JR_RX_NKSellCurrency)
					{
						UpdateRevenueExchangeRate();
					}
					
					OnRevenueExchangeRateChanged(null, new EventArgs());

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}

					JR_OSSellInvoiceAmtInfo.RefreshBinding();
					JR_OSSellInvoiceGSTAmtInfo.RefreshBinding();
					JR_OSSellInvoiceExRateInfo.RefreshBinding();

					JR_OSSellInvoiceAmt_ForDisplayInfo.RefreshBinding();
					JR_OSSellInvoiceGSTAmt_ForDisplayInfo.RefreshBinding();
					JR_OSSellInvoiceExRate_ForDisplayInfo.RefreshBinding();
				}
			}
		}

		bool JR_RX_NKSellInvoiceCurrency_ReadOnly
		{
			get { return !InvoiceTypeCalculationProvider.BillInLocalCurrency(JR_InvoiceType) || IsMainAllFieldsReadonly; }
		}

		#endregion

		#region JR_OSSellInvoiceAmt

		[DecimalPlaces(nameof(OSSellInvoiceCurrencyDecimals))]
		public ZDecimal JR_OSSellInvoiceAmt
		{
			get
			{
				var sellInvoiceAmount = ZDecimal.Zero;
				if (BillInInvoiceCurrency)
				{
					sellInvoiceAmount = BIllInInvoiceCurrencySameAsSellCurrency ? JR_OSSellAmt :
						(ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(JR_LocalSellInvoiceAmt, JR_OSSellInvoiceExRate, JR_RX_NKSellInvoiceCurrency);
				}
				return sellInvoiceAmount;
			}
		}

		public ZPropertyInfo JR_OSSellInvoiceAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_OSSellInvoiceAmt); }
		}

		public ZString JR_OSSellInvoiceAmt_ForDisplay
		{
			get { return BillInInvoiceCurrency ? Utilities.FormatNumberNationalWithGroupSeparators((decimal)JR_OSSellInvoiceAmt, OSSellInvoiceCurrencyDecimals) : string.Empty; }
		}

		public ZPropertyInfo JR_OSSellInvoiceAmt_ForDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.JR_OSSellInvoiceAmt_ForDisplay); }
		}

		#endregion

		#region JR_OSSellInvoiceExRate 

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public ZDecimal JR_OSSellInvoiceExRate
		{
			get
			{
				var result = decimal.Zero;
				if (BillInInvoiceCurrency)
				{
					if (IsRevenuePosted && ARLine.AL_RX_NKTransactionCurrency == JR_RX_NKSellInvoiceCurrency)
					{
						result = ARLine.AL_ExchangeRate;
					}
					else if (BIllInInvoiceCurrencySameAsSellCurrency)
					{
						result = JR_OSSellExRate;
					}
					else if (osSellInvoiceExRateOverrideForPosting.HasValue && this.HasContext(BusinessContext.PostingReceivableCharges))
					{
						result = osSellInvoiceExRateOverrideForPosting.Value;
					}
					else
					{
						result = SellInvoiceExchangeRate?.Rate ?? 0m;
					}
				}
				return result;
			}
		}
		ZDecimal? osSellInvoiceExRateOverrideForPosting;

		public void OverrideOSSellInvoiceExRateForPosting(ZDecimal newValue)
		{
			osSellInvoiceExRateOverrideForPosting = newValue;
			SellInvoiceRateWithoutCFX = newValue;
		}

		public ZPropertyInfo JR_OSSellInvoiceExRateInfo
		{
			get { return GetZPropertyInfo(Schema.JR_OSSellInvoiceExRate); }
		}

		public ZString JR_OSSellInvoiceExRate_ForDisplay
		{
			get { return BillInInvoiceCurrency ? Utilities.FormatNumberNationalWithGroupSeparators((decimal)JR_OSSellInvoiceExRate, ExchangeRateDecimalPlaces) : string.Empty; }
		}

		public ZPropertyInfo JR_OSSellInvoiceExRate_ForDisplayInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.JR_OSSellInvoiceExRate_ForDisplay);
				info.HumanReadableName = Res.GetString("1E44FA0E-66B0-4B09-955F-AAFF7CF861C0", "Sell Invoice Ex. Rate");
				return info;
			}
		}

		#endregion

		#region CFX

		protected internal override ZDecimal CalculateCFXAmt()
		{
			if (BillInInvoiceCurrencyWithLocalSellCurrency)
			{
				var rate = this.HasContext(BusinessContext.PostingReceivableCharges)
					? SellInvoiceRateWithoutCFX
					: SellInvoiceExchangeRate?.Rate ?? 1m;

				var convertedAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(JR_OSSellInvoiceAmt, rate);
				return convertedAmount - JR_LocalSellAmt;
			}
			else
			{
				var rate = this.HasContext(BusinessContext.PostingReceivableCharges)
					? SellRateWithoutCFX
					: RevenueExchangeRate?.Rate ?? 1m;

				var convertedAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(JR_OSSellAmt, rate);
				return JR_LocalSellAmt - convertedAmount;
			}
		}

		#endregion

		public override ZString JR_SellGovtChargeCode
		{
			get { return base.JR_SellGovtChargeCode; }
			set
			{
				base.JR_SellGovtChargeCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_SellSupplyType
		{
			get
			{
				return base.JR_SellSupplyType;
			}
			set
			{
				base.JR_SellSupplyType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		[ResourceStringData("619F49CC-2636-4550-9004-7F2405EC4C87", Caption = "Sell Compliance Description", ShortCaption = "Sell Comp. Desc.")]
		public ZString SellComplianceDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!IsRevenuePosted && InvoicingJob?.JobType != null && ChargeCode != null)
				{
					result = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetChargeCodeOverrideRulesRanker().GetBestSellComplianceDescriptionOverride(Factory, ChargeCode.ChargeComplianceDescriptions, InvoicingJob.JobType.Code, InvoicingJob.TransportMode, JR_SellSupplyType);
				}

				return result;
			}
		}

		public ZPropertyInfo SellComplianceDescriptionInfo => GetZPropertyInfo(Schema.SellComplianceDescription);

		public override ZBool JR_SellRated
		{
			get
			{
				return base.JR_SellRated;
			}
			set
			{
				if (JR_SellRated != value)
				{
					base.JR_SellRated = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
				}
			}
		}

		public override ZGuid JR_JR_RevenueLine
		{
			get { return base.JR_JR_RevenueLine; }
			set
			{
				base.JR_JR_RevenueLine = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public override ZString JR_LineType
		{
			get { return base.JR_LineType; }
			set
			{
				base.JR_LineType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		public ZString MatchedWithTNFJournalNum
		{
			get
			{
				var result = ZString.Empty;
				if (JR_OH_CostAccount.IsValid && CostAccount != null && !JR_RX_NKCostCurrency.IsEmpty && (!JR_APInvoiceNum.IsEmpty || !JR_CostReference.IsEmpty))
				{
					result = Factory.GetCachedValue(GetCachingKey(), GetMatchedWithTNFJournalNum);
				}
				return result;
			}
		}

		string GetCachingKey()
		{
			return "MatchedTNFJNL:" + CostAccount.OH_Code + JR_APInvoiceNum + JR_CostReference + JR_RX_NKCostCurrency;
		}

		ZString GetMatchedWithTNFJournalNum()
		{
			var query = new ZQuery();

			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GB, JR_GB);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JR_GC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, JR_OH_CostAccount);
			query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, JR_RX_NKCostCurrency);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Constants.TransactionCategory.Codes.TransactionNotFound);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.GreaterThan, 0);

			var referenceFilter = new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, JR_APInvoiceNum);
			referenceFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ChequeOrReference, JR_CostReference);
			query.AddToFilter(referenceFilter);

			var matchedJournal = Factory.LoadTop1<APJournal>(query);
			return matchedJournal != null ? matchedJournal.AH_TransactionNum : ZString.Empty;
		}

		public FunctionalitySuspender TaxAmountAdjustmentSuspender
		{
			get { return taxAmountAdjustmentSuspender ?? (taxAmountAdjustmentSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender taxAmountAdjustmentSuspender;

		public ZBool CostDueOverseasAgent
		{
			get { return CostAccount != null && CostAccount == Job.AgentCollect; }
		}

		public ZBool IsProfitShareCharge
		{
			get { return JR_AC.IsValid && (JR_AC == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value || JR_AC == AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value); }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal ProfitShareForCharge
		{
			get
			{
				var parentJob = Job as Job;
				if (parentJob == null || !JR_IsIncludedInProfitShare)
				{
					return ZDecimal.Zero;
				}

				var profitShareDetails = parentJob.ProfitShareAgreement;
				if (profitShareDetails == null)
				{
					return ZDecimal.Zero;
				}

				var party = profitShareDetails.PartyDetails.GetParty(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent);
				if (party == null)
				{
					return ZDecimal.Zero;
				}

				ZDecimal agentDeclaredProfit = JR_AgentDeclaredSellAmt - JR_AgentDeclaredCostAmt;
				var (result, _) = party.CalculateProfitShare(agentDeclaredProfit, JR_AgentDeclaredSellAmt, 0m, 0);
				return result;
			}
		}

		#region JR_InvoiceType

		public override ZString JR_InvoiceType
		{
			get { return base.JR_InvoiceType; }
			set
			{
				var hasChanges = JR_InvoiceType != value;

				var isInvoiceCurrencyTypeNeeded = hasChanges && !value.IsEmpty && InvoicingJob != null && SellCurrency != null && !IsRevenuePosted;
				var oldInvoiceCurrencyType = isInvoiceCurrencyTypeNeeded ? InvoiceCurrencyTypeForAR : InvoiceCurrencyType.NotApplicable;

				base.JR_InvoiceType = value;

				if (hasChanges)
				{
					UpdateSellInvoiceCurrency();
				}

				if (JR_RX_NKSellInvoiceCurrency.IsEmpty && isInvoiceCurrencyTypeNeeded)
				{
					var invoiceCurrencyType = InvoiceCurrencyTypeForAR;
					if (invoiceCurrencyType != oldInvoiceCurrencyType)
					{
						InvoicingJob.AddCurrency(SellCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
					}
				}

				OnRevenueExchangeRateChanged(null, new EventArgs());

				if (!IsValidationSuspended)
				{
					Validation.ValidateRow();
				}
			}
		}

		protected override bool JR_InvoiceType_ReadOnly
		{
			get
			{
				return IsMainAllFieldsReadonly;
			}
		}

		#endregion

		#region IsSelectedForAutoPopulation

		ZBool isSelectedForAutoPopulation;

		[ReadOnlyMember(nameof(IsSelectedForAutoPopulation_ReadOnly))]
		public ZBool IsSelectedForAutoPopulation
		{
			get { return isSelectedForAutoPopulation; }
			set { SetNonPersistentPropertyValue(IsSelectedForAutoPopulationInfo, ref isSelectedForAutoPopulation, value); }
		}

		public ZPropertyInfo IsSelectedForAutoPopulationInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(IsSelectedForAutoPopulation));
			}
		}

		bool IsSelectedForAutoPopulation_ReadOnly
		{
			get { return JR_IsApportioned || !JR_APInvoiceNum.IsEmpty; }
		}

		#endregion

		#region Has Valid Data for Posting

		/// <summary>
		/// This checks only Charge's values. BasePosterMaster & APInvoiceCreator check external conditions in its own context in addition to this.
		/// </summary>
		public bool HasValidDataForCostPosting
		{
			get
			{
				return !IsCostPosted &&
					CostAccount != null &&
					(
						(!JR_APInvoiceNum.IsEmpty && JR_APInvoiceDate.IsValid)
						||
						CostAccount.CompanyData.OB_APCostsSelfBilled
					) &&
					HasCostAmount &&
					ShouldPostCharge(true) &&
					!IsValidForAutoCostPosting &&
					!Job.IsReadyForFinancialClosureWithoutPostSecurity;
			}
		}

		public bool HasValidDataForRevenuePosting
		{
			get
			{
				return !IsRevenuePosted &&
					!IsParentJobWorkOnHold &&
					!IsParentJobInvoicingOnHold &&
					ShouldPostCharge(false) &&
					SellAccount != null &&
					HasSellAmount &&
					!IsValidForAutoRevenuePosting &&
					!Job.IsReadyForFinancialClosureWithoutPostSecurity;
			}
		}

		public bool IsValidForAutoRevenuePosting => AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && SellAccountIsOrgProxy && !this.IsExcludedFromAutoJRJ(SellAccount);

		bool IsValidForAutoCostPosting => AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && CostAccountIsOrgProxy && !this.IsExcludedFromAutoJRJ(CostAccount);

		#endregion

		public bool IsParentJobInvoicingOnHold
		{
			get { return Job.JH_Status == JobHeaderStatus.InvoiceOnHold.Code; }
		}

		public bool IsParentJobWorkOnHold
		{
			get { return Job.JH_Status == JobHeaderStatus.WorkOnHold.Code; }
		}

		public bool ShouldPostCharge(bool isForAPLine)
		{
			if (InvoicingJob != null && InvoicingJob.JobType != null && InvoicingJob.PlugInData != null)
			{
				return InvoicingJob.JobType.ShouldPostCharges(InvoicingJob.PlugInData, JR_InvoiceType, isForAPLine).PostAllowed;
			}
			else
			{
				return JR_InvoiceType != InvoiceTypesList.Codes.DoNotPost;
			}
		}

		public bool HasCostAmount
		{
			get
			{
				return JR_OSCostAmt != 0 && JR_LocalCostAmt != 0;
			}
		}

		bool HasSellAmount
		{
			get
			{
				return JR_LocalSellAmt != 0 || ChargeType == Core.Constants.ChargeType.Comment;
			}
		}

		#region Description

		public bool IsCalculationDescriptionRelevant
		{
			get
			{
				bool result = false;

				string displayOption = "";

				if (Job != null && SellAccount != null && ChargeCode != null)
				{
					string jobTypeCode = InvoicingJob.JobType != null ? InvoicingJob.JobType.Code : string.Empty;
					displayOption = new OrgInvoiceRollupOrGroup.Loader(SellAccount, Branch, Department).GetInvoiceLineDisplayOption(InvoicingJob.ServiceDirection, InvoicingJob.TransportMode, InvoicingJob.TransportMode, jobTypeCode);
				}
				else
				{
					displayOption = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All,
						OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code).InvoiceLineDisplayOption;
				}

				switch (displayOption)
				{
					case InvoiceDescriptionOptionsList.Codes.All:
					case InvoiceDescriptionOptionsList.Codes.AllExRate:
						result = true;
						break;

					case InvoiceDescriptionOptionsList.Codes.Freight:
					case InvoiceDescriptionOptionsList.Codes.FreightExRate:
						if (ChargeCode != null && ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight)
						{
							result = true;
						}
						break;

					case InvoiceDescriptionOptionsList.Codes.FreightFOB:
					case InvoiceDescriptionOptionsList.Codes.FreightFOBExRate:
						if (ChargeCode != null && (ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin ||
							ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Loading ||
							ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight))
						{
							result = true;
						}
						break;
				}

				return result;
			}
		}

		#endregion

		#region JR_RelatedConsolRef

		public ZString JR_RelatedConsolRef
		{
			get { return ParentConsolCost != null && ParentConsolCost.Consol != null ? ParentConsolCost.Consol.JK_UniqueConsignRef : ZString.Empty; }
		}

		public ZPropertyInfo JR_RelatedConsolRefInfo
		{
			get { return GetZPropertyInfo(nameof(JR_RelatedConsolRef)); }
		}

		#endregion

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Sell_LocalSellAmount
		{
			get { return JR_LocalSellAmt + JR_Sell_LocalGSTAmount; }
		}

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public ZDecimal JR_Sell_InvoiceOSAmount
		{
			get { return JR_OSSellInvoiceAmt + JR_OSSellInvoiceGSTAmount; }
		}

		#region JR_OSSellInvoiceGSTAmt 

		[DecimalPlaces(nameof(OSSellInvoiceCurrencyDecimals))]
		public ZDecimal JR_OSSellInvoiceGSTAmt
		{
			get
			{
				var sellInvoiceGstAmount = ZDecimal.Zero;
				if (BillInInvoiceCurrency)
				{
					if (BIllInInvoiceCurrencySameAsSellCurrency)
					{
						sellInvoiceGstAmount = JR_OSSellGSTAmt_Calc;
					}
					else
					{
						if (SellGSTRate != null && SellGSTRate.IsMexicoNeedExtraType)
						{
							sellInvoiceGstAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, JR_OSSellInvoiceAmt, SellGSTRate, SellGSTRate?.GetRate(JR_SellTaxDate), SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate), SellInvoiceCurrency, Company.PK, withoutRounding: true);
						}
						else
						{
							sellInvoiceGstAmount = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(JR_SellInvoice_LocalGSTAmount, JR_OSSellInvoiceExRate, JR_RX_NKSellInvoiceCurrency);
						}
					}
				}
				return sellInvoiceGstAmount;
			}
		}

		public ZPropertyInfo JR_OSSellInvoiceGSTAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_OSSellInvoiceGSTAmt); }
		}

		public ZString JR_OSSellInvoiceGSTAmt_ForDisplay
		{
			get { return BillInInvoiceCurrency ? Utilities.FormatNumberNationalWithGroupSeparators((decimal)JR_OSSellInvoiceGSTAmt, OSSellInvoiceCurrencyDecimals) : string.Empty; }
		}

		public ZPropertyInfo JR_OSSellInvoiceGSTAmt_ForDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.JR_OSSellInvoiceGSTAmt_ForDisplay); }
		}

		internal ZDecimal JR_SellInvoice_LocalGSTAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (BillInInvoiceCurrency)
				{
					if (IsRevenuePostedWithSellInvoiceCurrencyAndNotCopyingOnPosting)
					{
						result = Revenue.AL_GSTVAT;
					}
					else
					{
						result = SellGSTRate != null && SellGSTRate.IsMexicoNeedExtraType
							? TaxAmountCalculator.GetLocalTaxAmount(Factory, JR_GC, JR_LocalSellInvoiceAmt, SellGSTRate, SellGSTRate?.GetRate(JR_SellTaxDate), SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate), JR_OSSellInvoiceGSTAmt, JR_OSSellInvoiceExRate)
							: TaxAmountCalculator.GetLocalTaxAmount(Factory, JR_GC, JR_LocalSellInvoiceAmt, SellGSTRate, SellGSTRate?.GetRate(JR_SellTaxDate), SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate));
					}
				}

				return result;
			}
		}

		#endregion

		internal ZDecimal JR_OSSellInvoiceGSTAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (BillInInvoiceCurrency)
				{
					if (IsRevenuePosted)
					{
						result = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(Revenue.AL_GSTVAT, Revenue.AL_ExchangeRate, Revenue.AL_RX_NKTransactionCurrency);
					}
					else if (NZCustomsEntryFeeTaxCalculator.IsEntryFeeRevenueChargeWithCorrectAmount(this))
					{
						result = SellGSTRate == null ? ZDecimal.Zero : JR_OSSellInvoiceGSTAmt;
					}
					else
					{
						result = JR_OSSellInvoiceGSTAmt;
					}
				}

				return result;
			}
		}

		internal ZDecimal JR_Calc_OSSellInvoiceExtraTaxAmt
		{
			get
			{
				var result = ZDecimal.Zero;
				if (BillInInvoiceCurrency)
				{
					result = Utilities.Round((ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(JR_Calc_LocalSellInvoiceExtraTaxAmt, JR_OSSellInvoiceExRate, JR_RX_NKSellInvoiceCurrency), SellInvoiceCurrency.Decimals);
				}
				return result;
			}
		}

		internal ZDecimal JR_Calc_LocalSellInvoiceExtraTaxAmt
		{
			get
			{
				var result = ZDecimal.Zero;
				if (SellGSTRate != null && BillInInvoiceCurrency)
				{
					return TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalTaxAmount(Factory, JR_GC, JR_SellInvoice_LocalGSTAmount, SellGSTRate, SellGSTRate?.GetRate(JR_SellTaxDate), SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate));
				}
				return result;
			}
		}

		[ReadOnly(true)]
		public ZShort ChargeCodePrintSequence
		{
			get
			{
				ZShort result = ZShort.Zero;
				bool isFound = false;
				if (SellAccount != null)
				{
					foreach (AccClientInvoiceOrder order in SellAccount.InvoiceOrders)
					{
						if (order.AI_AC == JR_AC && (order.AI_InvoiceType == JR_InvoiceType || order.AI_InvoiceType == "ALL" || order.AI_InvoiceType == string.Empty))
						{
							result = order.AI_PrintOrder;
							isFound = true;
							break;
						}
					}
				}
				if (ChargeCode != null && !isFound)
				{
					result = ChargeCode.AC_PrintSequence;
				}

				return result;
			}
		}

		#region ZBool IsUsedForApportionment

		public ZBool IsUsedForApportionment
		{
			get { return isUsedForApportionment; }
			set
			{
				if (SetNonPersistentPropertyValue(IsUsedForApportionmentInfo, ref isUsedForApportionment, value))
				{
					if (!isUsedForApportionment)
					{
						JR_OSSellAmt = 0M;
					}

					if (ParentConsolRevenue != null)
					{
						ParentConsolRevenue.ApportionRevenue();
					}

					JR_OSSellAmtInfo.RefreshBinding();
				}
			}
		}
		ZBool isUsedForApportionment;

		public ZPropertyInfo IsUsedForApportionmentInfo => GetZPropertyInfo(nameof(IsUsedForApportionment));

		#endregion

		public IDisposable SuspendSplittingApportionAmountChangeIsUsedForApportionment() => SplittingApportionAmountChangeIsUsedForApportionmentSuspender.GetSuspender();

		bool IsSplittingApportionAmountChangeIsUsedForApportionmentSuspended => SplittingApportionAmountChangeIsUsedForApportionmentSuspender.IsSuspended;

		FunctionalitySuspender SplittingApportionAmountChangeIsUsedForApportionmentSuspender => splittingApportionAmountChangeIsUsedForApportionmentSuspender ?? (splittingApportionAmountChangeIsUsedForApportionmentSuspender = new FunctionalitySuspender());
		FunctionalitySuspender splittingApportionAmountChangeIsUsedForApportionmentSuspender;

		#region Bulk Charge Import Selection

		public ZBool IsSelectedForImport
		{
			get { return BulkChargeImportCollection?.IsChargeSelected(this) ?? false; }
			set
			{
				BulkChargeImportCollection?.SetChargeSelected(this, value);
				IsSelectedForImportInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSelectedForImportInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelectedForImport)); }
		}

		ChargeForBulkChargeImportCollection BulkChargeImportCollection => (ChargeForBulkChargeImportCollection)((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is ChargeForBulkChargeImportCollection);

		#endregion

		#endregion

		#region PropertyInfo Override

		#region JR_SellSupplyTypeInfo

		protected override bool JR_SellSupplyType_ReadOnly => base.JR_SellSupplyType_ReadOnly || ParentConsolRevenue != null;

		#endregion

		#region JR_CostSupplyTypeInfo

		protected override bool JR_CostSupplyType_ReadOnly => base.JR_CostSupplyType_ReadOnly || ParentConsolRevenue != null;

		#endregion

		#region JR_OSCostAmtInfo

		protected override bool JR_OSCostAmt_ReadOnly
		{
			get { return base.JR_OSCostAmt_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_LocalCostAmtInfo

		protected override bool JR_LocalCostAmt_ReadOnly
		{
			get { return base.JR_LocalCostAmt_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_OH_CostAccountInfo

		protected override bool JR_OH_CostAccount_ReadOnly
		{
			get { return base.JR_OH_CostAccount_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_OSSellAmtInfo

		protected override bool JR_OSSellAmt_ReadOnly
		{
			get
			{
				return base.JR_OSSellAmt_ReadOnly || IsCommentChargeCode || (ParentConsolRevenue != null && !IsUsedForApportionment);
			}
		}

		#endregion

		#region JR_LocalSellAmtInfo

		protected override bool JR_LocalSellAmt_ReadOnly
		{
			get { return base.JR_LocalSellAmt_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_LocalSellAmt

		public override ZDecimal JR_LocalSellAmt
		{
			get { return base.JR_LocalSellAmt; }
			set
			{
				var valueToSet = IsIcelandAndKronurLocalCurrency ? value.Round(0) : value;
				if (JR_LocalSellAmt != valueToSet)
				{
					var oldValue = JR_LocalSellAmt;
					base.JR_LocalSellAmt = valueToSet;

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
					MarkJobAsNeedingValidation();

					AddChangeInfoToCiriticalValidationCollectorIfRequires();

					if (this.HasContext(BusinessContext.JobChargeAfterOnSaving) && !IsInDatabase && Math.Sign(JR_OSSellAmt) != Math.Sign(JR_LocalSellAmt))
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount, () =>
						{
							return FormattableString.Invariant($@"{JobChargeSchema.JR_LocalSellAmt.Name} has been changed from {oldValue} to {JR_LocalSellAmt}.
{System.Environment.StackTrace}");
						});
					}
				}
			}
		}

		protected override string GetSellInvoiceRateWithoutCFXInfo() => Invariant($"{SellInvoiceRateWithoutCFX}");

		protected override string GetSellRateWithoutCFXInfo() => Invariant($"{SellRateWithoutCFX}");

		#endregion

		#region JR_TotalLocalRevenue

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_TotalLocalRevenue
		{
			get { return JR_LocalSellInvoiceAmt; }
		}

		#endregion

		#region Tax Expense

		public bool IsTaxExpense => JR_TotalTaxExpenseRevenue != 0 || JR_TotalTaxExpenseCost != 0;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_TotalTaxExpenseRevenue
		{
			get
			{
				var taxExpenseAmount = 0m;
				if (IsRevenuePosted && Revenue != null)
				{
					taxExpenseAmount = Revenue.AL_TotalTaxExpenseAmount;
				}
				return taxExpenseAmount;
			}
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_TotalTaxExpenseCost
		{
			get
			{
				var taxExpenseAmount = 0m;
				if (IsCostPosted && Cost != null)
				{
					taxExpenseAmount = Cost.AL_TotalTaxExpenseAmount;
				}
				return taxExpenseAmount;
			}
		}

		#endregion

		#region JR_ABInfo

		protected override bool JR_AB_ReadOnly
		{
			get { return base.JR_AB_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_APInvoiceDateInfo

		protected override bool JR_APInvoiceDate_ReadOnly
		{
			get { return base.JR_APInvoiceDate_ReadOnly || IsCommentChargeCode || CostAccount != null && CostAccount.CompanyData.OB_APCostsSelfBilled; }
		}

		#endregion

		#region JR_APDocumentReceivedDateInfo

		protected override bool JR_APDocumentReceivedDate_ReadOnly
		{
			get { return base.JR_APDocumentReceivedDate_ReadOnly || IsCommentChargeCode || CostAccount != null && CostAccount.CompanyData.OB_APCostsSelfBilled; }
		}

		#endregion

		#region JR_APInvoiceNumInfo

		protected override bool JR_APInvoiceNum_ReadOnly
		{
			get { return base.JR_APInvoiceNum_ReadOnly || IsCommentChargeCode || CostAccount != null && CostAccount.CompanyData.OB_APCostsSelfBilled; }
		}

		#endregion

		#region JR_CostReferenceInfo

		protected override bool JR_CostReference_ReadOnly
		{
			get { return base.JR_CostReference_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_PaymentDateInfo

		protected override bool JR_PaymentDate_ReadOnly
		{
			get { return base.JR_PaymentDate_ReadOnly || IsCommentChargeCode || CostAccount != null && CostAccount.CompanyData.OB_APCostsSelfBilled; }
		}

		#endregion

		#region JR_PaymentTypeInfo

		protected override bool JR_PaymentType_ReadOnly
		{
			get { return base.JR_PaymentType_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_AKInfo

		protected override bool JR_AK_ReadOnly
		{
			get { return base.JR_AK_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_ChequeNoInfo

		protected override bool JR_ChequeNo_ReadOnly
		{
			get { return base.JR_ChequeNo_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_RX_NKCostCurrencyInfo

		protected override bool JR_RX_NKCostCurrency_ReadOnly
		{
			get { return base.JR_RX_NKCostCurrency_ReadOnly || IsCommentChargeCode; }
		}

		#endregion

		#region JR_IsIncludedInProfitShareInfo

		bool JR_IsIncludedInProfitShare_ReadOnly
		{
			get { return IsCommentChargeCode || (IsInDatabaseAndReadyForRevenuePosting && IsInDatabaseAndReadyForCostPosting); }
		}

		#endregion

		#region JR_AgentDeclaredCostAmtInfo

		bool JR_AgentDeclaredCostAmt_ReadOnly
		{
			get { return IsCommentChargeCode || IsProfitShareAmountsReadOnly; }
		}

		#endregion

		#region JR_AgentDeclaredCostAmtLocalInfo

		protected override bool JR_AgentDeclaredCostAmtLocal_ReadOnly
		{
			get { return IsCommentChargeCode || IsProfitShareAmountsReadOnly; }
		}

		#endregion

		#region JR_AgentDeclaredSellAmtInfo

		bool JR_AgentDeclaredSellAmt_ReadOnly
		{
			get { return IsCommentChargeCode || IsProfitShareAmountsReadOnly; }
		}

		#endregion

		#region JR_AgentDeclaredSellAmtLocalInfo

		protected override bool JR_AgentDeclaredSellAmtLocal_ReadOnly
		{
			get { return IsCommentChargeCode || IsProfitShareAmountsReadOnly; }
		}

		#endregion

		#region JR_DisplaySequenceInfo

		bool JR_DisplaySequence_ReadOnly
		{
			get { return JR_IsRevenuePosted; }
		}

		#endregion

		bool IsProfitShareAmountsReadOnly
		{
			get { return InvoicingJob != null && InvoicingJob.ChargesProfitShareAmountsReadOnly; }
		}

#if DEBUG
		protected override void SimulateAnotherUserProcessAccrualAndWIP_ForTestOnly()
		{
			ProcessAccrualAndWIPViaAnotherUser_ForTestOnly?.Invoke(this, null);
		}

		public EventHandler ProcessAccrualAndWIPViaAnotherUser_ForTestOnly;
#endif

		#endregion

		#region Lookups

		#region PaymentTypes

		public override CodeDescriptionPairList PaymentTypes
		{
			get
			{
				var cacheKey = "Charge_PaymentTypes";
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var paymentTypes = new CodeDescriptionPairList();

					foreach (ICodeDescription pair in base.PaymentTypes)
					{
						if (pair.Code == ReceiptTypes.Cheque
							|| pair.Code == ReceiptTypes.Cash
							|| pair.Code == ReceiptTypes.CreditCard
							|| pair.Code == ReceiptTypes.DirectDebit
							|| pair.Code == ReceiptTypes.EFT
							|| pair.Code == ReceiptTypes.ScheduledEFT
							|| pair.Code == ReceiptTypes.CollectionRequest
							|| pair.Code == ReceiptTypes.eNettDirectDebit
							|| pair.Code == ReceiptTypes.eNettCreditCard)
						{
							paymentTypes.AddPair(pair.Code, pair.Description);
						}
					}

					return paymentTypes;
				});
			}
		}

		#endregion

		#endregion

		#region Customs Invoice Creator

		public bool PostAPWhenInvokedByCustomInvoiceCreator
		{
			get { return fPostAPWhenInvokedByCustomInvoiceCreator; }
			set { fPostAPWhenInvokedByCustomInvoiceCreator = value; }
		}
		bool fPostAPWhenInvokedByCustomInvoiceCreator;

		public bool PostARWhenInvokedByCustomInvoiceCreator
		{
			get { return fPostARWhenInvokedByCustomInvoiceCreator; }
			set { fPostARWhenInvokedByCustomInvoiceCreator = value; }
		}
		bool fPostARWhenInvokedByCustomInvoiceCreator;

		#endregion

		#region Validation

		public new ChargeValidation Validation
		{
			get { return (ChargeValidation)base.Validation; }
		}

		protected override JobChargeValidation GetNewValidation()
		{
			return new ChargeValidation(this, InvoicingJob?.ClosedJobReopener);
		}

		#endregion

		#region ConsumerAdditionalData

		IEnumerable<ICustomProperty> ICustomPropertyContainer.CustomProperties
		{
			get
			{
				if (Job != null)
				{
					var result = Job.Parent as IJobInvoicingAdditionalData;
					if (result != null)
					{
						var customPropertyContainer = result.GetAdditionalProperties();
						customProperties = customPropertyContainer.CustomProperties;
					}
				}
				return customProperties ?? Array.Empty<ICustomProperty>();
			}
		}

		IEnumerable<ICustomProperty> customProperties;

		#endregion

		#region IDefaultLandedCostInput Members

		ZGuid IDefaultLandedCostInput.FKToChargeCode
		{
			get { return JR_AC; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		ZString IDefaultLandedCostInput.ChargeDescription
		{
			get { return ChargeCode == null ? ZString.Empty : ChargeCode.AC_Desc; }
		}

		Money IDefaultLandedCostInput.AmountToDistribute
		{
			get
			{
				Money result = Money.Empty;
				if (SellCurrency != null)
				{
					result = new Money(JR_LocalSellAmt, GlbCompany.CurrentCompany.LocalCurrency);
				}
				return result;
			}
		}

		ZDecimal IDefaultLandedCostInput.ExchangeRate
		{
			get { return 1m; }
		}

		ZBool IDefaultLandedCostInput.IsValidToImport
		{
			get
			{
				return ChargeCode != null && !DisbursementChargeCodes.Contains(ChargeCode.PK) &&
					(!DocumentsDataRegistry.Instance.DoNotPullZeroAmountsFromBillingTab.Value ||
													JR_LocalSellAmt != 0);
			}
		}

		List<ZGuid> DisbursementChargeCodes
		{
			get
			{
				if (fDisbursementChargeCodes == null)
				{
					fDisbursementChargeCodes = new List<ZGuid>();
					fDisbursementChargeCodes.Add((ZGuid)RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
					if (InvoicingJob != null)
					{
						BaseJobDeclaration declaration = InvoicingJob.JobDeclaration;
						if (declaration != null && declaration.CustomsEntryHeaders.Count > 0)
						{
							EntryChargeTypeList entryChargeTypeList = declaration.CustomsEntryHeaders[0].EntryChargeTypeList;
							if (entryChargeTypeList != null && entryChargeTypeList.RegistryItem != null)
							{
								foreach (EntryChargeTypeSetting chargeType in entryChargeTypeList.RegistryItem.Value)
								{
									fDisbursementChargeCodes.Add(chargeType.AC_ChargeCode);
								}
							}
						}
					}
				}
				return fDisbursementChargeCodes;
			}
		}
		List<ZGuid> fDisbursementChargeCodes;
#if DEBUG
		public void ClearfDisbursementChargeCodesForTesting()
		{
			fDisbursementChargeCodes = null;
		}
#endif

		#endregion

		#region IPostingCharge Members

		ZString IPostingCharge.InvoiceType
		{
			get { return JR_InvoiceType; }
			set { JR_InvoiceType = value; }
		}

		IPostingJob IPostingCharge.Job
		{
			get { return InvoicingJob; }
		}

		ZString IPostingCharge.ChargeType
		{
			get { return ChargeType; }
		}

		ZString IPostingCharge.ChargeGroup
		{
			get { return ChargeCode != null ? ChargeCode.AC_ChargeGroup : ZString.Empty; }
		}

		ZGuid IPostingCharge.ChargeCode
		{
			get { return JR_AC; }
		}

		ZGuid IPostingCharge.Branch
		{
			get { return JR_GB; }
		}

		ZGuid IPostingCharge.Department
		{
			get { return JR_GE; }
		}

		ZShort IPostingCharge.DisplaySequence
		{
			get { return JR_DisplaySequence; }
		}

		IDisposable IPostingCharge.SuspendAutoCalculations() => Calculations.SuspendCalculations();

		#region IPayablesPostingCharge Members

		RefCurrency IPayablesPostingCharge.CostCurrency
		{
			get { return CostCurrency; }
		}

		OrgHeader IPayablesPostingCharge.Creditor
		{
			get { return CostAccount; }
		}

		#endregion

		#region IReceivablesPostingCharge Members

		void IReceivablesPostingCharge.SetRevenueTransactionLine(InvoicingLineBase invoiceLine)
		{
			ReverseWIP(invoiceLine.AL_PostDate);
			JR_AL_ARLine = invoiceLine.PK;
		}

		RefCurrency IReceivablesPostingCharge.SellCurrency
		{
			get { return BillInInvoiceCurrency ? SellInvoiceCurrency : SellCurrency; }
		}

		OrgHeader IReceivablesPostingCharge.Debtor
		{
			get { return SellAccount; }
		}

		ZGuid IReceivablesPostingCharge.DebtorAddressPK
		{
			get { return JR_OA_SellInvoiceAddress; }
			set { JR_OA_SellInvoiceAddress = value; }
		}

		ZGuid IReceivablesPostingCharge.DebtorContactPK
		{
			get { return JR_OC_SellInvoiceContact; }
			set { JR_OC_SellInvoiceContact = value; }
		}

		ZGuid IReceivablesPostingCharge.TaxRate
		{
			get { return JR_AT_SellGSTRate; }
		}

		ZShort IReceivablesPostingCharge.TaxRatePostingGroupId
		{
			get { return SellGSTRate != null ? SellGSTRate.AT_PostingGroupId : (ZShort)AccTaxRate.DefaultPostingGroupID; }
		}

		ZDecimal IReceivablesPostingCharge.OSSellAmount
		{
			get { return BillInInvoiceCurrency ? JR_OSSellInvoiceAmt : JR_OSSellAmt; }
		}

		ZDecimal IReceivablesPostingCharge.LocalSellAmount
		{
			get { return JR_LocalSellInvoiceAmt; }
			set
			{
				if (!BillInInvoiceCurrency)
				{
					JR_LocalSellInvoiceAmt = value;
				}
			}
		}

		ZGuid IReceivablesPostingCharge.SellGSTRate
		{
			get { return JR_AT_SellGSTRate; }
		}

		ZDate IReceivablesPostingCharge.SellTaxDate
		{
			get { return JR_SellTaxDate; }
			set { JR_SellTaxDate = value; }
		}

		ZGuid IReceivablesPostingCharge.SellTaxMessage
		{
			get { return JR_A9_SellVATClass; }
		}

		ZGuid IReceivablesPostingCharge.SellWHTRate
		{
			get { return JR_AW_SellWHTRate; }
		}

		ZString IReceivablesPostingCharge.SellReference
		{
			get { return JR_SellReference; }
		}

		bool IReceivablesPostingCharge.PreventInvoicePrintGrouping
		{
			get { return JR_PreventInvoicePrintGrouping; }
		}

		ZDecimal IReceivablesPostingCharge.OSSellTaxAmount
		{
			get { return BillInInvoiceCurrency ? JR_OSSellInvoiceGSTAmt : JR_OSSellGSTAmt_Calc; }
		}

		ZDecimal IReceivablesPostingCharge.LocalSellTaxAmount
		{
			get { return BillInInvoiceCurrency ? JR_SellInvoice_LocalGSTAmount : JR_Sell_LocalGSTAmount; }
		}

		ZDecimal IReceivablesPostingCharge.OSSellWHTAmount
		{
			get { return JR_OSSellWHTAmt; }
		}

		ZDecimal IReceivablesPostingCharge.SellExchangeRate
		{
			get { return BillInInvoiceCurrency ? JR_OSSellInvoiceExRate : JR_OSSellExRate; }
			set
			{
				if (BillInInvoiceCurrency && this.HasContext(BusinessContext.PostingReceivableCharges))
				{
					OverrideOSSellInvoiceExRateForPosting(value);
				}
				else
				{
					JR_OSSellExRate = value;
				}
			}
		}

		ZDecimal IReceivablesPostingCharge.InvoiceSellExchangeRate
		{
			get
			{
				if (BillInInvoiceCurrency)
				{
					return SellInvoiceExchangeRate?.SellRate ?? 1m;
				}
				else
				{
					return RevenueExchangeRate?.SellRate ?? 1m;
				}
			}
		}

		ZDecimal IReceivablesPostingCharge.CFXAmount
		{
			get { return JR_CFXAmt; }
		}

		ZBool IReceivablesPostingCharge.IsRevenuePosted
		{
			get { return IsRevenuePosted; }
		}

		ZBool IReceivablesPostingCharge.IsParentJobInvoicingOnHold
		{
			get { return IsParentJobInvoicingOnHold; }
		}

		ZBool IReceivablesPostingCharge.IsParentJobWorkOnHold
		{
			get { return IsParentJobWorkOnHold; }
		}

		void IReceivablesPostingCharge.ValidateAll()
		{
			if (InvoicingJob != null)
			{
				using (InvoicingJob.ChargesLoadSuspender.GetSuspender())
				{
					Validation.ValidateAll();
				}
			}
		}

		void IReceivablesPostingCharge.InitializeSellAddressContact()
		{
			using (TaxAmountAdjustmentSuspender.GetSuspender())
			{
				if (!JR_OC_SellInvoiceContact.IsValid && Job != null && (JR_OH_SellAccount == Job.LocalChargesPK || JR_OH_SellAccount == Job.AgentCollectPK))
				{
					JR_OC_SellInvoiceContact = DisplaySellInvoiceContact;
				}

				if (!JR_OA_SellInvoiceAddress.IsValid)
				{
					JR_OA_SellInvoiceAddress = DisplaySellInvoiceAddress;
				}
			}
		}

		ZDateTime IReceivablesPostingCharge.ARInvoiceDate
		{
			get
			{
				return this.JR_Calc_ARInvoiceDate;
			}
		}

		ZDateTime IReceivablesPostingCharge.ARInvoiceTaxDate => JR_SellTaxDate;

		ZString IReceivablesPostingCharge.GovtChargeCode
		{
			get { return JR_SellGovtChargeCode; }
		}

		ZString IReceivablesPostingCharge.SellPlaceOfSupply => JR_SellPlaceOfSupply;

		ZString IReceivablesPostingCharge.SellSupplyType => JR_SellSupplyType;

		ZDecimal IReceivablesPostingCharge.OsExTaxAmount
		{
			get
			{
				var result = JR_OSSellAmt;
				if (BillInLocalCurrency)
				{
					result = JR_LocalSellAmt;
				}
				else if (BillInInvoiceCurrency)
				{
					result = JR_OSSellInvoiceAmt;
				}
				return result;
			}
		}

		ZDecimal IReceivablesPostingCharge.OsTaxAmount
		{
			get
			{
				var result = JR_OSSellGSTAmt_Calc;
				if (BillInLocalCurrency)
				{
					result = JR_Sell_LocalGSTAmount;
				}
				else if (BillInInvoiceCurrency && !BIllInInvoiceCurrencySameAsSellCurrency)
				{
					result = JR_OSSellInvoiceGSTAmount;
				}
				return result;
			}
		}

		ZGuid IReceivablesPostingCharge.SellTaxBranch => JR_GB_SellTaxBranch;

		#region ISellComplianceDescription members

		ZString ISellComplianceDescription.Description { get => JR_Desc; set => JR_Desc = value; }

		ZString ISellComplianceDescription.SellComplianceDescription => SellComplianceDescription;

		#endregion

		#endregion

		#endregion

		#region IQuickCalculatorCharge Members

		bool IQuickCalculatorCharge.CanUpdateSell
		{
			get { return !JR_IsRevenuePosted && !IsDisbursementCharge; }
		}

		bool IQuickCalculatorCharge.CanUpdateCost
		{
			get { return !JR_IsCostPosted && !JR_IsApportioned && !IsRevenueCharge; }
		}

		void IQuickCalculatorCharge.SetAmount(CostSell costSell, AutoRateInfo result, OrgHeader org, bool recalculateOverriden)
		{
			Charge updatedCharge = null;
			if (costSell == CostSell.Revenue)
			{
				if (org != null)
				{
					JR_OH_SellAccount = org.PK;
				}

				updatedCharge =
					InvoicingJob.AddAutorateRevenue(result, this, recalculateOverriden, fromAutorating: false);
				JR_SellRated = false;
				SetSellRatingOverrideAlways(true);
			}
			else if (costSell == CostSell.Cost)
			{
				updatedCharge =
					InvoicingJob.AddAutorateCost(result, this, recalculateOverriden, fromAutorating: false);
				JR_CostRated = false;
				SetCostRatingOverrideAlways(true);
			}

			if (updatedCharge != null)
			{
				updatedCharge.AddAttributes(result.Attributes);
			}
		}

		string IQuickCalculatorCharge.SellCurrencyCode
		{
			get { return SellCurrency != null ? SellCurrency.RX_Code : ZString.Empty; }
		}

		string IQuickCalculatorCharge.CostCurrencyCode
		{
			get { return CostCurrency != null ? CostCurrency.RX_Code : ZString.Empty; }
		}

		string IQuickCalculatorCharge.RatingBehaviour
		{
			get { return string.Empty; }
		}

		BusinessObjectCollection<JobPaymentBasis> IPaymentBasisViewCharge.CostPaymentBasesView
		{
			get
			{
				var collection = new JobPaymentBasisViewCollection(Factory);
				collection.AddRange(CostPaymentBases);
				return collection;
			}
		}

		BusinessObjectCollection<JobPaymentBasis> IPaymentBasisViewCharge.SellPaymentBasesView
		{
			get
			{
				var collection = new JobPaymentBasisViewCollection(Factory);
				collection.AddRange(SellPaymentBases);
				return collection;
			}
		}

		#endregion

		#region Tax

		public ZBool ShouldDisplayTaxApplicabilityForRatingHeader
		{
			get
			{
				if (ChargeCode != null && InvoicingJob != null)
				{
					var parameters = InvoicingJob.GetTaxCalculationParameters();
					parameters.Organisation = InvoicingJob.LocalCharges;
					parameters.Branch = GlbBranch.CurrentBranch;

					parameters.CostOrSell = CostSell.Cost;
					parameters.FixedPlaceOfSupply = CostPlaceOfSupplyLocation;
					AccTaxRate gSTRateCost = ChargeCode.GetGSTRate(parameters, out _);

					parameters.CostOrSell = CostSell.Revenue;
					parameters.FixedPlaceOfSupply = SellPlaceOfSupplyLocation;
					AccTaxRate gSTRateRevenue = ChargeCode.GetGSTRate(parameters, out _);

					return InvoicingJob.JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix &&
					  ((gSTRateCost != null && gSTRateCost.GetRate(JR_CostTaxDate) > 0) || (gSTRateRevenue != null && gSTRateRevenue.GetRate(JR_SellTaxDate) > 0));
				}
				else
				{
					return false;
				}
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
		public ZDecimal GrossWeight => InvoicingJob?.PlugInData.GrossWeightInKilos() ?? 0m;
		public ZDecimal GrossVolume => InvoicingJob?.PlugInData.GrossVolumeInCubicMeters() ?? 0m;

		ZBool IApportionedCharge.IsUsedForApportionment
		{
			get { return IsUsedForApportionment; }
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
			get { return JR_RX_NKCostCurrency; }
			set { JR_RX_NKCostCurrency = value; }
		}

		ZDecimal IApportionedCharge.ExcessActualVolumeWeight => InvoicingJob != null ? InvoicingJob.ExcessActualVolumeWeight : 0;

		ZDecimal IApportionedCharge.ExcessChargeableVolumeWeight => InvoicingJob != null ? InvoicingJob.ExcessChargeableVolumeWeight : 0;

		#endregion

		#region RevenueApportionment

		[ResourceStringData("Charge|ChargeableRate", Caption = "Chargeable Rate", FullDescription = "Jobs’ Chargeable Rate = Apportioned Cost / Chargeable Units")]
		public ZString ChargeableRateForRevenueApportionment => GetChargeableRate(ParentConsolRevenue?.ApportionmentMethod, JR_LocalSellAmt);

		public ZPropertyInfo ChargeableRateForRevenueApportionmentInfo => GetZPropertyInfo(nameof(ChargeableRateForRevenueApportionment));

		internal ConsolRevenue.ConsolRevenue ParentConsolRevenue
		{
			get;
			set;
		}

		#endregion

		#region ICharge Members

		bool ICharge.IsBillInLocalCurrency
		{
			get { return BillInLocalCurrency; }
		}

		bool ICharge.IsBillInInvoiceCurrency
		{
			get { return BillInInvoiceCurrency; }
		}

		decimal ICharge.LocalSellInvoiceAmt
		{
			get { return JR_LocalSellInvoiceAmt; }
		}

		decimal ICharge.LocalSellGSTAmt
		{
			get { return JR_Sell_LocalGSTAmount; }
		}

		decimal ICharge.SellInvoiceCurrencyExRate
		{
			get { return JR_OSSellInvoiceExRate; }
		}

		decimal ICharge.OSSellInvoiceAmt
		{
			get { return JR_OSSellInvoiceAmt; }
		}

		decimal ICharge.CFXAmt
		{
			get { return JR_CFXAmt; }
		}

		#endregion
		public TaxTransactionsLinkedToJobChargeForDisplay TaxTransactionsLinkedToJobChargeForDisplay
		{
			get
			{
				return Factory.GetCachedValue("TaxTransactionsLinkedToJobChargeForDisplay|" + PK, () => new TaxTransactionsLinkedToJobChargeForDisplay(Factory, JR_AL_APLine, JR_AL_ARLine), CacheStalenessPolicy.StaleOnFactorySave);
			}
		}
	}
}
