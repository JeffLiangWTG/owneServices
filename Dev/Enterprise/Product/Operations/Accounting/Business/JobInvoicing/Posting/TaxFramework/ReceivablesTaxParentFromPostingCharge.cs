using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework
{
	public class ReceivablesTaxParentFromPostingCharge : ITaxRecordParentBase
	{
		public ReceivablesTaxParentFromPostingCharge(IReceivablesPostingChargeCollection postingCharges, ReadOnlyBusinessObjectFactory factory)
		{
			Argument.NotNull(postingCharges, "IReceivablesPostingChargeCollection");
			Argument.NotNull(factory, "ReadOnlyBusinessObjectFactory");
			Argument.GreaterThanOrEqual(postingCharges.Count, 1, "IReceivablesPostingChargeCollection.Count");

			PostingCharge = postingCharges.First();
			PostingCharges = postingCharges;
			Factory = factory;
			pk = ZGuid.NewZGuid();
		}

		readonly IReceivablesPostingCharge PostingCharge;
		readonly IReceivablesPostingChargeCollection PostingCharges;
		readonly ReadOnlyBusinessObjectFactory Factory;

		public PostingChargeKey Key => PostingCharges.Key;

		ZGuid ITaxRecordParentBase.PK => pk;
		readonly ZGuid pk;

		BusinessObjectFactory ITaxRecordParentBase.Factory => Factory;

		OrgHeader ITaxRecordParentBase.Org => org ?? (org = Factory.Load<OrgHeader>(PostingCharge.Debtor.PK));
		OrgHeader org;

		ZString ITaxRecordParentBase.Ledger => ZArchitecture.Core.LedgerTypes.AccountsReceivable;

		ZString ITaxRecordParentBase.Currency => PostingCharge.SellCurrency.RX_Code;

		ZDateTime ITaxRecordParentBase.PostDate => PostingCharge.ARInvoiceDate;

		GlbCompany ITaxRecordParentBase.Company => ((ITaxRecordParentBase)this).Branch.Company;

		GlbBranch ITaxRecordParentBase.Branch => branch ?? (branch = Factory.Load<GlbBranch>(PostingCharge.SellTaxBranch) ?? Factory.Load<GlbBranch>(PostingCharge.Branch));
		GlbBranch branch;

		GlbDepartment ITaxRecordParentBase.Department => department ?? (department = Factory.Load<GlbDepartment>(PostingCharge.Department));
		GlbDepartment department;

		bool ITaxRecordParentBase.IsPosted => false;

		IReadOnlyList<ITaxableTransactionLineBase> ITaxRecordParentBase.GetLines()
		{
			if (taxLines == null)
			{
				taxLines = new List<ITaxableTransactionLineBase>();
				foreach (IReceivablesPostingCharge charge in PostingCharges)
				{
					var taxLine = new TaxLineFromPostingCharge(Key, charge, Factory);

					taxLines.Add(taxLine);
				}
			}
			return taxLines;
		}
		List<ITaxableTransactionLineBase> taxLines;
	}
}
