namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>
	{
		public CusTempStorageRegLineTransactionCollection(CusTempStorageRegLine line) : base(line)
		{
		}

		protected override void SetDefaultsForNewElementCore(EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction transaction)
		{
			base.SetDefaultsForNewElementCore(transaction);
			transaction.SRT_TransactionType = Count == 0 ? TransactionTypes.Codes.OpeningBalance : TransactionTypes.Codes.Adjustment;
		}
	}
}
