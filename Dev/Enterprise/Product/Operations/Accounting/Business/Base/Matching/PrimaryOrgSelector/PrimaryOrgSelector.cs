using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class PrimaryOrgSelector : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PrimaryOrgSelector(BusinessObjectFactory factory, OrgHeader organisation)
			: base(factory)
		{
			this.Organisation = organisation;
		}

		readonly OrgHeader Organisation;

		public TransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new TransactionHeaderCollection(Factory);
				}
				return fTransactions;
			}
		}
		TransactionHeaderCollection fTransactions;

		public ZGuid OrganisationPK
		{
			get
			{
				return Organisation.PK;
			}
		}

		public ZString OrganisationCode
		{
			get
			{
				return Organisation.OH_Code;
			}
		}

		public ZString OrganisationName
		{
			get
			{
				return Organisation.OH_FullNameTruncated;
			}
		}

		public ZDecimal TotalLocalOutstandingAmount
		{
			get
			{
				ZDecimal total = 0;
				foreach (TransactionHeader transaction in Transactions)
				{
					total += transaction.AH_OutstandingAmount;
				}
				return total;
			}
		}

		public ZInt TransactionsCount
		{
			get
			{
				return Transactions.Count;
			}
		}
	}
}