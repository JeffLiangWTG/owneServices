using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalLine : DependentTransactionLine, IDebitCreditAmounts
	{
		public JobRevenueJournalLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobRevenueJournal ParentJournal
		{
			get { return (JobRevenueJournal)MasterTransactionHeader; }
		}

		protected override ZString LineType
		{
			get { return SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused && !AL_LineType.IsEmpty ? AL_LineType.ToString() : TransactionLineTypes.Revenue; }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override BaseCharge GetRelatedJobChargeCore()
		{
			if (relatedCharge_innerValue == null || relatedCharge_innerValue.IsDeleted ||
				 relatedCharge_innerValue.JR_AL_ARLine != PK && relatedCharge_innerValue.JR_AL_APLine != PK)
			{
				relatedCharge_innerValue = GetRelatedChargeWithCache(JobChargeSchema.JR_AL_ARLine) ?? GetRelatedChargeWithCache(JobChargeSchema.JR_AL_APLine);
			}
			return relatedCharge_innerValue != null && relatedCharge_innerValue.IsDeleted ? null : relatedCharge_innerValue;
		}

		public void CopyValuesFrom(JobRevenueJournalLine line)
		{
			AL_RX_NKTransactionCurrency = line.AL_RX_NKTransactionCurrency;
			AL_ExchangeRate = line.AL_ExchangeRate;

			SuspendAL_JHSettingDefaults();
			try
			{
				AL_JH = line.AL_JH;
			}
			finally
			{
				ResumeAL_JHSettingDefaults();
			}

			AL_AC = line.AL_AC;
			AL_Desc = line.AL_Desc;

			AL_GB = line.AL_GB;
			AL_GE = line.AL_GE;
			AL_RevRecognitionType = line.AL_RevRecognitionType;

			DebitCreditSign = line.DebitCreditSign;
			fCostRevenueType = line.CostRevenueType;
			AL_LineType = line.AL_LineType;

			AL_AG = line.AL_AG;

			OSUnsignedLineAmount = line.OSUnsignedLineAmount;
			LocalUnsignedLineAmount = line.LocalUnsignedLineAmount;
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (TransactionHeader != null && TransactionHeader.IsCancelled)
			{
				return GetEmptyValidation();
			}
			else
			{
				return new JobRevenueJournalLineValidation(this, ParentJournal?.ClosedJobReopener);
			}
		}

		JobRevenueJournalLineValidation JournalLineValidation
		{
			get { return Validation as JobRevenueJournalLineValidation; }
		}

		ZBool IsJournalCreatedViaBillingTab => ParentJournal != null && ParentJournal.Factory.HasContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule);

		ZBool DoesUserHasSecurityRightsToOverrideCostRevenueType => IsJournalCreatedViaBillingTab ? ParentJournal.SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCostRevenueType) : Env.Security.JobRevenueJournalAllowOverrideCostRevenueType.IsAllowed;

		bool SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused => AccountingConfigurationRegistry.Instance.SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		void SetDefaultCostRevenueType()
		{
			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SetDefaultCostRevenueType), () => Factory.RemoveContext(BusinessContext.SetDefaultCostRevenueType)))
			{
				var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code)
				{
					CostRevenueType = TransactionLineTypes.Revenue;
				}
				else if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code)
				{
					CostRevenueType = TransactionLineTypes.Cost;
				}
				else if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
				{
					CostRevenueType = (DebitCreditSign == DebitCreditDataEntry.DR) ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue;
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DebitCreditSign = DebitCreditDataEntry.DR;
			SetDefaultCostRevenueType();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			using (SuspendSettingHasChanges())
			{
				OSAmountDebitCredit.OnLoaded();
			}
		}

		public override ZString AL_RX_NKTransactionCurrency
		{
			get
			{
				return base.AL_RX_NKTransactionCurrency;
			}
			set
			{
				base.AL_RX_NKTransactionCurrency = value;
				SetExchangeRate();
			}
		}

		public override ZGuid AL_JH
		{
			get { return base.AL_JH; }
			set
			{
				base.AL_JH = value;
				if (!AL_JHSettingDefaultsSuspended)
				{
					CalculateBranchAndDepartmentFromJob();
					SetExchangeRate();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public override ZGuid AL_AC
		{
			get { return base.AL_AC; }
			set
			{
				base.AL_AC = value;

				if (ChargeCode != null)
				{
					if (ChargeCode.IsComment)
					{
						OSUnsignedLineAmount = 0M;
					}
					else
					{
						if (AL_Desc.IsEmpty || AL_Desc == DefaultDescription)
						{
							AL_Desc = ChargeCode.AC_Desc;
							DefaultDescription = AL_Desc;
						}
					}
				}
			}
		}

		protected override ZGuid GetGLAccountPKFromChargeCodeAccounts(AccChargeCode.GLPostingAccounts glPostingAccounts)
		{
			var glAccount = ZGuid.Empty;
			var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code)
			{
				glAccount = glPostingAccounts.RevenueAccount;
			}
			else if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code)
			{
				glAccount = glPostingAccounts.CostAccount;
			}
			else if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
			{
				glAccount = (CostRevenueType == TransactionLineTypes.Cost) ? glPostingAccounts.CostAccount : glPostingAccounts.RevenueAccount;
			}
			return glAccount;
		}

		#region DebitCreditSign

		[MaxLength(2)]
		[List("DebitCreditSignList")]
		public ZString DebitCreditSign
		{
			get { return LocalAmountDebitCredit.DebitCreditSign; }
			set
			{
				CheckMaximumLength(DebitCreditSignInfo, value);

				LocalAmountDebitCredit.DebitCreditSign = value;
				OSAmountDebitCredit.DebitCreditSign = LocalAmountDebitCredit.DebitCreditSign;
				if (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty)
					== AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code && ParentJournal != null && !ParentJournal.IsReverseTransaction)
				{
					using (new DisposableAction(() => Factory.SetContext(BusinessContext.SetDefaultCostRevenueType), () => Factory.RemoveContext(BusinessContext.SetDefaultCostRevenueType)))
					{
						CostRevenueType = (DebitCreditSign == DebitCreditDataEntry.DR) ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue;
					}
				}

				if (JournalLineValidation != null)
				{
					JournalLineValidation.ValidateDebitCreditSign();
				}
				DebitCreditSignInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DebitCreditSignInfo
		{
			get { return GetZPropertyInfo(nameof(DebitCreditSign)); }
		}

		public CodeDescriptionPairList DebitCreditSignList
		{
			get { return OSAmountDebitCredit.List; }
		}

		public void ReverseDebitCreditSign()
		{
			DebitCreditSign = DebitCreditSign == DebitCreditDataEntry.DR ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;
		}

		#endregion

		#region CostRevenueType

		[MaxLength(3)]
		[List("CostRevenueTypeList")]
		public ZString CostRevenueType
		{
			get
			{
				var result = fCostRevenueType;
				if (IsInDatabase && RelatedJobCharge != null)
				{
					if (PK == RelatedJobCharge.JR_AL_ARLine)
					{
						result = TransactionLineTypes.Revenue;
					}
					else if (PK == RelatedJobCharge.JR_AL_APLine)
					{
						result = TransactionLineTypes.Cost;
					}
				}
				return result;
			}
			set
			{
				CheckMaximumLength(CostRevenueTypeInfo, value);
				if (Factory.HasContext(BusinessContext.SetDefaultCostRevenueType) || DoesUserHasSecurityRightsToOverrideCostRevenueType)
				{
					fCostRevenueType = value;

					if (SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused)
					{
						AL_LineType = value;
					}

					var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
					{
						DefaultGLAccounts();
					}
					JournalLineValidation?.ValidateCostRevenueType();
					CostRevenueTypeInfo.RefreshBinding();
				}
				else
				{
					if (IsJournalCreatedViaBillingTab)
					{
						ParentJournal.SecurityHelper.ShowError(SecurityCore.AllowOverrideCostRevenueType);
					}
					else
					{
						Env.Security.ShowError(Env.Security.JobRevenueJournalAllowOverrideCostRevenueType);
					}
				}
			}
		}
		ZString fCostRevenueType;

		public ZPropertyInfo CostRevenueTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CostRevenueType)); }
		}

		protected bool CostRevenueType_ReadOnly
		{
			get
			{
				var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				return registrySetting != AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code;
			}
		}

		public CodeDescriptionPairList CostRevenueTypeList
		{
			get
			{
				if (fCostRevenueTypeList == null)
				{
					fCostRevenueTypeList = new CodeDescriptionPairList();
					fCostRevenueTypeList.AddPair(TransactionLineTypes.Cost, Res.GetString("7e3bebea-da0d-46aa-ba67-f53ab0a0148a", "Cost"));
					fCostRevenueTypeList.AddPair(TransactionLineTypes.Revenue, Res.GetString("5389648b-aa12-4c4a-a766-a316124884d2", "Revenue"));
				}
				return fCostRevenueTypeList;
			}
		}
		CodeDescriptionPairList fCostRevenueTypeList;

		#endregion

		#region Post To GL Account

		public ZString PostToGLAccount => GLHeader?.AccountNum ?? ZString.Empty;

		public ZPropertyInfo PostToGLAccountInfo
		{
			get { return GetZPropertyInfo(nameof(PostToGLAccount)); }
		}

		protected bool PostToGLAccount_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region UnsignedLineAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal OSUnsignedLineAmount
		{
			get { return OSAmountDebitCredit.UnsignedAmount; }
			set
			{
				OSAmountDebitCredit.UnsignedAmount = value;
				if (JournalLineValidation != null)
				{
					JournalLineValidation.ValidateOSUnsignedLineAmount();
					JournalLineValidation.ValidateLocalUnsignedLineAmount();
				}
				OSUnsignedLineAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OSUnsignedLineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSUnsignedLineAmount)); }
		}

		protected bool OSUnsignedLineAmount_ReadOnly
		{
			get { return ChargeCode != null && ChargeCode.IsComment; }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalUnsignedLineAmount
		{
			get { return LocalAmountDebitCredit.UnsignedAmount; }
			set
			{
				LocalAmountDebitCredit.UnsignedAmount = value;
				if (JournalLineValidation != null)
				{
					JournalLineValidation.ValidateOSUnsignedLineAmount();
					JournalLineValidation.ValidateLocalUnsignedLineAmount();
				}
				LocalUnsignedLineAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocalUnsignedLineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalUnsignedLineAmount)); }
		}

		protected bool LocalUnsignedLineAmount_ReadOnly
		{
			get { return OSUnsignedLineAmount_ReadOnly; }
		}

		DebitCreditDataEntry OSAmountDebitCredit
		{
			get
			{
				if (OSAmountDebitCredit_cached == null)
				{
					SetOSAmountDebitCredit(false);
				}
				return OSAmountDebitCredit_cached;
			}
		}
		DebitCreditDataEntry OSAmountDebitCredit_cached;

		public void SetOSAmountDebitCredit(bool isDebitForPositiveAmount)
		{
			OSAmountDebitCredit_cached = new DebitCreditDataEntry(() => AL_OSExTaxAmount, x => AL_OSExTaxAmount = x, isDebitForPositiveAmount);
		}

		DebitCreditDataEntry LocalAmountDebitCredit
		{
			get
			{
				if (LocalAmountDebitCredit_cached == null)
				{
					SetLocalAmountDebitCredit(false);
				}
				return LocalAmountDebitCredit_cached;
			}
		}
		DebitCreditDataEntry LocalAmountDebitCredit_cached;

		public void SetLocalAmountDebitCredit(bool isDebitForPositiveAmount)
		{
			LocalAmountDebitCredit_cached = new DebitCreditDataEntry(() => AL_LocalExTaxAmount, x => AL_LocalExTaxAmount = x, isDebitForPositiveAmount);
		}

		#endregion

		#region SuspendAL_JHSettingDefaults

		bool AL_JHSettingDefaultsSuspended;

		public void SuspendAL_JHSettingDefaults()
		{
			AL_JHSettingDefaultsSuspended = true;
		}

		public void ResumeAL_JHSettingDefaults()
		{
			AL_JHSettingDefaultsSuspended = false;
		}

		#endregion

		void SetExchangeRate()
		{
			var localCurrency = Env.CurrentCompany.LocalCurrency;

			if (localCurrency == null || AL_RX_NKTransactionCurrency != localCurrency.Code)
			{
				AL_ExchangeRate = GetExchangeRateFromJob(InvoicingJob);
			}
			else
			{
				AL_ExchangeRate = 1M;
			}
		}

		ZDecimal GetExchangeRateFromJob(Job job)
		{
			ZDecimal rate = ZDecimal.Zero;

			if (job != null)
			{
				job.InitializeParentFromGenericJobWithSettingDefaults();

				var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(Company, ExchangeRateValidLedgerEnum.AR, AL_RX_NKTransactionCurrency);
				rate = job.GetExchangeRate(AL_RX_NKTransactionCurrency, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateType.Buy, invoiceCurrencyType);

				if (rate.IsEmpty)
				{
					rate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TransactionCurrency, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
				}

				if (rate.IsEmpty)
				{
					ZAccExchangeRate accExRate = ExchangeRate as ZAccExchangeRate;
					if (accExRate != null)
					{
						rate = accExRate.TodaysRate(AL_RX_NKTransactionCurrency);
					}
				}
			}

			return rate;
		}

		public override ExchangeRateType RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		ZString DefaultDescription;
	}
}
