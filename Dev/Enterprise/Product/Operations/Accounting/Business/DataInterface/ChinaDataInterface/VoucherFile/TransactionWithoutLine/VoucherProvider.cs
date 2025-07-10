using System;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class VoucherProvider : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable, ISourceIdentifierProvider
	{
		public VoucherProvider(BusinessObjectFactory factory, IControlAccountProvider controlAccountProvider)
			: base(factory)
		{
			this.ControlAccountProvider = controlAccountProvider;
		}

		public VoucherProvider(AccTransactionHeader transaction, IControlAccountProvider controlAccountProvider)
			: base(transaction.Factory)
		{
			this.Transaction = transaction;
			this.ControlAccountProvider = controlAccountProvider;
		}

		#region Voucher Public Properties

		public virtual ZString CompanyName
		{
			get
			{
				return GetCompanyNameFromOrgProxy(Transaction?.Branch?.OrgProxy, Transaction?.Company?.OrgProxy);
			}
		}

		protected static ZString GetCompanyNameFromOrgProxy(OrgHeader branchOrgProxy, OrgHeader companyOrgProxy)
		{
			var result = ZString.Empty;

			if (branchOrgProxy != null)
			{
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.China:
						result = GetCompanyNameFromAddressByMainCapabilityAndLanguage(branchOrgProxy.Addresses, OrgAddressType.Receivables, Core.SharedConstants.Languages.ChineseSimplified);
						break;

					case Core.Constants.CountryCodes.Taiwan:
						result = GetCompanyNameFromAddressByMainCapabilityAndLanguage(branchOrgProxy.Addresses, OrgAddressType.Receivables, Core.SharedConstants.Languages.ChineseTraditional);
						break;
				}
			}

			if (result.IsEmpty && companyOrgProxy != null)
			{
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.China:
						result = GetCompanyNameFromAddressByMainCapabilityAndLanguage(companyOrgProxy.Addresses, OrgAddressType.Receivables, Core.SharedConstants.Languages.ChineseSimplified);
						break;

					case Core.Constants.CountryCodes.Taiwan:
						result = GetCompanyNameFromAddressByMainCapabilityAndLanguage(companyOrgProxy.Addresses, OrgAddressType.Receivables, Core.SharedConstants.Languages.ChineseTraditional);
						break;
				}
			}

			return !result.IsEmpty ? result : (companyOrgProxy?.OH_FullName ?? ZString.Empty);
		}

		static ZString GetCompanyNameFromAddressByMainCapabilityAndLanguage(OrgAddressDependentCollection addresses, OrgAddressType orgAddressType, ZString language)
		{
			var result = addresses.Cast<OrgAddress>()
					.FirstOrDefault(x => x.AddressCapability.GetCapabilityEnabled(orgAddressType) && x.AddressCapability.GetIsMainAddress(orgAddressType) && x.OA_Language == language)?.CompanyName ?? ZString.Empty;

			if (result.IsEmpty)
			{
				result = addresses.Cast<OrgAddress>()
					.FirstOrDefault(x => x.AddressCapability.GetCapabilityEnabled(orgAddressType) && x.AddressCapability.GetIsMainAddress(orgAddressType) && x.OA_Language == Core.SharedConstants.Languages.English)?.TranslatedAddresses.FirstOrDefault(y => y.OTA_Language == language)?.CompanyName ?? ZString.Empty;
			}

			return result;
		}

		public virtual ZString CompanyCode
		{
			get
			{
				return Transaction != null && Transaction.Company != null ? Transaction.Company.GC_Code : ZString.Empty;
			}
		}

		public virtual ZString BranchCode
		{
			get
			{
				return Transaction != null && Transaction.Branch != null ? Transaction.Branch.GB_Code : ZString.Empty;
			}
		}

		public virtual ZString DepartmentCode
		{
			get
			{
				return Transaction != null && Transaction.Department != null ? Transaction.Department.GE_Code : ZString.Empty;
			}
		}

		public virtual ZString CurrencyCode
		{
			get
			{
				return Transaction != null ? Transaction.AH_RX_NKTransactionCurrency : ZString.Empty;
			}
		}

		public virtual ZString TransactionType
		{
			get
			{
				return Transaction != null ? Transaction.AH_TransactionType : ZString.Empty;
			}
		}

		public virtual ZString TransactionNumber
		{
			get
			{
				return Transaction != null ? Transaction.AH_TransactionNum : ZString.Empty;
			}
		}

		public virtual ZString Ledger
		{
			get
			{
				return Transaction != null ? Transaction.AH_Ledger : ZString.Empty;
			}
		}

		public virtual ZBool IsReverseEntry
		{
			get
			{
				return Transaction != null && !Transaction.AH_TransactionBelongsToGroup.IsEmpty && Transaction.AH_IsCancelled;
			}
		}

		public virtual ZString BankCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (Transaction != null)
				{
					AccBankAccount bankAccount = Factory.Load<AccBankAccount>(Transaction.AH_AB);
					if (bankAccount != null && (Transaction.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Payment || Transaction.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Receipt || Transaction.AH_Ledger == ZArchitecture.Core.LedgerTypes.CashBook))
					{
						result = bankAccount.AB_Code;
					}
				}
				return result;
			}
		}

		public virtual ZString OrganisationCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (Transaction != null)
				{
					OrgHeader org = Factory.Load<OrgHeader>(Transaction.AH_OH);
					if (org != null)
					{
						result = org.OH_Code;
					}
				}
				return result;
			}
		}

		public virtual ZString OrganisationName
		{
			get
			{
				var org = Transaction?.Header;

				if (org == null)
				{
					return ZString.Empty;
				}

				foreach (OrgAddress address in org.Addresses)
				{
					if (address.IsMainAddressOfType(OrgAddressType.Receivables) && address.Language == Core.SharedConstants.Languages.ChineseSimplified && address.CompanyName != ZString.Empty)
					{
						return address.CompanyName;
					}
				}

				return org.OH_FullName;
			}
		}

		Guid BranchPK { get { return Transaction != null && Transaction.Branch != null ? Transaction.Branch.PK.ToGuid() : Guid.Empty; } }
		Guid CompanyPK { get { return Transaction != null && Transaction.Company != null ? Transaction.Company.PK.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid(); } }

		public virtual ZString PostedBy
		{
			get
			{
				if (Transaction != null && Transaction.Logs != null && Transaction.Logs.AddedLog != null && Transaction.Logs.AddedLog.User != null)
				{
					return Transaction.Logs.AddedLog.User.GS_FullName;
				}
				else
				{
					return Env.CurrentUser.FullName;
				}
			}
		}

		public virtual ZString EnterBy
		{
			get
			{
				if (Transaction != null && Transaction.Logs != null && Transaction.Logs.AddedLog != null && Transaction.Logs.AddedLog.User != null)
				{
					return Transaction.Logs.AddedLog.User.GS_FullName;
				}
				else
				{
					return Env.CurrentUser.FullName;
				}
			}
		}

		public virtual ZString Reviewer => !string.IsNullOrEmpty(Transaction?.AuditedBy?.GS_FullName) ? Transaction.AuditedBy.GS_FullName : AccountingConfigurationRegistry.Instance.GetVoucherAppointedPartiesDefault(CompanyPK, BranchPK, Guid.Empty, AccountingConstants.VoucherAppointedPartiesCode.VoucherReviewer);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual ZString Cashier
		{
			get
			{
				switch (TransactionType)
				{
					case TransactionTypes.Payment:
					case TransactionTypes.Receipt:
					case TransactionTypes.OpeningPayment:
					case TransactionTypes.OpeningReceipt:
					case TransactionTypes.DirectPayment:
					case TransactionTypes.DirectReceipt:
					case TransactionTypes.ExchangeDifference:
					case TransactionTypes.Transfer when Ledger == LedgerTypes.CashBook:
						return !string.IsNullOrEmpty(Transaction?.Cashier?.GS_FullName) ? Transaction.Cashier.GS_FullName : AccountingConfigurationRegistry.Instance.GetVoucherAppointedPartiesDefault(CompanyPK, BranchPK, Guid.Empty, AccountingConstants.VoucherAppointedPartiesCode.VoucherCasher);
					default:
						return ZString.Empty;
				}
			}
		}

		public virtual ZByte NumberOfSupportingDocuments
		{
			get { return (Transaction != null) ? Transaction.AH_NumberOfSupportingDocuments : (ZByte)0; }
		}

		public virtual ZDateTime PostDate
		{
			get
			{
				return (Transaction != null) ? Transaction.AH_PostDate : ZDateTime.Now;
			}
		}

		public virtual ZDateTime InvoiceDate
		{
			get
			{
				return Transaction != null ? Transaction.AH_InvoiceDate : ZDateTime.Empty;
			}
		}

		public virtual ZDateTime DueDate
		{
			get
			{
				return Transaction != null ? Transaction.AH_DueDate : ZDateTime.Empty;
			}
		}

		public virtual ZInt Period
		{
			get
			{
				if (fPeriod.IsEmpty)
				{
					if (Transaction != null)
					{
						AccountingPeriodCalculator fAccountingPeriodCalculator = new AccountingPeriodCalculator(Factory);
						AccPeriodManagement periodManagement = fAccountingPeriodCalculator.GetPeriodManagementFromDate(Transaction.AH_PostDate);
						fPeriod = periodManagement.AM_Period;
					}
				}
				return fPeriod;
			}
			set
			{
				fPeriod = value;
			}
		}
		ZInt fPeriod;

		public ZBool IncludeOrganisationCode
		{
			get
			{
				return UseDocBuilderAccountingVoucher ? ZBool.True : fIncludeOrganisationCode;
			}
			set
			{
				fIncludeOrganisationCode = value;
			}
		}
		ZBool fIncludeOrganisationCode;

		public ZBool IncludeJobNumber
		{
			get
			{
				return UseDocBuilderAccountingVoucher ? ZBool.True : fIncludeJobNumber;
			}
			set
			{
				fIncludeJobNumber = value;
			}
		}
		ZBool fIncludeJobNumber;

		public abstract VoucherLine[] VoucherLines
		{
			get;
#if DEBUG
			set;
#endif
		}

		public virtual VoucherLine[] VoucherLinesWithControllerPerLine
		{
			get
			{
				return VoucherLines;
			}
		}

		[DecimalPlaces(2)]
		public ZDecimal TotalDebitAmount => VoucherLines.Sum(x => x.DebitAmount);

		[DecimalPlaces(2)]
		public ZDecimal TotalCreditAmount => VoucherLines.Sum(x => x.CreditAmount);

		#endregion

		#region Implementation

		protected abstract ZGuid GetGLAccountPKFromTransactionHeader();
		protected abstract int MaxVoucherLineNo
		{
			get;
		}

		internal protected IControlAccountProvider ControlAccountProvider;
		public AccTransactionHeader Transaction;
		protected VoucherLine[] fVoucherLines;

		protected ZString GetLocalAccountNo(ZGuid accountPK)
		{
			return DataInterfaceUtils.GetLocalAccountNoWithNoTrailingZero(Factory, accountPK);
		}

		protected ZString GetLocalAccountDescription(ZGuid accountPK)
		{
			return DataInterfaceUtils.GetLocalAccountDescription(Factory, accountPK);
		}

		protected virtual ZDecimal GetControlCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetDebit();
		}

		protected virtual ZDecimal GetControlDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetCredit();
		}

		protected virtual ZDecimal GetControlOSCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetOSDebit();
		}

		protected virtual ZDecimal GetControlOSDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetOSCredit();
		}

		protected virtual ZDecimal GetCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetCredit();
		}

		protected virtual ZDecimal GetDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetDebit();
		}

		protected virtual ZDecimal GetOSCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetOSCredit();
		}

		protected virtual ZDecimal GetOSDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetOSDebit();
		}

		protected virtual ZGuid GetGLAccountPKFromControlAccount()
		{
			ControlAccountProvider.SetTransaction(Transaction);
			return ControlAccountProvider.PK;
		}

		protected virtual ZDateTime GetVoucherDate()
		{
			return Transaction.AH_PostDate;
		}

		protected virtual ZString GetVoucherDescription()
		{
			ZString headerDescription = VoucherDescriptionLookUp.GetDescription();
			ZString transactionLineDescription = VoucherDebitCreditLookUp.GetTransactionLineDescription();
			ZString jobNumber = VoucherDebitCreditLookUp.GetJobNumber();

			return DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, IncludeJobNumber);
		}

		protected virtual ZString GetTransactionLineDescription()
		{
			return VoucherDebitCreditLookUp.GetTransactionLineDescription();
		}

		protected virtual ZString GetOrganisationCode()
		{
			if (IncludeOrganisationCode)
			{
				return VoucherDebitCreditLookUp.GetOrganisationCode();
			}

			return ZString.Empty;
		}

		protected virtual ZString GetInvoiceNumber()
		{
			return VoucherDebitCreditLookUp.GetInvoiceNumber();
		}

		protected virtual ZString GetJobNumber()
		{
			return VoucherDebitCreditLookUp.GetJobNumber();
		}

		protected virtual ZString GetCurrencyCode()
		{
			return VoucherDebitCreditLookUp.GetCurrencyCode();
		}

		protected virtual ZDecimal GetForeignCurrencyAmount()
		{
			return VoucherDebitCreditLookUp.GetForeignCurrencyAmount();
		}

		protected virtual ZDecimal GetExchangeRate()
		{
			return VoucherDebitCreditLookUp.GetExchangeRate();
		}

		protected void SetControlAccountVoucherLine(VoucherLine voucherLine)
		{
			voucherLine.AccountPK = GetGLAccountPKFromControlAccount();
			voucherLine.AdditionalAccountDescription += GetOrganisationCode();
			voucherLine.VoucherType = VoucherTypeLookUp.GetVoucherType();
			voucherLine.VoucherNumber = VoucherNumberLookUp.GetVoucherNumber();
			voucherLine.DebitAmount = GetControlDebitAmount();
			voucherLine.CreditAmount = GetControlCreditAmount();
			voucherLine.OSDebitAmount = GetControlOSDebitAmount();
			voucherLine.OSCreditAmount = GetControlOSCreditAmount();
			voucherLine.VoucherDate = GetVoucherDate();
			voucherLine.Description = GetVoucherDescription();
			voucherLine.CurrencyCode = GetCurrencyCode();
			voucherLine.ForeignCurrencyAmount = GetForeignCurrencyAmount();
			voucherLine.ExchangeRate = GetExchangeRate();
			voucherLine.JobNumber = GetJobNumber();
			voucherLine.InvoiceNumber = GetInvoiceNumber();
		}

		protected void SetOriginalVoucherLine(VoucherLine voucherLine)
		{
			voucherLine.AccountPK = GetGLAccountPKFromTransactionHeader();
			voucherLine.VoucherType = VoucherTypeLookUp.GetVoucherType();
			voucherLine.VoucherNumber = VoucherNumberLookUp.GetVoucherNumber();
			voucherLine.DebitAmount = GetDebitAmount();
			voucherLine.CreditAmount = GetCreditAmount();
			voucherLine.OSDebitAmount = GetOSDebitAmount();
			voucherLine.OSCreditAmount = GetOSCreditAmount();
			voucherLine.VoucherDate = GetVoucherDate();
			voucherLine.Description = GetVoucherDescription();
			voucherLine.CurrencyCode = GetCurrencyCode();
			voucherLine.ForeignCurrencyAmount = GetForeignCurrencyAmount();
			voucherLine.ExchangeRate = GetExchangeRate();
			voucherLine.JobNumber = GetJobNumber();
			voucherLine.InvoiceNumber = GetInvoiceNumber();
		}

		protected void ResetAllLookUp()
		{
			fVoucherDebitCreditLookUp = null;
			fVoucherDescriptionLookUp = null;
			fVoucherNumberLookUp = null;
			fVoucherTypeLookUp = null;
		}

		protected VoucherDebitCreditLookUp VoucherDebitCreditLookUp
		{
			get
			{
				if (fVoucherDebitCreditLookUp == null)
				{
					fVoucherDebitCreditLookUp = new VoucherDebitCreditLookUp(Transaction);
				}
				return fVoucherDebitCreditLookUp;
			}
		}

		VoucherDebitCreditLookUp fVoucherDebitCreditLookUp;

		protected VoucherDescriptionLookUp VoucherDescriptionLookUp
		{
			get
			{
				if (fVoucherDescriptionLookUp == null)
				{
					fVoucherDescriptionLookUp = new VoucherDescriptionLookUp(Transaction);
				}
				return fVoucherDescriptionLookUp;
			}
		}

		VoucherDescriptionLookUp fVoucherDescriptionLookUp;

		protected VoucherNumberLookUp VoucherNumberLookUp
		{
			get
			{
				if (fVoucherNumberLookUp == null)
				{
					fVoucherNumberLookUp = new VoucherNumberLookUp(Transaction);
				}
				return fVoucherNumberLookUp;
			}
		}

		VoucherNumberLookUp fVoucherNumberLookUp;

		protected VoucherTypeLookUp VoucherTypeLookUp
		{
			get
			{
				if (fVoucherTypeLookUp == null)
				{
					fVoucherTypeLookUp = new VoucherTypeLookUp(Transaction);
				}
				return fVoucherTypeLookUp;
			}
		}

		VoucherTypeLookUp fVoucherTypeLookUp;

		#endregion

		#region IDocumentSupportable Members

		[XmlIgnore]
		public DocumentSupporter DocumentSupporter
		{
			get { return new VoucherProviderDocumentSupporter(this); }
		}

		#endregion

		#region ISourceIdentifierProvider members

		ZGuid ISourceIdentifierProvider.SourceIdentifier => Transaction?.PK ?? ZGuid.Empty;

		#endregion

		public class VoucherProviderDocumentSupporter : DocumentSupporter
		{
			public VoucherProviderDocumentSupporter(VoucherProvider voucherProvider)
				: base(voucherProvider)
			{
			}

			protected VoucherProvider VoucherProvider
			{
				get { return (VoucherProvider)BusinessObject; }
			}

			#region Overrides

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.None; }
			}

			public override BusinessContext BusinessContext
			{
				get { return VoucherProvider.UseDocBuilderAccountingVoucher ? BusinessContext.ARTransaction : BusinessContext.AccountingVoucher; }
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.AccountingVoucher };
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return VoucherProvider.UseDocBuilderAccountingVoucher
					? DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, VoucherProvider)
					: new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.AccountingVoucher, VoucherProvider) };
			}

			#endregion
		}

		protected bool UseDocBuilderAccountingVoucher
		{
			get
			{
				return DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.Value;
			}
		}
	}
}
