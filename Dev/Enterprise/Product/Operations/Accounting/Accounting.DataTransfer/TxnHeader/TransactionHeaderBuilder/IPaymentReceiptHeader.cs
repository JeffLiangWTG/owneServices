using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public interface IPaymentReceiptHeader
	{
		ZString Type { get; }
		ZString Ledger { get; }
		ZString TransactionType { get; }
		ZDateTime InvoiceDate { get; }
		ZDateTime PostDate { get; }
		ZString OrganizationCode { get; }
		ZString Description { get; }
		ZString ReceiptPaymentType { get; }
		ZString BankAccountCode { get; }
		ZString CheckBookCode { get; }
		ZString CheckOrReference { get; }
		ZString OSCurrencyCode { get; }
		ZString LocalCurrencyCode { get; }
		ZDecimal OSTotal { get; }
		ZDecimal OSExGSTVATAmount { get; }
		ZDecimal LocalTotal { get; }
		ZDecimal LocalExVATAmount { get; }
		ZString BranchCode { get; }
		ZString DepartmentCode { get; }
		ZString CheckDrawer { get; }
		ZString DrawerBank { get; }
		ZString DrawerBranch { get; }
		ZString OverrideAddress { get; }
		ZString OverrideContact { get; }
		bool IsContainOverrideAddress { get; }
		bool IsContainOverrideContact { get; }
		ZString ThirdPartyReference { get; }
	}
}
