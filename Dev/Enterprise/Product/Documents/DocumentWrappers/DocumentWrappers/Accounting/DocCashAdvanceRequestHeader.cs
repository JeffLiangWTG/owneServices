using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocCashAdvanceRequestHeader : DocBaseWrapper, IGenericTransactionHeaderPlugIn
	{
		protected DocCashAdvanceRequestHeader(AccCashAdvanceRequestHeader cashAdvanceRequest, BusinessObjectFactory factory)
				: base(cashAdvanceRequest, factory)
		{
		}

		public static DocCashAdvanceRequestHeader New(AccCashAdvanceRequestHeader cashAdvanceRequest, BusinessObjectFactory factory)
		{
			return (cashAdvanceRequest != null) ? new DocCashAdvanceRequestHeader(cashAdvanceRequest, factory) : null;
		}

		#region IGenericTransactionPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocCashAdvanceRequestGenericHeaderTransactionSupporter(this)); }
		}
		DocCashAdvanceRequestGenericHeaderTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocCashAdvanceRequestGenericHeaderTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocCashAdvanceRequestGenericHeaderTransactionSupporter(DocCashAdvanceRequestHeader parent)
			{
				Parent = parent;
			}
			readonly DocCashAdvanceRequestHeader Parent;

			protected internal override ZDateTime GetCreatedDate()
			{
				return Parent.CreatedDateTimeLocal;
			}

			protected internal override ZString GetDocumenTitle()
			{
				return Parent.DocumentTitle;
			}

			protected internal override ZString GetTransactionType()
			{
				return Parent.TransactionType;
			}

			protected internal override ZString GetStatus()
			{
				return Parent.Status;
			}

			protected internal override ZString GetTransactionNumber()
			{
				return Parent.TransactionNumber;
			}

			protected internal override ZString GetCurrencyCode()
			{
				return Parent.Currency == null ? ZString.Empty : Parent.Currency.Code;
			}

			protected internal override DocOrganisation GetOrganisation()
			{
				return Parent.Organisation;
			}

			protected internal override ZString GetOrganisationCode()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Code;
			}

			protected internal override ZString GetOrganisationName()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Name;
			}

			protected internal override ZGuid GetOrganisationARAddressOrgAddressPK()
			{
				return Parent?.Organisation?.ARAddress?.OrgAddress?.PK ?? ZGuid.Empty;
			}

			protected internal override ZString GetJobNumber()
			{
				return Parent.JobNumber;
			}

			protected internal override ZString GetTaxId()
			{
				return Parent.TaxId;
			}

			protected internal override ZString GetMessage()
			{
				return Parent.Message;
			}

			protected internal override ZDecimal GetTotalOSAmount()
			{
				return Parent.OSAmount;
			}

			protected internal override ZDecimal GetTotalLocalAmount()
			{
				return Parent.LocalAmount;
			}

			protected internal override ZDecimal GetTotalLocalPaidAmount()
			{
				return Parent.LocalPaidAmount;
			}

			protected internal override ZDecimal GetTotalOSPaidAmount()
			{
				return Parent.OSPaidAmount;
			}

			protected internal override ZString GetReceiptBankAccountBSB()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.BSB;
			}

			protected internal override ZString GetReceiptBankAccountSWIFT()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.SWIFT;
			}

			protected internal override ZString GetReceiptBankAccountAccountNum()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.AccountNum;
			}

			protected internal override ZString GetReceiptBankAccountBankName()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.BankName;
			}

			protected internal override ZString GetReceiptBankAccountBankAddress()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.BankAddress;
			}

			protected internal override ZString GetMailToAddressWithCountry()
			{
				return Parent.MailToAddressWithCountry;
			}

			protected internal override DocGenericTransactionLineCollection GetLinesForCashAdvanceRequestHeader()
			{
				return Parent.GetLinesForCashAdvanceRequestHeaderCore();
			}
		}

		public ZString TransactionType => "CAH";  //needs to provide an implementation as required by the GenericTransactionHeaderSupporter

		public ZString TransactionNumber => CashAdvanceRequest.CAH_RequestReferenceNumber;

		public ZString Status => CashAdvanceRequest.CAH_Status;

		public ZString JobNumber => CashAdvanceRequest.JobNumber;

		public ZDateTime CreatedDateTimeLocal => CashAdvanceRequest.CreatedDateTimeLocal;

		public ZString Message => AccountingConfigurationRegistry.Instance.CashAdvanceRequestDocumentMessage.Value;

		public ZString DocumentTitle => AccountingConfigurationRegistry.Instance.CashAdvanceRequestDocumentTitle.Value;

		public DocCurrency Currency
		{
			get { return DocCurrency.New(CashAdvanceRequest.TransactionCurrency, Factory); }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(Factory, CashAdvanceRequest.Organization.PK); }
		}

		public ZString TaxId
		{
			get
			{
				ZString result = ZString.Empty;

				if (AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.Value && GlbCompany.CurrentCompany != null)
				{
					result = Res.GetString("1257CDE0-E598-4447-B46F-11C84486B6C3", "TAX #: {0}", GlbCompany.CurrentCompany.GC_BusinessRegNo);
				}

				return result;
			}
		}

		public DocBankAccount ReceiptBankAccount
		{
			get
			{
				var organizationPK = CashAdvanceRequest.CAH_OH_Organization;
				var currencyNK = CashAdvanceRequest.CAH_RX_NKTransactionCurrency;
				var bankAccount = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organizationPK, currencyNK, GlbBranch.CurrentBranch, Factory);
				return DocBankAccount.New(bankAccount, Factory);
			}
		}

		public ZString MailToAddressWithCountry
		{
			get
			{
				var branch = Factory.Load<GlbBranch>(CashAdvanceRequest.Job?.JH_GB ?? ZGuid.Empty) ?? GlbBranch.CurrentBranch;
				var docBranch = DocBranch.New(branch, Factory);
				ZString result = ZString.Empty;
				if (branch != null && docBranch.MailToAddress != null)
				{
					DocAddress docAddress = docBranch.MailToAddress;
					if (!BrandName.IsEmpty)
					{
						result = BrandName + "\n" + docAddress.PostalAddressExcludeName;
					}
					else
					{
						result = docAddress.PostalAddress;
					}
				}
				return result;
			}
		}

		public ZDecimal OSAmount
		{
			get { return CashAdvanceRequest.CAH_OSAmount; }
		}

		public ZDecimal OSPaidAmount
		{
			get { return CashAdvanceRequest.CAH_OSPaidAmount; }
		}

		public ZDecimal LocalAmount
		{
			get { return CashAdvanceRequest.CAH_LocalAmount; }
		}

		public ZDecimal LocalPaidAmount
		{
			get { return CashAdvanceRequest.CAH_LocalPaidAmount; }
		}

		protected DocGenericTransactionLineCollection GetLinesForCashAdvanceRequestHeaderCore()
		{
			var result = new DocGenericTransactionLineCollection(CashAdvanceRequest.Factory);
			foreach (AccCashAdvanceRequestLine line in CashAdvanceRequest.Lines)
			{
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}
			return result;
		}

		AccCashAdvanceRequestHeader CashAdvanceRequest
		{
			get { return WrappedObject as AccCashAdvanceRequestHeader; }
		}
	}
}
