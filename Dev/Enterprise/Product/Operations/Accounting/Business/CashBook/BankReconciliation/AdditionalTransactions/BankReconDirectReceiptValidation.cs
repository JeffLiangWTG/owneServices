using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconDirectReceiptValidation : DirectReceipt.DirectReceiptValidation
	{
		public BankReconDirectReceiptValidation(BankReconDirectReceipt parent) : base(parent)
		{
		}

		new BankReconDirectReceipt Parent
		{
			get { return (BankReconDirectReceipt)base.Parent; }
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
					else if (collection is BankReconDirectReceiptCollection)
					{
						statementDate = ((BankReconDirectReceiptCollection)collection).StatementDate.Date;
					}
					if (!statementDate.IsEmpty)
					{
						if (((ZDateTime)info.Value).Date > statementDate)
						{
							info.AddError(Res.GetString("39b4d33e-26a4-4fa1-afc6-637b6901aaf4", "{0} cannot be after statement date of '{1}'.", info.Description, statementDate.ToShortDateString()));
						}
						break;
					}
				}
			}
		}

		#endregion
	}
}
