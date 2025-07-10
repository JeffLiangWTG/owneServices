using System;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Testing
{
	using System.Security.Cryptography;
	using NUnit.Framework;

	class ReopenPeriodKeyGeneratorTest : TestCase
	{
		[TestDate(2017, 09, 22)]
		public void TestGenerateKey()
		{
			ReopenPeriodKeyGenerator generator = new ReopenPeriodKeyGenerator();
			string keyIssueDate = new DateTime(2009, 09, 30, 22, 30, 0, 0, new System.Globalization.GregorianCalendar(), DateTimeKind.Utc).ToString("yyyyMMdd_HHmm");
			string key = generator.GenerateKey(keyIssueDate, "EDI", "EDI", "PL", 200909);
			AssertEquals("Key", "20090930_2230PL_9D22", key);
		}

		[TestDate(2009, 09, 20, 22, 30, 0)]
		public void TestValidateKey()
		{
			ReopenPeriodKeyGenerator generator = new ReopenPeriodKeyGenerator();
			string key = generator.GenerateKey("EDI", "EDI", "PL", 200909);
			string keyIssueDate = ZDateTime.UtcNow.ToString("yyyyMMdd_HHmm");
			string input = keyIssueDate + "EDI" + "EDI" + "PL_" + "200909";
			var md5Provider = MD5.Create();
			byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
			bs = md5Provider.ComputeHash(bs);
			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach (byte b in bs)
			{
				s.Append(b.ToString("x2").ToUpper());
			}

			string hash = s.ToString();
			string manualKey = keyIssueDate + "PL_" + hash.Substring(hash.Length - 4);
			AssertEquals("Key should match", manualKey, key);
			bool result = generator.ValidateKey(key, "EDI", "EDI", "PL", 200909);
			AssertEquals("Key should be valid", true, result);
			result = generator.ValidateKey(key, "EDU", "EDI", "PL", 200909);
			AssertEquals("Key should be invalid", false, result);
			result = generator.ValidateKey(key, "EDI", "EDA", "PL", 200909);
			AssertEquals("Key should be invalid", false, result);
			result = generator.ValidateKey(key, "EDI", "EDI", "AB", 200909);
			AssertEquals("Key should be invalid", false, result);
			result = generator.ValidateKey(key, "EDI", "EDI", "PL", 200908);
			AssertEquals("Key should be invalid", false, result);
			keyIssueDate = new DateTime(2009, 09, 15, 22, 30, 0, 0, new System.Globalization.GregorianCalendar(), DateTimeKind.Utc).ToString("yyyyMMdd_HHmm");
			key = generator.GenerateKey(keyIssueDate, "EDI", "EDI", "PL", 200909);
			result = generator.ValidateKey(key, "EDI", "EDI", "PL", 200909);
			AssertEquals("Key should be invalid", false, result);
		}
	}
}
