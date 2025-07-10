using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherLine : NonPersistentBusinessObject, IVoucherLine, IObsoleteValidation
	{
		public VoucherLine()
			: base(new BusinessObjectFactory())
		{
		}

		public VoucherLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public VoucherLine(AccTransactionHeader transactionHeader)
			: base(transactionHeader.Factory)
		{
			this.TransactionHeader = transactionHeader;
			SetVoucherLineDefaults();
		}

		public AccTransactionHeader TransactionHeader { get; private set; }

		#region ChargeCode
		public virtual ZString ChargeCode
		{
			get
			{
				return chargeCode;
			}
			set
			{
				chargeCode = value;
			}
		}
		ZString chargeCode;
		#endregion

		#region ChargeCodeLocalLanguageDescription
		public virtual ZString ChargeCodeLocalLangDesc
		{
			get
			{
				return chargeCodeLocalLangDesc;
			}
			set
			{
				chargeCodeLocalLangDesc = value;
			}
		}
		ZString chargeCodeLocalLangDesc;
		#endregion

		#region ChargeCodeDescription
		public virtual ZString ChargeCodeDesc
		{
			get
			{
				return chargeCodeDesc;
			}
			set
			{
				chargeCodeDesc = value;
			}
		}
		ZString chargeCodeDesc;
		#endregion

		#region BranchCode
		public ZString BranchCode
		{
			get
			{
				return fBranchCode;
			}
			set
			{
				fBranchCode = value;
			}
		}
		ZString fBranchCode;
		#endregion

		#region DepartmentCode
		public ZString DepartmentCode
		{
			get
			{
				return fDepartmentCode;
			}
			set
			{
				fDepartmentCode = value;
			}
		}
		ZString fDepartmentCode;
		#endregion

		#region ValidationError

		public ValidationInfo ValidationError
		{
			get;
			private set;
		}

		#endregion

		#region AccountPK
		public ZGuid AccountPK
		{
			get
			{
				return fAccountPK;
			}
			set
			{
				fAccountPK = value;
				fAccountNumber = DataInterfaceUtils.GetLocalAccountNo(Factory, fAccountPK);
				fGLAccountNumber = DataInterfaceUtils.GetGLAccountNoWithNoTrailingZero(Factory, fAccountPK);

				if (!fAccountNumber.IsEmpty)
				{
					fAdditionalAccountDescription = DataInterfaceUtils.GetLocalAccountDescription(Factory, fAccountPK);
				}
				else
				{
					fAdditionalAccountDescription = GetGLMappingMissingMessage(fAccountPK);
				}
			}
		}
		ZGuid fAccountPK;
		#endregion

		#region AccountNumber
		public ZString AccountNumber
		{
			get
			{
				return fAccountNumber;
			}
#if DEBUG
			set
			{
				fAccountNumber = value;
			}
#endif
		}
		ZString fAccountNumber;
		#endregion

		#region InvoiceNumber
		public ZString InvoiceNumber
		{
			get
			{
				return invoiceNumber;
			}
			set
			{
				invoiceNumber = value;
			}
		}
		ZString invoiceNumber;
		#endregion

		#region JobNumber
		public ZString JobNumber
		{
			get
			{
				return jobNumber;
			}
			set
			{
				jobNumber = value;
			}
		}
		ZString jobNumber;
		#endregion

		#region GLAccountNumber
		public ZString GLAccountNumber
		{
			get
			{
				return fGLAccountNumber;
			}
#if DEBUG
			set
			{
				fGLAccountNumber = value;
			}
#endif
		}
		ZString fGLAccountNumber;
		#endregion

		#region Appendix
		public ZString Appendix
		{
			get
			{
				return fAppendix;
			}
			set
			{
				fAppendix = value;
			}
		}
		ZString fAppendix;
		#endregion

		#region CreditAmount
		public ZDecimal CreditAmount
		{
			get
			{
				return fCreditAmount;
			}
			set
			{
				fCreditAmount = value;
			}
		}
		ZDecimal fCreditAmount;
		#endregion

		#region DebitAmount
		public ZDecimal DebitAmount
		{
			get
			{
				return fDebitAmount;
			}
			set
			{
				fDebitAmount = value;
			}
		}
		ZDecimal fDebitAmount;
		#endregion

		#region OSCreditAmount
		public ZDecimal OSCreditAmount
		{
			get
			{
				return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? CreditAmount : fOSCreditAmount;
			}
			set
			{
				fOSCreditAmount = value;
			}
		}
		ZDecimal fOSCreditAmount;
		#endregion

		#region OSDebitAmount
		public ZDecimal OSDebitAmount
		{
			get
			{
				return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? DebitAmount : fOSDebitAmount;
			}
			set
			{
				fOSDebitAmount = value;
			}
		}
		ZDecimal fOSDebitAmount;
		#endregion

		#region Description
		public ZString Description
		{
			get
			{
				return fDescription;
			}
			set
			{
				fDescription = value;
			}
		}
		ZString fDescription;
		#endregion

		#region VoucherDate
		public ZDateTime VoucherDate
		{
			get
			{
				return fVoucherDate;
			}
			set
			{
				fVoucherDate = value;
			}
		}
		ZDateTime fVoucherDate;
		#endregion

		#region VoucherNumberPrefix
		public ZString VoucherNumberPrefix
		{
			get
			{
				return fVoucherNumberPrefix;
			}
			set
			{
				fVoucherNumberPrefix = value;
			}
		}
		internal ZString fVoucherNumberPrefix;
		#endregion

		#region VoucherNumber
		public ZString VoucherNumber
		{
			get
			{
				return fVoucherNumber;
			}

			set
			{
				fVoucherNumber = value;
			}
		}
		internal ZString fVoucherNumber;
		#endregion

		#region VoucherType
		public ZString VoucherType
		{
			get
			{
				return fVoucherType;
			}
			set
			{
				fVoucherType = value;
			}
		}
		ZString fVoucherType;
		#endregion

		#region CurrencyCode
		public ZString CurrencyCode
		{
			get
			{
				return fCurrencyCode;
			}
			set
			{
				fCurrencyCode = value;
			}
		}
		ZString fCurrencyCode;
		#endregion

		#region ForeignCurrencyAmount
		public ZDecimal ForeignCurrencyAmount
		{
			get
			{
				return fForeignCurrencyAmount;
			}
			set
			{
				fForeignCurrencyAmount = value;
			}
		}
		ZDecimal fForeignCurrencyAmount;
		#endregion

		#region ExchangeRate
		public ZDecimal ExchangeRate
		{
			get
			{
				return (TransactionHeader != null && CurrencyCode == TransactionHeader.Company.GC_RX_NKLocalCurrency) || fExchangeRate <= 0 ? 1 : fExchangeRate;
			}
			set
			{
				fExchangeRate = value;
			}
		}
		ZDecimal fExchangeRate;
		#endregion

		#region AccountDescription
		public ZString AccountDescription
		{
			get
			{
				return fAccountDescription;
			}
			set
			{
				fAccountDescription = value;
			}
		}
		ZString fAccountDescription;
		#endregion

		#region AdditionalAccountDescription
		public ZString AdditionalAccountDescription
		{
			get
			{
				return fAdditionalAccountDescription;
			}
			set
			{
				fAdditionalAccountDescription = value;
			}
		}
		ZString fAdditionalAccountDescription;
		#endregion

		public ZString OrganisationCode { get; set; }
		public ZDecimal OutstandingAmount { get; set; }

		#region AttachmentCount
		public ZInt AttachmentCount
		{
			get
			{
				return fAttachmentCount;
			}
			set
			{
				fAttachmentCount = value;
			}
		}
		ZInt fAttachmentCount;
		#endregion

		#region IsDebit
		public ZBool IsDebit
		{
			get
			{
				return DebitAmount > 0 && CreditAmount == 0;
			}
		}
		#endregion

		#region IsCredit
		public ZBool IsCredit
		{
			get
			{
				return CreditAmount > 0 && DebitAmount == 0;
			}
		}
		#endregion

		public override string ToString()
		{
			return DataInterfaceConstant.Quote + DataInterfaceUtils.RemoveTrailingZero(AccountNumber) + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab +
				VoucherDate.ToString(DataInterfaceConstant.DateFormat) + DataInterfaceConstant.Tab +
				DebitAmount.ToString(2) + DataInterfaceConstant.Tab +
				CreditAmount.ToString(2) + DataInterfaceConstant.Tab +
				DataInterfaceConstant.Quote + VoucherType + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab +
				DataInterfaceConstant.Quote + VoucherNumber + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab +
				AttachmentCount + DataInterfaceConstant.Tab +
				DataInterfaceConstant.Quote + Description.Left(10) + DataInterfaceConstant.Quote;
		}

		public void Write(StreamWriter writer)
		{
			writer.WriteLine(ToString());
		}

		public bool ValidateLine()
		{
			bool hasNoError = true;
			ValidationError = null;

			if (AccountNumber.IsEmpty)
			{
				hasNoError = false;
				ValidationError = new ValidationInfo()
				{
					Message = GLAccountNumber,
					Key = VoucherLineErrorType.LocalGLNotMapped
				};
			}
			else if (Description.IsEmpty)
			{
				hasNoError = false;
				ValidationError = new ValidationInfo()
				{
					Message = Res.GetString("33EF484C-9887-45dd-90D1-FDFD29FCD3AD", "Voucher line of transaction '{0}' won't be created. Please check relative Transaction/Job Description.", VoucherNumber),
					Key = VoucherLineErrorType.NoVoucherDescription
				};
			}

			return hasNoError;
		}

		string GetGLMappingMissingMessage(ZGuid pK)
		{
			string message = "";

			AccGLHeader account = Factory.Load(typeof(AccGLHeader), pK) as AccGLHeader;
			if (account != null)
			{
				message = Res.GetString("6c89ec77-63e2-4aea-9fc9-18cbc5db8f43", "Error-No GL mapping for {0}", account.AG_AccountNum);
			}
			else
			{
				message = Res.GetString("082ec066-3b0a-45b0-a1e2-3e52bd21baf2", "Error-No GL Account found");
			}

			return message;
		}

		void SetVoucherLineDefaults()
		{
			fAttachmentCount = VoucherAttachmentNoLookUp.GetAttachmentNo();
		}

		VoucherAttachmentNoLookUp VoucherAttachmentNoLookUp
		{
			get
			{
				if (fVoucherAttachmentNoLookUp == null)
				{
					fVoucherAttachmentNoLookUp = new VoucherAttachmentNoLookUp(TransactionHeader);
				}
				return fVoucherAttachmentNoLookUp;
			}
		}

		VoucherAttachmentNoLookUp fVoucherAttachmentNoLookUp;
	}

	public class ValidationInfo
	{
		public ZString Message { get; set; }
		public VoucherLineErrorType Key { get; set; }
	}

	public enum VoucherLineErrorType
	{
		LocalGLNotMapped,
		NoVoucherDescription
	}
}
