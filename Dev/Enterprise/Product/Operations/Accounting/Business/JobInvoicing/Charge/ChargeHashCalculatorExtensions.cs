using System;
using System.IO;
using System.Security.Cryptography;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class ChargeHashCalculatorExtensions
	{
		public static byte[] CalculateCostPartHash(this BaseCharge charge, byte version)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				WriteCommonProperties(charge, writer);
				switch (version)
				{
					case 0:
						writer.Write(ZGuidToByteArraySafe(charge.JR_OH_CostAccount));
						writer.Write(charge.JR_RX_NKCostCurrency);
						writer.Write(charge.JR_OSCostAmt.Normalize());
						writer.Write(charge.JR_LocalCostAmt.Normalize());
						writer.Write(ZGuidToByteArraySafe(charge.JR_AT_CostGSTRate));
						writer.Write(ZGuidToByteArraySafe(charge.JR_A9_CostVATClass));
						break;
					default:
						throw new ArgumentException(FormattableString.Invariant($"Invalid hash version {version} for Job Charge Cost part."));
				}

				return CalculateSHA256FromStream(stream);
			}
		}

		public static byte[] CalculateSellPartHash(this BaseCharge charge, byte version)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				WriteCommonProperties(charge, writer);
				switch (version)
				{
					case 0:
						writer.Write(ZGuidToByteArraySafe(charge.JR_OH_SellAccount));
						writer.Write(charge.JR_RX_NKSellCurrency);
						writer.Write(charge.JR_OSSellAmt.Normalize());
						writer.Write(charge.JR_LocalSellAmt.Normalize());
						writer.Write(ZGuidToByteArraySafe(charge.JR_AT_SellGSTRate));
						writer.Write(ZGuidToByteArraySafe(charge.JR_A9_SellVATClass));
						writer.Write(charge.JR_RX_NKSellInvoiceCurrency);
						break;
					default:
						throw new ArgumentException(FormattableString.Invariant($"Invalid hash version {version} for Job Charge Sell part."));
				}

				return CalculateSHA256FromStream(stream);
			}
		}

		static void WriteCommonProperties(BaseCharge charge, BinaryWriter writer)
		{
				writer.Write(ZGuidToByteArraySafe(charge.JR_JH));
				writer.Write(ZGuidToByteArraySafe(charge.JR_AC));
				writer.Write(ZGuidToByteArraySafe(charge.JR_GB));
				writer.Write(ZGuidToByteArraySafe(charge.JR_GE));
		}

		static byte[] ZGuidToByteArraySafe(ZGuid zGuid)
		{
			return zGuid.IsValid ? zGuid.ToGuid().ToByteArray() : new byte[16];
		}

		static byte[] CalculateSHA256FromStream(Stream stream)
		{
			stream.Position = 0;
			return SHA256.Create().ComputeHash(stream);
		}
	}
}
