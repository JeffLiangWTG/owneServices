using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ProvideMetaDataProperty("PropertiesReadOnlyState", MetaDataTypes.ReadOnly)]
	public abstract partial class TransactionHeader : AccTransactionHeader,
		ITransactionHeader,
		IDocumentSupportable,
		IReversing,
		ITemplateCopyable,
		IeNettTransaction,
		ITransactionForApproval,
		IDataExportBatchSource,
		IHandleDeleteError,
		IImportExport,
		IAccountingNumberFountainDataSource,
		IComplianceNumberResetStatusInputData
	{
		public new abstract class Schema : AccTransactionHeader.Schema
		{
			public const string AH_LocalExTaxAmount = "AH_LocalExTaxAmount";
			public const string AH_LocalTaxAmount = "AH_LocalTaxAmount";
			public const string AH_LocalWHTAmount = "AH_LocalWHTAmount";
			public const string AH_LocalTotalAmount = "AH_LocalTotalAmount";
			public const string AH_LocalOutstandingAmount = "AH_LocalOutstandingAmount";

			public const string AH_OSExTaxAmount = "AH_OSExTaxAmount";
			public const string AH_OSTaxAmount = "AH_OSTaxAmount";
			public const string AH_OSTax = "AH_OSTax";
			public const string AH_OSWHTAmount = "AH_OSWHTAmount";
			public const string AH_OSTotalAmount = "AH_OSTotalAmount";
			public const string AH_Calc_OSOutstandingAmount = "AH_Calc_OSOutstandingAmount";
			public const string AH_OSOutstandingAmountWithoutMultiplier = "AH_OSOutstandingAmountWithoutMultiplier";
			public const string OSOutstandingAmountMatching = "OSOutstandingAmountMatching";

			public const string AH_Calc_LocalRXCode = "AH_Calc_LocalRXCode";
			public const string AH_Calc_RXDecimals = "AH_Calc_RXDecimals";
			public const string AH_Calc_LocalRXDecimals = "AH_Calc_LocalRXDecimals";

			public const string AH_NotionalWHTTax = "AH_NotionalWHTTax";
			public const string AH_RealizedWHTTax = "AH_RealizedWHTTax";

			public const string DepositBatchNumber = "DepositBatchNumber";
			public const string DirectDebitNumber = "DirectDebitNumber";

			public const string PostPeriod = "PostPeriod";
			public const string AgePeriod = "AgePeriod";
			public const string DaysFromInvoiceDateToFullyPaidDate = "DaysFromInvoiceDateToFullyPaidDate";
			public const string DaysFromDueDateToFullyPaidDate = "DaysFromDueDateToFullyPaidDate";
			public const string TransactionCategory = "TransactionCategory";

			public const string AH_IsGSTCashBasis_ReadOnly = "AH_IsGSTCashBasis_ReadOnly";
			public const string AH_GC_IsGSTRegistered_ReadOnly = "AH_GC_IsGSTRegistered_ReadOnly";

			public const string UnmatchDate = "UnmatchDate";

			public const string DisplayInvoiceAddressOverride = "DisplayInvoiceAddressOverride";
			public const string DisplayInvoiceContactOverride = "DisplayInvoiceContactOverride";

			public const string ReceivingOperator = "ReceivingOperator";
			public const string ReceivingBranch = "ReceivingBranch";
			public const string ReceivingDepartment = "ReceivingDepartment";

			public const string RelatedClaimStatus = "RelatedClaimStatus";
			public const string QueryNumber = "QueryNumber";

			public const string EInvoicingStatus = "EInvoicingStatus";
			public const string EInvoicingError = "EInvoicingError";
			public const string EInvoicingLastResponseReceivedUtc = "EInvoicingLastResponseReceivedUtc";
			public const string EInvoicingLastSentTimeUtc = "EInvoicingLastSentTimeUtc";
			public const string EInvoicingBatchNumber = "EInvoicingBatchNumber";
			public const string EInvoicingGovernmentAllocatedNumber = "EInvoicingGovernmentAllocatedNumber";
			public const string EInvoicingeHubAllocatedNumber = "EInvoicingeHubAllocatedNumber";
			public const string EInvoicingAuthorisationDateTime = nameof(EInvoicingAuthorisationDateTime);
			public const string EInvoicingAuthorisationNumber = "EInvoicingAuthorisationNumber";

			public const string SupportingDocumentNumber = "SupportingDocumentNumber";

			public const string SourceReference = "SourceReference";
			public const string AuthorizationNumberReference = "AuthorizationNumberReference";

			public const string AmendStatusCodeAndDescription = "AmendStatusCodeAndDescription";
			public const string ComplianceDocumentStatus = "ComplianceDocumentStatus";

			public const string PostingGroup = "PostingGroup";
			public const string RelatedDisbursementTransactions = "RelatedDisbursementTransactions";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "country specific menu name")]
		public static class GovernmentInvoiceMenuNames
		{
			public const string Vietnam_TXI = "VN Govt Tax Invoice";
			public const string Indonesia_TXI = "ARInvoice ID FakturPajak";
			public const string China_TXI = "Class A Invoice Preprinted";
			public const string Peru_TXI = "ARInvoice PE Factura";
			public const string Peru_TCR = "ARInvoice_PE_Nota_De_Credito";
			public const string Peru_TCD = "ARInvoice_PE_Nota_De_Debito";
		}

		public static readonly TransactionHeaderTypeDecider TypeDecider = new TransactionHeaderTypeDecider();

		public TransactionHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_PostToGL), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_TransactionNum), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_InvoicePaymentReferenceCode), ConcurrencyPolicy.Strict);

			if (OSOutstandingAmountValueChangeMonitor == null)
			{
				OSOutstandingAmountValueChangeMonitor = new OSOutstandingAmountValueChangeMonitor(this);
			}

			MatchingMonitor = new TransactionHeaderMatchingMonitorFactory().CreateMatchingMonitor(this);
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AH_TransactionNum.IsEmpty)
			{
				AH_TransactionNum = TestObjectCreator.GetRandomString(15);
			}
			if (AH_TransactionType.IsEmpty)
			{
				AH_TransactionType = TransactionType;
			}
			if (AH_Ledger.IsEmpty)
			{
				AH_Ledger = Ledger;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region Can Apply Data Refresh

		protected override ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			var additionalPropertiesWithStrictConcurrency = new ZPropertyInfo[] { AH_TransactionNumInfo, AH_InvoicePaymentReferenceCodeInfo };
			return base.GetPropertiesWithStrictConcurrency().Concat(additionalPropertiesWithStrictConcurrency).ToArray();
		}

		#endregion

		protected UnmatchingResult CanUnmatchMatchLink(ZDecimal matchLinkAmount)
		{
			var totalAmount = AH_LocalTotal;
			if ((totalAmount > 0 && matchLinkAmount < 0) || (totalAmount < 0 && matchLinkAmount > 0))
			{
				return UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns;
			}
			else if (Math.Abs(AH_OutstandingAmount + matchLinkAmount) > Math.Abs(totalAmount))
			{   // check if adding match amount exceeds original invoice amount
				return UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount;
			}
			else if (Math.Abs(AH_OutstandingAmount + matchLinkAmount) < Math.Abs(AH_OutstandingAmount))
			{   // check if adding match amount decreases absolute value of outstanding amount
				return UnmatchingResult.DataErrorAddingMatchAmountDecreaseAbsValueOfOutstandingAmount;
			}
			else
			{
				return UnmatchingResult.Success;
			}
		}

		public bool IsInMatchingContext
		{
			get { return ParentMatchingCollection != null; }
		}

		public bool IsAllowedToSetExchangeRate => AH_PostedToEFT || AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		protected override Type JobTypeForDeterminingJobNumber
		{
			get { return typeof(Job); }
		}

		IMatchingCollection ParentMatchingCollection
		{
			get
			{
				IMatchingCollection result = null;

				if (this is IMatching)
				{
					if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
					{
						foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
						{
							if (collection is IMatchingCollection)
							{
								result = (IMatchingCollection)collection;
								break;
							}
						}
					}
				}

				return result;
			}
		}

		public string EnterpriseInvoiceMenuName
		{
			get { return AccountingConfigurationRegistry.Instance.GetARInvoiceMenuItemName(Factory); }
		}

		public ZGuid GovernmentInvoiceMenuPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (Branch.Country.SupportComplianceSubType)
				{
					if (Branch.Country.Code == Core.Constants.CountryCodes.China)
					{
						result = AccountingConfigurationRegistry.Instance.ARLocalInvoiceMenuItem.Value;
					}
					else
					{
						AccComplianceSequence sequence = ComplianceSequenceFromTransactionReference;
						if (sequence != null && !sequence.XD_SU_MenuItem.IsEmpty)
						{
							result = sequence.XD_SU_MenuItem;
						}
					}
				}
				return result;
			}
		}

		public string GovernmentInvoiceMenuName
		{
			get
			{
				if (Branch.Country.SupportComplianceSubType)
				{
					if (Branch.Country.Code == Core.Constants.CountryCodes.China)
					{
						return AccountingConfigurationRegistry.Instance.GetARLocalInvoiceMenuItemName(Factory);
					}
					else
					{
						AccComplianceSequence sequence = ComplianceSequenceFromTransactionReference;
						if (sequence != null)
						{
							if (!sequence.XD_SU_MenuItem.IsEmpty)
							{
								StmMenuItem menu = Factory.Load<StmMenuItem>(sequence.XD_SU_MenuItem);
								if (menu != null)
								{
									return menu.SU_MenuName;
								}
								else
								{
									throw new GovernmentInvoiceMenuNotFoundException(AH_ComplianceSubType);
								}
							}
							else
							{
								throw new ComplianceSequenceHasNoDocumentMenuDefinedException();
							}
						}
						else
						{
							switch (Branch.Country.Code)
							{
								case Core.Constants.CountryCodes.VietNam:
									switch (AH_ComplianceSubType)
									{
										case VietnamComplianceInfo.ComplianceSubTypeCodes.TXI:
											return GovernmentInvoiceMenuNames.Vietnam_TXI;
										default:    // shouldn't happen as VN only has TXI
											throw new DefaultGovernmentInvoiceMenuNotFoundException(AH_ComplianceSubType, Core.Constants.CountryCodes.VietNam);
									}
								case Core.Constants.CountryCodes.Indonesia:
									switch (AH_ComplianceSubType)
									{
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T01:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T02:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T03:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T04:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T06:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T07:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T08:
										case IndonesiaComplianceInfo.ComplianceSubTypeCodes.T09:
											return GovernmentInvoiceMenuNames.Indonesia_TXI;
										default:    // shouldn't happen as ID only has TXI and sub type of TXI
											throw new DefaultGovernmentInvoiceMenuNotFoundException(AH_ComplianceSubType, Core.Constants.CountryCodes.Indonesia);
									}
								case Core.Constants.CountryCodes.Peru:
									switch (AH_ComplianceSubType)
									{
										case PeruComplianceInfo.ComplianceSubTypeCodes.TXI:
											return GovernmentInvoiceMenuNames.Peru_TXI;
										case PeruComplianceInfo.ComplianceSubTypeCodes.TCR:
											return GovernmentInvoiceMenuNames.Peru_TCR;
										case PeruComplianceInfo.ComplianceSubTypeCodes.TCD:
											return GovernmentInvoiceMenuNames.Peru_TCD;
										default:    // possible if user changes the subtype manually to TBO/DSB etc.
											throw new DefaultGovernmentInvoiceMenuNotFoundException(AH_ComplianceSubType, Core.Constants.CountryCodes.Peru);
									}
								default:
									return AccountingConfigurationRegistry.Instance.GetARLocalInvoiceMenuItemName(Factory);
							}
						}
					}
				}
				else
				{
					return AccountingConfigurationRegistry.Instance.GetARLocalInvoiceMenuItemName(Factory);
				}
			}
		}

		public virtual GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		protected ZGuid PaymentApprovalPKCurrentlyBeingMatched
		{
			get { return PaymentApprovalCurrentlyBeingMatched != null ? PaymentApprovalCurrentlyBeingMatched.PK : ZGuid.Empty; }
		}

		public PaymentApprovalBase PaymentApprovalCurrentlyBeingMatched
		{
			get
			{
				PaymentApprovalBase result = null;
				if (ParentMatchingCollection != null)
				{
					result = (PaymentApprovalBase)ParentMatchingCollection.FirstOrDefault(x => x is PaymentApprovalBase);
				}
				return result;
			}
		}

		public Payment PaymentCurrentlyBeingMatched
		{
			get
			{
				Payment result = null;
				if (ParentMatchingCollection != null)
				{
					result = (Payment)ParentMatchingCollection.FirstOrDefault(x => x is Payment);
				}
				return result;
			}
		}

		public PaymentApprovalItem GetPaymentApprovalItem(PaymentApprovalBase approval)
		{
			ZQuery findExistingPaymentApprovalItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, approval.PK);
			findExistingPaymentApprovalItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, PK);

			PaymentApprovalItem approvalItem = Factory.LoadTop1<PaymentApprovalItem>(findExistingPaymentApprovalItemQuery);

			if (approvalItem == null)
			{
				approvalItem = Factory.New<PaymentApprovalItem>();
				approvalItem.A2_AV = approval.PK;
				approvalItem.A2_AH = PK;
			}

			return approvalItem;
		}

		[List("Lookups.Headers")]
		public ZGuid IntercompanyOrgProxy
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (Branch != null && Branch.GB_GC != GlbCompany.CurrentCompany.PK)
				{
					result = Branch.OrgProxy != null ? Branch.GB_OH_OrgProxy : Branch.Company.GC_OH_OrgProxy;
				}
				else
				{
					result = AH_OH;
				}
				return result;
			}
		}

		public ZPropertyInfo IntercompanyOrgProxyInfo
		{
			get { return GetZPropertyInfo(nameof(IntercompanyOrgProxy)); }
		}

		protected bool IsInInvoiceBatchContext
		{
			get { return (this is InvoicingBase) && IsInCollectionOfType<InvoiceBatchLineCollection>(); }
		}

		protected bool IsInUnapprovedTransactionContext
		{
			get { return (this is InvoicingBase) && IsInCollectionOfType<UnapprovedTransactionCandidateCollection>(); }
		}

		protected bool IsInPeriodicInvoiceContext
		{
			get { return (this is InvoicingBase) && IsInCollectionOfType<PeriodicInvoiceMiscInvoiceCollection>(); }
		}

		internal bool IsInDirectDebitBatchLineContext
		{
			get { return (this is IDirectDebitBatchTransaction) && IsInCollectionOfType<DirectDebitBatchLineCollection>(); }
		}

		bool IsInCompletingInvoiceContext
		{
			get
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return (invoiceBase != null && invoiceBase.IsCompletingInvoice);
			}
		}

		public bool IsInRequisitionContext
		{
			get
			{
				return (this is APInvoice) && isInRequisitionContext;
			}
			set
			{
				if (isInRequisitionContext != value)
				{
					isInRequisitionContext = value;
				}
			}
		}
		bool isInRequisitionContext;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TransactionHeaderFetchStrategy(this);
		}

		#region Critical Validation

		protected override AccTransactionHeaderCriticalValidation GetCriticalValidation()
		{
			return new TransactionHeaderCriticalValidation(this);
		}

		#endregion

		#region UniqueIndexFailureHandler - NumberFountainForTransactionNum

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				var handlers = new List<IUniqueIndexFailureHandler>();
				if (ShouldAttemptToFixNumberFountain)
				{
					if (fUniqueIndexFailureHandler == null)
					{
						fUniqueIndexFailureHandler = GetNewUniqueIndexFailureHandler();
					}

					handlers.Add(fUniqueIndexFailureHandler);
				}
				else
				{
					handlers.AddRange(base.UniqueIndexFailureHandlers);
				}

				return handlers;
			}
		}
		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected virtual TransactionHeaderNumberFountainUniqueIndexFailureHandler GetNewUniqueIndexFailureHandler()
		{
			return new TransactionHeaderNumberFountainUniqueIndexFailureHandler(this);
		}

		bool ShouldAttemptToFixNumberFountain
		{
			get
			{
				bool result = true;

				if (AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					result = !(
							AH_TransactionType == ZArchitecture.Core.TransactionTypes.UAInvoice ||
							AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice ||
							AH_TransactionType == ZArchitecture.Core.TransactionTypes.UACreditNote ||
							AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
							AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote
						 );
				}

				return result;
			}
		}

		protected class TransactionHeaderNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public TransactionHeaderNumberFountainUniqueIndexFailureHandler(TransactionHeader header)
				: base("NR_UX__AH_GB_AH_Ledger_AH_TransactionType_AH_TransactionNum_AH_TransactionCount_AH_OH", header)
			{
				this.Header = header;
			}

			protected sealed override INumberFountainProxy NumberFountainToFix
			{
				get { return AccountingNumberFountainToFix.GetNumberFountain(Header.AH_PostDate); }
			}

			protected virtual AccountingNumberFountainWrapper AccountingNumberFountainToFix
			{
				get { return Header.NumberFountainForTransactionNumber; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				DbCommand result = connection.Command(FindMaxValueSqlString);
				result.AddParameter("@Company", SqlDbType.UniqueIdentifier, Header.Branch.Company.PK.ToGuid());
				result.AddParameter("@Ledger", SqlDbType.Char, Header.Ledger.ToString());
				result.AddParameter("@TransactionType", SqlDbType.Char, Header.TransactionType.ToString());
				return result;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL string")]
			string FindMaxValueSqlString
			{
				get
				{
					ZStringBuilder stringBuilder = new ZStringBuilder();

					string transactionNumExpression;
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
					{
						transactionNumExpression = "RIGHT(" + AccTransactionHeaderSchema.Constants.AH_TransactionNum + ", 6)";
					}
					else
					{
						transactionNumExpression = AccTransactionHeaderSchema.Constants.AH_TransactionNum;
					}

					stringBuilder.Append("SELECT MAX(" + transactionNumExpression + ") ");
					stringBuilder.Append("FROM " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + " ");

					stringBuilder.Append("INNER JOIN " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + " ");
					stringBuilder.Append("ON " + GlbBranchSchema.Constants.PK + " = " + AccTransactionHeaderSchema.Constants.AH_GB + " ");

					stringBuilder.Append("INNER JOIN " + GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName + " ");
					stringBuilder.Append("ON " + GlbCompanySchema.Constants.PK + " = " + GlbBranchSchema.Constants.GB_GC + " ");

					stringBuilder.Append("WHERE " + GlbCompanySchema.Constants.PK + " = @Company ");
					stringBuilder.Append("AND " + AccTransactionHeaderSchema.Constants.AH_Ledger + " = @Ledger ");
					stringBuilder.Append("AND " + AccTransactionHeaderSchema.Constants.AH_TransactionType + " = @TransactionType ");

					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
					{
						stringBuilder.Append("AND " + AccTransactionHeaderSchema.Constants.AH_TransactionNum + " LIKE @TransactionNumPrefix ");
					}

					return stringBuilder.ToStringWithNewLineBetweenAppends();
				}
			}

			readonly TransactionHeader Header;
		}

		#endregion

		#region ZValidation object

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		protected sealed override AccTransactionHeaderValidation GetNewValidation()
		{
			if (IsValidationSuspended) //keep this IF the first always to avoid any any db hits and calculations for a case when validation will not be used.
			{
				return GetNewEmptyValidation();//Return empty validation as just some not null value as actual validation calls will be skipped anyway.
			}
			else if (this.HasContext(BusinessContext.OverrideGovernmentAllocatedID) && this is InvoicingBase)
			{
				return new OverrideGovernmentAllocatedIDValidation((InvoicingBase)this);
			}
			else if (this.HasContext(BusinessContext.OverrideTransactionDescription) && this is InvoicingBase)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new OverrideTransactionDescriptionValidation(invoiceBase);
			}
			else if (this.HasContext(BusinessContext.OverrideInvoiceAddressContact) && this is InvoicingBase)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new OverrideInvoiceAddressContactValidation(invoiceBase);
			}
			else if (this.HasContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory) && this is ReceiptPaymentBase)
			{
				ReceiptPaymentBase invoiceBase = this as ReceiptPaymentBase;
				return new OverrideReceiptPaymentCashFlowCategoryValidation(invoiceBase);
			}
			else if (this.HasContext(BusinessContext.OverrideTransactionAgreedPaymentMethod) && this is InvoicingBase)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new OverrideTransactionAgreedPaymentMethodValidation(invoiceBase);
			}
			else if (this.HasContext(BusinessContext.OverrideInvoiceRemittanceType) && this is InvoicingBase)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new OverrideInvoiceRemittanceTypeValidation(invoiceBase);
			}
			else if (this.HasContext(BusinessContext.OverrideInvoiceReference) && this is InvoicingBase)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new OverrideInvoiceReferenceValidation(invoiceBase);
			}
			else if (this.HasContext(BusinessContext.OverrideMatchStatus) && this is InvoicingBase)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new OverrideMatchStatusValidation(invoiceBase);
			}
			else if (IsInPreviewingInvoicesContext)
			{
				return GetNewEmptyValidation();
			}
			else if (IsInMatchingContext)
			{
				if (ParentMatchingCollection.MatchingCollectionType == MatchingCollectionTypes.MatchedTransactions)
				{
					//MatchingService.GetInstance(Factory).IsInMatchingContext;
					return GetNewMatchingValidation();
				}
				else
				{
					return GetNewEmptyValidation();
				}
			}
			else if (IsInInvoiceBatchContext)
			{
				InvoicingBase invoiceBase = this as InvoicingBase;
				return new InvoiceBatchLineValidation(invoiceBase);
			}
			else if (IsInPeriodicInvoiceContext)
			{
				return new PeriodicInvoiceItemValidation(this);
			}
			else if (IsReverseTransaction)
			{
				if (this is InvoicingBase invoicingBase)
				{
					return GetNewInvoicingBaseReversalValidation(invoicingBase);
				}
				else
				{
					return GetNewReversalValidation();
				}
			}
			else if (IsInUnapprovedTransactionContext)
			{
				if (this is APInvoice)
				{
					return new UnapprovedInvoiceCandidateValidation((APInvoice)this);
				}
				else if (this is APCreditNote)
				{
					return new UnapprovedCreditNoteCandidateValidation((APCreditNote)this);
				}
				else
				{
					return GetNewEmptyValidation();
				}
			}
			else if (IsInRequisitionContext)
			{
				APInvoice invoice = this as APInvoice;
				return new RequisitionValidation(invoice);
			}
			else if (IsInCompletingInvoiceContext)
			{
				return GetNewValidationCore();
			}
			else if (ShouldValidateDirectDebitBatchComponent)
			{
				return GetNewValidationCore();
			}
			else if (IsInDatabase && IsTransactionInDatabaseReadOnly
				|| (IsInDatabase && (this is GLJournal) && !AH_PostDateInfo.HasChanges && PeriodCalculator.IsPeriodGLClosed(PostPeriod)))
			{
				return GetNewEmptyValidation();
			}
			else
			{
				return GetNewValidationCore();
			}
		}

		bool ShouldValidateDirectDebitBatchComponent
		{
			get
			{
				var directDebitBatchComponent = this as IDirectDebitBatchComponent;
				return directDebitBatchComponent != null && directDebitBatchComponent.ShouldValidateDirectDebitBatchComponent;
			}
		}

		protected virtual TransactionHeaderValidation GetNewValidationCore()
		{
			return new TransactionHeaderValidation(this);
		}

		protected virtual TransactionHeaderValidation GetNewReversalValidation()
		{
			return new TransactionReversalValidation(this, new DataRefreshBusUpdateActionDecider());
		}

		protected InvoicingBaseReversalValidation GetNewInvoicingBaseReversalValidation(InvoicingBase invoicingBase)
		{
			return new InvoicingBaseReversalValidation(invoicingBase, new DataRefreshBusUpdateActionDecider());
		}

		protected virtual AccTransactionHeaderValidation GetNewMatchingValidation()
		{
			return new MatchingValidation(this);
		}

		protected virtual AccTransactionHeaderValidation GetNewEmptyValidation()
		{
			return new TransactionHeaderEmptyValidation(this);
		}

		public TransactionHeaderValidation HeaderValidation
		{
			get { return Validation as TransactionHeaderValidation; }
		}

		internal AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (periodCalculator == null || periodCalculator.Company.PK != AH_GC)
				{
					periodCalculator = new AccountingPeriodCalculator(Factory, Company);
				}
				return periodCalculator;
			}
		}
		AccountingPeriodCalculator periodCalculator;

		#endregion

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new TransactionHeaderLookups(this);
		}

		public override GlbCompany Company
		{
			get
			{
				return base.Company ?? GlbCompany.CurrentCompany;
			}
		}

		#region Findbox Collections

		#region OperationsJob

		public IJobInvoicingPlugIn OperationsJob
		{
			get
			{
				if (fOperationsJob == null && Job != null)
				{
					fOperationsJob = Job.LoadGenericJob<GenericJob.GenericJob>();
				}
				return fOperationsJob;
			}
		}
		IJobInvoicingPlugIn fOperationsJob;

		#endregion

		#region AH_RX_NKTransactionCurrencyList

		public RefCurrencyCollection AH_RX_NKTransactionCurrencyList
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		#endregion

		#region PaymentMethods

		public virtual CodeDescriptionPairList PaymentMethods
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.PaymentMethod); }
		}

		#endregion

		#region ReceiptMethods

		public CodeDescriptionPairList ReceiptMethods
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.ReceiptMethod); }
		}

		#endregion

		#region PaymentReceiptMethods

		public CodeDescriptionPairList PaymentReceiptMethods
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.PaymentOrReceiptMethod); }
		}

		#endregion

		#region Checkbooks

		public AccChequeBookCollection Checkbooks
		{
			get { return FindboxLookupCollections.GetChequeBookCollection(Factory); }
		}

		#endregion

		#endregion

		#region Calculated Amount Properties

		#region Local Ex Tax Amount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public virtual ZDecimal AH_LocalExTaxAmount
		{
			get { return AH_InvoiceAmount * Multiplier; }
			set
			{
				if (AmountsCalculationsSuspender.IsSuspended)
				{
					AH_InvoiceAmount = value * Multiplier;
				}
				else
				{
					SetLocalExTaxAmountWithExRateRecalculation(value);
				}
			}
		}

		public ZPropertyInfo AH_LocalExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalExTaxAmount); }
		}

		protected virtual bool AH_LocalExTaxAmount_ReadOnly
		{
			get { return ah_LocalExTaxAmount_ReadOnly || AH_RX_NKTransactionCurrency.IsEmpty || AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency || IsInDatabase || IsReverseTransaction; }
			set { ah_LocalExTaxAmount_ReadOnly = value; }
		}
		bool ah_LocalExTaxAmount_ReadOnly;

		#endregion

		#region Local Tax Amount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public virtual ZDecimal AH_LocalTaxAmount
		{
			get { return AH_GSTAmount * Multiplier; }
			set
			{
				RecalculateOSTaxAmounts();
				AH_GSTAmount = value * Multiplier;
				AH_LocalTaxAmountInfo.RefreshBinding();
				TransactionHeaderValidation transactionHeaderValidation = Validation as TransactionHeaderValidation;
				if (transactionHeaderValidation != null)
				{
					transactionHeaderValidation.ValidateAH_LocalTaxAmount();
				}
			}
		}

		public ZPropertyInfo AH_LocalTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalTaxAmount); }
		}

		#endregion

		#region Local Tax Amount Tax Transactions For Display (this is that amount with convenient to user sign we display on invoice screen)

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public virtual ZDecimal AH_LocalTaxAmountOtherTaxes_ForDisplay
		{
			get { return AH_LocalTaxAmountOtherTaxes * Multiplier; }
		}

		public ZPropertyInfo AH_LocalTaxAmountOtherTaxes_ForDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(AH_LocalTaxAmountOtherTaxes_ForDisplay)); }
		}

		#endregion

		#region Local WHT Amount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public virtual ZDecimal AH_LocalWHTAmount
		{
			get { return AH_WithholdingTax * Multiplier; }
			set
			{
				AH_WithholdingTax = value * Multiplier;
				RecalculateOSWHTAmount();
				AH_LocalWHTAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_LocalWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalWHTAmount); }
		}

		#endregion

		#region Local Total Amount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalTotalAmount
		{
			get { return AH_LocalTotal * Multiplier; }
		}

		public ZPropertyInfo AH_LocalTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalTotalAmount); }
		}

		#endregion

		#region Local Outstanding Amount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalOutstandingAmount
		{
			get { return AH_OutstandingAmount * Multiplier; }
			set
			{
				AH_OutstandingAmount = value * Multiplier;
				AH_LocalOutstandingAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_LocalOutstandingAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalOutstandingAmount); }
		}

		#endregion

		#region OS Ex Tax Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public virtual ZDecimal AH_OSExTax
		{
			get
			{
				RecalculateOSTaxAmounts();
				return aH_OSExTaxAmount;
			}
		}

		ZDecimal aH_OSExTaxAmount;
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public virtual ZDecimal AH_OSExTaxAmount
		{
			get
			{
				RecalculateOSTaxAmounts();
				return aH_OSExTaxAmount * Multiplier;
			}
			set
			{
				if (AmountsCalculationsSuspender.IsSuspended)
				{
					aH_OSExTaxAmount = value * Multiplier;
				}
				else
				{
					SetLocalExTaxAmountWithoutExRateRecalculation(RoundAmountToLocalDecimals(value));
					ReCalculateOSAmountDecimalPlaces(value);
					AH_OSExTaxAmountInfo.RefreshBinding();
					if (HeaderValidation != null)
					{
						HeaderValidation.ValidateAH_OSExTaxAmount();
					}
				}
			}
		}

		protected virtual ZDecimal RoundAmountToLocalDecimals(ZDecimal amount)
		{
			return Env.CurrentCompany.ExchangeRate.ForeignToLocal(amount, AH_ExchangeRate);
		}

		protected virtual bool AH_OSExTaxAmount_ReadOnly { get; set; }

		protected void ReCalculateOSAmountDecimalPlaces(ZDecimal value)
		{
			AH_OSTotalAmount = RoundAmountToCurrencyDecimals(value) + RoundAmountToCurrencyDecimals(AH_OSTaxAmount) + AH_OSTaxAmountOtherTaxes_ForDisplay;
			aH_OSExTaxAmount = RoundAmountToCurrencyDecimals(value * Multiplier);
		}

		public virtual ZPropertyInfo AH_OSExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSExTaxAmount); }
		}

		#endregion

		#region OS Tax Amount

		ZDecimal aH_OSTaxAmount;
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public virtual ZDecimal AH_OSTaxAmount
		{
			get
			{
				RecalculateOSTaxAmounts();
				return aH_OSTaxAmount * Multiplier;
			}
			set
			{
				if (AmountsCalculationsSuspender.IsSuspended)
				{
					aH_OSTaxAmount = value * Multiplier;
				}
				else
				{
					AH_LocalTaxAmount = RoundAmountToLocalDecimals(value);
					ReCalculateOsTaxAmountDecimalPlaces(value);
					AH_OSTaxAmountInfo.RefreshBinding();
					TransactionHeaderValidation transactionHeaderValidation = Validation as TransactionHeaderValidation;
					if (transactionHeaderValidation != null)
					{
						transactionHeaderValidation.ValidateAH_OSTaxAmount();
					}
				}
			}
		}

		protected void ReCalculateOsTaxAmountDecimalPlaces(ZDecimal value)
		{
			AH_OSTotalAmount = RoundAmountToCurrencyDecimals(value) + RoundAmountToCurrencyDecimals(AH_OSExTaxAmount) + AH_OSTaxAmountOtherTaxes_ForDisplay;
			aH_OSTaxAmount = RoundAmountToCurrencyDecimals(value * Multiplier);
		}

		public virtual ZPropertyInfo AH_OSTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSTaxAmount); }
		}

		#endregion

		#region OS Tax Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSTax
		{
			get
			{
				RecalculateOSTaxAmounts();
				return AH_OSTaxAmount * Multiplier;
			}
		}

		public ZPropertyInfo AH_OSTaxInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSTax); }
		}

		#endregion

		#region OS Tax Amount Tax Transactions For Display (this is that amount with convenient to user sign we display on invoice screen)

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSTaxAmountOtherTaxes_ForDisplay
		{
			get { return AH_OSTaxAmountOtherTaxes * Multiplier; }
		}

		public ZPropertyInfo AH_OSTaxAmountOtherTaxes_ForDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(AH_OSTaxAmountOtherTaxes_ForDisplay)); }
		}

		public override ZDecimal AH_OSTaxAmountOtherTaxes
		{
			get => base.AH_OSTaxAmountOtherTaxes;
			set
			{
				base.AH_OSTaxAmountOtherTaxes = value;
				AH_OSTotalAmount = RoundAmountToCurrencyDecimals(AH_OSExTaxAmount) + RoundAmountToCurrencyDecimals(AH_OSTaxAmount) + AH_OSTaxAmountOtherTaxes_ForDisplay;
			}
		}

		#endregion

		#region OS WHT Amount

		protected ZDecimal aH_OSWHTAmount;
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSWHTAmount
		{
			get
			{
				RecalculateOSWHTAmount();
				return aH_OSWHTAmount * Multiplier;
			}
			set
			{
				SetNonPersistentPropertyValue(AH_OSWHTAmountInfo, ref aH_OSWHTAmount, value * Multiplier);
			}
		}

		public ZPropertyInfo AH_OSWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSWHTAmount); }
		}

		#endregion

		#region OS Total Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSTotalAmount
		{
			get { return AH_OSTotal * Multiplier; }
			set
			{
				AH_OSTotal = value * Multiplier;
				AH_OSTotalAmountInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo AH_OSTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSTotalAmount); }
		}

		#endregion

		#region OS Outstanding Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_Calc_OSOutstandingAmount
		{
			get { return Multiplier * AH_OSOutstandingAmountWithoutMultiplier; }
		}

		public ZPropertyInfo AH_Calc_OSOutstandingAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_OSOutstandingAmount); }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSOutstandingAmountWithoutMultiplier => TransactionHeaderOSOutstandingAmountProvider.GetAndRefreshOSOutstandingAmount(this);

		public ZPropertyInfo AH_OSOutstandingAmountWithoutMultiplierInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSOutstandingAmountWithoutMultiplier); }
		}

		public OSOutstandingAmountValueChangeMonitor OSOutstandingAmountValueChangeMonitor { get; private set; }
		public TransactionHeaderMatchingMonitor MatchingMonitor { get; private set; }

		internal bool InvoiceUnpaid
		{
			get { return AH_LocalTotalAmount == AH_LocalOutstandingAmount && AH_LocalOutstandingAmount != 0m; }
		}

		#endregion

		#region OS Outstanding Amount for Matching

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal OSOutstandingAmountMatching
		{
			get
			{
				fOSOutstandingAmountMatching = Multiplier * AH_Calc_OSOutstandingAmount;

				if (fOSOutstandingAmountMatching != 0m)
				{
					foreach (PaymentApprovalItem item in ExistingPaymentApprovalItems)
					{
						if (IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(item))
						{
							fOSOutstandingAmountMatching -= item.OSAmountPaidThisRun;
						}
					}
				}

				return fOSOutstandingAmountMatching;
			}
		}

		ZDecimal fOSOutstandingAmountMatching;

		public ZPropertyInfo OSOutstandingAmountMatchingInfo
		{
			get { return GetZPropertyInfo(Schema.OSOutstandingAmountMatching); }
		}

		public PaymentApprovalItemCollection ExistingPaymentApprovalItems
		{
			get
			{
				if (fPaymentApprovalItems == null)
				{
					ZQuery findPaymentApprovalItemsQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AH, PK);
					findPaymentApprovalItemsQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AV, SQLComparisonOperator.NotEqual, PaymentApprovalPKCurrentlyBeingMatched);
					fPaymentApprovalItems = new PaymentApprovalItemCollection(Factory, findPaymentApprovalItemsQuery);
					fPaymentApprovalItems.Load();
				}

				return fPaymentApprovalItems;
			}
		}

		PaymentApprovalItemCollection fPaymentApprovalItems;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal OutstandingAmountMatching
		{
			get
			{
				fOutstandingAmountMatching = AH_OutstandingAmount;

				if (fOutstandingAmountMatching != 0m)
				{
					foreach (PaymentApprovalItem item in ExistingPaymentApprovalItems)
					{
						if (IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(item))
						{
							fOutstandingAmountMatching -= item.A2_PaymentThisRun;
						}
					}
				}

				return fOutstandingAmountMatching;
			}
		}

		ZDecimal fOutstandingAmountMatching;

		public bool IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(PaymentApprovalItem item)
		{
			return PaymentApprovalItemOSAmountProvider.IsPaymentApprovalValidForMatch(item) &&
				item.Approval.PK != PaymentApprovalPKCurrentlyBeingMatched;
		}

		#endregion

		#region Withhold Tax Amounts
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ResourceStringData("fa0c0485-7af4-41bd-932f-50fc7eb73c74", Caption = "Notional WHT")]
		public ZDecimal AH_NotionalWHTTax => AH_NotionalWHTTaxCore;

		public ZPropertyInfo AH_NotionalWHTTaxInfo
		{
			get { return GetZPropertyInfo(Schema.AH_NotionalWHTTax); }
		}

		protected virtual ZDecimal AH_NotionalWHTTaxCore => 0M;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ResourceStringData("8c18e080-652c-4701-b4aa-9dc805d547b4", Caption = "Realized WHT")]
		public ZDecimal AH_RealizedWHTTax => AH_RealizedWHTTaxCore;

		public ZPropertyInfo AH_RealizedWHTTaxInfo
		{
			get { return GetZPropertyInfo(Schema.AH_RealizedWHTTax); }
		}

		protected virtual ZDecimal AH_RealizedWHTTaxCore => 0M;

		#endregion

		#endregion

		#region Other Calculated Properties

		#region AH_Calc_LocalRXCode

		[List("AH_RX_NKTransactionCurrencyList")]
		public ZString AH_Calc_LocalRXCode
		{
			get { return Company.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo AH_Calc_LocalRXCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AH_Calc_LocalRXCode)); }
		}

		#endregion

		#region AH_Calc_LocalRX

		[List("Lookups.TransactionCurrencies")]
		public ZGuid AH_Calc_LocalRX
		{
			get { return Company.LocalCurrency.PK; }
		}

		public ZPropertyInfo AH_Calc_LocalRXInfo
		{
			get { return GetZPropertyInfo(nameof(AH_Calc_LocalRX)); }
		}
		#endregion

		#region AH_Calc_RXDecimals

		public ZInt AH_Calc_RXDecimals
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, AH_RX_NKTransactionCurrency);
				return currency != null ? currency.Decimals : 2;
			}
		}

		public ZPropertyInfo AH_Calc_RXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(AH_Calc_RXDecimals)); }
		}

		#endregion

		#region AH_Calc_LocalRXDecimals

		public ZInt AH_Calc_LocalRXDecimals
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, AH_Calc_LocalRXCode);
				return currency != null ? currency.Decimals : 2;
			}
		}

		public ZPropertyInfo AH_Calc_LocalRXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(AH_Calc_LocalRXDecimals)); }
		}

		#endregion

		#region AH_Readonly_RXCode

		[List("Lookups.TransactionCurrencies")]
		public virtual ZString AH_Readonly_RXCode
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		public ZPropertyInfo AH_Readonly_RXCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AH_Readonly_RXCode)); }
		}

		#endregion

		#region DirectDebitNumber

		public virtual ZString DirectDebitNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo DirectDebitNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DirectDebitNumber); }
		}

		#endregion

		#region DepositBatchNumber

		public virtual ZString DepositBatchNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo DepositBatchNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DepositBatchNumber); }
		}

		#endregion

		#region DaysOverdue

		public ZInt DaysOverdue
		{
			get
			{
				if (!AH_DueDate.IsEmpty && AH_FullyPaidDate.IsEmpty)
				{
					TimeSpan diff = ZDateTime.Now.ToDateTime().Subtract(AH_DueDate.ToDateTime());
					return ((ZInt)diff.Days) > 0 ? (ZInt)diff.Days : ZInt.Zero;
				}
				else
				{
					return ZInt.Zero;
				}
			}
		}
		public ZPropertyInfo DaysOverdueInfo
		{
			get { return GetZPropertyInfo(nameof(DaysOverdue)); }
		}

		#endregion

		#region OH_APSettlementGroup

		public ZGuid OH_APSettlementGroup
		{
			get
			{
				if (Header != null)
				{
					return Header.APSettlementGroupPK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		#endregion

		#region AH_AKCode

		public virtual ZString AH_AKCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region AH_MatchStatus

		[List("MatchStatusList")]
		[ResourceStringData("939aa9c7-db35-4895-bb45-bdbf66af4d79", Caption = "Match Status")]
		public override ZString AH_MatchStatus { get => base.AH_MatchStatus; set => base.AH_MatchStatus = value; }

		public ReadOnlyCodeDescriptionPairList MatchStatusList => AccountingUtils.GetMatchStatusList();

		#endregion

		#region AH_MatchStatusReasonCode

		[List("MatchStatusReasonCodeList")]
		[ResourceStringData("2127d7ac-20ba-4a1c-9824-daba5b32ae9a", Caption = "Match Status Reason")]
		public override ZString AH_MatchStatusReasonCode { get => base.AH_MatchStatusReasonCode; set => base.AH_MatchStatusReasonCode = value; }

		public ReadOnlyCodeDescriptionPairList MatchStatusReasonCodeList => AccountingUtils.GetMatchStatusReasonCodeList();

		#endregion

		#region DisplayInvoiceAddressOverride

		[List("DisplayInvoiceAddressOverrides")]
		public ZGuid DisplayInvoiceAddressOverride
		{
			get { return AH_OA_InvoiceAddressOverride.IsValid ? AH_OA_InvoiceAddressOverride : DefaultOrgAddressPK; }
			set { AH_OA_InvoiceAddressOverride = value; }
		}

		public ZPropertyInfo DisplayInvoiceAddressOverrideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DisplayInvoiceAddressOverride, x => AH_OA_InvoiceAddressOverrideInfo); }
		}

		bool AddressAndContactOverrideReadOnly
		{
			get { return !AH_OH.IsValid || AH_OHInfo.ReadOnly; }
		}

		public bool DisplayInvoiceAddressOverride_ReadOnly
		{
			get { return AddressAndContactOverrideReadOnly; }
		}

		public OrgAddressDependentCollection DisplayInvoiceAddressOverrides
		{
			get
			{
				OrgAddressDependentCollection fAddresses = new OrgAddressDependentCollection(Factory);
				OrgHeader parent = Factory.Load<OrgHeader>(AH_OH);
				if (parent != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fAddresses = new OrgAddressDependentCollection(parent, filter);
					fAddresses.Load();
				}
				return fAddresses;
			}
		}

		protected ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			if (orgHeader != null)
			{
				AH_OH = orgHeader.PK;
			}
			else
			{
				AH_OH = ZGuid.Empty;
			}

			return DefaultOrgAddressPK;
		}

		protected virtual ZGuid DefaultOrgAddressPK
		{
			get
			{
				var result = ZGuid.Empty;
				if (IsAddressApplicableForDocumentSending)
				{
					if (Header != null)
					{
						if (AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.IncompleteTransactions || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
									|| AH_Ledger == LedgerTypes.TransactionsPendingAllocation)
						{
							var address = Header.AddressForSendingAPDocuments;
							result = address != null ? address.PK : ZGuid.Empty;
						}
						else if (AH_Ledger == LedgerTypes.AccountsReceivable)
						{
							var job = Factory.Load<Job>(AH_JH);
							if (job != null && job.LocalCharges != null && AH_OH == job.LocalCharges.PK)
							{
								result = job.JH_OA_LocalChargesAddr;
							}
							else if (job != null && AH_OH == job.AgentCollectPK)
							{
								result = job.JH_OA_AgentCollectAddr;
							}
							else
							{
								var address = Header.AddressForSendingARDocuments;
								result = address != null ? address.PK : ZGuid.Empty;
							}
						}
					}
				}

				return result;
			}
		}

		bool IsAddressApplicableForDocumentSending
		{
			get
			{
				return ((this is InvoicingBase) &&
					(AH_Ledger == LedgerTypes.AccountsReceivable ||
						AH_Ledger == LedgerTypes.AccountsPayable ||
						AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
						AH_Ledger == LedgerTypes.IncompleteTransactions))
						|| (this is TransactionPendingAllocation)
						|| (this is InvoiceBatchHeader);
			}
		}

		#endregion

		#region DisplayInvoiceContactOverride

		[List("DisplayInvoiceContactOverrides")]
		public ZGuid DisplayInvoiceContactOverride
		{
			get { return AH_OC_InvoiceContactOverride; }
			set { AH_OC_InvoiceContactOverride = value; }
		}

		public ZPropertyInfo DisplayInvoiceContactOverrideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DisplayInvoiceContactOverride, x => AH_OC_InvoiceContactOverrideInfo); }
		}

		public bool DisplayInvoiceContactOverride_ReadOnly
		{
			get { return AddressAndContactOverrideReadOnly; }
		}

		public OrgContactDependentCollection DisplayInvoiceContactOverrides
		{
			get
			{
				OrgContactDependentCollection fContacts = new OrgContactDependentCollection(Factory);
				OrgHeader parent = Factory.Load<OrgHeader>(AH_OH);
				if (parent != null)
				{
					ZQuery filter = new ZQuery(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fContacts = new OrgContactDependentCollection(parent, filter);
					fContacts.Load();
				}
				return fContacts;
			}
		}

		#endregion

		#region Agreed Payment Method

		public ReadOnlyCodeDescriptionPairList DisplayAgreedPaymentMethodsList
		{
			get
			{
				if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					return OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList();
				}
				else
				{
					return Env.Registry.PayablesCreditAgreedPaymentMethodsList;
				}
			}
		}

		[List(nameof(DisplayAgreedPaymentMethodsList))]
		public override ZString AH_AgreedPaymentMethodOverride
		{
			get
			{
				return base.AH_AgreedPaymentMethodOverride;
			}

			set
			{
				base.AH_AgreedPaymentMethodOverride = value;
			}
		}

		#endregion

		#region Receiving job properies

		public virtual ZString ReceivingOperator
		{
			get { return ZString.Empty; }
		}

		[List("Lookups.Branches")]
		public virtual ZGuid ReceivingBranch
		{
			get { return ZGuid.Empty; }
		}

		[List("Lookups.Departments")]
		public virtual ZGuid ReceivingDepartment
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region PostingGroup

		public ZShort PostingGroup
		{
			get
			{
				var postingGroup = new ZShort();
				if (this is TransactionHeaderWithLines headerWithLines)
				{
					var invoiceLines = headerWithLines.Lines.Cast<InvoicingLineBase>();
					var firstLine = invoiceLines.FirstOrDefault();
					if (firstLine != null)
					{
						postingGroup = firstLine.PostingGroupID;
					}
				}
				
				return postingGroup;
			}
		}

		public ZPropertyInfo PostingGroupInfo
		{
			get { return GetZPropertyInfo(nameof(Schema.PostingGroup)); }
		}

		#endregion

		#endregion

		#region ExchangeRate

		public virtual ZExchangeRate ExchangeRate
		{
			get
			{
				if (fAH_ExchangeRate == null)
				{
					fAH_ExchangeRate = new ZAccExchangeRate(this, RateType, AH_ExchangeRateInfo, (ZPropertyInfoString)AH_RX_NKTransactionCurrencyInfo, AH_GCInfo);
					fAH_ExchangeRate.IsCurrencyRequired = true;
					fAH_ExchangeRate.IsRateRequired = true;
					SetExchangeRateAdditionalRateReadOnlyCondition();
				}

				return fAH_ExchangeRate;
			}
		}

		protected ZExchangeRate fAH_ExchangeRate;

		void DeleteExchangeRate() => fAH_ExchangeRate?.Delete();

		protected virtual void SetExchangeRateAdditionalRateReadOnlyCondition()
		{
		}

		public virtual ExchangeRateType RateType
		{
			get
			{
				return Ledger == LedgerTypes.AccountsPayable ||
					Ledger == LedgerTypes.UnapprovedPayableTransactions ||
					Ledger == LedgerTypes.TransactionsPendingAllocation
					? ExchangeRateType.Buy : ExchangeRateType.Sell;
			}
		}

		#region High Precision Exchange Rate

		internal ZDecimal GetHighPrecisionExchangeRate()
		{
			return Env.CurrentCompany.ExchangeRate.GetRate(AH_LocalTotal, AH_OSTotal, AccTransactionHeaderSchema.AH_ExchangeRate.Scale);
		}

		#endregion

		#endregion

		#region Post Period

		protected ZInt fPostPeriod;

		public virtual ZInt PostPeriod
		{
			get
			{
				return (AH_PostDate.IsValid && PeriodCalculator != null) ? PeriodCalculator.GetPeriodFromDate(AH_PostDate) : fPostPeriod;
			}
			set
			{
				if (PeriodCalculator != null && PeriodCalculator.IsPeriodValid(value))
				{
					AH_PostDate = PeriodCalculator.GetLastDayForPeriod(value);
				}
				else
				{
					AH_PostDate = ZDateTime.Empty;
				}
				SetNonPersistentPropertyValue(PostPeriodInfo, ref fPostPeriod, value);
				if (HeaderValidation != null)
				{
					HeaderValidation.ValidatePostPeriod();
				}
			}
		}

		public ZPropertyInfo PostPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.PostPeriod); }
		}

		#endregion

		public override ZDateTime AH_PostDate
		{
			get { return base.AH_PostDate; }
			set
			{
				ZInt oldPostPeriod = PostPeriod;
				base.AH_PostDate = value;
				if (oldPostPeriod != PostPeriod)
				{
					PostPeriodInfo.RefreshBinding(oldPostPeriod);
				}
			}
		}

		#region Age Period

		protected ZInt fAgePeriod;

		public virtual ZInt AgePeriod
		{
			get
			{
				return (AH_DueDate.IsValid && PeriodCalculator != null) ? PeriodCalculator.GetPeriodFromDate(AH_DueDate) : fAgePeriod;
			}
			set
			{
				if (PeriodCalculator != null && PeriodCalculator.IsPeriodValid(value))
				{
					AH_DueDate = PeriodCalculator.GetLastDayForPeriod(value);
				}
				else
				{
					AH_DueDate = ZDateTime.Empty;
				}
				SetNonPersistentPropertyValue(AgePeriodInfo, ref fAgePeriod, value);
				if (HeaderValidation != null)
				{
					HeaderValidation.ValidateAgePeriod();
				}
			}
		}

		public ZPropertyInfo AgePeriodInfo
		{
			get { return GetZPropertyInfo(Schema.AgePeriod); }
		}

		#endregion

		public override ZDateTime AH_DueDate
		{
			get { return base.AH_DueDate; }
			set
			{
				ZInt oldAgePeriod = AgePeriod;
				base.AH_DueDate = value;
				if (oldAgePeriod != AgePeriod)
				{
					AgePeriodInfo.RefreshBinding(oldAgePeriod);
				}
			}
		}

		#region Invoice Remittance Type

		[List(nameof(InvoiceRemittanceTypeCodeList))]
		public override ZString AH_InvoicePaymentReferenceCode
		{
			get => base.AH_InvoicePaymentReferenceCode;
			set => base.AH_InvoicePaymentReferenceCode = value;
		}

		protected InvoiceRemittanceConfiguration[] GetInvoiceRemittanceConfigurationCollection()
		{
			var collection = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.GetValueWithoutFallback(AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (collection != null)
			{
				return collection.GetMatchInvoiceRemittanceConfiguration(Header);
			}
			else
			{
				return Array.Empty<InvoiceRemittanceConfiguration>();
			}
		}

		public ReadOnlyCodeDescriptionPairList InvoiceRemittanceTypeCodeList
		{
			get
			{
				var referenceCodeList = new CodeDescriptionPairList();
				if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					var collection = GetInvoiceRemittanceConfigurationCollection();
					foreach (var configuration in collection)
					{
						referenceCodeList.AddPair(configuration.Code, configuration.Description);
					}
				}

				return referenceCodeList;
			}
		}

		#endregion

		#region AR Client Number

		public ZString OrgARClientNumber
		{
			get { return Header != null ? Header.CompanyData.OB_ARClientNumber : ZString.Empty; }
		}

		#endregion

		#region Invoice Transaction Reference

		public virtual ZString InvoiceTransactionReference
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Supporting Document Number

		[MaxLength(60)]
		public virtual ZString SupportingDocumentNumber
		{
			get
			{
				if (IsInDatabase)
				{
					_ = TransactionHeaderReferenceIRD;
				}

				return fSupportingDocumentNumber;
			}
			set
			{
				if (fSupportingDocumentNumber != value)
				{
					CheckMaximumLength(SupportingDocumentNumberInfo, value);

					if (TransactionHeaderReferenceIRD == null)
					{
						fTransactionHeaderReferenceIRD = Factory.New<AccTransactionHeaderReference>();
						fTransactionHeaderReferenceIRD.AH1_AH = PK;
						fTransactionHeaderReferenceIRD.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD;
					}

					TransactionHeaderReferenceIRD.AH1_Reference = value;
					SetNonPersistentPropertyValue(SupportingDocumentNumberInfo, ref fSupportingDocumentNumber, value);
					SupportingDocumentNumberInfo.RefreshBinding();

					if (HeaderValidation != null)
					{
						HeaderValidation.ValidateSupportingDocumentNumber();
					}
				}
			}
		}
		ZString fSupportingDocumentNumber;

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(Schema.SupportingDocumentNumber); }
		}

		protected AccTransactionHeaderReference TransactionHeaderReferenceIRD
		{
			get
			{
				if (fTransactionHeaderReferenceIRD == null)
				{
					fTransactionHeaderReferenceIRD = GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD);
					if (fTransactionHeaderReferenceIRD != null)
					{
						fSupportingDocumentNumber = fTransactionHeaderReferenceIRD.AH1_Reference;
					}
				}

				return fTransactionHeaderReferenceIRD;
			}
		}
		AccTransactionHeaderReference fTransactionHeaderReferenceIRD;

		#endregion

		#region Invoice Remittance Reference

		[MaxLength(120)]
		public virtual ZString InvoiceRemittanceReference
		{
			get
			{
				if (IsInDatabase)
				{
					_ = TransactionHeaderReferenceIRR;
				}
				return fInvoiceRemittanceReference;
			}
			set
			{
				if (fInvoiceRemittanceReference != value)
				{
					CheckMaximumLength(InvoiceRemittanceReferenceInfo, value);

					if (TransactionHeaderReferenceIRR == null)
					{
						fTransactionHeaderReferenceIRR = Factory.New<AccTransactionHeaderReference>();
						fTransactionHeaderReferenceIRR.AH1_AH = PK;
						fTransactionHeaderReferenceIRR.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;
					}

					TransactionHeaderReferenceIRR.AH1_Reference = value;
					SetNonPersistentPropertyValue(InvoiceRemittanceReferenceInfo, ref fInvoiceRemittanceReference, value);
					InvoiceRemittanceReferenceInfo.RefreshBinding();
				}
			}
		}
		ZString fInvoiceRemittanceReference;

		public ZPropertyInfo InvoiceRemittanceReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceRemittanceReference)); }
		}

		protected AccTransactionHeaderReference TransactionHeaderReferenceIRR
		{
			get
			{
				if (fTransactionHeaderReferenceIRR == null)
				{
					fTransactionHeaderReferenceIRR = GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR);
					if (fTransactionHeaderReferenceIRR != null)
					{
						fInvoiceRemittanceReference = fTransactionHeaderReferenceIRR.AH1_Reference;
					}
				}

				return fTransactionHeaderReferenceIRR;
			}
		}
		AccTransactionHeaderReference fTransactionHeaderReferenceIRR;

		#endregion

		#region Cashbook Properties

		#region Debit

		protected ZDecimal fDebit;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public virtual ZDecimal Debit
		{
			get { return fDebit; }
		}

		public ZPropertyInfo DebitInfo
		{
			get { return GetZPropertyInfo(nameof(Debit)); }
		}

		#endregion

		#region Credit

		protected ZDecimal fCredit;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public virtual ZDecimal Credit
		{
			get { return fCredit; }
		}

		public ZPropertyInfo CreditInfo
		{
			get { return GetZPropertyInfo(nameof(Credit)); }
		}

		#endregion

		#region Debit/Credit for Direct Receipt/Direct Payment/Cashbook Transfer

		protected ZDecimal DebitForDirectReceiptPayment
		{
			get { return AH_OSTotal >= 0m ? AH_OSTotal : (ZDecimal)0m; }
		}

		protected ZDecimal CreditForDirectReceiptPayment
		{
			get { return AH_OSTotal < 0m ? -AH_OSTotal : 0m; }
		}

		#endregion

		#region Debit/Credit for Normal/Opening Receipt/Payments

		protected ZDecimal DebitForNormalReceiptPayment
		{
			get { return AH_OSTotal < 0m ? -AH_OSTotal : 0m; }
		}

		protected ZDecimal CreditForNormalReceiptPayment
		{
			get { return AH_OSTotal >= 0m ? AH_OSTotal : (ZDecimal)0m; }
		}

		#endregion

		#region Local Debit

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalDebit
		{
			get { return LocalDebitCore; }
		}

		protected virtual ZDecimal LocalDebitCore
		{
			get { return RoundAmountToLocalDecimals(Debit); }
		}

		public ZPropertyInfo LocalDebitInfo
		{
			get { return GetZPropertyInfo(nameof(LocalDebit)); }
		}

		#endregion

		#region Local Credit

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalCredit
		{
			get { return LocalCreditCore; }
		}

		protected virtual ZDecimal LocalCreditCore
		{
			get { return RoundAmountToLocalDecimals(Credit); }
		}

		public ZPropertyInfo LocalCreditInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCredit)); }
		}

		#endregion

		#endregion

		#region Matching Properties

		#region MatchedAmount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalMatchedAmount
		{
			get { return fLocalMatchedAmount; }
			private set { SetNonPersistentPropertyValue(LocalMatchedAmountInfo, ref fLocalMatchedAmount, value); }
		}
		ZDecimal fLocalMatchedAmount;

		public ZPropertyInfo LocalMatchedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalMatchedAmount)); }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal MatchedAmount
		{
			get { return fMatchedAmount; }
			private set { SetNonPersistentPropertyValue(MatchedAmountInfo, ref fMatchedAmount, value); }
		}
		ZDecimal fMatchedAmount;

		public ZPropertyInfo MatchedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(MatchedAmount)); }
		}

		public void SetMatchedAmount(TransactionMatchLink matchLink, bool isPositive)
		{
			var multiplier = isPositive ? 1 : -1;
			LocalMatchedAmount = multiplier * matchLink.AP_Amount;
			MatchedAmount = multiplier * TransactionMatchLinkOSAmountProvider.GetMatchLinkOSAmount(matchLink);
		}

		#endregion

		#region MatchedDate

		// different to the MatchDate on IMatching because
		// this date corresponds to a matchgroup that the
		// user has chosen
		public ZDateTime MatchedDate
		{
			get { return fMatchedDate; }
			set { SetNonPersistentPropertyValue(MatchedDateInfo, ref fMatchedDate, value); }
		}
		ZDateTime fMatchedDate;

		public ZPropertyInfo MatchedDateInfo
		{
			get { return GetZPropertyInfo(nameof(MatchedDate)); }
		}

		#endregion

		#region OpenQueryClaim

		public APAccQueryClaim OpenQueryClaim
		{
			get
			{
				var closedQueryStatuses = new string[] { QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed,
																								 QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed,
																								 QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed };
				var claimFilter = new ZQuery(AccQueryClaimSchema.AY_AH, PK);
				claimFilter.AddToFilter(AccQueryClaimSchema.AY_QueryClaimStatus, SQLComparisonOperator.NotEqual, closedQueryStatuses);
				return Factory.LoadTop1<APAccQueryClaim>(claimFilter);
			}
		}

		#endregion

		#region RelatedClaim

		public ZString RelatedClaimStatus
		{
			get
			{
				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					TransactionHeaderCollection collection = parentCollection as TransactionHeaderCollection;
					if (collection != null)
					{
						return collection.RelatedClaimStatus(PK);
					}
				}
				TransactionHeaderCollection col = new TransactionHeaderCollection(Factory);
				return col.RelatedClaimStatus(PK);
			}
		}

		public ZPropertyInfo RelatedClaimStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedClaimStatus); }
		}

		public ZString QueryNumber
		{
			get
			{
				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					TransactionHeaderCollection collection = parentCollection as TransactionHeaderCollection;
					if (collection != null)
					{
						return collection.RelatedClaimQueryNumber(PK);
					}
				}
				TransactionHeaderCollection col = new TransactionHeaderCollection(Factory);
				return col.RelatedClaimQueryNumber(PK);
			}
		}

		public ZPropertyInfo QueryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.QueryNumber); }
		}

		#endregion

		#region BindableInvoiceAmount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal BindableInvoiceAmount
		{
			get { return AH_InvoiceAmount; }
			set { SetBindableInvoiceAmountWithExRateRecalculation(value); }
		}

		protected bool BindableInvoiceAmount_ReadOnly
		{
			get { return IsMiscellaneousTransaction || AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo BindableInvoiceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(BindableInvoiceAmount)); }
		}

		#endregion

		#region BindableOSAmount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal BindableOSAmount
		{
			get { return AH_OSTotal; }
			set
			{
				SetBindableInvoiceAmountWithoutExRateRecalculation(RoundAmountToLocalDecimals(value));
				AH_OSTotal = value; // + Env.CurrentCompany.ExchangeRate.LocalToForeign(AH_GSTAmount, AH_ExchangeRate, AH_RX_NKTransactionCurrency.ToGuid());
									//fBindableOSAmount = value;
				BindableOSAmountInfo.RefreshBinding();
				AH_OutstandingAmountInfo.RefreshBinding();
			}
		}
		//private ZDecimal fBindableOSAmount;

		public ZPropertyInfo BindableOSAmountInfo
		{
			get { return GetZPropertyInfo(nameof(BindableOSAmount)); }
		}

		#endregion

		#region LatestMatchLink

		// the most recent matchlink for this transaction
		public TransactionMatchLink LatestMatchLink
		{
			get
			{
				var matchDateFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK);
				return Factory.Load<TransactionMatchLink>(matchDateFilter).OrderByDescending(x => x.AP_MatchDate).FirstOrDefault();
			}
		}

		#endregion

		#region InvoiceBatchNumber

		public virtual ZString InvoiceBatchNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo InvoiceBatchNumberInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceBatchNumber)); }
		}

		#endregion

		public AccBankAccount ReceiptBankAccount
		{
			get { return ReceiptBankAccountCore; }
		}

		protected virtual AccBankAccount ReceiptBankAccountCore
		{
			get { return null; }
		}

		#endregion

		#region Unmatching Properties

		public virtual ZBool AreRelatedTransactionsCreatedByMatching
		{
			get { return false; }
		}

		public virtual TransactionHeaderCollection RelatedTransactions
		{
			get
			{
				if (fRelatedTransactions == null)
				{
					fRelatedTransactions = new TransactionHeaderCollection(Factory);
				}
				return fRelatedTransactions;
			}
		}
		protected TransactionHeaderCollection fRelatedTransactions;

		public virtual IMatching GetTopLevelTransaction
		{
			get { return null; }
		}

		#endregion

		#region EDIMessages

		public EDIMessageCollection EDIMessages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = new EDIMessageCollection(this);
					ediMessages.SetReadOnlyIncludingChildren(true);
				}
				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		#endregion

		#region GenExportBatchSequence

		public GenExportBatchSequence ExportedBatchSequence
		{
			get
			{
				ZQuery filter = new ZQuery(GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport);
				filter.AddToFilter(GenExportBatchSequenceSchema.XB_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
				filter.AddToFilter(GenExportBatchSequenceSchema.XB_ParentID, PK);

				return Factory.LoadTop1<GenExportBatchSequence>(filter);
			}
		}

		#endregion

		#region Amend Status Code And Description

		public virtual ZString AmendStatusCodeAndDescription { get; }

		#endregion

		#region Transaction Header Reference RDI
		protected AccTransactionHeaderReference TransactionHeaderReferenceRDI
		{
			get
			{
				if (fTransactionHeaderReferenceRDI == null)
				{
					fTransactionHeaderReferenceRDI = GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice);
					if (fTransactionHeaderReferenceRDI != null)
					{
						fInvoiceRemittanceReference = fTransactionHeaderReferenceRDI.AH1_Reference;
					}
				}

				return fTransactionHeaderReferenceRDI;
			}
		}
		AccTransactionHeaderReference fTransactionHeaderReferenceRDI;

		#endregion

		#region Disbursement Relating To

		public virtual ZString RelatedDisbursementTransactions => TransactionHeaderReferenceRDI?.AH1_Reference ?? ZString.Empty;

		#endregion

		#region Creating User and Created Date

		public ZString CreatingUserID
		{
			get { return Creator?.GS_LoginName ?? ZString.Empty; }
		}

		public ZString CreatingUser
		{
			get { return Creator?.GS_FullName ?? ZString.Empty; }
		}

		public ZPropertyInfo CreatingUserInfo
		{
			get { return GetZPropertyInfo(nameof(CreatingUser)); }
		}

		public ZDateTime CreatedDate
		{
			get { return AH_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		public ZPropertyInfo CreatedDateInfo
		{
			get { return GetZPropertyInfo(nameof(CreatedDate)); }
		}

		protected StmALog CreateLog
		{
			get { return Logs.AddedLog.IsInDatabase || ShouldUseAddedLogNotInDatabase ? Logs.AddedLog : null; }
		}

		protected virtual bool ShouldUseAddedLogNotInDatabase { get { return false; } }

		#endregion

		#region Overridden Properties

		[ZUnbindableProperty()]
		public override ZDecimal AH_InvoiceAmount
		{
			get { return base.AH_InvoiceAmount; }
			set
			{
				base.AH_InvoiceAmount = value;

				if (HeaderValidation != null)
				{
					HeaderValidation.ValidateAH_LocalExTaxAmount();
				}
			}
		}

		public override ZDecimal AH_ExchangeRate
		{
			get { return base.AH_ExchangeRate; }
			set
			{
				var oldValue = AH_ExchangeRate;
				base.AH_ExchangeRate = Utilities.Round(value, ExchangeRateDecimalPlaces);
				if (!IsInDatabase && AH_ExchangeRate == 0m && (this.HasContext(BusinessContext.ShouldTraceExchangeRateError) || this.HasContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel)))
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory)?.AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.EvaluateTransactionHeaderWithZeroExchangeRate, () =>
					{
						var info = new ZStringBuilder();
						info.AppendLine(System.FormattableString.Invariant($"Origin ExchangeRate = {oldValue}, {nameof(AH_PostDate)} = {AH_PostDate}, {nameof(AH_InvoiceDate)} = {AH_InvoiceDate}, {nameof(InvoiceTaxDate)} = {InvoiceTaxDate}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					});
				}
				if (AH_ExchangeRate != oldValue && AH_ExchangeRate != 1m && AH_RX_NKTransactionCurrency.Equals(Company?.GC_RX_NKLocalCurrency))
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory)?.AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.EvaluateTransactionHeaderWithNotEqualOneExchangeRateAndLocalCurrency, () =>
					{
						var info = new ZStringBuilder();
						info.AppendLine(System.FormattableString.Invariant($"Origin ExchangeRate = {oldValue}, {nameof(AH_ExchangeRate)} = {AH_ExchangeRate}, {nameof(AH_RX_NKTransactionCurrency)} = {AH_RX_NKTransactionCurrency}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					});
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AccTransactionHeaderLookups.PlacesOfSupply))]
		[ResourceStringData("d6df7f67-c0da-4cce-aa82-0865465bff47", ShortCaption = "FPOS", Caption = "Place of Supply")]
		[ReadOnly(true)]
		public override ZString AH_PlaceOfSupply
		{
			get { return base.AH_PlaceOfSupply; }
			set
			{
				if (AH_PlaceOfSupply != value)
				{
					base.AH_PlaceOfSupply = value;
					AH_PlaceOfSupplyType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(Company, AH_PlaceOfSupply);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AccTransactionHeaderLookups.PlaceOfSupplyTypes))]
		[ResourceStringData("6bb47f17-19dd-4ae0-8b2e-7b1ad9044e58", ShortCaption = "FPOS Type", Caption = "Place of Supply Type")]
		public override ZString AH_PlaceOfSupplyType
		{
			get { return base.AH_PlaceOfSupplyType; }
			set
			{
				if (base.AH_PlaceOfSupplyType != value)
				{
					base.AH_PlaceOfSupplyType = value;
					Validation.ValidateAH_PlaceOfSupply();
				}
			}
		}

		public virtual bool NeedPlaceOfSupplyAtHeaderLevel => false;

		#endregion

		#region UserAllowedToBackPost

		public bool AllowBackPosting
		{
			get { return AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && UserAllowedToBackPost; }
		}

		public bool AllowFuturePosting
		{
			get { return AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value && UserAllowedToFuturePost(false); }
		}

		public bool AllowFuturePostingForReceiptPaymentOnInvoice
		{
			get { return AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value && UserAllowedToFuturePost(true); }
		}

		public virtual bool UserAllowedToBackPost
		{
			get
			{
				switch (AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						return Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.UnapprovedPayableTransactions:
					case LedgerTypes.TransactionsPendingAllocation:
					case LedgerTypes.IncompleteTransactions:
						return Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
					case LedgerTypes.CashBook:
						return Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;
					case LedgerTypes.JobCosting:
						return Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed;
					default:
						return false;
				}
			}
		}

		public bool UserAllowedToFuturePost(bool isReceiptPaymentBox)
		{
			switch (AH_Ledger)
			{
				case LedgerTypes.AccountsPayable:
				case LedgerTypes.AccountsReceivable:
					if (isReceiptPaymentBox || AH_TransactionType == TransactionTypes.Payment || AH_TransactionType == TransactionTypes.Receipt || AH_TransactionType == TransactionTypes.ReceiptBatch)
					{
						return Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint();
					}
					else
					{
						return false;
					}
				case LedgerTypes.CashBook:
					if (AH_TransactionType == TransactionTypes.DirectPayment || AH_TransactionType == TransactionTypes.DirectReceipt)
					{
						return Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint();
					}
					else
					{
						return false;
					}
				default:
					return false;
			}
		}

		#endregion

		public bool UserAllowedToModifyInvoiceDateWhenReversing
		{
			get
			{
				bool result = false;
				switch (AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						if (this is ARInvoice || this is ARCreditNote)
						{
							result = Env.Security.ReceivablesModifyInvoiceDateWhenReversing.IsAllowed;
						}
						break;
					case LedgerTypes.AccountsPayable:
						if (this is APInvoice || this is APCreditNote)
						{
							result = Env.Security.PayablesModifyInvoiceDateWhenReversing.IsAllowed;
						}
						break;
				}
				return result;
			}
		}

		#region Days before paid properties

		public ZInt DaysFromInvoiceDateToFullyPaidDate
		{
			get { return AH_FullyPaidDate.IsEmpty || AH_InvoiceDate.IsEmpty ? 0 : (AH_FullyPaidDate - AH_InvoiceDate).Days; }
		}

		public ZPropertyInfo DaysFromInvoiceDateToFullyPaidDateInfo
		{
			get { return GetZPropertyInfo(Schema.DaysFromDueDateToFullyPaidDate); }
		}

		public ZInt DaysFromDueDateToFullyPaidDate
		{
			get { return AH_FullyPaidDate.IsEmpty || AH_DueDate.IsEmpty ? 0 : (AH_FullyPaidDate - AH_DueDate).Days; }
		}

		public ZPropertyInfo DaysFromDueDateToFullyPaidDateInfo
		{
			get { return GetZPropertyInfo(Schema.DaysFromDueDateToFullyPaidDate); }
		}

		#endregion

		#region Implementation

		protected abstract bool InvertSigns { get; }
		protected abstract ZString TransactionType { get; }
		protected abstract ZString Ledger { get; }
		protected abstract AccountingNumberFountainWrapper NumberFountainForTransactionNumber { get; }

		// To be used by IMatching in all subclasses

		public ZString TransactionCategory
		{
			get { return AH_TransactionCategory; }
		}

		public ZPropertyInfo TransactionCategoryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.TransactionCategory, x => AH_TransactionCategoryInfo); }
		}

		public ZBool IsDocumentReceivedDateApplicable
		{
			get
			{
				return (AH_Ledger == LedgerTypes.AccountsPayable &&
						(AH_TransactionType == TransactionTypes.CreditNote ||
						AH_TransactionType == TransactionTypes.Invoice ||
						AH_TransactionType == TransactionTypes.AdjustmentNote)) ||
						AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||
						AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
						AH_Ledger == LedgerTypes.IncompleteTransactions;
			}
		}

		#region Readonlyness

		protected virtual bool IsTransactionInDatabaseReadOnlyCore
		{
			get { return true; }
		}

		internal bool IsTransactionInDatabaseReadOnly
		{
			get { return IsTransactionInDatabaseReadOnlyCore; }
		}

		List<string> WritableProperties
		{
			get
			{
				if (WritableProperties_cached == null)
				{
					WritableProperties_cached = GetWritableProperties();
				}
				return WritableProperties_cached;
			}
		}
		List<string> WritableProperties_cached;

		protected virtual List<string> GetWritableProperties()
		{
			return new List<string> { "OSPartialPaymentAmount", "MatchStatus", "MatchStatusReasonCode" };
		}

		void ResetWritableProperties()
		{
			WritableProperties_cached = null;
		}

		bool UseWritablePropertiesToDecideReadOnly
		{ get; set; }

		public void AddWritableProperties(string[] writablePropertyList)
		{
			if (writablePropertyList != null && writablePropertyList.Length > 0)
			{
				foreach (string property in writablePropertyList)
				{
					if (!WritableProperties.Contains(property))
					{
						WritableProperties.Add(property);
					}
				}
			}
			SetObjectReadOnly();
			RefreshBinding();
			AH_OA_InvoiceAddressOverrideInfo.RefreshBinding();
		}

		void SetObjectReadOnly()
		{
			UseWritablePropertiesToDecideReadOnly = true;

			ReadOnly = false;
			if (WritableProperties.Count == 0)
			{
				ReadOnly = true;
			}
		}

		protected override bool AH_ChequeOrReference_ReadOnly
		{
			get
			{
				if (this is IMatching)
				{
					return ((IMatching)this).ChequeOrReference_ReadOnly;
				}
				else
				{
					return chequeOrReference_ReadOnly;
				}
			}
			set { chequeOrReference_ReadOnly = value; }
		}
		bool chequeOrReference_ReadOnly;

		// At runtime, reflection will find this method and use it for all derived classes which have property ChequeOrReference
		protected bool ChequeOrReference_ReadOnly
		{
			get
			{
				return AH_ChequeOrReference_ReadOnly;
			}
		}

		protected virtual bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			bool result = true;
			if (property.HasSetter())
			{
				if (UseWritablePropertiesToDecideReadOnly && property.Name != UnmatchDateInfo.Name)
				{
					result = !WritableProperties.Contains(property.Name);
				}
				else
				{
					result = CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
				}
			}
			return result;
		}

		#endregion

		protected int Multiplier
		{
			get { return InvertSigns ? -1 : 1; }
		}

		public virtual bool CanUserPostToPreviousPeriods
		{
			get { return Env.Security.GeneralLedgerPostToPreviousOpenPeriod.IsAllowed; }
		}

		protected void SetTransactionCategoryOnly(ZString value)
		{
			AH_TransactionCategory = value;
		}

		[List("Lookups.Headers")]
		public sealed override ZGuid AH_OH
		{
			get { return AH_OHCore; }
			set { AH_OHCore = value; }
		}

		protected virtual ZGuid AH_OHCore
		{
			get { return base.AH_OH; }
			set { base.AH_OH = value; }
		}

		protected virtual bool AH_OH_ReadOnly
		{
			get { return IsMiscellaneousTransaction; }
		}

		protected ZDecimal RoundAmountToCurrencyDecimals(ZDecimal value)
		{
			return Utilities.Round(value, OSCurrencyDecimals);
		}

		public virtual bool OSPartialPaymentAmount_ReadOnly => osPartialPaymentAmountReadOnly;

		bool osPartialPaymentAmountReadOnly;

		public void SetOSPartialPaymentReadOnly() => osPartialPaymentAmountReadOnly = true;

		public override void OnLoaded()
		{
			base.OnLoaded();

			recalculateOSTaxAmounts = true;
			recalculateOSWHTAmount = true;

			if (IsInDatabase && IsTransactionInDatabaseReadOnly)
			{
				SetObjectReadOnly();
			}
		}

		bool recalculateOSTaxAmounts;
		bool recalculateOSWHTAmount;

		void RecalculateOSTaxAmounts()
		{
			if (recalculateOSTaxAmounts)
			{
				recalculateOSTaxAmounts = false;
				aH_OSTaxAmount = Company.GetExchangeRate().LocalToForeign(AH_LocalTaxAmount, AH_ExchangeRate, AH_RX_NKTransactionCurrency) * Multiplier;
				aH_OSExTaxAmount = (AH_OSTotalAmount - AH_OSTaxAmount - AH_OSTaxAmountOtherTaxes_ForDisplay) * Multiplier;
			}
		}

		void RecalculateOSWHTAmount()
		{
			if (recalculateOSWHTAmount)
			{
				recalculateOSWHTAmount = false;
				aH_OSWHTAmount = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(AH_LocalWHTAmount, AH_ExchangeRate, TransactionCurrency) * Multiplier;
			}
		}

		protected int FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader
		{
			get
			{
				return AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(this, FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeaderName);
			}
			set
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(this, FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeaderName, value);
			}
		}
		const string FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeaderName = "FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeaderName";

		protected bool IsTransactionNumSetFromNumberFountainAtTransactionHeader => Factory.IsEqualToCurrentSaveCount(FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader);

		#region IAccountingNumberFountainDataSource members

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => AH_PostDate;

		GlbBranch IAccountingNumberFountainDataSource.Branch => Branch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => Department;

		#endregion

		protected override void OnSavingCore()
		{
			base.OnSavingCore();

			var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
			using (new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended))
			using (this.GetValidationSuspender())
			{
				if (!IsTransactionNumSetFromNumberFountainAtTransactionHeader)
				{
					if (NumberFountainForTransactionNumber != null && NeedToUpdateTransactionNumberFromFountain)
					{
						if (Factory.HasContext(BusinessContext.eNettOutboundSubscriberLWKServiceTask))
						{
							RunInUserContextForCorrectCompany(() =>
							{
								AH_TransactionNum = NumberFountainForTransactionNumber.Generate(this);
							}
								);
						}
						else
						{
							AH_TransactionNum = NumberFountainForTransactionNumber.Generate(this);
						}
						FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader = Factory.SaveCount;
						RaiseTransactionNumberSet();
					}
				}
			}
			RunCriticalValidationForTransactionNumber();

			EInvoicingTransaction.EvaluateEligibilityAndQueue();

			DeleteTransactionHeaderReferenceIfEmpty();

			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
		}

		protected override void OnSavingFinalize()
		{
			TransactionHeaderOSOutstandingAmountProvider.UpdateOSOutstandingAmount(this);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				if (IsTransactionInDatabaseReadOnly)
				{
					SetObjectReadOnly();
					if (NeedToResetWritableProperties)
					{
						ResetWritableProperties();
						NeedToResetWritableProperties = false;
					}
				}
			}

			if (!saveSucceeded && IsTransactionNumSetFromNumberFountainAtTransactionHeader && NeedToUpdateTransactionNumberFromFountain)
			{
				AH_TransactionNum = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		public bool NeedToResetWritableProperties
		{ get; set; }

		public event EventHandler TransactionNumberSet;

		void RaiseTransactionNumberSet()
		{
			if (TransactionNumberSet != null)
			{
				TransactionNumberSet(this, EventArgs.Empty);
			}
		}

		void RunCriticalValidationForTransactionNumber()
		{
			if (AH_TransactionNum == ZString.Empty)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory)?.AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.InvoicingBaseDidNotCreateTransactionNumberOnSaving, () =>
				{
					return string.Format(CultureInfo.InvariantCulture, (NoResString)@"TransactionHeader OnSavingCore Info:
FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader: {0}
NumberFountainForTransactionNumber: {1}
NeedToUpdateTransactionNumberFromFountain: {2}",
						FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader.ToString(CultureInfo.InvariantCulture),
						NumberFountainForTransactionNumber == null ? "NULL" : NumberFountainForTransactionNumber.GetType().FullName,
						NeedToUpdateTransactionNumberFromFountain.ToString());
				});
			}
		}

		protected virtual bool NeedToUpdateTransactionNumberFromFountain
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && IsManuallySetTransactionNumber_ForTestOnly)
				{
					return false;
				}
#endif
				return !IsInDatabase;
			}
		}

#if DEBUG
		public bool IsManuallySetTransactionNumber_ForTestOnly;
#endif

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = base.ReasonForNotAbleToDelete;

				if (!AllowDelete)
				{
					result = ResString.GetMultilingualString("8409df3b-db1e-45ae-9efe-b22196065fa9", "This transaction has been exported and cannot be deleted.");
				}

				return result;
			}
		}

		public sealed override void Delete()
		{
			if (!IsDeleted)
			{
				if (AllowDelete)
				{
					DeleteCore();
				}
				else
				{
					throw new NotSupportedException("You cannot delete Accounting Transactions in Database.");
				}
			}
		}

		protected virtual void DeleteCore()
		{
			DeleteExchangeRate();
			DeleteFromDB();
		}

		protected virtual bool AllowDelete
		{
			get { return !IsInDatabase; }
		}

		public virtual void DeleteFromDB()
		{
			SubAccountHelper.DeleteSubAccounts(this as ISupportMultiSubAccounts);
			base.Delete();
		}

		void SetLocalExTaxAmountWithoutExRateRecalculation(ZDecimal value)
		{
			AH_InvoiceAmount = value * Multiplier;
			AH_InvoiceAmountInfo.RefreshBinding();
			SetOutstandingLocalAmountAfterLocalExTaxAmountSet();
		}

		void SetLocalExTaxAmountWithExRateRecalculation(ZDecimal value)
		{
			AH_InvoiceAmount = value * Multiplier;
			AH_InvoiceAmountInfo.RefreshBinding();
			AH_LocalExTaxAmountInfo.RefreshBinding();
			SetOutstandingLocalAmountAfterLocalExTaxAmountSet();
		}

		protected virtual void SetOutstandingLocalAmountAfterLocalExTaxAmountSet()
		{
		}

		protected void SetOutstandingLocalAmountAfterLocalExTaxAmountSetCore()
		{
			AH_LocalOutstandingAmount = AH_LocalTotalAmount;
		}

		void SetBindableInvoiceAmountWithoutExRateRecalculation(ZDecimal value)
		{
			AH_InvoiceAmount = value;
			AH_OutstandingAmount = AH_LocalTotal;
			if (!IsValidationSuspended && HeaderValidation != null)
			{
				HeaderValidation.ValidateBindableInvoiceAmount();
			}
		}

		void SetBindableInvoiceAmountWithExRateRecalculation(ZDecimal value)
		{
			AH_InvoiceAmount = value;
			AH_OutstandingAmount = AH_LocalTotal;
			BindableInvoiceAmountInfo.RefreshBinding();
			if (!IsValidationSuspended && HeaderValidation != null)
			{
				HeaderValidation.ValidateBindableInvoiceAmount();
			}
		}

		#region Default Values

		public virtual ZString DefaultDescription
		{
			get
			{
				ZString defaultDesbyReg = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(AH_TransactionType);
				if (defaultDesbyReg.Length > 2)
				{
					defaultDesbyReg = AH_Ledger + " " + defaultDesbyReg;
				}

				return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AH_Ledger + AH_TransactionType, defaultDesbyReg);
			}
		}

		public virtual ZBool AH_NumberOfSupportingDocumentsVisible_ReadOnly
		{
			get { return IsNumberOfSupportingDocumentsVisible; }
		}

		public static bool IsNumberOfSupportingDocumentsVisible
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_GB = GlbBranch.CurrentBranch.PK;
			AH_GE = GlbDepartment.CurrentDepartment.PK;
			AH_RX_NKTransactionCurrency = Company?.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AH_TransactionType = TransactionType;
			AH_Ledger = Ledger;
			AH_PostDate = ZDateTime.Now;
			AH_InvoiceDate = DefaultInvoiceDate;
			AH_Desc = DefaultDescription;
			AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AH_Ledger + AH_TransactionType, 0);
			OSOutstandingAmountValueChangeMonitor = new OSOutstandingAmountValueChangeMonitor(this);
		}

		#endregion

		public virtual ZDateTime DefaultInvoiceDate => ZDateTime.Now;

		public virtual ZBool AH_IsGSTCashBasis_ReadOnly
		{
			get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered && GlbCompany.CurrentCompany.GC_IsGSTCashBasis; }
		}

		public virtual ZBool AH_GC_IsGSTRegistered_ReadOnly
		{
			get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		internal FunctionalitySuspender AmountsCalculationsSuspender
		{
			get { return amountsCalculationsSuspender ?? (amountsCalculationsSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender amountsCalculationsSuspender;

		bool IsInCollectionOfType<T>() where T : BusinessObjectCollection
		{
			bool result = false;

			if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
			{
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					if (collection is T)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public bool HasNumberFountain => NumberFountainForTransactionNumber != null;

		#endregion

		public ZBool IsCreatedFromApprovalRequest
		{
			get; set;
		}

		#region IsTaxReportable

		public bool IsTaxReportable => IsTaxReportableCore;

		protected virtual bool IsTaxReportableCore => false;

		#endregion

		#region IReversing Members

		public bool IsReverseTransaction
		{
			get { return fIsReverseTransaction; }
			set { fIsReverseTransaction = value; }
		}
		protected bool fIsReverseTransaction;

		public TransactionHeader OriginalTransaction
		{
			get { return fOriginalTransaction; }
			set { fOriginalTransaction = value; }
		}
		protected TransactionHeader fOriginalTransaction;

		public TransactionHeader PairTransaction
		{
			get { return fPairTransaction; }
			set { fPairTransaction = value; }
		}
		TransactionHeader fPairTransaction;

		public bool IsReversing
		{
			get { return fIsReversing; }
		}
		protected bool fIsReversing;

		public bool IsReversed
		{
			get { return AH_IsCancelled; }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			GenerateReverseTransactionCore(mustTransform);
		}

		protected virtual Type TypeOfReverseTransaction
		{
			get { return this.GetType(); }
		}

		protected virtual Type TypeOfTransaction
		{
			get { return this.GetType(); }
		}

		protected virtual bool InvertSignsOfOriginalTransactionOnReversing
		{
			get { return true; }
		}

		protected virtual void GenerateReverseTransactionCore(bool mustTransform)
		{
			int reverseTransactionMultiplier = InvertSignsOfOriginalTransactionOnReversing ? -1 : 1;

			fIsReversing = true;

			fReverseTransaction = (TransactionHeader)Factory.New(TypeOfReverseTransaction);
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;

			using (fReverseTransaction.GetValidationSuspender())
			using (fReverseTransaction.AmountsCalculationsSuspender.GetSuspender())
			{
				fReverseTransaction.AH_OH = this.AH_OH;
				fReverseTransaction.AH_AB = this.AH_AB; // must be set after AH_OH
				fReverseTransaction.AH_RX_NKTransactionCurrency = this.AH_RX_NKTransactionCurrency;
				fReverseTransaction.AH_TransactionCategory = this.AH_TransactionCategory;
				fReverseTransaction.AH_AG = this.AH_AG;
				fReverseTransaction.AH_GB = this.AH_GB;
				fReverseTransaction.AH_GE = this.AH_GE;
				fReverseTransaction.AH_JH = this.AH_JH;
				fReverseTransaction.AH_PlaceOfSupply = this.AH_PlaceOfSupply;
				fReverseTransaction.AH_CashBasisGSTIndicator = this.AH_CashBasisGSTIndicator;
				fReverseTransaction.AH_ChequeDrawer = this.AH_ChequeDrawer;
				fReverseTransaction.AH_ChequeOrReference = this.AH_ChequeOrReference;
				fReverseTransaction.AH_DrawerBank = this.AH_DrawerBank;
				fReverseTransaction.AH_DrawerBranch = this.AH_DrawerBranch;
				fReverseTransaction.AH_InvoiceTerm = this.AH_InvoiceTerm;
				fReverseTransaction.AH_InvoiceTermDays = this.AH_InvoiceTermDays;
				fReverseTransaction.AH_ReceiptType = this.AH_ReceiptType;
				fReverseTransaction.AH_ExchangeRate = this.AH_ExchangeRate;
				fReverseTransaction.AH_AgreedPaymentMethodOverride = this.AH_AgreedPaymentMethodOverride;
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Peru // special rules when reversing Peru AP INV/CRD
					&& AH_Ledger == LedgerTypes.AccountsPayable
					&& (AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote))
				{
					switch (this.AH_ComplianceSubType)
					{
						case PeruComplianceInfo.ComplianceSubTypeCodes.TXI:
							fReverseTransaction.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
							break;
						case PeruComplianceInfo.ComplianceSubTypeCodes.TCR:
							fReverseTransaction.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
							break;
						case PeruComplianceInfo.ComplianceSubTypeCodes.TCD:
							fReverseTransaction.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
							break;
						case PeruComplianceInfo.ComplianceSubTypeCodes.DSB:
							fReverseTransaction.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
							break;
						default:
							fReverseTransaction.AH_ComplianceSubType = this.AH_ComplianceSubType;
							break;
					}
				}

				fReverseTransaction.AH_OSExTaxAmount = this.AH_OSExTaxAmount * reverseTransactionMultiplier;
				fReverseTransaction.AH_OSTaxAmount = this.AH_OSTaxAmount * reverseTransactionMultiplier;
				fReverseTransaction.AH_OSTotalAmount = this.AH_OSTotalAmount * reverseTransactionMultiplier;
				fReverseTransaction.AH_LocalExTaxAmount = this.AH_LocalExTaxAmount * reverseTransactionMultiplier;
				fReverseTransaction.AH_LocalTaxAmount = this.AH_LocalTaxAmount * reverseTransactionMultiplier;
				fReverseTransaction.AH_LocalOutstandingAmount = AH_LocalTotalAmount * reverseTransactionMultiplier;
				fReverseTransaction.AH_DueDate = Env.Time.CurrentLocalDateTime;
				fReverseTransaction.AH_OA_InvoiceAddressOverride = AH_OA_InvoiceAddressOverride;
				fReverseTransaction.AH_OC_InvoiceContactOverride = AH_OC_InvoiceContactOverride;
				fReverseTransaction.AH_GB_TaxBranch = AH_GB_TaxBranch;
			}
		}

		public IReversing ReverseTransaction
		{
			get { return fReverseTransaction; }
		}
		protected TransactionHeader fReverseTransaction;

		public void SetCancellationFlag(bool isCancelled)
		{
			AH_IsCancelled = new ZBool(isCancelled);
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
			SetTransactionBelongsToGroupFieldCore(groupingGuidValue);
		}

		protected virtual void SetTransactionBelongsToGroupFieldCore(ZGuid groupingGuidValue)
		{
			fReverseTransaction.AH_TransactionBelongsToGroup = this.PK;
		}

		public ZBool IsAmendingOrReversal
		{
			get
			{
				return IsReversalTransaction || IsAmendingTransaction;
			}
		}

		public ZBool IsReversalTransaction
		{
			get
			{
				IReversing reversing = this;
				return AH_TransactionBelongsToGroup.IsValid && reversing != null && reversing.IsReversed;
			}
		}

		public ZBool IsAmendingWithARCreditNote
		{
			get
			{
				var arCreditNote = this as ARCreditNote;
				return arCreditNote != null && !IsInDatabase && IsAmendingTransaction;
			}
		}

		public ZBool IsAmendingTransaction
		{
			get
			{
				IAmending amending = this as IAmending;
				return amending != null && amending.IsAmendingTransaction;
			}
		}

		public ZPropertyInfo IsAmendingOrReversalInfo
		{
			get { return GetZPropertyInfo(nameof(IsAmendingOrReversal)); }
		}

		protected override TransactionCreatingMode? GetTransactionCreatingMode()
			=> IsReversalTransaction
			? TransactionCreatingMode.Reversal
			: IsAmendingTransaction
			? TransactionCreatingMode.Amending
			: TransactionCreatingMode.Original;

		internal bool IsCashAccountType => BankAccount != null && BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CSH;

		internal bool IsCashReceiptType => AH_ReceiptType == ReceiptTypes.Cash;

		public void SetDescription(ZString descriptionToSet)
		{
			SetDescriptionCore(descriptionToSet);
		}

		protected virtual void SetDescriptionCore(ZString descriptionToSet)
		{
			AH_Desc = descriptionToSet;
		}
		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
			SetNumberOfSupportingDocumentsCore(numberOfSupportingDocumentsToSet);
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
			ApplyWorkflowTemplatesOnReverseTransactionCore();
		}

		protected virtual void ApplyWorkflowTemplatesOnReverseTransactionCore()
		{
		}

		protected virtual void SetNumberOfSupportingDocumentsCore(ZByte numberOfSupportingDocumentsToSet)
		{
			AH_NumberOfSupportingDocuments = numberOfSupportingDocumentsToSet;
		}
		public ZString ReversingReason
		{
			get { return fReversingReason; }
			set
			{
				AH_Desc = new ZString(AH_Desc + " " + value).Left(AH_DescInfo.MaxLength);
				fReversingReason = value;
			}
		}
		protected ZString fReversingReason;

		public ZString ReversingCode
		{
			get { return fReversingCode; }
			set
			{
				fReversingCode = value;
				if (AH_TransactionType == TransactionTypes.CreditNote ||
					AH_TransactionType == TransactionTypes.Invoice ||
					AH_TransactionType == TransactionTypes.AdjustmentNote)
				{
					AH_ReceiptType = value.Left(AH_ReceiptTypeInfo.MaxLength);
				}
			}
		}
		protected ZString fReversingCode;

		public bool IsClearedInCashbook
		{
			get { return !AH_DateClearedInCashbook.IsEmpty; }
		}

		public string[] MultipleReversingErrors
		{
			get { return MultipleReversingErrors_innerValue; }
			set { MultipleReversingErrors_innerValue = value; }
		}
		string[] MultipleReversingErrors_innerValue = Array.Empty<string>();

		public bool IsAmendingCreditNote
			=> TransactionType == TransactionTypes.CreditNote && this is IAmending amending && amending.IsAmendingTransaction;

		public bool IsAmendingInvoice
			=> TransactionType == TransactionTypes.Invoice && this is IAmending amending && amending.IsAmendingTransaction;

		#endregion

		#region IeNettTransaction Members

		RefCurrency IeNettTransaction.Currency
		{
			get { return TransactionCurrency; }
		}

		#endregion

		#region IDataExportBatchSource Members

		ZBool IDataExportBatchSource.IsDataExportBatchSupported
		{
			get
			{
				return AH_Ledger == LedgerTypes.AccountsPayable ||
					AH_Ledger == LedgerTypes.AccountsReceivable ||
					(AH_Ledger == LedgerTypes.CashBook && AH_TransactionType != TransactionTypes.DDRBatch) ||
					AH_Ledger == LedgerTypes.General ||
					AH_Ledger == LedgerTypes.JobCosting ||
					AH_Ledger == LedgerTypes.TransactionsPendingAllocation;
			}
		}

		public DataExportBatchDependentCollection DataExportBatchCollection
		{
			get
			{
				if (relatedBatchCollection == null)
				{
					relatedBatchCollection = new DataExportBatchDependentCollection(this);
					relatedBatchCollection.Load();
				}
				return relatedBatchCollection;
			}
		}

		DataExportBatchDependentCollection relatedBatchCollection;

		#endregion

		#region ITransaction Members

		ZString ITransaction.Ledger
		{
			get { return AH_Ledger; }
		}

		ZPropertyInfo ITransaction.LedgerInfo
		{
			get { return AH_LedgerInfo; }
		}

		ZString ITransaction.CurrencyCode
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		ZPropertyInfo ITransaction.CurrencyCodeInfo
		{
			get { return AH_RX_NKTransactionCurrencyInfo; }
		}

		ZDecimal ITransaction.OverseasTotalAmount
		{
			get { return AH_OSTotalAmount; }
		}

		ZPropertyInfo ITransaction.OverseasTotalAmountInfo
		{
			get { return AH_OSTotalAmountInfo; }
		}

		ZDateTime ITransaction.PostDate
		{
			get { return AH_PostDate; }
			set { AH_PostDate = value; }
		}

		ZPropertyInfo ITransaction.PostDateInfo
		{
			get { return AH_PostDateInfo; }
		}

		ZDateTime ITransaction.TransactionDate
		{
			get { return AH_InvoiceDate; }
			set { AH_InvoiceDate = value; }
		}

		ZPropertyInfo ITransaction.TransactionDateInfo
		{
			get { return AH_InvoiceDateInfo; }
		}

		ZString ITransaction.SupportingDocumentNumber
		{
			get { return SupportingDocumentNumber; }
			set
			{
				SupportingDocumentNumber = value;
			}
		}

		ZPropertyInfo ITransaction.SupportingDocumentNumberInfo
		{
			get { return SupportingDocumentNumberInfo; }
		}

		ZString ITransaction.TransactionNumber
		{
			get { return AH_TransactionNum; }
			set
			{
				AH_TransactionNum = value;
			}
		}

		ZPropertyInfo ITransaction.TransactionNumberInfo
		{
			get { return AH_TransactionNumInfo; }
		}

		ZString ITransaction.TransactionType
		{
			get { return AH_TransactionType; }
		}

		ZPropertyInfo ITransaction.TransactionTypeInfo
		{
			get { return AH_TransactionTypeInfo; }
		}

		ZGuid ITransaction.Organization
		{
			get { return AH_OH; }
			set { AH_OH = value; }
		}

		ZPropertyInfo ITransaction.OrganizationInfo
		{
			get { return AH_OHInfo; }
		}

		public ZString OriginalTransactionNumber
		{
			get { return OriginalTransaction != null ? OriginalTransaction.AH_TransactionNum : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionNumber)); }
		}

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public ZString OriginalTransactionType
		{
			get { return OriginalTransaction != null ? OriginalTransaction.AH_TransactionType : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionType)); }
		}

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		OrgHeaderCollection ITransaction.Headers
		{
			get { return Lookups.Headers; }
		}

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set
			{
				SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value);
				if (HeaderValidation != null && !IsValidationSuspended)
				{
					HeaderValidation.ValidateUnmatchDate();
				}
			}
		}
		ZDateTime unmatchDate;

		public bool UnmatchDate_ReadOnly
		{
			get
			{
				var thisAsUnmatchOnReversing = this as IUnmatchOnReversing;
				return thisAsUnmatchOnReversing == null || !thisAsUnmatchOnReversing.UnmatchingData.WasUnmatched || !thisAsUnmatchOnReversing.UnmatchingData.AllowBackPosting;
			}
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(Schema.UnmatchDate); }
		}

		#region ReversalStatusCode

		public virtual ZString ReversalStatusCode
		{
			get { return reversalStatusCode; }
			set
			{
				SetNonPersistentPropertyValue(ReversalStatusCodeInfo, ref reversalStatusCode, value);
			}
		}
		ZString reversalStatusCode;

		public ZPropertyInfo ReversalStatusCodeInfo => GetZPropertyInfo(nameof(ReversalStatusCode));

		bool ITransaction.ReversalStatusCode_ReadOnly => true;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => null;

		#endregion ReversalStatusCode

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return CopyTransaction();
		}

		protected virtual TransactionHeader CopyTransaction()
		{
			return Factory.New(this.GetType()) as TransactionHeader;
		}

		#endregion

		#region IPreviewInvoiceEnabled Members

		public bool IsInPreviewingInvoicesContext { get; set; }

		#endregion

		#region Document Printing

		void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			OnDocumentPrinted(sender, e);
		}

		protected virtual void OnDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			UpdatePrintedFlag();
		}

		protected void UpdatePrintedFlag(BusinessObjectFactory factory = null)
		{
			if (Globals.IsUserInteractive)
			{
				factory = new BusinessObjectFactory();
			}
			else
			{
				factory = Factory;
			}
			var invoice = factory.Load<TransactionHeader>(PK);
			invoice.AH_InvoicePrinted = true;

			if (Globals.IsUserInteractive)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
			}
		}

#if DEBUG
		internal void UpdatePrintedFlagExposed_TestOnly(BusinessObjectFactory factory = null)
		{
			UpdatePrintedFlag(factory);
		}
#endif

		#endregion

		#region IDocumentSupportable Members
		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new TransactionHeaderDocumentSupporter(this); }
		}
		#endregion

		#region TransactionHeaderDocumentSupporter
		public class TransactionHeaderDocumentSupporter : DocumentSupporter
		{
			public TransactionHeaderDocumentSupporter(TransactionHeader transactionHeader)
				: base(transactionHeader)
			{
			}

			protected TransactionHeader TransactionHeader
			{
				get { return (TransactionHeader)BusinessObject; }
			}

			#region Overrides

			void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				TransactionHeader.OnDocumentPrinted(sender, e);
			}

			protected override void InitialiseCore(IDocumentEvents documentEventSource)
			{
				base.InitialiseCore(documentEventSource);
				documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get
				{
					if (TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
					{
						return Env.Security.PayablesCustomiseDocuments;
					}
					else
					{
						return Env.Security.ReceivablesCustomiseDocuments;
					}
				}
			}

			public override CargoWise.Definitions.BusinessContext BusinessContext
			{
				get { return CargoWise.Definitions.BusinessContext.ARTransaction; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
				{
					return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, TransactionHeader);
				}
				else if (dataContext == Constants.DataContext.TransactionHeader || dataContext == Constants.DataContext.Cheques)
				{
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.TransactionHeader, TransactionHeader) };
				}
				else if (dataContext == Constants.DataContext.AccountingVoucher && ShouldSupportAccountingVoucher())
				{
					var transactionheader = TransactionHeaderHelper.ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(new TransactionHeader[] { TransactionHeader })[0];
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.AccountingVoucher, transactionheader) };
				}
				else
				{
					return null;
				}
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				List<DataContext> result = new List<DataContext> { Constants.DataContext.TransactionHeader, Constants.DataContext.Cheques, Constants.DataContext.CASSBilling, Constants.DataContext.GenericFreightJob };
				if (ShouldSupportAccountingVoucher())
				{
					result.Add(Constants.DataContext.AccountingVoucher);
				}
				return result.ToArray();
			}

			public bool ShouldSupportAccountingVoucher()
			{
				if ((GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
					 && VoucherProviderFactory.SupportList.Contains(TransactionHeader.AH_TransactionType))
				{
					return true;
				}
				return false;
			}

			#endregion

		}
		#endregion

		#region Miscalaneous Transactiuon Readonly Properties

		public virtual bool AH_RX_NKTransactionCurrency_ReadOnly { get; set; }

		public bool IsMiscellaneousTransaction
		{
			get { return isMiscellaneousTransaction; }
			set
			{
				isMiscellaneousTransaction = value;
				AH_OHInfo.RefreshBinding();
			}
		}
		bool isMiscellaneousTransaction;

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return base.AH_TransactionNum_ReadOnly || IsMiscellaneousTransaction; }
		}

		protected override bool AH_InvoiceDate_ReadOnly
		{
			get { return base.AH_InvoiceDate_ReadOnly || IsMiscellaneousTransaction; }
		}

		protected override bool AH_PostDate_ReadOnly
		{
			get { return base.AH_PostDate_ReadOnly || (!AllowBackPosting && !AllowFuturePosting) || IsMiscellaneousTransaction; }
		}

		protected virtual bool AH_DueDate_ReadOnly
		{
			get { return IsMiscellaneousTransaction; }
		}

		#endregion

		#region Binding AuthorisationLevel properies

		#region AuthorisationLevel

		public ZString AuthorisationLevel
		{
			get
			{
				return AuthorisationLevelCore();
			}
		}

		public ZPropertyInfo AuthorisationLevelInfo
		{
			get { return GetZPropertyInfo(nameof(AuthorisationLevel), "Variance Approval Level"); }
		}

		protected virtual ZString AuthorisationLevelCore()
		{
			return Res.GetString("1b36a35a-eff3-4d62-8e35-5f5e710c95f0", "Does Not Apply");
		}

		#endregion

		#region MaxAuthorisationLevel

		public static ZString GetMaxCompanyAuthorizationLevel(ZString glbCompanyCode)
		{
			ZString result = ZString.Empty;
			foreach (IntercompanyPostingConfiguration config in AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.Value)
			{
				if (config.Company == glbCompanyCode)
				{
					result = config.MaxCostVarianceApprovalLevel;
				}
			}
			return result;
		}

		public ZString MaxAuthorisationLevel
		{
			get
			{
				return MaxAuthorisationLevelCore();
			}
		}

		public ZPropertyInfo MaxAuthorisationLevelInfo
		{
			get { return GetZPropertyInfo(nameof(MaxAuthorisationLevel), "Maximum Variance Approval Level"); }
		}

		protected virtual ZString MaxAuthorisationLevelCore()
		{
			return Res.GetString("1b36a35a-eff3-4d62-8e35-5f5e710c95f0", "Does Not Apply");
		}

		#endregion

		#endregion

		#region Bindable (Billing -> AR Invoice tab) Properties

		public void PopulateExtraProperties()
		{
			try
			{
				if (TransactionPaymentStatus != null)
				{
					outstandingAmountBindable = TransactionPaymentStatus.OutstandingAmount(AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_ConsolidatedInvoiceRef);
					paymentStatus = TransactionPaymentStatus.PaymentStatus(AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_ConsolidatedInvoiceRef);
					DateTime? date = TransactionPaymentStatus.FullyPaidDate(AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_ConsolidatedInvoiceRef);
					fullyPaidDateBindable = date.HasValue ? new ZDateTime(date.Value) : ZDateTime.Empty;
				}
			}
			catch (InvalidOperationException)
			{
				throw;
			}
		}

		#region OutstandingAmountBindable

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal OutstandingAmountBindable
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value)
				{
					return outstandingAmountBindable;
				}
				return AH_OutstandingAmount;
			}
		}
		ZDecimal outstandingAmountBindable;

		public ZPropertyInfo OutstandingAmountBindableInfo
		{
			get { return GetZPropertyInfo(nameof(OutstandingAmountBindable)); }
		}

		#endregion

		#region PaymentStatus
		[MaxLength(20)]
		public ZString PaymentStatus
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value)
				{
					return paymentStatus;
				}

				ZString databasePaymentStatus;
				if (AH_OutstandingAmount == 0)
				{
					databasePaymentStatus = "PAID";
				}
				else if (AH_LocalTotal == AH_OutstandingAmount)
				{
					databasePaymentStatus = "UNPAID";
				}
				else
				{
					databasePaymentStatus = "PARTPAID";
				}

				return databasePaymentStatus;
			}
		}
		ZString paymentStatus = ZString.Empty;

		public ZPropertyInfo PaymentStatusInfo
		{
			get { return GetZPropertyInfo(nameof(PaymentStatus)); }
		}

		#endregion

		#region FullyPaidDateBindable

		public ZDateTime FullyPaidDateBindable
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value)
				{
					return fullyPaidDateBindable;
				}
				return AH_FullyPaidDate;
			}
		}
		ZDateTime fullyPaidDateBindable = ZDateTime.Empty;

		public ZPropertyInfo FullyPaidDateBindableInfo
		{
			get { return GetZPropertyInfo(nameof(FullyPaidDateBindable)); }
		}

		#endregion

		public ITransactionPaymentStatus TransactionPaymentStatus
		{
			get
			{
				if (transactionPaymentStatus == null && Header != null)
				{
					transactionPaymentStatus = ObjectFactory.Get<ITransactionPaymentStatusProvider>().GetTransactionPaymentStatus(Header.OH_Code);
				}
				return transactionPaymentStatus;
			}
		}
		ITransactionPaymentStatus transactionPaymentStatus;

		#endregion

		#region IeNettTransaction Members

		ZDecimal IeNettTransaction.ExchangeRate
		{
			get { return AH_ExchangeRate; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalExTaxAmount
		{
			get { return AH_LocalExTaxAmount; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalTaxAmount
		{
			get { return AH_LocalTaxAmount; }
		}

		ZDateTime IeNettTransaction.PostDate
		{
			get { return AH_PostDate; }
		}

		#endregion

		#region IHandleDeleteError Members

		bool IHandleDeleteError.RollbackAfterDeleteError
		{
			get { return IsInDatabase && (IsDeleted || !(IsCancelled && IsCancelledHasChanged)); }
		}

		bool IHandleDeleteError.RebindAfterDeleteError
		{
			get { return false; }
		}

		bool IHandleDeleteError.DisableFormOnDeleteConcurrencyError
		{
			get { return !((IHandleDeleteError)this).RollbackAfterDeleteError; }
		}

		#endregion

		#region EInvoicing Members

		[ResourceStringData("40D3BBDD-1294-4F5B-A00F-19F6CCEC1FE1", Caption = "E-Reporting Status")]
		public ZString EInvoicingStatus => EInvoicingProxy.CurrentStatus;

		public ZPropertyInfo EInvoicingStatusInfo => GetZPropertyInfo(Schema.EInvoicingStatus);

		[ResourceStringData("D8FEFC8B-3931-41E3-8BB0-9CF6ABDE71EC", ShortCaption = "E-Reporting Message", Caption = "E-Reporting Error/Warning/Status")]
		public ZString EInvoicingError => EInvoicingProxy.CurrentError;

		public ZPropertyInfo EInvoicingErrorInfo => GetZPropertyInfo(Schema.EInvoicingError);

		[ResourceStringData("3DAAC542-08AE-4B60-A573-CB8DEA7969CE", Caption = "E-Reporting Last Response Received (UTC)")]
		public ZDateTime EInvoicingLastResponseReceivedUtc => EInvoicingProxy.LastResponseReceivedUtc;

		public ZPropertyInfo EInvoicingLastResponseReceivedUtcInfo => GetZPropertyInfo(Schema.EInvoicingLastResponseReceivedUtc);

		[ResourceStringData("55994D43-88F8-4E45-AC31-F645BA520710", Caption = "E-Reporting Last Sent Time (UTC)")]
		public ZDateTime EInvoicingLastSentTimeUtc => EInvoicingProxy.LastSentTimeUtc;

		public ZPropertyInfo EInvoicingLastSentTimeUtcInfo => GetZPropertyInfo(Schema.EInvoicingLastSentTimeUtc);

		[ResourceStringData("46CB816E-8CE7-4e53-9B35-D70A8B80D653", Caption = "E-Reporting Batch")]
		public ZString EInvoicingBatchNumber => EInvoicingProxy.BatchNumber;

		public ZPropertyInfo EInvoicingBatchNumberInfo => GetZPropertyInfo(Schema.EInvoicingBatchNumber);

		[ResourceStringData("E3E56A42-879A-4740-B5D7-D72D9C62C549", Caption = "E-Reporting Govt #")]
		public ZString EInvoicingGovernmentAllocatedNumber => EInvoicingProxy.GovernmentAllocatedNumber;

		public ZPropertyInfo EInvoicingGovernmentAllocatedNumberInfo => GetZPropertyInfo(Schema.EInvoicingGovernmentAllocatedNumber);

		[ResourceStringData("FA729842-F3D2-4317-AE79-4B84D471C200", Caption = "E-Reporting eHub #")]
		public ZString EInvoicingeHubAllocatedNumber => EInvoicingProxy.eHubAllocatedNumber;

		public ZPropertyInfo EInvoicingeHubAllocatedNumberInfo => GetZPropertyInfo(Schema.EInvoicingeHubAllocatedNumber);

		[ResourceStringData("DE1B5820-5080-4C36-9995-99D93847D12A", ShortCaption = "E-Reporting Auth #", Caption = "E-Reporting Authorization #")]
		public ZString EInvoicingAuthorisationNumber => EInvoicingProxy.AuthorisationNumber;

		public ZPropertyInfo EInvoicingAuthorisationNumberInfo => GetZPropertyInfo(nameof(EInvoicingAuthorisationNumber));

		public ZDateTimeOffset EInvoicingAuthorisationDateTime => EInvoicingProxy.AuthorisationDateTime;

		public ZPropertyInfo EInvoicingAuthorisationDateTimeInfo => GetZPropertyInfo(Schema.EInvoicingAuthorisationDateTime);

		public ElectronicInvoicingTransactionProxy EInvoicingProxy
			=> eInvoicingProxy ?? (eInvoicingProxy = new ElectronicInvoicingTransactionProxy(this));

		ElectronicInvoicingTransactionProxy eInvoicingProxy;

		IEInvoicingTransaction EInvoicingTransaction
			=> EInvoicingTransactionValue ?? (EInvoicingTransactionValue = ObjectFactory.Get<IEInvoicingTransactionProxyFactory>().GetProxy(this));

		IEInvoicingTransaction EInvoicingTransactionValue;

		public bool IsEligibleToCreateEInvoicingTransactionPivot => EInvoicingProxy.IsEligibleToCreatePivot();

		public bool HasEReportingComplianceDateReached => EInvoicingProxy.HasEReportingComplianceDateReached();

		public AccEInvoicingTransactionPivot GetMostRecentEInvoicingTransactionPivot() => EInvoicingProxy.MostRecentPivot;

		public string ComplianceDocumentStatus
		{
			get
			{
				if (fTransactionHeaderReferenceECN == null)
				{
					fTransactionHeaderReferenceECN = GetTransactionHeaderReference(AccTransactionHeaderReferenceTypes.CDS);
				}

				var complianceDocumentStatusProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceDocumentStatusProvider(Company.GC_RN_NKCountryCode);
				var fComplianceDocumentStatus = fTransactionHeaderReferenceECN?.AH1_Reference ?? ZString.Empty;

				if (complianceDocumentStatusProvider != null && !string.IsNullOrEmpty(fComplianceDocumentStatus))
				{
					fComplianceDocumentStatus = complianceDocumentStatusProvider.GetComplianceDocumentStatus(fComplianceDocumentStatus);
				}

				return fComplianceDocumentStatus;
			}
		}
		AccTransactionHeaderReference fTransactionHeaderReferenceECN;

		/// <summary>
		/// Returns true if the transaction is posted before EInvoicing compliance date for this country,
		/// OR if a DCD pivot exists with specific error (transaction created when eInvoicing was manually disabled).
		/// </summary>
		public bool IsPreEInvoicingTransaction() => EInvoicingProxy.IsPreEInvoicingTransaction();

		public AccEInvoicingTransactionPivot EInvoicingTransactionPivotSubmitted
		{
			get
			{
				if (eInvoicingTransactionPivotSubmitted == null)
				{
					var query = new ZQuery();
					query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentID, PK);
					query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
					query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Submit);
					query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded);
					eInvoicingTransactionPivotSubmitted = Factory.LoadTop1<AccEInvoicingTransactionPivot>(query);
				}

				return eInvoicingTransactionPivotSubmitted;
			}
		}

		AccEInvoicingTransactionPivot eInvoicingTransactionPivotSubmitted;

		public static (bool pivotCreated, ZString errorReasonMessage) CreateEInvoicingPivotForRequest(BusinessObjectFactory factory, TransactionHeader transaction, string pivotActionType)
		{
			var pivotCreated = false;
			var errorReasonMessage = ZString.Empty;

			if (transaction.HasEReportingComplianceDateReached
				&& transaction.IsEligibleToCreateEInvoicingTransactionPivot
				&& transaction.EInvoicingProxy.MostRecentPivot != null
				&& transaction.IsInDatabase)
			{
				transaction.CreateNewEInvoicingPivot(factory, pivotActionType);
				pivotCreated = true;
			}
			else
			{
				errorReasonMessage = Res.GetString("06805B18-1B22-4520-819F-888D7A333BAD", "E-Reporting Compliance Date registry should be entered and E-Invoicing Functionality registry should be enabled.");
			}

			return (pivotCreated, errorReasonMessage);
		}

		public AccEInvoicingTransactionPivot CreateNewEInvoicingPivot(BusinessObjectFactory factory, string pivotActionType, string initialPivotStatus = EInvoicingPivotState.Queued)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrEmpty(pivotActionType, nameof(pivotActionType));
			Argument.NotNullOrEmpty(initialPivotStatus, nameof(initialPivotStatus));

			var transactionPivot = factory.New<AccEInvoicingTransactionPivot>();
			transactionPivot.AIP_ParentID = PK;
			transactionPivot.SetCompanyAndCountryCode(Company);
			transactionPivot.AIP_Status = initialPivotStatus;
			transactionPivot.AIP_ActionType = pivotActionType;
			return transactionPivot;
		}

		#endregion

		#region IImportExport Members

		public virtual Directions JobDirection
		{
			get { return Directions.Unknown; }
		}

		#endregion

		#region ITransactionHeader Members

		int ITransactionHeader.Multiplier
		{
			get { return Multiplier; }
		}

		#endregion

		#region Related GL Journals

		public List<GLJournal> RelatedGLJournals
		{
			get
			{
				if (fRelatedGLJournals == null)
				{
					if (this is InvoicingBase)
					{
						var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, PK);
						query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal);
						query.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);
						fRelatedGLJournals = Factory.Load<GLJournal>(query).ToList();
					}
					else
					{
						fRelatedGLJournals = new List<GLJournal>();
					}
				}
				return fRelatedGLJournals;
			}
		}
		List<GLJournal> fRelatedGLJournals;

		public bool IsPrePaymentTransaction => RelatedGLJournals.All(x => x.PostPeriod >= PostPeriod);

		#endregion

		#region ComplianceSequence

		public string ComplianceSequenceWithCodeAndDesc
		{
			get
			{
				return ComplianceSequence != null ? (ComplianceSequence.XD_Code + " - " + ComplianceSequence.XD_Description) : string.Empty;
			}
		}

		#endregion

		public bool IsUsedByActiveCollectionOrderLine
		{
			get
			{
				ZQuery query = new ZQuery(AccCollectionOrderLineSchema.AOL_AH, PK);
				query.AddToFilter(AccCollectionOrderLineSchema.AOL_IsCancelled, false);
				AccCollectionOrderLine[] lines = Factory.Load<AccCollectionOrderLine>(query);
				return lines.Length > 0;
			}
		}

		public bool OrgHeaderHasPaymentMethod(ZString paymentMethod, ZString currency)
		{
			bool result = false;
			if (Header != null && Header.CompanyData != null)
			{
				result = Header.CompanyData.ARAccountDetailsCollection.Cast<AccARAccountDetails>().Any(x => x.A1_PaymentMethod == paymentMethod
																										&& x.A1_IsDefaultAccount == ZBool.True
																										&& x.A1_RX_NKAccountCurrency == currency);
			}
			return result;
		}

		public FunctionalitySuspender DoNotValidateEmptyComplianceSubTypeSuspender
		{
			get { return doNotValidateEmptyComplianceSubTypeSuspender ?? (doNotValidateEmptyComplianceSubTypeSuspender = new FunctionalitySuspender()); }
		}

		FunctionalitySuspender doNotValidateEmptyComplianceSubTypeSuspender;

		public FunctionalitySuspender ValidateEmptyComplianceSequenceSuspender
		{
			get { return validateEmptyComplianceSequenceSuspender ?? (validateEmptyComplianceSequenceSuspender = new FunctionalitySuspender()); }
		}

		FunctionalitySuspender validateEmptyComplianceSequenceSuspender;

		public bool IsInstanceLedgerModified()
		{
			if (AH_Ledger != Ledger)
			{
				return true;
			}
			else
			{
				var newfactory = new BusinessObjectFactory();
				var txnInNewFactory = newfactory.Load<TransactionHeader>(PK);

				return txnInNewFactory != null && AH_Ledger != txnInNewFactory.AH_Ledger;
			}
		}

		public virtual ZDateTime InvoiceTaxDate => ZDateTime.Empty;

		public ZString PostedBy => GetPostedByCore();

		protected virtual ZString GetPostedByCore() => Logs.AddedLog?.SL_UserNameAndInitials ?? ZString.Empty;

		void RunInUserContextForCorrectCompany(Action method)
		{
			var newTempUsercontext = GetUserContextForCorrectCompany();
			if (newTempUsercontext != EnvProxy.Instance.CurrentUserContext)
			{
				using (EnvProxy.Instance.SetTemporaryUserContext(newTempUsercontext))
				{
					method();
				}
			}
			else
			{
				method();
			}
		}

		IUserContext GetUserContextForCorrectCompany()
		{
			IUserContext result = EnvProxy.Instance.CurrentUserContext;

			if (!IsDeleted && Branch != null && Branch.IsInDatabase && EnvProxy.Instance.CurrentCompany.PK != Branch.GB_GC)
			{
				Guid departmentPK = AH_GE.IsValid ? AH_GE.ToGuid() : result.Department.PK;

				var strategy = new DefaultErrorReportStrategy() { IsSilentReport = true };
				result = new UserContext(result.User.LoginName, Branch.PK.ToGuid(), departmentPK, strategy, factory: Factory);

				if (strategy.NotificationMessages.Count > 0)
				{
					throw new ZCannotSaveException(strategy.NotificationMessages[0], "Cannot Save the TransactionHeader");
				}
			}

			return result;
		}

		#region Amending transation for periodic invoice

		public (ZBool isAmendingTransactionForPeriodicInvoice, ZBool isSingleJob, JobHeader job) CheckIsAmendingTransactionForPeriodicInvoice()
		{
			var isAmendingTransactionForPeriodicInvoice = false;
			var isSingleJob = false;
			JobHeader job = null;

			if (IsAmendingTransaction && !AH_TransactionBelongsToGroup.IsEmpty)
			{
				var relatedTransaction = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, AH_TransactionBelongsToGroup));
				isAmendingTransactionForPeriodicInvoice = relatedTransaction != null && InvoiceTypeCalculationProvider.IsDeferredInvoiceType(relatedTransaction.TransactionCategory);

				if (isAmendingTransactionForPeriodicInvoice && this is TransactionHeaderWithLines headerWithLines)
				{
					var invoiceLines = headerWithLines.Lines.Cast<InvoicingLineBase>();
					var firstLine = invoiceLines.FirstOrDefault();
					if (firstLine != null && firstLine.Job != null)
					{
						isSingleJob = invoiceLines.All(x => x.AL_JH == firstLine.AL_JH);
						job = isSingleJob ? firstLine.Job : null;
					}
				}
			}

			return (isAmendingTransactionForPeriodicInvoice, isSingleJob, job);
		}

		#endregion

		#region IComplianceNumberResetStatusInputData

		ZGuid IComplianceNumberResetStatusInputData.CompanyPK => AH_GC;

		ZString IComplianceNumberResetStatusInputData.CountryCode => Company.GC_RN_NKCountryCode;

		ZDate IComplianceNumberResetStatusInputData.InvoiceDate => AH_InvoiceDate.Date;

		ZString IComplianceNumberResetStatusInputData.EInvoicingStatus => EInvoicingStatus;

		#endregion

#if DEBUG

		public void SetIsReversing(bool value)
		{
			fIsReversing = value;
		}

#endif
	}
}
