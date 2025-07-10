using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatchTransactionLineValidation : TransactionHeaderValidation
	{
		public DepositBatchTransactionLineValidation(DepositBatchTransactionLine parent)
			: base(parent)
		{
		}

		new DepositBatchTransactionLine Parent
		{
			get { return (DepositBatchTransactionLine)base.Parent; }
		}

		static string TransactionAlreadyBatchedMessage
		{
			get { return Res.GetString("7bdbc0ee-7faf-4a33-a979-b71d67db8d99", "This transaction has already been batched by") + " "; }
		}
		static string TransactionCancelled
		{
			get { return Res.GetString("1e004b13-abe8-4b9b-b7a8-63f2944e6004", "This transaction has been canceled."); }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsSelected();
		}

		public void ValidateIsSelected()
		{
			ValidateCalculatedProperty(Parent.IsSelectedInfo);
		}

		protected virtual void CheckIsSelected()
		{
			Parent.IsSelectedInfo.ClearAllNotifications();

			BusinessObjectFactory readOnlyFactory = Parent.Factory.GetCachedReadOnlyFactory();
			if (Parent.IsSelected && !Parent.Parent.IsInDatabase)
			{
				TransactionHeader thisTransactionInDB = readOnlyFactory.Load<TransactionHeader>(Parent.PK);
				if (thisTransactionInDB != null && !thisTransactionInDB.AH_ReceiptBatchNo.IsEmpty)
				{
					Parent.IsSelectedInfo.AddError(Res.GetString("74d22066-5940-4cab-a7cc-989ea4670696", "{0}{1} on", TransactionAlreadyBatchedMessage, thisTransactionInDB.Logs.LastModifiedByUserName) + " " + thisTransactionInDB.Logs.LastModifiedDate.ToShortDateString());
				}

				if (Parent.AH_IsCancelled)
				{
					Parent.IsSelectedInfo.AddError(TransactionCancelled);
				}
			}
		}

		protected override void CheckAH_DueDateIsValidZDateTimeRange()
		{
			//No need to be checked for Matching
		}

		protected override void CheckAH_InvoiceDateIsValidZDateTimeRange()
		{
			//No need to be checked for Matching
		}

		protected override void CheckAH_PostDateIsValidZDateTimeRange()
		{
			//No need to be checked for Matching
		}
		protected override void CheckAH_PostDate()
		{
			MandatoryValidation.CheckEntered(Parent.AH_PostDateInfo);
		}
	}
}