using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	/// <summary>
	/// Creates a CFX Journal for a set of charges that have been posted.
	/// </summary>
	public class CFXTransactionPoster
	{
		public CFXTransactionPoster(IReceivablesPostingChargeCollection charges, BusinessObjectFactory factory)
		{
			this.Factory = factory;
			this.Charges = charges;
		}

		#region Create CFX Transactions

		public void CreateCFXTransactions()
		{
			ZDateTime postingTime = ZDateTime.Now;

			if (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.Value && Charges.ShouldCreateCFXJournal)
			{
				Charges.CFXJournal = Factory.New<JCJournalHeader>();
				Charges.CFXJournal.SetCFXValues(postingTime, Charges);

				foreach (IReceivablesPostingCharge charge in Charges)
				{
					if (charge.CFXAmount != 0m)
					{
						charge.CreateCFXTransactionLine(Charges.CFXJournal, postingTime);
					}
				}
			}
		}

		readonly IReceivablesPostingChargeCollection Charges;
		readonly BusinessObjectFactory Factory;

		#endregion
	}
}