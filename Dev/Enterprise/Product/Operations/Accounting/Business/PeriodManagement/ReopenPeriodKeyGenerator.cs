using System;
using System.Security.Cryptography;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public class ReopenPeriodKeyGenerator
	{
		public string GenerateKey(ZString enterpriseLicenceCode, ZString enterpriseLicenceCompanyCode, ZString userStaffCode, int period)
		{
			ZDateTime keyIssueDate = ZDateTime.UtcNow;
			return GenerateKey(keyIssueDate.ToString("yyyyMMdd_HHmm"), enterpriseLicenceCode, enterpriseLicenceCompanyCode, userStaffCode, period);
		}

		public string GenerateKey(ZString keyIssueDate, ZString enterpriseLicenceCode, ZString enterpriseLicenceCompanyCode, ZString userStaffCode, int period)
		{
			userStaffCode = userStaffCode.PadRight(3, '_');
			string input = keyIssueDate;
			input += enterpriseLicenceCode + enterpriseLicenceCompanyCode + userStaffCode + period.ToString();
			string hash = getMD5Hash(input);
			string key = keyIssueDate + userStaffCode + hash.Substring(hash.Length - 4);
			return key;
		}

		string getMD5Hash(string input)
		{
			var md5Provider = MD5.Create();
			byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
			bs = md5Provider.ComputeHash(bs);
			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach (byte b in bs)
			{
				s.Append(b.ToString("x2").ToUpper());
			}
			return s.ToString();
		}

		public bool ValidateKey(ZString key, ZString enterpriseLicenceCode, ZString enterpriseLicenceCompanyCode, ZString userStaffCode, int period)
		{
			string keyIssueDate = key.Substring(0, 13);
			string userStaffCodeFromKey = key.Substring(13, 3);
			string checksum = key.Substring(key.Length - 4);

			userStaffCode = userStaffCode.PadRight(3, '_');
			string newKey = GenerateKey(keyIssueDate, enterpriseLicenceCode, enterpriseLicenceCompanyCode, userStaffCode, period);
			string newChecksum = newKey.Substring(newKey.Length - 4);

			if (checksum != newChecksum)
			{
				return false;
			}
			if (userStaffCodeFromKey != userStaffCode)
			{
				return false;
			}
			ZDateTime keyIssueDateTime = DateTime.ParseExact(keyIssueDate, "yyyyMMdd_HHmm", null);
			ZDateTime currentDateTime = ZDateTime.UtcNow;
			ZDateTime expiryDateTime = keyIssueDateTime.AddDays(1);
			if (expiryDateTime < currentDateTime)
			{
				return false;
			}

			return true;
		}
	}
}

