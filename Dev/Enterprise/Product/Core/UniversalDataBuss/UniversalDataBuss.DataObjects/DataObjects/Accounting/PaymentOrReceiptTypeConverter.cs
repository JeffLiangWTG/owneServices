using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public class PaymentOrReceiptTypeConverter : EnumConverter<PaymentOrReceiptType>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"AMF", "BDT", "BDP", "CSH", "CHQ",
				"CCD", "DCR", "DDR", "DDL", "EFT", "SFT", "CRQ",
				"ECC", "ENC", "END", "EDF", "INT",
				"INR", "MSF", "MSR", "NRB", "PPY", "STD", "EPA"
			};
		}

		protected override PaymentOrReceiptType[] GetEnumValues()
		{
			return new PaymentOrReceiptType[]
			{
				PaymentOrReceiptType.AMF,
				PaymentOrReceiptType.BDT,
				PaymentOrReceiptType.BDP,
				PaymentOrReceiptType.CSH,
				PaymentOrReceiptType.CHQ,
				PaymentOrReceiptType.CCD,
				PaymentOrReceiptType.DCR,
				PaymentOrReceiptType.DDR,
				PaymentOrReceiptType.DDL,
				PaymentOrReceiptType.EFT,
				PaymentOrReceiptType.SFT,
				PaymentOrReceiptType.CRQ,
				PaymentOrReceiptType.ECC,
				PaymentOrReceiptType.ENC,
				PaymentOrReceiptType.END,
				PaymentOrReceiptType.EDF,
				PaymentOrReceiptType.INT,
				PaymentOrReceiptType.INR,
				PaymentOrReceiptType.MSF,
				PaymentOrReceiptType.MSR,
				PaymentOrReceiptType.NRB,
				PaymentOrReceiptType.PPY,
				PaymentOrReceiptType.STD,
				PaymentOrReceiptType.EPA,
			};
		}
	}
}
