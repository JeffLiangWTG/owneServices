using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface
{
	public class ChinaJournalCollection : VoucherCollection
	{
		public ChinaJournalCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		public new ChinaJournal this[int index]
		{
			get { return (ChinaJournal)Elements[index]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChinaJournal();
		}

		public new ChinaJournal AddNew()
		{
			return (ChinaJournal)base.AddNew();
		}

		ZInt SerialNumber = 0;

		protected override void SetVoucherValue(ZInt year, AccTransactionHeader transaction, VoucherProvider provider)
		{
			ZInt i = 0;
			SerialNumber++;
			foreach (VoucherLine voucherLine in provider.VoucherLines)
			{
				i++;
				ChinaJournal voucher = AddNew();
				SetVoucherLineValue(year, transaction, provider, voucher, voucherLine, i);
				voucher.GLAccountNumAndDescription = voucherLine.AdditionalAccountDescription;
				voucher.Currency = ChineseUtils.GetCurrencyNameForChinese(Factory, voucherLine.CurrencyCode);

				if (voucher.CurrencyCode == GlbCompany.CurrentCompany.LocalCurrency.RX_Code)
				{
					voucher.OriginalAmount = voucherLine.DebitAmount == 0 ? voucherLine.CreditAmount : voucherLine.DebitAmount;
					voucher.ExRate = 1m;
				}
				else
				{
					voucher.ExRate = voucherLine.ExchangeRate <= 0 ? 1 : voucherLine.ExchangeRate;
					voucher.OriginalAmount = voucherLine.OSDebitAmount == 0 ? voucherLine.OSCreditAmount : voucherLine.OSDebitAmount;
				}

				voucher.SerialNumber = SerialNumber;
			}
		}
	}
}

