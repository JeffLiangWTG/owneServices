using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Enterprise.MailManager.SMS
{
	public class SMSHashProviderTest : TestCase
	{
		public SMSHashProviderTest() : base() { }

		//Expected results for these test come from the details specified by the SMSC provider, see SMSProviderHashDetails.txt

		protected bool BytesMatch(byte[] expr1, byte[] expr2)
		{
			if (expr1.Length != expr2.Length)
			{
				return false;
			}

			for (int i = 0; i < expr1.Length; i++)
			{
				if (!expr1[i].Equals(expr2[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void TestHashEndToEndGeneration()
		{
			var fExpectedHash = "071c7214b92ed390b5027f4988f86ecc";
			var fRecipient = "61411222333";
			var fText = "testmessage";
			var fPassKey = "testpassword";
			var fAuthKey = "1FC742D20795F202";
			var fId = "0001";
			var fTimeStamp = "20030612163345123";

			var hashMaker = new SMSHashProvider();
			var fHash = hashMaker.GetHash(fId, fRecipient, fText, fTimeStamp, fPassKey, fAuthKey);

			AssertEquals("Hash key", fExpectedHash, fHash);
		}

		public void TestXorCalcOnHexString()
		{
			var fAuthKey = "0000000000000000000000000000000000001FC742D20795F202";
			var fTimeStamp = "0000000000000000000000000000000000000030612163345123";
			var fExpectedXorResult = "0000000000000000000000000000000000001FF723F364A1A321";
			var fExpectedXorResultWithFormat = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-1F-F7-23-F3-64-A1-A3-21";

			AssertEquals("Xor string", fExpectedXorResult, SMSHashProvider.GetStringXorFromHexStrings(fAuthKey, fTimeStamp, withSeparator: false));
			AssertEquals("Xor string", fExpectedXorResultWithFormat, SMSHashProvider.GetStringXorFromHexStrings(fAuthKey, fTimeStamp, withSeparator: true));
		}

		public void TestGetBytesFromHexString()
		{
			var fFormattedHexString = "07-1c-72-14-cd-4b-a0-e4-c5-63-0c-3a-ff-97-1c-a8";
			var fExpectedBytes = new byte[] { 7, 28, 114, 20, 205, 75, 160, 228, 197, 99, 12, 58, 255, 151, 28, 168 };
			var fResultBytes = SMSHashProvider.GetBytes(fFormattedHexString);

			AssertEquals("Bytes match", expected: true, BytesMatch(fExpectedBytes, fResultBytes));
			AssertEquals("Bytes match", expected: false, BytesMatch(fExpectedBytes, SMSHashProvider.GetBytes(fFormattedHexString + "-07")));
		}

		public void TestGetMD5Hash()
		{
			var fExpectdMD5Hash = "071c7214cd4ba0e4c5630c3aff971ca8";
			var fBytesToHash = "30-30-30-31-74-65-73-74-6D-65-73-73-61-67-65-36-31-34-2E-C6-11-C1-56-92-90-12";

			var fMD5HashKey = MD5.Create().ComputeHash(SMSHashProvider.GetBytes(fBytesToHash));
			var fMD5 = BitConverter.ToString(fMD5HashKey).Replace("-", "").ToLower();

			AssertEquals("MD5 hash", fExpectdMD5Hash, fMD5);
		}
	}
}
