using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Integration.Compliance
{
	public interface IComplianceSubType
	{
		ZString Code { get; }
		Func<IMultilingualString> Description { get; }
		Func<ZString> LocalDescription { get; }
		LedgerOfUse Ledger { get; }
		TransactionTypeOfUse TransactionType { get; }
		ZString TaxStatusCode { get; }
		TransactionCreatingMode TransactionCreatingMode { get; }
	}
}
