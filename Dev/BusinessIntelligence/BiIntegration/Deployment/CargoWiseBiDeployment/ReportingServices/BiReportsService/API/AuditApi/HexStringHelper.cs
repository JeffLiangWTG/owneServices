using System;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public static class HexStringHelper
	{
		public static bool IsGreaterThan(byte[] firstArray, byte[] secondArray)
		{
			if (firstArray == null || secondArray == null)
			{
				throw new ArgumentNullException(firstArray == null ? nameof(firstArray) : nameof(secondArray),
					"Byte arrays cannot be null");
			}
			else if (firstArray.Length != secondArray.Length)
			{
				throw new ArgumentException("Byte arrays should have same lengths.");
			}

			for (int i = 0; i < firstArray.Length; i++)
			{
				if (firstArray[i] > secondArray[i])
				{
					return true;
				}
				else if (firstArray[i] < secondArray[i])
				{
					return false;
				}
			}

			// If all compared bytes are equal, then the first array is not greater than the second array
			return false;
		}

		public static byte[] HexStringToBytes(string hexString)
		{
			var clipped = hexString.Replace("0x", string.Empty);
			var hexBytes = new byte[clipped.Length / 2];
			for (var i = 0; i < hexBytes.Length; i++)
			{
				hexBytes[i] = Convert.ToByte(clipped.Substring(i * 2, 2), 16);
			}
			return hexBytes;
		}

		public static string BytesToHexString(byte[] hex) => $"0x{BitConverter.ToString(hex).Replace("-", string.Empty)}";
	}
}
