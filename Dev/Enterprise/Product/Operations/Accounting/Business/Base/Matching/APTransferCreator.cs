using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class APTransferCreator : TransferCreator
	{
		public APTransferCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Transfer GetNewTransfer()
		{
			return Transfer.New(typeof(APTransfer), Factory);
		}

		public override ZString GetTransferDescription()
		{
			return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsPayable + TransactionTypes.Transfer,
						   LedgerTypes.AccountsPayable + " " + new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionTypes.Transfer));
		}
		public override ZByte GetTransferAH_NumberOfSupportingDocuments() { return AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(LedgerTypes.AccountsPayable + TransactionTypes.Transfer, 0); }
	}
}