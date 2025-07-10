using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class InvoiceTermsChangedEmail : AccountingEmailDef
	{
		public InvoiceTermsChangedEmail(InvoicingBase invoice)
		{
			this.AH_TransactionType = invoice.AH_TransactionType;
			this.AH_TransactionNum = invoice.AH_TransactionNum;
			this.HeaderOHFullName = invoice.Header.OH_FullNameTruncated;

			this.AH_InvoiceTerm = invoice.AH_InvoiceTerm;
			this.AH_InvoiceTermDays = invoice.AH_InvoiceTermDays;
			this.AH_InvoiceDate = invoice.AH_InvoiceDate;

			var invoiceTerm = invoice.Header.CompanyData.GetARTerm(invoice.JobType, invoice.Direction, invoice.TransportMode, invoice.AH_GB, invoice.AH_GE, invoice.AH_TransactionCategory);
			if (!invoiceTerm.IsEmpty)
			{
				this.HeaderInvoiceTerm = invoiceTerm.Term;
				this.HeaderInvoiceTermDays = invoiceTerm.Days;
			}
		}

		#region Implementation

		protected ZString AH_TransactionType;
		protected ZString AH_TransactionNum;
		protected ZDateTime AH_InvoiceDate;
		protected ZString HeaderOHFullName;
		protected ZString AH_InvoiceTerm;
		protected ZString HeaderInvoiceTerm;
		protected ZByte AH_InvoiceTermDays;
		protected ZByte HeaderInvoiceTermDays;

		#region Overrides

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.DebtorCreditLimitNotifyGroup; }
		}

		protected override string GetBody()
		{
			return GetContent();
		}

		protected override string GetSubject()
		{
			return GetContent();
		}

		#endregion

		#region Helper functions

		internal string GetContent()
		{
			ZString changedDetail = "";

			if (IsTermChanged)
			{
				changedDetail = Res.GetString("51b709c4-be39-423e-b41a-8e40b384eb52", "invoice terms");
			}

			if (IsTermDaysChanged)
			{
				if (changedDetail.IsEmpty)
				{
					changedDetail = Res.GetString("a83fa2fb-a88c-42be-95f7-37d4e3e8a8d6", "invoice days");
				}
				else
				{
					changedDetail = Res.GetString("da47bdf5-8103-4cd7-a235-33a1cde565bf", "invoice terms and days");
				}
			}

			return Res.GetString("6da33cce-5bb2-49c3-9f8f-7c8147d2d1b5", "AR {0} {1} for {2} dated {3} has been posted by {4} with {5} different to the default {5} for {6}",
					TransactionNameFromCode(AH_TransactionType),
					AH_TransactionNum,
					HeaderOHFullName,
					AH_InvoiceDate.ToShortDateString(),
					Env.CurrentUser.FullName,
					changedDetail,
					HeaderOHFullName);
		}

		string TransactionNameFromCode(string transactionTypeCode)
		{
			CodeDescriptionPairList transactions = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
			return transactions.GetDescriptionFromCode(transactionTypeCode);
		}

		bool IsTermChanged
		{
			get
			{
				return AH_InvoiceTerm != HeaderInvoiceTerm;
			}
		}

		bool IsTermDaysChanged
		{
			get
			{
				return AH_InvoiceTermDays != HeaderInvoiceTermDays;
			}
		}

		#endregion
	}
}
#endregion