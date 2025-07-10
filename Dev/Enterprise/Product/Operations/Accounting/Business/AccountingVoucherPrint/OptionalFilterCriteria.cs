using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class OptionalFilterCriteria
	{
		public OptionalFilterCriteria()
		{
		}

		public OptionalFilterCriteria(ZString description, ZQuery filter)
		{
			fDescription = description;
			fFilter = filter;
		}

		public OptionalFilterCriteria(ZString description, SchemaColumn schemaColumn, object filterValue)
		{
			fDescription = description;
			fFilter = new ZQuery(schemaColumn, filterValue);
		}

		protected bool fEnabled;
		public bool Enabled
		{
			get { return fEnabled; }
			set { fEnabled = value; }
		}

		protected ZQuery fFilter;
		protected ZQuery fEmptyFilter;
		public ZQuery Filter
		{
			get
			{
				if (Enabled && fFilter != null)
				{
					return fFilter;
				}
				else
				{
					return EmptyFilter;
				}
			}
		}

		protected ZQuery EmptyFilter
		{
			get
			{
				if (fEmptyFilter == null)
				{
					fEmptyFilter = new ZQuery();
				}
				return fEmptyFilter;
			}
		}

		public override string ToString()
		{
			return Description;
		}

		protected ZString fDescription;
		public virtual ZString Description
		{
			get { return fDescription; }
			set { fDescription = value; }
		}
	}

	public class LedgerFilterCriteria : OptionalFilterCriteria
	{
		public LedgerFilterCriteria(ZString description, ZString ledgerType)
			: base(description, AccTransactionHeaderSchema.AH_Ledger, ledgerType)
		{
		}
	}

	public class TransactionFilterCriteria : OptionalFilterCriteria
	{
		public TransactionFilterCriteria(ZString description, ZString ledgerType)
			: base(description, AccTransactionHeaderSchema.AH_TransactionType, ledgerType)
		{
		}
	}
}
