using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public class DirectTransactionHeaderBaseCollection : BusinessObjectCollection<DirectTransactionHeaderBase>
	{
		public DirectTransactionHeaderBaseCollection(BusinessObjectFactory factory, ZDateTime statementDate)
			: base(factory, new ZQuery())
		{
			this.StatementDate = statementDate;
		}

		public readonly ZDateTime StatementDate;

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(BankReconDirectPayment);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		public delegate void ShowNotSupportedExceptionHandler(object sender, string message);
		public event ShowNotSupportedExceptionHandler NotifyUserAboutNotSupportedException;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			try
			{
				base.RemoveAndDelete(elementToDelete);
			}
			catch (NotSupportedException ex)
			{
				if (NotifyUserAboutNotSupportedException != null)
				{
					NotifyUserAboutNotSupportedException(this, ex.Message);
				}
				else
				{
					throw;
				}
			}
		}
	}
}