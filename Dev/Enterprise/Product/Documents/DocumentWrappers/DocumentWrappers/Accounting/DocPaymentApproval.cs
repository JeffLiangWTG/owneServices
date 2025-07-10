using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocPaymentApproval : DocBaseWrapper, IDocPaymentVoucherWithAuthorisationSupport, IDocMachedTransaction, IGenericTransactionHeaderPlugIn //IGenericTransactionWrapper
	{
		protected DocPaymentApproval(PaymentApprovalWithAuthorisation approval, BusinessObjectFactory factory)
			: base(approval, factory)
		{
		}

		public static DocPaymentApproval New(PaymentApprovalWithAuthorisation approval, BusinessObjectFactory factory)
		{
			return (approval != null) ? new DocPaymentApproval(approval, factory) : null;
		}

		#region IGenericTransactionPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocPaymentApprovalGenericTransactionSupporter(this)); }
		}
		DocPaymentApprovalGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocPaymentApprovalGenericTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocPaymentApprovalGenericTransactionSupporter(DocPaymentApproval parent)
			{
				this.Parent = parent;
			}
			protected readonly DocPaymentApproval Parent;

			protected internal override ZString GetPaymentCurrencyCode()
			{
				return Parent.PaymentCurrency == null ? ZString.Empty : Parent.PaymentCurrency.Code;
			}

			protected internal override ZString GetTransactionType()
			{
				return Parent.TransactionType;
			}

			protected internal override ZDateTime GetInvoiceDate()
			{
				return Parent.InvoiceDate;
			}

			protected internal override ZDateTime GetPostDate()
			{
				return Parent.PostDate;
			}

			protected internal override ZDecimal GetTotalOSAmount()
			{
				return Parent.OSAmount;
			}

			protected internal override ZDecimal GetTotalLocalInvoiceAmount()
			{
				return Parent.Amount;
			}

			protected internal override ZString GetBankAccountCode()
			{
				return Parent.BankAccount == null ? ZString.Empty : Parent.BankAccount.Code;
			}

			protected internal override ZString GetOrganisationCode()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Code;
			}

			protected internal override ZString GetOrganisationName()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Name;
			}

			protected internal override ZString GetReceiptTypeDescription()
			{
				return Parent.ReceiptTypeDescription;
			}

			protected internal override ZString GetReceiptType()
			{
				return Parent.ReceiptType;
			}

			protected internal override ZString GetChequeOrReference()
			{
				return Parent.ChequeOrReference;
			}

			protected internal override ZString GetTransactionNumber()
			{
				return Parent.TransactionNumber;
			}

			protected internal override ZString GetDesc()
			{
				return Parent.Desc;
			}

			protected internal override ZDecimal GetMatchLinkInvertedOSAmount()
			{
				return Parent.MatchLink == null ? ZDecimal.Zero : Parent.MatchLink.InvertedOSAmount;
			}

			protected internal override ZString GetCurrencyCode()
			{
				return Parent.Currency == null ? ZString.Empty : Parent.Currency.Code;
			}

			protected internal override ZString GetCurrentCompanyCurrencyCode()
			{
				return Parent.CurrentCompany == null || Parent.CurrentCompany.Currency == null ? ZString.Empty : Parent.CurrentCompany.Currency.Code;
			}

			protected internal override ZDecimal GetMatchLinkAmount()
			{
				return Parent.MatchLink == null ? ZDecimal.Zero : Parent.MatchLink.Amount;
			}

			protected internal override ZDecimal GetExchangeRate()
			{
				return Parent.ExchangeRate;
			}

			protected internal override ZDecimal GetOSTotalForRemittanceAdvice()
			{
				return Parent.OSTotalForRemittanceAdvice;
			}

			protected internal override DocGenericTransactionLineCollection GetFlattenedPayments()
			{
				return Parent.GetFlattenedPaymentsCore();
			}

			protected internal override ZString GetBarcode()
			{
				return Parent.Barcode;
			}

			protected internal override ZDecimal GetTotalPaidAsForPaymentVoucher()
			{
				return Parent.OSTotalForRemittanceAdvice;
			}

			protected internal override ZDecimal GetTotalPaymentForPaymentVoucher()
			{
				return Parent.InvoiceAmountForPaymentVoucher;
			}

			protected internal override ZDecimal GetExchangeRateForPaymentVoucher()
			{
				return Parent.RemittanceExchangeRate;
			}

			protected internal override ZBool GetShowNewAuthorisationFooter()
			{
				return Parent.ShowNewAuthorisationFooter;
			}

			protected internal override ZString GetApprovalStatus()
			{
				return Parent.ApprovalStatus;
			}

			protected internal override ZString GetPreparedBy()
			{
				return Parent.PreparedBy;
			}

			protected internal override ZString GetFirstAuthorisationDescription()
			{
				return Parent.FirstAuthorisationDescription;
			}

			protected internal override ZString GetSecondAuthorisationDescription()
			{
				return Parent.SecondAuthorisationDescription;
			}

			protected internal override ZString GetThirdAuthorisationDescription()
			{
				return Parent.ThirdAuthorisationDescription;
			}

			protected internal override ZString GetFirstAuthorisation()
			{
				return Parent.FirstAuthorisation;
			}

			protected internal override ZString GetSecondAuthorisation()
			{
				return Parent.SecondAuthorisation;
			}

			protected internal override ZString GetThirdAuthorisation()
			{
				return Parent.ThirdAuthorisation;
			}

			protected internal override ZDecimal GetPaymentVoucherOSTotal()
			{
				return Parent.OSTotalForRemittanceAdvice;
			}
		}

		protected DocGenericTransactionLineCollection GetFlattenedPaymentsCore()
		{
			var result = new DocGenericTransactionLineCollection(Approval.Factory);

			foreach (FlattenedLine line in FlattenedPayments)
			{
				//result.Add(Line);
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}
			return result;
		}

		public ZString PaymentType
		{
			get { return Approval.AV_PaymentType; }
		}

		public ZString ReceiptType
		{
			get { return PaymentType; }
		}

		public ZString ReceiptTypeDescription
		{
			get
			{
				return DocTransactionHeader.GetReceiptTypeDescriptionFromReceiptType(PaymentType);
			}
		}

		public ZString ChequeOrReference
		{
			get { return Approval.AV_ChequeOrReference; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(Factory, Approval.Organisation); }
		}

		public DocBankAccount BankAccount
		{
			get { return DocBankAccount.New(Approval.BankAccount, Factory); }
		}

		public ZString TransactionType
		{
			get { return "UNA"; }
		}

		public ZDateTime InvoiceDate
		{
			get { return Approval.AV_PaymentDate; }
		}

		public ZDateTime PostDate
		{
			get => Approval.AV_PostDate;
		}

		public ZString TransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZString Desc
		{
			get { return Approval.AV_PaymentComment; }
		}

		public ZDecimal OSAmount
		{
			get { return Approval.AV_Amount; }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(Approval.PaymentCurrency, Factory); }
		}

		public ZDecimal ExchangeRate
		{
			get { return Approval.AV_PayExRate; }
		}

		public ZDecimal Amount
		{
			get { return Approval.AV_Calc_LocalAmount; }
		}

		PaymentApprovalWithAuthorisation Approval
		{
			get { return WrappedObject as PaymentApprovalWithAuthorisation; }
		}

		public DocPaymentApprovalItemCollection Payments
		{
			get
			{
				if (fPayments == null)
				{
					fPayments = new DocPaymentApprovalItemCollection(Factory);

					ZQuery approvalItemsQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, Approval.PK);
					PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(Factory, approvalItemsQuery);
					approvalItems.Load();

					foreach (PaymentApprovalItem item in approvalItems)
					{
						fPayments.Add(DocPaymentApprovalItem.New(item, Factory));
					}

					if (DSCTransaction != null)
					{
						fPayments.Add(DSCTransaction);
					}

					if (EXXTransaction != null)
					{
						fPayments.Add(EXXTransaction);
					}
				}

				return fPayments;
			}
		}

		DocPaymentApprovalItemCollection fPayments;

		public FlattenedLineCollection FlattenedPayments
		{
			get { return flattenedPayments ?? (flattenedPayments = new FlattenedLineCollection(Payments)); }
		}
		FlattenedLineCollection flattenedPayments;

		public class FlattenedLineCollection : DocBaseWrapperCollection
		{
			public FlattenedLineCollection(DocPaymentApprovalItemCollection parentCollection)
				: base(parentCollection.Factory)
			{
				foreach (DocPaymentApprovalItem header in parentCollection)
				{
					this.Add(FlattenedLine.New(header, Factory));
				}
			}
		}

		public class FlattenedLine : DocBaseWrapper, IGenericTransactionLinePlugIn
		{
			public static FlattenedLine New(DocPaymentApprovalItem header, BusinessObjectFactory factoryToWrap)
			{
				return new FlattenedLine(header);
			}

			FlattenedLine(DocPaymentApprovalItem header)
				: base(null, header.Factory)
			{
				this.Header = header;
			}

			public DocPaymentApprovalItem Header
			{
				get;
				private set;
			}

			public DocTransactionLine Line
			{
				get;
				set;
			}

			#region IGenericTransactionLinePlugIn members

			GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
			{
				get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocFlattenedLineGenericTransactionSupporter(this)); }
			}
			DocFlattenedLineGenericTransactionSupporter fGenericTransactionSupporter;

			#endregion

			class DocFlattenedLineGenericTransactionSupporter : GenericTransactionLineSupporter
			{
				public DocFlattenedLineGenericTransactionSupporter(FlattenedLine parent)
				{
					this.Parent = parent;
				}
				protected readonly FlattenedLine Parent;

				protected internal override DocPaymentApprovalItem GetFlattenedHeaderForPaymentApprovalItem()
				{
					return Parent.Header;
				}

				protected internal override DocTransactionLine GetFlattenedLine()
				{
					return Parent.Line;
				}
			}
		}

		public DocPaymentApprovalItem DSCTransaction
		{
			get
			{
				if (fDSCTransaction == null && Approval.AV_Discount != 0)
				{
					fDSCTransaction = DocPaymentApprovalItem.New();
					fDSCTransaction.TransactionType = ZArchitecture.Core.TransactionTypes.Discount;
					fDSCTransaction.OSAmount = Approval.AV_Discount;
					fDSCTransaction.Amount = Approval.AV_Discount;
					fDSCTransaction.ExchangeRate = 1M;
					fDSCTransaction.CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					fDSCTransaction.Desc = Res.GetString("a16ebb01-9972-4b78-9182-16f4c8cbfba3", "DISCOUNT");
					fDSCTransaction.PreparedBy = PreparedBy;
				}

				return fDSCTransaction;
			}
		}

		DocPaymentApprovalItem fDSCTransaction;

		public DocPaymentApprovalItem EXXTransaction
		{
			get
			{
				if (fEXXTransaction == null && Approval.AV_ExchangeDifference != 0)
				{
					fEXXTransaction = DocPaymentApprovalItem.New();
					fEXXTransaction.TransactionType = ZArchitecture.Core.TransactionTypes.ExchangeDifference;
					fEXXTransaction.OSAmount = Approval.AV_ExchangeDifference;
					fEXXTransaction.Amount = Approval.AV_ExchangeDifference;
					fEXXTransaction.ExchangeRate = 1M;
					fEXXTransaction.CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					fEXXTransaction.Desc = Res.GetString("6298d9b5-eecd-4f83-bd89-636591c756c2", "EXCHANGE DIFFERENCE");
					fEXXTransaction.PreparedBy = PreparedBy;
				}

				return fEXXTransaction;
			}
		}

		public DocPaymentApprovalItem fEXXTransaction;

		public ZDecimal InvoiceAmountForPaymentVoucher
		{
			get { return Approval.AV_Calc_LocalAmount; }
		}

		public ZDecimal RemittanceExchangeRate
		{
			get { return Approval.AV_PayExRate; }
		}

		public ZDecimal OSTotalForRemittanceAdvice
		{
			get { return Approval.AV_Amount; }
		}

		public DocCurrency PaymentCurrency
		{
			get { return DocCurrency.New(Approval.PaymentCurrency, Factory); }
		}

		public ZString PreparedBy
		{
			get { return Approval.Logs.CreatedByUserName; }
		}

		public ZString ApprovalStatus
		{
			get { return Approval.AV_Calc_DescriptionOfAuthorisationRequired; }
		}

		public ZString FirstAuthorisationDescription
		{
			get { return Res.GetString("1e96e6a9-d270-4301-b2ae-40756020bc61", "First Authorization ({0})", Approval.Level1AuthorisationStatus); }
		}

		public ZString SecondAuthorisationDescription
		{
			get { return Res.GetString("0a43a272-c45f-43c9-9290-0b5dada9c6dd", "Second Authorization ({0})", Approval.Level2AuthorisationStatus); }
		}

		public ZString ThirdAuthorisationDescription
		{
			get { return Res.GetString("94604954-8a4e-47c5-9cd9-040b07bc7099", "Third Authorization ({0})", Approval.Level3AuthorisationStatus); }
		}

		public ZString FirstAuthorisation
		{
			get { return (Approval != null && Approval.Approval1st != null) ? Approval.Approval1st.GS_FullName : ZString.Empty; }
		}

		public ZString SecondAuthorisation
		{
			get { return (Approval != null && Approval.Approval2nd != null) ? Approval.Approval2nd.GS_FullName : ZString.Empty; }
		}

		public ZString ThirdAuthorisation
		{
			get { return (Approval != null && Approval.Approval3rd != null) ? Approval.Approval3rd.GS_FullName : ZString.Empty; }
		}

		public ZBool ShowNewAuthorisationFooter
		{
			get { return ZBool.True; }
		}

		public ZString Barcode
		{
			get { return ""; }
		}

		public DocPaymentApprovalMatchLink MatchLink
		{
			get { return fMatchLink ?? (fMatchLink = DocPaymentApprovalMatchLink.New(this, Factory)); }
		}
		DocPaymentApprovalMatchLink fMatchLink;

		#region IDocPaymentVoucherWithAuthorisationSupport Members

		DocumentWrapperCollection IDocPaymentVoucherWithAuthorisationSupport.Payments
		{
			get { return Payments; }
		}

		#endregion
	}
}
