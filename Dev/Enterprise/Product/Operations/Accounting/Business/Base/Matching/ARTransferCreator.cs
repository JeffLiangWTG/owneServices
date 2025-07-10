using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ARTransferCreator : TransferCreator
	{
		public ARTransferCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Transfer GetNewTransfer()
		{
			return Transfer.New(typeof(ARTransfer), Factory);
		}

		public override ZString GetTransferDescription()
		{
			return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsReceivable + TransactionTypes.Transfer,
						   LedgerTypes.AccountsReceivable + " " + new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionTypes.Transfer));
		}
		public override ZByte GetTransferAH_NumberOfSupportingDocuments() { return AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(LedgerTypes.AccountsReceivable + TransactionTypes.Transfer, 0); }
	}
}