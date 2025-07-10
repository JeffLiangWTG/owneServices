using TransactionTypes = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>
	{
		public CusTempStorageRegLineTransactionCollection(CusTempStorageRegLine line) : base(line)
		{
		}

		protected override void SetDefaultsForNewElementCore(EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.SRT_TransactionType = Count == 0 ? TransactionTypes.Codes.OpeningBalance : TransactionTypes.Codes.Adjustment;
		}
	}
}
