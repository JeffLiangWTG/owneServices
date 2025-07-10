using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconDirectPaymentValidation : DirectPayment.DirectPaymentValidation
	{
		public BankReconDirectPaymentValidation(BankReconDirectPayment parent) : base(parent)
		{
		}

		new BankReconDirectPayment Parent
		{
			get { return (BankReconDirectPayment)base.Parent; }
		}

		#region Overrides

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			CheckDate(Parent.AH_InvoiceDateInfo);
		}

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();
			CheckDate(Parent.AH_PostDateInfo);
		}

		#endregion

		#region Implementation

		void CheckDate(ZPropertyInfo info)
		{
			if (!info.HasErrors())
			{
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)Parent).ParentCollections)
				{
					ZDate statementDate = ZDate.Empty;
					if (collection is DirectTransactionHeaderBaseCollection)
					{
						statementDate = ((DirectTransactionHeaderBaseCollection)collection).StatementDate.Date;
					}
					else if (collection is BankReconDirectPaymentCollection)
					{
						statementDate = ((BankReconDirectPaymentCollection)collection).StatementDate.Date;
					}
					if (!statementDate.IsEmpty)
					{
						if (((ZDateTime)info.Value).Date > statementDate)
						{
							info.AddError(Res.GetString("f8a9e5d4-16b4-42fa-b3b1-eadd64df8531", "{0} cannot be after statement date of '{1}'.", info.Description, statementDate.ToShortDateString()));
						}
						break;
					}
				}
			}
		}

		#endregion
	}
}
