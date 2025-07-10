using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;
using AccGenericChargeCollection = Enterprise.Accounting.Business.GenericCharge.GenericChargeCollection;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class IntercompanyCostsApportionmentInvoiceLine : NonPersistentBusinessObject, IObsoleteValidation, IGenericChargeCollectionRequired, ITransactionLineTaxDate
	{
		#region Schema

		public abstract class Schema
		{
			public const string BranchName = "BranchName";
			public const string DepartmentDescription = "DepartmentDescription";
			public const string TaxBranch = "TaxBranch";
			public const string TaxBranchName = "TaxBranchName";
		}

		#endregion

		public IntercompanyCostsApportionmentInvoiceLine(BusinessObjectFactory factory, IntercompanyCostsApportionmentInvoice invoice)
			: base(factory)
		{
			Invoice = invoice;
			InvoicingLineTaxDateCacheProvider = new InvoicingLineTaxDateCacheProvider(this, () => Invoice?.InvoiceTaxDateCacheProvider);
		}

		#region Properties

		#region AL_AG

		public ZGuid AL_AG
		{
			get { return fAL_AG; }
			set
			{
				if (fAL_AG != value)
				{
					SetNonPersistentPropertyValue(AL_AGInfo, ref fAL_AG, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateAL_AG();
				}
			}
		}
		ZGuid fAL_AG;

		public ZPropertyInfo AL_AGInfo
		{
			get { return GetZPropertyInfo(nameof(AL_AG)); }
		}

		public void ValidateAL_AG()
		{
			AL_AGInfo.ClearAllNotifications();

			if (GLHeader != null && !GLHeader.AG_IsGlobal)
			{
				bool result = true;
				var template = ApportionmentTemplate;
				if (template != null)
				{
					foreach (AccApportionmentTemplateLines line in template.Lines)
					{
						if (!GLHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.ACF_GC_Company == line.Company))
						{
							result = false;
							break;
						}
					}

					if (!result)
					{
						AL_AGInfo.AddError(Res.GetString("e499503a-f3e2-4bde-abbc-5d87df8982b5", "This GL Account cannot be used for the companies set up in the apportionment template"));
					}
				}
			}
		}

		#endregion

		#region AL_AC

		public ZGuid AL_AC
		{
			get { return fAL_AC; }
			set
			{
				if (fAL_AC != value)
				{
					using (InvoicingLineTaxDateCacheProvider.BindChargeEvents())
					{
						SetNonPersistentPropertyValue(AL_ACInfo, ref fAL_AC, value);
					}
					InvoicingLineTaxDateCacheProvider.StaleCache();

					if (AL_AC.IsValid && AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
					{
						AL_GovtChargeCode = ChargeCode?.AC_GovtChargeCode ?? ZString.Empty;
					}
					else
					{
						AL_GovtChargeCode = ZString.Empty;
					}
				}
			}
		}
		ZGuid fAL_AC;

		public ZPropertyInfo AL_ACInfo
		{
			get { return GetZPropertyInfo(nameof(AL_AC)); }
		}

		#endregion

		#region AL_GovtChargeCode

		public ZString AL_GovtChargeCode
		{
			get { return al_GovtChargeCode; }
			set
			{
				if (al_GovtChargeCode != value)
				{
					SetNonPersistentPropertyValue(AL_GovtChargeCodeInfo, ref al_GovtChargeCode, value);
				}

				if (!IsValidationSuspended)
				{
					ValidateAL_GovtChargeCode();
				}
			}
		}

		void ValidateAL_GovtChargeCode()
		{
			AL_GovtChargeCodeInfo.ClearAllNotifications();
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				MandatoryValidation.CheckEntered(AL_GovtChargeCodeInfo);
			}
		}

		ZString al_GovtChargeCode;

		public ZPropertyInfo AL_GovtChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AL_GovtChargeCode)); }
		}

		protected bool AL_GovtChargeCode_ReadOnly
		{
			get { return !Env.Security.NewPayablesOverrideGovtCCodeAllows.IsAllowed; }
		}

		#endregion

		#region AccGLHeader

		public AccGLHeader GLHeader
		{
			get { return Factory.Load<AccGLHeader>(AL_AG); }
		}

		#endregion

		#region ChargeCode

		public AccChargeCode ChargeCode
		{
			get { return Factory.Load<AccChargeCode>(AL_AC); }
		}

		#endregion

		#region AL_AT

		[List("TaxRates")]
		public ZGuid AL_AT
		{
			get { return fAL_AT; }
			set
			{
				if (fAL_AT != value)
				{
					SetNonPersistentPropertyValue(AL_ATInfo, ref fAL_AT, value);
					InvoicingLineTaxDateCacheProvider.StaleCache();

					using (UpdateTaxSuspender.GetSuspender())
					{
						if (!fAL_AT.IsValid)
						{
							AL_TaxDate = ZDate.Empty;
						}
						else if (AL_TaxDate.IsEmpty)
						{
							AL_TaxDate = ZDate.Today;
						}
					}
					updateTax();
					updateTaxMessage();
				}
				if (!IsValidationSuspended)
				{
					ValidateAL_AT();
				}
			}
		}
		ZGuid fAL_AT;

		public ZPropertyInfo AL_ATInfo
		{
			get { return GetZPropertyInfo(nameof(AL_AT)); }
		}

		public bool AL_AT_ReadOnly
		{
			get
			{
				return ChargeCode != null && ChargeCode.IsComment ||
					!(IsCurrentCompanyGSTRegistered && IsCurrentOrganisationGSTRegistered && (AllowUserGSTOverride || IsCurrentChargeGLAccount));
			}
		}

		public void ValidateAL_AT()
		{
			AL_ATInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AL_ATInfo);
			if (!AL_AT.IsValid && !AL_ATInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(AL_ATInfo);
			}
		}
		#endregion

		#region AL_TaxDate

		public ZDate AL_TaxDate
		{
			get { return al_TaxDate; }
			set
			{
				if (al_TaxDate != value)
				{
					SetNonPersistentPropertyValue(AL_TaxDateInfo, ref al_TaxDate, value);
					InvoicingLineTaxDateCacheProvider.StaleCache();

					if (!Invoice.IsLocalCurrencyTransaction && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, Invoice.IsLocalCurrencyTransaction, GlbCompany.CurrentCompany.PK,
										AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code))
					{ 
						Invoice.SetExchangeRate();
					}
					else
					{
						updateTax();
					}

					if (!IsValidationSuspended)
					{
						ValidateAL_TaxDate();
					}
				}
			}
		}
		ZDate al_TaxDate;

		public ZPropertyInfo AL_TaxDateInfo => GetZPropertyInfo(nameof(AL_TaxDate));

		public bool AL_TaxDate_ReadOnly => !AL_AT.IsValid;

		public void ValidateAL_TaxDate()
		{
			AL_TaxDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateRange(AL_TaxDateInfo);

			if (TaxRate == null)
			{
				return;
			}

			MandatoryValidation.CheckEntered(AL_TaxDateInfo);
			if (!AL_TaxDateInfo.HasErrors())
			{
				var rateExists = TaxRate.DoesRateExists(AL_TaxDate);
				if (!rateExists)
				{
					AL_TaxDateInfo.AddError(Res.GetString("a70c77c7-c890-4c5d-afa3-1e222809b5a5", "No rate found for selected date."));
				}
			}
		}

		#endregion

		#region AL_A9_VATClass

		[List("TaxMessages")]
		public ZGuid AL_A9_VATClass
		{
			get { return fAL_A9_VATClass; }
			set
			{
				if (fAL_A9_VATClass != value)
				{
					SetNonPersistentPropertyValue(AL_A9_VATClassInfo, ref fAL_A9_VATClass, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateAL_A9_VATClass();
				}
			}
		}
		ZGuid fAL_A9_VATClass;

		public ZPropertyInfo AL_A9_VATClassInfo
		{
			get { return GetZPropertyInfo(nameof(AL_A9_VATClass)); }
		}

		public bool AL_A9_VATClass_ReadOnly
		{
			get
			{
				bool result = false;
				if (AL_AT == ZGuid.Empty || AL_AT_ReadOnly)
				{
					result = true;
				}
				else
				{
					result = !Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed;
				}
				return result;
			}
		}

		public void ValidateAL_A9_VATClass()
		{
			AL_A9_VATClassInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AL_A9_VATClassInfo);
		}
		#endregion

		#region TaxMessages

		public AccInvMsgCollection TaxMessages
		{
			get
			{
				if (fTaxMessages == null)
				{
					fTaxMessages = new AccInvMsgCollection(Factory, TaxRate?.AT_RN_NKCountry ?? ZString.Empty);
				}
				return fTaxMessages;
			}
		}
		AccInvMsgCollection fTaxMessages;

		#endregion

		#region TaxRates

		public AccTaxRateCollection TaxRates
		{
			get
			{
				if (fTaxRates == null)
				{
					ZQuery taxRatesFilter = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
					fTaxRates = new VATAccTaxRateCollection(Factory, taxRatesFilter);
				}
				return fTaxRates;
			}
		}
		AccTaxRateCollection fTaxRates;

		#endregion

		#region TaxRate

		public AccTaxRate TaxRate
		{
			get { return Factory.Load<AccTaxRate>(AL_AT); }
		}

		#endregion

		#region FallbackTaxRate

		protected AccTaxRate GetFallbackTaxRate(out ZGuid overrideInvTaxMsg)
		{
			AccTaxRate rate = null;
			overrideInvTaxMsg = ZGuid.Empty;
			if (ChargeCode != null)
			{
				AccTaxRate rateOverride = GetChargeCodeTaxRateOverride(out overrideInvTaxMsg);
				if (rateOverride != null)
				{
					rate = rateOverride;
				}
				else
				{
					rate = ChargeCode.GSTRate;
				}
			}
			return rate;
		}

		AccTaxRate GetChargeCodeTaxRateOverride(out ZGuid overrideInvTaxMsg)
		{
			AccTaxRate rateOverride = null;
			overrideInvTaxMsg = ZGuid.Empty;

			var chargeCode = ChargeCode;
			var header = chargeCode != null ? Invoice?.Header : null;
			if (header != null)
			{
				var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
				{
					CostOrSell = CostSell.Cost,
					Organisation = header,
					Branch = BranchObject,
				};

				rateOverride = chargeCode.GetGSTRate(parameters, out overrideInvTaxMsg);
			}
			return rateOverride;
		}

		#endregion

		#region AL_AW

		public ZGuid AL_AW
		{
			get { return fAL_AW; }
			set
			{
				if (fAL_AW != value)
				{
					SetNonPersistentPropertyValue(AL_AWInfo, ref fAL_AW, value);
				}
			}
		}
		ZGuid fAL_AW;

		public ZPropertyInfo AL_AWInfo
		{
			get { return GetZPropertyInfo(nameof(AL_AW)); }
		}

		#endregion

		#region GenericCharge

		public AccGenericCharge GenericTransactionCharge
		{
			get { return fGenericTransactionCharge; }
			set { fGenericTransactionCharge = value; }
		}
		AccGenericCharge fGenericTransactionCharge;
		
		void LoadGenericCharge(ZGuid chargePK)
		{
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, chargePK);
			GenericTransactionCharge = Factory.LoadTop1<AccGenericCharge>(filter);
		}

		public AccGenericCharge GenericChargeBizO
		{
			get
			{
				ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, (AL_AG.IsValid ? AL_AG : AL_AC));
				return Factory.LoadTop1(typeof(AccGenericCharge), filter) as AccGenericCharge;
			}
		}

		[RelatedBusinessObject("GenericChargeBizO")]
		[List("ChargeList")]
		public ZGuid GenericCharge
		{
			get
			{
				return fGenericCharge;
			}
			set
			{
				if (value != fGenericCharge)
				{
					ZGuid chargeGuid = value;

					if (chargeGuid.IsValid)
					{
						LoadGenericCharge(chargeGuid);
						if (GenericTransactionCharge != null)
						{
							Description = GenericTransactionCharge.VC_Description;

							if (GenericTransactionCharge.VC_IsGLAccount)
							{
								AL_AG = GenericTransactionCharge.PK;
							}
							else
							{
								AL_AC = GenericTransactionCharge.PK;

								if (Invoice != null)
								{
									if (Invoice.IsMiscServTaxApplicable)
									{
										CalculateGST(IsGSTMandatory);
									}

									if (Invoice.IsMiscServWHTApplicable)
									{
										AL_AW = GenericTransactionCharge.VC_WHTRate;
									}
								}
							}

							if (GenericTransactionCharge != null)
							{
								//TODO: Check if this is needed
								CalculateDepartmentFromDepartmentFilterlist();
							}
						}
						else
						{
							SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, ZGuid.Empty);
							GenericTransactionCharge = null;
						}
						AL_ATInfo.RefreshBinding();
						AL_A9_VATClassInfo.RefreshBinding();
						AL_AWInfo.RefreshBinding();
					}
					else
					{
						Description = ZString.Empty;
					}
					SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, value);
				}

				GenericChargeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateGenericCharge();
				}
			}
		}
		ZGuid fGenericCharge;

		public ZPropertyInfo GenericChargeInfo
		{
			get { return GetZPropertyInfo(nameof(GenericCharge)); }
		}

		public void ValidateGenericCharge()
		{
			GenericChargeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(GenericChargeInfo, ChargeList, ResString.GetMultilingualString("4e03adfd-355c-4243-9eff-f518261c11bc", "Please enter a valid Charge."));
			if (GenericCharge == ZGuid.Empty)
			{
				GenericChargeInfo.AddError(Res.GetString("4f0bcbb5-1b26-4969-874b-21408e61e5be", "Please enter a valid Charge."));
			}
		}

		void CalculateDepartmentFromDepartmentFilterlist()
		{
			ZString deptFillterList = GenericTransactionCharge.VC_DepartmentFilterList;
			if (!deptFillterList.IsEmpty && !deptFillterList.Contains(',') && deptFillterList.ToUpper() != "ALL")
			{
				GlbDepartment dept = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, deptFillterList.Trim());

				if (dept != null)
				{
					AL_GE = dept.PK;
				}
			}
		}

		#endregion

		#region ChargeList

		public AccGenericChargeCollection ChargeList
		{
			get
			{
				var chargeCollectionBuilder = new APOverheadsGenericChargeCollectionBuilder(this);
				var fChargeList = chargeCollectionBuilder.GetBuiltButNotLoadedCollection(ShowGLAccountsForImportAction);
				fChargeList.Load(chargeCollectionBuilder.GetQueryForValidation());
				return fChargeList;
			}
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		#endregion

		#region Description

		[MaxLength(80)]
		public ZString Description
		{
			get { return fDescription; }
			set
			{
				if (fDescription != value)
				{
					SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);
					updateApportionmentsDescription();
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region Branch

		[List("Branches")]
		public ZGuid Branch
		{
			get
			{
				return fBranch;
			}
			set
			{
				if (fBranch != value)
				{
					SetNonPersistentPropertyValue(BranchInfo, ref fBranch, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateBranch();

					if (GlbBranchCombinationValidation.ShouldValidateCombination(BranchObject))
					{
						ValidateDepartment();
					}
				}
			}
		}
		ZGuid fBranch;

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(nameof(Branch)); }
		}

		public void ValidateBranch()
		{
			BranchInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(BranchInfo, Branches, ResString.GetMultilingualString("20f77d44-f5a1-46b4-ac39-f519b71eca3a", "Please enter a valid Branch."));

			if (Invoice.Lines.Count > 1
				&& AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
			{
				if (!BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting
					, Invoice.Lines.Cast<IntercompanyCostsApportionmentInvoiceLine>().Select(x => x.Branch).ToHashSet()))
				{
					BranchInfo.AddError(Res.GetString("77781527-2264-4b24-b846-13e95f446f1d", @"Please review the charge lines entered and ensure all charges have been entered belong to the same Posting Group. All charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because charges have been entered using a mix of Posting Groups."));
				}
			}
		}

		GlbBranch BranchObject
		{
			get { return Factory.Load<GlbBranch>(Branch); }
		}

		public GlbBranchCollection Branches => AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory);

		#endregion

		#region BranchName

		public ZString BranchName
		{
			get { return BranchObject != null ? BranchObject.GB_BranchName : ZString.Empty; }
		}

		public ZPropertyInfo BranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.BranchName); }
		}

		#endregion

		#region TaxBranch

		[List("Branches")]
		public ZGuid TaxBranch
		{
			get
			{
				return fTaxBranch;
			}
			set
			{
				if (fTaxBranch != value)
				{
					SetNonPersistentPropertyValue(TaxBranchInfo, ref fTaxBranch, value);
					UpdateApportionmentsTaxBranch();
				}
			}
		}
		ZGuid fTaxBranch;

		public ZPropertyInfo TaxBranchInfo
		{
			get { return GetZPropertyInfo(Schema.TaxBranch); }
		}

		GlbBranch TaxBranchObject
		{
			get { return Factory.Load<GlbBranch>(TaxBranch); }
		}

		public bool TaxBranch_ReadOnly => true;

		#endregion

		#region TaxBranchName

		public ZString TaxBranchName
		{
			get { return TaxBranchObject != null ? TaxBranchObject.GB_BranchName : ZString.Empty; }
		}

		public ZPropertyInfo TaxBranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.TaxBranchName); }
		}

		#endregion

		#region Amount

		public ZDecimal Amount
		{
			get { return fAmount; }
			set
			{
				if (fAmount != value)
				{
					SetNonPersistentPropertyValue(AmountInfo, ref fAmount, value);
					updateTax();
					updateTotal();
					updateApportionments();
					Invoice.ValidateExpectedInvoiceTotal();
				}
				if (!IsValidationSuspended)
				{
					ValidateAmount();
				}
			}
		}
		ZDecimal fAmount;

		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		public void ValidateAmount()
		{
			AmountInfo.ClearAllNotifications();
			if (Amount == 0.0M)
			{
				AmountInfo.AddError(Res.GetString("5443e096-37d4-4aaa-8ca4-3439e0d33007", "Please enter an Amount."));
			}
			else
			{
				ZDecimal total = 0.0M;
				foreach (IntercompanyCostsApportionment apportionment in Apportionments)
				{
					total += apportionment.ForeignApportionedAmount;
				}
				if (Amount != total)
				{
					AmountInfo.AddError(Res.GetString("e0ef3487-ad65-43f4-926a-4d429db10f30", "Sum of Foreign Amounts on Apportionment Details must equal Charges Line Amount."));
				}
			}
		}

		#endregion

		#region Tax

		public ZDecimal Tax
		{
			get
			{
				return fTax;
			}
			set
			{
				if (fTax != value)
				{
					SetNonPersistentPropertyValue(TaxInfo, ref fTax, value);
					updateGSTAmount();
					updateApportionmentsForeignGST();
					updateApportionmentsLocalGST();
				}
			}
		}
		ZDecimal fTax;

		public ZPropertyInfo TaxInfo
		{
			get { return GetZPropertyInfo(nameof(Tax)); }
		}

		protected bool Tax_ReadOnly
		{
			get
			{
				return TaxRate == null || TaxRate.GetRate(AL_TaxDate) == 0m;
			}
		}

		#endregion

		#region GSTAmount

		public ZDecimal GSTAmount
		{
			get
			{
				return fGSTAmount;
			}
			set
			{
				if (fGSTAmount != value)
				{
					SetNonPersistentPropertyValue(GSTAmountInfo, ref fGSTAmount, value);
					updateTotal();
					updateGSTInclusiveAmount();
				}
			}
		}
		ZDecimal fGSTAmount;

		public ZPropertyInfo GSTAmountInfo
		{
			get { return GetZPropertyInfo(nameof(GSTAmount)); }
		}

		public bool GSTAmount_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region GSTInclusiveAmount

		public ZDecimal GSTInclusiveAmount
		{
			get
			{
				return fGSTInclusiveAmount;
			}
			set
			{
				if (fGSTInclusiveAmount != value)
				{
					SetNonPersistentPropertyValue(GSTInclusiveAmountInfo, ref fGSTInclusiveAmount, value);
					splitGSTInclusiveAmount();
				}
			}
		}
		ZDecimal fGSTInclusiveAmount;

		protected bool GSTInclusiveAmount_ReadOnly
		{
			get { return !Invoice.GSTInclusive; }
		}

		public ZPropertyInfo GSTInclusiveAmountInfo
		{
			get { return GetZPropertyInfo(nameof(GSTInclusiveAmount)); }
		}

		#endregion

		#region Total

		public ZDecimal Total
		{
			get
			{
				return fTotal;
			}
			set
			{
				if (fTotal != value)
				{
					SetNonPersistentPropertyValue(TotalInfo, ref fTotal, value);
				}
			}
		}
		ZDecimal fTotal;

		public ZPropertyInfo TotalInfo
		{
			get { return GetZPropertyInfo(nameof(Total)); }
		}

		protected bool Total_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region ApportionmentMethod

		[List("ApportionmentTemplates")]
		public ZGuid ApportionmentMethod
		{
			get
			{
				return fApportionmentMethod;
			}
			set
			{
				if (fApportionmentMethod != value)
				{
					SetNonPersistentPropertyValue(ApportionmentMethodInfo, ref fApportionmentMethod, value);
					if (!fApportionmentMethod.Equals(ZGuid.Empty))
					{
						using (Apportionments.GetApportionmentsAllowNewAndRemoveSuspender())
						{
							updateApportionments();
						}
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateApportionmentMethod();
					ValidateAL_AG();
				}
			}
		}
		ZGuid fApportionmentMethod;

		public ZPropertyInfo ApportionmentMethodInfo
		{
			get { return GetZPropertyInfo(nameof(ApportionmentMethod)); }
		}

		public void ValidateApportionmentMethod()
		{
			ApportionmentMethodInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(ApportionmentMethodInfo, ApportionmentTemplates, ResString.GetMultilingualString("24fde9c4-d813-44a0-b3ef-3d872a78efb2", "Please enter a valid Apportionment Method."));
			MandatoryValidation.CheckEntered(ApportionmentMethodInfo);

			var template = ApportionmentTemplate;
			var propertyInfo = ApportionmentMethodInfo;
			if (template != null && propertyInfo != null)
			{
				foreach (AccApportionmentTemplateLines lines in template.Lines)
				{
					GlbBranchCombinationValidation.CheckBranchDepartmentCombination(propertyInfo, lines.Branch, lines.Department);
				}
			}
		}

		public AccApportionmentTemplateCollection ApportionmentTemplates
		{
			get
			{
				if (fApportionmentTemplates == null)
				{
					fApportionmentTemplates = new AccApportionmentTemplateCollection(Factory);
				}
				return fApportionmentTemplates;
			}
		}
		AccApportionmentTemplateCollection fApportionmentTemplates;

		public AccApportionmentTemplate ApportionmentTemplate
		{
			get
			{
				return Factory.Load<AccApportionmentTemplate>(ApportionmentMethod);
			}
		}

		#endregion

		#region Apportionments

		public IntercompanyCostsApportionmentCollection Apportionments
		{
			get
			{
				if (fApportionments == null)
				{
					fApportionments = new IntercompanyCostsApportionmentCollection(Factory, this);
					RegisterEditableChildObject(fApportionments);
				}
				return fApportionments;
			}
		}
		IntercompanyCostsApportionmentCollection fApportionments;

		#endregion

		#region IsCurrentCompanyGSTRegistered

		protected bool IsCurrentCompanyGSTRegistered
		{
			get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		#endregion

		#region IsCurrentOrganisationGSTRegistered

		protected bool IsCurrentOrganisationGSTRegistered
		{
			get
			{
				bool result = false;
				if (Invoice != null && Invoice.Header != null && Invoice.Header.MiscServ != null)
				{
					result = Invoice.IsMiscServTaxApplicable;
				}
				return result;
			}
		}

		#endregion

		#region IsGSTMandatory

		public bool IsGSTMandatory
		{
			get { return (IsCurrentOrganisationGSTRegistered && GlbCompany.CurrentCompany.GC_IsGSTRegistered); }
		}

		#endregion

		#region AllowUserGSTOverride

		protected bool AllowUserGSTOverride
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			}
		}

		#endregion

		#region IsCurrentChargeGLAccount

		protected bool IsCurrentChargeGLAccount
		{
			get
			{
				bool result = false;
				if (GenericTransactionCharge != null)
				{
					result = new ZBool(GenericTransactionCharge.VC_IsGLAccount);
				}
				return result;
			}
		}

		#endregion

		#region Department

		[RelatedBusinessObject("Department")]
		[List("Departments")]
		public ZGuid AL_GE
		{
			get { return fAL_GE; }
			set
			{
				if (fAL_GE != value)
				{
					SetNonPersistentPropertyValue(AL_GEInfo, ref fAL_GE, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateDepartment();
				}
			}
		}
		ZGuid fAL_GE;

		public GlbDepartment Department
		{
			get { return Factory.Load<GlbDepartment>(AL_GE); }
		}

		public ZPropertyInfo AL_GEInfo
		{
			get { return GetZPropertyInfo(AccTransactionLinesSchema.Constants.AL_GE); }
		}

		public void ValidateDepartment()
		{
			AL_GEInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(AL_GEInfo, Departments, ResString.GetMultilingualString("052469fc-d233-47ea-af81-e9d77deb6c91", "Please enter a valid Department."));

			if (!IsInDatabase || BranchInfo.HasChanges || AL_GEInfo.HasChanges)
			{
				GlbBranchCombinationValidation.CheckBranchDepartmentCombination(AL_GEInfo, BranchObject, Department);
			}
		}

		public GlbDepartmentCollection Departments
		{
			get { return new GlbDepartmentCollection(Factory); }
		}

		#endregion

		#region DepartmentDescription

		public ZString DepartmentDescription
		{
			get { return Department != null ? Department.GE_Desc : ZString.Empty; }
		}

		public ZPropertyInfo DepartmentDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentDescription); }
		}

		#endregion

		#region LocalTax

		public ZDecimal LocalTax
		{
			get
			{
				return TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, LocalAmount, TaxRate, TaxRate?.GetRate(AL_TaxDate), TaxRate?.GetEffectiveExtraRate(AL_TaxDate), Tax, Invoice.ExchangeRate.Rate);
			}
		}

		public ZPropertyInfo LocalTaxInfo
		{
			get { return GetZPropertyInfo(nameof(LocalTax)); }
		}

		#endregion

		#region LocalAmount

		public ZDecimal LocalAmount
		{
			get
			{
				return Env.CurrentCompany.ExchangeRate.ForeignToLocal(Amount, Invoice.ExchangeRate.Rate);
			}
		}

		public ZPropertyInfo LocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmount)); }
		}

		#endregion

		#region Local Total

		public ZDecimal LocalTotal
		{
			get
			{
				return LocalAmount + LocalTax;
			}
		}

		public ZPropertyInfo LocalTotalInfo
		{
			get { return GetZPropertyInfo(nameof(LocalTotal)); }
		}

		#endregion

		#region IsApportionmentMethodEmpty

		public bool IsApportionmentMethodEmpty
		{
			get { return ApportionmentMethod == ZGuid.Empty; }
		}

		#endregion

		#region AL_SupplyType

		[List("SupplyTypes")]
		public ZString AL_SupplyType
		{
			get { return fAL_SupplyType; }
			set
			{
				if (fAL_SupplyType != value)
				{
					SetNonPersistentPropertyValue(AL_SupplyTypeInfo, ref fAL_SupplyType, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateAL_SupplyType();
				}
			}
		}
		ZString fAL_SupplyType;

		public ZPropertyInfo AL_SupplyTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AL_SupplyType)); }
		}

		public void ValidateAL_SupplyType()
		{
			AL_SupplyTypeInfo.ClearAllNotifications();

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				if (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.Value)
				{
					MandatoryValidation.CheckEntered(AL_SupplyTypeInfo);
				}
				else if (AL_SupplyType.IsEmpty)
				{
					AL_SupplyTypeInfo.AddWarning(Res.GetString("96D98C93-AC46-4671-B3E1-4B6A2441A507", "The Supply Type is not specified. Please check if a supply type is needed before posting."));
				}

				ListValidation.ErrorIfInvalidCode(AL_SupplyTypeInfo, SupplyTypes);
			}
		}

		public ReadOnlyCodeDescriptionPairList SupplyTypes => AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.GetValueWithoutFallback(BranchObject?.Company.PK.ToGuid() ?? Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();

		#endregion

		#endregion

		#region IGenericChargeCollectionRequired Members

		bool IGenericChargeCollectionRequired.IsJobRelated
		{
			get { return false; }
		}

		GlbDepartment IGenericChargeCollectionRequired.Department
		{
			get { return Department; }
		}

		BusinessObjectFactory IGenericChargeCollectionRequired.Factory
		{
			get { return Factory; }
		}

		#endregion

		#region Implementation

		public class ApportionmentAdjustmentSuspender : IDisposable
		{
			public ApportionmentAdjustmentSuspender(IntercompanyCostsApportionmentInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				invoiceLine.fAreApportionmentsBeingUpdated = true;
			}

			readonly IntercompanyCostsApportionmentInvoiceLine invoiceLine;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				invoiceLine.fAreApportionmentsBeingUpdated = false;
				if (invoiceLine.Apportionments.Count > 0)
				{
					invoiceLine.Apportionments[0].AdjustForeignApportionedAmounts();
					invoiceLine.Apportionments[0].AdjustForeignGST();
					invoiceLine.Apportionments[0].AdjustLocalAmounts();
					invoiceLine.Apportionments[0].AdjustLocalGST();
				}
			}

			#endregion
		}

		public ApportionmentAdjustmentSuspender GetApportionmentAdjustmentSuspender()
		{
			return new ApportionmentAdjustmentSuspender(this);
		}

		bool fAreApportionmentsBeingUpdated;
		public bool AreApportionmentsBeingUpdated
		{
			get
			{
				return fAreApportionmentsBeingUpdated;
			}
		}

		public void updateApportionments()
		{
			if (!ApportionmentMethod.Equals(ZGuid.Empty))
			{
				if (ApportionmentTemplate != null)
				{
					using (GetApportionmentAdjustmentSuspender())
					{
						Apportionments.RemoveAll();
						foreach (AccApportionmentTemplateLines line in ApportionmentTemplate.Lines)
						{
							IntercompanyCostsApportionment apportionment = new IntercompanyCostsApportionment(Factory, this);
							Apportionments.Add(apportionment);
							apportionment.Company = line.Company;
							apportionment.Branch = line.Y0_GB;
							apportionment.Department = line.Y0_GE;
							apportionment.ApportionmentFactor = line.Y0_Percentage;
							using (apportionment.GetAmountsSuspender())
							{
								apportionment.ForeignApportionedAmount = Amount * line.Y0_Percentage / 100.0M;
							}
							apportionment.ExchangeRate = Invoice.ExchangeRate.Rate;
							apportionment.TemplateLineDescription = line.Y0_Description;
							apportionment.Description = Description + " " + apportionment.TemplateLineDescription;
							apportionment.TaxBranch = TaxBranch;
						}
					}
					ValidateAmount();
				}
			}
		}

		void UpdateApportionmentsTaxBranch()
		{
			if (!ApportionmentMethod.Equals(ZGuid.Empty))
			{
				foreach (IntercompanyCostsApportionment apportionment in Apportionments)
				{
					apportionment.TaxBranch = TaxBranch;
				}
			}
		}

		void updateApportionmentsDescription()
		{
			if (!ApportionmentMethod.Equals(ZGuid.Empty))
			{
				foreach (IntercompanyCostsApportionment apportionment in Apportionments)
				{
					apportionment.Description = Description + " " + apportionment.TemplateLineDescription;
				}
			}
		}

		void updateApportionmentsLocalGST()
		{
			foreach (IntercompanyCostsApportionment apportionment in Apportionments)
			{
				try
				{
					apportionment.IsAdjustingLocalGSTSuspended = true;
					apportionment.updateLocalGST();
				}
				finally
				{
					apportionment.IsAdjustingLocalGSTSuspended = false;
				}
			}
			if (Apportionments.Count > 0)
			{
				Apportionments[0].AdjustLocalGST();
			}
		}

		void updateApportionmentsForeignGST()
		{
			foreach (IntercompanyCostsApportionment apportionment in Apportionments)
			{
				try
				{
					apportionment.IsAdjustingForeignGSTSuspended = true;
					apportionment.updateForeignGST();
				}
				finally
				{
					apportionment.IsAdjustingForeignGSTSuspended = false;
				}
			}
			if (Apportionments.Count > 0)
			{
				Apportionments[0].AdjustForeignGST();
			}
		}

		public void updateApportionmentsExchangeRate()
		{
			foreach (IntercompanyCostsApportionment apportionment in Apportionments)
			{
				try
				{
					apportionment.IsAdjustingLocalAmountSuspended = true;
					apportionment.IsAdjustingLocalGSTSuspended = true;
					apportionment.ExchangeRate = Invoice.ExchangeRate.Rate;
				}
				finally
				{
					apportionment.IsAdjustingLocalAmountSuspended = false;
					apportionment.IsAdjustingLocalGSTSuspended = false;
				}
			}
			if (Apportionments.Count > 0)
			{
				Apportionments[0].AdjustLocalAmounts();
				Apportionments[0].AdjustLocalGST();
			}
		}
		
		void updateTaxMessage()
		{
			if (AL_AT == ZGuid.Empty)
			{
				AL_A9_VATClass = ZGuid.Empty;
			}
			else
			{
				ZGuid overrideInvTaxMsg = ZGuid.Empty;
				AccTaxRate overrideTaxRate = GetFallbackTaxRate(out overrideInvTaxMsg);
				if (overrideTaxRate != null && overrideTaxRate.PK == AL_AT && overrideInvTaxMsg != ZGuid.Empty)
				{
					AL_A9_VATClass = overrideInvTaxMsg;
				}
				else
				{
					UpdateAL_A9_VatClassFromTaxRate();
				}
			}
		}

		void UpdateAL_A9_VatClassFromTaxRate()
		{
			AL_A9_VATClass = (TaxRate != null) ? TaxRate.AT_A9_DefaultVatClass : ZGuid.Empty;
		}

		internal void updateTax()
		{
			if (UpdateTaxSuspender.IsSuspended)
			{
				return;
			}

			ZDecimal result = 0m;
			if (TaxRate != null && Invoice.CurrencyObject != null)
			{
				result = Utilities.Round(Amount * (TaxRate.GetRate(AL_TaxDate) + TaxRate.GetEffectiveExtraRate(AL_TaxDate)) / 100, Invoice.CurrencyObject.Decimals);
			}
			Tax = result;
		}

		FunctionalitySuspender UpdateTaxSuspender => updateTaxSuspender ?? (updateTaxSuspender = new FunctionalitySuspender());
		FunctionalitySuspender updateTaxSuspender;

		protected ZDecimal RoundAmountToCurrencyDecimals(ZDecimal value)
		{
			return Utilities.Round(value, (Invoice.CurrencyObject != null ? Invoice.CurrencyObject.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals));
		}

		void updateGSTAmount()
		{
			GSTAmount = Tax;
		}

		internal void updateTotal()
		{
			Total = Amount + GSTAmount;
		}

		void updateGSTInclusiveAmount()
		{
			fGSTInclusiveAmount = Amount + GSTAmount;
			GSTInclusiveAmountInfo.RefreshBinding();
		}

		void splitGSTInclusiveAmount()
		{
			if (AL_AT.IsValid)
			{
				fAmount = GSTInclusiveAmount / (1.0M + (TaxRate.GetRate(AL_TaxDate) / 100M));
				Tax = GSTInclusiveAmount - fAmount;
				AmountInfo.RefreshBinding();
				updateTotal();
				updateApportionments();
				Invoice.ValidateExpectedInvoiceTotal();
			}
		}

		public void CalculateGST(ZBool isGSTRequired)
		{
			ZGuid result = ZGuid.Empty;
			if (isGSTRequired)
			{
				if (GenericTransactionCharge != null)
				{
					ZGuid overrideInvTaxMsg = ZGuid.Empty;
					AccTaxRate rate = GetFallbackTaxRate(out overrideInvTaxMsg);
					if (!GenericTransactionCharge.VC_IsGLAccount && rate != null)
					{
						result = rate.PK;
					}
					else
					{
						result = AL_AT;
					}
					AL_AT = result;
					AL_A9_VATClass = overrideInvTaxMsg;
				}
			}
			else
			{
				AL_AT = ZGuid.Empty;
				AL_A9_VATClass = ZGuid.Empty;
			}
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Branch = GlbBranch.CurrentBranch.PK;
			AL_GE = GlbDepartment.CurrentDepartment.PK;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateGenericCharge();
			ValidateApportionmentMethod();
			ValidateBranch();
			ValidateDepartment();
			ValidateAmount();
			ValidateAL_TaxDate();
			ValidateAL_SupplyType();
		}

		#endregion

		public IntercompanyCostsApportionmentInvoice Invoice;

		readonly InvoicingLineTaxDateCacheProvider InvoicingLineTaxDateCacheProvider;
	}
}
