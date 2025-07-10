using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class NewCashbookExchangeDiff : CashbookExchangeDiff
	{
		public NewCashbookExchangeDiff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CashbookExchangeDiff.Schema
		{
			public const string AH_NewCashbookExchangeDiff = "AH_NewCashbookExchangeDiff";
		}

		#region Properties

		#region Parent

		NewCashbookExchangeDiffHeader Parent => AH_NewCashbookExchangeDiff.IsValid ? Factory.Load<NewCashbookExchangeDiffHeader>(AH_NewCashbookExchangeDiff) : null;

		#endregion

		#region AH_NewCashbookExchangeDiff

		ZGuid fAH_NewCashbookExchangeDiff;
		public ZGuid AH_NewCashbookExchangeDiff
		{
			get => fAH_NewCashbookExchangeDiff;
			set
			{
				SetNonPersistentPropertyValue(AH_NewCashbookExchangeDiffInfo, ref fAH_NewCashbookExchangeDiff, value);
				RefreshDateFromHeader();
			}
		}

		public ZPropertyInfo AH_NewCashbookExchangeDiffInfo
		{
			get { return GetZPropertyInfo(nameof(AH_NewCashbookExchangeDiff)); }
		}

		#endregion

		#region Include

		ZBool fInclude;
		bool suspendedDueToInclude;
		public ZBool Include
		{
			get => fInclude;
			set
			{
				SetNonPersistentPropertyValue(IncludeInfo, ref fInclude, value);
				if (!value && !suspendedDueToInclude)
				{
					SuspendValidation();
					suspendedDueToInclude = true;
					ReloadSafe();
				}
				else if (value && suspendedDueToInclude)
				{
					ResumeValidation();
					suspendedDueToInclude = false;
				}
			}
		}

		public ZPropertyInfo IncludeInfo
		{
			get { return GetZPropertyInfo(nameof(Include)); }
		}

		#endregion

		#region AH_InvoiceDate

		public override ZDateTime AH_InvoiceDate
		{
			get => base.AH_InvoiceDate;
			set
			{
				if (Parent != null)
				{
					if (base.AH_InvoiceDate != Parent.AH_InvoiceDate)
					{
						base.AH_InvoiceDate = Parent.AH_InvoiceDate;
					}
				}
				else
				{
					base.AH_InvoiceDate = value;
				}
			}
		}

		#endregion

		#region AH_PostDate

		public override ZDateTime AH_PostDate
		{
			get => base.AH_PostDate;
			set
			{
				if (Parent != null)
				{
					if (base.AH_PostDate != Parent.AH_PostDate)
					{
						base.AH_PostDate = Parent.AH_PostDate;
					}
					ExchangeRate.Rate = GetNewExchangeRate();
				}
				else
				{
					base.AH_PostDate = value;
				}
			}
		}

		#endregion

		#region AH_Desc

		public override ZString AH_Desc
		{
			get => base.AH_Desc;
			set
			{
				if (Parent != null)
				{
					if (base.AH_Desc != Parent.AH_Desc)
					{
						base.AH_Desc = Parent.AH_Desc;
					}
				}
				else
				{
					base.AH_Desc = value;
				}
			}
		}

		#endregion

		#region Readonly

		protected override bool AH_AB_ReadOnly => true;

		#endregion

		#region Bank

		public ZString BankCurrency => BankAccount?.AB_RX_NKAccountCurrency ?? ZString.Empty;

		public ZString BankAccountDescription => BankAccount?.AB_Desc ?? ZString.Empty;

		#endregion

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Include = true;
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new NewCashbookExchangeDiffValidation(this);
		}

		public override bool IsSavedByFactory => base.IsSavedByFactory && Include;

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && Include;
		}

		#endregion

		#region Implementation

		void RefreshDateFromHeader()
		{
			if (Parent != null)
			{
				AH_InvoiceDate = Parent.AH_InvoiceDate;
				AH_PostDate = Parent.AH_PostDate;
				AH_Desc = Parent.AH_Desc;
			}
		}

		#endregion
	}
}
