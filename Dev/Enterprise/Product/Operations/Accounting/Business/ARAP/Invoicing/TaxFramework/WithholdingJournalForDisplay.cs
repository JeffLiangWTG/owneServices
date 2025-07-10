using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class WithholdingJournalForDisplay : NonPersistentBusinessObject
	{
		public WithholdingJournalForDisplay(APJournal parent, ZDate invoicePostDate)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
			InvoicePostDate = invoicePostDate;

			((IBindingList)Parent).ListChanged += Parent_ListChanged;
			PostDateInfo.AdditionalValidation += PostDateInfo_AdditionalValidation;
		}

		APJournal Parent { get; }
		ZDate InvoicePostDate { get; }
		public ZGuid JournalPK => Parent.PK;
		public ZString TransactionCategory => Parent.AH_TransactionCategory;
		public ZDate PostDate
		{
			get { return Parent.AH_PostDate.Date; }
			set
			{
				Parent.AH_PostDate = new ZDateTime(value);
			}
		}
		public ZPropertyInfo PostDateInfo => GetWrappedZPropertyInfo(nameof(PostDate), x => Parent.AH_PostDateInfo);

		public ZDate InvoiceDate => Parent.AH_InvoiceDate.Date;
		public ZString Organisation => Parent.Header?.OH_Code ?? ZString.Empty;
		public ZString Description
		{
			get { return Parent.AH_Desc; }
			set { Parent.AH_Desc = value; }
		}
		public ZPropertyInfo DescriptionInfo => GetWrappedZPropertyInfo(nameof(Description), x => Parent.AH_DescInfo);
		public ZString Currency => Parent.AH_RX_NKTransactionCurrency;
		public ZString DebitCreditSign => Parent.DebitCreditSign;
		public ZDecimal Amount => Parent.AH_OSExTaxAmount;
		public ZDecimal LocalAmount => Parent.AH_LocalExTaxAmount;
		public ZDecimal ExchangeRate => Parent.AH_ExchangeRate;
		public ZString GLAccount => Parent.GLHeader.AccountNum;
		public ZString Branch => Parent.Branch.GB_Code;
		public ZString Department => Parent.Department.GE_Code;

		void PostDateInfo_AdditionalValidation()
		{
			if (!IsValidationSuspended)
			{
				if (!PostDateInfo.HasErrors())
				{
					if (PostDate < InvoicePostDate)
					{
						PostDateInfo.AddError(Res.GetString("7007B8C0-ED01-403C-AA7C-F8E9CCF19CB9", "Journal Post Date cannot be earlier than associated Invoice Post Date. Invoice Post Date: {0}", InvoicePostDate.ToShortDateString()));
					}
				}
			}
		}

		void Parent_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshBinding();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Parent.RunPreSaveValidation();
		}

		protected override ZString HumanReadableNameCore => Parent.HumanReadableName;
	}
}
