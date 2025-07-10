using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public interface IVoucherLine : IFileOutput
	{
		ZString AccountNumber { get; }

		ZString Appendix { get; set; }

		ZDecimal CreditAmount { get; set; }

		ZDecimal DebitAmount { get; set; }

		ZString Description { get; set; }

		string ToString();

		ZDateTime VoucherDate { get; set; }

		ZString VoucherNumber { get; set; }

		ZString VoucherType { get; set; }

		ZString AccountDescription { get; set; }

		ZString AdditionalAccountDescription { get; }

		ZGuid AccountPK { get; set; }
	}
}